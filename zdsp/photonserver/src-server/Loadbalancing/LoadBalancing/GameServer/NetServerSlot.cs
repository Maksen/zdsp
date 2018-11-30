using System;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Logging;
using Zealot.Common.Entities;
using Zealot.Server.Entities;
using Zealot.Common.Actions;
using Zealot.Common;
using Photon.Hive;

namespace Photon.LoadBalancing.GameServer
{   
    public class ObjectState
    {
        public BaseNetEntity ne;
        public bool relevant;
        public uint lastActionSent;
        public uint lastTimeFoundRelevant;
        public Vector3 lastPos;
        public Vector3 lastForward;

        public ObjectState(BaseNetEntity ne)
        {
            this.ne = ne;
            relevant = false;
            lastActionSent = 0;
            lastTimeFoundRelevant = 0;
            lastPos = Vector3.zero;
            lastForward = Vector3.zero;
        }
    }

    public class SnapshotInfo : IComparable
    {        
        public NetEntity ne;
        public ObjectState os;
        public float distsqMoved;
        public float dotproductfactor;
        public uint actionTickElapsed;
        public float bossPriorityFactor;
        public float priority;        
        public SnapShotUpdateCommand snapshotCmd;
        public ActionCommand actionCmd;

        private uint currentTick;

        public SnapshotInfo(NetEntity ne, ObjectState os, uint currentTick, uint actionTickElapsed)
        {
            this.ne = ne;
            this.os = os;
            this.currentTick = currentTick;

            this.actionTickElapsed = actionTickElapsed;         
            distsqMoved = 0;
            dotproductfactor = 0;
            priority = 0;

            if (actionTickElapsed > 20) //if boss has performed a new action and 20 ticks or about 2000msec has elapsed and action not sent out yet, it will have top priority
            {
                Monster monster = ne as Monster;
                if (monster != null)
                {
                    //if (monster.mArchetype.monstertype == MonsterType.Boss)
                        //bossPriorityFactor = 100;
                }
                else
                    bossPriorityFactor = 0;
            }

            snapshotCmd = null;
            actionCmd = null;
        }

        public void ComputePriority()
        {            
            //log.InfoFormat("ComputePriority pid {0} actionTickElapsed {1} distsqMoved {2} ", ne.GetPersistentID(), actionTickElapsed, distsqMoved);
            priority = bossPriorityFactor + actionTickElapsed + distsqMoved / 2.0f + dotproductfactor * 2; //Each slot is updated every 2 ticks            
            //Prioritize action change over dist moved. This enables better entities synchronization.
            //When entity has moved 4m, dist weight will be 2 which is the same as action changed after 100msec (2 ticks)

            //Entities that are newly relevant will have high actionTickElapsed and their actions will be sent with top priority
            //Entities that perform action after a long time also have higher priority
            
        }

        public void SendToClient(HivePeer peer)
        {
            if (snapshotCmd!= null)
            {
                //log.Info("SendToClient pid = " + ne.GetPersistentID() + " forward = " + snapshotCmd.forward);
                (peer as GameClientPeer).ZRPC.ActionRPC.SendAction(ne.GetPersistentID(), snapshotCmd, peer);
                os.lastPos = snapshotCmd.pos;
                os.lastForward = snapshotCmd.forward;
            }

            if (actionCmd != null)
            {
                (peer as GameClientPeer).ZRPC.ActionRPC.SendAction(ne.GetPersistentID(), actionCmd, peer);
                os.lastActionSent = currentTick;                
            }
        }

        public int CompareTo(object obj)
        {
            SnapshotInfo si = obj as SnapshotInfo;
            if (priority < si.priority)
                return 1;
            else
                return-1;
        }
    }

    public class NetServerSlot
    {        
        public const float RelevanceRadius = 15.0f;
        public const float RelevanceBoundaryRadiusSq = 20.0f * 20.0f; //we continue to update entities at buffer zone until timeout
        public const int RelevantTimeOut = 40; //40 ticks * 100msec = 4000msec
        public const int MaxSpawnsPerUpdate = 10;
        public const int MaxRemovePerUpdate = 35;
        public const int MaxSnapshotsPerUpdate = 1;
        public const long SyncServerTimeUpdateRate = 30000;

        public int mReleventQueryTicker = 0;
        public int mUpdateTicker = 0;
        public GameClientPeer mPeer;        

