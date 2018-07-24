using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.Repository;
using Zealot.Server.Entities;
using Zealot.Server.Rules;

namespace Photon.LoadBalancing.GameServer
{
    public enum WelfareTaskState
    {
        Incomplete = 0,
        Complete = 1,
    }

    public class WelfareCtrlElement
    {
        internal int value;
        internal int unlockvalue;
        internal int id;
        internal string singkey;

        public WelfareCtrlElement(int id, int tv)
        {
            this.id = id;
            value = 0;
            unlockvalue = tv;
            singkey = "";
        }

        public WelfareCtrlElement(int id, int v, int uv)
        {
            this.id = id;
            value = v;
            unlockvalue = uv;
            singkey = "";
        }
    }

    public enum WelfareEventCode
    {
        NotStarted,
        NoMoreEvents,
        NoCurrent,
        SameEvent,
        NewEvent,
        ERROR_NOTFOUND
    }

    public class WelfareEventRetval
    {
        public WelfareEventCode eventCode;
        public int currEventId = -2;
        public int prevEventId = -2;
        public DateTime startDate = new DateTime(0);
        public DateTime endDate = new DateTime(0);
    }

    public class WelfareController
    {
        // Player Slot
        private GameClientPeer _peer;

        // Daily Active Tasks
        private Dictionary<string, WelfareCtrlElement> _incompleteTasksSing = new Dictionary<string, WelfareCtrlElement>();
        private Dictionary<string, List<WelfareCtrlElement>> _incompleteTasksMul = new Dictionary<string, List<WelfareCtrlElement>>();
        private Dictionary<string, int> _completedTasks = new Dictionary<string, int>();

        public WelfareController(GameClientPeer peer)
        {
            _peer = peer;

            _peer.CharacterData.WelfareInventory.InitTodayDT();
            _peer.CharacterData.WelfareInventory.InitLogoutDT(_peer.logoutDT);
        }

        public void UpdateWelfareContinuousDayNum()
        {
            short currentDay = _peer.CharacterData.WelfareInventory.ContinuousLoginDayNum;

            if (_peer.loginDT.Date > _peer.logoutDT.Date)
            {
                TimeSpan duration = _peer.loginDT.Date - _peer.logoutDT.Date;
                // If duration is 1 day
                if (duration.TotalHours <= 24 && currentDay < 7)
                {
                    currentDay++;
                }
                // Else duration is greater than 1 day
                else if (duration.TotalHours > 24)
                {
                    // Reset to 1
                    currentDay = 1;
                }

                _peer.CharacterData.WelfareInventory.UpdateContinuousLogin(currentDay);
            }
        }

        public void ResetOnNewDay()
        {
            UpdateWelfareContinuousDayNum();

            // Clear Character data
            _peer.CharacterData.WelfareInventory.NewDayReset();
            _peer.CharacterData.WelfareInventory.InitFromInventory(_peer.mPlayer.WelfareStats);
        }

        // SignIn Prize
        public void InitFirstLoginDT(int year, int month, int day)
        {
            _peer.CharacterData.WelfareInventory.InitFirstLoginDT(year, month, day);
        }
        public void InitServerStartDT(int year, int month, int day)
        {
            _peer.CharacterData.WelfareInventory.InitServerStartDT(year, month, day);
        }

        public DateTime GetServerStartDT()
        {
            return _peer.CharacterData.WelfareInventory.GetServerStartDT();
        }

        public void ClaimSignInPrizeFull(int dataid)
        {
            _peer.CharacterData.WelfareInventory.ClaimSignInPrizeFull(dataid);
            _peer.mPlayer.WelfareStats.continuousLoginFullCollection |= (1 << dataid);
        }

        public void ClaimSignInPrizePart(int dataid)
        {
            _peer.CharacterData.WelfareInventory.ClaimSignInPrizePart(dataid);
            _peer.mPlayer.WelfareStats.continuousLoginPartCollection |= (1 << dataid);
        }

