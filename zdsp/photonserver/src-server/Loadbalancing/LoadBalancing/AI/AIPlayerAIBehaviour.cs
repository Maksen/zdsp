using System.Collections.Generic;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Common.Entities;
using Zealot.Server.Entities;
using Zealot.Server.Actions;
using Zealot.Repository;
using UnityEngine;
using Kopio.JsonContracts;

namespace Zealot.Server.AI
{    
    public class AIPlayerAIBehaviour : BaseAIBehaviour
    { 
        public enum AttackType : int
        {
            BasicAttack = 0,
            JobSkill = 1,
            Red = 2,
            Green = 3,
            Blue = 4,                   
            None = 16
        }

        protected AIPlayer mAIPlayer;
        protected Actor mTarget;

        protected float mSkillRange;
        protected SkillData mSkillToExecute;
        protected int mSkillIndexToExecute;
        protected long mLastBasicAttackTime;
        protected byte mCastBasicAttackIndex; // 0 to 3

        private int mThreatScanCount;
        protected Dictionary<int, Threat> mThreats;

        private static AttackType[] mAttackSequence = new AttackType[] {
            //longest cooldown should be higher priority. If shortest cd style is set as higher priority, this style will always cast and other styles will not be given the chance to cast.            
            //AttackType.JobSkill,
            //AttackType.Red,            
            //AttackType.Green,
            //AttackType.Blue,            
            AttackType.BasicAttack            
        };

        public AIPlayerAIBehaviour(AIPlayer player) : base(player)
        {
            mAIPlayer = player;
            mTarget = null;
            mSkillToExecute = null;
            mSkillIndexToExecute = -1;
            mLastBasicAttackTime = 0;
            mCastBasicAttackIndex = 0;
            mThreats = new Dictionary<int, Threat>();
        }

        //function called after bahavior is setup 
        public override void StartMonitoring()
        {
            GotoState("Idle");
        }

        protected void SwitchTarget(Actor target)
        {
            if (mTarget != null)
                mTarget.RemoveNPCAttacker(mAIPlayer); //aiplayer is no longer seeking this target

            mTarget = target;
            if (mTarget != null)
                mTarget.AddNPCAttacker(mAIPlayer);
        }

        protected override void OnIdleUpdate(long dt)
        {
            //scan for player opponent once every 2 updates (each update is 500 msec)
            Actor threat = ThreatScan(2);
            if (threat != null)
            {
                AddThreat(threat.GetPersistentID(), threat, 0);
                SwitchTarget(threat);
                ResetSkillToExecute();
                GotoState("CombatApproach");
                return;
            }
        }

        protected Actor ThreatScan(int tickcount)
        {
            Actor threat = null;
            if (mThreatScanCount % tickcount == 0)
            {
                threat = mAIPlayer.QueryForThreat();
                mThreatScanCount = 0;
            }
            mThreatScanCount++;
            return threat;
        }

        protected bool IsTargetInRange()
        {
            return GameUtils.InRange(mAIPlayer.Position, mTarget.Position, mSkillRange, mTarget.Radius);
        }

        public void AddThreat(int attackerPID, Actor attackerActor, int aggro)
        {            
            if (mThreats.ContainsKey(attackerPID))
            {
                mThreats[attackerPID].aggro += aggro;
            }
            else
            {
                mThreats.Add(attackerPID, new Threat(attackerActor, aggro));
            }            
        }

        protected void ResetSkillToExecute()
        {         
            mSkillToExecute = null;
            mSkillIndexToExecute = -1;
        }

        protected void SelectBasicAttackToExecute()
        {
            SkillData skilldata = SkillRepo.GetSkillByGroupID(mAIPlayer.SkillStats.basicAttack1SId);
            mSkillToExecute = skilldata;
            mSkillIndexToExecute = -1;
        }

