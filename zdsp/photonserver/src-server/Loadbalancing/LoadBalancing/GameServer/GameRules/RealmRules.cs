using ExitGames.Concurrency.Fibers;
using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;
using Zealot.Common.RPC;
using Zealot.Common.Entities;
using Zealot.Server.Entities;
using Zealot.Repository;

namespace Zealot.Server.Rules
{
    enum RealmRetCode : byte
    {
        RealmRequiredlvl= 0,
        RealmGoldNotEnough,
        RealmEnterFailed,
        RealmEnterSuccess,
    }

    enum RealmPartyEnterResponse : byte
    {
        Pending = 0,
        Ready,
        Cancel
    }

    class RealmPartyMember
    {
        public string Name = "";
        public RealmPartyEnterResponse Response = RealmPartyEnterResponse.Pending;
    }

    class RealmParty
    {
        public Dictionary<string, RealmPartyMember> Members = new Dictionary<string, RealmPartyMember>();
        public RealmJson RealmInfo = null;
        public DateTime DTRequested;

        public RealmParty(RealmJson realmInfo)
        {
            RealmInfo = realmInfo;
            DTRequested = DateTime.Now;
        }

        public void AddMember(string name)
        {
            Members.Add(name, new RealmPartyMember() { Name = name });
        }
    }

    class InvitePVPData
    {
        public string GUID = "";
    }

    static class RealmRules
    {
        public static readonly int UpdateInterval = 5000;

        public static Dictionary<int, RealmParty> RealmPartyEnterStatus= null; // realmId <- RealmParty
        private static readonly PoolFiber executionFiber;

        private static Dictionary<string, InvitePVPData> InvitePVPList = new Dictionary<string, InvitePVPData>();

        static RealmRules()
        {
            executionFiber = GameApplication.Instance.executionFiber;
        }

        public static void Init()
        {
            RealmPartyEnterStatus = new Dictionary<int, RealmParty>();
        }

        public static void Update()
        {
            List<int> partyIds = RealmPartyEnterStatus.Keys.ToList();
            int count = partyIds.Count;
            for (int i = 0; i < count; ++i)
            {
                int partyId = partyIds[i];
                RealmParty realmParty = RealmPartyEnterStatus[partyId];
                TimeSpan timeElapsed = DateTime.Now.Subtract(realmParty.DTRequested);
                if (timeElapsed.TotalSeconds > 6)
                    RealmPartyEnterStatus.Remove(partyId);
            }
            executionFiber.Schedule(Update, UpdateInterval); // Reschedule next interval
        }

        public static RealmRetCode TeleportToLevelInPos(string levelName, RPCPosition pos, bool deductGold, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || !player.IsAlive())
                return RealmRetCode.RealmEnterFailed;

            RealmWorldJson realmWorldJson = RealmRepo.GetWorldByName(levelName);
            if (realmWorldJson == null) // Check if level name is realm world
                return RealmRetCode.RealmEnterFailed;
 
            int reqLvl = realmWorldJson.reqlvl;
            if (reqLvl > player.GetAccumulatedLevel())
            {
                string param = string.Format("level;{0}", player.GetLocalizedProgressLevelMin(reqLvl));
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_map_FailLvl", param, false, peer);
                return RealmRetCode.RealmRequiredlvl;
            }

            if (deductGold)
            {
                int amt = (realmWorldJson.sequence - 1) * 1000 + 2000;
                if (amt < 0) amt = 0;
                if (!player.IsCurrencySufficient(CurrencyType.Gold, amt))
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_map_FailGold", "", false, peer);
                    return RealmRetCode.RealmGoldNotEnough;
                }
                else
                    player.DeductGold(amt, true, true, "Teleport");
            }

