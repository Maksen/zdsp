using Kopio.JsonContracts;
using System.Collections.Generic;

namespace Zealot.Repository
{
    public struct RewardItem
    {
        public int itemId;
        public int count;

        public RewardItem(int id, int _count)
        {
            itemId = id;
            count = _count;
        }
    }

    public class Reward
    {
        public bool isExpGrade;
        public int job = 0;
        private int baseExperience = 0;
        private int skillPoints = 0;
        public int money = 0;
        public int donatepoint = 0;
        public int guildactivepoint = 0;
        public List<RewardItem> itemRewardLst = new List<RewardItem>();

        public Reward(RewardListJson json)
        {
            isExpGrade = json.isexpgrade;
            job = json.job;
            baseExperience = json.experience;
            skillPoints = json.skillpoint;
            money = json.money;
            donatepoint = json.donatept;
            guildactivepoint = json.guildactivept;

            if (json.itemid1 != 0)
                itemRewardLst.Add(new RewardItem(json.itemid1, json.itemcount1));
            if (json.itemid2 != 0)
                itemRewardLst.Add(new RewardItem(json.itemid2, json.itemcount2));
            if (json.itemid3 != 0)
                itemRewardLst.Add(new RewardItem(json.itemid3, json.itemcount3));
            if (json.itemid4 != 0)
                itemRewardLst.Add(new RewardItem(json.itemid4, json.itemcount4));
            if (json.itemid5 != 0)
                itemRewardLst.Add(new RewardItem(json.itemid5, json.itemcount5));
        }

        public void Append(RewardListJson json)
        {
            baseExperience += json.experience;
            skillPoints += json.skillpoint;
            money += json.money;
            donatepoint += json.donatept;
            guildactivepoint += json.guildactivept;

            if (json.itemid1 != 0)
                itemRewardLst.Add(new RewardItem(json.itemid1, json.itemcount1));
            if (json.itemid2 != 0)
                itemRewardLst.Add(new RewardItem(json.itemid2, json.itemcount2));
            if (json.itemid3 != 0)
                itemRewardLst.Add(new RewardItem(json.itemid3, json.itemcount3));
            if (json.itemid4 != 0)
                itemRewardLst.Add(new RewardItem(json.itemid4, json.itemcount4));
            if (json.itemid5 != 0)
                itemRewardLst.Add(new RewardItem(json.itemid5, json.itemcount5));
        }

        public void Append(Reward rwd)
        {
            baseExperience += rwd.baseExperience;
            skillPoints += rwd.skillPoints;
            money += rwd.money;
            donatepoint += rwd.donatepoint;
            guildactivepoint += rwd.guildactivepoint;

            itemRewardLst.AddRange(rwd.itemRewardLst);
        }

        public int Exp(int level)
        {
            if (!RewardListRepo.mExpRate.ContainsKey(level) || !isExpGrade)
                return baseExperience;

            return (int)(baseExperience * RewardListRepo.mExpRate[level].exprate);
        }
    }

    public class ActivityScoreReward
    {
        public int score = 0;
        public List<int> rewardGrpLst = new List<int>();

        public ActivityScoreReward(int _score, string rewardGrpStr)
        {
            score = _score;

            string[] strArr = rewardGrpStr.Split(';');
            foreach (string s in strArr)
            {
                int x = 0;
                if (!int.TryParse(s, out x))
                    continue;

                rewardGrpLst.Add(x);
            }
        }
    }
    
    public static class RewardListRepo
    {
        public static Dictionary<int, RewardListJson> mIdMap;
        public static Dictionary<int, ExperienceRateJson> mExpRate;
        public static Dictionary<int, Dictionary<int, Reward>> mRewardGrp;
        public static Dictionary<int, List<ActivityScoreReward>> mActivityReward;

