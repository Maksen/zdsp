using Kopio.JsonContracts;
using Photon.LoadBalancing.GameServer;
using System;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Repository;

namespace Zealot.Server.Entities
{
    public class RealmController : IServerEntity
    {
        public RealmControllerJson mPropertyInfos;
        public GameLogic mInstance;
        public RealmJson mRealmInfo;
        public RealmState mRealmState = RealmState.Created;
        public Dictionary<string, Player> mPlayers;
        //public Dictionary<string, bool> mPlayerCostDeducted;
        public Dictionary<string, List<ItemInfo>> mPlayerReward;
        public bool mMissionCompleted = false;
        protected DateTime mTimerStart;
        protected GameTimer timer;
        protected int mCountDownOnMissionCompleted = 20;

        public RealmController(RealmControllerJson info, GameLogic instance)
        {
            mRealmInfo = RealmRepo.GetInfoById(instance.mRoom.RealmID);
            if (mRealmInfo == null || !IsCorrectController())
                return;
            mPropertyInfos = info;
            mInstance = instance;
            mInstance.mRealmController = this;
            mPlayers = new Dictionary<string, Player>();
            //mPlayerCostDeducted = new Dictionary<string, bool>();
            mPlayerReward = new Dictionary<string, List<ItemInfo>>();
        }

        public virtual bool IsCorrectController()
        {
            return false;
        }

        public ServerEntityJson GetPropertyInfos()
        {
            return mPropertyInfos;
        }

        public bool IsWorld()
        {
            return mRealmInfo.type == RealmType.World;
        }

        public virtual void InstanceStartUp()
        {
            long preparationTime = GetPreparationTime();
            if (preparationTime > 0)
            {
                timer = mInstance.SetTimer(preparationTime, OnPreparationTimeUp, null);
                mRealmState = RealmState.Preparation;
                mTimerStart = DateTime.Now;
            }
            else
                RealmStart();
        }

        public void OnPreparationTimeUp(object arg=null)
        {
            timer = null;
            RealmStart();
        }

        public virtual void OnLifeTimeUp(object arg=null)
        {
            timer = null;
            RealmEnd();
        }

        public virtual void SetSpawnPos(Player player)
        {
            player.Position = mPropertyInfos.position;
            player.Forward = mPropertyInfos.forward;
        }

        public virtual bool CanReconnect()
        {
            return false;
        }

        public virtual void RealmStart()
        {
            long lifeTime = mRealmInfo.timelimit;
            if (lifeTime > 0)
                timer = mInstance.SetTimer(lifeTime * 1000, OnLifeTimeUp, null);
            mRealmState = RealmState.Started;
            mTimerStart = DateTime.Now;
        }

        public virtual void RealmEnd()
        {
            timer = null;
            mRealmState = RealmState.Ended;
            if (mPlayers.Count == 0)
            {
                if (mInstance.mRoom.RemoveTimer == null)
                    GameApplication.Instance.AddRoomToBeRemoved(mInstance.mRoom);
            }
            else
            {
                foreach (Player player in mPlayers.Values)
                    player.Slot.LeaveRealm();
                mPlayers.Clear();
            }          
        }

        public virtual long GetPreparationTime()
        {
            return mRealmInfo.preparation * 1000;
        }

        public virtual void OnPlayerEnter(Player player)
        {
            mPlayers.Add(player.Name, player);
            GameClientPeer peer = player.Slot;
            DateTime dtNow = DateTime.Now;
            int timespan = (int)(dtNow - mTimerStart).TotalSeconds;
            peer.ZRPC.CombatRPC.EnterRealm(mRealmInfo.id, (byte)mRealmState, timespan, dtNow.Ticks, peer);
        }

        public virtual void OnPlayerExit(Player player)
        {
            if (mRealmState != RealmState.Ended)
                mPlayers.Remove(player.Name);
        }

        public virtual void OnMissionCompleted(bool success, bool broadcast)
        {
            if (mMissionCompleted)
                return;
            mMissionCompleted = true;
            if (timer != null)
                mInstance.StopTimer(timer);
            foreach (var entry in mInstance.maMonsterSpawners)
                entry.DestoryAll();
            timer = mInstance.SetTimer(mCountDownOnMissionCompleted * 1000, (arg) => RealmEnd(), null);
            foreach (Player player in mPlayers.Values)
            {
                if (broadcast)
                    player.Slot.ZRPC.CombatRPC.OnMissionCompleted(success, mCountDownOnMissionCompleted, player.Slot);
            }
        }

        public virtual void Update(long dt) { }
        public virtual void OnDealtDamage(Player player, Actor defender, int damage) { }
        // Killer can be null
        public virtual void OnMonsterDead(Monster monster, Actor killer) { }
        public virtual void OnPlayerDead(Player player, Actor killer) { }
        public virtual void ClaimReward(Player player) { }

        public virtual bool OnAllPeerRemoved(bool removeDueDc)
        {
            return true; //todo: if need to close realm after realm time out return false.
        }

        #region Triggers
        public virtual void CompleteRealm(IServerEntity sender, object[] parameters = null)
        {
            OnMissionCompleted(true, true);    
        }
        #endregion
    } 
}
