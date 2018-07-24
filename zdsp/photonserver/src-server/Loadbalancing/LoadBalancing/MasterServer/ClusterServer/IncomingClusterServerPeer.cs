/* --------------------------------------------------------------------------------------------------------------------
write by chensheng 2018/03/15
--------------------------------------------------------------------------------------------------------------------
*/
namespace Photon.LoadBalancing.MasterServer.Cluster
{
    #region using directives
    using System;
    using ExitGames.Logging;
    using Photon.LoadBalancing.ServerToServer.Events;
    using Photon.LoadBalancing.ServerToServer.Operations;
    using Photon.SocketServer;
    using Photon.SocketServer.ServerToServer;
    using OperationCode = Photon.LoadBalancing.ServerToServer.Operations.OperationCode;
    using Zealot.RPC;
    using Zealot.Common;
    using Photon.Common;
    using System.Collections.Generic;
    #endregion

    public class IncomingClusterServerPeer : InboundS2SPeer
    {
        #region Constants and Fields
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private readonly MasterApplication application;
        public ZRPC ZRPC;
        #endregion
        #region Constructors and Destructors
        public IncomingClusterServerPeer(InitRequest initRequest, MasterApplication application)
            : base(initRequest)
        {
            this.application = application;
            log.InfoFormat("game server connection from {0}:{1} established (id={2})", this.RemoteIP, this.RemotePort, this.ConnectionId);
            ZRPC = new ZRPC();
            Key = "";
        }
        #endregion
        #region Properties
        public string Key { get; protected set; }
        #endregion
        #region Methods
        public bool IsReady()
        {
            return !string.IsNullOrEmpty(Key);
        }

        protected virtual OperationResponse HandleRegisterServerRequest(OperationRequest request)
        {
            var registerRequest = new RegisterClusterServer(this.Protocol, request);
            if (registerRequest.IsValid == false)
            {
                string msg = registerRequest.GetErrorMessage();
                log.ErrorFormat("RegisterClusterServer contract error: {0}", msg);
                return new OperationResponse(request.OperationCode) { DebugMessage = msg, ReturnCode = (short)ErrorCode.OperationInvalid };
            }

            var contract = new RegisterServerResponse();
            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    "Received register request: Address={0}, TcpPort={1}",
                    registerRequest.GameServerAddress,
                    registerRequest.TcpPort);
            }
            this.Key = string.Format("{0}:{1}", registerRequest.GameServerAddress, registerRequest.TcpPort);
            this.application.mMasterCluster.OnConnect(this);
            return new OperationResponse(request.OperationCode, contract);
        }

        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            if (log.IsInfoEnabled)
                log.InfoFormat("OnDisconnect: cluster server connection closed (connectionId={0}, ipaddress={1}, reason={2})", this.ConnectionId, Key, reasonCode);
            if (IsReady())
            {
                application.executionFiber.Enqueue(() =>
                {
                    this.application.mMasterCluster.OnDisconnect(this);
                });
            }
        }

        protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
        {
        }

        protected override void OnOperationRequest(OperationRequest request, SendParameters sendParameters)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("OnOperationRequest: pid={0}, op={1}", this.ConnectionId, request.OperationCode);
            }

            application.executionFiber.Enqueue(() =>
            {
                OperationResponse response = null;
                switch (request.OperationCode)
                {
                    case ServerEventCode.ClusterToMaster:
                        ZRPC.ClusterToMasterRPC.OnCommandServer(this.application.mMasterCluster, this, request, sendParameters);
                        break;
                    case OperationCode.RegisterClusterServer:
                        {
                            response = IsReady()
                                            ? new OperationResponse(request.OperationCode) { ReturnCode = -1, DebugMessage = "already registered" }
                                            : this.HandleRegisterServerRequest(request);
                            break;
                        }
                    default:
                        response = new OperationResponse(request.OperationCode) { ReturnCode = -1, DebugMessage = "Unknown operation code" };
                        break;
                }
                if (response != null)
                    this.SendOperationResponse(response, sendParameters);
            });
        }
        protected override void OnOperationResponse(OperationResponse opResponse, SendParameters sendParameters)
        {
        }
        #endregion
    }
}