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
    [SerializeField] Button levelUpBtn;

    public enum HeroStats
    {
        NAME,
        LVL,
        DESC,
        ATK,
        ACC,
        CRI,
        ATK_STYLE,
        ELE,
        STR,
        AGI,
        DEX,
        CON,
        INT,
        CRI_DMG,
        IGN_ARMOR,
        TOTAL_NUM
    }

    private Hero hero;
    private List<Hero_StatsData> statsList = new List<Hero_StatsData>();

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
            statsList[(int)HeroStats.NAME].Init(GetLocalizedStatsName(HeroStats.NAME), hero.HeroJson.localizedname);
            statsList[(int)HeroStats.LVL].Init(GetLocalizedStatsName(HeroStats.LVL), hero.Level);
            statsList[(int)HeroStats.DESC].Init(hero.HeroJson.description);
            statsList[(int)HeroStats.ATK].Init(GetLocalizedStatsName(HeroStats.ATK), hero.SynCombatStats.Attack);
            statsList[(int)HeroStats.ACC].Init(GetLocalizedStatsName(HeroStats.ACC), hero.SynCombatStats.Accuracy);
            statsList[(int)HeroStats.CRI].Init(GetLocalizedStatsName(HeroStats.CRI), hero.SynCombatStats.Critical);
            statsList[(int)HeroStats.ATK_STYLE].Init(GetLocalizedStatsName(HeroStats.ATK_STYLE), ClientUtils.GetLocalizedAttackStyle(hero.HeroJson.attackstyle));
            statsList[(int)HeroStats.ELE].Init(GetLocalizedStatsName(HeroStats.ELE), ClientUtils.GetLocalizedElement(hero.HeroJson.element));
            statsList[(int)HeroStats.STR].Init(GetLocalizedStatsName(HeroStats.STR), data.strength);
            statsList[(int)HeroStats.AGI].Init(GetLocalizedStatsName(HeroStats.AGI), data.agility);
            statsList[(int)HeroStats.DEX].Init(GetLocalizedStatsName(HeroStats.DEX), data.dexterity);
            statsList[(int)HeroStats.CON].Init(GetLocalizedStatsName(HeroStats.CON), data.constitution);
            statsList[(int)HeroStats.INT].Init(GetLocalizedStatsName(HeroStats.INT), data.intelligence);
            statsList[(int)HeroStats.CRI_DMG].Init(GetLocalizedStatsName(HeroStats.CRI_DMG), hero.SynCombatStats.CriticalDamage);
            statsList[(int)HeroStats.IGN_ARMOR].Init(GetLocalizedStatsName(HeroStats.IGN_ARMOR), hero.SynCombatStats.IgnoreArmor);

            moneyAmtText.text = data.levelupmoney.ToString("N0");
            levelUpBtn.interactable = hero.SlotIdx != -1;
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
            case HeroStats.NAME: name = "com_name"; break;
            case HeroStats.LVL: name = "com_level"; break;
            case HeroStats.ATK: name = "hro_attackpower"; break;
            case HeroStats.ACC: name = "stats_accuracy"; break;
            case HeroStats.CRI: name = "stats_critical"; break;
            case HeroStats.ATK_STYLE: name = "com_attackstyle"; break;
            case HeroStats.ELE: name = "com_element"; break;
            case HeroStats.STR: name = "ci_strength"; break;
            case HeroStats.AGI: name = "ci_agility"; break;
            case HeroStats.DEX: name = "ci_dexterity"; break;
            case HeroStats.CON: name = "ci_constitution"; break;
            case HeroStats.INT: name = "ci_inteligence"; break;
            case HeroStats.CRI_DMG: name = "stats_criticaldamage"; break;
            case HeroStats.IGN_ARMOR: name = "stats_ignorearmor"; break;
        }
        return GUILocalizationRepo.GetLocalizedString(name);
    }
}
