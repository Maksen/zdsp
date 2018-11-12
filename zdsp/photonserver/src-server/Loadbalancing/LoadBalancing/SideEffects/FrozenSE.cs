namespace Zealot.Server.SideEffects
{
    using Kopio.JsonContracts;
    using Zealot.Server.Entities;
    using Zealot.Common;

    class FrozenSE : ControlSE
    {
        public FrozenSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = true;
            mControlSEType = ControlSEType.Freeze | ControlSEType.Stun;
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (IsImmune)
            {
                //mTarget.ControlStats.StunImmuned  = true;
                mTarget.SetImmune(ImmuneSEType.Freeze);
                return;
            }
            //mTarget.OnFrozen();
            //mTarget.SetControlStatus(ControlSEType.Freeze);
        }

        public override bool IsHot()
        {
            return false;
        }

        protected override bool OnApply(int equipid = -1)
        {
            if (base.OnApply(equipid))
            {
                if (mTarget.IsMonster())
                {
                    Monster target = mTarget as Monster;
                    target.SetControlStatus(mControlSEType);
                    target.OnFrozen(mDuration);
                }
                return true;
            }


            return false;
        }

        protected override void OnInterval()
        {
            base.OnInterval();


        }

        protected override void OnRemove()
        {
            mTarget.RemoveControlStatus(mControlSEType);
            base.OnRemove();
        }
    }
}
