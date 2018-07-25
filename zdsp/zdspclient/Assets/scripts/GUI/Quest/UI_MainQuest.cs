using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Kopio.JsonContracts;
using Zealot.Repository;
using System.Collections.Generic;
using System.Collections;
using Candlelight.UI;

public class UI_MainQuest : MonoBehaviour
{
    [SerializeField]
    Image Background;

    [SerializeField]
    Text ChapterName;

    [SerializeField]
    Text ChapterProgress;

    [SerializeField]
    Text QuestName;

    [SerializeField]
    HyperText ObjectiveDescription;

    [SerializeField]
    Text Experience;

    [SerializeField]
    Text JobExperience;

    [SerializeField]
    UI_QuestReward Rewards;

    [SerializeField]
    GameObject RewardDescription;

    [SerializeField]
    GameObject RewardList;

    [SerializeField]
    UI_QuestList QuestList;

    [SerializeField]
    UI_ChapterList ChapterList;

    [SerializeField]
    Toggle QuestDetails;

    [SerializeField]
    Toggle ChapterDetails;

    private QuestClientController mQuestController;
    private CurrentQuestData mQuestData;
    private Dictionary<int, long> mMOEndTime;
    private Dictionary<int, long> mSOEndTime;
    private string mDescription;

    private void OnEnable()
    {
        if (GameInfo.gLocalPlayer == null)
        {
            return;
        }
        
        mQuestController = GameInfo.gLocalPlayer.QuestController;
        mQuestData = mQuestController.GetQuestData(QuestType.Main);
        QuestList.SelectedQuestId = mQuestData.QuestId;
        QuestList.SelectedChapterId = QuestRepo.GetChapterByQuestId(mQuestData.QuestId).groupid;
        UpdateMainQuest();
    }

    private void UpdateMainQuest()
    {
        QuestJson questJson = QuestRepo.GetQuestByID(mQuestData.QuestId);
        ChapterJson chapterJson = QuestRepo.GetChapterByQuestId(mQuestData.QuestId);

        ClientUtils.LoadIconAsync(chapterJson.background, UpdateBackground);
        ChapterName.text = chapterJson.name;
        ChapterProgress.text = QuestRepo.GetChapterProgress(chapterJson.groupid, mQuestData.QuestId);
        QuestName.text = questJson.questname;
        mMOEndTime = mQuestController.GetMainObjectiveEndtime(mQuestData);
        mSOEndTime = mQuestController.GetSubObjectiveEndtime(mQuestData);
        mDescription = mQuestController.DeserializedDescription(mQuestData);
        if (mMOEndTime.Count > 0 || mSOEndTime.Count > 0)
        {
            UpdateDescrption();
        }
        else
        {
            ObjectiveDescription.text = mDescription;
        }
        ObjectiveDescription.ClickedLink.RemoveAllListeners();
        ObjectiveDescription.ClickedLink.AddListener(OnClickHyperlink);
        Experience.text = "0";
        JobExperience.text = "0";
        
        if (QuestRepo.MultiQuestRewardGroup(mQuestData.QuestId))
        {
            RewardDescription.SetActive(true);
            RewardList.SetActive(false);
            RewardDescription.GetComponent<Text>().text = GUILocalizationRepo.GetLocalizedString("quest_multireward");
        }
        else
        {
            RewardDescription.SetActive(false);
            RewardList.SetActive(true);
            int rewardgroup = QuestRepo.GetQuestReward(mQuestData.QuestId, 0);
            Reward reward = RewardListRepo.GetRewardByGrpIDJobID(rewardgroup, -1);
            if (reward != null)
            {
                int exp = reward.Exp(GameInfo.gLocalPlayer.PlayerSynStats.Level);
                if (exp > 0)
                {
                    Experience.text = exp.ToString();
                }
                Rewards.Init(reward);
            }
        }
    }

    private void UpdateBackground(Sprite sprite)
    {
        Background.sprite = sprite;
    }

    public void OnQuestDetailClicked()
    {
        if (QuestDetails.isOn)
        {
            QuestList.Init(mQuestData, mQuestController);
        }
    }

    public void OnChapterDetailClicked()
    {
        if (ChapterDetails.isOn)
        {
            ChapterList.Init(mQuestData);
        }
    }

    private IEnumerator EndTmeCD()
    {
        yield return new WaitForSecondsRealtime(1);
        foreach (KeyValuePair<int, long> entry in mMOEndTime)
        {
            mMOEndTime[entry.Key] -= 1000;
        }
        foreach (KeyValuePair<int, long> entry in mSOEndTime)
        {
            mSOEndTime[entry.Key] -= 1000;
        }
        UpdateDescrption();
    }

    private void UpdateDescrption()
    {
        ObjectiveDescription.text = mQuestController.ReplaceEndTime(mDescription, mMOEndTime, mSOEndTime);
        StartCoroutine(EndTmeCD());
    }

    public void OnClickHyperlink(HyperText hyperText, HyperText.LinkInfo linkInfo)
    {
        mQuestController.ProcessObjectiveHyperLink(linkInfo.Name, mQuestData.QuestId);
    }
}