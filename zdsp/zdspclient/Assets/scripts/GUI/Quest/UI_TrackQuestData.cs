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
    HyperText Description;

    [SerializeField]
    GameObject DoingQuest;

    [SerializeField]
    GameObject CompletedQuest;

    private string mDescription;
    private Dictionary<int, long> mMOEndTime;
    private Dictionary<int, long> mSOEndTime;
    private QuestClientController mQuestController;

    public void UpdateQuestData(CurrentQuestData questData, QuestClientController questController)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questData.QuestId);
        if (questJson != null)
        {
            Type.text = GetTypeName((QuestType)questData.QuestType);
            Name.text = questJson.questname;
            mQuestController = questController;
            mMOEndTime = questController.GetMainObjectiveEndtime(questData);
            mSOEndTime = questController.GetSubObjectiveEndtime(questData);
            mDescription = questController.DeserializedDescription(questData);
            if (mMOEndTime.Count > 0 || mSOEndTime.Count > 0)
            {
                UpdateDescrption();
            }
            else
            {
                Description.text = mDescription;
            }
            DoingQuest.SetActive(false);
            bool submitable = questController.IsQuestCanSubmit(questData.QuestId);
            CompletedQuest.SetActive(submitable);
        }
    }

    private void UpdateDescrption()
    {
        Description.text = mQuestController.ReplaceEndTime(mDescription, mMOEndTime, mSOEndTime);
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
}
