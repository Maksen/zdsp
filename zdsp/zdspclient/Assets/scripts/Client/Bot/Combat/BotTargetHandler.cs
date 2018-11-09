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
            // 開啟掛機前，確認是否有手動選擇的目標
            if (IsTargetValidAndAlive((ActorGhost)GameInfo.gSelectedEntity)) 
                SetManualTarget((ActorGhost)GameInfo.gSelectedEntity);

            ActorGhost newTarget = GetFriendlyTarget(); // 針對非Enemy Target Type先進行一次設定

            if (newTarget == null) // Enemy Target Type的設定
            {
                if (!IsTargetValidAndAlive(mManuallySelectedTarget))
                {
                    do
                    {
                        newTarget = QueryContext.Instance.QueryResult();
                        yield return null;
                    } while (!IsTargetValidAndAlive(newTarget));
                }
                else
                    newTarget = mManuallySelectedTarget;

                SelectTarget(newTarget);
            }
            else
                SetupCurrentTargetNoMark(newTarget);
        }

        public override void Stop()
        {
            base.Stop();
            ClearTarget();
            ClearManuallyInput();
        }

        private void SetupCurrentTarget(ActorGhost newTarget)
        {
            //Debug.Log("Currrent target ID: " + newTarget.GetPersistentID());
            mCurrentTarget = newTarget;
            GameInfo.gCombat.OnSelectEntity(mCurrentTarget);
        }

        private void SetupCurrentTargetNoMark(ActorGhost newTarget)
        {
            ClearTarget();
            mCurrentTarget = newTarget;
            GameInfo.gSelectedEntity = mCurrentTarget;
        }

        private void SelectTarget(ActorGhost newTarget)
        {
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

        private void SetManualTarget(ActorGhost newTarget)
        {
            //Debug.Log("Manually changed target!" + newTarget.GetPersistentID());
            ClearTarget();
            mManuallySelectedTarget = newTarget;
            SetupCurrentTarget(mManuallySelectedTarget);
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