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
    public class QERewardsTask
    {
        public int mId;
        public QuestExtraType mType;
        public int mProgress;
        public int mRequired;
        public bool mIsCollected;

        public QERewardsTask(int id, QuestExtraType type, int progress, int required)
        {
            mId = id;
            mType = type;
            mProgress = progress;
            mRequired = required;
            mIsCollected = false;
        }

        public void Complete()
        {
            mProgress = mRequired;
        }

        public bool IsComplete()
        {
            return mProgress >= mRequired;
        }

        public void Collect()
        {
            mIsCollected = true;
        }

        public void Reset()
        {
            mProgress = 0;
            mIsCollected = false;
        }
    }

    public class QERBoxReward
    {
        public int mBoxNum;
        public int mReqAmt;
        public bool mIsCollected;

        public QERBoxReward(int boxNum, int reqAmt)
        {
            mBoxNum = boxNum;
            mReqAmt = reqAmt;
            mIsCollected = false;
        }

        public bool IsCollectable(int currActvPts)
        {
            return currActvPts >= mReqAmt;
        }

        public void Collect()
        {
            mIsCollected = true;
        }

        public void Reset()
        {
            mIsCollected = false;
        }
    }

    public partial class QuestExtraRewardsInvData
    {
        // Lasts until player logs out (for client use)
        List<QERewardsTask> taskProgressList;

        // Lasts until player logs out (for client use)
        List<QERBoxReward> boxRewardsList;

        // Resets every day
        [JsonProperty(PropertyName = "taskProgress")]
        public string TaskProgress;

        // Resets every day
        [JsonProperty(PropertyName = "taskCompletion")]
        public int TaskCompletion;

        // Resets every day
        [JsonProperty(PropertyName = "taskCompleted")]
        public string TaskCompleted;

        // Resets every day
        [JsonProperty(PropertyName = "taskCollection")]
        public int TaskCollection;

        // Resets every day
        [JsonProperty(PropertyName = "taskCollected")]
        public string TaskCollected;

        // Resets every day
        [JsonProperty(PropertyName = "boxRewardsCollection")]
        public int BoxRewardsCollection;

        // Resets every day
        [JsonProperty(PropertyName = "boxRewardsCollected")]
        public string BoxRewardsCollected;

        // Resets every day
        [JsonProperty(PropertyName = "activePoints")]
        public int ActivePoints;

        // Add constants here

        public QuestExtraRewardsInvData()
        {
            InitTaskProgressData();
            TaskCompletion          = 0;
            TaskCollection          = 0;
            TaskCollected           = "0";
            InitBoxRewardsData();
            ActivePoints            = 0;
        }

        public void InitDefault()
        {
            InitTaskProgressData();
            TaskCompletion          = 0;
            TaskCollection          = 0;
            TaskCollected           = "0";
            InitBoxRewardsData();
            ActivePoints            = 0;
        }
    }
}