        //protected Dictionary<int, NetEntity> mLocalEntities; //Entities belonging to this player
        protected Dictionary<int, ObjectState> mSpawnedObjects;
        protected Dictionary<int, bool> mLimitedPlayer; //pid <- isenemy;
        private int MaxSpawnLimitPlayer = 9;
        //protected int mPrimaryLocalEntityPID;

        private ServerEntitySystem mEntitysystem;
        private long mLastSynchronizedTime;
        private Zealot.Server.Counters.Profiler mEntitySyncStatsProfiler;
        private Zealot.Server.Counters.Profiler mLocalEntityUpdateProfiler;
        private RealmPVPType mRealmPVPType;

        public NetServerSlot(GameClientPeer peer, ServerEntitySystem entitySystem, RealmPVPType pvpType, bool isCity)
        {
            mPeer = peer;
            mEntitysystem = entitySystem;
            mRealmPVPType = pvpType;
            if (isCity)
                MaxSpawnLimitPlayer = 9;
            else
            {
                switch (mRealmPVPType)
                {
                    case RealmPVPType.Peace:
                        MaxSpawnLimitPlayer = 4; //for 
                        break;
                    case RealmPVPType.FreeForAll:
                    case RealmPVPType.Faction:
                    case RealmPVPType.Guild:
                        MaxSpawnLimitPlayer = 9;
                        break;
                }
            }

            //mPrimaryLocalEntityPID = -1;
            mLastSynchronizedTime = -99999;

            //mLocalEntities = new Dictionary<int, NetEntity>();
            mSpawnedObjects = new Dictionary<int, ObjectState>();
            mLimitedPlayer = new Dictionary<int, bool>();

            mEntitySyncStatsProfiler = new Zealot.Server.Counters.Profiler();
            mLocalEntityUpdateProfiler = new Zealot.Server.Counters.Profiler();
        }

        public void CleanUp()
        {
            //mLocalEntities.Clear();
            mSpawnedObjects.Clear();
            mPeer = null;
            mEntitysystem = null;
        }

        //SetLocalEntity will immediately cause the entity to be spawned in the local client (don't have to wait till next frame)
        public void SetLocalEntity(NetEntity ne)
        {
            int pid = ne.GetPersistentID();
            //mLocalEntities.Add(pid, ne);
            //if (mLocalEntities.Count == 1)            
            //    mPrimaryLocalEntityPID = pid;

            AddSpawnedObject(ne);
            ne.AddEntitySyncStats(mPeer);
            ne.AddLocalObject(mPeer);
        }

        public List<Entity> GetSpawnedObjects()
        {
            List<Entity> entities = new List<Entity>();
            foreach (var kvp in mSpawnedObjects)
                entities.Add(kvp.Value.ne);
            return entities;
        }

        protected bool AddSpawnedObject(BaseNetEntity bne)
        {
            int pid = bne.GetPersistentID();
            if(pid ==1|| pid == 2 || pid == 3)
            {

            }

            if (!mSpawnedObjects.ContainsKey(pid)) //if not spawned for this slot yet
            {
                bne.SpawnAtClient(mPeer);

                ObjectState os = new ObjectState(bne);
                os.lastPos = bne.Position;
                os.lastForward = bne.Forward;
                os.lastTimeFoundRelevant = mEntitysystem.Timers.GetTick();
                mSpawnedObjects.Add(pid, os);
                return true;
            }
            return false;
        }

        protected bool RemoveSpawnedObjct(BaseNetEntity bne)
        {
            int pid = bne.GetPersistentID();
            RemoveSpawnedObjct(pid);
            return true;
        }

        protected void RemoveSpawnedObjct(int pid)
        {
            mLimitedPlayer.Remove(pid);
            mPeer.ZRPC.CombatRPC.DestroyEntity(pid, mPeer);
        }

        protected bool UpdateRelevantObject(BaseNetEntity bne)
        {
            int pid = bne.GetPersistentID();
            if (mSpawnedObjects.ContainsKey(pid))
            {
                ObjectState os = mSpawnedObjects[pid];                
                os.lastTimeFoundRelevant = mEntitysystem.Timers.GetTick();
                os.relevant = true;
                return true;
            }
            return false;
        }

