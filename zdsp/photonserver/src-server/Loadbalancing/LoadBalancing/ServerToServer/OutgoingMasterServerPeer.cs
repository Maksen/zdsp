// --------------------------------------------------------------------------------------------------------------------
// chensheng 2018/03/19
// ClusterServer to MasterServer
// --------------------------------------------------------------------------------------------------------------------
namespace Photon.LoadBalancing.ServerToServer
{
    #region using directives
    using Photon.LoadBalancing.ServerToServer.Events;
    using Photon.LoadBalancing.ServerToServer.Operations;
    using Photon.SocketServer;
    using Zealot.RPC;
    using OperationCode = Photon.LoadBalancing.ServerToServer.Operations.OperationCode;
    using GameServer;
    using ClusterServer;
    #endregion
    public partial class OutgoingMasterServerPeer : OutgoingServerPeer
    {
        protected readonly ClusterApplication application;
        #region Constructors and Destructors
        public OutgoingMasterServerPeer(MasterServerConnection serverConnection)
            : base(serverConnection)
        {
            application = (ClusterApplication)serverConnection.Application;
        }
        #endregion
        #region Methods
        protected override void Register()
        {
            application.executionFiber.Enqueue(() =>
            {
                ClusterServer cluster = application.mClusterServer;
                var contract = new RegisterClusterServer
                {
                    GameServerAddress = ClusterServerSettings.Default.PublicIPAddress,
                    TcpPort = ClusterServerSettings.Default.PublicTcpPort

                };
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat(
                        "Registering cluster server with address {0}, TCP {1}",
                        contract.GameServerAddress,
                        contract.TcpPort);
                }
                var request = new OperationRequest((byte)OperationCode.RegisterClusterServer, contract);
                this.SendOperationRequest(request, new SendParameters());
                this.IsRegistered = true;
            });
        }

        protected override void OnOperationRequest(OperationRequest request, SendParameters sendParameters)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Received operation code {0}", request.OperationCode);
            }
            application.executionFiber.Enqueue(() =>
            {
                switch (request.OperationCode)
                {
                    case ServerEventCode.MasterToCluster:
                        ZRPC.MasterToClusterRPC.OnCommandServer(this.application, this, request, sendParameters);
                        break;
                    default:
                        break;
                }
            });
        }
        #endregion
    }
}