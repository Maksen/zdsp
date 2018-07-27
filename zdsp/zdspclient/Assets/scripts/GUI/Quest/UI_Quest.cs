using UnityEngine;
using Zealot.Common;

public class UI_Quest : MonoBehaviour
{
    [SerializeField]
    UI_MainQuest MainQuest;

    [SerializeField]
    UI_DestinyQuest DestinyQuest;

    [SerializeField]
    UI_OngoingQuest OngoingQuest;

    [SerializeField]
    UI_CutScene CutScene;

    private bool mIsActived = false;

    public void OnEnable()
    {
        mIsActived = true;
    }

    private void OnDisable()
    {
        mIsActived = false;
        OngoingQuest.ClearQngoingQuest();
    }

    public void OnDeleteQuest(bool result, int questid)
    {
        if (!mIsActived)
            return;

        OngoingQuest.OnDeleteQuestReturn(result, questid);
    }

    public void OnResetQuest(bool result, int questid)
    {
        if (!mIsActived)
            return;

        OngoingQuest.OnResetQuestReturn(result, questid);
    }

    public void UpdateQuestData(CurrentQuestData questData, CurrentQuestData oldquestData)
    {
        if (!mIsActived)
            return;

        if (OngoingQuest.gameObject.activeSelf)
        {
            OngoingQuest.UpdateQuestData(questData, oldquestData);
        }
    }

    public void UpdateOngoingQuestData()
    {
        if (!mIsActived)
            return;

        if (OngoingQuest.gameObject.activeSelf)
        {
            OngoingQuest.UpdateUnlockQuestList();
        }
    }
}
