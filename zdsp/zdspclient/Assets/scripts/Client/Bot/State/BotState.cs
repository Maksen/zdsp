using System.Collections;
using System.Collections.Generic;
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

        public BotStateType State
        {
            get { return mState; }
            protected set { mState = value; }
        }

        public abstract void StateBegin(string prevState);
        public abstract void StateEnd();
        public virtual void StateUpdate(long dt) { }
    }

    public class IdleState : BotState
    {
        public IdleState() : base()
        {
            State = BotStateType.Idle;
        }

        public override void StateBegin(string prevState)
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
            State = BotStateType.AutoAttack;
        }

        public override void StateBegin(string prevstate)
        {
            Debug.Log("Enter Auto state");

            // 切換搜尋模式
            QueryContext.Instance.SetQueryType(BotQueryType.NearestEnemyQuery);

            // 自動戰鬥
            BotCombatController.Instance.StartCombat();
        }

        public override void StateUpdate(long dt)
        {
            base.StateUpdate(dt);
            BotTargetHandler.Instance.ManuallyChangeTarget();
        }

        public override void StateEnd()
        {
            BotCombatController.Instance.StopCombat();
            Debug.Log("Leave Auto state");
        }
    }

    // 當玩家點選任務時，需轉換至這個state
    public class QuestState : BotState
    {
        public QuestState()  : base()
        {
            State = BotStateType.Quest;
        }

        public override void StateBegin(string prevstate)
        {
            Debug.Log("Enter Quest state");
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
            State = BotStateType.CombatQuest;
        }
        
        public override void StateBegin(string prevstate)
        {
            Debug.Log("Enter Combat Quest State");
            // TODO 切換戰鬥搜尋模式
            //QueryContext.Instance.SetQueryType(BotQueryType.SpecificEnemyQuery);

            // TODO 呼叫自動戰鬥
        }

        public override void StateEnd()
        {
            // TODO 當玩家移動角色/任務完成/關閉bot時，換回原本的搜尋模式
            Debug.Log("Leave Combat Quest State");
        }
    }

    // 當玩家點選非戰鬥型的任務時，需轉換至這個state
    public class NonCombatQuestState : BotState
    {
        public NonCombatQuestState() : base()
        {
            State = BotStateType.NonCombatQuest;
        }

        public override void StateBegin(string prevState)
        {
            Debug.Log("Enter Non Combat Quest State");
        }

        public override void StateEnd()
        {
            // TODO 當玩家移動角色/任務完成時
            Debug.Log("Leave Non Combat Quest State");
        }
    }

    // 當播放cutscene的時候，需轉換至這個state
    public class CutsceneState : BotState
    {
        public CutsceneState() : base()
        {
            State = BotStateType.Cutscene;
        }

        public override void StateBegin(string prevstate)
        {
            Debug.Log("Enter Cutscene state");
            // TODO 無敵、無法移動角色
        }

        public override void StateEnd()
        {
            // TODO 跳過或結束cutscene時，呼叫此function
            // TODO 3秒後，解除無敵、可移動角色，並回到Quest State
            Debug.Log("Leave Cutscene state");
        }
    }

    public class RealmState : BotState
    {
        public RealmState() : base()
        {
            State = BotStateType.Realm;
        }

        public override void StateBegin(string prevState)
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
            State = BotStateType.Party;
        }

        public override void StateBegin(string prevState)
        {
            
        }

        public override void StateEnd()
        {
            
        }
    }
}

