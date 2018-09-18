//#define COMBAT_DEBUG

using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Repository;
using Zealot.Server.Actions;
using Zealot.Server.Entities;

namespace Zealot.Server.AI
{
    public class HeroAIBehaviour : BaseAIBehaviour
    {
        private HeroEntity mHeroEntity;
        private Player mOwner;
        private Actor mTarget;
        private SkillData mSkillToExecute;
        private float mSkillRange;
        private long[] mSkillCDEnd;
        private bool mIsAttacking;
        private SkillData mBasicAttack;  //cache for quick access
        private SkillData mSkill1;  //cache for quick access
        private long mTimeElapsed;
        private long mSummonDuration;

        private int mThreatScanCount;
        private Dictionary<int, Threat> mThreats = new Dictionary<int, Threat>();

        public HeroAIBehaviour(HeroEntity hero) : base(hero)
        {
            mHeroEntity = hero;
            mOwner = mHeroEntity.Owner;
            mTarget = mOwner;
            mSkillCDEnd = new long[2];  // 0 - basic attack, 1 - skill
            mBasicAttack = SkillRepo.GetSkill(mHeroEntity.HeroData.basicattack);
            mSkill1 = SkillRepo.GetSkillByGroupIDOfLevel(mHeroEntity.HeroData.skill1grp, mHeroEntity.Hero.Skill1Level);
        }

        protected override void InitStates()
        {
            AddState("Summoning", OnSummoningEnter, OnSummoningLeave, OnSummoningUpdate);
            AddState("Idle", OnIdleEnter, OnIdleLeave, OnIdleUpdate);
            AddState("Follow", OnFollowEnter, OnFollowLeave, OnFollowUpdate);
            AddState("CombatApproach", OnCombatApproachEnter, OnCombatApproachLeave, OnCombatApproachUpdate);
            AddState("CombatExecute", OnCombatExecuteEnter, OnCombatExecuteLeave, OnCombatExecuteUpdate);
        }

        public override void StartMonitoring()
        {
            mHeroEntity.StartAIBehaviour();
        }

        #region Summoning State

        protected void OnSummoningEnter(string prevstate)
        {
            mSummonDuration = (long)(mHeroEntity.HeroData.summonduration * 1000) + 1000;  // add extra time for client to complete animation
            mTimeElapsed = 0;
        }

        protected void OnSummoningUpdate(long dt)
        {
            mTimeElapsed += dt;
            if (mTimeElapsed >= mSummonDuration)
            {
                GotoState("Idle");
            }
        }

        protected void OnSummoningLeave()
        {
            mHeroEntity.HeroSynStats.Summoning = false;
        }

        #endregion Summoning State

        #region Idle State

        protected override void OnIdleUpdate(long dt)
        {
#if !COMBAT_DEBUG
            BaseServerCastSkill skillAction = mOwner.GetAction() as BaseServerCastSkill;
            if (skillAction != null && !skillAction.IsFriendlySkill())  //owner is casting an enemy targeting skill
            {
                //get target of owner's cast skill and also attack that target
                Actor attackingTarget = skillAction.GetTarget();
                if (attackingTarget != null)
                {
                    mTarget = attackingTarget;
                    ResetSkillToExecute();
                    GotoState("CombatApproach");
                }
            }
            else //owner is not attacking
            {
                if (!IsTargetInRange())  //out of range so approach owner
                {
                    GotoState("Follow");
                    ApproachTarget();
                }
            }
#else
            //scan for player opponent once every 2 updates (each update is 500 msec)
            Actor threat = ThreatScan(2);
            if (threat != null)
            {
                AddThreat(threat.GetPersistentID(), threat, 0);
                SwitchTarget(threat);
                ResetSkillToExecute();
                GotoState("CombatApproach");
            }
#endif
        }

        #endregion Idle State

        #region Follow State

        private void OnFollowEnter(string prevstate)
        {
        }

        private void OnFollowLeave()
        {
        }

        private void OnFollowUpdate(long dt)
        {
            if (IsTargetInRange())
            {
                GotoState("Idle");
            }
            else  // not in range yet, can be idling or still approaching
            {
                ACTIONTYPE actiontype = mHeroEntity.GetActionCmd().GetActionType();
                if (actiontype == ACTIONTYPE.IDLE)
                {
                    ApproachTarget();
                }
            }
        }

        #endregion Follow State

        #region Combat State

        protected override void OnCombatApproachEnter(string prevstate)
        {
            mIsAttacking = true;
            if (mSkillToExecute == null)
                DetermineSkillToExecute(); //At the least, there will always be a basic attack available to use
        }