        public bool UpdateRelevantObjects()
        {
            if (mPeer == null || mPeer.mPlayer == null)
                return false;
            Player myPlayer = mPeer.mPlayer;
            int peerOwnerID = myPlayer.GetPersistentID();
            uint tick = mEntitysystem.Timers.GetTick();

            List<BaseNetEntity> newspawnedEnts = new List<BaseNetEntity>();

            mPeer.ZRPC.CombatRPC.BeginRPC(this.mPeer);

            // Sync server time
            long currtime = mEntitysystem.Timers.GetSynchronizedTime();
            if (currtime - mLastSynchronizedTime > SyncServerTimeUpdateRate)
            {
                mPeer.ZRPC.CombatRPC.SetServerTime(currtime, this.mPeer);
                mLastSynchronizedTime = currtime;
            }

            //RESET RELEVANT FLAG IN THE LIST OF SPAWNED OBJECTS
            foreach (KeyValuePair<int, ObjectState> kvp in mSpawnedObjects)
                kvp.Value.relevant = false;

            Vector3 refPos = myPlayer.Position;
            List<Entity> mAlwaysShowNetEntity = mEntitysystem.GetAlwaysShowNetEntities();
            List<Entity> qr = new List<Entity>();
            if (mAlwaysShowNetEntity.Count > 0)
            {
                qr.AddRange(mAlwaysShowNetEntity);
                mEntitysystem.QueryNetEntitiesInCircle(refPos, RelevanceRadius, (queriedEntity) =>
                {
                    return !mAlwaysShowNetEntity.Contains(queriedEntity);
                }, qr);              
            }
            else
                mEntitysystem.QueryNetEntitiesInCircle(refPos, RelevanceRadius, null, qr);

            int spawned = 0;
            //Check Queried entities that need to be spawned (will include local entities but ignore them)
            int myPartyId = myPlayer.PlayerSynStats.Party;
            bool hasParty = myPartyId > 0;
            string myPlayerName = myPlayer.Name;
            foreach (BaseNetEntity bne in qr)
            {
                if (UpdateRelevantObject(bne) == false)
                {
                    if (spawned < MaxSpawnsPerUpdate)
                    {
                        bool allowToSpawn = false;
                        switch (bne.EntityType)
                        {
                            case EntityType.Monster:
                                if (string.IsNullOrEmpty(bne.mSummoner) || bne.mSummoner == myPlayerName)
                                    allowToSpawn = true;
                                break;
                            case EntityType.Player:
                                Player targetPlayer = bne as Player;
                                if (!targetPlayer.InspectMode) //Player in InspectMode will not be seen by others
                                {
                                    if (hasParty && targetPlayer.PlayerSynStats.Party == myPartyId)
                                        allowToSpawn = true;
                                    else
                                    {
                                        bool isEnemy = CombatUtils.IsEnemy(myPlayer.Team, targetPlayer.Team);
                                        if (mLimitedPlayer.Count >= MaxSpawnLimitPlayer)
                                        {
                                            if (isEnemy)
                                            {
                                                int pidToRemove = -1;
                                                foreach (var kvp in mLimitedPlayer)
                                                {
                                                    if (!kvp.Value) //remove a player who not enemy
                                                    {
                                                        pidToRemove = kvp.Key;
                                                        break;
                                                    }
                                                }
                                                if (pidToRemove != -1)
                                                {
                                                    RemoveSpawnedObjct(pidToRemove);
                                                    mSpawnedObjects.Remove(pidToRemove);
                                                    allowToSpawn = true;
                                                    mLimitedPlayer.Add(targetPlayer.GetPersistentID(), true);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            allowToSpawn = true;
                                            mLimitedPlayer.Add(targetPlayer.GetPersistentID(), isEnemy);
                                        }
                                    }
                                }
                                break;
                            default:
                                allowToSpawn = true;
                                break;
                        }
                        if (allowToSpawn && AddSpawnedObject(bne))
                        {
                            newspawnedEnts.Add(bne);
                            spawned++;
                        }
                    }
                }
            }
            
            //Scan spawned objects to delete if they are already destroyed by entitysystem or it has been a while since it has been queried  
            //Make it relevant if it is still not destroyed and at boundary so that entities at boundary can have their snapshots or actions done with continuity
            //int removed = 0;
            bool remove = false;
            int removed = 0;
            List<int> removedPids = new List<int>();
            foreach(KeyValuePair<int, ObjectState> kvp  in mSpawnedObjects)//all new spawned entity surrounding local player and those spawned from last cycle, including of player
            {
                ObjectState os = kvp.Value;
                BaseNetEntity bne = os.ne;
                
                if (bne.GetOwnerID() != peerOwnerID)
                {
                    remove = false;
                    if (bne.Destroyed)
                        remove = true;
                    else
                    {
                        if ((refPos - bne.Position).sqrMagnitude < RelevanceBoundaryRadiusSq)
                        {
                            //as long as it spawned and not exceed RelevanceBoundaryRadiusSq, we will keep it relevant.                    
                            //Otherwise the entity is not relevant this frame, but we still monitor it until relevantTimeOut
                            os.lastTimeFoundRelevant = tick;
                            os.relevant = true;
                        }
                        else if (tick - os.lastTimeFoundRelevant > RelevantTimeOut)
                            remove = true;
                    }
                    if (remove)
                    {
                        if (removed < MaxRemovePerUpdate)//not too many removes at once
                        {
                            RemoveSpawnedObjct(os.ne);
                            removedPids.Add(os.ne.GetPersistentID());
                            removed++;
                        }
                    }
                }
            }

            foreach(int pid in removedPids)
                mSpawnedObjects.Remove(pid);

            mPeer.ZRPC.CombatRPC.EndRPC(this.mPeer);
                                  
            if(newspawnedEnts.Count > 0)
            {
                mPeer.ZRPC.LocalObjectRPC.BeginRPC(this.mPeer);
                foreach(BaseNetEntity bne in newspawnedEnts)//addlocalobject for those newly spawn object
                {
                    bne.AddEntitySyncStats(mPeer);                    
                    //bne.ResetSyncStats();
                }
                mPeer.ZRPC.LocalObjectRPC.EndRPC(this.mPeer);
            }
            return true;
        }

        /*public bool UpdateSnapShotsAndActions(out double prepareTime, out double sendTime) //done for all relevant spawned objects after updaterelevantobjects, to update their snapshot and action
        {
            prepareTime = 0;
            sendTime = 0;
            if (mSpawnedObjects.Count <= 0)
                return false;
            
            Zealot.Server.Counters.Profiler prepareTimeProfiler = new Zealot.Server.Counters.Profiler();
            prepareTimeProfiler.Start();
            uint tick = mEntitysystem.Timers.GetTick();

            List<SnapshotInfo> snapshotInfos = new List<SnapshotInfo>();
            
            foreach (KeyValuePair<int, ObjectState> kvp in mSpawnedObjects)
            {
                ObjectState os = kvp.Value;
                BaseNetEntity bne = os.ne;

                if (((uint)bne.EntityType & (uint)EntityTypeAttribute.ETA_STATIC) == 0)//not static netentity
                {
                    NetEntity ne = (NetEntity)bne;
                    //log.InfoFormat("os.lastActionSent = {0}", os.lastActionSent);                
                    long actionTickElapsed = (long)ne.mLastActionChanged - (long)os.lastActionSent;
                    actionTickElapsed = Math.Max(Math.Min(actionTickElapsed, 100), 0);

                    SnapshotInfo si = new SnapshotInfo(ne, os, tick, (uint)actionTickElapsed);

                    if (!ne.Destroyed && os.relevant && !IsOwnedBySlot(ne))
                    {
                        //send snapshot if dist moved
                        Vector3 pos = ne.Position;
                        float movedist = (pos - os.lastPos).sqrMagnitude;
                        float dotproduct = Vector3.Dot(os.lastForward, ne.Forward);
                        if (movedist > 1.0f || dotproduct < 0.95f) //more than 18 deg difference
                        {                            
                            SnapShotUpdateCommand snapshotCmd = new SnapShotUpdateCommand();
                            snapshotCmd.pos = pos;
                            snapshotCmd.forward = ne.Forward;

                            si.distsqMoved = movedist;
                            if (dotproduct < 0.95f)
                                si.dotproductfactor = 1 - dotproduct;
                            else
                                si.dotproductfactor = 0;
                            si.snapshotCmd = snapshotCmd;
                        }

                        if (actionTickElapsed > 0)
                        {
                            ActionCommand actionCmd = ne.GetActionCmd();
                            si.actionCmd = actionCmd;
                        }
                        si.ComputePriority();
                        if (si.priority > 0)
                            snapshotInfos.Add(si);
                    }
                }
            }

            int sendCount = snapshotInfos.Count;            
            if (sendCount > MaxSnapshotsPerUpdate) //we sort them by priority if there are more than we can send per frame            
            {
                if (MaxSnapshotsPerUpdate == 1)
                {
                    //if max is 1, we just pick the highest priority instead of sorting everything which is alot more expensive O(n) vs O(n^2)
                    SnapshotInfo highestPriorityInfo = snapshotInfos[0];
                    int highestPriorityIndex = 0;
                    for (int i=1;i<snapshotInfos.Count;i++)
                    {
                        if (snapshotInfos[i].priority > highestPriorityInfo.priority)
                        {
                            highestPriorityInfo = snapshotInfos[i];
                            highestPriorityIndex = i;
                        }
                    }
                    if (highestPriorityIndex != 0) //swap to first place
                    {
                        SnapshotInfo temp = snapshotInfos[0];
                        snapshotInfos[0] = highestPriorityInfo;
                        snapshotInfos[highestPriorityIndex] = temp;
                    }
                }
                else
                {
                    snapshotInfos.Sort();                    
                }
                sendCount = MaxSnapshotsPerUpdate;
            }
            prepareTime = prepareTimeProfiler.StopAndGetElapsed();

            Zealot.Server.Counters.Profiler sendTimeProfiler = new Zealot.Server.Counters.Profiler();
            sendTimeProfiler.Start();
            mPeer.ZRPC.ActionRPC.BeginRPC(mPeer);
            for (int i = 0; i < sendCount; i++)
            {                
                SnapshotInfo si = snapshotInfos[i];
                //log.InfoFormat("Snapshotupdate pid {0} priority {1}", si.ne.GetPersistentID(), si.priority);
                si.SendToClient(mPeer);
            }
            //log.InfoFormat("Snapshotupdate Complete^^^^^^^^^^^^^^^^^^^^^");
            mPeer.ZRPC.ActionRPC.EndRPC(mPeer);
            sendTime = sendTimeProfiler.StopAndGetElapsed();
            return true;
        }
        */

        //More optimized version that assumes only 1 entity update per frame specifically
        public bool UpdateSnapShotsAndActions(out double prepareTime, out double sendTime) //done for all relevant spawned objects after updaterelevantobjects, to update their snapshot and action
        {
            prepareTime = 0;
            sendTime = 0;

            if (mPeer == null || mPeer.mPlayer == null)
                return false;
            Player myPlayer = mPeer.mPlayer;
            int peerOwnerID = myPlayer.GetPersistentID();

            if (mSpawnedObjects.Count <= 0)
                return false;

            Zealot.Server.Counters.Profiler prepareTimeProfiler = new Zealot.Server.Counters.Profiler();
            prepareTimeProfiler.Start();
            SnapshotInfo highestPrioritySnapshot = null;
            uint tick = mEntitysystem.Timers.GetTick();

            List<SnapshotInfo> snapshotInfos = new List<SnapshotInfo>();

            foreach (KeyValuePair<int, ObjectState> kvp in mSpawnedObjects)
            {
                ObjectState os = kvp.Value;
                BaseNetEntity bne = os.ne;
                if (os.relevant && ((uint)bne.EntityType & (uint)EntityTypeAttribute.ETA_STATIC) == 0)//not static netentity
                {
                    NetEntity ne = (NetEntity)bne;
                    //log.InfoFormat("os.lastActionSent = {0}", os.lastActionSent);                
                    if (!ne.Destroyed && (ne.GetOwnerID() != peerOwnerID || ne.IsHero()))
                    {
                        long actionTickElapsed = (long)ne.mLastActionChanged - (long)os.lastActionSent;
                        actionTickElapsed = Math.Max(Math.Min(actionTickElapsed, 100), 0);

                        SnapshotInfo si = new SnapshotInfo(ne, os, tick, (uint)actionTickElapsed);

                        //send snapshot if dist moved
                        Vector3 pos = ne.Position;
                        float movedist = (pos - os.lastPos).sqrMagnitude;
                        float dotproduct = Vector3.Dot(os.lastForward, ne.Forward);
                        if (movedist > 1.0f || dotproduct < 0.95f) //more than 18 deg difference
                        {
                            SnapShotUpdateCommand snapshotCmd = new SnapShotUpdateCommand();
                            snapshotCmd.pos = pos;
                            snapshotCmd.forward = ne.Forward;

                            si.distsqMoved = movedist;
                            if (dotproduct < 0.95f)
                                si.dotproductfactor = 1 - dotproduct;
                            else
                                si.dotproductfactor = 0;
                            si.snapshotCmd = snapshotCmd;
                        }

                        if (actionTickElapsed > 0)
                        {
                            ActionCommand actionCmd = ne.GetActionCmd();
                            si.actionCmd = actionCmd;
                            System.Diagnostics.Debug.Print(actionCmd.GetActionType().ToString());
                        }
                        si.ComputePriority();
                        if (si.priority > 0)
                        {
                            if (highestPrioritySnapshot == null)                            
                                highestPrioritySnapshot = si;                            
                            else
                            {
                                if (si.priority > highestPrioritySnapshot.priority)
                                    highestPrioritySnapshot = si;
                            }
                        }
                    }
                }
            }
            
            prepareTime = prepareTimeProfiler.StopAndGetElapsed();

            if (highestPrioritySnapshot != null)
            {
                Zealot.Server.Counters.Profiler sendTimeProfiler = new Zealot.Server.Counters.Profiler();
                sendTimeProfiler.Start();
                mPeer.ZRPC.ActionRPC.BeginRPC(mPeer);

                //log.InfoFormat("Snapshotupdate pid {0} priority {1}", highestPrioritySnapshot.ne.GetPersistentID(), highestPrioritySnapshot.priority);
                highestPrioritySnapshot.SendToClient(mPeer);

                //log.InfoFormat("Snapshotupdate Complete^^^^^^^^^^^^^^^^^^^^^");
                mPeer.ZRPC.ActionRPC.EndRPC(mPeer);
                sendTime = sendTimeProfiler.StopAndGetElapsed();
            }
            return true;
        }

        public void SyncRelevantObjectStats(out double entitysyncstatstime, out double damageresulttime)
        {
            entitysyncstatstime = 0;
            damageresulttime = 0;

            if (mSpawnedObjects.Count <= 0)
                return;
          
            mPeer.ZRPC.LocalObjectRPC.BeginRPC(mPeer);
            mEntitySyncStatsProfiler.Start();
            mPeer.ZRPC.UnreliableCombatRPC.BeginRPC(mPeer);
            foreach (KeyValuePair<int, ObjectState> kvp in mSpawnedObjects)
            {
                ObjectState os = kvp.Value;
                BaseNetEntity bne = os.ne;
                if (os.relevant && !bne.Destroyed)
                {
                    bne.UpdateEntitySyncStats(mPeer); //TODO: should there be a limit to how many local objects can be updated per frame?                    
                }
            }
            mPeer.ZRPC.UnreliableCombatRPC.EndRPC(mPeer);
            entitysyncstatstime = mEntitySyncStatsProfiler.StopAndGetElapsed();

            //foreach (KeyValuePair<int, NetEntity> localkvp in mLocalEntities)
            //{
            //    if (localkvp.Value.EntityType == EntityType.Player) // local entity player can only have one?
            //    {
            //        (localkvp.Value as Player).UpdateLocalObject(mPeer);
            //    }
            //}

            mPeer.mPlayer.UpdateLocalObject(mPeer);

            mPeer.ZRPC.LocalObjectRPC.EndRPC(mPeer);

            // local entities update
            mLocalEntityUpdateProfiler.Start(); //localentity update includes sending damage results relevant to the player. Are we sending too many?
            mPeer.ZRPC.UnreliableCombatRPC.BeginRPC(mPeer);
            //foreach (KeyValuePair<int, NetEntity> localkvp in mLocalEntities)
            //{
            //    if (localkvp.Value.EntityType == EntityType.Player) // local entity player can only have one?
            //    {
            //        (localkvp.Value as Player).UnreliableLocalEntityUpdate(mPeer);
            //    }
            //}

            mPeer.mPlayer.UnreliableLocalEntityUpdate(mPeer);

            mPeer.ZRPC.UnreliableCombatRPC.EndRPC(mPeer);
            damageresulttime = mLocalEntityUpdateProfiler.StopAndGetElapsed();
        }

        public void DispatchChatMessages()
        {
            mPeer.ZRPC.CombatRPC.BeginRPC(mPeer);
          //  if (mLocalEntities.Count > 0)
            {
                //mLocalEntities[mPrimaryLocalEntityPID] as Player;
                Player player = mPeer.mPlayer;
                if(player != null)
                    player.DispatchChatMessages(mPeer);
            }
            mPeer.ZRPC.CombatRPC.EndRPC(mPeer);
        }

        public void RefreshTicker()
        {
            mReleventQueryTicker++;
            if (mReleventQueryTicker == 10)
                mReleventQueryTicker = 0;
            mUpdateTicker++;
            if (mUpdateTicker == 2)
                mUpdateTicker = 0;
        }
    }
}
