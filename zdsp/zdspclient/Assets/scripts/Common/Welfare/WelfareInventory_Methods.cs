using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel;
using Zealot.Common.Entities;
using Zealot.Repository;

namespace Zealot.Common
{
    public partial class WelfareInventoryData
    {
        public void InitFromInventory(WelfareStats welfareStats)
        {
            // Updating 31 elements
            for (int i = 0; i < MAX_CONTLOGINCLAIMS; ++i)
            {
                welfareStats.continuousLoginFullCollection = SignInPrizeFullClaim;
                welfareStats.continuousLoginPartCollection = SignInPrizePartClaim;
            }

            welfareStats.firstLoginYear = FirstLoginDT.Year;
            welfareStats.firstLoginMonth = FirstLoginDT.Month;
            welfareStats.firstLoginDay = FirstLoginDT.Day;
            welfareStats.serverStartYear = ServerStartDT.Year;
            welfareStats.serverStartMonth = ServerStartDT.Month;
            welfareStats.serverStartDay = ServerStartDT.Day;
            welfareStats.continuousLoginDayNum = ContinuousLoginDayNum;

            welfareStats.onlineRewardsClaims = OnlinePrizeClaim;
            welfareStats.onlineDuration = OnlineDuration;
            welfareStats.serviceFundsLvlCollection = OpenServiceFundLvlCollection;
            welfareStats.serviceFundLvlClaims = OpenServiceFundLvlClaims;
            welfareStats.serviceFundsPplCollection = OpenServiceFundPplCollection;
            welfareStats.serviceFundPplClaims = OpenServiceFundPplClaims;
            welfareStats.serviceFundsClaimNum = OpenServiceFundClaimedNum;
            welfareStats.serviceFundsBought = OpenServiceFundBought;

            welfareStats.firstGoldBuyCollected = FirstGoldBuyCollected;

            welfareStats.totalGoldCredited = TotalGoldCredited;
            welfareStats.totalCreditClaims = TotalCreditClaims;

            welfareStats.totalGoldSpent = TotalGoldSpent;
            welfareStats.totalSpendClaims = TotalSpendClaims;

            welfareStats.dailyGoldFirstLogin = DailyGoldFirstLogin;
            welfareStats.monthCardBought = MonthlyCardBought;
            welfareStats.monthCardBoughtDayNum = MonthlyCardBoughtDayNum;
            welfareStats.monthCardGoldCollected = MonthlyCardGoldCollected;
            welfareStats.permanentCardBought = PermanentCardBought;
            welfareStats.permanentCardGoldCollected = PermanentCardGoldCollected;

            welfareStats.goldJackpotResult = GoldJackpotResult;
            welfareStats.goldJackpotAllGold = GoldJackpotAllGold;
            welfareStats.goldJackpotCurrTier = GoldJackpotCurrTier;

            welfareStats.contLoginClaims = ContLoginClaims;
        }

        public void InitFromStats(WelfareStats welfareStats)
        {
            // Updating 31 elements
            for (int i = 0; i < MAX_CONTLOGINCLAIMS; ++i)
            {
                SignInPrizeFullClaim = welfareStats.continuousLoginFullCollection;
                SignInPrizePartClaim = welfareStats.continuousLoginPartCollection;
            }

            OnlinePrizeClaim = welfareStats.onlineRewardsClaims;

            OnlineDuration = welfareStats.onlineDuration;
            if (welfareStats.firstLoginYear > 0 && welfareStats.firstLoginMonth > 0 && welfareStats.firstLoginDay > 0)
            {
                FirstLoginDT = new DateTime(welfareStats.firstLoginYear, welfareStats.firstLoginMonth, welfareStats.firstLoginDay);
            }
            if (welfareStats.serverStartYear > 0 && welfareStats.serverStartMonth > 0 && welfareStats.serverStartDay > 0)
            {
                ServerStartDT = new DateTime(welfareStats.serverStartYear, welfareStats.serverStartMonth, welfareStats.serverStartDay);
            }
            ContinuousLoginDayNum = welfareStats.continuousLoginDayNum;

            OpenServiceFundLvlCollection = welfareStats.serviceFundsLvlCollection;
            OpenServiceFundLvlClaims = welfareStats.serviceFundLvlClaims;
            OpenServiceFundPplCollection = welfareStats.serviceFundsPplCollection;
            OpenServiceFundPplClaims = welfareStats.serviceFundPplClaims;
            OpenServiceFundClaimedNum = welfareStats.serviceFundsClaimNum;
            OpenServiceJoinMemberNum = welfareStats.serviceFundsJoinMemberNum;
            OpenServiceFundBought = welfareStats.serviceFundsBought;

            FirstGoldBuyFlag = welfareStats.firstBuyFlag == 1 ? true : false;
            FirstGoldBuyCollected = welfareStats.firstBuyCollected == 1 ? true : false;

            TotalGoldCredited = welfareStats.totalGoldCredited;
            TotalCreditClaims = welfareStats.totalCreditClaims;

            TotalGoldSpent = welfareStats.totalGoldSpent;
            TotalSpendClaims = welfareStats.totalSpendClaims;

            DailyGoldFirstLogin = welfareStats.dailyGoldFirstLogin;
            MonthlyCardBought = welfareStats.monthCardBought;
            MonthlyCardBoughtDayNum = welfareStats.monthCardBoughtDayNum;
            MonthlyCardGoldCollected = welfareStats.monthCardGoldCollected;
            PermanentCardBought = welfareStats.permanentCardBought;
            PermanentCardGoldCollected = welfareStats.permanentCardGoldCollected;

            GoldJackpotResult = welfareStats.goldJackpotResult;
            GoldJackpotAllGold = welfareStats.goldJackpotAllGold;
            GoldJackpotCurrTier = welfareStats.goldJackpotCurrTier;

            ContLoginClaims = welfareStats.contLoginClaims;
        }

