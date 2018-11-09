using System.Collections.Generic;
using Kopio.JsonContracts;

namespace Zealot.Repository
{
    public static class ScenesModelRepo
    {
        static Dictionary<string, ScenesModelJson> scenesModelMap;

        //Remove this when container is create
        static Dictionary<string, UnityEngine.Vector3> scenesModelScale;

        static ScenesModelRepo()
        {
            scenesModelMap = new Dictionary<string, ScenesModelJson>();
            scenesModelScale = new Dictionary<string, UnityEngine.Vector3>();
        }

        public static void Init(GameDBRepo gameData)
        {
            foreach (KeyValuePair<int, ScenesModelJson> entry in gameData.ScenesModel)
            {
                string modelName = entry.Value.archetype;
                if (!scenesModelMap.ContainsKey(modelName))
                {
                    scenesModelMap.Add(modelName, entry.Value);
                }

                //Remove this when container is create
                string path = entry.Value.modelprefabpath;
                if (!scenesModelScale.ContainsKey(path))
                {
                    scenesModelScale.Add(path, new UnityEngine.Vector3(entry.Value.modelscalex, entry.Value.modelscaley, entry.Value.modelscalez));
                }
            }
        }

        //Remove this when container is create
        public static UnityEngine.Vector3 GetScenesModelScaleByPath(string path)
        {
            if (scenesModelScale.ContainsKey(path))
            {
                return scenesModelScale[path];
            }
            return new UnityEngine.Vector3(1, 1, 1);
        }

        public static Dictionary<string, ScenesModelJson> GetScenesModelMap()
        {
            return scenesModelMap;
        }

        public static ScenesModelJson GetScenesModelJson(string modelName)
        {
            if (scenesModelMap.ContainsKey(modelName))
            {
                return scenesModelMap[modelName];
            }

            return null;
        }

        public static bool isScenesModel(string modelName)
        {
            return scenesModelMap.ContainsKey(modelName) ? false : true;
        }

        public static UnityEngine.Vector3 GetScenesModelScale(string modelName)
        {
            if (scenesModelMap.ContainsKey(modelName))
            {
                ScenesModelJson modelJson = GetScenesModelJson(modelName);
                return new UnityEngine.Vector3(modelJson.modelscalex, modelJson.modelscaley, modelJson.modelscalez);
            }

            return new UnityEngine.Vector3(1, 1, 1);
        }
    }
}
