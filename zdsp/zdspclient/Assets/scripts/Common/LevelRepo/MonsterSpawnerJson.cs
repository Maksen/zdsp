using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Zealot.Common;

namespace Zealot.Entities
{
    public abstract class MonsterSpawnerBaseJson : ServerEntityWithEventJson
    {
        [DefaultValue(true)]
        public bool activeOnStartup { get; set; }
        [DefaultValue("")]
        public string archetype { get; set; }
        public Vector3 forward = Vector3.forward;
    }

    public class MonsterSpawnerJson : MonsterSpawnerBaseJson
    {
        public string archetypeGroup;
        public float aggroRadius;
        public float combatRadius;
        public float spawnRadius;
        public int population;
        public long respawnTime;
        public int respawnCount;//this is the nubmer of monsters respawned. each time respawn one only.
        public bool respawnAll;
        public bool canroam;
        public bool canpathfind;
        public bool groupattack;
        public bool aggressive;
        public bool damageEvent;                      

        public override string GetServerClassName(){return "MonsterSpawner";}
    }

    public class GoblinSpawnerJson : MonsterSpawnerBaseJson
    {
        public long spawnInterval;
        public int spawnCount;
        public int path;
        public float distPerMove;
        public long pausePerMove;
        public byte factionType = (byte)FactionType.None;

        public override string GetServerClassName() { return "GoblinSpawner"; }
    }

    public class PositionHelperData
    {
        public float aggroRadius;
        public float combatRadius;
        public Vector3 position;
    }

    public class SpecialBossSpawnerJson : MonsterSpawnerBaseJson
    {
        public List<PositionHelperData> positionhelper;
        public bool canroam;
        public bool canpathfind;
        public bool aggressive;

        public override string GetServerClassName() { return "SpecialBossSpawner"; }
    }

    public class SMBossSpawnerJson : MonsterSpawnerBaseJson
    {
        public float aggroRadius;
        public float combatRadius;
        public float spawnRadius;
        public int population;
        public bool canroam;
        public bool canpathfind;
        public bool aggressive;

        public override string GetServerClassName() { return "SMBossSpawner"; }
    }

    public class PersonalMonsterSpawnerJson : MonsterSpawnerBaseJson
    {
        public float aggroRadius;
        public float combatRadius;
        public float spawnRadius;
        public int population;
        public bool canroam;
        public bool canpathfind;
        public bool groupattack;
        public bool aggressive;
        public bool damageEvent;

        public override string GetServerClassName() { return "PersonalMonsterSpawner"; }
    }
}
