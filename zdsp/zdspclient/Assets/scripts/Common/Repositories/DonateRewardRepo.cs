using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zealot.Common;

namespace Zealot.Repository
{
    public class DonateRewardRepo
    {
        public static Dictionary<int, DonateRewardIdJson> mDonateRewardList;
        public static Dictionary<int, int> mQualityMap;
        static DonateRewardRepo()
        {
            mDonateRewardList = new Dictionary<int, DonateRewardIdJson>();
            mQualityMap = new Dictionary<int, int>();
        }
        public static void Init(GameDBRepo gameData)
        {
            mDonateRewardList.Clear();
            mQualityMap.Clear();

            foreach (var kvp in gameData.DonateRewardId)
            {
                DonateRewardIdJson rewardInfo = kvp.Value;
                mDonateRewardList.Add(kvp.Key, kvp.Value);

                //HeroQuality quality = kvp.Value.heroquality;
                //mQualityMap.Add((int)quality, kvp.Key);
            }
        }

        //public static int GetRewardListIdByHeroQuality(HeroQuality quality)
        //{
        //    int res = -1;
        //    if (mQualityMap.ContainsKey((int)quality)) {
        //        var id = mQualityMap[(int)quality];
        //        res = mDonateRewardList[id].rewardlistid;
        //    }
        //    return res;
        //}
    }
}
