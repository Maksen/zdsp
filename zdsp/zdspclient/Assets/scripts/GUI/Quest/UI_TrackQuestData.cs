using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Candlelight.UI;
using Zealot.Common;
using System.Collections.Generic;
using Zealot.Repository;
using Kopio.JsonContracts;

public class UI_TrackQuestData : MonoBehaviour
{
    [SerializeField]
    Text Type;

    [SerializeField]
    Text Name;

    [SerializeField]
    Button ObjectiveButton;

    [SerializeField]
    HyperText Description;

    [SerializeField]
    GameObject DoingQuest;

    [SerializeField]
    GameObject CompletedQuest;

    private string mDescription;
    private string mLocation;
    private Dictionary<int, long> mMOEndTime;
    private Dictionary<int, long> mSOEndTime;
    private QuestClientController mQuestController;
    private int mQuestId;
    private bool bUnlockQuest = false;
    private CurrentQuestData mQuestData;

    public void UpdateQuestData(CurrentQuestData questData, QuestClientController questController)
    {
        mQuestData = questData;
        mQuestId = questData.QuestId;
        bUnlockQuest = false;
        QuestJson questJson = QuestRepo.GetQuestByID(questData.QuestId);
        if (questJson != null)
        {
            Type.text = GetTypeName((QuestType)questData.QuestType);
            Name.text = questJson.questname;
            mQuestController = questController;
            mMOEndTime = questController.GetMainObjectiveEndtime(questData);
            mSOEndTime = questController.GetSubObjectiveEndtime(questData);
            mDescription = questController.DeserializedDescription(questData);
            mLocation = "<size=5>\n</size>地點 ：" + questJson.subname;
            if (mMOEndTime.Count > 0 || mSOEndTime.Count > 0)
            {
                UpdateDescrption();
            }
            else
            {
                Description.text = mDescription + mLocation;
            }
            Description.ClickedLink.RemoveAllListeners();
            if (questController.HaveMultipleTarget(questData))
            {
                Description.ClickedLink.AddListener(OnClickHyperlink);
                ObjectiveButton.interactable = false;
            }
            else
            {
                ObjectiveButton.interactable = true;
            }
            DoingQuest.SetActive(false);
            bool submitable = questController.IsQuestCanSubmit(questData.QuestId);
            CompletedQuest.SetActive(submitable);
        }
    }

    public void UpdateQuestData(QuestJson questJson, QuestClientController questController)
    {
        mQuestId = questJson.questid;
        bUnlockQuest = true;
        Type.text = GetTypeName(questJson.type);
        Name.text = questJson.questname;
        mQuestController = questController;
        mDescription = questController.GetStartQuestDescription(questJson);
        mLocation = "<size=5>\n</size>地點 ：" + questJson.subname;
        Description.text = mDescription + mLocation;
        Description.ClickedLink.RemoveAllListeners();
        ObjectiveButton.interactable = true;
        DoingQuest.SetActive(false);
        CompletedQuest.SetActive(false);
    }

    private void UpdateDescrption()
    {
        Description.text = mQuestController.ReplaceEndTime(mDescription, mMOEndTime, mSOEndTime) + mLocation;
        StartCoroutine(EndTmeCD());
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

    private string GetTypeName(QuestType type)
    {
        switch(type)
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

    public void OnClickHyperlink(HyperText hyperText, HyperText.LinkInfo linkInfo)
    {
        mQuestController.ProcessObjectiveHyperLink(linkInfo.Name, mQuestId);
    }

    public void OnClickObjectiveButton()
    {
        if (bUnlockQuest)
        {
            int targetid;
            QuestTriggerType type;
            if (mQuestController.GetQuestTriggerTarget(mQuestId, out targetid, out type))
            {
                mQuestController.ProceedQuestTrigger(type, targetid, mQuestId);
            }
        }
        else
        {
            int targetid;
            QuestObjectiveType type;
            if (mQuestController.GetObjectiveTarget(mQuestData, out targetid, out type))
            {
                mQuestController.ProceedQuestObjective(type, targetid, mQuestId);
            }
        }
    }
}
