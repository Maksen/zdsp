using System;
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
    private Hero hero;
    private Action<int> OnSelectedCallback;
    private ToggleGroup toggleGroup;

    private void Awake()
    {
        toggleGroup = dataTransform.GetComponent<ToggleGroup>();
    }

    public void Init(Hero hero, int currentTier, Sprite currentSprite, Action<int> callback)
    {
        this.hero = hero;
        currentModelImage.sprite = currentSprite;
        OnSelectedCallback = callback;

        int highestPossible = HeroRepo.GetHighestModelTierByHeroId(hero.HeroId);
        int highestUnlocked = hero.GetHighestUnlockedTier();
        for (int i = 1; i <= 3; i++)
        {
            int reqPts = hero.GetModelTierUnlockPoints(i);
            if (reqPts > 0 && (i > highestUnlocked || i == highestPossible))
            {
                GameObject obj = ClientUtils.CreateChild(dataTransform, dataPrefab);
                Hero_ModelTierData tierData = obj.GetComponent<Hero_ModelTierData>();
                tierData.Setup(i, toggleGroup, OnTierSelected);
                bool unlocked = hero.SlotIdx != -1 && hero.GetTotalSkillPoints() >= reqPts;
                tierData.Init(hero.HeroJson, reqPts, unlocked);
                tierDataList.Add(i, tierData);
            }
        }

        InitSkinItems();

        EnableTogglesCallback(false);
        if (tierDataList.ContainsKey(currentTier))
            tierDataList[currentTier].SetToggleOn(true);
        EnableTogglesCallback(true);
    }

    // called by Init which is after Awake
    private void InitSkinItems()
    {
        string[] skinitems = hero.HeroJson.skinitemid.Split(';');
        for (int i = 0; i < skinitems.Length; i++)
        {
            string[] itemids = skinitems[i].Split(',');
            int bindItemId = 0;
            if (itemids.Length > 0 && int.TryParse(itemids[0], out bindItemId))  // only show from bind item
            {
                GameObject obj = ClientUtils.CreateChild(dataTransform, dataPrefab);
                Hero_ModelTierData tierData = obj.GetComponent<Hero_ModelTierData>();
                tierData.Setup(bindItemId, toggleGroup, OnTierSelected);
                tierData.InitSkinItem(hero.IsModelTierUnlocked(bindItemId));
                tierDataList.Add(bindItemId, tierData);
            }
        }
    }

    private void EnableTogglesCallback(bool value)
    {
        foreach (var toggle in tierDataList.Values)
            toggle.EnableToggleCallback(value);
    }

    private void OnTierSelected(int tier)
    {
        OnSelectedCallback(tier);
        GetComponent<UIDialog>().ClickClose();
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        tierDataList.Clear();
        ClientUtils.DestroyChildren(dataTransform);
    }
}