using System;
using System.Collections.Generic;
using Photon.LoadBalancing.GameServer;
using ExitGames.Concurrency.Fibers;
using Kopio.JsonContracts;
using Zealot.Common;
using Zealot.Common.RPC;
using Zealot.Common.Entities;
using Zealot.Server.Entities;
using Zealot.Repository;
using System.Linq;
using System.Text;
using Photon.Hive;

namespace Zealot.Server.Rules
{
    enum RealmRetCode : byte
    {
        RealmRequiredlvl= 0,
        RealmGoldNotEnough,
        RealmEnterFailed,
        RealmEnterSuccess,
    }

    enum DungeonMemberState : byte
    {
        Pending = 0,
        Ready,
        Cancel
    }

    class DungeonMember
    {
        public string memberName = "";
        public DungeonMemberState state = DungeonMemberState.Pending;
    }

    class DungeonParty
    {
        public List<DungeonMember> members = new List<DungeonMember>(3);
        public int realmId = 0;
        public DateTime dtRequested;

        public DungeonParty(int id)
        {
            realmId = id;
            dtRequested = DateTime.Now;
        }

        public void AddMember(string name)
        {
            members.Add(new DungeonMember() { memberName = name });
        }
    }

    class InvitePVPData
    {
        public string GUID = "";
    }

    static class RealmRules
    {
        public static Dictionary<string, DungeonParty> DungeonStateDict = null;
        private static readonly PoolFiber executionFiber;

        private static Dictionary<string, InvitePVPData> InvitePVPList = new Dictionary<string, InvitePVPData>();

        public static readonly int UpdateInterval = 5000;
        
        static RealmRules()
        {
            executionFiber = GameApplication.Instance.executionFiber;
        }

        public static void Init()
        {
            DungeonStateDict = new Dictionary<string, DungeonParty>();
        }

        public static void Update()
        {
            List<string> keys = DungeonStateDict.Keys.ToList();
            for (int index = 0; index < keys.Count; index++)
            {
                string partyName = keys[index];
                DungeonParty dungeonParty = DungeonStateDict[partyName];
                DateTime dtRequest = dungeonParty.dtRequested;
                TimeSpan timeElapsed = DateTime.Now.Subtract(dtRequest);
                if (timeElapsed.TotalSeconds > 6)
                    DungeonStateDict.Remove(partyName);
            }
            executionFiber.Schedule(Update, UpdateInterval); // Reschedule next interval
        }

        public static RealmRetCode TeleportToLevelInPos(string levelName, RPCPosition pos, bool deductGold, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && !player.Destroyed)
            {
                RealmWorldJson realmWorldJson = RealmRepo.GetWorldByName(levelName);
                if (realmWorldJson != null) // Check if level name is realm world
                {
                    int reqLvl = realmWorldJson.reqlvl;
                    if (reqLvl > player.GetAccumulatedLevel())
                    {
                        string param = string.Format("level;{0}", player.GetLocalizedProgressLevelMin(reqLvl));
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_map_FailLvl", param, false, peer);
                        return RealmRetCode.RealmRequiredlvl;
                    }

                    if (deductGold)
                    {
                        int amt = (realmWorldJson.worldseq - 1) * 1000 + 2000;
                        if (amt < 0) amt = 0;
                        if (!player.IsCurrencySufficient(CurrencyType.Gold, amt))
                        {
                            peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_map_FailGold", "", false, peer);
                            return RealmRetCode.RealmGoldNotEnough;
                        }
                        else
                            player.DeductGold(amt, true, true, "Teleport");
                    }

                    if (levelName == player.mInstance.currentlevelname)
                    {
                        if (pos != null)
                            peer.ZRPC.CombatRPC.TeleportSetPos(pos, peer);
                        else
                        {
                            player.mInstance.SetSpawnPos(player);
                            peer.ZRPC.CombatRPC.TeleportSetPos(player.Position.ToRPCPosition(), peer);
                        }
                    }
                    else
                    {
                        int realmId = realmWorldJson.id;
                        string roomGuid = GameApplication.Instance.GameCache.TryGetRealmRoomGuid(realmId, realmWorldJson.maxplayer);
                        if (!string.IsNullOrEmpty(roomGuid))
                            peer.TransferRoom(roomGuid, levelName);
                        else
                            peer.CreateRealm(realmId, levelName);
                        if (pos != null)
                            peer.mSpawnPos = pos.ToVector3();
                    }
                    return RealmRetCode.RealmEnterSuccess;
                }
            }
            return RealmRetCode.RealmEnterFailed;
        }

        public static void CreateRealmById(int realmId, bool logAI, bool checkAll, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.Destroyed)
                return;
            RealmJson realm = null;
            string levelScene = "";
            if (!GetRealmInfo(realmId, ref realm, ref levelScene))
                return;
            if (!CheckRealmRequirement(realm, player, peer, checkAll))
                return;

