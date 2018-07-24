// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MasterApplication.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the MasterApplication type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Photon.LoadBalancing.MasterServer
{
    using ExitGames.Concurrency.Fibers;
    using ExitGames.Logging;
    using ExitGames.Logging.Log4Net;
    using Facebook;
    using GameServer;
    using log4net;
    using log4net.Config;
    using Photon.LoadBalancing.MasterServer.Cluster;
    using Photon.LoadBalancing.MasterServer.GameManager;
    using Photon.SocketServer;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Zealot.Common;
    using Zealot.DBRepository;
    using LogManager = ExitGames.Logging.LogManager;

    public partial class MasterApplication : ApplicationBase
    {
        #region Constants and Fields
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static MasterApplication instance;
        public PoolFiber executionFiber;
        public MasterCluster mMasterCluster;
        public MasterGame mMasterGame;
        #endregion

        #region Constructors and Destructors
        public MasterApplication() : base()
        {
            executionFiber = new PoolFiber();
            executionFiber.Start();
            mMasterCluster = new MasterCluster();
            mMasterGame = new MasterGame();
        }
        #endregion

        #region Properties
        public static new MasterApplication Instance { get { return instance; } }
        public string FbAppToken { get; set; }
        public static ApplicationStats AppStats { get; protected set; }
        public Dictionary<string, string> CurVersionMap = new Dictionary<string, string>();
        public IncomingGMPeer GMPeer;
        public static DBRepositoryGM dbGMRepository { get; protected set; }
        #endregion

        #region Methods
        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            string applicationId = initRequest.ApplicationId;
            if (log.IsDebugEnabled)
                log.DebugFormat("Received init request from applicationId");
            if (initRequest.LocalPort == MasterServerSettings.Default.IncomingClientPeerPort)
            {
                if (applicationId == "Loadtest" || applicationId == "master")
                    return new MasterClientPeer(initRequest, this);
            }
            else
            {
                switch (applicationId)
                {
                    case "ClusterServer":
                        return new IncomingClusterServerPeer(initRequest, this);
                    case "GameServer":
                        return new IncomingGamePeer(initRequest, this);
                    case "GMServer":
                        if (this.GMPeer != null)
                            GMPeer.Disconnect();
                        GMPeer = new IncomingGMPeer(initRequest, this);
                        return GMPeer;
                    default:
                        break;
                }
            }
            return null;
        }
        protected virtual void Initialize()
        {
            InitVersion();
            if (MasterServerSettings.Default.AppStatsPublishInterval > 0)
            {
                AppStats = new ApplicationStats(MasterServerSettings.Default.AppStatsPublishInterval);
            }

            this.InitFBToken();
            //this.TestGoogleAuth();

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }

        /*protected void TestGoogleAuth()
        {
            // For testing google login using win forms
            var t = new Thread(new ThreadStart(() => {
                Auth m = new Auth();
                var result = m.ShowDialog();
            }));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }*/

        protected async void InitFBToken()
        {
            var fbClient = new FacebookClient();
            string url = string.Format("/oauth/access_token?client_id={0}&client_secret={1}&grant_type=client_credentials",
                            MasterServerSettings.Default.FbAppID, MasterServerSettings.Default.FbAppSecret);
            var appToken = await fbClient.GetTaskAsync(url) as IDictionary<string, object>;
            FbAppToken = (string)appToken["access_token"];
        }

        public void InitVersion()
        {
            CurVersionMap.Clear();
            string version = MasterServerSettings.Default.MasterVersion;
            if (string.IsNullOrEmpty(version))
                return;
            string[] platformVersions = version.Split(';');
            for (int index = 0; index < platformVersions.Length; index++)
            {
                string[] plaformAndVersion = platformVersions[index].Split(':');
                string _platform = plaformAndVersion[0];
                string _version = plaformAndVersion[1];
                CurVersionMap[_platform] = _version;
            }
        }

        public bool IsVersionMatch(string platform, string version)
        {
            if (CurVersionMap.Count == 0)
                return true;
            if (CurVersionMap.ContainsKey(platform))
                return CurVersionMap[platform] == version;
            return false;
        }

        protected override void OnStopRequested()
        {
            Dictionary<string, IncomingClusterServerPeer> clusterServers = mMasterCluster.ClusterServers;
            // in case of application restarts, we need to disconnect all GS peers to force them to reconnect. 
            if (log.IsDebugEnabled)
                log.DebugFormat("OnStopRequested... going to disconnect {0} GS peers", clusterServers.Count);
            // copy to prevent changes of the underlying enumeration
            var gameServers = new Dictionary<string, IncomingClusterServerPeer>(clusterServers);
            foreach (var peer in gameServers.Values)
            {
                if (log.IsDebugEnabled)
                    log.DebugFormat("Disconnecting GS peer {0}:{1}", peer.RemoteIP, peer.RemotePort);
                peer.Disconnect();
            }
        }
        protected override void Setup()
        {
            instance = this;
            string logPath = Path.Combine(this.ApplicationRootPath, "log");
            string configPath = Path.Combine(this.BinaryPath, "log4net.config");

            LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);

            GlobalContext.Properties["Photon:ApplicationLogPath"] = logPath;
            GlobalContext.Properties["LogFileName"] = "MS" + this.ApplicationName;

            XmlConfigurator.ConfigureAndWatch(new FileInfo(configPath));
            Protocol.AllowRawCustomValues = true;
            this.SetUnmanagedDllDirectory();
            this.Initialize();

            dbGMRepository = new DBRepositoryGM();
            bool _gmconnectDB = dbGMRepository.Initialize(MasterServerSettings.Default.GMConnectionString);
            if (!_gmconnectDB)
            {
                log.InfoFormat("DB connection failed, Killed Process, try restart server later.");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            RefreshServerLineInfo(dbGMRepository.ServerConfig.ServerLine_SelectAll());
        }

        public async Task ServerLineSelectAllAsync()
        {
            var serverlineList = await dbGMRepository.ServerConfig.ServerLine_SelectAllAsync();
            executionFiber.Enqueue(() => RefreshServerLineInfo(serverlineList));
        }

        public void RefreshServerLineInfo(List<Dictionary<string, object>> serverlineList)
        {
            var ServerLineList = mMasterGame.ServerLineList;
            ServerLineList.Clear();
            for (int index = 0; index < serverlineList.Count; index++)
            {
                Dictionary<string, object> row = serverlineList[index];
                int serverline = (int)row["serverline"];
                string name = (string)row["name"];
                bool recommend = (bool)row["recommend"];
                ServerLineList.list.Add(new ServerLine(serverline, name, recommend));
            }
            ServerLineList.serializeString = JsonConvertDefaultSetting.SerializeObject(ServerLineList);
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

        protected override void TearDown()
        {
            if (dbGMRepository != null)
                dbGMRepository.Dispose();
            if (this.executionFiber != null)
                this.executionFiber.Enqueue(() => this.executionFiber.Dispose());
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);
        /// <summary>
        /// Adds a directory to the search path used to locate the 32-bit or 64-bit version 
        /// for unmanaged DLLs used in the application.
        /// </summary>
        /// <remarks>
        /// Assemblies having references to unmanaged libraries (like SqlLite) require either a
        /// 32-Bit or a 64-Bit version of the library depending on the current process.
        /// </remarks>
        private void SetUnmanagedDllDirectory()
        {
            string unmanagedDllDirectory = Path.Combine(this.BinaryPath, IntPtr.Size == 8 ? "x64" : "x86");
            bool result = SetDllDirectory(unmanagedDllDirectory);
            if (result == false)
            {
                log.WarnFormat("Failed to set unmanaged dll directory to path {0}", unmanagedDllDirectory);
            }
        }
        #endregion
    }
}