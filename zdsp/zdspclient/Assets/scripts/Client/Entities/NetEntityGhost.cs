using UnityEngine;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Common.Entities;
using Xft;
using Zealot.Common.Datablock;


namespace Zealot.Client.Entities
{
    //using System;
    using Zealot.Client.Actions;

    public abstract class BaseNetEntityGhost : BaseClientEntity, IBaseNetEntity
    {
        protected int mnPersistentID;
        protected int mnOwnerID;
                
        public bool IsLocal { get; set; }
        
        public int GetPersistentID()
        {
            return mnPersistentID;
        }

        public void SetPersistentID(int id)
        {
            mnPersistentID = id;
        }

        public void SetOwnerID(int id)
        {
            mnOwnerID = id;
        }

        public int GetOwnerID()
        {
            return mnOwnerID;
        }

        #region LOCALOBJECT

        private Dictionary<LOTYPE, LocalObject> mLocalObjects;
        public LocalObject GetLocalObject(LOTYPE objtype)
        {
            if (mLocalObjects!= null && mLocalObjects.ContainsKey(objtype))
                return mLocalObjects[objtype];

            return null;
        }

        public virtual void AddLocalObject(LOTYPE objtype, LocalObject obj)
        {
            if (mLocalObjects == null)
                mLocalObjects = new Dictionary<LOTYPE, LocalObject>();

            mLocalObjects.Add(objtype, obj);
        }

        public virtual void RemoveLocalObject(LOTYPE objtype)
        {
            if (mLocalObjects.ContainsKey(objtype))
            {
                mLocalObjects.Remove(objtype);
            }
        }
        #endregion
    }


    public abstract class NetEntityGhost : BaseNetEntityGhost, INetEntity
    {
        //Peter, TODO: use actioncontroller?
        protected Action mAction;
        protected ActionCommand mActionCmd;
        public bool mActionSent;
        protected ActorNameTagController mHeadLabel;
        public ActorNameTagController HeadLabel
        {
            get { return mHeadLabel; }
            set { mHeadLabel = value; }
        }

        protected CharacterController mCharController;
        public CharacterController CharController
        {
            get { return mCharController; }
            set { mCharController = value; }
        }

        // Dynamic attach gameobject to entityghost, 
        // TODO: populate value from kopio
        public override void Init()
        {
            base.Init();
            InitCharacterController();
        }

        public void InitCharacterController()
        {
            if(AnimObj != null)
            {
                CharController = AnimObj.GetComponent<CharacterController>();
                // ===================================================================
                /*mCharController = animObj.AddComponent<CharacterController>();
                mCharController.slopeLimit = 45;
                mCharController.stepOffset = 0.3f;
                mCharController.center = new Vector3 (0, 1.5f, 0);
                mCharController.radius = 0.81f;
                mCharController.height = 2.5f;
                */
                // ====================================================================	
                mHeadLabel = AnimObj.AddComponent<ActorNameTagController>();
                //float yoffset = 140 * 4 / 3 / GameInfo.gCombat.CameraAspect;
                //mHeadLabel.nameTagOffsetPos = new Vector2(0, yoffset);  //Hard offset. This cannot be set only once, camera angle and position is always changing

                //Transform effectRef = AnimObj.transform.Find("root/effect_buff");
                //float heightOffset = (effectRef == null) ? 3.8f : effectRef.localPosition.y;
                mHeadLabel.CreatePlayerLabel();
                // =====================================================================
                //mEfxHandler = animObj.AddComponent<EffectHandler>();            						

                mActionSent = false;
            }
        }        

        public void OnGhostDie()
        {
            if (mHeadLabel != null)
            {
                mHeadLabel.OnGhostDied();
            }

            // deselect if local player is dead or selected entity is dead
            if (IsLocal || this == GameInfo.gSelectedEntity) 
                GameInfo.gCombat.OnSelectEntity(null);
        }         

        public virtual float RtReduction()
        {
            return 1.0f;
        }

