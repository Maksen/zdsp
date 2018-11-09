using System;
using System.Collections.Generic;
using UnityEngine;


namespace Zealot.Bot
{
    public class BotStateController
    {
        #region Singleton
        private static BotStateController instance = null;
        public static BotStateController Instance
        {
            get
            {
                if (instance == null)
                    instance = new BotStateController();

                return instance;
            }
        }
        #endregion

        private BotStateMachine mBotStateMachine = null;


        private BotStateController()
        {
            Debug.Log("Bot state controller init!!");
            mBotStateMachine = BotStateMachine.Instance;
            GoToState(BotStateType.Idle);
        }

        public void Start()
        {
            GoToState(BotStateType.AutoAttack);
        }

        public void Stop()
        {
            GoToState(BotStateType.Idle);
        }

        public void Interrupt()
        {
            GoToState(BotStateType.Idle);
        }

        public void Resume()
        {
            if (GameSettings.AutoBotEnabled)
                GoToState(BotStateType.AutoAttack);
            else
                GoToState(BotStateType.Idle);
        }

        public void Update(long dt)
        {
            mBotStateMachine.OnUpdate(dt);
        }

        public void Quest()
        {
            GoToState(BotStateType.Quest);
        }

        public void CombatQuest()
        {
            GoToState(BotStateType.CombatQuest);
        }

        public void NonCombatQuest()
        {
            GoToState(BotStateType.NonCombatQuest);
        }

        public void Cutscene()
        {
            GoToState(BotStateType.Cutscene);
        }

        private void GoToState(BotStateType state)
        {
            mBotStateMachine.GotoState(state.ToString());
        }

        private void GoToPrevState()
        {
            mBotStateMachine.GoToPrevState();
        }
    }
}