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
        public int basicAttack1SId { get;
            set; }

        [JsonProperty(PropertyName = "BasicAttack2SkillId")]
        public int basicAttack2SId { get; set; }

        [JsonProperty(PropertyName = "BasicAttack3SkillId")]
        public int basicAttack3SId { get; set; }

        [JsonProperty(PropertyName ="EquipGroup")]
        public int equipGroup { get; set; }

        [JsonProperty(PropertyName ="AutoGroup")]
        public int autoGroup { get; set; }

        [JsonProperty(PropertyName = "SkillInvCount")]
        public int SkillInvCount;

        [JsonProperty(PropertyName = "SkillInv")]
        public List<int> SkillInv; // stores all skill level with key being the skillid

        [JsonProperty(PropertyName = "EquippedSkill")]
        public List<int> EquippedSkill; // stores all equipped skills

        [JsonProperty(PropertyName = "AutoSkill")]
        public List<int> AutoSkill; // stores all equipped auto skills

        //cannot disable basic attack 
        public void InitDefault(JobsectJson jsj)
        {

            SkillInv = new List<int>(40);
            EquippedSkill = new List<int>(36);
            AutoSkill = new List<int>(36);
            SkillInvCount = 0;
            equipGroup = autoGroup = 1;
            for(int i = 0; i < 40; ++i)
            {
                SkillInv.Add(0);
            }
            for (int i = 0; i < 36; ++i)
            {
                EquippedSkill.Add(0);
                AutoSkill.Add(0);
            }
        }
    }
}
