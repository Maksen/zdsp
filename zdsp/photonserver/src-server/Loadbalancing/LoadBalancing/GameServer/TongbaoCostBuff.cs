using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zealot.Common.Entities;

namespace Photon.LoadBalancing.GameServer
{
    public static class TongbaoCostBuff
    {
        private static readonly PoolFiber executionFiber;
        public static readonly int UpdateInterval = 5 * 60000;
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public static TongbaoCostBuffInfo CostBuffData = new TongbaoCostBuffInfo();
        public static Dictionary<int, TongbaoCostBuffInfo> StartUpList = new Dictionary<int, TongbaoCostBuffInfo>();

        static TongbaoCostBuff()
        {
            executionFiber = GameApplication.Instance.executionFiber;
        }

        public static void Init(string from = "game")
        {
            var task = InitTask(from);
        }

        public static async Task InitTask(string from)
        {
            List<Dictionary<string, object>> listDB = await GameApplication.dbGM.TongbaoCostBuff.GetStartUpList();
            executionFiber.Enqueue(() =>
            {
                StartUpList.Clear();
                foreach (var info in listDB)
                {
                    int id = (int)info["id"];
                    string name = (string)info["name"];
                    DateTime starttime = (DateTime)info["starttime"];
                    DateTime endtime = (DateTime)info["endtime"];
                    int costamount = (int)info["costamount"];
                    int skillid = (int)info["skillid"];
                    bool isuse = (bool)info["isuse"];
                    TongbaoCostBuffInfo temp = new TongbaoCostBuffInfo();
                    temp.Set(id, name, starttime.ToString("yyyy/MM/dd HH:mm"), endtime.ToString("yyyy/MM/dd HH:mm"), costamount, skillid, isuse);
                    StartUpList.Add(id, temp);
                }

                bool is_change = false;
                bool is_init = true;
                foreach (KeyValuePair<int, TongbaoCostBuffInfo> item in StartUpList)
                {
                    TongbaoCostBuffInfo tmp = item.Value;
                    if (tmp.CheckInTime() == true)
                    {
                        is_init = false;
                        is_change = CostBuffData.Set(tmp.id, tmp.name, tmp.starttime, tmp.endtime, tmp.costamount, tmp.skillid, tmp.isuse);
                        break;
                    }
                }

                if (is_init == true)
                {
                    is_change = CostBuffData.Set(0, "", "", "", 0, 0, false);
                }

                if (is_change == true && from != "game")
                {
                    GameApplication.Instance.TongbaoCostBuffSendToAllCharPeers();
                }
                log.Info("CostBuffData.id: " + CostBuffData.id + ", StartUpList.Count: " + StartUpList.Count + ", time: " + DateTime.Now
                    + ", is_change: " + is_change + ", from: " + from);
            });
        }

        public static void Update()
        {
            Init("update");
        }

        public static bool IsReach(int sec_id, int sec_cost)
        {
            if (CostBuffData.CheckInTime() == true && sec_id == CostBuffData.id && sec_cost >= CostBuffData.costamount)
            {
                return true;
            }
            return false;
        }
    }
}