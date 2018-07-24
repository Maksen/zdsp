using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Realm/RealmControllerVIPRoom")]
    public class RealmControllerVIPRoom : RealmController
    {
        public override string[] Triggers
        {
            get
            {
                return new string[] { "BossKilledGiveReward" };
            }
        }

        /*public override ServerEntityJson GetJson()
        {
            RealmControllerVIPRoomJson jsonclass = new RealmControllerVIPRoomJson();
            GetJson(jsonclass);
            return jsonclass;
        }*/
    }
}
