namespace Zealot.Server.Entities
{
    using System.Collections.Generic;
    using Zealot.Common;
    using Zealot.Common.Entities;
    using Zealot.Common.RPC;
    using Photon.LoadBalancing.GameServer;
    using Zealot.Server.AI;
    using Zealot.Server.Actions;
    using Zealot.Common.Actions;
    using Zealot.Server.SideEffects;
    using Repository;
    using ExitGames.Logging;
    using Photon.LoadBalancing.Entities;
    using UnityEngine;

    public class AIPlayer : ComboSkillCaster
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public PlayerSynStats PlayerSynStats { get; set; }
        public EquipmentStats EquipmentStats { get; set; } //only used to render equipment at client. 
        public LocalCombatStats LocalCombatStats { get; set; }  //will be set to snapshot of final values. This is not sync to client. Only required by server for combat formula computation.
        public SkillSynStats SkillStats { get; set; }        

        private long elapsedDT;
        protected BaseAIBehaviour mAIController;

        private long[] mSkillCDEnd;
        private long[] mSkillCDDur;
        private long mSkillGCDEnd; 

        public AIPlayer() : base()
        {
            this.EntityType = EntityType.AIPlayer;
            elapsedDT = 0;            

            mSkillCDEnd = new long[4]; //AI Player only have fixed 5 styles to use and will not be able to change
            mSkillCDDur = new long[4]; //AI Player only have fixed 5 styles to use and will not be able to change
            mSkillGCDEnd = 0;
        }

        #region Implement abstract methods
        public override void SpawnAtClient(GameClientPeer peer)
        {
#if DEBUG
            log.InfoFormat("Spawn AIPlayer [{0}] to {1}", GetPersistentID(), peer.mChar);
#endif
            peer.ZRPC.CombatRPC.SpawnPlayerEntity(false, mnOwnerID, PlayerSynStats.name, mnPersistentID,
                                             ((PlayerSynStats)PlayerStats).jobsect, ((PlayerSynStats)PlayerStats).Gender, ((PlayerSynStats)PlayerStats).MountID, Position.ToRPCPosition(), Forward.ToRPCDirection(), GetHealth(), GetHealthMax(), peer);            
        }       
        #endregion

        public void SetAIBehaviour(BaseAIBehaviour behaviour)
        {
            mAIController = behaviour;
            mAIController.StartMonitoring();
        }

        public void DebugSendCombatStats(Player player)
        {
            GameClientPeer slot = player.Slot;
            player.AddToChatMessageQueue(new ChatMessage(MessageType.System,
                                         "*** CombatStats of " + Name + " Start ***", slot.mChar));

            for (int i = 0; i < (int)FieldName.LastField; ++i)
            {
                FieldName currFieldName = (FieldName)i;
                object val = CombatStats.GetField(currFieldName);
                string desc = currFieldName.ToString() + " = " + val.ToString();
                player.AddToChatMessageQueue(new ChatMessage(MessageType.System, desc, slot.mChar));
            }
            player.AddToChatMessageQueue(new ChatMessage(MessageType.System, "MoveSpeed = " + PlayerStats.MoveSpeed, player.Slot.mChar));

            player.AddToChatMessageQueue(new ChatMessage(MessageType.System,
                                         "*** CombatStats of " + Name + " End ***", slot.mChar));
        }

        public override void OnKilled(IActor attacker)
        {
            base.OnKilled(attacker);

            if (mAIController!= null)
                mAIController.OnKilled();
            PerformAction(new ServerAuthoASDead(this, new DeadActionCommand()));
            RealmControllerArena controller = mInstance.mRealmController as RealmControllerArena;
            if (controller != null)
                controller.OnAIPlayerDead(attacker as Player);
        }

        public override void Update(long dt)
        {
            base.Update(dt);
            elapsedDT += dt;
            if (elapsedDT > 300) //faster update rate than monster because aiplayer need to chain basic attack at 0.3sec
            {
                if (mAIController != null)
                    mAIController.OnUpdate(elapsedDT);
                elapsedDT = 0;
            }
        }
                
        public void InitEquipInvStats(EquipmentInventoryData eqInvData)
        {
            //for (int i = 0; i < (int)EquipmentSlot.MAXSLOTS; i++)
            //{
            //    if(eqinvdata.Slots[i] != null)
            //    {
            //        var eqitem = eqinvdata.Slots[i] as EquipItem;
            //        eqitem.EncodeItem();
            //        EquipInvStats.equipinventory[i] = eqitem.GetItemCodeForLocalObj();
            //    }
            //}
        }
        
        public int GetAccumulatedLevel()
        {
            return PlayerSynStats.Level;
        }

        public override bool IsInvalidTarget()
        {
            return !IsAlive(); //Do not check safezone because AI player's localcombatstats-safezone is never updated and it is only used in Arena which do not have safe zone
        }

        public override bool IsInSafeZone()
        {
            return false;
        }

        public bool IsSkillCooldown(int skillindex)
        {
            return EntitySystem.Timers.GetSynchronizedTime() < mSkillCDEnd[skillindex];
        }

        public override PlayerSynStats GetPlayerStats()
        {
            return PlayerSynStats;
        }

