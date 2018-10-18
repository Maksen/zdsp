using Kopio.JsonContracts;
using UnityEngine;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Common.Actions;

namespace Zealot.Client.Entities
{
    public abstract class ActorGhost : NetEntityGhost, IActor
    {
        #region IActor
        public ActorSynStats PlayerStats { get; set; }

        protected GameObject mSkillIndicator120;
        protected GameObject mSkillIndicator360;
        protected GameObject mSkillIndicatorLong;

        public bool IsInLocalCombat
        {
            get { return _isInLocalCombat; }
            set { _isInLocalCombat = value; }
        }

        public bool IsAlive()
        {
            if (Destroyed || PlayerStats == null)
                return false;
            return PlayerStats.Alive;
        }
        public abstract bool IsInvalidTarget();
        public abstract bool IsInSafeZone();
        public int Team { get { return PlayerStats.Team; } set { PlayerStats.Team = value; } }

        public abstract ICombatStats CombatStats { get; set; }
        public abstract SkillPassiveCombatStats SkillPassiveStats { get; set; }
        public abstract bool MaxEvasionChance { get; set; }
        public abstract bool MaxCriticalChance { get; set; }

        public virtual int GetParty()
        {
            return -1;
        }

        public void OnDamage(IActor attacker, AttackResult res, bool pbasicattack) { } //Only for server
        public void OnRecoverHealth(int origamount) { } //Only for server
        public virtual void QueueDmgResult(AttackResult res) { } //Only for server
        public virtual void QueueSideEffectHit(int targetPID, int sideeffectID) { } //Only for server
        public virtual void OnAttacked(IActor attacker, int aggro) { } //Only for server
        public virtual void OnKilled(IActor attacker) { } //Only for server        
        public virtual void UpdateLocalSkillPassiveStats() { } //Only for server  
        public virtual void OnComputeCombatStats() { } //Only for server  

        #endregion

        private string mPrevPosVisualSEName, mPrevNegVisualSEName;
        private bool _isHitByLocalPlayer;
        private bool _isInLocalCombat;
        protected Dictionary<string, bool> ControlSE_Status;

        public ActorGhost() : base()
        {
            mPrevPosVisualSEName = null;
            mPrevNegVisualSEName = null;
            _isHitByLocalPlayer = false;
            _isInLocalCombat = false;
            Radius = CombatUtils.DEFAULT_ACTOR_RADIUS;//todo: make sure this is same as server.
            ControlSE_Status = new Dictionary<string, bool>();
            for (int i = 0; i < (int)EffectVisualTypes.NUM; ++i)
            {
                ControlSE_Status.Add(((EffectVisualTypes)i).ToString(), false);
            }
        }

        protected virtual void PlayStunEffect(bool bplay)
        {
            PlayEffect("", "stun");
        }

        private long gethittime = 0;
        private long recoveringtime = 0;
        public override void Update(long dt)
        {
            base.Update(dt);
            recoveringtime -= dt;
            if (gethittime > 0)
            {
                if (gethittime - dt <= 0)
                {
                    HitFinished();
                }
            }
            gethittime -= dt;
        }

        public bool IsRecovering { get { return recoveringtime > 0; } }
        public override bool IsHitted()
        {
            return gethittime > 0;
        }

        /// <summary>
        /// only for recording the Player CastSkill recovering time;
        /// in the recovering time, it can play gethit feedback
        /// </summary>
        /// <param name="period"></param>
        public void StartRecovering(long period)
        {
            recoveringtime = period;
        }

        /// <summary>
        /// a time to record last time player player play gethit animation.
        /// consective gethit will not play the animation twice. 
        /// </summary>
        /// <param name="cooldown"></param>
        public void StartHitted(long cooldown)
        {
            //play hit animation condition check.   
            //if (IsRecovering || IsMoving()|| IsIdling()) // somehow this line is blocking
            {
                if (PlayerStats.HeavyStand) return;
                if (gethittime < 0)
                {
                    HitStarted();
                    gethittime = cooldown;
                }
            }
        }

