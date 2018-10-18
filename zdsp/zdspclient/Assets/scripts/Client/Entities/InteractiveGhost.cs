using UnityEngine;
using UnityEditor;
using Zealot.Common.Entities;

namespace Zealot.Client.Entities
{
    public class InteractiveGhost : BaseNetEntityGhost
    {
        public InteractiveGhost() : base()
        {
            EntityType = EntityType.InteractiveTrigger;
        }

        public void Init(string prefab, string parent, Vector3 pos, Vector3 rota)
        {
            var prefabObj = AssetManager.LoadSceneNPC(prefab);
            if(prefabObj != null)
                SpawnNpc(prefabObj, parent, pos, rota);
            else
                Debug.LogError("Interactive object doesn't found.");
        }

        void SpawnNpc(GameObject npc, string parent, Vector3 pos, Vector3 rota)
        {
            AnimObj = Object.Instantiate(npc);
            AnimObj.transform.position = pos;
            Vector3 lookRota = new Vector3(rota.x, pos.y, rota.z);
            AnimObj.transform.LookAt(lookRota);
            base.Init();
            AnimObj.transform.SetParent(GameObject.Find(parent).transform);
            AnimObj.layer = LayerMask.NameToLayer("Entities");
            AnimObj.AddComponent<InteractiveEntities>();
        }
    }
}
