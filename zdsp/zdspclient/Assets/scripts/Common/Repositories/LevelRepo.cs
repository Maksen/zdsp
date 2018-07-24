using System.Collections.Generic;
using Kopio.JsonContracts;

namespace Zealot.Repository
{
    public static class LevelRepo
    {
        public static Dictionary<string, int> mNameMap;
        public static Dictionary<int, LevelJson> mIdMap;

        static LevelRepo()
        {
            mNameMap = new Dictionary<string, int>();
            mIdMap = new Dictionary<int, LevelJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mIdMap = gameData.Level;
            mNameMap.Clear();
            foreach (KeyValuePair<int, LevelJson> entry in gameData.Level)
            {
                mNameMap.Add(entry.Value.unityscene, entry.Key);
            }
        }

        public static LevelJson GetInfoByName(string name)
        {
            if (mNameMap.ContainsKey(name))
                return mIdMap[mNameMap[name]];
            return null;
        }

        public static LevelJson GetInfoById(int id)
        {
            if (mIdMap.ContainsKey(id))
                return mIdMap[id];
            return null;
        }

        public static string GetLevelDataString()
        {
            string ret = "";
            foreach (var level in mIdMap)
            {
                ret += level.Value.excelname.ToString() + " " + level.Key + "|";
            }
            return ret;
        }

        /// <summary>
        /// Get unity scenes manifest for downloading of asset bundles.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllScenes()
        {
            List<string> result = new List<string>();
            foreach(KeyValuePair<int, LevelJson> kvp in mIdMap)
            {
                result.Add(kvp.Value.unityscene);
            }
            return result;
        }
    }
}

