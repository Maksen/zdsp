using UnityEngine;
using Zealot.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Zealot.Repository;
using System;
using Kopio.JsonContracts;

public enum UIQuestType
{
    All,
    Main,
    Destiny,
    Sub,
    Guild,
    Signboard,
    Event
}

public class UI_OngoingQuest : MonoBehaviour
{
    [SerializeField]
    GameObject OngoingQuestData;

    [SerializeField]
    Transform QuestListContent;

    [SerializeField]
    Transform[] OngoingGroupContent;

    [SerializeField]
    Transform[] UnlockGroupContent;

    [SerializeField]
    Scrollbar ContentScrollBar;

    [SerializeField]
    Button Reset;

    [SerializeField]
    Button Delete;

    [SerializeField]
    ComboBoxA TypeComboBox;

    [SerializeField]
    ComboBoxA MapComboBox;

    private QuestClientController mQuestController;
    private Dictionary<QuestType, Dictionary<int, CurrentQuestData>> mQuestList;
    private Dictionary<int, GameObject> mOngoingQuestList;
    private Dictionary<QuestType, List<int>> mUnlockQuest;
    private List<int> mTrackingList;
    private int mSelectedQuest;
    private List<int> mTypeList;
    private UIQuestType mSelectedQuestType;
    private List<string> mMapName;
    private string mSelectedMap;

    private void OnEnable()
    {
        if (GameInfo.gLocalPlayer == null)
        {
            return;
        }
        
        mSelectedQuestType = UIQuestType.All;
        mSelectedMap = GUILocalizationRepo.GetLocalizedString("inv_all");

        if (mQuestController == null)
        {
            mQuestController = GameInfo.gLocalPlayer.QuestController;
            
            mQuestList = new Dictionary<QuestType, Dictionary<int, CurrentQuestData>>();
            mQuestList.Add(QuestType.Main, mQuestController.GetQuestDataList(QuestType.Main));
            mQuestList.Add(QuestType.Destiny, mQuestController.GetQuestDataList(QuestType.Destiny));
            mQuestList.Add(QuestType.Sub, mQuestController.GetQuestDataList(QuestType.Sub));
            mQuestList.Add(QuestType.Guild, mQuestController.GetQuestDataList(QuestType.Guild));
            mQuestList.Add(QuestType.Signboard, mQuestController.GetQuestDataList(QuestType.Signboard));
            mQuestList.Add(QuestType.Event, mQuestController.GetQuestDataList(QuestType.Event));

            mUnlockQuest = new Dictionary<QuestType, List<int>>();
            mUnlockQuest.Add(QuestType.Main, new List<int>());
            mUnlockQuest.Add(QuestType.Destiny, new List<int>());
            mUnlockQuest.Add(QuestType.Sub, new List<int>());
            mUnlockQuest.Add(QuestType.Guild, new List<int>());
            mUnlockQuest.Add(QuestType.Signboard, new List<int>());
            mUnlockQuest.Add(QuestType.Event, new List<int>());

            mOngoingQuestList = new Dictionary<int, GameObject>();
            mTrackingList = mQuestController.GetTrackingList();
            mSelectedQuest = -1;
            UpdateOngoingList();
            CheckForQuestType();
        }
        else
        {
            mTrackingList = mQuestController.GetTrackingList();
            mSelectedQuest = -1;
            UpdateOngoingList();
            CheckForQuestType();
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
            QuestType questType = (QuestType)data.QuestType;
            if (mQuestList[questType].ContainsKey(questid))
            {
                mQuestList[questType][questid] = data;
                UpdateQuestData(questid, data);
            }
            else
            {
                mQuestList[questType].Add(questid, data);
                AddNewOngoingQuest(questid, data);
            }
        }
    }

    public void UpdateUnlockQuestList()
    {
        UIManager.StartHourglass();
        foreach (QuestType type in Enum.GetValues(typeof(QuestType)))
        {
            AddAvailableQuest(type);
            DeleteAvailableQuest(type);
        }
        UIManager.StopHourglass();
    }

