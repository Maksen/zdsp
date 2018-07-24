// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IncomingGameServerPeer.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the IncomingGameServerPeer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Photon.LoadBalancing.ClusterServer.GameServer
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

    public class IncomingGameServerPeer : InboundS2SPeer
    {
        #region Constants and Fields
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private readonly ClusterApplication application;
        public ZRPC ZRPC;
        public ServerConfig Serverconfig = null;
        #endregion
        #region Constructors and Destructors
        public IncomingGameServerPeer(InitRequest initRequest, ClusterApplication application)
            : base(initRequest)
        {
            this.application = application;
            log.InfoFormat("game server connection from {0}:{1} established (id={2})", this.RemoteIP, this.RemotePort, this.ConnectionId);
            ZRPC = new ZRPC();
            ServerId = 0;
        }
        #endregion
        #region Properties
        public string Key { get; protected set; }
        public int ServerId { get; protected set; }
        public string TcpAddress { get; protected set; }
        public string UdpAddress { get; protected set; }
        public string WebSocketAddress { get; protected set; }
        #endregion
        #region Methods
        public bool IsReady()
        {
            return ServerId != 0;
        }
        protected virtual OperationResponse HandleRegisterServerRequest(OperationRequest request)
        {
            var registerRequest = new RegisterGameServer(this.Protocol, request);
            if (registerRequest.IsValid == false)
            {
                string msg = registerRequest.GetErrorMessage();
                log.ErrorFormat("RegisterGameServer contract error: {0}", msg);
                return new OperationResponse(request.OperationCode) { DebugMessage = msg, ReturnCode = (short)ErrorCode.OperationInvalid };
            }

            ServerConfig serverConfig = ServerConfig.Deserialize(registerRequest.ServerConfig);
            serverConfig.serializeString = registerRequest.ServerConfig;
            Serverconfig = serverConfig;
            var contract = new RegisterServerResponse();
            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    "Received register request: Address={0}, UdpPort={1}, TcpPort={2}, WebSocketPort={3}",
                    registerRequest.GameServerAddress,
                    registerRequest.UdpPort,
                    registerRequest.TcpPort,
                    registerRequest.WebSocketPort);
            }
            if (registerRequest.UdpPort.HasValue)
            {
                this.UdpAddress = registerRequest.GameServerAddress + ":" + registerRequest.UdpPort;
            }
            if (registerRequest.TcpPort.HasValue)
            {
                this.TcpAddress = registerRequest.GameServerAddress + ":" + registerRequest.TcpPort;
            }
            if (registerRequest.WebSocketPort.HasValue && registerRequest.WebSocketPort != 0)
            {
                this.WebSocketAddress = registerRequest.GameServerAddress + ":" + registerRequest.WebSocketPort;
            }
            this.ServerId = serverConfig.id;
            this.Key = string.Format("{0}:{1}:{2}", registerRequest.GameServerAddress, registerRequest.UdpPort, registerRequest.TcpPort);
            this.application.mClusterServer.OnConnect(registerRequest, this);
            return new OperationResponse(request.OperationCode, contract);
        }

        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            if (log.IsInfoEnabled)
                log.InfoFormat("OnDisconnect: game server connection closed (connectionId={0}, serverId={1}, reason={2})", this.ConnectionId, ServerId, reasonCode);
            if (IsReady())
            {
                application.executionFiber.Enqueue(() =>
                {
                    application.mClusterServer.OnDisconnect(this);
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
                    case ServerEventCode.GameToCluster:
                        ZRPC.GameToClusterRPC.OnCommandServer(this.application.mClusterServer, this, request, sendParameters);
                        break;
                    case OperationCode.RegisterGameServer:
                        {
                            response = IsReady()
                                            ? new OperationResponse(request.OperationCode) { ReturnCode = -1, DebugMessage = "already registered" }
                                            : this.HandleRegisterServerRequest(request);
                            break;
                        }
                    default:
                        response = new OperationResponse(request.OperationCode) { ReturnCode = -1, DebugMessage = "Unknown operation code" + request.OperationCode };
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