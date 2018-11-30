using Kopio.JsonContracts;
using System;
using System.Linq;
using System.Collections.Generic;
using Zealot.Common;

namespace Zealot.Repository
{
    public class NPCLootLink
    {
        public List<int> lootList;
        public List<int> eventLootList;

        public static NPCLootLink Create(string loot, string eventLoot)
        {
            if (!string.IsNullOrEmpty(loot) || !string.IsNullOrEmpty(eventLoot))
            {
                var lootList = GameUtils.ParseStringToIntList(loot, ';');
                var eventLootList = GameUtils.ParseStringToIntList(eventLoot, ';');
                if (lootList.Count > 0 || eventLootList.Count > 0)
                {
                    NPCLootLink ret = new NPCLootLink();
                    ret.lootList = lootList;
                    ret.eventLootList = eventLootList;
                    return ret;
                }
            }
            return null;
        }

        public Dictionary<LootType, List<int>> GetLootGroupIDs(DateTime now)
        {
            Dictionary<LootType, List<int>> lootMap = new Dictionary<LootType, List<int>>();
            for (int index = 0; index < lootList.Count; ++index)
            {
                LootLink lootLink = LootRepo.GetLootLink(lootList[index]);
                if (lootLink == null || lootLink.gids.Count == 0)
                    continue;
                if (!lootMap.ContainsKey(lootLink.lootType))
                    lootMap.Add(lootLink.lootType, new List<int>());
                lootMap[lootLink.lootType].AddRange(lootLink.gids);
            }

            for (int index = 0; index < eventLootList.Count; ++index)
            {
                EventLootLink lootLink = LootRepo.GetEventLootLink(eventLootList[index]);
                if (lootLink == null || lootLink.gids.Count == 0 || !lootLink.IsInEvent(now))
                    continue;
                if (!lootMap.ContainsKey(lootLink.lootType))
                    lootMap.Add(lootLink.lootType, new List<int>());
                lootMap[lootLink.lootType].AddRange(lootLink.gids);
            }
            return lootMap;
        }
    }

    public static class CombatNPCRepo
    {
        public static Dictionary<int, CombatNPCJson> mIdMap;
        public static Dictionary<string, int> mNameMap;
        public static Dictionary<int, NPCLootLink> mNPCLootLinks;
        public static Dictionary<int, BossAIJson> mBossAIRaw;

        static CombatNPCRepo()
        {
            mNameMap = new Dictionary<string, int>();
            mNPCLootLinks = new Dictionary<int, NPCLootLink>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mNameMap.Clear();
            mNPCLootLinks.Clear();
            mIdMap = gameData.CombatNPC;
            mBossAIRaw = gameData.BossAI;

            foreach (KeyValuePair<int, CombatNPCJson> entry in mIdMap)
            {
                int id = entry.Key;
                CombatNPCJson combatNPCJson = entry.Value;
                mNameMap.Add(combatNPCJson.archetype, id);
                NPCLootLink npcLootLink = NPCLootLink.Create(combatNPCJson.lootids, combatNPCJson.eventlootids);
                if (npcLootLink != null)
                    mNPCLootLinks.Add(id, npcLootLink);
            }
        }

        public static BossAIJson GetBossAIByID(int id)
        {
            BossAIJson bossAI;
            mBossAIRaw.TryGetValue(id, out bossAI);
            return bossAI; 
        }

        public static CombatNPCJson GetNPCById(int id)
        {
            CombatNPCJson npcJson;
            mIdMap.TryGetValue(id, out npcJson);
            return npcJson;
        }

        public static CombatNPCJson GetNPCByArchetype(string archetype)
        {
            if (mNameMap.ContainsKey(archetype))
                return mIdMap[mNameMap[archetype]];
            return null;
        }

        public static NPCLootLink GetNPCLootLink(int id)
        {
            NPCLootLink npcLootLink;
            mNPCLootLinks.TryGetValue(id, out npcLootLink);
            return npcLootLink;
        }
    }

    public static class StaticNPCRepo
    {
        public static Dictionary<int, StaticNPCJson> mIdMap;
        public static Dictionary<string, int> mNameMap;
        
        static StaticNPCRepo()
        {
            mNameMap = new Dictionary<string, int>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mIdMap = gameData.StaticNPC;

            foreach (KeyValuePair<int, StaticNPCJson> kvp in mIdMap)
                mNameMap[kvp.Value.archetype] = kvp.Value.id;
        }

        public static StaticNPCJson GetNPCById(int id)
        {
            StaticNPCJson npcJson;
            mIdMap.TryGetValue(id, out npcJson);
            return npcJson;
        }

        public static StaticNPCJson GetNPCByArchetype(string archetype)
        {
            if (mNameMap.ContainsKey(archetype))
                return mIdMap[mNameMap[archetype]];
            return null;
        }

        public static string GetModelPrefabPathById(int id)
        {
            StaticNPCJson npcJson;
            if (mIdMap.TryGetValue(id, out npcJson))
                return npcJson.modelprefabpath;
            return "";
        }

