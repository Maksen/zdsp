using System.Collections;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;
using System.Linq;
using System;

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
            for (int index = 0; index < lootList.Count; index++)
            {
                LootLink lootLink = LootRepo.GetLootLink(lootList[index]);
                if (lootLink == null || lootLink.gids.Count == 0)
                    continue;
                if (!lootMap.ContainsKey(lootLink.lootType))
                    lootMap.Add(lootLink.lootType, new List<int>());
                lootMap[lootLink.lootType].AddRange(lootLink.gids);
            }

            for (int index = 0; index < eventLootList.Count; index++)
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

    public static class NPCRepo
    {
        public static Dictionary<string, int> mNameMap;
        public static Dictionary<int, CombatNPCJson> mIdMap;
        public static Dictionary<int, NPCLootLink> mNPCLootLinks;
        public static Dictionary<int, BossAIJson> mBossAIRaw;
        static NPCRepo()
        {
            mNameMap = new Dictionary<string, int>();
            mIdMap = new Dictionary<int, CombatNPCJson>();
            mNPCLootLinks = new Dictionary<int, NPCLootLink>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mNameMap.Clear();
            mNPCLootLinks.Clear();
            mIdMap = gameData.CombatNPC;
            mBossAIRaw =  gameData.BossAI;
            foreach (KeyValuePair<int, CombatNPCJson> entry in gameData.CombatNPC) {
                mNameMap.Add(entry.Value.archetype, entry.Key);
                NPCLootLink npcLooLink = NPCLootLink.Create(entry.Value.loot, entry.Value.eventloot);
                if (npcLooLink != null)
                    mNPCLootLinks.Add(entry.Key, npcLooLink);
            }
        }

        public static BossAIJson GetBossAIByID(int id)
        {
            BossAIJson bossai;
            mBossAIRaw.TryGetValue(id, out bossai);
            return bossai; 
        }

        public static CombatNPCJson GetArchetypeByName(string name)
        {
            if (mNameMap.ContainsKey(name))
                return mIdMap[mNameMap[name]];
            return null;
        }

        public static CombatNPCJson GetArchetypeById(int id)
        {
            CombatNPCJson npcJson;
            mIdMap.TryGetValue(id, out npcJson);
            return npcJson;
        }

        public static NPCLootLink GetNPCLootLink(int id)
        {
            NPCLootLink npcLootLink;
            mNPCLootLinks.TryGetValue(id, out npcLootLink);
            return npcLootLink;
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
                string groupName = entry.Value.name;
                if (!mNameMap.ContainsKey(groupName))
                    mNameMap.Add(groupName, new Dictionary<int, int>());
                mNameMap[groupName].Add(entry.Value.realmid, entry.Value.archetypeid);
            }
        }

        public static Dictionary<int, int> GetArchetypeByName(string name)
        {
            if (mNameMap.ContainsKey(name))
                return mNameMap[name];
            return null;
        }

        public static CombatNPCJson GetArchetypeByNameAndRealmID(string name, int realmid)
        {
            if (mNameMap.ContainsKey(name) && mNameMap[name].ContainsKey(realmid))
            {
                int npcid = mNameMap[name][realmid];
                return NPCRepo.GetArchetypeById(npcid);
            }
            return null;
        }
    }

    public static class StaticNPCRepo
    {
        public static Dictionary<int, StaticNPCJson> mIdMap;
        public static Dictionary<string, StaticNPCJson> mNameMap;

        static StaticNPCRepo()
        {
            mIdMap = new Dictionary<int, StaticNPCJson>();
            mNameMap = new Dictionary<string, StaticNPCJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mIdMap = gameData.StaticNPC;
            foreach (var kvp in gameData.StaticNPC)
                mNameMap[kvp.Value.archetype] = kvp.Value;
        }

        public static StaticNPCJson GetStaticNPCById(int id)
        {
            if (mIdMap.ContainsKey(id))
                return mIdMap[id];
            return null;
        }

        public static StaticNPCJson GetStaticNPCByName(string archetype)
        {
            if (mNameMap.ContainsKey(archetype))
                return mNameMap[archetype];
            return null;
        }

        public static string GetNPCArchetypePathById(int id)
        {
            if (mIdMap.ContainsKey(id))
                return mIdMap[id].modelprefabpath;
            return "";
        }

        public static string GetNPCArchetypePathByName(string archetype)
        {
            if (mNameMap.ContainsKey(archetype))
                return mNameMap[archetype].modelprefabpath;
            return "";
        }

        public static float[] ParseCameraPosInTalk(string camerapos)
        {
            float[] camerastats = new float[4]; 
            string[] camera = camerapos.Split(';');
            camerastats[0] = float.Parse(camera[0]);
            camerastats[1] = float.Parse(camera[1]);
            camerastats[2] = float.Parse(camera[2]);
            camerastats[3] = float.Parse(camera[3]);
            return camerastats;
        }
    }

    public static class SpecialBossRepo
    {
        public static Dictionary<int, SpecialBossJson> mIdMap; 
        public static Dictionary<string, int> mNameMap;
        public static Dictionary<BossCategory, List<SpecialBossJson>> mBossOrderedBySequence;
        public static int BossNoDmgRandomPos = 1800;
       
        static SpecialBossRepo()
        {            
            mIdMap = new Dictionary<int, SpecialBossJson>();
            mNameMap = new Dictionary<string, int>();
            mBossOrderedBySequence = new Dictionary<BossCategory, List<SpecialBossJson>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            BossNoDmgRandomPos = GameConstantRepo.GetConstantInt("Boss_NoDmgChangePos", 1800);
            mIdMap = gameData.SpecialBoss;
            mNameMap.Clear();
            mBossOrderedBySequence.Clear();
            Dictionary<BossCategory, List<SpecialBossJson>> _temp = new Dictionary<BossCategory, List<SpecialBossJson>>();
            foreach (KeyValuePair<int, SpecialBossJson> entry in gameData.SpecialBoss)
            {
                mNameMap.Add(entry.Value.name, entry.Key);
                BossCategory _category = entry.Value.category;
                if (!_temp.ContainsKey(_category))
                    _temp.Add(_category, new List<SpecialBossJson>());
                _temp[_category].Add(entry.Value);
            }
            foreach(var kvp in _temp)
                mBossOrderedBySequence.Add(kvp.Key, kvp.Value.OrderBy(x => x.sequence).ToList());
        }

        public static SpecialBossJson GetInfoByName(string name)
        {
            if (mNameMap.ContainsKey(name))
                return mIdMap[mNameMap[name]];
            return null;
        }

        public static SpecialBossJson GetInfoById(int id)
        {
            if (mIdMap.ContainsKey(id))
                return mIdMap[id];
            return null;
        }

        public static List<SpecialBossJson> GetOrderedBossIdsByCategory(BossCategory category)
        {
            List<SpecialBossJson> ret;
            mBossOrderedBySequence.TryGetValue(category, out ret);
            return ret;
        }
    }
}

