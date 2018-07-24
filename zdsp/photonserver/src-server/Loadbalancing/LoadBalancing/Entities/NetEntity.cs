namespace Zealot.Server.Entities
{
    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Server.Actions;
    using Photon.LoadBalancing.GameServer;    
    using System.Collections.Generic;
    using Zealot.Common.Datablock;

    #region BaseNetEntity
    public abstract class BaseNetEntity : Entity, IBaseNetEntity
    {
        public const int ServerOwnerID = 0;

        public GameLogic mInstance;
        protected int mnPersistentID;
        protected int mnOwnerID;
        public string mSummoner = ""; //only summoner can see this entity

        #region Abstract methods
        public abstract void SpawnAtClient(GameClientPeer peer);
        #endregion

        public BaseNetEntity():base()
        {
            mnOwnerID = ServerOwnerID;  //own by server
        }

        //Test if entity is garbage collected:
        //~BaseNetEntity()
        //{
        //    int destroyedpid = this.mnPersistentID;
        //}

        public virtual void SetInstance(GameLogic instance)
        {
            mInstance = instance;
        }

        public int GetPersistentID()
        {
            return mnPersistentID;
        }

        public void SetPersistentID(int pid)
        {
            mnPersistentID = pid;
        }

        public void SetOwnerID(int id)
        {
            mnOwnerID = id;
        }

        public int GetOwnerID()
        {
            return mnOwnerID;
        }

        public bool IsServerControl()
        {
            return mnOwnerID == ServerOwnerID;
        }        

        public virtual void ResetSyncStats() {}
        public virtual void UpdateLocalObject(GameClientPeer peer) { }
        public virtual void AddLocalObject(GameClientPeer peer) { }
        
        public virtual void UpdateEntitySyncStats(GameClientPeer peer) { }
        public virtual void AddEntitySyncStats(GameClientPeer peer) { }
    }
    #endregion

    #region NetEntity
    public abstract class NetEntity : BaseNetEntity, INetEntity
    {        
        //Peter, TODO: use actioncontroller?
        protected Action mAction;
        protected ActionCommand mActionCmd;
        public uint mLastActionChanged;       

        public NetEntity() : base()
        {            
            mLastActionChanged = 0;
        }                       
      
        public bool PerformAction(Action action, bool force = false, bool queue = false)
        {
            bool canStart = true;
            if (mAction != null)
            {
                ACTIONTYPE currActionType = (ACTIONTYPE)mAction.mdbCommand.GetActionType();
                var manager = AuthoASInterruptManager.manager;
                if (!force && manager.ContainsKey(currActionType))
                {
                    var interruptInfo = manager[currActionType];
                    ACTIONTYPE newActionType = (ACTIONTYPE)action.mdbCommand.GetActionType();
                    if (interruptInfo.ContainsKey(newActionType))
                        canStart = interruptInfo[newActionType](mAction, action);
                }
                if (canStart && !mAction.IsCompleted())
                {
                    mAction.Stop();
                    //System.Diagnostics.Debug.WriteLine(action.mdbCommand.GetType().Name + " interrupted " + mAction.mdbCommand.GetType().Name);
                }
            }
            if (canStart)
            {
                mAction = action;
                action.Start();
                //System.Diagnostics.Debug.WriteLine("Start New action " + action.mdbCommand.GetType().Name);
            }
            return canStart;
        }

        public void StopAction()
        {
            if (mAction != null && !mAction.IsCompleted())
            {
                mAction.Stop();
            }
        }

        public void ClearAction()
        {
            mAction = null;
        }

        public Action GetAction()
        {
            return mAction;
        }

        public void SetAction(ActionCommand cmd)
        {
            mActionCmd = cmd;            
            mLastActionChanged = EntitySystem.Timers.GetTick();
        }

        public void SetActionFromClient(ActionCommand cmd)
        {
            mActionCmd = cmd;
            mLastActionChanged = EntitySystem.Timers.GetTick() + 1; //So that in the next main loop, this action gets broadcasted to other nonlocal clients
        }

        public ActionCommand GetActionCmd()
        {
            return mActionCmd;
        }        

        public override void Update(long dt)
        {
            if (mAction != null)
            {
                mAction.OnUpdate(dt);
            }
        }

        public override void OnRemove()
        {
            base.OnRemove();
            if (mAction != null)
            {
                mAction.Stop();
                mAction = null;
            }
            //In case action is not stopped and seeker is not removed. So, we remove again here.
            PathManager.RemoveSeeker(mInstance.mRoom.Guid, GetPersistentID());
        }
    }
    #endregion

    #region StaticNetEntity
    public abstract class StaticNetEntity : BaseNetEntity, IStaticNetEntity
    {
        private StateMachine mStateMachine;
        private Dictionary<string, byte> mNameToStateIndex;
        private byte mNextStateIndex;

        protected StaticObjectStat mdbStats;

        public StaticNetEntity() : base()
        {
            mStateMachine = new StateMachine();
            mdbStats = null;
            mNameToStateIndex = new Dictionary<string, byte>();
            mNextStateIndex = 0;
            AddState("Active", OnActiveEnter, OnActiveLeave, OnActiveUpdate);
            AddState("InActive", OnInActiveEnter, OnInActiveLeave, OnInActiveUpdate);          
        }
                
        protected void AddState(string stateName, StateMachine.State.OnEnterStateDelegate enterDelegate, 
                                                  StateMachine.State.OnLeaveStateDelegate leaveDelegate, 
                                                  StateMachine.State.OnUpdateDelegate updateDelegate)
        {
            mStateMachine.AddState(stateName, enterDelegate, leaveDelegate, updateDelegate);
            mNameToStateIndex.Add(stateName, mNextStateIndex);
            mNextStateIndex++;
        }

        //Note that the order of Adding States at server StaticNetEntity and client StaticNetEntityGhost
        //have to be the same.
        protected void AddState(string stateName, StateMachine.State.OnEnterStateDelegate enterDelegate,
                                                  StateMachine.State.OnLeaveStateDelegate leaveDelegate)
        {
            mStateMachine.AddState(stateName, enterDelegate, leaveDelegate);
            mNameToStateIndex.Add(stateName, mNextStateIndex);
            mNextStateIndex++;
        }

        public string GetCurrentStateName()
        {
            return mStateMachine.GetCurrentStateName();
        }

        public byte GetCurrentStateIndex()
        {
            return mNameToStateIndex[mStateMachine.GetCurrentStateName()];
        }

        public void GotoState(string stateName)
        {            
            mStateMachine.GotoState(stateName);
            if (mdbStats != null)
                mdbStats.State = GetCurrentStateIndex();
        }

        public override void Update(long dt)
        {
            mStateMachine.OnUpdate(dt);
        }

        #region Active State
        protected virtual void OnActiveEnter(string prevstate) {}
        protected virtual void OnActiveUpdate(long dt) {}
        protected virtual void OnActiveLeave() {}        
        #endregion

        #region InActive State
        protected virtual void OnInActiveEnter(string prevstate) {}
        protected virtual void OnInActiveUpdate(long dt) {}
        protected virtual void OnInActiveLeave() {}        
        #endregion


        public override void AddEntitySyncStats(GameClientPeer peer)
        {
            if (mdbStats != null)
                peer.ZRPC.LocalObjectRPC.AddLocalObject((byte)LOCATEGORY.EntitySyncStats, GetPersistentID(), mdbStats, peer);
        }

        public override void UpdateEntitySyncStats(GameClientPeer peer)
        {
            if (mdbStats != null && mdbStats.IsDirty())
                peer.ZRPC.LocalObjectRPC.UpdateLocalObject((byte)LOCATEGORY.EntitySyncStats, GetPersistentID(), mdbStats, peer);
        }

        public override void ResetSyncStats()
        {
            if (mdbStats != null)
                mdbStats.Reset();
        }
    }
    #endregion
}