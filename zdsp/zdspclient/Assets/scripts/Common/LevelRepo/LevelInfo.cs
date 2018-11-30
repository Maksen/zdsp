using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;

namespace Zealot.Entities
{
    public class LevelInfo
    {
        public Dictionary<string, Dictionary<int, ServerEntityJson>> mEntities { get; set; }
    }

    public class LocationData
    {
        public string mName;
        public string mLevel;     
        public Vector3 mPosition;
        public Vector3 mForward;

        public LocationData(string name, string level, Vector3 pos, Vector3 forward)
        {
            mName = name;
            mLevel = level;
            mPosition = pos;
            mForward = forward;
        }
    }

    public class BossLocationData : LocationData
    {
        public int mArchetypeID;

        public BossLocationData(Vector3 pos, string level, int archetypeid) : base("", level, pos, Vector3.forward)
        {
            mArchetypeID = archetypeid;
        }
    }

    public class PortalEntryData
    {
        public string mName;
        public string mExitName;
        public string mLevel;    
        public Vector3 mPosition;
        
        public PortalEntryData(string name, string exitName, string level, Vector3 pos)
        {
            mName = name;
            mExitName = exitName;
            mLevel = level;
            mPosition = pos;
        }
    }

    public static class PortalInfos
    {
        public static Dictionary<string, PortalEntryData> mEntries;
        public static Dictionary<string, LocationData> mExits;
        
        static PortalInfos()
        {
            mEntries = new Dictionary<string, PortalEntryData>();
            mExits = new Dictionary<string, LocationData> ();
        }

        public static void Clear()
        {
            mEntries.Clear();
            mExits.Clear();
        }

        public static List<PortalEntryData> GetLevelPortals(string plevel)
        {
            List<PortalEntryData> res = new List<PortalEntryData>();
            foreach (KeyValuePair<string, PortalEntryData> entry in mEntries)
            {
                if (entry.Value.mLevel == plevel)
                {
                    res.Add(entry.Value);
                }
            }
            return res; 
        }

        public static List<LocationData> GetLevelPortalExits(string plevel)
        {
            List<LocationData> res = new List<LocationData>();
            foreach (KeyValuePair<string, LocationData> entry in mExits)
            {
                if (entry.Value.mLevel == plevel)
                {
                    res.Add(entry.Value);
                }
            }
            return res;
        }

        public static void AddPortal(string level, LevelInfo info)
        {
            Dictionary<int, ServerEntityJson> portalEntries;
            if (info.mEntities.TryGetValue("PortalEntryJson", out portalEntries))
            {
                string entryName;
                foreach(PortalEntryJson entry in portalEntries.Values)
                {
                    entryName = entry.myName;
                    if (string.IsNullOrEmpty(entryName))
                        continue;
                    mEntries.Add(entryName, new PortalEntryData(entryName, entry.exitName, level, entry.position));
                }
            }

            Dictionary<int, ServerEntityJson> portalExits;
            if (info.mEntities.TryGetValue("PortalExitJson", out portalExits))
            {
                string exitName;
                foreach(PortalExitJson entry in portalExits.Values)
                {
                    exitName = entry.myName;
                    if(string.IsNullOrEmpty(exitName))
                        continue;
                    mExits.Add(exitName, new LocationData(exitName, level, entry.position, entry.forward));
                }
            }
        }
    }

    public class SafeZoneData
    {
        public float radius;
        public Vector3 pos;
        public Vector3 size;
       
        public SafeZoneData(Vector3 _pos, float rad, Vector3 _size)
        {
            radius = rad;
            pos = _pos;
            size = _size;
        }
    }

    public static class SafeZoneInfo
    {
        public static Dictionary<int,SafeZoneData> mySafeZoneData;
        static SafeZoneInfo()
        {
            mySafeZoneData = new Dictionary<int, SafeZoneData>();
        }

        public static void Clear()
        {
            mySafeZoneData.Clear();          
        }

        public static void AddSafeZone(string level, LevelInfo info)
        {
            Dictionary<int, ServerEntityJson> mysafezoneJson;
            if (info.mEntities.TryGetValue("SafeZoneJson", out mysafezoneJson))
            {              
                foreach (SafeZoneJson zone in mysafezoneJson.Values)
                {
                    mySafeZoneData.Add(zone.ObjectID, new SafeZoneData(zone.position, zone.safeZoneRadius, zone.size));//zone.uniqueName
                }
            }
        }
    }

    public class MapInfoJson : ServerEntityJson
    {
        public Vector3 centerPoint;
        public Vector3 mapScale;
        public float width;
        public float height;
    }

    public static class NPCPosMap
    {
        public static Dictionary<string, Dictionary<string, List<Vector3>>> mMonsterLocationMap;
        public static Dictionary<string, Dictionary<string, List<Vector3>>> mStaticNPCLocationMap;
        public static Dictionary<string, Dictionary<string, List<Vector3>>> mStaticGuideLocationMap;

        static NPCPosMap()
        {
            mMonsterLocationMap = new Dictionary<string, Dictionary<string, List<Vector3>>>();
            mStaticNPCLocationMap = new Dictionary<string, Dictionary<string, List<Vector3>>>();
            mStaticGuideLocationMap = new Dictionary<string, Dictionary<string, List<Vector3>>>();
        }

        public static void Clear()
        {
            mMonsterLocationMap.Clear();
            mStaticNPCLocationMap.Clear();
            mStaticGuideLocationMap.Clear();
        }

