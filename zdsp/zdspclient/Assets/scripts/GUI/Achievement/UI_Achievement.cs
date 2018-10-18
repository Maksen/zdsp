using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class UI_Achievement : BaseWindowBehaviour
{
    [Header("Left Side")]
    [SerializeField] UI_ProgressBarC expProgressBar;
    [SerializeField] Text levelText;
    [SerializeField] Model_3DAvatar modelAvatar;
    [SerializeField] DragSpin3DAvatar dragSpinAvatar;
    [SerializeField] UI_DragEvent uiDragEvent;
    [SerializeField] Toggle messageToggle;
    [SerializeField] Text messageText;

    [Header("Right Side")]
    [SerializeField] UI_ProgressBarC tierProgressBar;
    [SerializeField] UI_ProgressBarC collectionProgressBar;
    [SerializeField] UI_ProgressBarC achievementProgressBar;
    [SerializeField] UI_ProgressBarC[] collectionTypesProgress;
    [SerializeField] UI_ProgressBarC[] achievementTypesProgress;
    [SerializeField] Button claimRewardBtn;

    private AchievementStatsClient achStats;
    private PlayerGhost player;

    private void Awake()
    {
        uiDragEvent.onClicked = OnClickAvatar;
    }

    private void Start()
    {
        collectionProgressBar.Max = AchievementRepo.collectionObjectives.Count;
        achievementProgressBar.Max = AchievementRepo.achievementObjectives.Count;

        int length = Enum.GetNames(typeof(CollectionType)).Length;
        for (int i = 0; i < length; ++i)
            collectionTypesProgress[i].Max = AchievementRepo.GetCollectionObjectivesByType((CollectionType)i).Count;

        length = Enum.GetNames(typeof(AchievementType)).Length;
        for (int i = 0; i < length; ++i)
            achievementTypesProgress[i].Max = AchievementRepo.GetAchievementObjectiveCountByMainType((AchievementType)i);
    }

    public override void OnOpenWindow()
    {
        base.OnOpenWindow();
        Init();
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        messageText.text = "";
        messageToggle.isOn = false;
    }

    private void Init()
    {
        player = GameInfo.gLocalPlayer;
        achStats = player.AchievementStats;

        UpdateLevelProgress();
        UpdateTierProgress();
        UpdateCollectionProgress();
        UpdateAchievementProgress();

        claimRewardBtn.interactable = achStats.HasUnclaimedRewards();
    }

    public void UpdateLevelProgress()
    {
        AchievementLevel levelInfo = AchievementRepo.GetAchievementLevelInfo(player.PlayerSynStats.AchievementLevel);
        if (levelInfo != null)
        {
            levelText.text = levelInfo.name;
            if (player.PlayerSynStats.AchievementLevel == AchievementRepo.ACHIEVEMENT_MAX_LEVEL)
            {
                expProgressBar.Max = 100;
                expProgressBar.Value = 100;
            }
            else
            {
                expProgressBar.Max = levelInfo.expToNextLv;
                expProgressBar.Value = player.SecondaryStats.AchievementExp;
            }
        }
    }

    public void UpdateTierProgress()
    {
        var currentTierInfo = AchievementRepo.GetLISATierInfoByLevel(player.PlayerSynStats.AchievementLevel);
        if (currentTierInfo != null)
        {
            var nextTierInfo = AchievementRepo.GetLISATierInfoByTier(currentTierInfo.tierid);
            if (nextTierInfo != null) // still have next tier
            {
                tierProgressBar.Max = nextTierInfo.reqlvl - currentTierInfo.reqlvl;
                tierProgressBar.Value = player.PlayerSynStats.AchievementLevel - currentTierInfo.reqlvl;
            }
            else  // already maxed tier
            {
                tierProgressBar.Max = currentTierInfo.reqlvl;
                tierProgressBar.Value = currentTierInfo.reqlvl;
            }
        }
    }

    public void UpdateCollectionProgress()
    {
        collectionProgressBar.Value = achStats.GetTotalCompletedCollectionsCount();

        int length = Enum.GetNames(typeof(CollectionType)).Length;
        for (int i = 0; i < length; ++i)
        {
            collectionTypesProgress[i].Value = achStats.GetCollectionCountByType(i);
        }
    }

    public void UpdateAchievementProgress()
    {
        achievementProgressBar.Value = achStats.GetTotalCompletedAchievementsCount();

        int length = Enum.GetNames(typeof(AchievementType)).Length;
        for (int i = 0; i < length; ++i)
        {
            achievementTypesProgress[i].Value = achStats.GetAchievementCountByType(i);
        }
    }

    public void SetClaimRewardButton(bool interactable)
    {
        claimRewardBtn.interactable = interactable;
    }

    private void OnClickAvatar()
    {
        
    }

    public void OnClickTier()
    {

    }

    public void OnClickExternalFunctions()
    {
        UIManager.OpenDialog(WindowType.DialogAchievementFunctions);
    }

    public void OnClickAbility()
    {
        UIManager.OpenDialog(WindowType.DialogAchievementAbility);
    }

    public void OnClickClaimRewards()
    {
        UIManager.OpenDialog(WindowType.DialogAchievementRewards,
            (window) => window.GetComponent<UI_Achievement_RewardsDialog>().InitRewardsList(achStats.claimsList));
    }
}