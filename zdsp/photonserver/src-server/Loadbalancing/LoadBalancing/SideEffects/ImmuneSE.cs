#define DEBUG_SIDEEFF
namespace Zealot.Server.SideEffects {
    using Zealot.Common;
    using Zealot.Server.Entities;
    using Kopio.JsonContracts;

    public abstract class ImmuneSE : SideEffect {
        protected ImmuneSEType m_ImmumeSEType;

        public ImmuneSE(SideEffectJson sideeffectData) : base(sideeffectData) {
            mNeedCaster = false;
        }

        protected override void InitKopioData() {
            base.InitKopioData();
        }

        protected override bool CheckApplyCondition() {
            return true;
        }
        protected virtual void OnStart() {

        }

        protected override bool OnApply(int equipid = -1) {
            if (base.OnApply(equipid)) //need for update time
            {
            }
            return false;
        }

        protected override void OnRemove() {
            base.OnRemove();//remove first then call SetControlStatus

        }
    }

    public class ImmuneAllDamageSE : ImmuneSE {
        public ImmuneAllDamageSE(SideEffectJson sideeffectData) : base(sideeffectData) {
        }
    }
}