        public bool IsSignInPrizeFullClaimed(int dataid)
        {
            return _peer.CharacterData.WelfareInventory.IsSignInPrizeFullClaimed(dataid);
        }

        public bool IsSignInPrizePartClaimed(int dataid)
        {
            return _peer.CharacterData.WelfareInventory.IsSignInPrizePartClaimed(dataid);
        }

        // Online Prize
        public void SetOnlineRewardsStartTime(long startTime)
        {
            _peer.CharacterData.WelfareInventory.SetOnlineRewardsStartTime(startTime);
        }

        public long GetOnlineRewardsStartTime()
        {
            return _peer.CharacterData.WelfareInventory.GetOnlineRewardsStartTime();
        }

        public void ClaimOnlineRewards(int dataid, int claimedItemId)
        {
            _peer.CharacterData.WelfareInventory.ClaimOnlineRewards(dataid, claimedItemId);
            _peer.mPlayer.WelfareStats.onlineRewardsClaims |= (1 << dataid);
            //_peer.mPlayer.WelfareStats.onlineRewardsClaimsItems[dataid] = claimedItemId;
        }

        public bool IsOnlineRewardsClaimed(int dataid)
        {
            return _peer.CharacterData.WelfareInventory.IsOnlineRewardsClaimed(dataid);
        }

        public bool IsAllOnlineRewardsClaimed()
        {
            return _peer.CharacterData.WelfareInventory.IsAllOnlineRewardsClaimed();
        }

        public long GetOnlineDuration()
        {
            TimeSpan duration = DateTime.Now - _peer.loginDT;
            return duration.Ticks;
        }

        public long GetLastOnlineDuration()
        {
            // Last online duration
            return _peer.CharacterData.WelfareInventory.OnlineDuration;
        }

        // Open Service Fund
        public int GetServiceFundCost()
        {
            return _peer.CharacterData.WelfareInventory.GetServiceFundCost();
        }

        public bool IsOpenServiceFundBought()
        {
            return _peer.CharacterData.WelfareInventory.IsOpenServiceFundBought();
        }

        public void BuyOpenServiceFund()
        {
            _peer.CharacterData.WelfareInventory.BuyOpenServiceFund();
            _peer.mPlayer.WelfareStats.serviceFundsBought = true;
        }

        public void UpdateJoinMembersNum(int newAmount)
        {
            _peer.CharacterData.WelfareInventory.UpdateJoinMembersNum(newAmount);
            _peer.mPlayer.WelfareStats.serviceFundsJoinMemberNum = newAmount;
        }

        public int GetCurrentServiceFundsMemberNum()
        {
            return _peer.CharacterData.WelfareInventory.GetCurrentServiceFundsMemberNum();
        }

        public void InitOpenServiceFundRewards()
        {
            _peer.CharacterData.WelfareInventory.InitOpenServiceFundLvlRewards();
            _peer.CharacterData.WelfareInventory.InitOpenServiceFundPplRewards();
        }

        public ServiceFundLvlReward GetOpenServiceFundLvlReward(int rewardId)
        {
            return _peer.CharacterData.WelfareInventory.GetOpenServiceFundLvlReward(rewardId);
        }

        public void ClaimLevelOpenServiceFund(int rewardId)
        {
            _peer.CharacterData.WelfareInventory.ClaimOpenServiceFundLvlReward(rewardId);
            //_peer.mPlayer.WelfareStats.serviceFundsLvlCollection |= InventoryUtils.GetBitShiftByPos(dataid);
        }

        public void SerializeLevelOpenServiceFundClaimed()
        {
            string claimed = _peer.CharacterData.WelfareInventory.SerializeOpenServiceFundLvlRewardClaimed();
            _peer.mPlayer.WelfareStats.serviceFundLvlClaims = claimed;
        }

        public void UpdateOpenServiceFundBoughtNum(int amount)
        {
            if (amount > 0)
            {
                _peer.CharacterData.WelfareInventory.UpdateOpenServiceFundBoughtNum(amount);
                _peer.mPlayer.WelfareStats.serviceFundsClaimNum += amount;
            }
        }

