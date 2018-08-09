using System.Collections.Generic;
using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/SpecialBossSpawner")]
    public class SpecialBossSpawner : ServerEntityWithEvent {
		[Tooltip("bossName Link to gamedb SpecialBoss table.")]
		public string bossName;
        public List<PositionHelper> randomPos;        
        public bool canroam;
        [Tooltip("All monsters in world instance do not need path find. Some monsters in realm may not need path find if combat radius ensure they can move safely")]
        public bool canpathfind;
        public bool aggressive;

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

        public override string[] Events { get {
				return new string[]{ "OnChildDead", "OnChildrenSpawn" };
			}}
		
		public override ServerEntityJson GetJson()
		{
            SpecialBossSpawnerJson jsonclass = new SpecialBossSpawnerJson();
			GetJson(jsonclass);
			return jsonclass;
		}
		
		public void GetJson(SpecialBossSpawnerJson jsonclass)
		{
            jsonclass.forward = transform.forward;
            jsonclass.activeOnStartup = false;
			jsonclass.archetype = bossName;
            jsonclass.positionhelper = new List<PositionHelperData>();
            foreach (var element in randomPos)
            {
                jsonclass.positionhelper.Add(new PositionHelperData()
                {
                    aggroRadius = element.aggroRadius,
                    combatRadius = element.combatRadius,
                    position = element.transform.position
                });
            }
            jsonclass.canroam = canroam;
            jsonclass.canpathfind = canpathfind;
            jsonclass.aggressive = aggressive;
            base.GetJson(jsonclass);
		}
    }
}