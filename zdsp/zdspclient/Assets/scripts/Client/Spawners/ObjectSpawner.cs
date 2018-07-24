using UnityEngine;
using System;
using System.Collections.Generic;
using Zealot.Entities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/ObjectSpawner")]
    public class ObjectSpawner : ServerEntity {
        public bool activeOnStartup;
        public GameObject prefab;

        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override string[] Triggers
        {
            get
            {
                return new string[] { "TriggerSpawn", "DestoryAll" };
            }
        }

        public override ServerEntityJson GetJson()
		{
            ObjectSpawnerJson jsonclass = new ObjectSpawnerJson();
			GetJson (jsonclass);
			return jsonclass;
		}

        public void GetJson(ObjectSpawnerJson jsonclass)
        {
            jsonclass.forward = transform.forward;
            jsonclass.activeOnStartup = activeOnStartup;
#if UNITY_EDITOR
            jsonclass.prefab = (prefab == null) ? "" : AssetDatabase.GetAssetPath(prefab).Remove(0, 7);
#endif
            base.GetJson(jsonclass);
        }
    }
}