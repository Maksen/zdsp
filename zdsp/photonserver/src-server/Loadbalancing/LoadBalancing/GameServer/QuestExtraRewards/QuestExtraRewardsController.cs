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
    public class QuestExtraRewardsController
    {
        // Player Slot
        private GameClientPeer _peer;
        
        private List<QERewardsTask> taskProgress;
        private List<QERBoxReward> boxRewards;

        public QuestExtraRewardsController(GameClientPeer peer)
        {
            _peer = peer;
            //taskProgress = new List<QERewardsTask>();
            //boxRewardCollection = new List<QERBoxReward>();
        }

        public void InitTaskProgress()
        {
            taskProgress = _peer.CharacterData.QuestExtraRewardsInventory.InitTaskProgressList();
            taskProgress = _peer.CharacterData.QuestExtraRewardsInventory.DeserializeTaskProgress(taskProgress);
            taskProgress = _peer.CharacterData.QuestExtraRewardsInventory.DeserializeTaskCollected(taskProgress);

            string taskProgressStr = _peer.CharacterData.QuestExtraRewardsInventory.SerializeTaskProgress(taskProgress);
            _peer.CharacterData.QuestExtraRewardsInventory.SetTaskProgress(taskProgressStr);

            string taskCollectedStr = _peer.CharacterData.QuestExtraRewardsInventory.SerializeTaskCollected(taskProgress);
            _peer.CharacterData.QuestExtraRewardsInventory.SetTaskCollected(taskCollectedStr);
        }

        public void InitBoxRewards()
        {
            boxRewards = _peer.CharacterData.QuestExtraRewardsInventory.InitBoxRewardsList();
            boxRewards = _peer.CharacterData.QuestExtraRewardsInventory.DeserializeBoxRewards(boxRewards);

            string boxRewardsCollectedStr = _peer.CharacterData.QuestExtraRewardsInventory.SerializeBoxRewards(boxRewards);
            _peer.CharacterData.QuestExtraRewardsInventory.SetBoxRewardsCollected(boxRewardsCollectedStr);
        }

        public void UpdateTask(QuestExtraType type, int progress = 0)
        {
            for (int i = 0; i < taskProgress.Count; ++i)
            {
                QERewardsTask task = taskProgress[i];
                if(task.mType == type)
                {
                    if(!task.IsComplete())
                    {
                        if (progress == 0)
                        {
                            ++taskProgress[i].mProgress;
                        }
                        else
                        {
                            // Set to requirement max if progress is now bigger than requirement
                            if(progress > task.mRequired || (task.mProgress + progress) > task.mRequired)
                            {
                                taskProgress[i].mProgress = task.mRequired;
                            }
                            // Otherwise just increment as normal
                            else
                            {
                                taskProgress[i].mProgress += progress;
                            }
                        }
                    }
                }
            }

            string taskProgressStr = _peer.CharacterData.QuestExtraRewardsInventory.SerializeTaskProgress(taskProgress);
            _peer.CharacterData.QuestExtraRewardsInventory.SetTaskProgress(taskProgressStr);
            _peer.mPlayer.QuestExtraRewardsStats.taskProgress = taskProgressStr;
        }

        public void SetTaskComplete(int taskid)
        {
            int taskPos = taskProgress.FindIndex(o => o.mId == taskid);
            if (taskPos != -1)
            {
                taskProgress[taskPos].Complete();
            }

            string taskProgressStr = _peer.CharacterData.QuestExtraRewardsInventory.SerializeTaskProgress(taskProgress);
            _peer.CharacterData.QuestExtraRewardsInventory.SetTaskProgress(taskProgressStr);
            _peer.mPlayer.QuestExtraRewardsStats.taskProgress = taskProgressStr;
        }

        //public void CompleteTask(int dataid)
        //{
        //    _peer.CharacterData.QuestExtraRewardsInventory.CompleteTask(dataid);
        //    _peer.mPlayer.QuestExtraRewardsStats.taskCompletion |= InventoryUtils.GetBitShiftByPos(dataid);
        //    QETaskData taskData = QuestExtraRewardsRepo.GetTaskList().GetByIdx(dataid);
        //    QuestExtraRewardsRules.LogQERTaskFinish(taskData.TaskID(), _peer);
        //}

        public void CompleteTaskByTaskID(int taskid)
        {
            _peer.CharacterData.QuestExtraRewardsInventory.CompleteTaskByTaskID(taskid);
            QETaskDataList taskDataList = QuestExtraRewardsRepo.GetTaskList();
            SetTaskComplete(taskid);
        }

        public bool IsTaskCompletedServer(int taskid)
        {
            QERewardsTask task = taskProgress.Find(o => o.mId == taskid);
            if(task != null)
            {
                return task.IsComplete();
            }

            return false;
        }

        //public bool IsTaskCompletedClient(int taskid)
        //{
        //    return _peer.CharacterData.QuestExtraRewardsInventory.IsTaskCompletedClient(taskid);
        //}

        public void CollectTaskReward(int taskid)
        {
            int taskPos = taskProgress.FindIndex(o => o.mId == taskid);
            if (taskPos != -1)
            {
                taskProgress[taskPos].Collect();
            }
        }

        public void SerializeTaskCollected()
        {
            string taskCollectedStr = _peer.CharacterData.QuestExtraRewardsInventory.SerializeTaskCollected(taskProgress);
            _peer.CharacterData.QuestExtraRewardsInventory.SetTaskCollected(taskCollectedStr);
            _peer.mPlayer.QuestExtraRewardsStats.taskCollected = taskCollectedStr;
        }

        public bool IsTaskCollectedServer(int taskid)
        {
            QERewardsTask task = taskProgress.Find(o => o.mId == taskid);
            if (task != null)
            {
                return task.mIsCollected;
            }

            return false;
        }

        //public bool IsTaskCollectedClient(int taskid)
        //{
        //    return _peer.CharacterData.QuestExtraRewardsInventory.IsTaskCollectedClient(taskid);
        //}

        public void CollectBoxReward(int boxNum)
        {
            //_peer.CharacterData.QuestExtraRewardsInventory.CollectBoxReward(dataid);
            //_peer.mPlayer.QuestExtraRewardsStats.boxRewardCollection |= InventoryUtils.GetBitShiftByPos(dataid);

            int rewardPos = boxRewards.FindIndex(o => o.mBoxNum == boxNum);
            if(rewardPos != -1)
            {
                boxRewards[rewardPos].Collect();
            }

            string boxRewardsCollectedStr = _peer.CharacterData.QuestExtraRewardsInventory.SerializeBoxRewards(boxRewards);
            _peer.CharacterData.QuestExtraRewardsInventory.SetBoxRewardsCollected(boxRewardsCollectedStr);
            _peer.mPlayer.QuestExtraRewardsStats.boxRewardCollected = boxRewardsCollectedStr;
        }

        public bool IsBoxRewardCollectableServer(int boxNum, int currPts)
        {
            QERBoxReward reward = boxRewards.Find(o => o.mBoxNum == boxNum);
            if (reward != null)
            {
                return reward.IsCollectable(currPts);
            }

            return false;
        }

        public bool IsBoxRewardCollectedServer(int boxNum)
        {
            QERBoxReward reward = boxRewards.Find(o => o.mBoxNum == boxNum);
            if (reward != null)
            {
                return reward.mIsCollected;
            }

            return false;
        }

        public bool IsBoxRewardAllCollectedServer()
        {
            for(int i = 0; i < boxRewards.Count; ++i)
            {
                if(boxRewards[i].mIsCollected == false)
                {
                    return false;
                }
            }

            return true;
        }

        public void AddActivePoints(int amount)
        {
            _peer.CharacterData.QuestExtraRewardsInventory.AddActivePoints(amount);
            _peer.mPlayer.QuestExtraRewardsStats.activePts += amount;
        }

        public bool IsUnlocked(QETaskData taskData)
        {
            Player player = _peer.mPlayer;
            if(player == null)
            {
                return false;
            }

            if((taskData.TaskType() == QuestExtraType.HeroesHouseBefriend || taskData.TaskType() == QuestExtraType.HeroHouseFight ||
                taskData.TaskType() == QuestExtraType.GuildYoumeng || taskData.TaskType() == QuestExtraType.GuildSMBoss ||
                taskData.TaskType() == QuestExtraType.GuildQuest || taskData.TaskType() == QuestExtraType.GuildWishingPoolBroadcast ||
                taskData.TaskType() == QuestExtraType.GuildWishingPoolDonate))
            {
                int unlockLvl = GuildRepo.CreateGuildMinLevel;
                if(player.PlayerSynStats.Level < unlockLvl)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if(taskData.TaskType() == QuestExtraType.StoryDungeon)
            {
                int unlockLvl = RealmRepo.GetStoryDungeonMinReqLvl();
                if(player.PlayerSynStats.Level < unlockLvl)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if(taskData.TaskType() == QuestExtraType.DailyDungeon)
            {
                int unlockLvl = RealmRepo.GetDailyDungeonMinReqLvl();
                if(player.PlayerSynStats.Level < unlockLvl)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if(taskData.TaskType() == QuestExtraType.TalentUpgrade)
            {
                int unlockLvl = GameConstantRepo.GetConstantInt("Talent_UnlockLvl", 1);
                if(player.PlayerSynStats.Level < unlockLvl)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if(taskData.TaskType() == QuestExtraType.ArenaTimes)
            {
                int unlockLvl = GameConstantRepo.GetConstantInt("Arena_UnlockLvl", 1);
                if(player.PlayerSynStats.Level < unlockLvl)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if(taskData.TaskType() == QuestExtraType.EquipmentUpgrade)
            {
                int unlockLvl = GameConstantRepo.GetConstantInt("Equipment_UnlockLvl", 1);
                if(player.PlayerSynStats.Level < unlockLvl)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }

        public int GetActivePoints()
        {
            return _peer.CharacterData.QuestExtraRewardsInventory.GetActivePoints();
        }

        public void ResetOnNewDay()
        {
            // Clear Character data
            _peer.CharacterData.QuestExtraRewardsInventory.NewDayReset();

            _peer.CharacterData.QuestExtraRewardsInventory.InitFromInventory(_peer.mPlayer.QuestExtraRewardsStats);

            InitTaskProgress();
            InitBoxRewards();
        }
    }
}
