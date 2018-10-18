// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameClientPeer.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the GamePeer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.LoadBalancing.GameServer
{
    #region using directives
    using ExitGames.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using ItemMall;
    using Mail;
    using OfflineExp;
    using Photon.LoadBalancing.Operations;
    using OperationCode = Operations.OperationCode;
    using Kopio.JsonContracts;
    using Zealot.Common;
    using Zealot.Common.Entities;
    using Zealot.Common.RPC;
    using Zealot.Repository;
    using Zealot.RPC;
    using Zealot.Server.Counters;
    using Zealot.Server.Entities;
    using Zealot.Server.Rules;
    using Hive;
    using Photon.SocketServer;
    using Hive.Caching;
    using Photon.Common;
    using Zealot.Entities;
    #endregion

    public class GameClientPeer : HivePeer
    {
        #region Constants and Fields

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly GameApplication application;
        
        #endregion

        #region Constructors and Destructors

        public ZRPC ZRPC;

        public GameClientPeer(InitRequest initRequest, GameApplication application)
            : base(initRequest)
        {
            this.application = application;
            this.PeerId = string.Empty;

            if (application.AppStatsPublisher != null)
            {
                application.AppStatsPublisher.IncrementPeerCount();
            }
            Valid = false;
            IsPeerDisconnect = false;
            IsExitToLobby = false;
            ZRPC = new ZRPC();
        }

        #endregion

        #region Properties

        private bool IsPeerDisconnect { get; set; }
        private bool IsExitToLobby { get; set; }
        private string TransferRoom_TargetRoomGuid { get; set; }

        public bool IsExitGame()
        {
            return IsPeerDisconnect || IsExitToLobby;
        }

        public void ResetExitGame()
        {
            IsPeerDisconnect = false;
            IsExitToLobby = false;
        }

        public bool Valid { get; protected set; }
        public string PeerId { get; protected set; }
        public DateTime LastActivity { get; protected set; }
        public byte LastOperation { get; protected set; }
        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                "{0}: {1}",
                this.GetType().Name,
                string.Format(
                    "PID {0}, IsConnected: {1}, IsDisposed: {2}, Last Activity: Operation {3} at UTC {4} in Room {7}, IP {5}:{6}, ",
                    this.ConnectionId,
                    this.Connected,
                    this.Disposed,
                    this.LastOperation,
                    this.LastActivity,
                    this.RemoteIP,
                    this.RemotePort,
                    this.RoomReference == null ? string.Empty : this.RoomReference.Room.Guid));
        }

        public void TransferRoom(string roomGuid, string levelName)
        {
            if (TransferRoom_TargetRoomGuid == roomGuid || (RoomReference != null && RoomReference.Room.Name == levelName))
            {
                log.InfoFormat("TransferRoom same room {0} - {1} - {2} - {3}", mChar, levelName, roomGuid, ConnectionId);
                return;
            }
            HandleJoinGameOperation(roomGuid, levelName);
            mSpawnPos = Vector3.zero;
            mSpawnForward = Vector3.forward;
            mInspectMode = false;
        }

        public string CreateRealm(int realmId, string levelName, bool logAI = false)
        {
            if (RoomReference == null)
            {
                log.InfoFormat("CreateRealm No RoomReference {0} - {1} - {2} - {3}", mChar, levelName, realmId, ConnectionId);
                return "";
            }
            else if (RoomReference.Room.Name == levelName)
            {
                log.InfoFormat("CreateRealm same room {0} - {1} - {2} - {3}", mChar, levelName, realmId, ConnectionId);
                return "";
            }
            mSpawnPos = Vector3.zero;
            mSpawnForward = Vector3.forward;
            mInspectMode = false;
            return HandleCreateGameOperation(realmId, levelName, logAI);
        }

        public void LeaveRealm()
        {
            if (mPlayer == null || !IsJoinedRoom)
                return;

            TransferToCity(mPlayer.PlayerSynStats.Level);
        }

        public void TransferToDefaultLevel(string levelName)
        {
            string roomGuid = application.GameCache.TryGetDefaultRoomGuid(levelName);
            if (!string.IsNullOrEmpty(roomGuid))
                TransferRoom(roomGuid, levelName);
        }

        public bool TransferToRealmWorld(string levelName)
        {
            RealmWorldJson realmWorldJson = RealmRepo.GetWorldByName(levelName);
            if (realmWorldJson != null) // Check if level name is realm world
            {
                int realmId = realmWorldJson.id;
                string roomGuid = application.GameCache.TryGetRealmRoomGuid(realmId, realmWorldJson.maxplayer);
                if (!string.IsNullOrEmpty(roomGuid))
                    TransferRoom(roomGuid, levelName);
                else
                    CreateRealm(realmId, levelName);

                return true;
            }
            return false;
        }

        public void TransferToCity(int progresslvl)
        {
            TransferToRealmWorld(RealmRepo.GetCity(progresslvl));
        }

        public void ExitGame()
        {
            IsExitToLobby = true;
            TransferToDefaultLevel("lobby");
        }

        public void OnPlayerRemovedFromRoom()
        {
            if (IsPeerDisconnect)
            {
                disconnectPeer();
                //log.InfoFormat("PlayerRemoved {0} EnqueueDc", mChar);
                //System.Diagnostics.Debug.WriteLine("Enqueue Dc");
            }
            else
            {
                if (IsExitToLobby)
                {
                    ClearChar();
                }
                Room room;
                application.GameCache.TryGetRoomWithoutReference(TransferRoom_TargetRoomGuid, out room);
                //System.Diagnostics.Debug.WriteLine("Enqueue " + room.Name);
                //log.InfoFormat("PlayerRemoved {0} Enqueue - {1}", mChar, room.Name);
                ((Game)room).OnJoinRoom(this);
            }
        }

        #endregion

        #region Methods

        protected virtual string HandleCreateGameOperation(int realmId, string levelName, bool logAI)
        {
            // Try to create the game
            RoomReference gameReference;
            Room room;
            if (application.GameCache.TryCreateRealmRoom(realmId, levelName, RealmRepo.IsWorld(levelName),
                this, out room, out gameReference))
            {
                log.InfoFormat("CreateRoom {0} - {1} - {2}", mChar, levelName, realmId);
                string guid = room.Guid;
                TransferRoom_TargetRoomGuid = guid;
                room.LogAI = logAI;
                string currentLevelName = this.RoomReference.Room.Name;
                if (currentLevelName == "lobby")
                    ZRPC.LobbyRPC.TransferRoom(levelName, this);
                else
                    ZRPC.CombatRPC.TransferRoom(levelName, this);
                this.RemovePeerFromCurrentRoom(0, "Join room"); // remove peer from current game
                this.RoomReference = gameReference; // Overwrite current room reference
                return guid;
            }
            return "";
        }

        public virtual void HandleJoinGameOperation(string roomGuid, string levelName)
        {
            RoomReference gameReference;
            if (application.GameCache.TryGetRoomReference(roomGuid, this, out gameReference))
            {
                log.InfoFormat("JoinRoom {0} - {1} - {2} - {3}", mChar, levelName, roomGuid, ConnectionId);
                TransferRoom_TargetRoomGuid = roomGuid;
                if (this.RoomReference != null) // If peer is currently in a room
                {
                    string currentLevelName = this.RoomReference.Room.Name;
                    if (currentLevelName == "lobby")
                        ZRPC.LobbyRPC.TransferRoom(levelName, this);
                    else
                        ZRPC.CombatRPC.TransferRoom(levelName, this);
                    this.RemovePeerFromCurrentRoom(0, "Join game");
                    this.RoomReference = gameReference; // Overwrite current room reference 
                }
                else
                {
                    this.RoomReference = gameReference; // Overwrite current room reference 
                    ((Game)gameReference.Room).OnJoinRoom(this);
                }
            }
        }

        protected override void HandleLeaveOperation(OperationRequest operationRequest, SendParameters sendParameters)
        {
            RemoveFromGame();
        }

        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat("OnDisconnect: conId={0}, reason={1}", this.ConnectionId, reasonCode);
            if (application.AppStatsPublisher != null)
                application.AppStatsPublisher.DecrementPeerCount();
            GameCounters.ExecutionFiberQueue.Increment();
            application.executionFiber.Enqueue(() =>
            {
                GameCounters.ExecutionFiberQueue.Decrement();
                IsPeerDisconnect = true;
                if (RoomReference != null)
                {
                    RoomReference.removeDueDc = true;
                    RemoveFromGame();
                }
                else
                    disconnectPeer();
            });
        }

        private void RemoveFromGame()
        {
            RemovePeerFromCurrentRoom(0, "");
        }

        protected void disconnectPeer()
        {
            ClearChar();
            application.RemoveUserPeer(mUserId, this);
        }

        protected override void OnOperationRequest(OperationRequest request, SendParameters sendParameters)
        {
#if DEBUG
            if (log.IsDebugEnabled)
            {
                if (request.OperationCode != (byte)Hive.Operations.OperationCode.RaiseEvent)
                {
                    string paramstring = "";

                    foreach (var param in request.Parameters)
                    {
                        paramstring += param.Value.ToString() + " ";
                    }
                    //log.DebugFormat("OnOperationRequest: conId={0}, opCode={1}, {2} Parameters: " + paramstring, this.ConnectionId, request.OperationCode, DateTime.Now);
                    log.DebugFormat("OnOperationRequest: conId={0}, opCode={1}, {2} Parameters: {3}", this.ConnectionId, request.OperationCode, DateTime.Now, paramstring);
                }
            }
#endif
            GameCounters.ExecutionFiberQueue.Increment();
            application.executionFiber.Enqueue(() =>
            {
                GameCounters.ExecutionFiberQueue.Decrement();
                this.LastActivity = DateTime.UtcNow;
                this.LastOperation = request.OperationCode;
                switch (request.OperationCode)
                {
                    case (byte)OperationCode.Combat:
                    case (byte)OperationCode.NonCombat:
                    case (byte)OperationCode.UnreliableCombat:
                    case (byte)OperationCode.Lobby:
                    case (byte)OperationCode.Action:
                        this.HandleCombatOperation(request, sendParameters);
                        return;

                    case (byte)OperationCode.AuthenticateCookie:
                        var task = this.HandleAuthenticateCookie(request, sendParameters);
                        return;

                    case (byte)OperationCode.JoinGame:
                        // Only enter lobby use this
                        this.TransferToDefaultLevel("lobby");
                        return;

                    case (byte)Operations.OperationCode.Leave:
                        this.HandleLeaveOperation(request, sendParameters);
                        return;

                    case (byte)Operations.OperationCode.Ping:
                        this.HandlePingOperation(request, sendParameters);
                        return;
                }

                string message = string.Format("Unknown operation code {0}", request.OperationCode);
                var response = new OperationResponse { ReturnCode = (short)ErrorCode.OperationDenied, DebugMessage = message, OperationCode = request.OperationCode };
                this.SendOperationResponse(response, sendParameters);
            });
        }

        public virtual string GetRoomCacheDebugString(string gameId)
        {
            return application.GameCache.GetDebugString(gameId);
        }

        protected virtual void HandleCombatOperation(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (RoomReference == null)
            {
                if (log.IsDebugEnabled)
                    log.DebugFormat("Receive {0} no room reference.", operationRequest.OperationCode);
                return;
            }
            if (IsJoinedRoom)
            {
                if (RoomReference.Room.Name == "lobby" && operationRequest.OperationCode != (byte)OperationCode.Lobby)
                {
                    if (log.IsDebugEnabled)
                        log.DebugFormat("Receive {0} in lobby", operationRequest.OperationCode);
                    return;
                }
            }
            else
            {
                if (!(operationRequest.OperationCode == (byte)OperationCode.Combat &&
                    (byte)ClientCombatRPCMethods.OnClientLevelLoaded == (byte)operationRequest.Parameters[1]))
                {
                    if (log.IsDebugEnabled)
                        log.DebugFormat("Receive {0} before player spawn.", operationRequest.OperationCode);
                    return;
                }
            }
            RoomReference.Room.ExecuteOperation(this, operationRequest, sendParameters);
        }

        protected async Task HandleAuthenticateCookie(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var request = new AuthenticateCookieRequest(this.Protocol, operationRequest);
            if (this.ValidateOperation(request, sendParameters) == false)
            {
                return;
            }
            var response = new OperationResponse
            {
                OperationCode = operationRequest.OperationCode,
                ReturnCode = (short)ErrorCode.Ok
            };

            //int maxplayer = application.MyServerConfig.maxplayer;
            ////can set to 0 to prevent players from entering a gameserver, -1 indicates infinite players, 0 and above indicates valid limit
            //int totalPlayers = application.GetOnlinePlayerCount();
            //if (maxplayer >= 0 && totalPlayers >= maxplayer)
            //{
            //    response.ReturnCode = (short)ErrorCode.GameFull;
            //    this.SendOperationResponse(response, sendParameters);
            //    return;
            //}

            string userid = request.UserId;
            string incomingCookie = request.Cookie;
            if (string.IsNullOrEmpty(incomingCookie) || string.IsNullOrEmpty(userid)
                || incomingCookie != application.GetCookie(userid))
            {
                response.ReturnCode = (short)ErrorCode.InvalidCookie;
                this.SendOperationResponse(response, sendParameters);
                return;
            }

            List<Dictionary<string, object>> _userinfo = await GameApplication.dbGM.PlayerAccount.GetAccountByUserIdAsync(userid);
            if (this != null) //still connected
            {
                application.executionFiber.Enqueue(() =>
                {
                    if (_userinfo.Count != 1 || incomingCookie != (string)_userinfo[0]["cookie"])
                    {
                        response.ReturnCode = (short)ErrorCode.InvalidCookie;
                        this.SendOperationResponse(response, sendParameters);
                        return;
                    }
                    object dtfreeze = _userinfo[0]["dtfreeze"];
                    DateTime freezedt = dtfreeze == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dtfreeze);
                    if (DateTime.Now < freezedt)
                    {
                        response.ReturnCode = (short)ErrorCode.UserBlocked;
                        this.SendOperationResponse(response, sendParameters);
                        return;
                    }

                    GameClientPeer peer = application.GetUserPeerByUserid(userid);
                    if (peer != null)
                        peer.Disconnect();
                    application.AddUserPeer(userid, this);
                    this.mUserId = userid;
                    this.PeerId = userid;

                    var opParameters = new Dictionary<byte, object>();
                    opParameters.Add((byte)ParameterCode.VoiceChatAddress, application.MyServerConfig.voicechat);
                    response.Parameters = opParameters;
                    this.SendOperationResponse(response, sendParameters);
                });
            }
        }

        private void OnJoinFailedInternal(ErrorCode result)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("OnJoinFailed: {0}", result);
            }

            // if join operation failed -> release the reference to the room
            if (result != ErrorCode.Ok && this.RoomReference != null)
            {
                RemovePeerFromCurrentRoom((int)result, "");
            }
        }
        #endregion

        #region Character Persistent

        private CharacterData characterData;
        public CharacterData CharacterData { get { return characterData; } set { characterData = value; } }
        private List<Dictionary<string, object>> characterList;
        public List<Dictionary<string, object>> CharacterList { get { return characterList; } set { characterList = value; } }

        private string charID = "";
        public string mLogin = "";
        public string mChar = "";
        public string mUserId = "";
        public DateTime mDTMute;
        public Vector3 mSpawnPos = Vector3.zero;
        public Vector3 mSpawnForward = Vector3.forward;
        public bool mInspectMode = false;
        public Player mPlayer;
        public TransferServerInfo mTransferServerInfo = null;

        public DateTime mLastSaveCharacterDT;
        public DateTime createdDT;
        public DateTime logoutDT;
        public DateTime loginDT;
        public bool mCanSaveDB = false;
        private DateTime lastLogLogin;
        public bool mFirstLogin = true;
        public LocationData mPortalExit = null;

        public ServerSettingsData GameSetting { get; set; }
        public int ArenaRankToChallenge { get; set; }
        public bool IsPartyInvitePending { get; set; }

        public ItemInventoryController mInventory;
        public WelfareController mWelfareCtrlr;
        public SevenDaysController mSevenDaysController;
        public QuestExtraRewardsController mQuestExtraRewardsCtrler;
        public SocialInventoryData mSocialInventory;
        public PowerUpController mPowerUpController;
        public EquipmentCraftController mEquipmentCraftController;
        public EquipFusionController mEquipFusionController;

        #region CharacterData
        public void SetChar(string charName) // Set char and charinfo.
        {
            characterData = null;
            if (characterList != null)
            {
                foreach (Dictionary<string, object> charinfo in characterList)
                {
                    if (charName.Equals((string)charinfo["charname"]))
                    {
                        charID = charinfo["charid"].ToString();
                        string strCharData = (string)charinfo["characterdata"];
                        characterData = CharacterData.DeserializeFromDB(strCharData);
                        if (characterData != null)
                        {
                            mChar = charName;
                            application.AddCharPeer(charName, this);
                            mLastSaveCharacterDT = DateTime.Now;
                            createdDT = (DateTime)charinfo["dtcreated"];
                            logoutDT = (DateTime)charinfo["dtlogout"];
                            loginDT = DateTime.Now;
                            //InitCharDataFirst(charinfo);
                            mInventory = new ItemInventoryController(this);
                            mPowerUpController = new PowerUpController(this);
                            mEquipmentCraftController = new EquipmentCraftController(this);
                            mEquipFusionController = new EquipFusionController(this);
                            mDTMute = (DateTime)charinfo["dtmute"];
                            LadderRules.OnPlayerOnline(charName, (string)charinfo["arenareport"]);

                            string gameSettingStr = (string)charinfo["gamesetting"];
                            GameSetting = string.IsNullOrEmpty(gameSettingStr)
                                ? new ServerSettingsData() : ServerSettingsData.Deserialize(gameSettingStr);

                            var ignoreAwait = InitSocialInventory((string)charinfo["friends"], (string)charinfo["friendrequests"]);

                            lastLogLogin = logoutDT;
                            LogLogin(createdDT == logoutDT);
                        }
                        log.InfoFormat("SetChar {0}", charName);
                        break;
                    }
                }
            }
        }

        private void LogLogin(bool alwayslog)
        {
            if (alwayslog || lastLogLogin.Date < DateTime.Today)
            {
                string message = string.Format("charName:{0}", mChar);
                Zealot.Logging.Client.LogClasses.LoginChar loginCharLog = new Zealot.Logging.Client.LogClasses.LoginChar();
                loginCharLog.userId = mUserId;
                loginCharLog.charId = charID;
                loginCharLog.message = message;
                loginCharLog.charName = mChar;
                var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(loginCharLog);
                lastLogLogin = DateTime.Now;
            }
        }

        public void InitCharDataFirst(Dictionary<string, object> charInfo)
        {
            characterData.GuildId = (int)charInfo["guildid"];
            characterData.GuildRank = (byte)charInfo["guildrank"];
            characterData.LeaveGuildCDEndTick = (charInfo["guildcdenddt"] != DBNull.Value) ? ((DateTime)charInfo["guildcdenddt"]).Ticks : 0;

            //characterData.FirstBuyFlag = (int)charInfo["firstbuyflag"];
            //characterData.FirstBuyCollected = (int)charInfo["firstbuycollected"];

            //characterData.CurrencyInventory.Money = (int)charInfo["money"];
            //characterData.CurrencyInventory.Gold = (int)charInfo["gold"];
            //characterData.CurrencyInventory.BindGold = (int)charInfo["bindgold"];
        }

        public string GetCharId()
        {
            return charID;
        }

        // this is the final task to clear char and persist to InstanceDB
        public void ClearChar()
        {
            if (mChar != "")
            {
                IsExitToLobby = false;
                logoutDT = DateTime.Now;
                LadderRules.OnPlayerOffline(mChar);
                // Save current online duration
                //characterData.WelfareInventory.OnlineDuration += mWelfareCtrlr.GetOnlineDuration();

                // Save logout time                   
                GuildRules.OnCharacterLogout(characterData, logoutDT);
                PartyRules.OnCharacterOffline(mChar);
                SaveCharacter();
                application.RemoveCharPeer(mChar, this);
                log.InfoFormat("ClearChar {0}", mChar);
                mChar = "";
                charID = "";
                characterData = null;
                mCanSaveDB = false;
            }
        }

        public void ForceSaveCharacter()
        {
            if (mPlayer != null)
                mPlayer.ForceSaveCharacter();
        }

        public void SaveCharacter()
        {
            if (!mCanSaveDB)
                return;
            mLastSaveCharacterDT = DateTime.Now;
            LogLogin(false);
            var saved = SaveUserAndCharacterData();
        }

        public void SaveCharacterForRemoveCharacter(string charid, string charname, CharacterData characterData)
        {
            mLastSaveCharacterDT = DateTime.Now;
            LogLogin(false);
            var saved = SaveCharacterData(charid, charname, characterData);
        }

        public bool IsTransferingRoom()
        {
            return mChar != "" && mPlayer == null;
        }

        private void CleanCharacterList()
        {
            characterList.Clear();
            characterList = null;
        }

        public int GetCharacterCount()
        {
            if (characterList != null)
                return characterList.Count;
            return 0;
        }

        public int GetProgressLvl()
        {
            if (mPlayer != null)
                return mPlayer.GetAccumulatedLevel();
            else if (characterData != null)
                return characterData.ProgressLevel;
            return 1;
        }

        private async Task<bool> SaveCharacterData(string charid, string charname, CharacterData cd)
        {
            string serializedData = cd.SerializeForDB();
            var saved = await GameApplication.dbRepository.Character.SaveCharacterData(charid, serializedData);
            log.InfoFormat("save char data {0} {1}", charname, saved);
            return saved;
        }

        private async Task<bool> SaveUserAndCharacterData()
        {
            string charname = mChar;
            string serializedData = characterData.SerializeForDB();
            // Clear the flag before await
            var saved = await GameApplication.dbRepository.Character.SaveCharacterAndUserAsync(charID, characterData.Experience, characterData.ProgressLevel,
                                                                        characterData.EquipScore, characterData.portraitID,
                                                                        characterData.CurrencyInventory.Money, characterData.CurrencyInventory.Gold, characterData.CurrencyInventory.BindGold,
                                                                        characterData.GuildId, characterData.GuildRank,
                                                                        0,
                                                                        characterData.CurrencyInventory.GuildFundToday,
                                                                        characterData.CurrencyInventory.GuildFundTotal,
                                                                        characterData.FactionKill, characterData.FactionDeath,
                                                                        0, 0,
                                                                        0, 0,
                                                                        new DateTime(characterData.LeaveGuildCDEndTick),
                                                                        SocialInvToString(mSocialInventory.friendList),
                                                                        SocialInvToString(mSocialInventory.friendRequestList),
                                                                        characterData.FirstBuyFlag, characterData.FirstBuyCollected,
                                                                        GameSetting.Serialize(),
                                                                        serializedData, loginDT, logoutDT);
            log.InfoFormat("save char {0} {1}", charname, saved);
            return saved;
        }
        #endregion

        #region Social        
        private async Task InitSocialInventory(string friendListStr, string friendRequestListStr)
        {
            mSocialInventory = new SocialInventoryData();
            if (string.IsNullOrEmpty(friendListStr) && string.IsNullOrEmpty(friendRequestListStr))
                return;

            string[] splitList = friendListStr.Split('|');
            int splitListLen = splitList.Length;
            if (splitListLen > 0)
            {
                List<string> names = new List<string>();
                for (int i = 0; i < splitListLen; ++i)
                {
                    string listInfo = splitList[i];
                    if (string.IsNullOrEmpty(listInfo))
                        continue;
                    names.Add(GetNameFromSocialInfo(listInfo));
                }
                // Update friends to latest info from DB
                List<Dictionary<string, object>> dbInfoList = await GameApplication.dbRepository.Character.GetSocialByNames(names);
                application.executionFiber.Enqueue(() => {
                    if (dbInfoList.Count > 0)
                    {
                        SocialInfo socialInfo = new SocialInfo();
                        int count = dbInfoList.Count;
                        for (int i = 0; i < count; ++i)
                        {
                            Dictionary<string, object> dbInfo = dbInfoList[i];
                            socialInfo.charName = dbInfo["charname"] as string;
                            socialInfo.portraitId = (int)dbInfo["portraitid"];
                            socialInfo.jobSect = (byte)dbInfo["jobsect"];
                            socialInfo.vipLvl = (byte)dbInfo["viplevel"];
                            socialInfo.charLvl = (int)dbInfo["progresslevel"];
                            socialInfo.combatScore = (int)dbInfo["combatscore"];
                            socialInfo.faction = (byte)dbInfo["faction"];
                            socialInfo.guildName = GuildRules.GetGuildNameById((int)dbInfo["guildid"]);
                            mSocialInventory.friendList.Add(socialInfo.ToString());
                        }
                    }
                });
            }
            splitList = friendRequestListStr.Split('|');
            splitListLen = splitList.Length;
            for (int i = 0; i < splitListLen; ++i)
            {
                string listInfo = splitList[i];
                if (string.IsNullOrEmpty(listInfo))
                    continue;
                mSocialInventory.friendRequestList.Add(listInfo);
            }
        }

        private string GetNameFromSocialInfo(string info)
        {
            if (!string.IsNullOrEmpty(info))
            {
                int sepIdx = info.IndexOf('`');
                if (sepIdx != -1)
                    return info.Substring(0, sepIdx);
            }
            return "";
        }

        private string SocialInvToString(IList<string> list)
        {
            int listCnt = list.Count;
            if (listCnt == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < listCnt; ++i)
            {
                if (i != 0)
                    sb.Append('|');
                sb.Append(list[i]);
            }
            return sb.ToString();
        }

        public void SendInspectPlayerInfo(GameClientPeer recievingPeer)
        {
            if (mPlayer == null)
                return;

            PlayerSynStats playerSynStats = mPlayer.PlayerSynStats;
            CharacterInspectData avatarData = new CharacterInspectData()
            {
                Name = mChar,
                JobSect = playerSynStats.jobsect,
                EquipmentInventory = characterData.EquipmentInventory,
                ProgressLevel = playerSynStats.Level,
                Faction = playerSynStats.faction,
                Guild = playerSynStats.guildName,
                //VIP = playerSynStats.vipLvl,
                //CombatScore = mPlayer.LocalCombatStats.CombatScore,
                Experience = mPlayer.SecondaryStats.experience
            };
            avatarData.InspectCombatStats.Update(mPlayer.LocalCombatStats);
            ZRPC.CombatRPC.SetInspectPlayerInfo(avatarData.SerializeForCharCreation(), recievingPeer);
        }
        
        public string GetOnlinePlayerInfoByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            /*SocialInfo socialInfo = new SocialInfo();
            // Find name in SocialStats
            string strData = "", result = "";
            int idx = -1;
            int socialType = GetNameInSocialStats(name, ref strData, ref idx);
            GameClientPeer peer = GameApplication.GetCharPeer(name);
            if (peer != null && peer.mPlayer != null) // Online
            {
                PlayerSynStats playerSynStats = peer.mPlayer.PlayerSynStats;
                socialInfo.name = name;
                socialInfo.jobsect = playerSynStats.jobsect;
                socialInfo.rank = playerSynStats.rank;
                socialInfo.progressLvl = playerSynStats.progressLevel;
                socialInfo.combatScore = peer.mPlayer.LocalCombatStats.CombatScore;
                socialInfo.lastLvlId = peer.CharacterData.lastlevelid;
                socialInfo.countryType = playerSynStats.countryType; // country
                socialInfo.guildName = playerSynStats.guildName; // guild
                socialInfo.isOnline = true;
                if (socialType != -1)
                {
                    result = socialInfo.ToString();
                    mPlayer.SocialStats.friendList[idx] = result;
                }
            }
            else
            {
                if (socialType != -1)
                {
                    socialInfo.InitFromString(strData);
                    socialInfo.isOnline = false;
                    result = socialInfo.ToString();
                }
                else
                    result = string.Format("{0}`0`0`1`9000`0`0``true", name);
            }*/
            return "";
        }
        #endregion

        #region Guild
        public void EnqueueNewGuildWeek()
        {
            if (mPlayer != null)
                mPlayer.NewGuildWeek();
        }
        #endregion

        #region Item Inventory
        public void OpenNewInvSlot(int numSlotsToUnlock, ItemInventoryController.OpenNewSlotType type)
        {
            if (numSlotsToUnlock < 1)
                return;

            int unlockedSlotCount = characterData.ItemInventory.UnlockedSlotCount;
            int maxInvSlots = (int)InventorySlot.MAXSLOTS;
            if (unlockedSlotCount >= maxInvSlots)
            {
                ZRPC.CombatRPC.Ret_OpenNewInvSlot((byte)OpenSlotRetCode.Fail_AllOpened, -1, mPlayer.Slot);
                return;
            }

            int slotsOverflow = unlockedSlotCount + numSlotsToUnlock - maxInvSlots;
            if (slotsOverflow > 0)
                numSlotsToUnlock -= slotsOverflow;
            //if (type == ItemInventoryController.OpenNewSlotType.VIP_AUTOOPEN)
            //{
            //    var time = GameUtils.GetExpandBagTime(mPlayer.SecondaryStats.OpenSlotTimePassed, mPlayer.SecondaryStats.DTSlotOpenTime, numOfSlots, DateTime.Now);
            //    if (numOfSlots != 1 || time > 0)
            //    {
            //        ZRPC.CombatRPC.Ret_OpenNewInvSlot((byte)OpenSlotRetCode.Fail_AutoOpen, time, mPlayer.Slot);
            //       return;
            //    }
            //}

            //int numOfGold = 0;
            //if (type == ItemInventoryController.OpenNewSlotType.FREE || type == ItemInventoryController.OpenNewSlotType.VIP_AUTOOPEN)
            //    numOfGold = 0;
            //else
            //    numOfGold = GameUtils.GetExpandBagCost(mPlayer.SecondaryStats.OpenSlotTimePassed, mPlayer.SecondaryStats.DTSlotOpenTime, numOfSlots, DateTime.Now);

            //if ((numOfGold == 0 && numOfSlots == 1) || mPlayer.DeductCurrency(CurrencyType.LockGold, numOfGold, true, "Inventory_SlotOpen"))//open a slot for free...
            //{
            int newUnlockedSlotCount = unlockedSlotCount + numSlotsToUnlock;
            mInventory.OnUnlockedNewSlot(newUnlockedSlotCount);
            mPlayer.SecondaryStats.UnlockedSlotCount = newUnlockedSlotCount;

            //    string message = string.Format("inventoryType: Bag | diamondCost: {0} | oldOpenSlots: {1} | newOpenSlots: {2}",
            //        numOfGold,
            //        oldunlockedslots,
            //        newunlockedslots);

            //    Zealot.Logging.Client.LogClasses.InventorySlot inventorySlotLog = new Zealot.Logging.Client.LogClasses.InventorySlot();
            //    inventorySlotLog.userId = mUserId;
            //    inventorySlotLog.charId = GetCharId();
            //    inventorySlotLog.message = message;
            //    inventorySlotLog.inventoryType = "Bag";
            //    inventorySlotLog.diamondCost = numOfGold;
            //    inventorySlotLog.oldOpenSlots = oldunlockedslots;
            //    inventorySlotLog.newOpenSlots = newunlockedslots;
            //    var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(inventorySlotLog);

                //var newtimeToUnlock = GameUtils.GetExpandBagTime(mPlayer.SecondaryStats.OpenSlotTimePassed, mPlayer.SecondaryStats.DTSlotOpenTime, numOfSlots, DateTime.Now);
                //ZRPC.CombatRPC.Ret_OpenNewInvSlot((byte)OpenSlotRetCode.Success, newtimeToUnlock, mPlayer.Slot);
            //}
            //else
            //{
            //    ZRPC.CombatRPC.Ret_OpenNewInvSlot((byte)OpenSlotRetCode.Fail_Gold, 0, mPlayer.Slot);
            //}
        }

        public bool RemoveItemFromInventory(ushort itemid, int count, string from)
        {
            InvRetval retval = mInventory.DeductItems(itemid, count, from);
            return retval.retCode == InvReturnCode.UseSuccess;
        }
        #endregion

        #region EquipmentUpgrade
        public void OnEquipmentUpgradeEquipment(int slotID, bool isEquipped, bool isUseGenMaterial, bool isSafeUpgrade, bool isSafeUseEquip, bool isSafeGenMat, int safeEquipSlotId = -1)
        {
            // Invalid slot!
            if (slotID <= -1)
            {
                return;
            }

            Equipment equipItem = isEquipped ? characterData.EquipmentInventory.Slots[slotID] as Equipment : characterData.ItemInventory.Slots[slotID] as Equipment;

            if (equipItem == null)
            {
                // Item is missing from the slot!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Missing_Item_In_Slot"), "", false, mPlayer.Slot);
                return;
            }
            
            //string equipSlotName = equipItem.LocalizedName;
            int currentLevel = equipItem.UpgradeLevel;
            int upgradeLevel = currentLevel + 1;
            //int playerLevel = mPlayer.PlayerSynStats.progressLevel;
            EquipmentType equipType = equipItem.EquipmentJson.equiptype;
            ItemRarity rarity = equipItem.EquipmentJson.rarity;
            int maxLevel = equipItem.EquipmentJson.upgradelimit;

            if (currentLevel >= maxLevel)
            {
                // Exceeded max upgrade level
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipUpgrade_UpgradeLevelMaxed"), "", false, mPlayer.Slot);
                return;
            }

            //if (currentLevel >= playerLevel)
            //{
            //    // Exceeded player level
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Equip_ReachedPlayerLevel"), "", false, mPlayer.Slot);
            //    return;
            //}

            EquipmentUpgradeJson currentData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, currentLevel);
            EquipmentUpgradeJson upgradeData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, upgradeLevel);

            if ((currentLevel > 0 && currentData == null) || upgradeData == null)
            {
                // Unable to get upgrade data
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipUpgrade_UpgradeDataReadFailed"), "", false, mPlayer.Slot);
                return;
            }

            //int currentSurmountLvl = currentData != null ? EquipmentRepo.ParseInt(currentData.surmountlevel) : 0;
            //int nextSurmountLvl = upgradeData != null ? EquipmentRepo.ParseInt(upgradeData.surmountlevel) : 0;

            //EquipMaterial upgradeMaterial = EquipmentRepo.GetUpgradeMaterial(upgradeData.equipmenttype, upgradeLevel);
            //int currMatCount = mInventory.GetItemStackcountByItemId((ushort)upgradeMaterial.mItemID);

            EquipUpgMaterial upgradeMaterial = EquipmentModdingRepo.GetEquipmentUpgradeMaterial(equipType, rarity, upgradeLevel, isUseGenMaterial, isSafeUpgrade, isSafeUseEquip);
            int currMatCount = mInventory.GetItemStackCountByItemId((ushort)upgradeMaterial.mMat.itemId);

            if (currMatCount < upgradeMaterial.mMat.stackCount)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipUpgrade_InsufficientMaterials"), "", false, mPlayer.Slot);
                return;
            }

            if(isSafeUpgrade)
            {
                if(!isSafeUseEquip)
                {
                    EquipUpgMaterial safeUpgMat = EquipmentModdingRepo.GetEquipmentUpgradeSafeMaterial(equipType, rarity, upgradeLevel, isSafeGenMat);
                    int safeMatCount = mInventory.GetItemStackCountByItemId((ushort)safeUpgMat.mMat.itemId);

                    if (safeMatCount < safeUpgMat.mMat.stackCount)
                    {
                        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipUpgrade_InsufficientSafeMaterials"), "", false, mPlayer.Slot);
                        return;
                    }
                }
                else if(isSafeUseEquip && safeEquipSlotId == -1)
                {
                    // Replacedment equipment not specificed!
                    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Missing_Replace_Equip"), "", false, mPlayer.Slot);
                    return;
                }

                Equipment replaceEquipment = characterData.ItemInventory.Slots[safeEquipSlotId] as Equipment;

                if (isSafeUpgrade && isSafeUseEquip && replaceEquipment == null)
                {
                    // Replacedment equipment not specificed!
                    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Missing_Replace_Equip"), "", false, mPlayer.Slot);
                    return;
                }
            }

            EquipUpgProbResult res = EquipmentModdingRepo.GetEquipmentUpgradeRoll(equipType, rarity, upgradeLevel);

            if(res == EquipUpgProbResult.Error)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipUpgrade_RollError"), "", false, mPlayer.Slot);
                return;
            }

            List<int> buffList = EquipmentModdingRepo.GetEquipmentUpgradeBuff(equipType, rarity, upgradeLevel);
            if(res == EquipUpgProbResult.Success)
            {
                float currIncrease = currentLevel > 0 ? currentData.increase : 0f;
                mInventory.UpdateEquipmentProperties((ushort)upgradeLevel, EquipPropertyType.Upgrade, isEquipped, slotID, ushort.MaxValue, buffList, false, currIncrease, upgradeData.increase);

                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipUpgrade_Success"), "", false, mPlayer.Slot);
                ZRPC.NonCombatRPC.EquipmentUpgradeEquipmentSuccess(mPlayer.Slot);
            }
            else if(res == EquipUpgProbResult.Drop)
            {
                if(!isSafeUpgrade)
                {
                    if(currentLevel > 0)
                    {
                        int droppedLevel = currentLevel - 1 > -1 ? currentLevel - 1 : 0;

                        if (droppedLevel > 0)
                        {
                            EquipmentUpgradeJson dropUpgData = EquipmentModdingRepo.GetEquipmentUpgradeData(equipType, rarity, droppedLevel);
                            mInventory.UpdateEquipmentProperties((ushort)droppedLevel, EquipPropertyType.Upgrade, isEquipped, slotID, ushort.MaxValue, buffList, false, currentData.increase, dropUpgData.increase);
                        }
                        else
                        {
                            mInventory.UpdateEquipmentProperties((ushort)droppedLevel, EquipPropertyType.Upgrade, isEquipped, slotID, ushort.MaxValue, buffList, false, currentData.increase, 0);
                        }
                    }
                }

                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipUpgrade_Failed_Drop"), "", false, mPlayer.Slot);
                ZRPC.NonCombatRPC.EquipmentUpgradeEquipmentFailed(mPlayer.Slot);
            }
            else if (res == EquipUpgProbResult.Fail)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipUpgrade_Failed"), "", false, mPlayer.Slot);
                ZRPC.NonCombatRPC.EquipmentUpgradeEquipmentFailed(mPlayer.Slot);
            }

            int moneyCost = EquipmentModdingRepo.GetEquipmentUpgradeCost(equipType, rarity, upgradeLevel, isSafeUpgrade);
            //int moneyBefore = mPlayer.SecondaryStats.money;
            //int moneyAfter = 0;
            //string systemName = Enum.GetName(typeof(EquipmentSystem), EquipmentSystem.Upgrade);

            //DeductEquipUpgradeGold(equipItem, upgradeLevel, isSafeUpgrade);
            //DeductEquipMaterial(upgradeMaterial, isSafeUpgrade);
            
            int money = mPlayer.SecondaryStats.Money;
            if (moneyCost > money)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_InsufficientMoney"), "", false, this);
                return;
            }
            else
            {
                mPlayer.DeductCurrency(CurrencyType.Money, moneyCost, true, "Equip_Upgrade");
            }

            InvRetval useUpgMatRes = EquipmentRules.UseMaterials(upgradeMaterial, this);

            if (useUpgMatRes.retCode == InvReturnCode.UseFailed)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_UseItemFailed"), "", false, this);
                return;
            }

            if (isSafeUpgrade)
            {
                if(isSafeUseEquip)
                {
                    characterData.ItemInventory.ItemRemoveBySlotId(safeEquipSlotId, 1); // Equipment Item should only have stack of 1
                }
                else
                {
                    EquipUpgMaterial safeUpgMat = EquipmentModdingRepo.GetEquipmentUpgradeSafeMaterial(equipType, rarity, upgradeLevel, isSafeGenMat);
                    InvRetval useSafeUpgMatRes = EquipmentRules.UseMaterials(safeUpgMat, this);

                    if (useSafeUpgMatRes.retCode == InvReturnCode.UseFailed)
                    {
                        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_UseItemFailed"), "", false, this);
                        return;
                    }
                }
            }

            mPlayer.UpdateAchievement(AchievementObjectiveType.RefineCount);
            mPlayer.UpdateAchievement(AchievementObjectiveType.RefineEquipmentLV, upgradeLevel.ToString(), false);

            //moneyAfter = mPlayer.SecondaryStats.money;
            //EquipmentRules.LogEquipUpgrade("Equipment Upgrade", equipSlotName, upgradeLevel, this);
            //EquipmentRules.LogEquipItemUse(upgradeMaterial.mItemID, upgradeMaterial.mMatCount, systemName, this);
            //EquipmentRules.LogEquipMoney(moneyCost, moneyBefore, moneyAfter, systemName, this);

            //mSevenDaysController.UpdateTask(NewServerActivityType.Equipupgrade_n);
            //mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.EquipmentUpgrade);
        }

        private void DeductEquipUpgradeGold(Equipment upgradeData, int upgradeLevel, bool isSafeUpgrade)
        {
        }

        //private void DeductEquipMaterial(EquipUpgMaterial upgradeMaterial, bool isSafeUpgrade)
        //{
        //    InvRetval useUpgMatRes = EquipmentRules.UseMaterials(upgradeMaterial, this);

        //    if (useUpgMatRes.retCode == InvReturnCode.UseFailed)
        //    {
        //        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_UseItemFailed"), "", false, this);
        //    }
        //}

        public void OnEquipmentReformEquipment(int slotID, bool isEquipped, int selection)
        {
            // Invalid slot!
            if (slotID <= -1)
            {
                return;
            }

            Equipment equipItem = isEquipped ? characterData.EquipmentInventory.Slots[slotID] as Equipment : characterData.ItemInventory.Slots[slotID] as Equipment;

            if(equipItem == null)
            {
                // Item is missing from the slot!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Missing_Item_In_Slot"), "", false, mPlayer.Slot);
                return;
            }

            string reformGrp = equipItem.EquipmentJson.evolvegrp;
            int currentStep = equipItem.ReformStep;
            int nextStep = currentStep + 1;
            int maxStep = EquipmentModdingRepo.GetEquipmentReformGroupMaxLevel(reformGrp);
            int nextEquipId = equipItem.EquipmentJson.evolvechange;

            if(nextStep > maxStep && nextEquipId == -1)
            {
                // Item is missing from the slot!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipReform_StepMaxed"), "", false, mPlayer.Slot);
                return;
            }

            // Get next reform step data
            List<EquipmentReformGroupJson> nextReformData = EquipmentModdingRepo.GetEquipmentReformDataByGroupStep(reformGrp, nextStep);

            if(nextReformData == null)
            {
                // Next step data not found!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipReform_Step_NotFound"), "", false, mPlayer.Slot);
                return;
            }

            // Check for selection out of bounds (if more than 1 option)
            if(selection < 0 || selection >= nextReformData.Count)
            {
                return;
            }

            // Check for sufficient gold
            int moneyCost = nextReformData[selection].cost;
            int money = mPlayer.SecondaryStats.Money;

            if(/*!mPlayer.SecondaryStats.IsGoldEnough(moneyCost)*/money < moneyCost)
            {
                // Not enough gold
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_InsufficientGold"), "", false, mPlayer.Slot);
                //ZRPC.NonCombatRPC.EquipmentReformEquipmentFailed(mPlayer.Slot);
                return;
            }

            // Check for sufficient materials
            List<ItemInfo> materialList = EquipmentModdingRepo.GetEquipmentReformMaterials(reformGrp, nextStep, selection);
            if(materialList == null)
            {
                // Unable to read materals list!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipReform_Material_Data_Read_Failed"), "", false, mPlayer.Slot);
                return;
            }

            if(!EquipmentRules.IsEnoughReformMaterials(materialList, this))
            {
                // Not enough reform materials!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipReform_Not_Enough_Materials"), "", false, mPlayer.Slot);
                //ZRPC.NonCombatRPC.EquipmentReformEquipmentFailed(mPlayer.Slot);
                return;
            }

            // If success
            // Deduct money
            mPlayer.DeductCurrency(CurrencyType.Money, moneyCost, false, "EquipReform");
            // Use materials
            EquipmentRules.UseMaterials(materialList, this);
            // Update reform step, add side effect and record reform selection (if more than 1 selection)
            // RPC play success animation
            mInventory.UpdateEquipmentProperties((ushort)nextStep, EquipPropertyType.Reform, isEquipped, slotID, (ushort)selection);
            // Reform success
            ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipReform_Success"), "", false, mPlayer.Slot);
            ZRPC.NonCombatRPC.EquipmentReformEquipmentSuccess(mPlayer.Slot);
        }

        public void OnEquipmentRecycleEquipment(int slotID, bool isEquipped)
        {
            // Invalid slot!
            if (slotID <= -1)
            {
                return;
            }

            Equipment equipItem = isEquipped ? characterData.EquipmentInventory.Slots[slotID] as Equipment : characterData.ItemInventory.Slots[slotID] as Equipment;

            if(equipItem == null)
            {
                // Item is missing from the slot!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Missing_Item_In_Slot"), "", false, mPlayer.Slot);
                return;
            }

            string reformGrp = equipItem.EquipmentJson.evolvegrp;
            int currentStep = equipItem.ReformStep;
            int prevStep = currentStep - 1;

            if(currentStep == 0 || prevStep < 0)
            {
                // Item is missing from the slot!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipReform_StepLowest"), "", false, mPlayer.Slot);
                return;
            }

            // Get next reform step data
            List<EquipmentReformGroupJson> currReformData = EquipmentModdingRepo.GetEquipmentReformDataByGroupStep(reformGrp, currentStep);

            if(currReformData == null)
            {
                // Next step data not found!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipReform_Step_NotFound"), "", false, mPlayer.Slot);
                return;
            }

            int selection = currReformData.Count != 1 ? equipItem.GetSelectionByReformStep(currentStep) : 0;

            if(selection == ushort.MaxValue)
            {
                // Item is missing from the slot!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipReform_InvalidSelection"), "", false, mPlayer.Slot);
                return;
            }

            // Inventory full
            if (!mInventory.mInvData.HasEmptySlot())
            {
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }
            
            List<ItemInfo> materialList = EquipmentModdingRepo.GetEquipmentReformMaterials(reformGrp, currentStep, selection);
            if(materialList == null)
            {
                // Unable to read materals list!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipReform_Material_Data_Read_Failed"), "", false, mPlayer.Slot);
                return;
            }

            if(!characterData.ItemInventory.CanAdd(materialList))
            {
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            // Add money
            int moneyCost = EquipmentModdingRepo.GetEquipmentReformCost(reformGrp, currentStep, selection);
            mPlayer.AddCurrency(CurrencyType.Money, moneyCost, "EquipRecycle");

            // Add materials
            InvRetval result = mInventory.AddItemsToInventory(materialList, true, "EquipRecycle");
            if(result.retCode == InvReturnCode.AddFailed)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
            }

            mInventory.UpdateEquipmentProperties((ushort)prevStep, EquipPropertyType.Recycle, isEquipped, slotID);
        }

        public void OnEquipGemSlotItem(int equipSlotID, int gemGrp, int gemSlot, int gemID)
        {
            // Invalid equipment slot!
            if (equipSlotID <= -1)
            {
                return;
            }

            // Invalid gem slot!
            if (gemSlot < 0 || gemSlot > 1)
            {
                return;
            }

            //EquipItem equipItem = characterData.EquippedInventory.Slots[equipSlotID] as EquipItem;

            //if (equipItem == null)
            //{
            //    // Equipment is missing from the equipment slot!
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Missing_Item_In_Slot"), "", false, mPlayer.Slot);
            //    return;
            //}

            //const int gemGrpSize = 2;
            //List<ItemInfo> gemsList = new List<ItemInfo>();
            //SocketGemItem socketGem = null;
            //GemSlotData gemSlotData = new GemSlotData();
            //if (gemID > -1)
            //{
            //    socketGem = mInventory.mInvData.GetItemByItemId((ushort)gemID) as SocketGemItem;
            //    if (socketGem != null)
            //    {
            //        List<ushort> gemIDs = equipItem.DecodeGemIDs();
            //        int pos = (gemGrp * gemGrpSize) + gemSlot;
            //        // Stop equipping gem if equipment already has the same itemid gem
            //        if (gemIDs[pos] == socketGem.ItemID)
            //        {
            //            return;
            //        }

            //        gemSlotData.Set(gemSlot, gemIDs[pos], socketGem.ItemID);
            //        ItemInfo socketGemInfo = new ItemInfo();
            //        socketGemInfo.itemid = socketGem.ItemID;
            //        socketGemInfo.stackcount = 1;
            //        gemsList.Add(socketGemInfo);
            //        string systemName = Enum.GetName(typeof(EquipmentSystem), EquipmentSystem.GemSlot);
            //        EquipmentRules.LogEquipItemUse(socketGem.ItemID, 1, systemName, this);
            //    }
            //    else
            //    {
            //        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipGemSlot_Failure"), "", false, this);

            //        return;
            //    }
            //}

            //UpdateRetval res = mInventory.UpdateEquipmentProperties(0, EquipPropertyType.Slotting, equipSlotID, "", gemGrp, gemSlot, socketGem.ItemID, socketGem.Attribute);

            //// Remove socket gems as they are added to the equipment
            //if (res == UpdateRetval.Success)
            //{
            //    mInventory.UseToolItems(gemsList, "Equip");

            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipGemSlot_Success"), "", false, this);
            //    EquipmentRules.LogEquipGemSlot("Equipment Upgrade", equipItem.LocalizedName, socketGem.ItemID, this);

            //}
            //else
            //{
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipGemSlot_Failure"), "", false, this);
            //}
        }

        public void OnUnequipGemSlotItem(int equipSlotID, int gemGrp, int gemSlot)
        {
            // Invalid equipment slot!
            if (equipSlotID <= -1)
            {
                return;
            }

            // gem slot out of bounds!
            if (gemSlot < 0 || gemSlot > 1)
            {
                return;
            }

            //const int gemGrpSize = 2;
            //EquipItem equipItem = characterData.EquippedInventory.Slots[equipSlotID] as EquipItem;

            //List<ushort> gemIDs = equipItem.DecodeGemIDs();
            //int pos = (gemGrp * gemGrpSize) + gemSlot;
            //if (gemIDs[pos] == 0)
            //{
            //    return;
            //}

            //if (equipItem == null)
            //{
            //    // Item is missing from the slot!
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Missing_Item_In_Slot"), "", false, mPlayer.Slot);
            //    return;
            //}

            //if (gemSlot > -1 && gemSlot < 2)
            //{
            //    UpdateRetval res = mInventory.UpdateEquipmentProperties(0, EquipPropertyType.Unslotting, equipSlotID, "", gemGrp, gemSlot);

            //    if (res == UpdateRetval.Success)
            //    {
            //        ZRPC.NonCombatRPC.EquipUnslotGemSuccess(gemSlot, mPlayer.Slot);

            //        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_UnequipGemSlot_Success"), "", false, mPlayer.Slot);

            //        GemSlotData gemSlotData = new GemSlotData();
            //        gemSlotData.Set(gemSlot, gemIDs[pos], 0);
            //        EquipmentRules.LogEquipGemSlot("Equipment Upgrade", equipItem.LocalizedName, gemIDs[pos], this);
            //        string systemName = Enum.GetName(typeof(EquipmentSystem), EquipmentSystem.GemSlot);
            //        EquipmentRules.LogEquipItemGet(gemIDs[pos], 1, systemName, this);
            //    }
            //}
            //else
            //{
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_UnequipGemSlot_Failure"), "", false, this);
            //}
        }

        public void OnEquipAutoUpgradeGem()
        {
            //List<AutoGemCraftCode> results = new List<AutoGemCraftCode>();
            //int gemGrp = characterData.EquippedInventory.SelectedGemGroup;
            //const int gemGrpSize = 2;
            //int equipSlotCount = characterData.EquippedInventory.Slots.Count;

            //for (int e = 0; e < equipSlotCount; ++e)
            //{
            //    EquipItem equipItem = characterData.EquippedInventory.Slots[e] as EquipItem;

            //    if (equipItem == null)
            //    {
            //        // Item is missing from the slot!
            //        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Missing_Item_In_Slot"), "", false, mPlayer.Slot);
            //        return;
            //    }

            //    List<ushort> gemIDs = equipItem.DecodeGemIDs();
            //    GemSlotData gemSlotData = new GemSlotData();
            //    for (int i = 0; i < gemIDs.Count; ++i)
            //    {
            //        //int pos = (gemGrp * gemGrpSize) + i;
            //        //if (pos >= gemIDs.Count)
            //        //{
            //        //    return;
            //        //}

            //        if (gemIDs[i] <= 0)
            //        {
            //            //ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipAutoUpgradeGem_NoGemSlotted"), "", false, mPlayer.Slot);
            //            results.Add(AutoGemCraftCode.Failed_NoGem);
            //            continue;
            //        }

            //        int gemGrp = i / gemGrpSize;
            //        int gemSlot = i % gemGrpSize;
            //        int gemId = gemIDs[i];
            //        int maxGemRank = GameConstantRepo.GetConstantInt("Equip_MaxGemRank", 1);
            //        SocketGemItem gemItem = GameRepo.ItemFactory.GetInventoryItem(gemId) as SocketGemItem;
            //        if (gemItem == null)
            //        {
            //            //ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipAutoUpgradeGem_NoGemSlotted"), "", false, mPlayer.Slot);
            //            results.Add(AutoGemCraftCode.Failed_NoGem);
            //            continue;
            //        }

            //        if (gemItem.Rank >= maxGemRank)
            //        {
            //            //ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipAutoUpgradeGem_MaxGemRankReached"), "", false, mPlayer.Slot);
            //            results.Add(AutoGemCraftCode.Failed_MaxRank);
            //            continue;
            //        }

            //        int newGemId = -1;
            //        Crafting.Crafting.CraftReturnCode code = mPlayer.mCrafting.AutoCraftAndLevelItem(gemId, out newGemId);

            //        if (code == Crafting.Crafting.CraftReturnCode.FAIL_MONEY)
            //        {
            //            //ZRPC.NonCombatRPC.EquipAutoUpgradeFailedMoney(mPlayer.Slot);
            //            results.Add(AutoGemCraftCode.Failed_Money);
            //        }
            //        else if (code == Crafting.Crafting.CraftReturnCode.FAIL_ITEM)
            //        {
            //            //ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipAutoUpgradeGem_Failed"), "", false, mPlayer.Slot);
            //            results.Add(AutoGemCraftCode.Failed_Materials);
            //        }
            //        else if (code == Crafting.Crafting.CraftReturnCode.FAIL)
            //        {
            //            //ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipAutoUpgradeGem_Failed"), "", false, mPlayer.Slot);
            //            results.Add(AutoGemCraftCode.Failed);
            //        }
            //        else
            //        {
            //            if (newGemId > 0)
            //            {
            //                gemSlotData.Set(gemSlot, gemIDs[i], newGemId);

            //                SocketGemItem socketGem = GameRepo.ItemFactory.GetInventoryItem(newGemId) as SocketGemItem;
            //                UpdateRetval res = mInventory.UpdateEquipmentProperties(0, EquipPropertyType.ChangeGem, e, "", gemGrp, gemSlot, socketGem.ItemID, socketGem.Attribute);
            //                //ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipAutoUpgradeGem_Success"), "", false, mPlayer.Slot);
            //                results.Add(AutoGemCraftCode.Success);
            //                string systemName = Enum.GetName(typeof(EquipmentSystem), EquipmentSystem.GemSlot);
            //                EquipmentRules.LogEquipItemGet(newGemId, 1, systemName, this);
            //            }
            //        }

            //        EquipmentRules.LogEquipGemSlot("Equipment Upgrade", equipItem.LocalizedName, newGemId, this);
            //    }
            //}

            //// Handle error messages here to prevent flooding of messages back to client
            //if (results.Contains(AutoGemCraftCode.Success))
            //{
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipAutoUpgradeGem_Success"), "", false, mPlayer.Slot);
            //}

            //if ((results.Contains(AutoGemCraftCode.Failed) ||
            //    results.Contains(AutoGemCraftCode.Failed_NoGem) ||
            //    results.Contains(AutoGemCraftCode.Failed_Materials)) &&
            //    !results.Contains(AutoGemCraftCode.Failed_MaxRank))
            //{
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipAutoUpgradeGem_Failed"), "", false, mPlayer.Slot);
            //}
            //else
            //{
            //    List<AutoGemCraftCode> maxedRankCheck = results.FindAll(o => o == AutoGemCraftCode.Failed_MaxRank);
            //    if (maxedRankCheck.Count == (equipSlotCount * gemGrpSize))
            //    {
            //        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipAutoUpgradeGem_MaxGemRankReached"), "", false, mPlayer.Slot);
            //    }
            //    else
            //    {
            //        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipAutoUpgradeGem_Failed"), "", false, mPlayer.Slot);
            //    }
            //}

            //if (results.Contains(AutoGemCraftCode.Failed_Money))
            //{
            //    ZRPC.NonCombatRPC.EquipAutoUpgradeFailedMoney(mPlayer.Slot);
            //}
        }

        public void OnEquipmentSetUpgradeLevel(int slotID, int level)
        {
            // Invalid slot!
            if (slotID <= -1)
            {
                return;
            }

            //EquipItem equipItem = characterData.EquippedInventory.Slots[slotID] as EquipItem;

            //if (equipItem == null)
            //{
            //    // Item is missing from the slot!
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Missing_Item_In_Slot"), "", false, mPlayer.Slot);
            //    return;
            //}

            //int upgradeLevel = level;
            //int surmountLevel = level / 10;

            //int playerLevel = mPlayer.PlayerSynStats.progressLevel;
            //int maxLevel = GameConstantRepo.GetConstantInt("Equip_UpgradeMaxLevel", 500);

            //if (upgradeLevel > maxLevel)
            //{
            //    // Exceeded max upgrade level
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Equip_UpgradeLevelMaxed"), "", false, mPlayer.Slot);
            //    return;
            //}

            //if (upgradeLevel >= playerLevel)
            //{
            //    // Exceeded player level
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Equip_ReachedPlayerLevel"), "", false, mPlayer.Slot);
            //    return;
            //}

            //EquipUpgradeJson upgradeData = EquipmentRepo.GetUpgrade(equipItem.EquipType, upgradeLevel);

            //if (upgradeData == null)
            //{
            //    return;
            //}

            //mInventory.UpdateEquipmentProperties((ushort)upgradeLevel, EquipPropertyType.Upgrade, slotID, upgradeData.attributevalue);
            //mInventory.UpdateEquipmentProperties((ushort)surmountLevel, EquipPropertyType.Surmount, slotID, upgradeData.attributevalue);
        }
        #endregion

        #region Welfare
        // Sign In Prize
        public void OnWelfareClaimSignInPrize(int year, int month, int day, int cltYear, int cltMonth, int cltDay, bool oldData)
        {
            DateTime today = DateTime.Today;
            DateTime rewardDate = !oldData ? new DateTime(year, month, day) : new DateTime(year, today.Month, day);
            DateTime clientDate = new DateTime(cltYear, cltMonth, cltDay);

            int dataid = day - 1;
            bool isFullClaimed = mWelfareCtrlr.IsSignInPrizeFullClaimed(dataid);
            bool isPartClaimed = mWelfareCtrlr.IsSignInPrizePartClaimed(dataid);
            bool isSpecialClaim = /*!oldData && */rewardDate < today && (!isPartClaimed && !isFullClaimed);

            // If already claimed, skip
            if (isFullClaimed)
            {
                // Send System Message to client: "SignInPrize already claimed!"
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_SignInPrizeAlreadyClaimed"), "", false, mPlayer.Slot);

                return;
            }

            if (rewardDate > today || clientDate != today)
            {
                // Send System Message to client: "SignInPrize claim failed"
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_SignInPrizeClaimFailed"), "", false, mPlayer.Slot);

                return;
            }

            VIPData vipData = WelfareRepo.GetVIPLevelMultplyByDate(year, month, day);
            int goldCost = GameConstantRepo.GetConstantInt("Welfare_ReclaimCost");
            //if (vipData != null && isPartClaimed && mPlayer.PlayerSynStats.vipLvl < vipData.mVIPLevel ||
            //    vipData != null && isPartClaimed && (goldCost == 0 || !mPlayer.SecondaryStats.IsGoldEnough(goldCost)))
            //{
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_InsufficientGold"), "", false, mPlayer.Slot);
            //    return;
            //}

            if (isSpecialClaim && (goldCost == 0 || !mPlayer.SecondaryStats.IsGoldEnough(goldCost)))
            {
                ZRPC.NonCombatRPC.Ret_WelfareNotEnoughGold(this);
                return;
            }

            // Inventory full
            if (!mInventory.mInvData.HasEmptySlot())
            {
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            int lockgold = mPlayer.SecondaryStats.bindgold;
            int gold = mPlayer.SecondaryStats.Gold;
            int lockgoldBef = lockgold;
            int goldBef = gold;
            if (lockgold >= goldCost)
            {
                lockgold = goldCost;
                gold = 0;
            }
            else
            {
                gold = goldCost - lockgold;
            }
            if (isSpecialClaim)
            {
                bool res = mPlayer.DeductGold(goldCost, true, true, "Welfare_Sign");

                if (res == false)
                {
                    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_DeductGoldFailed"), "", false, mPlayer.Slot);
                    return;
                }
            }
            else if (isPartClaimed)
            {
                //if (mPlayer.PlayerSynStats.vipLvl < vipData.mVIPLevel)
                //{
                //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_SignInPrizeInsufficientVIPLevel"), "", false, mPlayer.Slot);
                //    return;
                //}

                //bool res = mPlayer.DeductGold(goldCost, true, true);

                //if (res == false)
                //{
                //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_DeductGoldFailed"), "", false, mPlayer.Slot);
                //    return;
                //}
            }

            IInventoryItem reward = WelfareRepo.GetSignInPrizeByDate(year, month, day);
            if (reward == null)
            {
                // No rewards found!
                return;
            }

            int bonus = 1;
            //if (isPartClaimed && mPlayer.PlayerSynStats.vipLvl >= vipData.mVIPLevel)
            //{
            //    int count = reward.StackCount;
            //    reward.StackCount *= (ushort)vipData.mMultiply;
            //    reward.StackCount -= count;

            //    bonus *= vipData.mMultiply;
            //}
            //else if (!isPartClaimed && mPlayer.PlayerSynStats.vipLvl >= vipData.mVIPLevel)
            //{
            //    reward.StackCount *= (ushort)vipData.mMultiply;

            //    bonus *= vipData.mMultiply;
            //}

            // Add reward items to inventory
            InvRetval addRes = mInventory.AddItemsToInventory(reward, true, "Welfare_Sign");

            if (addRes.retCode == InvReturnCode.AddFailed)
            {
                // Don't claim if bag is full
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryAddFailed"), "", false, mPlayer.Slot);
                return;
            }
            else if (addRes.retCode == InvReturnCode.Full)
            {
                // Don't claim if add to bag failed
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            //VIPData vipData = WelfareRepo.GetVIPLevelMultplyByDate(year, month, day);

            // Claim SignIn Prize
            int playerVIPLvl = 0;// mPlayer.PlayerSynStats.vipLvl;
            if (isSpecialClaim || vipData == null || playerVIPLvl >= vipData.mVIPLevel)
            {
                mWelfareCtrlr.ClaimSignInPrizeFull(dataid);
            }
            else
            {
                mWelfareCtrlr.ClaimSignInPrizePart(dataid);
            }

            int repoVIPLvl = vipData == null ? 0 : vipData.mVIPLevel;
            int vipStackBonus = vipData == null ? 0 : vipData.mMultiply;
            WelfareRules.LogWelfareSignInPrizeGet("Welfare Claim Success", this);
            WelfareRules.LogWelfareSignInPrizeItemGet("Welfare Item Get", repoVIPLvl, playerVIPLvl, vipStackBonus, bonus,
                reward.StackCount, reward.ItemID, this);

            if (isSpecialClaim || (vipData != null && isPartClaimed && !isFullClaimed))
            {
                WelfareRules.LogWelfareSignInPrizeReGet(year, month, day, this);
                int lockgoldAft = mPlayer.SecondaryStats.bindgold;
                int goldAft = mPlayer.SecondaryStats.Gold;
                //WelfareRules.LogWelfareSignInPrizeReGetLockGoldUse(lockgold, lockgoldBef, lockgoldAft, this);

                // <Add> i think this should be the non-locked gold ba
                //WelfareRules.LogWelfareSignInPrizeReGetGoldUse(gold, goldBef, goldAft, this);
            }

            // Local Object will handle button refresh
        }

        // Online Prize
        public void OnWelfareClaimOnlinePrizes()
        {
            if (mWelfareCtrlr.IsAllOnlineRewardsClaimed() == true)
            {
                // Send System Message to client: "Online Rewards already claimed!"
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_OnlinePrizeAllClaimed"), "", false, mPlayer.Slot);

                return;
            }

            // Inventory full
            if (!mInventory.mInvData.HasEmptySlot())
            {
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            DateTime currentTime = DateTime.Now;

            if (currentTime.Ticks < loginDT.Ticks)
            {
                // currentTime is earlier than loginTime!
                return;
            }

            TimeSpan duration = currentTime - loginDT;
            duration += new TimeSpan(mWelfareCtrlr.GetLastOnlineDuration());

            int rollTimes = 0;
            List<int> toClaimList = new List<int>();
            List<IInventoryItem> rewardItems = new List<IInventoryItem>();
            for (int i = 0; i < WelfareInventoryData.MAX_ONLNRWRDCLAIMS; ++i)
            {
                int order = i + 1;
                if (duration.TotalMinutes > WelfareRepo.GetRewardTimeByOrder(order) && !characterData.WelfareInventory.IsOnlineRewardsClaimed(i))
                {
                    ++rollTimes;
                    toClaimList.Add(i);
                    rewardItems.Add(WelfareRepo.GetOnlinePrizeByOrder(order));
                }
            }

            // Add rolled items to inventory
            InvRetval addRes = mInventory.AddItemsToInventory(rewardItems, true, "Welfare");

            if (addRes.retCode == InvReturnCode.AddFailed)
            {
                // Don't claim if bag is full
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryAddFailed"), "", false, mPlayer.Slot);
                return;
            }
            else if (addRes.retCode == InvReturnCode.Full)
            {
                // Don't claim if add to bag failed
                // Add to inventory failed!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            // Claim online reward
            for (int i = 0; i < toClaimList.Count; ++i)
            {
                mWelfareCtrlr.ClaimOnlineRewards(toClaimList[i], rewardItems[i].ItemID);
                int order = i + 1;
                WelfareRules.LogWelfareOnlinePrizeGet(order, duration.TotalSeconds, this);
                WelfareRules.LogWelfareOnlinePrizeItemGet(rewardItems[i].ItemID, rewardItems[i].StackCount, this);
            }

            // Local Object ItemInventory will handle button refresh
        }

        public void OnWelfareClaimOnlinePrizesSingle(int dataid)
        {
            if (mWelfareCtrlr.IsOnlineRewardsClaimed(dataid) == true)
            {
                // Send System Message to client: "Online Rewards already claimed!"
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_OnlinePrizeClaimed"), "", false, mPlayer.Slot);

                return;
            }

            // Inventory full
            if (!mInventory.mInvData.HasEmptySlot())
            {
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            DateTime currentTime = DateTime.Now;

            if (currentTime.Ticks < loginDT.Ticks)
            {
                // currentTime is earlier than loginTime!
                return;
            }

            TimeSpan duration = currentTime - loginDT;
            duration += new TimeSpan(mWelfareCtrlr.GetLastOnlineDuration());
            int order = dataid + 1;

            if (duration.TotalMinutes < WelfareRepo.GetRewardTimeByOrder(order))
            {
                // Still not enough time passed!

                return;
            }

            IInventoryItem rewardItem = WelfareRepo.GetOnlinePrizeByOrder(order);

            // Add rolled items to inventory
            InvRetval addRes = mInventory.AddItemsToInventory(rewardItem, true, "Welfare");

            if (addRes.retCode == InvReturnCode.AddFailed)
            {
                // Don't claim if bag is full
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryAddFailed"), "", false, mPlayer.Slot);
                return;
            }
            else if (addRes.retCode == InvReturnCode.Full)
            {
                // Don't claim if add to bag failed
                // Add to inventory failed!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            // Claim online reward
            mWelfareCtrlr.ClaimOnlineRewards(dataid, rewardItem.ItemID);
            WelfareRules.LogWelfareOnlinePrizeGet(order, duration.TotalSeconds, this);
            WelfareRules.LogWelfareOnlinePrizeItemGet(rewardItem.ItemID, rewardItem.StackCount, this);

            // Local Object ItemInventory will handle button refresh
        }

        // Open Service Funds
        public void OnWelfareBuyOpenServiceFunds()
        {
            if (mWelfareCtrlr.IsOpenServiceFundBought() == true)
            {
                // Send System Message to client: "Open Service Fund already bought!"
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_OpenServiceFundAlreadyBought"), "", false, this);
                return;
            }

            int gold = mPlayer.SecondaryStats.Gold;
            // Get service fund cost from GMDB
            int goldCost = WelfareRules.GetServiceFundCost();

            if (goldCost == 0 || gold < goldCost)
            {
                ZRPC.NonCombatRPC.Ret_WelfareNotEnoughGold(this);
                return;
            }

            int goldBef = gold;
            if (mPlayer.DeductGold(goldCost, false, true, "Welfare_Fund") == false)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_DeductGoldFailed"), "", false, this);
                return;
            }
            int goldAft = mPlayer.SecondaryStats.Gold;

            mWelfareCtrlr.BuyOpenServiceFund();
            WelfareRules.BuyServiceFund();
            WelfareRules.LogWelfareOpenServiceFundsBuy("Welfare Open Service Funds Buy", this);

            ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_OpenServiceFundBuySuccess"), "", false, this);
        }

        public void OnWelfareClaimOpenServiceFundGoldReward(int rewardId)
        {
            if (mWelfareCtrlr.IsOpenServiceFundBought() == false)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_OpenServiceFundNotBought"), "", false, this);
                return;
            }

            if (mWelfareCtrlr.IsOpenServiceFundLevelClaimed(rewardId))
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_OpenServiceFundLvlRewardAlreadyClaimed"), "", false, this);
                return;
            }

            ServiceFundLvlReward goldReward = mWelfareCtrlr.GetOpenServiceFundLvlReward(rewardId);
            if (goldReward == null)
            {
                return;
            }

            int playerLvl = mPlayer.PlayerSynStats.Level;
            if (playerLvl < goldReward.mLevel)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_OpenServiceFundLvlRewardNotEnoughLvl"), "", false, this);
                return;
            }

            int lockGoldBef = mPlayer.SecondaryStats.bindgold;
            mPlayer.AddBindGold(goldReward.mgoldReward, "Welfare_Fund");
            mWelfareCtrlr.ClaimLevelOpenServiceFund(rewardId);
            mWelfareCtrlr.SerializeLevelOpenServiceFundClaimed();
            mWelfareCtrlr.UpdateOpenServiceFundBoughtNum(goldReward.mgoldReward);
            int lockGoldAft = mPlayer.SecondaryStats.bindgold;
            WelfareRules.LogWelfareOpenServiceFundsLockGoldGet("Welfare Open Service Funds Lock Gold Get", playerLvl, goldReward.mgoldReward, this);

            ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_OpenServiceFundLvlRewardClaimSuccess"), "", false, this);
        }

        //private GoldReward GetLevelDataByDataID(int dataid)
        //{
        //    string rewardStr = WelfareRules.GetServiceFundLvlRewards();
        //    if (!string.IsNullOrEmpty(rewardStr))
        //    {
        //        List<string> rewardList = rewardStr.Split(';').ToList();

        //        if (dataid >= 0 && dataid < rewardList.Count)
        //        {
        //            List<string> rewardData = rewardList[dataid].Split('|').ToList();

        //            int level = 0;
        //            int.TryParse(rewardData[0], out level);
        //            int goldReward = 0;
        //            int.TryParse(rewardData[1], out goldReward);

        //            return new GoldReward(level, goldReward);
        //        }
        //    }

        //    return null;
        //}

        public void OnWelfareClaimOpenServiceFundItemReward(int rewardId)
        {
            if (mWelfareCtrlr.IsOpenServiceFundBought() == false)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_OpenServiceFundNotBought"), "", false, this);
                return;
            }

            if (mWelfareCtrlr.IsOpenServiceFundPlayerClaimed(rewardId))
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_OpenServiceFundPplRewardAlreadyClaimed"), "", false, this);
                return;
            }

            ServiceFundPplReward playerReward = mWelfareCtrlr.GetOpenServiceFundPplReward(rewardId);
            if (playerReward == null)
            {
                return;
            }

            int joinMemberNum = mWelfareCtrlr.GetCurrentServiceFundsMemberNum();
            if (joinMemberNum < playerReward.mPplCount)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_OpenServiceFundPplRewardNotEnoughPpl"), "", false, this);
                return;
            }

            // Add reward item to inventory
            InvRetval addRes = mInventory.AddItemsToInventory(playerReward.mItem, true, "Welfare");

            if (addRes.retCode == InvReturnCode.AddFailed)
            {
                // Don't claim if bag is full
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryAddFailed"), "", false, mPlayer.Slot);
                return;
            }
            else if (addRes.retCode == InvReturnCode.Full)
            {
                // Don't claim if add to bag failed
                // Add to inventory failed!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }
            mWelfareCtrlr.ClaimPlayerOpenServiceFund(rewardId);
            mWelfareCtrlr.SerializePlayerOpenServiceFundClaimed();

            WelfareRules.LogWelfareOpenServiceFundsItemGet("Welfare Open Service Funds Item Get", joinMemberNum, playerReward.mItem.ItemID, playerReward.mItem.StackCount, this);

            ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_OpenServiceFundPplRewardClaimSuccess"), "", false, this);
        }

        private PlayerReward GetItemDataByDataID(int dataid)
        {
            string rewardStr = WelfareRules.GetServiceFundPplRewards();
            if (!string.IsNullOrEmpty(rewardStr))
            {
                List<string> rewardList = rewardStr.Split(';').ToList();

                if (dataid >= 0 && dataid < rewardList.Count)
                {
                    List<string> rewardData = rewardList[dataid].Split('|').ToList();

                    int pplCount = 0;
                    int.TryParse(rewardData[0], out pplCount);
                    int itemid = 0;
                    int.TryParse(rewardData[1], out itemid);
                    int itemcount = 0;
                    int.TryParse(rewardData[1], out itemcount);
                    IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(itemid);
                    if (item != null)
                    {
                        item.StackCount = (ushort)itemcount;
                    }

                    return new PlayerReward(pplCount, item);
                }
            }

            return null;
        }

        // First Gold Credit
        public void OnWelfareClaimFirstGoldCredit()
        {
            if (mWelfareCtrlr.IsFirstGoldCreditClaimed() == true)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_FirstCreditAlreadyClaimed"), "", false, mPlayer.Slot);
                return;
            }

            if (mPlayer.SecondaryStats.IsGoldEnough(1) == false)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_FirstCreditNotCreditedYet"), "", false, mPlayer.Slot);
                return;
            }

            // Inventory full
            if (!mInventory.mInvData.HasEmptySlot())
            {
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            List<IInventoryItem> rewardsList = mWelfareCtrlr.GetFirstGoldCreditRewards();

            if (rewardsList == null || rewardsList.Count == 0)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_FirstCreditGetRewardDataFailed"), "", false, mPlayer.Slot);
                return;
            }

            InvRetval addRes = mInventory.AddItemsToInventory(rewardsList, true, "FirstTopUp");

            if (addRes.retCode == InvReturnCode.AddFailed)
            {
                // Don't claim if bag is full
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryAddFailed"), "", false, mPlayer.Slot);
                return;
            }
            else if (addRes.retCode == InvReturnCode.Full)
            {
                // Don't claim if add to bag failed
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            mWelfareCtrlr.ClaimFirstGoldCredit();
            ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_FirstCreditClaimRewardSuccess"), "", false, mPlayer.Slot);

            string id, count;

            WelfareRules.SerialiseItemToString(out id, out count, rewardsList);
            WelfareRules.LogWelfareFirstTopUpItemGet("Welfare First Top Up Item Get", id, count, this);

        }

        // Total Gold Credit
        public void OnWelfareClaimTotalCreditReward(int rewardid)
        {
            if (mWelfareCtrlr.IsTotalCreditRewardClaimed(rewardid) == true)
            {
                return;
            }

            // Inventory full
            if (!mInventory.mInvData.HasEmptySlot())
            {
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            List<CreditSpendReward> rewardsList = mWelfareCtrlr.GetTotalGoldRewards(true);
            if (rewardsList == null)
            {
                return;
            }

            if (rewardsList.Count == 0)
            {
                return;
            }

            CreditSpendReward reward = rewardsList.Find(o => o.mRewardId == rewardid);

            if (reward == null)
            {
                return;
            }

            int currTotalGold = 0;

            currTotalGold = mWelfareCtrlr.GetTotalCreditedGold();

            WelfareRules.LogWelfareTotalCreditLockGoldGet("Welfare Total Credit Lock Gold", reward.mCreditCount, currTotalGold, this);

            if (reward.mClaimCount >= reward.mMaxClaim)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_TotalCreditRewardAllClaimed"), "", false, mPlayer.Slot);
                return;
            }
            else if (reward.mClaimCount < reward.mMaxClaim)
            {
                int nextClaimLevel = reward.mClaimCount + 1;
                if (currTotalGold >= (reward.mCreditCount * nextClaimLevel))
                {
                    // Get RewardList from WelfareInventory
                    List<IInventoryItem> rewardList = mWelfareCtrlr.GetTotalCreditReward(rewardid);

                    if (rewardList == null)
                    {
                        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_GetTotalCreditRewardFailed"), "", false, mPlayer.Slot);
                        return;
                    }

                    InvRetval addRes = mInventory.AddItemsToInventory(rewardList, true, "Welfare");

                    if (addRes.retCode == InvReturnCode.AddFailed)
                    {
                        // Don't claim if bag is full
                        // Inventory is full!
                        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryAddFailed"), "", false, mPlayer.Slot);
                        return;
                    }
                    else if (addRes.retCode == InvReturnCode.Full)
                    {
                        // Don't claim if add to bag failed
                        // Inventory is full!
                        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                        return;
                    }

                    mWelfareCtrlr.ClaimTotalCreditReward(rewardid);
                    mWelfareCtrlr.SerializeTotalGoldClaimed(true);

                    // <Added>
                    string id, count;

                    WelfareRules.SerialiseItemToString(out id, out count, reward.mRewardList);
                    WelfareRules.LogWelfareTotalCreditItemGet("Welfare Total Credit Item", reward.mCreditCount, id, count, this);
                }
            }
        }

        // Total Gold Spend
        public void OnWelfareClaimTotalSpendReward(int rewardid)
        {
            if (mWelfareCtrlr.IsTotalSpendRewardClaimed(rewardid) == true)
            {
                return;
            }

            // Inventory full
            if (!mInventory.mInvData.HasEmptySlot())
            {
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            List<CreditSpendReward> rewardsList = mWelfareCtrlr.GetTotalGoldRewards(true);
            if (rewardsList == null)
            {
                return;
            }

            if (rewardsList.Count == 0)
            {
                return;
            }

            CreditSpendReward reward = rewardsList.Find(o => o.mRewardId == rewardid);

            if (reward == null)
            {
                return;
            }

            int currTotalGold = 0;

            currTotalGold = mWelfareCtrlr.GetTotalSpentGold(); //total spend gold

            if (reward.mClaimCount >= reward.mMaxClaim)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_TotalSpendRewardAllClaimed"), "", false, mPlayer.Slot);
                return;
            }
            else if (reward.mClaimCount < reward.mMaxClaim)
            {
                int nextClaimLevel = reward.mClaimCount + 1;
                if (currTotalGold >= (reward.mCreditCount * nextClaimLevel))
                {
                    // Get RewardList from WelfareInventory
                    List<IInventoryItem> rewardList = mWelfareCtrlr.GetTotalSpendReward(rewardid);

                    if (rewardList == null)
                    {
                        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_GetTotalSpendRewardFailed"), "", false, mPlayer.Slot);
                        return;
                    }

                    InvRetval addRes = mInventory.AddItemsToInventory(rewardList, true, "Welfare");

                    if (addRes.retCode == InvReturnCode.AddFailed)
                    {
                        // Don't claim if bag is full
                        // Inventory is full!
                        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryAddFailed"), "", false, mPlayer.Slot);
                        return;
                    }
                    else if (addRes.retCode == InvReturnCode.Full)
                    {
                        // Don't claim if add to bag failed
                        // Inventory is full!
                        ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                        return;
                    }

                    mWelfareCtrlr.ClaimTotalSpendReward(rewardid);
                    mWelfareCtrlr.SerializeTotalGoldClaimed(false);

                    // <Added>
                    string id, count;
                    WelfareRules.SerialiseItemToString(out id, out count, reward.mRewardList);


                    WelfareRules.LogWelfareTotalSpendItemGet("Welfare Total Spend Item", reward.mCreditCount, id, count, this);
                }
            }
        }

        // Daily Gold
        public void OnWelfareDailyGoldBuyMonthlyCard()
        {
            if (mWelfareCtrlr.IsMonthlyCardBought())
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_DailyGoldMCardAlreadyBought"), "", false, mPlayer.Slot);
                return;
            }

            int goldCost = GameConstantRepo.GetConstantInt("Welfare_MCardCost");
            int gold = mPlayer.SecondaryStats.Gold;
            int totalGold = gold;

            if (totalGold < goldCost || goldCost == 0)
            {
                ZRPC.NonCombatRPC.Ret_WelfareNotEnoughGold(this);
                return;
            }

            if (mPlayer.DeductGold(goldCost, false, true, "Welfare_BuyMCard") == false)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_DeductGoldFailed"), "", false, this);
                return;
            }

            mWelfareCtrlr.BuyMonthlyCard();

            // <Add> log monthly gold?
            WelfareRules.LogWelfareDailyGoldMCardBuy("Welfare Daily Gold Monthly Card Bought", this);
        }

        public void OnWelfareDailyGoldClaimMonthlyCardGold()
        {
            if (mWelfareCtrlr.IsMonthlyCardGoldCollected())
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_DailyGoldMCardGoldAlreadyCollected"), "", false, mPlayer.Slot);
                return;
            }

            int goldPrize = GameConstantRepo.GetConstantInt("Welfare_MCardPrize");
            if (goldPrize == 0)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_DailyGoldMCardPrizeGetFailed"), "", false, mPlayer.Slot);
                return;
            }

            mWelfareCtrlr.ClaimMonthlyCardGold();

            int before = mPlayer.SecondaryStats.bindgold;

            mPlayer.AddBindGold(goldPrize, "Welfare_MCardPrize");

            int after = mPlayer.SecondaryStats.bindgold;

            // <Add> log locked gold?
            WelfareRules.LogWelfareDailyGoldMCardLockGoldGet("Welfare Daily Gold M Card Locked Gold Get", this);
        }

        public void OnWelfareDailyGoldBuyPermanentCard()
        {
            if (mWelfareCtrlr.IsPermanentCardBought())
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_DailyGoldPCardAlreadyBought"), "", false, mPlayer.Slot);
                return;
            }

            int goldCost = GameConstantRepo.GetConstantInt("Welfare_PCardCost");
            int gold = mPlayer.SecondaryStats.Gold;
            int totalGold = gold;

            if (totalGold < goldCost || goldCost == 0)
            {
                ZRPC.NonCombatRPC.Ret_WelfareNotEnoughGold(this);
                return;
            }

            if (mPlayer.DeductGold(goldCost, false, true, "Welfare_BuyPCard") == false)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_DeductGoldFailed"), "", false, this);
                return;
            }

            mWelfareCtrlr.BuyPermanentCard();

            // <Add> log daily gold Perma
            WelfareRules.LogWelfareDailyGoldPCardBuy("Welfare Daily Gold Permanent Card Bought", this);
        }

        public void OnWelfareDailyGoldClaimPermanentCardGold()
        {
            if (mWelfareCtrlr.IsPermanentCardGoldCollected())
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_DailyGoldPCardGoldAlreadyCollected"), "", false, mPlayer.Slot);
                return;
            }

            int goldPrize = GameConstantRepo.GetConstantInt("Welfare_PCardPrize");
            if (goldPrize == 0)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_DailyGoldPCardPrizeGetFailed"), "", false, mPlayer.Slot);
                return;
            }

            mWelfareCtrlr.ClaimPermanentCardGold();

            int before = mPlayer.SecondaryStats.bindgold;

            mPlayer.AddBindGold(goldPrize, "Welfare_PCardPrize");

            int after = mPlayer.SecondaryStats.bindgold;

            // <Add> log daily gold perma get
            WelfareRules.LogWelfareDailyGoldPCardLockGoldGet("Welfare Daily Gold Permanent Card Gold Get", goldPrize, before, after, this);
        }

        public void OnWelfareGoldJackpotGetResult()
        {
            int nextTier = mWelfareCtrlr.GetGoldJackpotNextTierNum();
            int highestTier = mWelfareCtrlr.GetGoldJackpotHighestTier();
            if (nextTier > highestTier)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_JackpotMaxTierReached"), "", false, this);
                return;
            }

            int lockGold = mPlayer.SecondaryStats.bindgold;
            int gold = mPlayer.SecondaryStats.Gold;
            int goldCost = mWelfareCtrlr.GetGoldJackpotNextTierCost();
            if (goldCost == 0 || !mPlayer.SecondaryStats.IsGoldEnough(goldCost, true))
            {
                ZRPC.NonCombatRPC.Ret_WelfareNotEnoughGold(this);
                return;
            }

            bool res = mPlayer.DeductGold(goldCost, true, true, "Welfare_Jackpot");

            if (res == false)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_DeductGoldFailed"), "", false, this);
                return;
            }

            System.Random rand = GameUtils.GetRandomGenerator();

            TierDataList tierDataList = new TierDataList(mWelfareCtrlr.GetGoldJackpotNextTierData());
            TierData rollResult = tierDataList.RollForGold();

            if (rollResult == null)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_GoldJackpotRollResultFailed"), "", false, mPlayer.Slot);
                return;
            }

            int lower = rollResult.mLowerbound;
            int upper = rollResult.mUpperbound;

            int result = rand.Next(lower, upper);

            mPlayer.SecondaryStats.bindgold += result;
            mWelfareCtrlr.SetGoldJackpotResult(result);

            // <Add> log gold jackpot results
            WelfareRules.LogWelfareGoldJackpotRewardGet("Welfare Gold Jackpot Rewards", mPlayer.WelfareStats.goldJackpotCurrTier, result, this);
        }

        public void OnWelfareResetGoldJackpotRoll()
        {
            mWelfareCtrlr.ResetGoldJackpotTier();
            mPlayer.WelfareStats.goldJackpotCurrTier = 0;
        }

        public void OnWelfareResetContLoginClaims()
        {
            mWelfareCtrlr.ResetContLoginClaimed();
        }

        public void OnWelfareClaimContLoginReward(int rewardId)
        {
            if (mWelfareCtrlr.IsContLoginRewardClaimed(rewardId))
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_ContLoginRewardAlreadyClaimed"), "", false, mPlayer.Slot);
                return;
            }

            // Inventory full
            if (!mInventory.mInvData.HasEmptySlot())
            {
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            ContLoginReward rewardData = mWelfareCtrlr.GetContLoginReward(rewardId);
            if (rewardData == null)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_ContLoginRewardNoRewardDataFound"), "", false, mPlayer.Slot);
                return;
            }

            InvRetval addRes = mInventory.AddItemsToInventory(rewardData.mRewardList, true, "Welfare");

            if (addRes.retCode == InvReturnCode.AddFailed)
            {
                // Don't claim if bag is full
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryAddFailed"), "", false, mPlayer.Slot);
                return;
            }
            else if (addRes.retCode == InvReturnCode.Full)
            {
                // Don't claim if add to bag failed
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            mWelfareCtrlr.ClaimContLogin(rewardId);
            mWelfareCtrlr.SerializeContLoginClaimed();

            // <Add> log continuous login reward the amount might be used the wrong wat, take NOTE
            WelfareRules.LogWelfareContinuousLogin("Welfare Continuous Login Reward Get", rewardId, rewardData.mRewardList.Count, this);
        }
        #endregion

        #region QuestExtraRewards
        public void OnQERFinishTask(int taskid)
        {
            if (mQuestExtraRewardsCtrler.IsTaskCompletedServer(taskid))
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskAlreadyComplete"), "", false, mPlayer.Slot);
                return;
            }

            QETaskDataList taskDataList = QuestExtraRewardsRepo.GetTaskList();
            QETaskData taskData = taskDataList.Get(taskid);

            if (taskData == null)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskDataMissing"), "", false, mPlayer.Slot);
                return;
            }

            if (!mQuestExtraRewardsCtrler.IsUnlocked(taskData))
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskNotUnlocked"), "", false, mPlayer.Slot);
                return;
            }

            int goldCost = taskData.GoldToDone();

            if (!mPlayer.SecondaryStats.IsGoldEnough(goldCost, true))
            {
                ZRPC.NonCombatRPC.QERFailedGold(mPlayer.Slot);
                return;
            }

            if (mPlayer.DeductGold(goldCost, true, true, "QER_Finish") == false)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_DeductGoldFailed"), "", false, this);
                return;
            }

            mQuestExtraRewardsCtrler.SetTaskComplete(taskid);
            QuestExtraRewardsRules.LogQERTaskFinish("Quest Extra Reward Task Finished", taskData.TaskID(), this);
        }

        public void OnQERFinishTaskAll(string taskidsStr)
        {
            List<string> taskidsRaw = taskidsStr.Split('|').ToList();
            List<int> taskids = new List<int>();
            for (int i = 0; i < taskidsRaw.Count; ++i)
            {
                int id = 0;

                if (int.TryParse(taskidsRaw[i], out id))
                {
                    taskids.Add(id);
                }
            }

            for (int i = 0; i < taskids.Count; ++i)
            {
                int taskid = taskids[i];
                if (mQuestExtraRewardsCtrler.IsTaskCompletedServer(taskid))
                {
                    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskAlreadyComplete"), "", false, mPlayer.Slot);
                    return;
                }
            }

            QETaskDataList taskDataList = QuestExtraRewardsRepo.GetTaskList();

            for (int i = 0; i < taskids.Count; ++i)
            {
                int taskid = taskids[i];
                QETaskData taskData = taskDataList.Get(taskid);

                if (taskData == null)
                {
                    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskDataMissing"), "", false, mPlayer.Slot);
                    return;
                }

                if (!mQuestExtraRewardsCtrler.IsUnlocked(taskData))
                {
                    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskNotUnlocked"), "", false, mPlayer.Slot);
                    return;
                }
            }

            int goldCost = 0;
            for (int i = 0; i < taskids.Count; ++i)
            {
                int taskid = taskids[i];
                QETaskData taskData = taskDataList.Get(taskid);

                if (taskData == null)
                {
                    continue;
                }

                if (!mQuestExtraRewardsCtrler.IsUnlocked(taskData))
                {
                    continue;
                }

                goldCost += taskData.GoldToDone();
            }

            if (!mPlayer.SecondaryStats.IsGoldEnough(goldCost, true))
            {
                ZRPC.NonCombatRPC.QERFailedGold(mPlayer.Slot);
                return;
            }

            if (mPlayer.DeductGold(goldCost, true, true, "QER_FinishAll") == false)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_DeductGoldFailed"), "", false, this);
                return;
            }

            StringBuilder taskIdStr = new StringBuilder();
            for (int i = 0; i < taskids.Count; ++i)
            {
                int taskid = taskids[i];
                taskIdStr.Append(taskid);
                taskIdStr.Append(",");

                mQuestExtraRewardsCtrler.SetTaskComplete(taskid);
            }

            string taskIds = taskIdStr.ToString().TrimEnd(',');
            QuestExtraRewardsRules.LogQERTaskFinishAll("Quest Extra Reward All Task Finished", taskIds, this);
        }

        public void OnQERClaimTaskReward(int taskid)
        {
            if (mQuestExtraRewardsCtrler.IsTaskCollectedServer(taskid))
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskAlreadyClaimed"), "", false, mPlayer.Slot);
                return;
            }

            if (!mQuestExtraRewardsCtrler.IsTaskCompletedServer(taskid))
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskNotComplete"), "", false, mPlayer.Slot);
                return;
            }

            QETaskDataList taskDataList = QuestExtraRewardsRepo.GetTaskList();
            if (taskDataList == null)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskList_NotFound"), "", false, this);
                return;
            }

            QETaskData taskData = taskDataList.Get(taskid);
            if (taskData == null)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskDataMissing"), "", false, mPlayer.Slot);
                return;
            }

            int activePtsBef = mQuestExtraRewardsCtrler.GetActivePoints();

            mQuestExtraRewardsCtrler.AddActivePoints(taskData.RewardPts());

            int activePtsAft = mQuestExtraRewardsCtrler.GetActivePoints();
            QuestExtraRewardsRules.LogQERActivePtsGet("QER_ActivePtsGet", taskData.RewardPts(), activePtsAft, this);
            QuestExtraRewardsRules.LogQERActivePtsGetQuestID("QER_ActivePtsGet", taskData.TaskID(), "Recieve Active Points", this);

            //int currencyBef = mPlayer.GetCurrencyAmt(taskData.RewardCurrencyType());
            //List<RewardItemInfo> itemInfos = RewardListRepo.GetRewardItemsById(taskData.RewardItem());
            //RewardItemInfo itemInfo = itemInfos[0];
            //int itemCountBef = mInventory.GetItemStackCountByItemId(itemInfo.itemid);

            //GameRules.GiveReward_Bag(mPlayer, new List<int>() { taskData.RewardItem() }, true, true, "QER");

            //int currencyAft = mPlayer.GetCurrencyAmt(taskData.RewardCurrencyType());
            //int itemCountAft = mInventory.GetItemStackCountByItemId(itemInfo.itemid);
            //LogQERCurrencyGet(taskData, taskData.RewardCurrency(), currencyBef, currencyAft, this);
            //QuestExtraRewardsRules.LogQERItemGet(taskData.TaskID(), "QER_ItemGet", itemInfo.count, this);

            //mQuestExtraRewardsCtrler.CollectTaskReward(taskid);
            //mQuestExtraRewardsCtrler.SerializeTaskCollected();
        }

        public void OnQERClaimTaskRewardAll(string taskidsStr)
        {
            List<string> taskidsRaw = taskidsStr.Split('|').ToList();
            List<int> taskids = new List<int>();
            for (int i = 0; i < taskidsRaw.Count; ++i)
            {
                int id = 0;

                if (int.TryParse(taskidsRaw[i], out id))
                {
                    taskids.Add(id);
                }
            }

            for (int i = 0; i < taskids.Count; ++i)
            {
                int taskid = taskids[i];
                if (mQuestExtraRewardsCtrler.IsTaskCollectedServer(taskid))
                {
                    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskAlreadyClaimed"), "", false, mPlayer.Slot);
                    return;
                }
            }

            QETaskDataList taskDataList = QuestExtraRewardsRepo.GetTaskList();
            if (taskDataList == null)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskList_NotFound"), "", false, this);
                return;
            }

            for (int i = 0; i < taskids.Count; ++i)
            {
                int taskid = taskids[i];
                QETaskData taskData = taskDataList.Get(taskid);

                if (taskData == null)
                {
                    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_TaskDataMissing"), "", false, mPlayer.Slot);
                    return;
                }
            }

            //List<QECurrencyReward> currencyRewards = new List<QECurrencyReward>();
            Dictionary<CurrencyType, int> currencyRewards = new Dictionary<CurrencyType, int>();
            List<IInventoryItem> rewardItems = new List<IInventoryItem>();
            for (int i = 0; i < taskids.Count; ++i)
            {
                int taskid = taskids[i];
                QETaskData taskData = taskDataList.Get(taskid);

                int activePtsBef = mQuestExtraRewardsCtrler.GetActivePoints();

                mQuestExtraRewardsCtrler.AddActivePoints(taskData.RewardPts());

                int activePtsAft = mQuestExtraRewardsCtrler.GetActivePoints();
                QuestExtraRewardsRules.LogQERActivePtsGet("QER_ActivePtsGet", taskData.RewardPts(), activePtsAft, this);
                QuestExtraRewardsRules.LogQERActivePtsGetQuestID("QER_ActivePtsGet", taskData.TaskID(), "Recieve Active Points", this);

                //int currencyBef = mPlayer.GetCurrencyAmt(taskData.RewardCurrencyType());
                //List<RewardItemInfo> itemInfos = RewardListRepo.GetRewardItemsById(taskData.RewardItem());
                //RewardItemInfo itemInfo = itemInfos[0];
                //int itemCountBef = mInventory.GetItemStackCountByItemId(itemInfo.itemid);

                //GameRules.GiveReward_Bag(mPlayer, new List<int>() { taskData.RewardItem() }, true, true, "QER");

                //int currencyAft = mPlayer.GetCurrencyAmt(taskData.RewardCurrencyType());
                //int itemCountAft = mInventory.GetItemStackCountByItemId(itemInfo.itemid);
                //LogQERCurrencyGet(taskData, taskData.RewardCurrency(), currencyBef, currencyAft, this);
                //QuestExtraRewardsRules.LogQERItemGet(taskData.TaskID(), "QER_ItemGet", itemInfo.count, this);
            }

            for (int i = 0; i < taskids.Count; ++i)
            {
                int taskid = taskids[i];
                mQuestExtraRewardsCtrler.CollectTaskReward(taskid);
            }

            mQuestExtraRewardsCtrler.SerializeTaskCollected();
        }

        private void LogQERCurrencyGet(QETaskData taskData, int amount, int currencyBef, int currencyAft, GameClientPeer peer)
        {
            if (taskData == null)
            {
                return;
            }

            switch (taskData.RewardCurrencyType())
            {
                case CurrencyType.Money:
                    QuestExtraRewardsRules.LogQERMoneyGet(taskData.TaskID(), "QER_MoneyGet", amount, currencyBef, currencyAft, peer);
                    break;
                //case CurrencyType.VIP:
                //    QuestExtraRewardsRules.LogQERVIPXPGet("Quest Extra Reward", taskData.TaskID(), "QER_VIPXPGet", amount, peer);
                //    break;
                case CurrencyType.LockGold:
                    QuestExtraRewardsRules.LogQERLockGoldGet(taskData.TaskID(), "QER_LockGoldGet", amount, currencyBef, currencyAft, peer);
                    break;
            }
        }

        public void OnQERClaimBoxReward(int boxNum)
        {
            // Inventory full
            if (!mInventory.mInvData.HasEmptySlot())
            {
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            if (mQuestExtraRewardsCtrler.IsBoxRewardCollectedServer(boxNum))
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_RewardAlreadyClaimed"), "", false, mPlayer.Slot);
                return;
            }

            int currActvPts = mQuestExtraRewardsCtrler.GetActivePoints();
            if (mQuestExtraRewardsCtrler.IsBoxRewardCollectableServer(boxNum, currActvPts) == false)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_InsufficientPoints"), "", false, mPlayer.Slot);
                return;
            }

            QERewardDataList rewardDataList = QuestExtraRewardsRepo.GetRewardList();
            if (rewardDataList == null)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_RewardDataMissing"), "", false, mPlayer.Slot);
                return;
            }

            //QERewardData rewardData = rewardDataList.Get(boxNum);
            //List<RewardItemInfo> rewardlist = RewardListRepo.GetRewardItemsById(rewardData.RewardList());
            //if (rewardlist == null)
            //{
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_QER_RewardList_NotFound"), "", false, this);
            //    return;
            //}

            //int itemCountBef = mInventory.GetItemStackCountByItemId(rewardlist[0].itemid);
            //bool isFull = false;
            //GameRules.GiveReward_CheckBagSlot(mPlayer, new List<int>() { rewardData.RewardList() }, out isFull, true, true, string.Format("QERBox id={0}", rewardData.BoxID()));
            //if (isFull)
            //{
            //    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
            //    return;
            //}

            //mQuestExtraRewardsCtrler.CollectBoxReward(boxNum);
            //int itemCountAft = mInventory.GetItemStackCountByItemId(rewardlist[0].itemid);
            //QuestExtraRewardsRules.LogQERBoxRewardGet("Quest Extra Reward Box Reward Get", currActvPts, rewardData.BoxID(), this);
        }
        #endregion

        #region ReviveItem
        public void StartReviveItemRequest(string requestor, string requestee, int itemId)
        {
            GameApplication.Instance.ReviveItemController.RequestReviveItem(requestor, requestee, itemId);
        }

        public void RequestReviveItem(string requestor, int requestId)
        {
            ZRPC.NonCombatRPC.RequestReviveItem(requestor, requestId, this);
        }

        public void AcceptReviveItem(int requestId)
        {
            GameApplication.Instance.ReviveItemController.AcceptReviveItemRequest(requestId);
        }

        public void RejectReviveItem(int requestId)
        {
            GameApplication.Instance.ReviveItemController.RejectReviveItemRequest(requestId);
        }

        public void ReviveItemRejected()
        {
            ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_Rejected"), "", false, this);
        }

        public void ConfirmAcceptReviveItem(int sessionId, string otherPlayer, string reviveItemrStatus)
        {
            ZRPC.NonCombatRPC.ConfirmAcceptReviveItem(sessionId, otherPlayer, reviveItemrStatus, this);
        }

        public void CancelReviveItem(int sessionId)
        {
            GameApplication.Instance.ReviveItemController.CancelReviveItem(sessionId);
        }

        public void ConfirmCancelReviveItem()
        {
            ZRPC.NonCombatRPC.ConfirmCancelReviveItem(this);
        }

        public void ConfirmCompleteReviveItem()
        {
            ZRPC.NonCombatRPC.ConfirmCompleteReviveItem(this);
        }

        public void DeductReviveItem(int sessionId, int itemId)
        {
            if (itemId == -1) // Invalid item id!
                return;

            InvRetval res = mInventory.DeductItems((ushort)itemId, 1, "Revive Item");
            if (res.retCode == InvReturnCode.UseFailed)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_DeductItemFailed"), "", false, this);
            }
            else
            {
                GameApplication.Instance.ReviveItemController.ConfirmItemDeducted(sessionId);
            }
        }

        public void RevivePlayer(int sessionId)
        {
            mPlayer.RespawnOnSpot();
            GameApplication.Instance.ReviveItemController.ConfirmRevived(sessionId);
        }
        #endregion

        #region PowerUp
        public void OnPowerUp(int part)
        {
            int currPartLevel = characterData.PowerUpInventory.powerUpSlots[part];
            int nextPartLevel = currPartLevel + 1;

            int playerLevel = mPlayer.PlayerSynStats.Level;
            if(nextPartLevel > playerLevel)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_PowerUp_MaxLevelReached"), "", false, mPlayer.Slot);
                return;
            }

            PowerUpJson nextPowerUp = PowerUpRepo.GetPowerUpByPartsLevel((PowerUpPartsType)part, nextPartLevel);
            if(nextPowerUp == null)
            {
                return;
            }
            
            List<ItemInfo> useMatList = PowerUpRepo.GetPowerUpMaterialByPartsEffect((PowerUpPartsType)part, nextPartLevel);
            InvRetval result = mInventory.DeductItems(useMatList, "PowerUp");
            if(result.retCode == InvReturnCode.UseFailed)
            {
                // Handle use item failure
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_PowerUp_NotEnoughMaterials"), "", false, mPlayer.Slot);
                return;
            }

            characterData.PowerUpInventory.powerUpSlots[part] = nextPartLevel;
            mPlayer.PowerUpStats.powerUpSlots[part] = nextPartLevel;
        }

        public void OnMeridianLevelUp(int type)
        {
            int currTypeLevel = characterData.PowerUpInventory.meridianLevelSlots[type];
            int currTypeExp = characterData.PowerUpInventory.meridianExpSlots[type];
            int currCurrency = mPlayer.SecondaryStats.Money;
            MeridianUnlockListJson unlockData = PowerUpRepo.GetMeridianUnlockByTypesLevel(type, currTypeLevel);
            MeridianExpListJson expData = PowerUpRepo.GetMeridianExpByTypesLevel(type, currTypeExp);

            if(currTypeExp < expData.exp)
            {
                return;
            }

            MeridianUnlockListJson unlockNextData = PowerUpRepo.GetMeridianUnlockByTypesLevel(type, currTypeLevel + 1);
            if (unlockNextData == null)
            {
                return;
            }

            if(currCurrency < unlockData.currency)
            {
                return;
            }

            List<ItemInfo> useMatList = PowerUpRepo.GetMeridianUnlockMaterial(type, currTypeLevel);
            InvRetval result = mInventory.DeductItems(useMatList, "MeridianLevelUp");
            if (result.retCode == InvReturnCode.UseFailed)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_MeridianLevelUp_NotEnoughMaterials"), "", false, mPlayer.Slot);
                return;
            }

            characterData.PowerUpInventory.meridianLevelSlots[type] = currTypeLevel + 1;
            characterData.PowerUpInventory.meridianExpSlots[type] = 0;
            mPlayer.MeridianStats.meridianLevelSlots[type] = currTypeLevel + 1;
            mPlayer.MeridianStats.meridianExpSlots[type] = 0;
        }

        public void OnMeridianExpUp(int type)
        {
            int currTypeLevel = characterData.PowerUpInventory.meridianLevelSlots[type];
            int currTypeExp = characterData.PowerUpInventory.meridianExpSlots[type];
            int currCurrency = mPlayer.SecondaryStats.Money;
            MeridianExpListJson expData = PowerUpRepo.GetMeridianExpByTypesLevel(type, currTypeExp);

            if(expData == null)
            {
                return;
            }
            
            if (currTypeExp >= expData.exp)
            {
                return;
            }

            if(currCurrency < expData.currency)
            {
                return;
            }

            int ctr = PowerUpRepo.MeridianCriticalExp(type, currTypeLevel);
            int expGain = mPowerUpController.GetExp() * ctr;
            if(currTypeExp + expGain > expData.exp)
            {
                characterData.PowerUpInventory.meridianExpSlots[type] = expData.exp;
                mPlayer.MeridianStats.meridianExpSlots[type] = expData.exp;
            } else
            {
                characterData.PowerUpInventory.meridianExpSlots[type] += expGain;
                mPlayer.MeridianStats.meridianExpSlots[type] = characterData.PowerUpInventory.meridianExpSlots[type];
            }
        }
        #endregion

        #region EquipmentCraft
        public void OnEquipmentCraft(int itemId)
        {
            if (EquipmentCraftRepo.GetEquipmentMaterial(itemId) == null)
            {
                return;
            }

            List<ItemInfo> useMatList = PowerUpUtilities.ConvertMaterialFormat(EquipmentCraftRepo.GetEquipmentMaterial(itemId));

            for (int i = 0; i < useMatList.Count; ++i)
            {
                bool hasEnoughItem = mInventory.HasItem(useMatList[i].itemId, useMatList[i].stackCount);
                if (!hasEnoughItem)
                {
                    return;
                }
            }

            if (!mInventory.mInvData.HasEmptySlot())
            {
                // Inventory is full!
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mPlayer.Slot);
                return;
            }

            InvRetval result = mInventory.DeductItems(useMatList, "EquipmentCraftUseMaterial");
            if (result.retCode == InvReturnCode.UseFailed)
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipmentCraft_NotEnoughMaterials"), "", false, mPlayer.Slot);
                return;
            }

            List<int> currency = EquipmentCraftRepo.GetCurrency(itemId);

            if (mPlayer.SecondaryStats.Money < currency[1])
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_EquipmentCraft_NotEnoughCurrency"), "", false, mPlayer.Slot);
                return;
            }
            mPlayer.DeductCurrency((CurrencyType)currency[0], currency[1], false, "EquipmentCraftUseCurrency");

            //GivePlayerEquipment
            IInventoryItem giveItem = GameRepo.ItemFactory.GetInventoryItem(itemId);
            mInventory.AddItemsToInventory(giveItem, true, "EquipmentCraftCraftedItem");
            mPlayer.EquipmentCraftStats.finishedCraft = true;

            // Achievements
            mPlayer.UpdateAchievement(AchievementObjectiveType.CraftingItem, itemId.ToString(), false);
            mPlayer.UpdateAchievement(AchievementObjectiveType.CraftingCount);
        }
        #endregion

        #region EquipFusion
        public void OnEquipFusion (int itemIndex, string consumeIndex, bool changed)
        {
            string fusionData = characterData.EquipFusionInventory.FusionData;

            if(fusionData == string.Empty)
            {
                List<int> consumeIdx = FusionCheck(itemIndex, consumeIndex);
                if (consumeIdx == null)
                {
                    return;
                }
                fusionData = FusionConsume(consumeIdx);
            }

            if (changed)
                mInventory.UpdateEquipFusion(itemIndex, fusionData);

            mPlayer.EquipFusionStats.FinishedFusion = true;
            characterData.EquipFusionInventory.FusionData = string.Empty;
            mPlayer.EquipFusionStats.FusionData = string.Empty;
        }

        public void OnEquipFusionGive (int itemIndex, string consumeIndex)
        {
            List<int> consumeIdx = FusionCheck(itemIndex, consumeIndex);
            if(consumeIdx == null)
            {
                return;
            }

            characterData.EquipFusionInventory.FusionItemSort = itemIndex;
            mPlayer.EquipFusionStats.FusionItemSort = itemIndex;
            characterData.EquipFusionInventory.FusionData = FusionConsume(consumeIdx);
            mPlayer.EquipFusionStats.FusionData = characterData.EquipFusionInventory.FusionData;
        }

        List<int> FusionCheck (int itemIndex, string consumeIndex)
        {
            Equipment equip = mInventory.mInvData.Slots[itemIndex] as Equipment;
            if (equip == null)
            {
                return null;
            }

            List<string> consumeItemIndex = consumeIndex.Split('|').ToList();
            List<int> consumeIntIndex = new List<int>();
            if (consumeItemIndex.Count != 4)
            {
                return null;
            }
            for (int i = 0; i < 4; ++i)
            {
                int index = 0;
                if (int.TryParse(consumeItemIndex[i], out index))
                {
                    consumeIntIndex.Add(index);
                }
                else
                {
                    return null;
                }
            }

            if (mInventory.mInvData.Slots[consumeIntIndex[0]] as Equipment == null)
            {
                return null;
            }

            List<ElementalStone> stone = new List<ElementalStone>();

            for (int i = 1; i < 4; ++i)
            {
                stone.Add(mInventory.mInvData.Slots[consumeIntIndex[i]] as ElementalStone);
            }

            int type = EquipFusionRepo.ConvertStoneType(equip.JsonObject.itemsort);

            for (int i = 0; i < stone.Count; ++i)
            {
                if(type != EquipFusionRepo.GetStoneJson(stone[i].ItemID).type)
                {
                    return null;
                }
            }
            return consumeIntIndex;
        }

        string FusionConsume(List<int> itemIndex)
        {
            List<ElementalStone> fusionStones = new List<ElementalStone>();
            for (int i = 1; i < 4; ++i)
            {
                fusionStones.Add(mInventory.mInvData.Slots[itemIndex[i]] as ElementalStone);
            }

            int totalCurrency = 0;
            totalCurrency = EquipFusionRepo.GetTotalCurrencyCount(fusionStones[0].ItemID,
                                                                   fusionStones[1].ItemID,
                                                                   fusionStones[2].ItemID);
            if (totalCurrency > mPlayer.SecondaryStats.Money)
            {
                return string.Empty;
            }

            for (int i = 0; i < itemIndex.Count; ++i)
            {
                mInventory.RemoveInventoryItem(itemIndex[i], "EquipFusionRemoveItem");
            }
            mPlayer.DeductCurrency((CurrencyType)1, totalCurrency, false, "EquipFusionUseCurrency");

            return EquipFusionRepo.RandomSideEffectPutEquip(fusionStones);
        }
        #endregion

        #region CharacterData Helper Functions

        public int GetTotalStackCountByItemID(int itemid)
        {
            return mInventory.GetItemStackCountByItemId((ushort)itemid);
        }

        #endregion

        #region Mail
        public bool HasNewMail()
        {
            return CharacterData.MailInventory.hasNewMail;
        }

        public void RetrieveMail()
        {
            if (RoomReference != null && mPlayer != null)
            {
                //inform client
                MailManager.Instance.RetrieveMail(this);
            }
        }

        public int OpenMail(int mailIndex)
        {
            int mailReturnCode = MailManager.Instance.OpenMail(this, mailIndex);
            return mailReturnCode;
        }

        public int TakeAttachment(int mailIndex)
        {
            int mailReturnCode = MailManager.Instance.TakeAttachment(this, mailIndex);
            return mailReturnCode;
        }

        public int TakeAllAttachment(out string lstTakenMailIndexSerialStr)
        {
            int mailReturnCode = MailManager.Instance.TakeAllAttachment(this, out lstTakenMailIndexSerialStr);
            return mailReturnCode;
        }

        public int DeleteMail(int mailIndex)
        {
            int mailReturnCode = MailManager.Instance.DeleteMail(this, mailIndex);
            return mailReturnCode;
        }

        public int DeleteAllMail()
        {
            int mailReturnCode = MailManager.Instance.DeleteAllMail(this);
            return mailReturnCode;
        }
        #endregion Mail

        #region ItemMall

        public int ItemMallPurchaseItem(int id, bool isGM, int stackToBuy, long dt)
        {
            int itemMallReturnCode = ItemMallManager.Instance.PurchaseItem(this, id, isGM, stackToBuy, dt);
            return itemMallReturnCode;
        }

        public string ItemMallInit_Client_MallData()
        {
            return ItemMallManager.Instance.ItemMallInit_Client_MallData(this);
        }

        public List<string> ItemMallInit_Client_ItemData(int category, long lastGrab)
        {
            return ItemMallManager.Instance.ItemMallInit_Client_ItemData(category, lastGrab);
        }

        #endregion ItemMall

        #region OfflineExp

        public OfflineExpManager2 GetOfflineExp2()
        {
            return OfflineExpManager2.Instance;
        }

        #endregion OfflineExp

        #region Tutorial
        public void OnTriggerTutorial(int id)
        {
            TutorialRules.TriggleTutorialEvent(id, mPlayer);
        }
        #endregion

        public void OnCurrencyExchange()
        {
            int goldBefore = mPlayer.SecondaryStats.Gold;
            int bindgoldBefore = mPlayer.SecondaryStats.bindgold;
            int moneyBefore = mPlayer.SecondaryStats.Money;
            int currExchangeTime = mPlayer.SecondaryStats.CurrencyExchangeTime;
            int reqGold = CurrencyExchangeRepo.GetReqGold(currExchangeTime);

            if (0 /*VIPRepo.GetVIPPrivilege("Alchemy", mPlayer.PlayerSynStats.vipLvl)*/ - currExchangeTime < 0)
            {
                ZRPC.CombatRPC.Ret_OnCurrencyExchange(0, 0, this);//failed
            }
            else if (mPlayer.DeductGold(reqGold, true, true, "Exchange"))
            {
                int rewardMoney = CurrencyExchangeRepo.GetRewardMoney(currExchangeTime);
                int roll = GameUtils.RandomInt(1, 100);
                int mutiplier = CurrencyExchangeRules.GetMultiplier(roll);
                mPlayer.AddMoney(rewardMoney * mutiplier, "Exchange");
                ZRPC.CombatRPC.Ret_OnCurrencyExchange((byte)mutiplier, (byte)mPlayer.SecondaryStats.CurrencyExchangeTime, this);
                ++mPlayer.SecondaryStats.CurrencyExchangeTime;
                if (mPlayer.SecondaryStats.CurrencyExchangeTime > byte.MaxValue - 1)
                {
                    mPlayer.SecondaryStats.CurrencyExchangeTime = byte.MaxValue - 1;
                }
                mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.AlchemyTimes);
                ZLog_CurrencyExchange(goldBefore, bindgoldBefore, moneyBefore, reqGold, rewardMoney, mutiplier);
            }
            else
                ZRPC.CombatRPC.Ret_OnCurrencyExchange(0, 0, this);//failed
        }

        void ZLog_CurrencyExchange(int beforeGold, int beforeBindGold, int beforeMoney, int reqGold, int rewardMoney, int multiplier)
        {
            string message = "";

            Zealot.Logging.Client.LogClasses.CurrencyExchange log = new Zealot.Logging.Client.LogClasses.CurrencyExchange();
            log.userId = mUserId;
            log.charId = GetCharId();
            log.message = message;

            log.bindGoldBefore = beforeBindGold;
            log.bindGoldAfter = mPlayer.SecondaryStats.bindgold;
            log.goldBefore = beforeGold;
            log.goldAfter = mPlayer.SecondaryStats.Gold;
            log.moneyBefore = beforeMoney;
            log.moneyAfter = mPlayer.SecondaryStats.Money;
            log.moneyOriginal = rewardMoney;
            log.moneyGained = rewardMoney * multiplier;
            log.multiplier = multiplier;
            log.reqGold = reqGold;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(log);
        }

        #endregion
    }

    public class DefaultLevelBeforeEnterRealm
    {
        public string levelName = "";
        public Vector3 pos = Vector3.zero;

        public DefaultLevelBeforeEnterRealm(string level, Vector3 position)
        {
            this.levelName = level;
            this.pos = position;
        }
    }

    public class TransferServerInfo
    {
        public string charname = "";
        public int serverid = 0;

        public TransferServerInfo(string charname, int serverid)
        {
            this.charname = charname;
            this.serverid = serverid;
        }
    }
}
