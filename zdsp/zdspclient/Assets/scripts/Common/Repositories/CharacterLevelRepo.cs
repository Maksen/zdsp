using System;
using System.Collections.Generic;
using Kopio.JsonContracts;

namespace Zealot.Repository
{
    public struct PartyShareLevelInfo
    {
        public int minLvl;
        public int maxLvl;
        public bool minLvl_GE;
        public bool maxLvl_LE;
        public int sharePercent;
    }

    public static class CharacterLevelRepo
    {
        public static List<int> mLevelExpLst = new List<int>();
        public static List<int> mLevelAccumExpLst = new List<int>();
        public static List<int> mLevelStatsPtLst = new List<int>();
        public static List<int> mLevelAccumStatsLst = new List<int>();
        public static List<int> mLevelSkillExpLst = new List<int>();
        public static List<int> mLevelAccumSkillExpLst = new List<int>();
        private static List<PartyShareLevelInfo> mPartyShareLst = new List<PartyShareLevelInfo>();

        static CharacterLevelRepo()
        {
            mLevelExpLst                = new List<int>();
            mLevelAccumExpLst           = new List<int>();
            mLevelStatsPtLst            = new List<int>();
            mLevelAccumStatsLst         = new List<int>();
            mLevelSkillExpLst           = new List<int>();
            mLevelAccumSkillExpLst      = new List<int>();
            mPartyShareLst              = new List<PartyShareLevelInfo>();
        }

        public static void Init(GameDBRepo gameData)
        {
            int count = gameData.LevelUpExp.Count;
            //Assuming in kopio table LevelUpExp's id == lvid
            for (int i = 1; i <= count; ++i)
            {
                mLevelExpLst.Add(gameData.LevelUpExp[i].expreq);
            }
            mLevelAccumExpLst.Add(mLevelExpLst[0]);
            for (int i = 1; i < count; ++i)
            {
                mLevelAccumExpLst.Add(mLevelAccumExpLst[i-1] + mLevelExpLst[i]);
            }

            count = gameData.Stats.Count;
            for (int i = 1; i <= count; ++i)
            {
                mLevelStatsPtLst.Add(gameData.Stats[i].statspoint);
            }
            mLevelAccumStatsLst.Add(mLevelStatsPtLst[0]);
            for (int i = 1; i < count; ++i)
            {
                mLevelAccumStatsLst.Add(mLevelAccumStatsLst[i-1] + mLevelStatsPtLst[i]);
            }

            count = gameData.SkillPoint.Count;
            for (int i = 1; i <= count; ++i)
            {
                mLevelSkillExpLst.Add(gameData.SkillPoint[i].expreq);
            }
            mLevelAccumSkillExpLst.Add(mLevelSkillExpLst[0]);
            for (int i = 1; i < count; ++i)
            {
                mLevelAccumSkillExpLst.Add(mLevelAccumSkillExpLst[i-1] + mLevelSkillExpLst[i]);
            }


            foreach (var share in gameData.ExpMonsterLvDifference)
            {
                PartyShareLevelInfo psli = new PartyShareLevelInfo();
                psli.minLvl = share.Value.minlvl;
                psli.minLvl_GE = share.Value.minlvlequal;
                psli.maxLvl = share.Value.maxlvl;
                psli.maxLvl_LE = share.Value.maxlvlequal;
                psli.sharePercent = share.Value.exppercent;

                mPartyShareLst.Add(psli);
            }
        }

        public static int GetExpByLevel(int level)
        {
            if (level < 1 || level > mLevelExpLst.Count)
                return -1;

            return mLevelExpLst[level-1];
        }

        public static int GetTotalExpByLevel(int level)
        {
            if (level < 1 || level > mLevelAccumExpLst.Count)
                return -1;

            return mLevelAccumExpLst[level-1];
        }

        public static int GetMaxLevel()
        {
            return mLevelExpLst.Count;
        }

        public static int GetJobMaxLevel()
        {
            return mLevelSkillExpLst.Count;
        }

        public static int GetStatsByLevel(int level)
        {
            if (level < 1 || level > mLevelStatsPtLst.Count)
                return -1;

            return mLevelStatsPtLst[level-1];
        }

        public static int GetTotalStatsByLevel(int level)
        {
            if (level < 1 || level > mLevelAccumStatsLst.Count)
                return -1;

            return mLevelAccumStatsLst[level-1];
        }

        public static int GetExpBySkillPt(int skillpt)
        {
            if (skillpt < 0 || skillpt >= mLevelSkillExpLst.Count)
                return -1;

            return mLevelSkillExpLst[skillpt];
        }

        public static int GetTotalExpBySkillPt(int skillpt)
        {
            if (skillpt < 0 || skillpt >= mLevelAccumSkillExpLst.Count)
                return -1;

            return mLevelAccumSkillExpLst[skillpt];
        }

        public static bool GetPartyShareInfoByLevelDiff(int playerlevel, int otherlevel, out PartyShareLevelInfo info)
        {
            int lvlgap = otherlevel - playerlevel;
            for (int i = 0; i < mPartyShareLst.Count; ++i)
            {
                //if beyond level range, skip
                if (lvlgap < mPartyShareLst[i].minLvl || lvlgap > mPartyShareLst[i].maxLvl ||
                    //if mainlevel equal to minlvl, but equal not set
                    !mPartyShareLst[i].minLvl_GE && lvlgap == mPartyShareLst[i].minLvl ||
                    //if mainlevel equal to maxlvl, but equal not set
                    !mPartyShareLst[i].maxLvl_LE && lvlgap == mPartyShareLst[i].maxLvl)
                    continue;

                info = mPartyShareLst[i];
                return true;
            }

            info = new PartyShareLevelInfo();
            return false;
        }

        public static int GetShareExpByLevelDiff(int playerlevel, int otherlevel)
        {
            PartyShareLevelInfo info;
            if (GetPartyShareInfoByLevelDiff(playerlevel, otherlevel, out info))
            {
                return info.sharePercent;
            }
            return -1;
        }
    }
}