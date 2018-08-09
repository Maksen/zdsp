using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zealot.Common;
using Zealot.Repository;
using Kopio.JsonContracts;

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

    private ActivatedClueData mClueData;

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
                Message.text = clueJson.message;
                ClueIcon.gameObject.SetActive(false);
            }
            else
            {
                Message.gameObject.SetActive(false);
                ClueIcon.gameObject.SetActive(true);
                ClientUtils.LoadIconAsync(clueJson.filepath, UpdateClueIcon);
            }
            ClockIcon.SetActive(false);
            TimeText.text = mClueData.ActivatedTime;
            NewClue.SetActive(mClueData.Status == (byte)ClueStatus.New ? true : false);
        }
    }

    private void InitDialogueClue(HeroDialogueClueJson dialogueClueJson)
    {
        ClientUtils.LoadIconAsync(dialogueClueJson.avatarpath, UpdateHeroIcon);
        Message.gameObject.SetActive(true);
        Message.text = dialogueClueJson.text;
        ClueIcon.gameObject.SetActive(false);
        ClockIcon.SetActive(false);
        TimeText.text = mClueData.ActivatedTime;
        NewClue.SetActive(mClueData.Status == (byte)ClueStatus.New ? true : false);
    }

    private void InitTimeClue(TimeClueJson timeClueJson)
    {
        ClientUtils.LoadIconAsync(timeClueJson.avatarpath, UpdateHeroIcon);
        if (timeClueJson.category == ClueCategory.Word)
        {
            Message.gameObject.SetActive(true);
            Message.text = timeClueJson.message;
            ClueIcon.gameObject.SetActive(false);
        }
        else
        {
            Message.gameObject.SetActive(false);
            ClueIcon.gameObject.SetActive(true);
            ClientUtils.LoadIconAsync(timeClueJson.filepath, UpdateClueIcon);
        }
        ClockIcon.SetActive(true);
        TimeText.text = mClueData.ActivatedTime;
        NewClue.SetActive(mClueData.Status == (byte)ClueStatus.New ? true : false);
    }

    private void UpdateHeroIcon(Sprite sprite)
    {
        HeroIcon.sprite = sprite;
    }

    private void UpdateClueIcon(Sprite sprite)
    {
        ClueIcon.sprite = sprite;
    }

    public void OnClickClueIcon()
    {
        ClueType type = (ClueType)mClueData.ClueType;
        //open other ui
    }
}
