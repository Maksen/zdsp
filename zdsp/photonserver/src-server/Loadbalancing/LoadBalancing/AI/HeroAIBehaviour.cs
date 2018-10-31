using Kopio.JsonContracts;
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

        public HeroAIBehaviour(HeroEntity hero) : base(hero)
        {
            mHeroEntity = hero;
            mOwner = mHeroEntity.Owner;
            mTarget = null;
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

        protected override void OnIdleEnter(string prevstate)
        {
            // Cannot set Idle on HeroEntity Init because it will interrupt summon action, so if ActionCommand is still null
            // when enter Idle state, set the Idle action
            ActionCommand cmd = mHeroEntity.GetActionCmd();
            if (cmd == null)
                mHeroEntity.Idle();
        }

        protected override void OnIdleUpdate(long dt)
        {
            if (mOwner.LocalCombatStats.IsInCombat) //owner has casted skill or got attacked
            {
                Actor potentialTarget = DeterminePotentialTarget();
                if (potentialTarget != null)
                {
                    SwitchTarget(potentialTarget);
                    ResetSkillToExecute();
                    GotoState("CombatApproach");
                    return;
                }
            }

            // owner not in combat or cannot get a valid target
            if (!IsTargetInRange())  //out of range so approach owner
            {
                GotoState("Follow");
                ApproachTarget();
            }
        }

        #endregion Idle State

        #region Follow State

        protected void OnFollowEnter(string prevstate)
        {
        }

        protected void OnFollowLeave()
        {
        }

        protected void OnFollowUpdate(long dt)
        {
            // Owner go into combat when approaching
            if (mOwner.LocalCombatStats.IsInCombat)
            {
                Actor potentialTarget = DeterminePotentialTarget();
                if (potentialTarget != null)
                {
                    SwitchTarget(potentialTarget);
                    ResetSkillToExecute();
                    GotoState("CombatApproach");
                    return;
                }
            }

            // owner not in combat or cannot get valid target, so usual follow
            if (IsTargetInRange())
            {
                GotoState("Idle");
            }
            else  // not in range yet, can be idling or still approaching
            {
                ActionCommand cmd = mHeroEntity.GetActionCmd();
                if (cmd == null || cmd.GetActionType() == ACTIONTYPE.IDLE)
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
            if (!CheckTargetValid())  // if invalid will go back to idle
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
                ActionCommand cmd = mHeroEntity.GetActionCmd();

                //Approach if out of range (include path find) and not already doing pathfind (note that monster can idle while waiting for pathfind result)
                if (cmd == null || cmd.GetActionType() == ACTIONTYPE.IDLE)
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

        private Actor DeterminePotentialTarget()
        {
            Actor potentialTarget = null;

            BaseServerCastSkill skillAction = mOwner.GetAction() as BaseServerCastSkill;
            if (skillAction != null && !skillAction.IsFriendlySkill())  //owner is casting an enemy targeting skill
            {
                //get target of owner's cast skill and also attack that target
                potentialTarget = skillAction.GetTarget();
            }

            // owner not attacking any target
            if (potentialTarget == null)
            {
                // check other players attacking owner first
                var playerAttackers = mOwner.GetPlayerAttackers();
                foreach (var player in playerAttackers)
                {
                    if (!IsTargetInvalid(player.Value))
                        return player.Value;
                }

                // check npcs attacking owner
                var npcAttackers = mOwner.GetNPCAttackers();
                foreach (var npc in npcAttackers)
                {
                    if (!IsTargetInvalid(npc.Value))
                        return npc.Value;
                }
            }

            return potentialTarget;
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
            }
            else
            {
                //Skill being cast definitely not in cooldown here, can safely cast
                mSkillCDEnd[1] = now + (long)(mSkillToExecute.skillJson.cooldown * 1000);
            }
            mHeroEntity.CastSkill(mSkillToExecute.skillJson.id, mTarget.GetPersistentID(), mTarget.Position);
        }

        protected bool IsTargetInvalid(Actor target)
        {
            return CombatUtils.IsInvalidTarget(target) || !CombatUtils.IsEnemy(mOwner, target)
                || !mOwner.IsAlive() || !mOwner.LocalCombatStats.IsInCombat;
        }

        protected virtual bool CheckTargetValid()
        {
            if (IsTargetInvalid(mTarget))
            {
                SwitchTarget(null);

                // current target is invalid, determine next potential target
                Actor potentialTarget = DeterminePotentialTarget();
                if (potentialTarget != null)
                {
                    SwitchTarget(potentialTarget);
                    ResetSkillToExecute();
                }

                if (mTarget == null)
                    GotoState("Idle");
                else
                    GotoState("CombatApproach");
                return false;
            }
            else  // target is still valid
            {
                Actor potentialTarget = DeterminePotentialTarget(); // check whether owner has switched target
                if (potentialTarget != mTarget) // if new target not the same as current target, switch target
                {
                    SwitchTarget(potentialTarget);
                    ResetSkillToExecute();
                    if (mTarget == null)
                        GotoState("Idle");
                    else
                        GotoState("CombatApproach");
                    return false;
                }
                return true;  // same target, so continue attacking this target
            }
        }

        protected void SwitchTarget(Actor target)
        {
            if (mTarget != null)
                mTarget.RemoveNPCAttacker(mHeroEntity);

            mTarget = target;
            if (mTarget != null)
                mTarget.AddNPCAttacker(mHeroEntity);
            else
                mIsAttacking = false;
        }
    }
}