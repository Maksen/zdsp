using UnityEngine;
using Photon.LoadBalancing.GameServer;
using Zealot.Repository;
using Zealot.Entities;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Server.AI;
using System.Collections.Generic;

namespace Zealot.Server.Entities
{
    public class PersonalMonsterSpawner : MonsterSpawnerBase
    {
        public PersonalMonsterSpawnerJson mPersonalMonsterSpawnerJson;
        private readonly static long mLiveDuration = 600000; //personal monster live for 10 minutes.
        private int mPopulation;
        private Dictionary<string, List<Monster>> mSummonerMonsters;

        public PersonalMonsterSpawner(PersonalMonsterSpawnerJson info, GameLogic instance) : base(info, instance)
        {
            mPersonalMonsterSpawnerJson = info;
            if(info.archetype != "")
                mArchetype = NPCRepo.GetArchetypeByName(info.archetype);
            mPopulation = mPersonalMonsterSpawnerJson.population;
            mSummonerMonsters = new Dictionary<string, List<Monster>>();
        }

        public override int GetPopulation()
        {
            return mPopulation;
        }

        public override void SpawnAllMonster()
        {
            return;
        }

        public void SpawnToMeOnly(Player player, int population)
        {
            if (mArchetype == null)
                return;
            string playername = player.Name;
            List<Monster> monsters;
            if (mSummonerMonsters.TryGetValue(playername, out monsters))
            {
                for (int index = 0; index < monsters.Count; index++)
                    monsters[index].CleanUp();
                monsters.Clear();
            }
            else
            {
                monsters = new List<Monster>();
                mSummonerMonsters.Add(playername, monsters);
            }
            for (int count = 1; count <= population; count++)
                SpawnMonster(playername, monsters);
        }

        public void SpawnMonster(string summoner, List<Monster> monsters)
        {
            //Spawn monster at server
            bool logflag = mArchetype.monsterclass == MonsterClass.Boss;
            Monster monster = mInstance.mEntitySystem.SpawnNetEntity<Monster>(logflag, mArchetype.archetype);
            NPCSynStats playerStats = new NPCSynStats();            
            monster.PlayerStats = playerStats;

            monster.mSummoner = summoner;
            monster.Position = RandomSpawnPosition();
            monster.Forward = RandomSpawnFacing();
            monster.Init(this, null, mLiveDuration);

            MonsterClass monsterClass = mArchetype.monsterclass;
            if (monsterClass == MonsterClass.Normal || monsterClass == MonsterClass.Mini)
                monster.SetAIBehaviour(new MonsterAIBehaviour(monster));
            else if(monsterClass == MonsterClass.Boss)
                monster.SetAIBehaviour(new BossAIBehaviour(monster));
            monsters.Add(monster);
        }

        public Vector3 RandomSpawnPosition()
        {
            Vector3 pos = mPropertyInfos.position;
            if (mPersonalMonsterSpawnerJson.population > 1)
            {
                float spawnRadius = mPersonalMonsterSpawnerJson.spawnRadius;
                if (spawnRadius > 0)
                    pos = GameUtils.RandomPos(pos, spawnRadius);                
            }
            return pos;
        }

        public Vector3 RandomSpawnFacing()
        {
            Vector3 facing = mPropertyInfos.forward;
            if (mPersonalMonsterSpawnerJson.population > 1)
            {
                System.Random random = GameUtils.GetRandomGenerator();
                facing = new Vector3((float)random.NextDouble() * 2 - 1, 0f, (float)random.NextDouble() * 2 - 1); 
                facing.Normalize();
            }
            return facing;
        }

        public override void OnChildDead(Monster child, IActor attacker)
        {
            string summoner = child.mSummoner;
            List<Monster> monsters;
            if (mSummonerMonsters.TryGetValue(summoner, out monsters))
            {
                monsters.Remove(child);
                if (monsters.Count == 0)
                    mSummonerMonsters.Remove(summoner);
            }
        }

        public override bool CanRoam()
        {
            return mPersonalMonsterSpawnerJson.canroam;
        }

        public override bool CanPathFind()
        {
            return mPersonalMonsterSpawnerJson.canpathfind;
        }

        public override bool IsAggressive()
        {
            return mPersonalMonsterSpawnerJson.aggressive;
        }

        public override bool IsGroupAggro()
        {
            return mPersonalMonsterSpawnerJson.groupattack;
        }

        public override void GroupAggro(int pid, IActor att)
        {             
            if (IsGroupAggro())
            {
                List<Monster> monsters;
                if (mSummonerMonsters.TryGetValue(att.Name, out monsters))
                {
                    for (int index = 0; index < monsters.Count; index++)
                        monsters[index].OnGroupAggro(pid, att);
                }               
            }
        }

        public override float GetCombatRadius()
        {
            return mPersonalMonsterSpawnerJson.combatRadius;
        }

        public override float GetSpawnRadius()
        {
            return mPersonalMonsterSpawnerJson.spawnRadius;
        }

        public override float GetAggroRadius()
        {
            return mPersonalMonsterSpawnerJson.aggroRadius;
        }

        #region Trigger
        #endregion
    }
}
