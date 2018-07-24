using ExitGames.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Server.Entities;
using Zealot.Repository;
using Newtonsoft.Json;
using ExitGames.Concurrency.Fibers;
using Zealot.Logging.Client.LogClasses;

namespace Photon.LoadBalancing.GameServer.Lottery
{
    public enum LotteryActiveStatus : byte
    {
        WaitStart,
        InActive,
        Expire,
        ForcePass
    }

    public struct LotteryRewardItemInfo
    {
        public int index;
        public int itemId;
        public int count;
        public int weight;
    }

    public struct LotteryPointRewardItemInfo
    {
        public int point;
        public Dictionary<int, int> items;
    }

    public class LotteryInfo
    {
        public int id = 0;
        public DateTime startTime;
        public DateTime endTime;
        public List<LotteryRewardItemInfo> rewardItemInfos;
        public Dictionary<int, LotteryPointRewardItemInfo> pointRewardInfo;
        public int dailyFreeTickets = 0;
        public int getPoint = 0;
        public CurrencyType currencyType = CurrencyType.None;
        public int costGold = 0;
        public string focusPointDescription = "";
        public int pointItemId = 0;
        public int pointItemGetPoint = 0;
        public int ticketId = 0;

        // for get reward item
        public int totalWeight = 0;
        public int[] sumWeight;

        public LotteryInfo(LotteryMainData main_data)
        {
            id = main_data.main.id;
            startTime = main_data.start;
            endTime = main_data.end;
            rewardItemInfos = new List<LotteryRewardItemInfo>();
            sumWeight = new int[10];
            AddItemData(main_data.item.itemid1, main_data.item.count1, main_data.item.weight1);
            AddItemData(main_data.item.itemid2, main_data.item.count2, main_data.item.weight2);
            AddItemData(main_data.item.itemid3, main_data.item.count3, main_data.item.weight3);
            AddItemData(main_data.item.itemid4, main_data.item.count4, main_data.item.weight4);
            AddItemData(main_data.item.itemid5, main_data.item.count5, main_data.item.weight5);
            AddItemData(main_data.item.itemid6, main_data.item.count6, main_data.item.weight6);
            AddItemData(main_data.item.itemid7, main_data.item.count7, main_data.item.weight7);
            AddItemData(main_data.item.itemid8, main_data.item.count8, main_data.item.weight8);
            AddItemData(main_data.item.itemid9, main_data.item.count9, main_data.item.weight9);
            AddItemData(main_data.item.itemid10, main_data.item.count10, main_data.item.weight10);

            pointRewardInfo = new Dictionary<int, LotteryPointRewardItemInfo>();
            var pointRewardList = main_data.pointreward;
            foreach (var info in pointRewardList) {
                AddPointRewardData(info.point, info.itemid1, info.itemid2, info.itemid3, info.count1, info.count2, info.count3);
            }
            getPoint = main_data.main.point;
            costGold = main_data.main.gold;
            currencyType = main_data.main.currencytype;
            focusPointDescription = main_data.main.focus;
            dailyFreeTickets = main_data.main.freetime;
            pointItemId = main_data.main.itemid;
            pointItemGetPoint = main_data.main.itemidpoint;
            ticketId = main_data.main.ticketid;
        }

        void AddItemData(int id, int count, int weight)
        {
            var idx = rewardItemInfos.Count;
            var it = new LotteryRewardItemInfo();
            it.itemId = id;
            it.count = count;
            it.weight = weight;
            rewardItemInfos.Add(it);
            totalWeight += weight;
            sumWeight[idx] = totalWeight;
        }

        void AddPointRewardData(int point, int id_1, int id_2, int id_3, int count_1, int count_2, int count_3)
        {
            if (pointRewardInfo.ContainsKey(point) == true)
                return;

            var prt = new LotteryPointRewardItemInfo();
            prt.point = point;
            prt.items = new Dictionary<int, int>();
            if (id_1 > 0)
                prt.items.Add(id_1, count_1);
            if (id_2 > 0)
                prt.items.Add(id_2, count_2);
            if (id_3 > 0)
                prt.items.Add(id_3, count_3);
            pointRewardInfo.Add(point, prt);
        }
    }

    class LotteryActivityController
    {
        LotteryInfo info;
        public LotteryInfo Info { get { return info; } }

