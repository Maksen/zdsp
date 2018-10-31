#define LOG_BOSS_AI
using ExitGames.Logging;
using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Common.Entities;
using Zealot.Server.Entities;
using Zealot.Entities;
using Zealot.Repository;

namespace Zealot.Server.AI
{
    public class BaseAIBehaviour : StateMachine
    {
        protected static readonly ILogger log = LogManager.GetCurrentClassLogger();
        protected Actor mActor;
        protected Vector3 mSpawnPos;
        public BaseAIBehaviour(Actor actor)
        {
            mActor = actor;
            mSpawnPos = mActor.Position;
            InitStates();
        }

        protected virtual void InitStates()
        {
            AddState("Idle", OnIdleEnter, OnIdleLeave, OnIdleUpdate);
            AddState("RecoverFromKnockedBack", OnRecoverFromKnockedBackEnter, OnRecoverFromKnockedBackLeave, OnRecoverFromKnockedBackUpdate);
            AddState("Stun", OnStunEnter, OnStunLeave, OnStunUpdate);
            AddState("Frozen", OnFrozenEnter, OnFrozenLeave, OnFrozenUpdate);
            AddState("CombatApproach", OnCombatApproachEnter, OnCombatApproachLeave, OnCombatApproachUpdate);
            AddState("CombatExecute", OnCombatExecuteEnter, OnCombatExecuteLeave, OnCombatExecuteUpdate);
            AddState("ResolveOverlap", OnResolveOverlapEnter, OnResolveOverlapLeave, OnResolveOverlapUpdate);
            AddState("Goback", OnGobackEnter, OnGobackLeave, OnGobackUpdate);             
        }

        //function called after bahavior is setup 
        public virtual void StartMonitoring()
        {
            GotoState("Idle");
        }

        public override void OnUpdate(long dt)
        {
            if (mActor.IsAlive())
                base.OnUpdate(dt);
        }

        public bool LogAI { get
            {
                Monster ms = mActor as Monster;
                if(ms != null)
                {
                    return ms.LogAI;
                }
                return false;
            }
        }

        #region Idle State
        protected virtual void OnIdleEnter(string prevstate)
        {
#if LOG_BOSS_AI
            if (LogAI)
                log.Info("OnIdleEnter");
#endif
        }
        protected virtual void OnIdleLeave()
        { 
        }

        protected virtual void OnIdleUpdate(long dt)
        {
        }
        #endregion
        #region RecoverFromKnockBack State
        protected virtual void OnRecoverFromKnockedBackEnter(string prevstate)
        {
#if LOG_BOSS_AI
            if (LogAI)
                log.Info("OnRecoverFromKnockedBackEnter");
#endif
        }
        protected virtual void OnRecoverFromKnockedBackLeave()
        {

        }

        protected virtual void OnRecoverFromKnockedBackUpdate(long dt)
        {
        }
        #endregion

        #region Stun State
        protected virtual void OnStunEnter(string prevstate)
        {
#if LOG_BOSS_AI
            if (LogAI)
                log.Info("OnStunEnter");
#endif
        }
        protected virtual void OnStunLeave()
        {

        }

        protected virtual void OnStunUpdate(long dt)
        {            
        }
        #endregion

        #region Frozen State
        protected virtual void OnFrozenEnter(string prevstate)
        {
#if LOG_BOSS_AI
            if (LogAI)
                log.Info("OnFrozenEnter");
#endif
        }
        protected virtual void OnFrozenLeave()
        {

        }

        protected virtual void OnFrozenUpdate(long dt)
        {
        }
        #endregion

        #region Combat State
        protected virtual void OnCombatApproachEnter(string prevstate)
        {
#if LOG_BOSS_AI
            if (LogAI)
                log.Info("OnCombatApproachEnter");
#endif
        }
        protected virtual void OnCombatApproachLeave()
        {

        }

        protected virtual void OnCombatApproachUpdate(long dt)
        {
        }
        #endregion

        #region CombatExecute State
        protected virtual void OnCombatExecuteEnter(string prevstate)
        {
#if LOG_BOSS_AI
            if (LogAI)
                log.Info("OnCombatExecuteEnter");
#endif
        }
        protected virtual void OnCombatExecuteLeave()
        {

        }

        protected virtual void OnCombatExecuteUpdate(long dt)
        {
        }
        #endregion

        #region ResolveOverlap State
        protected virtual void OnResolveOverlapEnter(string prevstate)
        {
#if LOG_BOSS_AI
            if (LogAI)
                log.Info("OnResolveOverlapEnter");
#endif
        }
        protected virtual void OnResolveOverlapLeave()
        {

        }

