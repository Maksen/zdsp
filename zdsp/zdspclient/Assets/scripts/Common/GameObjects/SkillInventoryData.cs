using System;
using Newtonsoft.Json;
using System.ComponentModel;
using Kopio.JsonContracts;
using Zealot.Repository;
using System.Collections.Generic;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SkillInventoryData
    {
        //all skill id below is skillgroup id ;
        [JsonProperty(PropertyName = "BasicAttack1SkillId")]
        public int basicAttack1SId { get; set; }

        [JsonProperty(PropertyName = "BasicAttack2SkillId")]
        public int basicAttack2SId { get; set; }

        [JsonProperty(PropertyName = "BasicAttack3SkillId")]
        public int basicAttack3SId { get; set; }

        [JsonProperty(PropertyName ="EquipGroup")]
        public int equipGroup { get; set; }

        [JsonProperty(PropertyName ="AutoGroup")]
        public int autoGroup { get; set; }

        [JsonProperty(PropertyName = "EquippedSkill")]
        public List<int> EquippedSkill; // stores all equipped skills

        [JsonProperty(PropertyName = "AutoSkill")]
        public List<int> AutoSkill; // stores all equipped auto skills

        [JsonProperty(PropertyName = "EquipSize")]
        public int EquipSize; // stores the size of skills that can be equipped

        [JsonProperty(PropertyName = "AutoSize")]
        public int AutoSize; // stores te size of skills that can be botted

        [JsonProperty(PropertyName = "UnlockedAutoSize")]
        public int UnlockedAutoSize;

        [JsonProperty(PropertyName = "SkillInventory")]
        public Dictionary<int, int> m_SkillInventory { get; set; }

        //cannot disable basic attack 
        public void InitDefault(JobsectJson jsj)
        {
            basicAttack1SId = 1;
            EquippedSkill = new List<int>(36);
            AutoSkill = new List<int>(42);
            equipGroup = autoGroup = 1;
            EquipSize = 6;
            AutoSize = 7;
            UnlockedAutoSize = 4;
            for (int i = 0; i < 36; ++i)
            {
                EquippedSkill.Add(0);
            }
            for(int i = 0; i < 42; ++i)
            {
                AutoSkill.Add(0);
            }

            m_SkillInventory = new Dictionary<int, int>(); // not synced
        }

        public void AddSkillToBag(int skillgroup, int skillid)
        {
            if (m_SkillInventory.ContainsKey(skillgroup))
            {
                m_SkillInventory[skillgroup] = skillid;
            }
            else
            {
                m_SkillInventory.Add(skillgroup, skillid);
            }
        }
    }
}
