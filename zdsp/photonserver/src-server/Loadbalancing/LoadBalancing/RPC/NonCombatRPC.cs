using System;
using System.Collections.Generic;
using Photon.LoadBalancing.Operations;
using Photon.LoadBalancing.GameServer;
using Zealot.Common.RPC;
using Zealot.Repository;

namespace Zealot.RPC
{
    using Zealot.Common.RPC;
    using Server.EventMessage;

    public class NonCombatRPC : ServerRPCBase
    {
        public NonCombatRPC(object zrpc) :
            base(typeof(NonCombatRPC), (byte)OperationCode.NonCombat, true, zrpc)
        {
            SetMainContext(typeof(GameLogic), RPCCategory.NonCombat);
        }

        public RPCBroadcastData GetSerializedRPC(ServerNonCombatRPCMethods rpcMethodId, params object[] args)
        {
            return base.GetSerializedRPC((byte)rpcMethodId, args);
        }

        #region LeaderBoard
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_GetLeaderBoard)]
        public void Ret_GetLeaderBoard(byte lbType, string lbData, object target)
        {
            ProxyMethod("Ret_GetLeaderBoard", lbType, lbData, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_GetLeaderBoardAvatar)]
        public void Ret_GetLeaderBoardAvatar(byte lbType, string inspectData, object target)
        {
            ProxyMethod("Ret_GetLeaderBoardAvatar", lbType, inspectData, target);
        }
        #endregion

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_StorePurchaseItem)]
        public void Ret_StorePurchaseItem(int retCode, int shelveNo, int currencyAmt1, int currencyAmt2, object target)
        {
            ProxyMethod("Ret_StorePurchaseItem", retCode, shelveNo, currencyAmt1, currencyAmt2, target);
        }
        #region GuildQuest
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_GetGuildQuest)]
        public void Ret_GetGuildQuest(string str, object target)
        {
            ProxyMethod("Ret_GetGuildQuest", str, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_GuildOperationResult)]
        public void Ret_GuildOperationResult(byte error,  object target)
        {
            ProxyMethod("Ret_GuildOperationResult", error, target);
        }
        #endregion

        #region Item Mall

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_ItemMallPurchaseItem)]
        public void Ret_ItemMallPurchaseItem(int itemMallReturnCode,string kopioPurchaseLimit,string gmPurchaseLimit, object target)
        {
            ProxyMethod("Ret_ItemMallPurchaseItem", itemMallReturnCode, kopioPurchaseLimit, gmPurchaseLimit, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_ItemMallInit)]
        public void Ret_ItemMallInit(string serializedMallData, object target)
        {
            ProxyMethod("Ret_ItemMallInit", serializedMallData, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_ItemMallGetList)]        
        public void Ret_ItemMallGetList(string itemInfoData,int count, int cat,bool cleanbefore, object target)
        {
            ProxyMethod("Ret_ItemMallGetList", itemInfoData, count,cat, cleanbefore, target);
        }
        
        #endregion Item Mall

        #region Lottery
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_LotteryGetSimpleInfo)]
        public void Ret_LotteryGetSimpleInfo(int lottery_id, object target)
        {
            ProxyMethod("Ret_LotteryGetSimpleInfo", lottery_id, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_LotteryGetInfo)]
        public void Ret_LotteryGetInfo(int lottery_id, string items, int cost_gold, string point_reward, object target)
        {
            ProxyMethod("Ret_LotteryGetInfo", lottery_id, items, cost_gold, point_reward, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_ShowLotteryRollOneResult)]
        public void Ret_ShowLotteryRollOneResult(int lottery_id, int item_id, int item_count, int free, int extra, int get_point, object target)
        {
            ProxyMethod("Ret_ShowLotteryRollOneResult", lottery_id, item_id, item_count, free, extra, get_point, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_ShowLotteryRollTenResult)]
        public void Ret_ShowLotteryRollTenResult(int lottery_id, string item_id, string item_count, int free, int extra, int get_point, object target)
        {
            ProxyMethod("Ret_ShowLotteryRollTenResult", lottery_id, item_id, item_count, free, extra, get_point, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_LotteryRollFailed)]
        public void Ret_LotteryRollFailed(int sys_msg_id, object target)
        {
            ProxyMethod("Ret_LotteryRollFailed", sys_msg_id, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_LotteryGetPointReward)]
        public void Ret_LotteryGetPointRewardResult(int lottery_id, int point, string items, string counts, object target)
        {
            ProxyMethod("Ret_LotteryGetPointRewardResult",lottery_id, point, items, counts, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_LotteryUsePointItem)]
        public void Ret_LotteryUsePointItemResult(int ret_code, int point, object target)
        {
            ProxyMethod("Ret_LotteryUsePointItemResult", ret_code, point, target);
        }
        #endregion

        #region EquipmentUpgrade
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.EquipmentUpgradeEquipmentFailed)]
        public void EquipmentUpgradeEquipmentFailed(object target)
        {
            ProxyMethod("EquipmentUpgradeEquipmentFailed", target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.EquipmentUpgradeEquipmentSuccess)]
        public void EquipmentUpgradeEquipmentSuccess(object target)
        {
            ProxyMethod("EquipmentUpgradeEquipmentSuccess", target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.EquipmentReformEquipmentFailed)]
        public void EquipmentReformEquipmentFailed(object target)
        {
            ProxyMethod("EquipmentReformEquipmentFailed", target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.EquipmentReformEquipmentSuccess)]
        public void EquipmentReformEquipmentSuccess(object target)
        {
            ProxyMethod("EquipmentReformEquipmentSuccess", target);
        }
        #endregion

        #region ReviveItem
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.RequestReviveItem)]
        public void RequestReviveItem(string requestor, int requestId, object target)
        {
            ProxyMethod("RequestReviveItem", requestor, requestId, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.ConfirmAcceptReviveItem)]
        public void ConfirmAcceptReviveItem(int sessionId, string otherPlayer, string traderStatus, object target)
        {
            ProxyMethod("ConfirmAcceptReviveItem", sessionId, otherPlayer, traderStatus, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.ConfirmCancelReviveItem)]
        public void ConfirmCancelReviveItem(object target)
        {
            ProxyMethod("ConfirmCancelReviveItem", target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.ConfirmCompleteReviveItem)]
        public void ConfirmCompleteReviveItem(object target)
        {
            ProxyMethod("ConfirmCompleteReviveItem", target);
        }
        #endregion

        #region SevenDays
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.SevenDaysNotEnoughGold)]
        public void SevenDaysNotEnoughGold(object target)
        {
            ProxyMethod("SevenDaysNotEnoughGold", target);
        }
        #endregion

        #region Welfare
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_WelfareNotEnoughGold)]
        public void Ret_WelfareNotEnoughGold(object target)
        {
            ProxyMethod("Ret_WelfareNotEnoughGold", target);
        }
        #endregion

        #region QuestExtraRewards
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.QERFailedGold)]
        public void QERFailedGold(object target)
        {
            ProxyMethod("QERFailedGold", target);
        }
        #endregion

        #region Misc
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.SendString)]
        public void SendString(string data, GameClientPeer target)
        {
            ProxyMethod("SendString", data, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.KickWithReason)]
        public void KickWithReason(string reason, GameClientPeer target)
        {
            ProxyMethod("KickWithReason", reason, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.GMMessage)]
        public void GMMessage(string reason, int mode, GameClientPeer target)
        {
            ProxyMethod("GMMessage", reason, mode, target);
        }
        #endregion

        #region ExchangeShop
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_ExchangeShopPurchase)]
        public void Ret_ExchangeShopPurchase(int retcode, int categoryid, int recipeid, int ic1, int ic2, int ic3, int ic4, object target)
        {
            ProxyMethod("Ret_ExchangeShopPurchase", retcode, categoryid, recipeid, ic1, ic2, ic3, ic4, target);
        }
        #endregion

		#region Store
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_StoreInit)]
        public void Ret_StoreInit(string scJsonString, object target)
		{
            ProxyMethod("Ret_StoreInit", scJsonString, target);
        }
        #endregion

        #region NPCStore
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_NPCStoreInit)]
        public void Ret_NPCStoreInit(string scJsonString, object target)
        {
            ProxyMethod("Ret_NPCStoreInit", scJsonString, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_NPCStoreGetPlayerTransactions)]
        public void Ret_NPCStoreGetPlayerTransactions(string scJsonString, object target)
        {
            ProxyMethod("Ret_NPCStoreGetPlayerTransactions", scJsonString, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_NPCStoreBuy)]
        public void Ret_NPCStoreBuy(string scJsonString, object target)
        {
            ProxyMethod("Ret_NPCStoreBuy", scJsonString, target);
        }
        #endregion

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_TransferServer)]
        public void Ret_TransferServer(int serverid, string serverAddress, object target)
        {
            ProxyMethod("Ret_TransferServer", serverid, serverAddress, target);
        }

        #region Quest
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_DeleteQuest)]
        public void Ret_DeleteQuest(bool result, int questid, object target)
        {
            ProxyMethod("Ret_DeleteQuest", result, questid, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_ResetQuest)]
        public void Ret_ResetQuest(bool result, int questid, object target)
        {
            ProxyMethod("Ret_ResetQuest", result, questid, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_CompleteQuest)]
        public void Ret_CompleteQuest(bool result, int questid, object target)
        {
            ProxyMethod("Ret_CompleteQuest", result, questid, target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_InteractAction)]
        public void Ret_InteractAction(object target)
        {
            ProxyMethod("Ret_InteractAction", target);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_QuestFull)]
        public void Ret_QuestFull(int questid, object target)
        {
            ProxyMethod("Ret_QuestFull", questid, target);
        }
        #endregion

        #region CharacterInfo
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_CharacterInfoSpendStatsPoints)]
        public void Ret_CharacterInfoSpendStatsPoints(int retVal, object target)
        {
            ProxyMethod("Ret_CharacterInfoSpendStatsPoints", retVal, target);
        }
        #endregion

        #region Skill
        [RPCMethod(RPCCategory.NonCombat, (byte)ServerNonCombatRPCMethods.Ret_AddToSkillInventory)]
        public void Ret_AddToSkillInventory(byte result, int skillid, int skillpoint, int money, object target)
        {
            ProxyMethod("Ret_AddToSkillInventory", result, skillid, skillpoint, money, target);
        }
        #endregion
    }
}

