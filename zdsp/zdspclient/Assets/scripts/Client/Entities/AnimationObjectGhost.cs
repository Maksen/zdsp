using System;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Common.Datablock;
using Zealot.Repository;

namespace Zealot.Client.Entities
{
    public class AnimationObjectGhost : StaticNetEntityGhost
    {        
        public AnimationObjectGhost() : base()
        {
            EntityType = EntityType.AnimationObjectGhost;
        }

        public void Init(string modelprefab, Vector3 pos, Vector3 forward)
        {
            GameObject prefab = AssetManager.LoadSceneNPC(modelprefab);
            if (prefab != null)
            {
                AnimObj = (GameObject)UnityEngine.Object.Instantiate(prefab);
                AnimObj.transform.position = pos;
                AnimObj.transform.rotation = Quaternion.identity;
                base.Init();
                AnimObj.layer = LayerMask.NameToLayer("Entities");
                Position = pos;
                Forward = forward;

                ParticleSystem _effect = AnimObj.GetComponent<ParticleSystem>();
                if (_effect != null)
                    _effect.Play();
            }
        }     

        #region Active State        
        protected override void OnActiveEnter(string prevstate)
        {                     
            //PlayEffect("forward"); //TODO: play loot animation/effect
        }
        #endregion
    }
}