        protected virtual void OnResolveOverlapUpdate(long dt)
        {
        }
        #endregion

        #region Goback State
        protected virtual void OnGobackEnter(string prevstate)
        {
#if LOG_BOSS_AI
            if (LogAI)
                log.Info("OnGobackEnter");
#endif
        }
        protected virtual void OnGobackLeave()
        {

        }

        protected virtual void OnGobackUpdate(long dt)
        {
        }
        #endregion

        public virtual void OnAttacked(IActor attacker, int aggro) { }

        public virtual void OnGroupAggro(IActor attacker, int aggro) { }
        public virtual void OnKilled() { }        
    }

    public class Threat
    {
        public Actor actor;
        public int aggro;
        public Threat(Actor actor, int aggro)
        {
            this.actor = actor;
            this.aggro = aggro;
        }
    }

    public class MonsterAIBehaviour : BaseAIBehaviour
    {
        public const int ROAM_COOLDOWN_TIME = 12000; //in msec
        
        protected int mLastRoam;
        protected bool mCanRoam;        
        protected int mCurrentRoamCoolDown;
        protected bool mCanPathFind;
        protected bool mGroupAggro;
        protected Monster mMonster;
        protected MonsterSpawnerBase mSpawner; 
        protected Actor mTarget;
        protected Actor mHighestThreatAttacker;

        private int mThreatScanCount;
        protected Vector3 mLastResolveOverlappedPos;
        //protected int mSkillIDToExecute;
        protected float mSkillRange;
        protected SkillData mSkillToExecute;
        protected int mSkillIndexToExecute;
        protected List<NPCSkillCondition> mSkillConditions;
        protected Dictionary<int, Threat> mThreats;
        protected bool mGoingBack;
        
        protected long mBasicAttackStartTime;
        protected long mBasicAttackCooldown;        
        protected long mSkillGCDEnd;        
        protected long[] mSkillCDEnd;

        protected bool mHasMoved;

        public MonsterAIBehaviour(Monster monster):base(monster)
        {            
            AddState("Roam", OnRoamEnter, OnRoamLeave, OnRoamUpdate);

            mMonster = monster;
            mSpawner = monster.mSp;
            mCanRoam = mSpawner.CanRoam(); //monsters in world can roam and roam y position is based off spawner y position        
            mCanPathFind = mSpawner.CanPathFind(); //Only certain monsters can path find
            mGroupAggro = mSpawner.IsGroupAggro();
            mLastResolveOverlappedPos = new Vector3(0, -300, 0);
            mTarget = null;
            mHighestThreatAttacker = null;
            mSkillToExecute = null;
            mSkillIndexToExecute = -1;
            mSkillConditions = NPCSkillsRepo.GetSkillConditions(mMonster.mArchetype.id);
            mThreats = new Dictionary<int, Threat>();

            mBasicAttackStartTime = 0;
            mBasicAttackCooldown = 1000;
            if (mSkillConditions != null)
            {
                int totalSkills = mSkillConditions.Count;
                if (totalSkills > 0)
                {                    
                    mSkillGCDEnd = 0;                 
                    mSkillCDEnd = new long[totalSkills];
                    for (int i =0;i<totalSkills;i++)
                    {                        
                        mSkillCDEnd[i] = 0;
                    }
                }
            }

            mHasMoved = false;
        }

        protected void SwitchTarget(Actor target)
        {
            if (mTarget != null)
            {
                mTarget.RemoveNPCAttacker(mMonster); //monster is no longer seeking this target
                mMonster.PlayerStats.TargetPID = 0;
            }

            mTarget = target;
            if (mTarget != null)
            {
                mTarget.AddNPCAttacker(mMonster);
                mMonster.PlayerStats.TargetPID = mTarget.GetPersistentID();
            }
        }

        protected override void OnIdleEnter(string prevstate)
        {
            base.OnIdleEnter(prevstate);
            mLastRoam = 0;
            mHasMoved = false;
            if (mMonster.PlayerStats.InvincibleCtl)
                mMonster.PlayerStats.InvincibleCtl = false;
            //handle recover from knockedback
            if (mTarget != null && currentState.Name != "Stun" ) //TODO: handle Root status
            {
                SwitchTarget(mTarget);
                ResetSkillToExecute();
                GotoState("CombatApproach");
                return;
            }
        }
        
