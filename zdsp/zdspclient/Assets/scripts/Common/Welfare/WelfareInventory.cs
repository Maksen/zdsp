using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using Zealot.Repository;

namespace Zealot.Common
{
    public class GoldReward
    {
        public int mLevel;
        public int mGold;

        public GoldReward(int level, int gold)
        {
            mLevel = level;
            mGold = gold;
        }
    }

    public class PlayerReward
    {
        public int mPlayerCount;
        public IInventoryItem mItem;

        public PlayerReward(int playerCount, IInventoryItem item)
        {
            mPlayerCount = playerCount;
            mItem = item;
        }
    }

    public class ServiceFundLvlReward
    {
        public int mRewardId;
        public int mLevel;
        public int mgoldReward;
        public bool mIsCollected;

        public ServiceFundLvlReward(int rewardId, int level, int goldReward)
        {
            mRewardId = rewardId;
            mLevel = level;
            mgoldReward = goldReward;
            mIsCollected = false;
        }
    }

    public class ServiceFundPplReward
    {
        public int mRewardId;
        public int mPplCount;
        public IInventoryItem mItem;
        //public List<IInventoryItem> mRewardList;
        public bool mIsCollected;

        public ServiceFundPplReward(int rewardId, int pplCount, int itemId, int itemCount)
        {
            mRewardId = rewardId;
            mPplCount = pplCount;
            mItem = GameRepo.ItemFactory.GetInventoryItem(itemId);
            mItem.StackCount = itemCount;
            mIsCollected = false;
        }

        //public void SetItemReward(int itemid, int itemcount)
        //{
        //    IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(itemid);
        //    if (item != null)
        //    {
        //        mItemId = itemid;
        //        mItemCount = itemcount;
        //    }
        //}
    }

    public class CreditSpendReward
    {
        public int mRewardId;
        public int mCreditCount;
        public int mMaxClaim;
        public List<IInventoryItem> mRewardList;
        public int mClaimCount;

        public CreditSpendReward(int rewardId, int creditCount, int maxClaim, List<IInventoryItem> rewardList)
        {
            mRewardId = rewardId;
            mCreditCount = creditCount;
            mMaxClaim = maxClaim;

            mRewardList = rewardList;

            mClaimCount = 0;
        }

        //public void Add(int itemid, int itemcount)
        //{
        //    IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(itemid);
        //    if (item != null)
        //    {
        //        item.StackCount = (ushort)itemcount;
        //        mRewardList.Add(item);
        //    }
        //}
    }

    public enum GetClaimsCode
    {
        Error_NoClaims,
        Success
    }

    public class TotalGoldGetClaimsRetval
    {
        public GetClaimsCode mClaimsCode;
        public int mClaimCount;

        public TotalGoldGetClaimsRetval(GetClaimsCode code, int claimCount)
        {
            mClaimsCode = code;
            mClaimCount = claimCount;
        }
    }

    public enum ClaimCode
    {
        ClaimFailed,
        ClaimSuccess
    }

    public class TotalGoldClaimRetval
    {
        public ClaimCode mClaimCode;
        public string mClaimStr;

        public TotalGoldClaimRetval(ClaimCode code, string claimStr)
        {
            mClaimCode = code;
            mClaimStr = claimStr;
        }
    }

    public class GoldJackpotTier
    {
        private int _tier;
        private int _cost;
        private string _tierData;

        public GoldJackpotTier(int tier, int cost, string tierData)
        {
            _tier = tier;
            _cost = cost;
            _tierData = tierData;
        }

        public int Tier()
        {
            return _tier;
        }

        public int Cost()
        {
            return _cost;
        }

        public string TierData()
        {
            return _tierData;
        }
    }

    public class ContLoginReward
    {
        public int mRewardId;
        public List<IInventoryItem> mRewardList;
        public bool mIsCollected;