        public int ID { get { return info.id; } }
        public int CostGold { get { return info.costGold; } }
        public CurrencyType CurrencyType { get { return info.currencyType; } }
        public string Focus { get { return info.focusPointDescription; } }
        public DateTime StartTime { get { return info.startTime; } }
        public DateTime EndTime { get { return info.endTime; } }

        string lotteryItems;
        public string LotteryItems { get { return lotteryItems; } }

        string pointRewards;
        public string PointRewards { get { return pointRewards; } }


        public LotteryActivityController(LotteryInfo info)
        {
            this.info = info;

            PackageLotteryItemsData lotteryitemsdata = new PackageLotteryItemsData();
            lotteryitemsdata.ids = new int[info.rewardItemInfos.Count];
            lotteryitemsdata.counts = new int[info.rewardItemInfos.Count];
            lotteryItems = "";
            for (int i = 0; i < info.rewardItemInfos.Count; i++)
            {
                lotteryitemsdata.ids[i] = info.rewardItemInfos[i].itemId;
                lotteryitemsdata.counts[i] = info.rewardItemInfos[i].count;
            }
            lotteryItems = JsonConvert.SerializeObject(lotteryitemsdata);

            PackagePointRewardsData[] pointrewardsdatas = new PackagePointRewardsData[info.rewardItemInfos.Count];
            int idx = 0;
            foreach(KeyValuePair<int, LotteryPointRewardItemInfo> kvp in info.pointRewardInfo)
            {
                PackagePointRewardsData prd = new PackagePointRewardsData();
                prd.point = kvp.Key;
                var items = kvp.Value.items;
                prd.ids = items.Keys.ToArray();
                prd.counts = items.Values.ToArray();
                pointrewardsdatas[idx] = prd;
                ++idx;
            }
            pointRewards = JsonConvert.SerializeObject(pointrewardsdatas);
        }

        public bool UpdatePlayerInventoryData(Player player)
        {
            return player.UpdateLotteryStats();
        }

        public bool CheckCanGetItems(Player player, int base_count = 1, int bonus_count = 0)
        {

            if (player.Slot.mInventory.GetEmptySlotCount() < (base_count + bonus_count))
            {
                if (base_count == 1)
                    player.Slot.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(GUILocalizationRepo.GetSysMsgIdByName("Lottery_Backpackspacelessthan1grid"), player.Slot);
                else
                    player.Slot.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(GUILocalizationRepo.GetSysMsgIdByName("Lottery_Backpackspacelessthan11grid"), player.Slot);
                return false;
            }

            return true;
        }