            if (levelName == player.mInstance.mCurrentLevelName)
            {
                if (pos != null)
                    peer.ZRPC.CombatRPC.TeleportSetPos(pos, peer);
                else
                {
                    player.mInstance.SetPlayerPosToSpawnPos(player);
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

        public static string CreateRealmById(int realmId, bool logAI, bool checkAll, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || !player.IsAlive())
                return "";
            RealmJson realm = null;
            string levelScene = "";
            if (!GetRealmInfo(realmId, out realm, out levelScene))
                return "";
            if (!CheckRealmRequirement(realm, player, peer, checkAll))
                return "";

            return peer.CreateRealm(realmId, levelScene, logAI);
        }

        public static string EnterRealmById(int realmId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || !player.IsAlive())
                return "";
            RealmJson realm = null;
            string levelScene = "";
            if (!GetRealmInfo(realmId, out realm, out levelScene))
                return "";
            if (!CheckRealmRequirement(realm, player, peer, true))
                return "";

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
                //    string partyName = peer.GetPartyName();
                //    PartyInfoStats partyStats = string.IsNullOrEmpty(partyName) ? null : GameRules.PartyController.GetPartyInfoStatsByPartyName(partyName);
                //    if (partyStats == null || partyStats.MemberCount() == 1)
                //    {
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
                //                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_activity_End", "", false, peer);
                //                    return;
                //                }
                //            }
                //            peer.TransferRoom(roomGuid, levelScene);
                //        }
                //        else
                //            peer.CreateRealm(realmId, levelScene);
                //    }
                //    else
                //        EnterRealmWithPartyRequest(partyStats, realm, peer);
                //    break;

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
            return roomGuid;
        }

        public static void InspectMode(int realmId, GameClientPeer peer)
        {
            //Player player = peer.mPlayer;
            //if (player == null || !player.IsAlive())
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

        public static void EnterRealmCheckAndLeaveParty(Player player, bool includeHero)
        {
            int partyId = player.PlayerSynStats.Party;
            PartyStats partyStats = PartyRules.GetPartyById(partyId);
            if (partyStats != null && partyStats.MemberCount(includeHero) > 1)
                PartyRules.LeaveParty(partyId, player.Name, LeavePartyReason.Self);
        }

        public static void DungeonEnterRequest(int realmId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            string myName = peer.mChar;
            if (player == null || !player.IsAlive())
                return;
            RealmJson realm = null;
            string levelScene = "";
            if (!GetRealmInfo(realmId, out realm, out levelScene))
                return;
            if (!CheckRealmRequirement(realm, player, peer, false) || realm.type != RealmType.Dungeon)
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
                int memberCount = partyStats.MemberCount();
                DungeonType dungeonType = ((DungeonJson)realm).dungeontype;
                switch (dungeonType)
                {
                    default:
                        if (realm.maxplayer == 1)
                        {
                            PartyRules.LeaveParty(partyId, player.Name, LeavePartyReason.Self);
                            peer.CreateRealm(realmId, levelScene);
                        }
                        else
                        {
                            if (memberCount >= realm.minplayer)
                            {
                                //EnterRealmWithPartyRequest(partyStats, memberCount, realm, peer);
                                // TODO: For testing, remove later
                                RealmParty realmParty = new RealmParty(realm);
                                Dictionary<string, PartyMember>.ValueCollection partyMembers = partyStats.GetPartyMemberList().Values;
                                foreach (var member in partyMembers)
                                    realmParty.AddMember(member.name);
                                EnterRealmWithParty(partyStats.partyId, realmParty);
                                // end testing
                            }
                            else
                                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_LessThanMinPlayer", "", false, peer);
                        }
                        break;
                }
            }
        }

