using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_Hero_ModelTierDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataTransform;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] Image currentModelImage;

    private Dictionary<int, Hero_ModelTierData> tierDataList = new Dictionary<int, Hero_ModelTierData>();
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
            tierDataList.Add(i, tierData);
        }
    }

    private void InitSkinItems()
    {
        ToggleGroup toggleGroup = dataTransform.GetComponent<ToggleGroup>();
        string[] skinitems = hero.HeroJson.skinitemid.Split(';');
        for (int i = 0; i < skinitems.Length; i++)
        {
            int itemId;
            if (int.TryParse(skinitems[i], out itemId))
            {
                GameObject obj = ClientUtils.CreateChild(dataTransform, dataPrefab);
                Hero_ModelTierData tierData = obj.GetComponent<Hero_ModelTierData>();
                tierData.Setup(itemId, toggleGroup, OnTierSelected);
                tierData.InitSkinItem(hero.IsModelTierUnlocked(itemId));
                tierDataList.Add(itemId, tierData);
            }
        }
    }

    public void Init(Hero hero, Sprite currentSprite)
    {
        this.hero = hero;
        currentModelImage.sprite = currentSprite;

        for (int i = 1; i <= 3; i++)
        {
            int reqPts = hero.GetModelTierUnlockPoints(i);
            bool unlocked = reqPts > 0 && hero.GetTotalSkillPoints() >= reqPts;
            tierDataList[i].Init(hero.HeroJson, reqPts, unlocked);
        }

        InitSkinItems();

        tierDataList[hero.ModelTier].SetToggleOn(true);  // select hero's current model tier
    }

    private void OnTierSelected(int tier)
    {
        //print("selected tier: " + tier);
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

        string[] skinitems = hero.HeroJson.skinitemid.Split(';');
        for (int i = 0; i < skinitems.Length; i++)
        {
            int itemId;
            if (int.TryParse(skinitems[i], out itemId))
            {
                Hero_ModelTierData data = tierDataList[itemId];
                tierDataList.Remove(itemId);
                Destroy(data.gameObject);
            }
        }
    }
}