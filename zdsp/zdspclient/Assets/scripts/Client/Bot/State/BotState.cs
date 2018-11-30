using UnityEngine;

namespace Zealot.Bot
{
    public enum BotStateType : byte
    {
        Idle = 0,
        AutoAttack = 1,
        Quest = 2,
        CombatQuest = 3,
        NonCombatQuest = 4,
        Cutscene = 5,
        Realm = 6,
        Party = 7
    }

    public abstract class BotState
    {
        private BotStateType mState = BotStateType.AutoAttack;

        protected BotState() { }

        public BotStateType StateType
        {
            get { return mState; }
            protected set { mState = value; }
        }

        public abstract void StateBegin();
        public abstract void StateEnd();
        public virtual void StateUpdate(long dt) { }
    }

    public class IdleState : BotState
    {
        public IdleState() : base()
        {
            StateType = BotStateType.Idle;
        }

        public override void StateBegin()
        {
            Debug.Log("Enter Idle state");
        }

        public override void StateEnd()
        {
            Debug.Log("Leave Idle state");
        }
    }

    public class AutoAttackState : BotState
    {
        public AutoAttackState() : base()
        {
            StateType = BotStateType.AutoAttack;
        }

        public override void StateBegin()
        {
            Debug.Log("Enter Auto attack state");

            QueryContext.Instance.SetQueryType(BotQueryType.NearestEnemyQuery);
            BotCombatController.Instance.StartCombat();
        }

        public override void StateUpdate(long dt)
        {
            BotTargetHandler.Instance.ManuallyChangeTarget();
        }

        public override void StateEnd()
        {
            BotCombatController.Instance.StopCombat();
            Debug.Log("Leave Auto attack state");
        }
    }

    // 當玩家點選任務時，需轉換至這個state
    public class QuestState : BotState
    {
        public QuestState()  : base()
        {
            StateType = BotStateType.Quest;
        }

        public override void StateBegin()
        {
            Debug.Log("Enter Quest state");
            // TODO 如果在組隊且跟隨的狀況下，如果去執行任何state的事項，都要取消跟隨
        }

        public override void StateEnd()
        {
            Debug.Log("Leave Quest state");
        }
    }

    // 當玩家點選戰鬥任務時，需轉換至這個state
    public class CombatQuestState : BotState
    {
        public CombatQuestState() : base()
        {
            StateType = BotStateType.CombatQuest;
        }
        
        public override void StateBegin()
        {
            Debug.Log("Enter Combat Quest State");
            
            QueryContext.Instance.SetQueryType(BotQueryType.SpecificEnemyQuery);
            BotCombatController.Instance.StartCombat();
        }

        public override void StateUpdate(long dt)
        {
            if (BotStateController.Instance.IsCombatQuestComplete())
                BotStateController.Instance.AutoAttack();
        }

        public override void StateEnd()
        {
            BotCombatController.Instance.StopCombat();
            Debug.Log("Leave Combat Quest State");
        }
    }

    // 當玩家點選非戰鬥型的任務時，需轉換至這個state
    public class NonCombatQuestState : BotState
    {
        public NonCombatQuestState() : base()
        {
            StateType = BotStateType.NonCombatQuest;
        }

        public override void StateBegin()
        {
            Debug.Log("Enter Non Combat Quest State");
        }

        public override void StateEnd()
        {
            Debug.Log("Leave Non Combat Quest State");
        }
    }

    // 當播放cutscene的時候，需轉換至這個state
    public class CutsceneState : BotState
    {
        public CutsceneState() : base()
        {
            StateType = BotStateType.Cutscene;
        }

        public override void StateBegin()
        {
            Debug.Log("Enter Cutscene state");
            // TODO 無敵、無法移動角色
        }

        public override void StateEnd()
        {
            // TODO 3秒後，解除無敵、可移動角色，並回到Quest State
            Debug.Log("Leave Cutscene state");
        }
    }

    public class RealmState : BotState
    {
        public RealmState() : base()
        {
            StateType = BotStateType.Realm;
        }

        public override void StateBegin()
        {
            
        }

        public override void StateEnd()
        {
            
        }
    }

    public class PartyState : BotState
    {
        public PartyState() : base()
        {
            StateType = BotStateType.Party;
        }

        public override void StateBegin()
        {
            Debug.Log("Enter Party state");
        }

        public override void StateEnd()
        {
            Debug.Log("Leave Party state");
        }
    }
}