        public static void EnterRealmWithPartyRequest(PartyStats partyStats, int memberCount, RealmJson realm, GameClientPeer requestPeer)
        {
            string requestPeerName = requestPeer.mChar;
            if (partyStats.IsLeader(requestPeerName))
            {
                requestPeer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_OnlyPartyLeaderAllow", "", false, requestPeer);
                return;
            }
            if (realm.maxplayer != 0 && memberCount > realm.maxplayer)
            {
                requestPeer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_PlayerCountOver", "", false, requestPeer);
                return;
            }

            // Party members availability check
            Dictionary<string, PartyMember>.ValueCollection partyMembers = partyStats.GetPartyMemberList().Values;
            List<GameClientPeer> memberPeerList = new List<GameClientPeer>();            
            foreach (var member in partyMembers)
            {
                string memberName = member.name;
                if (memberName != requestPeerName)
                {
                    GameClientPeer memberPeer = GameApplication.Instance.GetCharPeer(memberName);
                    Player memberPlayer = memberPeer == null ? null : memberPeer.mPlayer;
                    if (memberPlayer == null || !memberPlayer.IsAlive())
                    {
                        requestPeer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_TransferringRealm",
                            string.Format("name;{0}", memberName), false, requestPeer);
                        return;
                    }
                    if (!CheckRealmRequirement(realm, memberPlayer, requestPeer, false))
                        return;
                    memberPeerList.Add(memberPeer);
                }
                else
                    memberPeerList.Add(requestPeer);
            }

