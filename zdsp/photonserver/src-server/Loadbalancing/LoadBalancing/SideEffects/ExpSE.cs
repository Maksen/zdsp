namespace Zealot.Server.SideEffects
{
    using System;
    using Zealot.Common;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;
    using Zealot.Repository;
    using Kopio.JsonContracts;

    public class ExpSE : SideEffect
    {
        public ExpSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
        }
        protected override void InitKopioData()
        {
            base.InitKopioData();
            mDuration = 0;
        }
        protected override bool OnApply(int equipid = -1)
        {
            if (!mTarget.IsPlayer())
                return false;
            
            //Exp sideeffect is non durational. It's always once off.
            int exp = (int)GameUtils.Random(mSideeffectData.min, mSideeffectData.max);
            Player player = (Player) mTarget;
            player.AddExperience(exp);

            return base.OnApply(equipid);
        }        
    }
}