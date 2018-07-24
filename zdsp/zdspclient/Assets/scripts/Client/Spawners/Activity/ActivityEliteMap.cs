using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Activity/ActivityEliteMap")]
    public class ActivityEliteMap : RealmController
    {
        public override ServerEntityJson GetJson()
        {
            RealmControllerEliteMapJson jsonclass = new RealmControllerEliteMapJson();
            GetJson(jsonclass);
            return jsonclass;
        }
    }
}
