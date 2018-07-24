// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Game.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the Game type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.LoadBalancing.GameServer
{
    #region using directives
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using ExitGames.Logging;
    using Photon.LoadBalancing.ServerToServer.Events;
    using Photon.SocketServer;
    using Photon.SocketServer.Rpc;
    using Photon.SocketServer.Rpc.Protocols;

    using OperationCode = Photon.LoadBalancing.Operations.OperationCode;
    using Photon.Hive;
    using Photon.Hive.Plugin;
    using Photon.Hive.Operations;
    using Hive.Diagnostics.OperationLogging;
    using Hive.Messages;
    using Hive.Common;
    using Photon.Common;
    using Hive.Common.Lobby;
    #endregion

    public class Game : HiveHostGame
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public readonly GameApplication Application;

        private static readonly HttpRequestQueueOptions DefaultHttpRequestQueueOptions = new HttpRequestQueueOptions(
            GameServerSettings.Default.HttpQueueMaxErrors,
            GameServerSettings.Default.HttpQueueMaxTimeouts,
            GameServerSettings.Default.HttpQueueRequestTimeout,
            GameServerSettings.Default.HttpQueueQueueTimeout,
            GameServerSettings.Default.HttpQueueMaxBackoffTime,
            GameServerSettings.Default.HttpQueueReconnectInterval,
            GameServerSettings.Default.HttpQueueMaxQueuedRequests,
            GameServerSettings.Default.HttpQueueMaxConcurrentRequests);

        private bool logRoomRemoval;

        private byte maxPlayers;

        private bool isVisible = true;

        private bool isOpen = true;

        private string lobbyId;

        private AppLobbyType lobbyType = AppLobbyType.Default;

        /// <summary>
        /// Contains the keys of the game properties hashtable which should be listet in the lobby.
        /// </summary>
        private HashSet<object> lobbyProperties;

        //private CombatLogic controller;
        public GameLogic controller;
        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="application">The photon application instance the game belongs to.</param>
        /// <param name="gameId">The game id.</param>
        /// <param name="roomCache">The room cache the game belongs to.</param>
        /// <param name="pluginManager">plugin creator</param>
        /// <param name="pluginName">Plugin name which client expects to be loaded</param>
        /// <param name="environment">different environment parameters</param>
        /// <param name="executionFiber">Fiber which will execute all rooms actions</param>
        public Game(
            GameApplication application,
            string gameId,
            Hive.Caching.RoomCacheBase roomCache = null,
            IPluginManager pluginManager = null,
            string pluginName = "",
            Dictionary<string, object> environment = null
            )
            : base(
                gameId,
                roomCache,
                null,
                GameServerSettings.Default.MaxEmptyRoomTTL,
                pluginManager,
                pluginName,
                environment,
                GameServerSettings.Default.LastTouchSecondsDisconnect * 1000,
                GameServerSettings.Default.LastTouchCheckIntervalSeconds * 1000,
                DefaultHttpRequestQueueOptions,
                GameApplication.Instance.executionFiber
            )
        {
            this.Application = application;

            if (this.Application.AppStatsPublisher != null)
            {
                this.Application.AppStatsPublisher.IncrementGameCount();
            }

            this.HttpForwardedOperationsLimit = GameServerSettings.Default.HttpForwardLimit;
        }

        public string LobbyId
        {
            get
            {
                return this.lobbyId;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (controller != null)
            {
                controller.Dispose(disposing);
                controller = null;
            }
            if (disposing)
            {
                this.RemoveGameStateFromMaster();

                if (GameApplication.Instance.AppStatsPublisher != null)
                {
                    GameApplication.Instance.AppStatsPublisher.DecrementGameCount();
                }
            }
        }

        // player leave the game
        //protected override void HandleLeaveOperation(HivePeer peer, LeaveRequest leaveRequest, SendParameters sendParameters)
        protected void HandleLeaveOperation(HivePeer peer, LeaveRequest leaveRequest, SocketServer.SendParameters sendParameters)
        {
            this.RemovePeerFromGame(peer, leaveRequest.IsCommingBack);
            // is always reliable, so it gets a response
            peer.SendOperationResponse(new OperationResponse { OperationCode = leaveRequest.OperationRequest.OperationCode }, sendParameters);
        }

        //public override void InitController(string levelname)
        public void InitController(string levelname)
        {
            controller = new GameLogic(this);
            controller.LogAI = LogAI;
            controller.Startup(levelname);
        }

        // broadcast eventdata to all actors in the room
        public int SendEventToAllActors(EventData data, bool reliable = true)
        {
            SocketServer.SendParameters para = new SocketServer.SendParameters();

            para.Unreliable = !reliable;

            foreach (Actor actor in ActorsManager)
            {
                if (actor.Peer.Connected)
                    actor.Peer.SendEvent(data, para);
            }
            return ActorsManager.Count;
        }

        private void SetupGameProperties(HivePeer peer, JoinGameRequest createRequest,
            Hashtable gameProperties, ref SocketServer.SendParameters sendParameters, out byte? newMaxPlayer, out bool? newIsOpen, out bool? newIsVisible, out object[] newLobbyProperties)
        {
            newMaxPlayer = null;
            newIsOpen = null;
            newIsVisible = null;
            newLobbyProperties = null;

            // special treatment for game and actor properties sent by AS3/Flash or JSON clients
            var protocol = peer.Protocol.ProtocolType;
            if (/*protocol == ProtocolType.Amf3V152 || */protocol == ProtocolType.Json)
            {
                Utilities.ConvertAs3WellKnownPropertyKeys(createRequest.GameProperties, createRequest.ActorProperties);
            }

            // try to parse build in properties for the first actor (creator of the game)
            if (this.ActorsManager.Count == 0)
            {
                if (gameProperties != null && gameProperties.Count > 0)
                {
                    if (!TryParseDefaultProperties(peer, createRequest, gameProperties,
                        sendParameters, out newMaxPlayer, out newIsOpen, out newIsVisible, out newLobbyProperties))
                    {
                        return;
                    }
                }
            }
            return;
        }

        protected override int RemovePeerFromGame(HivePeer peer, bool isCommingBack)
        {
            // if peer left game, remove him from combat logic
            if (controller != null)
                controller.RemovePlayer((GameClientPeer)peer);

            int result = base.RemovePeerFromGame(peer, isCommingBack);
            if (this.IsDisposed)
                return result;
            return result;
        }

        protected override void HandleGetPropertiesOperation(HivePeer peer, GetPropertiesRequest getPropertiesRequest, SocketServer.SendParameters sendParameters)
        {
            // special handling for game properties send by AS3/Flash (Amf3 protocol) clients or JSON clients
            if (/*peer.Protocol.ProtocolType == ProtocolType.Amf3V152 ||*/ peer.Protocol.ProtocolType == ProtocolType.Json)
            {
                Utilities.ConvertAs3WellKnownPropertyKeys(getPropertiesRequest.GamePropertyKeys, getPropertiesRequest.ActorPropertyKeys);
            }

            base.HandleGetPropertiesOperation(peer, getPropertiesRequest, sendParameters);
        }

        protected override void HandleSetPropertiesOperation(HivePeer peer, SetPropertiesRequest request, SocketServer.SendParameters sendParameters)
        {
            byte? newMaxPlayer = null;
            bool? newIsOpen = null;
            bool? newIsVisible = null;
            object[] newLobbyProperties = null;

            // try to parse build in propeties if game properties should be set (ActorNumber == 0)
            bool updateGameProperties = request.ActorNumber == 0 && request.Properties != null && request.Properties.Count > 0;

            // special handling for game and actor properties send by AS3/Flash (Amf3 protocol) or JSON clients
            if (/*peer.Protocol.ProtocolType == ProtocolType.Amf3V152 ||*/ peer.Protocol.ProtocolType == ProtocolType.Json)
            {
                if (updateGameProperties)
                {
                    Utilities.ConvertAs3WellKnownPropertyKeys(request.Properties, null);
                }
                else
                {
                    Utilities.ConvertAs3WellKnownPropertyKeys(null, request.Properties);
                }
            }

            if (updateGameProperties)
            {
                if (!TryParseDefaultProperties(peer, request, request.Properties, sendParameters, out newMaxPlayer, out newIsOpen, out newIsVisible, out newLobbyProperties))
                {
                    return;
                }
            }

            base.HandleSetPropertiesOperation(peer, request, sendParameters);

            // report to master only if game properties are updated
            if (!updateGameProperties)
            {
                return;
            }

            // set default properties
            if (newMaxPlayer.HasValue && newMaxPlayer.Value != this.maxPlayers)
            {
                this.maxPlayers = newMaxPlayer.Value;
            }

            if (newIsOpen.HasValue && newIsOpen.Value != this.isOpen)
            {
                this.isOpen = newIsOpen.Value;
            }

            if (newIsVisible.HasValue && newIsVisible.Value != this.isVisible)
            {
                this.isVisible = newIsVisible.Value;
            }

            if (newLobbyProperties != null)
            {
                this.lobbyProperties = new HashSet<object>(newLobbyProperties);
            }

            Hashtable gameProperties;
            if (newLobbyProperties != null)
            {
                // if the property filter for the app lobby properties has been changed
                // all game properties are resend to the master server because the application 
                // lobby might not contain all properties specified.
                gameProperties = this.GetLobbyGameProperties(this.Properties.GetProperties());
            }
            else
            {
                // property filter hasn't chjanged; only the changed properties will
                // be updatet in the application lobby
                gameProperties = this.GetLobbyGameProperties(request.Properties);
            }

            //this.UpdateGameStateOnMaster(newMaxPlayer, newIsOpen, newIsVisible, newLobbyProperties, gameProperties);
        }

        public override void ExecuteOperation(HivePeer peer, OperationRequest operationRequest, SocketServer.SendParameters sendParameters)
        {
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("Executing operation {0}", operationRequest.OperationCode);
            }

            switch (operationRequest.OperationCode)
            {
                // game logic operations
                case (byte)OperationCode.Combat:
                    (peer as GameClientPeer).ZRPC.CombatRPC.OnCommand(this.controller, peer, operationRequest, sendParameters);
                    break;

                case (byte)OperationCode.NonCombat:
                    (peer as GameClientPeer).ZRPC.NonCombatRPC.OnCommand(this.controller, peer, operationRequest, sendParameters);
                    break;

                case (byte)OperationCode.Action:
                    (peer as GameClientPeer).ZRPC.ActionRPC.OnAction(this.controller, peer, operationRequest, sendParameters);
                    break;

                case (byte)OperationCode.Lobby:
                    (peer as GameClientPeer).ZRPC.LobbyRPC.OnCommand(this.controller, peer, operationRequest, sendParameters);
                    break;

                //not valid for game.    
                case (byte)OperationCode.CreateGame:
                case (byte)OperationCode.JoinGame:
                //case (byte)OperationCode.Join:
                    break;

                case (byte)OperationCode.DebugGame:
                    var debugGameRequest = new DebugGameRequest(peer.Protocol, operationRequest);
                    if (peer.ValidateOperation(debugGameRequest, sendParameters) == false)
                    {
                        return;
                    }

                    if (this.LogQueue.Log.IsDebugEnabled)
                    {

                        this.LogQueue.Add(
                            new LogEntry(
                                "ExecuteOperation: " + (OperationCode)operationRequest.OperationCode,
                                "Peer=" + peer.ConnectionId));
                    }

                    this.HandleDebugGameOperation(peer, debugGameRequest, sendParameters);
                    break;

                // all other operation codes will be handled by the Lite game implementation
                default:
                    base.ExecuteOperation(peer, operationRequest, sendParameters);
                    break;
            }
        }

        protected override void ProcessMessage(IMessage message)
        {
            //try
            //{
            switch (message.Action)
            {
                //case (byte)GameMessageCodes.ReinitializeGameStateOnMaster:
                //    if (ActorsManager.Count == 0)
                //    {
                //        Log.WarnFormat("Reinitialize tried to update GameState with ActorCount = 0. " + this);
                //    }
                //    else
                //    {
                //        var gameProperties = this.GetLobbyGameProperties(this.Properties.GetProperties());
                //        object[] lobbyPropertyFilter = null;
                //        if (this.lobbyProperties != null)
                //        {
                //            lobbyPropertyFilter = new object[this.lobbyProperties.Count];
                //            this.lobbyProperties.CopyTo(lobbyPropertyFilter);
                //        }

                //        this.UpdateGameStateOnMaster(this.maxPlayers, this.isOpen, this.isVisible, lobbyPropertyFilter, gameProperties, null, null, true);
                //    }

                //    break;

                //case (byte)GameMessageCodes.CheckGame:
                //    this.CheckGame();
                //    break;

                default:
                    base.ProcessMessage(message);
                    break;
            }
            //}
            //catch (Exception ex)
            //{
            //    Log.Error(ex);
            //}
        }

        /// <summary>
        /// Check routine to validate that the game is valid (ie., it is removed from the game cache if it has no longer any actors etc.). 
        /// CheckGame() is called by the Application at regular intervals. 
        /// </summary>
        protected virtual void CheckGame()
        {
            if (this.ActorsManager.ActorsCount == 0 && this.RemoveTimer == null)
            {
                // double check if the game is still in cache: 
                Room room;
                if (this.Application.GameCache.TryGetRoomWithoutReference(this.Guid, out room))
                {
                    Log.WarnFormat("Game with 0 Actors is still in cache:'{0}'. Actors Dump:'{1}', RemovePath:'{2}'",
                        this.roomCache.GetDebugString(room.Name), this.ActorsManager.DumpActors(), this.RemoveRoomPathString);
                    this.logRoomRemoval = true;
                }
            }
        }

        protected virtual void UpdateGameStateOnMaster(
            byte? newMaxPlayer = null,
            bool? newIsOpen = null,
            bool? newIsVisble = null,
            object[] lobbyPropertyFilter = null,
            Hashtable gameProperties = null,
            string newPeerId = null,
            string removedPeerId = null,
            bool reinitialize = false)
        {
            if (reinitialize && ActorsManager.Count == 0)
            {
                Log.WarnFormat("Reinitialize tried to update GameState with ActorCount = 0. " + this);
                return;
            }

            var updateGameEvent = new UpdateGameEvent
            {
                //GameName = this.Name,
                GameId = this.Name,
                ActorCount = (byte)this.ActorsManager.Count,
                Reinitialize = reinitialize,
                MaxPlayers = newMaxPlayer,
                IsOpen = newIsOpen,
                IsVisible = newIsVisble,
                PropertyFilter = lobbyPropertyFilter
            };

            if (reinitialize)
            {
                updateGameEvent.LobbyId = this.lobbyId;
                updateGameEvent.LobbyType = (byte)this.lobbyType;
            }

            if (gameProperties != null && gameProperties.Count > 0)
            {
                updateGameEvent.GameProperties = gameProperties;
            }

            if (newPeerId != null)
            {
                updateGameEvent.NewUsers = new[] { newPeerId };
            }

            if (removedPeerId != null)
            {
                updateGameEvent.RemovedUsers = new[] { removedPeerId };
            }

            this.UpdateGameStateOnMaster(updateGameEvent);
        }

        protected virtual void UpdateGameStateOnMaster(UpdateGameEvent updateEvent)
        {
            //var eventData = new EventData((byte)ServerEventCode.UpdateGameState, updateEvent);
            //GameApplication.Instance.MasterPeer.SendEvent(eventData, new SocketServer.SendParameters());
        }

        protected virtual void RemoveGameStateFromMaster()
        {
            // GameApplication.Instance.MasterPeer.RemoveGameState(this.Name);
        }

        private static bool TryParseDefaultProperties(
            HivePeer peer, Operation operation, Hashtable propertyTable, SocketServer.SendParameters sendParameters, out byte? maxPlayer, out bool? isOpen, out bool? isVisible, out object[] properties)
        {
            string debugMessage;

            if (!GameParameterReader.TryReadDefaultParameter(propertyTable, out maxPlayer, out isOpen, out isVisible, out properties, out debugMessage))
            {
                var response = new OperationResponse { OperationCode = operation.OperationRequest.OperationCode, ReturnCode = (short)ErrorCode.OperationInvalid, DebugMessage = debugMessage };
                peer.SendOperationResponse(response, sendParameters);
                return false;
            }

            return true;
        }

        private Hashtable GetLobbyGameProperties(Hashtable source)
        {
            if (source == null || source.Count == 0)
            {
                return null;
            }

            Hashtable gameProperties;

            if (this.lobbyProperties != null)
            {
                // filter for game properties is set, only properties in the specified list 
                // will be reported to the lobby 
                gameProperties = new Hashtable(this.lobbyProperties.Count);

                foreach (var entry in this.lobbyProperties)
                {
                    if (source.ContainsKey(entry))
                    {
                        gameProperties.Add(entry, source[entry]);
                    }
                }
            }
            else
            {
                // if no filter is set for properties which should be listet in the lobby
                // all properties are send
                gameProperties = source;
                gameProperties.Remove((byte)GameParameter.MaxPlayers);
                gameProperties.Remove((byte)GameParameter.IsOpen);
                gameProperties.Remove((byte)GameParameter.IsVisible);
                gameProperties.Remove((byte)GameParameter.LobbyProperties);
            }

            return gameProperties;
        }

        #region Refactor Code
        public void OnJoinRoom(HivePeer peer)
        {
            if (controller == null)
                InitController(this.Name);
            if (this.IsDisposed)
            {
                // join arrived after being disposed - repeat join operation                
                if (Log.IsWarnEnabled)
                {
                    Log.WarnFormat("Join operation on disposed game. GameName={0}", this.Name);
                }
            }

            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("Join operation from IP: {0} to port: {1}", peer.RemoteIP, peer.LocalPort);
            }

            // create an new actor
            Actor actor;
            bool isnew = false;
            ErrorCode err = 0;
            string reason;
            if (this.TryAddPeerToGame(peer, 0, out actor, out isnew, out err, out reason, null) == false)
            {
            }

            // check if a room removal is in progress and cancel it if so
            if (this.RemoveTimer != null)
            {
                this.RemoveTimer.Dispose();
                this.RemoveTimer = null;
            }

            string prevRoomName = peer.mPrevRoomName;
            if (prevRoomName == "lobby" || prevRoomName == "")
                (peer as GameClientPeer).ZRPC.LobbyRPC.LoadLevel(Name, peer);
            else
                (peer as GameClientPeer).ZRPC.CombatRPC.LoadLevel(Name, peer);
            if (Name == "lobby")
                peer.IsJoinedRoom = true;
            //if (log.IsDebugEnabled)
            //    log.DebugFormat("OnJoinRoom old={0}, new={1}", prevRoomName, Name);
            log.InfoFormat("OnJoinRoom fr={0} to={1}", prevRoomName, Name);
        }

        public override bool BeforeRemoveFromCache(bool removeDueDc)
        {
            if (controller != null && controller.mRealmController != null)
            {
                if (controller.mRealmController.OnAllPeerRemoved(removeDueDc))
                    return base.BeforeRemoveFromCache(removeDueDc);

                return false;
            }
            return base.BeforeRemoveFromCache(removeDueDc);
        }
        #endregion
    }
}