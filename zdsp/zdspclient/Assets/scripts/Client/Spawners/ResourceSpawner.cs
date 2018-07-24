using UnityEngine;
using System.Collections;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/ResourceSpawner")]
    public class ResourceSpawner : ServerEntityWithEvent {
		public bool activeOnStartup;
        [Tooltip("Archetype Link to gamedb Resource table.")]
        public string archetype;

        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override string[] Triggers { get {
				return new string[]{ "TurnOn", "DestoryAll" };
			}}

        public override string[] Events { get {
				return new string[]{ "OnPickedSpawnBoss" };
			}}

		public override ServerEntityJson GetJson()
		{
            ResourceSpawnerJson jsonclass = new ResourceSpawnerJson();
			GetJson (jsonclass);
			return jsonclass;
		}
		
		public void GetJson(ResourceSpawnerJson jsonclass)
		{
			jsonclass.activeOnStartup = activeOnStartup;
			jsonclass.archetype = archetype;
            jsonclass.forward = transform.forward;
            base.GetJson (jsonclass);
		}
	}
}
