using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/PlayerSpawner")]
    public class PlayerSpawner : ServerEntity
    {
        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override ServerEntityJson GetJson()
		{
			PlayerSpawnerJson jsonclass = new PlayerSpawnerJson();
            jsonclass.forward = transform.forward;
            jsonclass.forward.y = 0;
            GetJson (jsonclass);
			return jsonclass;
		}
	}
}