        protected override void OnCombatApproachUpdate(long dt)
        {
            //Determine if target still valid
            if (!CheckTargetValid())
                return;

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
                ACTIONTYPE actiontype = mHeroEntity.GetActionCmd().GetActionType();

                //Approach if out of range (include path find) and not already doing pathfind (note that monster can idle while waiting for pathfind result)
                if (actiontype == ACTIONTYPE.IDLE)
                {
                    ApproachTarget();
                }
            }
        }

        protected override void OnCombatExecuteEnter(string prevstate)
        {
            CastSkill();
        }

        protected override void OnCombatExecuteUpdate(long dt)
        {
            ACTIONTYPE actiontype = mHeroEntity.GetActionCmd().GetActionType();

            if (actiontype != ACTIONTYPE.CASTSKILL) //if has stopped attacking
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
                    GotoState("CombatApproach");
                    ApproachTarget();
                }
            }
        }

        #endregion Combat State

        public bool IsTargetInRange()
        {
            Actor target = mIsAttacking ? mTarget : mOwner;
            float range = mIsAttacking ? mSkillRange : mHeroEntity.HeroData.followdistance;
            return GameUtils.InRange(mHeroEntity.Position, target.Position, range, target.Radius);
        }

        public void ApproachTarget()
        {
            Actor target = mIsAttacking ? mTarget : mOwner;
            float range = mIsAttacking ? mSkillRange : mHeroEntity.HeroData.followdistance;
            range = System.Math.Max(0f, range - 0.5f); //-0.5f so that it does not toggle at bordercase
            mHeroEntity.ApproachTargetWithPathFind(target.GetPersistentID(), null, range, true, false);
        }

        public void UpdateSkill()
        {
            mSkill1 = SkillRepo.GetSkillByGroupIDOfLevel(mHeroEntity.HeroData.skill1grp, mHeroEntity.Hero.Skill1Level);
        }

        protected void DetermineSkillToExecute()
        {
            bool hasSkill = false;
            long now = mHeroEntity.EntitySystem.Timers.GetSynchronizedTime();

            if (mSkill1 != null && now >= mSkillCDEnd[1])  //skill not cooling down, can execute
            {
                mSkillToExecute = mSkill1;
                hasSkill = true;
            }

            if (!hasSkill)
            {
                mSkillToExecute = mBasicAttack;
            }

            if (mSkillToExecute != null)
            {
                SkillGroupJson skillgroupJson = mSkillToExecute.skillgroupJson;
                if (skillgroupJson.threatzone == Threatzone.LongStream)
                    mSkillRange = mSkillToExecute.skillJson.range;
                else
                    mSkillRange = mSkillToExecute.skillJson.radius;
            }
        }

        protected void ResetSkillToExecute()
        {
            mSkillToExecute = null;
        }

        protected void CastSkill()
        {
            //Basic Attack might still be in cooldown here
            long now = mHeroEntity.EntitySystem.Timers.GetSynchronizedTime();
            if (mSkillToExecute.skillgroupJson.skilltype == SkillType.BasicAttack)
            {
                if (now < mSkillCDEnd[0])
                    return;
                mSkillCDEnd[0] = now + (long)(mSkillToExecute.skillJson.cooldown * 1000);
                //GameUtils.DebugWriteLine("use basic attack!!");
            }
            else
            {
                //Skill being cast definitely not in cooldown here, can safely cast
                mSkillCDEnd[1] = now + (long)(mSkillToExecute.skillJson.cooldown * 1000);
                //GameUtils.DebugWriteLine("use skill!!");
            }
            mHeroEntity.CastSkill(mSkillToExecute.skillJson.id, mTarget.GetPersistentID());
        }

        protected bool IsTargetInvalid(Actor target)
        {
#if !COMBAT_DEBUG
            BaseServerCastSkill skillAction = mOwner.GetAction() as BaseServerCastSkill;
            return !mOwner.IsAlive() || skillAction == null || target != skillAction.GetTarget() || CombatUtils.IsInvalidTarget(target);
#else
            return CombatUtils.IsInvalidTarget(target);
#endif
        }

        protected virtual bool CheckTargetValid()
        {
            if (IsTargetInvalid(mTarget))
            {
#if !COMBAT_DEBUG
                mTarget = null;
                mIsAttacking = false;
                GotoState("Idle");
                return false;
#else
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
#endif
            }
            return true;
        }

        protected Actor ThreatScan(int tickcount)
        {
            Actor threat = null;
            if (mThreatScanCount % tickcount == 0)
            {
                threat = mHeroEntity.QueryForThreat();
                mThreatScanCount = 0;
            }
            mThreatScanCount++;
            return threat;
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

        protected void SwitchTarget(Actor target)
        {
            if (mTarget != null)
                mTarget.RemoveNPCAttacker(mHeroEntity); //aiplayer is no longer seeking this target

            mTarget = target;
            if (mTarget != null)
                mTarget.AddNPCAttacker(mHeroEntity);
        }
    }
}