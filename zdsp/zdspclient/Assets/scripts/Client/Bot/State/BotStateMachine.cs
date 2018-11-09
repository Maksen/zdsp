using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;

namespace Zealot.Bot
{
    public class BotStateMachine : StateMachine
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

        // State
        private IdleState mIdleState = null;
        private AutoAttackState mAutoAttackState = null;
        private QuestState mQuestState = null;
        private CombatQuestState mCombatQuestState = null;
        private NonCombatQuestState mNonCombatQuestState = null;
        private CutsceneState mCutsceneState = null;
        //private RealmState mRealmState = null;
        //private PartyState mPartyState = null;

        private string mPrevStateName = "";


        private BotStateMachine()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Create instance
            mIdleState = new IdleState();
            mAutoAttackState = new AutoAttackState();
            mQuestState = new QuestState();
            mCombatQuestState = new CombatQuestState();
            mNonCombatQuestState = new NonCombatQuestState();
            mCutsceneState = new CutsceneState();
            //mRealmState = new RealmState();
            //mPartyState = new PartyState();

            // Add state
            AddState(mIdleState.State.ToString(), mIdleState.StateBegin, mIdleState.StateEnd);
            AddState(mAutoAttackState.State.ToString(), mAutoAttackState.StateBegin, mAutoAttackState.StateEnd, mAutoAttackState.StateUpdate);
            AddState(mQuestState.State.ToString(), mQuestState.StateBegin, mQuestState.StateEnd);
            AddState(mCombatQuestState.State.ToString(), mCombatQuestState.StateBegin, mCombatQuestState.StateEnd);
            AddState(mNonCombatQuestState.State.ToString(), mNonCombatQuestState.StateBegin, mNonCombatQuestState.StateEnd);
            AddState(mCutsceneState.State.ToString(), mCutsceneState.StateBegin, mCutsceneState.StateEnd);
            //AddState(mRealmState.State.ToString(), mRealmState.StateBegin, mRealmState.StateEnd);
            //AddState(mPartyState.State.ToString(), mPartyState.StateBegin, mPartyState.StateEnd);
        }

        public override void OnPreLeaveState()
        {
            mPrevStateName = currentState.Name;
        }

        public override void GotoState(string stateName)
        {
            string prevStateName = "undefined";
            if (currentState == null)
            {
                currentState = states[stateName];
                currentState.OnEnterStateDel(prevStateName);

                return;
            }

            if (stateName == currentState.Name)
                return;

            OnPreLeaveState();

            prevStateName = currentState.Name;
            if (currentState.OnLeaveStateDel != null)
                currentState.OnLeaveStateDel();

            currentState = states[stateName];
            currentState.OnEnterStateDel(prevStateName);
        }

        public void GoToPrevState()
        {
            if (!string.IsNullOrEmpty(mPrevStateName))
                GotoState(mPrevStateName);
        }

        private BotStateType ConvertStateNameToType(string stateName)
        {
            BotStateType botStateType = (BotStateType)Enum.Parse(typeof(BotStateType), stateName);
            return botStateType;
        }
    }
}