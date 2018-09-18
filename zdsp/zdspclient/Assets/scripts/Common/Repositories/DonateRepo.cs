using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zealot.Repository
{
    public static class DonateRepo
    {
        public static Dictionary<int, DonateLimitJson> mDonateLimitMap;
        public static Dictionary<int, List<DonateJson>> mDonateGroupMap;
        public static Dictionary<int, DonateJson> mDonateIdMap;

        static DonateRepo()
        {
            mDonateLimitMap = new Dictionary<int, DonateLimitJson>();
            mDonateGroupMap = new Dictionary<int, List<DonateJson>>();
            mDonateIdMap = new Dictionary<int, DonateJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mDonateLimitMap.Clear();
            mDonateGroupMap.Clear();
            mDonateIdMap.Clear();

            mDonateLimitMap = gameData.DonateLimit;

            foreach(KeyValuePair<int, DonateJson> entry in gameData.Donate)
            {
                if (!mDonateGroupMap.ContainsKey(entry.Value.groupid))
                {
                    mDonateGroupMap.Add(entry.Value.groupid, new List<DonateJson>());
                }
                mDonateGroupMap[entry.Value.groupid].Add(entry.Value);
                mDonateIdMap.Add(entry.Value.id, entry.Value);
            }
        }

        public static bool GetMaxOrderAndGroup(int level, out int max, out int groupid)
        {
            max = 0;
            groupid = -1;

            foreach (KeyValuePair<int, DonateLimitJson> entry in mDonateLimitMap)
            {
                if (level >= entry.Value.lvlmin && level <= entry.Value.lvlmax)
                {
                    max = entry.Value.dailylimit;
                    groupid = entry.Value.groupid;
                    return true;
                }
            }
            return false;
        }

        public static int GetMaxOrder(int level)
        {
            foreach (KeyValuePair<int, DonateLimitJson> entry in mDonateLimitMap)
            {
                if (level >= entry.Value.lvlmin && level <= entry.Value.lvlmax)
                {
                    return entry.Value.dailylimit;
                }
            }
            return 0;
        }

        public static List<DonateJson> GetDonateListByGroupId(int groupid)
        {
            if (mDonateGroupMap.ContainsKey(groupid))
            {
                return mDonateGroupMap[groupid];
            }
            return null;
        }

        public static DonateJson GetDonateById(int donateid)
        {
            if (mDonateIdMap.ContainsKey(donateid))
            {
                return mDonateIdMap[donateid];
            }
            return null;
        }
    }
}
