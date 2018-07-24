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

        [JsonProperty(PropertyName = "SkillInvCount")]
        public int SkillInvCount; // stores all skill level with key being the skillid

        [JsonProperty(PropertyName = "SkillInv")]
        public List<int> SkillInv; // stores all skill level with key being the skillid

        [JsonProperty(PropertyName = "EquippedSkill")]
        public List<int> EquippedSkill; // stores all equipped skills

        //cannot disable basic attack 
        public void InitDefault(JobsectJson jsj)
        {
            //basicAttack1SId = SkillRepo.Rage_BasicAtk1;
            //basicAttack2SId = SkillRepo.Rage_BasicAtk2;
            //basicAttack3SId = SkillRepo.Rage_BasicAtk3;  

            SkillInv = new List<int>(40);
            EquippedSkill = new List<int>(36);
            SkillInvCount = 0;
            for(int i = 0; i < 40; ++i)
            {
                SkillInv.Add(0);
            }
            for (int i = 0; i < 36; ++i)
            {
                EquippedSkill.Add(0);
                
            }
        }
    }
}
