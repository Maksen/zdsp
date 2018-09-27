//#define ZEALOT_DEVELOPMENT
//#if ZEALOT_DEVELOPMENT

#if DEBUG
using System.Collections.Generic;
using System.Text;
using Zealot.Common.RPC;

namespace Photon.LoadBalancing.GameServer
{
    using Kopio.JsonContracts;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;
    using Zealot.Repository;
    using Zealot.RPC;
    using Zealot.Server.Rules;
    using Zealot.Server.SideEffects;
    using Zealot.Common;
    using System.Threading.Tasks;
    using System.Linq;
    using Hive.Caching;
    using Hive;
    using System;

    public partial class GameLogic
    {
        #region CombatRPC
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.CharInfoSetPortrait)]
        public void CharInfoSetPortrait(int portraitID, GameClientPeer peer)
        {
            peer.mPlayer.SetPortraitID(portraitID);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.CharInfoSetPortrait)]
        public void CharInfoSetPortraitProxy(object[] args)
        {
            CharInfoSetPortrait((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddItem)]
        public void AddItem(int itemId, int amount, GameClientPeer peer)
        {
            InvRetval retval = peer.mInventory.AddItemsIntoInventory((ushort)itemId, amount, true, "AddItem");
        }

        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddItem)]
        public void AddItemProxy(object[] args)
        {
            AddItem((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddCurrency)]
        public void AddCurrency(int type, int amount, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (amount > 0)
                player.AddCurrency((CurrencyType)type, amount, "AddCurrency");
            else
                player.DeductCurrency((CurrencyType)type, -amount, true, "AddCurrency");
                
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddCurrency)]
        public void AddCurrencyProxy(object[] args)
        {
            AddCurrency((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.BroadcastSysMsgToServer)]
        public void BroadcastSysMsgToServer(int msgid, string message, GameClientPeer peer)
        {
            // Receive chat messages from client
            ChatMessage newMsg = new ChatMessage(MessageType.System, message, peer.mChar);
            // Queue message at GameApplication
            GameApplication.Instance.BroadcastChatMessage(newMsg);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.BroadcastSysMsgToServer)]
        public void BroadcastSysMsgToServerProxy(object[] args)
        {
            BroadcastSysMsgToServer((int)args[0], (string)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ActiveRandomBox)]
        public void ActiveRandomBox(bool active, GameClientPeer peer)
        {
            //RandomBoxReward.RandomBoxRewardManager.Instance.SetActive(active);
        }

        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ActiveRandomBox)]
        public void ActiveRandomBoxProxy(object[] args)
        {
            ActiveRandomBox((bool)args[0], (GameClientPeer)args[1]);
        }

        #endregion

        #region NonCombatRPC
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RequestCombatStats)]
        public void RequestCombatStats(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            player.DebugSendCombatStats(peer);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RequestArenaAICombatStats)]
        public void RequestArenaAICombatStats(GameClientPeer peer)
        {
            RealmControllerArena arenaController = mRealmController as RealmControllerArena;
            if (arenaController == null)
                return;

            arenaController.DebugAIPlayerStats(peer.mPlayer);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RequestSideEffectsInfo)]
        public void RequestSideEffectsInfo(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            player.DebugSendSideEffectsInfo(peer);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleTeleportToLevel)]
        public void ConsoleTeleportToLevel(string levelName, GameClientPeer peer)
        {
            LevelJson lvlJ = LevelRepo.GetInfoByName(levelName);
            if (lvlJ != null)
            {
                RealmRules.TeleportToLevelInPos(levelName, null, false, peer);
            }
            else
            {
                peer.ZRPC.CombatRPC.SendMessageToConsoleCmd(string.Format("[Debug]: Level <{0}> does not exist.", levelName), peer);
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddSideEffect)]
        public void ConsoleAddSideEffect(int sideID, int pid, GameClientPeer peer)
        {
            if(sideID == 0)
            {
                return;
            }
            Zealot.Server.Entities.Actor target = mEntitySystem.GetEntityByPID(pid) as Zealot.Server.Entities.Actor;
            if (target != null)
                GameRules.ApplySideEffect(sideID, peer.mPlayer, target);
            else
                GameRules.ApplySideEffect(sideID, peer.mPlayer, peer.mPlayer);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleTestSideEffect)]
        public void ConsoleTestSideEffect(int sideEffType, int pid,  string otherParams, GameClientPeer peer)
        {
            if (sideEffType < 0)
            { 
                return;
                //setteam 
            }
            SideEffectJson sej = CombatUtils.SetupTestSideEffect(sideEffType, true, otherParams);              
            if (!SideEffectFactory.IsSideEffectInstantiatable(sej))
            {
                return;
            }
            if (sej.persistentafterdeath)// || sej.persistentonlogout)
            {
                SpecialSE sse = new SpecialSE(sej);
                sse.Apply(peer.mPlayer);
                return;
            }
           
            bool isPos = SideEffectsUtils.IsSideEffectPositive(sej);
            SideEffect se = SideEffectFactory.CreateSideEffect(sej);
            if (pid == -1)
                se.Apply(peer.mPlayer, peer.mPlayer, isPos);
            else
            {
                Zealot.Server.Entities.Actor actor = mEntitySystem.GetEntityByPID(pid) as Zealot.Server.Entities.Actor;
                if (actor == null)
                    return;

                se.Apply(actor, peer.mPlayer, isPos);
            }          
        }
        
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleTeleportToPosInLevel)]
        public void ConsoleTeleportToPosInLevel(RPCPosition pos, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && !player.Destroyed)
            {
                player.Position = pos.ToVector3();
                ZRPC.CombatRPC.TeleportSetPos(pos, peer);
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleClearItemInventory)]
        public void ConsoleClearItemInventory(GameClientPeer peer)
        {
            peer.mInventory.ClearItemInventory();
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddExperience)]
        public void ConsoleAddExperience(int exp, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && !player.Destroyed)
            {
                player.AddExperience(exp);
            }
        }
        
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddStats)]
        public void ConsoleAddStats(int statsType, float bonus, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (statsType == 1000)
            {
                player.PlayerSynStats.baSpeed = bonus;
                
                return;
            }else if (statsType == 2000)
            {
                //player.PlayerSynStats.rtReduction = bonus;
                player.CombatStats.AddToField(FieldName.AttackSpeedBuff, bonus);
                return;
            }
            else if (statsType == 3000)
            {
                player.PlayerSynStats.Gender = (byte)bonus;
                return;
            }
            FieldName type = (FieldName)(statsType);
            player.CombatStats.AddToField(type, (int)bonus);
            PlayerCombatStats pcs = player.CombatStats as PlayerCombatStats;
            player.CombatStats.ComputeAll();
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleEnterActivityByRealmID)]
        public void ConsoleEnterActivityByRealmID(int realmid, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            if (mRoom.RealmID == realmid)
                return;
            RealmJson realm = RealmRepo.GetInfoById(realmid);
            if (realm == null)
                return;
            LevelJson level = LevelRepo.GetInfoById(realm.level);
            int total_lvl = player.GetAccumulatedLevel();
            Room room;
            switch (realm.type)
            {
                //case RealmType.InvitePVP:
                //    RealmRules.PareInvitePVP(player.Name, "empty");
                //    RealmRules.EnterRealmById(realmid, peer);
                //    break;

                //case RealmType.ActivityGuardWar:
                //    int heroId = 1;
                //    if (!RealmRepo.mActivityGuardWar.ContainsKey(heroId))
                //        return;

                //    int worldLevel = GameApplication.Instance.Leaderboard.GetWorldLevel();
                //    ActivityGuardWarJson ActivityGuardWarJson = RealmRepo.GetActivityGuardWarJson(heroId, worldLevel);
                //    if (ActivityGuardWarJson == null)
                //        return;

                //    GameApplication.Instance.GameCache.TryGetRoomWithoutReference(GameApplication.Instance.GameCache.TryGetRealmRoomGuid(realmid, 0), out room);
                //    Game game = room as Game;
                //    if (game != null && game.controller != null && game.controller.mRealmController != null &&
                //        game.controller.mRealmController.CanReconnect())
                //    {
                //        peer.TransferRoom(room.Guid, level.unityscene);
                //    }
                //    else
                //        peer.CreateRealm(realmid, level.unityscene);
                //    break;

                //case RealmType.ActivityWorldBoss:
                //    string  roomGuid=GameApplication.Instance.GameCache.TryGetRealmRoomGuid(realmid, 0);
                //    if (string.IsNullOrEmpty(roomGuid) == false)
                //        peer.TransferRoom(roomGuid, level.unityscene);
                //    else
                //        peer.CreateRealm(realmid, level.unityscene);
                //    break;
                //case RealmType.DungeonDailySpecial: 
                //    string roomGuid2 = GameApplication.Instance.GameCache.TryGetRealmRoomGuid(realmid, 0);
                //    if (string.IsNullOrEmpty(roomGuid2) == false)
                //        peer.TransferRoom(roomGuid2, level.unityscene);
                //    else
                //        peer.CreateRealm(realmid, level.unityscene);
                //    break;
                default:
                    return;
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleInspect)]
        public void ConsoleInspect(GameClientPeer peer)
        {
            ConsoleEnterActivityByRealmID(23, peer);
            peer.mInspectMode = true;
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleNewDay)]
        public void ConsoleNewDay(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.NewDay();
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleServerNewDay)]
        public void ConsoleServerNewDay(GameClientPeer peer)
        {
            GameApplication.Instance.OnGameServerNewDay();
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleCompleteRealm)]
        public void ConsoleCompleteRealm(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && mRoom.RealmID > 0)
            {
                RealmJson realmJson = player.mInstance.mRealmController.mRealmInfo;
                RealmType type = realmJson.type;
                switch (type)
                {
                    case RealmType.Dungeon:
                        ((RealmControllerDungeon)mRealmController).OnMissionCompleted(true, true);
                        break;
                }
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleFullHealPlayer)]
        public void ConsoleFullHealPlayer(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
            {
                player.SetHealth(player.GetHealthMax());
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleFullRecoverMana)]
        public void ConsoleFullRecoverMana(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
            {
                player.SetMana(player.GetManaMax());
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGetAllRealmInfo)]
        public void ConsoleGetAllRealmInfo(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;

            PlayerSynStats playerSynStats = player.PlayerSynStats;
            Dictionary<string, RoomCacheBase.RoomInstance> rmInstances = GameApplication.Instance.GameCache.RoomInstances;
            int noOfRoom = rmInstances.Count;
            StringBuilder sb = new StringBuilder("Number of RoomInstances: ");
            sb.Append(noOfRoom);
            sb.Append(" (including lobby)\n");
            int idx = 0;
            foreach (KeyValuePair<string, RoomCacheBase.RoomInstance> kvp in rmInstances)
            {
                RoomCacheBase.RoomInstance rmInstance = kvp.Value;
                if (rmInstance.Room.Name == "lobby")
                    continue;
                sb.AppendFormat("{0}) RoomName: {1}, RefCount: {2}\n", ++idx, rmInstance.Room.Name, rmInstance.ReferenceCount);
            }
            ZRPC.CombatRPC.SendMessageToConsoleCmd(sb.ToString(), peer);
        }
        
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSpawnSpecialBoss)]
        public void ConsoleSpawnSpecialBoss(string name, GameClientPeer peer)
        {
            if (peer.mPlayer == null)
                return;
            int count = maMonsterSpawners.Count;
            for (int index = 0; index < count; ++index)
            {
                if (maMonsterSpawners[index] is SpecialBossSpawner && ((SpecialBossSpawner)maMonsterSpawners[index]).mSpecialBossSpawnerJson.archetype == name)
                    maMonsterSpawners[index].SpawnAllMonster();
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGuildList)]
        public void ConsoleGuildList(GameClientPeer peer)
        {
            foreach (var guild in GuildRules.GuildList)
            {
                string guildinfo = string.Format("{0} : {1}", guild.Key, guild.Value.name);
                peer.ZRPC.CombatRPC.SendMessageToConsoleCmd(guildinfo, peer);
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGuildAddFavour)]
        public void ConsoleGuildAddFavour(int amt, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if(player != null && !player.Destroyed)
            {
                int guildId = player.SecondaryStats.guildId;
                GuildStatsServer guildStats = GuildRules.GetGuildById(guildId);
                if (guildStats != null)
                {
                    if (guildStats.DreamHouseFavourability + amt > GuildRepo.DreamHouseTotalFavourability)
                        guildStats.DreamHouseFavourability = GuildRepo.DreamHouseTotalFavourability;
                    else
                        guildStats.DreamHouseFavourability += amt;
                    guildStats.saveToDB = true;
                }
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGuildSMLevel)]
        public void ConsoleGuildSMBossLevel(int level, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && !player.Destroyed)
            {
                int guildId = player.SecondaryStats.guildId;
                GuildStatsServer guildStats = GuildRules.GetGuildById(guildId);
                if (guildStats != null && GuildRepo.GetGuildSMBossByLvl(level) != null)
                {
                    guildStats.SMBossLevel = level;
                    guildStats.SMBossDmgDone = 0;
                    guildStats.saveToDB = true;
                }
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddRewardGroupMail)]
        public void ConsoleAddRewardMail(int grpID, GameClientPeer peer)
        {
            GameRules.GiveReward_Mail(peer.mPlayer.Name, "Reward_TestRewardGroup", peer.mPlayer.PlayerSynStats, new List<int>() { grpID }, null);
            //GameRules.GiveRewardGrp_Mail(peer.mPlayer.Name, "Reward_TestRewardGroup", new List<int>() { grpID }, null);
        }
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddRewardGroupBag)]
        public void ConsoleAddRewardBag(int grpID, GameClientPeer peer) 
        {
            //GameRules.GiveRewardGrp_Bag(peer.mPlayer, new List<int>() { grpID }, true, true, string.Format("ConsoleAddReward grpid={0}", grpID));
            GameRules.GiveReward_Bag(peer.mPlayer, new List<int>() { grpID }, true, true, string.Format("ConsoleAddReward grpid={0}", grpID));
        }
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddRewardGroupCheckBagSlot)]
        public void ConsoleAddRewardGroupCheckBagSlot(int grpID, GameClientPeer peer)
        {
            bool isFull = false;
            //GameRules.GiveRewardGrp_CheckBagSlot(peer.mPlayer, new List<int>() { grpID }, out isFull, 
            //    true, true, string.Format("ConsoleAddReward grpid={0}", grpID));

            GameRules.GiveReward_CheckBagSlot(peer.mPlayer, new List<int>() { grpID }, out isFull, true, true, string.Format("ConsoleAddReward grpid={0}", grpID));
        }
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddRewardGroupCheckBagMail)]
        public void ConsoleAddRewardGroupCheckBagMail(int grpID, GameClientPeer peer)
        {
            //GameRules.GiveRewardGrp_CheckBagSlotThenMail(peer.mPlayer, new List<int>() { grpID }, "Reward_TestRewardGroup", 
            //    null, true, true, string.Format("ConsoleAddReward grpid={0}", grpID));

            GameRules.GiveReward_CheckBagSlotThenMail(peer.mPlayer, new List<int>() { grpID }, "Reward_TestRewardGroup", 
                null, true, true, string.Format("ConsoleAddReward grpid={0}", grpID));
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGetCollection)]
        public void ConsoleGetCollection(string objtype, int target, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
            {
                if (string.Equals(objtype, "reset", StringComparison.OrdinalIgnoreCase))
                    player.AchievementStats.ConsoleResetCollections();
                else if (string.Equals(objtype, "all", StringComparison.OrdinalIgnoreCase))
                    player.AchievementStats.ConsoleGetAllCollections(-1);
                else
                {
                    int type;
                    if (int.TryParse(objtype, out type))
                    {
                        if (target == 0)
                            player.AchievementStats.ConsoleGetAllCollections(type);
                        else
                            player.AchievementStats.UpdateCollection((CollectionType)type, target);
                    }
                }
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGetAchievement)]
        public void ConsoleGetAchievement(string objtype, string target, int count, bool increment, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
            {
                if (string.Equals(objtype, "reset", StringComparison.OrdinalIgnoreCase))
                    player.AchievementStats.ConsoleResetAchievements();
                else if (string.Equals(objtype, "all", StringComparison.OrdinalIgnoreCase))
                    player.AchievementStats.ConsoleGetAllAchievements();
                else
                {
                    int type;
                    if (int.TryParse(objtype, out type))
                        player.AchievementStats.UpdateAchievement((AchievementObjectiveType)type, target, count, increment, true);
                }
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddHero)]
        public void ConsoleAddHero(int heroId, bool free, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)      
                player.HeroStats.UnlockHero(heroId, free);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleRemoveHero)]
        public void ConsoleRemoveHero(int heroId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.HeroStats.RemoveHero(heroId);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleResetExplorations)]
        public void ConsoleResetExplorations(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.HeroStats.ResetExplorations();
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleKillPlayer)]
        public void ConsoleKillPlayer(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
            {
                player.SetHealth(0);
                player.OnKilled(player);
            }
        }

		[RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleItemMallTopUp)]
        public void ConsoleItemMallTopUp(int amount, GameClientPeer peer)
        {
            if (peer != null)
            {
                ItemMall.ItemMallManager.Instance.UpdateTiming_Charged(peer, amount);
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleTestCombatFormula)]
        public void ConsoleTestCombatFormula(int amount, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            //TalentRepo.GetTalentDamagePercent(amount);
            //player.SkillStats.RedHeroCardSkillAttackSId = amount;
            //player.SkillStats.RedHeroCardSubskillId = amount;
            //SkillRepo.UpdateCompoundSkill(0, amount, amount);//test
        }
         

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddLotteryFreeTickets)]
        public void ConsoleAddLotteryFreeTickets(int lottery_id, int count, GameClientPeer peer)
        {
            
            LotteryMainData data = LotteryMainRepo.GetLottery(lottery_id);
            if (data != null) {
                Player player = peer.mPlayer;
                player.AddLotteryFreeTicket(data.main.id, count);
                player.UpdateLotteryStat(data.main.id);
            }
            
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddLotteryPoint)]
        public void ConsoleAddLotteryPoint(int lottery_id, int point, GameClientPeer peer)
        {
            LotteryMainData data = LotteryMainRepo.GetLottery(lottery_id);
            if (data != null)
            {
                Player player = peer.mPlayer;
                player.AddLotteryPoint(data.main.id, point);
                player.UpdateLotteryStat(data.main.id);
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleRefreshLeaderBoard)]
        public void ConsoleRefreshLeaderBoard(GameClientPeer peer)
        {
            GameApplication.Instance.Leaderboard.ForceRefreshLeaderBoard();
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleResetPrizeGuarantee)]
        public void ConsoleResetPrizeGuarantee(GameClientPeer peer)
        {
            //PrizeGuaranteeRules.ResetPrizeGuarantee(peer);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleDonateReset)]
        public void ConsoleDonateReset(GameClientPeer peer)
        {
            DonateRules.ConsoleResetDonate();
        }
        
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleResetDonateRemainingCount)]
        public void ConsoleResetDonateRemainingCount(GameClientPeer peer)
        {
            DonateRules.ConsoleResetDonateRemainingCount();
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddActivePoints)]
        public void ConsoleAddActivePoints(int amount, GameClientPeer peer)
        {
            peer.mQuestExtraRewardsCtrler.AddActivePoints(amount);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleFinishQERTask)]
        public void ConsoleFinishQERTask(int taskId, GameClientPeer peer)
        {
            peer.mQuestExtraRewardsCtrler.CompleteTaskByTaskID(taskId);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSocialAddFriend)]
        public async Task ConsoleSocialAddFriend(string playerName, GameClientPeer peer)
        {
            if (!string.IsNullOrEmpty(playerName))
                await peer.mPlayer.SocialAcceptFriendRequest(playerName);
            else
            {
                // Get Dict of peers
                Dictionary<string, GameClientPeer> charPeerDict = GameApplication.Instance.GetCharPeerDictCopy();
                if (charPeerDict == null)
                    return;

                Player player = peer.mPlayer;
                string myName = player.Name;
                StringBuilder friendRemoveSB = new StringBuilder();
                friendRemoveSB.AppendFormat("|{0}|", myName);
                charPeerDict.Remove(myName); // Remove your name from peer dict
                Dictionary<string, SocialInfo> myFriendsDict = player.SocialStats.GetFriendListDict();
                foreach (string friend in myFriendsDict.Keys) // Append your friends to filter dict
                {
                    friendRemoveSB.AppendFormat("{0}|", friend);
                    charPeerDict.Remove(friend); // Remove your friends from dict
                }

                Dictionary<string, string> friendsRecDict = new Dictionary<string, string>();
                int peerDictCnt = charPeerDict.Count, foundCnt = 0;
                if (peerDictCnt > 0) // Get random friends from online peer list first
                {
                    int min = (peerDictCnt < 100) ? peerDictCnt : 100;
                    int max = (peerDictCnt < 150) ? peerDictCnt : 150;
                    // Random sample size
                    int sampleSize = (min == max) ? peerDictCnt : GameUtils.RandomInt(min, max);

                    friendsRecDict.Clear();
                    for (int i = 0; i < sampleSize; ++i)
                    {
                        int currIdx = GameUtils.RandomInt(0, sampleSize-1);
                        GameClientPeer currPeer = charPeerDict.Values.ElementAt(currIdx);
                        string currCharName = currPeer.mChar;
                        if (myFriendsDict.ContainsKey(currCharName) || friendsRecDict.ContainsKey(currCharName))
                            continue;

                        friendRemoveSB.AppendFormat("{0}|", currCharName);
                        friendsRecDict[currCharName] = currCharName;
                        if (++foundCnt >= 5)
                            break;
                    }
                }

                if (foundCnt < 5) // If can't find more than 5 in local, search from db
                {
                    var dbInfo = await GameApplication.dbRepository.Character.GetSocialRandom(friendRemoveSB.ToString());
                    GameApplication.Instance.executionFiber.Enqueue(async () => {
                        int dbInfoCnt = dbInfo.Count;
                        if (dbInfoCnt > 0)
                        {
                            int dbIdx = 0;
                            for (int i = foundCnt; i < 5; ++i)
                            {
                                if (dbIdx >= dbInfoCnt)
                                    break;
                                Dictionary<string, object> infoDict = dbInfo[dbIdx];
                                string currCharName = infoDict["charname"] as string;
                                friendsRecDict[currCharName] = currCharName;
                                ++dbIdx;
                            }
                        }
                        foreach(string request in friendsRecDict.Keys)
                            await peer.mPlayer.SocialAcceptFriendRequest(request);
                    });
                }
                else
                {
                    foreach (string request in friendsRecDict.Keys)
                        await peer.mPlayer.SocialAcceptFriendRequest(request);
                }
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSetStoreRefreshTime)]
        public void ConsoleSetStoreRefreshTime(int storecat, int time, GameClientPeer peer)
        {
            Player p = peer.mPlayer;
            string stime = time.ToString();
            string s_hr = stime.Substring(0, 2);
            string s_min = stime.Substring(2, 2);
            int hr = 0, min = 0;
            bool success = int.TryParse(s_hr, out hr) && int.TryParse(s_min, out min);
            if (success && storecat > 0)
            {
                peer.CharacterData.StoreInventory.list_store[storecat].nextRefresh = System.DateTime.Now.Date + 
                                                                                new System.TimeSpan(hr, min, 0);
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleResetGoldJackpotRoll)]
        public void ConsoleResetGoldJackpotRoll(GameClientPeer peer)
        {
            peer.OnWelfareResetGoldJackpotRoll();
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleResetContLoginClaims)]
        public void ConsoleResetContLoginClaims(GameClientPeer peer)
        {
            peer.OnWelfareResetContLoginClaims();
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSetEquipmentUpgradeLevel)]
        public void ConsoleSetEquipmentUpgradeLevel(int slotID, int level, GameClientPeer peer)
        {
            peer.OnEquipmentSetUpgradeLevel(slotID, level);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleResetArenaEntey)]
        public void ConsoleResetArenaEntey(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                peer.CharacterData.ArenaInventory.Entries = 0;
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleTransferServer)]
        public void ConsoleTransferServer(int serverid, GameClientPeer peer)
        {
            if (serverid == GameApplication.Instance.GetMyServerId())
                return;
            bool request_sent = GameApplication.Instance.TransferServer(serverid, peer);
            if (!request_sent)
                peer.ZRPC.NonCombatRPC.Ret_TransferServer(serverid, "", peer);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleDCFromServer)]
        public void ConsoleDCFromServer(string server, GameClientPeer peer)
        {
            if (server == "cluster")
            {
                var _cluster_peer = GameApplication.Instance.clusterPeer;
                if (_cluster_peer != null && _cluster_peer.IsRegistered)
                    _cluster_peer.Disconnect();
            }
            else if (server == "master")
            {
                var _master_peer = GameApplication.Instance.masterPeer;
                if (_master_peer != null && _master_peer.IsRegistered)
                    _master_peer.Disconnect();
            }
            else if (server == "game")
            {
                peer.Disconnect();
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSpawnPersonalMonster)]
        public void ConsoleSpawnPersonalMonster(string archtype, int population, bool aggressive, int questid, GameClientPeer peer)
        {
            foreach(var entry in maMonsterSpawners)
            {
                if (entry is PersonalMonsterSpawner && entry.mArchetype.archetype == archtype)
                {
                    population = population == -1 ? ((PersonalMonsterSpawner)entry).GetPopulation() : population;
                    ((PersonalMonsterSpawner)entry).SpawnToMeOnly(peer.mPlayer, population, aggressive);
                    break;
                }
            }

            if (questid != -1)
            {
                peer.mPlayer.QuestController.UpdateQuestEventStatus(questid);
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleUpdateQuestProgress)]
        public void ConsoleUpdateQuestProgress(byte type, GameClientPeer peer)
        {
            peer.mPlayer.QuestController.UpdateQuestProgress((QuestType)type);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.TotalCrit)]
        public void TotalCrit()
        {
            CombatFormula.totalCrit = !CombatFormula.totalCrit;
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.CritRate)]
        public void CritRate(float crit, GameClientPeer peer)
        {
            CombatFormula.critRate = crit;
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddStatsPoint)]
        public void AddStatsPoint(int val, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && !player.Destroyed)
            {
                player.LocalCombatStats.StatsPoint += val;
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleChangeJob)]
        public void ConsoleChangeJob(byte job, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if(player != null)
            {
                player.UpdateJobSect(job);
            }
        }
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddSkillPoint)]
        public void ConsoleAddSkillPoint(int amt, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && !player.Destroyed)
                player.LocalCombatStats.SkillPoints += amt;
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleUpdateDonate)]
        public void ConsoleUpdateDonate(int type, GameClientPeer peer)
        {
            peer.mPlayer.DonateController.RefreshData(type == 6 ? true : false);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleRemoveAllSkills)]
        public void ConsoleRemoveAllSkills(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if(player != null)
            {
                player.SkillStats.SkillGroupIndex.Clear();
                player.SkillStats.SkillInv.Clear();
                player.SkillStats.SkillInvCount = 0;
                player.SkillStats.EquippedSkill.Clear();
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSendMail)]
        public void ConsoleSendMail(int mailid, GameClientPeer peer)
        {
            MailContentJson mcj = MailRepo.mIdMap[mailid];
            MailObject mo = new MailObject();
            mo.mailName = mcj.name;
            mo.rcvName = peer.mChar;
            PotionFood pf = new PotionFood();
            pf.LoadJson(GameRepo.ItemFactory.GetItemById(501));
            mo.lstAttachment.Add(pf);
            Photon.LoadBalancing.GameServer.Mail.MailManager.Instance.SendMail(mo);
        }
        #endregion
    }
}

#endif