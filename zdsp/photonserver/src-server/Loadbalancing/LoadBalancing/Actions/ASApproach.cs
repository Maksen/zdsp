namespace Zealot.Server.Actions
{
    using UnityEngine;
    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;

    public class ServerAuthoASApproach : Action
    {
        private const float Epsilon = 0.04f;
        protected Entity mTarget;
        protected Vector3 mTargetPos;
        protected float mRangeSqr;

        public ServerAuthoASApproach(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {
        }

        public override void Start()
        {
            ApproachCommand cmd = (ApproachCommand)mdbCommand;
            if (cmd.targetpid != -1)
            {
                mTarget = mEntity.EntitySystem.GetEntityByPID(cmd.targetpid);
                if (mTarget == null || mTarget.Destroyed)
                {
                    GotoState("Completed");
                    return;
                }
            }
            else
                mTargetPos = cmd.targetpos;
            
            mRangeSqr = cmd.range * cmd.range;
            base.Start();
        }       

        protected override void OnActiveEnter(string prevstate)
        {
            NetEntity entity = (NetEntity)mEntity;
            entity.SetAction(mdbCommand);
        }

        protected override void OnActiveUpdate(long dt) //dt in msec
        {
            Actor entity = (Actor)mEntity;
            Vector3 targetPos = (mTarget == null) ? mTargetPos : mTarget.Position;
            Vector3 forward = targetPos - entity.Position;
            forward.y = 0;

            if (forward.sqrMagnitude <= mRangeSqr + Epsilon)
            {
                GotoState("Completed");
                return;
            }

            if (entity.IsGettingHit())
                return;

            forward.Normalize();
            entity.Forward = forward;
            float moveSpeed = entity.PlayerStats.MoveSpeed;
            entity.Position = Vector3.MoveTowards(entity.Position, targetPos - ((ApproachCommand)mdbCommand).range * forward, moveSpeed * dt / 1000.0f);
        }

        public override bool Update(Action newAction)
        {
            if (newAction.mdbCommand.GetActionType() == mdbCommand.GetActionType())
            {
                ApproachCommand currentCmd = (ApproachCommand)mdbCommand;
                ApproachCommand newCmd = (ApproachCommand)newAction.mdbCommand;
                if(currentCmd.targetpid == newCmd.targetpid && currentCmd.targetpos == newCmd.targetpos && 
                    currentCmd.range == newCmd.range)
                    return true;
            }
            return false;
        }
    }
}
