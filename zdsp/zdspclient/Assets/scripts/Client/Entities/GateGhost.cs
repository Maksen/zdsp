using UnityEngine;
using Zealot.Common.Entities;

namespace Zealot.Client.Entities
{
    public class GateGhost : BaseNetEntityGhost
    {        
        public GateGhost() : base()
        {
            EntityType = EntityType.GateGhost;
        }

        public void Init(float width, float height, string prefab, Vector3 pos, Vector3 forward)
        {
            var gatePrefab = AssetManager.LoadSceneNPC(prefab);
            if (gatePrefab != null)
            {
                AnimObj = (GameObject)UnityEngine.Object.Instantiate(gatePrefab);
                AnimObj.transform.position = pos;
                AnimObj.transform.rotation = Quaternion.identity;
                base.Init();
                //AnimObj.layer = LayerMask.NameToLayer("Entities"); becease we need the collider to block player.
                Position = pos;
                Forward = forward;
                AnimObj.transform.localScale = new Vector3(width, height, 1);
            }
            else
                Debug.LogErrorFormat("{0} not found in SceneNPCContainer", prefab);
        }
    }
}
