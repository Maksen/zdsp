using ExitGames.Client.Photon;
using Zealot.Common.RPC;

public partial class NonCombatRPC : RPCBase
{
    public NonCombatRPC() : base(typeof(NonCombatRPC), OperationCode.NonCombat)
    {
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RequestCombatStats)]
    public void RequestCombatStats()
    {
        ProxyMethod("RequestCombatStats");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RequestArenaAICombatStats)]
    public void RequestArenaAICombatStats()
    {
        ProxyMethod("RequestArenaAICombatStats");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RequestSideEffectsInfo)]
    public void RequestSideEffectsInfo()
    {
        ProxyMethod("RequestSideEffectsInfo");
    }

    #region Development
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleTeleportToLevel)]
    public void ConsoleTeleportToLevel(string entryName)
    {
        ProxyMethod("ConsoleTeleportToLevel", entryName);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleTeleportToPosInLevel)]
    public void ConsoleTeleportToPosInLevel(RPCPosition pos)
    {
        ProxyMethod("ConsoleTeleportToPosInLevel", pos);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleClearItemInventory)]
    public void ConsoleClearItemInventory()
    {
        ProxyMethod("ConsoleClearItemInventory");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddExperience)]
    public void ConsoleAddExperience(int exp)
    {
        ProxyMethod("ConsoleAddExperience", exp);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGetEquipmentItemInfo)]
    public void ConsoleGetEquipmentItemInfo(int id)
    {
        ProxyMethod("ConsoleGetEquipmentItemInfo", id);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGetInventoryItemInfo)]
    public void ConsoleGetInventoryItemInfo(int id)
    {
        ProxyMethod("ConsoleGetInventoryItemInfo", id);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddStats)]
    public void ConsoleAddStats(int statsType, float bonus)
    {
        ProxyMethod("ConsoleAddStats", statsType, bonus);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSetDmgPercent)]
    public void ConsoleSetDmgPercent(int amt)
    {
        ProxyMethod("ConsoleSetDmgPercent", amt);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleEnterActivityByRealmID)]
    public void ConsoleEnterActivityByRealmID(int realmid)
    {
        ProxyMethod("ConsoleEnterActivityByRealmID", realmid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleInspect)]
    public void ConsoleInspect()
    {
        ProxyMethod("ConsoleInspect");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.GoToMainQuest)]
    public void GoToMainQuest(int questid)
    {
        ProxyMethod("GoToMainQuest", questid);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientNonCombatRPCMethods.ConsoleNewDay)]
    public void ConsoleNewDay()
    {
        ProxyMethod("ConsoleNewDay");
    }

    [RPCMethod(RPCCategory.Combat, (byte)ClientNonCombatRPCMethods.ConsoleServerNewDay)]
    public void ConsoleServerNewDay()
    {
        ProxyMethod("ConsoleServerNewDay");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleCompleteRealm)]
    public void ConsoleCompleteRealm()
    {
        ProxyMethod("ConsoleCompleteRealm");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleFullHealPlayer)]
    public void ConsoleFullHealPlayer()
    {
        ProxyMethod("ConsoleFullHealPlayer");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleFullRecoverMana)]
    public void ConsoleFullRecoverMana()
    {
        ProxyMethod("ConsoleFullRecoverMana");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGetAllRealmInfo)]
    public void ConsoleGetAllRealmInfo()
    {
        ProxyMethod("ConsoleGetAllRealmInfo");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddSideEffect)]
    public void ConsoleAddSideEffect(int sideID, int pid)
    {
        ProxyMethod("ConsoleAddSideEffect", sideID, pid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleTestSideEffect)]
    public void ConsoleTestSideEffect(int sideTypeID, int pid, string others)
    {
        ProxyMethod("ConsoleTestSideEffect", sideTypeID, pid, others);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleTestComboSkill)]
    public void ConsoleTestComboSkill(int mainskill, int lvl, float skilldur, int mainTypeID, int subTypeID, string OthersForMain, string OthersForSub)
    {
        ProxyMethod("ConsoleTestComboSkill", mainskill, lvl, skilldur, mainTypeID, subTypeID, OthersForMain, OthersForSub);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ClientGuildQuestOperation)]
    public void ClientGuildQuestOperation(int ope, int qid)
    {
        ProxyMethod("ClientGuildQuestOperation", ope, qid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSpawnSpecialBoss)]
    public void ConsoleSpawnSpecialBoss(string name)
    {
        ProxyMethod("ConsoleSpawnSpecialBoss", name);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGuildList)]
    public void ConsoleGuildList()
    {
        ProxyMethod("ConsoleGuildList");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGuildAddFavour)]
    public void ConsoleGuildAddFavour(int amt)
    {
        ProxyMethod("ConsoleGuildAddFavour", amt);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGuildSMLevel)]
    public void ConsoleGuildSMLevel(int level)
    {
        ProxyMethod("ConsoleGuildSMLevel", level);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddRewardGroupBag)]
    public void ConsoleAddRewardGroupBag(int grpID)
    {
        ProxyMethod("ConsoleAddRewardGroupBag", grpID);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddRewardGroupCheckBagMail)]
    public void ConsoleAddRewardGroupCheckBagMail(int grpID)
    {
        ProxyMethod("ConsoleAddRewardGroupCheckBagMail", grpID);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddRewardGroupMail)]
    public void ConsoleAddRewardGroupMail(int grpID)
    {
        ProxyMethod("ConsoleAddRewardGroupMail", grpID);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddRewardGroupCheckBagSlot)]
    public void ConsoleAddRewardGroupCheckBagSlot(int grpID)
    {
        ProxyMethod("ConsoleAddRewardGroupCheckBagSlot", grpID);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSetAchievementLevel)]
    public void ConsoleSetAchievementLevel(int level)
    {
        ProxyMethod("ConsoleSetAchievementLevel", level);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGetCollection)]
    public void ConsoleGetCollection(string objtype, int target)
    {
        ProxyMethod("ConsoleGetCollection", objtype, target);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleGetAchievement)]
    public void ConsoleGetAchievement(string objtype, string target, int count, bool increment)
    {
        ProxyMethod("ConsoleGetAchievement", objtype, target, count, increment);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleClearAchievementRewards)]
    public void ConsoleClearAchievementRewards()
    {
        ProxyMethod("ConsoleClearAchievementRewards");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddHero)]
    public void ConsoleAddHero(int heroId, bool free)
    {
        ProxyMethod("ConsoleAddHero", heroId, free);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleRemoveHero)]
    public void ConsoleRemoveHero(int heroId)
    {
        ProxyMethod("ConsoleRemoveHero", heroId);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleResetExplorations)]
    public void ConsoleResetExplorations()
    {
        ProxyMethod("ConsoleResetExplorations");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleKillPlayer)]
    public void ConsoleKillPlayer()
    {
        ProxyMethod("ConsoleKillPlayer");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleItemMallTopUp)]
    public void ConsoleItemMallTopUp(int amount)
    {
        ProxyMethod("ConsoleItemMallTopUp", amount);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleTestCombatFormula)]
    public void ConsoleTestCombatFormula(int value)
    {
        ProxyMethod("ConsoleTestCombatFormula", value);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddLotteryFreeTickets)]
    public void ConsoleAddLotteryFreeTickets(int lottery_id, int value)
    {
        ProxyMethod("ConsoleAddLotteryFreeTickets", lottery_id, value);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddLotteryPoint)]
    public void ConsoleAddLotteryPoint(int lottery_id, int point)
    {
        ProxyMethod("ConsoleAddLotteryPoint", lottery_id, point);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleRefreshLeaderBoard)]
    public void ConsoleRefreshLeaderBoard()
    {
        ProxyMethod("ConsoleRefreshLeaderBoard");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleResetPrizeGuarantee)]
    public void ConsoleResetPrizeGuarantee()
    {
        ProxyMethod("ConsoleResetPrizeGuarantee");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleDonateReset)]
    public void ConsoleDonateReset()
    {
        ProxyMethod("ConsoleDonateReset");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleResetDonateRemainingCount)]
    public void ConsoleResetDonateRemainingCount()
    {
        ProxyMethod("ConsoleResetDonateRemainingCount");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleNotifyNewGMItem)]
    public void ConsoleNotifyNewGMItem()
    {
        ProxyMethod("ConsoleNotifyNewGMItem");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddActivePoints)]
    public void ConsoleAddActivePoints(int amount)
    {
        ProxyMethod("ConsoleAddActivePoints", amount);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleFinishQERTask)]
    public void ConsoleFinishQERTask(int taskId)
    {
        ProxyMethod("ConsoleFinishQERTask", taskId);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSocialAddFriend)]
    public void ConsoleSocialAddFriend(string playerName)
    {
        ProxyMethod("ConsoleSocialAddFriend", playerName);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSetStoreRefreshTime)]
    public void ConsoleSetStoreRefreshTime(int storecat, int time)
    {
        ProxyMethod("ConsoleSetStoreRefreshTime", storecat, time);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleResetGoldJackpotRoll)]
    public void ConsoleResetGoldJackpotRoll()
    {
        ProxyMethod("ConsoleResetGoldJackpotRoll");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleResetContLoginClaims)]
    public void ConsoleResetContLoginClaims()
    {
        ProxyMethod("ConsoleResetContLoginClaims");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSetEquipmentUpgradeLevel)]
    public void ConsoleSetEquipmentUpgradeLevel(int slotID, int level)
    {
        ProxyMethod("ConsoleSetEquipmentUpgradeLevel", slotID, level);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleResetArenaEntey)]
    public void ConsoleResetArenaEntey()
    {
        ProxyMethod("ConsoleResetArenaEntey");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleTransferServer)]
    public void ConsoleTransferServer(int serverid)
    {
        ProxyMethod("ConsoleTransferServer", serverid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleDCFromServer)]
    public void ConsoleDCFromServer(string server)
    {
        ProxyMethod("ConsoleDCFromServer", server);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSpawnPersonalMonster)]
    public void ConsoleSpawnPersonalMonster(string archtype, int population, bool aggressive = false, int questid = -1)
    {
        ProxyMethod("ConsoleSpawnPersonalMonster", archtype, population, aggressive, questid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleUpdateQuestProgress)]
    public void ConsoleUpdateQuestProgress(byte type)
    {
        ProxyMethod("ConsoleUpdateQuestProgress", type);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddStatsPoint)]
    public void ConsoleAddStatsPoint(int val)
    {
        ProxyMethod("ConsoleAddStatsPoint", val);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.TotalCrit)]
    public void TotalCrit()
    {
        ProxyMethod("TotalCrit");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.CritRate)]
    public void CritRate(float rate)
    {
        ProxyMethod("CritRate", rate);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleChangeJob)]
    public void ConsoleChangeJob(byte job)
    {
        ProxyMethod("ConsoleChangeJob", job);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleAddSkillPoint)]
    public void ConsoleAddSkillPoint(int amt)
    {
        ProxyMethod("ConsoleAddSkillPoint", amt);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleUpdateDonate)]
    public void ConsoleUpdateDonate(int type)
    {
        ProxyMethod("ConsoleUpdateDonate", type);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleRemoveAllSkills)]
    public void ConsoleRemoveAllSkill()
    {
        ProxyMethod("ConsoleRemoveAllSkill");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ConsoleSendMail)]
    public void ConsoleSendMail(int id)
    {
        ProxyMethod("ConsoleSendMail", id);
    }
    #endregion

    #region LeaderBoard
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.GetLeaderBoard)]
    public void GetLeaderBoard(byte lbType)
    {
        ProxyMethod("GetLeaderBoard", lbType);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.GetLeaderBoardAvatar)]
    public void GetLeaderBoardAvatar(byte lbType, string charName)
    {
        ProxyMethod("GetLeaderBoardAvatar", lbType, charName);
    }
    #endregion

    #region ItemMall

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ItemMallPurchaseItem)]
    public void ItemMallPurchaseItem(int shopitemid, bool isGM, int stackToBuy, long lastGrabDateTime)
    {
        ProxyMethod("ItemMallPurchaseItem", shopitemid, isGM, stackToBuy, lastGrabDateTime);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ItemMallInit)]
    public void ItemMallInit()
    {
        ProxyMethod("ItemMallInit");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ItemMallGetList)]
    public void ItemMallGetList(int category, long datetime)
    {
        ProxyMethod("ItemMallGetList", category, datetime);
    }

    #endregion ItemMall

    #region Tutorial
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.TriggerTutorial)]
    public void OnTriggerTutorial(int id)
    {
        ProxyMethod("OnTriggerTutorial", id);
    }
    #endregion

    #region SevenDays
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.SevenDaysBuyDiscountItem)]
    public void SevenDaysBuyDiscountItem(int day)
    {
        ProxyMethod("SevenDaysBuyDiscountItem", day);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.SevenDaysClaimTaskReward)]
    public void SevenDaysClaimTaskReward(int day, int subcat, int taskId, string taskName)
    {
        ProxyMethod("SevenDaysClaimTaskReward", day, subcat, taskId, taskName);
    }
    #endregion

    #region Welfare
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimSignInPrize)]
    public void WelfareClaimSignInPrize(int year, int month, int day, int cltYear, int cltMonth, int cltDay, bool oldData)
    {
        ProxyMethod("WelfareClaimSignInPrize", year, month, day, cltYear, cltMonth, cltDay, oldData);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimOnlinePrizes)]
    public void WelfareClaimOnlinePrizes()
    {
        ProxyMethod("WelfareClaimOnlinePrizes");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimOnlinePrizesSingle)]
    public void WelfareClaimOnlinePrizesSingle(int dataid)
    {
        ProxyMethod("WelfareClaimOnlinePrizesSingle", dataid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareBuyOpenServiceFund)]
    public void WelfareBuyOpenServiceFund()
    {
        ProxyMethod("WelfareBuyOpenServiceFund");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimOpenServiceFundGoldReward)]
    public void WelfareClaimOpenServiceFundGoldReward(int dataid)
    {
        ProxyMethod("WelfareClaimOpenServiceFundGoldReward", dataid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimOpenServiceFundItemReward)]
    public void WelfareClaimOpenServiceFundItemReward(int dataid)
    {
        ProxyMethod("WelfareClaimOpenServiceFundItemReward", dataid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimFirstGoldCreditRewards)]
    public void WelfareClaimFirstGoldCreditRewards()
    {
        ProxyMethod("WelfareClaimFirstGoldCreditRewards");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimTotalCreditReward)]
    public void WelfareClaimTotalCreditReward(int rewardid)
    {
        ProxyMethod("WelfareClaimTotalCreditReward", rewardid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimTotalSpendReward)]
    public void WelfareClaimTotalSpendReward(int rewardid)
    {
        ProxyMethod("WelfareClaimTotalSpendReward", rewardid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldCloseFirstLoginFlag)]
    public void WelfareDailyGoldCloseFirstLoginFlag()
    {
        ProxyMethod("WelfareDailyGoldCloseFirstLoginFlag");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldBuyMCard)]
    public void WelfareDailyGoldBuyMCard()
    {
        ProxyMethod("WelfareDailyGoldBuyMCard");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldClaimMCardGold)]
    public void WelfareDailyGoldClaimMCardGold()
    {
        ProxyMethod("WelfareDailyGoldClaimMCardGold");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldBuyPCard)]
    public void WelfareDailyGoldBuyPCard()
    {
        ProxyMethod("WelfareDailyGoldBuyPCard");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareDailyGoldClaimPCardGold)]
    public void WelfareDailyGoldClaimPCardGold()
    {
        ProxyMethod("WelfareDailyGoldClaimPCardGold");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareGoldJackpotGetResult)]
    public void WelfareGoldJackpotGetResult()
    {
        ProxyMethod("WelfareGoldJackpotGetResult");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.WelfareClaimContLoginReward)]
    public void WelfareClaimContLoginReward(int rewardId)
    {
        ProxyMethod("WelfareClaimContLoginReward", rewardId);
    }
    #endregion

    #region QuestExtraRewards
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERFinishTask)]
    public void QERFinishTask(int dataid, bool useGold = false)
    {
        ProxyMethod("QERFinishTask", dataid, useGold);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERFinishTaskAll)]
    public void QERFinishTaskAll(string dataids, bool useGold = false)
    {
        ProxyMethod("QERFinishTaskAll", dataids, useGold);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERClaimTaskReward)]
    public void QERClaimTaskReward(int dataid)
    {
        ProxyMethod("QERClaimTaskReward", dataid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERClaimTaskRewardAll)]
    public void QERClaimTaskRewardAll(string dataids)
    {
        ProxyMethod("QERClaimTaskRewardAll", dataids);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.QERClaimBoxReward)]
    public void QERClaimBoxReward(int dataid)
    {
        ProxyMethod("QERClaimBoxReward", dataid);
    }
    #endregion

    #region Lottery
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryGetSimpleInfo)]
    public void LotteryGetSimpleInfo()
    {
        ProxyMethod("LotteryGetSimpleInfo");
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryGetInfo)]
    public void LotteryGetLotteryInfo(int lottery_id)
    {
        ProxyMethod("LotteryGetLotteryInfo", lottery_id);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryRollOne)]
    public void LotteryRollOne(int lottery_id, int cost_type)
    {
        ProxyMethod("LotteryRollOne", lottery_id, cost_type);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryRollTen)]
    public void LotteryRollTen(int lottery_id, int ticket_type)
    {
        ProxyMethod("LotteryRollTen", lottery_id, ticket_type);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryGetPointReward)]
    public void LotteryGetPointReward(int lottery_id, int point)
    {
        ProxyMethod("LotteryGetPointReward", lottery_id, point);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.LotteryUsePointItem)]
    public void LotteryUsePointItem(int lottery_id, int item_id)
    {
        ProxyMethod("LotteryUsePointItem", lottery_id, item_id);
    }
    #endregion

    #region EquipmentModding
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipmentUpgradeEquipment)]
    public void EquipmentUpgradeEquipment(int slotId, bool isEquipped, bool isUseGenMaterial, bool isSafeUpgrade, bool isSafeUseEquip, bool isSafeGenMat, int safeEquipSlotId = -1)
    {
        ProxyMethod("EquipmentUpgradeEquipment", slotId, isEquipped, isUseGenMaterial, isSafeUpgrade, isSafeUseEquip, isSafeGenMat, safeEquipSlotId);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipmentReformEquipment)]
    public void EquipmentReformEquipment(int slotId, bool isEquipped, int selection)
    {
        ProxyMethod("EquipmentReformEquipment", slotId, isEquipped, selection);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipmentRecycleEquipment)]
    public void EquipmentRecycleEquipment(int slotId, bool isEquipped)
    {
        ProxyMethod("EquipmentRecycleEquipment", slotId, isEquipped);
    }

    //[RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipSlotGem)]
    //public void EquipSlotGem(int equipSlotId, int gemGrp, int gemSlot, int gemId)
    //{
    //    ProxyMethod("EquipSlotGem", equipSlotId, gemGrp, gemSlot, gemId);
    //}

    //[RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.UnequipSlotGem)]
    //public void UnequipSlotGem(int equipSlotId, int gemGrp, int gemSlot)
    //{
    //    ProxyMethod("UnequipSlotGem", equipSlotId, gemGrp, gemSlot);
    //}

    //[RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipAutoUpgradeGem)]
    //public void EquipAutoUpgradeGem()
    //{
    //    ProxyMethod("EquipAutoUpgradeGem");
    //}
    #endregion

    #region Equipment
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.HideHelm)]
    public void HideHelm(bool hide)
    {
        ProxyMethod("HideHelm", hide);
    }
    #endregion

    #region ReviveItem
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StartReviveItemRequest)]
    public void StartReviveItemRequest(string requestor, string requestee, int itemId)
    {
        ProxyMethod("StartReviveItemRequest", requestor, requestee, itemId);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RejectReviveItem)]
    public void RejectReviveItem(int requestId)
    {
        ProxyMethod("RejectReviveItem", requestId);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.AcceptReviveItem)]
    public void AcceptReviveItem(int requestId)
    {
        ProxyMethod("AcceptReviveItem", requestId);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RequestCancelReviveItem)]
    public void RequestCancelReviveItem(int sessionId)
    {
        ProxyMethod("RequestCancelReviveItem", sessionId);
    }
    #endregion

    #region ExchangeShop
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ExchangeShopPurchase)]
    public void ExchangeShopPurchase(int categoryid, int exid)
    {
        ProxyMethod("ExchangeShopPurchase", categoryid, exid);
    }
    #endregion

    #region Portrait
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.OldenPortrait)]
    public void OldenPortrait(System.Collections.Generic.List<int> idLst)
    {
        string serializedIDLst = Newtonsoft.Json.JsonConvert.SerializeObject(idLst);
        ProxyMethod("OldenPortrait", serializedIDLst);
    }
    #endregion

    #region Store
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StoreInit)]
    public void StoreInit(int categoryID)
    {
        ProxyMethod("StoreInit", categoryID);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StoreRefresh)]
    public void StoreRefresh(int category)
    {
        ProxyMethod("StoreRefresh", category);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StorePurchase)]
    public void StorePurchase(int storeID, int stackamt, int shelveNo)
    {
        ProxyMethod("StorePurchase", storeID, stackamt, shelveNo);
    }
    #endregion

    #region NPCStore
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.NPCStoreInit)]
    public void NPCStoreInit(int storeid)
    {
        ProxyMethod("NPCStoreInit", storeid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.NPCStoreGetPlayerTransactions)]
    public void NPCStoreGetPlayerTransactions(int storeid)
    {
        ProxyMethod("NPCStoreGetPlayerTransactions", storeid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.NPCStoreBuy)]
    public void NPCStoreBuy(int storeid, int itemlistid, int purchaseamount, string charid)
    {
        ProxyMethod("NPCStoreBuy", storeid, itemlistid, purchaseamount, charid);
    }

    #endregion

    #region Quest
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.UpdateTrakingList)]
    public void UpdateTrakingList(string data)
    {
        ProxyMethod("UpdateTrakingList", data);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.DeleteQuest)]
    public void DeleteQuest(int questid, string data)
    {
        ProxyMethod("DeleteQuest", questid, data);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ResetQuest)]
    public void ResetQuest(int questid)
    {
        ProxyMethod("ResetQuest", questid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.UpdateQuestStatus)]
    public void UpdateQuestStatus(int questid)
    {
        ProxyMethod("UpdateQuestStatus", questid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.StartQuest)]
    public void StartQuest(int questid, int callerid, int groupid)
    {
        ProxyMethod("StartQuest", questid, callerid, groupid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.NPCInteract)]
    public void NPCInteract(int questid, int npcid, int choice, int talkid)
    {
        ProxyMethod("NPCInteract", questid, npcid, choice, talkid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.CompleteQuest)]
    public void CompleteQuest(int questid, bool replyid)
    {
        ProxyMethod("CompleteQuest", questid, replyid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.InteractAction)]
    public void InteractAction(int questid, int interactid)
    {
        ProxyMethod("InteractAction", questid, interactid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.FailQuest)]
    public void FailQuest(int questid)
    {
        ProxyMethod("FailQuest", questid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.SubmitEmptyObjective)]
    public void SubmitEmptyObjective(int questid)
    {
        ProxyMethod("SubmitEmptyObjective", questid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ApplyQuestEventBuff)]
    public void ApplyQuestEventBuff(int eventid, int questid)
    {
        ProxyMethod("ApplyQuestEventBuff", eventid, questid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ApplyQuestEventCompanion)]
    public void ApplyQuestEventCompanion(int eventid, int questid)
    {
        ProxyMethod("ApplyQuestEventCompanion", eventid, questid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ResetQuestEventCompanion)]
    public void ResetQuestEventCompanion(int companionid)
    {
        ProxyMethod("ResetQuestEventCompanion", companionid);
    }
    #endregion

    #region CharacterInfo
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.CharacterInfoSpendStatsPoints)]
    public void CharacterInfoSpendStatsPoints(int statVal1, int statVal2, int statVal3, int statVal4, int statVal5)
    {
        ProxyMethod("CharacterInfoSpendStatsPoints", statVal1, statVal2, statVal3, statVal4, statVal5);
    }
    #endregion

    #region Skills
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.AddToSkillInventory)]
    public void AddToSkillInventory(int skillid, int skillgrpid)
    {
        ProxyMethod("AddToSkillInventory", skillid, skillgrpid);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipSkill)]
    public void EquipSkill(int skillid, int slot, int slotGroup)
    {
        ProxyMethod("EquipSkill", skillid, slot, slotGroup);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RemoveEquipSkill)]
    public void RemoveEquipSkill(int slot, int slotGroup)
    {
        ProxyMethod("RemoveEquipSkill", slot, slotGroup);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.AutoEquipSkill)]
    public void AutoEquipSkill(int skillid, int slot, int slotGroup)
    {
        ProxyMethod("AutoEquipSkill", skillid, slot, slotGroup);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.RemoveAutoEquipSkill)]
    public void RemoveAutoEquipSkill(int slot, int slotGroup)
    {
        ProxyMethod("RemoveAutoEquipSkill", slot, slotGroup);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.UpdateEquipSlots)]
    public void UpdateEquipSlots(int equip, int auto)
    {
        ProxyMethod("UpdateEquipSlots", equip, auto);
    }
    #endregion

    #region PowerUp
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.PowerUp)]
    public void PowerUp(int part)
    {
        ProxyMethod("PowerUp", part);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.MeridianLevelUp)]
    public void MeridianLevelUp(int type)
    {
        ProxyMethod("MeridianLevelUp", type);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.MeridianExpUp)]
    public void MeridianExpUp(int type)
    {
        ProxyMethod("MeridianExpUp", type);
    }
    #endregion

    #region EquipmentCraft
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipmentCraft)]
    public void EquipmentCraft(int itemId)
    {
        ProxyMethod("EquipmentCraft", itemId);
    }
    #endregion

    #region EquipFusion
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipFusion)]
    public void EquipFusion(int itemIndex, string consumeIndex, bool changed)
    {
        ProxyMethod("EquipFusion", itemIndex, consumeIndex, changed);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.EquipFusionGive)]
    public void EquipFusionGive(int itemIndex, string consumeIndex)
    {
        ProxyMethod("EquipFusionGive", itemIndex, consumeIndex);
    }
    #endregion

    #region Destiny Clue
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.ReadClue)]
    public void ReadClue(int clueid, byte type)
    {
        ProxyMethod("ReadClue", clueid, type);
    }

    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.CollectClueReward)]
    public void CollectClueReward(int clueid)
    {
        ProxyMethod("CollectClueReward", clueid);
    }
    #endregion

    #region Donate
    [RPCMethod(RPCCategory.NonCombat, (byte)ClientNonCombatRPCMethods.DonateItem)]
    public void DonateItem(string guid)
    {
        ProxyMethod("DonateItem", guid);
    }
    #endregion
}