        protected void DetermineSkillToExecute()
        {            
            SkillSynStats skillStats = mAIPlayer.SkillStats;
            for (int i = 0; i < mAttackSequence.Length; i++)
            {
                AttackType attackType = mAttackSequence[i];
                if (attackType == AttackType.BasicAttack) //definitely will do basic attack if all other options fail
                {
                    SelectBasicAttackToExecute();
                    break;
                }
                else if (attackType >= AttackType.JobSkill && attackType <= AttackType.Blue) //skill 1 to 5
                {
                    if (mAIPlayer.HasControlStatus(ControlSEType.Silence))
                        continue;
                    if (mAIPlayer.IsSkillGlobalCoolDown())
                        continue;

                    int skillid = 0;
                    if (attackType == AttackType.JobSkill)
                    {
                        skillid = skillStats.JobskillAttackSId;
                        if (skillid == 0 || mAIPlayer.IsSkillCooldown(0))
                            continue;
                        mSkillIndexToExecute = 0;
                    }
                    //else if (attackType == AttackType.Red)
                    //{
                    //    skillid = skillStats.RedHeroCardSkillAttackSId;
                    //    if (skillid == 0 || mAIPlayer.IsSkillCooldown(1))
                    //        continue;
                    //    mSkillIndexToExecute = 1;
                    //}
                    //else if (attackType == AttackType.Green)
                    //{
                    //    skillid = skillStats.GreenHeroCardSkillAttackSId;
                    //    if (skillid == 0 || mAIPlayer.IsSkillCooldown(2))
                    //        continue;
                    //    mSkillIndexToExecute = 2;
                    //}
                    //else if (attackType == AttackType.Blue)
                    //{
                    //    skillid = skillStats.BlueHeroCardSkillAttackSId;
                    //    if (skillid == 0 || mAIPlayer.IsSkillCooldown(3))
                    //        continue;
                    //    mSkillIndexToExecute = 3;
                    //}
                    else
                        continue;
                    SkillData skilldata = SkillRepo.GetSkillByGroupID(skillid);
                    mSkillToExecute = skilldata;
                    break;
                }
            }

            if (mSkillToExecute != null) //now it's possible that no skill may be selected (very small chance if cooldown setting is wrong)
            {
                SkillGroupJson skillgroupJson = mSkillToExecute.skillgroupJson;
                if (skillgroupJson.threatzone == Threatzone.LongStream)
                    mSkillRange = mSkillToExecute.skillJson.range;
                else
                    mSkillRange = mSkillToExecute.skillJson.radius;
                if (mSkillRange < 2.0f)
                    mSkillRange = 2.0f;
            }
        }

        protected void ApproachTarget()
        {         
            PositionSlots slots = mTarget.PositionSlots;
            slots.DeallocateSlot(mAIPlayer);
            //System.Diagnostics.Debug.WriteLine("next skill range :" + mSkillRange);
            //Arena is open space with no obstacles, so path finding is not required
            mAIPlayer.ApproachTarget(mTarget.GetPersistentID(), mSkillRange - 0.5f); //-0.5f so that it does not toggle at bordercase
        }
        
        #region Combat State        
        protected override void OnCombatApproachEnter(string prevstate)
        {            
            if (mSkillToExecute == null)
                DetermineSkillToExecute(); //At the least, there will always be a basic attack available to use
        }

        protected override void OnCombatApproachUpdate(long dt)
        {
            //Determine if target still valid
            if (!CheckTargetValid())
                return;

            Action action = mAIPlayer.GetAction();
            if (mSkillToExecute == null)
            {
                DetermineSkillToExecute();
                return; //wait for next update to continue
            }

            //Determine if target is within range
            //Attack if within range
            if (IsTargetInRange())
            {
                //definitely will be either idle or approach or walk here
                
                GotoState("CombatExecute");                
            }
            else //Out of range, either idling or still approaching
            {
                //Approach if out of range (include path find) and not already doing pathfind (note that monster can idle while waiting for pathfind result)
                if (action.mdbCommand.GetActionType() == ACTIONTYPE.IDLE &&
                    !mAIPlayer.HasControlStatus(ControlSEType.Root))
                {
                    ApproachTarget();
                }
            }
        }

        #endregion


        #region CombatExecute State
        protected override void OnCombatExecuteEnter(string prevstate)
        {
            //Normal monsters only have 1 normal attack. While boss can have additional skills of various range                        
            CastSkill();
        }

        //protected override void OnCombatExecuteLeave() { }        

