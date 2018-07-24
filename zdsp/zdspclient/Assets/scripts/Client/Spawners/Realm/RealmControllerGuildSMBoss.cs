using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Realm/RealmControllerGuildSMBoss")]
    public class RealmControllerGuildSMBoss : RealmController
    {
        public override string[] Triggers
        {
            get
            {
                return new string[] { "OnBossKilled" };
            }
        }

        public override string[] Events
        {
            get
            {
                return new string[] { "OnPlayerEnter" };
            }
        }

        public override ServerEntityJson GetJson()
        {
            RealmControllerGuildSMBossJson jsonclass = new RealmControllerGuildSMBossJson();
            GetJson(jsonclass);
            return jsonclass;
        }
    }
}