        public void UpdateInventory(WelfareStats welfareStats)
        {
            InitFromStats(welfareStats);
        }

        public void SaveToInventory(WelfareStats welfareStats)
        {
            InitFromStats(welfareStats);
        }

        // Sign In Prize
        public void InitTodayDT()
        {
            TodayDT = DateTime.Today;
        }
        public void InitLogoutDT(DateTime logoutDT)
        {
            LogoutDT = logoutDT;
        }

        public void InitFirstLoginDT(int year, int month, int day)
        {
            FirstLoginDT = new DateTime(year, month, day);
        }

        public void InitServerStartDT(int year, int month, int day)
        {
            ServerStartDT = new DateTime(year, month, day);
        }

        public DateTime GetFirstLoginDT()
        {
            return FirstLoginDT;
        }

        public DateTime GetServerStartDT()
        {
            return ServerStartDT;
        }

        public void UpdateContinuousLogin(short day)
        {
            ContinuousLoginDayNum = day;
        }

        public void ClaimSignInPrizeFull(int dataid)
        {
            SignInPrizeFullClaim = GameUtils.SetBit(SignInPrizeFullClaim, dataid);
        }

        public void ClaimSignInPrizePart(int dataid)
        {
            SignInPrizePartClaim = GameUtils.SetBit(SignInPrizePartClaim, dataid);
        }

        public bool IsSignInPrizeFullClaimed(int dataid)
        {
            return GameUtils.IsBitSet(SignInPrizeFullClaim, dataid);
        }

        public bool IsSignInPrizePartClaimed(int dataid)
        {
            return GameUtils.IsBitSet(SignInPrizePartClaim, dataid);
        }

        public short GetSignInDayNum()
        {
            DateTime today = DateTime.Today;
            int monthDays = DateTime.DaysInMonth(today.Year, today.Month);
            short daysCount = 0;

            for (int i = 0; i < monthDays; ++i)
            {
                if (GameUtils.IsBitSet(SignInPrizeFullClaim, i) || GameUtils.IsBitSet(SignInPrizePartClaim, i))
                {
                    ++daysCount;
                }
            }

            return daysCount;
        }

        // Online Prize
        public void SetOnlineRewardsStartTime(long startTime)
        {
            StartTime = startTime;
        }

        public long GetOnlineRewardsStartTime()
        {
            return StartTime;
        }

        public void ClaimOnlineRewards(int dataid, int claimedItemId)
        {
            OnlinePrizeClaim = GameUtils.SetBit(OnlinePrizeClaim, dataid);
        }

        public bool IsOnlineRewardsClaimed(int dataid)
        {
            return GameUtils.IsBitSet(OnlinePrizeClaim, dataid);
        }

        public bool IsAllOnlineRewardsClaimed()
        {
            return OnlinePrizeClaim == Math.Pow(2, MAX_ONLNRWRDCLAIMS) - 1;
        }

        public long GetLastOnlineDuration()
        {
            return OnlineDuration;
        }

        // Open Service Funds
        public void SetServiceFundCost(int newAmount)
        {
            OpenServiceFundCost = newAmount;
        }

        public int GetServiceFundCost()
        {
            return OpenServiceFundCost;
        }

        private void IncrementServiceFundBuyNum(int amount)
        {
            OpenServiceFundClaimedNum += amount;
        }

        public bool IsOpenServiceFundBought()
        {
            return OpenServiceFundBought;
        }

        public void BuyOpenServiceFund()
        {
            if (OpenServiceFundBought == false)
            {
                OpenServiceFundBought = true;
            }
        }

        public void UpdateOpenServiceFundBoughtNum(int amount)
        {
            if (amount > 0)
            {
                OpenServiceFundClaimedNum += amount;
            }
        }

        public int GetOpenServiceFundsBoughtNum()
        {
            return OpenServiceFundClaimedNum;
        }

        public void UpdateJoinMembersNum(int newAmount)
        {
            OpenServiceJoinMemberNum = newAmount;
        }

        public int GetCurrentServiceFundsMemberNum()
        {
            return OpenServiceJoinMemberNum;
        }

        public void SetOpenServiceFundLevelRewards(string rewardList)
        {
            OpenServiceFundLvlRewards = rewardList;

            InitOpenServiceFundLvlRewards();
        }

        public void InitOpenServiceFundLvlRewards()
        {
            OpenServiceFundLvlRewardList = DeserializeOpenServiceFundLvlRewards();
            DeserializeOpenServiceFundLvlRewardClaimed();
        }

        public List<ServiceFundLvlReward> DeserializeOpenServiceFundLvlRewards()
        {
            List<ServiceFundLvlReward> rewards = new List<ServiceFundLvlReward>();

            if (!(string.IsNullOrEmpty(OpenServiceFundLvlRewards) || OpenServiceFundLvlRewards == "0"))
            {
                List<string> rewardDataStrList = OpenServiceFundLvlRewards.Split(';').ToList();

                for (int i = 0; i < rewardDataStrList.Count; ++i)
                {
                    List<string> rewardDataStr = rewardDataStrList[i].Split('|').ToList();
                    int rewardId = 0;
                    int level = 0;
                    int goldReward = 0;
                    if (int.TryParse(rewardDataStr[0], out rewardId) && int.TryParse(rewardDataStr[1], out level) &&
                        int.TryParse(rewardDataStr[2], out goldReward))
                    {
                        rewards.Add(new ServiceFundLvlReward(rewardId, level, goldReward));
                    }
                }
            }

            return rewards;
        }