        public void SetSkillCDEnd(int skillindex, float cooldown)
        {
            long now = EntitySystem.Timers.GetSynchronizedTime(); 
            float endtime = CombatUtils.GetFinalSkillCooldown(SkillPassiveStats, skillindex, cooldown);
            mSkillCDDur[skillindex] = (long)(1000 * endtime);
            mSkillCDEnd[skillindex] = now + mSkillCDDur[skillindex];
        }
        
        public void IncreaseSkillCD(int[] list, float perc)
        {
            long now = EntitySystem.Timers.GetSynchronizedTime();
            foreach (int idx in list)
            {
                if(now < mSkillCDEnd[idx])
                {  
                    //percentage increase based on the orginal skill cooldown duration. same as client.
                    mSkillCDEnd[idx] += (long)(mSkillCDDur[idx] *  perc * 0.01f);
                }
            }
        }

        public void SetSkillGCDEnd(long cooldown)
        {
            
            mSkillGCDEnd = cooldown;
        }

        public bool IsSkillGlobalCoolDown()
        {
            return EntitySystem.Timers.GetSynchronizedTime() < mSkillGCDEnd;
        }

        public bool HasPositiveSE(int otherSEID)
        {            
            for (int i = 0; i < mSideEffectsPos.Length; i++)
            {
                SideEffect se = mSideEffectsPos[i];
                if (se == null)
                    continue;

                if (otherSEID == se.mSideeffectData.id)
                    return true;
            }
            return false;
        }      

        public Actor QueryForThreat()
        {
            RealmController controller = mInstance.mRealmController;
            if (controller != null)
            {
                foreach (KeyValuePair<string, Player> entry in controller.mPlayers)
                    return entry.Value;
            }
            return null;              
        }
        
        public override void OnAttacked(IActor attacker, int aggro)
        {
            if (mAIController != null)
                mAIController.OnAttacked(attacker, aggro);
        }

        public override void OnStun()
        {
            base.OnStun();
            mAIController.GotoState("Stun");
        }

        public override void OnFrozen()
        {
            base.OnFrozen();
            mAIController.GotoState("Frozen");
        }

        public override void OnRoot()
        {
            base.OnRoot();
            if (IsMoving())
            {
                Idle();
            }
        }

        public override void SetHealth(int val)
        {
            base.SetHealth(val);
            float newhp = (float)val / GetHealthMax();
            PlayerStats.DisplayHp = newhp;
        }

        
        //----------------------------------------------------------------------------------
        //Available actions performable by AIPlayer:
        public void Idle()
        {
            ServerAuthoASIdle idleAction = new ServerAuthoASIdle(this, new IdleActionCommand());
            PerformAction(idleAction);
        }

        public void DashAttack(int skillid, int targetPID)
        {
            SkillData sdata = SkillRepo.GetSkillByGroupID(skillid);
            DashAttackCommand cmd = new DashAttackCommand();
            cmd.skillid = skillid;
            cmd.targetpid = targetPID;
            //dash to the target, as in server no collision check.
            Entity target = EntitySystem.GetEntityByPID(targetPID);
            if(target != null)
            {
                Vector3 dir = target.Position - Position; 
                Forward = dir.normalized; //face to target
            }
            cmd.targetpos = Position + Forward * sdata.skillJson.range;
            ServerAuthoDashAttack action = new ServerAuthoDashAttack(this, cmd);
            PerformAction(action);
        }

        public void CastSkill(int skillindex , int skillid, int targetPID)
        {
            if (IsInRT())
                return;
            CastSkillCommand cmd = new CastSkillCommand();
            int skillLvl = 1;
                 
            cmd.skillid = skillid;
            cmd.targetpid = targetPID;
            cmd.skilllevel = skillLvl;
            ServerAuthoCastSkill action = new ServerAuthoCastSkill(this, cmd);
            action.SetCompleteCallback(Idle);
            PerformAction(action);
        }

        public void WalkAndCastSkill(int skillid, int targetPID)
        {
            WalkAndCastCommand cmd = new WalkAndCastCommand();
            cmd.skillid = skillid;
            cmd.targetPos = Position;
            cmd.targetpid = targetPID;
            ServerAuthoWalkAndCast action = new ServerAuthoWalkAndCast(this, cmd);
            action.SetCompleteCallback(Idle);
            PerformAction(action);
        }

        public void ApproachTarget(int targetPID, float range)
        {
            ApproachCommand cmd = new ApproachCommand();
            cmd.targetpid = targetPID;
            cmd.range = range;
            ServerAuthoASApproach approachAction = new ServerAuthoASApproach(this, cmd);
            approachAction.SetCompleteCallback(Idle);
            PerformAction(approachAction);
        }

        public override void onDragged(Vector3 pos, float dur, float speed)
        {
            DraggedActionCommand cmd = new DraggedActionCommand();
            cmd.pos = pos;
            cmd.dur = dur;
            cmd.speed = speed;
            ASDragged action = new ASDragged(this, cmd);
            action.SetCompleteCallback(() => {
                Idle();
            });
            PerformAction(action);
        }
    }
}
