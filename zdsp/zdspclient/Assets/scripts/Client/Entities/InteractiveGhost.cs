using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Zealot.Common.Entities;
using Zealot.Repository;

namespace Zealot.Client.Entities
{
    public class InteractiveGhost : NetEntityGhost
    {
        public GameObject entityObj;

        public InteractiveGhost() : base()
        {
            EntityType = EntityType.InteractiveTrigger;
        }

        public void Init(string prefab, string parent, Vector3 pos, Vector3 rota)
        {
            var prefabObj = AssetManager.LoadSceneNPC(prefab);
            if (prefabObj != null)
            {
                SpawnEntity(prefabObj, parent, pos, rota, new Vector3(1, 1, 1), true);
            }
            else
            {
                prefabObj = AssetDatabase.LoadAssetAtPath(AssetLoader.GetLoadString("Assets", prefab), typeof(GameObject)) as GameObject;
                if (prefabObj != null)
                    SpawnEntity(prefabObj, parent, pos, rota, ScenesModelRepo.GetScenesModelScale(prefab));
                else
                    Debug.LogFormat("Interactive object doesn't found. {0}. {1}.", prefabObj, prefab);
            }
        }

        void SpawnEntity(GameObject npc, string parent, Vector3 pos, Vector3 rota, Vector3 scal, bool isNpc = false)
        {
            AnimObj = UnityEngine.Object.Instantiate(npc);
            AnimObj.transform.position = pos;
            Vector3 lookRota = new Vector3(rota.x, pos.y, rota.z);
            AnimObj.transform.LookAt(lookRota);
            AnimObj.transform.localScale = scal;
            AnimObj.transform.SetParent(GameObject.Find(parent).transform);
            AnimObj.layer = LayerMask.NameToLayer("Entities");
            AnimObj.AddComponent<InteractiveEntities>();
            entityObj = AnimObj;

            if (isNpc)
            {
                base.Init();
            }
            else
            {
                GameObjectToEntityRef entity = AnimObj.AddComponent<GameObjectToEntityRef>();
                entity.mParentEntity = this;
            }
        }
    }
}
