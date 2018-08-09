using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;
using System.Linq;
using System;

namespace Zealot.Repository
{
    public static class RealmRepo
    {
        public static Dictionary<int, RealmJson> mIdMap;
        public static Dictionary<string, RealmWorldJson> mRealmWorld; // key = level name
        public static Dictionary<int, Dictionary<DungeonDifficulty, DungeonJson>> mDungeon; // key = sequence <- difficulty
        //public static Dictionary<int, List<DungeonDailySpecialJson>> mDungeonDaily; // key = sequence <- List sort by reqlvl
        //public static Dictionary<int, List<DungeonDailySpecialJson>> mDungeonSpecial; // key = sequence <- List sort by reqlvl
        //public static Dictionary<int, RealmObjectiveJson> mRealmObjective; // key = index
        public static Dictionary<int, Dictionary<int, int>> mExtraEntryFees; // key = sequence <- Dict of gold to count
        public static Dictionary<int, Dictionary<int, int>> mStarRewards; // key = sequence <- Dict of starnumber to reward Id
        //public static Dictionary<int, List<ActivityGuardWarJson>> mActivityGuardWar;
        //public static Dictionary<string, ActivityGuildSMBossJson> mActivityGuildSMBoss; // key = id <- ActivityGuildSMBossJson
        //public static ArenaJson mArenaJson { get; private set; }
        static Dictionary<MapType, string> mMapCategory;
        static Dictionary<MapType, List<RealmWorldJson>> mAllMapRealmWorld;
        //public static RealmTutorialJson TutorialRealmJson;
        //public static ActivityWorldBossJson mActivityWorldBoss;
        //public static Dictionary<int, EliteMapJson> mEliteMap;

