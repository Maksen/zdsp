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

        private BotCastSkillHandler() : base() { }


        public void FinishCastSkill()
        {
            bFinishCast = true;
        }

        protected override IEnumerator Start()
        {
            bFinishCast = false;
            CastSkill();

            while (!bFinishCast)
            {
                yield return null;
            }
        }

        private void CastSkill()
        {
            GameInfo.gCombat.TryCastActiveSkill(BotAutoSkillHandler.Instance.SkillidToCast);
        }
    }
}