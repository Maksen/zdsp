using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_Achievement_TierDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataParent;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] UI_ProgressBarC progressBar;

    private Action<int> OnSelectedCallback;
    private ToggleGroup toggleGroup;
    private Dictionary<int, Achievement_TierData> tierDataList = new Dictionary<int, Achievement_TierData>();

    private void Awake()
    {
        toggleGroup = dataParent.GetComponent<ToggleGroup>();
    }

    public void Init(int currentTier, float currentTierProgress, Action<int> callback)
    {
        ClientUtils.DestroyChildren(dataParent);

        OnSelectedCallback = callback;
        progressBar.Max = 1;
        progressBar.Value = currentTierProgress;

        var tierList = AchievementRepo.lisaTiers.Values;
        foreach (var tier in tierList)
        {
            GameObject go = ClientUtils.CreateChild(dataParent, dataPrefab);
            Achievement_TierData tierData = go.GetComponent<Achievement_TierData>();
            bool unlocked = GameInfo.gLocalPlayer.PlayerSynStats.AchievementLevel >= tier.reqlvl;
            tierData.Init(tier, toggleGroup, OnTierSelected, unlocked);
            tierDataList.Add(tier.tierid, tierData);
        }

        EnableTogglesCallback(false);
        if (tierDataList.ContainsKey(currentTier))
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
        tierDataList.Clear();
        ClientUtils.DestroyChildren(dataParent);
    }
}