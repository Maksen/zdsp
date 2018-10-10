using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class UI_Achievement : BaseWindowBehaviour
{
    [SerializeField] UI_ProgressBarC expProgressBar;
    [SerializeField] Text levelText;
    [SerializeField] Model_3DAvatar modelAvatar;
    [SerializeField] UI_DragEvent uiDragEvent;
    [SerializeField] Text messageText;
    [SerializeField] UI_ProgressBarC tierProgressBar;
    [SerializeField] UI_ProgressBarC collectionProgressBar;
    [SerializeField] UI_ProgressBarC achievementProgressBar;
    [SerializeField] GameObject claimRewardButton;
    [SerializeField] UI_ProgressBarC[] collectionTypesProgress;
    [SerializeField] UI_ProgressBarC[] achievementTypesProgress;

    private AchievementStatsClient achStats;
    private PlayerGhost player;

    private void Awake()
    {
        uiDragEvent.onClicked = OnClickAvatar;
        collectionProgressBar.Max = AchievementRepo.collectionObjectives.Count;
        achievementProgressBar.Max = AchievementRepo.achievementObjectives.Count;
    }

    public override void OnOpenWindow()
    {
        base.OnOpenWindow();
        Init();       
    }

    private void Init()
    {
        player = GameInfo.gLocalPlayer;
        achStats = player.AchievementStats;

        UpdateLevelProgress();
        UpdateTierProgress();
        UpdateCollectionProgress();
        UpdateAchievementProgress();


        claimRewardButton.SetActive(achStats.HasUnclaimedRewards());
    }

    public void UpdateLevelProgress()
    {
        levelText.text = player.PlayerSynStats.AchievementLevel.ToString("F1");
        AchievementLevel levelInfo = AchievementRepo.GetAchievementLevelInfo(player.PlayerSynStats.AchievementLevel);
        if (levelInfo != null)
        {
            expProgressBar.Max = levelInfo.expToNextLv;
            expProgressBar.Value = player.SecondaryStats.AchievementExp;
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
            int max = AchievementRepo.GetCollectionObjectivesByType((CollectionType)i).Count;
            int current = achStats.GetCollectionCountByType(i);
            collectionTypesProgress[i].Max = max;
            collectionTypesProgress[i].Value = current;
        }
    }

    public void UpdateAchievementProgress()
    {
        achievementProgressBar.Value = achStats.GetTotalCompletedAchievementsCount();

        for (int i = 0; i < 6; ++i)
        {
            int mainTypeId = i + 1;
            int max = AchievementRepo.GetAchievementObjectiveCountByMainType(mainTypeId);
            int slotidx = AchievementRepo.achieveMainTypeToIndexMap[mainTypeId];
            int current = achStats.GetAchievementCountByType(slotidx);
            achievementTypesProgress[i].Max = max;
            achievementTypesProgress[i].Value = current;
        }
    }

    private void OnClickAvatar()
    {

    }

}
