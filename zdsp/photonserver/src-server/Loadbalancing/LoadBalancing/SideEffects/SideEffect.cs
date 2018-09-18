namespace Zealot.Server.SideEffects
{
    using Kopio.JsonContracts;
    using System;
    using System.Collections.Generic; 
    using Zealot.Common;
    using Zealot.Server.Entities;
    using Zealot.Repository; 

    public static class SideEffectFactory
    {
        private static readonly Dictionary<EffectType, Type> EffectTypeToClass = new Dictionary<EffectType, Type>()
        {
            {EffectType.Damage_NoElementDamage, typeof(DamageSE)},
            {EffectType.Damage_MetalDamage, typeof(DamageSE) },
            {EffectType.Damage_WoodDamage, typeof(DamageSE) },
            {EffectType.Damage_EarthDamage, typeof(DamageSE) },
            {EffectType.Damage_WaterDamage,typeof(DamageSE) },
            {EffectType.Damage_FireDamage, typeof(DamageSE) },
            {EffectType.Damage_DamageBasedOnWeaponElement, typeof(DamageSE) },
            {EffectType.Damage_PureDamage, typeof(DamageSE) },

            {EffectType.Stats_Strength, typeof(StatsSE) },
            {EffectType.Stats_Agility, typeof(StatsSE) },
            {EffectType.Stats_Dexterity, typeof(StatsSE) },
            {EffectType.Stats_Constitution, typeof(StatsSE) },
            {EffectType.Stats_Intelligence, typeof(StatsSE) },
            {EffectType.Stats_AttackSpeed, typeof(StatsSE) },
            {EffectType.Stats_AttackSpeed_Debuff, typeof(StatsSE) },
            {EffectType.Stats_CastSpeed, typeof(StatsSE) },
            {EffectType.Stats_CastSpeed_Debuff, typeof(StatsSE) },
            {EffectType.Stats_MoveSpeed, typeof(StatsSE) },
            {EffectType.Stats_MoveSpeed_Debuff, typeof(StatsSE) },
            {EffectType.Stats_ExpBonus, typeof(ExpBonusSE) },
            {EffectType.Stats_MaxHealth, typeof(StatsSE) },
            {EffectType.Stats_HealthRegen, typeof(StatsSE) },
            {EffectType.Stats_MaxMana, typeof(StatsSE) },
            {EffectType.Stats_ManaRegen, typeof(StatsSE) },
            {EffectType.Stats_EnergyShield, typeof(StatsSE) },
            {EffectType.Stats_IgnoreArmor, typeof(StatsSE) },
            {EffectType.Stats_ChangeEleToNone, typeof(ToNoneElementalSE) },
            {EffectType.Stats_ChangeEleToMetal, typeof(ToMetalElementalSE) },
            {EffectType.Stats_ChangeEleToWood, typeof(ToWoodElementalSE) },
            {EffectType.Stats_ChangeEleToEarth, typeof(ToEarthElementalSE) },
            {EffectType.Stats_ChangeEleToWater, typeof(ToWaterElementalSE) },
            {EffectType.Stats_ChangeEleToFire, typeof(ToFireElementalSE) },
            {EffectType.Stats_HeavyStand, typeof(HeavyStandSE) },
            {EffectType.Stats_SkillCostReduce, typeof(SideEffect) },
            {EffectType.Stats_SkillAffectEnhance, typeof(SideEffect) },
            {EffectType.Stats_HealingPoint, typeof(StatsSE) },
            {EffectType.Stats_HealingPoint_Debuff, typeof(StatsSE) },
            {EffectType.Stats_HealingEffect, typeof(StatsSE) },
            {EffectType.Stats_HealingEffect_Debuff, typeof(StatsSE) },
            {EffectType.Stats_HealingIncome, typeof(StatsSE) },

            {EffectType.Rejuvenate_HealthPotion, typeof(RejuvenateSE) },
            {EffectType.Rejuvenate_ManaPotion, typeof(RejuvenateSE) },
            {EffectType.Rejuvenate_Healing, typeof(RejuvenateSE) },

            {EffectType.StatsAttack_WeaponAttack, typeof(StatsSE) },
            {EffectType.StatsAttack_WeaponAttack_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_AttackPower, typeof(StatsSE) },
            {EffectType.StatsAttack_AttackPower_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_Accuracy, typeof(StatsSE) },
            {EffectType.StatsAttack_Accuracy_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_Critical, typeof(StatsSE) },
            {EffectType.StatsAttack_Critical_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_CriticalDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_CriticalDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_IncSmashDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_IncSmashDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_IncSliceDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_IncSliceDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_IncPierceDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_IncPierceDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_IncEleNoneDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_IncEleNoneDamageDebuff, typeof(StatsSE) },
            {EffectType.StatsAttack_IncEleMetalDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_IncEleMetalDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_IncEleWoodDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_IncEleWoodDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_IncEleEarthDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_IncEleEarthDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_IncEleWaterDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_IncEleWaterDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_IncEleFireDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_IncEleFireDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_VSHumanDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_VSHumanDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_VSZombieDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_VSZombieDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_VSVampireDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_VSVampireDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_VSAnimalDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_VSAnimalDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_VSPlantDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_VSPlantDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_VSEleNoneDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_VSEleNoneDamage_Deduff, typeof(StatsSE) },
            {EffectType.StatsAttack_VSEleMetalDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_VSEleMetalDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_VSEleWoodDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_VsEleWoodDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_VSEleEarthDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_VSEleEarthDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_VSEleWaterDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_VSEleWaterDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_VSEleFireDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_VSEleFireDamage_Debuff, typeof(StatsSE) },
            {EffectType.StatsAttack_VSBossDamage, typeof(StatsSE) },
            {EffectType.StatsAttack_IncFinalDamage, typeof(StatsSE) },

            {EffectType.StatsDefence_Armor, typeof(StatsSE) },
            {EffectType.StatsDefence_Armor_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_Block, typeof(StatsSE) },
            {EffectType.StatsDefence_Block_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_BlockValue, typeof(StatsSE) },
            {EffectType.StatsDefence_BlockValue_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_Evasion, typeof(StatsSE) },
            {EffectType.StatsDefence_Evasion_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_CoCritical, typeof(StatsSE) },
            {EffectType.StatsDefence_CoCritical_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_IncSmashDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_IncSmashDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_IncSliceDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_IncSliceDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_IncPierceDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_IncPierceDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_IncEleNoneDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_IncEleNoneDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_IncEleMetalDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_IncEleMetalDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_IncEleWoodDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_IncEleWoodDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_IncEleEarthDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_IncEleEarthDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_IncEleWaterDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_IncEleWaterDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_IncEleFireDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_IncEleFireDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_VSHumanDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_VSHumanDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_VSZombieDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_VSZombieDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_VSVampireDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_VsVampireDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_VSAnimalDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_VSAnimalDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_VSPlantDefence, typeof(StatsSE) },
            {EffectType.StatsDefence_VSPlantDefence_Debuff, typeof(StatsSE) },
            {EffectType.StatsDefence_DecreaseFinalDamage, typeof(StatsSE) },

            {EffectType.Control_Stun, typeof(StunSE) },
            {EffectType.Control_Root, typeof(RootSE) },
            {EffectType.Control_Fear, typeof(FearSE) }, 
            {EffectType.Control_Silence, typeof(SilenceSE) },
            {EffectType.Control_Taunt, typeof(TauntSE) }, // needs a new SE
            {EffectType.Control_BeakBack, typeof(KnockBackSE) },
            {EffectType.SpecialControl_Freeze, typeof(FrozenSE) },            

            {EffectType.Immune_AllDamage, typeof(SideEffect) }, // temp only
            {EffectType.Immune_AllDebuff, typeof(SideEffect) },
            {EffectType.Immune_AllImmune, typeof(SideEffect) },
            {EffectType.Immune_Stun, typeof(SideEffect) },
            {EffectType.Immune_Root, typeof(SideEffect) },
            {EffectType.Immune_Fear, typeof(SideEffect) },
            {EffectType.Immune_Silence, typeof(SideEffect) },
            {EffectType.Immune_Taunt, typeof(SideEffect) },

            {EffectType.Remove_AllControl, typeof(SideEffect) },
            {EffectType.Remove_Stun, typeof(SideEffect) },
            {EffectType.Remove_Root, typeof(SideEffect) },
            {EffectType.Remove_Fear, typeof(SideEffect) },
            {EffectType.Remove_Silence, typeof(SideEffect) },
            {EffectType.Remove_RandomBuff, typeof(RemoveRandomBuffSE) },
            {EffectType.Remove_RandomDebuff, typeof(SideEffect) },

            {EffectType.Stealth_Stealth, typeof(SideEffect) },
            {EffectType.Stealth_DetectStealth, typeof(SideEffect) },

            {EffectType.Trigger_OnNormalAttack, typeof(SideEffect) },

            {EffectType.Enhance_IncRepeatSE, typeof(SideEffect) },
            {EffectType.Enhance_IncSkillAffect, typeof(SideEffect) }
        };

        public static bool IsSideEffectInstantiatable(SideEffectJson sideeffectData)
        {
            return (EffectTypeToClass.ContainsKey(sideeffectData.effecttype));
        }

        public static SideEffect CreateSideEffect(SideEffectJson sideeffectData, bool isPassive=false)
        {
            try
            {
                if (sideeffectData.persistentafterdeath)// || sideeffectData.persistentonlogout)
                {
                    if (!isPassive)
                        throw new Exception("sideeffect with persistent should be created with an special type of sideeffect ");
                }
                EffectType effecttype = (EffectType)sideeffectData.effecttype;
                Type type;
                if (!EffectTypeToClass.TryGetValue(effecttype, out type))
                {
                    throw new Exception("Sideeffect " + Enum.GetName(typeof(EffectType), effecttype) + " cannot be created because it is not supported yet!");
                }

                object[] args = new object[1];
                args[0] = sideeffectData;
                object sideeffect = Activator.CreateInstance(type, args);

                return (SideEffect)sideeffect;
            }
            catch (Exception ex)
            {

            }

            return null;            
        }
    }

    public interface IPassiveSideEffect
    {
        void AddPassive(Actor target, bool isSkill=false);
        void RemovePassive(bool isSkill = false);
    }

    public abstract class SideEffect
    {
        //public readonly static long MAX_DURATION = 604800000; //7 days, in msecs
        public SideEffectJson mSideeffectData;

        public Actor mTarget;
        protected Actor mCaster;        
        protected long mElapsedTime;
        protected long mTotalElapsedTime;
        protected bool mPositiveEffect;
        protected long mDuration;
        protected bool mNeedCaster; //whether this sideeffect requires caster to work. Otherwise, when caster is gone, this sideeffect will be stopped too.
        public int mSkillID;//use this value in combatformula
        protected int Rank;

        public SideEffect(SideEffectJson sideeffectData)
        {
            mSideeffectData = sideeffectData;
            mElapsedTime = 0;
            mTotalElapsedTime = 0;
            Rank = mSideeffectData.rank;
            mDuration = (long)(mSideeffectData.duration * 1000);
            mNeedCaster = true;
            InitKopioData();
        }

        //values to change 
        protected virtual void InitKopioData()
        {            
        }

        public virtual void OnEnhanceBuffAffact(float perc, float value)
        {
            //implement this so that the sideeffect get affacted.
        }
        public virtual void OnEnhanceBuffDuration(float perc, float val)
        {
            //implement this so that the sideeffect get affacted.
        }

        public long GetTotalTimeElapsed()
        {
            return mTotalElapsedTime;
        }

        public virtual bool IsControl()
        {
            return false;
        }

        public virtual bool IsDot()
        {
            return false;
        }

        public virtual bool IsHot()
        {
            return false;
        }

        public virtual bool IsBuff()
        {
            return false;
        }

        public virtual bool IsDeBuff()
        {
            return false;
        }

        public void Apply(Actor target, Actor caster, bool positiveEffect)
        {
            mTarget = target;
            mCaster = caster;
            mPositiveEffect = positiveEffect;
            if (!mPositiveEffect && caster!=mTarget)
                mTarget.OnAttacked(caster, 1);
            bool appiled = OnApply();
            if (IsDurationalSE() && appiled)
            {
                if (IsHot())
                {
                    ++mTarget.PlayerStats.Havehot;
                }else if (IsDot())
                {
                    ++mTarget.PlayerStats.Havedot;
                }else if (IsControl())
                {
                    ++mTarget.PlayerStats.Havecontrol;
                }
                else if (IsBuff())
                    ++mTarget.PlayerStats.Havebuff;
                else if(IsDeBuff())
                    ++mTarget.PlayerStats.Havedebuff;
            }
        }

        public void Apply(Actor target, Actor caster, bool positiveEffect, int equipid)
        {
            mTarget = target;
            mCaster = caster;
            mPositiveEffect = positiveEffect;
            bool appiled = OnApply(equipid);
        }

        /*public void ResumeFrom(long elapsed, long duration)
        {
            mTotalElapsedTime = elapsed;
            mDuration = duration;
        }        

        public void Prolong(long extension)
        {
            mDuration += extension;
            if (mDuration > MAX_DURATION)
                mDuration = MAX_DURATION;
        }
        */
        public long GetDuration()
        {
            return mDuration;
        }

        public long GetTimeRemaining()
        {
            return Math.Max(mDuration - mTotalElapsedTime, 0);
        }

        public void Stop()
        {      
            if (IsDurationalSE())
            {
                if (IsHot())
                {
                    --mTarget.PlayerStats.Havehot;
                }
                else if (IsDot())
                {
                    --mTarget.PlayerStats.Havedot;
                }
                else if (IsControl())
                {
                    --mTarget.PlayerStats.Havecontrol;
                }
                else if (IsBuff())
                    --mTarget.PlayerStats.Havebuff;
                else if(IsDeBuff())
                    --mTarget.PlayerStats.Havedebuff;
            }
            OnRemove();
            OnFinalized();
        }

        public virtual void OnPlayerStop()
        {
            //todo:overide in child to stop it if the type of side effect can.
        }
        
        public virtual bool IsDurationalSE()
        {
            return mDuration != 0;
        }

        /// <summary>
        /// this function is only for handling the all sideeffect apply condition check determined by the sideeffect group
        ///     call this in overrides. 
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckApplyCondition()
        {
           bool shouldApply = true; 
            if (mTarget.HasSideEffectType(mSideeffectData.effecttype))
            {
                List<SideEffect> listse = mTarget.GetSideEffectOfType(mSideeffectData.effecttype);
                for(int i = listse.Count-1; i >=0; i --)
                {
                    SideEffect se = listse[i];
                    if (SideEffectRepo.InSameGroup(mSideeffectData.id, se.mSideeffectData.id))
                    {
                        if (SideEffectRepo.CanOverride(mSideeffectData.id, se.mSideeffectData.id))
                        {
                            //se.Prolong(se.GetTotalTimeElapsed());
                            se.Stop();//lower priority one is stop.
                            
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
               
        protected virtual bool OnApply(int equipid = -1) //if no more channel, will fail;
        {
            if (!mTarget.IsAlive())
                return false;
            if (IsDurationalSE())
            {
                //Add to target's sideeffect arrays so that this sideeffect gets updated
                if (CheckApplyCondition())
                {
                    if (equipid != -1)
                    {
                        if (mPositiveEffect)
                            return mTarget.AddEquipmentSideEffect(this, equipid);
                        else
                            return mTarget.RemoveEquipmentSideEffect(this, equipid);
                    }
                    else
                    {
                        int slot = mTarget.AddSideEffect(this, mPositiveEffect);
                        //return true;
                        return slot >= 0; //Apply could fail e.g. due to slot full, sideeffectgroup constraint              
                    }
                }
                return false;//can not apply due to sideeffect rules.
            }
            return true; //non durational sideeffect will always succeed
        }

        protected virtual void OnRemove()
        {            
            mTarget.RemoveSideEffect(this, mPositiveEffect); 
        }

        protected virtual void OnFinalized()
        {
            mTarget = null;
            mCaster = null;
        }

        protected virtual void OnInterval()
        {
        }

        public virtual void Update(long dt)
        {
            /*if (mTarget.Destroyed)
            {
                Stop();
                return;
            }

            if (!mTarget.IsAlive())
            {
                if (!mSideeffectData.persistentafterdeath)
                    Stop();
                return;
            }

            if (mNeedCaster)
            {
                if (mCaster.Destroyed || !mCaster.IsAlive())
                {
                    Stop();
                    return;
                }
            }*/

            mElapsedTime += dt;
            mTotalElapsedTime += dt;

            if (mSideeffectData.interval > 0 && mElapsedTime >= (long)(mSideeffectData.interval * 1000))
            {
                mElapsedTime = 0;
                OnInterval();
            }

            //negative duration will not stop
            if (mDuration > 0 && mTotalElapsedTime >= mDuration)
            {
                Stop();
            }
        }
    }
}
