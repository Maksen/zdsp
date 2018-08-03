namespace Zealot.Server.Actions
{
    using UnityEngine;
    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;
    using Zealot.Repository;
    using Kopio.JsonContracts;
    using System.Collections.Generic;
    using Zealot.Server.SideEffects;
    using Zealot.Server.Counters;
    using Photon.LoadBalancing.Entities;

    public class BaseServerCastSkill : Zealot.Common.Actions.Action
    {
        protected SkillData mSkillData;
        protected SkillSideEffect mSideEffects;
        protected List<SideEffectJson> mFeedbackSideEffects; 

        protected int[] mProced;
        protected long procTime = 0;
        protected long mProcTimeLeft;
        protected List<IActor> mLastQueryResult = new List<IActor>(); 
        protected List<IActor> mLastQueryEvasionResult = new List<IActor>();
        protected long mLastQueriedTime = -1;
        protected int mTargetPID;
        protected Actor mTarget;
        protected Vector3? mTargetPos;
        protected bool mFriendlySkill;
        //protected bool mIsMonsterBasicAttack;
        protected Vector3 targetPos; 
        protected float mSkillAffact=0;//affact skill damage 
        protected float mSkillAffactPerc=0;
        protected float mRejuvenateBonus=0;//affact Rejuvenat
        protected float mRejuvenateBonusPerc = 0;
        protected float mBuffDurationBonus = 0;//buff duration
        protected float mBuffDurationBonusPerc = 0;
        protected float mBuffBonus = 0;//buff affact
        protected float mBUffBonusPerc = 0;
        protected int mCasterPID = 0;
        //protected SideEffectJson OnTalentHit;
        protected bool bDashed = false;
        protected bool isPlayerBasicAttack = false;
        protected int playerBasicAttackIndex = 2;
        private int feedbackIndex = -2;
        private int mskillLevel = 1;
        public BaseServerCastSkill(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {           
        }

        protected virtual long GetSkillDuration()
        {
            return (long)(1000 * mSkillData.skillJson.skillduration);
        }
                
        public override void Start()
        {
            GameCounters.AttackCountsPerSec.Increment();
            ISkillCastCommandCommon cmd = (ISkillCastCommandCommon)mdbCommand;
            bDashed = cmd.IsDashed(); 
            feedbackIndex = cmd.GetFeedbackIndex();
            mTargetPID = cmd.GetTargetID(); 
            if (mEntity.IsActor())
                mCasterPID = ((Actor)(mEntity)).GetPersistentID();
            mTarget = mEntity.EntitySystem.GetEntityByPID(mTargetPID) as Actor;
            

            int skillid = cmd.GetSkillID();
            mSkillData = SkillRepo.GetSkill(skillid);
            isPlayerBasicAttack = mEntity.IsPlayer() && (mSkillData.skillgroupJson.skilltype == SkillType.BasicAttack);
            mskillLevel = cmd.GetSkillLevel();
            base.Start();
        }

        private void UpdateSkillValue(SideEffectJson se, bool positive, ref float percBonus, ref float bonus, bool affactAll=false)
        {
            if ( mFriendlySkill == positive || affactAll)//buff enhance effect will not take effect if added to wrong type of skill. 
            {
                float amount = (float)se.max;
                if (se.isrelative)
                {
                    percBonus += amount;
                }
                else
                {
                    bonus += amount;
                }
            }
        }
        //public void InitSubSkillEffect()
        //{
        //    if (mSkillData.subskills == null)
        //        return;
        //    ISkillCastCommandCommon cmd = (ISkillCastCommandCommon)mdbCommand;
             
        //    if (  mSkillData.subskills!=null)
        //    {
        //        //foreach (SideEffectJson se in mSkillData.subskills)
        //        //{
        //        //    switch (se.effecttype)
        //        //    {
        //        //        //case EffectType.Enhance_SkillDamage:
        //        //        //    UpdateSkillValue(se, false, ref mSkillAffactPerc, ref mSkillAffact);
        //        //        //    break;
        //        //        //case EffectType.Enhance_SkillDamage_RGB:
        //        //        //    UpdateSkillValue(se, false, ref mSkillAffactPerc, ref mSkillAffact);
        //        //        //    break;

        //        //        //case EffectType.Enhance_SkillDamage_Arc:
        //        //        //    //1 => Arc360, 0 => arc120
        //        //        //    if ((mSkillData.skillgroupJson.threatzone == Threatzone.DegreeArc120
        //        //        //        && se.parameter == 0) ||
        //        //        //        (mSkillData.skillgroupJson.threatzone == Threatzone.DegreeArc360
        //        //        //        && se.parameter == 1))
        //        //        //        UpdateSkillValue(se, false, ref mSkillAffactPerc, ref mSkillAffact);
        //        //        //    break;
        //        //        //case EffectType.Enhance_SkillDamage_Beam:
        //        //        //    if (mSkillData.skillgroupJson.threatzone == Threatzone.LongStream) {
        //        //        //        UpdateSkillValue(se, false, ref mSkillAffactPerc, ref mSkillAffact);
        //        //        //    }
        //        //        //    break;
        //        //        //case EffectType.Enhance_Rejuvenate:
        //        //        //    UpdateSkillValue(se, true, ref mRejuvenateBonusPerc, ref mRejuvenateBonus);
        //        //        //    break;
        //        //        //case EffectType.Enhance_BuffAffact:
        //        //        //    //affact friendly skill
        //        //        //    UpdateSkillValue(se, true, ref mBUffBonusPerc, ref mBuffBonus);
        //        //        //    break;
        //        //        //case EffectType.Enhance_DeBuffAffact:
        //        //        //    //affact  enmey skill
        //        //        //    UpdateSkillValue(se, false, ref mBUffBonusPerc, ref mBuffBonus);
        //        //        //    break;
        //        //        //case EffectType.Enhance_BuffDuration:
        //        //        //    //affact friendly skill
        //        //        //    UpdateSkillValue(se, true, ref mBuffDurationBonusPerc, ref mBuffDurationBonus);
        //        //        //    break;
        //        //        //case EffectType.Enhance_DeBuffDuration:
        //        //        //    //affact enemy skill
        //        //        //    UpdateSkillValue(se, false, ref mBuffDurationBonusPerc, ref mBuffDurationBonus);
        //        //        //    break;
        //        //        //case EffectType.Enhance_AttackSkillForJob:
        //        //        //case EffectType.Enhance_DefenceSkillForJob:
        //        //        //    if (mEntity.IsPlayer() || mEntity.IsAIPlayer()) {
        //        //        //        int job = ((ComboSkillCaster)mEntity).GetPlayerStats().jobsect;
        //        //        //        if (job == (int)se.parameter || (int)se.parameter == 0) {
        //        //        //            //for (int i = 0; i < mSkillData.mainskills.Count; i++)
        //        //        //            //{
        //        //        //            //    SideEffectJson temp = mSkillData.mainskills[i];
        //        //        //            //    CombatUtils.EnhanceMainStatsSE(temp, se);
        //        //        //            //}
        //        //        //            SideEffectJson sej = CombatUtils.ConvertSideEffectForApply(se.stat1, se.duration, se.max);
        //        //        //            if (SideEffectFactory.IsSideEffectInstantiatable(sej)) {
        //        //        //                SideEffect inst = SideEffectFactory.CreateSideEffect(sej, false);
        //        //        //                Actor caster = mEntity as Actor;
        //        //        //                bool ispos = SideEffectsUtils.IsSideEffectPositive(sej);
        //        //        //                inst.Apply(caster, caster, ispos);
        //        //        //            }
        //        //        //        }

        //        //        //    }

        //        //        //    break;
        //        //        //case EffectType.OnTalentSkill_Rejuvente:
        //        //        //    foreach (SideEffectJson temp in mSkillData.mainskills) {
        //        //        //        if (temp.effecttype == EffectType.Damage_DamageTalentCloth ||
        //        //        //            temp.effecttype == EffectType.Damage_DamageTalentScissor ||
        //        //        //            temp.effecttype == EffectType.Damage_DamageTalentStone ||
        //        //        //            temp.effecttype == EffectType.Damage_RandomTalent)
        //        //        //            OnTalentHit = se;
        //        //        //        break;
        //        //        //    }
        //        //        //    break;

        //        //        default: 
        //        //            break;
        //        //    }
                    
        //        //}//end of for loop; 

        //    }//end of check subskill is null;
        //    //handle other stats. 
        //    ComboSkillCaster player = mEntity as ComboSkillCaster;
        //    int count = player.EquipedAncientCards;
        //    int count2 = player.EquipedRareCards;
        //    if (count > 4) count = 4;
        //    if (count2 > 4) count2 = 4;
        //    int skillindex = 0;
        //    if (skillindex == 0)
        //    {
        //        mSkillAffactPerc += count * (int)player.SkillPassiveStats.GetField(SkillPassiveFieldName.RedSkill_DamagePerAncient);
        //        mSkillAffactPerc += count2 * (int)player.SkillPassiveStats.GetField(SkillPassiveFieldName.RedSkill_DamagePerRare);
        //    }
        //    else if (skillindex == 1)
        //    {
        //        mSkillAffactPerc += count * (int)player.SkillPassiveStats.GetField(SkillPassiveFieldName.GreenSkill_DamagePerAncient);
        //        mSkillAffactPerc += count2 * (int)player.SkillPassiveStats.GetField(SkillPassiveFieldName.GreenSkill_DamagePerRare);                
        //    }
        //    else if (skillindex == 2)
        //    {
        //        mSkillAffactPerc += count * (int)player.SkillPassiveStats.GetField(SkillPassiveFieldName.BlueSkill_DamagePerAncient);
        //        mSkillAffactPerc += count2 * (int)player.SkillPassiveStats.GetField(SkillPassiveFieldName.BlueSkill_DamagePerRare);
        //    }

        //}

        public bool IsFriendlySkill()
        {
            return mFriendlySkill;
        }

        public Actor GetTarget()
        {
            return mTarget;
        }

        public void FaceTarget()
        {
            Vector3 direction = mEntity.Forward;
            if (mTarget != null)
            {
                direction = mTarget.Position - mEntity.Position;

                direction.y = 0f;
                direction.Normalize(); 
            }
            if (mSkillData.skillgroupJson.skillbehavior == SkillBehaviour.Ground)
            {
                CastSkillCommand castcmd = (CastSkillCommand) mdbCommand;
                mTargetPos = castcmd.targetPos;
                direction = castcmd.targetPos - mEntity.Position;
                direction.y = 0;
                direction.Normalize();
            }
              
            mEntity.Forward = direction; 
        }

        private void UpdateAttackSkill()
        {
            int skillid  = mSkillData.skillJson.id;
            if (isPlayerBasicAttack)
            {
                PartsType playerweapon = (PartsType)((Player)mEntity).WeaponTypeUsed - 1;

                string genderStr = (((Player)mEntity).PlayerSynStats.Gender == 0) ? "M" : "F";
                mSkillData = SkillRepo.GetGenderWeaponBasicAttackData(playerweapon, playerBasicAttackIndex, genderStr);

                //hot fix for now
                if (mSkillData == null)
                    mSkillData = SkillRepo.GetWeaponsBasicAttackData(playerweapon, playerBasicAttackIndex);
            }

            mSideEffects = SkillRepo.GetSkillSideEffects(skillid);
            mFriendlySkill = mSkillData.skillgroupJson.targettype != TargetType.Enemy;
            FaceTarget();
        }
         
        private bool isBasicAttack = false;
        //private bool isPlayerbasicAttack = false;
        protected override void OnActiveEnter(string prevstate)
        {
            UpdateAttackSkill();
            Actor entity = (Actor)mEntity;   
            isBasicAttack = mSkillData.skillgroupJson.skilltype == (int)SkillType.BasicAttack;
            //isPlayerbasicAttack = isBasicAttack && mEntity.IsPlayer();

            mProced = new int[mSideEffects.mTarget.Count]; //Number of times each sideeffect has proced 
            for (int i = 0; i < mProced.Length; i++)
                mProced[i] = 0; //the time to handle the side effect, this is different from interval side effect.when skill duration finished, any proc after that is not handled.

            long _skill_duration = GetSkillDuration();
            procTime = (long)(mSkillData.skillgroupJson.proctime * 1000);
            mProcTimeLeft = procTime;
            mLastQueriedTime = -1;
            if (mProcTimeLeft == 0)        
                OnProc();
            SetTimer(_skill_duration - 20, OnActiveTimeUp, null);
        }

        protected virtual void OnActiveTimeUp(object arg)
        {
             GotoState("Completed");
        }

        private bool SelfSkillEffectCasted = false;
        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
            if (mProcTimeLeft <= 0)
                return;
            mLastQueriedTime -= dt; //if mProcTimeLeft <= 0 means no need to proc anymore, so no need to query target.
            mProcTimeLeft -= dt;
            if (mProcTimeLeft > 0)
                return;
            OnProc();
        }

        protected virtual void OnProc()
        {
            if (mSkillData.skillgroupJson.repeatproc)
                mProcTimeLeft = procTime < 200 ? 200 : procTime;

            System.Random random = GameUtils.GetRandomGenerator();
            //Apply sideeffects when proc time is up for each of them
            //Process targeted sideeffect
            foreach (SideEffectJson sej in mSkillData.skillJson.selfsideeffect.Values) {
                if (sej.id != 0 && !SelfSkillEffectCasted) {
                    SelfSkillEffectCasted = true;
                    SideEffectJson selfse = SideEffectRepo.GetSideEffect(sej.id);
                    if (SideEffectFactory.IsSideEffectInstantiatable(selfse)) {
                        SideEffect se = SideEffectFactory.CreateSideEffect(selfse);
                        bool isbuff = SideEffectsUtils.IsSideEffectPositive(selfse);
                        Actor actor = mEntity as Actor;
                        se.Apply(actor, actor, isbuff);
                    }
                }
            }

            foreach (SideEffectJson sideeffectjson in mSideEffects.mSelf) {
                // proc self
                SideEffect se = SideEffectFactory.CreateSideEffect(sideeffectjson);
                se.Apply((Actor)mEntity, (Actor)mEntity, SideEffectsUtils.IsPositiveEffectType(se.mSideeffectData.effecttype));
            }

            for (int i = 0; i < mSideEffects.mTarget.Count; i++)
            {
                SideEffectJson sideeffectjson = mSideEffects.mTarget[i];
                //Create Sideeffect, some sideeffect is to enhance other, can not instantiate.
                SideEffect se;
                if (!SideEffectFactory.IsSideEffectInstantiatable(sideeffectjson))
                {
                    continue;
                }

                int seteffectid = sideeffectjson.id;
                if (random.NextDouble() * 100 <= sideeffectjson.procchance) //0 to 100
                {
                    mProced[i]++;  //Number of times a sideeffect has proced

                    AcquireTargets();
                    //ApplySubSkills();
                    Actor caster = (Actor)mEntity;
                    if (mLastQueryResult.Count > 0)
                    {
                        for (int j = 0; j < mLastQueryResult.Count; j++)
                        {
                            //final check of destroy, alive
                            Actor target = mLastQueryResult[j] as Actor;
                            if (!target.IsAlive())
                                continue;
                            if (!mFriendlySkill && target.InvincibleMode)
                            {
                                //target.OnDamage(caster, new AttackResult(target.GetPersistentID(),  0, true, mCasterPID));
                                continue;
                            }

                            //one se is created for each target, only create se when hit
                            if (mskillLevel > 1)
                            {
                                //sideeffectjson = SkillRepo.GetSideEffectAtLevel(seteffectid, mskillLevel);
                                sideeffectjson = SideEffectRepo.GetSideEffect(seteffectid);
                            }
                            se = SideEffectFactory.CreateSideEffect(sideeffectjson);
                            se.OnEnhanceBuffAffact(mBUffBonusPerc, mBuffBonus);
                            se.OnEnhanceBuffDuration(mBuffDurationBonusPerc, mBuffDurationBonus);
                            DamageSE dmgse = se as DamageSE;

                            if (dmgse != null)
                            {
                                dmgse.labelCount = mSkillData.skillJson.labelcount <= 0 ? 1 : mSkillData.skillJson.labelcount;
                                //dmgse.OnTalentHit = OnTalentHit;
                                //dmgse.subskills = mSkillData.subskills;
                                dmgse.SetSkillDmgCount(mSkillAffact, mSkillAffactPerc, isBasicAttack, feedbackIndex);
                            }

                            if (se as RejuvenateSE != null)
                            {
                                ((RejuvenateSE)se).SetRejuvenateBonus(mRejuvenateBonusPerc, mRejuvenateBonus);
                            }
                            //active skill may contain debuff or buff 
                            bool ispos = SideEffectsUtils.IsSideEffectPositive(sideeffectjson);
                            se.Apply(target, caster, ispos);

                        }
                    }
                    //only one evasion sent for targets in the same procs, not per sideeffect wise. 
                    if (!mEvasionResultSent)
                    {
                        foreach (IActor evasionTarget in mLastQueryEvasionResult)
                        {
                            Actor thetarget = (Actor)evasionTarget;
                            int pid = thetarget.GetPersistentID();
                            AttackResult res = new AttackResult(pid, 0, false, mCasterPID);
                            res.IsEvasion = true;
                            if (thetarget.IsPlayer())
                                evasionTarget.OnDamage(caster.GetOwner(), res, isPlayerBasicAttack);//send label
                            else
                            {
                                evasionTarget.OnDamage(caster.GetOwner(), res, isPlayerBasicAttack);//send label
                                evasionTarget.OnAttacked(caster, 1);//update ai
                            }
                            thetarget.OnEvasion();
                        }
                        mEvasionResultSent = true;//the value reset if do query again
                    }
                }
            }

            
        }

        protected bool mEvasionResultSent = false;
        protected void AcquireTargets()
        {
            if (isBasicAttack || !string.IsNullOrEmpty(((BaseNetEntity)mEntity).mSummoner))
            {
                mLastQueryResult = new List<IActor>() { mTarget };
                return;
            }
            if (mLastQueriedTime > 0)
                return;
            mLastQueriedTime = 500;
            List<IActor> results;
            Player myPlayer = mEntity as Player;
            
            if (myPlayer != null)           
                results = CombatUtils.QueryTargetsForClientAndServer((IActor)mEntity, (IActor)mTarget, mSkillData,mTargetPos, myPlayer.mInstance.GetSpawnedObjectsByPeer(myPlayer.Slot));
            else
                results = CombatUtils.QueryTargetsForClientAndServer((IActor)mEntity, (IActor)mTarget, mSkillData);
            //handle for player basic attack dash to target. as the client dash distance is far enough to reach any target in view
            if ((bDashed && mEntity.IsPlayer() && mTarget != null && !results.Contains((IActor)mTarget)))
            {
                results.Add((IActor)mTarget); //make sure the target is attacked. due to fast move and attack, the query may missing the target passed from client
            }

            mLastQueryResult = new List<IActor>();
            mLastQueryEvasionResult = new List<IActor>();
            mEvasionResultSent = false;
            foreach (IActor actor in results)
            {
                if (actor.IsAlive() && !actor.IsInSafeZone())
                {
                    Actor entity = actor as Actor;
                    if ((HitType)mSkillData.skillgroupJson.hittype == HitType.Definite || mSkillData.skillgroupJson.targettype != TargetType.Enemy)
                        mLastQueryResult.Add(actor);//only enemy targeting skill have evasion
                    else if (entity.IsMonster() &&!((Monster)entity).HasEvasion())
                    {
                        mLastQueryResult.Add(actor);//monster which canbeknockback no evasion
                    }
                    else if(CombatFormula.IsEvade((Actor)mEntity, (Actor)actor))
                    {                        
                        mLastQueryEvasionResult.Add(actor);
                    }else
                        mLastQueryResult.Add(actor); 
                } 
            }
        }

        protected bool mbSubskillApplied = false;
        /// <summary>
        /// all passive sideeffect in subskills processed on serverside handled here. 
        /// for cooldown change of self, do at client side;
        /// </summary>
        //protected void ApplySubSkills()
        //{
        //    //for targets not including self;
        //    if (mSkillData.subskills == null || mbSubskillApplied)
        //        return;
        //    mbSubskillApplied = true;
        //    foreach (SideEffectJson sej in mSkillData.subskills)
        //    {
        //        if (!SideEffectsUtils.IsEffectiveSubSkill(sej))
        //        {
        //            continue;
        //        }               
                
        //        if (SideEffectsUtils.IsSideEffectPositive(sej)) //apply on self.
        //        {
        //            SideEffect se = SideEffectFactory.CreateSideEffect(sej);
        //            if (se != null)
        //            {
        //                Actor target = mEntity as Actor;
        //                se.Apply(target, target, true);
        //            }
        //        }
        //        else if(!mFriendlySkill) //skill is targeting enmey. apply debuff sideeffects.
        //        {
        //            foreach (IActor actor in mLastQueryResult)
        //            {
        //                SideEffect se = SideEffectFactory.CreateSideEffect(sej);
        //                if (se != null)
        //                {
        //                    Actor target = mEntity as Actor;
        //                    se.Apply((Actor)actor, target, false);
        //                }
        //            } 
        //        }
                
        //    }
            
        //}

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
        }
    }

    public class ServerAuthoCastSkill : BaseServerCastSkill
    {
        public ServerAuthoCastSkill(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
        }

        public override void Start()
        {
            base.Start();
            //if (mSkillData.skillgroupJson.movedist > 0)
            //{
            //    if (mEntity.IsMonster())
            //    {
            //        targetPos = mEntity.Position + mEntity.Forward * mSkillData.skillgroupJson.movedist;
            //    }else
            //    {
            //        targetPos = Vector3.zero;
            //    }
            //}
            Actor entity = (Actor)mEntity;
            entity.SetAction(mdbCommand);
        }
        protected override void OnActiveEnter(string prevstate)
        {            
            base.OnActiveEnter(prevstate);
            
            
        }

         

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
            if(targetPos.magnitude > 0)
            {
                float moveSpeed = 10.0f;//mSkillData.skillgroupJson.movedist / mSkillData.skillJson.skillduration;
                mEntity.Position = Vector3.MoveTowards(mEntity.Position, targetPos, moveSpeed * dt / 1000.0f);
            }
        }

        public bool IsBasicAttack()
        {
            return mSkillData.skillgroupJson.skilltype == SkillType.BasicAttack;
        }

        protected override void OnActiveTimeUp(object arg)
        {
            if (mSkillData.skillgroupJson.skilltype == SkillType.BasicAttack)
            {
                Vector3 pos = ((Actor)mEntity).Position;
                if (mTarget.IsAlive() && GameUtils.InRange(mTarget.Position, pos, 0.5f))
                {
                    GotoState("Active");
                    return;
                }
            }
            GotoState("Completed");
        }
    }

    //Note that ActionRPC call from client is reliable. So this action will be called reliably.
    public class NonServerAuthoCastSkill : BaseServerCastSkill
    {
        public NonServerAuthoCastSkill(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {          
        }

        protected override long GetSkillDuration()
        {
            return (long)(1000 * mSkillData.skillJson.skillduration) + GetRecoverTime(mSkillData);
        }

        private long GetRecoverTime(SkillData skilldata)
        {
            float dur = 0;
            Actor actor = mEntity as Actor;
            if (isPlayerBasicAttack)
                dur += skilldata.skillJson.rtfixedduration / actor.PlayerStats.baSpeed;
            else
            {
                //skill can be affacted by rt_duction. 
                dur += skilldata.skillJson.rtfixedduration;
                dur += skilldata.skillJson.rtvarduration * actor.PlayerStats.rtReduction;
            }
            float minrttime = skilldata.skillgroupJson.basicminrt;
            if (dur > minrttime)
                return (long)(1000 * dur);
            return 0;
        }

        private void OnRecoverDone()
        {
            if (isPlayerBasicAttack)
            {
                ++playerBasicAttackIndex;
                if (playerBasicAttackIndex == 4)
                    playerBasicAttackIndex = 1;
                GotoState("Active"); 
            }
            else
            {
                GotoState("Completed");
            }
        }

        protected override void OnActiveTimeUp(object arg)
        {
            OnRecoverDone();
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate); 
            if(mdbCommand is CastSkillCommand)
            {
                CastSkillCommand cmd = mdbCommand as CastSkillCommand;
                if (cmd.dashed)
                {
                    //piliq basic attack dash . 
                }
            }
            Player p = mEntity as Player;
            if (p != null)
            {
                p.CombatStarted();
            }
        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
        }

        protected override void OnCompleteEnter(string prevstate)
        {
            base.OnCompleteEnter(prevstate);
            //has to be done in complete state. 
            ((NetEntity)mEntity).ClearAction(); //so that this action won't be updated anymore in entitysystem when it completes or terminated
        }
    }
}
