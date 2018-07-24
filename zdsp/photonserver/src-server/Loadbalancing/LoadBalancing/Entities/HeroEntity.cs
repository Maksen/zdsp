namespace Zealot.Server.Entities
{
    using Kopio.JsonContracts;
    using Photon.LoadBalancing.GameServer;
    using UnityEngine;
    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Common.RPC;
    using Zealot.Repository;
    using Zealot.Server.Actions;
    using Zealot.Server.AI;

    public class HeroEntity : Actor
    {
        public HeroSynStats HeroSynStats { get; set; }
        public Hero Hero { get; private set; }
        public HeroJson HeroData { get; private set; }
        public Player Owner { get; private set; }

        private long elapsedDT;
        protected HeroAIBehaviour mAIController;
        private bool mSummoning;

        public HeroEntity() : base()
        {
            this.EntityType = EntityType.Hero;
            elapsedDT = 0;
        }

        #region Implement abstract methods

        public override bool IsInSafeZone()
        {
            return true; // Hero cannot be attacked
        }

        public override bool IsInvalidTarget()
        {
            return true;  // Hero cannot be attacked
        }

        public override void SpawnAtClient(GameClientPeer peer)
        {
            peer.ZRPC.CombatRPC.SpawnHeroEntity(mnPersistentID, HeroData.heroid, Hero.ModelTier, Owner.GetPersistentID(), Position.ToRPCPosition(), Forward.ToRPCDirection(), peer);
        }

        #endregion Implement abstract methods

        public void Init(Hero hero, Player player, bool summoning)
        {
            Hero = hero;
            HeroData = hero.HeroJson;
            Owner = player;

            SetSpawnPosition(player);

            mSummoning = summoning;

            // Set HeroSynStats
            HeroSynStats.Team = player.PlayerSynStats.Team;
            HeroSynStats.MoveSpeed = HeroData.movespeed;
            HeroSynStats.Level = Hero.Level;
            HeroSynStats.HeroId = hero.HeroId;
            HeroSynStats.ModelTier = hero.ModelTier;

            // Set CombatStats
            PlayerCombatStats heroCombatStats = new PlayerCombatStats();
            heroCombatStats.SetPlayerLocalAndSyncStats(null, HeroSynStats, null);
            CombatStats = heroCombatStats;
            SetCombatStats(hero.Level);
        }

        public void SetSpawnPosition(Player player)
        {
            Position = player.Position - player.Forward * (player.Radius + 0.5f);
            Forward = player.Forward;
        }

        public void CleanUp()
        {
            SetOwnerID(0);
            mInstance.mEntitySystem.RemoveEntityByPID(GetPersistentID());
        }

        public void SetCombatStats(int currentLevel)
        {
            HeroGrowthJson growthData = HeroRepo.GetHeroGrowthData(HeroData.growthgroup, currentLevel);
            if (growthData != null)
            {
                PlayerCombatStats combatStats = (PlayerCombatStats)CombatStats;
                combatStats.SuppressComputeAll = true;
                CombatStats.SetField(FieldName.WeaponAttackBase, HeroData.weaponattack);
                CombatStats.SetField(FieldName.StrengthBase, growthData.strength);
                CombatStats.SetField(FieldName.DexterityBase, growthData.dexterity);
                CombatStats.SetField(FieldName.IntelligenceBase, growthData.intelligence);
                CombatStats.SetField(FieldName.AttackBase, growthData.attackpower);
                CombatStats.SetField(FieldName.AccuracyBase, growthData.accuracy);
                CombatStats.SetField(FieldName.CriticalBase, growthData.critical);
                SetHealthMax(100);
                SetHealth(100);
                combatStats.SuppressComputeAll = false;
                combatStats.ComputeAll();
            }
        }

        public void SetAIBehaviour(HeroAIBehaviour behaviour)
        {
            mAIController = behaviour;
            mAIController.StartMonitoring();
        }

        public void StartAIBehaviour()
        {
            if (mSummoning)
            {
                HeroSynStats.Summoning = true;
                mAIController.GotoState("Summoning");
            }
            else
                mAIController.GotoState("Idle");
        }

        public override void Update(long dt)
        {
            base.Update(dt);

            elapsedDT += dt;
            if (elapsedDT > 300)
            {
                mAIController.OnUpdate(elapsedDT);
                elapsedDT = 0;
            }
        }

        public void OnSkillLevelChanged()
        {
            mAIController.UpdateSkill();
        }

        public override void QueueDmgResult(AttackResult res)
        {
            Owner.QueueDmgResult(res);   //show damage result for player in client
        }

        public Actor QueryForThreat()
        {
            return EntitySystem.QueryForClosestEntityInSphere(this.Position, 10, (queriedEntity) =>
            {
                IActor target = queriedEntity as IActor;
                return (target != null && CombatUtils.IsValidEnemyTarget(this, target));
            }) as Actor;
        }

        //----------------------------------------------------------------------------------
        //Available actions performable by Hero:

        public void Idle()
        {
            ServerAuthoASIdle idleAction = new ServerAuthoASIdle(this, new IdleActionCommand());
            PerformAction(idleAction);
        }

        public void ApproachTargetWithPathFind(int targetPID, Vector3? pos, float range, bool targetposSafe, bool movedirectonpathfound)
        {
            ApproachWithPathFindCommand cmd = new ApproachWithPathFindCommand();
            cmd.targetpid = targetPID;
            cmd.targetpos = pos;
            cmd.range = range;
            cmd.targetposSafe = targetposSafe;
            cmd.movedirectonpathfound = movedirectonpathfound;
            ASApproachWithPathFind approachAction = new ASApproachWithPathFind(this, cmd);
            approachAction.SetCompleteCallback(OnApproachCompleted);
            PerformAction(approachAction);
        }

        private void OnApproachCompleted()
        {
            if (mAIController.IsTargetInRange())
                Idle();
            else
                mAIController.ApproachTarget();
        }

        public void CastSkill(int skillid, int targetPID)
        {
            CastSkillCommand cmd = new CastSkillCommand();
            cmd.skillid = skillid;
            cmd.targetpid = targetPID;
            ServerAuthoCastSkill action = new ServerAuthoCastSkill(this, cmd);
            action.SetCompleteCallback(Idle);
            PerformAction(action);
        }
    }
}