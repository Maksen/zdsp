// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameApplication.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the GameApplication type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Photon.LoadBalancing.GameServer
{
    #region using directives
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using ExitGames.Concurrency.Fibers;
    using ExitGames.Logging;
    using ExitGames.Logging.Log4Net;
    using log4net;
    using log4net.Config;
    using Photon.LoadBalancing.Common;
    using Photon.SocketServer;
    using Photon.SocketServer.Diagnostics;
    using Photon.SocketServer.Diagnostics.Configuration;
    using LogManager = ExitGames.Logging.LogManager;
    using Zealot.RPC;
    using Zealot.Server.Counters;
    using Zealot.Server.Rules;
    using System.Globalization;
    using Photon.Common.LoadBalancer.LoadShedding;
    using ServerToServer;
    #endregion
    public partial class GameApplication : ApplicationBase
    {
        #region Constants and Fields
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private readonly NodesReader reader;
        private static GameApplication instance;
        public OutgoingClusterServerPeer clusterPeer;
        public OutgoingGameToMasterPeer masterPeer;
        public static int ServerId;
        public PoolFiber executionFiber;
        public ZRPC ZRPC;
        public ClusterServerConnection ClusterServerConnection { get; private set; }
        public GameToMasterConnection GameToMasterConnection { get; private set; }
        public virtual GameCache GameCache { get { return GameCache.Instance; } protected set { } }
        #endregion

        #region Constructors and Destructors
        public GameApplication()
        {
            this.GamingTcpPort = GameServerSettings.Default.GamingTcpPort;
            this.GamingUdpPort = GameServerSettings.Default.GamingUdpPort;
            this.GamingWebSocketPort = GameServerSettings.Default.GamingWebSocketPort;
            this.reader = new NodesReader(this.ApplicationRootPath, CommonSettings.Default.NodesFileName);

            InitLogic();
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }
        #endregion
        #region Public Properties
        public static new GameApplication Instance
        {
            get
            {
                return instance;
            }
            protected set
            {
                Interlocked.Exchange(ref instance, value);
            }
        }
        public int? GamingTcpPort { get; protected set; }
        public int? GamingUdpPort { get; protected set; }
        public int? GamingWebSocketPort { get; protected set; }
        public ApplicationStatsPublisher AppStatsPublisher { get; protected set; } 
        public IPAddress PublicIpAddress { get; protected set; }
        public WorkloadController WorkloadController { get; protected set; }
        public ReviveItemController ReviveItemController { get; protected set; }
        #endregion
        #region Public Methods
        public byte GetCurrentNodeId()
        {
            return this.reader.ReadCurrentNodeId();
        }
        #endregion
        #region Methods
        protected virtual void InitLogging()
        {
            string logPath = Path.Combine(this.ApplicationRootPath, "log");
            string configPath = Path.Combine(this.BinaryPath, "log4net.config");

            LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
            GlobalContext.Properties["Photon:ApplicationLogPath"] = logPath;
            GlobalContext.Properties["LogFileName"] = "GS" + this.ApplicationName;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configPath));
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        protected void SetupFeedbackControlSystem()
        {
            var workLoadConfigFile = GameServerSettings.Default.WorkloadConfigFile;

            this.WorkloadController = new WorkloadController(
                this, this.PhotonInstanceName, 1000, ServerId.ToString(), workLoadConfigFile);

            if (!this.WorkloadController.IsInitialized)
            {
                const string message = "WorkloadController failed to be constructed";

                if (CommonSettings.Default.EnablePerformanceCounters)
                {
                    throw new Exception(message);
                }

                log.Warn(message);
            }

            this.WorkloadController.Start();
        }

        protected override void OnStopRequested()
        {
            log.InfoFormat("OnStopRequested: serverid={0}", ServerId);
            LogPCU();

            if (Leaderboard != null)
                Leaderboard.Stop();

            GameRules.SaveToDB();
            GuildRules.OnStop();
            AuctionRules.StopTimer();

            if (ClusterServerConnection != null)
                ClusterServerConnection.Dispose();
            clusterPeer = null;
            if (GameToMasterConnection != null)
                GameToMasterConnection.Dispose();
            masterPeer = null;
            if (this.WorkloadController != null)
                this.WorkloadController.Stop();
            base.OnStopRequested();
        }

        protected override void TearDown()
        {
            log.InfoFormat("TearDown: serverId={0}", ServerId);

            if (Leaderboard != null)
                Leaderboard.Dispose();

            AuctionRules.Dispose();

            if (dbRepository != null) dbRepository.Dispose();
            if (dbGM != null) dbGM.Dispose();

            if (ClusterServerConnection != null)
                ClusterServerConnection.Dispose();
            clusterPeer = null;
            if (GameToMasterConnection != null)
                GameToMasterConnection.Dispose();
            masterPeer = null;
            if (this.WorkloadController != null)
                this.WorkloadController.Stop();
        }

        private IPEndPoint GetLatencyEndpointTcp()
        {
            if (!GameServerSettings.Default.EnableLatencyMonitor)
            {
                return null;
            }
            IPEndPoint latencyEndpointTcp;
            if (string.IsNullOrEmpty(GameServerSettings.Default.LatencyMonitorAddress))
            {
                if (this.GamingTcpPort.HasValue == false)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Error(
                            "Could not start latency monitor because no tcp port is specified in the application configuration.");
                    }
                    return null;
                }
                if (this.PublicIpAddress == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Error("Could not latency monitor because public ip adress could not be resolved.");
                    }
                    return null;
                }
                int? tcpPort = GameServerSettings.Default.RelayPortTcp == 0
                                   ? this.GamingTcpPort
                                   : GameServerSettings.Default.RelayPortTcp + this.GetCurrentNodeId() - 1;
                latencyEndpointTcp = new IPEndPoint(this.PublicIpAddress, tcpPort.Value);
            }
            else
            {
                if (Global.TryParseIpEndpoint(GameServerSettings.Default.LatencyMonitorAddress, out latencyEndpointTcp)
                    == false)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.ErrorFormat(
                            "Could not start latency monitor because an invalid endpoint ({0}) is specified in the application configuration.",
                            GameServerSettings.Default.LatencyMonitorAddress);
                    }
                    return latencyEndpointTcp;
                }
            }
            return latencyEndpointTcp;
        }
        private IPEndPoint GetLatencyEndpointUdp()
        {
            if (!GameServerSettings.Default.EnableLatencyMonitor)
            {
                return null;
            }
            IPEndPoint latencyEndpointUdp;
            if (string.IsNullOrEmpty(GameServerSettings.Default.LatencyMonitorAddressUdp))
            {
                if (this.GamingUdpPort.HasValue == false)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Error(
                            "Could not latency monitor because no udp port is specified in the application configuration.");
                    }
                    return null;
                }
                if (this.PublicIpAddress == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Error("Could not latency monitor because public ip adress could not be resolved.");
                    }
                    return null;
                }
                int? udpPort = GameServerSettings.Default.RelayPortUdp == 0
                                   ? this.GamingUdpPort
                                   : GameServerSettings.Default.RelayPortUdp + this.GetCurrentNodeId() - 1;
                latencyEndpointUdp = new IPEndPoint(this.PublicIpAddress, udpPort.Value);
            }
            else
            {
                if (Global.TryParseIpEndpoint(
                    GameServerSettings.Default.LatencyMonitorAddressUdp, out latencyEndpointUdp) == false)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.ErrorFormat(
                            "Coud not start latency monitor because an invalid endpoint ({0}) is specified in the application configuration.",
                            GameServerSettings.Default.LatencyMonitorAddressUdp);
                    }
                    return latencyEndpointUdp;
                }
            }
            return latencyEndpointUdp;
        }
        #endregion

        /// <summary>
        /// This is a hack, for async functions invoked by RPCs.
        /// This is to rethrow exceptions in async void functions that would otherwise not be caught.
        /// Only called in debug mode.
        /// </summary>
        /// <remarks>
        /// In order to properly handle exceptions in async Tasks, all async functions have to return the 
        /// Task up the call hierarchy back to the caller.
        /// </remarks>
        [System.Diagnostics.Conditional("DEBUG")]
        public void RethrowAsyncException(Exception ex)
        {
            this.executionFiber.Enqueue(() =>
            {
                throw ex;
            });
        }

        protected void InitCounters()
        {
            // initialize counter publisher and add your custom counter class(es):
            if (PhotonSettings.Default.CounterPublisher.Enabled)
            {
                // counters for the photon dashboard
                CounterPublisher.DefaultInstance.AddStaticCounterClass(typeof(GameCounters), this.ApplicationName);
                CounterPublisher.DefaultInstance.Start();
            }
            //PerfmonCounters.InitCounters(); //Not using Windows Performance Monitor
        }
    }
}
