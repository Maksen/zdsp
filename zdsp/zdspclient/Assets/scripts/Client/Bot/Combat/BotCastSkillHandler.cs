using System;
using System.Collections;
using UnityEngine;

namespace Zealot.Bot
{
    public class BotCastSkillHandler : BotCombatCommander
    {
        #region Singleton
        private static BotCastSkillHandler instance = null;
        public static BotCastSkillHandler Instance
        {
            get
            {
                if (instance == null)
                    instance = new BotCastSkillHandler();
                return instance;
            }
        }
        #endregion

        private bool bFinishCast = true;
        private IEnumerator mCountDownEnumerator = null;
        private const float CHANGE_TARGET_TIME = 3f;
        private float mCountDownTime = CHANGE_TARGET_TIME;


        private BotCastSkillHandler() : base() { }

        public void FinishCastSkill()
        {
            bFinishCast = true;
        }

        public void StartCastSkill()
        {
            StopCountDown();
            BotStateController.Instance.Combat();
        }

        protected override IEnumerator Start()
        {
            bFinishCast = false;
            StartCountDown();
            PrepareCastSkill();

            while (!bFinishCast)
            {
                yield return null;
            }

            StopCountDown();
        }

        public override void Stop()
        {
            StopCountDown();
            base.Stop();
        }

        private void PrepareCastSkill()
        {
            GameInfo.gCombat.TryCastActiveSkill(BotAutoSkillHandler.Instance.SkillidToCast);
        }

        private void StartCountDown()
        {
            if(mCountDownEnumerator == null)
            {
                mCountDownTime = CHANGE_TARGET_TIME;
                mCountDownEnumerator = CountDown();
                //Debug.Log("Start Count Down");
                GameInfo.gCombat.StartCoroutine(mCountDownEnumerator);
            }
        }

        private void StopCountDown()
        {
            if(mCountDownEnumerator != null)
            {
                GameInfo.gCombat.StopCoroutine(mCountDownEnumerator);
                //Debug.Log("Stop Count Down");
                mCountDownEnumerator = null;
            }
        }

        private IEnumerator CountDown()
        {
            while (mCountDownTime > 0)
            {
                yield return new WaitForSecondsRealtime(1f);
                mCountDownTime--;
            }

            //Debug.Log(CHANGE_TARGET_TIME + " seconds later...");
            FinishCastSkill();
            BotTargetHandler.Instance.ExcludeCurrentTarget();
        }
    }
}