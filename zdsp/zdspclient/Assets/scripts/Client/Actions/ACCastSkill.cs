using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Client.Entities;
using Zealot.Common.Actions;
using Zealot.Common;
using Kopio.JsonContracts;
using Zealot.Repository;
using System.Collections.Generic;
using EZCameraShake;

namespace Zealot.Client.Actions
{
    public class BaseClientCastSkill : Action
    {
        protected SkillData mSkillData;
        protected string mSkillName;
        public ActorGhost mTarget = null;
        public Vector3 mTargetPos;
        public ActorGhost mCaster = null;
        protected bool islocal = false;
        public bool PlayerBasicAttack { get; set; }
        protected int mBasicAttackIndex = 1;

        protected string hitEffect = "";
        protected int feedbackindex = -2;
        protected float _attackspeed = 1;

        private int skillid;

        // Designer designated hack
        public static float recovertime_mod = 0;
        public static bool isCrtical = false;
        // Better to remove if unwanted as it needs 1 if check

        public BaseClientCastSkill(Entity entity, ISkillCastCommandCommon cmd) : base(entity, cmd)
        {
            AddState("Recover", OnRecoverEnter, null);
            skillid = cmd.GetSkillID();
            mSkillData = SkillRepo.GetSkill(skillid);
            PlayerBasicAttack = IsPlayerBasicAttack();
            if (cmd.GetTargetID() > 0)
                mTarget = mEntity.EntitySystem.GetEntityByPID(cmd.GetTargetID()) as ActorGhost;
            mCaster = mEntity as ActorGhost;

            mSkillName = mSkillData.skillgroupJson.name;
        }

        protected void ShakeCamera()
        {
            if (CameraShaker.Instance != null)
                CameraShaker.Instance.ShakeOnce(1.5f, 3f, 0, 0.15f, 0);
        }

        #region Recover State
        //Ended in Success
        protected void OnRecoverEnter(string prevstate)
        {
            float rtdur = GetRecoverTime();

            ActorGhost ghost = mEntity as ActorGhost;
            long recovertime = (long)(rtdur * 1000);
            if (PlayerBasicAttack)
            {
                PlayerGhost pghost = mEntity as PlayerGhost;
                //rt animation speed is propertional to baspeed. not nesseccaryly correct.
                ghost.SetAnimSpeed(mSkillData.skillgroupJson.rtaction, _attackspeed);
            }
            else
            {
                ghost.SetAnimSpeed(mSkillData.skillgroupJson.rtaction, 1 / ghost.PlayerStats.rtReduction);
            }
            if (mBasicAttackIndex == 4)
            {
                mBasicAttackIndex = 1;
            }
            //Debug.Log("recover time : " + recovertime);
            long minrttime = (long)(1000 * mSkillData.skillgroupJson.basicminrt); //OnTimer Compensate
            if (recovertime_mod > 0) minrttime = (long)(1000 * recovertime_mod);
            if (recovertime > minrttime)
            {
                //if(!IsPlayerBasicAttack()) //basic attack not interruptable by hit
                //ghost.StartRecovering(recovertime);
                SetTimer(recovertime, OnRecoverDone, null);
                //ghost.PlayAnimation(mSkillData.skillgroupJson.rtaction, -1);
                ghost.PlayAnimation(mSkillData.skillgroupJson.rtaction, -1);
            }
            else
            {
                OnRecoverDone(null);
            }
        }

        /// <summary>
        /// functin to compute the recover time for the skill. 
        /// As only player will enter recover state. here only check if it is 
        /// basicattack or not.  the formula for basic attack and Active skill is different
        /// in compute the recover time.  basic attack is affected by the baspeed only. 
        /// skill is affacted by rtReduction.  
        /// </summary>
        /// <returns></returns>
        protected virtual float GetRecoverTime()
        {
            float rtdur = 0;
            if (PlayerBasicAttack)
            {
                rtdur += mSkillData.skillJson.rtfixedduration / mCaster.PlayerStats.baSpeed;
            }
            else
            {
                rtdur += mSkillData.skillJson.rtfixedduration;
                rtdur += mSkillData.skillJson.rtvarduration * mCaster.PlayerStats.rtReduction;
            }
            return rtdur;
        }