        public bool IsOpenServiceFundLevelClaimed(int rewardId)
        {
            return _peer.CharacterData.WelfareInventory.IsOpenServiceFundLvlRewardClaimed(rewardId);
        }

        public ServiceFundPplReward GetOpenServiceFundPplReward(int rewardId)
        {
            return _peer.CharacterData.WelfareInventory.GetOpenServiceFundPplReward(rewardId);
        }

        public void ClaimPlayerOpenServiceFund(int rewardId)
        {
            _peer.CharacterData.WelfareInventory.ClaimOpenServiceFundPplReward(rewardId);
            //_peer.mPlayer.WelfareStats.serviceFundsPplCollection |= InventoryUtils.GetBitShiftByPos(dataid);
        }

        public void SerializePlayerOpenServiceFundClaimed()
        {
            string claimed = _peer.CharacterData.WelfareInventory.SerializeOpenServiceFundPplRewardClaimed();
            _peer.mPlayer.WelfareStats.serviceFundPplClaims = claimed;
        }

        public bool IsOpenServiceFundPlayerClaimed(int rewardId)
        {
            return _peer.CharacterData.WelfareInventory.IsOpenServiceFundPplRewardClaimed(rewardId);
        }

        // First Gold Buy        
        public void CheckFirstGoldBuyReset()
        {
            //if(serverResetFlag != null && serverResetFlag.Count == 1)
            //{
            //    int key = (int)serverResetFlag[0]["reset"];
            //    bool isCollected = (key == 1) ? false : true;
            //    _peer.CharacterData.WelfareInventory.SetFirstGoldCreditFlag(isCollected);
            //    _peer.mPlayer.WelfareStats.firstGoldBuyCollected = isCollected;
            //}
            //else
            //{
            //    string charid = _peer.GetCharId();
            //    var charResetFlag = GameApplication.dbGM.Welfare.GetFirstBuyFlagCharReset(serverid, charid).Result;

            //    if (charResetFlag != null && charResetFlag.Count == 1)
            //    {
            //        int key = (int)charResetFlag[0]["reset"];
            //        bool isCollected = (key == 1) ? false : true;
            //        _peer.CharacterData.WelfareInventory.SetFirstGoldCreditFlag(isCollected);
            //        _peer.mPlayer.WelfareStats.firstGoldBuyCollected = isCollected;
            //    }
            //}
        }

        public List<IInventoryItem> GetFirstGoldCreditRewards()
        {
            return _peer.CharacterData.WelfareInventory.GetFirstGoldCreditRewards();
        }

        public void ClaimFirstGoldCredit()
        {
            if (_peer.CharacterData.FirstBuyCollected == 1 || IsFirstGoldCreditClaimed())
            {
                _peer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("welfare_FirstGoldBuyAlreadyClaimed"), "", false, _peer);

                return;
            }

            _peer.CharacterData.WelfareInventory.ClaimFirstGoldCredit();
            _peer.CharacterData.FirstBuyCollected = 1;
            _peer.mPlayer.WelfareStats.firstBuyCollected = 1;
        }

        public bool IsFirstGoldCreditClaimed()
        {
            return _peer.CharacterData.WelfareInventory.IsFirstGoldCreditClaimed();
        }

        public short GetContinuousLoginDayNum()
        {
            return _peer.CharacterData.WelfareInventory.GetSignInDayNum();
        }

