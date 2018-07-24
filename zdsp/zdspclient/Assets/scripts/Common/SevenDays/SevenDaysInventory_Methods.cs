using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Zealot.Common.Entities;
using Zealot.Common;
using Kopio.JsonContracts;
using Zealot.Repository;

namespace Zealot.Common
{
    public partial class SevenDaysInvData
    {
        public void InitFromInventory(SevenDaysStats sevenDaysStats)
        {
            // Check for collection resizing
            CheckResize();

            // Init single variables
            //sevenDaysStats.sevenDaysEventStart          = SevenDaysEventStart;

            sevenDaysStats.discountItemBoughtNums       = DiscountItemBoughtNums;

            sevenDaysStats.taskProgress                 = TaskProgress;

            sevenDaysStats.taskCollected                = TaskCollected;

            sevenDaysStats.dayOneTasksAllCollected      = DayOneTasksAllCollected;
            sevenDaysStats.dayTwoTasksAllCollected      = DayTwoTasksAllCollected;
            sevenDaysStats.dayThreeTasksAllCollected    = DayThreeTasksAllCollected;
            sevenDaysStats.dayFourTasksAllCollected     = DayFourTasksAllCollected;
            sevenDaysStats.dayFiveTasksAllCollected     = DayFiveTasksAllCollected;
            sevenDaysStats.daySixTasksAllCollected      = DaySixTasksAllCollected;
            sevenDaysStats.daySevenTasksAllCollected    = DaySevenTasksAllCollected;

            // Init multi-variables
            for (int i = 0; i < MAX_SUBCAT; ++i)
            {
                sevenDaysStats.dayOneTasksCollected[i]   = DayOneTasksCollected[i];
                sevenDaysStats.dayTwoTasksCollected[i]   = DayTwoTasksCollected[i];
                sevenDaysStats.dayThreeTasksCollected[i] = DayThreeTasksCollected[i];
                sevenDaysStats.dayFourTasksCollected[i]  = DayFourTasksCollected[i];
                sevenDaysStats.dayFiveTasksCollected[i]  = DayFiveTasksCollected[i];
                sevenDaysStats.daySixTasksCollected[i]   = DaySixTasksCollected[i];
                sevenDaysStats.daySevenTasksCollected[i] = DaySevenTasksCollected[i];
                
                sevenDaysStats.dayOneTasksCompleted[i]   = DayOneTasksCompleted[i];
                sevenDaysStats.dayTwoTasksCompleted[i]   = DayTwoTasksCompleted[i];
                sevenDaysStats.dayThreeTasksCompleted[i] = DayThreeTasksCompleted[i];
                sevenDaysStats.dayFourTasksCompleted[i]  = DayFourTasksCompleted[i];
                sevenDaysStats.dayFiveTasksCompleted[i]  = DayFiveTasksCompleted[i];
                sevenDaysStats.daySixTasksCompleted[i]   = DaySixTasksCompleted[i];
                sevenDaysStats.daySevenTasksCompleted[i] = DaySevenTasksCompleted[i];
            }
        }

