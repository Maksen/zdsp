using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Realm/RealmControllerDungeon")]
    public class RealmControllerDungeon : RealmController
    {
        public override ServerEntityJson GetJson()
        {
            RealmControllerDungeonJson jsonclass = new RealmControllerDungeonJson();
            GetJson(jsonclass);
            return jsonclass;
        }
    }
}