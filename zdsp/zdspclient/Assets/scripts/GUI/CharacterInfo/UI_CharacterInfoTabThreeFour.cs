using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using Zealot.Repository;

public class UI_CharacterInfoTabThreeFour : MonoBehaviour
{
    public enum Enum_CharInfoTabThreeStats
    {
        DMG_SMASH = 0,
        DMG_SLICE,
        DMG_PIERCE,
        DMG_NONELEM,
        DMG_METAL,
        DMG_WOOD,
        DMG_EARTH,
        DMG_WATER,
        DMG_FIRE,
        DMG_VSHUMAN,
        DMG_VSZOMBIE,
        DMG_VSVAMPIRE,
        DMG_VSANIMAL,
        DMG_VSPLANT,
        DMG_VSNONELEM,
        DMG_VSMETAL,
        DMG_VSWOOD,
        DMG_VSEARTH,
        DMG_VSWATER,
        DMG_VSFIRE,
        DMG_VSBOSS,
        DMG_FINALINCREASE,

        NUM_STATS
    };

    public enum Enum_CharInfoTabThreeHeader
    {
        DMG_ATKTYPE = 0,
        DMG_ELEMTYPE,
        DMG_VSRACE,
        DMG_VSELEM,
        DMG_SPECIAL,

        NUM_HEADER
    };

    public enum Enum_CharInfoTabFourStats
    {
        DEF_SMASH,
        DEF_SLICE,
        DEF_PIERCE,
        DEF_NONELEM,
        DEF_METAL,
        DEF_WOOD,
        DEF_EARTH,
        DEF_WATER,
        DEF_FIRE,
        DEF_VSHUMAN,
        DEF_VSZOMBIE,
        DEF_VSVAMPIRE,
        DEF_VSANIMAL,
        DEF_VSPLANT,
        DEF_FINALDECREASE,

        NUM_STATS
    };

    public enum Enum_CharInfoTabFourHeader
    {
        DEF_ATKTYPE,
        DEF_ELEMTYPE,
        DEF_VSRACE,
        DEF_SPECIAL,

        NUM_HEADER
    };

    [Header("Button Toggle")]
    [SerializeField]
    Toggle mAttackButton;
    [SerializeField]
    Toggle mDefenceButton;

    [Header("Prefabs")]
    [SerializeField]
    GameObject mTab34Prefab;
    [SerializeField]
    GameObject mTab34HeaderPrefab;
    List<GameObject> mTab3ObjLst = new List<GameObject>();
    List<GameObject> mTab4ObjLst = new List<GameObject>();
    List<UI_CharacterInfo_BasicStats> mStat3Lst = new List<UI_CharacterInfo_BasicStats>();
    List<UI_CharacterInfo_BasicStats> mStat4Lst = new List<UI_CharacterInfo_BasicStats>();

    [Header("Content Header")]
    [SerializeField]
    GameObject mAtkDefStatsHeader;