        public bool DeductCost(Player player, int cost_type, int base_count, out int free, out int extra, ref LotteryUseFreeAndGetPoint log_Object)
        {
            free = -1;
            extra = -1;
            if (cost_type == (byte)LotteryCostType.Free)
            {
                var ticketCount = player.GetLotteryFreeTicket(ID);
                if (ticketCount < base_count)
                {
                    if (ticketCount == 0)
                        player.Slot.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(GUILocalizationRepo.GetSysMsgIdByName("Lottery_Nofreenumberoftimes"), player.Slot);
                    else
                        player.Slot.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(GUILocalizationRepo.GetSysMsgIdByName("Lottery_Thereisnotenoughfree"), player.Slot);
                    return false;
                }

                free = player.DeductLotteryFreeTicket(ID, base_count);

                log_Object.useFreeCount = base_count;
                log_Object.lastFreeCount = free;
                // Log
                //Zealot.Logging.Client.LogClasses.LotteryUseFreeTicket useFreeLog = new Zealot.Logging.Client.LogClasses.LotteryUseFreeTicket();
                //useFreeLog.userId = player.Slot.mUserId;
                //useFreeLog.charId = player.Slot.GetCharId();
                //useFreeLog.lotteryId = ID;
                //useFreeLog.useFreeCount = base_count;
                //useFreeLog.oldFreeCount = ticketCount;
                //useFreeLog.newFreeCount = free;
                //useFreeLog.message = string.Format("Lottery use free ticket, use count: {0}, old count: {1}, new count: {2}", base_count, ticketCount, free);
                //var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(useFreeLog);
            }
            else if (cost_type == (byte)LotteryCostType.Extra)
            {
                int ticketCount = player.Slot.mInventory.GetItemStackCountByItemId((ushort)info.ticketId);
                if (ticketCount < base_count)
                {
                    if (ticketCount == 0)
                        player.Slot.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(GUILocalizationRepo.GetSysMsgIdByName("Lottery_Noextranumberoftimes"), player.Slot);
                    else
                        player.Slot.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(GUILocalizationRepo.GetSysMsgIdByName("Lottery_Insufficientnumberoftimes"), player.Slot);
                    return false;
                }

                InvRetval retval = player.Slot.mInventory.DeductItem((ushort)info.ticketId, (ushort)base_count, "Lottery");
                if (retval.retCode == InvReturnCode.UseSuccess)
                {
                    extra = Math.Max(0, ticketCount - base_count);

                    // Log
                    //Zealot.Logging.Client.LogClasses.LotteryUseExtraTicket useExtraLog = new Zealot.Logging.Client.LogClasses.LotteryUseExtraTicket();
                    //useExtraLog.userId = player.Slot.mUserId;
                    //useExtraLog.charId = player.Slot.GetCharId();
                    //useExtraLog.lotteryId = ID;
                    //useExtraLog.itemId = (ushort)info.ticketId;
                    //useExtraLog.useExtraCount = base_count;
                    //useExtraLog.newExtraCount = extra;
                    //useExtraLog.message = string.Format("Lottery use extra item, use count: {0}, old count: {1}, new count: {2}", base_count, ticketCount, extra);
                    //var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(useExtraLog);
                }
                else
                {
                    player.Slot.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(GUILocalizationRepo.GetSysMsgIdByName("Lottery_Insufficientnumberoftimes"), player.Slot);
                    return false;
                }
            }
            else if (cost_type == (byte)LotteryCostType.Gold)
            {
                var currencyType = CurrencyType;
                var totoCost = CostGold * base_count;
                int oldGold = player.SecondaryStats.gold;
                int oldBindGold = player.SecondaryStats.bindgold;
                if (/*player.IsCurrencySufficient(currencyType, totoCost) == false ||*/
                    player.DeductCurrency(currencyType, totoCost, true, "Lottery") == false)
                {
                    // Call client show add gold ui
                    player.Slot.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(-2, player.Slot);
                    return false;
                }

                // Log
                //Zealot.Logging.Client.LogClasses.LotteryUseGold useGoldLog = new Zealot.Logging.Client.LogClasses.LotteryUseGold();
                //useGoldLog.userId = player.Slot.mUserId;
                //useGoldLog.charId = player.Slot.GetCharId();
                //useGoldLog.lotteryId = ID;
                //useGoldLog.oldGold = oldGold;
                //useGoldLog.newGold = player.SecondaryStats.gold;
                //useGoldLog.useGold = oldGold - player.SecondaryStats.gold;
                //useGoldLog.oldBindGold = oldBindGold;
                //useGoldLog.newBindGold = player.SecondaryStats.bindgold;
                //useGoldLog.useBindGold = oldBindGold - player.SecondaryStats.bindgold;
                //useGoldLog.message = string.Format("Lottery use gold, use gold: {0}, old gold: {1}, new gold: {2}, use bindgold: {3}, old bindgold: {4}, new bindgold: {5}", useGoldLog.useGold, useGoldLog.oldGold, useGoldLog.newGold, useGoldLog.useBindGold, useGoldLog.oldBindGold, useGoldLog.newBindGold);
                //var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(useGoldLog);
            }

            return true;
        }

