using UnityEngine;
using System;
using System.Collections.Generic;
using Zealot.Entities;
using Zealot.Common;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/GoblinSpawner")]
    public class GoblinSpawner : ServerEntityWithEvent
    {
        public bool activeOnStartup = false;
        [Tooltip("Archetype Link to gamedb NPC table.")]
        public string archetype;
        public long spawnInterval;
        public int spawnCount;
        public float distPerMove;
        public long pausePerMove;
        public FactionType factionType = FactionType.None;
        public GameObject path;

        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override string[] Triggers { get {
				return new string[] { "TriggerSpawn", "DestoryAll" };
			}}
		public override string[] Events { get {
				return new string[]{ "OnChildSpawn", "OnPathCompleted", "OnChildDead" };
			}}

        public override ServerEntityJson GetJson()
        {
            GoblinSpawnerJson jsonclass = new GoblinSpawnerJson();
            GetJson(jsonclass);
            return jsonclass;
        }

        public void GetJson(GoblinSpawnerJson jsonclass)
        {
            jsonclass.forward = transform.forward;
            jsonclass.activeOnStartup = activeOnStartup;
            jsonclass.archetype = archetype;
            jsonclass.spawnInterval = spawnInterval;
            jsonclass.spawnCount = spawnCount;
            jsonclass.distPerMove = distPerMove;
            jsonclass.pausePerMove = pausePerMove;
            jsonclass.factionType = (byte)factionType;
            jsonclass.path = path.GetInstanceID();
            base.GetJson(jsonclass);
        }
    }
}