namespace Photon.LoadBalancing.GameServer
{
    using Zealot.Common;
    using Zealot.Server.Entities;
    using Zealot.RPC;
    using Zealot.Server.Rules;
    using Zealot.Repository;
    using Zealot.Server.SideEffects;
    using Mail;
    using Lottery;
    using Newtonsoft.Json;
    using Photon.LoadBalancing.GameServer.NPCStore;

    public partial class GameLogic
    {
        #region LeaderBoard
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.GetLeaderBoard)]
        public void GetLeaderBoard(byte lbType, GameClientPeer peer)
        {
            GameApplication.Instance.Leaderboard.PlayerRequestLeaderboard(peer, lbType);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.GetLeaderBoard)]
        public void GetLeaderBoardProxy(object[] args)
        {
            GetLeaderBoard((byte)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.GetLeaderBoardAvatar)]
        public void GetLeaderBoardAvatar(byte lbType, string charName, GameClientPeer peer)
        {
            GameApplication.Instance.Leaderboard.PlayerRequestAvatar(peer, lbType, charName);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.GetLeaderBoardAvatar)]
        public void GetLeaderBoardAvatarProxy(object[] args)
        {
            GetLeaderBoardAvatar((byte)args[0], (string)args[1], (GameClientPeer)args[2]);
        }
        #endregion

        #region ItemMall

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ItemMallPurchaseItem)]
        public void ItemMallPurchaseItem(int id, bool isGM, int stackToBuy, long lastGrabDateTime, GameClientPeer peer)
        {
            int itemMallReturnCode = peer.ItemMallPurchaseItem(id, isGM, stackToBuy, lastGrabDateTime);

            string limitKopio = ItemMall.ItemMallManager.Instance.GetPurchaseLimit(peer.mChar, false);
            string limitGM = ItemMall.ItemMallManager.Instance.GetPurchaseLimit(peer.mChar, true);

            ZRPC.NonCombatRPC.Ret_ItemMallPurchaseItem(itemMallReturnCode, limitKopio, limitGM, peer);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ItemMallPurchaseItem)]
        public void ItemMallPurchaseItemProxy(object[] args)
        {
            ItemMallPurchaseItem((int)args[0], (bool)args[1], (int)args[2], (long)args[3], (GameClientPeer)args[4]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ItemMallInit)]
        public void ItemMallInit(GameClientPeer peer)
        {
            string serializedMallData = peer.ItemMallInit_Client_MallData();
            ZRPC.NonCombatRPC.Ret_ItemMallInit(serializedMallData, peer);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ItemMallInit)]
        public void ItemMallInitProxy(object[] args)
        {
            ItemMallInit((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ItemMallGetList)]
        public void ItemMallGetList(int category, long ticks, GameClientPeer peer)
        {            
            var serializedMallData = peer.ItemMallInit_Client_ItemData(category, ticks);
            if (serializedMallData == null)
            {
                ZRPC.NonCombatRPC.Ret_ItemMallGetList("", 0, category,false, peer);
            }
            else
            {
                bool cleanup = true;
                foreach (var s in serializedMallData)
                {
                    ZRPC.NonCombatRPC.Ret_ItemMallGetList(s, serializedMallData.Count, category, cleanup, peer);
                    cleanup = false;//only the first one require cleanup
                }
            }
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ItemMallGetList)]
        public void ItemMallGetListProxy(object[] args)
        {
            ItemMallGetList((int)args[0], (long)args[1], (GameClientPeer)args[2]);
        }

        #endregion ItemMall

        #region Tutorial
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.TriggerTutorial)]
        public void OnTriggerTutorial(int id, GameClientPeer peer)
        {
            peer.OnTriggerTutorial(id);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.TriggerTutorial)]
        public void OnTriggerTutorialProxy(object[] args)
        {
            OnTriggerTutorial((int)args[0], (GameClientPeer)args[1]);
        }
        #endregion

        #region Welfare
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimSignInPrize)]
        public void WelfareClaimSignInPrize(int year, int month, int day, int cltYear, int cltMonth, int cltDay, bool oldData, GameClientPeer peer)
        {
            peer.OnWelfareClaimSignInPrize(year, month, day, cltYear, cltMonth, cltDay, oldData);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimSignInPrize)]
        public void WelfareClaimSignInPrizeProxy(object[] args)
        {
            WelfareClaimSignInPrize((int)args[0], (int)args[1], (int)args[2], (int)args[3], (int)args[4], (int)args[5], (bool)args[6], (GameClientPeer)args[7]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimOnlinePrizes)]
        public void WelfareClaimOnlinePrizes(GameClientPeer peer)
        {
            peer.OnWelfareClaimOnlinePrizes();
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimOnlinePrizes)]
        public void WelfareClaimOnlinePrizesProxy(object[] args)
        {
            WelfareClaimOnlinePrizes((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimOnlinePrizesSingle)]
        public void WelfareClaimOnlinePrizesSingle(int dataid, GameClientPeer peer)
        {
            peer.OnWelfareClaimOnlinePrizesSingle(dataid);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimOnlinePrizesSingle)]
        public void WelfareClaimOnlinePrizesSingleProxy(object[] args)
        {
            WelfareClaimOnlinePrizesSingle((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareBuyOpenServiceFund)]
        public void WelfareBuyOpenServiceFund(GameClientPeer peer)
        {
            peer.OnWelfareBuyOpenServiceFunds();
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareBuyOpenServiceFund)]
        public void WelfareBuyOpenServiceFundProxy(object[] args)
        {
            WelfareBuyOpenServiceFund((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimOpenServiceFundGoldReward)]
        public void WelfareClaimOpenServiceFundGoldReward(int dataid, GameClientPeer peer)
        {
            peer.OnWelfareClaimOpenServiceFundGoldReward(dataid);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimOpenServiceFundGoldReward)]
        public void WelfareClaimOpenServiceFundGoldRewardProxy(object[] args)
        {
            WelfareClaimOpenServiceFundGoldReward((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimOpenServiceFundItemReward)]
        public void WelfareClaimOpenServiceFundItemReward(int dataid, GameClientPeer peer)
        {
            peer.OnWelfareClaimOpenServiceFundItemReward(dataid);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimOpenServiceFundItemReward)]
        public void WelfareClaimOpenServiceFundItemRewardProxy(object[] args)
        {
            WelfareClaimOpenServiceFundItemReward((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimFirstGoldCreditRewards)]
        public void WelfareClaimFirstGoldCreditRewards(GameClientPeer peer)
        {
            peer.OnWelfareClaimFirstGoldCredit();
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimFirstGoldCreditRewards)]
        public void WelfareClaimFirstGoldCreditRewardsProxy(object[] args)
        {
            WelfareClaimFirstGoldCreditRewards((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimTotalCreditReward)]
        public void WelfareClaimTotalCreditReward(int dataid, GameClientPeer peer)
        {
            peer.OnWelfareClaimTotalCreditReward(dataid);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimTotalCreditReward)]
        public void WelfareClaimTotalCreditRewardProxy(object[] args)
        {
            WelfareClaimTotalCreditReward((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimTotalSpendReward)]
        public void WelfareClaimTotalSpendReward(int dataid, GameClientPeer peer)
        {
            peer.OnWelfareClaimTotalSpendReward(dataid);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimTotalSpendReward)]
        public void WelfareClaimTotalSpendRewardProxy(object[] args)
        {
            WelfareClaimTotalSpendReward((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldBuyMCard)]
        public void WelfareDailyGoldBuyMCard(GameClientPeer peer)
        {
            peer.OnWelfareDailyGoldBuyMonthlyCard();
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldBuyMCard)]
        public void WelfareDailyGoldBuyMCardProxy(object[] args)
        {
            WelfareDailyGoldBuyMCard((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldClaimMCardGold)]
        public void WelfareDailyGoldClaimMCardGold(GameClientPeer peer)
        {
            peer.OnWelfareDailyGoldClaimMonthlyCardGold();
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldClaimMCardGold)]
        public void WelfareDailyGoldClaimMCardGoldProxy(object[] args)
        {
            WelfareDailyGoldClaimMCardGold((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldBuyPCard)]
        public void WelfareDailyGoldBuyPCard(GameClientPeer peer)
        {
            peer.OnWelfareDailyGoldBuyPermanentCard();
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldBuyPCard)]
        public void WelfareDailyGoldBuyPCardProxy(object[] args)
        {
            WelfareDailyGoldBuyPCard((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldClaimPCardGold)]
        public void WelfareDailyGoldClaimPCardGold(GameClientPeer peer)
        {
            peer.OnWelfareDailyGoldClaimPermanentCardGold();
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldClaimPCardGold)]
        public void WelfareDailyGoldClaimPCardGoldProxy(object[] args)
        {
            WelfareDailyGoldClaimPCardGold((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareGoldJackpotGetResult)]
        public void WelfareGoldJackpotGetResult(GameClientPeer peer)
        {
            peer.OnWelfareGoldJackpotGetResult();
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareGoldJackpotGetResult)]
        public void WelfareGoldJackpotGetResultProxy(object[] args)
        {
            WelfareGoldJackpotGetResult((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimContLoginReward)]
        public void WelfareClaimContLoginReward(int rewardId, GameClientPeer peer)
        {
            peer.OnWelfareClaimContLoginReward(rewardId);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimContLoginReward)]
        public void WelfareClaimContLoginRewardProxy(object[] args)
        {
            WelfareClaimContLoginReward((int)args[0], (GameClientPeer)args[1]);
        }
        #endregion

        #region QuestExtraReward
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERFinishTask)]
        public void QERFinishTask(int dataid, GameClientPeer peer)
        {
            peer.OnQERFinishTask(dataid);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERFinishTask)]
        public void QERFinishTaskProxy(object[] args)
        {
            QERFinishTask((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERFinishTaskAll)]
        public void QERFinishTaskAll(string dataids, GameClientPeer peer)
        {
            peer.OnQERFinishTaskAll(dataids);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERFinishTaskAll)]
        public void QERFinishTaskAllProxy(object[] args)
        {
            QERFinishTaskAll((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERClaimTaskReward)]
        public void QERClaimTaskReward(int dataid, GameClientPeer peer)
        {
            peer.OnQERClaimTaskReward(dataid);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERClaimTaskReward)]
        public void QERClaimTaskRewardProxy(object[] args)
        {
            QERClaimTaskReward((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERClaimTaskRewardAll)]
        public void QERClaimTaskRewardAll(string dataids, GameClientPeer peer)
        {
            peer.OnQERClaimTaskRewardAll(dataids);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERClaimTaskRewardAll)]
        public void QERClaimTaskRewardAllProxy(object[] args)
        {
            QERClaimTaskRewardAll((string)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERClaimBoxReward)]
        public void QERClaimBoxReward(int dataid, GameClientPeer peer)
        {
            peer.OnQERClaimBoxReward(dataid);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERClaimBoxReward)]
        public void QERClaimBoxRewardProxy(object[] args)
        {
            QERClaimBoxReward((int)args[0], (GameClientPeer)args[1]);
        }
        #endregion

        #region Lottery
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryGetSimpleInfo)]
        public void LotteryGetSimpleInfo(GameClientPeer peer)
        {
            LotteryManager.Instance.GetLotterySimpleInfoToClient(peer);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryGetSimpleInfo)]
        public void LotteryGetSimpleInfoProxy(object[] args)
        {
            LotteryGetSimpleInfo((GameClientPeer)args[0]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryGetInfo)]
        public void LotteryGetLotteryInfo(int lottery_id, GameClientPeer peer)
        {
            LotteryManager.Instance.GetLotteryInfoToClient(peer, lottery_id);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryGetInfo)]
        public void LotteryGetLotteryInfoProxy(object[] args)
        {
            LotteryGetLotteryInfo((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryRollOne)]
        public void LotteryRollOne(int lottery_id, int cost_type, GameClientPeer peer)
        {
            LotteryManager.Instance.LotteryOne(peer, lottery_id, cost_type);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryRollOne)]
        public void LotteryRollOneProxy(object[] args)
        {
            LotteryRollOne((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryRollTen)]
        public void LotteryRollTen(int lottery_id, int cost_type, GameClientPeer peer)
        {
            LotteryManager.Instance.LotteryTen(peer, lottery_id, cost_type);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryRollTen)]
        public void LotteryRollTenProxy(object[] args)
        {
            LotteryRollTen((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryGetPointReward)]
        public void LotteryGetPointReward(int lottery_id, int point, GameClientPeer peer)
        {
            LotteryManager.Instance.GetPointRewardItem(peer, lottery_id, point);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryGetPointReward)]
        public void LotteryGetPointRewardProxy(object[] args)
        {
            LotteryGetPointReward((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryUsePointItem)]
        public void LotteryUsePointItem(int lottery_id, int item_id, GameClientPeer peer)
        {
            LotteryManager.Instance.UsePointItem(peer, lottery_id, item_id);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryUsePointItem)]
        public void LotteryUsePointItemProxy(object[] args)
        {
            LotteryUsePointItem((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }
        #endregion

        #region EquipmentUpgrade
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipmentUpgradeEquipment)]
        public void EquipmentUpgradeEquipment(int slotId, bool isEquipped, bool isUseGenMaterial, bool isSafeUpgrade, bool isSafeUseEquip, bool isSafeGenMat, int safeEquipSlotId, GameClientPeer peer)
        {
            peer.OnEquipmentUpgradeEquipment(slotId, isEquipped, isUseGenMaterial, isSafeUpgrade, isSafeUseEquip, isSafeGenMat, safeEquipSlotId);
            peer.mPlayer.CombatStats.ComputeAll();
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipmentReformEquipment)]
        public void EquipmentReformEquipment(int slotId, bool isEquipped, GameClientPeer peer)
        {
            //peer.EquipmentReformEquipment(slotId, isEquipped);
            peer.mPlayer.CombatStats.ComputeAll();
        }

        //[RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipSlotGem)]
        //public void EquipSlotGem(int equipSlotId, int gemGrp, int gemSlot, int gemId, GameClientPeer peer)
        //{
        //    peer.OnEquipGemSlotItem(equipSlotId, gemGrp, gemSlot, gemId);
        //    peer.mPlayer.CombatStats.ComputeAll();
        //}

        //[RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.UnequipSlotGem)]
        //public void UnequipSlotGem(int equipSlotId, int gemGrp, int gemSlot, GameClientPeer peer)
        //{
        //    peer.OnUnequipGemSlotItem(equipSlotId, gemGrp, gemSlot);
        //    peer.mPlayer.CombatStats.ComputeAll();
        //}

        //[RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipAutoUpgradeGem)]
        //public void EquipAutoUpgradeGem(GameClientPeer peer)
        //{
        //    peer.OnEquipAutoUpgradeGem();
        //    peer.mPlayer.CombatStats.ComputeAll();
        //}
        #endregion

        #region Equipment
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.HideHelm)]
        public void HideHelm(bool hide, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player != null && player.EquipmentStats.HideHelm != hide)
            {
                player.EquipmentStats.HideHelm = hide;
                peer.CharacterData.EquipmentInventory.HideHelm = hide;
            }
        }
        #endregion

        #region ReviveItem
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StartReviveItemRequest)]
        public void StartReviveItemRequest(string requestor, string requestee, int itemId, GameClientPeer peer)
        {
            GameClientPeer requesteePeer = GameApplication.Instance.GetCharPeer(requestee);
            if(requesteePeer == null)
            {
                peer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_OtherPlayerDisconnected"), "", false, peer);

                return;
            }

            if(requesteePeer.mPlayer.IsAlive())
            {
                peer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_RequesteeNotDead"), "", false, peer);

                return;
            }

            requesteePeer.StartReviveItemRequest(requestor, requestee, itemId);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StartReviveItemRequest)]
        public void StartReviveItemRequestProxy(object[] args)
        {
            StartReviveItemRequest((string)args[0], (string)args[1], (int)args[2], (GameClientPeer)args[3]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RejectReviveItem)]
        public void RejectReviveItem(int requestId, GameClientPeer peer)
        {
            peer.RejectReviveItem(requestId);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RejectReviveItem)]
        public void RejectReviveItemProxy(object[] args)
        {
            RejectReviveItem((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.AcceptReviveItem)]
        public void AcceptReviveItem(int requestId, GameClientPeer peer)
        {
            peer.AcceptReviveItem(requestId);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.AcceptReviveItem)]
        public void AcceptReviveItemProxy(object[] args)
        {
            AcceptReviveItem((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RequestCancelReviveItem)]
        public void RequestCancelReviveItem(int sessionId, GameClientPeer peer)
        {
            peer.CancelReviveItem(sessionId);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RequestCancelReviveItem)]
        public void RequestCancelReviveItemProxy(object[] args)
        {
            RequestCancelReviveItem((int)args[0], (GameClientPeer)args[1]);
        }
        #endregion

        #region ExchangeShop
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ExchangeShopPurchase)]
        public void ExchangeShopPurchase(int categoryid, int recipeid, GameClientPeer peer)
        {
            int retcode = 0;
            Kopio.JsonContracts.ExchangeShopItemJson itemInfo = ExchangeShopRepo.mExchangeShopInventoryMap2[categoryid][recipeid-1];
            var dictTotalCountMap = ExchangeShopRepo.mExchangeShopItemReqTotalCountMap[recipeid];
            Dictionary<int, int> exLeftMap = JsonConvert.DeserializeObject<Dictionary<int, int>>(peer.mPlayer.ExchangeShopSynStats.exchangeLeftMapJsonString);

            //Check if player has sufficient material
            foreach (var reqItem in dictTotalCountMap)
            {
                if (peer.mInventory.GetItemStackCountByItemId((ushort)reqItem.Key) < reqItem.Value)
                {
                    retcode = 1;
                    break;
                }
            }

            //Check if player has sufficient inventory space
            if (retcode == 0 && !peer.mInventory.mInvData.HasEmptySlot())
            {
                retcode = 2;
            }

            //Check if player has exceed exchange times
            else if (exLeftMap[recipeid] == 0)
            {
                retcode = 3;
            }

            //If player somehow able to see this item
            else if (itemInfo.rewarditem_reqlevel > peer.mPlayer.PlayerSynStats.Level)
            {
                retcode = 4;
            }

            //Success
            else
            {
                //Add item to player inventory
                peer.mInventory.AddItemsIntoInventory((ushort)itemInfo.rewarditem_id, itemInfo.rewarditem_count, true, "Exchange");

                //Decrease exchange count
                exLeftMap[recipeid]--;
                peer.mPlayer.ExchangeShopSynStats.exchangeLeftMapJsonString = JsonConvert.SerializeObject(exLeftMap);

                //Decrease item
                peer.mInventory.DeductItem((ushort)itemInfo.item1_id, (ushort)itemInfo.item1_count, "Exchange");
                peer.mInventory.DeductItem((ushort)itemInfo.item2_id, (ushort)itemInfo.item2_count, "Exchange");
                peer.mInventory.DeductItem((ushort)itemInfo.item3_id, (ushort)itemInfo.item3_count, "Exchange");
                peer.mInventory.DeductItem((ushort)itemInfo.item4_id, (ushort)itemInfo.item4_count, "Exchange");

                //Check if need rare item notification
                RareItemNotificationRules.CheckNotification(itemInfo.rewarditem_id, peer.mPlayer.PlayerSynStats.name);
            }

            int itemcount1 = peer.mInventory.GetItemStackCountByItemId((ushort)itemInfo.item1_id);
            int itemcount2 = peer.mInventory.GetItemStackCountByItemId((ushort)itemInfo.item2_id);
            int itemcount3 = peer.mInventory.GetItemStackCountByItemId((ushort)itemInfo.item3_id);
            int itemcount4 = peer.mInventory.GetItemStackCountByItemId((ushort)itemInfo.item4_id);
            //Send player the result
            peer.ZRPC.NonCombatRPC.Ret_ExchangeShopPurchase(retcode, categoryid, recipeid, itemcount1, itemcount2, itemcount3, itemcount4, peer);
        }
        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ExchangeShopPurchase)]
        public void ExchangeShopPurchaseProxy(object[] args)
        {
            ExchangeShopPurchase((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }
        #endregion

        #region guildquest
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ClientGuildQuestOperation)]
        public void ClientGuildQuestOperation(int ope, int qid, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            GuildQuestOperation questope = (GuildQuestOperation)ope;
            if (questope == GuildQuestOperation.Fetch)
            {
                player.Slot.CharacterData.GuildQuests.ComputeTime();
                if (player.Slot.CharacterData.GuildQuests.questlist.Count == 0)
                    player.Slot.CharacterData.GuildQuests.RefreshQuests(player.PlayerSynStats.Level, false);
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
                string str = JsonConvert.SerializeObject(player.Slot.CharacterData.GuildQuests, Formatting.None, jsonSetting);
                peer.ZRPC.NonCombatRPC.Ret_GetGuildQuest(str, peer);

            }
            else if (questope == GuildQuestOperation.Refresh)
            {
                int error;
                bool resultok = player.OnGuildQuestOperation(questope, qid, out error);
                if (resultok)
                {
                    JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
                    string str = JsonConvert.SerializeObject(player.Slot.CharacterData.GuildQuests, Formatting.None, jsonSetting);
                    peer.ZRPC.NonCombatRPC.Ret_GetGuildQuest(str, peer);
                }
                else
                {
                    peer.ZRPC.NonCombatRPC.Ret_GuildOperationResult((byte)error, peer);
                }
            }
            else
            {
                //player.Slot.CharacterData.GuildQuests.ComputeTime();
                int error = 0;
                bool success = player.OnGuildQuestOperation(questope, qid, out error);
                if (success)
                {
                    JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
                    string str = JsonConvert.SerializeObject(player.Slot.CharacterData.GuildQuests, Formatting.None, jsonSetting);
                    peer.ZRPC.NonCombatRPC.Ret_GetGuildQuest(str, peer);
                }
                else
                {
                    peer.ZRPC.NonCombatRPC.Ret_GuildOperationResult((byte)error, peer);
                }
            }
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ClientGuildQuestOperation)]
        public void ClientGuildQuestOperationProxy(object[] args)
        {
            ClientGuildQuestOperation((int)args[0], (int)args[1], (GameClientPeer)args[2]);
        }
        #endregion

        #region PortraitUI
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.OldenPortrait)]
        public void OldenPortrait(string serializedPortraitIDLst, GameClientPeer peer)
        {
            List<int> portraitIDList = JsonConvert.DeserializeObject<List<int>>(serializedPortraitIDLst);

            //Update the info
            peer.mPlayer.PortraitDataStats.SetPortraitData_OldPortrait(portraitIDList);
        }
        #endregion

        #region Store
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StoreInit)]
        public void StoreInit(int categoryID, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            StoreRules.StoreInit(peer);

            StoreRPCData data = new StoreRPCData();
            //Send category info back
            //Send category tabs back
            data.Init(peer.CharacterData.StoreData, categoryID);

            string scString = JsonConvert.SerializeObject(data);
            peer.ZRPC.NonCombatRPC.Ret_StoreInit(scString, peer);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StoreInit)]
        public void StoreInitProxy(object[] args)
        {
            StoreInit((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StoreRefresh)]
        public void StoreRefresh(int category, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            StoreRules.RefreshCategory(category, peer);

            StoreRPCData data = new StoreRPCData();
            //Send category info back
            //Send category tabs back
            data.Init(peer.CharacterData.StoreData, category);

            string scString = JsonConvert.SerializeObject(data);
            peer.ZRPC.NonCombatRPC.Ret_StoreInit(scString, peer);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StoreRefresh)]
        public void StoreRefreshProxy(object[] args)
        {
            StoreRefresh((int)args[0], (GameClientPeer)args[1]);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StorePurchase)]
        public void StorePurchase(int storeID, int stackAmt, int shelveNo, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;
            int cur1, cur2;
            var retCode = StoreRules.StorePurchase(storeID, stackAmt, shelveNo, out cur1, out cur2, peer);
            peer.ZRPC.NonCombatRPC.Ret_StorePurchaseItem((int)retCode, shelveNo, cur1, cur2, peer);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StorePurchase)]
        public void StorePurchaseProxy(object[] args)
        {
            StorePurchase((int)args[0], (int)args[1], (int)args[2], (GameClientPeer)args[3]);
        }
        #endregion

        #region NPCStore
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.NPCStoreInit)]
        public async void NPCStoreInit(int storeid, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;

            var starttime = DateTime.Now;

            var npcstores = await NPCStoreManager.InstanceAsync().ConfigureAwait(false);

            var contains = npcstores.storedata.ContainsKey(storeid);
            if (contains)
            {
                string scString = JsonConvert.SerializeObject(npcstores.storedata[storeid]);
                peer.ZRPC.NonCombatRPC.Ret_NPCStoreInit(scString, peer);
            }
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.NPCStoreGetPlayerTransactions)]
        public async void NPCStoreGetPlayerTransactions(int storeid, GameClientPeer peer)
        {
            var transactions = await GameApplication.dbGM.NPCStoreGMRepo.GetPlayerStoreTransactions(peer.mPlayer.Name).ConfigureAwait(false);
            peer.ZRPC.NonCombatRPC.Ret_NPCStoreGetPlayerTransactions(JsonConvert.SerializeObject(transactions), peer);
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.NPCStoreBuy)]
        public async void NPCStoreBuy(int storeid, int itemlistid, int purchaseamount, string charid, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null)
                return;

            var npcstores = await NPCStoreManager.InstanceAsync().ConfigureAwait(false);
            var store = npcstores.storedata[storeid];
            var storeentry = npcstores.storedata[storeid].inventory[itemlistid];

            // validate purchase
            var transactions = await GameApplication.dbGM.NPCStoreGMRepo.GetPlayerStoreTransactions(charid).ConfigureAwait(false);
            if (transactions == null) transactions = new Dictionary<string, NPCStoreInfo.Transaction>();

            var transactionkey = storeentry.Key();
            NPCStoreInfo.Transaction transaction = null;
            if (transactions.ContainsKey(transactionkey))
            {
                if (transactions[transactionkey].remaining < purchaseamount)
                {
                    peer.ZRPC.NonCombatRPC.Ret_NPCStoreBuy("purchase limit exceeded", peer);
                    return;
                }

                transactions[transactionkey].remaining -= purchaseamount;
                transaction = transactions[transactionkey];
            }
            else
            {
                var bought_time = DateTime.Now;

                var t = new NPCStoreInfo.Transaction();
                t.storeitem = storeentry;
                transactions.Add(t.storeitem.Key(), t);
            }

            bool buyresult = false;
            switch (storeentry.SoldType)
            {
                case NPCStoreInfo.SoldCurrencyType.Normal:
                    buyresult = peer.mPlayer.DeductMoney(storeentry.SoldValue, "NPCStoreBuy");
                    break;

                case NPCStoreInfo.SoldCurrencyType.Auction:
                    buyresult = peer.mPlayer.DeductGold(storeentry.SoldValue, false, true, "NPCStoreBuy");
                    break;
            }

            if (buyresult == false)
            {
                // money no enough, reject
                peer.ZRPC.NonCombatRPC.Ret_NPCStoreBuy("Money no enough", peer);
                return;
            }

            // transaction legal, give the item
            InvRetval retval = peer.mInventory.AddItemsIntoInventory((ushort)storeentry.ItemID, storeentry.ItemValue, true, "NPCStoreBuy");
                
            // record transaction            
            var success = GameApplication.dbGM.NPCStoreGMRepo.UpdateTransactions(transactions, charid).ConfigureAwait(false);

            string scString = JsonConvert.SerializeObject("success");
            peer.ZRPC.NonCombatRPC.Ret_NPCStoreBuy(scString, peer);
        }

        #endregion

        #region CharacterInfo
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.CharacterInfoSpendStatsPoints)]
        public void CharacterInfoSpendStatsPoints(int statVal1, int statVal2, int statVal3, int statVal4, int statVal5, GameClientPeer peer)
        {
            //Check if stats are legal
            int totalSpent = statVal1 + statVal2 + statVal3 + statVal4 + statVal5;
            int retVal = (peer.mPlayer.LocalCombatStats.StatsPoint < totalSpent) ? -1 : 0;
            if (retVal != -1)
            {
                Player p = peer.mPlayer;
                Zealot.Common.Entities.PlayerSynStats pss = p.PlayerSynStats;
                Zealot.Common.Entities.LocalCombatStats lcs = p.LocalCombatStats;

                lcs.Strength += statVal1;
                lcs.Agility += statVal2;
                lcs.Dexterity += statVal3;
                lcs.Constitution += statVal4;
                lcs.Intelligence += statVal5;
                lcs.StatsPoint -= totalSpent;

                //Str
                //peer.mPlayer.LocalCombatStats.WeaponAttack //NOTE: Reset everytime player change weapon
                lcs.IgnoreArmor += (int)(statVal1 * 0.05f); //NOTE: Make this right

                //Agi
                lcs.Evasion += statVal2;
                //atk spd

                //Dex
                //peer.mPlayer.LocalCombatStats.WeaponAttack //NOTE: Reset everytime player change weapon
                lcs.Accuracy += statVal3;
                //cast spd

                //Con
                //var JobJson = ExperienceRepo.GetJobParameterByJobType((JobType)pss.jobsect);
                //lcs.HealthMax = (int)(JobJson.hp * (statVal4) + 0);
                lcs.HealthMax = (int)(1f * (statVal4) + 0);
                lcs.Armor += statVal4 * 2;
                lcs.HealthRegen += statVal4 * 3;

                //Int
                //peer.mPlayer.LocalCombatStats.WeaponAttack  //NOTE: Reset everytime player change weapon
                //recovery bonus
                //elem def
                lcs.ManaMax += (int)(statVal5);
                lcs.ManaRegen += statVal5;
            }

            peer.ZRPC.NonCombatRPC.Ret_CharacterInfoSpendStatsPoints(retVal, peer);
        }

        [RPCMethodProxy(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.CharacterInfoSpendStatsPoints)]
        public void CharacterInfoSpendStatsPointsProxy(object[] args)
        {
            CharacterInfoSpendStatsPoints((int)args[0], (int)args[1], (int)args[2], (int)args[3], (int)args[4], (GameClientPeer)args[5]);
        }
        #endregion

        #region Skills
        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.AddToSkillInventory)]
        public void AddToSkillInventory(int skillid, int skillgid, GameClientPeer peer)
        {
            SkillData skill = SkillRepo.GetSkill(skillid);
            // do integrety check
            if (peer.mPlayer.LocalCombatStats.SkillPoints >= skill.skillJson.learningsp &&
                peer.mPlayer.SecondaryStats.money >= skill.skillJson.learningcost)
            {
                // try adding skill
                // find the skillid [groupid][id][groupid][id]
                if (peer.mPlayer.SkillStats.SkillGroupIndex.ContainsKey(skillgid))
                {
                    peer.mPlayer.SkillStats.SkillInv[peer.mPlayer.SkillStats.SkillGroupIndex[skillgid] + 1] = skillid;
                }
                else
                {
                    peer.mPlayer.SkillStats.SkillGroupIndex[skillgid] = peer.mPlayer.SkillStats.SkillInvCount;
                    peer.mPlayer.SkillStats.SkillInv[peer.mPlayer.SkillStats.SkillInvCount++] = skillgid;
                    peer.mPlayer.SkillStats.SkillInv[peer.mPlayer.SkillStats.SkillInvCount++] = skillid;
                }

                // deduct cost of skill, since the requirements is guaranteed in client

                peer.mPlayer.LocalCombatStats.SkillPoints -= skill.skillJson.learningsp;
                peer.mPlayer.SecondaryStats.money -= skill.skillJson.learningcost;

                ZRPC.NonCombatRPC.Ret_AddToSkillInventory((byte)SkillReturnCode.SUCCESS, skillid, peer.mPlayer.LocalCombatStats.SkillPoints,
                    peer.mPlayer.SecondaryStats.money, peer);
            }

            else
            {
                SkillReturnCode code = SkillReturnCode.EMPTY;
                if (peer.mPlayer.LocalCombatStats.SkillPoints < skill.skillJson.learningsp)
                    code |= SkillReturnCode.SKILLPOINTFAILED;
                if (peer.mPlayer.SecondaryStats.money < skill.skillJson.learningcost)
                    code |= SkillReturnCode.MONEYFAILED;

                ZRPC.NonCombatRPC.Ret_AddToSkillInventory((byte)code, skillid, peer.mPlayer.LocalCombatStats.SkillPoints,
                    peer.mPlayer.SecondaryStats.money, peer);
            }

            
        }

        [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipSkill)]
        public void EquipSkill(int skillid, int slot, int slotGroup, GameClientPeer peer)
        {
            peer.mPlayer.SkillStats.EquippedSkill[slot * slotGroup] = skillid;
        }
        #endregion
    }
}