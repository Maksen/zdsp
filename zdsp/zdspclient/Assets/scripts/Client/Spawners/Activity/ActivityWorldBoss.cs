using UnityEngine;
using System.Collections.Generic;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Activity/ActivityWorldBoss")]
    public class ActivityWorldBoss : RealmController
    {
        public override string[] Events
        {
            get
            {
                return new string[] { "OnRealmStart", "SetArchetype" };
            }
        }

        public override ServerEntityJson GetJson()
        {
            RealmControllerWorldBossJson jsonclass = new RealmControllerWorldBossJson();
            GetJson(jsonclass);
            return jsonclass;
        }
    }
}
