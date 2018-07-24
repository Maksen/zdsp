using Zealot.Common.Entities;
using UnityEngine;

namespace Zealot.Common.Actions
{
    public abstract class ABaseDashAttack : Action
    {        
        protected const long ATTACK_DURATION = 400;

        protected const float DefaultDashDur = 0.2f;        
        
        protected Vector3 mTargetPos;
        protected float mRange;

        protected long mDashDuration; //Peter, TODO: for now we allow duration to be keyed and tested by designers. By right, they should be constants.
        protected long mAttackDuration;

        public ABaseDashAttack(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
            AddState("Attack", OnAttackEnter, OnAttackLeave);
        }

        protected abstract void OnAttackEnter(string prevstate);

        protected abstract void OnAttackLeave();        

        //protected virtual void OnAttackUpdate(long dt)
        //{
        //}
    }
}

