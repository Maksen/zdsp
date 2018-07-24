using UnityEngine;
using System.Collections.Generic;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Realm/RealmControllerTutorial")]
    public class RealmControllerTutorial : RealmController
    {
        public override ServerEntityJson GetJson()
        {
            RealmControllerTutorialJson jsonclass = new RealmControllerTutorialJson();
            GetJson(jsonclass);
            return jsonclass;
        }
    }
}
