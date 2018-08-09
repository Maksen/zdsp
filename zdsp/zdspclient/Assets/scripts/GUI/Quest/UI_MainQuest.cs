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
    private QuestJson mQuestJson;
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
        if (mQuestData != null)
        {
            mQuestJson = QuestRepo.GetQuestByID(mQuestData.QuestId);
        }
        else
        {
            mQuestJson = mQuestController.GetUnlockMainQuest();
        }

        if (mQuestJson != null)
        {
            QuestList.SelectedQuestId = mQuestJson.questid;
            QuestList.SelectedChapterId = QuestRepo.GetChapterByQuestId(mQuestJson.questid).groupid;
            UpdateMainQuest();
        }
    }

    private void UpdateMainQuest()
    {
        bool unlockquest = mQuestData == null ? true : false;

        ChapterJson chapterJson = QuestRepo.GetChapterByQuestId(mQuestJson.questid);

        ClientUtils.LoadIconAsync(chapterJson.background, UpdateBackground);
        ChapterName.text = chapterJson.name;
        ChapterProgress.text = QuestRepo.GetChapterProgress(chapterJson.groupid, mQuestJson.questid);
        QuestName.text = mQuestJson.questname;
        if (unlockquest)
        {
            mDescription = mQuestController.GetStartQuestDescription(mQuestJson);
            ObjectiveDescription.text = mDescription;
        }
        else
        {
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
            ObjectiveDescription.raycastTarget = true;
            ObjectiveDescription.ClickedLink.RemoveAllListeners();
            ObjectiveDescription.ClickedLink.AddListener(OnClickHyperlink);
        }
        Experience.text = "0";
        JobExperience.text = "0";

        if (QuestRepo.MultiQuestRewardGroup(mQuestJson.questid))
        {
            RewardDescription.SetActive(true);
            RewardList.SetActive(false);
            RewardDescription.GetComponent<Text>().text = GUILocalizationRepo.GetLocalizedString("quest_multireward");
        }
        else
        {
            RewardDescription.SetActive(false);
            RewardList.SetActive(true);
            int rewardgroup = QuestRepo.GetQuestReward(mQuestJson.questid, unlockquest ? 0 : mQuestData.GroupdId);
            int jobsect = GameInfo.gLocalPlayer == null ? -1 : GameInfo.gLocalPlayer.PlayerSynStats.jobsect;
            Reward reward = RewardListRepo.GetRewardByGrpIDJobID(rewardgroup, jobsect);
            if (reward != null)
            {
                int level = GameInfo.gLocalPlayer == null ? 1 : GameInfo.gLocalPlayer.PlayerSynStats.Level;
                int joblevel = GameInfo.gLocalPlayer == null ? 1 : GameInfo.gLocalPlayer.PlayerSynStats.progressJobLevel;
                int exp = reward.Exp(level);
                Experience.text = exp > 0 ? exp.ToString() : "0";
                int jobexp = reward.Jxp(joblevel);
                JobExperience.text = jobexp > 0 ? jobexp.ToString() : "0";
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
            ChapterList.Init(mQuestJson, QuestList, this);
        }
    }

    public void OnChapterChanged()
    {
        if (ChapterDetails.isOn)
        {
            ChapterDetails.isOn = false;
            QuestDetails.isOn = true;
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