        protected override void OnIdleUpdate(long dt)
        {
            
            if (mMonster.IsAggressive())
            {
                Actor threat = ThreatScan(2); //scan once every 2 updates
                if (threat != null)
                {
                    AddThreat(threat.GetPersistentID(), threat, 0);
                    SwitchTarget(threat);
                    ResetSkillToExecute();
                    GotoState("CombatApproach");
                    return;
                }
            }
            
            if (mCanRoam)
            {
                mLastRoam += (int)dt;                                
                if (mLastRoam > mCurrentRoamCoolDown)
                {                    
                    int pid = mMonster.GetPersistentID();
                    if (mMonster.mInstance.CanMonsterRoam(pid)) //every 20 ticks (of 50msec each) allow monsters of out of 10 groups to roam. If level has 200 monsters, then each group will be about 20 monsters.
                    {
                        if (mMonster.mInstance.HasCPUResourceToRoam())
                        {
                            //System.Diagnostics.Debug.WriteLine(pid + " roam " + mMonster.mInstance.mCurrentLevelID);
                            GotoState("Roam");
                        }
                    }
                }
            }
        }

        #region RecoverFromKnockBack state
        protected override void OnRecoverFromKnockedBackEnter(string prevstate)
        {
            base.OnRecoverFromKnockedBackEnter(prevstate);
        }

        protected override void OnRecoverFromKnockedBackUpdate(long dt)
        {
            
            if (!IsInCombatRadius(mMonster.Position))
            {
                if (mMonster.mInstance.HasCPUResourceToGoBack())
                {
                    GotoState("Goback");
                }
                return;
            } 
            //base.OnRecoverFromKnockBackUpdate(dt); 
            if (mTarget != null)
            {
                if (mMonster.HasControlStatus(ControlSEType.Stun) || mMonster.HasControlStatus(ControlSEType.Stun))
                    return;
                ACTIONTYPE atype = mMonster.GetActionCmd().GetActionType();
                if (atype == ACTIONTYPE.KNOCKEDBACK || atype == ACTIONTYPE.KNOCKEDUP)
                    return;
                if (IsTargetInRange())
                {         
                    if (mHasMoved && CanSpreadOut())
                    {
                        GotoState("ResolveOverlap");
                    }
                    else
                    {
                        GotoState("CombatExecute");
                    }
                }
                else
                {
                    //Approach if out of range  //the current AI state can only be KnockedBack
                    if (!mMonster.HasControlStatus(ControlSEType.Root) )
                    {
                        GotoState("CombatApproach");//this may cause monster to go out of combat if knocked back too far, in order to aviod this, make sure the there is some buffer for the combat radius and the actual roaming radius, so that monster won't be knocked back by someone outside combat radius. yuning.
                    }
                }
            }
            
        }
        #endregion

        #region Roam State
        protected virtual void OnRoamEnter(string prevstate)
        {
            #if LOG_BOSS_AI
                    if (LogAI) log.Info("OnRoamEnter");
            #endif
            //Determine roam point          
            Vector3 randomPos = GameUtils.RandomPos(mSpawner.GetPos(), mSpawner.GetRoamRadius()); //assume height within combat radius of spawn position is "safe"
            //log.InfoFormat("monster pid {0} roaming to {1}", mMonster.GetPersistentID(), randomPos.ToString());
            
            mMonster.MoveTo(randomPos, true);
        }

        protected virtual void OnRoamLeave()
        {
            mLastRoam = 0;            
            mCurrentRoamCoolDown = ROAM_COOLDOWN_TIME + GameUtils.RandomInt(0, 6000);
        }

        protected virtual void OnRoamUpdate(long dt)
        {                       
            if (!mMonster.IsMoving())
            {
                GotoState("Idle");
            }
        }
        #endregion


        protected Actor ThreatScan(int tickcount)
        {
            Actor threat = null;
            if (mThreatScanCount % tickcount == 0)
            {
                threat = mMonster.QueryForThreat();
                mThreatScanCount = 0;
            }
            mThreatScanCount++;
            return threat;
        }

        protected bool IsTargetInRange()
        {
            return GameUtils.InRange(mMonster.Position, mTarget.Position,mSkillRange, mTarget.Radius);
        }

        protected bool IsInCombatRadius(Vector3 pos)
        {
            return GameUtils.InRange(mSpawner.GetPos(), pos, mSpawner.GetCombatRadius()+ 3.0f);
        }

        protected void ResetSkillToExecute()
        {
            //mSkillIDToExecute = -1;
            mSkillToExecute = null;
            mSkillIndexToExecute = -1;
        }

