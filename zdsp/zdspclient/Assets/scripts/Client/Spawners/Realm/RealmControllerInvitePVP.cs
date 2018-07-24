using UnityEngine;
using System.Collections.Generic;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Realm/RealmControllerInvitePVP")]
    class RealmControllerInvitePVP : RealmController
    {
        [Tooltip("player start position")]
        public Transform[] SpawnPt;

        public override string[] Events
        {
            get
            {
                return new string[] { "OnRealmStart" };
            }
        }

        public override ServerEntityJson GetJson()
        {
            var jsonclass = new RealmControllerInvitePVPJson();
            GetJson(jsonclass);
            return jsonclass;
        }

        public void GetJson(RealmControllerInvitePVPJson jsonclass)
        {
            int count = SpawnPt.Length;
            Vector3[] spawnPos = new Vector3[count];
            for (int index = 0; index < count; index++)
            {
                spawnPos[index] = SpawnPt[index].position;
            }
            jsonclass.spawnPos = spawnPos;
            base.GetJson(jsonclass);
        }
    }
}

