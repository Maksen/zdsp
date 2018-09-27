using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Common.Entities;

namespace Zealot.Client.Actions
{
    using Zealot.Common.Actions;

    public class BaseClientWalk : Zealot.Common.Actions.Action //: AWalk
    {
        private const float Epsilon = 0.04f;
        private const float EpsilonSqrt = 0.2f;
        protected bool mNewStart = true;
        private Vector3 startDir;
        protected Vector3 mDesiredForward;

        public BaseClientWalk(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {
            //base.OnActiveEnter(prevstate);
            NetEntityGhost ghost = (NetEntityGhost)mEntity;

            if (mNewStart)
            {
                //Debug.Log ("NetEntityGhost [" + ghost.GetPersistentID () + "] animate walk");
                ghost.PlayEffect(ghost.GetRunningAnimation());
            }

            Vector3 forward = ((WalkActionCommand)mdbCommand).targetPos - mEntity.Position;
            forward.y = 0;
            if (forward.sqrMagnitude < 0.05f)
                forward = ghost.Forward;
            forward.Normalize();
            mDesiredForward = forward;
            startDir = forward;
        }

        protected override void OnActiveUpdate(long dt) //dt in msec
        {
            ActorGhost ghost = (ActorGhost)mEntity;

            Vector3 targetpos = ((WalkActionCommand)mdbCommand).targetPos;
            Vector3 forward = targetpos - ghost.Position;
            forward.y = 0;
            float distToTarget = forward.magnitude;

            forward = forward / distToTarget; //normalize
            if (distToTarget <= EpsilonSqrt || (Vector3.Dot(startDir, forward) < 0.0f)) //or if turning back
            {
                ghost.Position = targetpos;
                GotoState("Completed");
                return;
            }
            mDesiredForward = forward;

            if (ghost.IsHitted())
                return;

            float moveSpeed = ((WalkActionCommand)mdbCommand).speed;
            if (moveSpeed == 0)
                moveSpeed = ghost.PlayerStats.MoveSpeed;
            Vector3 motion = ClientUtils.MoveTowards(forward, distToTarget, moveSpeed, dt / 1000.0f);
            ghost.Move(motion);
        }

        public override bool Update(Action newAction)
        {
            if (newAction.mdbCommand.GetActionType() == mdbCommand.GetActionType())
            {
                mdbCommand = newAction.mdbCommand;
                mNewStart = false;
                GotoState("Active");
                return true;
            }
            return false;
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
        }
    }

    public class ClientAuthoACWalk : BaseClientWalk //Responsible for Setting Action
    {
        public ClientAuthoACWalk(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            ghost.SetAction(mdbCommand);
        }

        protected override void OnCompleteEnter(string prevstate)
        {
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            IdleActionCommand cmd = new IdleActionCommand();
            ghost.PerformAction(new ClientAuthoACIdle(ghost, cmd));

            base.OnCompleteEnter(prevstate);
        }

        protected override void OnActiveUpdate(long dt) //dt in msec
        {
            base.OnActiveUpdate(dt);
            if (currentState.Name == "Active")
                ((NetEntityGhost)mEntity).Forward = mDesiredForward; //client authoritative walk is called from (analog) joystick input and does not need facing interpolation
        }
    }

    public class NonClientAuthoACWalk : BaseClientWalk
    {
        protected const float RotateCompleteTime = 400;     //in msec
        private float mRotateElapsed;

        public NonClientAuthoACWalk(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
            mRotateElapsed = 0;
        }

        protected override void OnCompleteEnter(string prevstate)
        {
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            IdleActionCommand cmd = new IdleActionCommand();
            ghost.PerformAction(new NonClientAuthoACIdle(ghost, cmd));

            base.OnCompleteEnter(prevstate);
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            //NetEntityGhost ghost = mEntity as NetEntityGhost;
            //ghost.PlayEffect(ghost.GetRunningAnimation());//client may walk from other place. which is show without animation
        }

        protected override void OnActiveUpdate(long dt) //dt in msec
        {
            base.OnActiveUpdate(dt);
            mRotateElapsed += dt;
            float slerpT = Mathf.Min(mRotateElapsed / RotateCompleteTime, 1.0f);
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            Vector3 interpolatedForward = Vector3.Slerp(ghost.Forward, mDesiredForward, slerpT);
            ghost.Forward = interpolatedForward;
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            ghost.Forward = mDesiredForward;
        }
    }

}