            // Send RPC to each party member to query enter realm
            int realmId = realm.id;
            RealmParty realmParty = new RealmParty(realm);
            int count = memberPeerList.Count;
            for (int i = 0; i < count; ++i)
            {
                GameClientPeer memberPeer = memberPeerList[i];
                bool hasEntry = true;
                //if (memberPeer.mChar != requestPeerName) // Check entry of other members
                //{
                //    Player memberPlayer = memberPeer.mPlayer;
                //    if (realm.type == RealmType.DungeonDailySpecial)
                //    {
                //        DungeonDailySpecialJson dDailySpecialJson = (DungeonDailySpecialJson)realm;
                //        Dictionary<int, RealmInfo> realmInfoDict = (dDailySpecialJson.dungeontype == DungeonType.Daily)
                //                                                    ? memberPlayer.RealmStats.GetDungeonDailyDict()
                //                                                    : memberPlayer.RealmStats.GetDungeonSpecialDict();
                //        int seq = dDailySpecialJson.sequence;
                //        hasEntry = (realmInfoDict[seq].DailyEntry + realmInfoDict[seq].ExtraEntry > 0);
                //    }
                //}
                memberPeer.ZRPC.CombatRPC.Ret_DungeonEnterConfirmResult(realmId, hasEntry, "", memberPeer);
                realmParty.AddMember(memberPeer.mChar);
            }
            RealmPartyEnterStatus[partyStats.partyId] = realmParty;
        }

        public static void EnterRealmWithPartyResponse(int realmId, byte status, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || !player.IsAlive())
                return;

            string playerName = player.Name;
            int partyId = player.PlayerSynStats.Party;
            RealmParty realmParty = null;
            if (RealmPartyEnterStatus.TryGetValue(partyId, out realmParty))
            {
                bool canEnter = true;
                Dictionary<string, RealmPartyMember>.ValueCollection realmPartyMembers = realmParty.Members.Values;
                foreach (RealmPartyMember realmPartyMember in realmPartyMembers)
                {
                    string memberName = realmPartyMember.Name;
                    if (memberName == playerName) // Is sender name
                        realmPartyMember.Response = (RealmPartyEnterResponse)status;
                    else if ((RealmPartyEnterResponse)status == RealmPartyEnterResponse.Cancel)
                    {
                        GameClientPeer memberPeer = GameApplication.Instance.GetCharPeer(memberName);
                        if (memberPeer != null)
                            memberPeer.ZRPC.CombatRPC.Ret_DungeonEnterConfirmResult(realmId, true, memberName, memberPeer);
                    }
                    if (realmPartyMember.Response != RealmPartyEnterResponse.Ready)
                        canEnter = false;
                }
                if (canEnter)
                {              
                    EnterRealmWithParty(partyId, realmParty);
                    RealmPartyEnterStatus.Remove(partyId);
                }
                else if (status == (byte)RealmPartyEnterResponse.Cancel)
                    RealmPartyEnterStatus.Remove(partyId);
            }
        }

        public static void EnterRealmWithParty(int partyId, RealmParty realmParty)
        {
            PartyStats partyStats = PartyRules.GetPartyById(partyId);
            if (partyStats == null)
                return;

            string partyLeader = partyStats.leader;
            GameClientPeer leaderPeer = GameApplication.Instance.GetCharPeer(partyLeader);
            Player leaderPlayer = leaderPeer == null ? null : leaderPeer.mPlayer;
            if (leaderPlayer == null || !leaderPlayer.IsAlive() || !realmParty.Members.ContainsKey(partyLeader))
                return;

            RealmJson realm = realmParty.RealmInfo;
            RealmType realmType = realm.type;
            string roomGuid = "";
            switch (realmType)
            {
                default:
                    roomGuid = CreateRealmById(realm.id, false, true, leaderPeer);
                    break;
            }
            if (string.IsNullOrEmpty(roomGuid))
                return;

            Dictionary<string, PartyMember> partyMembers = partyStats.GetPartyMemberList();
            Dictionary<string, RealmPartyMember>.ValueCollection realmPartyMembers = realmParty.Members.Values;
            foreach (RealmPartyMember realmPartyMember in realmPartyMembers)
            {
                string memberName = realmPartyMember.Name;
                if (memberName == partyLeader || !partyMembers.ContainsKey(memberName))
                    continue;
                GameClientPeer memberPeer = GameApplication.Instance.GetCharPeer(memberName);
                Player memberPlayer = memberPeer == null ? null : memberPeer.mPlayer;
                if (memberPlayer == null || !memberPlayer.IsAlive() || !CheckRealmRequirement(realm, memberPlayer, leaderPeer, false))
                    continue;

                memberPeer.TransferRoom(roomGuid, LevelRepo.GetSceneById(realm.level));
            }
        }

        public static void DungeonAutoClear(int realmId, bool clearAll, GameClientPeer peer)
        {
            //bool isSuccess = true;
            Player player = peer.mPlayer;
            if (player == null || !player.IsAlive())
                return;
            RealmJson realm = null;
            string levelScene = "";
            if (!GetRealmInfo(realmId, out realm, out levelScene))
                return;
            if (!CheckRealmRequirement(realm, player, peer, true) || realm.type != RealmType.Dungeon)
                return;

            DungeonJson dungeonJson = (DungeonJson)realm;
            int lootCount = player.RealmStats.DecreaseLootRewardCount(dungeonJson, clearAll);
            if (lootCount == 0)
                return;

            // Parse loot link ids
            string[] lootLinkIds = dungeonJson.lootdisplayids.Split(';');
            List<LootLink> lootLinks = new List<LootLink>();
            int lootLinkIdsLen = lootLinkIds.Length;
            for (int i = 0; i < lootLinkIdsLen; ++i)
            {
                int lootLinkId;
                if (int.TryParse(lootLinkIds[i], out lootLinkId))
                {
                    LootLink lootLink = LootRepo.GetLootLink(lootLinkId);
                    if (lootLink != null)
                        lootLinks.Add(lootLink);
                }
            }

            // Send Loot Rewards
            for (int i = 0; i < lootCount; ++i)
            {
                Dictionary<int, int> itemsToAdd = new Dictionary<int, int>();
                Dictionary<CurrencyType, int> currencyToAdd = new Dictionary<CurrencyType, int>();
                int lootLinksCnt = lootLinks.Count;
                for (int j = 0; j < lootLinksCnt; ++j)
                    LootRules.GenerateLootItems(lootLinks[j].gids, itemsToAdd, currencyToAdd);

                List<ItemInfo> itemInfoList = LootRules.GetItemInfoListToAdd(itemsToAdd, true);
                player.Slot.mInventory.AddItemsToInventoryMailIfFail(itemInfoList, currencyToAdd, "DungeonAutoClear");
            }

            //peer.ZRPC.CombatRPC.Ret_RaidReward(isSuccess, rewardGrp, GameUtils.SerializeItemInfoList(itemInfos), peer); // Return reward result
        }

        public static bool GetRealmInfo(int realmId, out RealmJson realm, out string levelScene)
        {
            RealmJson realmInfo = RealmRepo.GetInfoById(realmId);
            if (realmInfo != null && realmInfo.type != RealmType.World)
            {
                realm = realmInfo;
                LevelJson level = LevelRepo.GetInfoById(realm.level);
                if (level != null)
                {
                    levelScene = level.unityscene;
                    return true;
                }
            }
            realm = null;
            levelScene = "";
            return false;
        }

        public static bool CheckRealmRequirement(RealmJson realm, Player playerToCheck, GameClientPeer peerToInform, bool checkAll)
        {
            bool samePlayer = playerToCheck.Slot == peerToInform;
            if (!playerToCheck.IsInWorld)
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
                //int seq = 0;
                //int guildId = playerToCheck.SecondaryStats.guildId;
                //switch (realm.type)
                //{
                    //case RealmType.Dungeon:
                    //    DungeonJson dStoryJson = (DungeonJson)realm;
                    //    Dictionary<int, DungeonStoryInfo> dStoryInfoDict = playerToCheck.RealmStats.GetDungeonStoryDict();
                    //    seq = dStoryJson.sequence;
                    //    if (dStoryInfoDict[seq].DailyEntry+dStoryInfoDict[seq].ExtraEntry <= 0)
                    //    {
                    //        if (samePlayer)
                    //            peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_NoEntryLeft", "", false, peerToInform);
                    //        else
                    //            peerToInform.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Dun_NoEntryLeft_PartyMember",
                    //                string.Format("name;{0}", playerToCheck.Name), false, peerToInform);
                    //        return false;
                    //    }
                    //    break;
                //}
            }
            return true;
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

        public static bool CheckDungeonIsOpen(DungeonJson dungeonJson)
        {
            DayOfWeek dayOfWeek = DateTime.Today.DayOfWeek;
            switch(dayOfWeek)
            {
                case DayOfWeek.Monday:      return dungeonJson.isopenday1;
                case DayOfWeek.Tuesday:     return dungeonJson.isopenday2;
                case DayOfWeek.Wednesday:   return dungeonJson.isopenday3;
                case DayOfWeek.Thursday:    return dungeonJson.isopenday4;
                case DayOfWeek.Friday:      return dungeonJson.isopenday5;
                case DayOfWeek.Saturday:    return dungeonJson.isopenday6;
                case DayOfWeek.Sunday:      return dungeonJson.isopenday7;
            }
            return false;
        }

        #region Logging

        //private static void LogCollectStarReward(GameClientPeer peer, int seq, int starCount)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendFormat("seq:{0}|", seq);
        //    sb.AppendFormat("starCount:{0}", starCount);

        //    Zealot.Logging.Client.LogClasses.DungeonCollectStarReward sysLog = new Zealot.Logging.Client.LogClasses.DungeonCollectStarReward();
        //    sysLog.userId = peer.mUserId;
        //    sysLog.charId = peer.GetCharId();
        //    sysLog.message = sb.ToString();
        //    sysLog.seq = seq;
        //    sysLog.starCount = starCount;
        //    var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(sysLog);
        //}

        //private static void LogStoryAddExtraEntry(GameClientPeer peer, int realmId, int dailyExtraEntry, int entryAfter)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendFormat("realmId:{0}|", realmId);
        //    sb.AppendFormat("dailyExtraEntry:{0}|", dailyExtraEntry);
        //    sb.AppendFormat("entryBefore:{0}|", entryAfter - 1);
        //    sb.AppendFormat("entryAfter:{0}", entryAfter);  

        //    Zealot.Logging.Client.LogClasses.DungeonAddExtraEntry sysLog = new Zealot.Logging.Client.LogClasses.DungeonAddExtraEntry();
        //    sysLog.userId = peer.mUserId;
        //    sysLog.charId = peer.GetCharId();
        //    sysLog.message = sb.ToString();
        //    sysLog.realmId = realmId;
        //    sysLog.dailyExtraEntry = dailyExtraEntry;
        //    sysLog.extraEntryBefore = entryAfter - 1;
        //    sysLog.extraEntryAfter = entryAfter;
        //    var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(sysLog);
        //}

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
