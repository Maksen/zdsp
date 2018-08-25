// --------------------------------------------------------------------------------------------------------------------
// chensheng 2/4/2018
// --------------------------------------------------------------------------------------------------------------------
namespace Photon.LoadBalancing.GameServer
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using ExitGames.Concurrency.Fibers;
    using Photon.SocketServer;
    using Zealot.DBRepository;
    using Zealot.Server.Inventory;
    using Kopio.JsonContracts;
    using Zealot.Repository;
    using Zealot.RPC;
    using Zealot.Server.EventMessage;
    using Zealot.Server.Counters;
    using Zealot.Server.Rules;
    using Zealot.Entities;
    using Zealot.Common;
    using Zealot.Common.RPC;
    using Zealot.Common.Actions;
    using Zealot.Server.Actions;
    using Zealot.Server.Entities;
    using Zealot.Common.Datablock;
    using Hive;
    using Photon.Common.LoadBalancer.Common;
    using Hive.Caching;
    using ServerToServer;
    #endregion
    #region data class
    public class CharacterOnlineData
    {
        public int level { get; set; }
        public string server { get; set; } //empty server means offline
        public DateTime? logoutdt { get; set; }
        public int refcount = 0;
    }
    #endregion
    public partial class GameApplication : ApplicationBase
    {
        #region Constants and Fields
        private Dictionary<string, string> userCookies; //userid <- cookie
        private Dictionary<string, GameClientPeer> peerByUserId;
        private Dictionary<string, GameClientPeer> mPeerByChar;
        private Dictionary<string, CharacterOnlineData> mCharOnlineMap;
        public ServerConfig MyServerConfig;
        public int mPCU = 0;
        private DateTime mLastCPUMonitorDT;
        private TimeSpan mLastCPUTime;
        public GMServerStatus mServerStatus;
        //IDisposable ItemMall_LimitedItemSaleTimer;     
        public static DBRepository dbRepository;
        public static DBRepositoryGM dbGM;
        public LeaderboardController Leaderboard { get; protected set; }
        #endregion

        #region Constructors and Destructors
        protected void InitLogic()
        {
            userCookies = new Dictionary<string, string>();
            peerByUserId = new Dictionary<string, GameClientPeer>();
            mPeerByChar = new Dictionary<string, GameClientPeer>();
            mCharOnlineMap = new Dictionary<string, CharacterOnlineData>();
            mRoomsToBeRemoved = new List<Room>();
            RegisterActions();
        }

        private void RegisterActions()
        {
            ActionManager.RegisterAction(ACTIONTYPE.IDLE, typeof(IdleActionCommand), typeof(NonServerAuthoASIdle));
            ActionManager.RegisterAction(ACTIONTYPE.WALK, typeof(WalkActionCommand), typeof(NonServerAuthoASWalk));//placeholder
            ActionManager.RegisterAction(ACTIONTYPE.CASTSKILL, typeof(CastSkillCommand), typeof(NonServerAuthoCastSkill));
            ActionManager.RegisterAction(ACTIONTYPE.Flash, typeof(FlashActionCommand), typeof(NonServerAuthoFlash));
            ActionManager.RegisterAction(ACTIONTYPE.WALKANDCAST, typeof(WalkAndCastCommand), typeof(NonServerAuthoWalkAndCast));
            ActionManager.RegisterAction(ACTIONTYPE.DEAD, typeof(DeadActionCommand), null);
            ActionManager.RegisterAction(ACTIONTYPE.SNAPSHOTUPDATE, typeof(SnapShotUpdateCommand), null);
            ActionManager.RegisterAction(ACTIONTYPE.APPROACH, typeof(ApproachCommand), null);
            ActionManager.RegisterAction(ACTIONTYPE.WALK_WAYPOINT, typeof(WalkToWaypointActionCommand), null);
            ActionManager.RegisterAction(ACTIONTYPE.INTERACT, typeof(InteractCommand), null);
            ActionManager.RegisterAction(ACTIONTYPE.DASHATTACK, typeof(DashAttackCommand), typeof(NonServerAuthoDashAttack));
            ActionManager.RegisterAction(ACTIONTYPE.KNOCKEDBACK, typeof(KnockedBackCommand), null);//KnockBack only for monsters.
            ActionManager.RegisterAction(ACTIONTYPE.KNOCKEDUP, typeof(KnockedUpCommand), null);//KnockBackup only for monsters.
            ActionManager.RegisterAction(ACTIONTYPE.DRAGGED, typeof(DraggedActionCommand), null);
            ActionManager.RegisterAction(ACTIONTYPE.GETHIT, typeof(GetHitCommand), null);
        }
        #endregion

        #region Broadcast Channel
        public void BroadcastRoomMessage(RoomBroadcastMessage message)
        {
            foreach (KeyValuePair<string, GameClientPeer> kvp in mPeerByChar)
            {
                Player player = kvp.Value.mPlayer;
                if (player != null)
                    message.SendEvent(player);
            }
        }

        public void BroadcastMessage(BroadcastMessageType type, string parameters)
        {
            RPCBroadcastData rpcdata = ZRPC.CombatRPC.GetSerializedRPC(ServerCombatRPCMethods.BroadcastMessageToClient, (byte)type, parameters);
            RoomBroadcastMessage roommessage = new RoomBroadcastMessage(rpcdata);
            BroadcastRoomMessage(roommessage);
        }

        public void BroadcastMessage_Party(BroadcastMessageType type, string parameters, PartyStatsServer party)
        {
            RPCBroadcastData rpcdata = ZRPC.CombatRPC.GetSerializedRPC(ServerCombatRPCMethods.BroadcastMessageToClient, (byte)type, parameters);
            foreach (var _member in party.GetPartyMemberList())
            {
                if (_member.Value.IsHero())
                    continue;

                GameClientPeer _peer;
                if (mPeerByChar.TryGetValue(_member.Key, out _peer) && _peer.mPlayer != null)
                    _peer.SendEvent(rpcdata.EventData, rpcdata.SendParameters);
            }
        }

        public void BroadcastChatMessage(ChatMessage chatMessage)
        {
            ChatBroadcastMessage roommessage = new ChatBroadcastMessage(chatMessage);
            BroadcastRoomMessage(roommessage);
        }

        public void BroadcastChatMessage_Faction(ChatMessage chatMessage)
        {
            FactionChatBroadcastMessage roommessage = new FactionChatBroadcastMessage(chatMessage);
            BroadcastRoomMessage(roommessage);
        }

        public void BroadcastChatMessage_Guild(ChatMessage chatMessage, int guildId)
        {
            GuildChatBroadcastMessage roommessage = new GuildChatBroadcastMessage(chatMessage, guildId);
            BroadcastRoomMessage(roommessage);
        }

        public void BroadcastChatMessage_Party(ChatMessage chatMessage, int partyId)
        {
            var _party = PartyRules.GetPartyById(partyId);
            if (_party == null)
                return;

            foreach (var _member in _party.GetPartyMemberList())
            {
                if (_member.Value.IsHero())
                    continue;

                GameClientPeer _peer;
                if (mPeerByChar.TryGetValue(_member.Key, out _peer) && _peer.mPlayer != null)
                    _peer.mPlayer.AddToChatMessageQueue(chatMessage);
            }
        }


        #endregion
 
        #region Methods

        #region QAClientPeers
        private static readonly HashSet<QAClientPeer> qapeers = new HashSet<QAClientPeer>();
        public static void AddQAPeer(QAClientPeer qapeer)
        {
            lock (qapeers)
            {
                qapeers.Add(qapeer);
            }
        }
        public static bool RemoveQAPeer(QAClientPeer qapeer)
        {
            lock (qapeers)
            {
                return qapeers.Remove(qapeer);
            }
        }
        #endregion

        #region Setup
        protected override void Setup()
        {
            Instance = this;
            this.InitLogging();
            InitCounters();
            GameUtils.IsServer = true;
            ZRPC = new ZRPC();

            PathManager.InitNavData();
            PathManager.StartUpdate();

            // Initialize the levels 
            ReadGameDB();
            LevelReader.InitServer(AssemblyDirectory);

            dbRepository = new DBRepository();
            this.PublicIpAddress = PublicIPAddressReader.ParsePublicIpAddress(GameServerSettings.Default.PublicIPAddress);
            bool connectDB = dbRepository.Initialize(GameServerSettings.Default.ConnectionString);
            if (!connectDB)
            {
                log.InfoFormat("DB connection failed, Killed Process, try restart server later.");
                Process.GetCurrentProcess().Kill();
            }
            InitGMDB(); // Need to init before fiber start to prevent registerserver error
            Zealot.Logging.Client.LoggingAgent.Instance.Init(GetMyServerId().ToString());

            GameConfig.Init();

            this.executionFiber = new PoolFiber();
            this.executionFiber.Start();

            log.InfoFormat("Setup: serverId={0}", ServerId);

            Protocol.AllowRawCustomValues = true;
            this.SetupFeedbackControlSystem();
            ClusterServerConnection = new ClusterServerConnection(this, GameServerSettings.Default.ClusterIPAddress, GameServerSettings.Default.OutgoingClusterServerPeerPort, GameServerSettings.Default.ConnectReytryInterval, "GameServer");
            ClusterServerConnection.Initialize();

            GameToMasterConnection = new GameToMasterConnection(this, GameServerSettings.Default.MasterIPAddress, GameServerSettings.Default.OutgoingMasterServerPeerPort, GameServerSettings.Default.ConnectReytryInterval, "GameServer");
            GameToMasterConnection.Initialize();

            BossRules.Init();

            CreateRoomsOnStartup();

            if (GameServerSettings.Default.AppStatsPublishInterval > 0)
            {
                this.AppStatsPublisher = new ApplicationStatsPublisher(this, GameServerSettings.Default.AppStatsPublishInterval);
            }
            //CounterPublisher.DefaultInstance.AddStaticCounterClass(typeof(Lite.Diagnostics.Counter), this.ApplicationName);
            //CounterPublisher.DefaultInstance.AddStaticCounterClass(typeof(LoadShedding.Diagnostics.Counter), this.ApplicationName);

            // Game Rules           
            TongbaoCostBuff.Init();
            SystemSwitch.Init();
            LadderRules.Init();
            RealmRules.Init();
            GMActivityRules.Init();
            NewServerEventRules.Init();
            GuildRules.Init();
            //SevenDaysRules.Init();
            WelfareRules.Init();
            TickerTapeSystem.Init();
            CurrencyExchangeRules.Init();
            PartyRules.Init();
            QuestRules.Init();
            LootRules.Init();

            OnStartupNewDay();
            SetNewDayTimer();
            SetActivityTimer();
            SetGuildBossTimer();
            SystemMessageRules.Init();
            Task auctionInit = AuctionRules.Init();
            Task itemMallInit = ItemMall.ItemMallManager.Instance.ServerInit_ItemMall();
            itemMallInit.Wait();
            Lottery.LotteryManager.Instance.Init();
            ReviveItemController = new ReviveItemController();

            //Leaderboard = new LeaderboardController();
            //Leaderboard.Init();

            log.InfoFormat("Completed GameApplication Setup. Scheduling CheckGames...");

            this.executionFiber.Schedule(this.CheckGames, 50);
            this.executionFiber.ScheduleOnInterval(UpdateServerReport, 600000, 600000);
            if (MyServerConfig.IsGameServer())
            {
                this.executionFiber.ScheduleOnInterval(GameRules.SaveToDB, 0, 60000);
                this.executionFiber.Schedule(GuildRules.Update, GuildRules.UpdateInterval);
                this.executionFiber.Schedule(GuildRules.OnSaveGuildInterval, GuildRules.saveDBInterval);
                this.executionFiber.Schedule(RealmRules.Update, RealmRules.UpdateInterval);
                this.executionFiber.ScheduleOnInterval(TongbaoCostBuff.Update, 0, TongbaoCostBuff.UpdateInterval);
                this.executionFiber.Schedule(SystemSwitch.Update, SystemSwitch.UpdateInterval);
                this.executionFiber.ScheduleOnInterval(PartyRules.Update, 0, PartyRules.UpdateInterval);
            }

            mLastCPUMonitorDT = DateTime.Now;
            mLastCPUTime = Process.GetCurrentProcess().TotalProcessorTime;
            //test disconnect, chensheng
            //this.executionFiber.Schedule(() =>
            //{
            //    if (clusterPeer != null && clusterPeer.IsRegistered)
            //        clusterPeer.Disconnect();
            //}, 15000);
        }

        public int GetMyServerId()
        {
            return MyServerConfig.id;
        }

        public int GetMyServerline()
        {
            return MyServerConfig.serverline;
        }

        private void InitGMDB()
        {
            dbGM = new DBRepositoryGM();
            string ipaddr = PublicIpAddress.ToString().Trim();
            int port = (int)GamingUdpPort;
            log.InfoFormat("InitGMDB: {0}:{1}", ipaddr, port);
            bool connectDB = dbGM.Initialize(GameServerSettings.Default.GMConnectionString);
            if (!connectDB)
            {
                log.InfoFormat("DB connection failed, Killed Process, try restart server later.");
                Process.GetCurrentProcess().Kill();
            }
            List<Dictionary<string, object>> configs = dbGM.ServerConfig.GetConfig(ipaddr, port);
            if (configs.Count == 0)
            {
                log.InfoFormat("Server info with ipaddr {0}:{1} not exists in GMDB ServerConfig table, Killed Process, fix the problem and try restart server again.");
                Process.GetCurrentProcess().Kill();
            }
            // there should be one row for your server
            var myconfig = configs[0];
            string public_ipaddr = string.Format("{0}:{1}", (string)myconfig["publichost"], (int)myconfig["port"]);
            MyServerConfig = new ServerConfig((int)myconfig["id"], public_ipaddr, (string)myconfig["servername"],
                (byte)myconfig["servertype"], (int)myconfig["maxplayer"], (int)myconfig["serverline"], (string)myconfig["voicechat"]);
            MyServerConfig.serializeString = MyServerConfig.Serialize();
            ServerId = MyServerConfig.id;
            dbRepository.mServerId = MyServerConfig.id;
            dbRepository.mServerLine = MyServerConfig.serverline;
            mServerStatus = new GMServerStatus(MyServerConfig.id);
        }

        private void ReadGameDB()
        {
            string path = Path.Combine(AssemblyDirectory, "../Repository/gamedata.json");
            using (StreamReader sr = new StreamReader(path))
            {
                string contents = sr.ReadToEnd();
                GameRepo.SetItemFactory(new ServerItemFactory());
                GameRepo.InitServer(contents);
            }
        }

        private void CreateRoomsOnStartup()
        {
            CreateDefaultRoom("lobby"); // This should be the only default room

            foreach (KeyValuePair<string, RealmWorldJson> entry in RealmRepo.mRealmWorldByName)
            {
                string sceneName = entry.Key;
                if (LevelReader.levels.ContainsKey(sceneName))
                    CreateDefaultRealm(entry.Value.id, sceneName, true);
            }
        }

        protected void CreateDefaultRoom(string roomname)
        {
            log.InfoFormat("CreateDefaultRoom: serverId={0}:{1}", ServerId, roomname);
            Room room;
            RoomReference roomRef;
            object[] args = new object[1];
            args[0] = "default";
            GameCache.Application = instance;
            GameCache.TryCreateDefaultRoom(roomname, null, out room, out roomRef, args);
            Game defaultroom = room as Game;
            defaultroom.EmptyRoomLiveTime = -1; //to indicate the default room should not be removed regardless of peers inside.
            defaultroom.InitController(roomname);
        }

        protected void CreateDefaultRealm(int realmId, string levelName, bool isWorld = false)
        {
            log.InfoFormat("CreateDefaultRealm: serverId={0}:{1}", ServerId, levelName);
            Room room;
            RoomReference roomRef;
            GameCache.TryCreateRealmRoom(realmId, levelName, isWorld, null, out room, out roomRef);
            Game defaultroom = room as Game;
            defaultroom.EmptyRoomLiveTime = -1; //to indicate the default room should not be removed regardless of peers inside.
            defaultroom.InitController(levelName);
        }
        #endregion

        /// <summary>
        ///   Sanity check to verify that game states are cleaned up correctly
        /// </summary>
        protected virtual void CheckGames()
        {
            try
            {
                Profiler checkGamesProfiler = new Profiler();
                checkGamesProfiler.Start();

                //performance counters reset
                GameCounters.TotalEntSysDuration.RawValue = 0;
                GameCounters.TotalNetSlotUpdateDuration.RawValue = 0;
                GameCounters.TotalNetUpdateRelevantObj.RawValue = 0;
                GameCounters.TotalNetUpdateSnapShot.RawValue = 0;
                GameCounters.TotalNetSyncRelevantStats.RawValue = 0;
                GameCounters.TotalNetDispatchChat.RawValue = 0;
                GameCounters.TotalDamageResultsUpdate.RawValue = 0;
                GameCounters.TotalUpdateEntitySyncStats.RawValue = 0;
                GameCounters.TotalSnapShotPrepareTime.RawValue = 0;
                GameCounters.TotalSnapShotSendTime.RawValue = 0;
                //GameCounters.TotalRealmControllerUpdates.RawValue = 0;
                //GameCounters.TotalTimerUpdates.RawValue = 0;
                GameCounters.TotalResetSyncStats.RawValue = 0;

                var roomInstances = GameApplication.Instance.GameCache.RoomInstances;
                List<string> roomKeys = roomInstances.Keys.ToList();
                for (int index = 0; index < roomKeys.Count; index++)
                {
                    string roomKey = roomKeys[index];
                    RoomCacheBase.RoomInstance roominstance;
                    if (roomInstances.TryGetValue(roomKey, out roominstance) == false)
                        continue;

                    Game room = (Game)roominstance.Room;//(Game)roomInstances[roomKey].Room;
                    if (room.controller != null)
                        room.controller.MainLoop();

                    GameCounters.TotalEntSysDuration.RawValue += GameCounters.ShuangFeiEntSysDuration.RawValue;
                    GameCounters.TotalNetSlotUpdateDuration.RawValue += GameCounters.ShuangFeiNetSlotUpdateDuration.RawValue;
                    GameCounters.TotalNetUpdateRelevantObj.RawValue += GameCounters.ShuangFeiNetUpdateRelevantObj.RawValue;
                    GameCounters.TotalNetUpdateSnapShot.RawValue += GameCounters.ShuangFeiNetUpdateSnapShot.RawValue;
                    GameCounters.TotalNetSyncRelevantStats.RawValue += GameCounters.ShuangFeiNetSyncRelevantStats.RawValue;
                    GameCounters.TotalNetDispatchChat.RawValue += GameCounters.ShuangFeiNetDispatchChat.RawValue;
                    GameCounters.TotalDamageResultsUpdate.RawValue += GameCounters.ShuangFeiDamageResultsUpdate.RawValue;
                    GameCounters.TotalUpdateEntitySyncStats.RawValue += GameCounters.ShuangFeiUpdateEntitySyncStats.RawValue;
                    GameCounters.TotalSnapShotPrepareTime.RawValue += GameCounters.ShuangFeiSnapShotPrepareTime.RawValue;
                    GameCounters.TotalSnapShotSendTime.RawValue += GameCounters.ShuangFeiSnapShotSendTime.RawValue;
                    //GameCounters.TotalRealmControllerUpdates.RawValue += GameCounters.PerRoomRealmConUpdateUpdateDuration.RawValue;
                }

                //GameCounters.TotalMainLoopsTime.RawValue = (long)(mainLoopsProfiler.StopAndGetElapsed() * 1000);      
                GameCounters.TotalEntSysDuration.RawValue /= 1000;
                GameCounters.TotalNetSlotUpdateDuration.RawValue /= 1000;
                GameCounters.TotalNetUpdateRelevantObj.RawValue /= 1000;
                GameCounters.TotalNetUpdateSnapShot.RawValue /= 1000;
                GameCounters.TotalNetSyncRelevantStats.RawValue /= 1000;
                //GameCounters.TotalNetDispatchChat.RawValue /= 1000; //in microsec
                //GameCounters.TotalDamageResultsUpdate.RawValue /= 1000; //in microsec
                //GameCounters.TotalUpdateEntitySyncStats.RawValue /= 1000; //in microsec
                //GameCounters.TotalSnapShotPrepareTime.RawValue /= 1000; //in microsec
                //GameCounters.TotalSnapShotSendTime.RawValue /= 1000; //in microsec
                //GameCounters.TotalRealmControllerUpdates.RawValue /= 1000; //in microsec
                //GameCounters.TotalTimerUpdates.RawValue /= 1000; //in microsec
                //GameCounters.TotalResetSyncStats.RawValue /= 1000; //in microsec                

                for (int index = 0; index < mRoomsToBeRemoved.Count; index++)
                    mRoomsToBeRemoved[index].TryRemoveRoomFromCache();
                mRoomsToBeRemoved.Clear();

                ReviveItemController.Update();

                int profiledTime = (int)(checkGamesProfiler.StopAndGetElapsed() * 1000); //mili seconds

                mServerStatus.roomupdatedur = profiledTime;
                GameCounters.CheckGamesDuration.RawValue = profiledTime; //dashboard counter, this should take less than 50msec, more than that and the server will feel lag
                GameCounters.ProcessMemory.RawValue = Process.GetCurrentProcess().WorkingSet64;

                //if (PerfmonCounters.Enabled) //perfmon counter
                //    PerfmonCounters.Counters["checkGamesDuration"].RawValue = profiledTime; //Example of how to set counter for Windows performance monitor
            }
            catch (Exception ex)
            {
                Exception error = ex;
                while (error.InnerException != null)
                    error = error.InnerException;
                log.ErrorFormat("CheckGames {0} - {1}", error.GetType(), error.StackTrace); //we need to catch here, so that CheckGames can be rescheduled
            }
            this.executionFiber.Schedule(this.CheckGames, 50);
            //We schedule next CheckGames 50msec later, so there is a fixed time to process rpc requests (dequeue from executionfiber)
            //This also means that we update the next instance 50msec after the last instance is updated (Each tick is no longer fixed interval)
        }

        public List<Room> mRoomsToBeRemoved;
        public void AddRoomToBeRemoved(Room room)
        {
            mRoomsToBeRemoved.Add(room);
        }

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("CreatePeer for {0}", initRequest.ApplicationId);
            }
            // Game server latency monitor connects to self
            if (initRequest.ApplicationId == "LatencyMonitor")
            {
                //if (log.IsDebugEnabled)
                //{
                //    log.DebugFormat(
                //        "incoming latency peer at {0}:{1} from {2}:{3}, serverId={4}",
                //        initRequest.LocalIP,
                //        initRequest.LocalPort,
                //        initRequest.RemoteIP,
                //        initRequest.RemotePort,
                //        ServerId);
                //}
                //return new LatencyPeer(this, initRequest.Protocol, initRequest.PhotonPeer);

                return null;
            }
            else if (initRequest.ApplicationId == "QATools")
            {
                if (log.IsDebugEnabled)
                {
                    log.InfoFormat(
                        "incoming qatools peer at {0}:{1} from {2}:{3}, serverId={4}",
                        initRequest.LocalIP,
                        initRequest.LocalPort,
                        initRequest.RemoteIP,
                        initRequest.RemotePort,
                        ServerId);
                }

                var peer = new QAClientPeer(initRequest, this);
                AddQAPeer(peer);
                return peer;
            }

            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    "incoming game peer at {0}:{1} from {2}:{3}",
                    initRequest.LocalIP,
                    initRequest.LocalPort,
                    initRequest.RemoteIP,
                    initRequest.RemotePort);
            }
            return new GameClientPeer(initRequest, this);
        }

        private void UpdateServerReport()
        {
            int ccu = GetOnlineUserCount();
            if (ccu > mPCU)
                mPCU = ccu;
            if (masterPeer != null && masterPeer.IsRegistered)
            {
                TimeSpan newCPUTime = Process.GetCurrentProcess().TotalProcessorTime;
                DateTime now = DateTime.Now;
                int cpuusage = (int)(100 * (newCPUTime - mLastCPUTime).TotalMilliseconds / (now - mLastCPUMonitorDT).TotalMilliseconds);
                if (cpuusage > 100)
                    cpuusage = 100;
                mServerStatus.cpuusage = (byte)cpuusage;
                mLastCPUMonitorDT = now;
                mLastCPUTime = newCPUTime;
                masterPeer.ZRPC.GameToMasterRPC.RegularServerStatusUpdate(mServerStatus.roomupdatedur, mServerStatus.cpuusage, masterPeer);
            }
        }

        private void AddCookie(string userid, string cookie)
        {
            userCookies[userid] = cookie;
        }

        public string GetCookie(string userid)
        {
            string cookie;
            if (userCookies.TryGetValue(userid, out cookie))
                return cookie;
            return "";
        }

        private void RemoveCookie(string userid)
        {
            userCookies.Remove(userid);
        }

        public GameClientPeer GetUserPeerByUserid(string userid)
        {
            GameClientPeer peer;
            if (peerByUserId.TryGetValue(userid, out peer))
                return peer;
            return null;
        }

        public void AddUserPeer(string userid, GameClientPeer peer)
        {
            peerByUserId[userid] = peer;
            if (masterPeer != null && masterPeer.IsRegistered)
                masterPeer.ZRPC.GameToMasterRPC.RegUser(userid, masterPeer);      
        }

        public void RemoveUserPeer(string userid, GameClientPeer peer)
        {
            GameClientPeer p;
            if (peerByUserId.TryGetValue(userid, out p) && p == peer)
            {
                peerByUserId.Remove(userid);
                if (masterPeer != null && masterPeer.IsRegistered)
                    masterPeer.ZRPC.GameToMasterRPC.UnRegUser(userid, masterPeer);
            }
        }

        public string[] GetOnlineUserIds()
        {
            return peerByUserId.Keys.ToArray();
        }

        public int GetOnlineUserCount()
        {
            return peerByUserId.Count;
        }

        public int GetOnlinePlayerCount()
        {
            return mPeerByChar.Count;
        }

        public bool IsConnectedToCluster()
        {
            return clusterPeer != null && clusterPeer.IsRegistered;
        }

        public bool IsConnectedToMaster()
        {
            return masterPeer != null && masterPeer.IsRegistered;
        }
        #endregion

        #region PeerByChar
        public void AddCharPeer(string charName, GameClientPeer peer)
        {
            mPeerByChar[charName] = peer;
            CreateCharDatablock(charName, peer.CharacterData.ProgressLevel, Instance.MyServerConfig.servername, null, false);
            if (clusterPeer != null && clusterPeer.IsRegistered)
                clusterPeer.ZRPC.GameToClusterRPC.RegChar(charName, peer.mUserId, peer.CharacterData.ProgressLevel, clusterPeer);
        }
        public void RemoveCharPeer(string charName, GameClientPeer peer)
        {
            if (mPeerByChar.Remove(charName))
            {
                if (clusterPeer != null && clusterPeer.IsRegistered)
                    clusterPeer.ZRPC.GameToClusterRPC.UnRegChar(charName, peer.mUserId, clusterPeer);
            }
            DeleteCharDatablock(charName, true, false);             
        }
        public GameClientPeer GetCharPeer(string charName)
        {
            GameClientPeer peer;
            if (mPeerByChar.TryGetValue(charName, out peer))
                return peer;
            return null;
        }

        public string GetSerializedOnlineRegCharInfos()
        {
            Dictionary<string, RegCharInfo> ret = new Dictionary<string, RegCharInfo>();
            GameClientPeer peer;
            foreach (var kvp in mPeerByChar)
            {
                peer = kvp.Value;
                ret.Add(kvp.Key, new RegCharInfo { userid = peer.mUserId, lvl = peer.GetProgressLvl() });
            }
            return JsonConvertDefaultSetting.SerializeObject(ret);
        }

        public List<GameClientPeer> GetCharPeerList()
        {
            return mPeerByChar.Values.ToList();
        }

        public Dictionary<string, GameClientPeer> GetAllCharPeer()
        {
            return mPeerByChar;
        }

        public Dictionary<string, GameClientPeer> GetCharPeerDictCopy()
        {
            return new Dictionary<string, GameClientPeer>(mPeerByChar);
        }

        public void SendMessageToAllCharPeers(EventData eventData)
        {
            ApplicationBase.Instance.BroadCastEvent(eventData, mPeerByChar.Values.ToList(), new SendParameters());
        }

        public void TongbaoCostBuffSendToAllCharPeers()
        {
            if (TongbaoCostBuff.CostBuffData.IsDirty())
            {
                Dictionary<byte, object> packet = TongbaoCostBuff.CostBuffData.Serialize((byte)LOCATEGORY.SharedStats, -1, false);
                var eventData = GameRules.GetLocalObjEventData(packet);
                var sendPara = GameRules.GetSendParam(true);
                lock (mPeerByChar)
                {
                    foreach (KeyValuePair<string, GameClientPeer> entry in mPeerByChar)
                    {
                        GameClientPeer peer = entry.Value;
                        if (peer != null && peer.mPlayer != null && !peer.mPlayer.Destroyed)
                            peer.SendEvent(eventData, sendPara);
                    }
                    TongbaoCostBuff.CostBuffData.Reset();
                }
            }
        }
        #endregion

        #region Server New Day Timer
        private void OnStartupNewDay()
        {
            bool isNewDay = true;
            Dictionary<string, object> result = dbRepository.Progress.GetProgressByKey(GetMyServerId(), "ServerNewDay").Result;
            if (result.Count > 0)
            {
                long dtSvrNewDayTicks = Convert.ToInt64(result["value"]);
                if (dtSvrNewDayTicks != 0 && dtSvrNewDayTicks == DateTime.Today.Ticks)
                    isNewDay = false;
            }
            if (isNewDay)
                OnGameServerNewDay();
        }

        private void SetNewDayTimer()
        {
            //reset on 00:00.
            long durationUntilMidnight = (long)(DateTime.Now.AddDays(1).Date - DateTime.Now).TotalMilliseconds + 3000; // Delay 3 seconds
            this.executionFiber.ScheduleOnInterval(OnNewDayTimerUp, durationUntilMidnight, 86400000);
        }

        private void OnNewDayTimerUp()
        {
            DateTime now = DateTime.Now;
            DateTime day = DateTime.Today;
            if ((now - day).Hours > 0) //schedule run too fast, reschedule, something like 23:59:59
            {
                long durationUntilMidnight = (long)(day.AddDays(1) - now).TotalMilliseconds + 3000;
                this.executionFiber.Schedule(OnNewDayTimerUp, durationUntilMidnight);
                    return;
            }

            if (log.IsInfoEnabled)
                log.InfoFormat("NewDay");

            RPCBroadcastData rpcdata = ZRPC.CombatRPC.GetSerializedRPC(ServerCombatRPCMethods.BroadcastMessageToClient, (byte)BroadcastMessageType.NewDay, "");
            NewDayBroadcastMessage roommessage = new NewDayBroadcastMessage(rpcdata);
            BroadcastRoomMessage(roommessage);

            OnGameServerNewDay();
            LogPCU();
            mPCU = 0;
        }

        private void LogPCU()
        {
            if (mPCU == 0)
                return;
            string message = string.Format(@"pcu: {0}", mPCU);
            Zealot.Logging.Client.LogClasses.LogPCU pcuLog = new Zealot.Logging.Client.LogClasses.LogPCU();
            pcuLog.userId = "";
            pcuLog.charId = "";
            pcuLog.message = message;
            pcuLog.pcu = mPCU;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(pcuLog);
        }

        public void OnGameServerNewDay()
        {
            var result = dbRepository.Progress.Update(GetMyServerId(), "ServerNewDay", DateTime.Today.Ticks);
            GuildRules.OnNewDay();
            Lottery.LotteryManager.Instance.OnNewDay();
            DonateRules.IsResetToDay = false;
            //SevenDaysRules.OnNewDay();
            WelfareRules.OnNewDay();
        }
        #endregion

        #region Activity Timer
        public long mServerStartUpTicks { get; set; }
        private void SetActivityTimer()
        {
            DateTime now = DateTime.Now;
            //if (RealmRepo.mActivityWorldBoss != null)
            //{
            //    bool foundNext = false;
            //    long timetoNextOpen = GameUtils.TimeToNextEventDailyFormat(now, RealmRepo.mActivityWorldBoss.opendaily, out foundNext, 5000);
            //    if (foundNext)
            //        this.executionFiber.Schedule(() => OnActivityStart(NotificationType.WorldBoss), timetoNextOpen);
            //}
        }

        public void OnActivityStart(NotificationType type)
        {
            DateTime now = DateTime.Now;
            switch (type)
            {
                //case NotificationType.WorldBoss:
                //    CreateDefaultRealm(RealmRepo.mActivityWorldBoss.id, LevelRepo.GetInfoById(RealmRepo.mActivityWorldBoss.level).unityscene);
                //    GameUtils.mActivityStatus = GameUtils.SetBit(GameUtils.mActivityStatus, (int)ActivityStatusBitIndex.WorldBoss);
                //    bool foundNext = false;
                //    long timetoNextOpen = GameUtils.TimeToNextEventWeeklyFormat(now, RealmRepo.mActivityWorldBoss.opendaily, out foundNext, 5000);
                //    if (foundNext)
                //        this.executionFiber.Schedule(() => OnActivityStart(NotificationType.WorldBoss), timetoNextOpen);
                //    break;
            }
            BroadcastActivityStart(type);
        }

        public static void BroadcastActivityStart(NotificationType type)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat("Activity {0} start", type);
            byte type_byte = (byte)type;
            Instance.BroadcastMessage(BroadcastMessageType.NotifyActivityStart, type_byte.ToString());
        }

        //public static void BroadcastActivityStart(ActivityStatusBitIndex type)
        //{
        //    if (log.IsDebugEnabled)
        //        log.DebugFormat("ActivityStatus {0} start", type);
        //    byte type_byte = (byte)type;
        //    Instance.BroadcastMessage(BroadcastMessageType.StatusActivityStart, type_byte.ToString());
        //}

        public static void BroadcastActivityEnd(ActivityStatusBitIndex type)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat("ActivityStatus {0} end", type);
            byte type_byte = (byte)type;
            Instance.BroadcastMessage(BroadcastMessageType.StatusActivityEnd, type_byte.ToString());
        }
        #endregion

        #region Guild Boss Timer
        private void SetGuildBossTimer()
        {
            DateTime now = DateTime.Today;
            int diffFrmMonday = (int)DayOfWeek.Monday - (int)now.DayOfWeek;
            diffFrmMonday = (diffFrmMonday < 0) ? -diffFrmMonday : diffFrmMonday; // Abs
            DateTime nextBossResetDT = new DateTime(now.Year, now.Month, now.Day);
            nextBossResetDT = nextBossResetDT.AddDays(7 - diffFrmMonday); // Next boss reset datetime
            long elapsedTimeToBossReset = (long)nextBossResetDT.Subtract(now).TotalMilliseconds + 1000;
            this.executionFiber.Schedule(OnGuildBossTimerUp, elapsedTimeToBossReset);
        }

        private void OnGuildBossTimerUp()
        {
            foreach (var entry in mPeerByChar.Values)
                entry.EnqueueNewGuildWeek();
            GuildRules.ResetAllGuildBossInfo();
            this.executionFiber.Schedule(OnGuildBossTimerUp, 604800000);
        }
        #endregion

        #region Item Mall
        //public void ItemMall_OnCheckLimitedItem()
        //{
        //    //Check if i should switch on the UI 
        //    ItemMall.ItemMallManager.Instance.isLimitedItemUIOnOff = ItemMall.ItemMallManager.Instance.LimitedItem_GetSalesOnOff();

        //    BroadcastItemMall_NewUpdateOnDateTime(ItemMall.ItemMallManager.Instance.isLimitedItemUIOnOff);

        //    ItemMall.ItemMallManager.Instance.LimitedItem_PopNextTiming();
        //    long timeToNext = ItemMall.ItemMallManager.Instance.LimitedItem_TimeToNext(DateTime.Now);
        //    ItemMall_LimitedItemSaleTimer = this.executionFiber.Schedule(() => ItemMall_OnCheckLimitedItem(), timeToNext);
        //}

        //public void BroadcastItemMall_NewUpdateOnDateTime(bool isOn)
        //{
        //    var rpcdata = ZRPC.NonCombatRPC.GetSerializedRPC(ServerNonCombatRPCMethods.ItemMall_Broadcast_LimitedItemSaleOnOff, isOn);
        //    var roommessage = new RoomBroadcastMessage(rpcdata);
        //    BroadcastRoomMessage(roommessage);
        //}
        #endregion

        #region CharOnlineData
        public void CreateCharDatablock(string charName, int level, string server, DateTime? logoutdt, bool affectRefCount)
        {
            CharacterOnlineData char_online_data; 
            if (mCharOnlineMap.TryGetValue(charName, out char_online_data))
            {
                char_online_data.level = level;
                char_online_data.server = server;
                char_online_data.logoutdt = logoutdt;
            }
            else
            {
                char_online_data = new CharacterOnlineData() {level = level, server = server, logoutdt = logoutdt };
                mCharOnlineMap.Add(charName, char_online_data);
            }
            if (affectRefCount)
                char_online_data.refcount++;
        }

        public void UpdateCharDatablock(string charName, int level, string server, DateTime? logoutdt)
        {
            CharacterOnlineData char_online_data;
            if (mCharOnlineMap.TryGetValue(charName, out char_online_data))
            {
                char_online_data.level = level;
                char_online_data.server = server;
                char_online_data.logoutdt = logoutdt;
            }
        }

        public void DeleteCharDatablock(string charName, bool logout, bool affectRefCount)
        {
            CharacterOnlineData char_online_data;
            if (mCharOnlineMap.TryGetValue(charName, out char_online_data))
            {
                if (affectRefCount)
                    char_online_data.refcount--;
                if (char_online_data.refcount == 0)
                    mCharOnlineMap.Remove(charName);
                else if (logout)
                {
                    char_online_data.server = "";
                    char_online_data.logoutdt = DateTime.Now;
                }
            }              
        }

        public CharacterOnlineData GetCharDatablock(string charName)
        {
            CharacterOnlineData char_online_data;
            if (mCharOnlineMap.TryGetValue(charName, out char_online_data))
                return char_online_data;
            return null;
        }
        #endregion

        #region Transfer Server
        public bool TransferServer(int serverid, GameClientPeer peer)
        {
            string userid = peer.mUserId;
            string cookie = GetCookie(userid);
            if (!string.IsNullOrEmpty(cookie) && masterPeer != null && masterPeer.IsRegistered)
            {
                peer.mTransferServerInfo = new TransferServerInfo(peer.mChar, serverid);
                masterPeer.ZRPC.GameToMasterRPC.TransferServer(userid, cookie, serverid, masterPeer);
                return true;
            }
            return false;
        }
        #endregion
    }
}
