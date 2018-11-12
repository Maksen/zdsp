using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.Entities;
using Zealot.Server.SideEffects;

namespace Photon.LoadBalancing.Entities
{
    public class SkillComboData
    {
        public int red;
        public int red2;
        public int green;
        public int green2;
        public int blue;
        public int blue2;
        public int redlevel = 1;
        public int greenlevel = 1;
        public int bluelevel = 1;
    }   

    public abstract class ComboSkillCaster : Actor 
    {
        protected SkillData[] cachedPlayerSkills;
        public int EquipedAncientCards = 0;
        public int EquipedRareCards = 0;
        List<StatsSE> skillpassivelist = null;
        

        public ComboSkillCaster():base()
        {
            cachedPlayerSkills = new SkillData[3];
        }

        public virtual PlayerSynStats GetPlayerStats()
        {
            return null;
        }

        public SkillData GetCompoundSkill(int index)
        {
            return cachedPlayerSkills[index];
        }

        protected void UpdateCompoundSkillPassive()
        {
            //time to apply skill passive effect. 
            SkillData data = cachedPlayerSkills[0];
            SkillData data2 = cachedPlayerSkills[1];
            SkillData data3 = cachedPlayerSkills[2];
            List<SideEffectJson> list = new List<SideEffectJson> { };
            if (data != null && data.skills.mSelf != null) list.AddRange(data.skills.mSelf);
            if (data2 != null && data2.skills.mSelf != null) list.AddRange(data2.skills.mSelf);
            if (data3 != null && data3.skills.mSelf != null) list.AddRange(data3.skills.mSelf);
            //testing
            //SideEffectJson testsej = SideEffectRepo.GetSideEffect(7);
            //list.Add(testsej);
            //testsej = SideEffectRepo.GetSideEffect(6);
            //list.Add(testsej);
            PlayerCombatStats cbtstats = (PlayerCombatStats)CombatStats; 
            foreach (SideEffectJson sejson in list)
            {
                switch (sejson.effecttype)
                {
                    //case EffectType.Stats_HealthMax:
                    //case EffectType.StatsAttack_Attack:
                    //case EffectType.StatsAttack_Attack_Debuff:
                    case EffectType.StatsAttack_CriticalDamage:
                    case EffectType.StatsAttack_CriticalDamage_Debuff:
                    case EffectType.StatsAttack_Critical:
                    case EffectType.StatsAttack_Critical_Debuff:
                    case EffectType.StatsAttack_Accuracy:
                    case EffectType.StatsAttack_Accuracy_Debuff:
                    case EffectType.StatsDefence_Armor:
                    case EffectType.StatsDefence_Armor_Debuff:
                    //case EffectType.StatsDefence_CoCriticalDamage:
                    //case EffectType.StatsDefence_CoCriticalDamage_Debuff:
                    case EffectType.StatsDefence_CoCritical:
                    case EffectType.StatsDefence_CoCritical_Debuff:
                    case EffectType.StatsDefence_Evasion:
                    case EffectType.StatsDefence_Evasion_Debuff:
                        SideEffect se = SideEffectFactory.CreateSideEffect(sejson, SEORIGINID.NONE, -1, true);
                        ((StatsSE)se).AddPassive(this, true);
                        skillpassivelist.Add((StatsSE)se);
                        break;
                }
            } 
            cbtstats.ComputeAll(); 
        }

        public void ResetPassiveSkills()
        {
            if (skillpassivelist != null)
            {
                PlayerCombatStats pcs = CombatStats as PlayerCombatStats;
                 
                foreach (StatsSE se in skillpassivelist)
                {
                    se.RemovePassive(true);
                }
                 
                pcs.ComputeAll();
                skillpassivelist.Clear();
            }
        }

        public override void StopAllSideEffects()
        {
            base.StopAllSideEffects(); 
            //PlayerCombatStats combatStats = (PlayerCombatStats)CombatStats; 
            //combatStats.ComputeAll();
        }

        protected void ResetSkillCombo()
        {
            if(SkillPassiveStats ==null)
                SkillPassiveStats = new SkillPassiveCombatStats(EntitySystem.Timers, this);
            EquipedAncientCards = 0;
            EquipedRareCards = 0;
            if (skillpassivelist != null)
            {
                PlayerCombatStats pcs = CombatStats as PlayerCombatStats; 
                foreach (StatsSE se in skillpassivelist)
                {
                    se.RemovePassive(true);
                } 
                pcs.ComputeAll();
                skillpassivelist.Clear();
            }
            else
            {
                skillpassivelist = new List<StatsSE>();
            }
            SkillPassiveStats.ResetAll();
        }
         

        /// <summary>
        /// call for Player
        /// </summary>
        /// <param name="combdata"></param>
        /// <param name="SkillStats"></param>
        /// <param name="controller"></param>
        /// <param name="playerstats"></param>
         
         
        
    }
}
