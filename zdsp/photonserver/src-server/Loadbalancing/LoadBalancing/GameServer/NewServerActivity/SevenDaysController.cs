using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.Entities;
using Zealot.Server.Rules;
using Kopio.JsonContracts;

namespace Photon.LoadBalancing.GameServer
{
    public enum SevenDaysTaskState
    {
        Incomplete = 0,
        Complete = 1,
    }

    public class SevenDaysTaskElem
    {
        internal NewServerActivityType taskType;
        internal string taskName;
        internal int value1;
        internal int value2;
        internal int count1;
        internal int count2;
        internal int id;

        public SevenDaysTaskElem(NewServerActivityType type, string name, int id, int tv1, int tv2)
        {
            taskType = type;
            taskName = name;
            this.id = id;
            value1 = 0;
            value2 = 0;
            count1 = tv1;
            count2 = tv2;
        }

        public SevenDaysTaskElem(NewServerActivityType type, string name, int id, int v1, int v2, int uv1, int uv2)
        {
            taskType = type;
            taskName = name;
            this.id = id;
            value1 = v1;
            value2 = v2;
            count1 = uv1;
            count2 = uv2;
        }
    }

    public class SevenDaysTaskVal
    {
        public int val1;
        public int val2;

        public SevenDaysTaskVal(int v1, int v2)
        {
            val1 = v1;
            val2 = v2;
        }

        public bool IsComplete(int count1, int count2)
        {
            if (count2 > 0)
            {
                return val1 >= count1 && val2 >= count2;
            }
            else if (count1 > 0 && count2 == 0)
            {
                return val1 >= count1;
            }

            return false;
        }
    }

    public class SevenDaysController
    {
        // Player Slot
        private GameClientPeer _peer;
        // NewServerActivityType -> Count2 <-> Count 1 Progress
        //private Dictionary<NewServerActivityType, Dictionary<int, int>> taskProgress = new Dictionary<NewServerActivityType, Dictionary<int, int>>();
        //private Dictionary<Days, Dictionary<int, List<SevenDaysTask>>> taskProgress = new Dictionary<Days, Dictionary<int, List<SevenDaysTask>>>();
        private List<SevenDaysTask> taskProgress = new List<SevenDaysTask>();

        public SevenDaysController(DateTime startDate, GameClientPeer peer)
        {
            _peer = peer;
        }

        public BuyDiscItmCode BuyDiscountItem(int day, int amount)
        {
            BuyDiscItmRetval res = _peer.CharacterData.SevenDaysInventory.BuyDiscountItem(day, amount);

            if (res.mBuyCode == BuyDiscItmCode.BuySuccess)
            {
                _peer.mPlayer.SevenDaysStats.discountItemBoughtNums = res.mBuyStr;
            }

            return res.mBuyCode;
        }

        public int GetDiscountItemBoughtNums(int day)
        {
            return _peer.CharacterData.SevenDaysInventory.GetDiscountItemBoughtNums(day);
        }

        public bool IsTaskRewardCollected(int day, int subcat, int taskId)
        {
            return _peer.CharacterData.SevenDaysInventory.IsTaskRewardCollected((Days)day, subcat, taskId);
        }

        public bool CollectTaskReward(int day, int subcat, int taskId)
        {
            bool res = _peer.CharacterData.SevenDaysInventory.CollectTaskReward((Days)day, subcat, taskId);
            
            Zealot.Common.Datablock.CollectionHandler<object> todayCollectList = GetTaskCollectionByDay(day);
            int todayCollect = (int)todayCollectList[subcat];
            todayCollect = GameUtils.SetBit(todayCollect, taskId);
            todayCollectList[subcat] = todayCollect;
            
            if(CheckTaskRewardAllCollected(todayCollectList, (Days)day))
            {
                SetTaskRewardAllCollectedByDay((Days)day);
            }

            return res;
        }