        /// <summary>
        ///Overload for enemy get hit
        /// </summary>
        /// <param name="cooldown"></param>
        public void StartHitted(long cooldown, int index)
        {
            //play hit animation condition check.   
            if (IsRecovering || IsMoving() || IsIdling())
            {
                if (gethittime < 0)
                {
                    HitStarted(index);
                    gethittime = cooldown;
                }
            }
        }

        protected virtual void HitStarted(int index)
        {
            string anim = GetHitAnimation();// + index;
            PlayAnimation(anim, -1);
        }

        protected virtual void HitStarted()
        {

            string animstr = GetHitAnimation();
            //check for element
            PlayAnimation(animstr, -1);
        }

        protected virtual void HitFinished()
        {
            if (IsDying())
                return;
            if (IsIdling())
                PlayAnimation(GetStandbyAnimation(), -1);
            else if (IsMoving())
                PlayAnimation(GetRunningAnimation(), -1);
        }

        string applied_mat = "";
        void SetActorMaterial(EffectVisualTypes t)
        {
            Material loadedmat = null;
            var container = (MaterialContainer)AssetManager.GetAssetContainer("Effects_ZDSP_StatusMaterials");
            foreach (var mat in container.MaterialList)
            {
                if (mat.name.Contains(t.ToString()))
                {
                    loadedmat = mat;
                    break;
                }
            }

            if (loadedmat != null)
            {
                applied_mat = loadedmat.name;                

                var interp = mCharController.gameObject.AddComponent<ShaderPropertyInterpolate>();
                interp.AddMaterial(loadedmat);
                interp.Activate("_Height", 0.25f, 0.3f); 
            }
        }

        void RemoveActorMaterial(EffectVisualTypes t)
        {
            var removedname = applied_mat;
            
            var interp = mCharController.gameObject.AddComponent<ShaderPropertyInterpolate>();
            interp.Activate("_Height", 0.25f, 0.3f, true, removedname);
        }

        private void UpdateControlStatusEffect(EffectVisualTypes t, int info)
        {
            if ((((int)t) & info) > 0 && !ControlSE_Status[t.ToString()])
            {
                //PlaySEEffect(Vector3.zero, t.ToString(), -1f);

                SetActorMaterial(t);

                switch (t)
                {
                    case EffectVisualTypes.Stun:
                        {
                            //Debug.Log("stun start..");
                            PlayStunEffect(true);
                        }
                        break;

                    case EffectVisualTypes.Silence:
                        {
                            //Debug.Log("slience start..");
                            //if (UIManager.UIHudFX.HudFilter1 != null)
                            //    StartAnimFilter(UIManager.UIHudFX.HudFilter1, UIManager.UIHudFX.HudFilterAnim1);
                        }
                        break;

                    case EffectVisualTypes.Disarmed:
                        {
                            //if (UIManager.UIHudFX.HudFilter4 != null)
                            //    StartAnimFilter(UIManager.UIHudFX.HudFilter4, UIManager.UIHudFX.HudFilterAnim4);
                        }
                        break;

                    case EffectVisualTypes.Root:

                        break;

                    case EffectVisualTypes.Frozen:
                        {
                            //SetActorMaterial(t);
                            var animators = mCharController.gameObject.GetComponentsInChildren<Animator>();

                            foreach (var animator in animators)
                                animator.StartPlayback();
                        }
                        break;
                }

                ControlSE_Status[t.ToString()] = true;
            }
            else if ((((int)t) & info) == 0 && ControlSE_Status[t.ToString()])
            {
                RemoveActorMaterial(t);

                switch (t)
                {
                    case EffectVisualTypes.Stun:
                        {
                            //Debug.Log("stun stop...");
                            StopEffect("stun");
                        }
                        break;

                    case EffectVisualTypes.Silence:
                        {
                            Debug.Log("slience stop...");
                            //if (UIManager.UIHudFX.HudFilterAnim1 != null)
                            //    StopAnimFilter(UIManager.UIHudFX.HudFilter1, UIManager.UIHudFX.HudFilterAnim1);
                        }
                        break;

                    case EffectVisualTypes.Disarmed:
                        {
                            //if (UIManager.UIHudFX.HudFilterAnim4 != null)
                            //    StopAnimFilter(UIManager.UIHudFX.HudFilter4, UIManager.UIHudFX.HudFilterAnim4);
                        }
                        break;

                    case EffectVisualTypes.Root:

                        break;

                    case EffectVisualTypes.Frozen:
                        {
                            //SetActorMaterial(t);
                            var animators = mCharController.gameObject.GetComponentsInChildren<Animator>();

                            foreach (var animator in animators)
                                animator.StopPlayback();
                        }
                        break;
                }

                ControlSE_Status[t.ToString()] = false;
            }
        }