        // Total Gold Credit/Spend
        public List<CreditSpendReward> GetTotalGoldRewards(bool isCredit)
        {
            //List<CreditSpendReward> rewards = new List<CreditSpendReward>();
            //List<IInventoryItem> rewardList = new List<IInventoryItem>();
            //List<string> rewardsDataList = null;
            //if (isCredit)
            //{
            //    if (!string.IsNullOrEmpty(_peer.CharacterData.WelfareInventory.TotalCreditRewards))
            //    {
            //        rewardsDataList = _peer.CharacterData.WelfareInventory.TotalCreditRewards.Split(';').ToList();
            //    }
            //}
            //else
            //{
            //    if (!string.IsNullOrEmpty(_peer.CharacterData.WelfareInventory.TotalSpendRewards))
            //    {
            //        rewardsDataList = _peer.CharacterData.WelfareInventory.TotalSpendRewards.Split(';').ToList();
            //    }
            //}

            //if (rewardsDataList != null)
            //{
            //    for (int i = 0; i < rewardsDataList.Count; ++i)
            //    {
            //        List<string> rewardsData = rewardsDataList[i].Split('|').ToList();

            //        int rewardid = 0;
            //        int.TryParse(rewardsData[0], out rewardid);
            //        int creditcount = 0;
            //        int.TryParse(rewardsData[1], out creditcount);
            //        int maxclaims = 0;
            //        int.TryParse(rewardsData[2], out maxclaims);

            //        for (int j = 3; j < rewardsData.Count; j += 2)
            //        {
            //            int itemid = 0;
            //            int.TryParse(rewardsData[j], out itemid);
            //            int itemcount = 0;
            //            int.TryParse(rewardsData[j + 1], out itemcount);
            //            if (itemid > 0)
            //            {
            //                reward.Add(itemid, itemcount);
            //            }
            //        }
            //        CreditSpendReward reward = new CreditSpendReward(rewardid, creditcount, maxclaims);

            //        rewards.Add(reward);
            //    }
            //}

            return _peer.CharacterData.WelfareInventory.GetTotalGoldRewards(isCredit);
        }

        public void SendTotalGoldUncollectedRewardsToPlayer(bool isCredit)
        {
            List<CreditSpendReward> rewardsList = _peer.CharacterData.WelfareInventory.DeserializeTotalGoldRewards(isCredit);
            _peer.CharacterData.WelfareInventory.DeserializeTotalGoldRewardClaimed(isCredit);

            for (int i = 0; i < rewardsList.Count; ++i)
            {

                CreditSpendReward reward = rewardsList[i];

                if (reward == null)
                {
                    continue;
                }

                //TotalGoldGetClaimsRetval retval = null;
                //int currClaimCount = 0;
                int currTotalGold = 0;

                //retval = GetTotalCreditCurrClaimCount(i);
                //currClaimCount = retval.mClaimCount;
                currTotalGold = isCredit ? GetTotalCreditedGold() : GetTotalSpentGold();

                //int remainingclaims = 0;
                //if (retval.mClaimsCode == GetClaimsCode.Error_NoClaims)
                //{
                //    remainingclaims = retval.mClaimCount;
                //}
                //else if (retval.mClaimsCode == GetClaimsCode.Success)
                //{
                //    remainingclaims = reward.mMaxClaim - currClaimCount;
                //}

                if (reward.mClaimCount >= reward.mMaxClaim)
                {
                    //_peer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_TotalCreditRewardAllClaimed"), "", false, _peer);
                    continue;
                }
                else if (reward.mClaimCount < reward.mMaxClaim)
                {
                    int nextClaimLevel = reward.mClaimCount + 1;
                    if (currTotalGold >= (reward.mCreditCount * nextClaimLevel))
                    {
                        // Get RewardList from WelfareInventory
                        List<IInventoryItem> rewardList = isCredit ? GetTotalCreditReward(reward.mRewardId) : GetTotalSpendReward(reward.mRewardId);

                        if (rewardList == null)
                        {
                            _peer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_GetTotalCreditRewardFailed"), "", false, _peer);
                            return;
                        }

                        string mailTitle = isCredit ? "Reward_WelfareTotalCredit" : "Reward_WelfareTotalSpend";
                        GameRules.SendMailWithItem(_peer.mPlayer.Name, mailTitle, rewardList);

                        /*ClaimCode claimRes = */
                        ClaimTotalCreditReward(reward.mRewardId);

                        //if (claimRes == ClaimCode.ClaimFailed)
                        //{
                        //    _peer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_ClaimTotalCreditRewardFailed"), "", false, _peer);
                        //}
                        //else
                        //{
                        //    _peer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Welfare_ClaimTotalCreditRewardSuccess"), "", false, _peer);
                        //}
                    }
                }
            }
        }