        static RewardListRepo()
        {
            mIdMap = new Dictionary<int, RewardListJson>();
            mRewardGrp = new Dictionary<int, Dictionary<int, Reward>>();
            mExpRate = new Dictionary<int, ExperienceRateJson>();
            mActivityReward = new Dictionary<int, List<ActivityScoreReward>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mIdMap = gameData.RewardList;

            foreach (RewardListJson j in mIdMap.Values)
            {
                if (mRewardGrp.ContainsKey(j.rewardgroupid) == false)
                {
                    mRewardGrp.Add(j.rewardgroupid, new Dictionary<int, Reward>());
                }
                if (mRewardGrp[j.rewardgroupid].ContainsKey(j.job) == false)
                {
                    mRewardGrp[j.rewardgroupid].Add(j.job, new Reward(j));
                    continue;
                }

                mRewardGrp[j.rewardgroupid][j.job].Append(j);
            }

            //If reward group contains both job and all job rewards, append all job rewards to individual job
            foreach (Dictionary<int, Reward> rwdLst in mRewardGrp.Values)
            {
                //Do nothing if there are no "all job" reward
                //Or there is only "all job" rewards
                if (!rwdLst.ContainsKey(-1) || rwdLst.Count <= 1)
                    continue;

                //Append all job reward to individual job reward
                foreach (Reward rwd in rwdLst.Values)
                {
                    if (rwd.job == -1)
                        continue;

                    rwd.Append(rwdLst[-1]);
                }

                //Remove all job reward from rwdLst
                rwdLst.Remove(-1);
            }

            foreach (ExperienceRateJson j in gameData.ExperienceRate.Values)
            {
                if (mExpRate.ContainsKey(j.level) == false)
                    mExpRate.Add(j.level, j);
            }

            foreach (ActivityRewardJson a in gameData.ActivityReward.Values)
            {
                if (mActivityReward.ContainsKey(a.activitygroupid) == false)
                    mActivityReward.Add(a.activitygroupid, new List<ActivityScoreReward>());

                mActivityReward[a.activitygroupid].Add(new ActivityScoreReward(a.score, a.rewardgroupid));
                mActivityReward[a.activitygroupid].Sort((x,y) => x.score.CompareTo(y.score));
            }
        }

        public static RewardListJson GetRewardById(int id)
        {
            if(mIdMap.ContainsKey(id))
                return mIdMap[id];
            return null;
        }

        public static Dictionary<int, Reward> GetRewardDicByGrpId(int grpId)
        {
            if (mRewardGrp.ContainsKey(grpId))
                return mRewardGrp[grpId];
            return null;
        }

        public static Reward GetRewardByGrpIDJobID(int grpID, int jobID)
        {
            //If invalid reward group id
            if (!mRewardGrp.ContainsKey(grpID))
                return null;
            
            if (!mRewardGrp[grpID].ContainsKey(jobID))
            {
                //If reward group only contains all jobs specifc reward
                if (mRewardGrp[grpID].ContainsKey(-1))
                    return mRewardGrp[grpID][-1];
                else
                    return null;
            }
            //Otherwise, return job specific reward
            return mRewardGrp[grpID][jobID];
        }

        public static List<RewardItem> GetRewardItemsByGrpIDJobID(int grpID, int jobID = -1) 
        {
            //Try get reward
            Reward rwd = GetRewardByGrpIDJobID(grpID, jobID);

            if (rwd == null)
                return null;
            else
                return rwd.itemRewardLst;
        }

        public static ExperienceRateJson GetLvExpRate(int level)
        {
            return mExpRate[level];
        }

        public static ActivityScoreReward GetActivityReward(int actGrpID, int score)
        {
            if (!mActivityReward.ContainsKey(actGrpID) || mActivityReward[actGrpID].Count == 0)
                return null;

            int i = 0;
            for (i = 0; i < mActivityReward[actGrpID].Count; ++i)
            {
                if (score <= mActivityReward[actGrpID][i].score)
                    return mActivityReward[actGrpID][i];
            }

            return mActivityReward[actGrpID][i-1];
        }

        /// <summary>
        /// Determines if reward is different for each class.
        /// Return Value:
        /// -1 : Invalid group id
        /// 0  : reward is same for all class
        /// 1  : reward is different for each class
        /// </summary>
        /// <param name="grpID"></param>
        /// <returns></returns>
        public static int IsRewardJobSpecificByGrpId(int grpID)
        {
            if (!mRewardGrp.ContainsKey(grpID))
                return -1;

            return (mRewardGrp[grpID].Count > 1) ? 1 : 0;
        }
    }
}

