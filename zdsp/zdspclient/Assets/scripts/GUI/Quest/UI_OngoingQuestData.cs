using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Candlelight.UI;
using Zealot.Common;
using Kopio.JsonContracts;
using Zealot.Repository;
using System.Collections.Generic;

public class UI_OngoingQuestData : MonoBehaviour
{
    [SerializeField]
    Image Type;

    [SerializeField]
    Toggle QuestListToggle;

    [SerializeField]
    Toggle SelectedToggle;

    [SerializeField]
    Text QuestName;

    [SerializeField]
    HyperText Description;

    [SerializeField]
    Text Level;

    [SerializeField]
    Text MapName;

    [SerializeField]
    Text Experience;

    [SerializeField]
    Text JobExperience;

    [SerializeField]
    UI_QuestReward Rewards;

    private UI_OngoingQuest mParent;
    private QuestClientController mController;
    private int mQuestId;
    private Dictionary<int, long> mMOEndTime;
    private Dictionary<int, long> mSOEndTime;
    private string mDescription;

    public void Init(CurrentQuestData questData, QuestClientController controller, bool tracked, UI_OngoingQuest parent, bool maxtrack, ToggleGroup group)
    {
        mParent = parent;
        mController = controller;
        mQuestId = questData.QuestId;
        QuestJson questJson = QuestRepo.GetQuestByID(questData.QuestId);
        QuestName.text = questJson.questname;
        QuestListToggle.isOn = tracked;
        if (!tracked && maxtrack)
        {
            QuestListToggle.gameObject.SetActive(false);
        }
        SelectedToggle.group = group;
        mMOEndTime = controller.GetMainObjectiveEndtime(questData);
        mSOEndTime = controller.GetSubObjectiveEndtime(questData);
        mDescription = controller.DeserializedDescription(questData);
        if (mMOEndTime.Count > 0 || mSOEndTime.Count > 0)
        {
            UpdateDescrption();
        }
        else
        {
            Description.text = mDescription;
        }
        Level.text = questJson.minlv.ToString();
        MapName.text = questJson.subname;
        Experience.text = "0";
        JobExperience.text = "0";

        int rewardgroup = QuestRepo.GetQuestReward(questData.QuestId, questData.GroupdId);
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

    public void UpdateDescription(CurrentQuestData questData)
    {
        mMOEndTime = mController.GetMainObjectiveEndtime(questData);
        mSOEndTime = mController.GetSubObjectiveEndtime(questData);
        mDescription = mController.DeserializedDescription(questData);
        if (mMOEndTime.Count > 0 || mSOEndTime.Count > 0)
        {
            UpdateDescrption();
        }
        else
        {
            Description.text = mDescription;
        }
    }

    public void UpdateTrackStatus(bool active)
    {
        QuestListToggle.gameObject.SetActive(active);
    }
    
    public void OnShowToggle()
    {
        mParent.UpdateTrackingList(mQuestId, QuestListToggle.isOn);
    }

    public void OnSelected()
    {
        mParent.OnQuestSelectionChanged(SelectedToggle.isOn ? mQuestId : -1);
    }

    public void Deselect()
    {
        SelectedToggle.isOn = false;
    }

    private IEnumerator EndTmeCD()
    {
        yield return new WaitForSecondsRealtime(1);
        foreach(KeyValuePair<int, long> entry in mMOEndTime)
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
        Description.text = mController.ReplaceEndTime(mDescription, mMOEndTime, mSOEndTime);
        StartCoroutine(EndTmeCD());
    }
}