        public void SerializeTotalGoldClaimed(bool isCredit)
        {
            string claimed = _peer.CharacterData.WelfareInventory.SerializeTotalGoldRewardClaimed(isCredit);
            if (isCredit)
            {
                _peer.mPlayer.WelfareStats.totalCreditClaims = claimed;
            }
            else
            {
                _peer.mPlayer.WelfareStats.totalSpendClaims = claimed;
            }
        }

        // Total Gold Credit
        //public TotalGoldGetClaimsRetval GetTotalCreditCurrClaimCount(int rewardid)
        //{
        //    return _peer.CharacterData.WelfareInventory.GetTotalCreditCurrClaimCount(rewardid);
        //}

        public int GetTotalCreditedGold()
        {
            return _peer.CharacterData.WelfareInventory.GetTotalCreditedGold();
        }

        public void OnCredited(int credited)
        {
            if (_peer.CharacterData.WelfareInventory.TotalGoldCredited == 0)
            {
                _peer.CharacterData.FirstBuyFlag = 1;
                _peer.mPlayer.WelfareStats.firstBuyFlag = 1;
                WelfareRules.LogWelfareFirstTopUp("Welfare First Top Up", credited, _peer);
            }

            //I think here for total credited seems fair
            int before = _peer.mPlayer.WelfareStats.totalGoldCredited;
            _peer.CharacterData.WelfareInventory.TotalGoldCredited += credited;
            _peer.mPlayer.WelfareStats.totalGoldCredited += credited;
            _peer.mSevenDaysController.UpdateTask(NewServerActivityType.Points, credited);
            int after = _peer.mPlayer.WelfareStats.totalGoldCredited;

            // <Add> Log for TotalCredited
            WelfareRules.LogWelfareTotalCredit("Welfare Total Credited", credited, before, after, _peer);
        }

        public List<IInventoryItem> GetTotalCreditReward(int rewardId)
        {
            return _peer.CharacterData.WelfareInventory.GetTotalCreditReward(rewardId);
        }

        public bool IsTotalCreditRewardClaimed(int rewardId)
        {
            return _peer.CharacterData.WelfareInventory.IsTotalCreditRewardClaimed(rewardId);
        }

        public void ClaimTotalCreditReward(int rewardid)
        {
            /*TotalGoldClaimRetval res = */
            _peer.CharacterData.WelfareInventory.ClaimTotalCreditReward(rewardid);
            //if(res.mClaimCode == ClaimCode.ClaimSuccess)
            //{
            //    _peer.mPlayer.WelfareStats.totalCreditClaims = res.mClaimStr;
            //}

            //return res.mClaimCode;
        }

        // Total Gold Spend
        //public TotalGoldGetClaimsRetval GetTotalSpendCurrClaimCount(int dataid)
        //{
        //    return _peer.CharacterData.WelfareInventory.GetTotalSpendCurrClaimCount(dataid);
        //}

        public int GetTotalSpentGold()
        {
            return _peer.CharacterData.WelfareInventory.GetTotalSpentGold();
        }

        public void OnDeducted(int amount)
        {
            _peer.CharacterData.WelfareInventory.TotalGoldSpent += amount;
            _peer.mPlayer.WelfareStats.totalGoldSpent += amount;
        }

        public List<IInventoryItem> GetTotalSpendReward(int rewardId)
        {
            return _peer.CharacterData.WelfareInventory.GetTotalSpendReward(rewardId);
        }

        public bool IsTotalSpendRewardClaimed(int rewardId)
        {
            return _peer.CharacterData.WelfareInventory.IsTotalSpendRewardClaimed(rewardId);
        }

        public void ClaimTotalSpendReward(int rewardId)
        {
            /*TotalGoldClaimRetval res = */
            _peer.CharacterData.WelfareInventory.ClaimTotalSpendReward(rewardId);
            //if (res.mClaimCode == ClaimCode.ClaimSuccess)
            //{
            //    _peer.mPlayer.WelfareStats.totalSpendClaims = res.mClaimStr;
            //}

            //return res.mClaimCode;
        }