        //protected virtual bool CanCastSkill()

        protected virtual void OnRecoverDone(object args)
        {
            GotoState("Completed");
        }

        protected void OnRecoverUpdate(long dt)
        {

        }
        #endregion

        private void SetHitFeedback()
        {
            hitEffect = mSkillData.skillJson.name + "_gethit";
            MonsterGhost mg = mTarget as MonsterGhost;
            if (PlayerBasicAttack && mg != null)
            {
                if (islocal)
                {
                    //0 will knockback, 
                    ((CastSkillCommand)(mdbCommand)).feedbackindex = feedbackindex = 1;//set the feedbackindex in cmd
                }
                else
                {
                    feedbackindex = ((CastSkillCommand)(mdbCommand)).feedbackindex;//non local get feedbackindex from cmd;
                }
            }
        }

        protected virtual void SetupSkillCastTimer()
        {
            float atkdur = mSkillData.skillJson.skillduration;
            long dur = (long)(1000 * atkdur) - 15; //for on timer Compensate
            SetTimer(dur, OnActiveTimeUp, null);
        }

        protected List<long> mEffectProcs = null;
        protected void OnStartCastSkill()
        {
            mEffectProcs = new List<long>();
            long firstproc = (long)(mSkillData.skillgroupJson.proctime * 1000);
            mEffectProcs.Add(firstproc);
            NextProcTime = firstproc;

            //if (mSkillData.skillgroupJson.repeatproc)
            //{
            //    for (int i = 0; i < 100; i++)
            //    {
            //        long _proctime = firstproc * i;
            //        if (_proctime > mSkillData.skillJson.skillduration)
            //            break;
            //        mEffectProcs.Add(firstproc);
            //    }
            //}
            //determin hitfeedback. if it is approachdash, the feedback not set. so the first approach have no feedback anim
            SetHitFeedback();//this is to determin the feedback index; 
        }

        private long NextProcTime = 0;
        protected List<IActor> queryiedTargets;
        protected float lastqueryTime = 0;

        public float effectdurmod = 0;

        private void QueryForTargets()
        {
            if (Time.realtimeSinceStartup - lastqueryTime < 0.5f)
                return;

            lastqueryTime = Time.realtimeSinceStartup;
            if (mSkillData == null)
                Debug.LogError("Skill is missing from the start?");
            queryiedTargets = CombatUtils.QueryTargetsForClientAndServer((IActor)mEntity, mTarget, mTargetPos, mSkillData);
        }

        private void CheckEffectProcs(long dt)
        {
            if (mEffectProcs.Count <= 0)
                return;
            NextProcTime -= dt;
            if (NextProcTime <= 0)
            {
                mEffectProcs.RemoveAt(0);
                PlayHitEffect(null);
                if (mEffectProcs.Count > 0)
                {
                    NextProcTime = mEffectProcs[0];
                    if (NextProcTime < 200)
                        NextProcTime = 200;
                }
            }
        }

        protected virtual void PlayHitEffect(object args)
        {
            QueryForTargets();//simulate the logic in server
            GetHitCommand ghcmd = new GetHitCommand();
            ghcmd.skillid = skillid;
            foreach (IActor actor in queryiedTargets)
            {
                ActorGhost ghost = (ActorGhost)actor;
                if (actor.IsAlive() && !actor.IsInSafeZone())
                {
                    if (ghost.IsPlayer())
                    {
                        if (!ghost.IsRecovering && !ghost.IsMoving() && !ghost.IsIdling())
                            ghost.PlayEffect("", hitEffect);
                    }
                    else
                    {
                        // client will do the animation first and the server will later ondamage handle the action instead
                        ghost.PlayEffect("", hitEffect);
                        ((MonsterGhost)ghost).Flash(); //flash on monster
                        ghost.StartHitted(200);
                    }
                }
            }
            //player hit or  boss hit player will shake camera;
            if (queryiedTargets.Count > 0 && mEntity.IsPlayer() && islocal)
            {
                ShakeCamera();
            }
            else if (mEntity.IsMonster())
            {
                MonsterGhost mg = mEntity as MonsterGhost;
                if (mg.mArchetype.monstertype == MonsterType.Boss)
                {
                    if (queryiedTargets.Contains(GameInfo.gLocalPlayer))
                        ShakeCamera();
                }
            }
        }


