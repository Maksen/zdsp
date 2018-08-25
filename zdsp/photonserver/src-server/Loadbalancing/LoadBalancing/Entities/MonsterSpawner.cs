using UnityEngine;
using Photon.LoadBalancing.GameServer;
using Zealot.Repository;
using Zealot.Entities;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Server.AI;

namespace Zealot.Server.Entities
{
    public class MonsterSpawner : MonsterSpawnerBase
    {
        public MonsterSpawnerJson mMonsterSpawnerJson;
        private int mRespawnedCount = 0;
        private long mLastDamagedEvent = 0;

        public MonsterSpawner(MonsterSpawnerJson info, GameLogic instance) : base(info, instance)
        {
            mMonsterSpawnerJson = info;
            if(info.archetype != "")
                mArchetype = CombatNPCRepo.GetNPCByArchetype(info.archetype);
            else if(instance.mRoom.RealmID > 0)
                mArchetype = RealmNPCGroupRepo.GetNPCByGroupNameAndRealmId(info.archetypeGroup, instance.mRoom.RealmID);
            mRespawnedCount = mMonsterSpawnerJson.respawnCount;
        }

        public override int GetPopulation()
        {
            return mMonsterSpawnerJson.population;
        }

        public override void SpawnAllMonster()
        {
            if (mArchetype == null)
                return;
            //mRespawnedCount = 0;
            while (maChildren.Count < mMonsterSpawnerJson.population)           
                SpawnMonster();
        }

        public override void SpawnMonster()
        {
            // Spawn monster at server
            bool logflag = mArchetype.monsterclass == MonsterClass.Boss;
            Monster monster = mInstance.mEntitySystem.SpawnNetEntity<Monster>(logflag, mArchetype.archetype);
            NPCSynStats playerStats = new NPCSynStats();            
            monster.PlayerStats = playerStats;

            monster.Position = RandomSpawnPosition();
            monster.Forward = RandomSpawnFacing();
            monster.Init(this);

            MonsterClass monsterClass = mArchetype.monsterclass;
            //if (monsterClass == MonsterClass.Destructible)
            //    monster.SetAIBehaviour(new NullAIBehaviour(monster));
            if (monsterClass == MonsterClass.Normal)
                monster.SetAIBehaviour(new MonsterAIBehaviour(monster));
            else if(monsterClass == MonsterClass.Boss || monsterClass == MonsterClass.MiniBoss)
                monster.SetAIBehaviour(new BossAIBehaviour(monster));
            //else if(monsterClass == MonsterClass.Escape)
            //    monster.SetAIBehaviour(new MonsterEscapeAIBehaviour(monster));

            if (mArchetype.broadcast)
                GameApplication.Instance.BroadcastMessage(BroadcastMessageType.MonsterSpawn, mArchetype.id + ";" + mInstance.mCurrentLevelID);

            maChildren.Add(monster);
        }

        public void SpawnToMeOnly(Player player)
        {
            SpawnMonster();
            Monster monster = maChildren[maChildren.Count - 1];
            monster.mSummoner = player.Name;
        }

        public Vector3 RandomSpawnPosition()
        {
            Vector3 pos = mPropertyInfos.position;
            if (mMonsterSpawnerJson.population > 1)
            {
                float spawnRadius = mMonsterSpawnerJson.spawnRadius;
                if (spawnRadius > 0)
                    pos = GameUtils.RandomPos(pos, spawnRadius);                
            }
            return pos;
        }

        public Vector3 RandomSpawnFacing()
        {
            Vector3 facing = mPropertyInfos.forward;
            if (mMonsterSpawnerJson.population > 1)
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
            if (mRespawnedCount == -1 || mRespawnedCount > 0)
			{
				if (timer == null)
                	timer = mInstance.SetTimer(mMonsterSpawnerJson.respawnTime, OnRespawnTimeUp, null);
			}
            object[] paramters = { attacker, child };
            mInstance.BroadcastEvent(this, "OnChildDead", paramters);

            if (mArchetype.broadcast)
            {
                PlayerSynStats playerSynStats = (attacker as Player).PlayerSynStats;
                string paramStr = string.Format("{0};{1};{2}", mArchetype.id, mInstance.mCurrentLevelID, attacker.Name);
                GameApplication.Instance.BroadcastMessage(BroadcastMessageType.MonsterKilled, paramStr);
            }
        }

        public override void OnChildDamaged(IActor attacker)
        {
            //base.OnChildDamage(attacker);
            if (mMonsterSpawnerJson.damageEvent)
            {
                long now = mInstance.GetSynchronizedTime();
                if (now - mLastDamagedEvent > 5000)
                {
                    mLastDamagedEvent = now;
                    object[] paramters = { attacker };
                    mInstance.BroadcastEvent(this, "OnChildDamaged", paramters);
                }
            }
        }

        public void OnRespawnTimeUp(object arg)
        {
            timer = null;
            if (mMonsterSpawnerJson.respawnAll)
            {
                if (mRespawnedCount == -1)
                {
                    while (maChildren.Count < mMonsterSpawnerJson.population)
                        SpawnMonster();
                }
                else
                {
                    while (mRespawnedCount > 0 && maChildren.Count < mMonsterSpawnerJson.population)
                    {
                        SpawnMonster();
                        mRespawnedCount--;
                    }
                }
            }
            else
            {
                if (mRespawnedCount != -1)
                    mRespawnedCount--;
                SpawnMonster();
                if (maChildren.Count < mMonsterSpawnerJson.population && (mRespawnedCount == -1 || mRespawnedCount > 0))
                    timer = mInstance.SetTimer(mMonsterSpawnerJson.respawnTime, OnRespawnTimeUp, null);
            }  
        }

        public override bool CanRoam()
        {
            return mMonsterSpawnerJson.canroam;
        }

        public override bool CanPathFind()
        {
            return mMonsterSpawnerJson.canpathfind;
        }

        public override bool IsAggressive()
        {
            return mMonsterSpawnerJson.aggressive;
        }

        public override bool IsGroupAggro()
        {
            return mMonsterSpawnerJson.groupattack;
        }

        public override void GroupAggro(int pid, IActor att)
        {             
            if (IsGroupAggro())
            {
                foreach (var monster in maChildren)
                {
                    monster.OnGroupAggro(pid, att);
                }
            }
        }

        public override float GetCombatRadius()
        {
            return mMonsterSpawnerJson.combatRadius;
        }

        public override float GetSpawnRadius()
        {
            return mMonsterSpawnerJson.spawnRadius;
        }

        public override float GetAggroRadius()
        {
            return mMonsterSpawnerJson.aggroRadius;
        }

        #region Trigger
        public virtual void HelpAttack(IServerEntity sender, object[] parameters = null)
        {
            //used for normal monster to attack if the boss is attacked.
            foreach (Monster monster in maChildren)
            {
                monster.OnAttacked((IActor)parameters[0], 1);
            }
        }
        public virtual void SetArchetype(IServerEntity sender, object[] parameters = null)
        {
            if (parameters != null)
            {
                mArchetype = CombatNPCRepo.GetNPCById((int)parameters[0]);
            }
        }

        public virtual void SetArchetypeAndTriggerSpawn(IServerEntity sender, object[] parameters = null)
        {
            if (parameters != null)
            {
                mArchetype = CombatNPCRepo.GetNPCById((int)parameters[0]);
                SpawnAllMonster();
            }
        }
        #endregion
    }
}
