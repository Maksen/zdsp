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
        private int mTotalAmount = 0;
        private int mDeductAmt = 0;
        private bool mUnLimited = false;

        public ShieldSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            
            mNeedCaster = false;
            
        }

        protected override bool OnApply(int equipid = -1)
        {
            mUnLimited = float.Parse(mSideeffectData.parameter) <= 0; //minus value means unlimited amount
            mTotalAmount = (int)float.Parse(mSideeffectData.parameter);//value update in skillcombo.
            mDeductAmt = (int) mSideeffectData.max;
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
            int amount = 0; 
            amount = (int)(0.01f * mDeductAmt * dmg);
            amount = dmg - amount; //the damage after deduction.
            if (!mUnLimited) 
            {
                if (amount < mTotalAmount)
                {
                    mTotalAmount -= amount;
                    amount = 0;
                }
                else
                {
                    amount = amount - mTotalAmount;
                    mTotalAmount = 0;
                    Stop();
                }
            }
            if (mUnLimited && mDeductAmt == 0)//this case is strange
            {
                amount = 0; 
            }
            return amount;
        }

        public override bool IsBuff()
        {
            return true;
        }
    }
}