        public static Vector3 GetRandomSpawnerPosition(string lvl, string archetype)
        {
            Vector3 pos = Vector3.zero;
            if (mMonsterLocationMap.ContainsKey(archetype))
            {
                if(mMonsterLocationMap[archetype].ContainsKey(lvl))
                {
                    List<Vector3> listofspawners = mMonsterLocationMap[archetype][lvl];
                    if (listofspawners !=null && listofspawners.Count > 0)
                    {
                        int idx = GameUtils.RandomInt(0, listofspawners.Count - 1);
                        pos = listofspawners[idx];
                    }
                }
            }
            return pos;
        }

        public static void AddNPCInfo(string level, LevelInfo info)
        {
            Dictionary<int, ServerEntityJson> monsterSpawners;
            if (info.mEntities.TryGetValue("MonsterSpawnerJson", out monsterSpawners))
            {
                foreach (MonsterSpawnerJson value in monsterSpawners.Values)
                {
                    string archetype = value.archetype;
                    if (string.IsNullOrEmpty(archetype))
                        continue;
                    if (!mMonsterLocationMap.ContainsKey(archetype))
                        mMonsterLocationMap.Add(archetype, new Dictionary<string, List<Vector3>>());
                    if (!mMonsterLocationMap[archetype].ContainsKey(level))
                        mMonsterLocationMap[archetype].Add(level, new List<Vector3>());
                    mMonsterLocationMap[archetype][level].Add(value.position);
                }
            }
            Dictionary<int, ServerEntityJson> personalMonsterSpawners;
            if (info.mEntities.TryGetValue("PersonalMonsterSpawnerJson", out personalMonsterSpawners))
            {
                foreach (PersonalMonsterSpawnerJson value in personalMonsterSpawners.Values)
                {
                    string archetype = value.archetype;
                    if (string.IsNullOrEmpty(archetype))
                        continue;
                    if (!mMonsterLocationMap.ContainsKey(archetype))
                        mMonsterLocationMap.Add(archetype, new Dictionary<string, List<Vector3>>());
                    if (!mMonsterLocationMap[archetype].ContainsKey(level))
                        mMonsterLocationMap[archetype].Add(level, new List<Vector3>());
                    mMonsterLocationMap[archetype][level].Add(value.position);
                }
            }
            Dictionary<int, ServerEntityJson> staticNPCSpawners;
            if (info.mEntities.TryGetValue("StaticClientNPCSpawnerJson", out staticNPCSpawners))
            {
                foreach (StaticClientNPCSpawnerJson value in staticNPCSpawners.Values)
                {
                    string archetype = value.archetype;
                    if (string.IsNullOrEmpty(archetype))
                        continue;
                    if (!mStaticNPCLocationMap.ContainsKey(archetype))
                        mStaticNPCLocationMap.Add(archetype, new Dictionary<string, List<Vector3>>());
                    if (!mStaticNPCLocationMap[archetype].ContainsKey(level))
                        mStaticNPCLocationMap[archetype].Add(level, new List<Vector3>());
                    mStaticNPCLocationMap[archetype][level].Add(value.position);
                }
            }
            Dictionary<int, ServerEntityJson> staticGuideSpawners;
            if (info.mEntities.TryGetValue("StaticClientGuideSpawnerJson", out staticGuideSpawners))
            {
                foreach (StaticClientGuideSpawnerJson value in staticGuideSpawners.Values)
                {
                    string archetype = value.archetype;
                    if (string.IsNullOrEmpty(archetype))
                        continue;
                    if (!mStaticGuideLocationMap.ContainsKey(archetype))
                        mStaticGuideLocationMap.Add(archetype, new Dictionary<string, List<Vector3>>());
                    if (!mStaticGuideLocationMap[archetype].ContainsKey(level))
                        mStaticGuideLocationMap[archetype].Add(level, new List<Vector3>());
                    mStaticGuideLocationMap[archetype][level].Add(value.position);
                }
            }
        }

        private static bool FindNearest(Dictionary<string, Dictionary<string, List<Vector3>>> map, string archetype, string myLevel, Vector3 myPos, ref string level, ref Vector3 pos)
        {
            if (map.ContainsKey(archetype))
            {
                if (map[archetype].ContainsKey(myLevel))
                {
                    level = myLevel;
                    pos = FindNearestPos(map[archetype][myLevel], myPos);
                }
                else
                {
                    foreach (var kvp in map[archetype])
                    {
                        level = kvp.Key;
                        pos = map[archetype][level][0];
                        break;
                    }
                }
                return true;
            }
            return false;
        }

        public static bool FindNearestMonster(string archetype, string myLevel, Vector3 myPos, ref string level, ref Vector3 pos)
        {
            return FindNearest(mMonsterLocationMap, archetype, myLevel, myPos, ref level, ref pos);
        }

        public static bool FindNearestStaticNPC(string archetype, string myLevel, Vector3 myPos, ref string level, ref Vector3 pos)
        {
            return FindNearest(mStaticNPCLocationMap, archetype, myLevel, myPos, ref level, ref pos);
        }

        public static bool FindNearestStaticGuide(string archetype, string myLevel, Vector3 myPos, ref string level, ref Vector3 pos)
        {
            return FindNearest(mStaticGuideLocationMap, archetype, myLevel, myPos, ref level, ref pos);
        }

        private static Vector3 FindNearestPos(List<Vector3> positions, Vector3 myPos)
        {
            float closestDistSq = float.MaxValue;
            Vector3 closestPos = myPos;

            int count = positions.Count;
            for (int i = 0; i < count; ++i)
            {
                Vector3 pos = positions[i];
                float distSq = (pos - myPos).sqrMagnitude;
                if (distSq < closestDistSq)
                {
                    closestPos = pos;
                    closestDistSq = distSq;
                }
            }
            return closestPos;
        }
    }
}
