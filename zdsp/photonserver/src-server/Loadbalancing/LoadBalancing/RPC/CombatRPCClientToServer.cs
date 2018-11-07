using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zealot.Common.RPC;
using Zealot.Entities;

namespace Photon.LoadBalancing.GameServer
{
    using Kopio.JsonContracts;
    using Zealot.Common;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;
    using Zealot.RPC;
    using Zealot.Server.Rules;
    using Zealot.Repository;
    using TopUp;

    public partial class GameLogic
    {
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnEnterPortal)]
        public void OnEnterPortal(string entryName, GameClientPeer peer) //Peter, TODO: next time should not send string
        {
            Player player = peer.mPlayer;
            if (player != null && !player.Destroyed)
            {
                PortalEntryData entryData;
                if (PortalInfos.mEntries.TryGetValue(entryName, out entryData))
                {
                    if (entryData.mLevel != currentlevelname || (entryData.mPosition - player.Position).magnitude > 30)
                        return;
                    GameRules.TeleportToPortalExit(peer.mPlayer, entryData.mExitName);
                }
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnEnterPortal)]
        public void OnEnterPortalProxy(object[] args)
        {
            OnEnterPortal((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnEnterSafeZone)]
        public void OnSafeZone(bool enter, int id, int type, GameClientPeer peer)//string safeZoneName
        {
            Player player = peer.mPlayer;
            if (player != null && !player.Destroyed)
            {
                switch (type)
                {
                    case 0:
                        player.CheckSafeZoneSphere(enter, id);
                        break;
                    case 1:
                        player.CheckSafeZoneBox(enter, id);
                        break;
                    default:
                        break;
                }
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnEnterSafeZone)]
        public void OnSafeZoneProxy(object[] args)
        {
            OnSafeZone((bool)args[0], (int)args[1], (int)args[2], (GameClientPeer)args[3]);
        }

        // World map use this to transfer
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnClickWorldMap)]
        public void OnClickWorldMap(string levelName, GameClientPeer peer)
        {
            if (currentlevelname == levelName)
                RealmRules.TeleportToLevelInPos(levelName, null, true, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnClickWorldMap)]
        public void OnClickWorldMapProxy(object[] args)
        {
            OnClickWorldMap((string)args[0], (GameClientPeer)args[1]);
        }

        // Quest and others use this to transfer
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnTeleportToLevel)]
        public void OnTeleportToLevel(string levelName, GameClientPeer peer)
        {
            if (currentlevelname != levelName)
                RealmRules.TeleportToLevelInPos(levelName, null, false, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnTeleportToLevel)]
        public void OnTeleportToLevelProxy(object[] args)
        {
            OnTeleportToLevel((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnTeleportToLevelAndPos)]
        public void OnTeleportToLevelAndPos(string levelName, RPCPosition pos, int questid, GameClientPeer peer)
        {
            if (currentlevelname == levelName)
                peer.mPlayer.Position = pos.ToVector3();
            else
                RealmRules.TeleportToLevelInPos(levelName, pos, false, peer);

            if (questid != -1)
            {
                peer.QuestController.UpdateQuestEventStatus(questid);
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnTeleportToLevelAndPos)]
        public void OnTeleportToLevelAndPosProxy(object[] args)
        {
            OnTeleportToLevelAndPos((string)args[0], (RPCPosition)args[1], (int)args[2], (GameClientPeer)args[3]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnTeleportToPosInLevel)]
        public void OnTeleportToPosInLevel(RPCPosition pos, GameClientPeer peer) //enter lobby
        {
            Player player = peer.mPlayer;
            if (player != null && !player.Destroyed)
            {
                player.Position = pos.ToVector3();
                ZRPC.CombatRPC.TeleportSetPos(pos, peer);               
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnTeleportToPosInLevel)]
        public void OnTeleportToPosInLevelProxy(object[] args)
        {
            OnTeleportToPosInLevel((RPCPosition)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ExitGame)]
        public void ExitGame(GameClientPeer peer) //enter lobby
        {
            peer.ExitGame();
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ExitGame)]
        public void ExitGameProxy(object[] args)
        {
            ExitGame((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RespawnOnSpot)]
        public void RespawnOnSpot(bool force, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.Destroyed || player.IsAlive())
                return;
            if (force)
            {
                player.RespawnOnSpot();
                return;
            }

            RespawnJson mRespawnInfo;
            RealmController realmController = player.mInstance.mRealmController;

            int respawnId = realmController.mRealmInfo.respawn;
            mRespawnInfo = RespawnRepo.GetRespawnDataByID(respawnId);

            List<ItemInfo> itemList = RespawnRepo.GetItemListByID(respawnId);
            bool itemRes = true;
            if(itemList.Count > 0)
            {
                itemRes = DeathRules.IsEnoughRespawnItems(itemList, peer);
                if(itemRes == false)
                {
                    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Death_InsufficientReviveItem"), "", false, this);
                }
            }

            List<CurrencyInfo> currencyList = RespawnRepo.GetCurrencyListByID(respawnId);
            bool currencyRes = true;
            if(currencyList.Count > 0)
            {
                currencyRes = DeathRules.IsEnoughRespawnCurrency(currencyList, peer);
                if(currencyRes == false)
                {
                    ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Death_InsufficientReviveCurrency"), "", false, this);
                }
            }

            // Respawn fails if either not enough items or currency
            if(itemRes == false || currencyRes == false)
            {
                return;
            }

            // Deduct items and currency
            DeathRules.UseRespawnItem(itemList, peer);
            DeathRules.DeductRespawnCurrency(currencyList, peer);

            int mapId = player.mInstance.mCurrentLevelID;
            DeathRules.LogDeathRespawnType("Button", mapId, peer);

            player.RespawnOnSpot();
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RespawnOnSpot)]
        public void RespawnOnSpotProxy(object[] args)
        {
            RespawnOnSpot((bool)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RespawnAtCity)]
        public void RespawnAtCity(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.Destroyed || player.IsAlive())
                return;
            player.RespawnAtCity();
            int mapId = player.mInstance.mCurrentLevelID;
            DeathRules.LogDeathRespawnType("FreeRevive", mapId, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RespawnAtCity)]
        public void RespawnAtCityProxy(object[] args)
        {
            RespawnAtCity((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RespawnAtSafeZone)]
        public void RespawnAtSafeZone(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.Destroyed || player.IsAlive())
                return;

            player.RespawnAtSafezone();
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RespawnAtSafeZone)]
        public void RespawnAtSafeZoneProxy(object[] args)
        {
            RespawnAtSafeZone((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RespawnAtSafeZoneWithCost)]
        public void RespawnAtSafeZoneWithCost(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.Destroyed || player.IsAlive())
                return;

            int mapId = player.mInstance.mCurrentLevelID;
            //DeathRules.LogDeathRespawnType("Button", mapId, peer);

            int viplevel = 0;

            // VIP count
            int vipFreeLeft = 0; // VIPRepo.GetVIPPrivilege("FreeRevive", viplevel) - player.SecondaryStats.FreeReviveOnSpot;

            // Item count
            ushort itemid = (ushort)GameConstantRepo.GetConstantInt("Respawn_Item");
            int itemCount = peer.GetTotalStackCountByItemID(itemid);

            // Gold ocunt
            int goldCost = GameConstantRepo.GetConstantInt("Respawn_GoldIngot");
            int gold = player.SecondaryStats.Gold;
            int lockGold = player.SecondaryStats.bindgold;
            int lockgoldBef = lockGold;
            int goldBef = gold;
            if (vipFreeLeft > 0)
            {
                player.SecondaryStats.FreeReviveOnSpot += 1;
                peer.CharacterData.FreeReviveOnSpot += 1;
                player.RespawnAtSafezone();

                int newVipFreeLeft = 0; // VIPRepo.GetVIPPrivilege("FreeRevive", viplevel) - player.SecondaryStats.FreeReviveOnSpot;
                //DeathRules.LogDeathRespawnType("FreeRevive", mapId, peer);
                //DeathRules.LogDeathRespawnFree("FreeRevive", 1, vipFreeLeft, newVipFreeLeft, peer);
            }
            else if (peer.RemoveItemFromInventory(itemid, 1, "Respawn_SafeZone"))
            {
                player.RespawnAtSafezone();
                //DeathRules.LogDeathRespawnType("ItemRevive", mapId, peer);
                //DeathRules.LogDeathRespawnItem("ItemRevive", itemid, itemCount, peer);
            }
            else if (player.SecondaryStats.IsGoldEnough(goldCost))
            {
                if (lockGold >= goldCost)
                {
                    lockGold = goldCost;
                    gold = 0;
                }
                else
                {
                    gold = goldCost - goldCost;
                }

                // Case 1: Plenty of lock gold
                if (lockGold >= goldCost || (lockGold < goldCost && lockGold > 0))
                {
                    if (peer.mPlayer.DeductGold(goldCost, true, true, "Respawn_SafeZone"))
                    {
                        player.RespawnAtSafezone();
                    }
                }
                // Case 2: 0 lock gold
                else if (lockGold == 0)
                {
                    if (peer.mPlayer.DeductGold(goldCost, false, true, "Respawn_SafeZone"))
                    {
                        player.RespawnAtSafezone();
                    }
                }

                int lockGoldAft = player.SecondaryStats.bindgold;
                int goldAft = player.SecondaryStats.Gold;
                //DeathRules.LogDeathRespawnType("GoldRevive", mapId, peer);
                //DeathRules.LogDeathRespawnFree("LockGoldRevive", lockGold, lockgoldBef, lockGoldAft, peer);
                //DeathRules.LogDeathRespawnFree("GoldRevive", gold, goldBef, goldAft, peer);
            }
            else
            {
                ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Death_ReviveFailed"), "", false, this);
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RespawnAtSafeZoneWithCost)]
        public void RespawnAtSafeZoneWithCostProxy(object[] args)
        {
            RespawnAtSafeZoneWithCost((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ClientSendChatMessage)]
        public async Task ClientSendChatMessage(byte msgType, string message, string whisperTo, bool isVoiceChat, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            if (DateTime.Now < peer.mDTMute)
            {
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Mute", "", true, peer);
                return;
            }
            PlayerSynStats playerSynStats = player.PlayerSynStats;
            if (msgType == (byte)MessageType.Whisper)
            {
                GameClientPeer recipient = GameApplication.Instance.GetCharPeer(whisperTo);
                if (recipient != null && recipient.mPlayer != null)
                {
                    // Receive chat messages from client
                    if (recipient.mChar != peer.mChar) // Sender and recipient is different
                    {
                        if (recipient.GameSetting.RejectWhisper)
                        {
                            peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_PlayerRejectWhisper", "", false, peer);
                            return;
                        }

                        ChatMessage newMsg = new ChatMessage(MessageType.Whisper, message, player.Name, whisperTo,
                                                             playerSynStats.PortraitID, playerSynStats.jobsect,
                                                             0, playerSynStats.faction, isVoiceChat);
                        recipient.mPlayer.AddToChatMessageQueue(newMsg);
                        player.AddToChatMessageQueue(newMsg);
                        player.UpdateAchievement(AchievementObjectiveType.PrivateChat);
                    }
                    else
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Chat_NoWhisperYourself", "", true, peer);
                }
                else
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_TargetNotFind", string.Format("target;{0}", whisperTo), true, peer);
            }
            else
            {
#if DEBUG
                string message_lowerCase = message.ToLower();
                if (message_lowerCase.Equals("ccu"))
                {
                    int ccu = GameApplication.Instance.GetOnlineUserCount();
                    log.InfoFormat("{0} requested CCU = {1}, {2}", player.Name, ccu, DateTime.Now);
                    player.AddToChatMessageQueue(new ChatMessage((MessageType)msgType, "CCU: " + ccu));
                    return;
                }
                else if (message_lowerCase.Equals("dau"))
                {
                    DateTime now = DateTime.Now;
                    int dau = await GameApplication.dbRepository.Character.GetDAU(now.Date.AddDays(-1), now.Date);
                    log.InfoFormat("{0} requested DAU = {1}, {2}", player.Name, dau, now);
                    player.AddToChatMessageQueue(new ChatMessage((MessageType)msgType, "昨日DAU: " + dau));
                    return;
                }
                else if (message_lowerCase.Equals("monstercount"))
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    for (int index = 0; index < maMonsterSpawners.Count; ++index)
                    {
                        MonsterSpawnerBase spawner = maMonsterSpawners[index];
                        var children = spawner.maChildren;
                        string childPos = "";
                        for (int x = 0; x < children.Count; ++x)
                        {
                            childPos += string.Format("x={0}, z={1}; ", children[x].Position.x, children[x].Position.z);
                        }
                        sb.AppendFormat("archetype = {0}, child = {1}, pos = {2}", spawner.mArchetype == null ? 0 : spawner.mArchetype.id, children.Count, childPos);
                        sb.AppendLine();
                    }
                    string monstercount_result = sb.ToString();
                    log.InfoFormat("{0} requested monster = {1}, {2}", player.Name, monstercount_result, DateTime.Now);
                    player.AddToChatMessageQueue(new ChatMessage((MessageType)msgType, "Monster = " + monstercount_result));
                    return;
                }
#endif

                // Receive chat messages from client
                ChatMessage newMsg = new ChatMessage((MessageType)msgType, message, peer.mChar, whisperTo,
                                                     playerSynStats.PortraitID, playerSynStats.jobsect,
                                                     0, playerSynStats.faction, isVoiceChat);

                // Queue message at GameApplication
                switch (msgType)
                {
                    case (byte)MessageType.World:
                    case (byte)MessageType.System:
                        player.UpdateAchievement(AchievementObjectiveType.WorldChat);
                        GameApplication.Instance.BroadcastChatMessage(newMsg);
                        break;
                    //case (byte)MessageType.Faction:
                    //    GameApplication.Instance.BroadcastChatMessage_Faction(newMsg);
                    //    break;
                    case (byte)MessageType.Guild:
                        int guildId = player.SecondaryStats.guildId;
                        if (guildId == 0)
                            return;
                        player.UpdateAchievement(AchievementObjectiveType.GuildChat);
                        GameApplication.Instance.BroadcastChatMessage_Guild(newMsg, guildId);
                        break;
                    case (byte)MessageType.Party:
                        int partyId = player.PlayerSynStats.Party;
                        if (partyId == 0)
                            return;
                        GameApplication.Instance.BroadcastChatMessage_Party(newMsg, partyId);
                        break;
                }
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ClientSendChatMessage)]
        public void ClientSendChatMessageProxy(object[] args)
        {
            var task = ClientSendChatMessage((byte)args[0], (string)args[1], (string)args[2], (bool)args[3], (GameClientPeer)args[4]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetInspectPlayerInfo)]
        public void GetInspectPlayerInfo(string playerName, GameClientPeer peer)
        {
            // Check if peer exist
            GameClientPeer inspectedPeer = GameApplication.Instance.GetCharPeer(playerName);
            if (inspectedPeer != null && inspectedPeer.mPlayer != null && !inspectedPeer.mPlayer.Destroyed)
                inspectedPeer.SendInspectPlayerInfo(peer);
            else
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_PlayerIsOffline", "", false, peer);
        }

        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetInspectPlayerInfo)]
        public void GetInspectPlayerInfoProxy(object[] args)
        {
            GetInspectPlayerInfo((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SaveGameSetting)]
        public void SaveGameSetting(string settings, GameClientPeer peer)
        {
            peer.GameSetting = ServerSettingsData.Deserialize(settings);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SaveGameSetting)]
        public void SaveGameSettingProxy(object[] args)
        {
            SaveGameSetting((string)args[0], (GameClientPeer)args[1]);
        }

        #region Realm
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.CreateRealmByID)]
        public void CreateRealmByID(int realmId, bool logAI, bool checkAllReq, int questid, GameClientPeer peer)
        {
            RealmRules.CreateRealmById(realmId, logAI, checkAllReq, peer);
            peer.mPlayer.SetQuestRealmId(questid);
            if (questid != -1)
            {
                peer.QuestController.UpdateQuestEventStatus(questid);
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.CreateRealmByID)]
        public void CreateRealmByIDProxy(object[] args)
        {
            CreateRealmByID((int)args[0], (bool)args[1], (bool)args[2], (int)args[3], (GameClientPeer)args[4]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.EnterRealmByID)]
        public void EnterRealmByID(int realmId, GameClientPeer peer)
        {
            RealmRules.EnterRealmById(realmId, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.EnterRealmByID)]
        public void EnterRealmByIDProxy(object[] args)
        {
            EnterRealmByID((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.LeaveRealm)]
        public void LeaveRealm(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && mRoom.RealmID > 0)
                peer.LeaveRealm();
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.LeaveRealm)]
        public void LeaveRealmProxy(object[] args)
        {
            LeaveRealm((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InspectMode)]
        public void InspectMode(GameClientPeer peer)
        {
            RealmRules.InspectMode(23, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InspectMode)]
        public void InspectModeProxy(object[] args)
        {
            InspectMode((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DungeonEnterRequest)]
        public void DungeonEnterRequest(int realmId, GameClientPeer peer)
        {
            RealmRules.DungeonEnterRequest(realmId, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DungeonEnterRequest)]
        public void DungeonEnterRequestProxy(object[] args)
        {
            DungeonEnterRequest((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.EnterRealmWithPartyResponse)]
        public void EnterRealmWithPartyResponse(int realmId, byte status, GameClientPeer peer)
        {
            RealmRules.EnterRealmWithPartyResponse(realmId, status, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.EnterRealmWithPartyResponse)]
        public void DungeonEnterResponseProxy(object[] args)
        {
            EnterRealmWithPartyResponse((int)args[0], (byte)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DungeonAutoClear)]
        public void DungeonAutoClear(int realmId, bool clearAll, GameClientPeer peer)
        {
            RealmRules.DungeonAutoClear(realmId, clearAll, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DungeonAutoClear)]
        public void DungeonAutoClearProxy(object[] args)
        {
            DungeonAutoClear((int)args[0], (bool)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnPickResource)]
        public void OnPickResource(int pid, GameClientPeer peer)
        {
            //Player player = peer.mPlayer;
            //if (player == null)
            //    return;
            //RealmControllerResource realmController = mRealmController as RealmControllerResource;
            ////if (realmController == null || player.RealmInventoryStats.GetDailyLeftResource(DateTime.Now) == 0)
            ////    return;
            //Resource entity = mEntitySystem.GetEntityByPID(pid) as Resource;
            //if (entity == null || entity.Destroyed)
            //    return;
            //entity.OnPicked(player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnPickResource)]
        public void OnPickResourceProxy(object[] args)
        {
            OnPickResource((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RealmCollectReward)]
        public void RealmCollectReward(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.Destroyed || mRealmController == null)
                return;
            mRealmController.ClaimReward(player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RealmCollectReward)]
        public void RealmCollectRewardProxy(object[] args)
        {
            RealmCollectReward((GameClientPeer)args[0]);
        }
        #endregion

        #region Item
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UseItem)]
        public void UseItem(int slotId, int amount, GameClientPeer peer)
        {
            InvRetval retval = peer.mInventory.UseItemInInventory(slotId, amount);
            if (retval.retCode == InvReturnCode.UseFailed)
                ZRPC.CombatRPC.Ret_SendSystemMessage("sys_UseItemFailed", "", false, peer);
            else if (retval.retCode == InvReturnCode.MaxCurrency)
                ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Inv_CurrencyMax", "", false, peer);
            else if (retval.retCode == InvReturnCode.OverLevel)
                ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Inv_OverLevel", "", false, peer);
            else if (retval.retCode == InvReturnCode.BelowLevel)
                ZRPC.CombatRPC.Ret_SendSystemMessage("ret_UseItemFail_ReqlvlNotMeet", "", false, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UseItem)]
        public void UseItemProxy(object[] args)
        {
            UseItem((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SellItem)]
        public void SellItem(int slotId, int amount, GameClientPeer peer)
        {
            InvRetval retval = peer.mInventory.SellItem(slotId, (ushort)amount);

            if (retval.retCode == InvReturnCode.SellFailed)
                ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Inv_SellFailed", "", false, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SellItem)]
        public void SellItemProxy(object[] args)
        {
            SellItem((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.MassSellItems)]
        public void MassSellItems(string sellAmtToSlotIdStr, GameClientPeer peer)
        {
            if (!string.IsNullOrEmpty(sellAmtToSlotIdStr))
            {
                string[] splittedStr = sellAmtToSlotIdStr.Split(';');
                int length = splittedStr.Length;
                if (length > 0)
                {
                    Dictionary<int, int> sellAmtToSlotIdDict = new Dictionary<int, int>();
                    for (int i = 0; i < length; ++i)
                    {
                        string[] slotInfo = splittedStr[i].Split('`');
                        if (slotInfo.Length == 2)
                        {
                            int slotId = 0, amt = 0;
                            if (int.TryParse(slotInfo[0], out slotId))
                                if (int.TryParse(slotInfo[1], out amt))
                                    sellAmtToSlotIdDict[slotId] = amt;
                        }
                    }
                    InvRetval retval = peer.mInventory.MassSellItems(sellAmtToSlotIdDict);
                    if (retval.retCode == InvReturnCode.SellFailed)
                        ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Inv_SellFailed", "", false, peer);
                }
            }
            else
                ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Inv_SellFailed", "", false, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.MassSellItems)]
        public void MassSellItemProxy(object[] args)
        {
            MassSellItems((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.MassUseItems)]
        public void MassUseItems(string slotIds, GameClientPeer peer)
        {
            List<int> idlist = new List<int>();
            foreach (var s in slotIds.Split(','))
            {
                int slotIndex;
                if (int.TryParse(s, out slotIndex) && slotIndex >= 0)
                    idlist.Add(slotIndex);
            }

            if (idlist.Count > 0)
            {
                Dictionary<CurrencyType, int> currencyInc;
                InvRetval retval = peer.mInventory.MassUseCurrencyItems(idlist, out currencyInc);
                string currencyOutput = "";
                foreach (var c in currencyInc)
                {
                    currencyOutput += ((int)c.Key).ToString() + ":" + c.Value + "|";
                }
                peer.ZRPC.CombatRPC.Ret_MassUseItem((byte)retval.retCode, currencyOutput, peer);

                if (retval.retCode != InvReturnCode.UseSuccess)
                    ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Inv_UseFailed", "", false, peer);
            }
            else
            {

                ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Inv_UseFailed_1", "", false, peer);
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.MassUseItems)]
        public void MassUseItemsProxy(object[] args)
        {
            MassUseItems((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SortItem)]
        public void SortItem(GameClientPeer peer)
        {
            peer.mInventory.InvSortItem();
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SortItem)]
        public void SortItemProxy(object[] args)
        {
            SortItem((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OpenNewSlot)]
        public void OpenNewSlot(int numSlotsToUnlock, GameClientPeer peer)
        {
            peer.OpenNewInvSlot(numSlotsToUnlock, ItemInventoryController.OpenNewSlotType.DEFAULT);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OpenNewSlot)]
        public void OpenNewSlotProxy(object[] args)
        {
            OpenNewSlot((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OpenNewSlotAuto)]
        public void OpenNewSlotAuto(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
            {
                //if (VIPRepo.GetVIPPrivilege("AutoOpenBagSlot", player.PlayerSynStats.vipLvl) == 1)
                //    peer.OpenNewInvSlot(1, ItemInventoryController.OpenNewSlotType.VIP_AUTOOPEN);
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OpenNewSlotAuto)]
        public void OpenNewSlotAutoProxy(object[] args)
        {
            OpenNewSlotAuto((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.BuyPotion)]
        public void BuyPotion(bool auto, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;

            ushort itemid = (ushort)GameConstantRepo.GetConstantInt("Potion_ItemId");
            if (itemid == 0)
                return;
            if (auto)
            {
                int gold = GameConstantRepo.GetConstantInt("Potion_AutoGold", 10);
                if (!player.SecondaryStats.IsGoldEnough(gold))
                    return;
                int amount = GameConstantRepo.GetConstantInt("Potion_AutoNum", 50);
                if (peer.mInventory.AddItemsToInventory(itemid, amount, true, "Store").retCode == InvReturnCode.AddSuccess)
                    player.DeductGold(gold, true, true, "BuyPotion");
            }
            else
            {
                int money = GameConstantRepo.GetConstantInt("Potion_ManualMoney", 1000);
                if (player.SecondaryStats.Money < money)
                    return;
                int amount = GameConstantRepo.GetConstantInt("Potion_ManualNum", 50);
                if (peer.mInventory.AddItemsToInventory(itemid, amount, true, "Store").retCode == InvReturnCode.AddSuccess)
                    player.DeductMoney(money, "BuyPotion");
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.BuyPotion)]
        public void BuyPotionProxy(object[] args)
        {
            BuyPotion((bool)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnequipItem)]
        public void UnequipItem(int slotId, bool fashionslot, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            if (fashionslot)
                peer.mInventory.SwapFashionToInventory(slotId);
            else
                peer.mInventory.SwapEquipmentToInventory(slotId);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnequipItem)]
        public void UnequipItemProxy(object[] args)
        {
            UnequipItem((int)args[0], (bool)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SetItemHotbar)]
        public void SetItemHotbar(byte idx, int slotId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            peer.mInventory.SetItemToHotbar(idx, slotId);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SetItemHotbar)]
        public void SetItemHotbarProxy(object[] args)
        {
            SetItemHotbar((byte)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UseItemHotbar)]
        public void UseItemHotbar(byte idx, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            InvRetval retval = peer.mInventory.UseItemInHotbar(idx);
            if (retval.retCode == InvReturnCode.UseFailed)
                ZRPC.CombatRPC.Ret_SendSystemMessage("sys_UseItemFailed", "", false, peer);
            else if (retval.retCode == InvReturnCode.MaxCurrency)
                ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Inv_CurrencyMax", "", false, peer);
            else if (retval.retCode == InvReturnCode.OverLevel)
                ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Inv_OverLevel", "", false, peer);
            else if (retval.retCode == InvReturnCode.BelowLevel)
                ZRPC.CombatRPC.Ret_SendSystemMessage("ret_UseItemFail_ReqlvlNotMeet", "", false, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UseItemHotbar)]
        public void UseItemHotbarProxy(object[] args)
        {
            UseItemHotbar((byte)args[0], (GameClientPeer)args[1]);
        }
        #endregion

        #region IAP

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetProductsWithLockGold)]
        public async Task GetProductsWithLockGold(byte merchantType, GameClientPeer peer)
        {
            Player player = peer.mPlayer;

            if (player != null && !player.Destroyed)
            {
                TopUpManager topUpManager = await TopUpManager.InstanceAsync();
                var ignoreAwait = topUpManager.GetProductsWithLockGoldAsync(merchantType, peer);
            }
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.VerifyPurchase)]
        public async Task VerifyPurchase(string productId, string transactionId, string receipt, byte merchantType, GameClientPeer peer)
        {
            Player player = peer.mPlayer;

            if (player != null && !player.Destroyed)
            {
                TopUpManager topUpManager = await TopUpManager.InstanceAsync();
                var ignoreAwait = topUpManager.VerifyPurchaseAsync(productId, transactionId, receipt, merchantType, peer);
            }
        }

        #endregion IAP

        #region Old Code:Social
        //[RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialAcceptRequest)]
        //public void SocialAcceptRequest(string playerList, GameClientPeer peer)
        //{
        //    var task = peer.mPlayer.SocialAcceptFriendRequest(playerList);
        //}
        //[RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialAcceptRequest)]
        //public void SocialAcceptRequestProxy(object[] args)
        //{
        //    SocialAcceptRequest((string)args[0], (GameClientPeer)args[1]);
        //}

        //[RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialRemoveRequest)]
        //public void SocialRemoveRequest(string playerList, GameClientPeer peer)
        //{
        //    peer.mPlayer.SocialRemoveFriendRequest(playerList);
        //}
        //[RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialRemoveRequest)]
        //public void SocialRemoveRequestProxy(object[] args)
        //{
        //    SocialRemoveRequest((string)args[0], (GameClientPeer)args[1]);
        //}

        //[RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialSendRequest)]
        //public void SocialSendRequest(string playerList, GameClientPeer peer)
        //{
        //    var task = peer.mPlayer.SocialSendFriendRequest(playerList);
        //}
        //[RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialSendRequest)]
        //public void SocialSendRequestProxy(object[] args)
        //{
        //    SocialSendRequest((string)args[0], (GameClientPeer)args[1]);
        //}

        //[RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialRemoveFriend)]
        //public void SocialRemoveFriend(string playerName, GameClientPeer peer)
        //{
        //    var task = peer.mPlayer.SocialRemoveFriend(playerName);
        //}
        //[RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialRemoveFriend)]
        //public void SocialRemoveFriendProxy(object[] args)
        //{
        //    SocialRemoveFriend((string)args[0], (GameClientPeer)args[1]);
        //}

        //[RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialGetRecommendedFriends)]
        //public void SocialGetRecommendedFriends(GameClientPeer peer)
        //{
        //    var task = peer.mPlayer.SocialGetRecommendedFriends();
        //}
        //[RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialGetRecommendedFriends)]
        //public void SocialGetRecommendedFriendsProxy(object[] args)
        //{
        //    SocialGetRecommendedFriends((GameClientPeer)args[0]);
        //}

        //[RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialUpdateFriendsInfo)]
        //public void SocialUpdateFriendsInfo(GameClientPeer peer)
        //{
        //    peer.mPlayer.SocialUpdateFriendsInfo();
        //}
        //[RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SocialUpdateFriendsInfo)]
        //public void SocialUpdateFriendsInfoProxy(object[] args)
        //{
        //    SocialUpdateFriendsInfo((GameClientPeer)args[0]);
        //}
        #endregion

        #region Arena
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetArenaChallengers)]
        public void GetArenaChallengers(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            if (player.PlayerSynStats.Level < GameConstantRepo.GetConstantInt("Arena_UnlockLvl", 1))
                return;
            int myRank = 500;
            string result = LadderRules.GetArenaChallengers(player.Name, out myRank);
            ArenaInventoryData ArenaInventory = peer.CharacterData.ArenaInventory;
            int cooldown = 0;
            double elapsed = (DateTime.Now - ArenaInventory.LastBattleDT).TotalSeconds;
            if (elapsed < 50)
                cooldown = (int)(50 - elapsed);
            peer.ZRPC.CombatRPC.Ret_GetArenaChallenger(myRank, ArenaInventory.Entries, cooldown, result, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetArenaChallengers)]
        public void GetArenaChallengersProxy(object[] args)
        {
            GetArenaChallengers((GameClientPeer)args[0]);
        }
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChallengeArena)]
        public void ChallengeArena(int rank, bool free, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            if (player.PlayerSynStats.Level < GameConstantRepo.GetConstantInt("Arena_UnlockLvl", 1))
                return;
            //ArenaJson realm = RealmRepo.mArenaJson;
            //int realmid = realm.id;
            //if (mRoom.RealmID == realmid)
            //    return;
            //LevelJson level = LevelRepo.GetInfoById(realm.level);
            //if (level == null)
            //    return;
            if (!LadderRules.ValidateChallengeArena(player.Name, rank))
                return;
            ArenaInventoryData ArenaInventory = peer.CharacterData.ArenaInventory;
            //if (ArenaInventory.Entries >= realm.dailyentry)
            //{
            //    if (!free)
            //    {
            //        int totalentry = 0; // VIPRepo.GetVIPPrivilege("Arena", player.PlayerSynStats.vipLvl) + realm.dailyentry;
            //        if (ArenaInventory.Entries >= totalentry)
            //            return;
            //        int diamondBase = GameConstantRepo.GetConstantInt("Arena_GoldChallengeBase", 99);
            //        int diamond = (ArenaInventory.Entries - realm.dailyentry + 1) * diamondBase;
            //        if (player.DeductGold(diamond, true, true, "Arena_Buy"))
            //        {
            //            peer.ArenaRankToChallenge = rank;
            //            RealmRules.EnterRealmCheckAndLeaveParty(player);
            //            peer.CreateRealm(realmid, level.unityscene);
            //        }
            //    }
            //}
            //else
            //{
            //    peer.ArenaRankToChallenge = rank;
            //    RealmRules.EnterRealmCheckAndLeaveParty(player);
            //    peer.CreateRealm(realmid, level.unityscene);
            //}
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChallengeArena)]
        public void ChallengeArenaProxy(object[] args)
        {
            ChallengeArena((int)args[0], (bool)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ArenaClaimReward)]
        public void ArenaClaimReward(int rewardid, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            if (player.PlayerSynStats.Level < GameConstantRepo.GetConstantInt("Arena_UnlockLvl", 1))
                return;
            int mMyRank = LadderRules.GetArenaRank(player.Name);
            //ArenaJson arena_info = RealmRepo.mArenaJson;
            int real_rewardid = 0;
            //if (mMyRank == 0)
            //    real_rewardid = arena_info.reward1;
            //else if (mMyRank < 3)
            //    real_rewardid = arena_info.reward2;
            //else if (mMyRank < 10)
            //    real_rewardid = arena_info.reward3;
            //else if (mMyRank < 50)
            //    real_rewardid = arena_info.reward4;
            //else if (mMyRank < 100)
            //    real_rewardid = arena_info.reward5;
            //else if (mMyRank < 300)
            //    real_rewardid = arena_info.reward6;
            //else if (mMyRank < 500)
            //    real_rewardid = arena_info.reward7;
            //else
            //    real_rewardid = arena_info.reward8;
            if (rewardid != real_rewardid)
            {
                peer.ZRPC.CombatRPC.Ret_ArenaClaimReward(false, mMyRank, peer);
                return;
            }
            ArenaInventoryData ArenaInventory = peer.CharacterData.ArenaInventory;
            DateTime now = DateTime.Now;
            //if ((now - ArenaInventory.LastRewardDT).TotalSeconds < arena_info.rewardcd * 3600 - 60)
            //    return;

            GameRules.GiveReward_CheckBagSlotThenMail(player, new List<int>() { real_rewardid }, "Reward_ArenaDaily", null, true, true, string.Format("ArenaDaily reward={0}", real_rewardid));

            ArenaInventory.LastRewardDT = now;

            //log
            string message = string.Format("rank:{0}", mMyRank + 1);
            Zealot.Logging.Client.LogClasses.ArenaReward arenaRewardLog = new Zealot.Logging.Client.LogClasses.ArenaReward();
            arenaRewardLog.userId = peer.mUserId;
            arenaRewardLog.charId = peer.GetCharId();
            arenaRewardLog.message = message;
            arenaRewardLog.rank = mMyRank + 1;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(arenaRewardLog);

            peer.ZRPC.CombatRPC.Ret_ArenaClaimReward(true, mMyRank, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ArenaClaimReward)]
        public void ArenaClaimRewardProxy(object[] args)
        {
            ArenaClaimReward((int)args[0], (GameClientPeer)args[1]);
        }
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetArenaReport)]
        public void GetArenaReport(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            string report = LadderRules.GetArenaReport(player.Name);
            peer.ZRPC.CombatRPC.Ret_GetArenaReport(report, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetArenaReport)]
        public void GetArenaReportProxy(object[] args)
        {
            GetArenaReport((GameClientPeer)args[0]);
        }
        #endregion

        #region Mail

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.HasNewMail)]
        public void HasNewMail(GameClientPeer peer)
        {
            ZRPC.CombatRPC.Ret_HasNewMail(peer.HasNewMail(), "", peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.HasNewMail)]
        public void HasNewMailProxy(object[] args)
        {
            HasNewMail((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RetrieveMail)]
        public void RetrieveMail(GameClientPeer peer)
        {
            peer.RetrieveMail();
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RetrieveMail)]
        public void RetrieveMailProxy(object[] args)
        {
            RetrieveMail((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OpenMail)]
        public void OpenMail(int mailIndex, GameClientPeer peer)
        {
            int mailReturnCode = peer.OpenMail(mailIndex);

            ZRPC.CombatRPC.Ret_OpenMail(mailReturnCode, mailIndex, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OpenMail)]
        public void OpenMailProxy(object[] args)
        {
            OpenMail((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.TakeAttachment)]
        public void TakeAttachment(int mailIndex, GameClientPeer peer)
        {
            int mailReturnCode = peer.TakeAttachment(mailIndex);

            ZRPC.CombatRPC.Ret_TakeAttachment(mailReturnCode, mailIndex, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.TakeAttachment)]
        public void TakeAttachmentProxy(object[] args)
        {
            TakeAttachment((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.TakeAllAttachment)]
        public void TakeAllAttachment(GameClientPeer peer)
        {
            string lstTakenMailIndexSerialStr;
            int mailReturnCode = peer.TakeAllAttachment(out lstTakenMailIndexSerialStr);

            ZRPC.CombatRPC.Ret_TakeAllAttachment(mailReturnCode, lstTakenMailIndexSerialStr, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.TakeAllAttachment)]
        public void TakeAllAttachmentProxy(object[] args)
        {
            TakeAllAttachment((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DeleteMail)]
        public void DeleteMail(int mailIndex, GameClientPeer peer)
        {
            int mailReturnCode = peer.DeleteMail(mailIndex);

            ZRPC.CombatRPC.Ret_DeleteMail(mailReturnCode, mailIndex, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DeleteMail)]
        public void DeleteMailProxy(object[] args)
        {
            DeleteMail((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DeleteAllMail)]
        public void DeleteAllMail(GameClientPeer peer)
        {
            int mailReturnCode = peer.DeleteAllMail();

            ZRPC.CombatRPC.Ret_DeleteAllMail(mailReturnCode, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.DeleteAllMail)]
        public void DeleteAllMailProxy(object[] args)
        {
            DeleteAllMail((GameClientPeer)args[0]);
        }

        #endregion Mail

        #region OfflineExp

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OfflineExpGetData)]
        public void OfflineExpGetData(GameClientPeer peer)
        {
            //string serializedOfflineExp = peer.OfflineExpGetData();
            //string serializedOfflineExp = Newtonsoft.Json.JsonConvert.SerializeObject(peer.CharacterData.OfflineExpInv2);
            string serializedOfflineExp = peer.GetOfflineExp2().GetOfflineExpData(peer);

            ZRPC.CombatRPC.OfflineExpGetData(serializedOfflineExp, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OfflineExpGetData)]
        public void OfflineExpGetDataProxy(object[] args)
        {
            OfflineExpGetData((GameClientPeer)args[0]);
        }


        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OfflineExpStartReward)]
        public void OfflineExpStartReward(int rewardIdx, GameClientPeer peer)
        {
            //byte offlineExpReturnCode = peer.OfflineExpClaimReward(category);
            string serializedOfflineExp = peer.GetOfflineExp2().ChosenReward(peer, rewardIdx);

            ZRPC.CombatRPC.OfflineExpStartReward(serializedOfflineExp, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OfflineExpStartReward)]
        public void OfflineExpStartRewardProxy(object[] args)
        {
            OfflineExpStartReward((int)args[0], (GameClientPeer)args[1]);
        }


        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OfflineExpClaimReward)]
        public void OfflineExpClaimReward(int claimcode, GameClientPeer peer)
        {
            int newlvl;
            //byte offlineExpReturnCode = peer.OfflineExpClaimReward(category);
            int offlineExpReturnCode = peer.GetOfflineExp2().ClaimReward(peer, claimcode, out newlvl);

            ZRPC.CombatRPC.OfflineExpClaimReward(offlineExpReturnCode, newlvl, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OfflineExpClaimReward)]
        public void OfflineExpClaimRewardProxy(object[] args)
        {
            OfflineExpClaimReward((int)args[0], (GameClientPeer)args[1]);
        }



        #endregion OfflineExp

        #region Auction

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionGetAuctionItem)]
        public void AuctionGetAuctionItem(GameClientPeer peer)
        {
            AuctionRules.GetAuctionItem(peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionGetAuctionItem)]
        public void AuctionGetAuctionItemProxy(object[] args)
        {
            AuctionGetAuctionItem((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionGetRecords)]
        public void AuctionGetRecords(GameClientPeer peer)
        {
            Task task = AuctionRules.GetAuctionRecords(peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionGetRecords)]
        public void AuctionGetRecordsProxy(object[] args)
        {
            AuctionGetRecords((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionGetBidItems)]
        public void AuctionGetBidItems(GameClientPeer peer)
        {
            AuctionRules.GetBidItems(peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionGetBidItems)]
        public void AuctionGetBidItemsProxy(object[] args)
        {
            AuctionGetBidItems((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionCollectItem)]
        public void AuctionCollectItem(int bidId, GameClientPeer peer)
        {
            AuctionRules.CollectAuctionItem(bidId, peer);

        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionCollectItem)]
        public void AuctionCollectItemProxy(object[] args)
        {
            AuctionCollectItem((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionPlaceBid)]
        public void AuctionPlaceBid(int auctionId, int bidPrice, GameClientPeer peer)
        {
            AuctionRules.PlaceBid(auctionId, bidPrice, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AuctionPlaceBid)]
        public void AuctionPlaceBidProxy(object[] args)
        {
            AuctionPlaceBid((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }
        #endregion

        #region Guild
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildGetGuildInfo)]
        public void GuildGetGuildInfo(byte searchFilter, string searchString, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
            {
                if (player.SecondaryStats.guildId > 0)
                    return;
                int guildId = 0;
                if (searchFilter == (byte)GuildSearchFilter.GuildId)
                {
                    if (!int.TryParse(searchString, out guildId))
                        return;
                }
                GuildRules.SendGuildListToClient(searchFilter, guildId, searchString, player);
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildGetGuildInfo)]
        public void GuildGetGuildInfoProxy(object[] args)
        {
            GuildGetGuildInfo((byte)args[0], (string)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildCheckName)]
        public void GuildCheckName(string guildName, GameClientPeer peer)
        {
            var task = GuildRules.CheckGuildName(guildName, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildCheckName)]
        public void GuildCheckNameProxy(object[] args)
        {
            GuildCheckName((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildAdd)]
        public void GuildAdd(string guildName, int guildIcon, GameClientPeer peer)
        {
            Task task = GuildRules.CreateGuild(guildName, guildIcon, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildAdd)]
        public void GuildAddProxy(object[] args)
        {
            GuildAdd((string)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildJoin)]
        public void GuildJoin(int guildId, GameClientPeer peer)
        {
            GuildRules.JoinGuildCheck(guildId, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildJoin)]
        public void GuildJoinProxy(object[] args)
        {
            GuildJoin((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildLeave)]
        public void GuildLeave(string playerName, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                GuildRules.LeaveGuild(playerName, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildLeave)]
        public void GuildLeaveProxy(object[] args)
        {
            GuildLeave((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildMemberRequest)]
        public void GuildMemberRequest(string playerName, bool isAccept, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                GuildRules.CheckMemberRequest(playerName, isAccept, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildMemberRequest)]
        public void GuildMemberRequestProxy(object[] args)
        {
            GuildMemberRequest((string)args[0], (bool)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildRequestAll)]
        public void GuildRequestAll(bool isAccept, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                GuildRules.CheckMemberRequestAll(isAccept, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildRequestAll)]
        public void GuildRequestAllProxy(object[] args)
        {
            GuildRequestAll((bool)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildRequestSetting)]
        public void GuildRequestSetting(int combatscore, int level, byte viplevel, bool autoAccept, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                GuildRules.SetRequestSetting(combatscore, level, viplevel, autoAccept, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildRequestSetting)]
        public void GuildRequestSettingProxy(object[] args)
        {
            GuildRequestSetting((int)args[0], (int)args[1], (byte)args[2], (bool)args[3], (GameClientPeer)args[4]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildSetIcon)]
        public void GuildSetIcon(int icon, bool free, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                GuildRules.SetGuildIcon(icon, free, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildSetIcon)]
        public void GuildSetIconProxy(object[] args)
        {
            GuildSetIcon((int)args[0], (bool)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildSetNotice)]
        public void GuildSetNotice(string noticeStr, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                GuildRules.SetGuildNotice(noticeStr, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildSetNotice)]
        public void GuildSetNoticeProxy(object[] args)
        {
            GuildSetNotice((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildLvlUpTech)]
        public void GuildLvlUpTech(byte techType, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                GuildRules.GuildLvlUpTech((GuildTechType)techType, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildLvlUpTech)]
        public void GuildLvlUpTechProxy(object[] args)
        {
            GuildLvlUpTech((byte)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildGetHistory)]
        public void GuildGetHistory(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
            {
                string history = GuildRules.GetGuildHistory(player);
                peer.ZRPC.CombatRPC.Ret_GuildGetHistory(history, peer);
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildGetHistory)]
        public void GuildGetHistoryProxy(object[] args)
        {
            GuildGetHistory((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildShopExchange)]
        public void GuildShopExchange(byte shopId, int itemId, GameClientPeer peer)
        {
            GuildRules.GuildShopExchange(shopId, itemId, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildShopExchange)]
        public void GuildShopExchangeProxy(object[] args)
        {
            GuildShopExchange((byte)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildOpenSecretShop)]
        public void GuildOpenSecretShop(GameClientPeer peer)
        {
            GuildRules.OpenSecretShop(peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildOpenSecretShop)]
        public void GuildOpenSecretShopProxy(object[] args)
        {
            GuildOpenSecretShop((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildAppointRank)]
        public void GuildAppointRank(string appointName, byte appointRank, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            GuildRules.AppointGuildRank(appointName, appointRank, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildAppointRank)]
        public void GuildAppointRankProxy(object[] args)
        {
            GuildAppointRank((string)args[0], (byte)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildInviteWorld)]
        public void GuildInviteWorld(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            GuildRules.GuildInviteWorld(player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildInviteWorld)]
        public void GuildInviteWorldProxy(object[] args)
        {
            GuildInviteWorld((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildDreamHouseAdd)]
        public void GuildDreamHouseAdd(byte type, GameClientPeer peer)
        {
            GuildRules.GuildDreamHouseAdd((DreamhouseType)type, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildDreamHouseAdd)]
        public void GuildDreamHouseAddProxy(object[] args)
        {
            GuildDreamHouseAdd((byte)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildDreamHouseCollect)]
        public void GuildDreamHouseCollect(int milestone, GameClientPeer peer)
        {
            GuildRules.GuildDreamHouseCollect(milestone, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GuildDreamHouseCollect)]
        public void GuildDreamHouseCollectProxy(object[] args)
        {
            GuildDreamHouseCollect((int)args[0], (GameClientPeer)args[1]);
        }
        #endregion

        #region Party
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetPartyList)]
        public void GetPartyList(int locationId, int minLevel, bool autoAcceptOnly, bool isRefresh, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && !player.IsInParty())
                PartyRules.SendPartyListToClient(locationId, minLevel, autoAcceptOnly, isRefresh, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetPartyList)]
        public void GetPartyListProxy(object[] args)
        {
            GetPartyList((int)args[0], (int)args[1], (bool)args[2], (bool)args[3], (GameClientPeer)args[4]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.CreateParty)]
        public void CreateParty(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && !player.IsInParty())
                PartyRules.CreateParty(player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.CreateParty)]
        public void CreatePartyProxy(object[] args)
        {
            CreateParty((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.JoinParty)]
        public void JoinParty(int partyId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && !player.IsInParty())
                PartyRules.TryJoinParty(partyId, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.JoinParty)]
        public void JoinPartyProxy(object[] args)
        {
            JoinParty((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ProcessPartyRequest)]
        public void ProcessPartyRequest(string playerName, bool isAccept, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && player.IsInParty())
                PartyRules.ProcessMemberRequest(player.PlayerSynStats.Party, playerName, isAccept, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ProcessPartyRequest)]
        public void ProcessPartyRequestProxy(object[] args)
        {
            ProcessPartyRequest((string)args[0], (bool)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InviteToParty)]
        public void InviteToParty(string playerName, int heroId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                PartyRules.InviteToParty(playerName, heroId, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InviteToParty)]
        public void InviteToPartyProxy(object[] args)
        {
            InviteToParty((string)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AcceptPartyInvitation)]
        public void AcceptPartyInvitation(string senderName, bool isAccept, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && !player.IsInParty())
                PartyRules.AcceptPartyInvitation(senderName, isAccept, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AcceptPartyInvitation)]
        public void AcceptPartyInvitationProxy(object[] args)
        {
            AcceptPartyInvitation((string)args[0], (bool)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InviteToFollow)]
        public void InviteToFollow(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            PartyStatsServer partyStats = player.PartyStats;
            if (player != null && player.IsInParty() && partyStats.IsLeader(player.Name))
                PartyRules.InviteToFollow(player.PlayerSynStats.Party, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InviteToFollow)]
        public void InviteToFollowProxy(object[] args)
        {
            InviteToFollow((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AcceptFollowInvitation)]
        public void AcceptFollowInvitation(string senderName, bool isAccept, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && player.IsInParty())
                PartyRules.AcceptFollowInvitation(senderName, isAccept, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AcceptFollowInvitation)]
        public void AcceptFollowInvitationProxy(object[] args)
        {
            AcceptFollowInvitation((string)args[0], (bool)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SendPartyRecruitment)]
        public void SendPartyRecruitment(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            PartyStatsServer partyStats = player.PartyStats;
            if (player != null && player.IsInParty() && partyStats.IsLeader(player.Name))
                PartyRules.SendPartyRecruitment(player.PlayerSynStats.Party, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SendPartyRecruitment)]
        public void SendPartyRecruitmentProxy(object[] args)
        {
            SendPartyRecruitment((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.KickPartyMember)]
        public void KickPartyMember(string memberName, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            PartyStatsServer partyStats = player.PartyStats;
            if (player != null && player.IsInParty() && partyStats.IsLeader(player.Name))
                PartyRules.LeaveParty(player.PlayerSynStats.Party, memberName, LeavePartyReason.Kick);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.KickPartyMember)]
        public void KickPartyMemberProxy(object[] args)
        {
            KickPartyMember((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChangePartyLeader)]
        public void ChangePartyLeader(string newLeaderName, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && player.IsInParty())
                PartyRules.ChangePartyLeader(player.PlayerSynStats.Party, newLeaderName, player);
        }

        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChangePartyLeader)]
        public void ChangePartyLeaderProxy(object[] args)
        {
            ChangePartyLeader((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.LeaveParty)]
        public void LeaveParty(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && player.IsInParty())
                PartyRules.LeaveParty(player.PlayerSynStats.Party, player.Name, LeavePartyReason.Self);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.LeaveParty)]
        public void LeavePartyProxy(object[] args)
        {
            LeaveParty((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChangePartySetting)]
        public void ChangePartySetting(string setting, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && player.IsInParty())
                PartyRules.ChangePartySetting(player.PlayerSynStats.Party, setting, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChangePartySetting)]
        public void ChangePartySettingProxy(object[] args)
        {
            ChangePartySetting((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SaveAutoFollowSetting)]
        public void SaveAutoFollowSetting(bool isRejectFollow, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && player.IsInParty())
                PartyRules.SaveAutoFollowSetting(player.PlayerSynStats.Party, isRejectFollow, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SaveAutoFollowSetting)]
        public void SaveAutoFollowSettingProxy(object[] args)
        {
            SaveAutoFollowSetting((bool)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.FollowPartyMember)]
        public void FollowPartyMember(string memberName, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                PartyRules.FollowPartyMember(memberName, player);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.FollowPartyMember)]
        public void FollowPartyMemberProxy(object[] args)
        {
            FollowPartyMember((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetPartyMemberPosition)]
        public void GetPartyMemberPosition(string memberName, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && player.IsInParty())
            {
                string currLevelName;
                Vector3 currPosition;
                int pid = PartyRules.GetPartyMemberPosition(memberName, out currLevelName, out currPosition, player);
                if (pid != -1)
                    peer.ZRPC.CombatRPC.OnGetPartyMemberPosition(currLevelName, currPosition.ToRPCPosition(), pid, peer);
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetPartyMemberPosition)]
        public void GetPartyMemberPositionProxy(object[] args)
        {
            GetPartyMemberPosition((string)args[0], (GameClientPeer)args[1]);
        }
        #endregion

        #region Hero
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnlockHero)]
        public void UnlockHero(int heroId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.HeroStats.UnlockHero(heroId);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnlockHero)]
        public void UnlockHeroProxy(object[] args)
        {
            UnlockHero((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.LevelUpHero)]
        public void LevelUpHero(int heroId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.HeroStats.LevelUpHero(heroId);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.LevelUpHero)]
        public void LevelUpHeroProxy(object[] args)
        {
            LevelUpHero((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddHeroSkillPoint)]
        public void AddHeroSkillPoint(int heroId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.HeroStats.AddSkillPoint(heroId);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddHeroSkillPoint)]
        public void AddHeroSkillPointProxy(object[] args)
        {
            AddHeroSkillPoint((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ResetHeroSkillPoints)]
        public void ResetHeroSkillPoints(int heroId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.HeroStats.ResetSkillPoints(heroId);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ResetHeroSkillPoints)]
        public void ResetHeroSkillPointsProxy(object[] args)
        {
            ResetHeroSkillPoints((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.LevelUpHeroSkill)]
        public void LevelUpHeroSkill(int heroId, int skillNo, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.HeroStats.LevelUpSkill(heroId, skillNo);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.LevelUpHeroSkill)]
        public void LevelUpHeroSkillProxy(object[] args)
        {
            LevelUpHeroSkill((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChangeHeroInterest)]
        public void ChangeHeroInterest(int heroId, byte assignedInterest, bool acceptResult, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.HeroStats.ChangeHeroInterest(heroId, assignedInterest, acceptResult);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChangeHeroInterest)]
        public void ChangeHeroInterestProxy(object[] args)
        {
            ChangeHeroInterest((int)args[0], (byte)args[1], (bool)args[2], (GameClientPeer)args[3]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddHeroTrust)]
        public void AddHeroTrust(int heroId, int itemId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.HeroStats.AddHeroTrust(heroId, itemId);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddHeroTrust)]
        public void AddHeroTrustProxy(object[] args)
        {
            AddHeroTrust((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SummonHero)]
        public void SummonHero(int heroId, int tier, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.HeroStats.SummonHero(heroId, tier);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SummonHero)]
        public void SummonHeroProxy(object[] args)
        {
            SummonHero((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ExploreMap)]
        public void ExploreMap(int mapId, int targetId, string heroes, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
            {
                List<int> heroIds = new List<int>();
                string[] idstr = heroes.Split(';');
                for (int i = 0; i < idstr.Length; i++)
                {
                    int heroId;
                    if (int.TryParse(idstr[i], out heroId))
                        heroIds.Add(heroId);
                }
                player.HeroStats.ExploreMap(mapId, targetId, heroIds);
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ExploreMap)]
        public void ExploreMapProxy(object[] args)
        {
            ExploreMap((int)args[0], (int)args[1], (string)args[2], (GameClientPeer)args[3]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ClaimExplorationReward)]
        public void ClaimExplorationReward(int mapId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.HeroStats.ClaimExplorationReward(mapId);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ClaimExplorationReward)]
        public void ClaimExplorationRewardProxy(object[] args)
        {
            ClaimExplorationReward((int)args[0], (GameClientPeer)args[1]);
        }
        #endregion

        #region Achievement
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ClaimAchievementReward)]
        public void ClaimAchievementReward(byte type, int id, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.AchievementStats.ClaimReward((AchievementKind)type, id);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ClaimAchievementReward)]
        public void ClaimAchievementRewardProxy(object[] args)
        {
            ClaimAchievementReward((byte)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ClaimAllAchievementRewards)]
        public void ClaimAllAchievementRewards(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.AchievementStats.ClaimAllRewards();
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ClaimAllAchievementRewards)]
        public void ClaimAllAchievementRewardsProxy(object[] args)
        {
            ClaimAllAchievementRewards((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AchievementNPCInteract)]
        public void AchievementNPCInteract(int npcId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
            {
                player.AchievementStats.UpdateCollection(CollectionType.NPC, npcId);
                player.UpdateAchievement(AchievementObjectiveType.NPCInteract, npcId.ToString(), true);
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AchievementNPCInteract)]
        public void AchievementNPCInteractProxy(object[] args)
        {
            AchievementNPCInteract((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.StoreCollectionItem)]
        public void StoreCollectionItem(int id, bool isStore, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.AchievementStats.StoreCollectionItem(id, isStore);

        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.StoreCollectionItem)]
        public void StoreCollectionItemProxy(object[] args)
        {
            StoreCollectionItem((int)args[0], (bool)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnlockNextLISATier)]
        public void UnlockNextLISATier(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.AchievementStats.UnlockNextLISATier();

        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnlockNextLISATier)]
        public void UnlockNextLISATierProxy(object[] args)
        {
            UnlockNextLISATier((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChangeLISATier)]
        public void ChangeLISATier(int tier, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                player.AchievementStats.ChangeLISATier(tier);

        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ChangeLISATier)]
        public void ChangeLISATierProxy(object[] args)
        {
            ChangeLISATier((int)args[0], (GameClientPeer)args[1]);
        }
        #endregion

        #region Donate
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetGuildDonateData)]
        public void GetGuildDonateData(string lastUpdateTime, GameClientPeer peer)
        {
            DonateRules.SendGuildDonateDataToClient(lastUpdateTime, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetGuildDonateData)]
        public void GetGuildDonateDataProxy(object[] args)
        {
            GetGuildDonateData((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RequestGuildDonate)]
        public void RequestGuildDonate(int heroId, GameClientPeer peer)
        {
            DonateRules.RequestGuildDonate(heroId, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RequestGuildDonate)]
        public void RequestGuildDonateProxy(object[] args)
        {
            RequestGuildDonate((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ResponseGuildDonate)]
        public void ResponseGuildDonate(string requestPlayerName, int itemSlotId, int count, GameClientPeer peer)
        {
            DonateRules.ResponseGuildDonate(requestPlayerName, itemSlotId, count, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.ResponseGuildDonate)]
        public void ResponseGuildDonateProxy(object[] args)
        {
            ResponseGuildDonate((string)args[0], (int)args[1], (int)args[2], (GameClientPeer)args[3]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetGuildDonate)]
        public void GetGuildDonate(GameClientPeer peer)
        {
            DonateRules.GetDonate(peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetGuildDonate)]
        public void GetGuildDonateProxy(object[] args)
        {
            GetGuildDonate((GameClientPeer)args[0]);
        }
        #endregion
        
        #region Bot
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.BotSetting)]
        public void SendBotSettingToServer(string settings, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null)
                peer.CharacterData.BotSetting = settings;
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.BotSetting)]
        public void SendBotSettingToServerProxy(object[] args)
        {
            SendBotSettingToServer((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetClosestValidMonSpawnPos)]
        public void GetClosestValidMonSpawnPos(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;

            Vector3 pos = player.Position;
            float closestDistSq = float.MaxValue;
            MonsterSpawnerBase closestSpawner = null;
            foreach (MonsterSpawnerBase spawner in maMonsterSpawners)
            {
                if (!spawner.HasChildren())
                    continue;

                Vector3 currPos = spawner.GetPos();
                float distSq = (pos - currPos).sqrMagnitude;
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closestSpawner = spawner;
                }
            }

            if (closestSpawner != null)
            {
                //reply if there is a result, otherwise let the client time out and client will requery after 5sec
                int randomIdx = GameUtils.RandomInt(0, closestSpawner.maChildren.Count - 1);
                Vector3 closestPos = closestSpawner.maChildren[randomIdx].Position;
                //Vector3 closestPos = closestSpawner.GetPos() + new Vector3(1f, 0, 1f);
                ZRPC.CombatRPC.ReplyValidMonSpawnPos((closestPos).ToRPCPosition(), peer);
            }
            else
            {
                ZRPC.CombatRPC.ReplyValidMonSpawnPos(Vector3.zero.ToRPCPosition(), peer);
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetClosestValidMonSpawnPos)]
        public void GetClosestValidMonSpawnPosProxy(object[] args)
        {
            GetClosestValidMonSpawnPos((GameClientPeer)args[0]);
        }
        #endregion

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RemoveBuff)]
        public void RemoveSideBuff(int sideID, GameClientPeer peer)
        {
            peer.mPlayer.RemoveMyBuff(sideID);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RemoveBuff)]
        public void RemoveSideBuffProxy(object[] args)
        {
            RemoveSideBuff((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.CurrencyExchange)]
        public void CurrencyExchange(GameClientPeer peer)
        {
            peer.OnCurrencyExchange();
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.CurrencyExchange)]
        public void CurrencyExchangeProxy(object[] args)
        {
            CurrencyExchange((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RedeemSerialCode)]
        public void RedeemSerialCode(string serial, GameClientPeer peer)
        {
            if (serial.Length != 20)
            {
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Redeem_InvalidCode", "", false, peer);
                return;
            }
            Task task = RedemptionRules.TryRedeemSerialCode(serial, peer);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetRandomBoxReward)]
        public void GetRandomBoxReward(int reward_id, GameClientPeer peer)
        {
            //Player player = peer.mPlayer;
            //if (player == null)
            //    return;

            //int result = RandomBoxReward.RandomBoxRewardManager.Instance.GetRewards(reward_id, player);
            //peer.ZRPC.CombatRPC.Ret_GetRandomBoxReward((byte)result, reward_id, peer);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetRandomBoxReward)]
        public void GetRandomBoxRewardProxy(object[] args)
        {
            GetRandomBoxReward((int)args[0], (GameClientPeer)args[1]);
        }

        #region Wardrobe
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.EquipFashion)]
        public void EquipFashion(int item_id, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;

            player.WardrobeController.EquipFashion(item_id);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnequipFashion)]
        public void UnequipFashion(int item_id, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;

            player.WardrobeController.UnequipFashion(item_id);
        }
        #endregion

        #region Mount
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.Mount)]
        public void Mount(int mount_id, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;

            if (player.PlayerSynStats.MountID > 0 && player.PlayerSynStats.MountID == mount_id)
            {
                player.PlayerSynStats.MountID *= -1;
                player.PlayerStats.MoveSpeed = 15;
            }
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UnMount)]
        public void UnMount(GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;

            if (player.PlayerSynStats.MountID < 0)
            {
                player.PlayerSynStats.MountID *= -1;
                player.PlayerSynStats.MoveSpeed = 6;
            }
        }
        #endregion

        #region Combat
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SetPlayerTeam)]
        public void SetPlayerTeam(int teamid, GameClientPeer peer)
        {
            peer.mPlayer.PlayerSynStats.Team = teamid;
        }
        #endregion

        #region InvitePVP
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InvitePvpAsk)]
        public void InvitePvpAsk(string targetname, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;

            player.InvitePvpAsk(targetname);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.InvitePvpReply)]
        public void InvitePvpReply(string askername, int reply, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;

            player.InvitePvpReply(askername, reply);
        }
        #endregion

        #region Tutorial
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.TutorialStep)]
        public void TutorialStep(int step, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            RealmControllerTutorial realmcontroller = mRealmController as RealmControllerTutorial;
            if (realmcontroller == null)
                return;
            //GameApplication.Instance.executionFiber.Enqueue;//consider use this fiber if the realm 
            //have many players.
            if (step == (int)Trainingstep.Finished)
                realmcontroller.OnMissionCompleted(true, false);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.TutorialStep)]
        public void TutorialStepProxy(object[] args)
        {
            TutorialStep((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.UpdateTutorialList)]
        public void UpdateTutorialList(int bitpos, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;

            player.UpdateTutorialList(bitpos);
        }
        #endregion

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.SyncAttackResult)]
        public void SyncAttackResult(int targetpid, int dmg, bool heal, bool interrupte, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.IsInRT())
                return;
            Actor actor = player.EntitySystem.GetEntityByID(targetpid) as Actor;
            //AttackResult res = new AttackResult(targ) 
            //if(actor!=null)
            //actor.OnSyncDamage(player,dmg, heal, interrupte);
        } 

        #region Triggers
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnColliderTrigger)]
        public void OnColliderTrigger(int objectId, bool enter, GameClientPeer peer)
        {
            IServerEntity _serverEntity;
            if (mObjectMap.TryGetValue(objectId, out _serverEntity))
            {
                ColliderTrigger _trigger = _serverEntity as ColliderTrigger;
                if (_trigger != null)
                {
                    if (enter)
                        _trigger.OnEnter(peer.mPlayer);
                    else
                        _trigger.OnLeave(peer.mPlayer);
                }
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnColliderTrigger)]
        public void OnColliderTriggerProxy(object[] args)
        {
            OnColliderTrigger((int)args[0], (bool)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnCutsceneFinished)]
        public void OnCutsceneFinished(int objectId, GameClientPeer peer)
        {
            IServerEntity _serverEntity;
            if (mObjectMap.TryGetValue(objectId, out _serverEntity))
            {
                CutsceneBroadcaster cutsceneBroadcaster = _serverEntity as CutsceneBroadcaster;
                if (cutsceneBroadcaster != null)
                    cutsceneBroadcaster.OnCutsceneFinished();
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnCutsceneFinished)]
        public void OnCutsceneFinishedProxy(object[] args)
        {
            OnCutsceneFinished((int)args[0], (GameClientPeer)args[1]);
        }
        #endregion

        #region InteractiveTrigger
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnInteractiveInit)]
        public void OnInteractiveInit(GameClientPeer peer)
        {
            peer.mPlayer.InteractiveTriggerController.Init(peer.mPlayer.mInstance.mCurrentLevelID);
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnInteractiveInit)]
        public void OnInteractiveInitProxy(object[] args)
        {
            OnInteractiveInit((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnInteractiveUse)]
        public void OnInteractiveUse(int objectId, bool enter, GameClientPeer peer)
        {
            IServerEntity _serverEntity;
            if (mObjectMap.TryGetValue(objectId, out _serverEntity))
            {
                InteractiveTrigger _trigger = _serverEntity as InteractiveTrigger;
                if (_trigger != null)
                {
                    _trigger.OnInteractiveUse(objectId, enter, peer);
                }
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnInteractiveUse)]
        public void OnInteractiveUseProxy(object[] args)
        {
            OnInteractiveUse((int)args[0], (bool)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnInteractiveTrigger)]
        public void OnInteractiveTrigger(int objectId, GameClientPeer peer)
        {
            IServerEntity _serverEntity;
            if (mObjectMap.TryGetValue(objectId, out _serverEntity))
            {
                InteractiveTrigger _trigger = _serverEntity as InteractiveTrigger;
                if (_trigger != null)
                {
                    _trigger.OnInteractive(objectId, peer);
                }
            }
        }
        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.OnInteractiveTrigger)]
        public void OnInteractiveTriggerProxy(object[] args)
        {
            OnInteractiveTrigger((int)args[0], (GameClientPeer)args[1]);
        }
        #endregion

        #region World Boss
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetWorldBossList)]
        public void GetWorldBossList(GameClientPeer peer)
        {
            peer.ZRPC.CombatRPC.Ret_GetWorldBossList(BossRules.GetSpecialBossStatusString(), peer);
        }

        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetWorldBossList)]
        public void GetWorldBossListProxy(object[] args)
        {
            GetWorldBossList((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetWorldBossDmgList)]
        public void GetWorldBossDmgList(int bossid, GameClientPeer peer)
        {
            peer.ZRPC.CombatRPC.Ret_GetWorldBossDmgList(BossRules.GetBossDmgList(bossid), peer);
        }

        [RPCMethodProxy(RPCCategory.Combat, (byte)ClientCombatRPCMethods.GetWorldBossDmgList)]
        public void GetWorldBossDmgListProxy(object[] args)
        {
            GetWorldBossDmgList((int)args[0], (GameClientPeer)args[1]);
        }
        #endregion

        #region SideEffect
        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.AddSideEffect)]
        public void AddSideEffect(int targetpid, int sideeffect_id, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.IsInRT())
                return;
            Actor target = player.EntitySystem.GetEntityByPID(targetpid) as Actor;
            Zealot.Server.SideEffects.SideEffectsUtils.ClientAddSideEffect(target, player, SideEffectRepo.GetSideEffect(sideeffect_id));
        }

        [RPCMethod(RPCCategory.Combat, (byte)ClientCombatRPCMethods.RemoveSideEffect)]
        public void RemoveSideEffect(int targetpid, int sideeffect_id, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.IsInRT())
                return;
            Actor target = player.EntitySystem.GetEntityByPID(targetpid) as Actor;
            Zealot.Server.SideEffects.SideEffectsUtils.ClientRemoveSideEffect(target, player, SideEffectRepo.GetSideEffect(sideeffect_id));
        }
        #endregion
    }
}
