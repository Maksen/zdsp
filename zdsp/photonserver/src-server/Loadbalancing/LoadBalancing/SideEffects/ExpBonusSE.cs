namespace Zealot.Server.SideEffects
{
    using System;
    using Zealot.Common;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;
    using Zealot.Repository;
    using Kopio.JsonContracts;

    public class ExpBonusSE : SideEffect, IPassiveSideEffect
    {
        private float mAmount = 0;
        public ExpBonusSE(SideEffectJson sideeffectData)
            : base(sideeffectData)
        {
            mNeedCaster = false;
        }

        protected override bool OnApply()
        {
            if (!mTarget.IsPlayer())
                return false;

            if (base.OnApply())
            {
                if (IsDurationalSE())
                {
                    mAmount  =(float) GameUtils.Random(mSideeffectData.min, mSideeffectData.max);
                    ((Player)mTarget).AddMonsterExpBonus(mAmount * 0.01f); //should not be random otherwise persistent will show inconsistent bonus
                }
                return true;
            }
            return false;
        }
         

        protected override void OnRemove()
        {
            ((Player)mTarget).AddMonsterExpBonus(-mAmount * 0.01f);
            base.OnRemove();
        }

        #region PassiveSideEffect
        public void AddPassive(Actor target, bool isSkill=false)
        {
            mTarget = target;
            if(!mTarget.IsPlayer())
                return;

            mAmount = (float)GameUtils.Random(mSideeffectData.min, mSideeffectData.max);

            ((Player)mTarget).AddMonsterExpBonus(mAmount * 0.01f);
        }

        public void RemovePassive(bool isSkill = false)
        {
            ((Player)mTarget).AddMonsterExpBonus(-mAmount * 0.01f);
        }
        #endregion
    }
}
