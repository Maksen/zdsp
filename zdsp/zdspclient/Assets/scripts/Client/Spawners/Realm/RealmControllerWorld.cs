using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Realm/RealmControllerWorld")]
    public class RealmControllerWorld : RealmController
    {
        public Transform[] PlayerSpawnPt;

        public override ServerEntityJson GetJson()
        {
            RealmControllerWorldJson jsonclass = new RealmControllerWorldJson();
            GetJson(jsonclass);
            return jsonclass;
        }

        public void GetJson(RealmControllerWorldJson jsonclass)
        {
            int count = PlayerSpawnPt.Length;
            Vector3[] spawnPos = new Vector3[count];
            Vector3[] spawnDir = new Vector3[count];
            for (int index = 0; index < count; index++)
            {
                spawnPos[index] = PlayerSpawnPt[index].position;
                spawnDir[index] = PlayerSpawnPt[index].forward;
            }
            jsonclass.spawnPos = spawnPos;
            jsonclass.spawnDir = spawnDir;
            base.GetJson(jsonclass);
        }
    }
}