        public bool GetItems(Player player, int count, out string str_ids, out string str_counts)
        {
            str_ids = "";
            str_counts = "";
            int[] picks = new int[count];
            for (int i = 0; i < count; i++)
            {
                Random rand = GameUtils.GetRandomGenerator();
                picks[i] = rand.Next(0, info.totalWeight);
            }

            for (int i = 0; i < picks.Length; i++)
            {
                for (int j = 0; j < info.sumWeight.Length; j++)
                {
                    if (picks[i] <= info.sumWeight[j])
                    {
                        picks[i] = j;
                        break;
                    }
                }
            }

            List<ItemInfo> additems = new List<ItemInfo>();
            foreach (int idx in picks)
            {
                var iteminfo = info.rewardItemInfos[idx];
                if (iteminfo.itemId > 0 && iteminfo.count > 0)
                {
                    int itemId = iteminfo.itemId, itemCount = iteminfo.count;
                    // Check PrizeGuarantee
                    Zealot.Server.Rules.PrizeGuaranteeRules.GetPrizeGuarantee(player.Slot, PrizeGuaranteeType.Activity, ID, ref itemId, ref itemCount);
                    ItemInfo _iteminfo = additems.Find(x => x.itemId == itemId);
                    if (_iteminfo != null)
                        _iteminfo.stackCount += itemCount;
                    else
                        additems.Add(new ItemInfo() { itemId = (ushort)itemId, stackCount = (ushort)itemCount });
                    str_ids += (itemId + ",");
                    str_counts += (itemCount + ",");
                }
            }

            str_ids = str_ids.Substring(0, str_ids.Length - 1);
            str_counts = str_counts.Substring(0, str_counts.Length - 1);

            InvRetval retval = player.Slot.mInventory.AddItemsIntoInventory(additems, false, "Lottery");
            if (retval.retCode == InvReturnCode.AddFailed || retval.retCode == InvReturnCode.Full)
                return false;

            foreach (var item in additems)
            {
                Zealot.Server.Rules.RareItemNotificationRules.CheckNotification(item.itemId, player.Name);

                //// Log
                //Zealot.Logging.Client.LogClasses.LotteryGetItem getItem = new Zealot.Logging.Client.LogClasses.LotteryGetItem();
                //getItem.userId = player.Slot.mUserId;
                //getItem.charId = player.Slot.GetCharId();
                //getItem.lotteryId = ID;
                //getItem.itemId = item.itemid;
                //getItem.count = item.stackcount;
                //getItem.message = string.Format("Lottery get reward item, itemid: {0}, count: {1}", item.itemid, item.stackcount);
                //var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(getItem);
            }

            return true;
        }

        public bool AddCountAndPoint(Player player, int base_count, out int get_point)
        {
            int oldpt = player.GetLotteryPoint(ID);
            get_point = base_count * info.getPoint;
            player.AddLotteryCountAndPoint(ID, base_count, get_point);

            // Log
            //Zealot.Logging.Client.LogClasses.LotteryGetPoint getpt = new Zealot.Logging.Client.LogClasses.LotteryGetPoint();
            //getpt.userId = player.Slot.mUserId;
            //getpt.charId = player.Slot.GetCharId();
            //getpt.lotteryId = ID;
            //getpt.getPoint = get_point;
            //getpt.oldPoint = oldpt;
            //getpt.newPoint = player.GetLotteryPoint(ID);
            //getpt.rollTimes = base_count;
            //getpt.message = string.Format("Lottery get point from roll, get point: {0}, old point: {1}, new point: {2}, roll times: {3}", getpt.getPoint, getpt.oldPoint, getpt.newPoint, getpt.rollTimes);
            //var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(getpt);

            return true;
        }

        public void LotteryOne(Player player, int cost_type = 0)
        {
            if (CheckCanGetItems(player, 1, 0) == false)
            {
                return;
            }

            int free = -1;
            int extra = -1;
            LotteryUseFreeAndGetPoint lotteryLog = new LotteryUseFreeAndGetPoint();
            lotteryLog.lotteryId = ID;
            lotteryLog.userId = player.Slot.mUserId;
            lotteryLog.charId = player.Slot.GetCharId();
            if (DeductCost(player, cost_type, 1, out free, out extra, ref lotteryLog) == false)
            {
                return;
            }

            string ids = "";
            string counts = "";
            if (GetItems(player, 1, out ids, out counts) == false)
            {
                return;
            }

            var getPoint = 0;
            AddCountAndPoint(player, 1, out getPoint);
            lotteryLog.getPoint = getPoint;

            // update local object for all change at last
            player.UpdateLotteryStat(ID);

            player.Slot.ZRPC.NonCombatRPC.Ret_ShowLotteryRollOneResult(ID, int.Parse(ids), int.Parse(counts),
                free, extra, getPoint, player.Slot);

            player.Slot.mQuestExtraRewardsCtrler.UpdateTask(Zealot.Common.QuestExtraType.LotteryTimes);

            lotteryLog.message = string.Format("Lottery, use free times: {0}, last free times: {1}, get point: {2}", lotteryLog.useFreeCount, lotteryLog.lastFreeCount, lotteryLog.getPoint);
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(lotteryLog);
        }

