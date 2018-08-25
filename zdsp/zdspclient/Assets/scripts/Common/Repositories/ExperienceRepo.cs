using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class ExperienceRepo
    {
        //static Dictionary<int, LevelUpJson> mLevelUpJson;
        //static Dictionary<int, LevelUpIconJson> mLevelUpIconJson;
        public static Dictionary<JobType, JobCombatStatsJson> mAllJobParameterJson;

        static ExperienceRepo()
        {
            //mLevelUpJson = new Dictionary<int, LevelUpJson>();
            //mLevelUpIconJson = new Dictionary<int, LevelUpIconJson>();
            mAllJobParameterJson = new Dictionary<JobType, JobCombatStatsJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            //mLevelUpJson.Clear();
            //mLevelUpIconJson.Clear();
            mAllJobParameterJson.Clear();
            
            //foreach(var kvp in gameData.LevelUp)
            //    mLevelUpJson[kvp.Value.level] = kvp.Value;
            //foreach (var kvp in gameData.LevelUpIcon)
            //    mLevelUpIconJson[kvp.Value.level] = kvp.Value;
            foreach (var kvp in gameData.JobCombatStats)
                mAllJobParameterJson[kvp.Value.jobsect] = kvp.Value;
        }

        public static JobCombatStatsJson GetJobParameterByJobType(JobType type)
        {
            if (mAllJobParameterJson.ContainsKey(type))
                return mAllJobParameterJson[type];
            else
                return null;
        }

        /// <summary>
        /// the experience return for the given level is the amount needed to level up to next level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        //public static int GetExperienceByLevel(int level)
        //{
        //    LevelUpJson result = null;
        //    mLevelUpJson.TryGetValue(level, out result);
        //    if(result != null)
        //        return result.experience;
        //    else
        //        return 0;
        //}

        //public static LevelUpJson GetLevelInfo(int level)
        //{
        //    if (mLevelUpJson.ContainsKey(level))
        //        return mLevelUpJson[level];
        //    return null;
        //}

        //public static int GetMaxLevel()
        //{
        //    return mLevelUpJson.Count;
        //}

        //public static LevelUpIconJson GetLevelUpIconInfo(int level)
        //{
        //    if (mLevelUpIconJson.ContainsKey(level))
        //        return mLevelUpIconJson[level];
        //    return null;
        //}
    }
}
