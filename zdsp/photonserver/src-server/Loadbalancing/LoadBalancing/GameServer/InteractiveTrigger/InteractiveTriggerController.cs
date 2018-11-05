using System.Collections.Generic;
using Zealot.Server.Entities;
using Zealot.Server.Rules;

namespace Photon.LoadBalancing.GameServer
{
    public class InteractiveTriggerController
    {
        public static Dictionary<string, List<int>> interactiveEntity = new Dictionary<string, List<int>>();
        static Dictionary<int, string> searchEntityInScene = new Dictionary<int, string>();
        static Dictionary<int, int> objectIdToPid = new Dictionary<int, int>();

        public InteractiveTriggerController()
        {
        }

        public void Init()
        {
            InteractiveTriggerRule.Init();
        }

        public void InitController()
        {
            if (interactiveEntity.Count != 0)
            {
                return;
            }

            Dictionary<int, string> levelEntityList = Zealot.Entities.LevelReader.mInteractiveTriggerMap;
            foreach (KeyValuePair<int, string> entry in levelEntityList)
            {
                string sceneName = entry.Value;
                int key = entry.Key;
                if (!interactiveEntity.ContainsKey(sceneName))
                {
                    interactiveEntity.Add(sceneName, new List<int>());
                }
                interactiveEntity[sceneName].Add(key);
                searchEntityInScene.Add(key, sceneName);
            }
        }

        public List<int> CurrentSceneEntities(string sceneName)
        {
            return interactiveEntity[sceneName];
        }

        public static void AddEntityToPid(InteractiveGate pid)
        {
            InteractiveTriggerRule.interactiveEntity.Add(pid);
            objectIdToPid.Add(pid.mPropertyInfos.ObjectID, pid.GetPersistentID());
        }
    }
}
