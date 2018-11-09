using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

namespace Zealot.Bot
{
    public class AutoSkill
    {
        public int skillid;
        public int priority;

        public AutoSkill(int skillid, int skillPriority)
        {
            this.skillid = skillid;
            this.priority = skillPriority;
        }
    }

    public class BotAutoSkillHandler : BotCombatCommander
    {
        #region Singleton
        private static BotAutoSkillHandler instance = null;
        public static BotAutoSkillHandler Instance
        {
            get
            {
                if (instance == null)
                    instance = new BotAutoSkillHandler();
                return instance;
            }
        }
        #endregion

        private List<AutoSkill> autoSkills = null;
        public int SkillidToCast { get; private set; }
        private int autoSkillIndex;
        

        private BotAutoSkillHandler() : base()
        {
            autoSkills = new List<AutoSkill>();
            SkillidToCast = 0;
            autoSkillIndex = -1; // The first index of list is 0
        }

        protected override IEnumerator Start()
        {
            while (!IsAutoSkillRowValid())
            {
                yield return null;
            }

            do
            {
                SetupNextAutoSkill();
                yield return null;
            } while (!CanCastSkill());

            //Debug.Log("Skill ID: " + SkillidToCast);
        }

        public override void Stop()
        {
            base.Stop();
            autoSkillIndex = -1; // Reset skill
        }

        /// <summary>
        /// Update the auto skills that player can cast in the bot mode.
        /// </summary>
        public void UpdateAutoSkillRow()
        {
            List<int> autoSkillRow = GetAutoSkillRow();
            if (autoSkillRow == null)
                return;

            autoSkills.Clear();
            for (int i = 0; i < autoSkillRow.Count; i++)
            {
                autoSkills.Add(new AutoSkill(autoSkillRow[i], SkillRepo.GetSkillPriority(autoSkillRow[i])));
            }

            SortAutoSkillByPriority();
        }

        public TargetType GetSkillTargetType()
        {
            return SkillRepo.GetSkillTargetType(SkillidToCast);
        }

        public Threatzone GetSkillThreatzone()
        {
            return SkillRepo.GetSkillThreatzone(SkillidToCast);
        }

        private void SetupNextAutoSkill()
        {
            SkillidToCast = GetNextAutoSkill();
        }

        private void SortAutoSkillByPriority()
        {
            // Order by descending
            autoSkills.Sort((skill1, skill2) => { return -skill1.priority.CompareTo(skill2.priority); });
        }

        private int GetNextAutoSkill()
        {
            if (++autoSkillIndex >= autoSkills.Count)
                autoSkillIndex = 0;

            return autoSkills[autoSkillIndex].skillid;
        }

        private List<int> GetAutoSkillRow()
        {
            List<int> autoSkillRow = new List<int>();
            int autoSkillSize = GameInfo.gLocalPlayer.SkillStats.EquipSize;

            for (int i = 0; i < autoSkillSize; ++i)
            {
                int autoSkillID = (int)GameInfo.gLocalPlayer.SkillStats.AutoSkill[ConvertToAutoSkillIndex(i)];

                if (autoSkillID != 0) // 0 means no skill in the slot
                {
                    autoSkillRow.Add(autoSkillID);
                }
            }

            return autoSkillRow;
        }

        private int ConvertToAutoSkillIndex(int index)
        {
            int equipSize = GameInfo.gLocalPlayer.SkillStats.EquipSize;
            int autoGroupNum = GameInfo.gLocalPlayer.SkillStats.AutoGroup - 1;

            return equipSize * autoGroupNum + index;
        }

        private bool IsAutoSkillRowValid()
        {
            if (autoSkills.Count == 0)
                return false;
            return true;
        }

        private bool CanCastSkill()
        {
            return IsSkillValid() && !IsSkillCoolingDown() && IsManaEnough() && IsHPEnough();
        }

        private bool IsSkillValid()
        {
            SkillData sdata = SkillRepo.GetSkill(SkillidToCast);
            if (sdata == null)
                return false;
            return true;
        }

        private bool IsSkillCoolingDown()
        {
            PlayerSkillCDState cdstate = GameInfo.gSkillCDState;
            if (cdstate.IsSkillCoolingDown(SkillidToCast))
                return true;
            return false;
        }

        private bool IsHPEnough()
        {
            return true;
        }

        private bool IsManaEnough()
        {
            SkillData sdata = SkillRepo.GetSkill(SkillidToCast);

            float cost = 0;
            if (sdata.skillgroupJson.costab)
                cost = sdata.skillJson.cost;
            else
                cost = GameInfo.gLocalPlayer.GetManaMax() * sdata.skillJson.cost * 0.01f;

            return GameInfo.gLocalPlayer.GetMana() >= cost;
        }
    }
}