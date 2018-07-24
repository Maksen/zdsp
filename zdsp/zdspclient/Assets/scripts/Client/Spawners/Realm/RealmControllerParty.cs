using UnityEngine;
using System.Collections.Generic;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Realm/RealmControllerParty")]
    public class RealmControllerParty : RealmController
    {
        public override ServerEntityJson GetJson()
        {
            RealmControllerPartyJson jsonclass = new RealmControllerPartyJson();
            GetJson(jsonclass);
            return jsonclass;
        }
    }
}