        public ContLoginReward(int rewardId, List<IInventoryItem> rewardList)
        {
            mRewardId = rewardId;
            mRewardList = rewardList;
            mIsCollected = false;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class WelfareInventoryData
    {
        #region serializable properties
        // Resets everyday
        public DateTime TodayDT;

        // Updates each login
        public DateTime LogoutDT;

        // Permanent
        public DateTime FirstLoginDT;

        // Permanent
        public DateTime ServerStartDT;

        // Online Prize
        // Initialized when login
        public long StartTime;

        // Resets when days are skipped
        [JsonProperty(PropertyName = "continuousLoginDayNum")]
        public short ContinuousLoginDayNum;

        // Resets everyday
        [JsonProperty(PropertyName = "onlinePrizeClaim")]
        public int OnlinePrizeClaim;

        // Resets everyday
        [JsonProperty(PropertyName = "onlineDuration")]
        public long OnlineDuration;

        // SignIn Prize
        // Resets every month
        [JsonProperty(PropertyName = "signInPrizeFullClaim")]
        public int SignInPrizeFullClaim;

        // Resets everyday
        [JsonProperty(PropertyName = "signInPrizePartClaim")]
        public int SignInPrizePartClaim;

        // Open Service Funds
        public string OpenServiceFundLvlRewards;

        // Permanent
        [JsonProperty(PropertyName = "openServiceFundLvlCollection")]
        public int OpenServiceFundLvlCollection;

        // Permanent
        [JsonProperty(PropertyName = "openServiceFundLvlClaims")]
        public string OpenServiceFundLvlClaims;

        public List<ServiceFundLvlReward> OpenServiceFundLvlRewardList;

        public string OpenServiceFundPplRewards;

        // Permanent
        [JsonProperty(PropertyName = "openServiceFundPplCollection")]
        public int OpenServiceFundPplCollection;

        // Permanent
        [JsonProperty(PropertyName = "openServiceFundPplClaims")]
        public string OpenServiceFundPplClaims;

        public List<ServiceFundPplReward> OpenServiceFundPplRewardList;

        public int OpenServiceFundCost;

        // Permanent
        [JsonProperty(PropertyName = "openServiceFundClaimedNum")]
        public int OpenServiceFundClaimedNum;

        // Permanent
        public int OpenServiceJoinMemberNum;

        // Permanent
        [JsonProperty(PropertyName = "openServiceFundBought")]
        public bool OpenServiceFundBought;

        // First Gold Top Up
        public string FirstGoldBuyRewards;

        // Permanent but can be reset by GMTools
        public bool FirstGoldBuyFlag;

        // Permanent but can be reset by GMTools
        public bool FirstGoldBuyCollected;

        // Permanent but can be reset by GMTools
        //[JsonProperty(PropertyName = "firstGoldBuyCollected")]
        //public bool FirstGoldBuyCollected;

        // Total Credit
        public bool isTotalCreditInitialized;

        public bool isTotalCreditEventRunning;

        // Updates when event period ends
        [JsonProperty(PropertyName = "totalCreditPrevEventId")]
        public int TotalCreditPrevEventId;

        // Updates when event period ends
        [JsonProperty(PropertyName = "totalCreditCurrEventId")]
        public int TotalCreditCurrEventId;

        // Resets when event period ends
        public long TotalCreditEventStart;

        // Resets when event period ends
        public long TotalCreditEventEnd;

        // Updates when gold credited to account
        [JsonProperty(PropertyName = "totalGoldCredited")]
        public int TotalGoldCredited;

        // Resets when event period ends
        public string TotalCreditRewards;

        public List<CreditSpendReward> totalCreditRewardList;

        // Resets when event period ends
        [JsonProperty(PropertyName = "totalCreditClaims")]
        public string TotalCreditClaims;

        // Total Spend
        public bool isTotalSpendInitialized;

        public bool isTotalSpendEventRunning;

        // Updates when event period ends
        [JsonProperty(PropertyName = "totalSpendPrevEventId")]
        public int TotalSpendPrevEventId;

        // Updates when event period ends
        [JsonProperty(PropertyName = "totalSpendCurrEventId")]
        public int TotalSpendCurrEventId;

        // Resets when event period ends
        public long TotalSpendEventStart;

        // Resets when event period ends
        public long TotalSpendEventEnd;

        // Updates when gold deducted from account
        [JsonProperty(PropertyName = "totalGoldSpent")]
        public int TotalGoldSpent;

        // Resets when event period ends
        public string TotalSpendRewards;

        public List<CreditSpendReward> totalSpendRewardList;

        // Resets when event period ends
        [JsonProperty(PropertyName = "totalSpendClaims")]
        public string TotalSpendClaims;

        // Daily Gold
        // Permanent
        [JsonProperty(PropertyName = "dailyGoldFirstLogin")]
        public bool DailyGoldFirstLogin;

        // Updates daily
        [JsonProperty(PropertyName = "monthlyCardBought")]
        public bool MonthlyCardBought;

        // Resets every month
        [JsonProperty(PropertyName = "monthlyCardBoughtDayNum")]
        public int MonthlyCardBoughtDayNum;

        // Resets daily
        [JsonProperty(PropertyName = "monthlyCardGoldCollected")]
        public bool MonthlyCardGoldCollected;

        // Permanent
        [JsonProperty(PropertyName = "permanentCardBought")]
        public bool PermanentCardBought;

        // Resets daily
        [JsonProperty(PropertyName = "permanentCardGoldCollected")]
        public bool PermanentCardGoldCollected;

        // Gold Jackpot
        public bool isGoldJackpotEventRunning;

        // Updates when event period ends
        [JsonProperty(PropertyName = "goldJackpotPrevEventId")]
        public int GoldJackpotPrevEventId;

        // Updates when event period ends
        [JsonProperty(PropertyName = "goldJackpotCurrEventId")]
        public int GoldJackpotCurrEventId;

        // Resets when event period ends
        public long GoldJackpotEventStart;

        // Resets when event period ends
        public long GoldJackpotEventEnd;

        // Global data for GoldJackpot Gold and TierData
        public List<GoldJackpotTier> GoldJackpotTierList;

        // Updates each roll
        public int GoldJackpotTierGold;

        // Updates each roll
        public string GoldJackpotTierData;

        // Updates each roll
        public string GoldJackpotResult;

        // Updates each roll
        [JsonProperty(PropertyName = "goldJackpotAllGold")]
        public int GoldJackpotAllGold;

        // Updates each roll
        [JsonProperty(PropertyName = "goldJackpotCurrTier")]
        public int GoldJackpotCurrTier;

        public int GoldJackpotHighestTier;

        // Continuous Login
        public bool isContLoginEventRunning;

        // Updates when event period ends
        [JsonProperty(PropertyName = "contLoginCurrEventId")]
        public int ContLoginCurrEventId;

        // Resets when event period ends
        public long ContLoginEventStart;

        // Resets when event period ends
        public string ContLoginRewards;

        public List<ContLoginReward> contLoginRewardList;

        // Resets when event period ends
        [JsonProperty(PropertyName = "contLoginClaims")]
        public string ContLoginClaims;
        #endregion

        public const int MAX_DAILYACTIVE = 15;
        public const int MAX_CLAIMS = 10;
        public const int MAX_ONLNRWRDCLAIMS = 5;
        public const int MAX_CONTLOGINCLAIMS = 31;

        public WelfareInventoryData()
        {
            // Open Service Funds
            OpenServiceFundBought = false;

            // Total Credit
            TotalCreditClaims = "";

            // Total Spend
            TotalSpendClaims = "";

            // Continuous Login
            ContLoginClaims = "";
        }

        public void InitDefault()
        {
            // Continuous Login First Login DT
            ServerStartDT = DateTime.MaxValue;
            // Continuous Login Day Number
            ContinuousLoginDayNum = 1;
            // Continuous Login
            SignInPrizeFullClaim = 0;
            SignInPrizePartClaim = 0;

            // Online Rewards
            OnlinePrizeClaim = 0;

            // Total Credit
            TotalCreditClaims = "";

            // Total Spend
            TotalSpendClaims = "";

            // Open Service Funds
            OpenServiceFundLvlCollection = 0;
            OpenServiceFundPplCollection = 0;
            OpenServiceFundLvlClaims = "";
            OpenServiceFundPplClaims = "";
            OpenServiceFundClaimedNum = 0;
            OpenServiceFundBought = false;

            // Daily Gold
            DailyGoldFirstLogin = true;
            MonthlyCardBought = false;
            MonthlyCardBoughtDayNum = 0;
            MonthlyCardGoldCollected = false;
            PermanentCardBought = false;
            PermanentCardGoldCollected = false;

            // Continuous Login
            ContLoginClaims = "";
        }

        public int GetSignDayCount()
        {
            return GetSignInDayNum();
        }
    }
}