using ExitGames.Logging;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Server.Counters;

namespace Zealot.Server.Entities
{
    public class ServerEntitySystem : EntitySystem
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public int ID
        {
            private set;
            get;
        }

        public Dictionary<string, Player> mPlayerDict; //Online player names (exclude AI player)
        private List<Entity> mAlwaysShowNetEntity;

        public ServerEntitySystem(Timers timers, int id) : base(timers)
        {
            mPlayerDict = new Dictionary<string, Player>();
            mAlwaysShowNetEntity = new List<Entity>();
            ID = id;
        }

        ~ServerEntitySystem()
        {
            GameCounters.TotalEntities.IncrementBy(-mNetEntities.Count);            
        }

        //Entity that exists in server and will have a corresponding ghost entity at client
        public T SpawnNetEntity<T>(bool logflag=false, string extraLogInfo = null) where T : BaseNetEntity, new()
        {
            uint currentTick = mTimers.GetTick();
            int id = mIDPool.AllocID(currentTick);
            int pid = mPIDPool.AllocID(currentTick);     

            T entity = new T();
            entity.EntitySystem = this;
            entity.SetID(id);
            entity.SetPersistentID(pid);

            OnAddEntity(id, pid, entity);

            bool _logflag = logflag;
#if DEBUG
            _logflag = true;
#endif
            if (_logflag)
            {
                if (extraLogInfo == null)
                    extraLogInfo = string.Empty;
                log.InfoFormat("[{0}]: entadd {1} ({2}) {3}", ID, entity.EntityType.ToString(), pid, extraLogInfo);
            }
            GameCounters.TotalEntities.Increment();
            return entity;
        }

        public override bool RemoveEntityByPID(int pid, bool logflag = false)
        {
            bool success = base.RemoveEntityByPID(pid, logflag);
            if (success)
            {
                bool _logflag = logflag;
#if DEBUG
                _logflag = true;
#endif
                if (_logflag)
                    log.InfoFormat("[{0}]: entrmv ({1})", ID, pid);
                GameCounters.TotalEntities.Decrement();
            }
            return success;
        }

        public void ResetSyncStats()
        {
            foreach (KeyValuePair<int, Entity> entry in mNetEntities)
            {
                ((BaseNetEntity)entry.Value).ResetSyncStats();
            }
        }

        public void RegisterPlayerName(string name, Player player)
        {
            if (mPlayerDict.ContainsKey(name))
            {
                mPlayerDict[name] = player;
                log.InfoFormat("{0} in {1} not clear properly", name, player.mInstance.mCurrentLevelName);
            }
            else
                mPlayerDict.Add(name, player);
        }

        public void UnregisterPlayerName(string name)
        {
            mPlayerDict.Remove(name);
        }

        public Player GetPlayerByName(string name)
        {
            if (mPlayerDict.ContainsKey(name))
                return mPlayerDict[name];
            return null;
        }

        public void AddAlwaysShow(BaseNetEntity entity)
        {
            mAlwaysShowNetEntity.Add(entity);
        }

        public void RemoveAlwaysShow(BaseNetEntity entity)
        {
            mAlwaysShowNetEntity.Remove(entity);
        }

        public List<Entity> GetAlwaysShowNetEntities()
        {
            return mAlwaysShowNetEntity;
        }
    }
}