        protected void DetermineSkillToExecute()
        {
            bool hasSkill = false;
            SkillData skilldata = null;
            
            if (mSkillConditions != null && !mMonster.HasControlStatus(ControlSEType.Silence))
            {
                long now = mMonster.EntitySystem.Timers.GetSynchronizedTime();
                if (now >= mSkillGCDEnd) //global cooldown
                {
                    System.Random random = GameUtils.GetRandomGenerator();
                    //Pick a skill                
                    int index = 0;
                    foreach(NPCSkillCondition cond in mSkillConditions)
                    {                                            
                        if (now < mSkillCDEnd[index]) //still cooling down
                            continue;

                        if (random.NextDouble() * 100 <= cond.chance)
                        {
                            //mSkillIDToExecute = cond.skilldata.skillJson.id;
                            skilldata = cond.skilldata;
                            mSkillToExecute = skilldata;
                            mSkillIndexToExecute = index;
                            hasSkill = true;
                            break;
                        }
                        index++;
                    }
                }
            }

            if (!hasSkill)
            {
                //mSkillIDToExecute = Monster.BASIC_ATTACK_SKILLID;
                //skilldata = SkillRepo.GetSkill(mSkillIDToExecute);
                //if (mMonster.mArchetype.basicattack > 0)
                //{
                    skilldata = SkillRepo.GetSkill(mMonster.mArchetype.basicattack);
                //}
                //else
                //   skilldata = SkillRepo.mMonsterBasicAttack;
                mSkillToExecute = skilldata;
                mSkillIndexToExecute = -1;

                //Peter, TODO: basic attack also have cooldown and cooldown can differ for different monster    
                //We will not check basic attack cooldown here to allow at least 1 skill and its range to refer to e.g. when approaching
            }            
            
            SkillGroupJson skillgroupJson = skilldata.skillgroupJson;

            //Check if skill is friendly, it is assumed monsters/boss would not have any friendly skills
            if ((TargetType)skillgroupJson.targettype != TargetType.Enemy)
            {
                //Revert to basic attack
                log.Info("Warning!! Monster archetype id: " + mMonster.mArchetype.id + " has unsupported friendly skill: " + mSkillToExecute.skillJson.id);
                //mSkillIDToExecute = Monster.BASIC_ATTACK_SKILLID;
                //skilldata = SkillRepo.GetSkill(mSkillIDToExecute);
                //skillgroupJson = skilldata.skillgroupJson;
                //skilldata = SkillRepo.mMonsterBasicAttack;
                //mSkillToExecute = skilldata;
                //mSkillIndexToExecute = -1;
            }
            
            if ((Threatzone)skillgroupJson.threatzone == Threatzone.LongStream)
                mSkillRange = mSkillToExecute.skillJson.range;
            else if(skillgroupJson.threatzone == Threatzone.Single)
                mSkillRange = 2.0f;
            else
                mSkillRange = mSkillToExecute.skillJson.radius; //120,360,single 
        }

        protected void ApproachTarget()
        {
            mHasMoved = true;

            PositionSlots slots = mTarget.PositionSlots;
            slots.DeallocateSlot(mMonster);

            //Approach action is supposed to be a black box and will help the monster get nearer to its target  
            //Note: preferredRange should be at least 0.5m bigger than attacker radius      
            if (mCanPathFind)
                mMonster.ApproachTargetWithPathFind(mTarget.GetPersistentID(), null, mSkillRange - 0.5f, true, false); //-0.5f so that it does not toggle at bordercase
            else
                mMonster.ApproachTarget(mTarget.GetPersistentID(), mSkillRange - 0.5f); //-0.5f so that it does not toggle at bordercase
        }
        //todo:how AIBehaviour work together with knockedback
       
        
        protected bool IsOverlapping()
        {
            Dictionary<int, Actor> attackers = mTarget.GetNPCAttackers();
            Vector3 pos = mMonster.Position;
            float radius = mMonster.Radius;
            int count = 0;
            bool overlap = false;

            foreach (KeyValuePair<int, Actor> kvp in attackers)
            {
                Actor attacker = kvp.Value;
                if (attacker == null || !attacker.IsAlive())
                    continue;

                if (attacker != mMonster /*&& !attacker.IsPerformingApproach()*/)//if the other attacker is not approaching target entity
                {
                    float sqdist = (attacker.Position - pos).sqrMagnitude;
                    float combinedRadii = attacker.Radius + radius;
                    if (sqdist <= 0.64f * combinedRadii * combinedRadii) // >20% overlapped
                    {
                        overlap = true;
                        break;
                    }
                }
                count++;
                if (count > 8)
                {
                    overlap = false;
                    break;
                }
            }
            return overlap;
        }

