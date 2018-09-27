using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Hero_UnlockedPanel : MonoBehaviour
{
    [SerializeField] Text heroNameText;
    [SerializeField] GameObject skillPointsObj;
    [SerializeField] GameObject levelUpObj;
    [SerializeField] Text reqSkillPointsText;
    [SerializeField] UI_Hero_AddSkillPointsDialog uiSkillPts;
    [SerializeField] Text reqLevelText;
    [SerializeField] Text currentLevelText;
    [SerializeField] Text moneyAmtText;

    private int heroId;

    public void ShowSkillPointsPanel(Hero hero, int reqPts)
    {
        gameObject.SetActive(true);
        SetHero(hero);
        InitSkillPoints(hero, reqPts);
        skillPointsObj.SetActive(true);
        levelUpObj.SetActive(false);
    }

    public void ShowLevelUpPanel(Hero hero, int reqLvl)
    {
        gameObject.SetActive(true);
        SetHero(hero);
        InitLevelUp(hero, reqLvl);
        levelUpObj.SetActive(true);
        skillPointsObj.SetActive(false);
    }

    public void ShowFullPanel(Hero hero, int reqPts, int reqLvl)
    {
        gameObject.SetActive(true);
        SetHero(hero);
        InitSkillPoints(hero, reqPts);
        InitLevelUp(hero, reqLvl);
        skillPointsObj.SetActive(true);
        levelUpObj.SetActive(true);
    }

    private void SetHero(Hero hero)
    {
        heroId = hero.HeroId;
        heroNameText.text = hero.HeroJson.localizedname;
    }

    public void InitSkillPoints(Hero hero, int reqPts)
    {
        reqSkillPointsText.text = reqPts.ToString();
        uiSkillPts.Init(hero, hero.GetTotalSkillPoints());
    }

    public void InitLevelUp(Hero hero, int reqLvl)
    {
        reqLevelText.text = reqLvl.ToString();
        currentLevelText.text = hero.Level.ToString();
        HeroGrowthJson data = HeroRepo.GetHeroGrowthData(hero.HeroJson.growthgroup, hero.Level);
        if (data != null)
            moneyAmtText.text = data.levelupmoney.ToString("N0");
    }

    public void UpdatePanel(Hero hero)
    {
        uiSkillPts.Init(hero, hero.GetTotalSkillPoints());
        currentLevelText.text = hero.Level.ToString();
        HeroGrowthJson data = HeroRepo.GetHeroGrowthData(hero.HeroJson.growthgroup, hero.Level);
        if (data != null)
            moneyAmtText.text = data.levelupmoney.ToString("N0");
    }

    public void OnClickLevelUp()
    {
        RPCFactory.CombatRPC.LevelUpHero(heroId);
    }

    public void Show(bool bShow)
    {
        gameObject.SetActive(bShow);
    }
}