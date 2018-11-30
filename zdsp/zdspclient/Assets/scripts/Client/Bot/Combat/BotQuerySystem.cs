using System.Collections.Generic;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Client.Entities;

namespace Zealot.Bot
{
    public class BotQuerySystem
    {
        #region Singleton
        private static BotQuerySystem instance = null;
        public static BotQuerySystem Instance
        {
            get
            {
                if (instance == null)
                    instance = new BotQuerySystem();
                return instance;
            }
        }
        #endregion

        private List<int> mExcludeList;
        

        private BotQuerySystem()
        {
            mExcludeList = new List<int>();
        }

        public ActorGhost GetNearestEnemyInRange(float radius)
        {
            AddExcludedTarget(GameInfo.gLocalPlayer);
            return QueryForNonSpecificTarget(radius, true, mExcludeList);
        }

        public ActorGhost GetNearestEnemyByID(float radius, int targetID)
        {
            return QueryForSpecificTarget(radius, targetID);
        }

        public void AddExcludedTarget(ActorGhost target)
        {
            if (target == null)
                return;

            int targetID = target.ID;

            if (!mExcludeList.Contains(targetID))
                mExcludeList.Add(targetID);
        }

        public void ClearAllExcludedTargets()
        {
            mExcludeList.Clear();
        }

        private ActorGhost QueryForNonSpecificTarget(float radius, bool includeEliteAndBoss, List<int> ExcludeList)
        {
            EntitySystem entitySystem = GameInfo.gLocalPlayer.EntitySystem;

            Entity target = entitySystem.QueryForClosestEntityInSphere(GameInfo.gLocalPlayer.Position, radius, (queriedEntity) =>
            {
                if (queriedEntity.EntityType == EntityType.HeroGhost)
                    return false;
                int entityID = queriedEntity.ID;
                if (ExcludeList != null && ExcludeList.Contains(entityID))
                    return false;

                MonsterGhost ghost = queriedEntity as MonsterGhost;
                if (ghost != null && ghost.IsAlive() && CombatUtils.IsValidEnemyTarget(GameInfo.gLocalPlayer, ghost))
                {
                    MonsterType monsterType = ghost.mArchetype.monstertype;
                    // The target type is set to exclude the mini boss and boss
                    if (!includeEliteAndBoss && (monsterType == MonsterType.MiniBoss || monsterType == MonsterType.Boss))
                        return false;
                    return true;
                }
                else
                {
                    //Bot initiate attack against other players based on pvp rules when not in questing mode
                    PlayerGhost otherPlayer = queriedEntity as PlayerGhost;
                    if (otherPlayer != null && otherPlayer.IsAlive() && CombatUtils.IsValidEnemyTarget(GameInfo.gLocalPlayer, otherPlayer))
                        return true;
                }
                return false;
            });

            return target as ActorGhost;
        }

        private ActorGhost QueryForSpecificTarget(float radius, int targetID)
        {
            EntitySystem entitySystem = GameInfo.gLocalPlayer.EntitySystem;

            Entity target = entitySystem.QueryForClosestEntityInSphere(GameInfo.gLocalPlayer.Position, radius, (queriedEntity) =>
            {
                MonsterGhost ghost = queriedEntity as MonsterGhost;
                if (ghost == null)
                    return false;

                int monsterID = ghost.mArchetype.id;

                if (monsterID != targetID)
                    return false;

                if (IsTargetValidAndAlive(ghost))
                    return true;

                return false;
            });

            return target as ActorGhost;
        }

        private bool IsTargetValidAndAlive(ActorGhost newTarget)
        {
            return CombatUtils.IsValidEnemyTarget(GameInfo.gLocalPlayer, newTarget);
        }
    }
}