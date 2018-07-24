using UnityEngine;
using System;
using System.Collections.Generic;
using Zealot.Entities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/CutSceneBroadcaster")]
    public class CutSceneBroadcaster : ServerEntity {

        public override string[] Triggers
        {
            get
            {
                return new string[] { "Play" };
            }
        }

        public CutsceneEntity cutsceneEntity;

        void Start()
        {
            if(PhotonNetwork.connected && cutsceneEntity != null)
            {
                GameInfo.AddPlayerSpawnListener(OnPlayerSpawned);                
            }
        }

        public void OnPlayerSpawned()
        {
            GameInfo.gCombat.CutsceneManager.RegisterCutsceneBroadcaster(this);
        }

        public override ServerEntityJson GetJson()
		{
            CutSceneBroadcasterJson jsonclass = new CutSceneBroadcasterJson();
			GetJson (jsonclass);
			return jsonclass;
		}

        void OnDestroy()
        {
            cutsceneEntity = null;
        }
    }
}