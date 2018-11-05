using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_Achievement_TierDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataParent;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] UI_ProgressBarC progressBar;

    private ToggleGroup toggleGroup;
    private Dictionary<int, Achievement_TierData> tierDataList = new Dictionary<int, Achievement_TierData>();
    private AchievementStatsClient achStats;

    private void Awake()
    {
        toggleGroup = dataParent.GetComponent<ToggleGroup>();
        Setup();
    }

    private void Setup()
    {
        progressBar.Max = 1;

        var tierList = AchievementRepo.lisaTiers.Values;
        foreach (var tier in tierList)
        {
            GameObject go = ClientUtils.CreateChild(dataParent, dataPrefab);
            Achievement_TierData tierData = go.GetComponent<Achievement_TierData>();
            tierData.Init(tier, toggleGroup, OnTierSelected);
            tierDataList.Add(tier.tierid, tierData);
        }
    }

    public void Init(int currentTier, float currentTierProgress)
    {
        progressBar.Value = currentTierProgress;
        progressBar.Refresh();

        achStats = GameInfo.gLocalPlayer.AchievementStats;
        foreach (var data in tierDataList)
            data.Value.SetUnlocked(achStats.HighestUnlockedTier >= data.Key);

        EnableTogglesCallback(false);
        Achievement_TierData tierData;
        if (tierDataList.TryGetValue(currentTier, out tierData))
            tierData.SetToggleOn(true);
        EnableTogglesCallback(true);

        // todo: jm scroll to selected data
    }

    public void Refresh(float currentTierProgress)
    {
        progressBar.Value = currentTierProgress;
        progressBar.Refresh();

        foreach (var data in tierDataList)
            data.Value.SetUnlocked(achStats.HighestUnlockedTier >= data.Key);
    }

    private void EnableTogglesCallback(bool value)
    {
        foreach (var toggle in tierDataList.Values)
            toggle.EnableToggleCallback(value);
    }

    private void OnTierSelected(int tier)
    {
        if (tier != achStats.CurrentLISATier)
            RPCFactory.CombatRPC.ChangeLISATier(tier);
        GetComponent<UIDialog>().ClickClose();
    }

    public void OnClickUnlockNextTier()
    {
        if (achStats.CanUnlockLISATier())
            RPCFactory.CombatRPC.UnlockNextLISATier();
    }
}