using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Common.Entities;
using Photon.LoadBalancing.GameServer;
using Kopio.JsonContracts;
using Zealot.Repository;

namespace Zealot.Server.Entities
{
    public abstract class MonsterSpawnerBase : IServerEntity
    {
        public MonsterSpawnerBaseJson mPropertyInfos;
        public CombatNPCJson mArchetype;
        public GameLogic mInstance;
        protected GameTimer timer;
        public List<Monster> maChildren;

        public MonsterSpawnerBase(MonsterSpawnerBaseJson info, GameLogic instance)
        {
            mPropertyInfos = info;
            mInstance = instance;
            maChildren = new List<Monster>();
        }

        public bool LogAI { get; set; }

        public ServerEntityJson GetPropertyInfos()
        {
            return mPropertyInfos;
        }

        public virtual int GetPopulation()
        {
            return 1;
        }

        public virtual void InstanceStartUp()
        {
            if (mPropertyInfos.activeOnStartup)
                SpawnAllMonster();
        }

        public virtual void SpawnAllMonster()
        {
        }

        public virtual void SpawnMonster()
        {
        }

        public virtual Vector3 GetPos()
        {
            return mPropertyInfos.position;
        }

        public virtual bool CanRoam()
        {
            return false;
        }

        public virtual bool CanPathFind()
        {
            return false;
        }

        public virtual bool IsGroupAggro()
        {
            return false;
        }

        public virtual bool IsAggressive()
        {
            return false;
        }

        public virtual void GroupAggro(int pid, IActor att)
        {          
        }

        public virtual float GetCombatRadius()
        {
            return 10.0f;
        }

        public virtual float GetRoamRadius()
        {
            float res = GetCombatRadius() / 2;
            return res;//smaller than combat radius .
        }

        public virtual float GetSpawnRadius()
        {
            return 0f;
        }

        public virtual float GetAggroRadius()
        {
            return 0f;
        }

        //attacker may be null which means child live time up and removed by server.
        public virtual void OnChildDead(Monster child, IActor attacker)
        {
            if (maChildren.Remove(child))
            {
                RealmController mRealmController = mInstance.mRealmController;
                if (mRealmController != null)
                    mRealmController.OnMonsterDead(child, attacker as Actor);
            }
        }

        public virtual void OnChildDamaged(IActor attacker)
        {           
        }

        public void DestoryAll()
        {
            for (int index = 0; index < maChildren.Count; index++)
                maChildren[index].CleanUp();
            maChildren.Clear();
            if (timer != null)
            {
                mInstance.StopTimer(timer);
                timer = null;
            }
        }

        public bool HasChildren()
        {
            return maChildren.Count > 0;
        }

        //Trigger
        #region Trigger
        public virtual void TriggerSpawn(IServerEntity sender, object[] parameters = null)
        {
            SpawnAllMonster();
        }

        public virtual void DestoryAll(IServerEntity sender, object[] parameters = null)
        {
            DestoryAll();
        }
        #endregion
    }
}
