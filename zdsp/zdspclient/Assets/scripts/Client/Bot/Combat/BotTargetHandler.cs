using System.Collections;
using UnityEngine;
using Zealot.Common;
using Zealot.Client.Entities;


namespace Zealot.Bot
{
    public class BotTargetHandler : BotCombatCommander
    {
        #region Singleton
        private static BotTargetHandler instance = null;
        public static BotTargetHandler Instance
        {
            get
            {
                if (instance == null)
                    instance = new BotTargetHandler();
                return instance;
            }
        }
        #endregion

        private ActorGhost mCurrentTarget = null;
        private ActorGhost mManuallySelectedTarget = null;


        private BotTargetHandler() : base() { }

        protected override IEnumerator Start()
        {
            ActorGhost theTarget = null;

            var selectedTarget = GameInfo.gSelectedEntity as ActorGhost;
            if (selectedTarget != null && selectedTarget != mCurrentTarget && IsMonsterValid(selectedTarget))
            {
                SetManualTarget(selectedTarget);
                theTarget = mManuallySelectedTarget;
            }

            theTarget = GetFriendlyTarget();
            if (theTarget == null)
            {
                if (!IsTargetValidAndAlive(mManuallySelectedTarget))
                {
                    do
                    {
                        theTarget = QueryContext.Instance.QueryResult();
                        yield return null;
                    } while (!IsTargetValidAndAlive(theTarget));
                }
                else
                    theTarget = mManuallySelectedTarget;
            }

            SelectTarget(theTarget);
            BotQuerySystem.Instance.ClearAllExcludedTargets();
        }

        public override void Stop()
        {
            base.Stop();
            ClearTarget();
            ClearManuallyInput();
            ClearManaulTarget();
            BotQuerySystem.Instance.ClearAllExcludedTargets();
        }

        public void ExcludeCurrentTarget()
        {
            BotQuerySystem.Instance.AddExcludedTarget(mCurrentTarget);
            ClearTarget();
        }

        private void SetupCurrentTarget(ActorGhost newTarget)
        {
            ClearTarget();
            mCurrentTarget = newTarget;
            GameInfo.gCombat.OnSelectEntity(mCurrentTarget);
            //Debug.Log("Currrent target ID: " + newTarget.GetPersistentID());
        }

        private void SetupCurrentTargetNoMark(ActorGhost newTarget)
        {
            ClearTarget();
            mCurrentTarget = newTarget;
            GameInfo.gSelectedEntity = mCurrentTarget;
            //Debug.Log("No mark target ID: " + newTarget.GetPersistentID());
        }

        private void SelectTarget(ActorGhost newTarget)
        {
            if (newTarget.IsPlayer())
            {
                SetupCurrentTargetNoMark(newTarget);
                return;
            }

            switch (BotAutoSkillHandler.Instance.GetSkillThreatzone())
            {
                case Threatzone.Single:
                    SetupCurrentTarget(newTarget);
                    break;
                case Threatzone.DegreeArc120:
                case Threatzone.DegreeArc360:
                case Threatzone.LongStream:
                    SetupCurrentTargetNoMark(newTarget);
                    break;
                default:
                    break;
            }
        }

        private void ClearTarget()
        {
            mCurrentTarget = null;
            GameInfo.gCombat.OnSelectEntity(null);
        }

        private bool IsTargetValidAndAlive(ActorGhost newTarget)
        {
            return CombatUtils.IsValidEnemyTarget(GameInfo.gLocalPlayer, newTarget);
        }

        private ActorGhost GetFriendlyTarget()
        {
            switch (BotAutoSkillHandler.Instance.GetSkillTargetType())
            {
                case TargetType.Friendly:
                    // TODO 針對一般玩家做輔助技能的施放
                    return GameInfo.gLocalPlayer;
                case TargetType.Party:
                    // TODO 進入Party狀態才會用到，會有另外的Query方式
                    return GameInfo.gLocalPlayer;
                default:
                    return null;
            }
        }

        public void ManuallyChangeTarget()
        {
            GameInfo.gCombat.mPlayerInput.ListenForNewEnemy((ActorGhost entity) =>
            {
                if (entity == null) return;

                if (IsMonsterValid(entity))
                {
                    SetManualTarget(entity);
                    BotCastSkillHandler.Instance.FinishCastSkill();
                }
            });
        }

        public ActorGhost GetCurrentTarget()
        {
            return mCurrentTarget;
        }

        private void SetManualTarget(ActorGhost newTarget)
        {
            //Debug.Log("Manually changed target!" + newTarget.GetPersistentID());
            ClearTarget();
            mManuallySelectedTarget = newTarget;
            SetupCurrentTarget(mManuallySelectedTarget);
        }

        private void ClearManaulTarget()
        {
            mManuallySelectedTarget = null;
        }

        private void ClearManuallyInput()
        {
            GameInfo.gCombat.mPlayerInput.ListenForNewEnemy(null);
        }

        private bool IsMonsterValid(ActorGhost entity)
        {
            if (entity != null && entity.IsMonster())
                return true;
            return false;
        }
    }
}