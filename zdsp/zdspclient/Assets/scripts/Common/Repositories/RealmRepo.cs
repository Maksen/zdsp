using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class RealmRepo
    {
        public static Dictionary<int, RealmJson> mIdMap;
        public static Dictionary<MapType, string> mMapCategory;
        public static Dictionary<string, RealmWorldJson> mRealmWorldByName;
        public static Dictionary<MapType, List<RealmWorldJson>> mRealmWorldByMapType;
        public static Dictionary<DungeonType, List<Dictionary<DungeonDifficulty, DungeonJson>>> mDungeons; // DungeonType <- List of dungeons
        public static Dictionary<DungeonType, Dictionary<int, byte>> mDungeonDaysOpen; // DungeonType <- (Seq <- daysopen)
        public static TutorialJson mTutorialInfo;

        static RealmRepo()
        {
            mIdMap = new Dictionary<int, RealmJson>();
            mMapCategory = new Dictionary<MapType, string>();
            mRealmWorldByName = new Dictionary<string, RealmWorldJson>();
            mRealmWorldByMapType = new Dictionary<MapType, List<RealmWorldJson>>();
            mDungeons = new Dictionary<DungeonType, List<Dictionary<DungeonDifficulty, DungeonJson>>>();
            mDungeonDaysOpen = new Dictionary<DungeonType, Dictionary<int, byte>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mIdMap.Clear();
            mMapCategory.Clear();
            mRealmWorldByName.Clear();
            mRealmWorldByMapType.Clear();
            mDungeons.Clear();
            
            foreach (KeyValuePair<int, MapCategoryJson> kvp in gameData.MapCategory)
            {
                mMapCategory[kvp.Value.maptype] = kvp.Value.localizedname;
            }

            foreach (KeyValuePair<int, RealmWorldJson> kvp in gameData.RealmWorld)
            {
                RealmWorldJson realmWorldJson = kvp.Value;
                mIdMap.Add(kvp.Key, realmWorldJson);

                LevelJson level = LevelRepo.GetInfoById(realmWorldJson.level);
                if (level != null)
                    mRealmWorldByName[level.unityscene] = realmWorldJson;

                MapType maptype = realmWorldJson.maptype;
                if (!mRealmWorldByMapType.ContainsKey(maptype))
                    mRealmWorldByMapType[maptype] = new List<RealmWorldJson>();

                mRealmWorldByMapType[maptype].Add(kvp.Value);
            }
            
            Dictionary<MapType, List<RealmWorldJson>>.ValueCollection mRealmWorldByMapTypeVals = mRealmWorldByMapType.Values;
            foreach (List<RealmWorldJson> value in mRealmWorldByMapTypeVals)
            {
                value.Sort((x, y) => x.reqlvl.CompareTo(y.reqlvl));
            }

            foreach (KeyValuePair<int, DungeonJson> kvp in gameData.Dungeon)
            {
                DungeonJson dungeonJson = kvp.Value;
                mIdMap.Add(kvp.Key, dungeonJson);

                DungeonType dungeonType = dungeonJson.dungeontype;
                int sequence = dungeonJson.sequence;

                if (!mDungeonDaysOpen.ContainsKey(dungeonType))
                    mDungeonDaysOpen[dungeonType] = new Dictionary<int, byte>();
                mDungeonDaysOpen[dungeonType][sequence] = IsDungeonDaysOpen(dungeonJson);

                if (!mDungeons.ContainsKey(dungeonType))
                    mDungeons[dungeonType] = new List<Dictionary<DungeonDifficulty, DungeonJson>>();

                DungeonDifficulty difficulty = dungeonJson.difficulty;
                List<Dictionary<DungeonDifficulty, DungeonJson>> dungeonList = mDungeons[dungeonType];
                bool isExist = false;
                int dungeonCount = dungeonList.Count;
                for (int i = 0; i < dungeonCount; ++i)
                {
                    Dictionary<DungeonDifficulty, DungeonJson> dungeonDict = dungeonList[i];
                    if (dungeonDict.Values.First().sequence == sequence)
                    {
                        dungeonDict[difficulty] = dungeonJson;
                        isExist = true;
                        break;
                    } 
                }
                if (!isExist)
                    dungeonList.Add(new Dictionary<DungeonDifficulty, DungeonJson>() { { difficulty, dungeonJson } });
            }

            Dictionary<DungeonType, List<Dictionary<DungeonDifficulty, DungeonJson>>>.ValueCollection dungeonValues = mDungeons.Values;
            foreach (List<Dictionary<DungeonDifficulty, DungeonJson>> list in dungeonValues)
            {
                list.Sort((a, b) => a.Values.First().sequence.CompareTo(b.Values.First().sequence));
            }

            mTutorialInfo = gameData.Tutorial.Values.First();
            mIdMap.Add(mTutorialInfo.id, mTutorialInfo);
        }

        public static RealmJson GetInfoById(int id)
        {
            RealmJson realmJson;
            mIdMap.TryGetValue(id, out realmJson);
            return realmJson;
        }

        public static string GetLocalizedMapType(MapType type)
        {
            string localizedTxt = "";
            mMapCategory.TryGetValue(type, out localizedTxt);
            return localizedTxt;
        }

        public static List<RealmWorldJson> GetWorldListByMapType(MapType type)
        {
            List<RealmWorldJson> realmWorldList;
            mRealmWorldByMapType.TryGetValue(type, out realmWorldList);
            return realmWorldList;
        }
 
        public static RealmWorldJson GetWorldByName(string levelName)
        {
            RealmWorldJson realmWorldJson;
            mRealmWorldByName.TryGetValue(levelName, out realmWorldJson);
            return realmWorldJson;
        }

        public static bool IsWorld(string levelName)
        {
            return mRealmWorldByName.ContainsKey(levelName);
        }

        public static bool IsCity(string levelName)
        {
            RealmWorldJson realmWorldJson;
            if (mRealmWorldByName.TryGetValue(levelName, out realmWorldJson))
                return realmWorldJson.maptype == MapType.City;
            return false;
        }

        public static string GetCity(int progresslvl)
        {
            string cityName = "";
            List<RealmWorldJson> realmWorldList = GetWorldListByMapType(MapType.City);
            if (realmWorldList != null)
            {
                int count = realmWorldList.Count;
                for (int i = 0; i < count; ++i)
                {
                    RealmWorldJson realmWorldJson = realmWorldList[i];
                    if (realmWorldJson.reqlvl > progresslvl)
                        break;
                    cityName = LevelRepo.GetSceneById(realmWorldJson.level);
                }
            }

            return !string.IsNullOrEmpty(cityName) ? cityName : "handan";
        }

        public static byte IsDungeonDaysOpen(DungeonJson dungeonJson)
        {
            byte dayOpenData = 0;
            string openDate = dungeonJson.opendate;
            if (!string.IsNullOrEmpty(openDate))
            {
                DateTime dtOpenDate = DateTime.ParseExact(openDate, "MMdd", CultureInfo.InvariantCulture);
                string closeDate = dungeonJson.closedate;
                DateTime dtcloseDate = DateTime.ParseExact(closeDate, "MMdd", CultureInfo.InvariantCulture);
                int dayOfWeekStart = (int)dtOpenDate.DayOfWeek;
                int daysDiff = (dtcloseDate - dtOpenDate).Days + 1;
                if (daysDiff < 7)
                {
                    for (int i = 0; i < daysDiff; ++i)
                    {
                        int dayIndex = (dayOfWeekStart + i) % 7;
                        dayOpenData = (byte)GameUtils.SetBit(dayOpenData, dayIndex);
                    }
                }
                else
                    dayOpenData = 127;
            }
            else
            {
                if (dungeonJson.isopenday1)
                    dayOpenData = (byte)GameUtils.SetBit(dayOpenData, 1);
                if (dungeonJson.isopenday2)
                    dayOpenData = (byte)GameUtils.SetBit(dayOpenData, 2);
                if (dungeonJson.isopenday3)
                    dayOpenData = (byte)GameUtils.SetBit(dayOpenData, 3);
                if (dungeonJson.isopenday4)
                    dayOpenData = (byte)GameUtils.SetBit(dayOpenData, 4);
                if (dungeonJson.isopenday5)
                    dayOpenData = (byte)GameUtils.SetBit(dayOpenData, 5);
                if (dungeonJson.isopenday6)
                    dayOpenData = (byte)GameUtils.SetBit(dayOpenData, 6);
                if (dungeonJson.isopenday7)
                    dayOpenData = (byte)GameUtils.SetBit(dayOpenData, 0);
            }
            return dayOpenData;
        }

        public static byte GetDungeonDaysOpen(DungeonType type, int sequence)
        {
            byte daysOpen = 0;
            Dictionary<int, byte> dungeonDaysOpenByDay;
            if (mDungeonDaysOpen.TryGetValue(type, out dungeonDaysOpenByDay))
                dungeonDaysOpenByDay.TryGetValue(sequence, out daysOpen);
            return daysOpen;
        }

        public static Dictionary<DungeonDifficulty, DungeonJson> GetDungeonByTypeAndSeq(DungeonType type, int sequence)
        {
            List<Dictionary<DungeonDifficulty, DungeonJson>> dungeonByType;
            if (mDungeons.TryGetValue(type, out dungeonByType))
            {
                int count = dungeonByType.Count;
                for (int i = 0; i < count; ++i)
                {
                    if (dungeonByType[i].Values.First().sequence == sequence)
                        return dungeonByType[i];
                }
            }
            return null;
        }

        public static RealmJson GetPortalExitRealmInfo(string levelName)
        {
            RealmWorldJson realmWorldJson = GetWorldByName(levelName);
            if (realmWorldJson != null)
                return realmWorldJson;
            else
            {
                //TODO: not world level, test other case.
            }
            return null;
        }
    }
}
