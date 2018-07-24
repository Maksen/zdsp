using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.LoadBalancing.GameServer;
using Zealot.Repository;

namespace Zealot.Server.Rules
{
    public class EventRewards
    {
        private int _eventId;
        private string _rewardData;

        public EventRewards(int eventid, string rewardData)
        {
            _eventId = eventid;
            _rewardData = rewardData;
        }

        public int EventID()
        {
            return _eventId;
        }

        public string RewardData()
        {
            return _rewardData;
        }
    }

    public static class WelfareRules
    {
        // Open Service Fund
        private static int serviceFundCost = 0;
        private static int serviceFundBought = 0;
        private static int serviceFundJoinMembers = 0;
        private static string serviceFundLvlRewards = "0";
        private static string serviceFundPplRewards = "0";

        // First Gold Credit
        private static string firstGoldCreditRewards = "0";

        // Total Gold Credit
        private static bool isTotalCreditEventRunning = false;
        private static int TotalCreditCurrEventId = -1;
        private static long TotalCreditEventStart = 0;
        private static long TotalCreditEventEnd = 0;
        private static string TotalCreditRewards = "";
        private static List<EventRewards> TotalCreditAllRewards = new List<EventRewards>();

        // Total Gold Spend
        private static bool isTotalSpendEventRunning = false;
        private static int TotalSpendCurrEventId = -1;
        private static long TotalSpendEventStart = 0;
        private static long TotalSpendEventEnd = 0;
        private static string TotalSpendRewards = "";
        private static List<EventRewards> TotalSpendAllRewards = new List<EventRewards>();

        // Gold Jackpot
        private static bool IsGoldJackpotEventRunning = false;
        private static int GoldJackpotPrevEventId = -1;
        private static int GoldJackpotCurrEventId = -1;
        private static long GoldJackpotEventStart = 0;
        private static long GoldJackpotEventEnd = 0;
        private static string GoldJackpotTierData = "";
        private static int GoldJackpotHighestTier = 0;

        // Continuous Login
        //private static int                  contLoginMaxDays        = 0;
        private static bool isContLoginEventRunning = false;
        private static int ContLoginCurrEventId = -1;
        private static long ContLoginEventStart = 0;
        private static string ContLoginRewards = "";
        private static List<EventRewards> ContLoginAllRewards = new List<EventRewards>();


        public static void Init()
        {
            int serverid = GameApplication.Instance.GetMyServerId();

            // Open Service Fund
            InitServiceFunds(serverid);
            // First Gold Top Up
            InitFirstGoldCredit(serverid);
            // Total Gold Credit/Spend
            InitTotalCredit(serverid);
            InitTotalSpend(serverid);
            // Gold Jackpot
            InitGoldJackpot(serverid);
            // Continuous Login
            InitContinuousLogin(serverid);
        }

        // Open Service Fund
        private static void InitServiceFunds(int serverline)
        {
            var serviceFundsCost = GameApplication.dbGM.Welfare.GetServiceFundsCost(serverline).Result;
            if (serviceFundsCost != null && serviceFundsCost.Count > 0)
            {
                Dictionary<string, object> entry = serviceFundsCost[0];
                serviceFundCost = (int)entry["goldcost"];
            }

            var serviceFundsInstance = GameApplication.dbRepository.Progress.GetProgress(serverline).Result;

            serviceFundsInstance = serviceFundsInstance.OrderBy(entry => entry["serverline"]).ToList();
            foreach (var entry in serviceFundsInstance)
            {
                if (entry == null)
                {
                    continue;
                }

                string key = (string)entry["key"];
                string val = (string)entry["value"];
                if (key == "serviceFundJoinMembers")
                {
                    int.TryParse(val, out serviceFundJoinMembers);
                }
            }

            var serviceFundsLvlRewards = GameApplication.dbGM.Welfare.GetServiceFundsLvlRewards(serverline).Result;
            if (serviceFundsLvlRewards != null && serviceFundsLvlRewards.Count > 0)
            {
                Dictionary<string, object> firstEntry = serviceFundsLvlRewards[0];
                StringBuilder rewardStr = new StringBuilder(string.Format("{0}|{1}|{2}", (int)firstEntry["rewardid"], (int)firstEntry["level"], (int)firstEntry["goldreward"]));
                rewardStr.Append(";");
                int rewardCount = serviceFundsLvlRewards.Count;
                for (int i = 1; i < rewardCount; ++i)
                {
                    Dictionary<string, object> entry = serviceFundsLvlRewards[i];
                    rewardStr.Append(string.Format("{0}|{1}|{2}", (int)entry["rewardid"], (int)entry["level"], (int)entry["goldreward"]));

                    if (i < rewardCount - 1)
                    {
                        rewardStr.Append(";");
                    }
                }

                serviceFundLvlRewards = rewardStr.ToString();
            }

            var serviceFundsPplRewards = GameApplication.dbGM.Welfare.GetServiceFundsPplRewards(serverline).Result;
            if (serviceFundsPplRewards != null && serviceFundsPplRewards.Count > 0)
            {
                Dictionary<string, object> firstEntry = serviceFundsPplRewards[0];
                StringBuilder rewardStr = new StringBuilder(string.Format("{0}|{1}|{2}|{3}", (int)firstEntry["rewardid"], (int)firstEntry["pplcount"], (int)firstEntry["itemid"], (int)firstEntry["itemcount"]));
                rewardStr.Append(";");
                int rewardCount = serviceFundsPplRewards.Count;
                for (int i = 1; i < rewardCount; ++i)
                {
                    Dictionary<string, object> entry = serviceFundsPplRewards[i];
                    rewardStr.Append(string.Format("{0}|{1}|{2}|{3}", (int)entry["rewardid"], (int)entry["pplcount"], (int)entry["itemid"], (int)entry["itemcount"]));

                    if (i < rewardCount - 1)
                    {
                        rewardStr.Append(";");
                    }
                }

                serviceFundPplRewards = rewardStr.ToString();
            }
        }

        public static int GetServiceFundCost()
        {
            return serviceFundCost;
        }

        public static void BuyServiceFund()
        {
            ++serviceFundJoinMembers;
            List<GameClientPeer> peerList = GameApplication.Instance.GetCharPeerList();
            for (int i = 0; i < peerList.Count; ++i)
            {
                GameClientPeer peer = peerList[i];
                if (peer != null && peer.mPlayer != null)
                {
                    peer.mWelfareCtrlr.UpdateJoinMembersNum(serviceFundJoinMembers);
                }
            }
            SaveToDB();
        }

        public static string GetServiceFundLvlRewards()
        {
            return serviceFundLvlRewards;
        }

        public static string GetServiceFundPplRewards()
        {
            return serviceFundPplRewards;
        }

        public static void SaveToDB()
        {
            int serverid = GameApplication.Instance.GetMyServerId();
            //var serviceFundBuySaved = GameApplication.dbRepository.Progress.Update(serverid, "serviceFundBought", serviceFundBought);
            var serviceFundJoinMemSaved = GameApplication.dbRepository.Progress.Update(serverid, "serviceFundJoinMembers", serviceFundJoinMembers);
        }

        public static string SerializeWelfareServiceFund()
        {
            StringBuilder serializedData = new StringBuilder();
            serializedData.Append(serviceFundCost.ToString());
            serializedData.Append("*");
            serializedData.Append(serviceFundBought.ToString());
            serializedData.Append("*");
            serializedData.Append(serviceFundJoinMembers.ToString());
            serializedData.Append("*");
            serializedData.Append(serviceFundLvlRewards);
            serializedData.Append("*");
            serializedData.Append(serviceFundPplRewards);

            return serializedData.ToString();
        }

