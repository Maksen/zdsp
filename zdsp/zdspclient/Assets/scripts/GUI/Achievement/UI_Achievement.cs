using Kopio.JsonContracts;
using System;
using System.Collections;
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
    [SerializeField] UI_Achievement_CollectionDetails uiColDetails;

    private AchievementStatsClient achStats;
    private PlayerGhost player;
    private LISATransformTierJson currentTierData;
    private DateTime lastLisaMsgTime;
    private bool hasRewardFunctionUnlocked;
    private Coroutine refreshCoroutine;
    private WaitForSeconds refreshWait;

    private void Awake()
    {
        uiDragEvent.onClicked = OnClickAvatar;
        uiDragEvent.onBeginDrag = OnClickAvatar;
        float refreshInterval = GameConstantRepo.GetConstantInt("LISA_MessageRefreshInterval", 60);
        refreshWait = new WaitForSeconds(refreshInterval);
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

        player = GameInfo.gLocalPlayer;
        achStats = player.AchievementStats;

        UpdateLevelProgress();
        UpdateTierProgress();
        UpdateCollectionProgress();
        UpdateAchievementProgress();
        UpdateAvatarModel(achStats.CurrentLISATier);

        claimRewardBtn.interactable = achStats.HasUnclaimedRewards();

        lastLisaMsgTime = DateTime.Now.AddHours(-1);
        messageText.text = "";
        ShowLisaMessage(LISAMsgBehaviourType.OnOpen);
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        messageText.text = "";
        messageToggle.isOn = false;
        messageObj.SetActive(true);
        claimRewardBtn.gameObject.SetActive(true);
        dragSpinAvatar.Reset();
        modelAvatar.Cleanup();
        uiAchDetails.CleanUp();
        uiColDetails.CleanUp();
        hasRewardFunctionUnlocked = false;
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
            refreshCoroutine = null;
        }
    }

    public void OnAchievementLevelUp(bool isUIOpen)
    {
        if (isUIOpen)
        {
            UpdateLevelProgress();
            UpdateTierProgress();
        }

        AchievementLevel levelInfo = AchievementRepo.GetAchievementLevelInfo(player.PlayerSynStats.AchievementLevel);
        if (levelInfo != null)
            hasRewardFunctionUnlocked = levelInfo.rewardFunction != LISAFunction.None;
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
        var currentTierInfo = AchievementRepo.GetLISATierInfoByTier(achStats.HighestUnlockedTier);
        if (currentTierInfo != null)
        {
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

        GameObject tierDialog = UIManager.GetWindowGameObject(WindowType.DialogAchievementTier);
        if (tierDialog != null && tierDialog.activeInHierarchy)
        {
            float currentProgress = tierProgressBar.Value / tierProgressBar.Max;
            tierDialog.GetComponent<UI_Achievement_TierDialog>().Refresh(currentProgress);
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
        ShowLisaMessage(LISAMsgBehaviourType.OnTouchAvatar);
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
        ShowLisaMessage(LISAMsgBehaviourType.OnOpen);
    }

    public void OnClickTier()
    {
        if (currentTierData == null)
            return;

        float currentProgress = tierProgressBar.Value / tierProgressBar.Max;
        UIManager.OpenDialog(WindowType.DialogAchievementTier,
            (window) => window.GetComponent<UI_Achievement_TierDialog>().Init(currentTierData.tierid, currentProgress, this));
        ShowAvatar(false);
    }

    public void UpdateAvatarModel(int tier)
    {
        dragSpinAvatar.Reset();

        LISATransformTierJson tierData = AchievementRepo.GetLISATierInfoByTier(tier);
        if (tierData != null)
        {
            currentTierData = tierData;
            modelAvatar.Change(tierData.modelpath, OnModelLoaded);
        }
    }

    private void OnModelLoaded(GameObject model)
    {
        float[] camera = StaticNPCRepo.ParseCameraPosInTalk(currentTierData.mainuipos);
        Vector3 pos = model.transform.parent.localPosition;
        model.transform.parent.localPosition = new Vector3(camera[0], camera[1], pos.z);
        model.transform.localRotation = Quaternion.Euler(new Vector3(0, camera[2], 0));
        model.transform.localScale = new Vector3(camera[3], camera[3], camera[3]);
    }

    public void ShowAvatar(bool value)
    {
        modelAvatar.ShowModel(value);
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

    public void ShowLisaMessage(LISAMsgBehaviourType behaviourType)
    {
        DateTime now = DateTime.Now;
        TimeSpan interval = now.Subtract(lastLisaMsgTime);
        if (interval.TotalSeconds >= 3)
        {
            lastLisaMsgTime = now;

            string message = AchievementRepo.GetRandomLisaMessage(behaviourType, hasRewardFunctionUnlocked,
                achStats.HasUnclaimedRewards(), achStats.CanUnlockLISATier());

            if (!string.IsNullOrEmpty(message))
            {
                string currentMsg = messageText.text;
                messageText.text = string.IsNullOrEmpty(currentMsg) ? message : string.Format("{0}\n{1}", currentMsg, message);
            }

            if (refreshCoroutine != null)
                StopCoroutine(refreshCoroutine);
            refreshCoroutine = StartCoroutine(IdleRefreshLisaMessage());
        }
    }

    private IEnumerator IdleRefreshLisaMessage()
    {
        yield return refreshWait;
        ShowLisaMessage(LISAMsgBehaviourType.RegularRefresh);
    }
}