        public string SerializeOpenServiceFundLvlRewardClaimed()
        {
            StringBuilder claimedStr = new StringBuilder();
            for (int i = 0; i < OpenServiceFundLvlRewardList.Count; ++i)
            {
                if (OpenServiceFundLvlRewardList[i].mIsCollected)
                {
                    claimedStr.Append(OpenServiceFundLvlRewardList[i].mRewardId);
                    claimedStr.Append("|");
                    claimedStr.Append(OpenServiceFundLvlRewardList[i].mIsCollected ? 1 : 0);
                    claimedStr.Append(";");
                }
            }

            string claimed = claimedStr.ToString();
            claimed = claimed.TrimEnd(';');

            OpenServiceFundLvlClaims = claimed;

            return claimed;
        }

        public void DeserializeOpenServiceFundLvlRewardClaimed()
        {
            if (string.IsNullOrEmpty(OpenServiceFundLvlClaims) || OpenServiceFundLvlClaims == "0")
            {
                return;
            }

            if (OpenServiceFundLvlRewardList.Count == 0)
            {
                return;
            }

            List<string> claimedStrList = OpenServiceFundLvlClaims.Split(';').ToList();
            for (int i = 0; i < claimedStrList.Count; ++i)
            {
                List<string> claimedDataList = claimedStrList[i].Split('|').ToList();
                int rewardId = 0;
                int isCollected = 0;
                if (int.TryParse(claimedDataList[0], out rewardId) && int.TryParse(claimedDataList[1], out isCollected))
                {
                    int rewardPos = OpenServiceFundLvlRewardList.FindIndex(o => o.mRewardId == rewardId);
                    if (rewardPos > -1)
                    {
                        OpenServiceFundLvlRewardList[rewardPos].mIsCollected = isCollected == 1 ? true : false;
                    }
                }
            }
        }

        public List<ServiceFundLvlReward> GetOpenServiceFundLevelRewards()
        {
            return OpenServiceFundLvlRewardList;
        }

        //public void ClaimLevelOpenServiceFund(int dataid)
        //{
        //    OpenServiceFundLvlCollection = InventoryUtils.SetBitwiseBool(OpenServiceFundLvlCollection, dataid, true);
        //}

        //public bool IsOpenServiceFundLevelClaimed(int dataid)
        //{
        //    return InventoryUtils.GetBitwiseBool(OpenServiceFundLvlCollection, dataid);
        //}

        public ServiceFundLvlReward GetOpenServiceFundLvlReward(int rewardId)
        {
            return OpenServiceFundLvlRewardList.Find(o => o.mRewardId == rewardId);
        }

        public void ClaimOpenServiceFundLvlReward(int rewardId)
        {
            int rewardPos = OpenServiceFundLvlRewardList.FindIndex(o => o.mRewardId == rewardId);

            if (rewardPos != -1)
            {
                OpenServiceFundLvlRewardList[rewardPos].mIsCollected = true;
            }
        }

        public bool IsOpenServiceFundLvlRewardClaimed(int rewardId)
        {
            ServiceFundLvlReward reward = OpenServiceFundLvlRewardList.Find(o => o.mRewardId == rewardId);

            if (reward != null)
            {
                return reward.mIsCollected;
            }

            return false;
        }

        public void SetOpenServiceFundPlayerRewards(string rewardList)
        {
            OpenServiceFundPplRewards = rewardList;

            InitOpenServiceFundPplRewards();
        }

        public void InitOpenServiceFundPplRewards()
        {
            OpenServiceFundPplRewardList = DeserializeOpenServiceFundPplRewards();
            DeserializeOpenServiceFundPplRewardClaimed();
        }

        public List<ServiceFundPplReward> DeserializeOpenServiceFundPplRewards()
        {
            List<ServiceFundPplReward> rewards = new List<ServiceFundPplReward>();

            if (!(string.IsNullOrEmpty(OpenServiceFundPplRewards) || OpenServiceFundPplRewards == "0"))
            {
                List<string> rewardDataStrList = OpenServiceFundPplRewards.Split(';').ToList();

                for (int i = 0; i < rewardDataStrList.Count; ++i)
                {
                    List<string> rewardDataStr = rewardDataStrList[i].Split('|').ToList();
                    int rewardId = 0;
                    int pplCount = 0;
                    int itemId = 0;
                    int stackCount = 0;
                    if (int.TryParse(rewardDataStr[0], out rewardId) && int.TryParse(rewardDataStr[1], out pplCount) &&
                        int.TryParse(rewardDataStr[2], out itemId) && int.TryParse(rewardDataStr[3], out stackCount))
                    {
                        rewards.Add(new ServiceFundPplReward(rewardId, pplCount, itemId, stackCount));
                    }
                }
            }

            return rewards;
        }

        public string SerializeOpenServiceFundPplRewardClaimed()
        {
            StringBuilder claimedStr = new StringBuilder();
            for (int i = 0; i < OpenServiceFundLvlRewardList.Count; ++i)
            {
                if (OpenServiceFundLvlRewardList[i].mIsCollected)
                {
                    claimedStr.Append(OpenServiceFundLvlRewardList[i].mRewardId);
                    claimedStr.Append("|");
                    claimedStr.Append(OpenServiceFundLvlRewardList[i].mIsCollected ? 1 : 0);
                    claimedStr.Append(";");
                }
            }

            string claimed = claimedStr.ToString();
            claimed = claimed.TrimEnd(';');

            OpenServiceFundLvlClaims = claimed;

            return claimed;
        }

