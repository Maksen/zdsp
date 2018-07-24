using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Realm/RealmControllerDungeonDailySpecial")]
    public class RealmControllerDungeonDailySpecial : RealmController
    {
        public override ServerEntityJson GetJson()
        {
            RealmControllerDungeonDailySpecialJson jsonclass = new RealmControllerDungeonDailySpecialJson();
            GetJson(jsonclass);
            return jsonclass;
        }
    }
}