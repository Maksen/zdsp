using System;
using System.Collections.Generic;
using ExitGames.Logging;

using Photon.LoadBalancing.Operations;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using Zealot.RPC;
using Zealot.Common;
using Zealot.Repository;
using Photon.Hive;
using Photon.Common;

namespace Photon.LoadBalancing.GameServer
{
    public class QAClientPeer : HivePeer
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly GameApplication application;

        #region Properties
        public string PeerId { get; protected set; }
        public DateTime LastActivity { get; protected set; }
        public byte LastOperation { get; protected set; }
        public bool Authenticated { get; private set; }
        #endregion

        public QAClientPeer(InitRequest initRequest, GameApplication application)
            : base(initRequest)
        {
            this.application = application;
            this.PeerId = string.Empty;
        }

        /// <summary>
        ///   Called when client disconnects.
        ///   Ensures that disconnected players leave the game <see cref = "Room" />.
        ///   The player is not removed immediately but a message is sent to the room. This avoids
        ///   threading issues by making sure the player remove is not done concurrently with operations.
        /// </summary>
        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("OnDisconnect: conId={0}, reason={1}, reasonDetail={2}", this.ConnectionId, reasonCode, reasonDetail);
            }

            GameApplication.RemoveQAPeer(this);
        }

        /// <summary>
        ///   Called when the client sends an <see cref = "OperationRequest" />.
        /// </summary>
        /// <param name = "operationRequest">
        ///   The operation request.
        /// </param>
        /// <param name = "sendParameters">
        ///   The send Parameters.
        /// </param>
        protected override void OnOperationRequest(OperationRequest request, SendParameters sendParameters)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("OnOperationRequest. Code={0}", request.OperationCode);
            }

            switch (request.OperationCode)
            {
                case (byte)OperationCode.Authenticate:
                    this.HandleAuthenticateOperation(request, sendParameters);
                    return;

                case (byte)OperationCode.AuthenticateCookie:
                    this.HandleAuthenticateCookie(request, sendParameters);
                    return;

                case (byte)OperationCode.QATools:
                    this.HandleQAOperation(request, sendParameters);
                    break;
            }

            //string message = string.Format("Unknown operation code {0}", request.OperationCode);
            //this.SendOperationResponse(new OperationResponse { OperationCode = request.OperationCode, ReturnCode = -1, DebugMessage = message }, sendParameters);
        }

        /// <summary>
        /// mirrors gameclientpeer code for now
        /// </summary>
        /// <param name="operationRequest"></param>
        /// <param name="sendParameters"></param>
        protected virtual void HandleAuthenticateOperation(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var request = new AuthenticateRequest(this.Protocol, operationRequest);
            if (this.ValidateOperation(request, sendParameters) == false)
            {
                return;
            }

            //TODO: check cookie
            this.Authenticated = true;

            if (request.UserId != null)
            {
                this.PeerId = request.UserId;
            }

            var response = new OperationResponse { OperationCode = operationRequest.OperationCode };
            this.SendOperationResponse(response, sendParameters);
        }

        protected void HandleAuthenticateCookie(OperationRequest operationRequest, SendParameters sendParameters)
        {
            string incomingCookie = operationRequest.Parameters[(byte)ParameterCode.CookieId].ToString();
            //string userid = operationRequest.Parameters[(byte)ParameterCode.UniqueId].ToString();
            short returnCode = (short)ErrorCode.Ok;
            var opParameter = new Dictionary<Byte, Object>();
            opParameter.Add((byte)ParameterCode.ApplicationId, "QATools");
            var response = new OperationResponse
            {
                OperationCode = operationRequest.OperationCode,
                Parameters = opParameter,
                ReturnCode = returnCode
            };
            this.SendOperationResponse(response, sendParameters);
        }

        protected virtual void HandleQAOperation(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //ZRPC.QAToolsRPC.OnCommand(this, this, operationRequest, sendParameters);
        }

        public OperationResponse GetCharacters(HivePeer peer)
        {

            return null;
        }

        public OperationResponse AddInventoryItem(string charname, int itemid, HivePeer peer)
        {
            IInventoryItem item = GameRepo.ItemFactory.CreateItemInstance(itemid);

            return null;
        }

        public OperationResponse GetInventory(string charname, HivePeer peer)
        {
            return null;
        }
    }
}
