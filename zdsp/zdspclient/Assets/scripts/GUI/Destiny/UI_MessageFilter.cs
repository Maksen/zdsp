using Kopio.JsonContracts;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_MessageFilter : MonoBehaviour
{
    [SerializeField]
    GameObject PhotoObj;

    [SerializeField]
    GameObject VideoObj;

    [SerializeField]
    GameObject AudioObj;

    [SerializeField]
    GameObject WordObj;

    [SerializeField]
    UI_PhotoClueMessage PhotoMessage;

    [SerializeField]
    UI_VideoClueMessage VideoMessage;

    [SerializeField]
    UI_AudioClueMessage AudioMessage;

    [SerializeField]
    UI_WordClueMessage WordMessage;

    [SerializeField]
    Text Title;

    [SerializeField]
    Text Objective;

    [SerializeField]
    Text Date;

    [SerializeField]
    GameObject ClockObj;

    [SerializeField]
    AudioSource AudioPlayer;

    private ActivatedClueData mClueData;
    private DateTime mCountDownTime;

    public void Init(ActivatedClueData clueData)
    {
        mClueData = clueData;
        if (mClueData.ClueType == (byte)ClueType.Normal)
        {
            InitNormalClue();
        }
        else
        {
            InitTimeClue();
        }
    }

    private void InitNormalClue()
    {
        DestinyClueJson clueJson = DestinyClueRepo.GetDestinyClueById(mClueData.ClueId);
        if (clueJson != null)
        {
            SetClueCategory(clueJson.category);
            UpdateClueFilePath(clueJson.category, clueJson.filepath);
            UpdateClueMessage(clueJson.category, clueJson.message);
            UpdateTitle(clueJson.questid);
            Date.text = mClueData.ActivatedDate;
            ClockObj.SetActive(false);
            Objective.text = clueJson.task;
        }
    }

    private void InitTimeClue()
    {
        TimeClueJson clueJson = DestinyClueRepo.GetTimeClueById(mClueData.ClueId);
        if (clueJson != null)
        {
            SetClueCategory(clueJson.category);
            UpdateClueFilePath(clueJson.category, clueJson.filepath);
            UpdateClueMessage(clueJson.category, clueJson.message);
            UpdateTitle(clueJson.questid);
            mCountDownTime = mClueData.ActivatedDT.AddMinutes(clueJson.time);
            UpdateTime();
            ClockObj.SetActive(true);
            Objective.text = clueJson.task;
        }
    }

    private void UpdateTime()
    {
        double remaintime = (mCountDownTime - DateTime.Now).TotalSeconds;
        if (remaintime > 0)
        {
            Date.text = GUILocalizationRepo.GetLocalizedTimeString((int)remaintime, 2);
            StartCoroutine(TimeCountDown());
        }
    }

    private IEnumerator TimeCountDown()
    {
        yield return new WaitForSecondsRealtime(1);
        UpdateTime();
    }

    private void SetClueCategory(ClueCategory category)
    {
        switch(category)
        {
            case ClueCategory.Photo:
                PhotoObj.SetActive(true);
                VideoObj.SetActive(false);
                AudioObj.SetActive(false);
                WordObj.SetActive(false);
                break;
            case ClueCategory.Sound:
                PhotoObj.SetActive(false);
                VideoObj.SetActive(false);
                AudioObj.SetActive(true);
                WordObj.SetActive(false);
                break;
            case ClueCategory.Video:
                PhotoObj.SetActive(false);
                VideoObj.SetActive(true);
                AudioObj.SetActive(false);
                WordObj.SetActive(false);
                break;
            case ClueCategory.Word:
                PhotoObj.SetActive(false);
                VideoObj.SetActive(false);
                AudioObj.SetActive(false);
                WordObj.SetActive(true);
                break;
        }
    }

    private void UpdateClueFilePath(ClueCategory category, string filepath)
    {
        switch (category)
        {
            case ClueCategory.Photo:
                PhotoMessage.Init(filepath);
                break;
            case ClueCategory.Sound:
                AudioMessage.Init(filepath, AudioPlayer);
                break;
            case ClueCategory.Video:
                VideoMessage.Init(filepath, AudioPlayer);
                break;
        }
    }

    private void UpdateClueMessage(ClueCategory category, string message)
    {
        if (category == ClueCategory.Word)
        {
            WordMessage.Init(message);
        }
    }

    private void UpdateTitle(int questid)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            Title.text = questJson.questname;
        }
        else
        {
            Title.text = "";
        }
    }

    public void OnClickClose()
    {
        mClueData = null;
        CleanClue();
        UIManager.CloseDialog(WindowType.DialogMessageFilter);
    }

    private void CleanClue()
    {
        PhotoMessage.Clean();
        AudioMessage.Clean();
        VideoMessage.Clean();
    }
}