        public void DeserializeOpenServiceFundPplRewardClaimed()
        {
            if (string.IsNullOrEmpty(OpenServiceFundLvlClaims) || OpenServiceFundLvlClaims == "0")
            {
                return;
            }

            if (OpenServiceFundLvlRewardList.Count == 0)
            {
                return;
            }

            List<string> claimedStrList = OpenServiceFundLvlClaims.Split(';').ToList();
            for (int i = 0; i < claimedStrList.Count; ++i)
            {
                List<string> claimedDataList = claimedStrList[i].Split('|').ToList();
                int rewardId = 0;
                int isCollected = 0;
                if (int.TryParse(claimedDataList[0], out rewardId) && int.TryParse(claimedDataList[1], out isCollected))
                {
                    int rewardPos = OpenServiceFundLvlRewardList.FindIndex(o => o.mRewardId == rewardId);
                    if (rewardPos > -1)
                    {
                        OpenServiceFundLvlRewardList[rewardPos].mIsCollected = isCollected == 1 ? true : false;
                    }
                }
            }
        }

        public List<ServiceFundPplReward> GetOpenServiceFundPlayerRewards()
        {
            return OpenServiceFundPplRewardList;
        }

        public ServiceFundPplReward GetOpenServiceFundPplReward(int rewardId)
        {
            return OpenServiceFundPplRewardList.Find(o => o.mRewardId == rewardId);
        }

        public void ClaimOpenServiceFundPplReward(int rewardId)
        {
            int rewardPos = OpenServiceFundPplRewardList.FindIndex(o => o.mRewardId == rewardId);

            if (rewardPos != -1)
            {
                OpenServiceFundPplRewardList[rewardPos].mIsCollected = true;
            }
        }

        public bool IsOpenServiceFundPplRewardClaimed(int rewardId)
        {
            ServiceFundPplReward reward = OpenServiceFundPplRewardList.Find(o => o.mRewardId == rewardId);

            if (reward != null)
            {
                return reward.mIsCollected;
            }

            return false;
        }

        //public void ClaimPlayerOpenServiceFund(int dataid)
        //{
        //    OpenServiceFundPplCollection = InventoryUtils.SetBitwiseBool(OpenServiceFundPplCollection, dataid, true);
        //}

        public bool IsOpenServiceFundPlayerComplete(int reqMemberNum)
        {
            return OpenServiceJoinMemberNum >= reqMemberNum;
        }

        //public bool IsOpenServiceFundPlayerClaimed(int dataid)
        //{
        //    return InventoryUtils.GetBitwiseBool(OpenServiceFundPplCollection, dataid);
        //}

        // First Gold Credit
        public void UpdateFirstGoldCredit(int firstBuyFlag, int firstBuyCollected, string firstGoldCreditStatus)
        {
            FirstGoldBuyRewards = firstGoldCreditStatus;
            FirstGoldBuyFlag = firstBuyFlag == 1 ? true : false;
            FirstGoldBuyCollected = firstBuyCollected == 1 ? true : false;
        }

        public List<IInventoryItem> GetFirstGoldCreditRewards()
        {
            if (string.IsNullOrEmpty(FirstGoldBuyRewards))
            {
                return null;
            }

            List<string> rewardsDataList = FirstGoldBuyRewards.Split(';').ToList();
            List<IInventoryItem> rewardsList = new List<IInventoryItem>();

            for (int i = 0; i < rewardsDataList.Count; ++i)
            {
                List<string> rewardData = rewardsDataList[i].Split('|').ToList();

                if (rewardData.Count != 2)
                {
                    continue;
                }

                int itemid = 0;
                if (int.TryParse(rewardData[0], out itemid) && itemid > 0)
                {
                    IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(itemid);
                    int itemcount = 0;
                    if (int.TryParse(rewardData[1], out itemcount))
                    {
                        item.StackCount = (ushort)itemcount;

                        rewardsList.Add(item);
                    }
                }
            }

            return rewardsList;
        }


        public void SetFirstGoldCreditCollectedFlag(bool isCollected)
        {
            FirstGoldBuyCollected = isCollected;
        }

        public void ClaimFirstGoldCredit()
        {
            FirstGoldBuyCollected = true;
        }

        public bool HasFirstGoldCredited()
        {
            return FirstGoldBuyFlag;
        }

        public bool IsFirstGoldCreditClaimed()
        {
            return FirstGoldBuyCollected;
        }

        // Total Gold
        public void UpdateTotalGold(string totalGoldStatus, bool isCredit)
        {
            List<string> statusList = totalGoldStatus.Split('*').ToList();

            if (statusList.Count < 5)
            {
                return;
            }

            int eventRunning = 0;
            int.TryParse(statusList[0], out eventRunning);
            bool IsEventRunning = eventRunning == 1 ? true : false;

            int currentEventId = 0;
            int.TryParse(statusList[1], out currentEventId);

            long eventStartDT = 0;
            long.TryParse(statusList[2], out eventStartDT);

            long eventEndDT = 0;
            long.TryParse(statusList[3], out eventEndDT);

            if (isCredit)
            {
                isTotalCreditEventRunning = IsEventRunning;
                TotalCreditCurrEventId = currentEventId;
                TotalCreditEventStart = eventStartDT;
                TotalCreditEventEnd = eventEndDT;
                TotalCreditRewards = statusList[4];
            }
            else
            {
                isTotalSpendEventRunning = IsEventRunning;
                TotalSpendCurrEventId = currentEventId;
                TotalSpendEventStart = eventStartDT;
                TotalSpendEventEnd = eventEndDT;
                TotalSpendRewards = statusList[4];
            }

            InitTotalGoldRewards(isCredit);
        }