        public void InitFromStats(SevenDaysStats sevenDaysStats)
        {
            // Check for collection resizing
            CheckResize();

            // Init single variable
            //SevenDaysEventStart         = sevenDaysStats.sevenDaysEventStart;

            DiscountItemBoughtNums      = sevenDaysStats.discountItemBoughtNums;

            TaskProgress                = sevenDaysStats.taskProgress;
            TaskProgressList            = DeserializeTaskProgress(TaskProgressList);
            
            TaskCollected               = sevenDaysStats.taskCollected;
            TaskProgressList            = DeserializeTaskCollected(TaskProgressList);

            DayOneTasksAllCollected     = sevenDaysStats.dayOneTasksAllCollected;
            DayTwoTasksAllCollected     = sevenDaysStats.dayTwoTasksAllCollected;
            DayThreeTasksAllCollected   = sevenDaysStats.dayThreeTasksAllCollected;
            DayFourTasksAllCollected    = sevenDaysStats.dayFourTasksAllCollected;
            DayFiveTasksAllCollected    = sevenDaysStats.dayFiveTasksAllCollected;
            DaySixTasksAllCollected     = sevenDaysStats.daySixTasksAllCollected;
            DaySevenTasksAllCollected   = sevenDaysStats.daySevenTasksAllCollected;

            // Init multi-variables
            for(int i = 0; i < MAX_SUBCAT; ++i)
            {
                if(sevenDaysStats.dayOneTasksCollected[i] != null)
                {
                    DayOneTasksCollected[i]     = (int)sevenDaysStats.dayOneTasksCollected[i];
                }
                if (sevenDaysStats.dayTwoTasksCollected[i] != null)
                {
                    DayTwoTasksCollected[i]     = (int)sevenDaysStats.dayTwoTasksCollected[i];
                }
                if (sevenDaysStats.dayThreeTasksCollected[i] != null)
                {
                    DayThreeTasksCollected[i]   = (int)sevenDaysStats.dayThreeTasksCollected[i];
                }
                if (sevenDaysStats.dayFourTasksCollected[i] != null)
                {
                    DayFourTasksCollected[i]    = (int)sevenDaysStats.dayFourTasksCollected[i];
                }
                if (sevenDaysStats.dayFiveTasksCollected[i] != null)
                {
                    DayFiveTasksCollected[i]    = (int)sevenDaysStats.dayFiveTasksCollected[i];
                }
                if (sevenDaysStats.daySixTasksCollected[i] != null)
                {
                    DaySixTasksCollected[i]     = (int)sevenDaysStats.daySixTasksCollected[i];
                }
                if (sevenDaysStats.daySevenTasksCollected[i] != null)
                {
                    DaySevenTasksCollected[i]   = (int)sevenDaysStats.daySevenTasksCollected[i];
                }

                if (sevenDaysStats.dayOneTasksCompleted[i] != null)
                {
                    DayOneTasksCompleted[i] = (int)sevenDaysStats.dayOneTasksCompleted[i];
                }
                if (sevenDaysStats.dayTwoTasksCompleted[i] != null)
                {
                    DayTwoTasksCompleted[i] = (int)sevenDaysStats.dayTwoTasksCompleted[i];
                }
                if (sevenDaysStats.dayThreeTasksCompleted[i] != null)
                {
                    DayThreeTasksCompleted[i] = (int)sevenDaysStats.dayThreeTasksCompleted[i];
                }
                if (sevenDaysStats.dayFourTasksCompleted[i] != null)
                {
                    DayFourTasksCompleted[i] = (int)sevenDaysStats.dayFourTasksCompleted[i];
                }
                if (sevenDaysStats.dayFiveTasksCompleted[i] != null)
                {
                    DayFiveTasksCompleted[i] = (int)sevenDaysStats.dayFiveTasksCompleted[i];
                }
                if (sevenDaysStats.daySixTasksCompleted[i] != null)
                {
                    DaySixTasksCompleted[i] = (int)sevenDaysStats.daySixTasksCompleted[i];
                }
                if (sevenDaysStats.daySevenTasksCompleted[i] != null)
                {
                    DaySevenTasksCompleted[i] = (int)sevenDaysStats.daySevenTasksCompleted[i];
                }
            }
        }

        public void UpdateInventory(SevenDaysStats sevenDaysStats)
        {
            InitFromStats(sevenDaysStats);
        }

        public void SaveToInventory(SevenDaysStats sevenDaysStats)
        {
            InitFromStats(sevenDaysStats);
        }

