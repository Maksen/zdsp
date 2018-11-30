namespace Photon.LoadBalancing.GameServer
{
    using ExitGames.Logging;
    using ExitGames.Concurrency.Fibers;
    using Hive;

    using Kopio.JsonContracts;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Common.RPC;
    using Zealot.Server.Actions;
    using Zealot.Server.Entities;
    using Zealot.Server.Counters;
    using Zealot.Server.Rules;
    using Zealot.RPC;
    using Zealot.Entities;
    using Zealot.Repository;

    public static class GameLogicIDPool
    {
        private static IDPool idpool;

        public static int AllocID()
        {
            if (idpool == null)
                idpool = new IDPool();

            return idpool.AllocID(0, false);
        }

        public static void FreeID(int id)
        {
            idpool.FreeID(id, 0);
        }
    }

    public partial class GameLogic
    {
        public const int MAX_ROAMS_PER_FRAME = 10; //Control the maximum monster roams allowable per frame
        public const int MAX_GOBACKSAFE_PER_FRAME = 20; //Control the maximum monster goback allowable per frame

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private Dictionary<HivePeer, NetServerSlot> Connections;
        private PoolFiber executionFiber;
        public ServerEntitySystem mEntitySystem;
        protected Timers mTimers;
        private List<Vector3> mPlayerSpawnPt;
        private List<Vector3> mPlayerSpawnDir;
        public Dictionary<int, IServerEntity> mObjectMap;
        public List<MonsterSpawnerBase> maMonsterSpawners;
        public RealmController mRealmController;
        public Game mRoom;
        public int mCurrentLevelId = 0;
        public string mCurrentLevelName;
        public bool mIsWorld = false;
        public bool mIsCity = false;
        public RealmPVPType mRealmPVPType = RealmPVPType.Peace;

        private int mCurrRoamsInFrame;
        private int mCurrGoBacksInFrames;
        private int mCurrRoamNo;
        private int mIncrementCurrRoam;
        public ZRPC ZRPC;

        private Dictionary<RPCCategory, Dictionary<byte, Action<object[]>>> RPCCatMethodIDToDelegate;

        private Zealot.Server.Counters.Profiler mainloopProfiler;

        private static MethodInfo[] RPCMethods;
        private static RPCMethodProxyAttribute[] MethodRPCProxyAttribs;

        public static void InitRPCProxyAttribs()
        {
            RPCMethods = typeof(GameLogic).GetMethods();
            int length = RPCMethods.Length;
            MethodRPCProxyAttribs = new RPCMethodProxyAttribute[length];
            for (int i = 0; i < length; ++i)
            {
                MethodRPCProxyAttribs[i] = (RPCMethodProxyAttribute)RPCMethods[i].GetCustomAttribute(typeof(RPCMethodProxyAttribute)); //This is very expensive. Avoid after setup.
            }
        }

        public int ID { private set; get; }

        public GameLogic(Game roomReference)
        {
            mRoom = roomReference;
            ID = GameLogicIDPool.AllocID();
            log.InfoFormat("realmadd [{0}], {1}", ID, mRoom.Guid);

            mTimers = new Timers();
            mTimers.SetLogHandler(ErrHandler);
            mEntitySystem = new ServerEntitySystem(mTimers, ID);
            mObjectMap = new Dictionary<int, IServerEntity>();
            maMonsterSpawners = new List<MonsterSpawnerBase>();
            ZRPC = new ZRPC();
            mCurrRoamNo = 0;
            SetupRPCCalleeProxies(); //TODO: if setup rpc proxy becomes expensive, consider doing it only for world instances only
            GameCounters.TotalRealms.Increment();

            mainloopProfiler = new Zealot.Server.Counters.Profiler();
        }

        public void ErrHandler(string errmsg)
        {
            log.Error(errmsg);
        }

        ~GameLogic()
        {
            GameLogicIDPool.FreeID(ID);
            log.InfoFormat("realmrmv [{0}]", ID);
            GameCounters.TotalRealms.Decrement();
        }

        public void Startup(string levelName)
        {
            executionFiber = mRoom.ExecutionFiber;

            Connections = new Dictionary<HivePeer, NetServerSlot>();
            mTimers.PrepareTickForNetworkEvents();
            //Start from 1 so that level initialization that creates entities(monsters), 
            //will have lastactionchanged set from 1 onwards and their actions will be broadcast in the first mainloop

            mCurrentLevelName = levelName;
            mIsWorld = RealmRepo.IsWorld(levelName);
            mIsCity = RealmRepo.IsCity(levelName);
            if (levelName != "lobby")
            {                
                InitLevel(levelName);
                LevelJson info = LevelRepo.GetInfoByName(levelName);
                if (info != null)
                    mCurrentLevelId = info.id;
                else
                    log.WarnFormat("Cannot find such level {0} in LevelRepo", levelName);
            }

            //executionFiber.ScheduleOnInterval(delegate()
            //{
            //    MainLoop();
            //}, 0, 50); //Each game room is processed every 50msec
        }

        public void Dispose(bool disposing)
        {
            //UnsubscribeRoomChannels();
            mRoom = null;
        }

        //The first parameter is the sender
        public void BroadcastEvent(IServerEntity sender, string eventName, object[] parameters = null)
        {
            Dictionary<string, List<EntityLinkServer>> event_Links_dict = ((ServerEntityWithEventJson)sender.GetPropertyInfos()).EntityLinks_Server;
            if (event_Links_dict.ContainsKey(eventName))
            {
                List<EntityLinkServer> entityLinks = event_Links_dict[eventName];
                foreach (EntityLinkServer link in entityLinks)
                {
                    int receiverID = link.mReceiver;
                    string trigger = link.mTrigger;
                    if (mObjectMap.ContainsKey(receiverID))
                    {
                        var receiver = mObjectMap[receiverID];
                        var methodInfo = receiver.GetType().GetMethod(trigger, new Type[] { typeof(IServerEntity), typeof(object[]) });
                        if (methodInfo != null)
                            methodInfo.Invoke(receiver, new object[] { sender, parameters });
                    }
                }
            }
        }

        public void RemovePlayer(GameClientPeer peer)
        {
            Player myPlayer = peer.mPlayer;
            int playerPID = myPlayer == null ? - 1 : myPlayer.GetPersistentID();
            log.InfoFormat("[{0}]: RemovePlayer {1} ({2}) {3}", ID, peer.ConnectionId, playerPID, mCurrentLevelName);

            // Schedule peer to be removed from PeerList on the main thread
            // because now player doesn't control any entity, so we use this
            if (myPlayer != null)
            {
                if (mRealmController != null)
                    mRealmController.OnPlayerExit(myPlayer);

                myPlayer.SaveToCharacterData(true);
                GameCounters.TotalPlayers.Decrement();
                mEntitySystem.RemoveEntityByPID(playerPID, true);

                List<IBaseNetEntity> neList = mEntitySystem.GetNetEntitiesByOwner(playerPID);
                int count = neList.Count;
                for (int i = 0; i < count; ++i)
                {
                    int pid = neList[i].GetPersistentID();
                    mEntitySystem.RemoveEntityByPID(pid);
                }
            }
            // Old Code
            //if (playerPID != -1)
            //{
            //    List<IBaseNetEntity> list = mEntitySystem.GetNetEntitiesByOwner(playerPID);
            //    foreach (IBaseNetEntity ne in list)
            //    {
            //        int pid = ne.GetPersistentID();
            //        if (((Entity)ne).IsPlayer())
            //        {
            //            Player player = (Player)ne;
            //            if (mRealmController != null)
            //                mRealmController.OnPlayerExit(player);
            //            player.SaveToCharacterData(true);
            //            GameCounters.TotalPlayers.Decrement();
            //        }
            //        mEntitySystem.RemoveEntityByPID(pid, true);
            //    }
            //}

            if (Connections.ContainsKey(peer))
            {
                NetServerSlot slot = Connections[peer];                
                slot.CleanUp();
                Connections.Remove(peer);
            }

            if (peer.mPlayer != null)
                peer.mPlayer = null;

            peer.OnPlayerRemovedFromRoom();

            //if (log.IsDebugEnabled)
            //    log.DebugFormat("Leave Room {0}", currentlevelname);
        }

        public List<Entity> GetSpawnedObjectsByPeer(GameClientPeer peer)
        {
            if (Connections.ContainsKey(peer))
                return Connections[peer].GetSpawnedObjects();
            return null;
        }

        public void MainLoop()
        {                        
            mTimers.PrepareTickForMainLoop();
            mTimers.Update();

            UpdateAICounters();                                       
            long dt = mTimers.GetDeltaTime();
            GameCounters.AverageRealmUpdateRate.IncrementBy(dt); //if this dt is much higher than 50msec, it implies that cpu is being held up                        

            //if (IsShuangFei) //Peter, test
            //{
                mainloopProfiler.Start();
                mEntitySystem.Update(dt);
                long profiledTime = (long)(mainloopProfiler.StopAndGetElapsed() * 1000000);
                //GameCounters.AvgEntitySysUpdateDuration.IncrementBy(profiledTime);
                GameCounters.ShuangFeiEntSysDuration.RawValue = profiledTime;
            //}
            //else
            //    mEntitySystem.Update(dt);
                                    
            if (mRealmController != null)
                mRealmController.Update(dt);
            
            //if (IsShuangFei) //Peter, test
            //{
                mainloopProfiler.Start();
                UpdateNetServerSlots();
                profiledTime = (long)(mainloopProfiler.StopAndGetElapsed() * 1000000);
                //GameCounters.AverageNetServerSlotUpdateDuration.IncrementBy(profiledTime);
                GameCounters.ShuangFeiNetSlotUpdateDuration.RawValue = profiledTime;
            //}
            //else
            //    UpdateNetServerSlots();
            //{
                mainloopProfiler.Start();
                mEntitySystem.ResetSyncStats();
                profiledTime = (long)(mainloopProfiler.StopAndGetElapsed() * 1000000);
                GameCounters.TotalResetSyncStats.RawValue += profiledTime;
            //}
            //GC.Collect(); //Test force gargage collect every loop to trace if tested objects are released.

            mTimers.PrepareTickForNetworkEvents();            
        }

        private void UpdateAICounters()
        {
            mCurrRoamsInFrame = 0;
            mIncrementCurrRoam++;
            if (mIncrementCurrRoam > 20)
            {
                mIncrementCurrRoam = 0;
                mCurrRoamNo++;  // increase 1 every 20 frames. 20 frames = 1 sec
                if (mCurrRoamNo >= 10) //10 because in 200 monsters, about 20 will have the same last digit and about 20 will move in 1 sec
                    mCurrRoamNo = 0;
            }
            mCurrGoBacksInFrames = 0;
        }

        private void UpdateNetServerSlots()
        {
            //Peter, test performance
            GameCounters.ProxyMethod.RawValue = 0;//reset for each frame            
            Zealot.Server.Counters.Profiler profiler = new Zealot.Server.Counters.Profiler();
            double updateRelevantObjTime = 0;
            double updateSnapShotTime = 0;
            double syncRelevantStatsTime = 0;
            double dispatchChatTime = 0;
            double entitySyncStatsTime = 0;
            double damageResultTime = 0;
            double snapShotActionPrepareTime = 0;
            double snapShotActionSendTime = 0;
            NetServerSlot slot = null;//for logging purpose

            try
            {            
                foreach (KeyValuePair<HivePeer, NetServerSlot> kvp in Connections)
                {
                    slot = kvp.Value;
                    if (slot.mPeer == null || slot.mPeer.mPlayer == null)
                        continue;
                    if (slot.mReleventQueryTicker == 0) //every 10 updates = 0.6s, check once
                    {
                        profiler.Start();
                        slot.UpdateRelevantObjects();
                        updateRelevantObjTime += profiler.StopAndGetElapsed();
                    }
                    if (slot.mUpdateTicker == 0) //every 2 updates = 0.12s check once.
                    { 
                        profiler.Start();
                        double currSnapShotActionPrepareTime, currsnapShotActionSendTime;
                        slot.UpdateSnapShotsAndActions(out currSnapShotActionPrepareTime, out currsnapShotActionSendTime);
                        updateSnapShotTime += profiler.StopAndGetElapsed();
                        snapShotActionPrepareTime += currSnapShotActionPrepareTime;
                        snapShotActionSendTime += currsnapShotActionSendTime;

                        profiler.Start();
                        slot.DispatchChatMessages();
                        dispatchChatTime += profiler.StopAndGetElapsed();
                    }

                    profiler.Start();
                    double currEntitySyncStatsTime, currDamageResultTime;
                    slot.SyncRelevantObjectStats(out currEntitySyncStatsTime, out currDamageResultTime);
                    syncRelevantStatsTime += profiler.StopAndGetElapsed();
                    entitySyncStatsTime += currEntitySyncStatsTime;
                    damageResultTime += currDamageResultTime;

                    slot.RefreshTicker();
                }
            }
            catch(Exception ex)
            {
                Exception error = ex;
                while (error.InnerException != null)
                    error = error.InnerException;
                
                if(slot != null && slot.mPeer != null)
                    log.ErrorFormat("UpdateNetServerSlots {0} - {1} - {2}", error.GetType(), error.StackTrace, slot.mPeer.mChar);
                else
                    log.ErrorFormat("UpdateNetServerSlots {0} - {1}", error.GetType(), error.StackTrace);
            }

            GameCounters.ShuangFeiNetUpdateRelevantObj.RawValue = (long)(updateRelevantObjTime * 1000000); //scaled up by 1000, microsec so as to keep precision when summing up
            GameCounters.ShuangFeiNetUpdateSnapShot.RawValue = (long)(updateSnapShotTime * 1000000);
            GameCounters.ShuangFeiNetSyncRelevantStats.RawValue = (long)(syncRelevantStatsTime * 1000000);
            GameCounters.ShuangFeiNetDispatchChat.RawValue = (long)(dispatchChatTime * 1000000);
            GameCounters.ShuangFeiUpdateEntitySyncStats.RawValue = (long)(entitySyncStatsTime * 1000000); //scaled up by 1000, microsec
            GameCounters.ShuangFeiDamageResultsUpdate.RawValue = (long)(damageResultTime * 1000000); //scale up by 1000
            GameCounters.ShuangFeiSnapShotPrepareTime.RawValue = (long)(snapShotActionPrepareTime * 1000000); //scale up by 1000
            GameCounters.ShuangFeiSnapShotSendTime.RawValue = (long)(snapShotActionSendTime * 1000000); //scale up by 1000
        }

        private void InitLevel(string levelname)
        {
            LevelInfo linfo = LevelReader.GetLevel(levelname);
            if (linfo == null)
            {
                log.WarnFormat("Cannot find such level {0}", levelname);
                //return; make it error
            }
            mEntitySystem.InitGrid(256, 256);
            //mEntitySystem.InitGrid(levelsizex, levelsizez, leveloriginx, leveloriginz, cellsizex, cellsizez);
            InitSpawners(linfo);
        }

        public bool LogAI { get; set; }
        public void InitSpawners(LevelInfo linfo)
        {
            mPlayerSpawnPt = new List<Vector3>();
            mPlayerSpawnDir = new List<Vector3>();
            foreach (KeyValuePair<string, Dictionary<int, ServerEntityJson>> entry in linfo.mEntities)
            {
                foreach (ServerEntityJson sp in entry.Value.Values)
                {
                    if (entry.Key == "PlayerSpawnerJson")
                    {
                        mPlayerSpawnPt.Add(sp.position);
                        mPlayerSpawnDir.Add(((PlayerSpawnerJson)sp).forward);
                        continue;
                    }
                    string className = sp.GetServerClassName();
                    if (className == "")
                        continue;

                    Type spType = Type.GetType("Zealot.Server.Entities." + className);
                    IServerEntity sp_handler = (IServerEntity)Activator.CreateInstance(spType, sp, this);
                    if (sp_handler is RealmController && !((RealmController)sp_handler).IsCorrectController())
                        continue;

                    mObjectMap.Add(sp.ObjectID, sp_handler);
                    if (sp_handler is MonsterSpawnerBase)
                    {
                        ((MonsterSpawnerBase)sp_handler).LogAI = LogAI;
                        maMonsterSpawners.Add((MonsterSpawnerBase)sp_handler);
                    }
                }
            }
            foreach (var sp_handler in mObjectMap.Values)
            {
                sp_handler.InstanceStartUp();
            }
            if (mRealmController != null)
                mRealmPVPType = mRealmController.mRealmInfo.pvptype;
        }        
               
        public GameTimer SetTimer(long duration, TimerDelegate del, object arg)
        {            
            return mTimers.SetTimer(duration, del, arg);
        }

        public void StopTimer(GameTimer t)
        {
            mTimers.StopTimer(t);
        }

        public void QueueDestoryTimer(GameTimer t)
        {
            mTimers.QueueDestoryTimer(t);
        }

        public long GetSynchronizedTime()
        {
            return mTimers.GetSynchronizedTime();
        }

        public bool HasCPUResourceToRoam()
        {
            if (mCurrRoamsInFrame < MAX_ROAMS_PER_FRAME)
            {
                mCurrRoamsInFrame++;
                return true;
            }
            return false;
        }

        public bool CanMonsterRoam(int monsterPID)
        {
            return (monsterPID % 10) == mCurrRoamNo;
        }

        public bool HasCPUResourceToGoBack()
        {
            if (mCurrGoBacksInFrames < MAX_GOBACKSAFE_PER_FRAME)
            {
                mCurrGoBacksInFrames++;
                return true;
            }
            return false;
        }      
  
        public void SetPlayerPosToSpawnPos(Player player)
        {
            if (mRealmController != null)
                mRealmController.SetSpawnPos(player);
            else
            {
                int count = mPlayerSpawnPt.Count;
                int index = (count > 1) ? GameUtils.RandomInt(0, count-1) : 0;
                player.Position = mPlayerSpawnPt[index];
                player.Forward = mPlayerSpawnDir[index];
            }
        }

        public bool IsDoingTutorialRealm()
        {
            return mRealmController != null && mRealmController.mRealmInfo.type == RealmType.Tutorial;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //RPC call from client
        //Player joins the room
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnClientLevelLoaded)]
        public void OnClientLevelLoaded(GameClientPeer peer)
        {
            if (Connections.ContainsKey(peer))
                return;

            DateTime now = DateTime.Now;
            string playername = peer.mChar;
            log.DebugFormat("OnClientLevelLoaded: charname = {0}, {1}, {2}", playername, peer.ConnectionId, mCurrentLevelName);
            peer.ResetExitGame();
            NetServerSlot slot = new NetServerSlot(peer, mEntitySystem, mRealmPVPType, mIsCity);
            Connections.Add(peer, slot);
            //Spawn player at server
            Player player = mEntitySystem.SpawnNetEntity<Player>(true, playername);
            player.InspectMode = peer.mInspectMode;
            peer.mInspectMode = false;
            GameCounters.TotalPlayers.Increment();
            CharacterData characterData = peer.CharacterData;

            player.InitLOStats();
            peer.IsJoinedRoom = true;

            /*********************   PlayerSynStats   ***************************/
            PlayerSynStats playerStats = new PlayerSynStats();
            playerStats.name = playername;
            playerStats.jobsect = characterData.JobSect;
            playerStats.Level = characterData.ProgressLevel;
            playerStats.PortraitID = characterData.portraitID;
            playerStats.faction = characterData.Faction;
            playerStats.Gender = characterData.Gender;
            playerStats.TutorialStatus = characterData.TutorialStatus;
         
            player.PlayerStats = playerStats;
            player.PlayerSynStats = playerStats;
            player.Name = playerStats.name;
            player.InitLevelUpCost();

            /*********************   LocalCombatStats   ***************************/
            LocalCombatStats lcs = player.LocalCombatStats;

            //lcs.Health;
            //lcs.HealthMax;
            //lcs.HealthRegen;
            //lcs.Mana;
            //lcs.ManaMax;
            //lcs.ManaRegen;
            //lcs.MoveSpeed;
            //lcs.ExpBonus;
            //lcs.WeaponAttack;
            //lcs.AttackPower;
            //lcs.Armor;
            //lcs.IgnoreArmor;
            //lcs.Block;
            //lcs.BlockValue;
            //lcs.Accuracy;
            //lcs.Evasion;
            //lcs.Critical;
            //lcs.CoCritical;
            //lcs.CriticalDamage;

            //lcs.Strength = characterData.CharInfoData.Str;
            //lcs.Agility = characterData.CharInfoData.Agi;
            //lcs.Dexterity = characterData.CharInfoData.Dex;
            //lcs.Constitution = characterData.CharInfoData.Con;
            //lcs.Intelligence = characterData.CharInfoData.Int;
            //lcs.StatsPoint = characterData.CharInfoData.StatPoint;
            //lcs.SkillPoints = characterData.CharInfoData.StatPoint;

            //lcs.SmashDamage;
            //lcs.SliceDamage;
            //lcs.PierceDamage;
            //lcs.IncElemNoneDamage;
            //lcs.IncElemMetalDamage;
            //lcs.IncElemWoodDamage;
            //lcs.IncElemEarthDamage;
            //lcs.IncElemWaterDamage;
            //lcs.IncElemFireDamage;
            //lcs.VSHumanDamage;
            //lcs.VSZombieDamage;
            //lcs.VSVampireDamage;
            //lcs.VSAnimalDamage;
            //lcs.VSPlantDamage;
            //lcs.VSElemNoneDamage;
            //lcs.VSElemMetalDamage;
            //lcs.VSElemWoodDamage;
            //lcs.VSElemEarthDamage;
            //lcs.VSElemWaterDamage;
            //lcs.VSElemFireDamage;
            //lcs.VSBossDamage;
            //lcs.IncFinalDamage;

            //lcs.SmashDefence;
            //lcs.SliceDefence;
            //lcs.PierceDefence;
            //lcs.IncElemNoneDefence;
            //lcs.IncElemMetalDefence;
            //lcs.IncElemWoodDefence;
            //lcs.IncElemEarthDefence;
            //lcs.IncElemWaterDefence;
            //lcs.IncElemFireDefence;
            //lcs.VSHumanDefence;
            //lcs.VSZombieDefence;
            //lcs.VSVampireDefence;
            //lcs.VSAnimalDefence;
            //lcs.VSPlantDefence;
            //lcs.DncFinalDamage;

            /*********************   GuildStats   ***************************/
            int myGuildId = characterData.GuildId;
            if (!GuildRules.GuildList.ContainsKey(myGuildId))
                characterData.ClearGuild();
            else
            {
                GuildStatsServer guildStats = GuildRules.GuildList[myGuildId];
                Dictionary<string, GuildMemberStats> memberStatsDict = guildStats.GetMemberStatsDict();
                if (memberStatsDict.ContainsKey(playername))
                {
                    playerStats.guildName = guildStats.name;
                    characterData.GuildRank = memberStatsDict[playername].rank;
                    if (!memberStatsDict[playername].online)
                    {
                        memberStatsDict[playername].online = true;
                        guildStats.members[memberStatsDict[playername].localObjIdx] = memberStatsDict[playername].ToString();
                    }
                }
                else
                    characterData.ClearGuild();
            }
            
            /*********************   SecondaryStats   ***************************/
            SecondaryStats secStats = player.SecondaryStats;
            secStats.experience = characterData.Experience;
            secStats.CurrencyExchangeTime = characterData.CurrencyExchangeTime;

            /*********************   CurrencyInventoryData   ***************************/
            CurrencyInventoryData CurrencyInventory = characterData.CurrencyInventory;
            secStats.Money = CurrencyInventory.Money;
            secStats.Gold = CurrencyInventory.Gold;
            secStats.bindgold = CurrencyInventory.BindGold;
            secStats.lotterypoints = CurrencyInventory.LotteryPoints;
            secStats.honor = CurrencyInventory.Honor;
            secStats.battlecoin = CurrencyInventory.BattleCoin;

            secStats.guildId = characterData.GuildId;
            secStats.guildRank = characterData.GuildRank;
            secStats.contribute = CurrencyInventory.GuildContribute;
            secStats.guildShopBuyCount = characterData.GuildShopBuyCount;
            secStats.GuildSMBossEntry = characterData.GuildSMBossEntry;
            secStats.GuildSMBossExtraEntry = characterData.GuildSMBossExtraEntry;
            secStats.guildLeaveGuildCDEnd = characterData.LeaveGuildCDEndTick;
            secStats.GuildDreamHouseUsed = characterData.GuildDreamHouseUsed;
            secStats.GuildDreamHouseCollected = characterData.GuildDreamHouseCollected;
            secStats.lastFreeLotteryRoll = characterData.LastFreeLotteryRoll;
            secStats.FreeReviveOnSpot = characterData.FreeReviveOnSpot;

            secStats.RandomBoxTimeTick = characterData.RandomBoxTimeTick;
            secStats.costbuffid = characterData.costbuffid;
            secStats.costbuffgold = characterData.costbuffgold;
            secStats.tutorialreddot = characterData.tutorialreddot;
            secStats.UnlockWorldBossLevel = GameConfig.UnlockWorldBossLevel;
            secStats.BattleTime = characterData.BattleTime;

            //secStats.guildDonateDot = DonateRules.CheckDonateNewDot(characterData.Name, characterData.GuildId);

            //-------------------- Destiny Clue Stats --------------------//
            DestinyClueInventory clueInventory = characterData.ClueInventory;
            peer.DestinyClueController.InitFromData(clueInventory, player.PlayerSynStats.Level);
            DestinyClueSynStats destinyClueStats = player.DestinyClueStats;
            peer.DestinyClueController.InitDestinyClueStats(ref destinyClueStats);

            //-------------------- Quest Stats --------------------//
            QuestInventoryData questInventory = characterData.QuestInventory;
            peer.QuestController.InitFromData(characterData, player);
            QuestSynStats questStats = player.QuestStats;
            peer.QuestController.InitQuestStats(ref questStats);
            player.PlayerSynStats.QuestCompanionId = peer.QuestController.GetQuestCompanionData();

            //-------------------- Donate Stats --------------------//
            DonateInventoryData donateInventory = characterData.DonateInventory;
            player.DonateController.InitFromData(characterData);
            DonateSynStats donateStats = player.DonateStats;
            player.DonateController.InitDonateStats(ref donateStats);

            /*********************   Realm Stats   ***************************/
            player.RealmStats.Init(characterData.RealmInventory);

            /*********************   Items/Equipments   ***************************/
            player.InitInventoryStats(peer.mInventory.mInvData);
            player.ItemHotbarStats.Init(characterData.ItemHotBarData);
            EquipmentInventoryData eqInvData = characterData.EquipmentInventory;
            if (!IsDoingTutorialRealm())
            {
                player.InitEquipmentStats(eqInvData);
            }
            else // Modify for tutorial realm
            {
                playerStats.jobsect = (byte)JobType.Tutorial;
                questStats.mainQuest = "";
                questStats.trackingList = "";
            }

            /*********************   SynCombatStats   ***************************/
            LocalCombatStats localCombatStats = player.LocalCombatStats;
            localCombatStats.IsInSafeZone = mIsCity;

            /*********************   CombatStats   ***************************/
            PlayerCombatStats combatStats = new PlayerCombatStats();
            
            IActor playerActor = player as IActor;
            combatStats.SetPlayerLocalAndSyncStats(localCombatStats, playerStats, playerActor);
            combatStats.SuppressComputeAll = true;
            Player.SetPlayerStats(playerStats.jobsect, playerStats.Level, combatStats, characterData.CharInfoData);
            player.CombatStats = combatStats;

            for (int i = 0; i < (int)EquipmentSlot.MAXSLOTS; ++i)
            {
                Equipment equipment = eqInvData.Slots[i];
                if (equipment != null)
                    peer.mInventory.UpdateEquipmentSideEffect(player, equipment.EquipmentJson, true);
            }

            // Add equipped combatstats
            //int selectedGemGroup = player.EquipInvStats.selectedGemGroup;
            //for (int e = 0; e < (int)EquipmentSlot.MAXSLOTS; e++)
            //{
            //    if (eqInvData.Slots[e] != null)
            //        combatStats.ComputeEquippedCombatStats(eqInvData.Slots[e], selectedGemGroup, selectedGemGroup, true);
            //}

            // Skills done after equipment
            player.SkillPassiveStats = new SkillPassiveCombatStats(player.EntitySystem.Timers, player);

            /*********************   SkillSynStats   *********************/
            player.SkillStats.Init(characterData.SkillInventory);

            /*********************   ChatStats   *********************/
            //player.ChatStats.Init(characterData.ChatInventory);

            /*********************   SocialStats   *********************/
            //SocialStats socialStats = player.SocialStats;
            //SocialInventoryData socialInventory = peer.mSocialInventory;
            player.SocialStats.LoadFromInventoryData(peer.mSocialInventory);

            //listCount = socialInventory.friendList.Count;
            //for(int i = 0; i < listCount; ++i)
            //{
            //    string info = socialInventory.friendList[i];
            //    SocialInfo socialInfo = new SocialInfo(info);
            //    socialInfo.localObjIdx = i;
            //    socialStats.GetFriendListDict()[socialInfo.charName] = socialInfo;
            //    socialStats.friendList[i] = info;
            //}
            //listCount = socialInventory.friendRequestList.Count;
            //for (int i = 0; i < listCount; ++i)
            //{
            //    string info = socialInventory.friendRequestList[i];
            //    SocialInfoBase socialInfo = new SocialInfoBase(info);
            //    socialInfo.localObjIdx = i;
            //    socialStats.GetFriendRequestListDict()[socialInfo.charName] = socialInfo;
            //    socialStats.friendRequestList[i] = info;
            //}

            /*********************   Store Stats   ***************************/
            //StoreSynStats storeStats = player.StoreSynStats;
            //StoreData storeData = characterData.StoreData;
            //if (storeData.list_store == null)
            //    storeData.InitDefault();
            //storeStats.Load(storeData);

            /*********************   HeroStats   ***************************/
            player.HeroStats.Init(player, peer, characterData.HeroInventory);

            /*********************   PartyStats   ***************************/
            int partyId = PartyRules.GetPartyIdByPlayerName(playername);
            if (partyId > 0)
            {
                player.PlayerSynStats.Party = partyId;
                player.PartyStats = PartyRules.GetPartyById(partyId);
                if (peer.mFirstLogin)
                    PartyRules.OnCharacterOnline(peer, player);
            }

            /*********************   AchievementStats   ***************************/
            player.AchievementStats.Init(player, peer);

            /************************   BattleTime Stats   *****************/
            //player.InitFromInventory(characterData.BattleTimeInventoryData);
            //player.BattleTime(characterData.BattleTimeInventoryData);

            /*********************   LotteryInventory   ***************************/
            player.InitLotteryStats(characterData.LotteryInventory, peer);

            /*********************   PowerUpInventory   ***************************/
            player.InitPowerUpStats(characterData.PowerUpInventory);

            /*********************   EquipmentCraftInventory   ***************************/
            player.InitEquipmentCraftStats(characterData.EquipmentCraftInventory);

            /*********************   EquipFusionInventory   ***************************/
            player.InitEquipFusionStats(characterData.EquipFusionInventory);

            /*********************   SevenDaysInventory   ***************************/
            //player.InitSevenDaysStats(characterData.SevenDaysInventory);

            /*********************   WelfareInventory   ***************************/
            //player.InitWelfareStats(characterData.WelfareInventory);
            //player.InitWelfareDailyActiveTask(peer, characterData.WelfareInventory);
            player.WelfareStats.firstBuyFlag = characterData.FirstBuyFlag;
            player.WelfareStats.firstBuyCollected = characterData.FirstBuyCollected;

            /*********************   QuestExtraRewardsInventory   ***************************/
            //player.InitQuestExtraRewardsStats(characterData.QuestExtraRewardsInventory);

            /*********************   DNAInventory   ***************************/
            player.InitDNAStats(characterData.DNAInventory);

            /*********************   ExchangeShopInventory   ***************************/
            //player.InitExchangeShopStats(characterData.ExchangeShopInv);

            /***************************   PortraitData   ***************************/
            //player.InitPortraitDataStats(characterData.PortraitData);

            /********************   Offline Exp   ********************/
            OfflineExp.OfflineExpManager2.Instance.GetRedDot(peer);
            //if (questStats.isTraining)
            //    player.WardrobeController.Init();

            /*********************   Initialize Player   *********************/
            player.Slot = peer;
            peer.mPlayer = player;
            player.SetInstance(this);
            player.SetOwnerID(player.GetPersistentID());

            if (!IsDoingTutorialRealm())
                player.InitSaveCharacterTimer();

            Vector3 spawnPos = peer.mSpawnPos;
            if (spawnPos.Equals(Vector3.zero))
                SetPlayerPosToSpawnPos(player);
            else
            {
                player.Position = spawnPos;
                player.Forward = peer.mSpawnForward;
            }
            peer.mSpawnPos = Vector3.zero;
            peer.mSpawnForward = Vector3.forward;

            player.PlayerStats.MoveSpeed = 6;
            mEntitySystem.RegisterPlayerName(player.Name, player);
            slot.SetLocalEntity(player); // Set player Local Objects
            player.ResetSyncStats();
            if (characterData.NewDayDts < DateTime.Today)
                player.NewDay();

            /*********************   Post player initialization   *********************/
            // Summon hero post player spawn
            player.HeroStats.PostPlayerSpawn();

            // Update post guild initialization
            //if (GuildRules.GuildList.ContainsKey(secStats.guildId))
            //{
            //    GuildStatsServer guildStats = GuildRules.GuildList[secStats.guildId];
            //    Dictionary<string, GuildMemberStats> memberStatsDict = guildStats.GetMemberStatsDict();
            //    if(memberStatsDict.ContainsKey(playername))
            //    {
            //        player.ApplyGuildPassiveSE(guildStats);
            //    }
            //}

            // TongbaoCostBuff
            //player.ApplyTongbaoCostBuffPassiveSE();

            // Update gold and diamond that gain during offline through auction
            //await player.UpdateGoldAndDiamond();

            //peer.ZRPC.CombatRPC.InitGameSetting(peer.GameSetting.Serialize(), characterData.BotSetting, peer);

            // Call this only when player last scene was lobby or yongheng (aka just log in) and current scene is not yongheng 
            if (peer.mPrevRoomName == "lobby" && player.mInstance.mCurrentLevelName != "yongheng")
            {
                // On login
                //executionFiber.Enqueue(async () => await Mail.MailManager.Instance.ClientInit(peer));
            }
            else // On change map, update 
            {
                //peer.ZRPC.CombatRPC.Ret_HasNewMail(peer.HasNewMail(), "", peer);
            }

            combatStats.SuppressComputeAll = false;
            combatStats.ComputeAll(); // We only compute it once here, since initialization might cause computeall to be done multiple times

            //--------------------------------------------------------------------------------------
            // Place code here that depends on latest combat stats after computeall
            //--------------------------------------------------------------------------------------
            if (characterData.Health <= 0 || characterData.Health > player.GetHealthMax())
                characterData.Health = player.GetHealthMax();
            player.SetHealth(characterData.Health);

            if (characterData.Mana < 0 || characterData.Mana > player.GetManaMax())
                characterData.Mana = player.GetManaMax();
            player.SetMana(characterData.Mana);
            player.StartManaRegen();

            if (mRealmController != null)
                mRealmController.OnPlayerEnter(player);

            foreach (var item in characterData.SkillInventory.m_SkillInventory)
            {
                peer.mPlayer.mSkillRecord.Add(item.Key, item.Value);
            }
            string inv = Newtonsoft.Json.JsonConvert.SerializeObject(peer.mPlayer.mSkillRecord);
            peer.ZRPC.NonCombatRPC.Ret_SkillInventory(inv, peer);

            peer.ZRPC.CombatRPC.SendInfoOnPlayerSpawner(GameApplication.Instance.mServerStartUpTicks, now.Ticks, 0, 0,
                player.InspectMode, peer.GetCharId(), peer);
            //peer.ZRPC.NonCombatRPC.ItemMallGetIsUIOn(ItemMall.ItemMallManager.Instance.isLimitedItemUIOnOff, peer);

            //SystemSwitch
            string swStr = SystemSwitch.mSysSwitch.GetSemicolonList();
            if (swStr != "")
                peer.ZRPC.CombatRPC.InitSystemSwitch(swStr, peer);

            //peer.mInventory.SyncEquipmentRequirement();

            // SideEffect Inventory
            player.ResumeSideEffects(characterData.SideEffectInventory);

            player.AchievementStats.PostSpawnCheckAchievements();

            if (peer.mFirstLogin)  // put this last line
                peer.mFirstLogin = false;

            // Designer Combat testing
            CombatFormula.Debug.CreateLog("Attacker_Stats");
            CombatFormula.Debug.CreateLog("Defender_Stats");
            CombatFormula.Debug.CreateLog("Player");
            CombatFormula.Debug.CreateLog("Monster");
            CombatFormula.Debug.CreateLog("Hero");
            CombatFormula.Debug.CreateLog("Profile");
            CombatFormula.Debug.CreateLog("Action");
            CombatFormula.Debug.CreateLog("SideEffect Logs");
            CombatFormula.Debug.CreateLog("Monster Issue");
            CombatFormula.Debug.CreateLog("DamageSE");
            //CombatFormula.Debug.LogOnly("DamageSE");            
        }

        public void OnActionCommand(int pid, ActionCommand cmd, HivePeer peer)
        {
            Player myPlayer = ((GameClientPeer)peer).mPlayer;
            if (myPlayer == null)
                return;
            int peerOwnerID = myPlayer.GetPersistentID();
            Entity entity = mEntitySystem.GetEntityByPID (pid);
            NetEntity ne = (NetEntity)entity;
            if (entity == null) 
            {
                log.DebugFormat("OnActionCommand: NetEntity pid [{0}] does not exist from non-owner playerid [{1}]", pid, peerOwnerID);
                return;
            }

            ACTIONTYPE actionType = cmd.GetActionType();
            if (actionType == ACTIONTYPE.SNAPSHOTUPDATE)
            {
                if (ne.GetOwnerID() != peerOwnerID)
                {
                    log.DebugFormat("SnapshotUpdate entity of pid [{0}] should not receive update from non-owner playerid [{1}]", pid, peerOwnerID);
                    return;
                }
                //Server always take whatever is sent from authoritative client and use it subsequently for any combat calculation
                SnapShotUpdateCommand snapCmd = (SnapShotUpdateCommand)cmd;
                ne.Position = snapCmd.pos;
                ne.Forward = snapCmd.forward;
            }
            else
            {
                // Non-snapshot command can set command
                ne.SetActionFromClient(cmd);
                //OnActionCommand always execute either before mainloop or after mainloop. But Tick only increments within mainloop.

                //If server needs to imitate action from client, it has to be performed here while broadcast is still done in netserverslot update
                Type type = ActionManager.GetAction(actionType);
                if (type != null)
                {
                    //object[] args = new object[2];
                    //args[0] = entity;
                    //args[1] = cmd;                    
                    //object action = Activator.CreateInstance(type, args);                    
                    if (actionType == ACTIONTYPE.CASTSKILL)
                        ne.PerformAction(new NonServerAuthoCastSkill(entity, cmd));
                    else if (actionType == ACTIONTYPE.Flash)
                        ne.PerformAction(new NonServerAuthoFlash(entity, cmd));
                    else if (actionType == ACTIONTYPE.WALKANDCAST)
                        ne.PerformAction(new NonServerAuthoWalkAndCast(entity, cmd));
                    else if (actionType == ACTIONTYPE.DASHATTACK)
                        ne.PerformAction(new NonServerAuthoDashAttack(entity, cmd));
                    else if (actionType == ACTIONTYPE.IDLE)
                        ne.PerformAction(new NonServerAuthoASIdle(entity, cmd));
                    else if (actionType == ACTIONTYPE.WALK)
                        ne.PerformAction(new NonServerAuthoASWalk(entity, cmd));
                    else if (actionType == ACTIONTYPE.GETHIT)
                        ne.PerformAction(new NonServerAuthoGethit(entity, cmd));
                }
                else
                {   
                    //We stop any previous nonauthoserver action still being executed by the netentity
                    ne.StopAction();
                }
            }
        }

        public void ProfileTestFunctionCall()
        {
            Type mytype = typeof(GameLogic);
            string methodname = "TestFunction";
            MethodInfo m = mytype.GetMethod(methodname); //, new Type[] {typeof(int), typeof(string)}
            //Func<int, string, int> converted = (Func<int, string, int>)Delegate.CreateDelegate(typeof(Func<int, string, int>), this, m); //return type, use Func
            Action<int, string> converted = (Action<int, string>)Delegate.CreateDelegate(typeof(Action<int, string>), this, m); //no return type, use Action            

            Zealot.Server.Counters.Profiler profiler = new Zealot.Server.Counters.Profiler();            
            profiler.Start();
            //m.Invoke(this, new object[] { 1, "test" }); //>10x time taken
            //TestFunction(1, "test"); //1x
            converted(1, "test"); //2x time taken
            double elapsed = profiler.StopAndGetElapsed(); 
            log.DebugFormat("invoke time = {0}", elapsed * 1000000);
        }

        private void SetupRPCCalleeProxies()
        {
            Zealot.Server.Counters.Profiler profiler = new Zealot.Server.Counters.Profiler();
            profiler.Start();

            if (MethodRPCProxyAttribs == null)
                InitRPCProxyAttribs();

            RPCCatMethodIDToDelegate = new Dictionary<RPCCategory, Dictionary<byte, Action<object[]>>>();
            
            long createDelegateTime = 0;
            Zealot.Server.Counters.Profiler createProfiler = new Zealot.Server.Counters.Profiler();

            int methodIndex = 0;
            foreach (MethodInfo methodinfo in RPCMethods)
            {                
                RPCMethodProxyAttribute proxyAtt = MethodRPCProxyAttribs[methodIndex];
                if (proxyAtt != null)
                {                    
                    bool supportedCategory = false;  
                    for (int i = (int)RPCCategory.Lobby; i < (int)RPCCategory.TotalCategory; i++)
                    {                        
                        if (proxyAtt.Category == (RPCCategory)i)
                        {
                            supportedCategory = true;
                            break;
                        }
                    }

                    if (supportedCategory)
                    {                        
                        Action<object[]> proxyDelegate = (Action<object[]>)Delegate.CreateDelegate(typeof(Action<object[]>), this, methodinfo);                        
                        if (RPCCatMethodIDToDelegate.ContainsKey(proxyAtt.Category))                        
                            RPCCatMethodIDToDelegate[proxyAtt.Category].Add(proxyAtt.MethodID, proxyDelegate);                        
                        else                        
                            RPCCatMethodIDToDelegate[proxyAtt.Category] = new Dictionary<byte, Action<object[]>>() { {proxyAtt.MethodID, proxyDelegate} };                        
                    }                    
                }
                createDelegateTime += (long)(createProfiler.StopAndGetElapsed() * 1000000);
                methodIndex++;
            }
            log.DebugFormat("SetupRPCCalleeProxies duration : {0}microsec", (long)(profiler.StopAndGetElapsed()*1000000));            
        }

        public bool RPCCallee(RPCCategory category, byte methodid, object [] objects)
        {
            if (RPCCatMethodIDToDelegate == null)
                return false;

            if (RPCCatMethodIDToDelegate.ContainsKey(category))
            {
                Action<object[]> callee;
                RPCCatMethodIDToDelegate[category].TryGetValue(methodid, out callee);
                if (callee != null)
                {
                    callee(objects);
                    return true;
                }
            }
            return false;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
