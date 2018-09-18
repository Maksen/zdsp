using UnityEngine;
using Photon.LoadBalancing.GameServer;
using Kopio.JsonContracts;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Entities;
using Zealot.Repository;
using Zealot.Server.AI;

namespace Zealot.Server.Entities
{
    public class SMBossSpawner : MonsterSpawnerBase
    {
        public SMBossSpawnerJson mSMBossSpawnerJson;
        public GuildSMBossJson mSMBossInfo;

        public SMBossSpawner(SMBossSpawnerJson info, GameLogic instance)
            : base(info, instance)
        {
            mSMBossSpawnerJson = info;
            if (info.archetype != "")
                mArchetype = CombatNPCRepo.GetNPCByArchetype(info.archetype);
        }

        public override int GetPopulation()
        {
            return mSMBossSpawnerJson.population;
        }

        public override void InstanceStartUp()
        {}

        public override void SpawnAllMonster()
        {
            if (mArchetype == null)
                return;
            SpawnMonster();
        }

        public override void SpawnMonster()
        {
            // Spawn monster at server
            Monster monster = mInstance.mEntitySystem.SpawnNetEntity<Monster>(true, mArchetype.archetype);
            mInstance.mEntitySystem.AddAlwaysShow(monster); // Because only one monster
            NPCSynStats playerStats = new NPCSynStats();            
            monster.PlayerStats = playerStats;

            monster.Position = RandomSpawnPosition();
            monster.Forward = RandomSpawnFacing();
            monster.Init(this, () => 
            {
                // Get SM Boss level and combat stats
                RealmControllerGuildSMBoss realmController = (RealmControllerGuildSMBoss)mInstance.mRealmController;
                GuildSMBossJson guildSMBossJson = GuildRepo.GetGuildSMBossByLvl(realmController.GetSMBossLevel());
                if (guildSMBossJson == null)
                    return false;

                // todo(jason): remove this later
                //float healthregenamt = monster.mArchetype.healthregenamt;
                //bool recoveronreturn = monster.mArchetype.recoveronreturn;

                PlayerCombatStats combatStats = (PlayerCombatStats)monster.CombatStats;
                combatStats.SuppressComputeAll = true;
                combatStats.SetField(FieldName.AttackBase, guildSMBossJson.attack);
                combatStats.SetField(FieldName.ArmorBase, guildSMBossJson.armor);
                combatStats.SetField(FieldName.AccuracyBase, guildSMBossJson.accuracy);
                combatStats.SetField(FieldName.EvasionBase, guildSMBossJson.evasion);
                combatStats.SetField(FieldName.CriticalDamageBase, guildSMBossJson.criticaldamage);
                combatStats.SetField(FieldName.CocriticalBase, guildSMBossJson.cocritical);
                combatStats.SetField(FieldName.CriticalBase, guildSMBossJson.critical);
                //combatStats.SetField(FieldName.CoCriticalDamageBase, guildSMBossJson.cocriticaldamage);
                //combatStats.SetField(FieldName.TalentPointCloth, guildSMBossJson.talentcloth);
                //combatStats.SetField(FieldName.TalentPointScissors, guildSMBossJson.talentscissors);
                //combatStats.SetField(FieldName.TalentPointStone, guildSMBossJson.talentstone);
                int maxHp = guildSMBossJson.healthmax;
                monster.SetHealthMax(maxHp); // Init max health first
                int dmgDone = realmController.GetSMBossDmgDone();
                int health = maxHp - dmgDone;
                if (health < 0)
                    health = 0;
                monster.SetHealth(health); // Use custom max health           
                combatStats.SuppressComputeAll = false;
                combatStats.ComputeAll();//TODO:check the above stats is initialized properly.
                return true;
            });

            MonsterType monsterType = mArchetype.monstertype;
            //if (monsterType == MonsterType.Destructible)
            //    monster.SetAIBehaviour(new NullAIBehaviour(monster));
            if (monsterType == MonsterType.Normal)
                monster.SetAIBehaviour(new MonsterAIBehaviour(monster));
            else if (monsterType == MonsterType.Boss || monsterType == MonsterType.MiniBoss)
                monster.SetAIBehaviour(new BossAIBehaviour(monster));
            //else if (monsterType == MonsterType.Escape)
            //    monster.SetAIBehaviour(new MonsterEscapeAIBehaviour(monster));
            else
                monster.SetAIBehaviour(new MonsterAIBehaviour(monster));

            maChildren.Add(monster);
        }

        public Vector3 RandomSpawnPosition()
        {
            Vector3 pos = mPropertyInfos.position;
            if (mSMBossSpawnerJson.population > 1)
            {
                float spawnRadius = mSMBossSpawnerJson.spawnRadius;
                if (spawnRadius > 0)
                    pos = GameUtils.RandomPos(pos, spawnRadius);                
            }
            return pos;
        }

        public Vector3 RandomSpawnFacing()
        {
            Vector3 facing = mPropertyInfos.forward;
            if (mSMBossSpawnerJson.population > 1)
            {
                System.Random random = GameUtils.GetRandomGenerator();
                facing = new Vector3((float)random.NextDouble() * 2 - 1, 0f, (float)random.NextDouble() * 2 - 1); 
                facing.Normalize();
                //facing = GameUtils.YawToDirection(random.NextDouble() * Math.PI * 2);
            }
            return facing;
        }

        public override void OnChildDead(Monster child, IActor attacker)
        {
            base.OnChildDead(child, attacker);
            object[] paramters = { attacker };
            mInstance.BroadcastEvent(this, "OnChildDead", paramters);
        }

        public override bool CanRoam()
        {
            return mSMBossSpawnerJson.canroam;
        }

        public override bool CanPathFind()
        {
            return mSMBossSpawnerJson.canpathfind;
        }

        public override bool IsAggressive()
        {
            return mSMBossSpawnerJson.aggressive;
        }

        public override float GetCombatRadius()
        {
            return mSMBossSpawnerJson.combatRadius;
        }

        public override float GetSpawnRadius()
        {
            return mSMBossSpawnerJson.spawnRadius;
        }

        public override float GetAggroRadius()
        {
            return mSMBossSpawnerJson.aggroRadius;
        }
    }
}
