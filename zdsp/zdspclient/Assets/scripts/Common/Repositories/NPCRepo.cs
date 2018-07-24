using System.Collections;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;
using System.Linq;

namespace Zealot.Repository
{
    public static class NPCRepo
    {
        public static Dictionary<string, int> mNameMap;
        public static Dictionary<int, CombatNPCJson> mIdMap;
        public static Dictionary<int, BossAIJson> mBossAIRaw;
        static NPCRepo()
        {
            mNameMap = new Dictionary<string, int>();
            mIdMap = new Dictionary<int, CombatNPCJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mNameMap.Clear();
            mIdMap = gameData.CombatNPC;
            mBossAIRaw =  gameData.BossAI;
            foreach (KeyValuePair<int, CombatNPCJson> entry in gameData.CombatNPC) {
                mNameMap.Add(entry.Value.archetype, entry.Key);
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
        public static Dictionary<BossCategory, List<int>> mBossOrderedBySequence;
        public static int BossNoDmgRandomPos = 1800;
       
        static SpecialBossRepo()
        {            
            mIdMap = new Dictionary<int, SpecialBossJson>();
            mNameMap = new Dictionary<string, int>();
            mBossOrderedBySequence = new Dictionary<BossCategory, List<int>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            BossNoDmgRandomPos = GameConstantRepo.GetConstantInt("Boss_NoDmgChangePos", 1800);
            mIdMap = gameData.SpecialBoss;
            mNameMap.Clear();
            mBossOrderedBySequence.Clear();
            Dictionary<BossCategory, Dictionary<int, int>> _temp = new Dictionary<BossCategory, Dictionary<int, int>>();
            foreach (KeyValuePair<int, SpecialBossJson> entry in gameData.SpecialBoss)
            {
                mNameMap.Add(entry.Value.name, entry.Key);
                BossCategory _category = entry.Value.category;
                if (!_temp.ContainsKey(_category))
                    _temp.Add(_category, new Dictionary<int, int>());
                _temp[_category][entry.Value.sequence] = entry.Value.id;
            }
            foreach(var kvp in _temp)
            {
                BossCategory _category = kvp.Key;
                mBossOrderedBySequence.Add(_category, new List<int>());
                foreach (int sequence in kvp.Value.Keys.ToList().OrderBy(x => x))
                    mBossOrderedBySequence[_category].Add(kvp.Value[sequence]);
            }
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

        public static List<int> GetOrderedBossIdsByCategory(BossCategory category)
        {
            List<int> ret;
            mBossOrderedBySequence.TryGetValue(category, out ret);
            return ret;
        }
    }
}

