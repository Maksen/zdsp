using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_Hero_ModelTierDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataTransform;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] Image currentModelImage;

    private List<Hero_ModelTierData> dataList = new List<Hero_ModelTierData>();
    private int selectedTier;
    private Hero hero;

    public override void OnRegister()
    {
        base.OnRegister();

        ToggleGroup toggleGroup = dataTransform.GetComponent<ToggleGroup>();
        for (int i = 1; i <= 3; i++)
        {
            GameObject obj = ClientUtils.CreateChild(dataTransform, dataPrefab);
            Hero_ModelTierData tierData = obj.GetComponent<Hero_ModelTierData>();
            tierData.Setup(i, toggleGroup, OnTierSelected);
            dataList.Add(tierData);
        }
    }

    public void Init(Hero hero, Sprite currentSprite)
    {
        this.hero = hero;
        currentModelImage.sprite = currentSprite;

        for (int i = 0; i < dataList.Count; i++)
            dataList[i].Init(hero.HeroJson, hero.IsModelTierUnlocked(i + 1));

        dataList[hero.ModelTier - 1].SetToggleOn(true);
    }

    private void OnTierSelected(int tier)
    {
        if (hero.IsModelTierUnlocked(tier))
            selectedTier = tier;
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_hero_SkinIsLocked"));
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        if (selectedTier != hero.ModelTier)
            RPCFactory.CombatRPC.ChangeHeroModelTier(hero.HeroId, selectedTier);
    }
}