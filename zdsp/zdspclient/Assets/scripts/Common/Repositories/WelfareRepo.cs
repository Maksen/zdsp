using System.Collections.Generic;
using Kopio.JsonContracts;
using System;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
    public class OnlineReward
    {
        public int mItemId;
        public int mCount;

        public OnlineReward(int itemid, int count)
        {
            mItemId = itemid;
            mCount = count;
        }
    }
    public class VIPData
    {
        public int mVIPLevel;
        public int mMultiply;

        public VIPData(int level, int multiply)
        {
            mVIPLevel = level;
            mMultiply = multiply;
        }
    }

    public class TierData
    {
        public int mId;
        public int mLowerbound;
        public int mUpperbound;
        public int mWeight;

        public TierData(int id, int lowerbound, int upperbound, int weight)
        {
            mId = id;
            mLowerbound = lowerbound;
            mUpperbound = upperbound;
            mWeight = weight;
        }
    }

    public class TierDataList
    {
        private List<TierData> _dataList;

        public TierDataList(string tierRawData)
        {
            _dataList = new List<TierData>();

            Init(tierRawData);
        }

        private void Init(string tierRawData)
        {
            List<string> tierDataList = tierRawData.Split(';').ToList();

            for (int i = 0; i < tierDataList.Count; ++i)
            {
                List<string> tierData = tierDataList[i].Split('|').ToList();
                int lBound = int.Parse(tierData[0]);
                int uBound = int.Parse(tierData[1]);
                int weight = int.Parse(tierData[2]);

                TierData newTierData = new TierData(i, lBound, uBound, weight);
                _dataList.Add(newTierData);
            }
        }

        public TierData RollForGold()
        {
            int totalWeight         = GetFullWeight();
            List<int> probabilities = GetBuckets();

            Random rand = GameUtils.GetRandomGenerator();
            int pick = rand.Next(0, totalWeight);

            return GetGold(probabilities, pick);
        }

        public int Count()
        {
            return _dataList.Count;
        }

        public List<TierData> List()
        {
            return _dataList;
        }

        public int GetLootPositionByItemId(int id)
        {
            return _dataList.FindIndex(o => o.mId == id);
        }

        private TierData GetGold(List<int> prob, int pick)
        {
            int pos = -1;

            if(prob.Count == 0 || pick < 0 || pick > prob[prob.Count - 1])
            {
                return null;
            }

            if ((pick >= 0 && pick <= prob[0]))
            {
                pos = 0;
            }
            else
            {
                for(int i = 1; i < prob.Count; ++i)
                {
                    if(pick > prob[i - 1] && pick <= prob[i])
                    {
                        pos = i;
                    }
                }
            }

            return _dataList[pos];
        }

        private int GetFullWeight()
        {
            int sum = 0;

            for(int i = 0; i < _dataList.Count; ++i)
            {
                TierData data = _dataList[i];
                sum += data.mWeight;
            }

            return sum;
        }

        private List<int> GetBuckets()
        {
            List<int> buckets = new List<int>();
            int curr = 0;

            for(int i = 0; i < _dataList.Count; ++i)
            {
                TierData data = _dataList[i];
                curr += data.mWeight;
                buckets.Add(curr);
            }

            return buckets;
        }
    }

    public class WelfareRepo
    {
        // Welfare Continuous Login
        public static Dictionary<int, Dictionary<int, Dictionary<int, SignInPrizeJson>>> _yearMonthDayJsonMap;

        // Welfare Online Reward
        public static Dictionary<int, OnlinePrizeJson> _orderJsonMap; // Online Order -> Json

        static WelfareRepo()
        {
            // Welfare Continuous Login
            _yearMonthDayJsonMap = new Dictionary<int, Dictionary<int, Dictionary<int, SignInPrizeJson>>>();

            // Welfare Online Reward
            _orderJsonMap = new Dictionary<int, OnlinePrizeJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            InitSignInPrize(gameData.SignInPrize);
            InitOnlinePrize(gameData.OnlinePrize);
        }

        private static void InitSignInPrize(Dictionary<int, SignInPrizeJson> signInPrizeData)
        {
            foreach (KeyValuePair<int, SignInPrizeJson> entry in signInPrizeData)
            {
                int year = entry.Value.year;
                int month = entry.Value.month;
                int day = entry.Value.day;
                if(!_yearMonthDayJsonMap.ContainsKey(year))
                {
                    Dictionary<int, Dictionary<int, SignInPrizeJson>> monthDayJsonMap = new Dictionary<int, Dictionary<int, SignInPrizeJson>>();
                    _yearMonthDayJsonMap.Add(year, monthDayJsonMap);
                }

                if(!_yearMonthDayJsonMap[year].ContainsKey(month))
                {
                    Dictionary<int, SignInPrizeJson> dayJsonMap = new Dictionary<int, SignInPrizeJson>();
                    _yearMonthDayJsonMap[year].Add(month, dayJsonMap);
                }

                if(!_yearMonthDayJsonMap[year][month].ContainsKey(day))
                {
                    _yearMonthDayJsonMap[year][month].Add(day, entry.Value);
                }
            }
        }

        private static void InitOnlinePrize(Dictionary<int, OnlinePrizeJson> onlinePrizeData)
        {
            foreach (KeyValuePair<int, OnlinePrizeJson> entry in onlinePrizeData)
            {
                int order = entry.Value.serial;
                if(!_orderJsonMap.ContainsKey(order))
                {
                    _orderJsonMap.Add(order, entry.Value);
                }
            }
        }

        public static IInventoryItem GetSignInPrizeByDate(int year, int month, int day)
        {
            if(_yearMonthDayJsonMap.ContainsKey(year))
            {
                if(_yearMonthDayJsonMap[year].ContainsKey(month))
                {
                    if(_yearMonthDayJsonMap[year][month].ContainsKey(day))
                    {
                        IInventoryItem rewardItem = GameRepo.ItemFactory.GetInventoryItem(_yearMonthDayJsonMap[year][month][day].itemid);
                        rewardItem.StackCount = (ushort)_yearMonthDayJsonMap[year][month][day].amount;

                        return rewardItem;
                    }
                }
            }

            return null;
        }

        public static VIPData GetVIPLevelMultplyByDate(int year, int month, int day)
        {
            if (_yearMonthDayJsonMap.ContainsKey(year))
            {
                if (_yearMonthDayJsonMap[year].ContainsKey(month))
                {
                    if (_yearMonthDayJsonMap[year][month].ContainsKey(day))
                    {
                        int vipLevel = _yearMonthDayJsonMap[year][month][day].viplevel;
                        int vipTimes = _yearMonthDayJsonMap[year][month][day].viptimes;
                        VIPData vip = new VIPData(vipLevel, vipTimes);

                        return vip;
                    }
                }
            }

            return null;
        }

        public static Dictionary<int, SignInPrizeJson> GetSignInDataListByYearMonth(int year, int month)
        {
            if(_yearMonthDayJsonMap.ContainsKey(year))
            {
                if(_yearMonthDayJsonMap[year].ContainsKey(month))
                {
                    return _yearMonthDayJsonMap[year][month];
                }
            }

            return null;
        }

        public static int GetSignInPrizeJsonCount()
        {
            return _yearMonthDayJsonMap.Count;
        }

        public static IInventoryItem GetOnlinePrizeByOrder(int order)
        {
            OnlinePrizeJson reward = null;
            if(_orderJsonMap.TryGetValue(order, out reward))
            {
                IInventoryItem rewardItem = GameRepo.ItemFactory.GetInventoryItem(reward.itemid);
                rewardItem.StackCount = (ushort)reward.amount;

                return rewardItem;
            }

            return null;
        }

        public static int GetRewardTimeByOrder(int order)
        {
            OnlinePrizeJson reward = null;
            if (_orderJsonMap.TryGetValue(order, out reward))
            {
                return reward.time;
            }

            return -1;
        }
    }
}
