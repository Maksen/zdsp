namespace Zealot.Server.SideEffects
{
    using Zealot.Common;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;
    using Kopio.JsonContracts;

    public class StatsSE : SideEffect, IPassiveSideEffect
    {
        float mAmount=0;
        FieldName mTargetedField;
        private float mAffactBonus = 0;
        private float mAffactPerc = 0;
        private bool mIsPostiveBuff;

        public StatsSE(SideEffectJson sideeffectData)
            : base(sideeffectData)
        {
            mNeedCaster = false;
        }
         
        protected override void InitKopioData()
        {
            base.InitKopioData();

            mIsPostiveBuff = SideEffectsUtils.IsPositiveEffectType(mSideeffectData.effecttype);
        }

        public override bool IsBuff()
        {
            return mIsPostiveBuff ==true;
        }

        public override bool IsDeBuff()
        {
            return mIsPostiveBuff == false;
        }

        public override void OnEnhanceBuffAffact(float perc, float value)
        {
            //base.OnEnhanceBuffAffact(perc, value);
            mAffactBonus = value;
            mAffactPerc = perc;
        }

        public override void OnEnhanceBuffDuration(float perc, float val)
        {
            //base.OnEnhanceBuffDuration(perc, val);
            if (mDuration > 0 && (perc > 0 || val > 0))//all duration buff get affected.
            {
                mDuration = (long)(mDuration * (1 + perc * 0.01f) + val * 1000);
                mDuration = mDuration >= 0 ? mDuration : 0;
            }
        }

        private bool CheckImmunity()
        {
            //TODO: handle the immunity state of actor.
            Actor actor = mTarget as Actor;
            EffectType efftype = mSideeffectData.effecttype;
            if(actor != null)
            {
                if(SideEffectsUtils.IsNegativeEffectType(efftype)) {
                    return actor.CheckImmuneStatus(ImmuneSEType.AllDebuff);
                }
                //if (efftype == EffectType.StatsAttack_Accuracy_Debuff
                //        //|| efftype == EffectType.StatsAttack_Attack_Debuff
                //        || efftype == EffectType.StatsAttack_Critical_Debuff
                //        || efftype == EffectType.StatsAttack_CriticalDamage_Debuff)
                //{
                //    return actor.PlayerStats.invincibleStatsAtk; 
                //}
                //if (efftype == EffectType.StatsDefence_Armor_Debuff
                //        || efftype == EffectType.StatsDefence_CoCritical_Debuff
                //        || efftype == EffectType.StatsDefence_Evasion_Debuff)
                //        //|| efftype == EffectType.StatsDefence_CoCriticalDamage_Debuff)
                //{
                //    return actor.PlayerStats.invincibleStatsDef;
                //}
            }
            return false;
        }
          
        protected override bool OnApply(int equipid = -1)
        {
            if (!CheckImmunity() && base.OnApply(equipid)) //statsSE is not interval updated.
            {
                //mainskill support debuff also as the proc time is needed.
                if (IsDurationalSE())
                {
                    SideEffectsUtils.GetStatsFieldAndValue(mSideeffectData, mTarget.CombatStats, out mTargetedField, out mAmount, mIsPostiveBuff);
                    if(mAffactBonus > 0 || mAffactBonus > 0)
                    {
                        if (mSideeffectData.isrelative)
                            mAmount += (int)(mAffactPerc * 10);//this mamount is base on 1000
                        else
                            mAmount += (int)mAffactBonus;
                    }
                    if (mTargetedField != FieldName.LastField)//active skill need to update the noscoreField always.
                    {
                        FieldName noscorefield = SideEffectsUtils.GetNoScroreField(mTargetedField);
                        mTarget.CombatStats.AddToField(noscorefield, mAmount);
                        mTarget.CombatStats.ComputeAll();                   
                    }
                    return true;
                }
                else // include in non-durational buffs
                {                 
                    if (equipid != -1)
                    {
                        bool result = (mPositiveEffect) 
                            ? mTarget.AddEquipmentSideEffect(this, equipid) : mTarget.RemoveEquipmentSideEffect(this, equipid);
                        if (!result)
                            return false;
                    }

                    SideEffectsUtils.GetStatsFieldAndValue(mSideeffectData, mTarget.CombatStats, out mTargetedField, out mAmount, mPositiveEffect);
                    mTarget.CombatStats.AddToField(mTargetedField, mAmount);
                    mTarget.CombatStats.ComputeAll();
                    return true;
                }
            }
            else
                return false;
        }

        protected override void OnRemove()
        {
            if (mTargetedField!=FieldName.LastField)
            {
                //mTarget.CombatStats.AddToField(mTargetedField, -mAmount);//all system and changes are in whole numbers.
                FieldName noscorefield = SideEffectsUtils.GetNoScroreField(mTargetedField);
                mTarget.CombatStats.AddToField(noscorefield, -mAmount);
                mTarget.CombatStats.ComputeAll();
            } 
            base.OnRemove();
        }

        #region PassiveSideEffect
        public void AddPassive(Actor target, bool isSkill=false)
        {
            //Passive sideeffect should not be random i.e. max == min. Responsibility of designers to ensure that.
            mTarget = target; 
            SideEffectsUtils.GetStatsFieldAndValue(mSideeffectData, mTarget.CombatStats, out mTargetedField,out mAmount , mIsPostiveBuff);
            if (isSkill)
            {
                FieldName noscorefield = SideEffectsUtils.GetNoScroreField(mTargetedField);
                mTarget.CombatStats.AddToField(noscorefield, mAmount);
            }
            else
            {
                mTarget.CombatStats.AddToField(mTargetedField, mAmount);
            }
            mTarget.CombatStats.ComputeAll();
        }

        public void RemovePassive(bool isSkill=false)
        {
            
            if (isSkill)
            {
                FieldName noscorefield = SideEffectsUtils.GetNoScroreField(mTargetedField);
                mTarget.CombatStats.AddToField(noscorefield, -mAmount);
            }else
            {
                mTarget.CombatStats.AddToField(mTargetedField, -mAmount);
            }
            mTarget.CombatStats.ComputeAll();
        }
        #endregion
    }

    public class ToNoneElementalSE : SideEffect {
        public ToNoneElementalSE(SideEffectJson sideeffectData)
            : base(sideeffectData) {
            mNeedCaster = false;
        }

        protected override void InitKopioData() {
            base.InitKopioData();
        }

        protected override bool OnApply(int equipid = -1) {
            if (base.OnApply(equipid)) {
                mTarget.CombatStats.SetField(FieldName.ElementSideEffect, (float)Element.None);
                return true;
            }
            return false;

        }
    }

    public class ToMetalElementalSE : SideEffect {
        public ToMetalElementalSE(SideEffectJson sideeffectData)
            : base(sideeffectData) {
            mNeedCaster = false;
        }

        protected override void InitKopioData() {
            base.InitKopioData();
        }

        protected override bool OnApply(int equipid = -1) {
            if (base.OnApply(equipid)) {
                mTarget.CombatStats.SetField(FieldName.ElementSideEffect, (float)Element.Metal);
                return true;
            }
            return false;

        }
    }

    public class ToWoodElementalSE : SideEffect {
        public ToWoodElementalSE(SideEffectJson sideeffectData)
            : base(sideeffectData) {
            mNeedCaster = false;
        }

        protected override void InitKopioData() {
            base.InitKopioData();
        }

        protected override bool OnApply(int equipid = -1) {
            if (base.OnApply(equipid)) {
                mTarget.CombatStats.SetField(FieldName.ElementSideEffect, (float)Element.Wood);
                return true;
            }
            return false;

        }
    }

    public class ToEarthElementalSE : SideEffect {
        public ToEarthElementalSE(SideEffectJson sideeffectData)
            : base(sideeffectData) {
            mNeedCaster = false;
        }

        protected override void InitKopioData() {
            base.InitKopioData();
        }

        protected override bool OnApply(int equipid = -1) {
            if (base.OnApply(equipid)) {
                mTarget.CombatStats.SetField(FieldName.ElementSideEffect, (float)Element.Earth);
                return true;
            }
            return false;

        }
    }

    public class ToWaterElementalSE : SideEffect {
        public ToWaterElementalSE(SideEffectJson sideeffectData)
            : base(sideeffectData) {
            mNeedCaster = false;
        }

        protected override void InitKopioData() {
            base.InitKopioData();
        }

        protected override bool OnApply(int equipid = -1) {
            if (base.OnApply(equipid)) {
                mTarget.CombatStats.SetField(FieldName.ElementSideEffect, (float)Element.Water);
                return true;
            }
            return false;

        }
    }

    public class ToFireElementalSE : SideEffect {
        public ToFireElementalSE(SideEffectJson sideeffectData)
            : base(sideeffectData) {
            mNeedCaster = false;
        }

        protected override void InitKopioData() {
            base.InitKopioData();
        }

        protected override bool OnApply(int equipid = -1) {
            if (base.OnApply(equipid)) {
                mTarget.CombatStats.SetField(FieldName.ElementSideEffect, (float)Element.Fire);
                return true;
            }
            return false;

        }
    }
}
