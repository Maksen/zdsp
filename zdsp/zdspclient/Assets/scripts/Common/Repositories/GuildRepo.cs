using System;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class GuildRepo
    {
        public static int MAX_MEMBERS = 50;
        public static int MAX_REQUEST = 12;
        public static int MAX_APPLY = 5;
        public static int CreateGuildMinLevel = 20;
        public static Dictionary<PositionType, Dictionary<int, string>> mGuildIcons;
        public static Dictionary<string, int> mConstants;
        public static Dictionary<int, GuildSMBossJson> mSMBossLvlMap; // SMBoss lvl <- GuildSMBossJson
        public static Dictionary<GuildTechType, GuildTechClassJson> mGuildTechClassMap;
        public static Dictionary<GuildTechType, Dictionary<int, GuildTechLevelJson>> mGuildTechLevelMap; // type -> level -> GuildTechLevelJson
        public static Dictionary<int, int> mGuildDreamhouseMap; // Now theres only 1 row 5 columns, favourability progress <- rewardlist Id
        public static int DreamHouseTotalFavourability = 0;
        public static List<int> mGuildDreamhouseUsedRewards;
        public static Dictionary<int, GuildQuestJson> mQuestlist;
        private static int maxprob = 0;

        static GuildRepo()
		{
            mGuildIcons = new Dictionary<PositionType, Dictionary<int, string>>();
            mConstants = new Dictionary<string, int>();
            mSMBossLvlMap = new Dictionary<int, GuildSMBossJson>();
            mGuildTechClassMap = new Dictionary<GuildTechType, GuildTechClassJson>();
            mGuildTechLevelMap = new Dictionary<GuildTechType, Dictionary<int, GuildTechLevelJson>>();
            mGuildDreamhouseMap = new Dictionary<int, int>();
            mGuildDreamhouseUsedRewards = new List<int>();
            mQuestlist = new Dictionary<int, GuildQuestJson>();
        }

		public static void Init(GameDBRepo gameData)
		{
            mGuildIcons.Clear();
            mConstants.Clear();
            mSMBossLvlMap.Clear();
            mGuildTechClassMap.Clear();
            mGuildTechLevelMap.Clear();
            mGuildDreamhouseMap.Clear();
            mGuildDreamhouseUsedRewards.Clear();
            mQuestlist.Clear();

            foreach (var entry in gameData.GuildBadge)
            {
                if (mGuildIcons.ContainsKey(entry.Value.type) == false)
                    mGuildIcons.Add(entry.Value.type, new Dictionary<int, string>());

                mGuildIcons[entry.Value.type].Add(entry.Value.sortorder, entry.Value.iconpath);
            }

            foreach (var entry in gameData.GuildConstant)
            {
                mConstants.Add(entry.Value.name, entry.Value.value);
            }

            foreach (var kvp in gameData.GuildSMBoss)
            {
                GuildSMBossJson guildSMBossJson = kvp.Value;
                mSMBossLvlMap.Add(guildSMBossJson.level, guildSMBossJson);
            }
            foreach (var entry in gameData.GuildTechClass)
                mGuildTechClassMap[entry.Value.type] = entry.Value;

            foreach (var entry in gameData.GuildTechLevel)
            {
                GuildTechType type = entry.Value.type;
                if (!mGuildTechLevelMap.ContainsKey(type))
                    mGuildTechLevelMap.Add(type, new Dictionary<int, GuildTechLevelJson>());
                mGuildTechLevelMap[type][entry.Value.level] = entry.Value;
            }

            int max_members = GetValue("max_members");
            if (max_members > 0)
                MAX_MEMBERS = max_members;
            int max_request = GetValue("max_request");
            if (max_request > 0)
                MAX_REQUEST = max_request;
            int max_apply = GetValue("MaxGuildApplyCount");
            if (max_apply > 0)
                MAX_APPLY = max_apply;
            int createGuildMinLevel = GetValue("CreateGuildMinLevel");
            if (createGuildMinLevel > 0)
                CreateGuildMinLevel = createGuildMinLevel;

            foreach (var kvp in gameData.GuildDreamhouse)
            {
                GuildDreamhouseJson guildDreamhouseJson = kvp.Value;
                mGuildDreamhouseMap.Add(guildDreamhouseJson.progresscnt1, guildDreamhouseJson.rewardlist1);
                mGuildDreamhouseMap.Add(guildDreamhouseJson.progresscnt2, guildDreamhouseJson.rewardlist2);
                mGuildDreamhouseMap.Add(guildDreamhouseJson.progresscnt3, guildDreamhouseJson.rewardlist3);
                mGuildDreamhouseMap.Add(guildDreamhouseJson.progresscnt4, guildDreamhouseJson.rewardlist4);
                mGuildDreamhouseMap.Add(guildDreamhouseJson.progresscnt5, guildDreamhouseJson.rewardlist5);
                mGuildDreamhouseUsedRewards.Add(guildDreamhouseJson.smrewardlist1);
                mGuildDreamhouseUsedRewards.Add(guildDreamhouseJson.smrewardlist2);
                mGuildDreamhouseUsedRewards.Add(guildDreamhouseJson.smrewardlist3);
            }
            Dictionary<int, int>.KeyCollection gDreamhouseMapKeys = mGuildDreamhouseMap.Keys;
            foreach (int progress in gDreamhouseMapKeys)
            {
                if (progress > DreamHouseTotalFavourability)
                    DreamHouseTotalFavourability = progress;
            }

            // Guild quest
            mQuestlist = gameData.GuildQuest;
            //handle the probability    
        }

        public static GuildQuestJson GetQuestJson(int id)
        {
            if (mQuestlist.ContainsKey(id))
                return mQuestlist[id];
            return null;
        }

        public static List<int> GetRandomizeQuestID(List<GuildQuestJson> questlist)
        {
            Dictionary<int, ProbabilityRange> mQuestProbByID = new Dictionary<int, ProbabilityRange>();
            maxprob = 0;
            foreach ( GuildQuestJson entry in questlist)
            {
                int min = maxprob;
                maxprob += entry.probability;
                mQuestProbByID.Add(entry.id, new ProbabilityRange(min, maxprob));
            }
            List<int> res = new List<int>();
            while (res.Count < 3)
            {
                int roll = Zealot.Common.GameUtils.RandomInt(1, maxprob);
                foreach (KeyValuePair<int, ProbabilityRange> entry in mQuestProbByID)
                {
                    if (entry.Value.IsInRange(roll) && !res.Contains(entry.Key))
                    {
                        res.Add(entry.Key);
                    }
                }
            }
           
            return res;//should not happen.
        }

        public static int GetValue(string constant)
        {
            int value = 0;
            mConstants.TryGetValue(constant, out value);
            return value;
        }

        public static int[] GetGuildIconIdsForTotalId(int flag_id)
        {
            return new int[] { Math.Max(1, flag_id % 100), Math.Max(1, (flag_id / 100) % 100), Math.Max(1, (flag_id / 10000) % 100)};
        }

        public static int GetGuildFlagIdByIconId(int background_id, int patten_id, int frame_id)
        {
            return frame_id * 10000 + patten_id * 100 + background_id;
        }

        public static string[] GetGuildIconPaths(int flag_id)
        {
            var ids = GetGuildIconIdsForTotalId(flag_id);
            string[] paths = new string[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
            {
                if (mGuildIcons.ContainsKey((PositionType)i))
                    mGuildIcons[(PositionType)i].TryGetValue(ids[i], out paths[i]);
                else
                    paths[i] = "";
            }
            return paths;
        }

        public static string GetGuildIconPath(int icon_type, int icon_id)
        {
            string path = "";
            if (mGuildIcons.ContainsKey((PositionType)icon_type))
                mGuildIcons[(PositionType)icon_type].TryGetValue(icon_id, out path);

            return path;
        }

        public static bool IsIconValid(int flag_id)
        {
            var ids = GetGuildIconIdsForTotalId(flag_id);
            for (int i = 0; i < ids.Length; ++i)
            {
                if (mGuildIcons.ContainsKey((PositionType)i) == false || mGuildIcons[(PositionType)i].ContainsKey(ids[i]) == false)
                {
                    return false;
                }
            }
            return true;
        }

        public static GuildSMBossJson GetGuildSMBossByLvl(int lvl)
        {
            if(mSMBossLvlMap.ContainsKey(lvl))
                return mSMBossLvlMap[lvl];
            return null;
        }
        
        public static GuildTechClassJson GetGuildTechClassByType(GuildTechType type)
        {
            if (mGuildTechClassMap.ContainsKey(type))
                return mGuildTechClassMap[type];
            return null;
        }
        
        public static GuildTechLevelJson GetGuildTechByTypeAndLevel(GuildTechType type, int level)
        {
            if (mGuildTechLevelMap.ContainsKey(type) && mGuildTechLevelMap[type].ContainsKey(level))
                return mGuildTechLevelMap[type][level];
            return null;
        }

        public static int GetGuildTechMaxLevelByType(GuildTechType type)
        {
            if (mGuildTechLevelMap.ContainsKey(type))
                return mGuildTechLevelMap[type].Count;
            return 0;
        } 

        private class ProbabilityRange
        {
            private int _min=-1;
            private int _max=-1;
            public ProbabilityRange(int min, int max)
            {
                _min = min;
                _max = max;
            }

            public bool IsInRange(int n)
            {
                return (n >= _min && n <=_max && _min < _max);
            }
        }
    }
}