        protected override void OnCombatExecuteUpdate(long dt)
        {            
            ACTIONTYPE actiontype = mAIPlayer.GetActionCmd().GetActionType();
            
            if (actiontype == ACTIONTYPE.CASTSKILL || actiontype == ACTIONTYPE.WALKANDCAST)
            {
                if (IsTargetInvalid(mTarget))
                    return;

                if (mSkillToExecute.skillgroupJson.canturn)
                {
                    Vector3 diff = mTarget.Position - mAIPlayer.Position;
                    diff.y = 0;
                    mAIPlayer.Forward = diff.normalized;
                }
                else if (mSkillToExecute.skillgroupJson.moonwalk)
                {
                    //walkandcast action, try to move towards target here
                    ServerAuthoWalkAndCast action = (ServerAuthoWalkAndCast) mAIPlayer.GetAction();
                    Vector3 desiredPos = mTarget.Position + (mAIPlayer.Position - mTarget.Position).normalized * (mTarget.Radius + mAIPlayer.Radius);
                    action.WalkToNewPos(desiredPos); //Simple walk to. This is assuming this AI behaviour is only used in Arena and there are no obstacles blocking
                }
                else if (actiontype == ACTIONTYPE.CASTSKILL) //Allow basic attack as soon as possible
                {
                    ServerAuthoCastSkill skillaction = mAIPlayer.GetAction() as ServerAuthoCastSkill;
                    if (skillaction != null && skillaction.IsBasicAttack())
                    {
                        //check basic attack 1st & 2nd
                        //checktargetvalid
                        //check within time limit                        
                        //check target within basic attack range
                        int basicAttackIndex = SkillIDToBasicAttackIndex(mSkillToExecute.skillJson.id);
                        if (basicAttackIndex >=1 && basicAttackIndex <=2)
                        {
                            long now = mAIPlayer.EntitySystem.Timers.GetSynchronizedTime();
                            if (now >= mLastBasicAttackTime + 300 && now <= mLastBasicAttackTime + 1000) //combo possible
                            {
                                if (IsTargetInRange()) //used previous basic attack range to check for range
                                {
                                    SelectBasicAttackToExecute();                                    
                                    CastSkill(); //if rooted or silence, can still cast here (in range)
                                }
                            }
                        }                                                                       
                    }
                }
            }
            else //if has stopped attacking
            {
                ResetSkillToExecute(); //prepare to decide next skill

                if (!CheckTargetValid())
                    return;

                //just finished last execution
                DetermineSkillToExecute();
                if (mSkillToExecute == null)
                    return;

                if (IsTargetInRange())
                {
                    CastSkill();
                }
                else
                {
                    if (!mAIPlayer.HasControlStatus(ControlSEType.Root))
                    {
                        GotoState("CombatApproach");
                        ApproachTarget();
                    }
                }
            }            
        }

        private int SkillIDToBasicAttackIndex(int skillid)
        {
            if (skillid == mAIPlayer.SkillStats.basicAttack1SId)
                return 0;
            else if (skillid == mAIPlayer.SkillStats.basicAttack2SId)
                return 1;
            else if (skillid == mAIPlayer.SkillStats.basicAttack3SId)
                return 2;

            return -1;
        }