    public void Awake()
    {
        List<Text> header3Lst = new List<Text>();
        List<Text> header4Lst = new List<Text>();
        List<GameObject> header3ObjLst = new List<GameObject>();
        List<GameObject> header4ObjLst = new List<GameObject>();

        //Tab03 - Create headers
        int max = (int)Enum_CharInfoTabThreeHeader.NUM_HEADER;
        GameObject obj = null;
        Text header = null;
        for (int i = 0; i < max; ++i)
        {
            obj = Instantiate(mTab34HeaderPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            header3ObjLst.Add(obj);

            header = obj.GetComponentInChildren<Text>();
            header3Lst.Add(header);
            mTab3ObjLst.Add(obj);
        }

        //Tab03 - Create fields
        max = (int)Enum_CharInfoTabThreeStats.NUM_STATS;
        UI_CharacterInfo_BasicStats stats = null;
        for (int i = 0; i < max; ++i)
        {
            obj = Instantiate(mTab34Prefab, Vector3.zero, Quaternion.identity) as GameObject;
            stats = obj.GetComponent<UI_CharacterInfo_BasicStats>();

            mStat3Lst.Add(stats);
            mTab3ObjLst.Add(obj);
        }

        //Tab03 - Setup hiearchy
        int a = (int)Enum_CharInfoTabThreeStats.DMG_SMASH;
        int b = (int)Enum_CharInfoTabThreeStats.DMG_NONELEM;

        header3ObjLst[(int)Enum_CharInfoTabThreeHeader.DMG_ATKTYPE].transform.SetParent(mAtkDefStatsHeader.transform, false);
        for (; a < b; ++a)
        {
            mStat3Lst[a].transform.SetParent(mAtkDefStatsHeader.transform, false);
        }

        a = (int)Enum_CharInfoTabThreeStats.DMG_NONELEM;
        b = (int)Enum_CharInfoTabThreeStats.DMG_VSHUMAN;
        header3ObjLst[(int)Enum_CharInfoTabThreeHeader.DMG_ELEMTYPE].transform.SetParent(mAtkDefStatsHeader.transform, false);
        for (; a < b; ++a)
        {
            mStat3Lst[a].transform.SetParent(mAtkDefStatsHeader.transform, false);
        }

        a = (int)Enum_CharInfoTabThreeStats.DMG_VSHUMAN;
        b = (int)Enum_CharInfoTabThreeStats.DMG_VSNONELEM;
        header3ObjLst[(int)Enum_CharInfoTabThreeHeader.DMG_VSRACE].transform.SetParent(mAtkDefStatsHeader.transform, false);
        for (; a < b; ++a)
        {
            mStat3Lst[a].transform.SetParent(mAtkDefStatsHeader.transform, false);
        }

        a = (int)Enum_CharInfoTabThreeStats.DMG_VSNONELEM;
        b = (int)Enum_CharInfoTabThreeStats.DMG_VSBOSS;
        header3ObjLst[(int)Enum_CharInfoTabThreeHeader.DMG_VSELEM].transform.SetParent(mAtkDefStatsHeader.transform, false);
        for (; a < b; ++a)
        {
            mStat3Lst[a].transform.SetParent(mAtkDefStatsHeader.transform, false);
        }

        a = (int)Enum_CharInfoTabThreeStats.DMG_VSBOSS;
        b = (int)Enum_CharInfoTabThreeStats.NUM_STATS;
        header3ObjLst[(int)Enum_CharInfoTabThreeHeader.DMG_SPECIAL].transform.SetParent(mAtkDefStatsHeader.transform, false);
        for (; a < b; ++a)
        {
            mStat3Lst[a].transform.SetParent(mAtkDefStatsHeader.transform, false);
        }

        //Tab03 - Set fields, localization
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_SMASH].CombineName = "Slash Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_SLICE].CombineName = "Slice Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_PIERCE].CombineName = "Pierce Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_NONELEM].CombineName = "Non-elemental Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_METAL].CombineName = "Metal Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_WOOD].CombineName = "Wood Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_EARTH].CombineName = "Earth Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_WATER].CombineName = "Water Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_FIRE].CombineName = "Fire Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSHUMAN].CombineName = "VSHuman Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSZOMBIE].CombineName = "VSZombie Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSVAMPIRE].CombineName = "VSVampire Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSANIMAL].CombineName = "VSAnimal Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSPLANT].CombineName = "VSPlant Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSNONELEM].CombineName = "VSNon-elemental Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSMETAL].CombineName = "VSMetal Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSWOOD].CombineName = "VSWood Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSEARTH].CombineName = "VSEarth Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSWATER].CombineName = "VSWater Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSFIRE].CombineName = "VSFire Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSBOSS].CombineName = "VSBoss Damage";
        //mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_FINALINCREASE].CombineName = "Final Increase Damage";

        //header3Lst[(int)Enum_CharInfoTabThreeHeader.DMG_ATKTYPE].text = "AttackType Damage";
        //header3Lst[(int)Enum_CharInfoTabThreeHeader.DMG_ELEMTYPE].text = "ElementType Damage";
        //header3Lst[(int)Enum_CharInfoTabThreeHeader.DMG_VSRACE].text = "VersusRace Damage";
        //header3Lst[(int)Enum_CharInfoTabThreeHeader.DMG_VSELEM].text = "VersusElement Damage";
        //header3Lst[(int)Enum_CharInfoTabThreeHeader.DMG_SPECIAL].text = "SpecialMod Damage";

        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_SMASH].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incsmashdmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_SLICE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incslicedmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_PIERCE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incpiercedmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_NONELEM].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incelenonedmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_METAL].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incelemateldmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_WOOD].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incelewooddmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_EARTH].CombineName = GUILocalizationRepo.GetLocalizedString("stats_inceleearthdmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_WATER].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incelewaterdmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_FIRE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incelefiredmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSHUMAN].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vshumandmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSZOMBIE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vszombiedmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSVAMPIRE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vsvampiredmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSANIMAL].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vsanimaldmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSPLANT].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vsplantdmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSNONELEM].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vselenonedmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSMETAL].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vselemateldmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSWOOD].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vselewooddmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSEARTH].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vseleearthdmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSWATER].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vselewaterdmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSFIRE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vselefiredmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSBOSS].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vsbossdmg");
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_FINALINCREASE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incfinaldmg");

        header3Lst[(int)Enum_CharInfoTabThreeHeader.DMG_ATKTYPE].text = GUILocalizationRepo.GetLocalizedString("ci_incatktypedmg");
        header3Lst[(int)Enum_CharInfoTabThreeHeader.DMG_ELEMTYPE].text = GUILocalizationRepo.GetLocalizedString("ci_inceletypedmg");
        header3Lst[(int)Enum_CharInfoTabThreeHeader.DMG_VSRACE].text = GUILocalizationRepo.GetLocalizedString("ci_vsracedmg");
        header3Lst[(int)Enum_CharInfoTabThreeHeader.DMG_VSELEM].text = GUILocalizationRepo.GetLocalizedString("ci_vseledmg");
        header3Lst[(int)Enum_CharInfoTabThreeHeader.DMG_SPECIAL].text = GUILocalizationRepo.GetLocalizedString("ci_specialmoddmg");



        //******************** Tab End *******************

        //Tab04 - Create headers
        max = (int)Enum_CharInfoTabFourHeader.NUM_HEADER;
        for (int i = 0; i < max; ++i)
        {
            obj = Instantiate(mTab34HeaderPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            header4ObjLst.Add(obj);

            header = obj.GetComponentInChildren<Text>();
            header4Lst.Add(header);
            mTab4ObjLst.Add(obj);
        }

        //Tab04 - Create fields
        max = (int)Enum_CharInfoTabFourStats.NUM_STATS;
        for (int i = 0; i < max; ++i)
        {
            obj = Instantiate(mTab34Prefab, Vector3.zero, Quaternion.identity) as GameObject;
            stats = obj.GetComponent<UI_CharacterInfo_BasicStats>();

            mStat4Lst.Add(stats);
            mTab4ObjLst.Add(obj);
        }

        //Tab04 - setup hiearchy
        a = (int)Enum_CharInfoTabFourStats.DEF_SMASH;
        b = (int)Enum_CharInfoTabFourStats.DEF_NONELEM;

        header4ObjLst[(int)Enum_CharInfoTabFourHeader.DEF_ATKTYPE].transform.SetParent(mAtkDefStatsHeader.transform, false);
        for (; a < b; ++a)
        {
            mStat4Lst[a].transform.SetParent(mAtkDefStatsHeader.transform, false);
        }

        a = (int)Enum_CharInfoTabFourStats.DEF_NONELEM;
        b = (int)Enum_CharInfoTabFourStats.DEF_VSHUMAN;
        header4ObjLst[(int)Enum_CharInfoTabFourHeader.DEF_ELEMTYPE].transform.SetParent(mAtkDefStatsHeader.transform, false);
        for (; a < b; ++a)
        {
            mStat4Lst[a].transform.SetParent(mAtkDefStatsHeader.transform, false);
        }

        a = (int)Enum_CharInfoTabFourStats.DEF_VSHUMAN;
        b = (int)Enum_CharInfoTabFourStats.DEF_FINALDECREASE;
        header4ObjLst[(int)Enum_CharInfoTabFourHeader.DEF_VSRACE].transform.SetParent(mAtkDefStatsHeader.transform, false);
        for (; a < b; ++a)
        {
            mStat4Lst[a].transform.SetParent(mAtkDefStatsHeader.transform, false);
        }

        a = (int)Enum_CharInfoTabFourStats.DEF_FINALDECREASE;
        b = (int)Enum_CharInfoTabFourStats.NUM_STATS;
        header4ObjLst[(int)Enum_CharInfoTabFourHeader.DEF_SPECIAL].transform.SetParent(mAtkDefStatsHeader.transform, false);
        for (; a < b; ++a)
        {
            mStat4Lst[a].transform.SetParent(mAtkDefStatsHeader.transform, false);
        }

        //Tab04 - Set fields, localization
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_SMASH].CombineName = "Smash Defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_SLICE].CombineName = "Slice Defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_PIERCE].CombineName = "Pierce Defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_NONELEM].CombineName = "Non elemental defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_METAL].CombineName = "Metal defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_WOOD].CombineName = "Wood defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_EARTH].CombineName = "Earth defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_WATER].CombineName = "Water defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_FIRE].CombineName = "Fire defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSHUMAN].CombineName = "VSHuman defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSZOMBIE].CombineName = "VSZombie defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSVAMPIRE].CombineName = "VSVampire defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSANIMAL].CombineName = "VSAnimal defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSPLANT].CombineName = "VSPlant defence";
        //mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_FINALDECREASE].CombineName = "Final damage decrease";

        //header4Lst[(int)Enum_CharInfoTabFourHeader.DEF_ATKTYPE].text = "AttackType Defence";
        //header4Lst[(int)Enum_CharInfoTabFourHeader.DEF_ELEMTYPE].text = "ElementType Defence";
        //header4Lst[(int)Enum_CharInfoTabFourHeader.DEF_VSRACE].text = "VersusRace Defence";
        //header4Lst[(int)Enum_CharInfoTabFourHeader.DEF_SPECIAL].text = "SpecialMod Defence";

        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_SMASH].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incsmashdef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_SLICE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incslicedef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_PIERCE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incpiercedef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_NONELEM].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incelenonedef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_METAL].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incelemateldef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_WOOD].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incelewooddef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_EARTH].CombineName = GUILocalizationRepo.GetLocalizedString("stats_inceleearthdef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_WATER].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incelewaterdef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_FIRE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_incelefiredef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSHUMAN].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vshumandef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSZOMBIE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vszombiedef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSVAMPIRE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vsvampiredef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSANIMAL].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vsanimaldef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSPLANT].CombineName = GUILocalizationRepo.GetLocalizedString("stats_vsplantdef");
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_FINALDECREASE].CombineName = GUILocalizationRepo.GetLocalizedString("stats_decfinaldmg");

        header4Lst[(int)Enum_CharInfoTabFourHeader.DEF_ATKTYPE].text = GUILocalizationRepo.GetLocalizedString("ci_incatktypedef");
        header4Lst[(int)Enum_CharInfoTabFourHeader.DEF_ELEMTYPE].text = GUILocalizationRepo.GetLocalizedString("ci_inceletypedef");
        header4Lst[(int)Enum_CharInfoTabFourHeader.DEF_VSRACE].text = GUILocalizationRepo.GetLocalizedString("ci_vsracedef");
        header4Lst[(int)Enum_CharInfoTabFourHeader.DEF_SPECIAL].text = GUILocalizationRepo.GetLocalizedString("ci_specialmoddef");

        if (mAttackButton.isOn)
            TurnOnAttackStats();
        else if (mDefenceButton.isOn)
            TurnOnDefenceStats();
    }

    public void OnEnable()
    {
        //Update stats according to localcombatstats
        if (mAttackButton.isOn)
            TurnOnAttackStats();
        else if (mDefenceButton.isOn)
            TurnOnDefenceStats();
    }
    public void OnDisable()
    {
        
    }

    public void TurnOnAttackStats()
    {
        if (mTab4ObjLst.Count == 0 || mTab3ObjLst.Count == 0)
            return;

        //Update stats according to localcombatstats
        SetStatsFieldTab03();

        for (int i = 0; i < mTab4ObjLst.Count; ++i)
            mTab4ObjLst[i].SetActive(false);
        for (int i = 0; i < mTab3ObjLst.Count; ++i)
            mTab3ObjLst[i].SetActive(true);
    }
    public void TurnOnDefenceStats()
    {
        if (mTab4ObjLst.Count == 0 || mTab3ObjLst.Count == 0)
            return;

        //Update stats according to localcombatstats
        SetStatsFieldTab04();

        for (int i = 0; i < mTab4ObjLst.Count; ++i)
            mTab4ObjLst[i].SetActive(true);
        for (int i = 0; i < mTab3ObjLst.Count; ++i)
            mTab3ObjLst[i].SetActive(false);
    }

    /// <summary>
    /// Call this when character info window has a popup and now return back to focus
    /// </summary>
    public void OnRegainWindowContext()
    {
        if (mAttackButton.isOn)
            TurnOnAttackStats();
        else if (mDefenceButton.isOn)
            TurnOnDefenceStats();
    }

    public UI_CharacterInfo_BasicStats GetTab3Stats(int i)
    {
        return mStat3Lst[i];
    }
    public UI_CharacterInfo_BasicStats GetTab3Stats(Enum_CharInfoTabThreeStats e)
    {
        return mStat3Lst[(int)e];
    }
    public UI_CharacterInfo_BasicStats GetTab4Stats(int i)
    {
        return mStat4Lst[i];
    }
    public UI_CharacterInfo_BasicStats GetTab4Stats(Enum_CharInfoTabFourStats e)
    {
        return mStat4Lst[(int)e];
    }

    private void SetStatsFieldTab03()
    {
        if (GameInfo.gLocalPlayer == null)
            return;

        var lcs = GameInfo.gLocalPlayer.LocalCombatStats;

        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_SMASH].CombineVal = lcs.SmashDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_SLICE].CombineVal = lcs.SliceDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_PIERCE].CombineVal = lcs.PierceDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_NONELEM].CombineVal = lcs.IncElemNoneDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_METAL].CombineVal = lcs.IncElemMetalDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_WOOD].CombineVal = lcs.IncElemWoodDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_EARTH].CombineVal = lcs.IncElemEarthDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_WATER].CombineVal = lcs.IncElemWaterDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_FIRE].CombineVal = lcs.IncElemFireDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSHUMAN].CombineVal = lcs.VSHumanDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSZOMBIE].CombineVal = lcs.VSZombieDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSVAMPIRE].CombineVal = lcs.VSVampireDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSANIMAL].CombineVal = lcs.VSAnimalDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSPLANT].CombineVal = lcs.VSPlantDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSNONELEM].CombineVal = lcs.VSElemNoneDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSMETAL].CombineVal = lcs.VSElemMetalDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSWOOD].CombineVal = lcs.VSElemWoodDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSEARTH].CombineVal = lcs.VSElemEarthDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSWATER].CombineVal = lcs.VSElemWaterDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSFIRE].CombineVal = lcs.VSElemFireDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_VSBOSS].CombineVal = lcs.VSBossDamage;
        mStat3Lst[(int)Enum_CharInfoTabThreeStats.DMG_FINALINCREASE].CombineVal = lcs.IncFinalDamage;
    }

    private void SetStatsFieldTab04()
    {
        if (GameInfo.gLocalPlayer == null)
            return;

        var lcs = GameInfo.gLocalPlayer.LocalCombatStats;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_SMASH].CombineVal = lcs.SmashDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_SLICE].CombineVal = lcs.SliceDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_PIERCE].CombineVal = lcs.PierceDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_NONELEM].CombineVal = lcs.IncElemNoneDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_METAL].CombineVal = lcs.IncElemMetalDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_WOOD].CombineVal = lcs.IncElemWoodDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_EARTH].CombineVal = lcs.IncElemEarthDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_WATER].CombineVal = lcs.IncElemWaterDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_FIRE].CombineVal = lcs.IncElemFireDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSHUMAN].CombineVal = lcs.VSHumanDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSZOMBIE].CombineVal = lcs.VSZombieDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSVAMPIRE].CombineVal = lcs.VSVampireDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSANIMAL].CombineVal = lcs.VSAnimalDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_VSPLANT].CombineVal = lcs.VSPlantDefence;
        mStat4Lst[(int)Enum_CharInfoTabFourStats.DEF_FINALDECREASE].CombineVal = lcs.DncFinalDamage;
    }
}
