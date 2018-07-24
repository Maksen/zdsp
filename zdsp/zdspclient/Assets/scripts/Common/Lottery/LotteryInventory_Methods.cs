using System;
using System.Collections.Generic;
using Zealot.Common.Entities;
using Zealot.Repository;

namespace Zealot.Common
{
    public static partial class LotteryDefine
    {
        public static int MaxLotteryPoint = 9999999;
        public static ushort ExtraTicketItemId = 1229;
    }

    public partial class LotteryInventoryData
    {
        Dictionary<int, LotteryInfo> IdToData = null;
        // Server add inventory data to local object to client
        public void InitFromInventory(LotteryInfoStats lotteryStats)
        {
            ResetToNewData();

            for (int i = 0; i < lotteryDatas.Count; i++)
            {
                LotteryInfo info = lotteryDatas[i];
                var stat = new Zealot.Common.Entities.LotteryInfo();

                stat.id = info.lotteryId;
                stat.point = info.point;
                stat.freeTicket = info.freeTicket;
                stat.lastUpdateTime = info.lastUpdateDateTicks;
                if (stat.rewardPoints == null)
                    stat.rewardPoints = new List<int>();

                for (int j = 0; j < info.rewardPoints.Count; j++)
                {
                    stat.rewardPoints.Add(info.rewardPoints[j]);
                }
                // String object too old can not use join by int array, server's can
                //stat.rewardPoints = "";
                //for (int j = 0; j < info.rewardPoints.Count; j++)
                //{
                //    if (j > 0)
                //        stat.rewardPoints += ",";
                //    stat.rewardPoints += info.rewardPoints[j].ToString();
                //}
                lotteryStats.lotteryInfos[i] = JsonConvertDefaultSetting.SerializeObject(stat);
            }
        }

        // Client init inventory data by local object from server
        public void InitFromStats(LotteryInfoStats lotteryStats)
        {
            IdToData = new Dictionary<int, LotteryInfo>();

            for (int i = 0; i < lotteryStats.lotteryInfos.Count; i++)
            {
                var jsonStat = lotteryStats.lotteryInfos[i];
                if (jsonStat == null)
                    continue;

                Entities.LotteryInfo stat = JsonConvertDefaultSetting.DeserializeObject<Entities.LotteryInfo>((string)jsonStat);
                //Entities.LotteryInfo stat = (Entities.LotteryInfo)lotteryStats.lotteryInfos[i];
                if (stat != null)
                {
                    LotteryInfo info = new LotteryInfo();
                    info.lotteryId = stat.id;
                    info.point = stat.point;
                    info.freeTicket = stat.freeTicket;
                    info.LastUpdateDate = new System.DateTime(stat.lastUpdateTime);
                    //var arr = stat.rewardPoints.Split(',');
                    //info.rewardPoints = new List<int>(Array.ConvertAll<string, int>(arr, int.Parse));
                    for (int j = 0; j < stat.rewardPoints.Count; j++)
                    {
                        info.rewardPoints.Add(stat.rewardPoints[j]);
                    }

                    lotteryDatas.Add(info);
                    if (IdToData.ContainsKey(info.lotteryId) == false)
                        IdToData.Add(info.lotteryId, info);
                }
            }
        }

        public void UpdateFromStat(Entities.LotteryInfo stat)
        {
            LotteryInfo info;
            if (stat != null && IdToData.TryGetValue(stat.id, out info) == true)
            {
                info.lotteryId = stat.id;
                info.point = stat.point;
                info.freeTicket = stat.freeTicket;
                info.LastUpdateDate = new System.DateTime(stat.lastUpdateTime);
                //var arr = stat.rewardPoints.Split(',');
                //info.rewardPoints = new List<int>(Array.ConvertAll<string, int>(arr, int.Parse));
                for (int j = 0; j < stat.rewardPoints.Count; j++)
                {
                    if (j >= info.rewardPoints.Count)
                        info.rewardPoints.Add(stat.rewardPoints[j]);
                    else
                        info.rewardPoints[j] = stat.rewardPoints[j];
                }

            }
        }

