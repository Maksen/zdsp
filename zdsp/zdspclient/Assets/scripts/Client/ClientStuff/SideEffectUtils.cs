using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Repository;

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

    public static string GetEffectTypeLocalizedName(EffectType effectType)
    {
        string guiname = "";
        switch (effectType)
        {
            case EffectType.Stats_Strength:
                guiname = "ci_strength";
                break;
            case EffectType.Stats_Agility:
                guiname = "ci_agility";
                break;
            case EffectType.Stats_Dexterity:
                guiname = "ci_dexterity";
                break;
            case EffectType.Stats_Constitution:
                guiname = "ci_constitution";
                break;
            case EffectType.Stats_Intelligence:
                guiname = "ci_inteligence";
                break;
            case EffectType.Stats_MoveSpeed:
                guiname = "stats_movespeed";
                break;
            case EffectType.Stats_ExpBonus:
                guiname = "stats_expbonus";
                break;
            case EffectType.Stats_MaxHealth:
                guiname = "stats_maxhealth";
                break;
            case EffectType.Stats_HealthRegen:
                guiname = "stats_healthregen";
                break;
            case EffectType.Stats_MaxMana:
                guiname = "stats_maxmana";
                break;
            case EffectType.Stats_ManaRegen:
                guiname = "stats_manaregen";
                break;
            case EffectType.Stats_IgnoreArmor:
                guiname = "stats_ignorearmor";
                break;
            case EffectType.StatsAttack_WeaponAttack:
                guiname = "stats_weaponattack";
                break;
            case EffectType.StatsAttack_AttackPower:
                guiname = "stats_attackpower";
                break;
            case EffectType.StatsAttack_Accuracy:
                guiname = "stats_accuracy";
                break;
            case EffectType.StatsAttack_Critical:
                guiname = "stats_critical";
                break;
            case EffectType.StatsAttack_CriticalDamage:
                guiname = "stats_criticaldamage";
                break;
            case EffectType.StatsAttack_IncSmashDamage:
                guiname = "stats_incsmashdmg";
                break;
            case EffectType.StatsAttack_IncSliceDamage:
                guiname = "stats_incslicedmg";
                break;
            case EffectType.StatsAttack_IncPierceDamage:
                guiname = "stats_incpiercedmg";
                break;
            case EffectType.StatsAttack_IncEleNoneDamage:
                guiname = "stats_incelenonedmg";
                break;
            case EffectType.StatsAttack_IncEleMetalDamage:
                guiname = "stats_incelemateldmg";
                break;
            case EffectType.StatsAttack_IncEleWoodDamage:
                guiname = "stats_incelewooddmg";
                break;
            case EffectType.StatsAttack_IncEleEarthDamage:
                guiname = "stats_inceleearthdmg";
                break;
            case EffectType.StatsAttack_IncEleWaterDamage:
                guiname = "stats_incelewaterdmg";
                break;
            case EffectType.StatsAttack_IncEleFireDamage:
                guiname = "stats_incelefiredmg";
                break;
            case EffectType.StatsAttack_VSHumanDamage:
                guiname = "stats_vshumandmg";
                break;
            case EffectType.StatsAttack_VSZombieDamage:
                guiname = "stats_vszombiedmg";
                break;
            case EffectType.StatsAttack_VSVampireDamage:
                guiname = "stats_vsvampiredmg";
                break;
            case EffectType.StatsAttack_VSAnimalDamage:
                guiname = "stats_vsanimaldmg";
                break;
            case EffectType.StatsAttack_VSPlantDamage:
                guiname = "stats_vsplantdmg";
                break;
            case EffectType.StatsAttack_VSEleNoneDamage:
                guiname = "stats_vselenonedmg";
                break;
            case EffectType.StatsAttack_VSEleMetalDamage:
                guiname = "stats_vselemateldmg";
                break;
            case EffectType.StatsAttack_VSEleWoodDamage:
                guiname = "stats_vselewooddmg";
                break;
            case EffectType.StatsAttack_VSEleEarthDamage:
                guiname = "stats_vseleearthdmg";
                break;
            case EffectType.StatsAttack_VSEleWaterDamage:
                guiname = "stats_vselewaterdmg";
                break;
            case EffectType.StatsAttack_VSEleFireDamage:
                guiname = "stats_vselefiredmg";
                break;
            case EffectType.StatsAttack_VSBossDamage:
                guiname = "stats_vsbossdmg";
                break;
            case EffectType.StatsAttack_IncFinalDamage:
                guiname = "stats_incfinaldmg";
                break;
            case EffectType.StatsDefence_Armor:
                guiname = "stats_armor";
                break;
            case EffectType.StatsDefence_Block:
                guiname = "stats_block";
                break;
            case EffectType.StatsDefence_BlockValue:
                guiname = "stats_blockvalue";
                break;
            case EffectType.StatsDefence_Evasion:
                guiname = "stats_evasion";
                break;
            case EffectType.StatsDefence_CoCritical:
                guiname = "stats_cocritical";
                break;
            case EffectType.StatsDefence_IncSmashDefence:
                guiname = "stats_incsmashdef";
                break;
            case EffectType.StatsDefence_IncSliceDefence:
                guiname = "stats_incslicedef";
                break;
            case EffectType.StatsDefence_IncPierceDefence:
                guiname = "stats_incpiercedef";
                break;
            case EffectType.StatsDefence_IncEleNoneDefence:
                guiname = "stats_incelenonedef";
                break;
            case EffectType.StatsDefence_IncEleMetalDefence:
                guiname = "stats_incelemateldef";
                break;
            case EffectType.StatsDefence_IncEleWoodDefence:
                guiname = "stats_incelewooddef";
                break;
            case EffectType.StatsDefence_IncEleEarthDefence:
                guiname = "stats_inceleearthdef";
                break;
            case EffectType.StatsDefence_IncEleWaterDefence:
                guiname = "stats_incelewaterdef";
                break;
            case EffectType.StatsDefence_IncEleFireDefence:
                guiname = "stats_incelefiredef";
                break;
            case EffectType.StatsDefence_VSHumanDefence:
                guiname = "stats_vshumandef";
                break;
            case EffectType.StatsDefence_VSZombieDefence:
                guiname = "stats_vszombiedef";
                break;
            case EffectType.StatsDefence_VSVampireDefence:
                guiname = "stats_vsvampiredef";
                break;
            case EffectType.StatsDefence_VSAnimalDefence:
                guiname = "stats_vsanimaldef";
                break;
            case EffectType.StatsDefence_VSPlantDefence:
                guiname = "stats_vsplantdef";
                break;
            case EffectType.StatsDefence_DecreaseFinalDamage:
                guiname = "stats_decfinaldmg";
                break;
        }

        if (!string.IsNullOrEmpty(guiname))
            return GUILocalizationRepo.GetLocalizedString(guiname);
        else
            return effectType.ToString();
    }

    public static void AddToBuffDict(Dictionary<EffectType, ValuePair<float, float>> buffDict, SideEffectJson se)
    {
        if (se == null)
            return;

        ValuePair<float, float> buffPair;
        if (buffDict.TryGetValue(se.effecttype, out buffPair))
        {
            if (se.isrelative)
                buffPair.Item2 += se.max;
            else
                buffPair.Item1 += se.max;
        }
        else
        {
            if (se.isrelative)
                buffDict.Add(se.effecttype, new ValuePair<float, float>(0, se.max));
            else
                buffDict.Add(se.effecttype, new ValuePair<float, float>(se.max, 0));
        }
    }
}