using UnityEngine;
using Zealot.Entities;
using Zealot.Common;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/GateSpawner")]
    public class GateSpawner : ServerEntity {
        public bool activeOnStartup = true;
        public float widthRatio = 1f;
        public float heightRatio = 1f;
        public GameObject prefab;

        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override string[] Triggers { get {
				return new string[] { "Open", "Close" };
			}}

		public override ServerEntityJson GetJson()
		{
            GateSpawnerJson jsonclass = new GateSpawnerJson();
			GetJson (jsonclass);
			return jsonclass;
		}

        public void GetJson(GateSpawnerJson jsonclass)
        {
            jsonclass.activeOnStartup = activeOnStartup;
            jsonclass.forward = transform.forward;
            jsonclass.width = widthRatio;
            jsonclass.height = heightRatio;
            jsonclass.prefab = prefab.name;
            base.GetJson(jsonclass);
        }
    }
}
