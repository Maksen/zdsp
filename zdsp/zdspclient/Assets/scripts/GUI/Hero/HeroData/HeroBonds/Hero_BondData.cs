using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Common.Entities;

public class Hero_BondData : MonoBehaviour
{
    [SerializeField] Text bondNameText;
    [SerializeField] Text bondLevelText;
    [SerializeField] Text heroCountText;
    [SerializeField] Transform heroDataParent;
    [SerializeField] GameObject heroDataPrefab;
    [SerializeField] Transform bondLevelDataParent;
    [SerializeField] GameObject bondLevelDataPrefab;
    [SerializeField] UI_SnapToScrollviewChild scrollviewSnap;

    private HeroStatsClient heroStats;
    private List<Hero_BondHeroData> heroList = new List<Hero_BondHeroData>();
    private List<Hero_BondLevelData> bondList = new List<Hero_BondLevelData>();
    private HeroBond heroBond;
    private HeroBondJson nextLevelBond;
    private int currentLevel;
    private UI_Hero_BondsDialog parent;
    private int selectedHeroId = -1;
    private int childIndex;

    public void Init(HeroBond bond, UI_Hero_BondsDialog myParent)
    {
        heroStats = GameInfo.gLocalPlayer.HeroStats;
        heroBond = bond;
        parent = myParent;

        bondNameText.text = bond.heroBondGroupJson.localizedname;
        DetermineFulfilledBondLevel();
        InitHeroList();
        InitBondLevelList();
    }

    private void DetermineFulfilledBondLevel()
    {
        HeroBondJson currentLevelBond = heroBond.GetHighestFulfilledLevel(heroStats);
        if (currentLevelBond != null)
        {
            currentLevel = currentLevelBond.bondlevel;
            childIndex = heroBond.heroBondJsonList.IndexOf(currentLevelBond);
            nextLevelBond = childIndex + 1 < heroBond.heroBondJsonList.Count ? heroBond.heroBondJsonList[childIndex + 1] : null;
        }
        else
        {
            currentLevel = 0;
            childIndex = 0;
            nextLevelBond = heroBond.heroBondJsonList.Count > 0 ? heroBond.heroBondJsonList[0] : null;
        }
        bondLevelText.text = nextLevelBond != null ? nextLevelBond.bondlevel.ToString() : currentLevel.ToString();
    }

    private void InitHeroList()
    {
        ToggleGroup toggleGroup = heroDataParent.GetComponent<ToggleGroup>();
        int totalCount = heroBond.heroIds.Count;
        int fulfulledHeroCount = 0;
        for (int i = 0; i < totalCount; i++)
        {
            GameObject obj = ClientUtils.CreateChild(heroDataParent, heroDataPrefab);
            Hero_BondHeroData heroData = obj.GetComponent<Hero_BondHeroData>();
            heroData.Init(heroBond.heroIds[i], toggleGroup, OnHeroSelected);
            bool fulfilled = heroStats.HasHeroFulfilledBond(nextLevelBond, heroBond.heroIds[i]);
            heroData.SetFulfilled(fulfilled);
            heroList.Add(heroData);
            if (fulfilled)
                fulfulledHeroCount++;
        }
        heroCountText.text = fulfulledHeroCount + "/" + totalCount;
    }

    private void InitBondLevelList()
    {
        int totalCount = heroBond.heroBondJsonList.Count;
        for (int i = 0; i < totalCount; i++)
        {
            var bond = heroBond.heroBondJsonList[i];
            GameObject obj = ClientUtils.CreateChild(bondLevelDataParent, bondLevelDataPrefab);
            Hero_BondLevelData data = obj.GetComponent<Hero_BondLevelData>();
            data.Init(bond, bond.bondlevel <= currentLevel);
            bondList.Add(data);
        }
        scrollviewSnap.SnapToChild(childIndex);
    }

    public void Refresh()
    {
        DetermineFulfilledBondLevel();

        int totalCount = heroBond.heroBondJsonList.Count;
        for (int i = 0; i < totalCount; i++)
            bondList[i].SetUnlocked(heroBond.heroBondJsonList[i].bondlevel <= currentLevel);

        int fulfulledHeroCount = 0;
        totalCount = heroBond.heroIds.Count;
        for (int i = 0; i < totalCount; i++)
        {
            bool fulfilled = heroStats.HasHeroFulfilledBond(nextLevelBond, heroBond.heroIds[i]);
            heroList[i].SetFulfilled(fulfilled);
            if (fulfilled)
                fulfulledHeroCount++;
        }
        heroCountText.text = fulfulledHeroCount + "/" + totalCount;
    }

    private void OnHeroSelected(int heroId, bool toggledOn)
    {
        if (heroId == -1)
            return;

        //print("selected heroId: " + heroId + "/" + toggledOn);
        if (toggledOn)
        {
            selectedHeroId = heroId;
            if (nextLevelBond == null)
                parent.HideSubPanels();
            else
            {
                Hero hero = heroStats.GetHero(heroId);
                if (hero == null)
                    parent.ShowLockedPanel(heroId);
                else
                {
                    bool type1Passed = hero.HasFulfilledBondType(nextLevelBond.bondtype1, nextLevelBond.bondvalue1);
                    bool type2Passed = hero.HasFulfilledBondType(nextLevelBond.bondtype2, nextLevelBond.bondvalue2);
                    if (type1Passed && type2Passed)
                    {
                        parent.HideSubPanels();
                    }
                    else if (!type1Passed && type2Passed)
                    {
                        if (nextLevelBond.bondtype1 == HeroBondType.HeroLevel)
                            parent.ShowLevelUpPanel(hero, nextLevelBond.bondvalue1);
                        else if (nextLevelBond.bondtype1 == HeroBondType.HeroSkill)
                            parent.ShowSkillPointsPanel(hero, nextLevelBond.bondvalue1);
                    }
                    else if (type1Passed && !type2Passed)
                    {
                        if (nextLevelBond.bondtype2 == HeroBondType.HeroLevel)
                            parent.ShowLevelUpPanel(hero, nextLevelBond.bondvalue2);
                        else if (nextLevelBond.bondtype2 == HeroBondType.HeroSkill)
                            parent.ShowSkillPointsPanel(hero, nextLevelBond.bondvalue2);
                    }
                    else  // both req failed
                    {
                        int reqLvl = nextLevelBond.bondtype1 == HeroBondType.HeroLevel ? nextLevelBond.bondvalue1 : nextLevelBond.bondvalue2;
                        int reqPts = nextLevelBond.bondtype1 == HeroBondType.HeroSkill ? nextLevelBond.bondvalue1 : nextLevelBond.bondvalue2;
                        parent.ShowFullPanel(hero, reqPts, reqLvl);
                    }
                }
            }
        }
        else
        {
            selectedHeroId = -1;
            parent.HideSubPanels();
        }
    }

    public void ToggleHeroSelectionOff()
    {
        for (int i = 0; i < heroList.Count; i++)
        {
            if (heroList[i].IsToggleOn())
            {
                heroList[i].SetToggleOn(false);
                break;
            }
        }
    }
}