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
        private int mQuestID;
        private int mTargetID;


        private BotStateController()
        {
            mBotStateMachine = BotStateMachine.Instance;
            Idle();
        }

        public void Start()
        {
            AutoAttack();
        }

        public void Stop()
        {
            Idle();
        }

        public void Interrupt()
        {
            Idle();
        }

        public void Resume()
        {
            if (GameSettings.AutoBotEnabled)
                AutoAttack();
            else
                Idle();
        }

        public void Update(long dt)
        {
            mBotStateMachine.OnUpdate(dt);
        }

        public void Combat()
        {
            if (GameSettings.AutoBotEnabled)
            {
                if (GetCurrentState() != BotStateType.CombatQuest)
                    AutoAttack();
            }
        }

        #region States
        public void Idle()
        {
            GoToState(BotStateType.Idle);
        }

        public void AutoAttack()
        {
            GoToState(BotStateType.AutoAttack);
        }

        public void Quest()
        {
            GoToState(BotStateType.Quest);
        }

        public void CombatQuest(int questID, int targetID)
        {
            if (!GameSettings.AutoBotEnabled)
                GameInfo.gLocalPlayer.Bot.StartBot();

            mQuestID = questID;
            mTargetID = targetID;

            QueryContext.Instance.GetQueryData().SetTargetID(targetID);
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

        private BotStateType GetCurrentState()
        {
            string currentStateName = mBotStateMachine.GetCurrentStateName();
            return mBotStateMachine.ConvertStateNameToType(currentStateName);
        }
        #endregion

        public bool IsCombatQuestComplete()
        {
            if (GameInfo.gLocalPlayer.QuestController.GetObjectiveIdByTargetId(mQuestID, mTargetID) == -1)
                return true;
            return false;
        }
    }
}