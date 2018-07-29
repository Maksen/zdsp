using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Hero_StatsDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataTransform;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Text moneyAmtText;

    public enum HeroStats
    {
        LVL,
        ATK,
        ACC,
        CRI,
        ATK_STYLE,
        WPN_ATTR,
        ELE,
        STR,
        AGI,
        DEX,
        INT,
        TOTAL_NUM
    }

    private Hero hero;
    private List<Hero_StatsData> statsList = new List<Hero_StatsData>();

    private void Start()
    {
        ClientUtils.DestroyChildren(dataTransform);
        OnRegister();
        Hero hero1 = new Hero(1, HeroInterestType.MartialArts, 0, HeroRepo.GetHeroById(1));
        hero1.Level = 2;
        Init(hero1);
    }

    public override void OnRegister()
    {
        base.OnRegister();

        int length = (int)HeroStats.TOTAL_NUM;
        for (int i = 0; i < length; i++)
        {
            GameObject obj = ClientUtils.CreateChild(dataTransform, dataPrefab);
            Hero_StatsData stats = obj.GetComponent<Hero_StatsData>();
            statsList.Add(stats);
        }
    }

    public void Init(Hero hero)
    {
        this.hero = hero;
        HeroGrowthJson data = HeroRepo.GetHeroGrowthData(hero.HeroJson.growthgroup, hero.Level);
        if (data != null)
        {
            statsList[(int)HeroStats.LVL].Init(GetLocalizedStatsName(HeroStats.LVL), hero.Level);
            statsList[(int)HeroStats.ATK].Init(GetLocalizedStatsName(HeroStats.ATK), data.attackpower);
            statsList[(int)HeroStats.ACC].Init(GetLocalizedStatsName(HeroStats.ACC), data.accuracy);
            statsList[(int)HeroStats.CRI].Init(GetLocalizedStatsName(HeroStats.CRI), data.critical);
            statsList[(int)HeroStats.ATK_STYLE].Init(GetLocalizedStatsName(HeroStats.ATK_STYLE), GetLocalizedAttackStyle(hero.HeroJson.attackstyle));
            statsList[(int)HeroStats.WPN_ATTR].Init(GetLocalizedStatsName(HeroStats.WPN_ATTR), GetLocalizedWeaponAttribute(hero.HeroJson.weaponattrib));
            statsList[(int)HeroStats.ELE].Init(GetLocalizedStatsName(HeroStats.ELE), GetLocalizedElement(hero.HeroJson.element));
            statsList[(int)HeroStats.STR].Init(GetLocalizedStatsName(HeroStats.STR), data.strength);
            statsList[(int)HeroStats.AGI].Init(GetLocalizedStatsName(HeroStats.AGI), data.agility);
            statsList[(int)HeroStats.DEX].Init(GetLocalizedStatsName(HeroStats.DEX), data.dexterity);
            statsList[(int)HeroStats.INT].Init(GetLocalizedStatsName(HeroStats.INT), data.intelligence);

            moneyAmtText.text = data.levelupmoney.ToString("N0");
        }
    }

    public void OnClickLevelUp()
    {
        RPCFactory.CombatRPC.LevelUpHero(hero.HeroId);
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private string GetLocalizedStatsName(HeroStats stats)
    {
        string name = "";
        switch (stats)
        {
            case HeroStats.LVL: name = "com_level"; break;
            case HeroStats.ATK: name = "com_attack"; break;
            case HeroStats.ACC: name = "stats_accuracy"; break;
            case HeroStats.CRI: name = "stats_critical"; break;
            case HeroStats.ATK_STYLE: name = "com_attackstyle"; break;
            case HeroStats.WPN_ATTR: name = "id_wpnmainattr"; break;
            case HeroStats.ELE: name = "com_element"; break;
            case HeroStats.STR: name = "ci_strength"; break;
            case HeroStats.AGI: name = "ci_agility"; break;
            case HeroStats.DEX: name = "ci_dexterity"; break;
            case HeroStats.INT: name = "ci_inteligence"; break;
        }
        return GUILocalizationRepo.GetLocalizedString(name);
    }

    private string GetLocalizedAttackStyle(AttackStyle style)
    {
        string name = "";
        switch (style)
        {
            case AttackStyle.Slice: name = "com_styleslice"; break;
            case AttackStyle.Pierce: name = "com_stylepierce"; break;
            case AttackStyle.Smash: name = "com_stylesmash"; break;
            case AttackStyle.God: name = "com_stylegod"; break;
            case AttackStyle.Normal: name = "com_stylenormal"; break;
        }
        return GUILocalizationRepo.GetLocalizedString(name);
    }

    private string GetLocalizedWeaponAttribute(MainWeaponAttribute attribute)
    {
        string name = "";
        switch (attribute)
        {
            case MainWeaponAttribute.Str: name = "ci_strength"; break;
            case MainWeaponAttribute.Dex: name = "ci_dexterity"; break;
            case MainWeaponAttribute.Int: name = "ci_inteligence"; break;
        }
        return GUILocalizationRepo.GetLocalizedString(name);
    }

    private string GetLocalizedElement(Element element)
    {
        string name = "";
        switch (element)
        {
            case Element.None:
                name = "com_elemnone";
                break;
            case Element.Metal:
                name = "com_elemfire";
                break;
            case Element.Wood:
                name = "com_elemwood";
                break;
            case Element.Earth:
                name = "com_elemearth";
                break;
            case Element.Water:
                name = "com_elemwater";
                break;
            case Element.Fire:
                name = "com_elemfire";
                break;
            default:
                break;
        }
        return GUILocalizationRepo.GetLocalizedString(name);
    }
}