        public void UpdateSevenDaysStatus(string sevenDaysStatus)
        {
            //List<string> statusRaw = sevenDaysStatus.Split('|').ToList();
            //int sevenDaysRunning = 0;
            //int.TryParse(statusRaw[0], out sevenDaysRunning);
            //IsEventRunning = sevenDaysRunning == 1 ? true : false;

            //int sevenDaysEventPeriod = 0;
            //int.TryParse(statusRaw[1], out sevenDaysEventPeriod);
            //IsEventPeriod = sevenDaysEventPeriod == 1 ? true : false;

            //int sevenDaysCollectPeriod = 0;
            //int.TryParse(statusRaw[2], out sevenDaysCollectPeriod);
            //IsCollectionPeriod = sevenDaysCollectPeriod == 1 ? true : false;

            //long serverStartDT = 0;
            //long.TryParse(statusRaw[3], out serverStartDT);
            //SevenDaysEventStart = serverStartDT;
        }

        public void UpdateSevenDaysStart(long sevenDaysStart)
        {
            SevenDaysEventStart = sevenDaysStart;
        }

        public bool IsSevenDaysRunning()
        {
            return IsSevenDaysEventPeriod() || IsSevenDaysCollectionPeriod();
        }

        public bool IsSevenDaysEventPeriod()
        {
            DateTime start = GetSevenDaysServerStartDT();

            int maxDays = (int)Days.NUM_DAYS;
            DateTime endDT = start.AddDays(maxDays);

            return DateTime.Now >= start && DateTime.Now < endDT;
        }

        public bool IsSevenDaysCollectionPeriod()
        {
            DateTime start = GetSevenDaysServerStartDT();
            DateTime endDT = start.AddDays(10);

            return !IsSevenDaysEventPeriod() && DateTime.Now < endDT;
        }

        public DateTime GetSevenDaysServerStartDT()
        {
            return new DateTime(SevenDaysEventStart);
        }

        public BuyDiscItmRetval BuyDiscountItem(int day, int amount)
        {
            BuyDiscItmRetval retval = new BuyDiscItmRetval(BuyDiscItmCode.BuyFailed, "");

            if (string.IsNullOrEmpty(DiscountItemBoughtNums))
            {
                return retval;
            }

            List<string> numsList = DiscountItemBoughtNums.Split('|').ToList();

            if (day < 0 || day >= numsList.Count)
            {
                return retval;
            }

            int num = -1;
            bool res = int.TryParse(numsList[day], out num);

            if (res == false)
            {
                return retval;
            }

            numsList[day] = (num + amount).ToString();

            string buyStr = SerializeStringList(numsList);
            DiscountItemBoughtNums = buyStr;

            retval.mBuyCode = BuyDiscItmCode.BuySuccess;
            retval.mBuyStr = buyStr;

            return retval;
        }

        public int GetDiscountItemBoughtNums(int day)
        {
            if(string.IsNullOrEmpty(DiscountItemBoughtNums))
            {
                return -1;
            }

            List<string> numsList = DiscountItemBoughtNums.Split('|').ToList();

            if(day < 0 || day >= numsList.Count)
            {
                return -1;
            }

            int num = -1;
            int.TryParse(numsList[day], out num);

            return num;
        }

        public int GetTaskProgress(NewServerActivityJson taskData)
        {
            SevenDaysTask task = TaskProgressList.Find(o => o.ID() == taskData.id);

            return task != null ? task.Progress() : -1;
        }

        public bool CollectTaskReward(Days day, int subcat, int taskId)
        {
            List<int> tasksList = GetDayTasksCollected(day);
            int taskCount = SevenDaysRepo.GetNewServerActivityTasksCount(day, subcat);
            if(tasksList != null && (taskId > taskCount || subcat > tasksList.Count))
            {
                return false;
            }

            tasksList[subcat] = GameUtils.SetBit(tasksList[subcat], taskId);
            SetDayTasksCollected(day, subcat, tasksList);

            if(CheckAllTaskRewardCollected(tasksList, day) == true)
            {
                SetTaskRewardAllCollectedByDay(day);
            }

            return true;
        }

