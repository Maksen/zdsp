namespace Zealot.Client.Actions
{
    using UnityEngine;
    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Client.Entities;
    using Zealot.Repository;
    using System.Collections.Generic;

    public class BaseDashAttack : BaseClientCastSkill
    {
        private const float Epsilon = 0.04f;
        private Vector3 mTargetPos;
        private float mDashSpeed;
        public BaseDashAttack(Entity entity, ISkillCastCommandCommon cmd)
            : base(entity, cmd)
        {
        }

        public override void Start()
        {
            DashAttackCommand cmd = (DashAttackCommand)mdbCommand;
            mTargetPos = cmd.targetpos;
            mDashSpeed = mSkillData.skillJson.range / mSkillData.skillJson.skillduration;
            Vector3 direction = mTargetPos - mEntity.Position;
            direction.y = 0f;
            direction.Normalize();
            ((NetEntityGhost)mEntity).Forward = direction;//forward updated here
            GotoState("Active");
        }

        protected override void PlaySkillEffect()
        {
            ((ActorGhost)mEntity).PlayEffect(mSkillData.skillgroupJson.action, mSkillName + "1");
            ((ActorGhost)mEntity).PlaySEEffect( mSkillName + "2", mEntity.Forward);//play in the world
        }

        //protected long warningPeriod = 100;
        //protected override void SetupSkillCastTimer()
        //{
        //    //100 is the warningperiod for effect, skill duration is not affacted.
        //    mfSkillDuration = mSkillData.skillgroupJson.skillduration + warningPeriod * 0.001f;
        //    SetTimer((long)(1000 * mfSkillDuration), OnActiveTimeUp, null);
        //}
        protected override void OnActiveUpdate(long dt) //dt in msec
        {
            base.OnActiveUpdate(dt);
            //if (warningPeriod > 0)
            //{
            //    warningPeriod -= dt;
            //    if (warningPeriod < 0)
            //    {
            //        ((ActorGhost)mEntity).PlayEffect(mSkillData.skillgroupJson.action, mSkillName + "1");
            //    }
            //    return;
            //}

            ActorGhost ghost = (ActorGhost)mEntity;
            Vector3 forward = mTargetPos - ghost.Position;
            forward.y = 0;
            float distToTarget = forward.magnitude;
            if (distToTarget <= Epsilon)
            {
                //GotoState("Completed"); 
                return;
            }
            forward.Normalize();
            Vector3 motion = ClientUtils.MoveTowards(forward, distToTarget, mDashSpeed, dt / 1000.0f);
            ghost.Move(motion);
        }

        public void OnCollide()
        {
            Debug.Log("Dash attack Collided");
            //GotoState("Attack");
            //GotoState("Completed");
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
            //NetEntityGhost ghost = (NetEntityGhost)mEntity;
            //ghost.StopEffect(mSkillName + "1");
            //ghost.StopEffect(mSkillName + "2"); 
        }
    }

    public class ClientAuthoDashAttack : BaseDashAttack
    {
        public ClientAuthoDashAttack(Entity entity, ISkillCastCommandCommon cmd)
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

    public class NonClientAuthoDashAttack : BaseDashAttack
    {
        public NonClientAuthoDashAttack(Entity entity, ISkillCastCommandCommon cmd)
            : base(entity, cmd)
        {
        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
        }
    }





}


