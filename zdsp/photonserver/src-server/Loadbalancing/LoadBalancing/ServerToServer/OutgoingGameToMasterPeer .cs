// --------------------------------------------------------------------------------------------------------------------
// chensheng 2018/03/19
// GameServer to MasterServer
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
    using Zealot.Common;
    #endregion
    public partial class OutgoingGameToMasterPeer : OutgoingServerPeer
    {
        protected readonly GameApplication application;
        #region Constructors and Destructors
        public OutgoingGameToMasterPeer(GameToMasterConnection serverConnection)
            : base(serverConnection)
        {
            application = (GameApplication)serverConnection.Application;
        }
        #endregion
        #region Methods
        protected override void Register()
        {
            application.executionFiber.Enqueue(() =>
            {
                ServerConfig srvconfig = application.MyServerConfig;
                var contract = new RegisterGameServer
                {
                    GameServerAddress = application.PublicIpAddress.ToString(),
                    UdpPort = this.application.GamingUdpPort,
                    TcpPort = this.application.GamingTcpPort,
                    WebSocketPort = this.application.GamingWebSocketPort,
                    ServerConfig = srvconfig.serializeString,
                    OnlineUsers = application.GetOnlineUserIds()
                };
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat(
                        "Registering game server with address {0}, TCP {1}, UDP {2}, WebSocket {3}, ServerID {4}",
                        contract.GameServerAddress,
                        contract.TcpPort,
                        contract.UdpPort,
                        contract.WebSocketPort,
                        srvconfig.id);
                }
                var request = new OperationRequest((byte)OperationCode.RegisterGameServer, contract);
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
                    case ServerEventCode.MasterToGame:
                        ZRPC.MasterToGameRPC.OnCommandServer(this.application, this, request, sendParameters);
                        break;
                    default:
                        break;
                }
            });
        }
        #endregion
    }
}