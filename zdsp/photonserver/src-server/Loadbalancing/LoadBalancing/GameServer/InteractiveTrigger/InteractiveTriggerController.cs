using System.Collections.Generic;
using Zealot.Server.Entities;
using Zealot.Server.Rules;

namespace Photon.LoadBalancing.GameServer
{
    public class InteractiveTriggerController
    {
        public static Dictionary<int, List<int>> interactiveEntity = new Dictionary<int, List<int>>();
        public static Dictionary<int, int> objectIdToPid = new Dictionary<int, int>();

        public InteractiveTriggerController()
        {
            
        }

        public void Init(int levelId)
        {
            if (levelId != -1)
                InteractiveTriggerRule.Init(levelId);
        }

        public void LoadSceneData()
        {
            if(interactiveEntity.Count != 0)
            {
                return;
            }

            Dictionary<int, int> levelEntityList = Zealot.Entities.LevelReader.mInteractiveTriggerMap;
            foreach (KeyValuePair<int, int> entry in levelEntityList)
            {
                int sceneId = entry.Value;
                int key = entry.Key;
                if (!interactiveEntity.ContainsKey(sceneId))
                {
                    interactiveEntity.Add(sceneId, new List<int>());
                }
                interactiveEntity[sceneId].Add(key);
            }
        }

        public static void AddEntityToPid(InteractiveGate pid)
        {
            InteractiveTriggerRule.interactiveEntity.Add(pid.GetPersistentID(), pid);
            objectIdToPid.Add(pid.mPropertyInfos.ObjectID, pid.GetPersistentID());
        }

        public static List<int> GetSceneEntities(int sceneId)
        {
            List<int> objectsId = interactiveEntity[sceneId];
            List<int> list = new List<int>();
            for (int i = 0; i < objectsId.Count; ++i)
            {
                list.Add(objectIdToPid[objectsId[i]]);
            }
            return list;
        }
    }
}
