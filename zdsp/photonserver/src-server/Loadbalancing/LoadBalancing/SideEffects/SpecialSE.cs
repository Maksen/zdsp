namespace Zealot.Server.SideEffects
{
    using System.Collections.Generic;
    using System;
    using Zealot.Common;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;
    using Zealot.Repository;
    using Kopio.JsonContracts;

    public class SpecialSE  
    {
        //public readonly static long MAX_DURATION = 604800000; //7 days, in msecs
        public SideEffectJson mSideeffectData;

        public Actor mTarget; 
        public long mTotalElapsedTime; 
        protected long mDuration;
        protected long mInterval;
        protected int Rank;
        protected bool mbInfinite = false;
        float mAmount=0;
        FieldName mTargetedField = FieldName.LastField; 
        private bool mIsPostiveBuff;
        public SpecialSE(SideEffectJson sideeffectData)        
        { 
            mSideeffectData = sideeffectData;
            mDuration =(long)(mSideeffectData.duration * 1000);
            mInterval = (long)(mSideeffectData.interval * 1000);
            InitKopioData();
        }

        public bool Apply(Actor target )
        {
            mTarget = target;
            if (CheckApplyCondition())
            {
                //Persistent sideeffect always added to the last in the list. 
                //hence make sure 30 is enough for this.  //null slot is not reused. 
                bool applied = mTarget.AddSpecialSideEffect(this);
                if (applied)
                    OnApply();
            }
           
            return true;  
        }

        private void OnApply(int equipid = -1)
        {
           
            switch (mSideeffectData.effecttype)
            {
                //case EffectType.Rejuvenate_Health:
                //    mAmount = (int) GameUtils.Random(mSideeffectData.min, mSideeffectData.max);
                //    mTarget.OnRecoverHealth(mAmount);
                //    break;
                //case EffectType.Stats_HealthMax:
                //case EffectType.StatsAttack_Attack:
                case EffectType.StatsAttack_Accuracy:
                case EffectType.StatsAttack_Critical:
                case EffectType.StatsAttack_CriticalDamage:
                case EffectType.StatsDefence_Armor:
                //case EffectType.StatsDefence_CoCriticalDamage:
                case EffectType.StatsDefence_CoCritical:
                case EffectType.StatsDefence_Evasion:
                    SideEffectsUtils.GetStatsFieldAndValue(mSideeffectData, mTarget.CombatStats, out mTargetedField,out mAmount, mIsPostiveBuff);
                    if (mTargetedField != FieldName.LastField)
                    {
                        FieldName noscorefield = SideEffectsUtils.GetNoScroreField(mTargetedField);
                        mTarget.CombatStats.AddToField(noscorefield, mAmount); 
                        mTarget.CombatStats.ComputeAll();
                    }                    
                    break;

            }
        }
        
        protected void InitKopioData()
        {
            if(mSideeffectData.persistentafterdeath ==false)//&&mSideeffectData.persistentonlogout == false)
            {
                throw new Exception("SpecialSE created for Persistent true entry");
            }

            if (//mSideeffectData.effecttype == EffectType.Stats_HealthMax_Debuff ||
                //mSideeffectData.effecttype == EffectType.StatsAttack_Attack_Debuff ||
                mSideeffectData.effecttype == EffectType.StatsAttack_Accuracy_Debuff ||
                mSideeffectData.effecttype == EffectType.StatsAttack_Critical_Debuff ||
                mSideeffectData.effecttype == EffectType.StatsAttack_CriticalDamage_Debuff ||
                mSideeffectData.effecttype == EffectType.StatsDefence_Armor_Debuff ||
                //mSideeffectData.effecttype == EffectType.StatsDefence_CoCriticalDamage_Debuff ||
                mSideeffectData.effecttype == EffectType.StatsDefence_CoCritical_Debuff ||
                mSideeffectData.effecttype == EffectType.StatsDefence_Evasion_Debuff )  
            {
                mIsPostiveBuff = false;
            }else
            {
                mIsPostiveBuff = true;
            }
            if (mDuration <= 0)
                mbInfinite = true;
        }
 
 
        protected virtual bool CheckApplyCondition()
        {
            bool shouldApply = true;
            List<SpecialSE> currentList = mTarget.GetPersistentSEList();
            List<SpecialSE> listOfInterest = new List<SpecialSE>();
            foreach (SpecialSE spe in currentList)
            {
                if(spe.mSideeffectData.effecttype == mSideeffectData.effecttype)
                {
                    listOfInterest.Add(spe);
                }
            }
            if (listOfInterest.Count > 0)
            {                 
                for (int i = listOfInterest.Count -1; i >=0; i--)
                {
                    SpecialSE se = listOfInterest[i];
                    if (SideEffectRepo.InSameGroup(mSideeffectData.id, se.mSideeffectData.id))
                    {
                        if (SideEffectRepo.CanOverride(mSideeffectData.id, se.mSideeffectData.id))
                        {
                            se.Stop(); //overwrite by the samegroup
                            shouldApply = true;
                        }
                        else
                        {
                            shouldApply = false;//someone in the list have higher priority than me
                            break;
                        }
                    }
                }

            }
            return shouldApply;
        } 

        public long GetTimeRemaining()
        {
            return Math.Max(mDuration - mTotalElapsedTime, 0);
        }      

        private long intervalAccumulated = 0;
        public  void OnInterval(long dt)
        {
            intervalAccumulated+=dt;
            if (mInterval > 0)
            {
            //    if (mSideeffectData.effecttype == EffectType.Rejuvenate_Health)
            //    {
            //        if (intervalAccumulated > mInterval)
            //        {
            //            intervalAccumulated = 0;
            //            if (mTarget.IsAlive())
            //                mTarget.OnRecoverHealth(mAmount);
            //        }
            //    }
            }
            mTotalElapsedTime += dt;
            if(mDuration <= mTotalElapsedTime && mDuration > 0)
            {
                Stop();
            }
        }

        public void OnRemove()
        {
            if (mTargetedField != FieldName.LastField)
            {
                mTarget.CombatStats.AddToField(mTargetedField, -mAmount);//all system and changes are in whole numbers.
                FieldName noscorefield = SideEffectsUtils.GetNoScroreField(mTargetedField);
                mTarget.CombatStats.AddToField(noscorefield, -mAmount);
                mTarget.CombatStats.ComputeAll();
            }
        }

        public void Stop()
        {
            OnRemove();
            mTarget.RemoveSpecialSideEffect(this);
        }
    }
     
}
