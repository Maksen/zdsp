namespace Zealot.Server.SideEffects
{
    using Kopio.JsonContracts;
    using System.Collections.Generic;
    using Zealot.Common;   
    using Zealot.Common.Entities;
    using Zealot.Repository;
    using Zealot.Server.Entities;
    
    public static class SideEffectsUtils
    {
        private static List<Dictionary<EffectType, byte>> m_SideEffectTypeCollection = new List<Dictionary<EffectType, byte>>() {
                new Dictionary<EffectType, byte>() {
                    {EffectType.Damage_NoElementDamage, 0 },
                    {EffectType.Damage_MetalDamage, 0 },
                    {EffectType.Damage_WoodDamage, 0},
                    {EffectType.Damage_EarthDamage, 0 },
                    {EffectType.Damage_WaterDamage, 0 },
                    {EffectType.Damage_FireDamage, 0 },
                    {EffectType.Damage_DamageBaseOnWeaponElement, 0 },
                    {EffectType.Damage_PureDamage, 0 }
                }, // Damage types
                new Dictionary<EffectType, byte>() {
                    {EffectType.Stats_Strength, 0 },
                    {EffectType.Stats_Agility, 0 },
                    {EffectType.Stats_Dexterity, 0 },
                    {EffectType.Stats_Constitution, 0 },
                    {EffectType.Stats_Intelligence, 0 },
                    {EffectType.Stats_AttackSpeed, 0 },
                    {EffectType.Stats_CastSpeed, 0 },
                    {EffectType.Stats_MoveSpeed, 0 },
                    {EffectType.Stats_ExpBonus, 0 },
                    {EffectType.Stats_MaxHealth, 0 },
                    {EffectType.Stats_HealthRegen, 0 },
                    {EffectType.Stats_MaxMana, 0 },
                    {EffectType.Stats_ManaRegen, 0 },
                    {EffectType.Stats_EnergyShield, 0 },
                    {EffectType.Stats_IgnoreArmor, 0 },
                    {EffectType.Stats_ChangeEleToNone, 0 },
                    {EffectType.Stats_ChangeEleToMetal, 0 },
                    {EffectType.Stats_ChangeEleToWood, 0 },
                    {EffectType.Stats_ChangeEleToEarth, 0 },
                    {EffectType.Stats_ChangeEleToWater, 0 },
                    {EffectType.Stats_ChangeEleToFire, 0 },
                    {EffectType.Stats_HeavyStand, 0 },
                    {EffectType.Stats_SkillCostReduce, 0 },
                    {EffectType.Stats_SkillAffectEnhance, 0 },
                    {EffectType.Stats_HealingPoint, 0 },
                    {EffectType.Stats_HealingEffect, 0 },
                    {EffectType.Stats_HealingIncome, 0 },
                    {EffectType.Rejuvenate_HealthPotion, 1 },
                    {EffectType.Rejuvenate_ManaPotion, 1 },
                    {EffectType.Rejuvenate_Healing, 1 },
                    {EffectType.StatsAttack_WeaponAttack, 2 },
                    {EffectType.StatsAttack_AttackPower, 2 },
                    {EffectType.StatsAttack_Accuracy, 2 },
                    {EffectType.StatsAttack_Critical, 2 },
                    {EffectType.StatsAttack_CriticalDamage, 2 },
                    {EffectType.StatsAttack_IncSmashDamage, 2 },
                    {EffectType.StatsAttack_IncSliceDamage, 2 },
                    {EffectType.StatsAttack_IncPierceDamage, 2 },
                    {EffectType.StatsAttack_IncEleNoneDamage, 2 },
                    {EffectType.StatsAttack_IncEleMetalDamage, 2 },
                    {EffectType.StatsAttack_IncEleWoodDamage, 2 },
                    {EffectType.StatsAttack_IncEleEarthDamage, 2 },
                    {EffectType.StatsAttack_IncEleWaterDamage, 2 },
                    {EffectType.StatsAttack_IncEleFireDamage, 2 },
                    {EffectType.StatsAttack_VSHumanDamage, 2 },
                    {EffectType.StatsAttack_VSZombieDamage, 2 },
                    {EffectType.StatsAttack_VSVampireDamage, 2 },
                    {EffectType.StatsAttack_VSAnimalDamage, 2 },
                    {EffectType.StatsAttack_VSPlantDamage, 2 },
                    {EffectType.StatsAttack_VSEleNoneDamage, 2 },
                    {EffectType.StatsAttack_VSEleMetalDamage, 2 },
                    {EffectType.StatsAttack_VSEleWoodDamage, 2 },
                    {EffectType.StatsAttack_VSEleEarthDamage, 2 },
                    {EffectType.StatsAttack_VSEleWaterDamage, 2 },
                    {EffectType.StatsAttack_VSEleFireDamage, 2 },
                    {EffectType.StatsAttack_VSBossDamage, 2 },
                    {EffectType.StatsAttack_IncFinalDamage, 2 },
                    {EffectType.StatsDefence_Armor, 3 },
                    {EffectType.StatsDefence_Block, 3 },
                    {EffectType.StatsDefence_BlockValue, 3 },
                    {EffectType.StatsDefence_Evasion, 3 },
                    {EffectType.StatsDefence_CoCritical, 3 },
                    {EffectType.StatsDefence_IncSmashDefence, 3 },
                    {EffectType.StatsDefence_IncSliceDefence, 3 },
                    {EffectType.StatsDefence_IncPierceDefence, 3 },
                    {EffectType.StatsDefence_IncEleNoneDefence, 3 },
                    {EffectType.StatsDefence_IncEleMetalDefence, 3 },
                    {EffectType.StatsDefence_IncEleWoodDefence, 3 },
                    {EffectType.StatsDefence_IncEleEarthDefence, 3 },
                    {EffectType.StatsDefence_IncEleWaterDefence, 3 },
                    {EffectType.StatsDefence_IncEleFireDefence, 3 },
                    {EffectType.StatsDefence_VSHumanDefence, 3 },
                    {EffectType.StatsDefence_VSZombieDefence, 3 },
                    {EffectType.StatsDefence_VSVampireDefence, 3 },
                    {EffectType.StatsDefence_VSAnimalDefence, 3 },
                    {EffectType.StatsDefence_VSPlantDefence, 3 },
                    {EffectType.StatsDefence_DecreaseFinalDamage, 3 }
                }, // positive types // 0 --> Stats type || 1 --> Healing type || 2 --> StatsAttack type || 3 --> StatsDefence type
                new Dictionary<EffectType, byte>() {
                    {EffectType.Stats_AttackSpeed_Debuff, 1 },
                    {EffectType.Stats_CastSpeed_Debuff, 1 },
                    {EffectType.Stats_MoveSpeed_Debuff, 1 },
                    {EffectType.Stats_HealingPoint_Debuff, 1 },
                    {EffectType.Stats_HealingEffect_Debuff, 1 },
                    {EffectType.Stats_HealingIncome_Debuff, 1 },
                    {EffectType.StatsAttack_WeaponAttack_Debuff, 2 },
                    {EffectType.StatsAttack_AttackPower_Debuff, 2 },
                    {EffectType.StatsAttack_Accuracy_Debuff, 2 },
                    {EffectType.StatsAttack_Critical_Debuff, 2 },
                    {EffectType.StatsAttack_CriticalDamage_Debuff, 2 },
                    {EffectType.StatsAttack_IncSmashDamage_Debuff, 2 },
                    {EffectType.StatsAttack_IncSliceDamage_Debuff, 2 },
                    {EffectType.StatsAttack_IncPierceDamage_Debuff, 2 },
                    {EffectType.StatsAttack_IncEleNoneDamage_Debuff, 2 },
                    {EffectType.StatsAttack_IncEleMetalDamage_Debuff, 2 },
                    {EffectType.StatsAttack_IncEleWoodDamage_Debuff, 2 },
                    {EffectType.StatsAttack_IncEleEarthDamage_Debuff, 2 },
                    {EffectType.StatsAttack_IncEleWaterDamage_Debuff, 2 },
                    {EffectType.StatsAttack_IncEleFireDamage_Debuff, 2 },
                    {EffectType.StatsAttack_VSHumanDamage_Debuff, 2 },
                    {EffectType.StatsAttack_VSZombieDamage_Debuff, 2 },
                    {EffectType.StatsAttack_VSVampireDamage_Debuff, 2 },
                    {EffectType.StatsAttack_VSAnimalDamage_Debuff, 2 },
                    {EffectType.StatsAttack_VSPlantDamage_Debuff, 2 },
                    {EffectType.StatsAttack_VSEleNoneDamage_Debuff, 2 },
                    {EffectType.StatsAttack_VSEleMetalDamage_Debuff, 2 },
                    {EffectType.StatsAttack_VsEleWoodDamage_Debuff, 2 },
                    {EffectType.StatsAttack_VSEleEarthDamage_Debuff, 2 },
                    {EffectType.StatsAttack_VSEleWaterDamage_Debuff, 2 },
                    {EffectType.StatsAttack_VSEleFireDamage_Debuff, 2 },
                    {EffectType.StatsDefence_Armor_Debuff, 3 },
                    {EffectType.StatsDefence_Block_Debuff, 3 },
                    {EffectType.StatsDefence_BlockValue_Debuff, 3 },
                    {EffectType.StatsDefence_Evasion_Debuff, 3 },
                    {EffectType.StatsDefence_CoCritical_Debuff, 3 },
                    {EffectType.StatsDefence_IncSmashDefence_Debuff, 3 },
                    {EffectType.StatsDefence_IncSliceDefence_Debuff, 3 },
                    {EffectType.StatsDefence_IncPierceDefence_Debuff, 3 },
                    {EffectType.StatsDefence_IncEleNoneDefence_Debuff, 3 },
                    {EffectType.StatsDefence_IncEleMetalDefence_Debuff, 3 },
                    {EffectType.StatsDefence_IncEleWoodDefence_Debuff, 3 },
                    {EffectType.StatsDefence_IncEleEarthDefence_Debuff, 3 },
                    {EffectType.StatsDefence_IncEleWaterDefence_Debuff, 3 },
                    {EffectType.StatsDefence_IncEleFireDefence_Debuff, 3 },
                    {EffectType.StatsDefence_VSHumanDefence_Debuff, 3 },
                    {EffectType.StatsDefence_VSZombieDefence_Debuff, 3 },
                    {EffectType.StatsDefence_VsVampireDefence_Debuff, 3 },
                    {EffectType.StatsDefence_VSAnimalDefence_Debuff, 3 },
                    {EffectType.StatsDefence_VSPlantDefence_Debuff, 3 }
                }, //negative types // 0 --> Stats type || 1 --> Healing type || 2 --> StatsAttack type || 3 --> StatsDefence type
                new Dictionary<EffectType, byte>() {
                    {EffectType.Control_Stun, 0 },
                    {EffectType.Control_Root, 0 },
                    {EffectType.Control_Fear, 0 },
                    {EffectType.Control_Silence, 0 },
                    {EffectType.Control_Taunt, 0 },
                    {EffectType.Control_BeakBack, 0 },
                    {EffectType.SpecialControl_Freeze, 0 },
                    {EffectType.Immune_AllDamage, 1 },
                    {EffectType.Immune_AllDebuff, 1 },
                    {EffectType.Immune_AllImmune, 1 },
                    {EffectType.Immune_Stun, 1 },
                    {EffectType.Immune_Root, 1 },
                    {EffectType.Immune_Fear, 1 },
                    {EffectType.Immune_Silence, 1 },
                    {EffectType.Immune_Taunt, 1 },
                    {EffectType.Remove_AllControl, 2 },
                    {EffectType.Remove_Stun, 2 },
                    {EffectType.Remove_Root, 2 },
                    {EffectType.Remove_Fear, 2 },
                    {EffectType.Remove_Silence, 2 },
                    {EffectType.Remove_RandomBuff, 2 },
                    {EffectType.Remove_RandomDebuff, 2 },
                    {EffectType.Stealth_Stealth, 3 },
                    {EffectType.Stealth_DetectStealth, 3 },
                    {EffectType.Trigger_OnNormalAttack, 4 },
                    {EffectType.Enhance_IncRepeatSE, 5 },
                    {EffectType.Enhance_IncSkillAffect, 5 }
                } //misc types
        };

        private static Dictionary<EffectType, ControlSEType> m_ControlTypeDictionary = new Dictionary<EffectType, ControlSEType>() {
            {EffectType.Control_Stun, ControlSEType.Stun },
            {EffectType.Control_Root, ControlSEType.Root },
            {EffectType.Control_Fear, ControlSEType.Fear },
            {EffectType.Control_Silence,ControlSEType.Silence },
            {EffectType.Control_Taunt, ControlSEType.Taunt }
        };

        public enum EffectHandleType : byte
        {
            Buff,
            Debuff,
            Control,
            Immune,
            Trigger,
            NonUpdates,
            Updates
        }

        private static Dictionary<EffectType, EffectHandleType> m_EffectHandleTypes = new Dictionary<EffectType, EffectHandleType>()
        {
            {EffectType.Damage_NoElementDamage,             EffectHandleType.NonUpdates},
            {EffectType.Damage_MetalDamage,                 EffectHandleType.NonUpdates },
            {EffectType.Damage_WoodDamage,                  EffectHandleType.NonUpdates },
            {EffectType.Damage_EarthDamage,                 EffectHandleType.NonUpdates },
            {EffectType.Damage_WaterDamage,                 EffectHandleType.NonUpdates },
            {EffectType.Damage_FireDamage,                  EffectHandleType.NonUpdates },
            {EffectType.Damage_DamageBaseOnWeaponElement,   EffectHandleType.NonUpdates },
            {EffectType.Damage_PureDamage,                  EffectHandleType.NonUpdates },

            {EffectType.Stats_Strength,                     EffectHandleType.Buff },
            {EffectType.Stats_Agility,                      EffectHandleType.Buff },
            {EffectType.Stats_Dexterity,                    EffectHandleType.Buff },
            {EffectType.Stats_Constitution,                 EffectHandleType.Buff },
            {EffectType.Stats_Intelligence,                 EffectHandleType.Buff },
            {EffectType.Stats_AttackSpeed,                  EffectHandleType.Buff },
            {EffectType.Stats_AttackSpeed_Debuff,           EffectHandleType.Debuff },
            {EffectType.Stats_CastSpeed,                    EffectHandleType.Buff },
            {EffectType.Stats_CastSpeed_Debuff,             EffectHandleType.Debuff },
            {EffectType.Stats_MoveSpeed,                    EffectHandleType.Buff },
            {EffectType.Stats_MoveSpeed_Debuff,             EffectHandleType.Debuff },
            {EffectType.Stats_ExpBonus,                     EffectHandleType.Buff },
            {EffectType.Stats_MaxHealth,                    EffectHandleType.Buff },
            {EffectType.Stats_HealthRegen,                  EffectHandleType.Buff },
            {EffectType.Stats_MaxMana,                      EffectHandleType.Buff },
            {EffectType.Stats_ManaRegen,                    EffectHandleType.Buff },
            {EffectType.Stats_EnergyShield,                 EffectHandleType.NonUpdates },
            {EffectType.Stats_IgnoreArmor,                  EffectHandleType.Buff },
            {EffectType.Stats_ChangeEleToNone,              EffectHandleType.NonUpdates },
            {EffectType.Stats_ChangeEleToMetal,             EffectHandleType.NonUpdates },
            {EffectType.Stats_ChangeEleToWood,              EffectHandleType.NonUpdates },
            {EffectType.Stats_ChangeEleToEarth,             EffectHandleType.NonUpdates },
            {EffectType.Stats_ChangeEleToWater,             EffectHandleType.NonUpdates },
            {EffectType.Stats_ChangeEleToFire,              EffectHandleType.NonUpdates },
            {EffectType.Stats_HeavyStand,                   EffectHandleType.NonUpdates },
            {EffectType.Stats_SkillCostReduce,              EffectHandleType.Buff },
            {EffectType.Stats_SkillAffectEnhance,           EffectHandleType.Buff },
            {EffectType.Stats_HealingPoint,                 EffectHandleType.Buff },
            {EffectType.Stats_HealingPoint_Debuff,          EffectHandleType.Debuff },
            {EffectType.Stats_HealingEffect,                EffectHandleType.Buff },
            {EffectType.Stats_HealingEffect_Debuff,         EffectHandleType.Debuff },
            {EffectType.Stats_HealingIncome,                EffectHandleType.Buff },
            {EffectType.Stats_HealingIncome_Debuff,         EffectHandleType.Debuff },       

            {EffectType.Rejuvenate_HealthPotion,            EffectHandleType.NonUpdates },
            {EffectType.Rejuvenate_ManaPotion,              EffectHandleType.NonUpdates },
            {EffectType.Rejuvenate_Healing,                 EffectHandleType.NonUpdates },

            {EffectType.StatsAttack_WeaponAttack,               EffectHandleType.Buff },
            {EffectType.StatsAttack_WeaponAttack_Debuff,        EffectHandleType.Debuff },
            {EffectType.StatsAttack_AttackPower,                EffectHandleType.Buff },
            {EffectType.StatsAttack_AttackPower_Debuff,         EffectHandleType.Debuff },
            {EffectType.StatsAttack_Accuracy,                   EffectHandleType.Buff },
            {EffectType.StatsAttack_Accuracy_Debuff,            EffectHandleType.Debuff },
            {EffectType.StatsAttack_Critical,                   EffectHandleType.Buff },
            {EffectType.StatsAttack_Critical_Debuff,            EffectHandleType.Debuff },
            {EffectType.StatsAttack_CriticalDamage,             EffectHandleType.Buff },
            {EffectType.StatsAttack_CriticalDamage_Debuff,      EffectHandleType.Debuff },
            {EffectType.StatsAttack_IncSmashDamage,             EffectHandleType.Buff },
            {EffectType.StatsAttack_IncSmashDamage_Debuff,      EffectHandleType.Debuff },
            {EffectType.StatsAttack_IncSliceDamage,             EffectHandleType.Buff },
            {EffectType.StatsAttack_IncSliceDamage_Debuff,      EffectHandleType.Debuff },
            {EffectType.StatsAttack_IncPierceDamage,            EffectHandleType.Buff },
            {EffectType.StatsAttack_IncPierceDamage_Debuff,     EffectHandleType.Debuff },
            {EffectType.StatsAttack_IncEleNoneDamage,           EffectHandleType.Buff },
            {EffectType.StatsAttack_IncEleNoneDamage_Debuff,    EffectHandleType.Debuff },
            {EffectType.StatsAttack_IncEleMetalDamage,          EffectHandleType.Buff },
            {EffectType.StatsAttack_IncEleMetalDamage_Debuff,   EffectHandleType.Debuff },
            {EffectType.StatsAttack_IncEleWoodDamage,           EffectHandleType.Buff },
            {EffectType.StatsAttack_IncEleWoodDamage_Debuff,    EffectHandleType.Debuff },
            {EffectType.StatsAttack_IncEleEarthDamage,          EffectHandleType.Buff },
            {EffectType.StatsAttack_IncEleEarthDamage_Debuff,   EffectHandleType.Debuff },
            {EffectType.StatsAttack_IncEleWaterDamage,          EffectHandleType.Buff },
            {EffectType.StatsAttack_IncEleWaterDamage_Debuff,   EffectHandleType.Debuff },
            {EffectType.StatsAttack_IncEleFireDamage,           EffectHandleType.Buff },
            {EffectType.StatsAttack_IncEleFireDamage_Debuff,    EffectHandleType.Debuff },
            {EffectType.StatsAttack_VSHumanDamage,              EffectHandleType.Buff },
            {EffectType.StatsAttack_VSHumanDamage_Debuff,       EffectHandleType.Debuff },
            {EffectType.StatsAttack_VSZombieDamage,             EffectHandleType.Buff },
            {EffectType.StatsAttack_VSZombieDamage_Debuff,      EffectHandleType.Debuff },
            {EffectType.StatsAttack_VSVampireDamage,            EffectHandleType.Buff },
            {EffectType.StatsAttack_VSVampireDamage_Debuff,     EffectHandleType.Debuff },
            {EffectType.StatsAttack_VSAnimalDamage,             EffectHandleType.Buff },
            {EffectType.StatsAttack_VSAnimalDamage_Debuff,      EffectHandleType.Debuff },
            {EffectType.StatsAttack_VSPlantDamage,              EffectHandleType.Buff },
            {EffectType.StatsAttack_VSPlantDamage_Debuff,       EffectHandleType.Debuff },
            {EffectType.StatsAttack_VSEleNoneDamage,            EffectHandleType.Buff },
            {EffectType.StatsAttack_VSEleNoneDamage_Debuff,     EffectHandleType.Debuff },
            {EffectType.StatsAttack_VSEleMetalDamage,           EffectHandleType.Buff },
            {EffectType.StatsAttack_VSEleMetalDamage_Debuff,    EffectHandleType.Debuff },
            {EffectType.StatsAttack_VSEleWoodDamage,            EffectHandleType.Buff },
            {EffectType.StatsAttack_VsEleWoodDamage_Debuff,     EffectHandleType.Debuff },
            {EffectType.StatsAttack_VSEleEarthDamage,           EffectHandleType.Buff },
            {EffectType.StatsAttack_VSEleEarthDamage_Debuff,    EffectHandleType.Debuff },
            {EffectType.StatsAttack_VSEleWaterDamage,           EffectHandleType.Buff },
            {EffectType.StatsAttack_VSEleWaterDamage_Debuff,    EffectHandleType.Debuff },
            {EffectType.StatsAttack_VSEleFireDamage,            EffectHandleType.Buff },
            {EffectType.StatsAttack_VSEleFireDamage_Debuff,     EffectHandleType.Debuff },
            {EffectType.StatsAttack_VSBossDamage,               EffectHandleType.Buff },
            {EffectType.StatsAttack_IncFinalDamage,             EffectHandleType.Buff },

            {EffectType.StatsDefence_Armor,                     EffectHandleType.Buff },
            {EffectType.StatsDefence_Armor_Debuff,              EffectHandleType.Debuff },
            {EffectType.StatsDefence_Block,                     EffectHandleType.Buff },
            {EffectType.StatsDefence_Block_Debuff,              EffectHandleType.Debuff },
            {EffectType.StatsDefence_BlockValue,                EffectHandleType.Buff },
            {EffectType.StatsDefence_BlockValue_Debuff,         EffectHandleType.Debuff },
            {EffectType.StatsDefence_Evasion,                   EffectHandleType.Buff },
            {EffectType.StatsDefence_Evasion_Debuff,            EffectHandleType.Debuff },
            {EffectType.StatsDefence_CoCritical,                EffectHandleType.Buff },
            {EffectType.StatsDefence_CoCritical_Debuff,         EffectHandleType.Debuff },
            {EffectType.StatsDefence_IncSmashDefence,           EffectHandleType.Buff },
            {EffectType.StatsDefence_IncSmashDefence_Debuff,    EffectHandleType.Debuff },
            {EffectType.StatsDefence_IncSliceDefence,           EffectHandleType.Buff },
            {EffectType.StatsDefence_IncSliceDefence_Debuff,    EffectHandleType.Debuff },
            {EffectType.StatsDefence_IncPierceDefence,          EffectHandleType.Buff },
            {EffectType.StatsDefence_IncPierceDefence_Debuff,   EffectHandleType.Debuff },
            {EffectType.StatsDefence_IncEleNoneDefence,         EffectHandleType.Buff },
            {EffectType.StatsDefence_IncEleNoneDefence_Debuff,  EffectHandleType.Debuff },
            {EffectType.StatsDefence_IncEleMetalDefence,        EffectHandleType.Buff },
            {EffectType.StatsDefence_IncEleMetalDefence_Debuff, EffectHandleType.Debuff },
            {EffectType.StatsDefence_IncEleWoodDefence,         EffectHandleType.Buff },
            {EffectType.StatsDefence_IncEleWoodDefence_Debuff,  EffectHandleType.Debuff },
            {EffectType.StatsDefence_IncEleEarthDefence,        EffectHandleType.Buff },
            {EffectType.StatsDefence_IncEleEarthDefence_Debuff, EffectHandleType.Debuff },
            {EffectType.StatsDefence_IncEleWaterDefence,        EffectHandleType.Buff },
            {EffectType.StatsDefence_IncEleWaterDefence_Debuff, EffectHandleType.Debuff },
            {EffectType.StatsDefence_IncEleFireDefence,         EffectHandleType.Buff },
            {EffectType.StatsDefence_IncEleFireDefence_Debuff,  EffectHandleType.Debuff },
            {EffectType.StatsDefence_VSHumanDefence,            EffectHandleType.Buff },
            {EffectType.StatsDefence_VSHumanDefence_Debuff,     EffectHandleType.Debuff },
            {EffectType.StatsDefence_VSZombieDefence,           EffectHandleType.Buff },
            {EffectType.StatsDefence_VSZombieDefence_Debuff,    EffectHandleType.Debuff },
            {EffectType.StatsDefence_VSVampireDefence,          EffectHandleType.Buff },
            {EffectType.StatsDefence_VsVampireDefence_Debuff,   EffectHandleType.Debuff },
            {EffectType.StatsDefence_VSAnimalDefence,           EffectHandleType.Buff },
            {EffectType.StatsDefence_VSAnimalDefence_Debuff,    EffectHandleType.Debuff },
            {EffectType.StatsDefence_VSPlantDefence,            EffectHandleType.Buff },
            {EffectType.StatsDefence_VSPlantDefence_Debuff,     EffectHandleType.Debuff },
            {EffectType.StatsDefence_DecreaseFinalDamage,       EffectHandleType.Buff },

            {EffectType.Control_Stun,                           EffectHandleType.Control },
            {EffectType.Control_Root,                           EffectHandleType.Control },
            {EffectType.Control_Fear,                           EffectHandleType.Control },
            {EffectType.Control_Silence,                        EffectHandleType.Control },
            {EffectType.Control_Taunt,                          EffectHandleType.Control }, // needs a new SE
            {EffectType.Control_BeakBack,                       EffectHandleType.Control },
            {EffectType.SpecialControl_Freeze,                  EffectHandleType.Control },

            {EffectType.Immune_AllDamage,                       EffectHandleType.Immune }, // temp only
            {EffectType.Immune_AllDebuff,                       EffectHandleType.Immune },
            {EffectType.Immune_AllImmune,                       EffectHandleType.Immune },
            {EffectType.Immune_Stun,                            EffectHandleType.Immune },
            {EffectType.Immune_Root,                            EffectHandleType.Immune },
            {EffectType.Immune_Fear,                            EffectHandleType.Immune },
            {EffectType.Immune_Silence,                         EffectHandleType.Immune },
            {EffectType.Immune_Taunt,                           EffectHandleType.Immune },

            {EffectType.Remove_AllControl,                      EffectHandleType.NonUpdates },
            {EffectType.Remove_Stun,                            EffectHandleType.NonUpdates },
            {EffectType.Remove_Root,                            EffectHandleType.NonUpdates },
            {EffectType.Remove_Fear,                            EffectHandleType.NonUpdates },
            {EffectType.Remove_Silence,                         EffectHandleType.NonUpdates },
            {EffectType.Remove_RandomBuff,                      EffectHandleType.NonUpdates },
            {EffectType.Remove_RandomDebuff,                    EffectHandleType.NonUpdates },

            {EffectType.Stealth_Stealth,                        EffectHandleType.NonUpdates },
            {EffectType.Stealth_DetectStealth,                  EffectHandleType.NonUpdates },

            {EffectType.Trigger_OnNormalAttack,                 EffectHandleType.Trigger },

            {EffectType.Enhance_IncRepeatSE,                    EffectHandleType.Buff },
            {EffectType.Enhance_IncSkillAffect,                 EffectHandleType.Buff }
        };

        public static ControlSEType EffectTypeToControlSEType(EffectType type) {
            if (m_ControlTypeDictionary.ContainsKey(type))
                return m_ControlTypeDictionary[type];
            return 0;
        }
        public static bool IsPositiveEffectType(EffectType type) {
            return m_SideEffectTypeCollection[1].ContainsKey(type);
        }
        
        public static bool IsNegativeEffectType(EffectType type) {
            return m_SideEffectTypeCollection[2].ContainsKey(type);
        }

        public static byte CheckForEffectType(EffectType type) {
            if (m_SideEffectTypeCollection[1].ContainsKey(type))
                return m_SideEffectTypeCollection[1][type];
            else if (m_SideEffectTypeCollection[2].ContainsKey(type))
                return m_SideEffectTypeCollection[2][type];
            return 0;
        }

        public static void GetStatsFieldAndValue(SideEffectJson seData, ICombatStats combatStats, out FieldName targetedField,out float AmountOut, bool ispos)
        {
            targetedField = FieldName.LastField;
            double amount = GetRandomValue(seData, ispos);

            FieldName relative = FieldName.AbsorbDamage, nonRelative = FieldName.AbsorbDamage;
            switch (seData.effecttype)
            {
                case EffectType.Stats_Strength:
                    relative = FieldName.StrengthPercBonus;
                    nonRelative = FieldName.StrengthBonus;
                    break;
                case EffectType.Stats_Agility:
                    relative = FieldName.AgilityPercBonus;
                    nonRelative = FieldName.AgilityBonus;
                    break;
                case EffectType.Stats_Dexterity:
                    relative = FieldName.DexterityPercBonus;
                    nonRelative = FieldName.DexterityBonus;
                    break;
                case EffectType.Stats_Constitution:
                    relative = FieldName.ConstitutionPercBonus;
                    nonRelative = FieldName.ConstitutionBonus;
                    break;
                case EffectType.Stats_Intelligence:
                    relative = FieldName.IntelligencePercBonus;
                    nonRelative = FieldName.IntelligenceBonus;
                    break;
                case EffectType.Stats_AttackSpeed:
                    relative = FieldName.AttackSpeedBuff;
                    nonRelative = FieldName.AttackSpeedBuff;
                    break;
                case EffectType.Stats_AttackSpeed_Debuff:
                    relative = FieldName.AttackSpeedDebuff;
                    nonRelative = FieldName.AttackSpeedDebuff;
                    break;
                case EffectType.Stats_CastSpeed:
                    relative = FieldName.CastSpeedBuff;
                    nonRelative = FieldName.CastSpeedBuff;
                    break;
                case EffectType.Stats_CastSpeed_Debuff:
                    relative = FieldName.CastSpeedDebuff;
                    nonRelative = FieldName.CastSpeedDebuff;
                    break;
                case EffectType.Stats_MoveSpeed:
                    relative = FieldName.MoveSpeedBuff;
                    nonRelative = FieldName.MoveSpeedBuff;
                    break;
                case EffectType.Stats_MoveSpeed_Debuff:
                    relative = FieldName.MoveSpeedDebuff;
                    nonRelative = FieldName.MoveSpeedDebuff;
                    break;
                case EffectType.Stats_MaxHealth:
                    relative = FieldName.HealthPercBonus;
                    nonRelative = FieldName.HealthBonus;
                    break;
                case EffectType.Stats_HealthRegen:
                    relative = FieldName.HealthRegenPercBonus;
                    nonRelative = FieldName.HealthRegenBonus;
                    break;
                case EffectType.Stats_MaxMana:
                    relative = FieldName.ManaPercBonus;
                    nonRelative = FieldName.ManaBonus;
                    break;
                case EffectType.Stats_ManaRegen:
                    relative = FieldName.ManaReducePercBonus;
                    nonRelative = FieldName.ManaReduceBonus;
                    break;
                case EffectType.Stats_EnergyShield:
                    relative = FieldName.EnergyShield;
                    nonRelative = FieldName.EnergyShield;
                    break;
                case EffectType.Stats_IgnoreArmor:
                    relative = FieldName.IgnoreArmorBonus;
                    nonRelative = FieldName.IgnoreArmorBonus;
                    break;
                case EffectType.Stats_HealingPoint:
                case EffectType.Stats_HealingPoint_Debuff:
                    relative = FieldName.HealingPoint;
                    nonRelative = FieldName.HealingPoint;
                    break;
                case EffectType.Stats_HealingEffect:
                case EffectType.Stats_HealingEffect_Debuff:
                    relative = FieldName.HealingEffect;
                    nonRelative = FieldName.HealingEffect;
                    break;
                case EffectType.Stats_HealingIncome:
                case EffectType.Stats_HealingIncome_Debuff:
                    relative = FieldName.HealingRejuvenation;
                    nonRelative = FieldName.HealingRejuvenation;
                    break;
                case EffectType.StatsAttack_WeaponAttack:
                case EffectType.StatsAttack_WeaponAttack_Debuff:
                    relative = FieldName.WeaponAttackPercBonus;
                    nonRelative = FieldName.WeaponAttackBonus;
                    break;
                case EffectType.StatsAttack_AttackPower:
                case EffectType.StatsAttack_AttackPower_Debuff:
                    relative = FieldName.AttackPercBonus;
                    nonRelative = FieldName.AttackBonus;
                    break;
                case EffectType.StatsAttack_Accuracy:
                case EffectType.StatsAttack_Accuracy_Debuff:
                    relative = FieldName.AccuracyPercBonus;
                    nonRelative = FieldName.AccuracyBonus;
                    break;
                case EffectType.StatsAttack_Critical:
                case EffectType.StatsAttack_Critical_Debuff:
                    relative = FieldName.CriticalPercBonus;
                    nonRelative = FieldName.CriticalBonus;
                    break;
                case EffectType.StatsAttack_CriticalDamage:
                case EffectType.StatsAttack_CriticalDamage_Debuff:
                    relative = FieldName.CriticalDamageBonus;
                    nonRelative = FieldName.CriticalDamageBonus;
                    break;
                case EffectType.StatsAttack_IncSmashDamage:
                case EffectType.StatsAttack_IncSmashDamage_Debuff:
                    relative = FieldName.SmashDamagePercBonus;
                    nonRelative = FieldName.SmashDamageBonus;
                    break;
                case EffectType.StatsAttack_IncSliceDamage:
                case EffectType.StatsAttack_IncSliceDamage_Debuff:
                    relative = FieldName.SliceDamagePercBonus;
                    nonRelative = FieldName.SliceDamageBonus;
                    break;
                case EffectType.StatsAttack_IncPierceDamage:
                case EffectType.StatsAttack_IncPierceDamage_Debuff:
                    relative = FieldName.PierceDamagePercBonus;
                    nonRelative = FieldName.PierceDamageBonus;
                    break;
                case EffectType.StatsAttack_IncEleNoneDamage:
                case EffectType.StatsAttack_IncEleNoneDamage_Debuff:
                    relative = FieldName.NullDamagePercBonus;
                    nonRelative = FieldName.NullDamageBonus;
                    break;
                case EffectType.StatsAttack_IncEleMetalDamage:
                case EffectType.StatsAttack_IncEleMetalDamage_Debuff:
                    relative = FieldName.MetalDamagePercBonus;
                    nonRelative = FieldName.MetalDamageBonus;
                    break;
                case EffectType.StatsAttack_IncEleWoodDamage:
                case EffectType.StatsAttack_IncEleWoodDamage_Debuff:
                    relative = FieldName.WoodDamagePercBonus;
                    nonRelative = FieldName.WoodDamageBonus;
                    break;
                case EffectType.StatsAttack_IncEleEarthDamage:
                case EffectType.StatsAttack_IncEleEarthDamage_Debuff:
                    relative = FieldName.EarthDamagePercBonus;
                    nonRelative = FieldName.EarthDamageBonus;
                    break;
                case EffectType.StatsAttack_IncEleWaterDamage:
                case EffectType.StatsAttack_IncEleWaterDamage_Debuff:
                    relative = FieldName.WaterDamagePercBonus;
                    nonRelative = FieldName.WaterDamageBonus;
                    break;
                case EffectType.StatsAttack_IncEleFireDamage:
                case EffectType.StatsAttack_IncEleFireDamage_Debuff:
                    relative = FieldName.FireDamagePercBonus;
                    nonRelative = FieldName.FireDamageBonus;
                    break;
                case EffectType.StatsAttack_VSHumanDamage:
                case EffectType.StatsAttack_VSHumanDamage_Debuff:
                    relative = FieldName.VSHumanDamagePercBonus;
                    nonRelative = FieldName.VSHumanDamageBonus;
                    break;
                case EffectType.StatsAttack_VSZombieDamage:
                case EffectType.StatsAttack_VSZombieDamage_Debuff:
                    relative = FieldName.VSZombieDamagePercBonus;
                    nonRelative = FieldName.VSZombieDamageBonus;
                    break;
                case EffectType.StatsAttack_VSVampireDamage:
                case EffectType.StatsAttack_VSVampireDamage_Debuff:
                    relative = FieldName.VSVampireDamagePercBonus;
                    nonRelative = FieldName.VSVampireDamageBonus;
                    break;
                case EffectType.StatsAttack_VSAnimalDamage:
                case EffectType.StatsAttack_VSAnimalDamage_Debuff:
                    relative = FieldName.VSBeastDamagePercBonus;
                    nonRelative = FieldName.VSBeastDamageBonus;
                    break;
                case EffectType.StatsAttack_VSPlantDamage:
                case EffectType.StatsAttack_VSPlantDamage_Debuff:
                    relative = FieldName.VSPlantDamagePercBonus;
                    nonRelative = FieldName.VSPlantDamageBonus;
                    break;
                case EffectType.StatsAttack_VSEleNoneDamage:
                case EffectType.StatsAttack_VSEleNoneDamage_Debuff:
                    relative = FieldName.VSNullPercDamage;
                    nonRelative = FieldName.VSNullPercDamage;
                    break;
                case EffectType.StatsAttack_VSEleMetalDamage:
                case EffectType.StatsAttack_VSEleMetalDamage_Debuff:
                    relative = FieldName.VSMetalPercDamage;
                    nonRelative = FieldName.VSMetalPercDamage;
                    break;
                case EffectType.StatsAttack_VSEleWoodDamage:
                case EffectType.StatsAttack_VsEleWoodDamage_Debuff:
                    relative = FieldName.VSWoodPercDamage;
                    nonRelative = FieldName.VSWoodPercDamage;
                    break;
                case EffectType.StatsAttack_VSEleEarthDamage:
                case EffectType.StatsAttack_VSEleEarthDamage_Debuff:
                    relative = FieldName.VSEarthPercDamage;
                    nonRelative = FieldName.VSEarthPercDamage;
                    break;
                case EffectType.StatsAttack_VSEleWaterDamage:
                case EffectType.StatsAttack_VSEleWaterDamage_Debuff:
                    relative = FieldName.VSWaterPercDamage;
                    nonRelative = FieldName.VSWaterDamage;
                    break;
                case EffectType.StatsAttack_VSEleFireDamage:
                case EffectType.StatsAttack_VSEleFireDamage_Debuff:
                    relative = FieldName.VSFirePercDamage;
                    nonRelative = FieldName.VSFirePercDamage;
                    break;
                case EffectType.StatsAttack_VSBossDamage:
                    relative = FieldName.VSBossDamageBonus;
                    nonRelative = FieldName.VSBossDamageBonus;
                    break;
                case EffectType.StatsAttack_IncFinalDamage:
                    relative = FieldName.IncreaseFinalDamageBonus;
                    nonRelative = FieldName.IncreaseFinalDamageBonus;
                    break;
                case EffectType.StatsDefence_Armor:
                case EffectType.StatsDefence_Armor_Debuff:
                    relative = FieldName.ArmorPercBonus;
                    nonRelative = FieldName.ArmorBonus;
                    break;
                case EffectType.StatsDefence_Block:
                case EffectType.StatsDefence_Block_Debuff:
                    relative = FieldName.BlockRate;
                    nonRelative = FieldName.BlockRate;
                    break;
                case EffectType.StatsDefence_BlockValue:
                case EffectType.StatsDefence_BlockValue_Debuff:
                    relative = FieldName.BlockValuePercBonus;
                    nonRelative = FieldName.BlockValueBonus;
                    break;
                case EffectType.StatsDefence_Evasion:
                case EffectType.StatsDefence_Evasion_Debuff:
                    relative = FieldName.EvasionPercBonus;
                    nonRelative = FieldName.EvasionBonus;
                    break;
                case EffectType.StatsDefence_CoCritical:
                case EffectType.StatsDefence_CoCritical_Debuff:
                    relative = FieldName.CocriticalBonus;
                    nonRelative = FieldName.CocriticalBonus;
                    break;
                case EffectType.StatsDefence_IncSmashDefence:
                case EffectType.StatsDefence_IncSmashDefence_Debuff:
                    relative = FieldName.SmashDefensePercBonus;
                    nonRelative = FieldName.SmashDefensePercBonus_NoScore;
                    break;
                case EffectType.StatsDefence_IncSliceDefence:
                case EffectType.StatsDefence_IncSliceDefence_Debuff:
                    relative = FieldName.SliceDefensePercBonus;
                    nonRelative = FieldName.SliceDefenseBonus;
                    break;
                case EffectType.StatsDefence_IncPierceDefence:
                case EffectType.StatsDefence_IncPierceDefence_Debuff:
                    relative = FieldName.PierceDefensePercBonus;
                    nonRelative = FieldName.PierceDefenseBonus;
                    break;
                case EffectType.StatsDefence_IncEleNoneDefence:
                case EffectType.StatsDefence_IncEleNoneDefence_Debuff:
                    relative = FieldName.NullDefensePercBonus;
                    nonRelative = FieldName.NullDefenseBonus;
                    break;
                case EffectType.StatsDefence_IncEleMetalDefence:
                case EffectType.StatsDefence_IncEleMetalDefence_Debuff:
                    relative = FieldName.MetalDefensePercBonus;
                    nonRelative = FieldName.MetalDefenseBonus;
                    break;
                case EffectType.StatsDefence_IncEleWoodDefence:
                case EffectType.StatsDefence_IncEleWoodDefence_Debuff:
                    relative = FieldName.WoodDefensePercBonus;
                    nonRelative = FieldName.WoodDefenseBonus;
                    break;
                case EffectType.StatsDefence_IncEleEarthDefence:
                case EffectType.StatsDefence_IncEleEarthDefence_Debuff:
                    relative = FieldName.EarthDefensePercBonus;
                    nonRelative = FieldName.EarthDefenseBonus;
                    break;
                case EffectType.StatsDefence_IncEleWaterDefence:
                case EffectType.StatsDefence_IncEleWaterDefence_Debuff:
                    relative = FieldName.WaterDefensePercBonus;
                    nonRelative = FieldName.WaterDefenseBonus;
                    break;
                case EffectType.StatsDefence_IncEleFireDefence:
                case EffectType.StatsDefence_IncEleFireDefence_Debuff:
                    relative = FieldName.FireDefensePercBonus;
                    nonRelative = FieldName.FireDefenseBonus;
                    break;
                case EffectType.StatsDefence_VSHumanDefence:
                case EffectType.StatsDefence_VSHumanDefence_Debuff:
                    relative = FieldName.VSHumanDefensePercBonus;
                    nonRelative = FieldName.VSHumanDefenseBonus;
                    break;
                case EffectType.StatsDefence_VSZombieDefence:
                case EffectType.StatsDefence_VSZombieDefence_Debuff:
                    relative = FieldName.VSZombieDefensePercBonus;
                    nonRelative = FieldName.VSZombieDefenseBonus;
                    break;
                case EffectType.StatsDefence_VSVampireDefence:
                case EffectType.StatsDefence_VsVampireDefence_Debuff:
                    relative = FieldName.VSVampireDefensePercBonus;
                    nonRelative = FieldName.VSVampireDefenseBonus;
                    break;
                case EffectType.StatsDefence_VSAnimalDefence:
                case EffectType.StatsDefence_VSAnimalDefence_Debuff:
                    relative = FieldName.VSBeastDefensePercBonus;
                    nonRelative = FieldName.VSBeastDefenseBonus;
                    break;
                case EffectType.StatsDefence_VSPlantDefence:
                case EffectType.StatsDefence_VSPlantDefence_Debuff:
                    relative = FieldName.VSPlantDefensePercBonus;
                    nonRelative = FieldName.VSPlantDefenseBonus;
                    break;
                case EffectType.StatsDefence_DecreaseFinalDamage:
                    relative = FieldName.DecreaseFinalDamageBonus;
                    nonRelative = FieldName.DecreaseFinalDamageBonus;
                    break;
            }

            if (seData.isrelative) {
                targetedField = relative;
                //amount *= 10;
            }
            else
                targetedField = nonRelative;
            
            AmountOut = (float)amount;
        }

        private static double GetRandomValue(SideEffectJson mSideeffectData, bool ispos)
        {
            if (mSideeffectData.isrelative)
            {
                double res = (GameUtils.Random(mSideeffectData.min, mSideeffectData.max));
                if (ispos)
                    return res;
                else
                    return -res;
            }
            else
            {
                double res = (GameUtils.Random(mSideeffectData.min, mSideeffectData.max));
                if (ispos)
                    return res;
                else
                    return -res;
            }
        }

        public static FieldName GetNoScroreField(FieldName field)
        {
            FieldName res = FieldName.LastField;
            if (field == FieldName.HealthBonus)
                res = FieldName.HealthBonus_NoScore;
            else if (field == FieldName.HealthPercBonus)
                res = FieldName.HealthPercBonus_NoScore;
            else if (field == FieldName.ArmorBonus)
                res = FieldName.ArmorBonus_NoScore;
            else if (field == FieldName.ArmorPercBonus)
                res = FieldName.ArmorPercBonus_NoScore;
            else if (field == FieldName.CocriticalBonus)
                res = FieldName.CocriticalBonus_NoScore;
            else if (field == FieldName.CocriticalPercBonus)
                res = FieldName.CocriticalPercBonus_NoScore;
            //else if (field == FieldName.CoCriticalDamageBonus)
            //    res = FieldName.CoCriticalDamageBonus_NoScore;
            //else if (field == FieldName.CoCriticalDamagePercBonus)
            //    res = FieldName.CoCriticalDamagePercBonus_NoScore;
            else if (field == FieldName.EvasionBonus)
                res = FieldName.EvasionBonus_NoScore;
            else if (field == FieldName.EvasionPercBonus)
                res = FieldName.EvasionPercBonus_NoScore;
            else if (field == FieldName.AccuracyBonus)
                res = FieldName.AccuracyBonus_NoScore;
            else if (field == FieldName.AccuracyPercBonus)
                res = FieldName.AccuracyPercBonus_NoScore;
            else if (field == FieldName.AttackBonus)
                res = FieldName.AttackBonus_NoScore;
            else if (field == FieldName.AttackPercBonus)
                res = FieldName.AttackPercBonus_NoScore;
            else if (field == FieldName.CriticalBonus)
                res = FieldName.CriticalBonus_NoScore;
            else if (field == FieldName.CriticalPercBonus)
                res = FieldName.CriticalPercBonus_NoScore;
            else if (field == FieldName.CriticalDamageBonus)
                res = FieldName.CriticalDamageBonus_NoScore;
            //else if (field == FieldName.CriticalDamagePercBonus)
            //    res = FieldName.CriticalDamagePercBonus_NoScore; 
            return res;
        }

        public static bool IsSideEffectPositive(SideEffectJson sej)
        {
            //list down all side effect which is able to be instantiated and is friendly
            bool res =
                sej.effecttype == EffectType.Stats_Strength ||
                sej.effecttype == EffectType.Stats_Agility ||
                sej.effecttype == EffectType.Stats_Dexterity ||
                sej.effecttype == EffectType.Stats_Constitution ||
                sej.effecttype == EffectType.Stats_Intelligence ||
                sej.effecttype == EffectType.Stats_AttackSpeed ||
                sej.effecttype == EffectType.Stats_CastSpeed ||
                sej.effecttype == EffectType.Stats_MoveSpeed ||
                sej.effecttype == EffectType.Stats_HealingPoint ||
                sej.effecttype == EffectType.Stats_HealingEffect ||
                sej.effecttype == EffectType.Stats_HealingIncome ||
                sej.effecttype == EffectType.StatsAttack_WeaponAttack ||
                sej.effecttype == EffectType.StatsAttack_AttackPower ||
                sej.effecttype == EffectType.StatsAttack_Accuracy ||
                sej.effecttype == EffectType.StatsAttack_Critical ||
                sej.effecttype == EffectType.StatsAttack_CriticalDamage ||
                sej.effecttype == EffectType.StatsAttack_IncSmashDamage ||
                sej.effecttype == EffectType.StatsAttack_IncSliceDamage ||
                sej.effecttype == EffectType.StatsAttack_IncPierceDamage ||
                sej.effecttype == EffectType.StatsAttack_IncEleNoneDamage ||
                sej.effecttype == EffectType.StatsAttack_IncEleMetalDamage ||
                sej.effecttype == EffectType.StatsAttack_IncEleWoodDamage ||
                sej.effecttype == EffectType.StatsAttack_IncEleEarthDamage ||
                sej.effecttype == EffectType.StatsAttack_IncEleWaterDamage ||
                sej.effecttype == EffectType.StatsAttack_IncEleFireDamage ||
                sej.effecttype == EffectType.StatsAttack_VSHumanDamage ||
                sej.effecttype == EffectType.StatsAttack_VSZombieDamage ||
                sej.effecttype == EffectType.StatsAttack_VSVampireDamage ||
                sej.effecttype == EffectType.StatsAttack_VSAnimalDamage ||
                sej.effecttype == EffectType.StatsAttack_VSPlantDamage ||
                sej.effecttype == EffectType.StatsAttack_VSEleNoneDamage ||
                sej.effecttype == EffectType.StatsAttack_VSEleMetalDamage ||
                sej.effecttype == EffectType.StatsAttack_VSEleWoodDamage ||
                sej.effecttype == EffectType.StatsAttack_VSEleEarthDamage ||
                sej.effecttype == EffectType.StatsAttack_VSEleWoodDamage ||
                sej.effecttype == EffectType.StatsAttack_VSEleFireDamage ||
                sej.effecttype == EffectType.StatsDefence_Armor ||
                sej.effecttype == EffectType.StatsDefence_Block ||
                sej.effecttype == EffectType.StatsDefence_BlockValue ||
                sej.effecttype == EffectType.StatsDefence_Evasion ||
                sej.effecttype == EffectType.StatsDefence_CoCritical ||
                sej.effecttype == EffectType.StatsDefence_IncSmashDefence ||
                sej.effecttype == EffectType.StatsDefence_IncSliceDefence ||
                sej.effecttype == EffectType.StatsDefence_IncPierceDefence ||
                sej.effecttype == EffectType.StatsDefence_IncEleNoneDefence ||
                sej.effecttype == EffectType.StatsDefence_IncEleMetalDefence ||
                sej.effecttype == EffectType.StatsDefence_IncEleWoodDefence ||
                sej.effecttype == EffectType.StatsDefence_IncEleEarthDefence ||
                sej.effecttype == EffectType.StatsDefence_IncEleWaterDefence ||
                sej.effecttype == EffectType.StatsDefence_IncEleFireDefence ||
                sej.effecttype == EffectType.StatsDefence_VSHumanDefence ||
                sej.effecttype == EffectType.StatsDefence_VSZombieDefence ||
                sej.effecttype == EffectType.StatsDefence_VSVampireDefence ||
                sej.effecttype == EffectType.StatsDefence_VSAnimalDefence ||
                sej.effecttype == EffectType.StatsDefence_VSPlantDefence ||
                sej.effecttype == EffectType.StatsDefence_AmplifyDamage;

            return res;
        }

        public static bool IsEffectiveSubSkill(SideEffectJson sej)
        {
            bool res = false;
            //list down all sideeffect which may be instantiated in subskills.
            switch (sej.effecttype)
            {
                //case EffectType.Rejuvenate_Health:
                //case EffectType.Remove_DeBuff:
                //case EffectType.Remove_Buff:
                //case EffectType.Remove_Random_Buff:
                //case EffectType.Remove_Random_DeBuff:
                //case EffectType.Remove_All_Immune:
                //case EffectType.Passive_Increase_SkillCD:
                case EffectType.Control_Stun:
                case EffectType.Control_Silence:
                //case EffectType.Control_Disarmed:
                //case EffectType.Control_KnockBack:
                //case EffectType.BasicAttack_Supress: 
                    res = true;
                    break;
            }
            return res;
        }

        public static EffectHandleType GetEffectHandleType(EffectType type)
        {
            return m_EffectHandleTypes[type];
        }

        public static void ClientAddSideEffect(Actor target, Actor caster, SideEffectJson sideeffect)
        {
            if (SideEffectFactory.IsSideEffectInstantiatable(sideeffect))
            {
                SideEffect se = SideEffectFactory.CreateSideEffect(sideeffect, SEORIGINID.NONE);
                bool ispos = IsSideEffectPositive(sideeffect);
                if (sideeffect.delay != 0)
                {
                    System.Timers.Timer delay = new System.Timers.Timer(sideeffect.delay);
                    delay.Elapsed += delegate { se.Apply(target, caster, ispos); };
                    delay.AutoReset = false;
                    delay.Start();
                }
                else
                    se.Apply(target, caster, ispos);
            }
        }

        public static void ClientRemoveSideEffect(Actor target, Actor caster, SideEffectJson sideeffect)
        {
            if (SideEffectFactory.IsSideEffectInstantiatable(sideeffect))
            {
                SideEffect se = SideEffectFactory.CreateSideEffect(sideeffect, SEORIGINID.NONE);
                bool ispos = !IsSideEffectPositive(sideeffect);
                if (sideeffect.delay != 0)
                {
                    System.Timers.Timer delay = new System.Timers.Timer(sideeffect.delay);
                    delay.Elapsed += delegate { se.Apply(target, caster, ispos); };
                    delay.AutoReset = false;
                    delay.Start();
                }
                else
                    se.Apply(target, caster, ispos);
            }
        }

        public static void ClientUpdateEquipmentSideEffects(Actor caster, List<int> sideEffectIds, int equipId, bool isAdd)
        {
            int count = sideEffectIds.Count;
            for (int i = 0; i < count; ++i)
            {
                SideEffectJson selfse = SideEffectRepo.GetSideEffect(sideEffectIds[i]);
                if (SideEffectFactory.IsSideEffectInstantiatable(selfse))
                {
                    SideEffect se = SideEffectFactory.CreateSideEffect(selfse, SEORIGINID.NONE);
                    se.Apply(caster, caster, isAdd, equipId);
                }
            }
        }
    }
}
