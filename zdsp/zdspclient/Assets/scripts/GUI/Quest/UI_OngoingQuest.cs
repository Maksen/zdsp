using UnityEngine;
using Zealot.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_OngoingQuest : MonoBehaviour
{
    [SerializeField]
    GameObject OngoingQuestData;

    [SerializeField]
    Transform QuestListContent;

    [SerializeField]
    Scrollbar ContentScrollBar;

    [SerializeField]
    Button Reset;

    [SerializeField]
    Button Delete;

    private QuestClientController mQuestController;
    private Dictionary<int, CurrentQuestData> mQuestList;
    private Dictionary<int, GameObject> mOngoingQuestList;
    private bool mIsLoading = false;
    private List<int> mTrackingList;
    private int mSelectedQuest;

    private void OnEnable()
    {
        if (GameInfo.gLocalPlayer == null)
        {
            return;
        }

        if (mQuestController == null)
        {
            mQuestController = GameInfo.gLocalPlayer.QuestController;
            mQuestList = new Dictionary<int, CurrentQuestData>();
            mQuestList = mQuestController.GetQuestDataList(QuestType.Main);
            mQuestList = mQuestList.Concat(mQuestController.GetQuestDataList(QuestType.Destiny)).ToDictionary(o => o.Key, o => o.Value);
            mQuestList = mQuestList.Concat(mQuestController.GetQuestDataList(QuestType.Sub)).ToDictionary(o => o.Key, o => o.Value);
            mQuestList = mQuestList.Concat(mQuestController.GetQuestDataList(QuestType.Guild)).ToDictionary(o => o.Key, o => o.Value);
            mQuestList = mQuestList.Concat(mQuestController.GetQuestDataList(QuestType.Signboard)).ToDictionary(o => o.Key, o => o.Value);
            mQuestList = mQuestList.Concat(mQuestController.GetQuestDataList(QuestType.Event)).ToDictionary(o => o.Key, o => o.Value);
            mOngoingQuestList = new Dictionary<int, GameObject>();
            mTrackingList = mQuestController.GetTrackingList();
            mSelectedQuest = -1;
            UpdateQuestList();
        }
        else
        {
            mTrackingList = mQuestController.GetTrackingList();
            mSelectedQuest = -1;
            UpdateQuestList();
        }
    }

    public void UpdateQuestData(CurrentQuestData questData, CurrentQuestData oldquestData)
    {
        int questid = -1;
        CurrentQuestData data = null;
        if (questData == null && oldquestData != null)
        {
            questid = oldquestData.QuestId;
            data = oldquestData;
        }
        else if (questData != null)
        {
            questid = questData.QuestId;
            data = questData;
        }

        if (questid != -1)
        {
            if (mQuestList.ContainsKey(questid))
            {
                mQuestList[questid] = data;
            }
            else
            {
                mQuestList.Add(questid, data);
            }
            RefreshQuestList();
        }
    }

    private void RefreshQuestList()
    {
        UIManager.StartHourglass();
        mIsLoading = true;
        foreach(KeyValuePair<int, GameObject> entry in mOngoingQuestList)
        {
            CurrentQuestData questData = null;
            mQuestList.TryGetValue(entry.Key, out questData);
            if (questData != null)
            {
                entry.Value.GetComponent<UI_OngoingQuestData>().UpdateDescription(questData);
            }
        }
        mIsLoading = false;
        UIManager.StopHourglass();
    }

    private void UpdateQuestList()
    {
        UIManager.StartHourglass();
        mIsLoading = true;
        for (int i = 0; i < QuestRepo.MaxTrackingCount; i++)
        {
            foreach(KeyValuePair<int, CurrentQuestData> questdata in mQuestList)
            {
                if (questdata.Value == null)
                {
                    break;
                }
                if (!mOngoingQuestList.ContainsKey(questdata.Key))
                {
                    GameObject ongoingdata = Instantiate(OngoingQuestData);
                    bool tracked;
                    if (mTrackingList == null)
                    {
                        tracked = false;
                    }
                    else
                    {
                        tracked = mTrackingList.Contains(questdata.Key) ? true : false;
                    }
                    ToggleGroup toggleGroup = QuestListContent.GetComponent<ToggleGroup>();
                    ongoingdata.GetComponent<UI_OngoingQuestData>().Init(questdata.Value, mQuestController, tracked, this, mTrackingList.Count >= 20, toggleGroup);
                    ongoingdata.transform.SetParent(QuestListContent, false);
                    mOngoingQuestList.Add(questdata.Key, ongoingdata);
                }
            }
        }
        mIsLoading = false;
        UIManager.StopHourglass();
    }

    private void Update()
    {
        if (!mIsLoading && ContentScrollBar.gameObject.activeSelf && ContentScrollBar.value == 0)
        {
            UpdateQuestList();
            Debug.Log("Ongoing Quest List Refresh");
        }
    }

    private void ClearQuestList()
    {
        foreach(KeyValuePair<int, GameObject> obj in mOngoingQuestList)
        {
            Destroy(obj.Value);
        }
        mOngoingQuestList = new Dictionary<int, GameObject>();
    }

    public void ClearQngoingQuest()
    {
        if (mOngoingQuestList != null)
        {
            ClearQuestList();
        }
        mQuestController = null;
        mOngoingQuestList = null;
        mQuestList = null;
        mTrackingList = null;
    }

    public void UpdateTrackingList(int questid, bool ison)
    {
        mQuestController.TrackingListUpdate = true;
        if (mTrackingList.Contains(questid) && !ison)
        {
            mTrackingList.Remove(questid);
        }
        else if (!mTrackingList.Contains(questid) && ison)
        {
            if (mTrackingList.Count < QuestRepo.MaxTrackingCount)
            {
                mTrackingList.Add(questid);
            }
        }

        UpdateTrackingStatus();
    }

    private void UpdateTrackingStatus()
    {
        foreach(KeyValuePair<int, GameObject>entry in mOngoingQuestList)
        {
            entry.Value.GetComponent<UI_OngoingQuestData>().UpdateTrackStatus(mTrackingList.Count >= 20 ? false : true);
        }
    }

    private void OnDisable()
    {
        mQuestController.UpdateTrackingList();
    }

    public void OnQuestSelectionChanged(int questid)
    {
        mSelectedQuest = questid;
        UpdateQuestButton(mSelectedQuest == -1 ? false : true);
    }

    public void OnClickedReset()
    {
        UIManager.OpenYesNoDialog(GUILocalizationRepo.GetLocalizedSysMsgByName("quest_reset"), delegate { OnConfirmResetQuest(mSelectedQuest); }, delegate { OnCancelAction(); });
    }

    public void OnClickedDelete()
    {
        UIManager.OpenYesNoDialog(GUILocalizationRepo.GetLocalizedSysMsgByName("quest_delete"), delegate { OnDeleteResetQuest(mSelectedQuest); }, delegate { OnCancelAction(); });
    }

    private void OnConfirmResetQuest(int questid)
    {
        UIManager.StartHourglass();
        RPCFactory.NonCombatRPC.ResetQuest(questid);
    }

    private void OnDeleteResetQuest(int questid)
    {
        UIManager.StartHourglass();
        RPCFactory.NonCombatRPC.DeleteQuest(questid);
    }

    private void OnCancelAction()
    {
        RemoveSelectedQuest(mSelectedQuest);
        mSelectedQuest = -1;
    }

    private void RemoveSelectedQuest(int questid)
    {
        if (mOngoingQuestList.ContainsKey(questid))
        {
            mOngoingQuestList[mSelectedQuest].GetComponent<UI_OngoingQuestData>().Deselect();
        }
       
        UpdateQuestButton(false);
    }

    public void OnDeleteQuestReturn(bool result, int questid)
    {
        UIManager.StopHourglass();
        string msg = result ? GUILocalizationRepo.GetLocalizedSysMsgByName("quest_deletesuccess") : GUILocalizationRepo.GetLocalizedSysMsgByName("quest_deletefailed");
        UIManager.ShowSystemMessage(msg);
        RemoveSelectedQuest(questid);
        if(result)
        {
            if (mOngoingQuestList.ContainsKey(questid))
            {
                Destroy(mOngoingQuestList[questid]);
                mOngoingQuestList.Remove(questid);
            }
            if (mQuestList.ContainsKey(questid))
            {
                mQuestList.Remove(questid);
            }
            mQuestController.RollBackQuestEvent(questid);
        }
    }

    public void OnResetQuestReturn(bool result, int questid)
    {
        UIManager.StopHourglass();
        string msg = result ? GUILocalizationRepo.GetLocalizedSysMsgByName("quest_resetsuccess") : GUILocalizationRepo.GetLocalizedSysMsgByName("quest_resetfailed");
        UIManager.ShowSystemMessage(msg);
        RemoveSelectedQuest(questid);
        mQuestController.RollBackQuestEvent(questid);
    }

    private void UpdateQuestButton(bool status)
    {
        Reset.interactable = status;
        Delete.interactable = status;
    }
}
