using Photon.LoadBalancing.Operations;
using Photon.LoadBalancing.GameServer;
using Zealot.Server.EventMessage;

namespace Zealot.RPC
{
    using Zealot.Common.RPC;

    public class CombatRPC : ServerRPCBase
    {
        public CombatRPC(object zrpc) :
            base(typeof(CombatRPC), (byte)OperationCode.Combat, true, zrpc)
        {
            SetMainContext(typeof(GameLogic), RPCCategory.Combat);
        }

        public RPCBroadcastData GetSerializedRPC(ServerCombatRPCMethods rpcMethodId, params object[] args)
        {
            return base.GetSerializedRPC((byte)rpcMethodId, args);
        }

        [RPCUnsuspend]
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.RetSuspendedRPC)]
        public void RetSuspendedRPC(int ret, string optionalparam, bool addToChatLog, object target)
        {
            ProxyMethod("RetSuspendedRPC", ret, optionalparam, addToChatLog, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SpawnPlayerEntity)]
        public void SpawnPlayerEntity(bool isLocal, int ownerid, string playername, int pid, byte jobsect, byte gender, int mountID, RPCPosition rpcpos, RPCDirection rpcdir, int health, int maxhealth, object target)
        {
            ProxyMethod("SpawnPlayerEntity", isLocal, ownerid, playername, pid, jobsect, gender, mountID, rpcpos, rpcdir, health, maxhealth, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SpawnMonsterEntity)]
        public void SpawnMonsterEntity(int pid, int archetypeid, RPCPosition rpcpos, RPCDirection dir, int health, object target)
        {
            ProxyMethod("SpawnMonsterEntity", pid, archetypeid, rpcpos, dir, health, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SpawnHeroEntity)]
        public void SpawnHeroEntity(int pid, int heroid, int tier, int ownerpid, RPCPosition rpcpos, RPCDirection rpcdir, object target)
        {
            ProxyMethod("SpawnHeroEntity", pid, heroid, tier, ownerpid, rpcpos, rpcdir, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.DestroyEntity)]
        public void DestroyEntity(int pid, object target)
        {
            ProxyMethod("DestroyEntity", pid, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OnPlayerDead)]
        public void OnPlayerDead(string killer, byte respawntype, object target)
        {
            ProxyMethod("OnPlayerDead", killer, respawntype, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OnPlayerDragged)]
        public void OnPlayerDragged(RPCPosition pos, float dur, float speed, object target)
        {
            ProxyMethod("OnPlayerDragged", pos, dur, speed, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.TrainingStepDone)]
        public void TrianingStepDone(int step, object target)
        {
            ProxyMethod("TrianingStepDone", step, target);
        }


        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.TrainingDodgeResult)]
        public void TrainingDodgeResult(bool res, object target)
        {
            ProxyMethod("TrainingDodgeResult", res, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OnIncreaseCD)]
        public void OnIncreaseCooldown(string list, float dur, object target)
        {
            ProxyMethod("OnIncreaseCooldown", list, dur, target);
        }


        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.RespawnPlayer)]
        public void RespawnPlayer(RPCPosition pos, RPCDirection dir, object target)
        {
            ProxyMethod("RespawnPlayer", pos, dir, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.TeleportSetPos)]
        public void TeleportSetPos(RPCPosition pos, object target)
        {
            ProxyMethod("TeleportSetPos", pos, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.TeleportSetPosDirection)]
        public void TeleportSetPosDirection(RPCPosition pos, RPCDirection dir, object target)
        {
            ProxyMethod("TeleportSetPosDirection", pos, dir, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.TransferRoom)]
        public void TransferRoom(string levelname, object target)
        {
            ProxyMethod("TransferRoom", levelname, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.LoadLevel)]
        public void LoadLevel(string levelname, object target)
        {
            ProxyMethod("LoadLevel", levelname, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.EnterRealm)]
        public void EnterRealm(int realmId, byte realmState, int elapsed, object target)
        {
            ProxyMethod("EnterRealm", realmId, realmState, elapsed, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SpawnGate)]
        public void SpawnGate(int pid, float width, float height, string prefab, RPCPosition pos, RPCDirection dir, object target)
        {
            ProxyMethod("SpawnGate", pid, width, height, prefab, pos, dir, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SpawnAnimationObject)]
        public void SpawnAnimationObject(int pid, string prefab, RPCPosition rpcpos, RPCDirection rpcdir, object target)
        {
            ProxyMethod("SpawnAnimationObject", pid, prefab, rpcpos, rpcdir, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.ServerSendChatMessage)]
        public void ServerSendChatMessage(byte messagetype, string message, string sender, string whisperTo, int portraitId, byte jobSect,
                                          byte vipLvl, byte faction, bool voice, object target)
        {
            ProxyMethod("ServerSendChatMessage", messagetype, message, sender, whisperTo, portraitId, jobSect, vipLvl, faction, voice, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.BroadcastMessageToClient)]
        public void BroadcastMessageToClient(byte type, string parameters, object target)
        {
            ProxyMethod("BroadcastMessageToClient", type, parameters, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_SendSystemMessageById)]
        public void Ret_SendSystemMessageId(int ret, string optionalparam, bool addToChatLog, object target)
        {
            ProxyMethod("Ret_SendSystemMessageId", ret, optionalparam, addToChatLog, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_SendSystemMessageByStr)]
        public void Ret_SendSystemMessage(string ret, string optionalparam, bool addToChatLog, object target)
        {
            ProxyMethod("Ret_SendSystemMessage", ret, optionalparam, addToChatLog, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SetInspectPlayerInfo)]
        public void SetInspectPlayerInfo(string inspectdata, object target)
        {
            ProxyMethod("SetInspectPlayerInfo", inspectdata, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.StartCountDown)]
        public void StartCountDown(int count, object target)
        {
            ProxyMethod("StartCountDown", count, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OnMissionCompleted)]
        public void OnMissionCompleted(bool success, int countdown, object target)
        {
            ProxyMethod("OnMissionCompleted", success, countdown, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.ShowScoreBoard)]
        public void ShowScoreBoard(bool success, int countdown, int score, int bosspid, object target)
        {
            ProxyMethod("ShowScoreBoard", success, countdown, score, bosspid, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_RaidReward)]
        public void Ret_RaidReward(bool isSuccess, int rewardGrpId, string itemsStr, object target)
        {
            ProxyMethod("Ret_RaidReward", isSuccess, rewardGrpId, itemsStr, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_DungeonEnterConfirmResult)]
        public void Ret_DungeonEnterConfirmResult(int realmId, bool hasEntry, string cancelMember, object target)
        {
            ProxyMethod("Ret_DungeonEnterConfirmResult", realmId, hasEntry, cancelMember, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.ShowRewardDialog)]
        public void ShowRewardDialog(string partyMembersInfo, string iteminfos, int multiplier, object target)
        {
            ProxyMethod("ShowRewardDialog", partyMembersInfo, iteminfos, multiplier, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.BroadcastCutScene)]
        public void BroadcastCutScene(int cutscene, object target)
        {
            ProxyMethod("BroadcastCutScene", cutscene, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SetServerTime)]
        public void SetServerTime(long time, object target)
        {
            ProxyMethod("SetServerTime", time, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SendInfoOnPlayerSpawner)]
        public void SendInfoOnPlayerSpawner(long serverstartupticks, long servernowticks, int activityStatus, long arenarewardticks, bool inspectMode, string charId, object target)
        {
            ProxyMethod("SendInfoOnPlayerSpawner", serverstartupticks, servernowticks, activityStatus, arenarewardticks, inspectMode, charId, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.InitGameSetting)]
        public void InitGameSetting(string gameSetting, string botSetting, object target)
        {
            ProxyMethod("InitGameSetting", gameSetting, botSetting, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OpenUIWindow)]
        public void OpenUIWindow(byte uitype, int itemSlotId, object target)
        {
            ProxyMethod("OpenUIWindow", uitype, itemSlotId, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.UsingChestResult)]
        public void UsingChestResult(Common.UsingChestCode code, object target)
        {
            ProxyMethod("UsingChestResult", code, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SendDonateData)]
        public void SendDonateData(string donateToClientDataStr, Common.DonateReturnCode code, object target)
        {
            ProxyMethod("SendDonateData", donateToClientDataStr, code, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SendDonateReturnCode)]
        public void SendDonateReturnCode(Common.DonateReturnCode code, object target)
        {
            ProxyMethod("SendDonateReturnCode", code, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GetArenaChallenger)]
        public void Ret_GetArenaChallenger(int myrank, int entries, int cooldown, string result, object target)
        {
            ProxyMethod("Ret_GetArenaChallenger", myrank, entries, cooldown, result, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_ArenaClaimReward)]
        public void Ret_ArenaClaimReward(bool success, int myrank, object target)
        {
            ProxyMethod("Ret_ArenaClaimReward", success, myrank, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GetArenaReport)]
        public void Ret_GetArenaReport(string report, object target)
        {
            ProxyMethod("Ret_GetArenaReport", report, target);
        }

        #region Activity
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.ActivityPullHeroResult)]
        public void ActivityPullHeroResult(bool isAttacker, bool isWin, int intimate, object target)
        {
            ProxyMethod("ActivityPullHeroResult", isAttacker, isWin, intimate, target);
        }
        #endregion

        #region Mail

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_HasNewMail)]
        public void Ret_HasNewMail(bool hasNewMail, string mailHUD_ID, object target)
        {
            ProxyMethod("Ret_HasNewMail", hasNewMail, mailHUD_ID, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_RetrieveMail)]
        public void Ret_RetrieveMail(string serializedMailString, object target)
        {
            ProxyMethod("Ret_RetrieveMail", serializedMailString, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_OpenMail)]
        public void Ret_OpenMail(int mailReturnCode, int mailIndex, object target)
        {
            ProxyMethod("Ret_OpenMail", mailReturnCode, mailIndex, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_TakeAttachment)]
        public void Ret_TakeAttachment(int mailReturnCode, int mailIndex, object target)
        {
            ProxyMethod("Ret_TakeAttachment", mailReturnCode, mailIndex, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_TakeAllAttachment)]
        public void Ret_TakeAllAttachment(int mailReturnCode, string lstTakenMailIndexSerialStr, object target)
        {
            ProxyMethod("Ret_TakeAllAttachment", mailReturnCode, lstTakenMailIndexSerialStr, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_DeleteMail)]
        public void Ret_DeleteMail(int mailReturnCode, int mailIndex, object target)
        {
            ProxyMethod("Ret_DeleteMail", mailReturnCode, mailIndex, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_DeleteAllMail)]
        public void Ret_DeleteAllMail(int mailReturnCode, object target)
        {
            ProxyMethod("Ret_DeleteAllMail", mailReturnCode, target);
        }

        #endregion Mail

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_OpenNewInvSlot)]
        public void Ret_OpenNewInvSlot(byte retcode, int nextOpenTime, object target)
        {
            ProxyMethod("Ret_OpenNewInvSlot", retcode, nextOpenTime, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_MassUseItem)]
        public void Ret_MassUseItem(byte result, string currencyOutput, object target)
        {
            ProxyMethod("Ret_MassUseItem", result, currencyOutput, target);
        }

        #region OfflineExp

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OfflineExpRedDot)]
        public void OfflineExpRedDot(bool flag, object target)
        {
            ProxyMethod("OfflineExpRedDot", flag, target);
        }
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OfflineExpGetData)]
        public void OfflineExpGetData(string serializedOfflineExp, object target)
        {
            ProxyMethod("OfflineExpGetData", serializedOfflineExp, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OfflineExpStartReward)]
        public void OfflineExpStartReward(string serializedOfflineExp, object target)
        {
            ProxyMethod("OfflineExpStartReward", serializedOfflineExp, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OfflineExpClaimReward)]
        public void OfflineExpClaimReward(int offlineExpReturnCode, int newlvl, object target)
        {
            ProxyMethod("OfflineExpClaimReward", offlineExpReturnCode, newlvl, target);
        }

        #endregion OfflineExp

        #region Auction
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_AuctionGetAuctionItem)]
        public void Ret_AuctionGetAuctionItem(string stringData, int bidPrice, object target)
        {
            ProxyMethod("Ret_AuctionGetAuctionItem", stringData, bidPrice, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_AuctionGetRecords)]
        public void Ret_AuctionGetRecords(string stringData, object target)
        {
            ProxyMethod("Ret_AuctionGetRecords", stringData, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_AuctionGetBidItems)]
        public void Ret_AuctionGetBidItems(string stringData, object target)
        {
            ProxyMethod("Ret_AuctionGetBidItems", stringData, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_AuctionCollectItem)]
        public void Ret_AuctionCollectItem(byte result, object target)
        {
            ProxyMethod("Ret_AuctionCollectItem", result, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_AuctionPlaceBid)]
        public void Ret_AuctionPlaceBid(byte result, int bidPrice, object target)
        {
            ProxyMethod("Ret_AuctionPlaceBid", result, bidPrice, target);
        }

        #endregion

        #region Guild
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GuildGetGuildInfo)]
        public void Ret_GuildGetGuildInfo(string guildInfoListStr, object target)
        {
            ProxyMethod("Ret_GuildGetGuildInfo", guildInfoListStr, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GuildResult)]
        public void Ret_GuildResult(byte retcode, string param, object target)
        {
            ProxyMethod("Ret_GuildResult", retcode, param, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GuildAdd)]
        public void Ret_GuildAdd(byte retcode, object target)
        {
            ProxyMethod("Ret_GuildAdd", retcode, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GuildCheckName)]
        public void Ret_GuildCheckName(byte retcode, object target)
        {
            ProxyMethod("Ret_GuildCheckName", retcode, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GuildLeave)]
        public void Ret_GuildLeave(byte retcode, object target)
        {
            ProxyMethod("Ret_GuildLeave", retcode, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GuildJoin)]
        public void Ret_GuildJoin(byte retcode, object target)
        {
            ProxyMethod("Ret_GuildJoin", retcode, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GuildGetHistory)]
        public void Ret_GuildGetHistory(string history, object target)
        {
            ProxyMethod("Ret_GuildGetHistory", history, target);
        }
        #endregion

        #region Development
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SendMessageToConsoleCmd)]
        public void SendMessageToConsoleCmd(string msg, object target)
        {
            ProxyMethod("SendMessageToConsoleCmd", msg, target);
        }
        #endregion

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.ReplyValidMonSpawnPos)]
        public void ReplyValidMonSpawnPos(RPCPosition pos, object target)
        {
            ProxyMethod("ReplyValidMonSpawnPos", pos, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_SocialReturnResult)]
        public void Ret_SocialReturnResult(byte retCode, string param, object target)
        {
            ProxyMethod("Ret_SocialReturnResult", retCode, param, target);
        }

        #region IAP

        [RPCUnsuspend]
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GetProductsWithLockGold)]
        public void Ret_GetProductsWithLockGold(string productsWithLockGold, object target)
        {
            ProxyMethod("Ret_GetProductsWithLockGold", productsWithLockGold, target);
        }

        [RPCUnsuspend]
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_VerifyPurchase)]
        public void Ret_VerifyPurchase(int gold, int lockGold, int vipPoints, object target)
        {
            ProxyMethod("Ret_VerifyPurchase", gold, lockGold, vipPoints, target);
        }

        #endregion IAP

        #region Party
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GetPartyList)]
        public void Ret_GetPartyList(string result, object target)
        {
            ProxyMethod("Ret_GetPartyList", result, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SendPartyInvitation)]
        public void SendPartyInvitation(string senderName, object target)
        {
            ProxyMethod("SendPartyInvitation", senderName, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SendFollowInvitation)]
        public void SendFollowInvitation(string senderName, object target)
        {
            ProxyMethod("SendFollowInvitation", senderName, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OnFollowPartyMember)]
        public void OnFollowPartyMember(int pid, string targetName, string levelName, RPCPosition position, object target)
        {
            ProxyMethod("OnFollowPartyMember", pid, targetName, levelName, position, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OnGetPartyMemberPosition)]
        public void OnGetPartyMemberPosition(string currLevelName, RPCPosition position, object target)
        {
            ProxyMethod("OnGetPartyMemberPosition", currLevelName, position, target);
        }
        #endregion

        #region Hero
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_RandomInterestResult)]
        public void Ret_RandomInterestResult(byte interest, object target)
        {
            ProxyMethod("Ret_RandomInterestResult", interest, target);
        }
        #endregion

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_OnCurrencyExchange)]
        public void Ret_OnCurrencyExchange(byte result, byte oldExchangeTime, object target)
        {
            ProxyMethod("Ret_OnCurrencyExchange", result, oldExchangeTime, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GetRandomBoxReward)]
        public void Ret_GetRandomBoxReward(byte result, int reward_id, object target)
        {
            ProxyMethod("Ret_GetRandomBoxReward", result, reward_id, target);
        }

        #region InvitePVP
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_InvitePvpResult)]
        public void Ret_InvitePvpResult(byte retCode, string param, object target)
        {
            ProxyMethod("Ret_InvitePvpResult", retCode, param, target);
        }
        #endregion

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_OnUpdateWardrobe)]
        public void Ret_OnUpdateWardrobe(byte result, object target)
        {
            ProxyMethod("Ret_OnUpdateWardrobe", result, target);
        }

        #region SystemSwitch
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.InitSystemSwitch)]
        public void InitSystemSwitch(string str, object target)
        {
            ProxyMethod("InitSystemSwitch", str, target);
        }
        #endregion

        #region world boss
        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GetWorldBossList)]
        public void Ret_GetWorldBossList(string result, object target)
        {
            ProxyMethod("Ret_GetWorldBossList", result, target);
        }

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GetWorldBossDmgList)]
        public void Ret_GetWorldBossDmgList(string result, object target)
        {
            ProxyMethod("Ret_GetWorldBossDmgList", result, target);
        }
        #endregion

        [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.LootItemDisplay)]
        public void LootItemDisplay(string data, object target)
        {
            ProxyMethod("LootItemDisplay", data, target);
        }
    }
}