// --------------------------------------------------------------------------------------------------------------------
// chensheng 2018/03/19
// --------------------------------------------------------------------------------------------------------------------
namespace Photon.LoadBalancing.ServerToServer
{
    #region using directives
    using ExitGames.Logging;
    using Photon.Common;
    using Photon.SocketServer;
    using Photon.SocketServer.ServerToServer;
    using PhotonHostRuntimeInterfaces;
    using Zealot.RPC;
    using OperationCode = Photon.LoadBalancing.ServerToServer.Operations.OperationCode;
    #endregion
    public abstract class OutgoingServerPeer : OutboundS2SPeer
    {
        #region Constants and Fields
        protected static readonly ILogger log = LogManager.GetCurrentClassLogger();
        protected readonly ServerConnectionBase serverConnection;
        public ZRPC ZRPC;
        public bool IsRegistered = false;
        #endregion
        #region Constructors and Destructors
        public OutgoingServerPeer(ServerConnectionBase serverConnection)
            : base(serverConnection.Application)
        {
            this.serverConnection = serverConnection;
            ZRPC = new ZRPC();
            log.InfoFormat("connection to server at {0}:{1} established (id={2})", this.RemoteIP, this.RemotePort, this.ConnectionId);
        }
        #endregion
        #region Methods
        protected abstract void Register();
        protected virtual void HandleRegisterServerResponse(OperationResponse operationResponse)
        {
            switch (operationResponse.ReturnCode)
            {
                case (short)ErrorCode.Ok:
                    {
                        log.InfoFormat("Successfully registered server at {0}:{1}", this.RemoteIP, this.RemotePort);
                        this.IsRegistered = true;
                        break;
                    }
                default:
                    {
                        log.WarnFormat("Failed to register server: err={0}, msg={1}", operationResponse.ReturnCode, operationResponse.DebugMessage);
                        this.IsRegistered = false;
                        this.Disconnect();
                        break;
                    }
            }
        }

        protected override void OnEvent(IEventData eventData, SendParameters sendParameters){}
        protected override void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
        {
            switch (operationResponse.OperationCode)
            {
                default:
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.DebugFormat("Received unknown operation code {0}", operationResponse.OperationCode);
                        }
                        break;
                    }
                case OperationCode.RegisterGameServer:
                case OperationCode.RegisterClusterServer:
                    {
                        this.HandleRegisterServerResponse(operationResponse);
                        break;
                    }
            }
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            this.IsRegistered = false;
            log.InfoFormat("connection closed (id={0}, reason={1}, detail={2})", this.ConnectionId, reasonCode, reasonDetail);
            OnConnectionFailed((int)reasonCode, reasonDetail);
        }

        protected override void OnConnectionEstablished(object responseObject)
        {
            serverConnection.OnConnectionEstablished(responseObject);
            Register();
        }

        protected override void OnConnectionFailed(int errorCode, string errorMessage)
        {
            serverConnection.OnConnectionFailed(errorCode, errorMessage);
        }
        #endregion
    }
}