        public virtual bool IsHitted()
        {
            return false;
        }
        public virtual bool PerformAction(Action action, bool force = false, bool queue = false)
        { 
            bool canStart = true;
            if (mAction != null)
            {
                ACTIONTYPE currActionType = mAction.mdbCommand.GetActionType();               
                var manager = this.IsLocal ? AuthoACInterruptManager.manager : NonAuthoACInterruptManager.manager;               
                if (!force && manager.ContainsKey(currActionType))
                {
                    var interruptInfo = manager[currActionType];
                    ACTIONTYPE newActionType = action.mdbCommand.GetActionType();
                    if (interruptInfo.ContainsKey(newActionType))
                        canStart = interruptInfo[newActionType](mAction, action);
                }
                if (canStart)
                {
                    if (!mAction.IsCompleted())
                    {
                        mAction.Stop();
                        LogManager.DebugLog(action.mdbCommand.GetType().Name + " interrupted " + mAction.mdbCommand.GetType().Name);
                    }
                }
                else if (queue)
                {
                    mAction.SetCompleteCallback(() => PerformAction(action));
                    LogManager.DebugLog(action.mdbCommand.GetType().Name + " queued "  );

                }

            }
            if (canStart)
            {
                mAction = action;
                action.Start();
                LogManager.DebugLog("Start New action " + action.mdbCommand.GetType().Name);
            }
            return canStart;
        }

        public virtual string GetStandbyAnimation()
        {
            return "standby";
        }

        public virtual string GetDyingEffect()
        {
            return "";
        }

        public virtual string GetWeaponExtension()
        {
            return "";
        }

        public virtual long GetDyingDuration()
        {
            return 2000;
        }

        public virtual string GetRunningAnimation()
        {
            return "run";
        }


        public virtual string GetHitAnimation()
        {
            return "gethit";
        }

        public virtual string GetHitCritEffect() {
            return "hit_crit";
        }


        public Action GetAction()
        {
            return mAction;
        }

        public void SetAction(ActionCommand cmd)
        {
            mActionCmd = cmd;
            mActionSent = false;
        }

        public void SetActionDontSend(ActionCommand cmd)
        {
            mActionCmd = cmd;
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
        }

        public virtual void Move(Vector3 motion)
        {
            if (mCharController == null)
                return;
            mCharController.Move(motion);
            Transform transform = mAnimObj.transform;
            Position = transform.position;
        }

        public virtual void MoveWithTransform(Vector3 motion,float movespeed,float delta)
        {
            Position = Vector3.MoveTowards(Position, motion, delta * movespeed);
        }
         
    }

    public abstract class StaticNetEntityGhost : BaseNetEntityGhost, IStaticNetEntity
    {
        private StateMachine mStateMachine;        
        private Dictionary<byte, string> mStateIndexToName;
        private byte mNextStateIndex;

        protected StaticObjectStat mdbStats;

        public StaticNetEntityGhost() : base()
        {
            this.EntityType = EntityType.LootGhost;
            mStateMachine = new StateMachine();
            mdbStats = null;
            mStateIndexToName = new Dictionary<byte, string>();
            mNextStateIndex = 0;
            AddState("Active", OnActiveEnter, OnActiveLeave, OnActiveUpdate);
            AddState("InActive", OnInActiveEnter, OnInActiveLeave, OnInActiveUpdate);         
        }      

        protected void AddState(string stateName, StateMachine.State.OnEnterStateDelegate enterDelegate,
                                                  StateMachine.State.OnLeaveStateDelegate leaveDelegate,
                                                  StateMachine.State.OnUpdateDelegate updateDelegate)
        {
            mStateMachine.AddState(stateName, enterDelegate, leaveDelegate, updateDelegate);            
            mStateIndexToName.Add(mNextStateIndex, stateName);
            mNextStateIndex++;
        }

        protected void AddState(string stateName, StateMachine.State.OnEnterStateDelegate enterDelegate,
                                                  StateMachine.State.OnLeaveStateDelegate leaveDelegate)
        {
            mStateMachine.AddState(stateName, enterDelegate, leaveDelegate);            
            mStateIndexToName.Add(mNextStateIndex, stateName);
            mNextStateIndex++;
        }

        public string GetCurrentStateName()
        {
            return mStateMachine.GetCurrentStateName();
        }

        public string StateIndexToName(byte index)
        {
            return mStateIndexToName[index];
        }
        
        public void GotoState(string stateName)
        {
            mStateMachine.GotoState(stateName);
        }

        public override void Update(long dt)
        {
            mStateMachine.OnUpdate(dt);
        }

        #region Active State
        protected virtual void OnActiveEnter(string prevstate) { }
        protected virtual void OnActiveUpdate(long dt) { }
        protected virtual void OnActiveLeave() { }
        #endregion

        #region InActive State
        protected virtual void OnInActiveEnter(string prevstate) { }
        protected virtual void OnInActiveUpdate(long dt) { }
        protected virtual void OnInActiveLeave() { }
        #endregion
    }
}
