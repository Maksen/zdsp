using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/CutsceneBroadcaster")]
    public class CutsceneBroadcaster : ServerEntityWithEvent
    {
        public CutsceneEntity cutsceneEntity;

        public override string[] Triggers
        {
            get { return new string[] { "Play" }; }
        }

        public override string[] Events
        {
            get { return new string[] { "OnCutsceneFinished" }; }
        }

        void Start()
        {
            if (PhotonNetwork.connected && cutsceneEntity != null)
                GameInfo.AddPlayerSpawnListener(OnPlayerSpawned);                
        }

        public void OnPlayerSpawned()
        {
            cutsceneEntity.OnCutsceneFinished.AddListener(OnCutsceneFinished);
            GameInfo.gCombat.CutsceneManager.RegisterCutsceneBroadcaster(this);
        }

        void OnCutsceneFinished()
        {
            RPCFactory.CombatRPC.OnCutsceneFinished(EntityId);
        }

        public override ServerEntityJson GetJson()
        {
            CutsceneBroadcasterJson jsonclass = new CutsceneBroadcasterJson();
            GetJson(jsonclass);
            return jsonclass;
        }

        void OnDestroy()
        {
            cutsceneEntity = null;
        }
    }
}