        private Animator LastAnimFilter = null;
        private GameObject LastFilter = null;
        public void StartAnimFilter(GameObject newFilter, Animator currAnimFilter)
        {
            //only one filter will be active at a time.  newly comming state will stop old state,
            //a small issue with this approach is that the new coming state maybe short and stoped fast,
            //while the old state is long that it still active, then its filter is not shown as it's stoped by 
            //the short state already.           
            if (LastFilter != null)
            {
                LastAnimFilter.Play("ActionLines_FadeOut");//stop the old one without anim 
                LastFilter.SetActive(false);
                LastFilter = null;
                LastAnimFilter = null;
            }

            newFilter.SetActive(true);
            LastFilter = newFilter;
            LastAnimFilter = currAnimFilter;
        }

        public void StopAnimFilter(GameObject newFilter, Animator currAnimFilter)
        {
            if (LastAnimFilter != null && LastAnimFilter == currAnimFilter) //no need to play animation if not the last one.as it is already stopped
            {
                LastAnimFilter.Play("ActionLines_FadeOut");
                LastAnimFilter = null;
                LastFilter = null;
            }
        }

        public void HandleBuffStatus(string field, object value)
        {
            if (field == "havebuff" || field == "havedebuff" || field == "havedot" || field == "havecontrol" || field == "havehot")
            {
#if UNITY_EDITOR
                //string str = Name +  "  buff/debuff/hot/dot/ctrl :" + PlayerStats.havebuff + "/"
                //    + PlayerStats.havedebuff + "/" + PlayerStats.havehot + "/"
                //    + PlayerStats.havedot + "/" + PlayerStats.havecontrol;
                //Debug.Log("<color=green>" + str + "</color>");
#endif
                //TODO:update UI
                //int count = (int)value;

                //if (field == "havebuff" && HeadLabel != null)
                //{
                //    HeadLabel.mPlayerLabel.AddDeleteBuffDebuff(BuffEnum.ArrowUp, count);
                //}
                //if (field == "havedebuff" && HeadLabel != null)
                //{
                //    HeadLabel.mPlayerLabel.AddDeleteBuffDebuff(BuffEnum.ArrowDown, count);
                //}
                //if (field == "havedot" && HeadLabel != null)
                //{
                //    HeadLabel.mPlayerLabel.AddDeleteBuffDebuff(BuffEnum.GreenWaterdrop, count);
                //}
                //if (field == "havecontrol" && HeadLabel != null)
                //{
                //    HeadLabel.mPlayerLabel.AddDeleteBuffDebuff(BuffEnum.Chain, count);
                //}
                //if (field == "havehot" && HeadLabel != null)
                //{
                //    HeadLabel.mPlayerLabel.AddDeleteBuffDebuff(BuffEnum.HeartBroken, count);
                //}
            }
        }