        // Daily Gold
        public void BuyMonthlyCard()
        {
            _peer.CharacterData.WelfareInventory.BuyMonthlyCard();
            _peer.mPlayer.WelfareStats.monthCardBought = true;
        }

        public bool IsMonthlyCardBought()
        {
            return _peer.CharacterData.WelfareInventory.IsMonthlyCardBought();
        }

        public int GetMonthlyCardBoughtDayNum()
        {
            return _peer.CharacterData.WelfareInventory.GetMonthlyCardBoughtDayNum();
        }

        public void ClaimMonthlyCardGold()
        {
            _peer.CharacterData.WelfareInventory.ClaimMonthlyCardGold();
            _peer.mPlayer.WelfareStats.monthCardGoldCollected = true;
        }

        public bool IsMonthlyCardGoldCollected()
        {
            return _peer.CharacterData.WelfareInventory.IsMonthlyCardGoldCollected();
        }

        public void BuyPermanentCard()
        {
            _peer.CharacterData.WelfareInventory.BuyPermanentCard();
            _peer.mPlayer.WelfareStats.permanentCardBought = true;
        }

        public bool IsPermanentCardBought()
        {
            return _peer.CharacterData.WelfareInventory.IsPermanentCardBought();
        }

        public void ClaimPermanentCardGold()
        {
            _peer.CharacterData.WelfareInventory.ClaimPermanentCardGold();
            _peer.mPlayer.WelfareStats.permanentCardGoldCollected = true;
        }

        public bool IsPermanentCardGoldCollected()
        {
            return _peer.CharacterData.WelfareInventory.IsPermanentCardGoldCollected();
        }

        // Gold Jackpot
        public int GetGoldJackpotNextTierNum()
        {
            return _peer.CharacterData.WelfareInventory.GetGoldJackpotNextTierNum();
        }

        public int GetGoldJackpotNextTierCost()
        {
            return _peer.CharacterData.WelfareInventory.GetGoldJackpotNextTierCost();
        }

        public void SetGoldJackpotTierData(string tierData)
        {
            List<GoldJackpotTier> tierList = new List<GoldJackpotTier>();
            List<string> tierDataListRaw = tierData.Split(';').ToList();
            int prevTier = 1;
            int prevCost = 0;
            StringBuilder tierDataStr = new StringBuilder();
            for (int i = 0; i < tierDataListRaw.Count; ++i)
            {
                List<string> tierDataList = tierDataListRaw[i].Split('|').ToList();
                if (tierDataList.Count < 5)
                {
                    continue;
                }

                int currTier = 0;
                bool getTier = int.TryParse(tierDataList[0], out currTier);

                int currCost = 0;
                int.TryParse(tierDataList[1], out currCost);

                // Changing to next tier
                if (getTier && prevTier != currTier)
                {
                    GoldJackpotTier tier = new GoldJackpotTier(prevTier, prevCost, tierDataStr.ToString());
                    tierList.Add(tier);

                    prevTier = currTier;
                    prevCost = currCost;

                    tierDataStr = new StringBuilder();
                    tierDataStr.Append(string.Format("{0}|{1}|{2}", tierDataList[2], tierDataList[3], tierDataList[4]));
                    tierDataStr.Append(";");
                }
                // Same tier
                else
                {
                    bool isOOB = (i + 1) >= tierDataListRaw.Count;
                    int nextTier = 0;
                    bool getNextTier = false;
                    if (!isOOB)
                    {
                        List<string> nextTierDataList = tierDataListRaw[i + 1].Split('|').ToList();
                        getNextTier = int.TryParse(nextTierDataList[0], out nextTier);
                    }

                    tierDataStr.Append(string.Format("{0}|{1}|{2}", tierDataList[2], tierDataList[3], tierDataList[4]));
                    if (!isOOB && getNextTier && currTier == nextTier)
                    {
                        tierDataStr.Append(";");
                    }

                    prevTier = currTier;
                    prevCost = currCost;
                }
            }

            GoldJackpotTier finaltier = new GoldJackpotTier(prevTier, prevCost, tierDataStr.ToString());
            tierList.Add(finaltier);

            _peer.CharacterData.WelfareInventory.SetGoldJackpotTierList(tierList);
        }

