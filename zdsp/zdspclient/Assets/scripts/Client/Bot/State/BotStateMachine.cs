using System;
using System.Collections.Generic;

namespace Zealot.Bot
{
    public class BotStateMachine
    {
        #region Singleton
        private static BotStateMachine instance = null;
        public static BotStateMachine Instance
        {
            get
            {
                if (instance == null)
                    instance = new BotStateMachine();
                return instance;
            }
        }
        #endregion
        
        private string mPrevStateName = "";
        private Dictionary<string, BotState> states;
        private BotState currentState;


        private BotStateMachine()
        {
            Initialize();
        }

        private void Initialize()
        {
            states = new Dictionary<string, BotState>();

            AddState<IdleState>();
            AddState<AutoAttackState>();
            AddState<QuestState>();
            AddState<CombatQuestState>();
            AddState<NonCombatQuestState>();
            AddState<CutsceneState>();
            //AddState<PartyState>();
        }

        protected virtual void AddState<T>() where T : BotState
        {
            T state = (T)Activator.CreateInstance(typeof(T));
            states.Add(state.StateType.ToString(), state);
        }

        public virtual void OnUpdate(long dt)
        {
            if (currentState != null)
                currentState.StateUpdate(dt);
        }

        protected virtual void OnPreLeaveState()
        {
            mPrevStateName = currentState.StateType.ToString();
        }

        public virtual void GotoState(string stateName)
        {
            if (currentState == null)
            {
                EnterNewState(stateName);
                return;
            }

            if (IsSameAsCurrentState(stateName))
                return;

            LeaveState();
            EnterNewState(stateName);
        }

        public string GetCurrentStateName()
        {
            return currentState.StateType.ToString();
        }

        public void GoToPrevState()
        {
            if (!string.IsNullOrEmpty(mPrevStateName))
                GotoState(mPrevStateName);
        }

        public BotStateType ConvertStateNameToType(string stateName)
        {
            BotStateType botStateType = (BotStateType)Enum.Parse(typeof(BotStateType), stateName, true);
            return botStateType;
        }

        private bool IsSameAsCurrentState(string stateName)
        {
            return (stateName == currentState.StateType.ToString());
        }

        private void EnterNewState(string stateName)
        {
            currentState = states[stateName];
            currentState.StateBegin();
        }

        private void LeaveState()
        {
            OnPreLeaveState();
            currentState.StateEnd();
        }
    }
}