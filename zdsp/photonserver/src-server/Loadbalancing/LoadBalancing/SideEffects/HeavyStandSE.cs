namespace Zealot.Server.SideEffects {
    using System;
    using Zealot.Common;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;
    using Zealot.Repository;
    using Kopio.JsonContracts;
    public class HeavyStandSE : SideEffect {
        public HeavyStandSE(SideEffectJson sideeffectData) : base(sideeffectData) {
            mNeedCaster = true;
        }
        protected override void InitKopioData() {
            base.InitKopioData();
        }

        protected override bool OnApply(int equipid = -1) {
            if (base.OnApply(equipid)) {
                mTarget.PlayerStats.HeavyStand = true;
                return true;
            }
            return false;
        }

        protected override void OnRemove() {
            base.OnRemove();
            mTarget.PlayerStats.HeavyStand = false;
        }
    }
}
