using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Realm/RealmControllerDungeonStory")]
    public class RealmControllerDungeonStory : RealmController
    {
        public override ServerEntityJson GetJson()
        {
            RealmControllerDungeonStoryJson jsonclass = new RealmControllerDungeonStoryJson();
            GetJson(jsonclass);
            return jsonclass;
        }
    }
}