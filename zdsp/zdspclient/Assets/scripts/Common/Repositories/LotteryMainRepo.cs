using System;
using System.Collections.Generic;
using Kopio.JsonContracts;
using System.Linq;
using System.Globalization;
//using ExitGames.Logging;

namespace Zealot.Repository
{
    public class LotteryMainData
    {
        public LotteryMainJson main;
        public LotteryItemJson item;
        public List<LotteryPointRewardJson> pointreward;
        bool dateformat = false;
        public DateTime start;
        public DateTime end;
        private Dictionary<int, bool> AwardItemMap;

        public LotteryMainData(LotteryMainJson lm, LotteryItemJson li, List<LotteryPointRewardJson> pr)
        {
            main = lm;
            item = li;
            pointreward = pr;

            AwardItemMap = new Dictionary<int, bool>();
            SetAwardItemMap(item.itemid1, item.award1);
            SetAwardItemMap(item.itemid2, item.award2);
            SetAwardItemMap(item.itemid3, item.award3);
            SetAwardItemMap(item.itemid4, item.award4);
            SetAwardItemMap(item.itemid5, item.award5);
            SetAwardItemMap(item.itemid6, item.award6);
            SetAwardItemMap(item.itemid7, item.award7);
            SetAwardItemMap(item.itemid8, item.award8);
            SetAwardItemMap(item.itemid9, item.award9);
            SetAwardItemMap(item.itemid10, item.award10);
        }

        private void SetAwardItemMap(int item_id, bool is_award)
        {
            if (is_award && AwardItemMap.ContainsKey(item_id) == false)
                AwardItemMap.Add(item_id, true);
        }

        public bool CheckDateFormat()
        {
            DateTime st;
            if (DateTime.TryParseExact(main.starttime, "yyyyMMdd_HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out st) == false)
                return false;
            DateTime ed;
            if (DateTime.TryParseExact(main.endtime, "yyyyMMdd_HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out ed) == false)
                return false;

            dateformat = true;
            start = st;
            end = ed;
            return true;
        }

        public bool InPeriod()
        {
            if (dateformat == true)
            {
                DateTime nowtime = DateTime.Now;
                int result1 = DateTime.Compare(nowtime, start);
                int result2 = DateTime.Compare(nowtime, end);
                if (result1 >= 0 && result2 < 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsFuture()
        {
            if (dateformat == true)
            {
                DateTime nowtime = DateTime.Now;
                int result1 = DateTime.Compare(nowtime, start);
                int result2 = DateTime.Compare(nowtime, end);
                if (result1 > 0 && result2 > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CheckHasAwardItem(int item_id)
        {
            return AwardItemMap.ContainsKey(item_id);
        }
    }

    public static class LotteryMainRepo
    {
        //private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public static Dictionary<int, LotteryMainJson> mLotteryMainsRaw;
        public static Dictionary<int, LotteryItemJson> mLotteryItemsRaw;
        public static Dictionary<int, LotteryMainData> mLotteryMains;
        public static Dictionary<int, List<LotteryPointRewardJson>> mLotteryMainPointRewards;

        static LotteryMainRepo()
        {
            mLotteryMains = new Dictionary<int, LotteryMainData>();
            mLotteryMainPointRewards = new Dictionary<int, List<LotteryPointRewardJson>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            Dictionary<int, List<LotteryPointRewardJson>> mTemp = new Dictionary<int, List<LotteryPointRewardJson>>();
            foreach (LotteryMain__lotterypointidJson val in gameData.LotteryMain__lotterypointid)
            {
                LotteryPointRewardJson pointreward = LotteryPointRewardRepo.GetPointReward(val.lotterypointidid);
                if (pointreward == null)
                {
                    throw new System.Exception("Unknown LotteryPointReward[" + val.lotterypointidid + "]" + " linked to lotterymain [" + val.lotterymainid + "]");
                }

                if (mTemp.ContainsKey(val.lotterymainid))
                    mTemp[val.lotterymainid].Add(pointreward);
                else
                    mTemp.Add(val.lotterymainid, new List<LotteryPointRewardJson>() { pointreward });
            }

            //sort
            foreach (KeyValuePair<int, List<LotteryPointRewardJson>> entry in mTemp)
            {
                List<LotteryPointRewardJson> pr = entry.Value;
                pr = pr.OrderBy(x => x.point).ToList();
                mLotteryMainPointRewards.Add(entry.Key, pr);
            }

            mLotteryMainsRaw = gameData.LotteryMain;
            mLotteryItemsRaw = gameData.LotteryItem;
            foreach (KeyValuePair<int, LotteryMainJson> entry in mLotteryMainsRaw)
            {
                LotteryMainJson mainJson = entry.Value;
                LotteryItemJson itemJson;
                if (!mLotteryItemsRaw.TryGetValue(mainJson.lotteryid, out itemJson))
                {
                    throw new System.Exception("Invalid lotteryid [" + mainJson.lotteryid + "] linked to LotteryMain[" + mainJson.id + "]");
                }

                List<LotteryPointRewardJson> preward = new List<LotteryPointRewardJson>();
                if (mLotteryMainPointRewards.ContainsKey(mainJson.id))
                {
                    preward = mLotteryMainPointRewards[mainJson.id];
                }

                LotteryMainData mainData = new LotteryMainData(mainJson, itemJson, preward);
                mLotteryMains.Add(mainJson.id, mainData);
                if (mainData.CheckDateFormat() == false)
                {
                    //throw new System.Exception("Invalid LotteryMain Date, id[" + mainJson.lotteryid + "], starttime[" + mainJson.starttime + "], endtime[" + mainJson.endtime + "]");
                    //log.InfoFormat("LotteryMainData Error, id:{0}, starttime:{1}, endtime:{2}", mainJson.id, mainJson.starttime, mainJson.endtime);
                }
            }
        }

        public static LotteryMainData GetLottery(int id)
        {
            LotteryMainData lotteryData;
            mLotteryMains.TryGetValue(id, out lotteryData);
            return lotteryData;
        }

        public static List<LotteryMainData> GetPeriodLottery()
        {
            List<LotteryMainData> retList = new List<LotteryMainData>();
            foreach (KeyValuePair<int, LotteryMainData> entry in mLotteryMains)
            {
                LotteryMainData lotteryData = entry.Value;
                if (lotteryData.InPeriod() == true)
                {
                    retList.Add(lotteryData);
                }
            }

            return retList;
        }

        public static List<LotteryMainData> GetFutureLottery()
        {
            List<LotteryMainData> retList = new List<LotteryMainData>();
            foreach (KeyValuePair<int, LotteryMainData> entry in mLotteryMains)
            {
                LotteryMainData lotteryData = entry.Value;
                if (lotteryData.IsFuture())
                {
                    retList.Add(lotteryData);
                }
            }

            return retList;
        }
    }

    public static class LotteryPointRewardRepo
    {
        public static Dictionary<int, LotteryPointRewardJson> mIdMap;

        static LotteryPointRewardRepo()
        {
            mIdMap = new Dictionary<int, LotteryPointRewardJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mIdMap = gameData.LotteryPointReward;
        }

        public static LotteryPointRewardJson GetPointReward(int id)
        {
            LotteryPointRewardJson pointreward;
            mIdMap.TryGetValue(id, out pointreward);
            return pointreward;
        }
    }
}