        public void LotteryTen(Player player, int cost_type = 0)
        {
            if (cost_type == (byte)LotteryCostType.Free)
            {
                return;
            }

            if (CheckCanGetItems(player, 10, 1) == false)
            {
                return;
            }

            var baseCount = 10;
            var totalCount = 11;
            int free = -1;
            int extra = -1;
            LotteryUseFreeAndGetPoint lotteryLog = new LotteryUseFreeAndGetPoint();
            lotteryLog.lotteryId = ID;
            lotteryLog.userId = player.Slot.mUserId;
            lotteryLog.charId = player.Slot.GetCharId();
            if (DeductCost(player, cost_type, baseCount, out free, out extra, ref lotteryLog) == false)
            {
                return;
            }

            string ids = "";
            string counts = "";
            if (GetItems(player, totalCount, out ids, out counts) == false)
            {
                return;
            }

            var getPoint = 0;
            AddCountAndPoint(player, baseCount, out getPoint);
            lotteryLog.getPoint = getPoint;

            // update local object for all change at last
            player.UpdateLotteryStat(ID);

            player.Slot.ZRPC.NonCombatRPC.Ret_ShowLotteryRollTenResult(ID, ids, counts,
                free, extra, getPoint, player.Slot);

            player.Slot.mQuestExtraRewardsCtrler.UpdateTask(Zealot.Common.QuestExtraType.LotteryTimes, baseCount);

            lotteryLog.message = string.Format("Lottery, use free times: {0}, last free times: {1}, get point: {2}", lotteryLog.useFreeCount, lotteryLog.lastFreeCount, lotteryLog.getPoint);
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(lotteryLog);
        }

        public bool GetPointRewardItem(Player player, int point)
        {
            if (info.pointRewardInfo.ContainsKey(point))
            {
                if (player.GetLotteryPoint(ID) < point)
                {
                    player.Slot.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(GUILocalizationRepo.GetSysMsgIdByName("Lottery_Notreachingthetargetpoints"), player.Slot);
                    return false;
                }

                var items = info.pointRewardInfo[point].items;
                if (player.Slot.mInventory.GetEmptySlotCount() < items.Count)
                {
                    player.Slot.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(GUILocalizationRepo.GetSysMsgIdByName("Lottery_Backpackspacelessthan"), player.Slot);
                    return false;
                }

                if (player.AddLotteryRewarderPoint(ID, point) == false)
                {
                    player.Slot.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(GUILocalizationRepo.GetSysMsgIdByName("Lottery_Rewardhasbeenredeemed"), player.Slot);
                    return false;
                }

                string ids = "";
                string counts = "";
                List<ItemInfo> additems = new List<ItemInfo>();
                string logmsg = "";
                foreach (var kvp in items)
                {
                    if (kvp.Key > 0 && kvp.Value > 0)
                    {
                        ushort itemId = (ushort)kvp.Key;
                        ushort itemCount = (ushort)kvp.Value;
                        ItemInfo _iteminfo = additems.Find(x => x.itemId == itemId);
                        if (_iteminfo != null)
                            _iteminfo.stackCount += itemCount;
                        else
                            additems.Add(new ItemInfo() { itemId = itemId, stackCount = itemCount });

                        ids += kvp.Key + ",";
                        counts += kvp.Value + ",";
                        logmsg += "itemid" + additems.Count + ": " + kvp.Key + ", count" + additems.Count + ": " + kvp.Value + ", ";
                    }
                }
                ids = ids.Substring(ids.Length - 1, 1);
                counts = counts.Substring(counts.Length - 1, 1);

                InvRetval retval = player.Slot.mInventory.AddItemsIntoInventory(additems, false, "Lottery");
                if (retval.retCode == InvReturnCode.AddFailed || retval.retCode == InvReturnCode.Full)
                {
                    // Maybe need error message
                    return false;
                }

                // update local object for all change at last
                player.UpdateLotteryStat(ID);

                player.Slot.ZRPC.NonCombatRPC.Ret_LotteryGetPointRewardResult(ID, point, ids, counts, player.Slot);

                // Log
                //Zealot.Logging.Client.LogClasses.LotteryGetPointReward getptitem = new Zealot.Logging.Client.LogClasses.LotteryGetPointReward();
                //getptitem.userId = player.Slot.mUserId;
                //getptitem.charId = player.Slot.GetCharId();
                //getptitem.lotteryId = ID;
                //getptitem.point = point;
                //if (additems.Count > 0)
                //{
                //    getptitem.itemId1 = additems[0].itemid;
                //    getptitem.count1 = additems[0].stackcount;
                //}
                //if (additems.Count > 1)
                //{
                //    getptitem.itemId2 = additems[1].itemid;
                //    getptitem.count2 = additems[1].stackcount;
                //}
                //if (additems.Count > 2)
                //{
                //    getptitem.itemId3 = additems[2].itemid;
                //    getptitem.count3 = additems[2].stackcount;
                //}
                //getptitem.message = string.Format("Lottery get point reward item, point: " + getptitem.point + ", " + logmsg);
                //var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(getptitem);
            }

            return true;
        }