        protected void CastSkill()
        {
            //Basic Attack might still be in cooldown here
            long now = mAIPlayer.EntitySystem.Timers.GetSynchronizedTime();
            SkillGroupJson skillgroupJson = mSkillToExecute.skillgroupJson;
            if (skillgroupJson.skilltype == SkillType.BasicAttack)
            {
                if (mAIPlayer.NumOfDisarm > 0)
                    return;
                if (now >= mLastBasicAttackTime + 300 && now <= mLastBasicAttackTime + 1000) //combo possible
                {
                    mCastBasicAttackIndex++;
                    if (mCastBasicAttackIndex >= 3)
                        mCastBasicAttackIndex = 0;
                                        
                    if (mCastBasicAttackIndex == 1)
                    {
                        SkillData skilldata = SkillRepo.GetSkillByGroupID(mAIPlayer.SkillStats.basicAttack2SId);
                        mSkillToExecute = skilldata;
                        //System.Diagnostics.Debug.WriteLine(now + " cast attack2");
                    }
                    else if (mCastBasicAttackIndex == 2)
                    {
                        SkillData skilldata = SkillRepo.GetSkillByGroupID(mAIPlayer.SkillStats.basicAttack3SId);
                        mSkillToExecute = skilldata;
                        //System.Diagnostics.Debug.WriteLine(now + " cast attack3");
                    }
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine(now + " cast attack1");
                    mCastBasicAttackIndex = 0;
                }
                mLastBasicAttackTime = now;
            }    
            else //skill
            {
                if (mAIPlayer.NumOfStun > 0 || mAIPlayer.NumOfDisarm > 0 || mAIPlayer.NumOfSlience > 0)
                    return;
                float cooldown = System.Math.Max (mSkillToExecute.skillJson.cooldown, 0);
                mAIPlayer.SetSkillCDEnd(mSkillIndexToExecute, cooldown);
                if (mSkillToExecute.skillJson.globalcd > 0)
                {
                    mAIPlayer.SetSkillGCDEnd(now + (long)(mSkillToExecute.skillJson.globalcd * 1000));
                }
            }
            
            if (skillgroupJson.moonwalk)
                mAIPlayer.WalkAndCastSkill(mSkillToExecute.skillgroupJson.id, mTarget.GetPersistentID());
            //else if (skillgroupJson.dashattack)
            //{
            //    mAIPlayer.DashAttack(mSkillToExecute.skillgroupJson.id, mTarget.GetPersistentID());
            //}
            else
                mAIPlayer.CastSkill(mSkillIndexToExecute,mSkillToExecute.skillgroupJson.id, mTarget.GetPersistentID());

        }
        #endregion

        protected bool IsTargetInvalid(Actor target)
        {
            return CombatUtils.IsInvalidTarget(target);
        }

        protected virtual bool CheckTargetValid()
        {            
            if (IsTargetInvalid(mTarget))
            {
                if (mTarget != null)
                {
                    mThreats.Remove(mTarget.GetPersistentID());
                }
                SwitchTarget(null);

                List<int> removeList = new List<int>();
                foreach (KeyValuePair<int, Threat> entry in mThreats)
                {
                    int pid = entry.Key;
                    Actor potentialTarget = entry.Value.actor;
                    if (IsTargetInvalid(potentialTarget)) //Take this opportunity to remove all invalid targets (by right arena only 1 opponent)
                    {
                        removeList.Add(pid);
                    }
                    else if (mTarget == null)
                    {
                        SwitchTarget(potentialTarget);
                        ResetSkillToExecute();
                    }
                }

                foreach (int pid in removeList)
                    mThreats.Remove(pid);

                if (mTarget == null)
                    GotoState("Idle");
                else
                    GotoState("CombatApproach");
                return false;
            }
            return true;
        }

        public override void OnAttacked(IActor attacker, int aggro)
        {
            //We are not having an aggro system right now. AIPlayer will attack the very first target only.            
            Actor attackerActor = attacker as Actor;//Currently, we handle only for actor. Maybe, there could be other forms of IActor in the future e.g. StaticActor?
            if (attackerActor == null)
                return;

            int attackerPID = attackerActor.GetPersistentID();
            AddThreat(attackerPID, attackerActor, aggro);

            string currentStateName = GetCurrentStateName();
            if (currentStateName == "CombatApproach" || currentStateName == "CombatExecute" || currentStateName == "Goback" ||
                currentStateName == "RecoverFromUlti")
                return;

            SwitchTarget(attackerActor); //The first attacker will be targeted
            ResetSkillToExecute();

            if (!mAIPlayer.HasControlStatus(ControlSEType.Stun)) //if not in stun state
                GotoState("CombatApproach");
        }

        public override void OnKilled()
        {
            SwitchTarget(null);
            mThreats.Clear();
        }
        
        #region stun state
        protected override void OnStunEnter(string prevstate)
        {
            base.OnStunEnter(prevstate);
            mAIPlayer.Idle();
        }

        protected override void OnStunUpdate(long dt)
        {
            if (!mActor.HasControlStatus(ControlSEType.Stun))
            {
                GotoState("CombatApproach");
            }
        }

        protected override void OnStunLeave()
        {
            base.OnStunLeave();
        }
        #endregion
    }
}