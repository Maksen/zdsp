using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/ColliderTrigger")]
    public class ColliderTrigger : ServerEntityWithEvent
    {
        public bool activeOnStartup = true;
        public int triggerCount = 1;

        public override string[] Triggers
        {
            get { return new string[] { "On", "Off", "ResetCount" }; }
        }

        public override string[] Events
        {
            get { return new string[] { "OnEnter", "OnLeave" }; }
        }

        public override ServerEntityJson GetJson()
        {
            ColliderTriggerJson jsonclass = new ColliderTriggerJson();
            GetJson(jsonclass);
            return jsonclass;
        }
        
        public void GetJson(ColliderTriggerJson jsonclass)
        {
            jsonclass.activeOnStartup = activeOnStartup;
            jsonclass.count = triggerCount;
            base.GetJson(jsonclass);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("LocalPlayer"))
            {
                Debug.LogFormat("Player OnTriggerEnter spawner Id = {0}", EntityId);
                RPCFactory.CombatRPC.OnColliderTrigger(EntityId, true);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("LocalPlayer"))
            {
                Debug.LogFormat("Player OnTriggerExit spawner Id = {0}", EntityId);
                RPCFactory.CombatRPC.OnColliderTrigger(EntityId, false);
            }
        }
    }
}
