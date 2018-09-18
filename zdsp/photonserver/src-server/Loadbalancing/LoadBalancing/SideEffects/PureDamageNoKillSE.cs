namespace Zealot.Server.SideEffects
{
    using Zealot.Common;    
    using Kopio.JsonContracts;    

    public class PureDamageNoKillSE : SideEffect
    {                
        public PureDamageNoKillSE(SideEffectJson sideeffectData) : base(sideeffectData)
        {
            mDuration = 0; //This sideeffect doesn't support interval. It's once off.
        }
        
        protected override bool OnApply(int equipid = -1)
        {
            if (base.OnApply(equipid))
            {                
                int dmg = 0;

                if (mSideeffectData.isrelative)
                    dmg = (int)(mSideeffectData.max *  0.01f * mTarget.GetHealthMax());
                else
                    dmg = (int) mSideeffectData.max;

                int health = mTarget.GetHealth();
                if (dmg >= health)
                {
                    dmg = health - 1;
                    //note that this sideeffect can be used to self inflict damage, so the damage should not kill self.
                }

                if (dmg == 0) //when hp is 1
                    return false;

                AttackResult res = new AttackResult(mTarget.GetPersistentID(),  dmg, false, mCaster.GetPersistentID());
                mTarget.OnDamage(mCaster.GetOwner(), res, false);

                return true;
            }
            return false;
        }                
    }
}
