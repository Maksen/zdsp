namespace Zealot.Client.Actions
{
	using UnityEngine;
	using Zealot.Common;
	using Zealot.Common.Actions;
	using Zealot.Common.Entities;
	using Zealot.Client.Entities;

	public class BaseClientApproach : Action
	{
        private const float Epsilon = 0.04f;
        private const float RotateCompleteTime = 400;     //in msec
            
        protected Entity mTarget;
		protected Vector3 mTargetPos;
		protected float mRange;
        private float mRotateElapsed;
        private Vector3 mDesiredForward;

        public BaseClientApproach(Entity entity, ActionCommand cmd)
			: base(entity, cmd)
		{
		}
		
		public override void Start()
		{
            mDesiredForward = mEntity.Forward;
            mRotateElapsed = 0;
            ApproachCommand cmd = (ApproachCommand)mdbCommand;
			if (cmd.targetpid != -1)
			{
				mTarget = mEntity.EntitySystem.GetEntityByPID(cmd.targetpid);
				if (mTarget == null || mTarget.Destroyed)
				{
                    ForceIdle();
                    return;
				}
			}
			else
				mTargetPos = cmd.targetpos;		
			mRange = cmd.range;
			base.Start();
		}       

		protected override void OnActiveEnter(string prevstate)
		{
			//base.OnActiveEnter(prevstate);            
			NetEntityGhost ghost = (NetEntityGhost)mEntity;
			//Debug.Log ("NetEntityGhost [" + ghost.GetPersistentID() + "] animate walk");			
			ghost.PlayEffect(ghost.GetRunningAnimation() );
            //Debug.Log("Moving with ACApproach!");

        }


        protected override void OnActiveUpdate(long dt) //dt in msec
		{
			ActorGhost ghost = (ActorGhost)mEntity;
            if (mTarget != null && mTarget.Destroyed)
            {
                ForceIdle();
                return;
            }
           
            Vector3 targetPos = (mTarget == null) ? mTargetPos : mTarget.Position;
			Vector3 forward = targetPos - ghost.Position;
			forward.y = 0;
			float distToTarget = forward.magnitude;
			if (distToTarget <= mRange + Epsilon)
			{
				GotoState("Completed");
				return;
			}            

            if (ghost.IsHitted())
                return;

            forward.Normalize();
            mRotateElapsed += dt;
            mDesiredForward = forward;
            
            float slerpT = Mathf.Min(mRotateElapsed / RotateCompleteTime, 1.0f);
            Vector3 interpolatedForward = Vector3.Slerp(ghost.Forward, mDesiredForward, slerpT);
            
            ghost.Forward = interpolatedForward;

            float moveSpeed = ghost.PlayerStats.MoveSpeed;
            Vector3 motion = ClientUtils.MoveTowards (forward, distToTarget - mRange, moveSpeed, dt / 1000.0f);
			ghost.Move(motion);	
		}

        protected virtual void ForceIdle()
        {
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            IdleActionCommand cmd = new IdleActionCommand();
            ghost.PerformAction(new ClientAuthoACIdle(ghost, cmd), true);
        }
	
	    protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            ghost.Forward = mDesiredForward;
        }
	}
	
    public class ClientAuthoACApproach : BaseClientApproach
    {
        public ClientAuthoACApproach(Entity entity, ActionCommand cmd)
			: base(entity, cmd)
		{
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            ghost.SetAction(mdbCommand);
        }       
    }

    public class NonClientAuthoACApproach : BaseClientApproach
	{
		public NonClientAuthoACApproach(Entity entity, ActionCommand cmd)
			: base(entity, cmd)
		{
		}

		protected override void OnCompleteEnter(string prevstate)
		{
            ForceIdle();
        }
    }
}