        private List<IInventoryItem> GetTotalGoldRewardItems(string rewardsDataStr)
        {
            List<string> rewardData = rewardsDataStr.Split('|').ToList();
            if (rewardData.Count <= 2)
            {
                return null;
            }

            List<IInventoryItem> rewardsList = new List<IInventoryItem>();
            if (rewardData.Count > 2)
            {
                int itemid1 = 0;
                if (int.TryParse(rewardData[2], out itemid1))
                {
                    int itemcount1 = 0;
                    int.TryParse(rewardData[3], out itemcount1);
                    IInventoryItem item1 = GameRepo.ItemFactory.GetInventoryItem(itemid1);
                    item1.StackCount = (ushort)itemcount1;
                    rewardsList.Add(item1);
                }
            }

            if (rewardData.Count > 4)
            {
                int itemid2 = 0;
                if (int.TryParse(rewardData[4], out itemid2))
                {
                    int itemcount2 = 0;
                    int.TryParse(rewardData[5], out itemcount2);
                    IInventoryItem item2 = GameRepo.ItemFactory.GetInventoryItem(itemid2);
                    item2.StackCount = (ushort)itemcount2;
                    rewardsList.Add(item2);
                }
            }

            if (rewardData.Count > 6)
            {
                int itemid3 = 0;
                if (int.TryParse(rewardData[6], out itemid3))
                {
                    int itemcount3 = 0;
                    int.TryParse(rewardData[7], out itemcount3);
                    IInventoryItem item3 = GameRepo.ItemFactory.GetInventoryItem(itemid3);
                    item3.StackCount = (ushort)itemcount3;
                    rewardsList.Add(item3);
                }
            }

            return rewardsList;
        }

        private string SerializeStringList(List<string> dataList)
        {
            int listCount = dataList.Count;
            StringBuilder claimsStr = new StringBuilder();
            for (int i = 0; i < listCount; ++i)
            {
                claimsStr.Append(dataList[i]);

                if (i < listCount - 1)
                {
                    claimsStr.Append("|");
                }
            }

            return claimsStr.ToString();
        }

        public void InitTotalGoldRewards(bool isCredit)
        {
            if (isCredit)
            {
                totalCreditRewardList = DeserializeTotalGoldRewards(isCredit);
            }
            else
            {
                totalSpendRewardList = DeserializeTotalGoldRewards(isCredit);
            }
            DeserializeTotalGoldRewardClaimed(isCredit);
            SetTotalGoldInitialized(isCredit, true);
        }

        public void SetTotalGoldInitialized(bool isCredit, bool isInited)
        {
            if (isCredit)
            {
                isTotalCreditInitialized = isInited;
            }
            else
            {
                isTotalSpendInitialized = isInited;
            }
        }

        public bool IsTotalGoldInitialized(bool isCredit)
        {
            return isCredit ? isTotalCreditInitialized : isTotalSpendInitialized;
        }

        public List<CreditSpendReward> DeserializeTotalGoldRewards(bool isCredit)
        {
            List<CreditSpendReward> rewards = new List<CreditSpendReward>();
            string totalGoldRewards = isCredit ? TotalCreditRewards : TotalSpendRewards;

            if (!(string.IsNullOrEmpty(totalGoldRewards) || totalGoldRewards == "0"))
            {
                List<string> rewardDataStrList = totalGoldRewards.Split(';').ToList();

                for (int i = 0; i < rewardDataStrList.Count; ++i)
                {
                    List<string> rewardDataStr = rewardDataStrList[i].Split('|').ToList();
                    int rewardId = 0;
                    if (int.TryParse(rewardDataStr[0], out rewardId))
                    {
                        int creditCount = 0;
                        int.TryParse(rewardDataStr[1], out creditCount);
                        int maxClaim = 0;
                        int.TryParse(rewardDataStr[2], out maxClaim);

                        List<IInventoryItem> rewardList = new List<IInventoryItem>();
                        for (int j = 3; j < rewardDataStr.Count; j += 2)
                        {
                            int itemId = 0;
                            if (int.TryParse(rewardDataStr[j], out itemId))
                            {
                                IInventoryItem rewardItem = GameRepo.ItemFactory.GetInventoryItem(itemId);
                                if (rewardItem != null)
                                {
                                    int stackCount = 0;
                                    if (int.TryParse(rewardDataStr[j + 1], out stackCount))
                                    {
                                        rewardItem.StackCount = stackCount;
                                        rewardList.Add(rewardItem);
                                    }
                                }
                            }
                        }

                        rewards.Add(new CreditSpendReward(rewardId, creditCount, maxClaim, rewardList));
                    }
                }
            }

            return rewards;
        }

        public string SerializeTotalGoldRewardClaimed(bool isCredit)
        {
            List<CreditSpendReward> totalGoldRewardList = isCredit ? totalCreditRewardList : totalSpendRewardList;

            StringBuilder claimedStr = new StringBuilder();
            for (int i = 0; i < totalGoldRewardList.Count; ++i)
            {
                if (totalGoldRewardList[i].mClaimCount > 0)
                {
                    claimedStr.Append(totalGoldRewardList[i].mRewardId);
                    claimedStr.Append("|");
                    claimedStr.Append(totalGoldRewardList[i].mClaimCount);
                    claimedStr.Append(";");
                }
            }

            string claimed = claimedStr.ToString();
            claimed = claimed.TrimEnd(';');

            if (isCredit)
            {
                TotalCreditClaims = claimed;
            }
            else
            {
                TotalSpendClaims = claimed;
            }

            return claimed;
        }