        public bool IsTaskRewardCollected(Days day, int subcat, int taskId)
        {
            List<int> tasksList = GetDayTasksCollected(day);
            int taskCount = SevenDaysRepo.GetNewServerActivityTasksCount(day, subcat);
            if(tasksList != null && (taskId > taskCount || subcat > tasksList.Count))
            {
                return false;
            }

            return GameUtils.IsBitSet(tasksList[subcat], taskId);
        }

        public bool IsTaskRewardAllCollected(Days day)
        {
            return GetTaskRewardAllCollectedByDay(day);
        }

        public bool CompleteTask(Days day, int subcat, int taskId)
        {
            List<int> completionList = GetDayTasksCompleted(day);
            int taskCount = SevenDaysRepo.GetNewServerActivityTasksCount(day, subcat);
            if (completionList != null && (taskId > taskCount || subcat > completionList.Count))
            {
                return false;
            }

            completionList[subcat] = GameUtils.SetBit(completionList[subcat], taskId);
            SetDayTasksCompleted(day, subcat, completionList);

            return true;
        }

        public bool IsTaskCompleted(Days day, int subcat, int taskId)
        {
            List<int> completionList = GetDayTasksCompleted(day);
            int taskCount = SevenDaysRepo.GetNewServerActivityTasksCount(day, subcat);
            if (completionList != null && (taskId > taskCount || subcat > completionList.Count))
            {
                return false;
            }

            return GameUtils.IsBitSet(completionList[subcat], taskId);
        }