        public string GetGoldJackpotNextTierData()
        {
            return _peer.CharacterData.WelfareInventory.GetGoldJackpotNextTierData();
        }

        public int GetGoldJackpotHighestTier()
        {
            return _peer.CharacterData.WelfareInventory.GetGoldJackpotHighestTier();
        }

        public void ResetGoldJackpotTier()
        {
            _peer.CharacterData.WelfareInventory.ResetGoldJackpotTier();
        }

        public void SetGoldJackpotResult(int result)
        {
            int nextTier = _peer.CharacterData.WelfareInventory.GetGoldJackpotNextTierNum();
            int highestTier = _peer.CharacterData.WelfareInventory.GetGoldJackpotHighestTier();
            if (nextTier <= highestTier)
            {
                _peer.CharacterData.WelfareInventory.SetGoldJackpotResult(result);
                _peer.CharacterData.WelfareInventory.SetGoldJackpotGoldClaimed(result);
                _peer.mPlayer.WelfareStats.goldJackpotResult = result.ToString();
                _peer.mPlayer.WelfareStats.goldJackpotAllGold += result;

                _peer.CharacterData.WelfareInventory.IncrementGoldJackTier();
                ++_peer.mPlayer.WelfareStats.goldJackpotCurrTier;
            }
        }

        // Continuous Login
        public bool IsContLoginRewardClaimed(int rewardId)
        {
            return _peer.CharacterData.WelfareInventory.IsContLoginRewardClaimed(rewardId);
        }

        // General Helper functions
        public WelfareEventRetval GetEventStatus(List<Dictionary<string, object>> eventData, int currEventId)
        {
            WelfareEventRetval retval = new WelfareEventRetval();
            DateTime startDate = new DateTime(0);
            DateTime endDate = new DateTime(0);
            int lastElem = eventData.Count - 1;

            // Case 1: Jackpot events not yet started
            if (DateTime.Now < (DateTime)eventData[0]["start"])
            {
                retval.eventCode = WelfareEventCode.NotStarted;

                return retval;
            }
            // Case 2: All jackpot events over
            else if (DateTime.Now > (DateTime)eventData[lastElem]["end"])
            {
                Dictionary<string, object> entry = eventData[lastElem];
                int eventid = (int)entry["eventid"];
                DateTime start = (DateTime)entry["start"];
                DateTime end = (DateTime)entry["end"];

                retval.eventCode = WelfareEventCode.NotStarted;
                retval.currEventId = eventid;
                retval.prevEventId = eventid;
                retval.startDate = start;
                retval.endDate = end;

                return retval;
            }
            // Case 3: A)Is in an event or B)in between events
            else
            {
                int eventid = 0;
                for (int i = 0; i < eventData.Count; ++i)
                {
                    Dictionary<string, object> entry = eventData[i];

                    if (entry != null)
                    {
                        DateTime start = (DateTime)entry["start"];
                        DateTime end = (DateTime)entry["end"];
                        if (DateTime.Now >= start && DateTime.Now <= end)
                        {
                            eventid = (int)entry["eventid"];
                            startDate = start;
                            endDate = end;

                            break;
                        }
                    }
                }

                // B) No matching events found
                if (startDate == DateTime.MinValue || endDate == DateTime.MinValue)
                {
                    retval.eventCode = WelfareEventCode.NoCurrent;
                    retval.currEventId = 0;
                    retval.startDate = startDate;
                    retval.endDate = endDate;

                    return retval;
                }
                // A) Found an event
                else
                {
                    // If is a new event
                    if (eventid > 0 && currEventId != eventid)
                    {
                        retval.eventCode = WelfareEventCode.NewEvent;
                        retval.currEventId = eventid;
                        retval.prevEventId = currEventId;
                        retval.startDate = startDate;
                        retval.endDate = endDate;

                        return retval;
                    }
                    // If is the same event
                    else if (eventid > 0 && currEventId == eventid)
                    {
                        retval.eventCode = WelfareEventCode.SameEvent;
                        retval.startDate = startDate;
                        retval.endDate = endDate;

                        return retval;
                    }
                }
            }

            retval.eventCode = WelfareEventCode.ERROR_NOTFOUND;

            return retval;
        }

