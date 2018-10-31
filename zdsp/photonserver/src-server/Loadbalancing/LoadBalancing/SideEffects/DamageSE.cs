namespace Zealot.Server.SideEffects {

    using Kopio.JsonContracts;
    using System.Collections.Generic;
    using UnityEngine;
    using Zealot.Common;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;
    using Photon.LoadBalancing.GameServer.CombatFormula;

    public class DamageSE : SideEffect {
        protected AttackResult mAttackResult;
        protected float mSkillDmgPercent;
        protected int mExtraDamage;
        protected bool mbBasicAttack;//use this value in combatformula

        public int mSkillid;
        //public SideEffectJson OnTalentHit;
        protected FieldName elementalName;

        public List<SideEffectJson> subskills;
        protected bool mbIsDot = false;
        public int labelCount = 1;

        public DamageSE(SideEffectJson sideeffectData) : base(sideeffectData) {
            if (mSideeffectData.isrelative) {
                float effectrandom = (float)GameUtils.Random(mSideeffectData.min, mSideeffectData.max);
                mSkillDmgPercent = mSideeffectData.basicskilldamageperc + effectrandom;
            }
            else {
                mSkillDmgPercent = mSideeffectData.basicskilldamageperc;//this is the basic perc;
                mExtraDamage = (int)(GameUtils.Random(mSideeffectData.min, mSideeffectData.max));
            }

            
        }

        protected override void InitKopioData() {
            base.InitKopioData();
            //if (mSideeffectData.effecttype == EffectType.Damage_ExtraWhenInDot) {
            //    //mSideeffectData.duration = 0;//this type of sideeffect can not have duration.
            //    mDuration = 0;
            //}
            if (mDuration > 0 && mSideeffectData.interval > 0) {
                mbIsDot = true;
            }
        }

        //this is the bonus add to the skill for the compound skill.
        private int playerBasicattackFeedback = -2;//client randomrized feedback index.0 means knockback

        public void SetSkillDmgCount(float skilldamage, float skilldamgeperc, bool isbasicattack, int feedback = -2) {
            mbBasicAttack = isbasicattack;
            mExtraDamage += (int)skilldamage;
            mSkillDmgPercent += skilldamgeperc;
            playerBasicattackFeedback = feedback;
        }

        public override bool IsDot() {
            return mSideeffectData.interval > 0;
        }

        protected override bool OnApply(int equipid = -1) {
            if (base.OnApply(equipid)) {

                var watch = System.Diagnostics.Stopwatch.StartNew();

                string target = string.Empty;
                if (mCaster.IsPlayer())
                    target = "Player";
                else if (mCaster.IsMonster())
                    target = "Monster";
                else if (mCaster.IsHero())
                    target = "Hero";
                else
                    target = "Player";

                ApplyElemental();
                ComputeDamage();
                ApplyDamage();
                //if (OnTalentHit != null&&mCaster !=null)
                //{
                //    //reghealth when talent hit.
                //    if(GameUtils.GetRandomGenerator().NextDouble() <= OnTalentHit.procchance*0.01f)
                //    {
                //        int val = (int)(mCaster.GetHealthMax() * OnTalentHit.max * 0.01f);
                //        mCaster.OnRecoverHealth(val);
                //    }
                //}
                if (mDuration == 0 && mCaster != null) {
                    mCaster.onHit(mAttackResult.RealDamage);
                }
                if (mCaster != null && mCaster.IsPlayer())//caster maybe killed at the time
                {
                    Player theplayer = mCaster as Player;
                    theplayer.HandleHitCombo();
                }
                
                ResetElemental();

                watch.Stop();

                Photon.LoadBalancing.GameServer.CombatFormula.Debug.Log(target, "AAR : ");
                Photon.LoadBalancing.GameServer.CombatFormula.Debug.Log(target, "Attacker " + mCaster.Name + " HP : " + mCaster.GetHealth());
                Photon.LoadBalancing.GameServer.CombatFormula.Debug.Log(target, "Defender " + mTarget.Name + " HP : " + mTarget.GetHealth());
                Photon.LoadBalancing.GameServer.CombatFormula.Debug.Log("Profile", "System Time : " + watch.ElapsedMilliseconds + "ms");
                Photon.LoadBalancing.GameServer.CombatFormula.Debug.Log(target, "--------------------------------------------");

                return true;
            }
            return false;
        }

        protected override void OnInterval() {
            base.OnInterval();

            ApplyDamage();
        }

        protected override void OnRemove() {
            //if (mSideeffectData.effecttype == EffectType.Damage_DamageWithRejuvenateOnEnd && totalDamageApplied > 0) {
            //    int val = (int)(totalDamageApplied);
            //    ApplyRejuvenate(val);
            //}
            base.OnRemove();
        }

        protected void ApplyElemental() {
            if (mSideeffectData.effecttype == EffectType.Damage_NoElementDamage) {
                elementalName = FieldName.NullDamageBonus;
            }
            else if (mSideeffectData.effecttype == EffectType.Damage_MetalDamage) {
                elementalName = FieldName.MetalDamageBonus;
            }
            else if(mSideeffectData.effecttype == EffectType.Damage_WoodDamage) {
                elementalName = FieldName.WoodDamageBonus; 
            }
            else if (mSideeffectData.effecttype == EffectType.Damage_FireDamage) {
                elementalName = FieldName.FireDamageBonus;
            }
            else if(mSideeffectData.effecttype == EffectType.Damage_EarthDamage) {
                elementalName = FieldName.EarthDamageBonus;
            }
            else if(mSideeffectData.effecttype == EffectType.Damage_WaterDamage) {
                elementalName = FieldName.WaterDamageBonus;
            }

            mCaster.CombatStats.AddToField(elementalName, mSideeffectData.basicskilldamageperc);
            mCaster.CombatStats.ComputeAll();
        }

        protected void ResetElemental() {
            mCaster.CombatStats.AddToField(elementalName, -mSideeffectData.basicskilldamageperc);
            elementalName = FieldName.NullDamageBonus;
            mCaster.CombatStats.ComputeAll();
        }

        protected void ComputeDamage() {
            bool isDot = mbIsDot;

            CombatFormula.FIELDNAMEPACKET package = new CombatFormula.FIELDNAMEPACKET();
            package.attacker = mCaster;
            package.defender = mTarget;
            package.sedata = mSideeffectData;
            package.basicsInfo.attacker = AttackStyle.Slice; // this is hack only since not implemented...
            if (mTarget.IsMonster()) {
                package.basicsInfo.defender = ((Monster)mTarget).mArchetype.weakness;
                package.basicsInfo.monsterType = ((Monster)mTarget).mArchetype.monstertype;
                package.basicsInfo.elementInfo.defender = ((Monster)mTarget).mArchetype.element;
                package.basicsInfo.race.defender = ((Monster)mTarget).mArchetype.race;
                package.basicsInfo.isDefenderNPC = true;
                
            }
            else {
                package.basicsInfo.defender = AttackStyle.Slice; // this is hack only since not implemented...
                package.basicsInfo.monsterType = MonsterType.Normal;
                package.basicsInfo.elementInfo.attacker = (Element)mCaster.CombatStats.GetField(FieldName.Element);
                package.basicsInfo.race.attacker = Race.Human;
                package.basicsInfo.isDefenderNPC = false;
            }
            package.basicsInfo.weaponAttribute = MainWeaponAttribute.Str; // this is hack only 

            if (mSideeffectData.effecttype == EffectType.Damage_NoElementDamage) {
                package.basicsInfo.elementInfo.attacker = Element.None;
            }
            else if (mSideeffectData.effecttype == EffectType.Damage_MetalDamage) {
                package.basicsInfo.elementInfo.attacker = Element.Metal;
            }
            else if (mSideeffectData.effecttype == EffectType.Damage_WoodDamage) {
                package.basicsInfo.elementInfo.attacker = Element.Wood;
            }
            else if (mSideeffectData.effecttype == EffectType.Damage_EarthDamage) {
                package.basicsInfo.elementInfo.attacker = Element.Earth;
            }
            else if (mSideeffectData.effecttype == EffectType.Damage_WaterDamage) {
                package.basicsInfo.elementInfo.attacker = Element.Water;
            }
            else if (mSideeffectData.effecttype == EffectType.Damage_FireDamage) {
                package.basicsInfo.elementInfo.attacker = Element.Fire;
            }


            string target = string.Empty;
            if (mCaster.IsPlayer())
                package.target = "Player";
            else if (mCaster.IsMonster())
                package.target = "Monster";
            else if (mCaster.IsHero())
                package.target = "Hero";
            else
                package.target = "Player";

            CombatFormula.GeneratePackage(package);
            mAttackResult = CombatFormula.ComputeDamage(package, mSkillDmgPercent, mExtraDamage, isDot, mbBasicAttack);
            mTarget.PlayerStats.ElementalVisualSE = mSideeffectData.id;
            
            
            mAttackResult.TargetPID = mTarget.GetPersistentID();
            mAttackResult.IsDot = isDot;
            if (mAttackResult.IsCritical) {
                if (subskills != null) {
                    foreach (SideEffectJson sej2 in subskills) {
                        long dur = (long)(sej2.duration * 1000);
                        int val = (int)sej2.max;
                        if (val <= 0)
                            continue;
                        //if (sej2.effecttype == EffectType.OnCritical_Buff && dur > 0) {
                        //    ApplyCritcalSE(sej2.stat1, val, dur, true);
                        //    ApplyCritcalSE(sej2.stat2, val, dur, true);
                        //}
                        //if (sej2.effecttype == EffectType.OnCritical_DeBuff && dur > 0) {
                        //    ApplyCritcalSE(sej2.stat1, val, dur, false);//record absolute value only
                        //    ApplyCritcalSE(sej2.stat2, val, dur, false);//record absolute value only
                        //}
                        //if (sej2.effecttype == EffectType.OnCritical_Rej) {
                        //    double chance = sej2.procchance * 0.01f;
                        //    if (GameUtils.GetRandomGenerator().NextDouble() < chance) {
                        //        Actor actor = mCaster as Actor;
                        //        int val2 = (int)(actor.GetHealthMax() * sej2.max * 0.01f);
                        //        actor.OnRecoverHealth(val2);
                        //    }
                        //}
                    }
                }
            }
            mAttackResult.LabelNum = labelCount;
            mAttackResult.Skillid = mSkillid;
        }

        private void ApplyCritcalSE(ActorStatsType statstype, int val, long dur, bool buff) {
            if (statstype == ActorStatsType.None)
                return;
            SkillPassiveFieldName targetfield = CombatUtils.GetSkillOnCriticalPassiveFieldByStatsType(statstype, buff);
            if (buff)//buff is on the caster.
            {
                bool changed = mCaster.SkillPassiveStats.ChangeField(targetfield, val, dur, () => {
                    mCaster.PlayerStats.Havebuff--;
                    mCaster.UpdateLocalSkillPassiveStats();
                    //System.Diagnostics.Debug.WriteLine("player havebuff " + mCaster.PlayerStats.havebuff);
                });
                if (changed) {
                    mCaster.UpdateLocalSkillPassiveStats();
                    mCaster.PlayerStats.Havebuff++;
                }
            }
            else //oncritical debuf on target;
            {
                bool changed = mTarget.SkillPassiveStats.ChangeField(targetfield, val, dur, () => {
                    mTarget.PlayerStats.Havebuff--;
                    mTarget.UpdateLocalSkillPassiveStats();
                });
                if (changed) {
                    mTarget.PlayerStats.Havebuff++;
                    mTarget.UpdateLocalSkillPassiveStats();
                }
            }
        }

        private void ApplyRejuvenate(int totaldamage) {
            if (mCaster == null)
                return;
            System.Random ran = GameUtils.GetRandomGenerator();

            float perc = float.Parse(mSideeffectData.parameter) * 0.01f;
            int amount = (int)(totaldamage * perc);
            mCaster.OnRecoverHealth(amount);
        }

        private int totalDamageApplied = 0;

        private void ApplyDamage() {
            //test
            //mAttackResult.RealDamage = 1;//TESTING
            if (mTarget.PlayerStats.InvincibleDmg)
                return;
            else if (mbIsDot && mTarget.PlayerStats.InvincibleDot)
                return;

            mTarget.OnDamage(mCaster.GetOwner(), mAttackResult, mbBasicAttack);
            totalDamageApplied += mAttackResult.RealDamage;
            Photon.LoadBalancing.GameServer.CombatFormula.Debug.Log("DamageSE", "Damage of " + mAttackResult.RealDamage + " is applied");
            //if(mSideeffectData.effecttype == EffectType.Damage_DamageWithRejuvenate)
            //{
            //    ApplyRejuvenate(mAttackResult.RealDamage);
            //}
            if (playerBasicattackFeedback == 0 && mbBasicAttack && mCaster.IsPlayer() && !mAttackResult.IsDot) {
                if (mTarget.IsMonster()) {
                    Vector3 dir = (mTarget.Position - mCaster.Position);
                    dir.y = 0;
                    dir.Normalize();
                    Vector3 pos = mTarget.Position + dir * 1f;//targetpos is the same y

                    ((Monster)mTarget).OnKnockedBack(pos);
                }
            }
        }


        private void ApplyGetHit()
        {
            if (mTarget.IsMonster())
            {
                
            }
        }
    }
}