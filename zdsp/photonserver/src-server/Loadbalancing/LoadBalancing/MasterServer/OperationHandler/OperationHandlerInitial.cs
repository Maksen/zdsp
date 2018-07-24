// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationHandlerInitial.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the OperationHandlerInitial type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Photon.LoadBalancing.Common;

namespace Photon.LoadBalancing.Master.OperationHandler
{
    #region using directives

    using ExitGames.Logging;

    using Photon.LoadBalancing.MasterServer;
    using Photon.LoadBalancing.Operations;
    using Photon.SocketServer;
    

    #endregion

    public class OperationHandlerInitial : OperationHandlerBase
    {
        #region Constants and Fields

        public static readonly OperationHandlerInitial Instance = new OperationHandlerInitial();

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Methods

        public override OperationResponse OnOperationRequest(PeerBase peer, OperationRequest operationRequest, SendParameters sendParameters)
        {
            OperationResponse response;
            switch (operationRequest.OperationCode)
            {
                default:
                    response = HandleUnknownOperationCode(operationRequest, log);
                    break;

                case (byte)OperationCode.Authenticate:
                    response = ((MasterClientPeer)peer).HandleAuthenticateAsync(operationRequest, sendParameters).GetAwaiter().GetResult();
                    break;
                case (byte)OperationCode.CreateGame:
                case (byte)OperationCode.JoinGame:
                case (byte)OperationCode.JoinLobby:
                case (byte)OperationCode.JoinRandomGame:
                case (byte)OperationCode.FindFriends:
                case (byte)OperationCode.LobbyStats:
                case (byte)OperationCode.LeaveLobby:
                case (byte)OperationCode.DebugGame:
                case (byte)OperationCode.Rpc:
                    response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = (int)Photon.Common.ErrorCode.OperationDenied, 
                        DebugMessage = LBErrorMessages.NotAuthorized
                    };
                    break;
            }

            return response;
        }

        #endregion
    }
}