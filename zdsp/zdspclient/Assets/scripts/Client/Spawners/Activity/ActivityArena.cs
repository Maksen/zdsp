using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Activity/ActivityArena")]
    public class ActivityArena : RealmController
    {
        public Transform AIPlayerSpawnPt;

        public override string[] Events
        {
            get
            {
                return new string[] { "OnRealmStart" };
            }
        }

        public override ServerEntityJson GetJson()
        {
            RealmControllerArenaJson jsonclass = new RealmControllerArenaJson();
            GetJson(jsonclass);
            return jsonclass;
        }

        public void GetJson(RealmControllerArenaJson jsonclass)
        {
            jsonclass.aiPos = AIPlayerSpawnPt.position;
            jsonclass.aiForward = AIPlayerSpawnPt.forward;
            base.GetJson(jsonclass);
        }
    }
}
