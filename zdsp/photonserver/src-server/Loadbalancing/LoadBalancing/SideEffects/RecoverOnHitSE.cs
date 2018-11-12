namespace Zealot.Server.SideEffects
{
    using Zealot.Common;
    using Kopio.JsonContracts;

    public class RecoverOnHitSE : SideEffect
    {
        private int mHitRemaining;
        private bool mInfiniteHits;
        //private float mProcChance;

        public RecoverOnHitSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mHitRemaining = (int)float.Parse(mSideeffectData.parameter);            
            mInfiniteHits = mHitRemaining <= 0;
            //procchanceis reserved for the sideeffect apply chance.
            //mProcChance = mSideeffectData.procchance / 100.0f; //we reuse procchance for recoveronhit se for chance to recover when hit
            mNeedCaster = false;
        }

       
        protected override bool OnApply(int equipid = -1)
        {          
            if (base.OnApply(equipid))
            {    
                if(IsDurationalSE())
                {
                    //if (mTarget.recoverOnHitSE != null)
                        //mTarget.recoverOnHitSE.Stop();//only one instance is active at the time for this speically
                    mTarget.recoverOnHitSE = this;
                    return true;
                }
                else
                    return false;
            }
           
            return false;
        }

        protected override void OnRemove()
        {
            mTarget.recoverOnHitSE = null;
            base.OnRemove();
        }

        public void OnHit(int dmg)
        {
            if (!mInfiniteHits)
            {
                mHitRemaining--;
                if (mHitRemaining < 0)
                {
                    Stop();
                    return;
                }
            }

            //if (GameUtils.GetRandomGenerator().NextDouble() <= mProcChance)
            {
                double randomAmt = GameUtils.Random(mSideeffectData.min, mSideeffectData.max);
                int amount = 0;
                if (mSideeffectData.isrelative)
                    amount = (int)(randomAmt * 0.01f * dmg); 
                else
                    amount = (int)randomAmt;
                mTarget.OnRecoverHealth(amount);
            }
        }

        public override bool IsHot()
        {
            return true;
        }
    }
}