        protected bool CanSpreadOut()
        {
            PositionSlots slots = mTarget.PositionSlots;
            if (slots.IsAttackerInSlots(mMonster)) //Already in position, don't need to move
                return false;

            if (!slots.HasAvailableSlot())
                return false;

            if (mMonster.HasControlStatus(ControlSEType.Root) || mMonster.HasControlStatus(ControlSEType.Stun))
                return false;

            return true;
        }


        #region Combat State        
        protected override void OnCombatApproachEnter(string prevstate)
        {
            //if (mSkillIDToExecute < 0)
            if (mSkillToExecute == null)
                DetermineSkillToExecute(); //At the least, there will always be a basic attack available to use
        }

        //protected override void OnCombatApproachLeave() {}
        
        protected override void OnCombatApproachUpdate(long dt)
        {            
            //Determine if target still valid
            if (!CheckTargetValid())
                return;

            if (!IsInCombatRadius(mMonster.Position)) 
            {
                if (mMonster.mInstance.HasCPUResourceToGoBack())
                {
                    GotoState("Goback");
                }                
                return;
            }

            
            Zealot.Common.Actions.Action action = mMonster.GetAction();    

            //Determine if target is within range
            //Attack if within range
            
            if (IsTargetInRange()) 
            {
                //definitely will be either idle or approach or walk here

                //Check overlapping on reach target. Those that are already attacking do not need to move to resolve overlap.                
                if (mHasMoved && CanSpreadOut())
                {                    
                    GotoState("ResolveOverlap");
                }
                else
                {
                    GotoState("CombatExecute");
                }
            }
            else //Out of range, either idling or still approaching
            {
                //Approach if out of range (include path find) and not already doing pathfind (note that monster can idle while waiting for pathfind result)
                if (action.mdbCommand.GetActionType() == ACTIONTYPE.IDLE && !mMonster.IsPerformingApproach() &&
                    !mMonster.HasControlStatus(ControlSEType.Root))
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
            base.OnCombatExecuteEnter(prevstate);                
            TryCastSkill();
        }

        //protected override void OnCombatExecuteLeave() { }

        protected override void OnCombatExecuteUpdate(long dt)
        {            
            if (mMonster.GetActionCmd().GetActionType() != ACTIONTYPE.CASTSKILL) //if has stopped attacking
            {
                ResetSkillToExecute(); //prepare to decide next skill

                if (!CheckTargetValid())
                    return;

                //just finished last execution
                DetermineSkillToExecute();                   

                if (IsTargetInRange())
                {
                    TryCastSkill();
                }
                else
                {
                    if (!mMonster.HasControlStatus(ControlSEType.Root))
                    {
                        GotoState("CombatApproach");
                        ApproachTarget();
                    }
                }
            }
        }

        protected void TryCastSkill()
        {
            //if (mMonster.HasControlStatus(ControlSEType.Disarmed))
            //    return;
            //do a simply action interruption check here to prevent enter into cooldown but failed cast.
            //Common.Actions.Action action = mMonster.GetAction();
            //if (action != null)
            //{
            //    ACTIONTYPE currentAction = action.mdbCommand.GetActionType();
            //    if (!AuthoASInterruptManager.CanPerformAction(currentAction, ACTIONTYPE.CASTSKILL))
            //        return;
            //}
            
            //Basic Attack might still be in cooldown here
            long now = mMonster.EntitySystem.Timers.GetSynchronizedTime();
            if (now < mActor.RecoverTime + CombatUtils.RECOVER_FROM_HIT_TIME)
                return;
            if (mSkillToExecute == null)
                return;
            if (mSkillToExecute.skillgroupJson.skilltype == SkillType.BasicAttack)
            {                 
                if (now - mBasicAttackStartTime < mBasicAttackCooldown)
                {
                    return;
                }
                mBasicAttackCooldown = (long)(1000 * mSkillToExecute.skillJson.cooldown);
                mBasicAttackCooldown = mBasicAttackCooldown < 500 ? 500 : mBasicAttackCooldown;
                mBasicAttackStartTime = now;
            }
            else
            {
                //Skill being cast definitely not in cooldown here, can safely cast                
                mSkillCDEnd[mSkillIndexToExecute] = now + (long) (mSkillToExecute.skillJson.cooldown * 1000);
                if (mSkillToExecute.skillJson.globalcd > 0)
                {                    
                    mSkillGCDEnd = now + (long)(mSkillToExecute.skillJson.globalcd * 1000);
                }
            }
            mMonster.CastSkill(mSkillToExecute.skillJson.id, mTarget.GetPersistentID(), mTarget.Position);
#if LOG_BOSS_AI
            if (LogAI) log.Info("cast skill" + mSkillToExecute.skillJson.id);
#endif
        }
        #endregion

        #region ResolveOverlap State
        protected override void OnResolveOverlapEnter(string prevstate)
        {
            base.OnResolveOverlapEnter(prevstate);
            PositionSlots slots = mTarget.PositionSlots;
            //slots.DeallocateSlot(mMonster);            
            //if (!slots.HasAvailableSlot())
            //{
            //    GotoState("CombatExecute");
            //    return;
            //}

            Vector3? desiredPos = slots.AllocateEmptySlot(mMonster, mSkillRange - 0.5f);
            if (desiredPos != null)
            {
                //Peter, TODO: if monster can't move, immediately attack instead of moving to resolve overlap
                if (!IsInCombatRadius((Vector3)desiredPos) || (mLastResolveOverlappedPos - (Vector3)desiredPos).sqrMagnitude < 0.1f
                    || (mMonster.Position - (Vector3)desiredPos).sqrMagnitude < 1.0f)
                {
                    GotoState("CombatExecute");
                    return;
                }

                //if (IsOverlapping()) //By checking overlapping here, it implies that attackers that reach earlier will not move to resolve and may subsequently overlap with later attackers
                if (mTarget.GetNPCAttackers().Count > 1) //if more than 1 attacker than we spread
                {
                    if (mCanPathFind)
                        mMonster.ApproachTargetWithPathFind(-1, desiredPos, 0.0f, false, true); //It's possible that the desiredpos is invalid. Action will complete immediately in this case.
                    else
                        mMonster.MoveTo((Vector3)desiredPos);
                    mLastResolveOverlappedPos = (Vector3)desiredPos;
                }                
            }
            else
            {
                GotoState("CombatExecute");
            }            
        }

        protected override void OnResolveOverlapLeave()
        {            
        }

        protected override void OnResolveOverlapUpdate(long dt)
        {
            if (!mMonster.IsMoving()) //Path completed, Peter, TODO: next time there will be stun which cause monster unable to move...
            {
                GotoState("CombatApproach");
            }
        }

        #endregion

        protected bool IsTargetInvalid(Actor target)
        {
            return  CombatUtils.IsInvalidTarget(target) || !IsInCombatRadius(target.Position);
        }

        protected virtual bool CheckTargetValid()
        {
            if (mMonster.mArchetype.monstertype == MonsterType.Boss)
            {
                if (mHighestThreatAttacker != null)//boss attack the highestThreat attacker
                {
                    SwitchTarget(mHighestThreatAttacker);
                    ResetSkillToExecute();
                    DetermineSkillToExecute();
                }
            }

            if (IsTargetInvalid(mTarget))
            {
                if (mTarget != null)
                {
                    mThreats.Remove(mTarget.GetPersistentID());
                    if (mHighestThreatAttacker == mTarget)                    
                        mHighestThreatAttacker = null;                    
                }
                SwitchTarget(null);

                List<int> removeList = new List<int>();
                foreach(KeyValuePair<int, Threat> entry in mThreats) 
                {
                    int pid = entry.Key;
                    Actor potentialTarget = entry.Value.actor;
                    if (IsTargetInvalid(potentialTarget)) //Take this opportunity to remove all invalid targets
                    {
                        removeList.Add(pid);      
                        if (mHighestThreatAttacker != null && potentialTarget == mHighestThreatAttacker)
                            mHighestThreatAttacker = null;
                    }
                    else if (mTarget==null)
                    {
                        SwitchTarget(potentialTarget);
                        ResetSkillToExecute();
                    }                    
                }                

                foreach (int pid in removeList)
                    mThreats.Remove(pid);

                if (mTarget == null)                                    
                    GotoState("Goback");                
                else                                 
                    GotoState("CombatApproach");                
                return false;
            }
            return true;
        }


        #region Goback State

        protected void GoBackToSafePoint()
        {            
            mGoingBack = true;
            Vector3 randomPos = mSpawnPos;
            if (mCanPathFind)
                mMonster.ApproachTargetWithPathFind(-1, randomPos, 0, true, false);
            else
                mMonster.MoveTo(randomPos);   
        }

        protected override void OnGobackEnter(string prevstate)
        {
            base.OnGobackEnter(prevstate);
            mMonster.PlayerStats.InvincibleCtl = true;
            mHighestThreatAttacker = null;
            mThreats.Clear();
            SwitchTarget(null);
            mGoingBack = false;
            if (mMonster.mInstance.HasCPUResourceToGoBack())            
                GoBackToSafePoint();            
        }        

        protected override void OnGobackUpdate(long dt)
        {
            //TODO: might want to check for aggro while going back... 
           
            if(!mGoingBack)
            {
                if (mMonster.mInstance.HasCPUResourceToGoBack())
                    GoBackToSafePoint();
                return;
            }

            if (!mMonster.IsPerformingApproach())
            {
                if (mMonster.mArchetype.recoveronreturn)
                    mMonster.SetHealth(mMonster.GetHealthMax());
                                
                //mMonster.StopAllSideEffects();
                mMonster.ResetDamageRecords();
                GotoState("Idle");
            }
        }
        #endregion        

        #region stun state
        protected override void OnStunEnter(string prevstate)
        {
            base.OnStunEnter(prevstate);
            mMonster.Idle();
        }

        protected override void OnStunUpdate(long dt)
        {
            if (!mActor.HasControlStatus(ControlSEType.Stun) && !mActor.HasControlStatus(ControlSEType.Stun))
            {
                GotoState("CombatApproach");
            }
        }

        protected override void OnStunLeave()
        {
            base.OnStunLeave();
        }
        #endregion

        #region Frozen state
        protected override void OnFrozenEnter(string prevstate)
        {
            base.OnFrozenEnter(prevstate);
            mMonster.Idle();
        }

        protected override void OnFrozenUpdate(long dt)
        {
            if (!mActor.HasControlStatus(ControlSEType.Freeze) && !mActor.HasControlStatus(ControlSEType.Stun))
            {
                GotoState("CombatApproach");
            }
        }

        protected override void OnFrozenLeave()
        {
            base.OnFrozenLeave();
        }
        #endregion

        public void AddThreat(int attackerPID, Actor attackerActor, int aggro)
        {
            int accAggro;
            if (mThreats.ContainsKey(attackerPID))
            {
                mThreats[attackerPID].aggro += aggro;
                accAggro = mThreats[attackerPID].aggro;
            }
            else
            {
                mThreats.Add(attackerPID, new Threat(attackerActor, aggro));
                accAggro = aggro;
            }
            
            if (mHighestThreatAttacker != null)
            {
                int highestAggro = mThreats[mHighestThreatAttacker.GetPersistentID()].aggro;
                if (accAggro >= highestAggro && attackerActor != mHighestThreatAttacker)                
                    mHighestThreatAttacker = attackerActor;
            }
            else
            {
                mHighestThreatAttacker = attackerActor;
            }
        }

        private bool OnNormalAIAttacked(IActor attacker, int aggro)
        {
            //We are not having an aggro system right now. Monster will attack the very first target only.            
            Actor attackerActor = attacker as Actor;//Currently, we handle only for actor. Maybe, there could be other forms of IActor in the future e.g. StaticActor?
            if (attackerActor == null)
                return false;

            int attackerPID = attackerActor.GetPersistentID();
            AddThreat(attackerPID, attackerActor, aggro);

            bool shouldSwitchToPlayer = false; 
            if (mActor.IsMonster())//monster can switch from monster target to player target.
            {
                if (mTarget!= null && mTarget.IsMonster() && attackerActor.IsPlayer())
                {
                    shouldSwitchToPlayer = true;
                }
            }           
            string currentStateName = GetCurrentStateName();
            if (!shouldSwitchToPlayer && (currentStateName == "CombatApproach" || currentStateName == "CombatExecute" || currentStateName == "Goback"))
                return false;

            SwitchTarget(attackerActor); //The first attacker will be targeted
            ResetSkillToExecute();

            if (!mMonster.HasControlStatus(ControlSEType.Stun) && !mMonster.HasControlStatus(ControlSEType.Stun)) //if not in stun state
                GotoState("CombatApproach");
            return true;
        }

        
        public override void OnAttacked(IActor attacker, int aggro) //Only boss will attack target that is top of its aggro list
        {
            if (mGroupAggro)
            {
                string currentStateName = GetCurrentStateName();
                if ((currentStateName == "Idle" || currentStateName == "Roam"))
                    mSpawner.GroupAggro(mActor.GetPersistentID(), attacker);//call other monster to check if they attack also.
            } 
            if (!OnNormalAIAttacked(attacker, aggro))
            return;
            
        }

        public override void OnGroupAggro(IActor attacker, int aggro)
        {
            OnNormalAIAttacked(attacker, aggro);
        }

        public override void OnKilled() 
        {
            SwitchTarget(null);
            mHighestThreatAttacker = null;
            mThreats.Clear();
        }        
    }