        private bool CheckAllTaskRewardCollected(List<int> tasksList, Days day)
        {
            int subcatCount = SevenDaysRepo.GetSubCategoryIDsByDay(day).Count;
            for(int i = 0; i < subcatCount; ++i)
            {
                int taskCount = SevenDaysRepo.GetNewServerActivityTasksCount(day, i);
                int doneCount = (int)Math.Pow(2, taskCount) - 1;

                if(tasksList[i] != doneCount)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsTaskCompletedClient(int taskid)
        {
            SevenDaysTask task = TaskProgressList.Find(o => o.ID() == taskid);
            if (task != null)
            {
                return task.IsComplete();
            }

            return false;
        }

        public bool IsTaskCollectedClient(int taskid)
        {
            SevenDaysTask task = TaskProgressList.Find(o => o.ID() == taskid);
            if (task != null)
            {
                return task.IsCollected();
            }

            return false;
        }

        private List<int> GetDayTasksCollected(Days day)
        {
            List<int> taskList = null;
            switch (day)
            {
                case Days.One:
                    taskList = DayOneTasksCollected;
                    break;
                case Days.Two:
                    taskList = DayTwoTasksCollected;
                    break;
                case Days.Three:
                    taskList = DayThreeTasksCollected;
                    break;
                case Days.Four:
                    taskList = DayFourTasksCollected;
                    break;
                case Days.Five:
                    taskList = DayFiveTasksCollected;
                    break;
                case Days.Six:
                    taskList = DaySixTasksCollected;
                    break;
                case Days.Seven:
                    taskList = DaySevenTasksCollected;
                    break;
                default:
                    taskList = null;
                    break;
            }

            return taskList;
        }

        private void SetDayTasksCollected(Days day, int subcat, List<int> tasksList)
        {
            switch (day)
            {
                case Days.One:
                    DayOneTasksCollected[subcat] = tasksList[subcat];
                    break;
                case Days.Two:
                    DayTwoTasksCollected[subcat] = tasksList[subcat];
                    break;
                case Days.Three:
                    DayThreeTasksCollected[subcat] = tasksList[subcat];
                    break;
                case Days.Four:
                    DayFourTasksCollected[subcat] = tasksList[subcat];
                    break;
                case Days.Five:
                    DayFiveTasksCollected[subcat] = tasksList[subcat];
                    break;
                case Days.Six:
                    DaySixTasksCollected[subcat] = tasksList[subcat];
                    break;
                case Days.Seven:
                    DaySevenTasksCollected[subcat] = tasksList[subcat];
                    break;
            }
        }

        private void SetTaskRewardAllCollectedByDay(Days day)
        {
            switch(day)
            {
                case Days.One:
                     DayOneTasksAllCollected = true;
                    break;
                case Days.Two:
                    DayTwoTasksAllCollected = true;
                    break;
                case Days.Three:
                    DayThreeTasksAllCollected = true;
                    break;
                case Days.Four:
                    DayFourTasksAllCollected = true;
                    break;
                case Days.Five:
                    DayFiveTasksAllCollected = true;
                    break;
                case Days.Six:
                    DaySixTasksAllCollected = true;
                    break;
                case Days.Seven:
                    DaySevenTasksAllCollected = true;
                    break;
            }
        }

        private bool GetTaskRewardAllCollectedByDay(Days day)
        {
            switch (day)
            {
                case Days.One:
                    return DayOneTasksAllCollected;
                case Days.Two:
                    return DayTwoTasksAllCollected;
                case Days.Three:
                    return DayThreeTasksAllCollected;
                case Days.Four:
                    return DayFourTasksAllCollected;
                case Days.Five:
                    return DayFiveTasksAllCollected;
                case Days.Six:
                    return DaySixTasksAllCollected;
                case Days.Seven:
                    return DaySevenTasksAllCollected;
                default:
                    return false;
            }
        }

        private List<int> GetDayTasksCompleted(Days day)
        {
            CheckResize();

            List<int> completionList = null;
            switch (day)
            {
                case Days.One:
                    completionList = DayOneTasksCompleted;
                    break;
                case Days.Two:
                    completionList = DayTwoTasksCompleted;
                    break;
                case Days.Three:
                    completionList = DayThreeTasksCompleted;
                    break;
                case Days.Four:
                    completionList = DayFourTasksCompleted;
                    break;
                case Days.Five:
                    completionList = DayFiveTasksCompleted;
                    break;
                case Days.Six:
                    completionList = DaySixTasksCompleted;
                    break;
                case Days.Seven:
                    completionList = DaySevenTasksCompleted;
                    break;
                default:
                    completionList = null;
                    break;
            }

            return completionList;
        }

        private void SetDayTasksCompleted(Days day, int subcat, List<int> tasksList)
        {
            CheckResize();

            switch (day)
            {
                case Days.One:
                    DayOneTasksCompleted[subcat] = tasksList[subcat];
                    break;
                case Days.Two:
                    DayTwoTasksCompleted[subcat] = tasksList[subcat];
                    break;
                case Days.Three:
                    DayThreeTasksCompleted[subcat] = tasksList[subcat];
                    break;
                case Days.Four:
                    DayFourTasksCompleted[subcat] = tasksList[subcat];
                    break;
                case Days.Five:
                    DayFiveTasksCompleted[subcat] = tasksList[subcat];
                    break;
                case Days.Six:
                    DaySixTasksCompleted[subcat] = tasksList[subcat];
                    break;
                case Days.Seven:
                    DaySevenTasksCompleted[subcat] = tasksList[subcat];
                    break;
            }
        }

        public void SetTaskProgress(string taskProgressStr)
        {
            TaskProgress = taskProgressStr;
        }

        public void SetTaskCollected(string taskCollectedStr)
        {
            TaskCollected = taskCollectedStr;
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

        public List<SevenDaysTask> InitTaskProgressList()
        {
            List<SevenDaysTask> taskProgress = new List<SevenDaysTask>();
            
            int maxDays = (int)Days.NUM_DAYS;
            for (int i = 0; i < maxDays; ++i)
            {
                for (int j = 0; j < MAX_SUBCAT; ++j)
                {
                    List<NewServerActivityJson> currentTaskList = SevenDaysRepo.GetNewServerActivityList((Days)i, j);
                    if (currentTaskList != null)
                    {
                        for (int t = 0; t < currentTaskList.Count; ++t)
                        {
                            NewServerActivityJson taskData = currentTaskList[t];
                            if (taskData == null)
                            {
                                continue;
                            }

                            int prog = taskData.type == NewServerActivityType.Militantrank ? 500 : 0;
                            SevenDaysTask task = new SevenDaysTask(taskData.id, taskData.type, prog, taskData.count1, taskData.count2);
                            taskProgress.Add(task);
                        }
                    }
                }
            }

            return taskProgress;
        }

        public string SerializeTaskProgress(List<SevenDaysTask> taskProgress)
        {
            StringBuilder taskStr = new StringBuilder();

            for(int i = 0; i < taskProgress.Count; ++i)
            {
                SevenDaysTask task = taskProgress[i];
                
                if(task.Progress() > 0)
                {
                    if(task.Count2() == 0)
                    {
                        taskStr.Append((int)task.Type());
                        taskStr.Append(";");
                        taskStr.Append(task.Progress());
                        taskStr.Append("|");
                    }
                    else
                    {
                        taskStr.Append((int)task.Type());
                        taskStr.Append(";");
                        taskStr.Append(task.Count1());
                        taskStr.Append(";");
                        taskStr.Append(task.Count2());
                        taskStr.Append(";");
                        taskStr.Append(task.Progress());
                        taskStr.Append("|");
                    }
                }
            }

            string tasks = taskStr.ToString();
            tasks = tasks.TrimEnd('|');

            return tasks;
        }

        public List<SevenDaysTask> DeserializeTaskProgress(List<SevenDaysTask> taskProgress)
        {
            if(!(string.IsNullOrEmpty(TaskProgress) || TaskProgress == "0"))
            {
                List<string> taskTypeStrList = TaskProgress.Split('|').ToList();

                for(int i = 0; i < taskTypeStrList.Count; ++i)
                {
                    List<string> taskStrList = taskTypeStrList[i].Split(';').ToList();
                    int serializeCount = taskStrList.Count;

                    if(serializeCount != 2 && serializeCount != 4)
                    {
                        return taskProgress;
                    }

                    for(int j = 0; j < taskStrList.Count; j += serializeCount)
                    {
                        int type    = 0;
                        int count1  = 0;
                        int count2  = 0;
                        int prog    = 0;

                        if(serializeCount == 2)
                        {
                            if(int.TryParse(taskStrList[j], out type) == false ||
                                int.TryParse(taskStrList[j + 1], out prog) == false)
                            {
                                continue;
                            }

                            //int taskPos = taskProgress.FindIndex(o => o.Type() == (NewServerActivityType)type);

                            //if(taskPos != -1)
                            //{
                            //    taskProgress[taskPos].SetProgress(prog);
                            //}

                            List<SevenDaysTask> taskProgressType = taskProgress.FindAll(o => o.Type() == (NewServerActivityType)type);

                            if(taskProgressType != null)
                            {
                                for(int t = 0; t < taskProgressType.Count; ++t)
                                {
                                    taskProgressType[t].SetProgress(prog);
                                }
                            }
                        }
                        else if(serializeCount == 4)
                        {
                            if(int.TryParse(taskStrList[j], out type) == false ||
                                int.TryParse(taskStrList[j + 1], out count1) == false ||
                                int.TryParse(taskStrList[j + 2], out count2) == false ||
                                int.TryParse(taskStrList[j + 3], out prog) == false)
                            {
                                continue;
                            }

                            int taskPos = taskProgress.FindIndex(o => o.Type() == (NewServerActivityType)type &&
                            o.Count1() == count1 && o.Count2() == count2);

                            if(taskPos != -1)
                            {
                                taskProgress[taskPos].SetProgress(prog);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < taskProgress.Count; ++i)
                {
                    taskProgress[i].Reset();
                }
            }

            return taskProgress;
        }

        public string SerializeTaskCollected(List<SevenDaysTask> taskProgress)
        {
            StringBuilder taskCollectedStr = new StringBuilder();

            for (int i = 0; i < taskProgress.Count; ++i)
            {
                SevenDaysTask task = taskProgress[i];

                if(task.IsCollected() == true)
                {
                    if(task.Count2() == 0)
                    {
                        taskCollectedStr.Append((int)task.Type());
                        taskCollectedStr.Append(";");
                        taskCollectedStr.Append(task.Count1());
                        taskCollectedStr.Append("|");
                    }
                    else
                    {
                        taskCollectedStr.Append((int)task.Type());
                        taskCollectedStr.Append(";");
                        taskCollectedStr.Append(task.Count1());
                        taskCollectedStr.Append(";");
                        taskCollectedStr.Append(task.Count2());
                        taskCollectedStr.Append("|");
                    }
                }
            }

            string tasksCollected = taskCollectedStr.ToString();
            tasksCollected = tasksCollected.TrimEnd('|');

            return tasksCollected;
        }

        public List<SevenDaysTask> DeserializeTaskCollected(List<SevenDaysTask> taskProgress)
        {
            if (!(string.IsNullOrEmpty(TaskCollected) || TaskCollected == "0"))
            {
                List<string> taskCollectedStrList = TaskCollected.Split('|').ToList();
                
                for (int i = 0; i < taskCollectedStrList.Count; ++i)
                {
                    List<string> taskStrList = taskCollectedStrList[i].Split(';').ToList();
                    int serializeCount = taskStrList.Count;

                    if(serializeCount < 2 || serializeCount > 3)
                    {
                        return taskProgress;
                    }

                    for (int j = 0; j < taskStrList.Count; j += serializeCount)
                    {
                        int type = 0;
                        int count1 = 0;
                        int count2 = 0;

                        if(serializeCount == 2)
                        {
                            if(int.TryParse(taskStrList[j], out type) == false ||
                                int.TryParse(taskStrList[j + 1], out count1) == false)
                            {
                                continue;
                            }

                            int taskPos = taskProgress.FindIndex(o => o.Type() == (NewServerActivityType)type);

                            if(taskPos != -1)
                            {
                                taskProgress[taskPos].SetCollected(true);
                            }
                        }
                        else if(serializeCount == 3)
                        {
                            if(int.TryParse(taskStrList[j], out type) == false ||
                                int.TryParse(taskStrList[j + 1], out count1) == false ||
                                int.TryParse(taskStrList[j + 2], out count2) == false)
                            {
                                continue;
                            }

                            int taskPos = taskProgress.FindIndex(o => o.Type() == (NewServerActivityType)type &&
                            o.Count1() == count1 && o.Count2() == count2);

                            if(taskPos != -1)
                            {
                                taskProgress[taskPos].SetCollected(true);
                            }
                        }
                    }
                }
            }
            else
            {
                for(int i = 0; i < taskProgress.Count; ++i)
                {
                    taskProgress[i].ResetCollected();
                }
            }

            return taskProgress;
        }

        private void CheckResize()
        {
            // Task collection
            if (DayOneTasksCollected != null && DayOneTasksCollected.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DayOneTasksCollected, DayOneTasksCollected, MAX_SUBCAT);
            }
            else if(DayOneTasksCollected == null)
            {
                DayOneTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            if (DayTwoTasksCollected != null && DayTwoTasksCollected.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DayTwoTasksCollected, DayTwoTasksCollected, MAX_SUBCAT);
            }
            else if (DayTwoTasksCollected == null)
            {
                DayTwoTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            if (DayThreeTasksCollected != null && DayThreeTasksCollected.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DayThreeTasksCollected, DayThreeTasksCollected, MAX_SUBCAT);
            }
            else if (DayThreeTasksCollected == null)
            {
                DayThreeTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            if (DayFourTasksCollected != null && DayFourTasksCollected.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DayFourTasksCollected, DayFourTasksCollected, MAX_SUBCAT);
            }
            else if (DayFourTasksCollected == null)
            {
                DayFourTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            if (DayFiveTasksCollected != null && DayFiveTasksCollected.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DayFiveTasksCollected, DayFiveTasksCollected, MAX_SUBCAT);
            }
            else if (DayFiveTasksCollected == null)
            {
                DayFiveTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            if (DaySixTasksCollected != null && DaySixTasksCollected.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DaySixTasksCollected, DaySixTasksCollected, MAX_SUBCAT);
            }
            else if (DaySixTasksCollected == null)
            {
                DaySixTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            if (DaySevenTasksCollected != null && DaySevenTasksCollected.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DaySevenTasksCollected, DaySevenTasksCollected, MAX_SUBCAT);
            }
            else if (DaySevenTasksCollected == null)
            {
                DaySevenTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }

            // Task completion
            if (DayOneTasksCompleted != null && DayOneTasksCompleted.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DayOneTasksCompleted, DayOneTasksCompleted, MAX_SUBCAT);
            }
            else if (DayOneTasksCompleted == null)
            {
                DayOneTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }
            if (DayTwoTasksCompleted != null && DayTwoTasksCompleted.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DayTwoTasksCompleted, DayTwoTasksCompleted, MAX_SUBCAT);
            }
            else if (DayTwoTasksCompleted == null)
            {
                DayTwoTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }
            if (DayThreeTasksCompleted != null && DayThreeTasksCompleted.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DayThreeTasksCompleted, DayThreeTasksCompleted, MAX_SUBCAT);
            }
            else if (DayThreeTasksCompleted == null)
            {
                DayThreeTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }
            if (DayFourTasksCompleted != null && DayFourTasksCompleted.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DayFourTasksCompleted, DayFourTasksCompleted, MAX_SUBCAT);
            }
            else if (DayFourTasksCompleted == null)
            {
                DayFourTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }
            if (DayFiveTasksCompleted != null && DayFiveTasksCompleted.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DayFiveTasksCompleted, DayFiveTasksCompleted, MAX_SUBCAT);
            }
            else if (DayFiveTasksCompleted == null)
            {
                DayFiveTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }
            if (DaySixTasksCompleted != null && DaySixTasksCompleted.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DaySixTasksCompleted, DaySixTasksCompleted, MAX_SUBCAT);
            }
            else if (DaySixTasksCompleted == null)
            {
                DaySixTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }
            if (DaySevenTasksCompleted != null && DaySevenTasksCompleted.Count != MAX_SUBCAT)
            {
                ResizeInventoryList(out DaySevenTasksCompleted, DaySevenTasksCompleted, MAX_SUBCAT);
            }
            else if (DaySevenTasksCompleted == null)
            {
                DaySevenTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }
        }

        private void ResizeInventoryList(out List<int> newDataList, List<int> oldDataList, int newSize)
        {
            newDataList = new List<int>(new int[newSize]);
            int oldSize = oldDataList.Count;

            if(oldSize == newSize)
            {
                return;
            }

            if(oldSize > newSize)
            {
                for (int i = 0; i < newSize; ++i)
                {
                    newDataList[i] = oldDataList[i];
                }
            }
            else
            {
                for (int i = 0; i < oldSize; ++i)
                {
                    newDataList[i] = oldDataList[i];
                }
            }
        }

        private void InitTaskProgressData()
        {
            TaskProgress = "0";
            TaskProgressList = InitTaskProgressList();
            TaskProgressList = DeserializeTaskProgress(TaskProgressList);
            
            TaskCollected = "0";
            TaskProgressList = DeserializeTaskCollected(TaskProgressList);
        }
    }
}