    private void DeleteAvailableQuest(QuestType type)
    {
        List<int> newquestlist = mQuestController.GetUnlockQuest(type).Select(o => o.questid).ToList();
        List<int> oldquestlist = mUnlockQuest[type];
        foreach (int questid in oldquestlist)
        {
            if (!newquestlist.Contains(questid))
            {
                if (mOngoingQuestList.ContainsKey(questid))
                {
                    Destroy(mOngoingQuestList[questid]);
                    mOngoingQuestList.Remove(questid);
                }
                mUnlockQuest[type].Remove(questid);
            }
        }
    }

    private void CheckForQuestType()
    {
        TypeComboBox.ClearItemList();
        mTypeList = new List<int>();

        foreach (UIQuestType type in Enum.GetValues(typeof(UIQuestType)))
        {
            string name = GetTypeName(type);
            int typenum = (int)type;
            if (type == UIQuestType.All)
            {
                TypeComboBox.AddItem(name, typenum.ToString());
                mTypeList.Add(typenum);
            }
            else
            {
                QuestType questType = (QuestType)(typenum - 1);
                Transform ongoingcontent = GetOngoingGroup(questType);
                Transform unlockcontent = GetUnlockGroup(questType);
                if (ongoingcontent.gameObject.activeSelf || unlockcontent.gameObject.activeSelf)
                {
                    TypeComboBox.AddItem(name, typenum.ToString());
                    mTypeList.Add(typenum);
                }
            }
        }
        TypeComboBox.SelectedIndex = 0;
        UpdateQuestDataByQyestType();
    }

    public void OnQuestTypeChanged(int index)
    {
        if (mTypeList.Count >index)
        {
            mSelectedQuestType = (UIQuestType)mTypeList[index];
        }

        UpdateQuestDataByQyestType();
    }

    private void UpdateQuestDataByQyestType()
    {
        if (mSelectedQuestType == UIQuestType.All)
        {
            foreach(KeyValuePair<int, GameObject> entry in mOngoingQuestList)
            {
                entry.Value.SetActive(true);
            }
        }
        else
        {
            foreach (KeyValuePair<int, GameObject> entry in mOngoingQuestList)
            {
                UI_OngoingQuestData ongoingQuestData = entry.Value.GetComponent<UI_OngoingQuestData>();
                if (ongoingQuestData.IsSameType(mSelectedQuestType))
                {
                    entry.Value.SetActive(true);
                }
                else
                {
                    entry.Value.SetActive(false);
                }
            }
        }

        CheckForQuestMap();
    }

    private string GetTypeName(UIQuestType type)
    {
        switch (type)
        {
            case UIQuestType.All:
                return GUILocalizationRepo.GetLocalizedString("inv_all");
            case UIQuestType.Main:
                return GUILocalizationRepo.GetLocalizedString("quest_main");
            case UIQuestType.Destiny:
                return GUILocalizationRepo.GetLocalizedString("quest_adventure");
            case UIQuestType.Sub:
                return GUILocalizationRepo.GetLocalizedString("quest_sub");
            case UIQuestType.Guild:
                return GUILocalizationRepo.GetLocalizedString("quest_guild");
            case UIQuestType.Event:
                return GUILocalizationRepo.GetLocalizedString("quest_event");
            case UIQuestType.Signboard:
                return GUILocalizationRepo.GetLocalizedString("quest_signboard");
        }
        return "";
    }

    private void UpdateQuestData(int questid, CurrentQuestData questData)
    {
        if (mOngoingQuestList.ContainsKey(questid))
        {
            UI_OngoingQuestData ongoingQuestData = mOngoingQuestList[questid].GetComponent<UI_OngoingQuestData>();
            ongoingQuestData.UpdateDescription(questData);
        }
    }

