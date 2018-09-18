using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zealot.Common;
using Zealot.Repository;
using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Client.Entities;
using System;

public class UI_ClueMessageData : MonoBehaviour
{
    [SerializeField]
    Image HeroIcon;

    [SerializeField]
    Text Message;

    [SerializeField]
    Image ClueIcon;

    [SerializeField]
    GameObject ClockIcon;

    [SerializeField]
    Text TimeText;

    [SerializeField]
    GameObject NewClue;

    [SerializeField]
    Sprite SystemIcon;

    [SerializeField]
    Sprite VideoIcon;

    [SerializeField]
    Sprite SoundIcon;

    [SerializeField]
    Sprite PhotoIcon;

    private ActivatedClueData mClueData;
    private DateTime mCountDownTime;

    public void Init(ActivatedClueData clueData)
    {
        mClueData = clueData;
        ClueType type = (ClueType)mClueData.ClueType;
        if (type == ClueType.Normal)
        {
            DestinyClueJson clueJson = DestinyClueRepo.GetDestinyClueById(mClueData.ClueId);
            if (clueJson != null)
            {
                InitDestinyClue(clueJson);
            }
        }
        else if (type == ClueType.Dialogue)
        {
            HeroDialogueClueJson dialogueClueJson = DestinyClueRepo.GetHeroDialogueClueById(mClueData.ClueId);
            if (dialogueClueJson != null)
            {
                InitDialogueClue(dialogueClueJson);
            }
        }
        else if (type == ClueType.Time)
        {
            TimeClueJson timeClueJson = DestinyClueRepo.GetTimeClueById(mClueData.ClueId);
            if (timeClueJson != null)
            {
                InitTimeClue(timeClueJson);
            }
        }
    }

    private void InitDestinyClue(DestinyClueJson clueJson)
    {
        HeroIcon.sprite = SystemIcon;
        if (clueJson != null)
        {
            if (clueJson.category == ClueCategory.Word)
            {
                Message.gameObject.SetActive(true);
                Message.text = clueJson.text;
                ClueIcon.gameObject.SetActive(false);
            }
            else
            {
                Message.gameObject.SetActive(false);
                ClueIcon.gameObject.SetActive(true);
                ClueIcon.sprite = GetIconSprite(clueJson.category);
            }
            ClockIcon.SetActive(false);
            TimeText.text = mClueData.ActivatedTime;
            NewClue.SetActive(mClueData.Status == (byte)ClueStatus.New ? true : false);
        }
    }

    private void InitDialogueClue(HeroDialogueClueJson dialogueClueJson)
    {
        HeroIcon.sprite = ClientUtils.LoadIcon(dialogueClueJson.avatarpath);
        Message.gameObject.SetActive(true);
        Message.text = dialogueClueJson.text;
        ClueIcon.gameObject.SetActive(false);
        ClockIcon.SetActive(false);
        TimeText.text = mClueData.ActivatedTime;
        NewClue.SetActive(mClueData.Status == (byte)ClueStatus.New ? true : false);
    }

    private void InitTimeClue(TimeClueJson timeClueJson)
    {
        HeroIcon.sprite = ClientUtils.LoadIcon(timeClueJson.avatarpath);
        if (timeClueJson.category == ClueCategory.Word)
        {
            Message.gameObject.SetActive(true);
            Message.text = timeClueJson.text;
            ClueIcon.gameObject.SetActive(false);
        }
        else
        {
            Message.gameObject.SetActive(false);
            ClueIcon.gameObject.SetActive(true);
            ClueIcon.sprite = GetIconSprite(timeClueJson.category);
        }
        ClockIcon.SetActive(true);
        mCountDownTime = mClueData.ActivatedDT.AddMinutes(timeClueJson.time);
        UpdateTime();
        NewClue.SetActive(mClueData.Status == (byte)ClueStatus.New ? true : false);
    }

    private void UpdateTime()
    {
        double remaintime = (mCountDownTime - DateTime.Now).TotalSeconds;
        if (remaintime > 0)
        {
            TimeText.text = GUILocalizationRepo.GetLocalizedTimeString((int)remaintime, 2);
            StartCoroutine(TimeCountDown());
        }
    }

    private IEnumerator TimeCountDown()
    {
        yield return new WaitForSecondsRealtime(1);
        UpdateTime();
    }

    private Sprite GetIconSprite(ClueCategory category)
    {
        if (category == ClueCategory.Photo)
        {
            return PhotoIcon;
        }
        else if (category == ClueCategory.Sound)
        {
            return SoundIcon;
        }
        else if (category == ClueCategory.Video)
        {
            return VideoIcon;
        }
        else
        {
            return null;
        }
    }

    public void OnClickClueIcon()
    {
        ClueStatus status = (ClueStatus)mClueData.Status;

        if (status == ClueStatus.New)
        {
            RPCFactory.NonCombatRPC.ReadClue(mClueData.ClueId, mClueData.ClueType);
        }

        ClueType type = (ClueType)mClueData.ClueType;
        if (type == ClueType.Dialogue)
        {
            PlayerGhost player = GameInfo.gLocalPlayer;
            HeroDialogueClueJson dialogueClueJson = DestinyClueRepo.GetHeroDialogueClueById(mClueData.ClueId);
            if (dialogueClueJson != null && player != null)
            {
                CollectRewardData data = new CollectRewardData(mClueData.ClueId, CollectRewardType.DestinyClue);
                List<RewardItem> rewardlist = RewardListRepo.GetRewardItemsByGrpIDJobID(dialogueClueJson.reward, player.PlayerSynStats.jobsect);
                UIManager.OpenDialog(WindowType.DialogClaimReward, 
                    (window) => window.GetComponent<UI_DialogClaimReward>().Init(rewardlist, data, "destinycliamreward_title", status != ClueStatus.Collected ? true : false));
            }
        }
        else
        {
            UIManager.OpenDialog(WindowType.DialogMessageFilter, (window) => window.GetComponent<UI_MessageFilter>().Init(mClueData));
        }
    }
}
