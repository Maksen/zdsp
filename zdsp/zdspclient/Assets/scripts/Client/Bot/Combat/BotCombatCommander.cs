using System.Collections;

namespace Zealot.Bot
{
    public abstract class BotCombatCommander
    {
        private IEnumerator enumerator;
        private bool isRunning = false;


        public BotCombatCommander()
        {
            SetupCoroutine();
        }

        public virtual IEnumerator Execute()
        {
            StartMyCoroutine();
            yield return enumerator;
            StopMyCoroutine();
        }

        public virtual void Stop()
        {
            StopMyCoroutine();
        }

        private void SetupCoroutine()
        {
            if (enumerator == null)
                enumerator = Start();
        }

        private void ClearCoroutine()
        {
            enumerator = null;
        }

        private void StartMyCoroutine()
        {
            if (!isRunning)
            {
                SetupCoroutine();
                GameInfo.gCombat.StartCoroutine(enumerator);
                isRunning = true;
            }
        }

        private void StopMyCoroutine()
        {
            if (enumerator != null)
            {
                GameInfo.gCombat.StopCoroutine(enumerator);
                ClearCoroutine();
                isRunning = false;
            }
        }

        protected abstract IEnumerator Start();
    }
}