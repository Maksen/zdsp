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
    public partial class QuestExtraRewardsInvData
    {
        public void InitFromInventory(QuestExtraRewardsStats questExtraRewardsStats)
        {
            // Init single variables
            questExtraRewardsStats.taskProgress         = TaskProgress;

            questExtraRewardsStats.taskCompletion       = TaskCompletion;
            questExtraRewardsStats.taskCompleted        = TaskCompleted;
            questExtraRewardsStats.taskCollection       = TaskCollection;
            questExtraRewardsStats.taskCollected        = TaskCollected;
            questExtraRewardsStats.boxRewardCollection  = BoxRewardsCollection;
            questExtraRewardsStats.boxRewardCollected   = BoxRewardsCollected;
            questExtraRewardsStats.activePts            = ActivePoints;
        }

        public void InitFromStats(QuestExtraRewardsStats questExtraRewardsStats)
        {
            // Init single variable
            TaskProgress            = questExtraRewardsStats.taskProgress;
            taskProgressList        = DeserializeTaskProgress(taskProgressList);

            TaskCompletion          = questExtraRewardsStats.taskCompletion;
            TaskCompleted           = questExtraRewardsStats.taskCompleted;

            TaskCollection          = questExtraRewardsStats.taskCollection;
            TaskCollected           = questExtraRewardsStats.taskCollected;
            taskProgressList        = DeserializeTaskCollected(taskProgressList);

            BoxRewardsCollection    = questExtraRewardsStats.boxRewardCollection;
            BoxRewardsCollected     = questExtraRewardsStats.boxRewardCollected;
            boxRewardsList          = DeserializeBoxRewards(boxRewardsList);

            ActivePoints            = questExtraRewardsStats.activePts;
        }

        public void UpdateInventory(QuestExtraRewardsStats questExtraRewardsStats)
        {
            InitFromStats(questExtraRewardsStats);
        }

        public void SaveToInventory(QuestExtraRewardsStats questExtraRewardsStats)
        {
            InitFromStats(questExtraRewardsStats);
        }

        public List<QERewardsTask> InitTaskProgressList()
        {
            List<QERewardsTask> taskProgress = new List<QERewardsTask>();

            QETaskDataList taskList = QuestExtraRewardsRepo.GetTaskList();

            for (int i = 0; i < taskList.Count(); ++i)
            {
                QETaskData taskData = taskList.GetByIdx(i);
                if (taskData == null)
                {
                    continue;
                }

                QERewardsTask task = new QERewardsTask(taskData.TaskID(), taskData.TaskType(), 0, taskData.TaskCount());
                taskProgress.Add(task);
            }

            return taskProgress;
        }

        public string SerializeTaskProgress(List<QERewardsTask> taskProgress)
        {
            StringBuilder taskStr = new StringBuilder();

            for(int i = 0; i < taskProgress.Count; ++i)
            {
                QERewardsTask task = taskProgress[i];

                if(task.mProgress > 0)
                {
                    taskStr.Append(task.mId);
                    taskStr.Append(";");
                    taskStr.Append(task.mProgress);
                    taskStr.Append("|");
                }
            }

            string tasks = taskStr.ToString();
            tasks = tasks.TrimEnd('|');

            return tasks;
        }

        public string SerializeTaskCollected(List<QERewardsTask> taskProgress)
        {
            StringBuilder taskCollectedStr = new StringBuilder();

            for (int i = 0; i < taskProgress.Count; ++i)
            {
                QERewardsTask task = taskProgress[i];

                if(task.mIsCollected == true)
                {
                    taskCollectedStr.Append(task.mId);
                    taskCollectedStr.Append(";");
                    int collected = task.mIsCollected ? 1 : 0;
                    taskCollectedStr.Append(collected);
                    taskCollectedStr.Append("|");
                }
            }

            string tasksCollected = taskCollectedStr.ToString();
            tasksCollected = tasksCollected.TrimEnd('|');

            return tasksCollected;
        }

        public List<QERewardsTask> DeserializeTaskProgress(List<QERewardsTask> taskProgress)
        {
            if (!(string.IsNullOrEmpty(TaskProgress) || TaskProgress == "0"))
            {
                List<string> taskTypeStrList = TaskProgress.Split('|').ToList();
                int serializeCount = 2;

                for (int i = 0; i < taskTypeStrList.Count; ++i)
                {
                    List<string> taskStrList = taskTypeStrList[i].Split(';').ToList();
                    if (taskStrList.Count % serializeCount != 0)
                    {
                        return taskProgress;
                    }

                    for (int j = 0; j < taskStrList.Count; j += serializeCount)
                    {
                        int id = 0;
                        int prog = 0;

                        if (int.TryParse(taskStrList[j], out id) == false ||
                            int.TryParse(taskStrList[j + 1], out prog) == false)
                        {
                            continue;
                        }

                        int taskPos = taskProgress.FindIndex(o => o.mId == id);

                        if(taskPos != -1)
                        {
                            taskProgress[taskPos].mProgress = prog;
                        }
                    }
                }
            }
            else
            {
                for(int i = 0; i < taskProgress.Count; ++i)
                {
                    taskProgress[i].Reset();
                }
            }

            return taskProgress;
        }

        public List<QERewardsTask> DeserializeTaskCollected(List<QERewardsTask> taskProgress)
        {
            if (!(string.IsNullOrEmpty(TaskCollected) || TaskCollected == "0"))
            {
                List<string> taskCollectedStrList = TaskCollected.Split('|').ToList();
                int serializeCount = 2;

                for (int i = 0; i < taskCollectedStrList.Count; ++i)
                {
                    List<string> taskStrList = taskCollectedStrList[i].Split(';').ToList();
                    if (taskStrList.Count % serializeCount != 0)
                    {
                        return taskProgress;
                    }

                    for (int j = 0; j < taskStrList.Count; j += serializeCount)
                    {
                        int id = 0;
                        int collected = 0;

                        if (int.TryParse(taskStrList[j], out id) == false ||
                            int.TryParse(taskStrList[j + 1], out collected) == false)
                        {
                            continue;
                        }

                        int taskPos = taskProgress.FindIndex(o => o.mId == id);

                        if (taskPos != -1)
                        {
                            taskProgress[taskPos].mIsCollected = collected == 1 ? true : false;
                        }
                    }
                }
            }

            return taskProgress;
        }

        public void SetTaskProgress(string taskProgressStr)
        {
            TaskProgress = taskProgressStr;
        }

        public void SetTaskCollected(string taskCollectedStr)
        {
            TaskCollected = taskCollectedStr;
        }

        public void SetBoxRewardsCollected(string boxRewardsCollectedStr)
        {
            BoxRewardsCollected = boxRewardsCollectedStr;
        }

        public void SetTaskComplete(int dataid)
        {
            taskProgressList[dataid].mProgress = taskProgressList[dataid].mRequired;
        }

        public string GetTaskProgress()
        {
            return TaskProgress;
        }

        public int GetTaskProgress(int taskid)
        {
            QERewardsTask task = taskProgressList.Find(o => o.mId == taskid);
            if (task != null)
            {
                return task.mProgress;
            }

            return 0;
        }

        public void CompleteTask(int dataid)
        {
            TaskCompletion = GameUtils.SetBit(TaskCompletion, dataid);
        }

        public void CompleteTaskByTaskID(int taskid)
        {
            QETaskDataList taskDataList = QuestExtraRewardsRepo.GetTaskList();
            int dataid = taskDataList.GetIdxByTaskID(taskid);
            if(dataid > -1)
            {
                CompleteTask(dataid);
            }
        }

        public bool IsTaskCompletedClient(int taskid)
        {
            QERewardsTask task = taskProgressList.Find(o => o.mId == taskid);
            if (task != null)
            {
                return task.IsComplete();
            }

            return false;
        }

        public bool IsTaskCompletedClient(List<int> taskids)
        {
            for(int i = 0; i < taskids.Count; ++i)
            {
                int taskid = taskids[i];
                if(IsTaskCompletedClient(taskid) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsTaskAllCompleted()
        {
            QETaskDataList taskDataList = QuestExtraRewardsRepo.GetTaskList();
            for (int i = 0; i < taskDataList.Count(); ++i)
            {
                bool isCompleted = GameUtils.IsBitSet(TaskCompletion, i);
                if (!isCompleted)
                {
                    return false;
                }
            }

            return true;
        }

        public void CollectTask(int dataid)
        {
            TaskCollection = GameUtils.SetBit(TaskCollection, dataid);
        }

        public bool IsTaskCollectedClient(int taskid)
        {
            QERewardsTask task = taskProgressList.Find(o => o.mId == taskid);
            if (task != null)
            {
                return task.mIsCollected;
            }

            return false;
        }

        public bool IsTaskCollectedClient(List<int> taskids)
        {
            for(int i = 0; i < taskids.Count; ++i)
            {
                int taskid = taskids[i];
                if(IsTaskCollectedClient(taskid) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsTaskAllCollected()
        {
            QETaskDataList taskDataList = QuestExtraRewardsRepo.GetTaskList();
            for (int i = 0; i < taskDataList.Count(); ++i)
            {
                bool isCollected = GameUtils.IsBitSet(TaskCollection, i);
                if (!isCollected)
                {
                    return false;
                }
            }

            return true;
        }

        //public void CollectBoxReward(int dataid)
        //{
        //    BoxRewardsCollection = InventoryUtils.SetBitwiseBool(BoxRewardsCollection, dataid, true);
        //}

        public bool IsBoxRewardCollectableClient(int boxNum, int currPts)
        {
            QERBoxReward reward = boxRewardsList.Find(o => o.mBoxNum == boxNum);
            if (reward != null)
            {
                return reward.IsCollectable(currPts);
            }

            return false;
        }

        public bool IsBoxRewardCollectedClient(int boxNum)
        {
            QERBoxReward reward = boxRewardsList.Find(o => o.mBoxNum == boxNum);
            if (reward != null)
            {
                return reward.mIsCollected;
            }

            return false;
        }

        public bool IsBoxRewardAllCollectedClient()
        {
            for(int i = 0; i < boxRewardsList.Count; ++i)
            {
                if(boxRewardsList[i].mIsCollected == false)
                {
                    return false;
                }
            }

            return true;
        }

        public List<QERBoxReward> InitBoxRewardsList()
        {
            List<QERBoxReward> boxRewards = new List<QERBoxReward>();

            QERewardDataList rewardsList = QuestExtraRewardsRepo.GetRewardList();

            for (int i = 0; i < rewardsList.Count(); ++i)
            {
                QERewardData rewardData = rewardsList.GetByIdx(i);
                if (rewardData == null)
                {
                    continue;
                }

                QERBoxReward boxReward = new QERBoxReward(rewardData.BoxID(), rewardData.ReqPoints());
                boxRewards.Add(boxReward);
            }

            return boxRewards;
        }

        public string SerializeBoxRewards(List<QERBoxReward> boxRewards)
        {
            StringBuilder rewardStr = new StringBuilder();

            for (int i = 0; i < boxRewards.Count; ++i)
            {
                QERBoxReward reward = boxRewards[i];

                if (reward.mIsCollected == true)
                {
                    rewardStr.Append(reward.mBoxNum);
                    rewardStr.Append(";");
                    int collected = reward.mIsCollected ? 1 : 0;
                    rewardStr.Append(collected);
                    rewardStr.Append("|");
                }
            }

            string rewards = rewardStr.ToString();
            rewards = rewards.TrimEnd('|');

            return rewards;
        }

        public List<QERBoxReward> DeserializeBoxRewards(List<QERBoxReward> boxRewards)
        {
            if (!(string.IsNullOrEmpty(BoxRewardsCollected) || BoxRewardsCollected == "0"))
            {
                List<string> boxRewardsStrList = BoxRewardsCollected.Split('|').ToList();
                int serializeCount = 2;

                for (int i = 0; i < boxRewardsStrList.Count; ++i)
                {
                    List<string> rewardDataStrList = boxRewardsStrList[i].Split(';').ToList();
                    if (rewardDataStrList.Count % serializeCount != 0)
                    {
                        return boxRewards;
                    }

                    for (int j = 0; j < rewardDataStrList.Count; j += serializeCount)
                    {
                        int boxNum = 0;
                        int collected = 0;

                        if (int.TryParse(rewardDataStrList[j], out boxNum) == false ||
                            int.TryParse(rewardDataStrList[j + 1], out collected) == false)
                        {
                            continue;
                        }

                        int rewardPos = boxRewards.FindIndex(o => o.mBoxNum == boxNum);

                        if (rewardPos != -1)
                        {
                            boxRewards[rewardPos].mIsCollected = collected == 1 ? true : false;
                        }
                    }
                }
            }
            else
            {
                for(int i = 0; i < boxRewards.Count; ++i)
                {
                    QERBoxReward reward = boxRewards[i];
                    if(reward != null)
                    {
                        reward.Reset();
                    }
                }
            }

            return boxRewards;
        }

        public void SetActivePoints(int amount)
        {
            ActivePoints = amount;
        }

        public void AddActivePoints(int amount)
        {
            ActivePoints += amount;
        }

        public int GetActivePoints()
        {
            return ActivePoints;
        }

        // New Day Reset
        public void NewDayReset()
        {
            InitTaskProgressData();
            TaskCompletion          = 0;
            TaskCompleted           = "0";
            TaskCollection          = 0;
            TaskCollected           = "0";
            InitBoxRewardsData();
            ActivePoints            = 0;
        }

        private void InitTaskProgressData()
        {
            //StringBuilder taskStr = new StringBuilder();

            //QETaskDataList taskList = QuestExtraRewardsRepo.GetTaskList();
            //for(int i = 0; i < taskList.Count(); ++i)
            //{
            //    QETaskData taskData = taskList.GetByIdx(i);
            //    if(taskData == null)
            //    {
            //        continue;
            //    }

            //    taskStr.Append(taskData.TaskID());
            //    taskStr.Append(";");
            //    taskStr.Append(0);

            //    if (i < (taskList.Count() - 1))
            //    {
            //        taskStr.Append("|");
            //    }
            //}

            //TaskProgress = taskStr.ToString();
            TaskProgress = "0";
            taskProgressList = InitTaskProgressList();
            taskProgressList = DeserializeTaskProgress(taskProgressList);
        }

        private void InitBoxRewardsData()
        {
            BoxRewardsCollected = "0";
            boxRewardsList = InitBoxRewardsList();
            boxRewardsList = DeserializeBoxRewards(boxRewardsList);
        }
    }
}