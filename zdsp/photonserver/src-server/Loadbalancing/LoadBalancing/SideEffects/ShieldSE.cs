namespace Zealot.Server.SideEffects
{
    using System;
    using Zealot.Common;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;
    using Zealot.Repository;
    using Kopio.JsonContracts;

    public class ShieldSE : SideEffect
    { 
        private float mTotalAmount = 0;

        public ShieldSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            
            mNeedCaster = false;
            
        }

        protected override bool OnApply(int equipid = -1)
        {
            if (mSideeffectData.isrelative)
            {
                mTotalAmount = mTarget.GetHealthMax() * mSideeffectData.max * 0.01f;
            }
            else
            {
                mTotalAmount = mSideeffectData.max;
            }

            if (base.OnApply(equipid))
            {  
                if (IsDurationalSE())
                {
                    //if (mTarget.shieldSE != null)
                    //    mTarget.shieldSE.Stop();//no stop the old to support multiply,  if need to 
                    //can set two shiledSE of the same group to overrite,  
                    mTarget.shieldSE = this;
                    return true;
                }             
                else
                    return false;
            }
            return false;
        }

        protected override void OnRemove()
        {
            mTarget.shieldSE = null;
            base.OnRemove();
        }

        public int OnAttacked(int dmg)
        {
            //System.Diagnostics.Debug.WriteLine("your message here"); 
            mTotalAmount -= dmg;

            if(mTotalAmount <= 0)
            {
                Stop();
                return (int)Math.Abs(mTotalAmount);
            }
            
            return 0;
        }

        public override bool IsBuff()
        {
            return true;
        }
    }
}