        public static string GetModelPrefabPathByArchetype(string archetype)
        {
            if (mNameMap.ContainsKey(archetype))
                return mIdMap[mNameMap[archetype]].modelprefabpath;
            return "";
        }

        public static float[] ParseCameraPosInTalk(string cameraPos)
        {
            float[] result = new float[] { 0, 0, 0, 1 };
            string[] strArray = cameraPos.Split(';');
            if (strArray.Length == 4)
            {
                float.TryParse(strArray[0], out result[0]);
                float.TryParse(strArray[1], out result[1]);
                float.TryParse(strArray[2], out result[2]);
                float.TryParse(strArray[3], out result[3]);
            }
            return result;
        }
    }

    public static class RealmNPCGroupRepo
    {
        public static Dictionary<string, Dictionary<int, int>> mNameMap;

        static RealmNPCGroupRepo()
        {
            mNameMap = new Dictionary<string, Dictionary<int, int>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mNameMap.Clear();

            foreach (KeyValuePair<int, RealmNPCGroupJson> entry in gameData.RealmNPCGroup)
            {
                RealmNPCGroupJson realmNPCGroupJson = entry.Value;
                string groupName = realmNPCGroupJson.name;
                if (!mNameMap.ContainsKey(groupName))
                    mNameMap.Add(groupName, new Dictionary<int, int>());
                mNameMap[groupName].Add(realmNPCGroupJson.realmid, realmNPCGroupJson.archetypeid);
            }
        }

        public static Dictionary<int, int> GetNPCGroupByName(string groupName)
        {
            Dictionary<int, int> realmNPCGroup;
            mNameMap.TryGetValue(groupName, out realmNPCGroup);
            return realmNPCGroup;
        }

        public static CombatNPCJson GetNPCByGroupNameAndRealmId(string groupName, int realmId)
        {
            Dictionary<int, int> realmNPCGroup;
            if (mNameMap.TryGetValue(groupName, out realmNPCGroup))
            {
                int npcId;
                if (realmNPCGroup.TryGetValue(realmId, out npcId))
                    return CombatNPCRepo.GetNPCById(npcId);
            }
            return null;
        }
    }

    public static class SpecialBossRepo
    {
        public static Dictionary<int, SpecialBossJson> mIdMap; 
        public static Dictionary<string, int> mNameMap;
        public static Dictionary<BossType, List<SpecialBossJson>> mBossOrderedBySequence;
        public static int BossNoDmgRandomPos = 1800;
       
        static SpecialBossRepo()
        {            
            mNameMap = new Dictionary<string, int>();
            mBossOrderedBySequence = new Dictionary<BossType, List<SpecialBossJson>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mIdMap = gameData.SpecialBoss;
            mNameMap.Clear();
            mBossOrderedBySequence.Clear();
            BossNoDmgRandomPos = GameConstantRepo.GetConstantInt("Boss_NoDmgChangePos", 1800);

            Dictionary<BossType, List<SpecialBossJson>> _temp = new Dictionary<BossType, List<SpecialBossJson>>();
            foreach (KeyValuePair<int, SpecialBossJson> entry in gameData.SpecialBoss)
            {
                SpecialBossJson specialBossJson = entry.Value;
                mNameMap.Add(specialBossJson.name, entry.Key);
                BossType _category = specialBossJson.bosstype;
                if (!_temp.ContainsKey(_category))
                    _temp.Add(_category, new List<SpecialBossJson>());
                _temp[_category].Add(specialBossJson);
            }
            foreach(var kvp in _temp)
                mBossOrderedBySequence.Add(kvp.Key, kvp.Value.OrderBy(x => x.sequence).ToList());
        }

        public static SpecialBossJson GetInfoById(int id)
        {
            SpecialBossJson specialBossJson;
            mIdMap.TryGetValue(id, out specialBossJson);
            return specialBossJson;
        }

        public static SpecialBossJson GetInfoByName(string name)
        {
            if (mNameMap.ContainsKey(name))
                return mIdMap[mNameMap[name]];
            return null;
        }

        public static List<SpecialBossJson> GetOrderedBossIdsByType(BossType type)
        {
            List<SpecialBossJson> bossList;
            mBossOrderedBySequence.TryGetValue(type, out bossList);
            return bossList;
        }
    }

    public static class StaticGuideRepo
    {
        public static Dictionary<int, StaticGuideJson> mIdMap;
        public static Dictionary<string, int> mNameMap;

        static StaticGuideRepo()
        {
            mIdMap = new Dictionary<int, StaticGuideJson>();
            mNameMap = new Dictionary<string, int>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mIdMap = gameData.StaticGuide;

            foreach (KeyValuePair<int, StaticGuideJson> kvp in mIdMap)
            {
                mNameMap[kvp.Value.archetype] = kvp.Value.id;
            }  
        }

        public static StaticGuideJson GetNPCById(int id)
        {
            StaticGuideJson npcJson;
            mIdMap.TryGetValue(id, out npcJson);
            return npcJson;
        }

        public static StaticGuideJson GetNPCByArchetype(string archetype)
        {
            if (mNameMap.ContainsKey(archetype))
                return mIdMap[mNameMap[archetype]];
            return null;
        }
    }
}
