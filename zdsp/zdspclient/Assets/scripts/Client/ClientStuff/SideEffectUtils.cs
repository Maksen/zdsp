using System.Collections.Generic;
using Zealot.Common;

public static class SideEffectUtils
{
    public static readonly List<EffectType>[] buffTypeGroups = new List<EffectType>[11]
    {
        new List<EffectType>()
        {
            EffectType.Stats_Strength,
            EffectType.Stats_Agility,
            EffectType.Stats_Dexterity,
            EffectType.Stats_Constitution,
            EffectType.Stats_Intelligence
        },

        new List<EffectType>()
        {
            EffectType.Stats_MaxHealth,
            EffectType.Stats_HealthRegen,
            EffectType.Stats_MaxMana,
            EffectType.Stats_ManaRegen,
            EffectType.Stats_MoveSpeed,
            EffectType.Stats_ExpBonus,
            EffectType.StatsAttack_WeaponAttack,
            EffectType.StatsAttack_AttackPower,
            EffectType.StatsDefence_Armor,
            EffectType.Stats_IgnoreArmor,
            EffectType.StatsDefence_Block,
            EffectType.StatsDefence_BlockValue,
            EffectType.StatsAttack_Accuracy,
            EffectType.StatsDefence_Evasion,
            EffectType.StatsAttack_Critical,
            EffectType.StatsDefence_CoCritical,
            EffectType.StatsAttack_CriticalDamage
        },

        new List<EffectType>()
        {
            EffectType.StatsAttack_IncSmashDamage,
            EffectType.StatsAttack_IncSliceDamage,
            EffectType.StatsAttack_IncPierceDamage
        },

        new List<EffectType>()
        {
            EffectType.StatsAttack_IncEleNoneDamage,
            EffectType.StatsAttack_IncEleMetalDamage,
            EffectType.StatsAttack_IncEleWoodDamage,
            EffectType.StatsAttack_IncEleEarthDamage,
            EffectType.StatsAttack_IncEleWaterDamage,
            EffectType.StatsAttack_IncEleFireDamage
        },

        new List<EffectType>()
        {
            EffectType.StatsAttack_VSHumanDamage,
            EffectType.StatsAttack_VSZombieDamage,
            EffectType.StatsAttack_VSVampireDamage,
            EffectType.StatsAttack_VSAnimalDamage,
            EffectType.StatsAttack_VSPlantDamage
        },

        new List<EffectType>()
        {
            EffectType.StatsAttack_VSEleNoneDamage,
            EffectType.StatsAttack_VSEleMetalDamage,
            EffectType.StatsAttack_VSEleWoodDamage,
            EffectType.StatsAttack_VSEleEarthDamage,
            EffectType.StatsAttack_VSEleWaterDamage,
            EffectType.StatsAttack_VSEleFireDamage
        },

        new List<EffectType>()
        {
            EffectType.StatsAttack_VSBossDamage,
            EffectType.StatsAttack_IncFinalDamage
        },

        new List<EffectType>()
        {
            EffectType.StatsDefence_IncSmashDefence,
            EffectType.StatsDefence_IncSliceDefence,
            EffectType.StatsDefence_IncPierceDefence
        },

        new List<EffectType>()
        {
            EffectType.StatsDefence_IncEleNoneDefence,
            EffectType.StatsDefence_IncEleMetalDefence,
            EffectType.StatsDefence_IncEleWoodDefence,
            EffectType.StatsDefence_IncEleEarthDefence,
            EffectType.StatsDefence_IncEleWaterDefence,
            EffectType.StatsDefence_IncEleFireDefence
        },

        new List<EffectType>()
        {
            EffectType.StatsDefence_VSHumanDefence,
            EffectType.StatsDefence_VSZombieDefence,
            EffectType.StatsDefence_VSVampireDefence,
            EffectType.StatsDefence_VSAnimalDefence,
            EffectType.StatsDefence_VSPlantDefence
        },

        new List<EffectType>()
        {
            EffectType.StatsDefence_DecreaseFinalDamage
        }
    };
}