        public void InitTaskProgress()
        {
            if(!IsEventRunning())
            {
                return;
            }

            taskProgress = _peer.CharacterData.SevenDaysInventory.InitTaskProgressList();
            taskProgress = _peer.CharacterData.SevenDaysInventory.DeserializeTaskProgress(taskProgress);
            taskProgress = _peer.CharacterData.SevenDaysInventory.DeserializeTaskCollected(taskProgress);

            UpdateTaskWithoutStats(NewServerActivityType.Level, _peer.mPlayer.PlayerSynStats.Level);
            UpdateTaskWithoutStats(NewServerActivityType.Militantrank, _peer.CharacterData.ArenaInventory.ArenaRankHighest);
            UpdateTaskWithoutStats(NewServerActivityType.Equipupgrade_n);
            UpdateTaskWithoutStats(NewServerActivityType.Herolevel_n);
            UpdateTaskWithoutStats(NewServerActivityType.Heroskilltotal_n);

            string taskProgressStr = _peer.CharacterData.SevenDaysInventory.SerializeTaskProgress(taskProgress);
            _peer.CharacterData.SevenDaysInventory.SetTaskProgress(taskProgressStr);

            string taskCollectedStr = _peer.CharacterData.SevenDaysInventory.SerializeTaskCollected(taskProgress);
            _peer.CharacterData.SevenDaysInventory.SetTaskCollected(taskCollectedStr);
        }

        public void UpdateTask(DungeonDifficulty difficulty)
        {
            int completedCount = _peer.mPlayer.RealmStats.GetDungeonStoryCompletedCount(difficulty);
            switch(difficulty)
            {
                case DungeonDifficulty.Easy:
                    UpdateTask(NewServerActivityType.Normalchapter);
                    break;
                case DungeonDifficulty.Normal:
                    UpdateTask(NewServerActivityType.Elitechapter);
                    break;
                case DungeonDifficulty.Hard:
                    UpdateTask(NewServerActivityType.Hellchapter);
                    break;
            }
        }

