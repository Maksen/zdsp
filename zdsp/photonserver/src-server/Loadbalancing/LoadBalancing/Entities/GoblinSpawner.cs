using Zealot.Entities;
using Zealot.Common.Entities;
using Photon.LoadBalancing.GameServer;
using UnityEngine;
using Zealot.Repository;
using Zealot.Server.AI;

namespace Zealot.Server.Entities
{
    public class GoblinSpawner : MonsterSpawnerBase
    {
        public GoblinSpawnerJson mGoblinSpawnerJson;
        public PathStraightJson mPath;
        private int mSpawnedCount = 0;

        public GoblinSpawner(GoblinSpawnerJson info, GameLogic instance)
            : base(info, instance)
        {
            mGoblinSpawnerJson = info;
            mArchetype = NPCRepo.GetArchetypeByName(info.archetype);
            LevelInfo linfo = LevelReader.GetLevel(mInstance.currentlevelname);
            mPath = linfo.mEntities["PathStraightJson"][mGoblinSpawnerJson.path] as PathStraightJson;
        }

        public override int GetPopulation()
        {
            return mGoblinSpawnerJson.spawnCount;
        }

        public override void SpawnAllMonster()
        {
            mSpawnedCount = 0;
            if (mArchetype == null || mPath == null)
                return;          
            SpawnMonster();
        }

        public override void SpawnMonster()
        {
            //Spawn monster at server
            Monster monster = mInstance.mEntitySystem.SpawnNetEntity<Monster>(false, mArchetype.archetype);
            NPCSynStats playerStats = new NPCSynStats();
            monster.PlayerStats = playerStats;

            monster.Position = mPath.nodes[0];//mPropertyInfos.position;
            monster.Forward = new Vector3(0, 0, 1);
            monster.Team = mGoblinSpawnerJson.factionType;
            monster.Init(this);
            monster.SetAIBehaviour(new GoblinAIBehaviour(monster));
            maChildren.Add(monster);

            object[] paramters = { monster };
            mInstance.BroadcastEvent(this, "OnChildSpawn", paramters);

            mSpawnedCount++;
            if (mSpawnedCount < mGoblinSpawnerJson.spawnCount)
            {
                if (mGoblinSpawnerJson.spawnInterval == 0)
                    SpawnMonster();
                else
                    timer = mInstance.SetTimer(mGoblinSpawnerJson.spawnInterval, (arg) =>
                    {
                        timer = null; 
                        SpawnMonster();
                    }, null);
            }   
        }

        public override void OnChildDead(Monster child, IActor attacker)
        {
            base.OnChildDead(child, attacker);
            object[] paramters = { attacker, child };
            mInstance.BroadcastEvent(this, "OnChildDead", paramters);
        }

        public void OnPathCompleted(Monster child)
        {
            maChildren.Remove(child);
            object[] paramters = { child };
            mInstance.BroadcastEvent(this, "OnPathCompleted", paramters); //should link to somewhere in case need to record how many goblin escape.            
            mInstance.mEntitySystem.RemoveEntityByPID(child.GetPersistentID());
        }
    }
}
