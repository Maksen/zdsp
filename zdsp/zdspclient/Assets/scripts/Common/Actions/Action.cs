
namespace Zealot.Common.Actions
{
    using Common;
    using Entities;
    using System;

    public abstract class Action : StateMachine
    {        
        //TODO: datablock/event of action
        protected Entity mEntity;
        protected Timers mTimers;
        protected GameTimer timer;
		public ActionCommand mdbCommand;

        public delegate void CompleteCallBackDelegate();
        protected CompleteCallBackDelegate mCompleteCallBack;

		public Action(Entity entity, IActionCommand cmd)
        {
            AddState("Active", OnActiveEnter, OnActiveLeave, OnActiveUpdate);
            AddState("Completed", OnCompleteEnter, null);
            AddState("Terminated", OnTerminatedEnter, null);

            mEntity = entity;
            if (cmd is ActionCommand)
                mdbCommand = cmd as ActionCommand;
            else
                throw new Exception("please make sure you Interace command is inherited from ActionCommand");
            mTimers = mEntity.EntitySystem.Timers;
            mCompleteCallBack = null;
        }

       
        public Entity GetEntity()
        {
            return mEntity;
        }

        public void SetCompleteCallback(CompleteCallBackDelegate del)
        {
            mCompleteCallBack += del;
        }

        public CompleteCallBackDelegate GetCompleteCallback()
        {
            return mCompleteCallBack;
        }

        public override void OnPreLeaveState()
        {
            if (timer != null)
            {
                mTimers.StopTimer(timer);
                timer = null;
            }
        }

        protected void SetTimer(long duration, TimerDelegate del, object arg)
        {
            if (timer != null)
            {
                mTimers.StopTimer(timer);
            }

            timer = mTimers.SetTimer(duration, del, arg);
        }

        public virtual void Start()
        {
            GotoState("Active");
        }

        public virtual void Stop()
        {
            if (timer != null)
            {
                mTimers.StopTimer(timer);
            }
            mCompleteCallBack = null;
            GotoState("Terminated");
        }

        #region Active State
        protected virtual void OnActiveEnter(string prevstate)
        {

        }
        protected virtual void OnActiveLeave()
        {

        }

        protected virtual void OnActiveUpdate(long dt)
        {
        }
        #endregion

        #region Complete State
        //Ended in Success
        protected virtual void OnCompleteEnter(string prevstate)
        {
            if (mCompleteCallBack != null)
                mCompleteCallBack();
        }
        #endregion

        #region Terminated State
        //Ended in Failure
        protected virtual void OnTerminatedEnter(string prevstate)
        {
            //log.Debug("Action Terminated");
        }
        #endregion

		public bool IsCompleted()
		{
            if (currentState != null)
                return currentState.Name == "Completed";
            else
                return true;
		}

		public virtual bool Update(Action newAction)
		{
			return false;
		}
    }

	public class DummyAction : Action
	{
		public DummyAction(Entity entity, ActionCommand cmd) : base(entity, cmd) {
		}
	}
}
