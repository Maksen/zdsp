using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Realm/RealmControllerWorldBoss")]
    public class RealmControllerWorldBoss : RealmController
    {
        public override ServerEntityJson GetJson()
        {
            RealmControllerWorldBossJson jsonclass = new RealmControllerWorldBossJson();
            GetJson(jsonclass);
            return jsonclass;
        }
    }
}