        public void DeserializeTotalGoldRewardClaimed(bool isCredit)
        {
            string totalGoldClaims = isCredit ? TotalCreditClaims : TotalSpendClaims;
            List<CreditSpendReward> totalGoldRewardList = isCredit ? totalCreditRewardList : totalSpendRewardList;

            if (string.IsNullOrEmpty(totalGoldClaims) || totalGoldClaims == "0")
            {
                return;
            }

            if (totalGoldRewardList.Count == 0)
            {
                return;
            }

            List<string> claimedStrList = totalGoldClaims.Split(';').ToList();
            for (int i = 0; i < claimedStrList.Count; ++i)
            {
                List<string> claimedDataList = claimedStrList[i].Split('|').ToList();
                int rewardId = 0;
                int claimCount = 0;
                if (int.TryParse(claimedDataList[0], out rewardId) && int.TryParse(claimedDataList[1], out claimCount))
                {
                    int rewardPos = totalGoldRewardList.FindIndex(o => o.mRewardId == rewardId);
                    if (rewardPos > -1)
                    {
                        totalGoldRewardList[rewardPos].mClaimCount = claimCount;
                    }
                }
            }
        }

        public List<CreditSpendReward> DeserializeTotalGoldRewardClaimed(bool isCredit, List<CreditSpendReward> rewardsList)
        {
            string totalGoldClaims = isCredit ? TotalCreditClaims : TotalSpendClaims;

            if (string.IsNullOrEmpty(totalGoldClaims) || totalGoldClaims == "0")
            {
                return rewardsList;
            }

            if (rewardsList.Count == 0)
            {
                return rewardsList;
            }

            List<string> claimedStrList = totalGoldClaims.Split(';').ToList();
            for (int i = 0; i < claimedStrList.Count; ++i)
            {
                List<string> claimedDataList = claimedStrList[i].Split('|').ToList();
                int rewardId = 0;
                int claimCount = 0;
                if (int.TryParse(claimedDataList[0], out rewardId) && int.TryParse(claimedDataList[1], out claimCount))
                {
                    int rewardPos = rewardsList.FindIndex(o => o.mRewardId == rewardId);
                    if (rewardPos > -1)
                    {
                        rewardsList[rewardPos].mClaimCount = claimCount;
                    }
                }
            }

            return rewardsList;
        }

        public List<CreditSpendReward> GetTotalGoldRewards(bool isCredit)
        {
            return isCredit ? totalCreditRewardList : totalSpendRewardList;
        }

        public List<IInventoryItem> GetTotalGoldReward(bool isCredit, int rewardId)
        {
            CreditSpendReward reward = isCredit ? totalCreditRewardList.Find(o => o.mRewardId == rewardId) : totalSpendRewardList.Find(o => o.mRewardId == rewardId);
            if (reward != null)
            {
                return reward.mRewardList;
            }

            return new List<IInventoryItem>();
        }

        public bool IsTotalGoldRewardClaimed(bool isCredit, int rewardId)
        {
            CreditSpendReward reward = isCredit ? totalCreditRewardList.Find(o => o.mRewardId == rewardId) : totalSpendRewardList.Find(o => o.mRewardId == rewardId);
            if (reward == null)
            {
                return false;
            }

            return reward.mClaimCount >= reward.mMaxClaim;
        }

        public void ClaimTotalGoldReward(bool isCredit, int rewardId)
        {
            int rewardPos = isCredit ? totalCreditRewardList.FindIndex(o => o.mRewardId == rewardId) : totalSpendRewardList.FindIndex(o => o.mRewardId == rewardId);

            if (rewardPos != -1)
            {
                if (isCredit)
                {
                    totalCreditRewardList[rewardPos].mClaimCount += 1;
                }
                else
                {
                    totalSpendRewardList[rewardPos].mClaimCount += 1;
                }
            }
        }

        // Total Credit
        public bool IsTotalCreditEventRunning()
        {
            return isTotalCreditEventRunning;
        }

        public int GetTotalCreditPreviousEventId()
        {
            return TotalCreditPrevEventId;
        }

        public int GetTotalCreditCurrentEventId()
        {
            return TotalCreditCurrEventId;
        }

        public DateTime GetTotalCreditEventStart()
        {
            return new DateTime(TotalCreditEventStart);
        }

        public DateTime GetTotalCreditEventEnd()
        {
            return new DateTime(TotalCreditEventEnd);
        }

        public int GetTotalCreditedGold()
        {
            return TotalGoldCredited;
        }

        public List<IInventoryItem> GetTotalCreditReward(int rewardId)
        {
            CreditSpendReward reward = totalCreditRewardList.Find(o => o.mRewardId == rewardId);
            if (reward != null)
            {
                return reward.mRewardList;
            }

            return new List<IInventoryItem>();
        }

        public bool IsTotalCreditRewardClaimed(int rewardId)
        {
            CreditSpendReward reward = totalCreditRewardList.Find(o => o.mRewardId == rewardId);
            if (reward == null)
            {
                return false;
            }

            return reward.mClaimCount >= reward.mMaxClaim;
        }

        public void ClaimTotalCreditReward(int rewardId)
        {
            int rewardPos = totalCreditRewardList.FindIndex(o => o.mRewardId == rewardId);

            if (rewardPos != -1)
            {
                totalCreditRewardList[rewardPos].mClaimCount += 1;
            }
        }

        // Total Spend
        public bool IsTotalSpendEventRunning()
        {
            return isTotalSpendEventRunning;
        }

        public int GetTotalSpendPreviousEventId()
        {
            return TotalSpendPrevEventId;
        }

        public int GetTotalSpendCurrentEventId()
        {
            return TotalSpendCurrEventId;
        }

        public DateTime GetTotalSpendEventStart()
        {
            return new DateTime(TotalSpendEventStart);
        }

        public DateTime GetTotalSpendEventEnd()
        {
            return new DateTime(TotalSpendEventEnd);
        }

        public int GetTotalSpentGold()
        {
            return TotalGoldSpent;
        }

