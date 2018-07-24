using System.Collections.Generic;
using UnityEngine;
using Zealot.Client.Actions;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Common.Entities;
using Zealot.Repository;

namespace Zealot.Client.Entities
{
    class EffectEntityGhost : StaticClientNPC
    {
        protected string prefabPath;
        public int mArchetypeID;

        private GameTimer removeTimer;

        public long mRemoveDur { get; private set; }

        public EffectEntityGhost()
        {
            this.EntityType = EntityType.ShopNPC;
        }

        public void Init(int archetypeid, Vector3 pos, Vector3 forward, float dur)
        {        
            this.prefabPath = StaticNPCRepo.GetNPCArchetypePathById(archetypeid);
            this.mArchetypeID = archetypeid;

            Position = pos;
            Forward = forward;
            mRemoveDur = (long)(dur * 1000);
            base.Init();
            OnAnimObjLoaded(AssetManager.LoadSceneNPC(prefabPath));
        }

        public override void InitAnimObj()
        {
            AnimObj.transform.position = Position;
            AnimObj.transform.forward = Forward;
            AnimObj.tag = "loot";
            AnimObj.name = prefabPath;
            base.InitAnimObj();
            ClientUtils.SetLayerRecursively(AnimObj, LayerMask.NameToLayer("Entities"));
            //var temp = this.EffectController.mEfxMap;
            //foreach (KeyValuePair<string, EffectRef> kvp in temp)
            //{
            //    if (kvp.Value != null)
            //    {
                     
            //        PlayEffect(kvp.Key, "", -1);
            //        break;
            //    }
            //}
            
            removeTimer = GameInfo.gCombat.mTimers.SetTimer(mRemoveDur, onTimeUp, null);//5 second to determine ending of combat
        }

        
        public override void OnRelevant()
        {
            //Do nothing
        }

        public override void OnIrrelevant()
        {
            //Remove myself and inform spawner                        
        }

        private void onTimeUp(object args)
        {
            EntitySystem.RemoveEntityByID(ID);
            
        }       

        public override int GetDisplayLevel()
        {
            return 1;
        }

        public override void OnRemove()
        {
            base.OnRemove();
            Debug.Log("EffectEntityGhost Remove Called.");
        }
    }
}
