using Kopio.JsonContracts;
using System.Collections.Generic;
using System.Linq;

namespace Zealot.Repository
{
    public static class OfflineExpRepo
    {
        public static Dictionary<int, List<int>> mDic_ExpRewardByLevel;

        public static void Init(GameDBRepo gameData)
        {
            mDic_ExpRewardByLevel = new Dictionary<int, List<int>>();

            foreach (var lr in gameData.ExpReward)
            {
                mDic_ExpRewardByLevel.Add(lr.Value.playerlv, new List<int>());

                mDic_ExpRewardByLevel[lr.Value.playerlv].Add( lr.Value.expreward01 );
                mDic_ExpRewardByLevel[lr.Value.playerlv].Add( lr.Value.expreward02 );
                mDic_ExpRewardByLevel[lr.Value.playerlv].Add( lr.Value.expreward04 );
                mDic_ExpRewardByLevel[lr.Value.playerlv].Add( lr.Value.expreward06 );
                mDic_ExpRewardByLevel[lr.Value.playerlv].Add( lr.Value.expreward08 );
                mDic_ExpRewardByLevel[lr.Value.playerlv].Add( lr.Value.expreward12 );
            }
        }

        public static int GetNumberExpReward()
        {
            if (mDic_ExpRewardByLevel.Count == 0)
                return -1;

            return mDic_ExpRewardByLevel.First().Value.Count;
        }

        public static List<int> GetExpRewardListByLevel(int lv)
        {
            if (!mDic_ExpRewardByLevel.ContainsKey(lv))
                return null;

            return mDic_ExpRewardByLevel[lv];
        }

        public static int GetExpReward(int lv, int index)
        {
            if (!mDic_ExpRewardByLevel.ContainsKey(lv) ||
                index >= mDic_ExpRewardByLevel[lv].Count)
                return -1;

            return mDic_ExpRewardByLevel[lv][index];
        }
    }
}