        public List<IInventoryItem> GetTotalSpendReward(int rewardId)
        {
            CreditSpendReward reward = totalSpendRewardList.Find(o => o.mRewardId == rewardId);
            if (reward != null)
            {
                return reward.mRewardList;
            }

            return new List<IInventoryItem>();
        }

        public bool IsTotalSpendRewardClaimed(int rewardId)
        {
            CreditSpendReward reward = totalSpendRewardList.Find(o => o.mRewardId == rewardId);
            if (reward == null)
            {
                return false;
            }

            return reward.mClaimCount >= reward.mMaxClaim;
        }

        public void ClaimTotalSpendReward(int rewardId)
        {
            int rewardPos = totalSpendRewardList.FindIndex(o => o.mRewardId == rewardId);

            if (rewardPos != -1)
            {
                totalSpendRewardList[rewardPos].mClaimCount += 1;
            }
        }

        // Daily Gold
        public bool GetDailyGoldFirstTimeLogin()
        {
            return DailyGoldFirstLogin;
        }


        public void BuyMonthlyCard()
        {
            MonthlyCardBoughtDayNum = 0;
            MonthlyCardBought = true;
        }

        public bool IsMonthlyCardBought()
        {
            return MonthlyCardBought;
        }

        public int GetMonthlyCardBoughtDayNum()
        {
            return MonthlyCardBoughtDayNum;
        }

        public void ClaimMonthlyCardGold()
        {
            MonthlyCardGoldCollected = true;
        }

        public bool IsMonthlyCardGoldCollected()
        {
            return MonthlyCardGoldCollected;
        }

        public void BuyPermanentCard()
        {
            PermanentCardBought = true;
        }

        public bool IsPermanentCardBought()
        {
            return PermanentCardBought;
        }

        public void ClaimPermanentCardGold()
        {
            PermanentCardGoldCollected = true;
        }

        public bool IsPermanentCardGoldCollected()
        {
            return PermanentCardGoldCollected;
        }

        // Gold Jackpot
        public void SetGoldJackpotEventRunning(bool isEventRunning)
        {
            isGoldJackpotEventRunning = isEventRunning;
        }

        public bool IsGoldJackpotEventRunning()
        {
            return isGoldJackpotEventRunning;
        }

        public void SetGoldJackpotPreviousEventId(int eventid)
        {
            GoldJackpotPrevEventId = eventid;
        }

        public int GetGoldJackpotPreviousEventId()
        {
            return GoldJackpotPrevEventId;
        }

        public void SetGoldJackpotCurrentEventId(int eventid)
        {
            GoldJackpotCurrEventId = eventid;
        }

        public int GetGoldJackpotCurrentEventId()
        {
            return GoldJackpotCurrEventId;
        }

        public void SetGoldJackpotEventStart(long ticks)
        {
            GoldJackpotEventStart = ticks;
        }

        public DateTime GetGoldJackpotEventStart()
        {
            return new DateTime(GoldJackpotEventStart);
        }

        public void SetGoldJackpotEventEnd(long ticks)
        {
            GoldJackpotEventEnd = ticks;
        }

        public DateTime GetGoldJackpotEventEnd()
        {
            return new DateTime(GoldJackpotEventEnd);
        }

        public int GetGoldJackpotNextTierNum()
        {
            return GoldJackpotCurrTier + 1;
        }

        public void SetGoldJackpotTierList(List<GoldJackpotTier> list)
        {
            GoldJackpotTierList = list;
        }

        //public void SetGoldJackpotNextTierGold(int amount)
        //{
        //    GoldJackpotTierGold = amount;
        //}

        public int GetGoldJackpotNextTierCost()
        {
            if (GoldJackpotTierList.Count == 0)
            {
                return 0;
            }

            int tier = 1;
            if (GetGoldJackpotNextTierNum() > GetGoldJackpotHighestTier())
            {
                tier = GoldJackpotCurrTier;
            }
            else
            {
                tier = GetGoldJackpotNextTierNum();
            }

            GoldJackpotTier tierData = GoldJackpotTierList.Find(o => o.Tier() == tier);

            if (tierData == null)
            {
                return 0;
            }

            return tierData.Cost();
        }

        public string GetGoldJackpotNextTierData()
        {
            if (GoldJackpotTierList.Count == 0)
            {
                return "";
            }

            int tier = 1;
            if (GetGoldJackpotNextTierNum() > GetGoldJackpotHighestTier())
            {
                tier = GoldJackpotCurrTier;
            }
            else
            {
                tier = GetGoldJackpotNextTierNum();
            }

            GoldJackpotTier tierData = GoldJackpotTierList.Find(o => o.Tier() == tier);

            if (tierData == null)
            {
                return "";
            }

            return tierData.TierData();
        }

        public void IncrementGoldJackTier()
        {
            ++GoldJackpotCurrTier;
        }

        public void SetGoldJackpotHighestTier(int highestTier)
        {
            GoldJackpotHighestTier = highestTier;
        }

        public int GetGoldJackpotHighestTier()
        {
            return GoldJackpotHighestTier;
        }

        public void ResetGoldJackpotTier()
        {
            GoldJackpotCurrTier = 0;
        }

        public void SetGoldJackpotResult(int result)
        {
            GoldJackpotResult = result.ToString();
        }

        public string GetGoldJackpotResult()
        {
            return GoldJackpotResult;
        }

        public void SetGoldJackpotGoldClaimed(int result)
        {
            GoldJackpotAllGold += result;
        }

        public int GetGoldJackpotGoldClaimed()
        {
            return GoldJackpotAllGold;
        }

        // Continuous Login
        public void SetContLoginEventRunning(bool isEventRunning)
        {
            isContLoginEventRunning = isEventRunning;
        }