        private void FaceTarget()
        {
            Vector3 direction = mEntity.Forward;
            //if (mTarget != null)
            //{
            //    direction = mTarget.Position - mEntity.Position;
            //}
            //else if (mSkillData.skillgroupJson.skillbehavior == SkillBehaviour.Ground)
            //{
                Vector3 pos = ((CastSkillCommand)mdbCommand).targetPos;
                direction = pos - mEntity.Position;
                mTargetPos = pos;
            //}
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.000001f) //if too close, simply use current forward
                return;
            direction.Normalize();
            mEntity.Forward = direction;//forward updated here  

        }

        private bool IsPlayerBasicAttack()
        {
            return mEntity.IsPlayer() && mSkillData.skillgroupJson.skilltype == SkillType.BasicAttack;
        }

        protected virtual void PlaySkillEffect()
        {
            NetEntityGhost ghost = (NetEntityGhost)mEntity;//the 1 that cast the skill                       
            SkillGroupJson skillgroupJson = mSkillData.skillgroupJson;
            string animationName = mSkillData.skillgroupJson.action;
            string effectName = mSkillData.skillJson.name;
            float effectdur = mSkillData.skillJson.effect_dur;
            ghost.PlayEffect(animationName, effectName, null, effectdur + effectdurmod, mTargetPos, mTarget, false);
            
            if(skillgroupJson.weapon_ot > 0)
            {
                GameTimer timer = null;
                timer = mTimers.SetTimer((long)skillgroupJson.weapon_ot * 1000,
                    delegate
                    {
                        if (ghost.IsPlayer())
                        {
                            ((PlayerGhost)ghost).AnimObj.GetComponent<AvatarController>().TransformWeapon(true);
                            GameTimer closeTimer = null;
                            closeTimer = mTimers.SetTimer((long)skillgroupJson.weapon_ct * 1000,
                                delegate
                                {
                                    ((PlayerGhost)ghost).AnimObj.GetComponent<AvatarController>().TransformWeapon(false);
                                }, closeTimer);
                        }
                    }, timer);
            }
        }

        protected override void OnActiveEnter(string prevstate)
        {
            GameInfo.gLocalPlayer.HideSkillIndicator();
            FaceTarget();
            _attackspeed = mCaster.PlayerStats.baSpeed;
            if (PlayerBasicAttack)
            {
                PlayerGhost pg = mCaster as PlayerGhost;
                int weaponType = (int)(pg.WeaponTypeUsed);
                string genderStr = (pg.PlayerSynStats.Gender == 0) ? "M" : "F";
                mSkillData = SkillRepo.GetGenderWeaponBasicAttackData((PartsType)weaponType, mBasicAttackIndex, genderStr);
                if (mSkillData == null)
                    mSkillData = SkillRepo.GetWeaponsBasicAttackData((PartsType)weaponType, mBasicAttackIndex);

                //mSkillData = SkillRepo.GetSkillGivenWeapon(mSkillData.skillJson.skillgroupid, pg.WeaponTypeUsed);
                //mSkillData = SkillRepo.GetSkillByGroupID(mBasicAttackSeq[mBasicAttackIndex]);
                mBasicAttackIndex++;
            }

            SetupSkillCastTimer();
            OnStartCastSkill();
            PlaySkillEffect();
        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
            CheckEffectProcs(dt);
        }

