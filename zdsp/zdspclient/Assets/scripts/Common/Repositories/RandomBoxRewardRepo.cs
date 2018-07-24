using System;
using System.Collections.Generic;
using Kopio.JsonContracts;
using System.Linq;

namespace Zealot.Repository
{
    public static class RandomBoxRewardRepo
    {
        public static Dictionary<int, RandomBoxRewardJson> mRandomBoxRewardInfo;

        static RandomBoxRewardRepo()
        {
            mRandomBoxRewardInfo = new Dictionary<int, RandomBoxRewardJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            foreach (KeyValuePair<int, RandomBoxRewardJson> entry in gameData.RandomBoxReward)
                mRandomBoxRewardInfo.Add(entry.Value.id, entry.Value);
        }

        public static RandomBoxRewardJson GetRandomBoxRewardInfoById(int id)
        {
            if (mRandomBoxRewardInfo.ContainsKey(id))
                return mRandomBoxRewardInfo[id];

            return null;
        }

        public static RandomBoxRewardJson[] GetRandomBoxRewardInfos()
        {
            return mRandomBoxRewardInfo.Values.ToArray();
        }
    }
}