    private void AddNewOngoingQuest(int questid, CurrentQuestData questData)
    {
        Transform content = GetOngoingGroup((QuestType)questData.QuestType);
        content.gameObject.SetActive(true);

        if (mOngoingQuestList.ContainsKey(questid))
        {
            UI_OngoingQuestData ongoingQuestData = mOngoingQuestList[questid].GetComponent<UI_OngoingQuestData>();
            ongoingQuestData.transform.SetParent(content, false);
            ongoingQuestData.UpdateDescription(questData);
        }
        else
        {
            GameObject ongoingdata = Instantiate(OngoingQuestData);
            bool tracked = mTrackingList == null ? false : mTrackingList.Contains(questid);
            ToggleGroup toggleGroup = QuestListContent.GetComponent<ToggleGroup>();
            ongoingdata.GetComponent<UI_OngoingQuestData>().Init(questData, mQuestController, tracked, this, mTrackingList.Count >= 20, toggleGroup);
            ongoingdata.transform.SetParent(content, false);
            mOngoingQuestList.Add(questid, ongoingdata);
        }
    }

    private Transform GetOngoingGroup(QuestType type)
    {
        return OngoingGroupContent[(int)type];
    }

    private Transform GetUnlockGroup(QuestType type)
    {
        return UnlockGroupContent[(int)type];
    }

    private void UpdateOngoingList()
    {
        UIManager.StartHourglass();
        foreach (QuestType type in Enum.GetValues(typeof(QuestType)))
        {
            AddOngoingQuest(mQuestList[type], type);
            AddAvailableQuest(type);
            DeleteAvailableQuest(type);
        }
        UIManager.StopHourglass();
    }

    private void AddOngoingQuest(Dictionary<int, CurrentQuestData> questdatas, QuestType type)
    {
        Transform content = GetOngoingGroup(type);
        content.gameObject.SetActive(questdatas.Count == 0 ? false : true);
        foreach (KeyValuePair<int, CurrentQuestData> questdata in questdatas)
        {
            if (questdata.Value == null)
            {
                break;
            }
            if (!mOngoingQuestList.ContainsKey(questdata.Key))
            {
                GameObject ongoingdata = Instantiate(OngoingQuestData);
                bool tracked = mTrackingList == null ? false : mTrackingList.Contains(questdata.Key);
                ToggleGroup toggleGroup = QuestListContent.GetComponent<ToggleGroup>();
                ongoingdata.GetComponent<UI_OngoingQuestData>().Init(questdata.Value, mQuestController, tracked, this, mTrackingList.Count >= 20, toggleGroup);
                ongoingdata.transform.SetParent(content, false);
                mOngoingQuestList.Add(questdata.Key, ongoingdata);
            }
        }
    }

    private void AddAvailableQuest(QuestType type)
    {
        Transform content = GetUnlockGroup(type);
        List<QuestJson> questlist = mQuestController.GetUnlockQuest(type);
        content.gameObject.SetActive(questlist.Count == 0 ? false : true);
        foreach (QuestJson questJson in questlist)
        {
            if (!mOngoingQuestList.ContainsKey(questJson.questid))
            {
                GameObject ongoingdata = Instantiate(OngoingQuestData);
                bool tracked = mTrackingList == null ? false : mTrackingList.Contains(questJson.questid);
                ToggleGroup toggleGroup = QuestListContent.GetComponent<ToggleGroup>();
                ongoingdata.GetComponent<UI_OngoingQuestData>().Init(questJson, mQuestController, tracked, this, mTrackingList.Count >= 20, toggleGroup);
                ongoingdata.transform.SetParent(content, false);
                mOngoingQuestList.Add(questJson.questid, ongoingdata);

                if (!mUnlockQuest[type].Contains(questJson.questid))
                {
                    mUnlockQuest[type].Add(questJson.questid);
                }
            }
        }
    }