        public void UsePointItem(Player player, int item_id)
        {
            if (item_id != info.pointItemId)
            {
                // Maybe need error message
                return;
            }

            int oldpt = player.GetLotteryPoint(ID);
            InvRetval retval = player.Slot.mInventory.DeductItem((ushort)item_id, 1, "Lottery");
            if (retval.retCode == InvReturnCode.UseSuccess)
            {
                player.AddLotteryPoint(ID, info.pointItemGetPoint);

                // update local object for all change at last
                player.UpdateLotteryStat(ID);

                player.Slot.ZRPC.NonCombatRPC.Ret_LotteryUsePointItemResult((int)retval.retCode, info.pointItemGetPoint, player.Slot);

                // Log
                //Zealot.Logging.Client.LogClasses.LotteryUsePointItem useptitem = new Zealot.Logging.Client.LogClasses.LotteryUsePointItem();
                //useptitem.userId = player.Slot.mUserId;
                //useptitem.charId = player.Slot.GetCharId();
                //useptitem.lotteryId = ID;
                //useptitem.itemId = item_id;
                //useptitem.count = 1;
                //useptitem.getPoint = info.pointItemGetPoint;
                //useptitem.oldPoint = oldpt;
                //useptitem.newPoint = player.GetLotteryPoint(ID);
                //useptitem.message = string.Format("Lottery use point item, itemid: {0}, count: {1}, get point: {2}, old point: {3}, new point: {4}", item_id, 1, useptitem.getPoint, useptitem.oldPoint, useptitem.newPoint);
                //var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(useptitem);
            }
            else
            {
                player.Slot.ZRPC.NonCombatRPC.Ret_LotteryUsePointItemResult((int)retval.retCode, 0, player.Slot);
            }
        }
    }

    class LotteryActivityFSM : StateMachine
    {
        bool isActive = false;
        public bool InActive { get { return isActive; } }
        DateTime now;
        LotteryActivityController lotteryCtrl;
        public LotteryActivityController Controller { get { return lotteryCtrl; } }

        public delegate void OnStartDelegate(int id);
        public delegate void OnExpireDelegate(int id);


        public OnStartDelegate OnStartDel { get; set; }
        public OnExpireDelegate OnExpireDel { get; set; }

        public LotteryActivityFSM(LotteryActivityController info_controller)
        {
            lotteryCtrl = info_controller;

            AddState(LotteryActiveStatus.WaitStart.ToString(), OnWaitStartEnter, null, OnWaitStartUpdate);
            AddState(LotteryActiveStatus.InActive.ToString(), OnInActiveEnter, OnInActiveLeave, OnInActiveUpdate);
            AddState(LotteryActiveStatus.Expire.ToString(), OnFinishEnter, null, null);
            AddState(LotteryActiveStatus.ForcePass.ToString(), OnForcePassEnter, null, null);
        }

        public override void OnUpdate(long dt)
        {
            now = DateTime.Now;
            base.OnUpdate(dt);
        }

        #region State function
        void OnWaitStartEnter(string pre_state_name)
        {
            isActive = false;
        }

        void OnWaitStartUpdate(long dt)
        {
            if (now >= lotteryCtrl.StartTime && now < lotteryCtrl.EndTime)
            {
                GotoState(LotteryActiveStatus.InActive.ToString());
            }
            else if (now >= lotteryCtrl.EndTime)
            {
                GotoState(LotteryActiveStatus.Expire.ToString());
            }
        }

        void OnInActiveEnter(string pre_state_name)
        {
            isActive = true;
            OnStartDel(Controller.ID);
        }

        void OnInActiveLeave()
        {
            OnExpireDel(Controller.ID);
        }

        void OnInActiveUpdate(long dt)
        {
            if (now > lotteryCtrl.EndTime)
            {
                GotoState(LotteryActiveStatus.Expire.ToString());
            }
        }

        void OnFinishEnter(string pre_state_name)
        {
            isActive = false;
        }

        void OnForcePassEnter(string pre_state_name)
        {
            isActive = false;
        }
        #endregion

        public void Start()
        {
            GotoState(LotteryActiveStatus.WaitStart.ToString());
        }

        public void ForceActive()
        {
            GotoState(LotteryActiveStatus.InActive.ToString());
        }

