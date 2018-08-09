using Kopio.JsonContracts;
using UnityEngine;
using Zealot.Common.Entities;

namespace Zealot.Client.Entities
{
    public class CompanionGhost : BaseClientEntity
    {
        protected string mModelPath;
        private StaticNPCJson mNPCJson;
        private PlayerGhost mParent;

        public CompanionGhost()
        {
            EntityType = EntityType.CompanionGhost;
        }

        public void Init(StaticNPCJson npcjson, Vector3 pos, Vector3 forward, PlayerGhost player)
        {
            mNPCJson = npcjson;
            mModelPath = npcjson.containerprefabpath;
            mParent = player;
            Position = pos;
            Forward = forward;

            base.Init();
            OnAnimObjLoaded(AssetManager.LoadAsset<GameObject>(mModelPath));
        }

        public override void OnAnimObjLoaded(Object asset)
        {
            if (asset != null)
                AnimObj = (GameObject)Object.Instantiate(asset);
            InitAnimObj();
        }

        public void InitAnimObj()
        {
            InitEntityComponents();

            AnimObj.transform.position = Position;
            AnimObj.transform.forward = Forward;
            AnimObj.tag = "NPC";
            AnimObj.name = mNPCJson.archetype;

            ClientUtils.SetLayerRecursively(AnimObj, LayerMask.NameToLayer("Entities"));

            Show(true);
            ShowEffect(true);
            mShadow.SetActive(true);
        }

        public int GetNpcId()
        {
            return mNPCJson.id;
        }

        public void UpdatePosition(Vector3 pos, Vector3 forward)
        {
            if (AnimObj != null)
            {
                AnimObj.transform.position = Position = pos;
                AnimObj.transform.forward = Forward = forward;
            }
        }

        public override void Update(long dt)
        {
            //if (Vector3.Distance(transform.position, currentVec) <= 0.1f)
            //{
                
            //}
            //else
            //{

            //}
        }
    }
}