    public class GoblinAIBehaviour : BaseAIBehaviour
    {
        //todo: Goblin follow path, no attack, on path end call ((GoblinSpawner)monster.mSp).OnPathCompleted.
        protected Monster mMonster;
        protected GoblinSpawner mSpawner; 
        private PathStraightJson mPath;
        private int mNextNode = 0;

        private float mDistPerMove; //Recommended 4m with 3 sec pause and spawner spawn rate 5sec
        private long mPausePerMove; //in msec
        private Vector3 mNextShortTermPos;
        private bool mNextShortTermPosIsWaypoint;
        private long mPauseElapsed;

        public GoblinAIBehaviour(Monster monster)
            : base(monster)
        {
            

            mMonster = monster;
            mSpawner = (GoblinSpawner)monster.mSp;
            mPath = mSpawner.mPath;
            
            mDistPerMove = mSpawner.mGoblinSpawnerJson.distPerMove;
            mPausePerMove = mSpawner.mGoblinSpawnerJson.pausePerMove;

            mNextShortTermPosIsWaypoint = false;
            mNextNode = 0;

            GotoState("FollowPath");
        }

        protected override void InitStates()
        {
            AddState("FollowPath", OnFollowPathEnter, OnFollowPathLeave, OnFollowPathUpdate);
            AddState("Pause", OnPauseEnter, OnPauseLeave, OnPauseUpdate);
            base.InitStates();
        }