        // Convert existing old data into new activity initial data
        // now is multi so just remove expired data
        // client call will change, but it's fake, must need LocalObject sync to reset
        public void ResetToNewData()
        {
            // if first time, need set old data
            if (IdToData == null)
            {
                IdToData = new Dictionary<int, LotteryInfo>();
                if (lotteryDatas.Count > 0)
                {
                    lotteryDatas.ForEach(item => IdToData.Add(item.lotteryId, item));
                }
            }


            var tempList = new List<LotteryInfo>();
            var tempMap = new Dictionary<int, LotteryInfo>();
            var dataList = LotteryMainRepo.GetPeriodLottery();
            if (dataList.Count != 0)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    var data = dataList[i];
                    LotteryInfo info;
                    if (IdToData.TryGetValue(data.main.id, out info) == true)
                    {
                        if (info.lastUpdateDateTicks < DateTime.Today.Ticks && info.freeTicket != data.main.freetime)
                        {
                            info.freeTicket = data.main.freetime;
                            info.LastUpdateDate = DateTime.Now;
                        }
                        else
                        {
                            info.LastUpdateDate = new DateTime(info.lastUpdateDateTicks);
                        }
                    }
                    else
                    {
                        info = new LotteryInfo();
                        info.lotteryId = data.main.id;
                        info.freeTicket = data.main.freetime;
                        info.point = 0;
                        info.lotteryCount = 0;
                        info.LastUpdateDate = DateTime.Now;
                        info.rewardPoints = new List<int>();
                        info.pointRewardCount = data.pointreward.Count;
                    }
                    tempList.Add(info);
                    tempMap.Add(info.lotteryId, info);
                }
            }
            IdToData = tempMap;
            lotteryDatas = tempList;
        }

        public bool UpdateInfoToStat(LotteryInfoStats lotteryStats, int index)
        {
            var stat = JsonConvertDefaultSetting.DeserializeObject<Entities.LotteryInfo>((string)lotteryStats.lotteryInfos[index]);
            LotteryInfo info;
            if (IdToData.TryGetValue(stat.id, out info) == false)
                return false;

            stat.point = info.point;
            stat.freeTicket = info.freeTicket;
            stat.lastUpdateTime = info.lastUpdateDateTicks;
            for (int j = 0; j < info.rewardPoints.Count; j++)
            {
                if (j >= stat.rewardPoints.Count)
                    stat.rewardPoints.Add(info.rewardPoints[j]);
                else
                    stat.rewardPoints[j] = info.rewardPoints[j];
            }
            //stat.rewardPoints = "";
            //for (int j = 0; j < info.rewardPoints.Count; j++)
            //{
            //    if (j > 0)
            //        stat.rewardPoints += ",";
            //    stat.rewardPoints += info.rewardPoints[j].ToString();
            //}
            lotteryStats.lotteryInfos[index] = JsonConvertDefaultSetting.SerializeObject(stat);
            return true;
        }

        public Dictionary<int, int> GetIdToIndex()
        {
            var map = new Dictionary<int, int>();
            if (lotteryDatas != null && lotteryDatas.Count > 0)
            {
                var idx = -1;
                lotteryDatas.ForEach(info => map.Add(info.lotteryId, ++idx));
            }
            return map;
        }

        public bool CheckHaveData(int lottery_id)
        {
            //if (IdToData.ContainsKey(lottery_id) == false)
            //{
            //    var data = LotteryMainRepo.GetLottery(lottery_id);
            //    if (data != null && data.InPeriod() == true) 
            //    {
            //        ResetToNewData();
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            //return true;
            return IdToData.ContainsKey(lottery_id);
        }

        // reset when 11:59pm cross to 0:00am
        public void NewDayReset()
        {
            var dataList = LotteryMainRepo.GetPeriodLottery();
            for (int i = 0; i < dataList.Count; ++i)
            {
                var data = dataList[i];
                LotteryInfo info;
                if (IdToData.TryGetValue(data.main.id, out info) == false)
                    continue;

                if (info.LastUpdateDate >= DateTime.Today)
                    continue;

                if (info.freeTicket != data.main.freetime)
                {
                    info.freeTicket = data.main.freetime;
                    info.LastUpdateDate = DateTime.Now;
                }
            }
        }

        public DateTime GetLastUpdateDateTime(int lottery_id)
        {
            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
                return info.LastUpdateDate;

            return DateTime.Now;
        }

        public long GetLastUpdateDateTimeTicks(int lottery_id)
        {
            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
                return info.lastUpdateDateTicks;

            return 0;
        }

        public int AddFreeTicket(int lottery_id, int count)
        {
            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
            {
                info.freeTicket += count;
                info.LastUpdateDate = DateTime.Now;
                return info.freeTicket;
            }

            return 0;
        }

        public void SetFreeTicket(int lottery_id, int count)
        {
            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
            {
                info.freeTicket = count;
            }
        }

        public int GetFreeTicketCount(int lottery_id)
        {
            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
            {
                return info.freeTicket;
            }

            return 0;
        }
        public int DeductFreeTicket(int lottery_id, int used_free_tickets)
        {
            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
            {
                info.freeTicket = System.Math.Max(0, info.freeTicket - used_free_tickets);
                info.LastUpdateDate = DateTime.Now;
                return info.freeTicket;
            }

            return 0;

        }

        public void SetPoint(int lottery_id, int point)
        {
            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
                info.point = point;
        }

        public int GetPoint(int lottery_id)
        {
            return IdToData[lottery_id].point;
        }

        public int AddPoint(int lottery_id, int point)
        {
            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
            {
                info.point = System.Math.Min(LotteryDefine.MaxLotteryPoint, info.point + point);
                info.LastUpdateDate = DateTime.Now;
                return info.point;
            }

            return 0;
        }

        public void SetLastUpdateTime(int lottery_id, DateTime time)
        {
            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
            {
                info.LastUpdateDate = time;
            }
        }

        public int GetLotteryCount(int lottery_id)
        {
            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
            {
                return info.lotteryCount;
            }
            return 0;
        }

        public bool AddCountAndPoint(int lottery_id, int count, int point)
        {
            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
            {
                info.lotteryCount += count;
                info.point = System.Math.Min(LotteryDefine.MaxLotteryPoint, info.point + point);
                info.LastUpdateDate = DateTime.Now;
                return true;
            }

            return false;
        }

        public List<int> GetRewardedPoints(int lottery_id)
        {
            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
            {
                return info.rewardPoints;
            }

            return new List<int>();
        }

        public int AddRewarderPoint(int lottery_id, int add_point)
        {
            var idx = -1;
            if (add_point <= 0)
                return idx;

            LotteryInfo info;
            if (IdToData.TryGetValue(lottery_id, out info))
            {
                var points = info.rewardPoints;
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i] == add_point)
                        return idx;

                    if (idx == -1 && points[i] == 0)
                        idx = i;
                }

                if (idx > -1)
                {
                    info.rewardPoints[idx] = add_point;
                }
                else
                {
                    idx = info.rewardPoints.Count;
                    info.rewardPoints.Add(add_point);
                }
                info.LastUpdateDate = DateTime.Now;
            }

            return idx;
        }
    }
}