        static RealmRepo()
        {
            mIdMap = new Dictionary<int, RealmJson>();
            mRealmWorld = new Dictionary<string, RealmWorldJson>();
            mDungeon = new Dictionary<int, Dictionary<DungeonDifficulty, DungeonJson>>();
            //mRealmObjective = new Dictionary<int, RealmObjectiveJson>();
            mExtraEntryFees = new Dictionary<int, Dictionary<int, int>>();
            mStarRewards = new Dictionary<int, Dictionary<int, int>>();
            //mActivityGuardWar = new Dictionary<int, List<ActivityGuardWarJson>>();
            //mActivityGuildSMBoss = new Dictionary<string, ActivityGuildSMBossJson>();
            mAllMapRealmWorld = new Dictionary<MapType, List<RealmWorldJson>>();
            mMapCategory = new Dictionary<MapType, string>();
            //mEliteMap = new Dictionary<int, EliteMapJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mIdMap.Clear();
            mRealmWorld.Clear();
            mDungeon.Clear();
            //mRealmObjective.Clear();
            mExtraEntryFees.Clear();
            mStarRewards.Clear();
            //mActivityGuardWar.Clear();
            //mActivityGuildSMBoss.Clear();
            mAllMapRealmWorld.Clear();
            mMapCategory.Clear();
            //mEliteMap.Clear();

            foreach (KeyValuePair<int, MapCategoryJson> kvp in gameData.MapCategory)
            {
                mMapCategory[kvp.Value.maptype] = kvp.Value.localizedname;
                mAllMapRealmWorld.Add(kvp.Value.maptype, new List<RealmWorldJson>());
            }

            foreach (KeyValuePair<int, RealmWorldJson> kvp in gameData.RealmWorld)
            {
                mIdMap.Add(kvp.Key, kvp.Value);
                RealmWorldJson currRealmWorld = kvp.Value;                
                mRealmWorld[LevelRepo.GetInfoById(currRealmWorld.level).unityscene] = currRealmWorld;

                MapType maptype = currRealmWorld.maptype;
                if (mAllMapRealmWorld.ContainsKey(maptype))
                    mAllMapRealmWorld[maptype].Add(kvp.Value);
            }

            for(int i=0; i<mAllMapRealmWorld.Count; ++i)
            {
                var kvp = mAllMapRealmWorld.ElementAt(i);
                mAllMapRealmWorld[kvp.Key] = kvp.Value.OrderBy(o => o.reqlvl).ToList();
            }

            foreach (KeyValuePair<int, DungeonJson> kvp in gameData.Dungeon)
            {
                mIdMap.Add(kvp.Key, kvp.Value);
                DungeonJson dungeonJson = kvp.Value;
                int seq = dungeonJson.sequence;
                if (!mDungeon.ContainsKey(seq))
                    mDungeon[seq] = new Dictionary<DungeonDifficulty, DungeonJson>();
                mDungeon[seq][dungeonJson.difficulty] = dungeonJson;
            }

            /*Dictionary<int, List<DungeonDailySpecialJson>> currDungeonDict = null;
            foreach (KeyValuePair<int, DungeonDailySpecialJson> kvp in gameData.DungeonDailySpecial)
            {
                DungeonDailySpecialJson dDailySpecialJson = kvp.Value;
                mIdMap.Add(kvp.Key, dDailySpecialJson);

                currDungeonDict = (dDailySpecialJson.dungeontype == DungeonType.Daily) ? mDungeonDaily : mDungeonSpecial;

                int seq = dDailySpecialJson.sequence;
                if (!currDungeonDict.ContainsKey(seq))
                    currDungeonDict[seq] = new List<DungeonDailySpecialJson>();
                currDungeonDict[seq].Add(dDailySpecialJson);
            }
            // Sort list by require levels
            foreach(KeyValuePair<int, List<DungeonDailySpecialJson>> kvp in mDungeonDaily)
                kvp.Value.Sort((x, y) => x.reqlvl.CompareTo(y.reqlvl));
            foreach (KeyValuePair<int, List<DungeonDailySpecialJson>> kvp in mDungeonSpecial)
                kvp.Value.Sort((x, y) => x.reqlvl.CompareTo(y.reqlvl));*/

            /*foreach (KeyValuePair<int, RealmObjectiveJson> kvp in gameData.RealmObjective)
            {
                mRealmObjective.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<int, ExtraEntryFeesJson> kvp in gameData.ExtraEntryFees)
            {
                ExtraEntryFeesJson extraEntryFeesJson = kvp.Value;
                int seq = extraEntryFeesJson.sequence;
                if (!mExtraEntryFees.ContainsKey(seq))
                    mExtraEntryFees[seq] = new Dictionary<int, int>();
                mExtraEntryFees[seq].Add(extraEntryFeesJson.count, extraEntryFeesJson.gold);
            }

            foreach (KeyValuePair<int, StarRewardJson> kvp in gameData.StarReward)
            {
                StarRewardJson starRewardJson = kvp.Value;
                int seq = starRewardJson.sequence;
                if (!mStarRewards.ContainsKey(seq))
                    mStarRewards[seq] = new Dictionary<int, int>();
                mStarRewards[seq].Add(starRewardJson.starnumber, starRewardJson.rewardgrp);
            }

            foreach (var kvp in gameData.ActivityGuardWar)
            {
                mIdMap.Add(kvp.Key, kvp.Value);
                if(mActivityGuardWar.ContainsKey(kvp.Value.heroid) == false)
                    mActivityGuardWar[kvp.Value.heroid] = new List<ActivityGuardWarJson>();

                mActivityGuardWar[kvp.Value.heroid].Add(kvp.Value);
                mActivityGuardWar[kvp.Value.heroid].Sort((x, y) => { return x.worldlv.CompareTo(y.worldlv); });
            }

            foreach (var kvp in gameData.InvitePVP)
            {
                mIdMap.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<int, ActivityGuildSMBossJson> kvp in gameData.ActivityGuildSMBoss)
            {
                ActivityGuildSMBossJson activityGuildSMBossJson = kvp.Value;
                mIdMap.Add(kvp.Key, activityGuildSMBossJson);
                mActivityGuildSMBoss.Add(activityGuildSMBossJson.excelname, activityGuildSMBossJson);
            }

            foreach(KeyValuePair<int, RealmTutorialJson> kvp in gameData.RealmTutorial)
            {
                if (kvp.Value.id > 0)
                {
                    mIdMap.Add(kvp.Key, kvp.Value); 
                    if(TutorialRealmJson ==null)
                        TutorialRealmJson =  kvp.Value;//this set to the one player first login.
                }
            }

            foreach (KeyValuePair<int, ActivityWorldBossJson> kvp in gameData.ActivityWorldBoss)
            {
                mIdMap.Add(kvp.Key, kvp.Value);
                mActivityWorldBoss = kvp.Value;
                break;
            }

            foreach (KeyValuePair<int, ArenaJson> kvp in gameData.Arena)
            {
                mIdMap.Add(kvp.Key, kvp.Value);
                mArenaJson = kvp.Value;
            }

            foreach (KeyValuePair<int, EliteMapJson> kvp in gameData.EliteMap)
            {
                mIdMap.Add(kvp.Key, kvp.Value);
                mEliteMap.Add(kvp.Value.reqlvl, kvp.Value);
            }*/
        }

        /*public static ActivityGuardWarJson GetActivityGuardWarJson(int heroid, int worldLevel = 0)
        {
            if (mActivityGuardWar.ContainsKey(heroid) == false)
                return null;
            var datas = mActivityGuardWar[heroid];
            ActivityGuardWarJson ret = datas[0];
            foreach (var data in datas)
            {
                if (data.worldlv <= worldLevel)
                    ret = data;
                else break;
            }
            return ret;
        }*/