    private void CheckForQuestMap()
    {
        mMapName = new List<string>();
        mMapName.Add(GUILocalizationRepo.GetLocalizedString("inv_all"));
        foreach (KeyValuePair<int, GameObject> entry in mOngoingQuestList)
        {
            UI_OngoingQuestData ongoingQuestData = entry.Value.GetComponent<UI_OngoingQuestData>();
            if (ongoingQuestData.gameObject.activeSelf)
            {
                string mapname = ongoingQuestData.GetMapName();
                if (!mMapName.Contains(mapname))
                {
                    mMapName.Add(mapname);
                }
            }
        }

        MapComboBox.ClearItemList();
        foreach (string mapname in mMapName)
        {
            MapComboBox.AddItem(mapname, mapname);
        }
        
        if (mMapName.Contains(mSelectedMap))
        {
            MapComboBox.SelectedIndex = MapComboBox.GetIndexByValue(mSelectedMap);
        }
        else
        {
            MapComboBox.SelectedIndex = 0;
            mSelectedMap = GUILocalizationRepo.GetLocalizedString("inv_all");
        }
        UpdateQuestDataByMap();
    }

    public void OnMapNameChanged(int index)
    {
        if (mMapName.Count > index)
        {
            mSelectedMap = mMapName[index];
        }

        UpdateQuestDataByMap();
    }

    private void UpdateQuestDataByMap()
    {
        if (mSelectedQuestType == UIQuestType.All)
        {
            foreach (KeyValuePair<int, GameObject> entry in mOngoingQuestList)
            {
                UI_OngoingQuestData ongoingQuestData = entry.Value.GetComponent<UI_OngoingQuestData>();
                if (ongoingQuestData.IsSameMap(mSelectedMap))
                {
                    entry.Value.SetActive(true);
                }
                else
                {
                    entry.Value.SetActive(false);
                }
            }
        }
        else
        {
            foreach (KeyValuePair<int, GameObject> entry in mOngoingQuestList)
            {
                UI_OngoingQuestData ongoingQuestData = entry.Value.GetComponent<UI_OngoingQuestData>();
                if (ongoingQuestData.IsSameType(mSelectedQuestType) && ongoingQuestData.IsSameMap(mSelectedMap))
                {
                    entry.Value.SetActive(true);
                }
                else
                {
                    entry.Value.SetActive(false);
                }
            }
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

    public void OnQuestSelectionChanged(int questid, bool unlockquest)
    {
        mSelectedQuest = questid;
        UpdateQuestButton(unlockquest ? false : mSelectedQuest == -1 ? false : true);
    }

    public void OnClickedReset()
    {
        UIManager.OpenYesNoDialog(GUILocalizationRepo.GetLocalizedString("quest_reset"), delegate { OnConfirmResetQuest(mSelectedQuest); }, delegate { OnCancelAction(); });
    }

    public void OnClickedDelete()
    {
        UIManager.OpenYesNoDialog(GUILocalizationRepo.GetLocalizedString("quest_delete"), delegate { OnDeleteDeleteQuest(mSelectedQuest); }, delegate { OnCancelAction(); });
    }

    private void OnConfirmResetQuest(int questid)
    {
        UIManager.StartHourglass();
        RPCFactory.NonCombatRPC.ResetQuest(questid);
    }

    private void OnDeleteDeleteQuest(int questid)
    {
        UIManager.StartHourglass();
        RPCFactory.NonCombatRPC.DeleteQuest(questid, JsonConvertDefaultSetting.SerializeObject(mTrackingList));
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
            QuestType questType = QuestRepo.GetQuestTypeByQuestId(questid);
            if (mQuestList[questType].ContainsKey(questid))
            {
                mQuestList[questType].Remove(questid);
            }
            if (mTrackingList.Contains(questid))
            {
                mTrackingList.Remove(questid);
            }
            mQuestController.RemoveQuest(questid);
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
