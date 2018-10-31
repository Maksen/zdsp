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
    Text Type;

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

    [SerializeField]
    GameObject RewardDescription;

    [SerializeField]
    GameObject RewardList;

    [SerializeField]
    Button Complete;

    private UI_OngoingQuest mParent;
    private QuestClientController mController;
    private int mQuestId;
    private QuestType mQuestType;
    private string mQuestMap;
    private Dictionary<int, long> mMOEndTime;
    private Dictionary<int, long> mSOEndTime;
    private string mDescription;
    private bool bIsUnlockQuest = false;

    public void Init(CurrentQuestData questData, QuestClientController controller, bool tracked, UI_OngoingQuest parent, bool maxtrack, ToggleGroup group)
    {
        mParent = parent;
        mController = controller;
        mQuestId = questData.QuestId;
        GetComponent<Image>().color = new Color(106.0f / 255.0f, 119f / 255.0f, 122f / 255.0f, 100f / 255.0f);
        bIsUnlockQuest = false;
        QuestJson questJson = QuestRepo.GetQuestByID(questData.QuestId);
        mQuestType = questJson.type;
        mQuestMap = questJson.subname;
        QuestName.text = questJson.questname;
        Type.text = GetTypeName(mQuestType);
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
        Description.raycastTarget = true;
        Description.ClickedLink.RemoveAllListeners();
        Description.ClickedLink.AddListener(OnClickHyperlink);
        Level.text = questJson.minlv.ToString();
        MapName.text = questJson.subname;
        Experience.text = "0";
        JobExperience.text = "0";
        Complete.gameObject.SetActive(questData.Status == (byte)QuestStatus.CompletedAllObjective ? true : false);

        int rewardgroup = QuestRepo.GetQuestReward(questData.QuestId, questData.GroupdId);
        int jobsect = GameInfo.gLocalPlayer == null ? -1 : GameInfo.gLocalPlayer.PlayerSynStats.jobsect;
        Reward reward = RewardListRepo.GetRewardByGrpIDJobID(rewardgroup, jobsect);
        if (reward != null)
        {
            RewardDescription.SetActive(false);
            int exp = reward.Exp(GameInfo.gLocalPlayer.PlayerSynStats.Level);
            if (exp > 0)
            {
                Experience.text = exp.ToString();
            }
            Rewards.Init(reward);
        }
    }

    public void Init(QuestJson questJson, QuestClientController controller, bool tracked, UI_OngoingQuest parent, bool maxtrack, ToggleGroup group)
    {
        mParent = parent;
        mController = controller;
        mQuestId = questJson.questid;
        mQuestType = questJson.type;
        mQuestMap = questJson.subname;
        GetComponent<Image>().color = new Color(250f / 255.0f, 191f / 255.0f, 143f / 255.0f, 100f / 255.0f);
        bIsUnlockQuest = true;
        QuestName.text = questJson.questname;
        Type.text = GetTypeName(mQuestType);
        QuestListToggle.isOn = tracked;
        if (!tracked && maxtrack)
        {
            QuestListToggle.gameObject.SetActive(false);
        }
        SelectedToggle.group = group;
        mDescription = controller.GetStartQuestDescription(questJson);
        Description.text = mDescription;
        Description.raycastTarget = true;
        Description.ClickedLink.RemoveAllListeners();
        Description.ClickedLink.AddListener(OnClickHyperlink);
        Level.text = questJson.minlv.ToString();
        MapName.text = questJson.subname;
        Experience.text = "0";
        JobExperience.text = "0";
        Complete.gameObject.SetActive(false);

        if (QuestRepo.MultiQuestRewardGroup(questJson.questid))
        {
            RewardDescription.SetActive(true);
            RewardList.SetActive(false);
            RewardDescription.GetComponent<Text>().text = GUILocalizationRepo.GetLocalizedString("quest_multireward");
        }
        else
        {
            RewardDescription.SetActive(false);
            RewardList.SetActive(true);
            int rewardgroup = QuestRepo.GetQuestReward(questJson.questid, 0);
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
        mParent.OnQuestSelectionChanged(SelectedToggle.isOn ? mQuestId : -1, bIsUnlockQuest);
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

    public void OnClickHyperlink(HyperText hyperText, HyperText.LinkInfo linkInfo)
    {
        mController.ProcessObjectiveHyperLink(linkInfo.Name, mQuestId);
    }

    public bool IsSameType(UIQuestType questType)
    {
        switch (mQuestType)
        {
            case QuestType.Main:
                return questType == UIQuestType.Main ? true : false;
            case QuestType.Destiny:
                return questType == UIQuestType.Destiny ? true : false;
            case QuestType.Sub:
                return questType == UIQuestType.Sub ? true : false;
            case QuestType.Guild:
                return questType == UIQuestType.Guild ? true : false;
            case QuestType.Event:
                return questType == UIQuestType.Event ? true : false;
            case QuestType.Signboard:
                return questType == UIQuestType.Signboard ? true : false;
        }
        return false;
    }

    public string GetMapName()
    {
        return mQuestMap;
    }

    public bool IsSameMap(string map)
    {
        if (map == GUILocalizationRepo.GetLocalizedString("inv_all"))
        {
            return true;
        }
        else
        {
            return mQuestMap == map ? true : false;
        }
    }

    public void OnClickComplete()
    {
        UIManager.StartHourglass();
        RPCFactory.NonCombatRPC.CompleteQuest(mQuestId, true);
    }

    private string GetTypeName(QuestType type)
    {
        switch (type)
        {
            case QuestType.Main:
                return GUILocalizationRepo.GetLocalizedString("quest_main");
            case QuestType.Destiny:
                return GUILocalizationRepo.GetLocalizedString("quest_adventure");
            case QuestType.Sub:
                return GUILocalizationRepo.GetLocalizedString("quest_sub");
            case QuestType.Guild:
                return GUILocalizationRepo.GetLocalizedString("quest_guild");
            case QuestType.Event:
                return GUILocalizationRepo.GetLocalizedString("quest_event");
            case QuestType.Signboard:
                return GUILocalizationRepo.GetLocalizedString("quest_signboard");
        }
        return "";
    }
}
