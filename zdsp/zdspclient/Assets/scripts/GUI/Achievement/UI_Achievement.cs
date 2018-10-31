using Kopio.JsonContracts;
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
    [SerializeField] Animator zoomAnimator;
    [SerializeField] Toggle messageToggle;
    [SerializeField] Text messageText;
    [SerializeField] GameObject messageObj;

    [Header("Right Side")]
    [SerializeField] UI_ProgressBarC tierProgressBar;
    [SerializeField] UI_ProgressBarC collectionProgressBar;
    [SerializeField] UI_ProgressBarC achievementProgressBar;
    [SerializeField] UI_ProgressBarC[] collectionTypesProgress;
    [SerializeField] UI_ProgressBarC[] achievementTypesProgress;
    [SerializeField] Button claimRewardBtn;
    [SerializeField] UI_Achievement_AchievementDetails uiAchDetails;

    private AchievementStatsClient achStats;
    private PlayerGhost player;
    private LISATransformTierJson currentTierData;

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
        dragSpinAvatar.Reset();
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
        else
        {
            expProgressBar.Max = 100;
            expProgressBar.Value = 0;
        }
        expProgressBar.Refresh();
    }

    public void UpdateTierProgress()
    {
        var currentTierInfo = AchievementRepo.GetLISATierInfoByLevel(player.PlayerSynStats.AchievementLevel);
        if (currentTierInfo != null)
        {
            UpdateTier(currentTierInfo.tierid);
            var nextTierInfo = AchievementRepo.GetLISATierInfoByTier(currentTierInfo.tierid + 1);
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
        else
        {
            tierProgressBar.Max = 100;
            tierProgressBar.Value = 0;
        }
        tierProgressBar.Refresh();
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

    public void ZoomInAvatar()
    {
        if (currentTierData != null)
        {
            switch (currentTierData.modeltype)
            {
                case LISAModelType.Energy:
                    zoomAnimator.Play("ZoomScaler_Energy_ZoomIn");
                    break;
                case LISAModelType.Bird:
                    zoomAnimator.Play("ZoomScaler_Bird_ZoomIn");
                    break;
                case LISAModelType.LISA:
                    zoomAnimator.Play("ZoomScaler_Lisa_ZoomIn");
                    break;
            }
        }

        messageObj.SetActive(false);
        claimRewardBtn.gameObject.SetActive(false);
    }

    public void ZoomOutAvatar()
    {
        if (currentTierData != null)
        {
            switch (currentTierData.modeltype)
            {
                case LISAModelType.Energy:
                    zoomAnimator.Play("ZoomScaler_Energy_ZoomOut");
                    break;
                case LISAModelType.Bird:
                    zoomAnimator.Play("ZoomScaler_Bird_ZoomOut");
                    break;
                case LISAModelType.LISA:
                    zoomAnimator.Play("ZoomScaler_Lisa_ZoomOut");
                    break;
            }
        }

        messageObj.SetActive(true);
        claimRewardBtn.gameObject.SetActive(true);
    }

    public void OnCloseAchievementDetails()
    {
        uiAchDetails.CleanUp();
    }

    public void OnClickTier()
    {
        if (currentTierData == null)
            return;

        float currentProgress = tierProgressBar.Value / tierProgressBar.Max;
        UIManager.OpenDialog(WindowType.DialogAchievementTier,
            (window) => window.GetComponent<UI_Achievement_TierDialog>().Init(currentTierData.tierid, currentProgress, UpdateTier));
    }

    private void UpdateTier(int tier)
    {
        dragSpinAvatar.Reset();

        LISATransformTierJson tierData = AchievementRepo.GetLISATierInfoByTier(tier);
        if (tierData != null)
        {
            modelAvatar.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);  //to be removed
            currentTierData = tierData;
            modelAvatar.Change(tierData.modelpath, OnModelLoaded);
        }
        else
            modelAvatar.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true); //to be removed
    }

    private void OnModelLoaded(GameObject model)
    {
        float[] camera = StaticNPCRepo.ParseCameraPosInTalk(currentTierData.posinui);
        Vector3 pos = model.transform.parent.localPosition;
        model.transform.parent.localPosition = new Vector3(camera[0], camera[1], pos.z);
        model.transform.localRotation = Quaternion.Euler(new Vector3(0, camera[2], 0));
        model.transform.localScale = new Vector3(camera[3], camera[3], camera[3]);
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