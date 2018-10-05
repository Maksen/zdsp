//#define _ENABLE_GET_PROPERTY_

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using Zealot.Repository;

public class UI_CharacterInfoTabOne : MonoBehaviour
{
    public enum Enum_CharInfoTabOneStats
    {
        MAXHP = 0,
        HPREGEN,
        MAXMANA,
        MANAREGEN,
        MOVESPD,
        EXPBONUS,
        WEAPONATK,
        ATKPOWER,
        ARMOR,
        IGNOREARMOR,
        BLOCK,
        BLOCKVALUE,
        ACCURACY,
        EVASION,
        CRITICAL,
        COCRITICAL,
        CRITICALDAMAGE,

        NUM_STATS
    };

    [SerializeField]
    GameObject mTabOnePrefab;
    [SerializeField]
    GameObject mBasicStatsHeader;
    List<UI_CharacterInfo_BasicStats> mStatLst = new List<UI_CharacterInfo_BasicStats>();

    public void Awake()
    {
        GameObject obj = null;
        UI_CharacterInfo_BasicStats stat = null;
        int max = (int)Enum_CharInfoTabOneStats.NUM_STATS;
        for (int i = 0; i < max; ++i)
        {
            obj = Instantiate(mTabOnePrefab, Vector3.zero, Quaternion.identity) as GameObject;
            obj.transform.SetParent(mBasicStatsHeader.transform, false);
            stat = obj.GetComponent<UI_CharacterInfo_BasicStats>();

            mStatLst.Add(stat);
        }

        mStatLst[(int)Enum_CharInfoTabOneStats.MAXHP].CombineName = GUILocalizationRepo.GetLocalizedString("stats_maxhealth");
        mStatLst[(int)Enum_CharInfoTabOneStats.HPREGEN].CombineName = GUILocalizationRepo.GetLocalizedString("stats_healthregen");
        mStatLst[(int)Enum_CharInfoTabOneStats.MAXMANA].CombineName = GUILocalizationRepo.GetLocalizedString("stats_maxmana");
        mStatLst[(int)Enum_CharInfoTabOneStats.MANAREGEN].CombineName = GUILocalizationRepo.GetLocalizedString("stats_manaregen");
        mStatLst[(int)Enum_CharInfoTabOneStats.MOVESPD].CombineName = GUILocalizationRepo.GetLocalizedString("stats_movespeed");
        mStatLst[(int)Enum_CharInfoTabOneStats.EXPBONUS].CombineName = GUILocalizationRepo.GetLocalizedString("stats_expbonus");
        mStatLst[(int)Enum_CharInfoTabOneStats.WEAPONATK].CombineName = GUILocalizationRepo.GetLocalizedString("stats_weaponattack");
        mStatLst[(int)Enum_CharInfoTabOneStats.ATKPOWER].CombineName = GUILocalizationRepo.GetLocalizedString("stats_attackpower");
        mStatLst[(int)Enum_CharInfoTabOneStats.ARMOR].CombineName = GUILocalizationRepo.GetLocalizedString("stats_armor");
        mStatLst[(int)Enum_CharInfoTabOneStats.IGNOREARMOR].CombineName = GUILocalizationRepo.GetLocalizedString("stats_ignorearmor");
        mStatLst[(int)Enum_CharInfoTabOneStats.BLOCK].CombineName = GUILocalizationRepo.GetLocalizedString("stats_block");
        mStatLst[(int)Enum_CharInfoTabOneStats.BLOCKVALUE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_blockvalue");
        mStatLst[(int)Enum_CharInfoTabOneStats.ACCURACY].CombineName = GUILocalizationRepo.GetLocalizedString("stats_accuracy");
        mStatLst[(int)Enum_CharInfoTabOneStats.EVASION].CombineName = GUILocalizationRepo.GetLocalizedString("stats_evasion");
        mStatLst[(int)Enum_CharInfoTabOneStats.CRITICAL].CombineName = GUILocalizationRepo.GetLocalizedString("stats_critical");
        mStatLst[(int)Enum_CharInfoTabOneStats.COCRITICAL].CombineName = GUILocalizationRepo.GetLocalizedString("stats_cocritical");
        mStatLst[(int)Enum_CharInfoTabOneStats.CRITICALDAMAGE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_criticaldamage");

        mStatLst[(int)Enum_CharInfoTabOneStats.EXPBONUS].ValPostfix = "%";
        mStatLst[(int)Enum_CharInfoTabOneStats.IGNOREARMOR].ValPostfix = "%";
        mStatLst[(int)Enum_CharInfoTabOneStats.BLOCK].ValPostfix = "%";
        mStatLst[(int)Enum_CharInfoTabOneStats.CRITICALDAMAGE].ValPostfix = "%";
    }

    public void OnEnable()
    {
        //Update stats according to localcombatstats
        SetStatsField();
    }
    public void OnDisable()
    {
        
    }

    /// <summary>
    /// Call this when character info window has a popup and now return back to focus
    /// </summary>
    public void OnRegainWindowContext()
    {
        SetStatsField();
    }

    public UI_CharacterInfo_BasicStats this[Enum_CharInfoTabOneStats e]
    {
        get { return mStatLst[(int)e]; }
    }
    public UI_CharacterInfo_BasicStats this[int i]
    {
        get { return mStatLst[i]; }
    }

    private void SetStatsField()
    {
        if (GameInfo.gLocalPlayer == null)
            return;

        var ps = GameInfo.gLocalPlayer.PlayerStats;
        var lcs = GameInfo.gLocalPlayer.LocalCombatStats;

        mStatLst[(int)Enum_CharInfoTabOneStats.MAXHP].CombineVal = lcs.HealthMax;
        mStatLst[(int)Enum_CharInfoTabOneStats.HPREGEN].CombineVal = lcs.HealthRegen;
        mStatLst[(int)Enum_CharInfoTabOneStats.MAXMANA].CombineVal = lcs.ManaMax;
        mStatLst[(int)Enum_CharInfoTabOneStats.MANAREGEN].CombineVal = lcs.ManaRegen;
        mStatLst[(int)Enum_CharInfoTabOneStats.MOVESPD].CombineVal = (int)ps.MoveSpeed;
        mStatLst[(int)Enum_CharInfoTabOneStats.EXPBONUS].CombineValPercent = lcs.ExpBonus;
        mStatLst[(int)Enum_CharInfoTabOneStats.WEAPONATK].CombineVal = lcs.WeaponAttack;
        mStatLst[(int)Enum_CharInfoTabOneStats.ATKPOWER].CombineVal = lcs.AttackPower;
        mStatLst[(int)Enum_CharInfoTabOneStats.ARMOR].CombineVal = lcs.Armor;
        mStatLst[(int)Enum_CharInfoTabOneStats.IGNOREARMOR].CombineValPercent = lcs.IgnoreArmor;
        mStatLst[(int)Enum_CharInfoTabOneStats.BLOCK].CombineValPercent = lcs.Block;
        mStatLst[(int)Enum_CharInfoTabOneStats.BLOCKVALUE].CombineVal = lcs.BlockValue;
        mStatLst[(int)Enum_CharInfoTabOneStats.ACCURACY].CombineVal = lcs.Accuracy;
        mStatLst[(int)Enum_CharInfoTabOneStats.EVASION].CombineVal = lcs.Evasion;
        mStatLst[(int)Enum_CharInfoTabOneStats.CRITICAL].CombineVal = lcs.Critical;
        mStatLst[(int)Enum_CharInfoTabOneStats.COCRITICAL].CombineVal = lcs.CoCritical;
        mStatLst[(int)Enum_CharInfoTabOneStats.CRITICALDAMAGE].CombineValPercent = lcs.CriticalDamage;
    }
}