            peer.CreateRealm(realmId, levelScene, logAI);
        }

        public static void PareInvitePVP(string name1, string name2)
        {
            InvitePVPData data = new InvitePVPData();
            InvitePVPList[name1] = data;
            InvitePVPList[name2] = data;
        }

        public static InvitePVPData GetInvitePVPData(string name)
        {
            if (InvitePVPList.ContainsKey(name) == false)
                return null;
            else
                return InvitePVPList[name];
        }

        public static void RemoveInvitePCPData(string name)
        {
            if (InvitePVPList.ContainsKey(name) == true)
                InvitePVPList.Remove(name);
        }

        public static void EnterRealmById(int realmId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.Destroyed)
                return;
            RealmJson realm = null;
            string levelScene = "";
            if (!GetRealmInfo(realmId, ref realm, ref levelScene))
                return;
            if (!CheckRealmRequirement(realm, player, peer, true))
                return;

            // Enter realm logic here
            string roomGuid = "";
            switch (realm.type)
            {
                //case RealmType.InvitePVP:
                //    EnterRealmCheckAndLeaveParty(player);
                //    var data = GetInvitePVPData(player.Name);
                //    if (data.GUID == "")
                //        data.GUID = peer.CreateRealm(realmId, levelScene);
                //    else
                //    {
                //        Room room;
                //        GameApplication.Instance.GameCache.TryGetRoomWithoutReference(data.GUID, out room);
                //        if (((Game)room).controller != null)
                //        {
                //            RealmController realmcontroller = ((Game)room).controller.mRealmController;
                //            if (realmcontroller == null || !realmcontroller.CanReconnect())
                //            {
                //                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_activity_End", "", false, peer);
                //                return;
                //            }
                //        }
                //        peer.TransferRoom(data.GUID, levelScene);
                //    }
                //    break;

                //case RealmType.ActivityGuardWar:
                    //string partyName = peer.GetPartyName();
                    //PartyInfoStats partyStats = string.IsNullOrEmpty(partyName) ? null : GameRules.PartyController.GetPartyInfoStatsByPartyName(partyName);
                    //if (partyStats == null || partyStats.MemberCount() == 1)
                    //{
                    //    roomGuid = GameApplication.Instance.GameCache.TryGetRealmRoomGuid(realmId, realm.maxplayer);
                    //    if (!string.IsNullOrEmpty(roomGuid))
                    //    {
                    //        Room room;
                    //        GameApplication.Instance.GameCache.TryGetRoomWithoutReference(roomGuid, out room);
                    //        if (((Game)room).controller != null)
                    //        {
                    //            RealmController realmcontroller = ((Game)room).controller.mRealmController;
                    //            if (realmcontroller == null || !realmcontroller.CanReconnect())
                    //            {
                    //                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_activity_End", "", false, peer);
                    //                return;
                    //            }
                    //        }
                    //        peer.TransferRoom(roomGuid, levelScene);
                    //    }
                    //    else
                    //        peer.CreateRealm(realmId, levelScene);
                    //}
                    //else
                    //    PartyEnterRequest(partyStats, realm, peer);
                    //break;

                //case RealmType.ActivityGuildSMBoss:
                //    EnterRealmCheckAndLeaveParty(player);
                //    GuildRules.EnterRealmGuildSMBoss(player.SecondaryStats.guildId, realmId, levelScene, peer);
                //    break;
                //case RealmType.ActivityWorldBoss:
                //    ActivityWorldBossJson ActivityWorldBossJson = RealmRepo.mActivityWorldBoss;

                //    DateTime now = DateTime.Now;
                //    bool isOpen = GameUtils.IsEventOpen(now, ActivityWorldBossJson.opendaily, ActivityWorldBossJson.timelimit);
                //    if (!isOpen)
                //    {
                //        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_activity_End", "", false, peer);
                //        return;
                //    }

                //    EnterRealmCheckAndLeaveParty(player);
                //    roomGuid = GameApplication.Instance.GameCache.TryGetRealmRoomGuid(realmId, realm.maxplayer);
                //    if (!string.IsNullOrEmpty(roomGuid))
                //    {
                //        Room room;
                //        GameApplication.Instance.GameCache.TryGetRoomWithoutReference(roomGuid, out room);
                //        RealmController realmcontroller = ((Game)room).controller.mRealmController;
                //        if (realmcontroller == null || !realmcontroller.CanReconnect())
                //        {
                //            peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_activity_End", "", false, peer);
                //            return;
                //        }
                //        peer.TransferRoom(roomGuid, levelScene);
                //    }
                //    else
                //        peer.CreateRealm(realmId, levelScene);
                //    break;
                //case RealmType.EliteMap:
                //    EnterRealmCheckAndLeaveParty(player);
                //    roomGuid = GameApplication.Instance.GameCache.TryGetRealmRoomGuid(realmId, realm.maxplayer);
                //    if (!string.IsNullOrEmpty(roomGuid))
                //        peer.TransferRoom(roomGuid, levelScene);
                //    else
                //        peer.CreateRealm(realmId, levelScene);
                //    break;
                default:
                    roomGuid = GameApplication.Instance.GameCache.TryGetRealmRoomGuid(realmId, realm.maxplayer);
                    if (!string.IsNullOrEmpty(roomGuid))
                        peer.TransferRoom(roomGuid, levelScene);
                    break;
            }
        }

        public static void InspectMode(int realmId, GameClientPeer peer)
        {
            //Player player = peer.mPlayer;
            //if (player == null || player.Destroyed)
            //    return;
            //RealmJson realm = null;
            //string levelScene = "";
            //if (!GetRealmInfo(realmId, ref realm, ref levelScene))
            //    return;
            //if (!player.mInstance.IsWorld())
            //    return;

            //int heroId, defendeGuild;
            //int[] attackGuild;
            //HeroesHouseRules.GetGuardBattleInfo(out heroId, out defendeGuild, out attackGuild);
            //if (defendeGuild == 0)
            //    return;
            //if (attackGuild == null || attackGuild.Length == 0)
            //    return;
            //if (!RealmRepo.mActivityGuardWar.ContainsKey(heroId))
            //    return;          
            //int worldLevel = GameApplication.Instance.Leaderboard.GetWorldLevel();
            //ActivityGuardWarJson ActivityGuardWarJson = RealmRepo.GetActivityGuardWarJson(heroId, worldLevel);
            //if (ActivityGuardWarJson == null)
            //    return;

            //EnterRealmCheckAndLeaveParty(player);
            //string roomGuid = GameApplication.Instance.GameCache.TryGetRealmRoomGuid(realmId, realm.maxplayer);
            //if (!string.IsNullOrEmpty(roomGuid))
            //{
            //    Room room;
            //    GameApplication.Instance.GameCache.TryGetRoomWithoutReference(roomGuid, out room);
            //    if (((Game)room).controller != null)
            //    {
            //        RealmController realmcontroller = ((Game)room).controller.mRealmController;
            //        if (realmcontroller == null || !realmcontroller.CanReconnect())
            //        {
            //            peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_activity_End", "", false, peer);
            //            return;
            //        }
            //    }
            //    peer.TransferRoom(roomGuid, levelScene);
            //}
            //else
            //    peer.CreateRealm(realmId, levelScene);
            //peer.mInspectMode = true;
        }

        public static void EnterRealmCheckAndLeaveParty(Player player)
        {
            //string partyName = player.Slot.GetPartyName();
            //if (string.IsNullOrEmpty(partyName))
            //    return;
            //PartyInfoStats partyStats = GameRules.PartyController.GetPartyInfoStatsByPartyName(partyName);
            //if (partyStats != null && partyStats.MemberCount() > 1)
            //    GameRules.PartyController.LeaveParty(partyName, player, player.Name);
        }

        public static void OnStoryAddExtraEntry(int realmId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.Destroyed)
                return;
            RealmJson realm = RealmRepo.GetInfoById(realmId);
            if (realm == null || realm.type != RealmType.DungeonStory) // Can only add extry entry for dungeon story
                return;

            DungeonStoryJson dStoryJson = (DungeonStoryJson)realm;
            int seq = dStoryJson.sequence;
            Dictionary<int, int> extraEntryFeesDict = RealmRepo.GetExtraEntryFeesBySeq(seq);
            DungeonStoryInfo dungeonStoryInfo = player.RealmStats.GetDungeonStoryDict()[seq];
            //int dailyLimit = VIPRepo.GetVIPPrivilege("DungeonStory", player.PlayerSynStats.vipLvl);
            //if (dungeonStoryInfo.DailyExtraEntry >= dailyLimit) // Check daily limit
            //{
            //    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Dun_DailyExtraEntryVipLimit", "", false, peer);
            //    return;
            //}
            int addFee = 0;
            if (extraEntryFeesDict.TryGetValue(dungeonStoryInfo.DailyExtraEntry+1, out addFee))
            {
                if (player.DeductGold(addFee, true, true, "Realm_AddEntry"))
                {
                    ++dungeonStoryInfo.DailyExtraEntry;
                    player.RealmStats.AddExtraEntry(realm.type, seq, 1);
                    LogStoryAddExtraEntry(peer, realmId, dungeonStoryInfo.DailyExtraEntry, dungeonStoryInfo.ExtraEntry);
                }
                else
                    peer.ZRPC.CombatRPC.OpenUIWindow((byte)LinkUIType.GoTopUp, -1, peer);
            }
        }

        public static bool OnDungeonRaid(int realmId, GameClientPeer peer)
        {
            bool isSuccess = true;
            Player player = peer.mPlayer;
            RealmJson realm = null;
            string levelScene = "";
            if (player == null || player.Destroyed)
                isSuccess = false;
            else if (!GetRealmInfo(realmId, ref realm, ref levelScene))
                isSuccess = false;
            else if (realm.type != RealmType.DungeonStory) // Can only raid on dungeon story
                isSuccess = false;
            else if (!CheckRealmRequirement(realm, player, peer, true))
                isSuccess = false;

            int rewardGrp = 0;
            List<ItemInfo> itemInfos = null;
            if (isSuccess)
            {
                DungeonStoryJson dStoryJson = (DungeonStoryJson)realm;
                rewardGrp = dStoryJson.rewardgrp;
                Dictionary<int, DungeonStoryInfo> dStoryDict = player.RealmStats.GetDungeonStoryDict();
                DungeonStoryInfo dStoryInfo = dStoryDict[dStoryJson.sequence];
                int diffIdx = (int)dStoryJson.difficulty * 3;
                if (!dStoryInfo.StarObjectiveCompleted[diffIdx] || !dStoryInfo.StarObjectiveCompleted[diffIdx + 1] ||
                    !dStoryInfo.StarObjectiveCompleted[diffIdx + 2])
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Dun_RaidRequirementNotMet", "", false, peer);
                    isSuccess = false;
                }
                else
                {
                    if (dStoryInfo.DailyEntry > 0)
                        --dStoryInfo.DailyEntry;
                    else if (dStoryInfo.ExtraEntry > 0)
                        --dStoryInfo.ExtraEntry;
                    player.RealmStats.DungeonStory[dStoryInfo.LocalObjIdx] = dStoryInfo.ToString();
                    player.Slot.mSevenDaysController.UpdateTask(dStoryJson.difficulty);
                    peer.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.StoryDungeon);
                }

                bool isEventOn = false;
                int rewardMultiplier = 1, extraRewardItemID = 0, extraRewardPercent = 0, extraRewardStackCount = 0;
                var configs = GMActivityConfig.GetConfigIntList(GMActivityType.Dungeon, DateTime.Now);//get list of activity that is currently ON
                if (configs.Count > 0)
                {
                    foreach (var config in configs)
                    {
                        int seq = config.mDataList[0];
                        int realmTypeID = config.mDataList[1];
                        if (seq == dStoryJson.sequence && realmTypeID == (int)dStoryJson.type)
                        {
                            rewardMultiplier = config.mDataList[2];
                            extraRewardItemID = config.mDataList[3];
                            extraRewardPercent = config.mDataList[4];
                            extraRewardStackCount = config.mDataList[5];
                            isEventOn = true;
                            break;
                        }
                    }
                }

                if (isEventOn)
                {
                    itemInfos = GameRules.GiveRewardGrp_CheckBagSlotThenMail_WithAdditionalItems(player, new List<int>() { rewardGrp }, "Reward_DungeonStory",
                    null, true, false, string.Format("RealmStory id={0}", realmId),
                    rewardMultiplier, extraRewardItemID, extraRewardPercent, extraRewardStackCount);
                }
                else
                {
                    itemInfos = GameRules.GiveReward_CheckBagSlotThenMail(player, new List<int>() { rewardGrp }, "Reward_DungeonStory",
                    null, true, false, string.Format("RealmStory id={0}", realmId));
                }
            }

            peer.ZRPC.CombatRPC.Ret_RaidReward(isSuccess, rewardGrp, GameUtils.SerializeItemInfoList(itemInfos), peer); // Return reward result
            return isSuccess;
        }

        public static void TryDungeonEnter(int realmId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            string myName = peer.mChar;
            if (player == null || player.Destroyed)
                return;

            RealmJson realm = null;
            string levelScene = "";
            if (!GetRealmInfo(realmId, ref realm, ref levelScene))
                return;
            bool isCompletingQuest = false;
            if (realm.type == RealmType.DungeonStory)
            {
                DungeonStoryJson dStoryJson = (DungeonStoryJson)realm;
            }
            if (!CheckRealmRequirement(realm, player, peer, !isCompletingQuest))
                return;

            int partyId = player.PlayerSynStats.Party;
            PartyStats partyStats = PartyRules.GetPartyById(partyId);           
            if (partyStats == null || partyStats.MemberCount() == 1)
            {
                if (realm.minplayer == 1)
                    peer.CreateRealm(realmId, levelScene);
                else
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_LessThanMinPlayer", "", false, peer);
            }
            else
            {
                int membersCnt = partyStats.MemberCount();
                switch (realm.type)
                {
                    case RealmType.DungeonStory:
                        //PartyRules.LeaveParty(partyId, player, myName);
                        peer.CreateRealm(realmId, levelScene);
                        break;
                    case RealmType.DungeonDailySpecial:
                        if (realm.maxplayer == 1)
                        {
                            //GameRules.PartyController.LeaveParty(partyName, player, myName);
                            peer.CreateRealm(realmId, levelScene);
                        }
                        else
                        {
                            if (membersCnt < realm.minplayer)
                                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_LessThanMinPlayer", "", false, peer);
                            else
                                PartyEnterRequest(partyStats, realm, peer);
                        }
                        break;
                }
            }
        }

        public static void PartyEnterRequest(PartyStats partyStats, RealmJson realm, GameClientPeer peer)
        {
            string myName = peer.mChar;
            if (partyStats.IsLeader(myName))
            {
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_OnlyPartyLeaderAllow", "", false, peer);
                return;
            }
            int membersCnt = partyStats.MemberCount();
            if (realm.maxplayer != 0 && membersCnt > realm.maxplayer)
            {
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_PlayerCountOver", "", false, peer);
                return;
            }

            Player player = peer.mPlayer;
            int guildId = player.SecondaryStats.guildId;
            var members = partyStats.GetPartyMemberList();
            Dictionary<string, GameClientPeer> memberPeers = new Dictionary<string, GameClientPeer>();            
            foreach (var member in members.Values)
            {
                string memberName = member.name;
                if (memberName != myName)
                {
                    GameClientPeer memberPeer = GameApplication.Instance.GetCharPeer(memberName);
                    Player member_Player = memberPeer == null ? null : memberPeer.mPlayer;
                    //Member requirements check.
                    if (member_Player == null || member_Player.Destroyed)
                    {
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_TransferringRealm",
                                string.Format("name;{0}", memberName), false, peer);
                        return;
                    }
                    if (!CheckRealmRequirement(realm, member_Player, peer, false))
                        return;
                    //if (realm.type == RealmType.ActivityGuardWar)
                    {
                        if (member_Player.SecondaryStats.guildId != guildId)
                        {
                            peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_HeroFight_NotSameGuild", 
                                string.Format("member;{0}", memberName), false, peer);
                            return;
                        }
                    }
                    memberPeers.Add(memberName, memberPeer);
                }
                else
                    memberPeers.Add(myName, peer);
            }

            // Send RPC to each party member to open enter realm confirm dialog
            int realmId = realm.id;
            DungeonParty dungeonParty = new DungeonParty(realmId);
            foreach (var member in members.Values)
            {
                string memberName = member.name;
                GameClientPeer memberPeer = memberPeers[memberName];
                bool hasEntry = true;
                if (memberName != myName) // Check entry of other members
                {
                    Player memberPlayer = memberPeer.mPlayer;
                    if (realm.type == RealmType.DungeonDailySpecial)
                    {
                        DungeonDailySpecialJson dDailySpecialJson = (DungeonDailySpecialJson)realm;
                        Dictionary<int, RealmInfo> realmInfoDict = (dDailySpecialJson.dungeontype == DungeonType.Daily)
                                                                    ? memberPlayer.RealmStats.GetDungeonDailyDict()
                                                                    : memberPlayer.RealmStats.GetDungeonSpecialDict();
                        int seq = dDailySpecialJson.sequence;
                        hasEntry = (realmInfoDict[seq].DailyEntry + realmInfoDict[seq].ExtraEntry > 0);
                    }
                }
                memberPeer.ZRPC.CombatRPC.Ret_DungeonEnterConfirmResult(realmId, hasEntry, "", memberPeer);
                dungeonParty.AddMember(memberName);
            }
            DungeonStateDict[myName] = dungeonParty;
        }

        public static void PartyEnterResponse(int realmId, byte state, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.Destroyed)
                return;

            //string charName = peer.mChar;
            //string partyName = peer.GetPartyName();
            //DungeonParty dungeonParty = null;
            //if(DungeonStateDict.TryGetValue(partyName, out dungeonParty))
            //{
            //    bool canEnter = true;
            //    int membersCnt = dungeonParty.members.Count;
            //    for (int i = 0; i < membersCnt; ++i)
            //    {
            //        DungeonMember dungeonMember = dungeonParty.members[i];
            //        if (dungeonMember.memberName == charName) // Is sender peer
            //            dungeonMember.state = (DungeonMemberState)state;
            //        else if((DungeonMemberState)state == DungeonMemberState.Cancel)
            //        {
            //            GameClientPeer memberPeer = GameApplication.Instance.GetCharPeer(dungeonMember.memberName);
            //            if (memberPeer != null)
            //                memberPeer.ZRPC.CombatRPC.Ret_DungeonEnterConfirmResult(realmId, true, charName, memberPeer);
            //        }
            //        DungeonMemberState currState = dungeonMember.state;
            //        if (currState == DungeonMemberState.Pending || currState == DungeonMemberState.Cancel)
            //            canEnter = false;
            //    }
            //    if (canEnter)
            //    {
            //        DungeonStateDict.Remove(partyName);
            //        PartyEnter(partyName, dungeonParty);
            //    }
            //    else if (state == (byte)DungeonMemberState.Cancel)
            //        DungeonStateDict.Remove(partyName);   
            //}
        }

        public static void PartyEnter(string partyLeader, DungeonParty dungeonParty)
        {
            //PartyInfoStats partyStats = string.IsNullOrEmpty(partyLeader) ? null : GameRules.PartyController.GetPartyInfoStatsByPartyName(partyLeader);
            //if (partyStats == null)
            //    return;
            //List<PartyInfoStats.PartyMemberInfo> members = partyStats.AllPartyMemberInfo;
            //List<string> memberNames = members.Select(x => x.name).ToList();
            //GameClientPeer leaderPeer = GameApplication.Instance.GetCharPeer(partyLeader);
            //Player player = leaderPeer.mPlayer;
            //if (player == null || player.Destroyed || !memberNames.Contains(partyLeader))
            //    return;
            //RealmJson realm = null;
            //string levelScene = "";
            //int realmId = dungeonParty.realmId;
            //if (!GetRealmInfo(realmId, ref realm, ref levelScene))
            //    return;
            //string roomGuid = "";
            //switch (realm.type)
            //{
            //    case RealmType.DungeonDailySpecial:
            //        if (!CheckRealmRequirement(realm, player, leaderPeer, true))
            //            return;
            //        roomGuid = leaderPeer.CreateRealm(realmId, levelScene);
            //        break;
            //    case RealmType.ActivityGuardWar:
            //        if (!CheckRealmRequirement(realm, player, leaderPeer, true))
            //            return;
            //        roomGuid = GameApplication.Instance.GameCache.TryGetRealmRoomGuid(realmId, realm.maxplayer);
            //        if (!string.IsNullOrEmpty(roomGuid))
            //        {
            //            Room room;
            //            GameApplication.Instance.GameCache.TryGetRoomWithoutReference(roomGuid, out room);
            //            if (((Game)room).controller != null)
            //            {
            //                RealmController realmcontroller = ((Game)room).controller.mRealmController;
            //                if (realmcontroller == null || !realmcontroller.CanReconnect())
            //                {
            //                    leaderPeer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_activity_End", "", false, leaderPeer);
            //                    return;
            //                }
            //            }
            //            leaderPeer.TransferRoom(roomGuid, levelScene);
            //        }
            //        else
            //            roomGuid = leaderPeer.CreateRealm(realmId, levelScene);
            //        break;
            //    default:
            //        return;
            //}
            //if (string.IsNullOrEmpty(roomGuid))
            //    return;

            //int guildId = player.SecondaryStats.guildId;
            //int membersCnt = dungeonParty.members.Count;
            //for (int i = 0; i < membersCnt; ++i)
            //{
            //    DungeonMember dungeonMember = dungeonParty.members[i];
            //    string memberName = dungeonMember.memberName;
            //    if (partyLeader == memberName || !memberNames.Contains(memberName))
            //        continue;
            //    GameClientPeer memberPeer = GameApplication.Instance.GetCharPeer(memberName);
            //    Player member_Player = memberPeer == null ? null : memberPeer.mPlayer;
            //    if (member_Player == null || member_Player.Destroyed || !CheckRealmRequirement(realm, member_Player, leaderPeer, false))
            //        continue;
            //    if (realm.type == RealmType.ActivityGuardWar)
            //    {
            //        if (member_Player.SecondaryStats.guildId != guildId)
            //        {
            //            leaderPeer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_HeroFight_NotSameGuild",
            //                string.Format("member;{0}", member_Player.Name), false, leaderPeer);
            //            continue;
            //        }
            //    }

            //    memberPeer.TransferRoom(roomGuid, levelScene);
            //}
        }

        public static void DungeonCollectStarReward(int seq, int starCount, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.Destroyed)
                return;
            Dictionary<int, DungeonStoryInfo> storyDict = player.RealmStats.GetDungeonStoryDict();
            if (!storyDict.ContainsKey(seq))
                return;
            DungeonStoryInfo dStoryInfo = storyDict[seq];
            Dictionary<int, bool> starCollectedDict = dStoryInfo.GetStarCollectedDict();
            if (starCollectedDict.ContainsKey(starCount))
            {
                if (dStoryInfo.TotalStarCompleted < starCount)
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_StarCountNotEnough", "", false, peer);
                    return;
                }
                if (starCollectedDict[starCount])
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_RewardAlreadyClaimed", "", false, peer);
                    return;
                }
                Dictionary<int, int> starRewardsDict = RealmRepo.GetStarRewardsBySeq(seq);
                int rewardGrpId = 0;
                if (starRewardsDict.TryGetValue(starCount, out rewardGrpId))
                {
                    bool isFull = false;
                    GameRules.GiveReward_CheckBagSlot(peer.mPlayer, new List<int>() { rewardGrpId }, out isFull, true, true, string.Format("Realm start={0}", starCount));
                    if (!isFull)
                    {
                        starCollectedDict[starCount] = true;
                        dStoryInfo.StarCollectedDictToString();
                        player.RealmStats.DungeonStory[dStoryInfo.LocalObjIdx] = dStoryInfo.ToString();
                        LogCollectStarReward(peer, seq, starCount);
                    }
                    else
                    {
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_BagInventoryFull", "", false, peer);
                        return;
                    }
                }
            }
        }

        public static bool GetRealmInfo(int realmId, ref RealmJson mRealm, ref string levelScene)
        {
            RealmJson realm = RealmRepo.GetInfoById(realmId);
            if (realm == null || realm.type == RealmType.RealmWorld)
                return false;
            mRealm = realm;
            LevelJson level = LevelRepo.GetInfoById(realm.level);
            if (level == null)
                return false;
            levelScene = level.unityscene;
            return true;
        }

        public static bool CheckRealmRequirement(RealmJson realm, Player playerToCheck, GameClientPeer peerToInform, bool checkAll)
        {
            bool samePlayer = playerToCheck.Slot == peerToInform;
            if (!playerToCheck.mInstance.IsWorld())
            {
                if (samePlayer)
                    peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_InRealm", "", false, peerToInform);
                else
                    peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_InRealm_PartyMember",
                        string.Format("name;{0}", playerToCheck.Name), false, peerToInform);
                return false;
            }

            if (realm.reqlvl > playerToCheck.GetAccumulatedLevel())
            {
                if (samePlayer)
                    peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_PlayerReqLvlNotMet", "", false, peerToInform);
                else
                    peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_PlayerReqLvlNotMet_PartyMember",
                        string.Format("name;{0}", playerToCheck.Name), false, peerToInform);
                return false;
            }

            if (checkAll)
            {
                int seq = 0;
                int guildId = playerToCheck.SecondaryStats.guildId;
                switch (realm.type)
                {
                    case RealmType.DungeonStory:
                        DungeonStoryJson dStoryJson = (DungeonStoryJson)realm;
                        Dictionary<int, DungeonStoryInfo> dStoryInfoDict = playerToCheck.RealmStats.GetDungeonStoryDict();
                        seq = dStoryJson.sequence;
                        if (dStoryInfoDict[seq].DailyEntry+dStoryInfoDict[seq].ExtraEntry <= 0)
                        {
                            if (samePlayer)
                                peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_NoEntryLeft", "", false, peerToInform);
                            else
                                peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_NoEntryLeft_PartyMember",
                                    string.Format("name;{0}", playerToCheck.Name), false, peerToInform);
                            return false;
                        }
                        break;

                    case RealmType.DungeonDailySpecial:
                        DungeonDailySpecialJson dDailySpecialJson = (DungeonDailySpecialJson)realm;
                        if (!CheckDungeonIsOpen(dDailySpecialJson))
                        {
                            peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_DungeonNotOpen", "", false, peerToInform);
                            return false;
                        }
                        Dictionary<int, RealmInfo> realmInfoDict = (dDailySpecialJson.dungeontype == DungeonType.Daily)
                                                                    ? playerToCheck.RealmStats.GetDungeonDailyDict()
                                                                    : playerToCheck.RealmStats.GetDungeonSpecialDict();
                        seq = dDailySpecialJson.sequence;
                        if (realmInfoDict[seq].DailyEntry+realmInfoDict[seq].ExtraEntry <= 0)
                        {
                            if (samePlayer)
                                peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_NoEntryLeft", "", false, peerToInform);
                            else
                                peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_NoEntryLeft_PartyMember",
                                    string.Format("name;{0}", playerToCheck.Name), false, peerToInform);
                            return false;
                        }
                        break;

                    //case RealmType.InvitePVP:
                    //    if (GetInvitePVPData(playerToCheck.Name) == null)
                    //        return false;
                    //    break;

                    //case RealmType.ActivityGuardWar:
                        //int heroId, defendeGuild;
                        //int[] attackGuild;

                        //HeroesHouseRules.GetGuardBattleInfo(out heroId, out defendeGuild, out attackGuild);
                        //if (defendeGuild == 0)
                        //    return false;
                        //if (attackGuild == null || attackGuild.Length == 0)
                        //    return false;
                        //if (!RealmRepo.mActivityGuardWar.ContainsKey(heroId))
                        //    return false;
                        //bool pass = false;
                        //if (guildId == defendeGuild)
                        //    pass = true;
                        //foreach (var id in attackGuild)
                        //{
                        //    if (guildId == id)
                        //        pass = true;
                        //}
                        //if (pass == false)
                        //    return false;

                        //int worldLevel = GameApplication.Instance.Leaderboard.GetWorldLevel();
                        //ActivityGuardWarJson ActivityGuardWarJson = RealmRepo.GetActivityGuardWarJson(heroId, worldLevel);
                        //if (ActivityGuardWarJson == null)
                        //    return false;
                        //break;

                    //case RealmType.ActivityGuildSMBoss:
                    //    if (guildId == 0)
                    //        return false;

                    //    GuildStats guildStats = GuildRules.GetGuildById(guildId);
                    //    GuildSMBossJson guildSMBossJson = GuildRepo.GetGuildSMBossByLvl(guildStats.SMBossLevel);
                    //    if (guildSMBossJson != null && guildStats.SMBossDmgDone >= guildSMBossJson.healthmax)
                    //    {
                    //        peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Guild_SMBossDefeated", "", false, peerToInform);
                    //        return false;
                    //    }
                    //    if (playerToCheck.SecondaryStats.GuildSMBossEntry+playerToCheck.SecondaryStats.GuildSMBossExtraEntry <= 0)
                    //    {
                    //        peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Guild_SMBossNoEntryLeft", "", false, peerToInform);
                    //        return false;
                    //    }
                    //    break;

                    //case RealmType.ActivityWorldBoss:
                    //    RealmInfo dWorldBossInfo = playerToCheck.RealmStats.GetWorldBossDict()[0];
                    //    if (dWorldBossInfo.DailyEntry + dWorldBossInfo.ExtraEntry <= 0)
                    //    {
                    //        peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_NoEntryLeft", "", false, peerToInform);
                    //        return false;
                    //    }
                    //    break;

                    //case RealmType.EliteMap:
                    //    int progressLvl = playerToCheck.GetAccumulatedLevel();
                    //    EliteMapJson eliteMapInfo = RealmRepo.GetEliteMapByPlayerLevel(progressLvl);
                    //    if (eliteMapInfo == null)
                    //        return false;
                    //    if (playerToCheck.RealmStats.GetEliteMapDailyTimeLeft(progressLvl) == 0)
                    //    {
                    //        peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_EliteMap_TimeUp", "", false, peerToInform);
                    //        return false;
                    //    }                     
                    //    break;
                }
            }
            return true;
        }

        public static bool CheckDungeonIsOpen(DungeonDailySpecialJson dDailySpecialJson)
        {
            DayOfWeek dayOfWeek = DateTime.Today.DayOfWeek;
            switch(dayOfWeek)
            {
                case DayOfWeek.Monday:      return dDailySpecialJson.isopenday1;
                case DayOfWeek.Tuesday:     return dDailySpecialJson.isopenday2;
                case DayOfWeek.Wednesday:   return dDailySpecialJson.isopenday3;
                case DayOfWeek.Thursday:    return dDailySpecialJson.isopenday4;
                case DayOfWeek.Friday:      return dDailySpecialJson.isopenday5;
                case DayOfWeek.Saturday:    return dDailySpecialJson.isopenday6;
                case DayOfWeek.Sunday:      return dDailySpecialJson.isopenday7;
            }
            return false;
        }

        public static void InitRealmStats(RealmStats realmStats)
        {
            Dictionary<int, Dictionary<DungeonDifficulty, DungeonStoryJson>> dStorySeq = RealmRepo.mDungeonStory;
            int count = dStorySeq.Count;
            Dictionary<int, DungeonStoryInfo> dStoryDict = realmStats.GetDungeonStoryDict();
            for (int i = 0; i < count; ++i)
            {
                int seq = i+1;
                if (dStoryDict.Count == i)
                {
                    dStoryDict.Add(seq, new DungeonStoryInfo(dStorySeq[seq][DungeonDifficulty.Easy].dailyentry, 0, 0, false, false, false,
                                                             false, false, false, false, false, false, "", i));
                    realmStats.DungeonStory[i] = dStoryDict[seq].ToString();
                }
            }

            Dictionary<int, List<DungeonDailySpecialJson>> dDailySeq = RealmRepo.mDungeonDaily;
            count = dDailySeq.Count;
            Dictionary<int, RealmInfo> dDailyDict = realmStats.GetDungeonDailyDict();
            for (int i = 0; i < count; ++i)
            {
                int seq = i+1;
                if (dDailyDict.Count == i)
                {
                    dDailyDict.Add(seq, new RealmInfo(dDailySeq[seq][0].dailyentry, 0, i));
                    realmStats.DungeonDaily[i] = dDailyDict[seq].ToString();
                }
            }

            Dictionary<int, List<DungeonDailySpecialJson>> dSpecialSeq = RealmRepo.mDungeonSpecial;
            count = dSpecialSeq.Count;
            Dictionary<int, RealmInfo> dSpecialDict = realmStats.GetDungeonSpecialDict();
            for (int i = 0; i < count; ++i)
            {
                int seq = i+1;
                if (dSpecialDict.Count == i)
                {
                    dSpecialDict.Add(seq, new RealmInfo(dSpecialSeq[seq][0].dailyentry, 0, i));
                    realmStats.DungeonSpecial[i] = dSpecialDict[seq].ToString();
                }
            }

            //ActivityWorldBossJson mActivityWorldBossInfo = RealmRepo.mActivityWorldBoss;
            //Dictionary<int, RealmInfo> dWorldBossDict = realmStats.GetWorldBossDict();
            //if (dWorldBossDict.Count == 0)
            //{
            //    dWorldBossDict.Add(0, new RealmInfo(mActivityWorldBossInfo.dailyentry, 0, 0));
            //    realmStats.WorldBoss[0] = dWorldBossDict[0].ToString();
            //}
        }

        #region Logging

        private static void LogCollectStarReward(GameClientPeer peer, int seq, int starCount)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("seq:{0}|", seq);
            sb.AppendFormat("starCount:{0}", starCount);

            Zealot.Logging.Client.LogClasses.DungeonCollectStarReward sysLog = new Zealot.Logging.Client.LogClasses.DungeonCollectStarReward();
            sysLog.userId = peer.mUserId;
            sysLog.charId = peer.GetCharId();
            sysLog.message = sb.ToString();
            sysLog.seq = seq;
            sysLog.starCount = starCount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(sysLog);
        }

        private static void LogStoryAddExtraEntry(GameClientPeer peer, int realmId, int dailyExtraEntry, int entryAfter)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("realmId:{0}|", realmId);
            sb.AppendFormat("dailyExtraEntry:{0}|", dailyExtraEntry);
            sb.AppendFormat("entryBefore:{0}|", entryAfter - 1);
            sb.AppendFormat("entryAfter:{0}", entryAfter);  

            Zealot.Logging.Client.LogClasses.DungeonAddExtraEntry sysLog = new Zealot.Logging.Client.LogClasses.DungeonAddExtraEntry();
            sysLog.userId = peer.mUserId;
            sysLog.charId = peer.GetCharId();
            sysLog.message = sb.ToString();
            sysLog.realmId = realmId;
            sysLog.dailyExtraEntry = dailyExtraEntry;
            sysLog.extraEntryBefore = entryAfter - 1;
            sysLog.extraEntryAfter = entryAfter;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(sysLog);
        }

        #endregion

        /*private bool EnterRealmByRealmIDWithRet(int realmid, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return false;
            if (mRoom.RealmID == realmid)
                return false;
            RealmJson realm = RealmRepo.GetInfoById(realmid);
            if (realm == null || realm.type == Zealot.Common.RealmType.RealmWorld)
                return false;
            LevelJson level = LevelRepo.GetInfoById(realm.level);
            if (level == null)
                return false;

            int total_lvl = player.GetAccumulatedLevel();
            int viplevel = player.PlayerSynStats.vipLvl;
            Room room;
            RealmType realmtype = realm.type;
            RealmController realmcontroller;
            switch (realmtype)
            {
                case RealmType.ActivityQianKun:
                    ActivityQianKunJson ActivityQianKunInfo = RealmRepo.GetQianKunInfoByRank(rank);
                    if (ActivityQianKunInfo == null)
                        return false;
                    if (player.RealmInventoryStats.GetDailyLeftQianKun(total_lvl, viplevel, player.QuestStats) <= 0)
                        return false;
                    long qiankun_interval;
                    long qiankun_time = RealmRepo.GetNextQianKun(DateTime.Now, out qiankun_interval);
                    if (ActivityQianKunInfo.preparation * 1000 < (qiankun_interval - qiankun_time)) //pass the preparation time, can't enter.
                        return false;
                    GameApplication.Instance.GameCache.TryGetRoomByID(realmid, out room, ActivityQianKunInfo.maxplayer);
                    if (room != null)
                        peer.TransferRoom(room.Guid, level.unityscene);
                    else
                        peer.CreateRealm(realmid, level.unityscene);
                    break;
                case RealmType.ActivityShuangFei:
                    ActivityShuangFeiJson ActivityShuangFeiInfo = RealmRepo.GetShuangFeiInfoByRank(rank);
                    if (ActivityShuangFeiInfo == null)
                        return false;
                    if (player.RealmInventoryStats.GetDailyLeftShuangFei(total_lvl, viplevel, player.QuestStats) <= 0)
                        return false;
                    long shuangfei_interval;
                    long shuangfei_time = RealmRepo.GetNextShuangFei(DateTime.Now, out shuangfei_interval);
                    if (ActivityShuangFeiInfo.preparation * 1000 < (shuangfei_interval - shuangfei_time)) //pass the preparation time, can't enter.
                        return false;
                    GameApplication.Instance.GameCache.TryGetRoomByID(realmid, out room, ActivityShuangFeiInfo.maxplayer);
                    if (room != null)
                        peer.TransferRoom(room.Guid, level.unityscene);
                    else
                        peer.CreateRealm(realmid, level.unityscene);
                    break;
                case RealmType.ActivityGuZhanChang:
                    ActivityGuZhanChangJson ActivityGuZhanChangInfo = RealmRepo.GetGuZhanChangInfoByRank(rank);
                    if (ActivityGuZhanChangInfo == null)
                        return false;
                    if (player.RealmInventoryStats.GetDailyLeftGuZhanChang(total_lvl, DateTime.Now, player.QuestStats) == 0)
                        return false;
                    GameApplication.Instance.GameCache.TryGetRoomByID(realmid, out room, 0);
                    if (room != null)
                        peer.TransferRoom(room.Guid, level.unityscene);
                    else
                        peer.CreateRealm(realmid, level.unityscene);
                    break;
                case RealmType.ActivityResourceMap:
                case RealmType.ActivityTianZiWar:
                case RealmType.ActivityYiZuZhan:
                    if (player.PlayerSynStats.countryType == (byte)CountryType.None || player.PlayerSynStats.guildName == "")
                        return false;
                    if (realmtype == RealmType.ActivityResourceMap)
                    {
                        if (total_lvl < RealmRepo.mActivityResourceMap.requirelvlmin)
                            return false;
                    }
                    else if (realmtype == RealmType.ActivityTianZiWar)
                    {
                        if (total_lvl < RealmRepo.mActivityTianZiWar.requirelvlmin)
                            return false;
                    }
                    else if (realm.type == RealmType.ActivityYiZuZhan)
                    {
                        if (total_lvl < RealmRepo.mActivityYiZuZhan.requirelvlmin)
                            return false;
                        if (RealmRepo.mActivityYiZuZhan.dailyentry <= player.RealmInventoryStats.YiZuZhan)
                            return false;
                    }
                    GameApplication.Instance.GameCache.TryGetRoomByID(realmid, out room, 0);
                    if (room == null)
                    {
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_activity_NotOpen", "", false, peer);
                        return false;
                    }              
                    realmcontroller = ((Game)room).controller.mRealmController;
                    if (realmcontroller == null || !realmcontroller.CanReconnect())
                    {
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_activity_End", "", false, peer);
                        return false;
                    }
                    peer.TransferRoom(room.Guid, level.unityscene);
                    break;
                case RealmType.ActivityTianZiShouChang:
                    byte my_countrytype = player.PlayerSynStats.countryType; 
                    if (my_countrytype == (byte)CountryType.None || player.PlayerSynStats.guildName == "")
                        return false;
                    if (my_countrytype != CountryRules.GetKingCountryType())
                        return false;
                    GameApplication.Instance.GameCache.TryGetRoomByID(realmid, out room, 0);
                    if (room == null)
                    {
                        if (!player.PlayerSynStats.isTianZi || player.RealmInventoryStats.TianZiShouChang)
                            return false;
                        ActivityTianZiShouChangJson ActivityTianZiShouChangJson = RealmRepo.mActivityTianZiShouChang;
                        ushort itemid = (ushort)ActivityTianZiShouChangJson.summonitembind;
                        if (!peer.RemoveItemFromInventory(itemid, 1))
                        {
                            itemid = (ushort)ActivityTianZiShouChangJson.summonitem;
                            if (!peer.RemoveItemFromInventory(itemid, 1))
                                return false;
                        }
                        player.RealmInventoryStats.TianZiShouChang = true;
                        peer.CreateRealm(realmid, level.unityscene);
                    }
                    else
                    {
                        realmcontroller = ((Game)room).controller.mRealmController;
                        if (realmcontroller == null || !realmcontroller.CanReconnect())
                        {
                            peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_activity_End", "", false, peer);
                            return false;
                        }
                        peer.TransferRoom(room.Guid, level.unityscene);
                    }
                    break;
                case RealmType.RealmGuild:
                    int guildId = player.SecondaryStats.guildId;
                    if(guildId == 0)
                        return false;
                    string guild_roomguild = GuildRules.GetGuildRealmGuild(guildId, realmid, level.unityscene);
                    if (string.IsNullOrEmpty(guild_roomguild))
                        return false;
                    peer.TransferRoom(guild_roomguild, level.unityscene);
                    break;
                case RealmType.VIPRoom:
                    VIPRoomJson VIPRoomJson = (VIPRoomJson)realm;
                    if (viplevel < VIPRoomJson.vip)
                        return false;
                    if (peer.SpendDiamonds(VIPRoomJson.diamond))
                    {
                        GameApplication.Instance.GameCache.TryGetRoomByID(realmid, out room, 0);
                        peer.TransferRoom(room.Guid, level.unityscene);
                    }
                    break;
                case RealmType.ActivityLeiYinTa:
                    ActivityLeiYinTaJson ActivityLeiYinTaJson = RealmRepo.mActivityLeiYinTa;
                    if (ActivityLeiYinTaJson == null)
                        return false;
                    if (ActivityLeiYinTaJson.dailyentry <= player.RealmInventoryStats.LeiYinTa || ActivityLeiYinTaJson.requirelvlmin > total_lvl)
                        return false;
                    GameApplication.Instance.GameCache.TryGetRoomByID(realmid, out room, 0);
                    if (room == null)
                    {
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_activity_NotOpen", "", false, peer);
                        return false;
                    }
                    realmcontroller = ((Game)room).controller.mRealmController;
                    if (realmcontroller == null || !realmcontroller.CanReconnect())
                    {
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_activity_End", "", false, peer);
                        return false;
                    }
                    peer.TransferRoom(room.Guid, level.unityscene);
                    break;
                default:
                    return false;
            }
            return true;
        }*/
    }
}
