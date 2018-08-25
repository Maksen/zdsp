using Kopio.JsonContracts;
using System.Collections.Generic;

namespace Zealot.Repository
{
    public static class TeleportNPCListRepo
    {
        public static Dictionary<string, int> mTeleportNPCList;
        public static Dictionary<int, TeleportNPCListJson> mTeleportNPCId;
        public static Dictionary<string, string> mTeleportNPCPModelPrefabPath;

        static TeleportNPCListRepo()
        {
            mTeleportNPCList = new Dictionary<string, int>();
            mTeleportNPCId = new Dictionary<int, TeleportNPCListJson>();
            mTeleportNPCPModelPrefabPath = new Dictionary<string, string>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mTeleportNPCId = gameData.TeleportNPCList;
            mTeleportNPCList.Clear();

            foreach (KeyValuePair<int, TeleportNPCListJson> entry in mTeleportNPCId)
            {
                string Archetype = entry.Value.archetype;
                mTeleportNPCList.Add(Archetype, entry.Key);
                //string Path = StaticNPCRepo.GetNPCArchetypePathByName(Archetype);
                //mTeleportNPCPModelPrefabPath.Add(Archetype, Path);
            }
        }

        public static TeleportNPCListJson GetInfoByArchetype(string archetype)
        {
            if (mTeleportNPCList.ContainsKey(archetype))
                return mTeleportNPCId[mTeleportNPCList[archetype]];
            return null;
        }

        public static TeleportNPCListJson GetInfoById(int id)
        {
            if (mTeleportNPCId.ContainsKey(id))
                return mTeleportNPCId[id];
            return null;
        }

        public static string GetNPCPrefabPath(string archetype)
        {
            if (mTeleportNPCPModelPrefabPath.ContainsKey(archetype))
                return mTeleportNPCPModelPrefabPath[archetype];
            return null;
        }

        //public static string GetNPCAssetContainerByArchetype(string archetype)
        //{
        //    var staticNPC =  StaticNPCRepo.GetStaticNPCByName(archetype);
        //    return staticNPC.containerprefab;
        //}
    }
}