        public string GetTotalGoldRewardsData(List<Dictionary<string, object>> rewardsData)
        {
            StringBuilder rewardStr = new StringBuilder();

            if (rewardsData != null && rewardsData.Count > 0)
            {
                for (int i = 0; i < rewardsData.Count; ++i)
                {
                    Dictionary<string, object> entry = rewardsData[i];

                    int itemid1 = (int)entry["itemid1"];
                    int itemcount1 = (int)entry["itemcount1"];
                    int itemid2 = (int)entry["itemid2"];
                    int itemcount2 = (int)entry["itemcount2"];
                    int itemid3 = (int)entry["itemid3"];
                    int itemcount3 = (int)entry["itemcount3"];

                    if (itemid1 == -1 && itemid2 == -1 && itemid3 == -1)
                    {
                        continue;
                    }

                    rewardStr.Append(((int)entry["creditcount"]).ToString());
                    rewardStr.Append("|");
                    rewardStr.Append(((int)entry["maxclaim"]).ToString());
                    rewardStr.Append("|");
                    if (itemid1 > 0)
                    {
                        rewardStr.Append(itemid1.ToString());
                        rewardStr.Append("|");
                        rewardStr.Append(itemcount1.ToString());
                    }
                    if (itemid2 > 0)
                    {
                        rewardStr.Append("|");
                        rewardStr.Append(itemid2.ToString());
                        rewardStr.Append("|");
                        rewardStr.Append(itemcount2.ToString());
                    }
                    if (itemid3 > 0)
                    {
                        rewardStr.Append("|");
                        rewardStr.Append(itemid3.ToString());
                        rewardStr.Append("|");
                        rewardStr.Append(itemcount3.ToString());
                    }
                    if (i < rewardsData.Count - 1)
                    {
                        rewardStr.Append(";");
                    }
                }
            }

            return rewardStr.ToString();
        }

        public void InitTotalGoldRewards(bool isCredit)
        {
            _peer.CharacterData.WelfareInventory.InitTotalGoldRewards(isCredit);
        }

        public string InitTotalGoldClaims(int eventid, string rewardsData)
        {
            if (eventid <= 0)
            {
                return "";
            }

            StringBuilder claimsStr = new StringBuilder();
            List<string> rewardsDataList = rewardsData.Split(';').ToList();
            int listCount = rewardsDataList.Count;
            for (int i = 0; i < listCount; ++i)
            {
                claimsStr.Append("0");
                claimsStr.Append("|");
            }
            string claims = claimsStr.ToString();
            claims = claims.TrimEnd('|');

            return claims;
        }

        public void InitContLoginRewards()
        {
            _peer.CharacterData.WelfareInventory.InitContLoginRewards();
        }

        public ContLoginReward GetContLoginReward(int rewardId)
        {
            return _peer.CharacterData.WelfareInventory.GetContLoginReward(rewardId);
        }

        public void ClaimContLogin(int rewardId)
        {
            _peer.CharacterData.WelfareInventory.ClaimContLogin(rewardId);
        }

        public void SerializeContLoginClaimed()
        {
            string claimed = _peer.CharacterData.WelfareInventory.SerializeContLoginRewardClaimed();
            _peer.mPlayer.WelfareStats.contLoginClaims = claimed;
        }

        public void ResetContLoginClaimed()
        {
            _peer.CharacterData.WelfareInventory.ResetContLoginClaimed();
            string claimed = _peer.CharacterData.WelfareInventory.SerializeContLoginRewardClaimed();
            _peer.mPlayer.WelfareStats.contLoginClaims = claimed;
        }
    }
}