        /// <summary>
        /// only the player skill have recover time and action. which is handled in the RecoverState
        /// </summary>
        /// <param name="arg"></param>
        protected virtual void OnActiveTimeUp(object arg)
        {
            if (mEntity.IsPlayer())
                GotoState("Recover");
            else
                GotoState("Completed");
        }
    }

    public class ClientAuthoCastSkill : BaseClientCastSkill
    {
        private int mComboRound = 0;

        public ClientAuthoCastSkill(Entity entity, ISkillCastCommandCommon cmd) : base(entity, cmd)
        {
            islocal = true;
        }

        public int Targetpid()
        {
            return mTarget != null ? mTarget.GetPersistentID() : 0;
        }
        public override void Start()
        {
            base.Start();
            ActorGhost ghost = (ActorGhost)mEntity;
            ghost.SetAction(mdbCommand);
        }

        /// <summary>
        /// the state change determined in the authoritive client
        /// </summary>
        /// <param name="args"></param>
        protected override void OnRecoverDone(object args)
        {
            if (PlayerBasicAttack)
            {
                if (mTarget.IsAlive() && GameUtils.InRange(mCaster.Position,
                            mTarget.Position, mSkillData.skillJson.radius))
                {
                    if (mBasicAttackIndex == 1)
                    {
                        mComboRound++;
                        if (mComboRound == 5)
                        {
                            ((ActorGhost)mEntity).SetAction(mdbCommand);
                            mComboRound = 0;
                        }
                    }

                    if (mCaster.IsPlayer() && PartyFollowTarget.IsPaused())  // if is player will always be local
                    {
                        PartyFollowTarget.Resume();
                        GotoState("Completed");
                        return;
                    }
                    Bot.BotCastSkillHandler.Instance.FinishCastSkill();
                    GotoState("Active");
                    return;
                }
                else
                {
                    if (mCaster.IsPlayer() && PartyFollowTarget.IsPaused()) // if is player will always be local
                        PartyFollowTarget.Resume();
                }
            }

            GotoState("Completed");
        }
    }

    public class NonClientAuthoCastSkill : BaseClientCastSkill
    {
        private bool mSkillIndicatorDisplayed;

        public NonClientAuthoCastSkill(Entity entity, ISkillCastCommandCommon cmd) : base(entity, cmd)
        {
            mSkillIndicatorDisplayed = false;
        }

        protected override void OnActiveEnter(string prevstate)
        {
            CastSkillCommand cmd = (CastSkillCommand)mdbCommand;
            if (cmd.targetpid > 0 && mTarget == null)
            {
                Idle();
                GotoState("Completed");
                return;
            }

            base.OnActiveEnter(prevstate);
            //Debug.LogFormat("casting skillgroup: {0}, name:{1}" , mSkillData.skillgroupJson.id,mSkillName);

            if (mEntity.IsMonster())
            {
                MonsterGhost monster = (MonsterGhost)mEntity;
                if (monster.mArchetype.monstertype == MonsterType.Boss)
                {
                    if (mSkillData.skillgroupJson.skilltype == SkillType.Active && !CutsceneManager.instance.IsPlaying()) //boss casts skill, we show the warning indicator
                    {
                        monster.DisplaySkillIndicator(mSkillData, null);
                        mSkillIndicatorDisplayed = true;
                    }
                }
            }
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();

            if (mSkillIndicatorDisplayed)
                mCaster.HideSkillIndicator();
        }

        /// <summary>
        /// the state change determined in the authoritive client
        /// </summary>
        /// <param name="args"></param>
        protected override void OnRecoverDone(object args)
        {
            if (PlayerBasicAttack)
            {
                if (mTarget.IsAlive() && GameUtils.InRange(mCaster.Position, mTarget.Position, mSkillData.skillJson.radius))
                {
                    GotoState("Active");
                    return;
                }
            }

            GotoState("Completed");
        }

        protected void Idle()
        {
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            IdleActionCommand cmd = new IdleActionCommand();
            ghost.PerformAction(new NonClientAuthoACIdle(ghost, cmd));
        }
    }
}
