using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zealot.Bot
{
    public class BotCombatController
    {
        #region Singleton
        private static BotCombatController instance = null;
        public static BotCombatController Instance
        {
            get
            {
                if (instance == null)
                    instance = new BotCombatController();
                return instance;
            }
        }
        #endregion

        private IList<BotCombatCommander> commands = new List<BotCombatCommander>();
        private IEnumerator mRunCombatCoroutine = null;
        private Coroutine mExecuteCoroutine = null;
        private bool isRunning = false;


        private BotCombatController()
        {
            SetCommand(BotAutoSkillHandler.Instance);
            SetCommand(BotTargetHandler.Instance);
            SetCommand(BotCastSkillHandler.Instance);
        }

        private void SetCommand(BotCombatCommander cmd)
        {
            commands.Add(cmd);
        }

        public void StartCombat()
        {
            if (!isRunning)
            {
                isRunning = true;
                mRunCombatCoroutine = Run();
                GameInfo.gCombat.StartCoroutine(mRunCombatCoroutine);
            }
        }

        public void StopCombat()
        {
            if(mRunCombatCoroutine != null)
            {
                isRunning = false;

                GameInfo.gCombat.StopCoroutine(mRunCombatCoroutine);
                mRunCombatCoroutine = null;

                ClearAllCoroutine();
            }
        }

        private IEnumerator Run()
        {
            while (isRunning)
            {
                foreach (var cmd in commands)
                {
                    mExecuteCoroutine = GameInfo.gCombat.StartCoroutine(cmd.Execute());
                    yield return mExecuteCoroutine;
                    ClearExecuteCoroutine();
                }
            }
        }

        private void ClearExecuteCoroutine()
        {
            if (mExecuteCoroutine != null)
            {
                GameInfo.gCombat.StopCoroutine(mExecuteCoroutine);
                mExecuteCoroutine = null;
            }
        }

        private void ClearAllCoroutine()
        {
            foreach (var cmd in commands)
                cmd.Stop();

            ClearExecuteCoroutine();
        }
    }
}