        // First Gold Top Up
        private static void InitFirstGoldCredit(int serverid)
        {
            var firstCreditRewards = GameApplication.dbGM.Welfare.GetFirstCreditRewards(serverid).Result;
            if (firstCreditRewards != null && firstCreditRewards.Count > 0)
            {
                Dictionary<string, object> entry = firstCreditRewards[0]; // There should be only 1 entry
                StringBuilder rewardStr = new StringBuilder();
                int rewardCount = 6;
                for (int i = 0; i < rewardCount; ++i)
                {
                    string itemidStr = string.Format("itemid{0}", i + 1);
                    string itemcountStr = string.Format("itemcount{0}", i + 1);

                    int itemid = (int)entry[itemidStr];
                    int itemcount = (int)entry[itemcountStr];

                    if (itemid > 0)
                    {
                        if (i > 0)
                        {
                            rewardStr.Append(";");
                        }

                        rewardStr.Append(string.Format("{0}|{1}", (int)entry[itemidStr], (int)entry[itemcountStr]));
                    }
                }

                firstGoldCreditRewards = rewardStr.ToString();
            }
        }

        public static string SerializeWelfareFirstGoldCredit()
        {
            return firstGoldCreditRewards;
        }

        // Total Gold Credit/Spend
        private static void CheckTotalGoldEventRunning()
        {
            int serverid = GameApplication.Instance.GetMyServerId();

            Task totalCreditTask = UpdateTotalCredit(serverid);
            Task totalSpendTask = UpdateTotalSpend(serverid);
        }

        // Total Gold Credit
        private static void InitTotalCredit(int serverid)
        {
            List<Dictionary<string, object>> totalCreditEventData = GameApplication.dbGM.Welfare.GetTotalCreditEventData(serverid).Result;
            List<Dictionary<string, object>> totalCreditAllRewardData = GameApplication.dbGM.Welfare.GetTotalCreditRewardsAll(serverid).Result;
            RefreshTotalCredit(totalCreditEventData, totalCreditAllRewardData);
        }