        public bool IsContLoginEventRunning()
        {
            return isContLoginEventRunning;
        }

        public void SetContLoginCurrentEventId(int eventid)
        {
            ContLoginCurrEventId = eventid;
        }

        public int GetContLoginCurrentEventId()
        {
            return ContLoginCurrEventId;
        }

        public void SetContLoginEventStart(long ticks)
        {
            ContLoginEventStart = ticks;
        }

        public DateTime GetContLoginEventStart()
        {
            return new DateTime(ContLoginEventStart);
        }

        public void SetContLoginRewards(string rewardListStr)
        {
            ContLoginRewards = rewardListStr;

            InitContLoginRewards();
        }

        public void InitContLoginRewards()
        {
            contLoginRewardList = DeserializeContLoginRewards();
            DeserializeContLoginRewardClaimed();
        }

        public List<ContLoginReward> DeserializeContLoginRewards()
        {
            List<ContLoginReward> rewards = new List<ContLoginReward>();

            if (!(string.IsNullOrEmpty(ContLoginRewards) || ContLoginRewards == "0"))
            {
                List<string> rewardDataStrList = ContLoginRewards.Split(';').ToList();

                for (int i = 0; i < rewardDataStrList.Count; ++i)
                {
                    List<string> rewardDataStr = rewardDataStrList[i].Split('|').ToList();
                    int rewardId = 0;
                    if (int.TryParse(rewardDataStr[0], out rewardId))
                    {
                        List<IInventoryItem> rewardList = new List<IInventoryItem>();
                        for (int j = 1; j < rewardDataStr.Count; j += 2)
                        {
                            int itemId = 0;
                            if (int.TryParse(rewardDataStr[j], out itemId))
                            {
                                IInventoryItem rewardItem = GameRepo.ItemFactory.GetInventoryItem(itemId);
                                if (rewardItem != null)
                                {
                                    int stackCount = 0;
                                    if (int.TryParse(rewardDataStr[j + 1], out stackCount))
                                    {
                                        rewardItem.StackCount = stackCount;
                                        rewardList.Add(rewardItem);
                                    }
                                }
                            }
                        }

                        rewards.Add(new ContLoginReward(rewardId, rewardList));
                    }
                }
            }

            return rewards;
        }

        public string SerializeContLoginRewardClaimed()
        {
            StringBuilder claimedStr = new StringBuilder();
            for (int i = 0; i < contLoginRewardList.Count; ++i)
            {
                if (contLoginRewardList[i].mIsCollected)
                {
                    claimedStr.Append(contLoginRewardList[i].mRewardId);
                    claimedStr.Append("|");
                }
            }

            string claimed = claimedStr.ToString();
            claimed = claimed.TrimEnd('|');

            ContLoginClaims = claimed;

            return claimed;
        }

        public void DeserializeContLoginRewardClaimed()
        {
            if (string.IsNullOrEmpty(ContLoginClaims) || ContLoginClaims == "0")
            {
                return;
            }

            if (contLoginRewardList.Count == 0)
            {
                return;
            }

            List<string> claimedStrList = ContLoginClaims.Split('|').ToList();
            for (int i = 0; i < claimedStrList.Count; ++i)
            {
                int rewardId = 0;
                if (int.TryParse(claimedStrList[i], out rewardId))
                {
                    int rewardPos = contLoginRewardList.FindIndex(o => o.mRewardId == rewardId);
                    if (rewardPos > -1)
                    {
                        contLoginRewardList[rewardPos].mIsCollected = true;
                    }
                }
            }
        }

        public List<ContLoginReward> GetContLoginRewards()
        {
            return contLoginRewardList;
        }

        public ContLoginReward GetContLoginReward(int rewardId)
        {
            return contLoginRewardList.Find(o => o.mRewardId == rewardId);
        }

        public void ClaimContLogin(int rewardId)
        {
            int rewardPos = contLoginRewardList.FindIndex(o => o.mRewardId == rewardId);

            if (rewardPos != -1)
            {
                contLoginRewardList[rewardPos].mIsCollected = true;
            }
        }

        public bool IsContLoginRewardClaimed(int rewardId)
        {
            ContLoginReward reward = contLoginRewardList.Find(o => o.mRewardId == rewardId);

            if (reward != null)
            {
                return reward.mIsCollected;
            }

            return false;
        }

        public void ResetContLoginClaimed()
        {
            ContLoginClaims = "";
        }

        // New Day Reset
        public void NewDayReset()
        {
            // Online Prize
            OnlinePrizeClaim = 0;
            OnlineDuration = 0;

            // Sign In Prize
            DateTime newToday = DateTime.Today;

            // If new month
            if (LogoutDT.Year < newToday.Year || LogoutDT.Month < newToday.Month)
            {
                SignInPrizeFullClaim = 0;
                SignInPrizePartClaim = 0;
            }
            else
            {
                for (int i = 0; i < MAX_CONTLOGINCLAIMS; ++i)
                {
                    if (GameUtils.IsBitSet(SignInPrizePartClaim, i))
                    {
                        SignInPrizeFullClaim = GameUtils.SetBit(SignInPrizeFullClaim, i);
                        SignInPrizePartClaim = GameUtils.UnsetBit(SignInPrizePartClaim, i);
                    }
                }
            }

            TodayDT = newToday;

            // Daily Gold
            if (MonthlyCardBought && MonthlyCardBoughtDayNum < 30)
            {
                ++MonthlyCardBoughtDayNum;
            }
            else
            {
                MonthlyCardBought = false;
                MonthlyCardBoughtDayNum = 0;
            }
            MonthlyCardGoldCollected = false;
            PermanentCardGoldCollected = false;
        }
    }
}
