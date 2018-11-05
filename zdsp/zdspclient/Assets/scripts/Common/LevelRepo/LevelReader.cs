using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Zealot.Repository;
using Zealot.Common;

namespace Zealot.Entities
{
    public static class LevelReader
    {
        public static Dictionary<string, LevelInfo> levels;
        public static Dictionary<int, BossLocationData> mSpecialBossLocationDataMap;
        public static Dictionary<BossType, Dictionary<int, int>> mSpecialBossByType;
        public static Dictionary<int, string> mInteractiveTriggerMap;
        public static bool IsClientInitialized = false;

        static LevelReader()
        {
            levels = new Dictionary<string, LevelInfo>();
            mSpecialBossLocationDataMap = new Dictionary<int, BossLocationData>();
            mSpecialBossByType = new Dictionary<BossType, Dictionary<int, int>>();
            mInteractiveTriggerMap = new Dictionary<int, string>();
            var boss_categories = Enum.GetValues(typeof(BossType));
            foreach (BossType entry in boss_categories)
                mSpecialBossByType.Add(entry, new Dictionary<int, int>());
        }

        public static void InitClient(Dictionary<string, string> levelAssets)
        {
            PortalInfos.Clear();
            SafeZoneInfo.Clear();
            NPCPosMap.Clear();
            levels.Clear();
            mSpecialBossLocationDataMap.Clear();
            foreach (var entry in mSpecialBossByType)
                entry.Value.Clear();
            foreach (KeyValuePair<string, string> kvp in levelAssets)
            {
                string levelName = kvp.Key;
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                jsonSetting.Converters.Add(new Vector3Converter());
                try
                {
                    LevelInfo lvlInfo = JsonConvert.DeserializeObject<LevelInfo>(kvp.Value, jsonSetting);
                    PortalInfos.AddPortal(levelName, lvlInfo);
                    SafeZoneInfo.AddSafeZone(levelName, lvlInfo);
                    NPCPosMap.AddNPCInfo(levelName, lvlInfo);
                    AddSpecialBossLocationData(levelName, lvlInfo);
                    levels[levelName] = lvlInfo;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogErrorFormat("Level json [{0}] out of data, please remove unused stuff", levelName);
                }
            }
            IsClientInitialized = true;
        }

        
        public static void InitServer(string assemblypath)
        {
            string prefix = "../levels/";
            string curdir = Path.Combine(assemblypath, prefix);
            string[] files = Directory.GetFiles(curdir, "*.json");
            foreach (string path in files)
            {
                string levelName = Path.GetFileNameWithoutExtension(path);
                using (StreamReader file = File.OpenText(path))
                {
                    string content = file.ReadToEnd();
                    content = content.Replace("Assembly-CSharp", "Common");
                    JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                    jsonSetting.Converters.Add(new Vector3Converter());

                    try
                    {
                        LevelInfo lvlInfo = JsonConvert.DeserializeObject<LevelInfo>(content, jsonSetting);
                        PortalInfos.AddPortal(levelName, lvlInfo);
                        SafeZoneInfo.AddSafeZone(levelName, lvlInfo);
                        AddSpecialBossLocationData(levelName, lvlInfo);
                        AddInteractiveTriggerMap(levelName, lvlInfo);
                        levels[levelName] = lvlInfo;
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }

        public static void AddSpecialBossLocationData(string level, LevelInfo info)
        {
            Dictionary<int, ServerEntityJson> aSpecialBossSpawnerJson;
            if (info.mEntities.TryGetValue("SpecialBossSpawnerJson", out aSpecialBossSpawnerJson))
            {
                string entryName;
                foreach (SpecialBossSpawnerJson entry in aSpecialBossSpawnerJson.Values)
                {
                    entryName = entry.archetype;                    
                    if (string.IsNullOrEmpty(entryName))
                        continue;                   
                    var boss_info = SpecialBossRepo.GetInfoByName(entryName);                    
                    if (boss_info != null)
                    {
                        mSpecialBossByType[boss_info.bosstype].Add(boss_info.sequence, boss_info.id);
                        mSpecialBossLocationDataMap.Add(boss_info.id, new BossLocationData(entry.position, level, boss_info.archetypeid));
                    }
                }
            }
        }

        public static Dictionary<int, int> GetSpecialBossByType(BossType type)
        {
            return mSpecialBossByType[type];
        }

        public static BossLocationData GetSpecialBossLocationData(int id)
        {
            if (mSpecialBossLocationDataMap.ContainsKey(id))
                return mSpecialBossLocationDataMap[id];
            return null;
        }

        public static LevelInfo GetLevel(string name)
        {
            if (levels.ContainsKey(name))
                return levels[name];
            return null;
        }

        public static void AddInteractiveTriggerMap(string level, LevelInfo info)
        {
            Dictionary<int, ServerEntityJson> aInteractiveTriggerJson;
            if (info.mEntities.TryGetValue("InteractiveTriggerJson", out aInteractiveTriggerJson))
            {
                foreach (InteractiveTriggerJson entry in aInteractiveTriggerJson.Values)
                {
                    mInteractiveTriggerMap.Add(entry.ObjectID, level);
                }
            }
        }
    }
}