        private static async Task UpdateTotalCredit(int serverid)
        {
            List<Dictionary<string, object>> totalCreditEventData = await GameApplication.dbGM.Welfare.GetTotalCreditEventData(serverid);
            List<Dictionary<string, object>> totalCreditAllRewardData = await GameApplication.dbGM.Welfare.GetTotalCreditRewardsAll(serverid);
            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                RefreshTotalCredit(totalCreditEventData, totalCreditAllRewardData);
            });
        }

        public static void RefreshTotalCredit(List<Dictionary<string, object>> totalCreditEventData,
            List<Dictionary<string, object>> totalCreditAllRewardData)
        {
            totalCreditEventData = totalCreditEventData.OrderBy(entry => entry["start"]).ToList();

            if (totalCreditEventData != null && totalCreditEventData.Count > 0)
            {
                WelfareEventRetval retval = GetEventStatus(totalCreditEventData, TotalCreditCurrEventId);

                // Case 1: Total credit events not yet started
                if (retval.eventCode == WelfareEventCode.NotStarted)
                {
                    //TotalCreditPrevEventId  = -1;
                    TotalCreditCurrEventId = -1;

                    TotalCreditEventStart = 0;
                    TotalCreditEventEnd = 0;

                    //TotalCreditClaims       = "";

                    isTotalCreditEventRunning = false;
                }
                // Case 2: All total credit events over
                // Send unclaimed rewards to player via mail
                else if (retval.eventCode == WelfareEventCode.NoMoreEvents)
                {
                    //TotalCreditPrevEventId  = retval.prevEventId;
                    TotalCreditCurrEventId = retval.currEventId;

                    TotalCreditEventStart = retval.startDate.Ticks;
                    TotalCreditEventEnd = retval.endDate.Ticks;

                    isTotalCreditEventRunning = false;
                }
                // Case 3: A)Is in an event or B)in between events
                // B) No matching events found
                // Send unclaimed rewards to player via mail
                else if (retval.eventCode == WelfareEventCode.NoCurrent)
                {
                    TotalCreditCurrEventId = 0;

                    TotalCreditEventStart = retval.startDate.Ticks;
                    TotalCreditEventEnd = retval.endDate.Ticks;

                    //TotalCreditClaims       = "";

                    isTotalCreditEventRunning = false;
                }
                // A) Found an event
                // If is a new event
                // Send unclaimed rewards to player via mail
                else if (retval.eventCode == WelfareEventCode.NewEvent)
                {
                    //TotalCreditPrevEventId  = retval.prevEventId;
                    TotalCreditCurrEventId = retval.currEventId;

                    TotalCreditEventStart = retval.startDate.Ticks;
                    TotalCreditEventEnd = retval.endDate.Ticks;

                    isTotalCreditEventRunning = true;
                }
                // A) Found an event
                // If is the same event
                //else if (retval.eventCode == WelfareEventCode.SameEvent)
                //{
                //    TotalCreditEventStart    = retval.startDate.Ticks;
                //    TotalCreditEventEnd      = retval.endDate.Ticks;
                //}

                List<Dictionary<string, object>> totalCreditRewardData = null;
                if (TotalCreditCurrEventId > 0)
                {
                    totalCreditRewardData = RetrieveDataByEventID(TotalCreditCurrEventId, totalCreditAllRewardData);
                }

                TotalCreditRewards = GetTotalGoldRewardsData(totalCreditRewardData);
                TotalCreditAllRewards = GetTotalGoldAllRewardsData(totalCreditAllRewardData);

                //if (retval.eventCode != WelfareEventCode.SameEvent)
                //{
                //    //TotalCreditClaims = GetTotalGoldClaims(TotalCreditCurrEventId, totalCreditRewards);
                //}
            }
        }

        public static bool IsTotalCreditEventRunning()
        {
            return isTotalCreditEventRunning;
        }

        public static int GetTotalCreditEventId()
        {
            return TotalCreditCurrEventId;
        }

        public static string GetTotalCreditRewardData()
        {
            return TotalCreditRewards;
        }

        public static string GetTotalCreditRewardDataByEvent(int eventid)
        {
            if (TotalCreditAllRewards == null)
            {
                return "";
            }

            if (TotalCreditAllRewards.Count == 0)
            {
                return "";
            }

            EventRewards rewardData = TotalCreditAllRewards.Find(o => o.EventID() == eventid);
            if (rewardData == null)
            {
                return "";
            }

            return TotalCreditAllRewards.Find(o => o.EventID() == eventid).RewardData();
        }

        public static string SerializeTotalGoldCredit()
        {
            StringBuilder serializedData = new StringBuilder();
            serializedData.Append(isTotalCreditEventRunning ? 1 : 0);
            serializedData.Append("*");
            //serializedData.Append(TotalCreditPrevEventId.ToString());
            //serializedData.Append("*");
            serializedData.Append(TotalCreditCurrEventId.ToString());
            serializedData.Append("*");
            serializedData.Append(TotalCreditEventStart.ToString());
            serializedData.Append("*");
            serializedData.Append(TotalCreditEventEnd.ToString());
            serializedData.Append("*");
            serializedData.Append(TotalCreditRewards);

            return serializedData.ToString();
        }

        // Total Gold Spend
        private static void InitTotalSpend(int serverid)
        {
            List<Dictionary<string, object>> totalSpendEventData = GameApplication.dbGM.Welfare.GetTotalSpendEventData(serverid).Result;
            List<Dictionary<string, object>> totalSpendAllRewardData = GameApplication.dbGM.Welfare.GetTotalSpendRewardsAll(serverid).Result;
            RefreshTotalSpend(totalSpendEventData, totalSpendAllRewardData);
        }

        private static async Task UpdateTotalSpend(int serverid)
        {
            List<Dictionary<string, object>> totalSpendEventData = await GameApplication.dbGM.Welfare.GetTotalSpendEventData(serverid);
            List<Dictionary<string, object>> totalSpendAllRewardData = await GameApplication.dbGM.Welfare.GetTotalSpendRewardsAll(serverid);
            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                RefreshTotalSpend(totalSpendEventData, totalSpendAllRewardData);
            });
        }

        public static void RefreshTotalSpend(List<Dictionary<string, object>> totalSpendEventData,
            List<Dictionary<string, object>> totalSpendAllRewardData)
        {
            totalSpendEventData = totalSpendEventData.OrderBy(entry => entry["start"]).ToList();

            if (totalSpendEventData != null && totalSpendEventData.Count > 0)
            {
                WelfareEventRetval retval = GetEventStatus(totalSpendEventData, TotalSpendCurrEventId);

                // Case 1: Total Spend events not yet started
                if (retval.eventCode == WelfareEventCode.NotStarted)
                {
                    //TotalSpendPrevEventId = -1;
                    TotalSpendCurrEventId = -1;

                    TotalSpendEventStart = 0;
                    TotalSpendEventEnd = 0;

                    isTotalSpendEventRunning = false;
                }
                // Case 2: All total Spend events over
                // Send unclaimed rewards to player via mail
                else if (retval.eventCode == WelfareEventCode.NoMoreEvents)
                {
                    //TotalSpendPrevEventId = retval.prevEventId;
                    TotalSpendCurrEventId = retval.currEventId;

                    TotalSpendEventStart = retval.startDate.Ticks;
                    TotalSpendEventEnd = retval.endDate.Ticks;

                    isTotalSpendEventRunning = false;
                }
                // Case 3: A)Is in an event or B)in between events
                // B) No matching events found
                // Send unclaimed rewards to player via mail
                else if (retval.eventCode == WelfareEventCode.NoCurrent)
                {
                    TotalSpendCurrEventId = 0;

                    TotalSpendEventStart = retval.startDate.Ticks;
                    TotalSpendEventEnd = retval.endDate.Ticks;

                    isTotalSpendEventRunning = false;
                }
                // A) Found an event
                // If is a new event
                // Send unclaimed rewards to player via mail
                else if (retval.eventCode == WelfareEventCode.NewEvent)
                {
                    //TotalSpendPrevEventId = retval.prevEventId;
                    TotalSpendCurrEventId = retval.currEventId;

                    TotalSpendEventStart = retval.startDate.Ticks;
                    TotalSpendEventEnd = retval.endDate.Ticks;

                    isTotalSpendEventRunning = true;
                }
                // A) Found an event
                // If is the same event
                //else if (retval.eventCode == WelfareEventCode.SameEvent)
                //{
                //    TotalSpendEventStart = retval.startDate.Ticks;
                //    TotalSpendEventEnd = retval.endDate.Ticks;
                //}

                List<Dictionary<string, object>> totalSpendRewardData = null;
                if (TotalSpendCurrEventId > 0)
                {
                    totalSpendRewardData = RetrieveDataByEventID(TotalSpendCurrEventId, totalSpendAllRewardData);
                }

                TotalSpendRewards = GetTotalGoldRewardsData(totalSpendRewardData);
                TotalSpendAllRewards = GetTotalGoldAllRewardsData(totalSpendAllRewardData);

                //if (retval.eventCode != WelfareEventCode.SameEvent)
                //{
                //    //TotalSpendClaims = GetTotalGoldClaims(TotalCreditCurrEventId, totalSpendRewards);
                //}
            }
        }

        public static bool IsTotalSpendEventRunning()
        {
            return isTotalSpendEventRunning;
        }

        public static int GetTotalSpendEventId()
        {
            return TotalSpendCurrEventId;
        }

        public static string GetTotalSpendRewardData()
        {
            return TotalSpendRewards;
        }

        public static string GetTotalSpendRewardDataByEvent(int eventid)
        {
            if (TotalSpendAllRewards == null)
            {
                return "";
            }

            if (TotalSpendAllRewards.Count == 0)
            {
                return "";
            }

            EventRewards rewardData = TotalSpendAllRewards.Find(o => o.EventID() == eventid);
            if (rewardData == null)
            {
                return "";
            }

            return TotalSpendAllRewards.Find(o => o.EventID() == eventid).RewardData();
        }

        public static string SerializeTotalGoldSpend()
        {
            StringBuilder serializedData = new StringBuilder();
            serializedData.Append(isTotalSpendEventRunning ? 1 : 0);
            serializedData.Append("*");
            //serializedData.Append(TotalSpendPrevEventId.ToString());
            //serializedData.Append("*");
            serializedData.Append(TotalSpendCurrEventId.ToString());
            serializedData.Append("*");
            serializedData.Append(TotalSpendEventStart.ToString());
            serializedData.Append("*");
            serializedData.Append(TotalSpendEventEnd.ToString());
            serializedData.Append("*");
            serializedData.Append(TotalSpendRewards);

            return serializedData.ToString();
        }

        // Gold Jackpot
        private static void CheckGoldJackpotEventRunning()
        {
            int serverid = GameApplication.Instance.GetMyServerId();

            Task goldJackpotTask = UpdateGoldJackpot(serverid);
        }

        private static void InitGoldJackpot(int serverid)
        {
            List<Dictionary<string, object>> goldJackpotEventData = GameApplication.dbGM.Welfare.GetGoldJackpotEventData(serverid).Result;
            List<Dictionary<string, object>> goldJackpotAllJackpotData = GameApplication.dbGM.Welfare.GetGoldJackpotDataByServerID(serverid).Result;
            RefreshGoldJackpot(goldJackpotEventData, goldJackpotAllJackpotData);
        }

        private static async Task UpdateGoldJackpot(int serverid)
        {
            List<Dictionary<string, object>> goldJackpotEventData = await GameApplication.dbGM.Welfare.GetGoldJackpotEventData(serverid);
            List<Dictionary<string, object>> goldJackpotAllJackpotData = await GameApplication.dbGM.Welfare.GetGoldJackpotDataByServerID(serverid);
            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                RefreshGoldJackpot(goldJackpotEventData, goldJackpotAllJackpotData);
            });
        }

        public static void RefreshGoldJackpot(List<Dictionary<string, object>> goldJackpotEventData,
            List<Dictionary<string, object>> goldJackpotAllJackpotData)
        {
            goldJackpotEventData = goldJackpotEventData.OrderBy(entry => entry["start"]).ToList();

            if (goldJackpotEventData != null && goldJackpotEventData.Count > 0)
            {
                WelfareEventRetval retval = GetEventStatus(goldJackpotEventData, GoldJackpotCurrEventId);

                // Case 1: Jackpot events not yet started
                if (retval.eventCode == WelfareEventCode.NotStarted)
                {
                    GoldJackpotPrevEventId = -1;
                    GoldJackpotCurrEventId = -1;

                    GoldJackpotEventStart = 0;
                    GoldJackpotEventEnd = 0;

                    GoldJackpotTierData = "";

                    IsGoldJackpotEventRunning = false;
                }
                // Case 2: All jackpot events over
                else if (retval.eventCode == WelfareEventCode.NoMoreEvents)
                {
                    GoldJackpotPrevEventId = retval.prevEventId;
                    GoldJackpotCurrEventId = retval.currEventId;

                    GoldJackpotEventStart = retval.startDate.Ticks;
                    GoldJackpotEventEnd = retval.endDate.Ticks;

                    IsGoldJackpotEventRunning = false;
                }
                // Case 3: A)Is in an event or B)in between events
                // B) No matching events found
                else if (retval.eventCode == WelfareEventCode.NoCurrent)
                {
                    GoldJackpotCurrEventId = 0;

                    GoldJackpotEventStart = retval.startDate.Ticks;
                    GoldJackpotEventEnd = retval.endDate.Ticks;

                    IsGoldJackpotEventRunning = false;
                }
                // A) Found an event
                // If is a new event
                else if (retval.eventCode == WelfareEventCode.NewEvent)
                {
                    GoldJackpotPrevEventId = retval.prevEventId;
                    GoldJackpotCurrEventId = retval.currEventId;

                    GoldJackpotEventStart = retval.startDate.Ticks;
                    GoldJackpotEventEnd = retval.endDate.Ticks;

                    IsGoldJackpotEventRunning = true;
                }
                // A) Found an event
                // If is the same event
                //else if(retval.eventCode == WelfareEventCode.SameEvent)
                //{
                //    GoldJackpotEventStart    = retval.startDate.Ticks;
                //    GoldJackpotEventEnd      = retval.endDate.Ticks;
                //}

                List<Dictionary<string, object>> goldJackpotTierData = null;
                int highestTier = 0;
                if (retval.eventCode == WelfareEventCode.SameEvent || retval.eventCode == WelfareEventCode.NewEvent)
                {
                    goldJackpotTierData = RetrieveDataByEventID(GoldJackpotCurrEventId, goldJackpotAllJackpotData);
                    highestTier = RetrieveHighestTier(GoldJackpotCurrEventId, goldJackpotTierData);
                }
                else
                {
                    goldJackpotTierData = RetrieveDataByEventID(GoldJackpotPrevEventId, goldJackpotAllJackpotData);
                    highestTier = RetrieveHighestTier(GoldJackpotPrevEventId, goldJackpotTierData);
                }

                GoldJackpotHighestTier = highestTier;
                SetGoldJackpotTierData(goldJackpotTierData);
            }
        }

        private static int RetrieveHighestTier(int eventId, List<Dictionary<string, object>> goldJackpotTierData)
        {
            int highestTier = 0;
            for (int i = 0; i < goldJackpotTierData.Count; ++i)
            {
                Dictionary<string, object> entry = goldJackpotTierData[i];
                int currTier = (int)entry["tier"];
                if (currTier > highestTier)
                {
                    highestTier = currTier;
                }
            }

            return highestTier;
        }

        public static int GetGoldJackpotPrevEventId()
        {
            return GoldJackpotPrevEventId;
        }

        public static int GetGoldJackpotCurrEventId()
        {
            return GoldJackpotCurrEventId;
        }

        public static int GetGoldJackpotHighestTier()
        {
            return GoldJackpotHighestTier;
        }

        public static string GetGoldJackpotTierData()
        {
            return GoldJackpotTierData;
        }

        public static void SetGoldJackpotTierData(List<Dictionary<string, object>> goldJackpotTierData)
        {
            if (goldJackpotTierData != null && goldJackpotTierData.Count > 0)
            {
                StringBuilder tierDataStr = new StringBuilder();
                for (int i = 0; i < goldJackpotTierData.Count; ++i)
                {
                    Dictionary<string, object> entry = goldJackpotTierData[i];
                    int tier = (int)entry["tier"];
                    int cost = (int)entry["cost"];
                    int lowerbound = (int)entry["lowerbound"];
                    int upperbound = (int)entry["upperbound"];
                    int weight = (int)entry["weight"];

                    tierDataStr.Append(string.Format("{0}|{1}|{2}|{3}|{4}", tier.ToString(), cost.ToString(), lowerbound.ToString(), upperbound.ToString(), weight.ToString()));

                    if (i < (goldJackpotTierData.Count - 1))
                    {
                        tierDataStr.Append(";");
                    }
                }

                GoldJackpotTierData = tierDataStr.ToString();
            }
        }

        public static string SerializeGoldJackpot()
        {
            StringBuilder serializedData = new StringBuilder();
            serializedData.Append(IsGoldJackpotEventRunning ? 1 : 0);
            serializedData.Append("*");
            serializedData.Append(GoldJackpotPrevEventId.ToString());
            serializedData.Append("*");
            serializedData.Append(GoldJackpotCurrEventId.ToString());
            serializedData.Append("*");
            serializedData.Append(GoldJackpotEventStart.ToString());
            serializedData.Append("*");
            serializedData.Append(GoldJackpotEventEnd.ToString());
            serializedData.Append("*");
            serializedData.Append(GoldJackpotHighestTier);
            serializedData.Append("*");
            serializedData.Append(GoldJackpotTierData);

            return serializedData.ToString();
        }

        // Continuous Login
        private static void CheckContLoginEventRunning()
        {
            int serverid = GameApplication.Instance.GetMyServerId();

            Task contLoginTask = UpdateContinuousLogin(serverid);
        }

        private static void InitContinuousLogin(int serverid)
        {
            List<Dictionary<string, object>> contLoginEventData = GameApplication.dbGM.Welfare.GetContLoginEventData(serverid).Result;
            List<Dictionary<string, object>> contLoginAllRewardData = GameApplication.dbGM.Welfare.GetContLoginRewardsAll(serverid).Result;
            RefreshContLogin(contLoginEventData, contLoginAllRewardData);
        }

        private static async Task UpdateContinuousLogin(int serverid)
        {
            List<Dictionary<string, object>> contLoginEventData = await GameApplication.dbGM.Welfare.GetContLoginEventData(serverid);
            List<Dictionary<string, object>> contLoginAllRewardData = await GameApplication.dbGM.Welfare.GetContLoginRewardsAll(serverid);
            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                RefreshContLogin(contLoginEventData, contLoginAllRewardData);
            });
        }

        private static void RefreshContLogin(List<Dictionary<string, object>> contLoginEventData, List<Dictionary<string, object>> contLoginAllRewardData)
        {
            contLoginEventData = contLoginEventData.OrderBy(entry => entry["start"]).ToList();

            if (contLoginEventData != null && contLoginEventData.Count > 0)
            {
                WelfareEventRetval retval = GetEventStatus(contLoginEventData, ContLoginCurrEventId);

                // Case 1: Total Spend events not yet started
                if (retval.eventCode == WelfareEventCode.NotStarted)
                {
                    //ContLoginPrevEventId = -1;
                    ContLoginCurrEventId = -1;

                    ContLoginEventStart = 0;

                    isContLoginEventRunning = false;
                }
                // Case 2: All total Spend events over
                // Send unclaimed rewards to player via mail
                else if (retval.eventCode == WelfareEventCode.NoMoreEvents)
                {
                    //ContLoginPrevEventId = retval.prevEventId;
                    ContLoginCurrEventId = retval.currEventId;

                    ContLoginEventStart = retval.startDate.Ticks;

                    isContLoginEventRunning = false;
                }
                // Case 3: A)Is in an event or B)in between events
                // B) No matching events found
                // Send unclaimed rewards to player via mail
                else if (retval.eventCode == WelfareEventCode.NoCurrent)
                {
                    ContLoginCurrEventId = 0;

                    ContLoginEventStart = retval.startDate.Ticks;

                    isContLoginEventRunning = false;
                }
                // A) Found an event
                // If is a new event
                // Send unclaimed rewards to player via mail
                else if (retval.eventCode == WelfareEventCode.NewEvent)
                {
                    //ContLoginPrevEventId = retval.prevEventId;
                    ContLoginCurrEventId = retval.currEventId;

                    ContLoginEventStart = retval.startDate.Ticks;

                    isContLoginEventRunning = true;
                }

                List<Dictionary<string, object>> contLoginRewardData = null;
                if (ContLoginCurrEventId > 0)
                {
                    contLoginRewardData = RetrieveDataByEventID(ContLoginCurrEventId, contLoginAllRewardData);
                }

                ContLoginRewards = GetContLoginRewardsData(contLoginRewardData);
                ContLoginAllRewards = GetContLoginAllRewardsData(contLoginAllRewardData);
            }
        }

        public static bool IsContLoginEventRunning()
        {
            return isContLoginEventRunning;
        }

        public static int GetContLoginEventId()
        {
            return ContLoginCurrEventId;
        }

        public static string GetContLoginRewardData()
        {
            return ContLoginRewards;
        }

        public static string GetContLoginRewardDataByEvent(int eventid)
        {
            if (ContLoginAllRewards == null)
            {
                return "";
            }

            if (ContLoginAllRewards.Count == 0)
            {
                return "";
            }

            EventRewards rewardData = ContLoginAllRewards.Find(o => o.EventID() == eventid);
            if (rewardData == null)
            {
                return "";
            }

            return ContLoginAllRewards.Find(o => o.EventID() == eventid).RewardData();
        }

        public static string SerializeContLogin()
        {
            StringBuilder serializedData = new StringBuilder();
            serializedData.Append(isContLoginEventRunning ? 1 : 0);
            serializedData.Append("*");
            serializedData.Append(ContLoginCurrEventId.ToString());
            serializedData.Append("*");
            serializedData.Append(ContLoginEventStart.ToString());
            serializedData.Append("*");
            serializedData.Append(ContLoginRewards);

            return serializedData.ToString();
        }

        private static string GetContLoginRewardsData(List<Dictionary<string, object>> rewardsData)
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
                    int itemid4 = (int)entry["itemid4"];
                    int itemcount4 = (int)entry["itemcount4"];

                    if (itemid1 == -1 && itemid2 == -1 && itemid3 == -1 && itemid4 == -1)
                    {
                        continue;
                    }

                    rewardStr.Append(((int)entry["rewardid"]).ToString());
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
                    if (itemid4 > 0)
                    {
                        rewardStr.Append("|");
                        rewardStr.Append(itemid4.ToString());
                        rewardStr.Append("|");
                        rewardStr.Append(itemcount4.ToString());
                    }

                    rewardStr.Append(";");
                }
            }

            string rewards = rewardStr.ToString();
            rewards = rewards.TrimEnd(';');

            return rewards;
        }

        private static List<EventRewards> GetContLoginAllRewardsData(List<Dictionary<string, object>> rewardsData)
        {
            StringBuilder rewardStr = new StringBuilder();
            List<EventRewards> rewardsDataList = new List<EventRewards>();
            int prevEvent = 0;

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
                    int itemid4 = (int)entry["itemid4"];
                    int itemcount4 = (int)entry["itemcount4"];

                    if (itemid1 == -1 && itemid2 == -1 && itemid3 == -1 && itemid4 == -1)
                    {
                        continue;
                    }

                    rewardStr.Append(((int)entry["rewardid"]).ToString());
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
                    if (itemid4 > 0)
                    {
                        rewardStr.Append("|");
                        rewardStr.Append(itemid4.ToString());
                        rewardStr.Append("|");
                        rewardStr.Append(itemcount4.ToString());
                    }
                    int currEvent = (int)entry["eventid"];
                    int nextPos = i + 1;
                    if (nextPos < rewardsData.Count)
                    {
                        Dictionary<string, object> next = rewardsData[nextPos];

                        int nextEvent = (int)next["eventid"];
                        if (currEvent == nextEvent)
                        {
                            rewardStr.Append(";");
                        }
                        else
                        {
                            EventRewards eventRewardData = new EventRewards((int)entry["eventid"], rewardStr.ToString());
                            rewardsDataList.Add(eventRewardData);

                            rewardStr = new StringBuilder();
                        }
                    }

                    prevEvent = currEvent;
                }
            }
            EventRewards finalEventRewardData = new EventRewards(prevEvent, rewardStr.ToString());
            rewardsDataList.Add(finalEventRewardData);

            return rewardsDataList;
        }

        // General Helper functions
        private static List<Dictionary<string, object>> RetrieveDataByEventID(int currEventId, List<Dictionary<string, object>> allRewardData)
        {
            List<Dictionary<string, object>> rewardData = new List<Dictionary<string, object>>();
            for (int i = 0; i < allRewardData.Count; ++i)
            {
                Dictionary<string, object> entry = allRewardData[i];
                if (entry.ContainsKey("eventid") && currEventId == (int)entry["eventid"])
                {
                    rewardData.Add(entry);
                }
            }

            return rewardData;
        }

        private static WelfareEventRetval GetEventStatus(List<Dictionary<string, object>> eventData, int currEventId)
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

        private static WelfareEventRetval GetEventStatusFixedPeriod(List<Dictionary<string, object>> eventData, int period, int currEventId)
        {
            WelfareEventRetval retval = new WelfareEventRetval();
            DateTime startDate = (DateTime)eventData[0]["start"];
            DateTime endDate = startDate.AddDays(period);
            int lastElem = eventData.Count - 1;

            // Case 1: Events not yet started
            if (DateTime.Now < (DateTime)eventData[0]["start"])
            {
                retval.eventCode = WelfareEventCode.NotStarted;

                return retval;
            }
            // Case 2: All events over
            else if (DateTime.Now > endDate)
            {
                Dictionary<string, object> entry = eventData[lastElem];
                int eventid = (int)entry["eventid"];
                DateTime start = (DateTime)entry["start"];
                DateTime end = endDate;

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
                        DateTime end = start.AddDays(period);
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

        private static string GetTotalGoldRewardsData(List<Dictionary<string, object>> rewardsData)
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

                    rewardStr.Append(((int)entry["rewardid"]).ToString());
                    rewardStr.Append("|");
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

        private static List<EventRewards> GetTotalGoldAllRewardsData(List<Dictionary<string, object>> rewardsData)
        {
            StringBuilder rewardStr = new StringBuilder();
            List<EventRewards> rewardsDataList = new List<EventRewards>();
            int prevEvent = 0;

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

                    rewardStr.Append(((int)entry["rewardid"]).ToString());
                    rewardStr.Append("|");
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
                    int currEvent = (int)entry["eventid"];
                    int nextPos = i + 1;
                    if (nextPos < rewardsData.Count)
                    {
                        Dictionary<string, object> next = rewardsData[nextPos];

                        int nextEvent = (int)next["eventid"];
                        if (currEvent == nextEvent)
                        {
                            rewardStr.Append(";");
                        }
                        else
                        {
                            EventRewards eventRewardData = new EventRewards((int)entry["eventid"], rewardStr.ToString());
                            rewardsDataList.Add(eventRewardData);

                            rewardStr = new StringBuilder();
                        }
                    }

                    prevEvent = currEvent;
                }
            }
            EventRewards finalEventRewardData = new EventRewards(prevEvent, rewardStr.ToString());
            rewardsDataList.Add(finalEventRewardData);

            return rewardsDataList;
        }

        public static void SerialiseItemToString(out string id, out string count, List<Common.IInventoryItem> items)
        {
            StringBuilder sbid = new StringBuilder();
            StringBuilder sbc = new StringBuilder();
            foreach (var iter in items)
            {
                sbid.Append(iter.ItemID.ToString());
                sbid.Append("*");

                sbc.Append(iter.StackCount);
                sbc.Append("*");
            }
            id = sbid.ToString();
            count = sbc.ToString();
        }

        public static void LogWelfareSignInPrizeGet(string getType, GameClientPeer peer)
        {
            string message = string.Format("Type: {0}",
                getType);

            Zealot.Logging.Client.LogClasses.WelfareSignInPrizeGet welfareSignInPrizeGetLog = new Zealot.Logging.Client.LogClasses.WelfareSignInPrizeGet();
            welfareSignInPrizeGetLog.userId = peer.mUserId;
            welfareSignInPrizeGetLog.charId = peer.GetCharId();
            welfareSignInPrizeGetLog.message = message;
            welfareSignInPrizeGetLog.getType = getType;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareSignInPrizeGetLog);
        }

        public static void LogWelfareSignInPrizeItemGet(string getType, int vipLvl, int playerVIPLvl,
            int vipStackBonus, int actualStackBonus, int rewardItemid, int rewardItemCount, GameClientPeer peer)
        {
            string message = string.Format("Get Type: {0} | Repo VIP Level: {1} | Player VIP Level: {2} | VIP Stack Bonus: {3} | Actual Stack Bonus: {4} | Reward Item ID: {5} | Reward Item Count: {6}",
                getType,
                vipLvl,
                playerVIPLvl,
                vipStackBonus,
                actualStackBonus,
                rewardItemid,
                rewardItemCount);

            Zealot.Logging.Client.LogClasses.WelfareSignInPrizeItemGet welfareSignInPrizeGetLog = new Zealot.Logging.Client.LogClasses.WelfareSignInPrizeItemGet();
            welfareSignInPrizeGetLog.userId = peer.mUserId;
            welfareSignInPrizeGetLog.charId = peer.GetCharId();
            welfareSignInPrizeGetLog.message = message;
            welfareSignInPrizeGetLog.getType = getType;
            welfareSignInPrizeGetLog.vipLvl = vipLvl;
            welfareSignInPrizeGetLog.playerVIPLvl = playerVIPLvl;
            welfareSignInPrizeGetLog.vipStackBonus = vipStackBonus;
            welfareSignInPrizeGetLog.actualStackBonus = actualStackBonus;
            welfareSignInPrizeGetLog.rewardItemCount = rewardItemCount;
            welfareSignInPrizeGetLog.rewardItemID = rewardItemid;

            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareSignInPrizeGetLog);
        }

        public static void LogWelfareSignInPrizeReGet(int year, int month, int day, GameClientPeer peer)
        {
            string message = string.Format("Year: {0} | Month: {1} | Day: {2}",
                year,
                month,
                day);

            Zealot.Logging.Client.LogClasses.WelfareSignInPrizeReGet welfareSignInPrizeReGetLog = new Zealot.Logging.Client.LogClasses.WelfareSignInPrizeReGet();
            welfareSignInPrizeReGetLog.userId = peer.mUserId;
            welfareSignInPrizeReGetLog.charId = peer.GetCharId();
            welfareSignInPrizeReGetLog.message = message;
            welfareSignInPrizeReGetLog.year = year;
            welfareSignInPrizeReGetLog.month = month;
            welfareSignInPrizeReGetLog.day = day;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareSignInPrizeReGetLog);
        }

        public static void LogWelfareOnlinePrizeGet(int serial, double duration, GameClientPeer peer)
        {
            string message = string.Format("Serial: {0} | Duration: {1}",
                serial,
                duration);

            Zealot.Logging.Client.LogClasses.WelfareOnlinePrizeGet welfareOnlinePrizeGetLog = new Zealot.Logging.Client.LogClasses.WelfareOnlinePrizeGet();
            welfareOnlinePrizeGetLog.userId = peer.mUserId;
            welfareOnlinePrizeGetLog.charId = peer.GetCharId();
            welfareOnlinePrizeGetLog.message = message;
            welfareOnlinePrizeGetLog.serial = serial;
            welfareOnlinePrizeGetLog.duration = duration;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareOnlinePrizeGetLog);
        }

        public static void LogWelfareOnlinePrizeItemGet(int itemId, int itemAmount, GameClientPeer peer)
        {
            string message = string.Format("Item Id: {0} | Item Amount: {1}",
                itemId,
                itemAmount);

            Zealot.Logging.Client.LogClasses.WelfareOnlinePrizeItemGet welfareOnlinePrizeItemGetLog = new Zealot.Logging.Client.LogClasses.WelfareOnlinePrizeItemGet();
            welfareOnlinePrizeItemGetLog.userId = peer.mUserId;
            welfareOnlinePrizeItemGetLog.charId = peer.GetCharId();
            welfareOnlinePrizeItemGetLog.message = message;
            welfareOnlinePrizeItemGetLog.itemId = itemId;
            welfareOnlinePrizeItemGetLog.itemAmount = itemAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareOnlinePrizeItemGetLog);
        }

        public static void LogWelfareDailyGoldMCardBuy(string buyType, GameClientPeer peer)
        {
            string message = string.Format("Buy Type: {0}", buyType);

            Zealot.Logging.Client.LogClasses.WelfareDailyGoldMCardBuy welfareDailyGoldMCardBuyLog = new Zealot.Logging.Client.LogClasses.WelfareDailyGoldMCardBuy();
            welfareDailyGoldMCardBuyLog.userId = peer.mUserId;
            welfareDailyGoldMCardBuyLog.charId = peer.GetCharId();
            welfareDailyGoldMCardBuyLog.message = message;
            welfareDailyGoldMCardBuyLog.buyType = buyType;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareDailyGoldMCardBuyLog);
        }

        public static void LogWelfareDailyGoldPCardBuy(string buyType, GameClientPeer peer)
        {
            string message = string.Format("Buy Type: {0}", buyType);

            Zealot.Logging.Client.LogClasses.WelfareDailyGoldPCardBuy welfareDailyGoldPCardBuyLog = new Zealot.Logging.Client.LogClasses.WelfareDailyGoldPCardBuy();
            welfareDailyGoldPCardBuyLog.userId = peer.mUserId;
            welfareDailyGoldPCardBuyLog.charId = peer.GetCharId();
            welfareDailyGoldPCardBuyLog.message = message;
            welfareDailyGoldPCardBuyLog.buyType = buyType;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareDailyGoldPCardBuyLog);
        }

        public static void LogWelfareDailyGoldMCardLockGoldGet(string getType, GameClientPeer peer)
        {
            string message = string.Format("Get Type: {0}",
                getType);

            Zealot.Logging.Client.LogClasses.WelfareDailyGoldMCardLockGoldGet welfareDailyGoldMCardLockGoldGetLog = new Zealot.Logging.Client.LogClasses.WelfareDailyGoldMCardLockGoldGet();
            welfareDailyGoldMCardLockGoldGetLog.userId = peer.mUserId;
            welfareDailyGoldMCardLockGoldGetLog.charId = peer.GetCharId();
            welfareDailyGoldMCardLockGoldGetLog.message = message;
            welfareDailyGoldMCardLockGoldGetLog.getType = getType;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareDailyGoldMCardLockGoldGetLog);
        }

        public static void LogWelfareDailyGoldPCardLockGoldGet(string getType, int lockGoldAmount, int lockGoldBef, int lockGoldAft, GameClientPeer peer)
        {
            string message = string.Format("Get Type: {0}",
                getType);

            Zealot.Logging.Client.LogClasses.WelfareDailyGoldPCardLockGoldGet welfareDailyGoldPCardLockGoldGetLog = new Zealot.Logging.Client.LogClasses.WelfareDailyGoldPCardLockGoldGet();
            welfareDailyGoldPCardLockGoldGetLog.userId = peer.mUserId;
            welfareDailyGoldPCardLockGoldGetLog.charId = peer.GetCharId();
            welfareDailyGoldPCardLockGoldGetLog.message = message;
            welfareDailyGoldPCardLockGoldGetLog.getType = getType;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareDailyGoldPCardLockGoldGetLog);
        }

        // Removed From Commit for now
        public static void LogWelfareFirstTopUp(string type, int topUpAmount, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Top Up Amount: {1}",
                type,
                topUpAmount);

            Zealot.Logging.Client.LogClasses.WelfareFirstTopUp welfareFirstTopUpLog = new Zealot.Logging.Client.LogClasses.WelfareFirstTopUp();
            welfareFirstTopUpLog.userId = peer.mUserId;
            welfareFirstTopUpLog.charId = peer.GetCharId();
            welfareFirstTopUpLog.message = message;
            welfareFirstTopUpLog.type = type;
            welfareFirstTopUpLog.topUpAmount = topUpAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareFirstTopUpLog);
        }

        // Removed From Commit for now
        public static void LogWelfareFirstTopUpItemGet(string type, string itemId, string itemAmount, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Item Ids: {1} | Item Amounts: {2}",
                type,
                itemId,
                itemAmount);

            Zealot.Logging.Client.LogClasses.WelfareFirstTopUpItemGet welfareFirstTopUpItemGetLog = new Zealot.Logging.Client.LogClasses.WelfareFirstTopUpItemGet();
            welfareFirstTopUpItemGetLog.userId = peer.mUserId;
            welfareFirstTopUpItemGetLog.charId = peer.GetCharId();
            welfareFirstTopUpItemGetLog.message = message;
            welfareFirstTopUpItemGetLog.type = type;
            welfareFirstTopUpItemGetLog.itemId = itemId;
            welfareFirstTopUpItemGetLog.itemAmount = itemAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareFirstTopUpItemGetLog);
        }

        // Removed From Commit for now
        public static void LogWelfareTotalCredit(string type, int creditAmount, int creditBef, int creditAft, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Credit Amount: {1} | Credit Before: {2} | Credit After: {3}",
                type,
                creditAmount,
                creditBef,
                creditAft);

            Zealot.Logging.Client.LogClasses.WelfareTotalCredit welfareTotalCreditLog = new Zealot.Logging.Client.LogClasses.WelfareTotalCredit();
            welfareTotalCreditLog.userId = peer.mUserId;
            welfareTotalCreditLog.charId = peer.GetCharId();
            welfareTotalCreditLog.message = message;
            welfareTotalCreditLog.type = type;
            welfareTotalCreditLog.creditAmount = creditAmount;
            welfareTotalCreditLog.creditBef = creditBef;
            welfareTotalCreditLog.creditAft = creditAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareTotalCreditLog);
        }

        // Removed From Commit for now
        public static void LogWelfareTotalCreditItemGet(string type, int serial, string itemId, string itemAmount, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} |  Serial: {1} | Item Id: {2} | Item Amount: {3}",
                type,
                serial,
                itemId,
                itemAmount);

            Zealot.Logging.Client.LogClasses.WelfareTotalCreditItemGet welfareTotalCreditItemGetLog = new Zealot.Logging.Client.LogClasses.WelfareTotalCreditItemGet();
            welfareTotalCreditItemGetLog.userId = peer.mUserId;
            welfareTotalCreditItemGetLog.charId = peer.GetCharId();
            welfareTotalCreditItemGetLog.message = message;
            welfareTotalCreditItemGetLog.type = type;
            welfareTotalCreditItemGetLog.serial = serial;
            welfareTotalCreditItemGetLog.itemId = itemId;
            welfareTotalCreditItemGetLog.itemAmount = itemAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareTotalCreditItemGetLog);
        }

        // Removed From Commit for now
        public static void LogWelfareTotalCreditLockGoldGet(string type, int serial, int lockGold, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Serial: {1} | Lock Gold Id: {2}",
               type,
               serial,
               lockGold);

            Zealot.Logging.Client.LogClasses.WelfareTotalCreditLockGoldGet welfareTotalCreditLockGoldGetLog = new Logging.Client.LogClasses.WelfareTotalCreditLockGoldGet();
            welfareTotalCreditLockGoldGetLog.userId = peer.mUserId;
            welfareTotalCreditLockGoldGetLog.charId = peer.GetCharId();
            welfareTotalCreditLockGoldGetLog.message = message;
            welfareTotalCreditLockGoldGetLog.type = type;
            welfareTotalCreditLockGoldGetLog.serial = serial;
            welfareTotalCreditLockGoldGetLog.lockGold = lockGold;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareTotalCreditLockGoldGetLog);
        }

        // Removed From Commit for now
        // 開服基金購買
        public static void LogWelfareOpenServiceFundsBuy(string type, GameClientPeer peer)
        {
            string message = string.Format("Type: {0}",
                type);

            Zealot.Logging.Client.LogClasses.WelfareOpenServiceFundsBuy welfareOpenServiceFundsBuyLog = new Zealot.Logging.Client.LogClasses.WelfareOpenServiceFundsBuy();
            welfareOpenServiceFundsBuyLog.userId = peer.mUserId;
            welfareOpenServiceFundsBuyLog.charId = peer.GetCharId();
            welfareOpenServiceFundsBuyLog.message = message;
            welfareOpenServiceFundsBuyLog.type = type;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareOpenServiceFundsBuyLog);
        }

        // Removed From Commit for now
        // 開服基金領取
        public static void LogWelfareOpenServiceFundsLockGoldGet(string type, int playerLevel, int lockGoldAmount, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Player Level: {1} | Lock Gold Amount: {2}",
                type,
                playerLevel,
                lockGoldAmount);

            Zealot.Logging.Client.LogClasses.WelfareOpenServiceFundsLockGoldGet welfareOpenServiceFundsLockGoldGetLog = new Zealot.Logging.Client.LogClasses.WelfareOpenServiceFundsLockGoldGet();
            welfareOpenServiceFundsLockGoldGetLog.userId = peer.mUserId;
            welfareOpenServiceFundsLockGoldGetLog.charId = peer.GetCharId();
            welfareOpenServiceFundsLockGoldGetLog.message = message;
            welfareOpenServiceFundsLockGoldGetLog.type = type;
            welfareOpenServiceFundsLockGoldGetLog.playerLvl = playerLevel;
            welfareOpenServiceFundsLockGoldGetLog.lockGoldAmount = lockGoldAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareOpenServiceFundsLockGoldGetLog);
        }

        // Removed From Commit for now
        // 全民福利
        public static void LogWelfareOpenServiceFundsItemGet(string type, int joinMemberNum, int itemId, int itemAmount, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Version: {1} | Serial: {2} | Item Id: {3} | Item Amount: {4}",
                type,
                joinMemberNum,
                itemId,
                itemAmount);

            Zealot.Logging.Client.LogClasses.WelfareOpenServiceFundsItemGet welfareOpenServiceFundsItemGetLog = new Zealot.Logging.Client.LogClasses.WelfareOpenServiceFundsItemGet();
            welfareOpenServiceFundsItemGetLog.userId = peer.mUserId;
            welfareOpenServiceFundsItemGetLog.charId = peer.GetCharId();
            welfareOpenServiceFundsItemGetLog.message = message;
            welfareOpenServiceFundsItemGetLog.type = type;
            welfareOpenServiceFundsItemGetLog.joinMemberNum = joinMemberNum;
            welfareOpenServiceFundsItemGetLog.itemId = itemId;
            welfareOpenServiceFundsItemGetLog.itemAmount = itemAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareOpenServiceFundsItemGetLog);
        }

        public static void LogWelfareTotalSpend(string type, int spendAmount, int spendBef, int spendAft, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Spend Amount: {1} | Spend Before: {2} | Spend After: {3}",
                type,
                spendAmount,
                spendBef,
                spendAft);

            Zealot.Logging.Client.LogClasses.WelfareTotalSpend welfareTotalSpendLog = new Zealot.Logging.Client.LogClasses.WelfareTotalSpend();
            welfareTotalSpendLog.userId = peer.mUserId;
            welfareTotalSpendLog.charId = peer.GetCharId();
            welfareTotalSpendLog.message = message;
            welfareTotalSpendLog.type = type;
            welfareTotalSpendLog.spendAmount = spendAmount;
            welfareTotalSpendLog.spendBef = spendBef;
            welfareTotalSpendLog.spendAft = spendAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareTotalSpendLog);
        }

        // Removed From Commit for now
        public static void LogWelfareTotalSpendItemGet(string type, int serial, string itemId, string itemAmount, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Serial: {1} | Item Id: {2} | Item Amount: {3}",
                type,
                serial,
                itemId,
                itemAmount);

            Zealot.Logging.Client.LogClasses.WelfareTotalSpendItemGet welfareTotalSpendItemGetLog = new Zealot.Logging.Client.LogClasses.WelfareTotalSpendItemGet();
            welfareTotalSpendItemGetLog.userId = peer.mUserId;
            welfareTotalSpendItemGetLog.charId = peer.GetCharId();
            welfareTotalSpendItemGetLog.message = message;
            welfareTotalSpendItemGetLog.type = type;
            welfareTotalSpendItemGetLog.serial = serial;
            welfareTotalSpendItemGetLog.itemId = itemId;
            welfareTotalSpendItemGetLog.itemAmount = itemAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareTotalSpendItemGetLog);
        }

        //public static void WelfareTotalSpendGoldGet(string type, int version, int serial, int goldAmount, GameClientPeer peer)
        //{
        //    string message = string.Format("Type: {0} | Version: {1} | Serial: {2} | Gold Amount: {3}",
        //        type,
        //        version,
        //        serial,
        //        goldAmount);

        //    Zealot.Logging.Client.LogClasses.WelfareTotalSpendGoldGet welfareTotalSpendGoldGetLog = new Zealot.Logging.Client.LogClasses.WelfareTotalSpendGoldGet();
        //    welfareTotalSpendGoldGetLog.userId      = peer.mUserId;
        //    welfareTotalSpendGoldGetLog.charId      = peer.GetCharId();
        //    welfareTotalSpendGoldGetLog.message     = message;
        //    welfareTotalSpendGoldGetLog.type        = type;
        //    welfareTotalSpendGoldGetLog.version     = version;
        //    welfareTotalSpendGoldGetLog.serial      = serial;
        //    welfareTotalSpendGoldGetLog.goldAmount  = goldAmount;
        //    var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareTotalSpendGoldGetLog);
        //}

        //public static void LogWelfareGoldJackpotSpend(string type, int tier, int goldBef, int goldAft, int lockGoldBef, int lockGoldAft, GameClientPeer peer)
        //{
        //    string message = string.Format("Type: {0} | Tier: {1} | Gold Before: {2} | Gold After: {3} | Lock Gold Before: {4} | Lock Gold After: {5}",
        //        type,
        //        tier,
        //        goldBef,
        //        goldAft,
        //        lockGoldBef,
        //        lockGoldAft);

        //    Zealot.Logging.Client.LogClasses.WelfareGoldJackpotSpend welfareGoldJackpotSpendLog = new Zealot.Logging.Client.LogClasses.WelfareGoldJackpotSpend();
        //    welfareGoldJackpotSpendLog.userId       = peer.mUserId;
        //    welfareGoldJackpotSpendLog.charId       = peer.GetCharId();
        //    welfareGoldJackpotSpendLog.message      = message;
        //    welfareGoldJackpotSpendLog.tier         = tier;
        //    welfareGoldJackpotSpendLog.goldBef      = goldBef;
        //    welfareGoldJackpotSpendLog.goldAft      = goldAft;
        //    welfareGoldJackpotSpendLog.lockGoldBef  = lockGoldBef;
        //    welfareGoldJackpotSpendLog.lockGoldAft  = lockGoldAft;
        //    var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareGoldJackpotSpendLog);
        //}

        public static void LogWelfareGoldJackpotRewardGet(string type, int tier, int jackpotAmount, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Tier: {1} | Jackpot Amount: {2}",
                type,
                tier,
                jackpotAmount);

            Zealot.Logging.Client.LogClasses.WelfareGoldJackpotRewardGet welfareGoldJackpotRewardGetLog = new Zealot.Logging.Client.LogClasses.WelfareGoldJackpotRewardGet();
            welfareGoldJackpotRewardGetLog.userId = peer.mUserId;
            welfareGoldJackpotRewardGetLog.charId = peer.GetCharId();
            welfareGoldJackpotRewardGetLog.message = message;
            welfareGoldJackpotRewardGetLog.type = type;
            welfareGoldJackpotRewardGetLog.tier = tier;
            welfareGoldJackpotRewardGetLog.jackpotAmount = jackpotAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareGoldJackpotRewardGetLog);
        }

        // Removed From Commit for now
        public static void LogWelfareContinuousLogin(string type, int itemid, int itemAmount, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Item: {1} | Item Amount: {2}",
                type,
                itemid,
                itemAmount);

            Zealot.Logging.Client.LogClasses.WelfareContinuousLogin welfareContinuousLoginGetLog = new Logging.Client.LogClasses.WelfareContinuousLogin();
            welfareContinuousLoginGetLog.userId = peer.mUserId;
            welfareContinuousLoginGetLog.charId = peer.GetCharId();
            welfareContinuousLoginGetLog.message = message;
            welfareContinuousLoginGetLog.type = type;
            welfareContinuousLoginGetLog.itemId = itemid;
            welfareContinuousLoginGetLog.itemAmount = itemAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(welfareContinuousLoginGetLog);
        }

        public static void OnNewDay()
        {
            CheckTotalGoldEventRunning();
            CheckGoldJackpotEventRunning();
            CheckContLoginEventRunning();
        }
    }
}
