using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/GateSpawner")]
    public class GateSpawner : ServerEntity
    {
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
			GetJson(jsonclass);
			return jsonclass;
		}

        public void GetJson(GateSpawnerJson jsonclass)
        {
            jsonclass.activeOnStartup = activeOnStartup;
            jsonclass.forward = transform.forward;
            jsonclass.width = widthRatio;
            jsonclass.height = heightRatio;
#if UNITY_EDITOR
            jsonclass.prefab = UnityEditor.AssetDatabase.GetAssetPath(prefab).Remove(0, 7);
#endif
            base.GetJson(jsonclass);
        }
    }
}