        public override void StartMonitoring()
        {
            GotoState("FollowPath");
        }

        #region FollowPath State
        protected virtual void OnFollowPathEnter(string prevstate)
        {
            //log.InfoFormat("monster pid {0} roaming to {1}", mMonster.GetPersistentID(), randomPos.ToString())            
            MoveToNextPos();
        }

        protected void MoveToNextPos()
        {
            if (mMonster.HasControlStatus(ControlSEType.Root))
                return;

            Vector3 nextWaypoint = mPath.nodes[mNextNode];

            Vector3 dir = (nextWaypoint - mMonster.Position);
            float dist = dir.magnitude;
            if (dist > mDistPerMove)
            {
                dir = dir / dist;
                mNextShortTermPos = mMonster.Position + dir * mDistPerMove;
                mNextShortTermPosIsWaypoint = false;
            }
            else
            {
                mNextShortTermPos = nextWaypoint;
                mNextShortTermPosIsWaypoint = true;
            }

            mMonster.MoveTo(mNextShortTermPos);
        }
       
        protected virtual void OnFollowPathLeave() {}

        protected virtual void OnFollowPathUpdate(long dt)
        {
            if (mMonster.HasControlStatus(ControlSEType.Root))
                return;

            if (!mMonster.IsMoving())
            {
                if ((mMonster.Position - mNextShortTermPos).sqrMagnitude < 0.16f) //if has reached and not stopped by stun,etc
                {                    
                    if (mNextShortTermPosIsWaypoint)
                    {   //Check if waypoint is reached before incrementing
                        mNextNode++;                        
                        if (mNextNode >= mPath.nodes.Length)
                        {
                            mSpawner.OnPathCompleted(mMonster);
                            return;
                        }
                    }

                    GotoState("Pause");
                }    
                else
                {   
                    MoveToNextPos(); //It has stopped due to root, we resume it here
                }
            }
        }
        #endregion

        #region Pause State
        protected virtual void OnPauseEnter(string prevstate)
        {
            mPauseElapsed = 0;
        }

        protected virtual void OnPauseLeave() {}
        protected virtual void OnPauseUpdate(long dt)
        {
            mPauseElapsed += dt;
            if (mPauseElapsed > mPausePerMove)
            {                
                GotoState("FollowPath");
            }
        }
        #endregion

        #region stun state
        protected override void OnStunEnter(string prevstate)
        {
            base.OnStunEnter(prevstate);
            mMonster.Idle();
        }

        protected override void OnStunUpdate(long dt)
        {
            if (!mActor.HasControlStatus(ControlSEType.Stun))
            {
                GotoState("FollowPath");
            }
        }
        #endregion
    }

    public class NullAIBehaviour : BaseAIBehaviour
    {
        public NullAIBehaviour(Actor actor) : base (actor)
        {
        }

        protected override void InitStates()
        {
             
        }
        public override void StartMonitoring()
        {
        }
        public override void OnUpdate(long dt)
        {
        }
    }
}
