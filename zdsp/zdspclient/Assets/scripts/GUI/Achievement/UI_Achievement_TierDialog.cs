using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Repository;

public class UI_Achievement_TierDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataParent;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] UI_ProgressBarC progressBar;
    [SerializeField] Model_3DAvatar modelAvatar;
    [SerializeField] DragSpin3DAvatar dragSpinAvatar;
    [SerializeField] Button useTierButton;
    [SerializeField] Button unlockTierButton;

    private ToggleGroup toggleGroup;
    private Dictionary<int, Achievement_TierData> tierDataList = new Dictionary<int, Achievement_TierData>();
    private AchievementStatsClient achStats;
    private LISATransformTierJson selectedTierData;
    private UI_Achievement parent;

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

    public void Init(int currentTier, float currentTierProgress, UI_Achievement parent)
    {
        this.parent = parent;

        progressBar.Value = currentTierProgress;
        progressBar.Refresh();

        PlayerGhost player = GameInfo.gLocalPlayer;
        achStats = player.AchievementStats;

        int highestUnlockableTier = AchievementRepo.GetHighestLISATierByLevel(player.PlayerSynStats.AchievementLevel);
        unlockTierButton.interactable = achStats.HighestUnlockedTier != highestUnlockableTier;

        foreach (var data in tierDataList)
        {
            bool unlocked = data.Key <= achStats.HighestUnlockedTier;
            bool canUnlock = !unlocked && data.Key <= highestUnlockableTier;
            data.Value.SetUnlocked(unlocked, canUnlock);
        }

        Achievement_TierData tierData;
        if (tierDataList.TryGetValue(currentTier, out tierData))
            tierData.SetToggleOn(true);

        // todo: jm scroll to selected data
    }

    public void Refresh(float currentTierProgress)
    {
        progressBar.Value = currentTierProgress;
        progressBar.Refresh();

        int highestUnlockableTier = AchievementRepo.GetHighestLISATierByLevel(GameInfo.gLocalPlayer.PlayerSynStats.AchievementLevel);
        unlockTierButton.interactable = achStats.HighestUnlockedTier != highestUnlockableTier;

        foreach (var data in tierDataList)
        {
            bool unlocked = data.Key <= achStats.HighestUnlockedTier;
            bool canUnlock = !unlocked && data.Key <= highestUnlockableTier;
            data.Value.SetUnlocked(unlocked, canUnlock);
        }

        if (selectedTierData != null)
            useTierButton.interactable = selectedTierData.tierid <= achStats.HighestUnlockedTier;
    }

    private void OnTierSelected(int tier)
    {
        dragSpinAvatar.Reset();

        var tierData = AchievementRepo.GetLISATierInfoByTier(tier);
        if (tierData != null)
        {
            selectedTierData = tierData;
            modelAvatar.Change(tierData.modelpath, OnModelLoaded);

            useTierButton.interactable = tier <= achStats.HighestUnlockedTier;
        }
    }

    private void OnModelLoaded(GameObject model)
    {
        float[] camera = StaticNPCRepo.ParseCameraPosInTalk(selectedTierData.previewpos);
        Vector3 pos = model.transform.parent.localPosition;
        model.transform.parent.localPosition = new Vector3(camera[0], camera[1], pos.z);
        model.transform.localRotation = Quaternion.Euler(new Vector3(0, camera[2], 0));
        model.transform.localScale = new Vector3(camera[3], camera[3], camera[3]);
    }

    public void OnClickUnlockNextTier()
    {
         RPCFactory.CombatRPC.UnlockNextLISATier();
    }

    public void OnClickChangeTier()
    {
        if (selectedTierData != null && selectedTierData.tierid != achStats.CurrentLISATier)
            RPCFactory.CombatRPC.ChangeLISATier(selectedTierData.tierid);

        GetComponent<UIDialog>().ClickClose();
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();

        bool allowSwitchOff = toggleGroup.allowSwitchOff;
        toggleGroup.allowSwitchOff = true;
        foreach (var data in tierDataList.Values)
            data.SetToggleOn(false);
        toggleGroup.allowSwitchOff = allowSwitchOff;

        modelAvatar.Cleanup();

        parent.ShowAvatar(true);
    }
}