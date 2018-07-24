using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net;
using log4net.Config;
using Photon.LoadBalancing.ClusterServer.GameServer;
using Photon.LoadBalancing.MasterServer;
using Photon.LoadBalancing.ServerToServer;
using Photon.SocketServer;
using System.IO;
using Zealot.DBRepository;
using Zealot.RPC;
using LogManager = ExitGames.Logging.LogManager;

namespace Photon.LoadBalancing.ClusterServer
{
    public partial class ClusterApplication : ApplicationBase
    {
        #region Constants, Fields, Properties
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static ClusterApplication instance;
        public readonly PoolFiber executionFiber;
        public ClusterServer mClusterServer;
        public OutgoingMasterServerPeer masterPeer;
        public MasterServerConnection MasterServerConnection;
        public ZRPC ZRPC;
        public static new ClusterApplication Instance { get { return instance; } }
        public static DBRepositoryGM dbGMRepository { get; protected set; }
        #endregion

        #region Constructors and Destructors
        public ClusterApplication() : base()
        {
            instance = this;
            executionFiber = new PoolFiber();
            executionFiber.Start();
            mClusterServer = new ClusterServer(this);
            ZRPC = new ZRPC();
        }
        #endregion

        #region override base functions
        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            string applicationId = initRequest.ApplicationId;
            if (log.IsDebugEnabled)
                log.DebugFormat("Received init request from applicationId");
            switch (applicationId)
            {
                case "GameServer":
                    return new IncomingGameServerPeer(initRequest, this);
                default:
                    break;
            }
            return null;
        }

        protected override void Setup()
        {
            string logPath = Path.Combine(this.ApplicationRootPath, "log");
            string configPath = Path.Combine(this.BinaryPath, "log4net.config");

            LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
            GlobalContext.Properties["Photon:ApplicationLogPath"] = logPath;
            GlobalContext.Properties["LogFileName"] = "MS" + this.ApplicationName;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configPath));       
            Protocol.AllowRawCustomValues = true;

            dbGMRepository = new DBRepositoryGM();
            bool _gmconnectDB = dbGMRepository.Initialize(MasterServerSettings.Default.GMConnectionString);
            if (!_gmconnectDB)
            {
                log.InfoFormat("DB connection failed, Killed Process, try restart server later.");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            MasterServerConnection = new MasterServerConnection(this, ClusterServerSettings.Default.MasterIPAddress, ClusterServerSettings.Default.OutgoingMasterServerPeerPort, ClusterServerSettings.Default.ConnectReytryInterval, "ClusterServer");
            MasterServerConnection.Initialize();

            //test disconnect, chensheng
            //this.executionFiber.Schedule(() =>
            //{
            //    if (masterPeer != null && masterPeer.IsRegistered)
            //        masterPeer.Disconnect();
            //}, 15000);
        }

        protected override void OnStopRequested()
        {
            log.InfoFormat("OnStopRequested: cluster");
            base.OnStopRequested();
            if (MasterServerConnection != null)
                MasterServerConnection.Dispose();
            masterPeer = null;
        }

        protected override void TearDown()
        {
            log.InfoFormat("TearDown: cluster");
            if (dbGMRepository != null)
                dbGMRepository.Dispose();
            if (executionFiber != null)
                executionFiber.Enqueue(() => executionFiber.Dispose());
            if (MasterServerConnection != null)
                MasterServerConnection.Dispose();
            masterPeer = null;
        }
        #endregion
    }
}