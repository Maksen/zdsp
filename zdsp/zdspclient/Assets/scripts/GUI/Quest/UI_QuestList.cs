using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Repository;
using System;
using UnityEngine.UI;
using Zealot.Common;

public class UI_QuestList : MonoBehaviour
{
    [SerializeField]
    Transform QuestList;

    [SerializeField]
    GameObject QuestDetail;

    [SerializeField]
    Transform ObjectiveList;

    [SerializeField]
    GameObject ObjectiveDetail;

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

    public int SelectedChapterId { get; set; }
    public int SelectedQuestId { get; set; }

    private List<GameObject> mQuestList = new List<GameObject>();
    private List<GameObject> mObjectiveList = new List<GameObject>();
    private CurrentQuestData mQuestData;
    private QuestClientController mQuestController;

    public void Init(CurrentQuestData questData, QuestClientController questController)
    {
        mQuestData = questData;
        mQuestController = questController;
        UpdateQuestList();
        UpdateObjectiveList();
        UpdateQuestReward();
    }

    private void UpdateQuestList()
    {
        QuestListClear();
        if (SelectedChapterId <= 0)
        {
            return;
        }
        List<ChapterJson> list = QuestRepo.GetQuestsInChapter(SelectedChapterId);
        foreach(ChapterJson chapter in list)
        {
            int questid = chapter.questid;
            QuestJson questJson = QuestRepo.GetQuestByID(questid);
            GameObject questdetail = Instantiate(QuestDetail);
            questdetail.GetComponent<UI_QuestListData>().Init(questJson.questname);
            questdetail.transform.SetParent(QuestList, false);
            questdetail.GetComponent<Toggle>().group = QuestList.GetComponent<ToggleGroup>();
            questdetail.GetComponent<Toggle>().isOn = questid == SelectedQuestId ? true : false;
            questdetail.GetComponent<Toggle>().onValueChanged.AddListener(delegate { OnSelectQuestData(questid); });
            mQuestList.Add(questdetail);
        }
    }

    private void QuestListClear()
    {
        foreach(GameObject obj in mQuestList)
        {
            GameObject.Destroy(obj);
        }
        mQuestList = new List<GameObject>();
    }

    private void OnSelectQuestData(int questid)
    {
        SelectedQuestId = questid;
        UpdateObjectiveList();
        UpdateQuestReward();
    }

    private void UpdateObjectiveList()
    {
        ObjectiveListClear();
        Dictionary<int, Dictionary<int, List<string>>> objectivelist = QuestRepo.GetQuestObjectiveByQuestId(SelectedQuestId);
        if (objectivelist == null)
        {
            return;
        }
        foreach(KeyValuePair<int, Dictionary<int, List<string>>> entry in objectivelist)
        {
            int group = entry.Key;
            foreach(KeyValuePair<int, List<string>> objective in entry.Value)
            {
                int seq = objective.Key;
                foreach (string id in objective.Value)
                {
                    int objectiveid = int.Parse(id);
                    QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
                    if (objectiveJson.type == QuestObjectiveType.Empty)
                    {
                        continue;
                    }
                    if (objectiveJson.type != QuestObjectiveType.MultipleObj)
                    {
                        bool completed = mQuestController.IsMainQuestObjectiveCompleted(SelectedQuestId, group, seq);
                        bool ongoing = mQuestData == null ? false : mQuestData.QuestId == SelectedQuestId &&  mQuestData.GroupdId == group && mQuestData.MainObjective.SequenceNum == seq ? true : false;
                        string description = mQuestController.DeserializedDescription(QuestType.Main, SelectedQuestId, objectiveid, completed, ongoing);
                        GameObject objectivedetail = Instantiate(ObjectiveDetail);
                        objectivedetail.GetComponent<UI_ObjectiveListData>().Init(description, ongoing, completed, mQuestController, SelectedQuestId);
                        objectivedetail.transform.SetParent(ObjectiveList, false);
                        mObjectiveList.Add(objectivedetail);
                    }
                    else
                    {
                        Dictionary<int, List<int>> subobjectivelist = QuestRepo.GetSubObjective(objectiveid);
                        foreach(KeyValuePair<int, List<int>> subobjective in subobjectivelist)
                        {
                            bool completed = mQuestController.IsMainQuestObjectiveCompleted(SelectedQuestId, group, seq, SelectedQuestId, subobjective.Key);
                            bool ongoing = mQuestData == null ? false : mQuestData.GroupdId == group && mQuestData.MainObjective.SequenceNum == seq && mQuestData.SubObjective[SelectedQuestId].SequenceNum == subobjective.Key ? true : false;
                            string description = mQuestController.DeserializedDescription(QuestType.Main, SelectedQuestId, objectiveid, subobjective.Value, completed, ongoing);
                            GameObject objectivedetail = Instantiate(ObjectiveDetail);
                            objectivedetail.GetComponent<UI_ObjectiveListData>().Init(description, ongoing, completed, mQuestController, SelectedQuestId);
                            objectivedetail.transform.SetParent(ObjectiveList, false);
                            mObjectiveList.Add(objectivedetail);
                        }
                    }
                }
            }
        }
    }

    private void ObjectiveListClear()
    {
        foreach (GameObject obj in mObjectiveList)
        {
            GameObject.Destroy(obj);
        }
        mObjectiveList = new List<GameObject>();
    }

    private void UpdateQuestReward()
    {
        if (SelectedQuestId <= 0)
        {
            return;
        }
        Experience.text = "0";
        JobExperience.text = "0";
        if (QuestRepo.MultiQuestRewardGroup(SelectedQuestId))
        {
            RewardDescription.SetActive(true);
            RewardList.SetActive(false);
            RewardDescription.GetComponent<Text>().text = GUILocalizationRepo.GetLocalizedString("quest_multiplereward");
        }
        else
        {
            RewardDescription.SetActive(false);
            RewardList.SetActive(true);
            int rewardgroup = QuestRepo.GetQuestReward(SelectedQuestId, 0);
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
}