        public void ForcePass()
        {
            GotoState(LotteryActiveStatus.ForcePass.ToString());
        }

        public void Resume()
        {
            GotoState(LotteryActiveStatus.WaitStart.ToString());
        }
    }

    class LotteryManager
    {
        private static volatile LotteryManager instance;
        private static object syncRoot = new object();
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public static LotteryManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new LotteryManager();
                        }
                    }
                }

                return instance;
            }
        }

        private readonly PoolFiber executionFiber;

        // Current and next lottery activity
        private Dictionary<int, LotteryActivityFSM> LotteryActivities;
        // All about now and future lottery information
        private Dictionary<int, LotteryInfo> LotteryInfos;

        public LotteryManager()
        {
            LotteryActivities = new Dictionary<int, LotteryActivityFSM>();
            LotteryInfos = new Dictionary<int, LotteryInfo>();
            executionFiber = GameApplication.Instance.executionFiber;
        }

        public void Init()
        {
            OnNewDay();
            SetUpdateNextInterval();
        }

        private void SetUpdateNextInterval()
        {
            executionFiber.Schedule(Update, 1000);
        }

        public void OnNewDay()
        {
            if (LotteryActivities.Count == 0)
                FindCurrentActivity();
            //FindNextActivity();
        }

        public void Update()
        {
            foreach(KeyValuePair<int, LotteryActivityFSM> kvp in LotteryActivities)
            {
                kvp.Value.OnUpdate(0);
            }
            SetUpdateNextInterval();
        }

        #region Control
        bool FindCurrentActivity()
        {
            // Find now in-active info and add FSM
            var dataList = LotteryMainRepo.GetPeriodLottery();
            if (dataList.Count > 0)
            {
                dataList.ForEach(mainData => SetCurrentActivity(mainData));
                return true;
            }
            else
            {
                return false;
            }
        }

        void SetCurrentActivity( LotteryMainData main_data)
        {
            lock (syncRoot)
            {
                LotteryActivityFSM fsm;
                if (LotteryActivities.TryGetValue(main_data.main.id, out fsm) == true)
                {
                    if (fsm.InActive == false)
                        fsm.ForceActive();
                    return;
                }

                var info = new LotteryInfo(main_data);
                var ctrl = new LotteryActivityController(info);
                fsm = new LotteryActivityFSM(ctrl);
                fsm.OnStartDel = OnNewestActivityStart;
                fsm.OnExpireDel = OnCurrentActivityExpired;

                // Need add first
                LotteryActivities.Add(main_data.main.id, fsm); 

                // Force to active
                fsm.ForceActive();
            }
        }

        void FindNextActivity()
        {
            // Find next recently info and check in LotteryActivities or add FSM
            // but now change lottery at server maintain, so no this not test yet
            lock (syncRoot)
            {
                var dataList = LotteryMainRepo.GetFutureLottery();
                for (int i = 0; i < dataList.Count; i++)
                {
                    var data = dataList[i];
                    if (LotteryActivities.ContainsKey(data.main.id) == false)
                    {
                        TimeSpan span = new TimeSpan(data.start.Ticks - DateTime.Now.Ticks);
                        if (span.TotalDays < 1) {
                            var info = new LotteryInfo(data);
                            var ctrl = new LotteryActivityController(info);
                            var fsm = new LotteryActivityFSM(ctrl);
                            fsm.OnStartDel = OnNewestActivityStart;
                            fsm.OnExpireDel = OnCurrentActivityExpired;

                            // Need add first
                            LotteryActivities.Add(data.main.id, fsm);

                            // start
                            fsm.Start();
                        }
                    }
                }
            }
            
        }

        void OnNewestActivityStart(int lottery_id)
        {
            //var fsm;
            //if (LotteryActivities.TryGetValue(lottery_id, fsm))
            //{
            //    if (newestActive != null)
            //    {
            //        if (newestActive.Controller.ID == lottery_id)
            //            return;

            //        if (newestActive.Controller.StartTime >fsm.Controller.StartTime)
            //            return;

            //        if (newestActive.Controller.EndTime > fsm.Controller.EndTime)
            //            return;

            //        if (newestActive.Controller.ID > lottery_id)
            //            return;
            //    }

            //    newestActive = fsm;
            //}
            if (log.IsDebugEnabled)
                log.DebugFormat("Lottery start, id : {0} ", lottery_id);
        }

        void OnCurrentActivityExpired(int lottery_id)
        {
            //if (newestActive != null && newestActive.Controller.ID == lottery_id)
            //    newestActive = null;

            if (LotteryActivities.ContainsKey(lottery_id)) {
                LotteryActivities.Remove(lottery_id);
            }

            if (LotteryActivities.Count == 0)
                FindCurrentActivity();

            if (log.IsDebugEnabled)
                log.DebugFormat("Lottery end, id : {0} ", lottery_id);
        }
        #endregion

        #region Get Data
        public int GetDailyFreeTicketCount(int lottery_id = -1)
        {
            var controller = GetControllerWhenActive(lottery_id);
            if (controller != null)
            {
                if (lottery_id == -1 || controller.ID == lottery_id)
                    return controller.Info.dailyFreeTickets;
            }

            return 0;
        }

        public void GetLotterySimpleInfoToClient(GameClientPeer peer)
        {
            // this no using and contradiction!
            //var controller = GetControllerWhenActive(lottery_id);
            //if (controller == null)
            //{
            //    if (controller.UpdatePlayerInventoryData(peer.mPlayer) == true)
            //    {
            //        peer.ZRPC.NonCombatRPC.Ret_LotteryGetSimpleInfo(controller.ID, peer);
            //    }
            //}

            //peer.ZRPC.NonCombatRPC.Ret_LotteryGetSimpleInfo(0, peer);

            //peer.ZRPC.NonCombatRPC.Ret_LotteryGetSimpleInfo(controller.ID, controller.StartTime.Ticks, controller.EndTime.Ticks, controller.Focus, peer);
        }

        public void GetLotteryInfoToClient(GameClientPeer peer, int lottery_id)
        {
            var controller = GetControllerWhenActive(lottery_id);
            if (controller == null)
                return;

            if (controller.ID != lottery_id)
                return;

            //var info = controller.Info;
            //peer.ZRPC.NonCombatRPC.Ret_LotteryGetInfo(controller.ID, controller.LotteryItems, info.costGold, controller.PointRewards, peer);
        }
        #endregion

        public bool CheckHaveActivity(int lottery_id)
        {
            if (LotteryActivities.Count <= 0)
            {
                return FindCurrentActivity();
            }

            LotteryActivityFSM fsm;
            if (LotteryActivities.TryGetValue(lottery_id, out fsm) == false)
                return false;

            return fsm.InActive;
        }

        public LotteryActivityController GetControllerWhenActive(int lottery_id)
        {
            if (LotteryActivities.Count <= 0)
            {
                if (FindCurrentActivity() == false)
                    return null;
            }

            LotteryActivityFSM fsm;
            if (LotteryActivities.TryGetValue(lottery_id, out fsm) == false)
                return null;

            if (fsm.InActive == false)
                return null;

            return fsm.Controller;
        }

        bool CheckCanLottery(int lottery_id)
        {
            if (CheckHaveActivity(lottery_id) == false)
                return false;

            return true;
        }

        public void LotteryOne(GameClientPeer peer, int lottery_id, int cost_type = 0)
        {
            var  controller = GetControllerWhenActive(lottery_id);
            if (controller == null)
            {
                peer.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(-1, peer);
                return;
            }

            controller.LotteryOne(peer.mPlayer, cost_type);
        }

        public void LotteryTen(GameClientPeer peer, int lottery_id, int cost_type = 0)
        {
            var controller = GetControllerWhenActive(lottery_id);
            if (controller == null)
            {
                peer.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(-1, peer);
                return;
            }

            controller.LotteryTen(peer.mPlayer, cost_type);    
        }

        public void GetPointRewardItem(GameClientPeer peer, int lottery_id, int point)
        {
            var controller = GetControllerWhenActive(lottery_id);
            if (controller == null)
            { 
                peer.ZRPC.NonCombatRPC.Ret_LotteryRollFailed(GUILocalizationRepo.GetSysMsgIdByName("Lottery_Notreachingthetargetpoints"), peer);
                return;
            }

            controller.GetPointRewardItem(peer.mPlayer, point);
        }

        public void UsePointItem(GameClientPeer peer, int lottery_id, int item_id)
        {
            var controller = GetControllerWhenActive(lottery_id);
            if (controller == null)
            {
                peer.ZRPC.NonCombatRPC.Ret_LotteryUsePointItemResult(-1, 0, peer);
                return;
            }

            controller.UsePointItem(peer.mPlayer, item_id);
        }
    }
}