        public void UpdateTaskWithoutStats(NewServerActivityType type, int progress = 0)
        {
            if (!IsEventRunning())
            {
                return;
            }

            if (IsCollectionPeriod())
            {
                // Task completion period ended!
                return;
            }

            int maxDays = (int)Days.NUM_DAYS;
            for (int i = 0; i < maxDays; ++i)
            {
                Days currDay = (Days)i;
                List<int> subcatList = SevenDaysRepo.GetSubCategoryIDsByDay(currDay);
                for (int j = 0; j < subcatList.Count; ++j)
                {
                    List<NewServerActivityJson> currentTaskList = SevenDaysRepo.GetNewServerActivityList(currDay, j);
                    if (currentTaskList != null)
                    {
                        for (int t = 0; t < currentTaskList.Count; ++t)
                        {
                            NewServerActivityJson taskData = currentTaskList[t];
                            if (taskData != null && taskData.type == type)
                            {
                                int taskPos = taskProgress.FindIndex(o => o.ID() == taskData.id);
                                if (taskPos != -1)
                                {
                                    // Special case: Equipment item upgrade
                                    if (taskProgress[taskPos].Type() == NewServerActivityType.Equipupgrade_n)
                                    {
                                        int realProg = 0;
                                        //List<EquipItem> equipList = _peer.CharacterData.EquippedInventory.Slots;
                                        //for (int e = 0; e < equipList.Count; ++e)
                                        //{
                                        //    EquipItem equipment = equipList[e];
                                        //    if (equipment != null && equipment.UpgradeLevel >= taskProgress[taskPos].Count2())
                                        //    {
                                        //        ++realProg;
                                        //    }
                                        //}
                                        taskProgress[taskPos].Update(realProg);
                                    }
                                    else if (taskProgress[taskPos].Type() == NewServerActivityType.Herolevel_n)
                                    {
                                        int realProg = 0;
                                        //List<NewHeroData> heroList = _peer.CharacterData.NewHeroInvData.GetHeroCardOnly();
                                        //for(int h = 0; h < heroList.Count; ++h)
                                        //{
                                        //    NewHeroData hero = heroList[h];
                                        //    if (hero != null && hero.herocardlevel >= taskProgress[taskPos].Count2())
                                        //    {
                                        //        ++realProg;
                                        //    }
                                        //}
                                        taskProgress[taskPos].Update(realProg);
                                    }
                                    else if (taskProgress[taskPos].Type() == NewServerActivityType.Heroskilltotal_n)
                                    {
                                        int realProg = 0;
                                        //List<NewHeroData> heroList = _peer.CharacterData.NewHeroInvData.GetHeroCardOnly();
                                        //for(int h = 0; h < heroList.Count; ++h)
                                        //{
                                        //    NewHeroData hero = heroList[h];
                                        //    if (hero != null && hero.heroskilllevel >= taskProgress[taskPos].Count2())
                                        //    {
                                        //        ++realProg;
                                        //    }
                                        //}
                                        taskProgress[taskPos].Update(realProg);
                                    }
                                    else if (taskProgress[taskPos].Type() == NewServerActivityType.Normalchapter ||
                                        taskProgress[taskPos].Type() == NewServerActivityType.Elitechapter ||
                                        taskProgress[taskPos].Type() == NewServerActivityType.Hellchapter)
                                    {
                                        taskProgress[taskPos].Update();
                                    }
                                    else
                                    {
                                        taskProgress[taskPos].Update(progress);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UpdateTask(NewServerActivityType type, int progress = 0)
        {
            UpdateTaskWithoutStats(type, progress);

            SerializeToClient();
        }

        private void SerializeToClient()
        {
            string taskProgressStr = _peer.CharacterData.SevenDaysInventory.SerializeTaskProgress(taskProgress);
            _peer.CharacterData.SevenDaysInventory.SetTaskProgress(taskProgressStr);
            _peer.mPlayer.SevenDaysStats.taskProgress = taskProgressStr;
        }

        //public void SetTaskComplete(int dataid)
        //{
        //    //_peer.CharacterData.QuestExtraRewardsInventory.SetTaskComplete(dataid);
        //    //_peer.mPlayer.QuestExtraRewardsStats.taskProgress = _peer.CharacterData.QuestExtraRewardsInventory.SerializeTaskProgressFromInv();
        //}

        //public void CompleteTask(int day, int subcat, int dataid)
        //{
        //    _peer.CharacterData.SevenDaysInventory.CompleteTask((Days)day, subcat, dataid);
        //}

        //public void CompleteTaskStats(int day, int subcat, int dataid)
        //{
        //    Zealot.Common.Datablock.CollectionHandler<object> todayCompletionList = GetTaskCompletionByDay(day);
        //    int todayComplete = (int)todayCompletionList[subcat];
        //    todayComplete |= InventoryUtils.GetBitShiftByPos(dataid);
        //    todayCompletionList[subcat] = todayComplete;
        //}

        public bool IsTaskCompletedServer(int taskid)
        {
            SevenDaysTask task = taskProgress.Find(o => o.ID() == taskid);
            if (task != null)
            {
                return task.IsComplete();
            }

            return false;
        }

        public bool CollectTaskReward(int taskid)
        {
            int taskPos = taskProgress.FindIndex(o => o.ID() == taskid);
            if (taskPos != -1)
            {
                taskProgress[taskPos].Collect();
                SerializeTaskCollected();
                return true;
            }

            return false;
        }

        public void SerializeTaskCollected()
        {
            string taskCollectedStr = _peer.CharacterData.SevenDaysInventory.SerializeTaskCollected(taskProgress);
            _peer.CharacterData.SevenDaysInventory.SetTaskCollected(taskCollectedStr);
            _peer.mPlayer.SevenDaysStats.taskCollected = taskCollectedStr;
        }

        public bool IsTaskCollectedServer(int taskid)
        {
            SevenDaysTask task = taskProgress.Find(o => o.ID() == taskid);
            if (task != null)
            {
                return task.IsCollected();
            }

            return false;
        }

        public void SendUncollectedRewardsToPlayer()
        {
            int maxDays = (int)Days.NUM_DAYS;

            for (int i = 0; i < maxDays; ++i)
            {
                for (int j = 0; j < SevenDaysInvData.MAX_SUBCAT; ++j)
                {
                    List<NewServerActivityJson> currentTaskList = SevenDaysRepo.GetNewServerActivityList((Days)i, j);
                    if (currentTaskList != null)
                    {
                        for (int t = 0; t < currentTaskList.Count; ++t)
                        {
                            NewServerActivityJson taskData = currentTaskList[t];
                            if(IsTaskCompletedServer(taskData.id) && !IsTaskCollectedServer(taskData.id))
                            {
                                SevenDaysTaskDataList taskList = SevenDaysRepo.GetNewServerActivityTasksList((Days)i, j);
                                SevenDaysTaskData taskRewardData = taskList.GetTask(SevenDaysRepo.GetFormattedTaskName(taskData));
                                //List<RewardItemInfo> rewardlist = RewardListRepo.GetRewardItemsById(taskRewardData.mRewardId);

                                //if (rewardlist == null)
                                //{
                                //    _peer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_SevenDaysRewardList_NotFound"), "", false, this);
                                //    return;
                                //}

                                //List<IInventoryItem> realRewardList = new List<IInventoryItem>();
                                //for(int r = 0; r < rewardlist.Count; ++r)
                                //{
                                //    RewardItemInfo reward = rewardlist[r];
                                //    IInventoryItem realReward = GameRepo.ItemFactory.GetInventoryItem(reward.itemid);
                                //    realRewardList.Add(realReward);
                                //}

                                //GameRules.SendMailWithItem(_peer.mPlayer.Name, "Reward_SevenDays", realRewardList);
                                //CollectTaskReward(taskData.id);
                            }
                        }
                    }
                }
            }
        }

        private bool CheckTaskRewardAllCollected(Zealot.Common.Datablock.CollectionHandler<object> todayCollectList, Days day)
        {
            int subcatCount = SevenDaysRepo.GetSubCategoryIDsByDay(day).Count;
            for (int i = 0; i < subcatCount; ++i)
            {
                int taskCount = SevenDaysRepo.GetNewServerActivityTasksCount(day, i);
                int doneCount = (int)Math.Pow(2, taskCount) - 1;

                if ((int)todayCollectList[i] != doneCount)
                {
                    return false;
                }
            }

            return true;
        }

        private Zealot.Common.Datablock.CollectionHandler<object> GetTaskCollectionByDay(int day)
        {
            switch ((Days)day)
            {
                case Days.One:
                    return _peer.mPlayer.SevenDaysStats.dayOneTasksCollected;
                case Days.Two:
                    return _peer.mPlayer.SevenDaysStats.dayTwoTasksCollected;
                case Days.Three:
                    return _peer.mPlayer.SevenDaysStats.dayThreeTasksCollected;
                case Days.Four:
                    return _peer.mPlayer.SevenDaysStats.dayFourTasksCollected;
                case Days.Five:
                    return _peer.mPlayer.SevenDaysStats.dayFiveTasksCollected;
                case Days.Six:
                    return _peer.mPlayer.SevenDaysStats.daySixTasksCollected;
                case Days.Seven:
                    return _peer.mPlayer.SevenDaysStats.daySevenTasksCollected;
                default:
                    return null;
            }
        }

        private void SetTaskRewardAllCollectedByDay(Days day)
        {
            switch (day)
            {
                case Days.One:
                    _peer.mPlayer.SevenDaysStats.dayOneTasksAllCollected = true;
                    break;
                case Days.Two:
                    _peer.mPlayer.SevenDaysStats.dayTwoTasksAllCollected = true;
                    break;
                case Days.Three:
                    _peer.mPlayer.SevenDaysStats.dayThreeTasksAllCollected = true;
                    break;
                case Days.Four:
                    _peer.mPlayer.SevenDaysStats.dayFourTasksAllCollected = true;
                    break;
                case Days.Five:
                    _peer.mPlayer.SevenDaysStats.dayFiveTasksAllCollected = true;
                    break;
                case Days.Six:
                    _peer.mPlayer.SevenDaysStats.daySixTasksAllCollected = true;
                    break;
                case Days.Seven:
                    _peer.mPlayer.SevenDaysStats.daySevenTasksAllCollected = true;
                    break;
            }
        }

        private Zealot.Common.Datablock.CollectionHandler<object> GetTaskCompletionByDay(int day)
        {
            switch ((Days)day)
            {
                case Days.One:
                    return _peer.mPlayer.SevenDaysStats.dayOneTasksCompleted;
                case Days.Two:
                    return _peer.mPlayer.SevenDaysStats.dayTwoTasksCompleted;
                case Days.Three:
                    return _peer.mPlayer.SevenDaysStats.dayThreeTasksCompleted;
                case Days.Four:
                    return _peer.mPlayer.SevenDaysStats.dayFourTasksCompleted;
                case Days.Five:
                    return _peer.mPlayer.SevenDaysStats.dayFiveTasksCompleted;
                case Days.Six:
                    return _peer.mPlayer.SevenDaysStats.daySixTasksCompleted;
                case Days.Seven:
                    return _peer.mPlayer.SevenDaysStats.daySevenTasksCompleted;
                default:
                    return null;
            }
        }

        // General Helper functions
        public bool IsEventRunning()
        {
            return IsEventPeriod() || IsCollectionPeriod();
        }

        public bool IsEventPeriod()
        {
            int maxDays = (int)Days.NUM_DAYS;
            DateTime endDT = _peer.createdDT.AddDays(maxDays);

            return DateTime.Now >= _peer.createdDT && DateTime.Now < endDT;
        }

        public bool IsCollectionPeriod()
        {
            DateTime endDT = _peer.createdDT.AddDays(10);

            return !IsEventPeriod() && DateTime.Now < endDT;
        }

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

        public int GetTaskProgress(GameClientPeer peer, NewServerActivityJson taskData)
        {
            int numHeroes = 0;
            //List<NewHeroData> heroCardData = peer.CharacterData.NewHeroInvData.ownedList;

            switch (taskData.type)
            {
                case NewServerActivityType.Level:
                    return peer.mPlayer.PlayerSynStats.Level;
                case NewServerActivityType.Points:
                    return peer.mPlayer.Slot.mWelfareCtrlr.GetTotalCreditedGold();
                case NewServerActivityType.Normalchapter:
                    return peer.mPlayer.RealmStats.GetDungeonStoryCompletedCount(DungeonDifficulty.Easy);
                case NewServerActivityType.Elitechapter:
                    return peer.mPlayer.RealmStats.GetDungeonStoryCompletedCount(DungeonDifficulty.Normal);
                case NewServerActivityType.Hellchapter:
                    return peer.mPlayer.RealmStats.GetDungeonStoryCompletedCount(DungeonDifficulty.Hard);
                case NewServerActivityType.ChapterStars:
                    return peer.mPlayer.RealmStats.GetTotalStarsCompleted();
                case NewServerActivityType.Herototal:
                    //return heroCardData.Count;
                case NewServerActivityType.Herolevel_n:
                    numHeroes = 0;

                    //foreach (NewHeroData entry in heroCardData)
                    //{
                    //    if (entry.herocardlevel >= taskData.count2)
                    //    {
                    //        ++numHeroes;
                    //    }
                    //}

                    return numHeroes;
                case NewServerActivityType.Equipupgrade_n:
                    int numEquip = 0;
                    //for (int i = 0; i < peer.CharacterData.EquippedInventory.Slots.Count; ++i)
                    //{
                    //    EquipItem equipment = peer.CharacterData.EquippedInventory.Slots[i];
                    //    if (equipment.UpgradeLevel >= taskData.count2)
                    //    {
                    //        ++numEquip;
                    //    }
                    //}

                    return numEquip;
                case NewServerActivityType.Heroskilltotal_n:
                    numHeroes = 0;

                    //foreach (NewHeroData entry in heroCardData)
                    //{
                    //    if (entry.heroskilllevel >= taskData.count2)
                    //    {
                    //        ++numHeroes;
                    //    }
                    //}

                    return numHeroes;
                case NewServerActivityType.Playerfighting:
                    return 0;
                case NewServerActivityType.Herofighting_n:
                    int heroPower = 0;

                    //foreach (NewHeroData entry in heroCardData)
                    //{
                         
                    //    int power = HeroRepo.GetHeroPower(entry.herocardid, entry.herocardlevel, entry.heroskilllevel);
                    //    heroPower += power;
                    //}

                    return heroPower;
                case NewServerActivityType.Militantrank:
                    return peer.CharacterData.ArenaInventory.ArenaRankHighest;
                case NewServerActivityType.Herosquality_n:
                    numHeroes = 0;

                    //foreach (NewHeroData entry in heroCardData)
                    //{
                    //    HeroCardJson heroCard = HeroRepo.GetHeroCardByID(entry.herocardid);
                    //    if ((int)heroCard.heroquality == (taskData.count2 - 1))
                    //    {
                    //        ++numHeroes;
                    //    }
                    //}

                    return numHeroes;
            }

            return -1;
        }

        public bool IsCurrentTaskCompleted(NewServerActivityJson taskData, int currProg, int reqCount)
        {
            if (taskData.type == NewServerActivityType.Militantrank)
            {
                return currProg <= reqCount;
            }
            else
            {
                return currProg >= reqCount;
            }
        }
    }
}