        public bool HandleSideEffectVisuals(string field, object value)//can only play one pos and one nag at a time.
        {
            if (field == "VisualEffectTypes")
            {
                //Debug.Log(System.Convert.ToString((int)value, 2));
                for (EffectVisualTypes se = 0; se < EffectVisualTypes.NUM; ++se)
                {
                    UpdateControlStatusEffect(se, (int)value);
                }
            }
            else if (field == "ElementalVisualSE")
            {
                if ((int)value > 0)
                {
                    SideEffectJson se = SideEffectRepo.GetSideEffect((int)value);
                    //Debug.Log("To Play elemental Effect: " + se.name);
                    if (value != null)
                    {
                        mPrevPosVisualSEName = se.name;
                        PlaySEEffect(se.name);
                    }
                }
            }
            else if (field == "positiveVisualSE")
            {
                if (mPrevPosVisualSEName != null)
                {
                    StopEffect(mPrevPosVisualSEName);
                    mPrevPosVisualSEName = null;
                }

                int seid = (int)value;
                if (seid > 0)
                {
                    SideEffectJson sideeffectJson = SideEffectRepo.GetSideEffect(seid);
                    //Debug.Log("To Play +ve Effect: " + sideeffectJson.effectpath);
                    if (sideeffectJson != null)
                    {
                        mPrevPosVisualSEName = sideeffectJson.name;
                        PlaySEEffect(mPrevPosVisualSEName);
                    }

                }
                return true;
            }
            else if (field == "negativeVisualSE")
            {
                if (mPrevNegVisualSEName != null)
                {
                    StopEffect(mPrevNegVisualSEName);
                    mPrevNegVisualSEName = null;
                }

                int seid = (int)value;
                if (seid > 0)
                {
                    SideEffectJson sideeffectJson = SideEffectRepo.GetSideEffect(seid);
                    //Debug.Log("To Play -ve Effect: " + sideeffectJson.effectpath);
                    mPrevNegVisualSEName = sideeffectJson.name;
                    PlaySEEffect(mPrevNegVisualSEName);
                }
                return true;
            }
            return false;
        }

        public bool IsMoving()
        {
            if (mAction == null)
                return false;
            return IsMovingAction(mAction.mdbCommand.GetActionType());

            //ActionCommand cmd = GetActionCmd();
            //if (cmd == null)
            //    return false;

            //return IsMovingAction(cmd.GetActionType());
        }

        public bool IsDying()
        {
            if (mAction == null)
                return false;
            return mAction.mdbCommand.GetActionType() == ACTIONTYPE.DEAD;
        }
        public bool IsIdling()
        {
            if (mAction == null)
                return true;
            return (mAction.mdbCommand.GetActionType() == ACTIONTYPE.IDLE);
        }

        public bool IsMovingAction(ACTIONTYPE actiontype)
        {
            return (actiontype == ACTIONTYPE.WALK ||
                actiontype == ACTIONTYPE.APPROACH_PATHFIND ||
                //actiontype == ACTIONTYPE.WALKANDCAST || //root on walkandcast is done within the action
                actiontype == ACTIONTYPE.WALK_WAYPOINT);
        }

        public virtual void SetHeadLabel(bool init = false)
        {
        }

