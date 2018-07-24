namespace Zealot.Server.SideEffects
{
    using System;
    using Zealot.Common;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;
    using Zealot.Repository;
    using Kopio.JsonContracts;

    public class WorldExpBonusSE : SideEffect
    {
        public WorldExpBonusSE(SideEffectJson sideeffectData)
            : base(sideeffectData)
        {
            mNeedCaster = false;
        }

        protected override bool OnApply()
        {
            if (!mTarget.IsPlayer())
                return false;

            DateTime now = DateTime.Now;
            int hours = (24 - now.Hour) - 1;
            int minutes = (60 - now.Minute) - 1;
            int seconds = (60 - now.Second) - 1;
            mDuration = (seconds + (minutes * 60) + (hours * 3600)) * 1000;
            mTotalElapsedTime = 0;

            if (base.OnApply())
            {
                return true;
            }
            return false;
        }

        protected override void OnRemove()
        {            
            base.OnRemove();
        }

    }
}
