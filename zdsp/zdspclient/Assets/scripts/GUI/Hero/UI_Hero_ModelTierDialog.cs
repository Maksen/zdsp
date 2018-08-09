using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Hero_ModelTierDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataTransform;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] Image currentModelImage;

    private Dictionary<int, Hero_ModelTierData> tierDataList = new Dictionary<int, Hero_ModelTierData>();
    private Hero hero;
    private Action<int> OnSelectedCallback;

    private void Awake()
    {
        ToggleGroup toggleGroup = dataTransform.GetComponent<ToggleGroup>();
        for (int i = 1; i <= 3; i++)
        {
            GameObject obj = ClientUtils.CreateChild(dataTransform, dataPrefab);
            Hero_ModelTierData tierData = obj.GetComponent<Hero_ModelTierData>();
            tierData.Setup(i, toggleGroup, OnTierSelected);
            tierDataList.Add(i, tierData);
        }
    }

    // called by Init which is after Awake
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

    public void Init(Hero hero, int currentTier, Sprite currentSprite, Action<int> callback)
    {
        this.hero = hero;
        currentModelImage.sprite = currentSprite;
        OnSelectedCallback = callback;

        for (int i = 1; i <= 3; i++)
        {
            int reqPts = hero.GetModelTierUnlockPoints(i);
            bool unlocked = hero.SlotIdx != -1 && reqPts > 0 && hero.GetTotalSkillPoints() >= reqPts;
            tierDataList[i].Init(hero.HeroJson, reqPts, unlocked);
        }

        InitSkinItems();

        EnableTogglesCallback(false);
        tierDataList[currentTier].SetToggleOn(true);
        EnableTogglesCallback(true);
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