using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Client.Entities;
using Zealot.Common.Actions; 
using Kopio.JsonContracts;
using Zealot.Repository;

namespace Zealot.Client.Actions
{    
    public class BaseWalkAndCast : BaseClientCastSkill
    {
        private const float Epsilon = 0.04f;
        protected const float EpsilonSqrt = 0.2f;
        protected bool mNewStart = true;
        protected Vector3 startDir;
        protected Vector3 mDesiredForward;
         
        
        public BaseWalkAndCast(Entity entity, ISkillCastCommandCommon cmd) : base(entity, cmd)
        {
             
        }
        
        public void ComputeStartDir()
        {
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            Vector3 forward = ((WalkAndCastCommand)mdbCommand).targetPos - mEntity.Position;
            forward.y = 0;
            if (forward.sqrMagnitude < 0.05f)
                forward = ghost.Forward;
            forward.Normalize();
            mDesiredForward = forward;
            startDir = forward;
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
        }

        protected override void OnActiveUpdate(long dt) //dt in msec
        {
            base.OnActiveUpdate(dt);
            ActorGhost ghost = (ActorGhost)mEntity;           
            
            Vector3 forward = ((WalkAndCastCommand)mdbCommand).targetPos - ghost.Position;
            forward.y = 0;
            float distToTarget = forward.magnitude;

            forward = forward / distToTarget; //normalize            
            if (distToTarget <= EpsilonSqrt || (Vector3.Dot(startDir, forward) < 0.0f)) //or if turning back
            {
                //if (dotproduct < 0.0f)
                //    Debug.LogFormat("distToTarget = {0} dotproduct = {1}", distToTarget, dotproduct);

                ghost.Position = ((WalkAndCastCommand)mdbCommand).targetPos;                
                return;
            }
            mDesiredForward = forward;
            float moveSpeed = ghost.PlayerStats.MoveSpeed;
            Vector3 motion = ClientUtils.MoveTowards(forward, distToTarget, moveSpeed, dt / 1000.0f);
            ghost.Move(motion);
        }

         

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
            if (mSkillName != null)
            {
                NetEntityGhost ghost = (NetEntityGhost)mEntity;
                ghost.StopEffect(mSkillName);
            }
        }
    }

    public class ClientAuthoWalkAndCast : BaseWalkAndCast
    {
        public ClientAuthoWalkAndCast(Entity entity, ISkillCastCommandCommon cmd) : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            ghost.SetAction(mdbCommand);
            //PlayskillCutIn(mSkillData.skillgroupJson.id);
            //ApplyPassiveSkill();
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
            PlayerGhost ghost = mEntity as PlayerGhost;
            //TODO: handle root and stun;
            /*
            if (ghost != null)
            {
                if (ghost.IsRooted()) //handle root when walking and casting
                {
                    WalkAndCastCommand cmd = (WalkAndCastCommand)mdbCommand;
                    Vector3 forward = cmd.targetPos - ghost.Position;
                    forward.y = 0;
                    float distToTarget = forward.magnitude;

                    forward = forward / distToTarget; //normalize            
                    if (!(distToTarget <= EpsilonSqrt || (Vector3.Dot(startDir, forward) < 0.0f))) //if movement required
                    {
                        WalkToNewPos(ghost.Position);
                        return;
                    }
                }
            }
            */
            base.OnActiveUpdate(dt);
            ((NetEntityGhost)mEntity).Forward = mDesiredForward; //client authoritative walk is called from (analog) joystick input and does not need facing interpolation
        }

        public void WalkToNewPos(Vector3 pos)
        {
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            WalkAndCastCommand cmd = (WalkAndCastCommand) mdbCommand;
            cmd.targetPos = pos;

            ComputeStartDir();
            
            ghost.SetAction(mdbCommand);
        }
    }


    public class NonClientAuthoWalkAndCast : BaseWalkAndCast
    {
        protected const float RotateCompleteTime = 400;     //in msec
        private float mRotateElapsed;

        public NonClientAuthoWalkAndCast(Entity entity, ISkillCastCommandCommon cmd) : base(entity, cmd)
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

        protected override void OnActiveUpdate(long dt) //dt in msec
        {
            base.OnActiveUpdate(dt);
            //mRotateElapsed += dt;
            //float slerpT = Mathf.Min(mRotateElapsed / RotateCompleteTime, 1.0f);
            //NetEntityGhost ghost = (NetEntityGhost)mEntity;
            //Vector3 interpolatedForward = Vector3.Slerp(ghost.Forward, mDesiredForward, slerpT);
            //ghost.Forward = interpolatedForward;
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            ghost.Forward = mDesiredForward;
        }
    }
}