        public void DisplaySkillIndicator(SkillData skillgroup, Vector3? pos)
        {
            Threatzone threatzone = skillgroup.skillgroupJson.threatzone;

            if (threatzone == Threatzone.Single)
            {
                HideSkillIndicator();
                return;
            }
            if (skillgroup.skillgroupJson.skillbehavior == SkillBehaviour.Ground)
            {
                if (mSkillIndicator360 == null)
                {
                    mSkillIndicator360 = ObjPoolMgr.Instance.GetObject(OBJTYPE.MODEL, "Effects_ZDSP_Indicators_prefab/SkillIndicator360.prefab", true);
                }
                Vector3 offsetpos = new Vector3(pos.Value.x, pos.Value.y + 0.3f, pos.Value.z);
                mSkillIndicator360.transform.position = offsetpos;
                Projector projector = mSkillIndicator360.GetComponent<Projector>();
                projector.orthographicSize = skillgroup.skillJson.radius;
                mSkillIndicator360.SetActive(true);
                return;
            }
            if (threatzone == Threatzone.DegreeArc120)
            {
                if (mSkillIndicator120 == null)
                {
                    mSkillIndicator120 = ObjPoolMgr.Instance.GetObject(OBJTYPE.MODEL, "Effects_ZDSP_Indicators_prefab/SkillIndicator120.prefab", true);
                    mSkillIndicator120.transform.SetParent(AnimObj.transform, false);
                }
                Projector projector = mSkillIndicator120.GetComponent<Projector>();
                projector.orthographicSize = skillgroup.skillJson.radius;
                mSkillIndicator120.SetActive(true);
            }
            else if (threatzone == Threatzone.DegreeArc360)
            {
                if (mSkillIndicator360 == null)
                {
                    mSkillIndicator360 = ObjPoolMgr.Instance.GetObject(OBJTYPE.MODEL, "Effects_ZDSP_Indicators_prefab/SkillIndicator360.prefab", true);
                    mSkillIndicator360.transform.SetParent(AnimObj.transform, false);
                }
                Projector projector = mSkillIndicator360.GetComponent<Projector>();
                projector.orthographicSize = skillgroup.skillJson.radius;
                mSkillIndicator360.SetActive(true);
            }
            else if (threatzone == Threatzone.LongStream)
            {
                if (mSkillIndicatorLong == null)
                {
                    mSkillIndicatorLong = ObjPoolMgr.Instance.GetObject(OBJTYPE.MODEL, "Effects_ZDSP_Indicators/SkillIndicatorLong.prefab", true);
                    mSkillIndicatorLong.transform.SetParent(AnimObj.transform, false);
                }

                Projector projector = mSkillIndicatorLong.GetComponent<Projector>();
                projector.orthographicSize = skillgroup.skillJson.range; //x
                projector.aspectRatio = skillgroup.skillJson.radius / skillgroup.skillJson.range; // z/x
                mSkillIndicatorLong.SetActive(true);
            }
        }

        public void HideSkillIndicator()
        {
            if (mSkillIndicator120 != null)
                mSkillIndicator120.SetActive(false);

            if (mSkillIndicator360 != null)
                mSkillIndicator360.SetActive(false);

            if (mSkillIndicatorLong != null)
                mSkillIndicatorLong.SetActive(false);
        }

        public override bool Interact()
        {
            PlayerGhost player = GameInfo.gLocalPlayer;
            if ((IsMonster() || IsPlayer()) && CombatUtils.IsValidEnemyTarget(player, this))
            {
                if (GameInfo.gSelectedEntity != null && GameInfo.gSelectedEntity.ID == ID)
                {
                    GameInfo.gCombat.CommonCastBasicAttack(GetPersistentID());
                    return true;
                }
            }
            return false;
        }

        public virtual void SetAnimationActive(bool active)
        {
            if (active)
            {
                var animators = mCharController.gameObject.GetComponentsInChildren<Animator>();

                foreach (var animator in animators)
                    animator.StartPlayback();
            }
            else
            {
                var animators = mCharController.gameObject.GetComponentsInChildren<Animator>();

                foreach (var animator in animators)
                    animator.StopPlayback();
            }
        }

        public abstract int GetMinDmg();
        public abstract int GetAccuracy();
        public abstract int GetAttack();
        public abstract int GetCritical();
        public abstract int GetCriticalDamage();
        public abstract int GetArmor();
        public abstract int GetEvasion();
        public abstract int GetCocritical();
        public abstract int GetCocriticalDamage();
        public abstract float GetExDamage();
    }
}
