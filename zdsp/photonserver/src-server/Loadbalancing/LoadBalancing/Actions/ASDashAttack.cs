using UnityEngine;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Common.Entities;
using Zealot.Server.Entities;
using Zealot.Repository;
using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Server.SideEffects;
using Zealot.Server.Rules;
using Zealot.Server.Actions;

namespace Zealot.Server.Actions
{    
    public class ServerAuthoDashAttack : BaseServerCastSkill
    {
        protected Vector3 mTargetPos;
        protected float mDashSpeed;
        public ServerAuthoDashAttack(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {
            if (cmd is DashAttackCommand)
            {
                mTargetPos = ((DashAttackCommand)cmd).targetpos;
            }
        }

        public override void Start()
        {
            base.Start();
            mDashSpeed = mSkillData.skillJson.range / mSkillData.skillJson.skillduration;
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            Actor actor = (Actor)mEntity;
            actor.SetAction(mdbCommand);
        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
            Actor actor = (Actor)mEntity;
            Vector3 forward = mTargetPos - mEntity.Position;
            forward.y = 0;
            float distToTarget = forward.magnitude;
            if(distToTarget < 0.04)
            {
                return;
            }
            forward.Normalize();
            mEntity.Position = Vector3.MoveTowards(mEntity.Position, mTargetPos, mDashSpeed * dt / 1000.0f);

        }
    } 
     
    public class NonServerAuthoDashAttack : NonServerAuthoCastSkill
    {
        //protected long mDashDuration;
        protected float mDashSpeed = 20.0f;
        protected Vector3 mTargetPos;
        public NonServerAuthoDashAttack(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {
        }
        
        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            //because query is done when se procs. but if I am moving forward. the time i query will 
            //miss those target in the area behind me, so i query first before doing se proc. 
            DashAttackCommand cmd = mdbCommand as DashAttackCommand;            
            //mDashDuration = (long)(cmd.dashduration * 1000);//NOT USED.handled in basic class
            mDashSpeed = 20.0f;
            mTargetPos = cmd.targetpos;
            Vector3 dir = cmd.targetpos - mEntity.Position;
            mEntity.Forward = dir.normalized;
            AcquireTargets();//targets query at the start of casting skill 
        }
         

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();

            ((NetEntity)mEntity).ClearAction();
        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
            return;
        }

         
    }    
}
