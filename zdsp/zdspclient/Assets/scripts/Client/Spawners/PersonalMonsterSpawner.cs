using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/PersonalMonsterSpawner")]
    public class PersonalMonsterSpawner : ServerEntityWithEvent
    {
		[Tooltip("Archetype Link to gamedb NPC table.")]
		public string archetype;
        [Tooltip("Radius measured from the monster within which it will be able detect any threats")]
        public float aggroRadius;
        [Tooltip("Radius measured from the spawner within which it is free to engage in combat")]
        public float combatRadius;
        [Tooltip("Spawn radius generally is smaller than combat radius to ensure that the monsters are spawned in walkable area")]
        public float spawnRadius;

        public int population = 1;

        public bool canroam;
        [Tooltip("All monsters in world instance do not need path find. Some monsters in realm may not need path find if combat radius ensure they can move safely")]
        public bool canpathfind;
        [Tooltip("If the monster spawned will be Aggressive")]
        public bool aggressive;
        [Tooltip("when one monster is attacked, the other monster belong to the same spawner will also help attack if in aggro radius. should set to non-aggressive")]
        public bool groupattack;
        [Tooltip("If the monster damage event is broadcast")]
        public bool damageEvent;

        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override string[] Triggers
        {
            get
            {
                return new string[] { "TriggerSpawn", "DestoryAll", "HelpAttack", "SetArchetype", "SetArchetypeAndTriggerSpawn" };
            }
        }
        public override string[] Events
        {
            get
            {
                return new string[] { "OnChildDead", "OnChildDamaged" };
            }
        }

        public override ServerEntityJson GetJson()
		{
            PersonalMonsterSpawnerJson jsonclass = new PersonalMonsterSpawnerJson();
			GetJson(jsonclass);
			return jsonclass;
		}
		
		public void GetJson(PersonalMonsterSpawnerJson jsonclass)
		{
            jsonclass.forward = transform.forward;
			jsonclass.archetype = archetype;
            jsonclass.aggroRadius = aggroRadius;
			jsonclass.combatRadius = combatRadius;
            jsonclass.spawnRadius = spawnRadius;
            jsonclass.population = population;
            jsonclass.canroam = canroam;
            jsonclass.canpathfind = canpathfind;
            jsonclass.groupattack = groupattack;
            jsonclass.aggressive = aggressive;
            jsonclass.damageEvent = damageEvent;
            base.GetJson(jsonclass);
		}

        void OnDrawGizmosSelected()
        {
            Color color = Gizmos.color;
            if (spawnRadius > 0)
                DrawCircle(transform.position, spawnRadius, Color.blue);
            if (combatRadius > 0)
                DrawCircle(transform.position, combatRadius, Color.red);
            Gizmos.color = color;
        }

        void DrawCircle(Vector3 center, float radius, Color color)
        {
            Gizmos.color = color;
            float theta = 0f;
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            Vector3 pos = center + new Vector3(x, 2, y);
            Vector3 newPos = pos;
            Vector3 lastPos = pos;
            for (theta = 0.1f; theta < Mathf.PI * 2; theta += 0.1f)
            {
                x = radius * Mathf.Cos(theta);
                y = radius * Mathf.Sin(theta);
                newPos = center + new Vector3(x, 2, y);
                Gizmos.DrawLine(pos, newPos);
                pos = newPos;
            }
            Gizmos.DrawLine(pos, lastPos);
        }
    }
}