        public static List<RealmWorldJson> GetWorldListByMapType(MapType type)
        {
            if (mAllMapRealmWorld.ContainsKey(type))
                return mAllMapRealmWorld[type];
            return null;
        }

        public static string GetMapTypeLocalizedName(MapType type)
        {
            if (mMapCategory.ContainsKey(type))
                return mMapCategory[type];
            return "";
        }

        public static RealmJson GetInfoById(int id)
        {
            if (mIdMap.ContainsKey(id))
                return mIdMap[id];
            return null;
        }

        public static RealmWorldJson GetWorldByName(string levelname)
        {
            if (mRealmWorld.ContainsKey(levelname))
                return mRealmWorld[levelname];
            return null;
        }

        public static bool IsWorld(string levelname)
        {
            if (mRealmWorld.ContainsKey(levelname))
            {
                MapType maptype = mRealmWorld[levelname].maptype;
                return maptype == MapType.City || maptype == MapType.Wilderness;
            }
            return false;
        }

        public static bool IsCity(string levelname)
        {
            if (mRealmWorld.ContainsKey(levelname))
            {
                MapType maptype = mRealmWorld[levelname].maptype;
                return maptype == MapType.City;
            }
            return false;
        }

        public static string GetCity(int progresslvl)
        {
            int current_reqlvl = 0;
            string current_city = "";
            foreach (var kvp in mRealmWorld)
            {
                if (kvp.Value.maptype != MapType.City)
                    continue;
                int reqlvl = kvp.Value.reqlvl;
                if (progresslvl < reqlvl)
                    continue;
                if (current_reqlvl < reqlvl)
                {
                    current_reqlvl = reqlvl;
                    current_city = kvp.Key;
                }
            }
            return string.IsNullOrEmpty(current_city) ? "daliang_field_test" : current_city;
        }

        public static Dictionary<DungeonDifficulty, DungeonJson> GetDungeonStoryBySeq(int sequence)
        {
            if (mDungeon.ContainsKey(sequence))
                return mDungeon[sequence];
            return null;
        }

        /*public static RealmObjectiveJson GetRealmObjectiveById(int id)
        {
            if (mRealmObjective.ContainsKey(id))
                return mRealmObjective[id];
            return null;
        }*/

        public static Dictionary<int, int> GetExtraEntryFeesBySeq(int sequence)
        {
            if (mExtraEntryFees.ContainsKey(sequence))
                return mExtraEntryFees[sequence];
            return null;
        }

        public static Dictionary<int, int> GetStarRewardsBySeq(int sequence)
        {
            if (mStarRewards.ContainsKey(sequence))
                return mStarRewards[sequence];
            return null;
        }

        /*public static ActivityGuildSMBossJson GetGuildSMBossById(string name)
        {
            if (mActivityGuildSMBoss.ContainsKey(name))
                return mActivityGuildSMBoss[name];
            return null;
        }*/

        public static string GetDungeonName(RealmType rtype, int sequence, DungeonType dType)
        {
            string res = "";
            switch (rtype)
            {
                case RealmType.Dungeon:
                    var dungeonStoryInfo = GetDungeonStoryBySeq(sequence);
                    if (dungeonStoryInfo != null)
                        res = dungeonStoryInfo[DungeonDifficulty.Easy].localizedname;
                    break;
                //case RealmType.DungeonDailySpecial:
                //    var realmInfo = (dType == DungeonType.Daily)
                //        ? GetDungeonDailyBySeq(sequence) : GetDungeonSpecialBySeq(sequence);
                //    if (realmInfo != null && realmInfo.Count > 0)
                //        res = realmInfo[0].localizedname;
                //    break;

                //case RealmType.ActivityGuildSMBoss:
                //    if (mActivityGuildSMBoss.ContainsKey("guildsmboss"))
                //        res = mActivityGuildSMBoss["guildsmboss"].localizedname;
                //    break;
            }
            return res;
        }

        /*public static EliteMapJson GetEliteMapByPlayerLevel(int playerLevel)
        {
            List<int> eligibleLvl = mEliteMap.Keys.Where(x => x <= playerLevel).ToList();
            int maxLvl = 0;
            if (eligibleLvl.Count > 0)
                maxLvl = eligibleLvl.Max();

            if (mEliteMap.ContainsKey(maxLvl))
                return mEliteMap[maxLvl];
            return null;
        }*/

        public static RealmJson GetPortalExitRealmInfo(string level)
        {
            RealmWorldJson realmWorldJson = GetWorldByName(level);
            if (realmWorldJson != null)
                return realmWorldJson;
            else
            {
                //todo not world level, test other case.
            }
            return null;
        }
    }
}
