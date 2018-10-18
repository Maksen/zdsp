using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;

public class Hero_BondSimpleData : MonoBehaviour
{
    [SerializeField] Text bondNameText;
    [SerializeField] Text bondLevelText;
    [SerializeField] Text heroCountText;
    [SerializeField] Transform heroDataParent;
    [SerializeField] GameObject heroDataPrefab;
    [SerializeField] Text[] reqmtText;
    [SerializeField] Text[] reqmtValueText;
    [SerializeField] Text[] seText;

    private HeroStatsClient heroStats;
    private List<Hero_BondHeroData> heroList = new List<Hero_BondHeroData>();
    private HeroBond heroBond;
    private HeroBondJson nextLevelBond;
    private Toggle toggle;

    public void SetUp(ToggleGroup group)
    {
        toggle = GetComponent<Toggle>();
        toggle.group = group;
    }

    public void Init(HeroBond bond)
    {
        heroStats = GameInfo.gLocalPlayer.HeroStats;
        heroBond = bond;

        bondNameText.text = bond.heroBondGroupJson.localizedname;
        DetermineFulfilledBondLevel();
        InitHeroList();
    }

    private void DetermineFulfilledBondLevel()
    {
        int currentLevel;
        HeroBondJson currentLevelBond = heroBond.GetHighestFulfilledLevel(heroStats);
        if (currentLevelBond != null)
        {
            currentLevel = currentLevelBond.bondlevel;
            int index = heroBond.heroBondJsonList.IndexOf(currentLevelBond);
            nextLevelBond = index + 1 < heroBond.heroBondJsonList.Count ? heroBond.heroBondJsonList[index + 1] : currentLevelBond;
        }
        else
        {
            currentLevel = 0;
            nextLevelBond = heroBond.heroBondJsonList.Count > 0 ? heroBond.heroBondJsonList[0] : null;
        }
        bondLevelText.text = nextLevelBond != null ? nextLevelBond.bondlevel.ToString() : currentLevel.ToString();

        InitBondInfo(nextLevelBond); // may be null if no data in kopio
    }

    private void InitBondInfo(HeroBondJson bondData)
    {
        seText[0].text = ""; // empty out the text first
        seText[1].text = ""; // empty out the text first

        if (bondData != null)
        {
            SetBondRequirementText(0, bondData.bondtype1, bondData.bondvalue1);
            SetBondRequirementText(1, bondData.bondtype2, bondData.bondvalue2);

            int index = 0;
            foreach (SideEffectJson se in bondData.sideeffects.Values)
            {
                if (index < seText.Length)
                {
                    if (se.isrelative)
                        seText[index++].text = string.Format("{0} +{1}%", se.effecttype.ToString(), se.max);  // todo: jm localize
                    else
                        seText[index++].text = string.Format("{0} +{1}", se.effecttype.ToString(), se.max);
                }
            }
        }
        else // no bond data, this should not happen unless kopio is not filled
        {
            SetBondRequirementText(0, HeroBondType.None, 0);
            SetBondRequirementText(1, HeroBondType.None, 0);
        }
    }

    private void InitHeroList()
    {
        ClientUtils.DestroyChildren(heroDataParent);
        heroList.Clear();

        int fulfulledHeroCount = 0;
        int totalCount = heroBond.heroIds.Count;
        for (int i = 0; i < totalCount; i++)
        {
            GameObject obj = ClientUtils.CreateChild(heroDataParent, heroDataPrefab);
            Hero_BondHeroData heroData = obj.GetComponent<Hero_BondHeroData>();
            heroData.Init(heroBond.heroIds[i], OnHeroSelected);
            bool fulfilled = heroStats.HasHeroFulfilledBond(nextLevelBond, heroBond.heroIds[i]);
            heroData.SetFulfilled(fulfilled);
            heroList.Add(heroData);
            if (fulfilled)
                fulfulledHeroCount++;
        }
        heroCountText.text = fulfulledHeroCount + "/" + totalCount;
    }

    public void Refresh(int heroId)
    {
        if (heroBond.IsHeroInvolved(heroId))
        {
            DetermineFulfilledBondLevel();

            int fulfulledHeroCount = 0;
            int totalCount = heroBond.heroIds.Count;
            for (int i = 0; i < totalCount; i++)
            {
                bool fulfilled = heroStats.HasHeroFulfilledBond(nextLevelBond, heroBond.heroIds[i]);
                heroList[i].SetFulfilled(fulfilled);
                if (fulfilled)
                    fulfulledHeroCount++;
            }
            heroCountText.text = fulfulledHeroCount + "/" + totalCount;
        }
    }

    private void SetBondRequirementText(int index, HeroBondType bondType, int value)
    {
        string guiname = "";
        switch (bondType)
        {
            case HeroBondType.None:
                guiname = "";
                break;

            case HeroBondType.HeroLevel:
                guiname = "hro_bondreq_herolevel";
                break;

            case HeroBondType.HeroSkill:
                guiname = "hro_bondreq_heroskill";
                break;
        }

        if (!string.IsNullOrEmpty(guiname))
        {
            reqmtText[index].text = GUILocalizationRepo.GetLocalizedString(guiname);
            reqmtValueText[index].text = value.ToString();
            reqmtText[index].transform.parent.gameObject.SetActive(true);
        }
        else
        {
            reqmtText[index].transform.parent.gameObject.SetActive(false);
        }
    }

    private void OnHeroSelected(int heroId)
    {
        UIManager.OpenDialog(WindowType.DialogHeroBonds,
            (window) => window.GetComponent<UI_Hero_BondsDialog>().Init(heroId, heroBond.heroBondGroupJson.id));
    }

    public bool IsToggleOn()
    {
        return toggle.isOn;
    }

    public void ToggleOff()
    {
        if (toggle.isOn)
        {
            toggle.isOn = false;
        }
    }
}