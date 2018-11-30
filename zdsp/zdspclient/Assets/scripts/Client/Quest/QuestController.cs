using Kopio.JsonContracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zealot.Bot;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Common.RPC;
using Zealot.Entities;
using Zealot.Repository;

public class QuestSynStatsClient : QuestSynStats
{
    
}

public class QuestTeleportAction
{
    public byte ActionType;
    public int TargetId;
    public int QuestId;
    public bool IsObjective;

    public QuestTeleportAction(byte type, int targetid, int questid, bool isobjective)
    {
        ActionType = type;
        TargetId = targetid;
        QuestId = questid;
        IsObjective = isobjective;
    }
}

public class QuestClientController
{
    //Main Quest
    private CurrentQuestData mMainQuest;
    private List<int> mCompletedMainQuest;

    //Adventure Quest
    private Dictionary<int, CurrentQuestData> mAdventureQuest;
    private List<int> mCompletedAdventureQuest;
    private List<int> mLockedAdventureQuest;

    //Subline Quest
    private Dictionary<int, CurrentQuestData> mSublineQuest;
    private List<int> mCompletedSublineQuest;

    //Guild Quest
    private Dictionary<int, CurrentQuestData> mGuildQuest;
    private List<int> mCompletedGuildQuest;

    //Signboard Quest
    private Dictionary<int, CurrentQuestData> mSignboardQuest;
    private List<int> mCompletedSignboardQuest;
    private int mSignboardRewardBoost;
    private int mSignboardLimit;

    //Event Quest
    private Dictionary<int, CurrentQuestData> mEventQuest;
    private List<int> mCompletedEventQuest;

    //Tracking List
    private List<int> mTrackingList;
    private List<int> mTrackingList_UI;
    public bool TrackingListUpdate { get; set; }

    //Wonderful List
    private List<int> mWonderfulList;

    //Quest Unlock List
    private List<int> mUnlockQuestList;

    //Signboard Unlock List
    private List<int> mUnlockSignboardList;

    private PlayerGhost mPlayer;
    private Dictionary<QuestType, Dictionary<int, int>> mQuestIdByIndex;
    private bool bInit = false;
    private List<StaticClientNPCAlwaysShow> mTriggerArea;
    private StaticClientNPCAlwaysShow mActivedTrigger;
    private HUD_QuestList mQuestListHUD;
    private QuestTriggerController mQuestTriggerController;
    private QuestGuideController mQuestGuideController;
    private QuestRequirementController mQuestRequirementController;
    private QuestDialogueController mQuestDialogueController;

    public QuestClientController(PlayerGhost player)
    {
        mCompletedMainQuest = new List<int>();

        mAdventureQuest = new Dictionary<int, CurrentQuestData>();
        mCompletedAdventureQuest = new List<int>();
        mLockedAdventureQuest = new List<int>();

        mSublineQuest = new Dictionary<int, CurrentQuestData>();
        mCompletedSublineQuest = new List<int>();

        mGuildQuest = new Dictionary<int, CurrentQuestData>();
        mCompletedGuildQuest = new List<int>();

        mSignboardQuest = new Dictionary<int, CurrentQuestData>();
        mCompletedSignboardQuest = new List<int>();
        mSignboardRewardBoost = 100;
        mSignboardLimit = 0;

        mEventQuest = new Dictionary<int, CurrentQuestData>();
        mCompletedEventQuest = new List<int>();

        mTrackingList = new List<int>();
        mWonderfulList = new List<int>();
        mUnlockQuestList = new List<int>();
        mUnlockSignboardList = new List<int>();

        mPlayer = player;
        TrackingListUpdate = false;

        mQuestIdByIndex = new Dictionary<QuestType, Dictionary<int, int>>();
        foreach (QuestType type in Enum.GetValues(typeof(QuestType)))
        {
            mQuestIdByIndex.Add(type, new Dictionary<int, int>());
        }
        mTriggerArea = new List<StaticClientNPCAlwaysShow>();
        mActivedTrigger = null;
    }

    public void Init(ClientEntitySystem entitySystem)
    {
        bInit = true;
        UpdateNpcDisplayStatus();
        mQuestRequirementController = new QuestRequirementController(this);
        mQuestTriggerController = new QuestTriggerController(this, mQuestRequirementController, entitySystem);
        mQuestGuideController = new QuestGuideController(this, entitySystem);
        mQuestDialogueController = new QuestDialogueController(this);
        OnLevelLoaded();
    }

    public void UpdateValue(string field, int value)
    {
        switch (field)
        {
            case "signboardRewardBoost":
                mSignboardRewardBoost = value;
                UpdateDailyQuestUI();
                break;
            case "signboardLimit":
                mSignboardLimit = value;
                UpdateDailyQuestUI();
                break;
        }
    }

    public void DeserializeData(string field, string data, string olddata)
    {
        if (string.IsNullOrEmpty(data))
            return;

        List<int> oldcompleteddata = new List<int>();
        switch (field)
        {
            case "mainQuest":
                CurrentQuestData questData = DeserializeQuestData(data);
                UpdateQuestData(questData, mMainQuest, QuestType.Main, -1);
                break;
            case "completedMain":
                oldcompleteddata = mCompletedMainQuest.CloneJson();
                mCompletedMainQuest = DeserializeCompletedQuest(data);
                CompletedListUpdated(mCompletedMainQuest, oldcompleteddata);
                break;
            case "completedAdventure":
                oldcompleteddata = mCompletedAdventureQuest.CloneJson();
                mCompletedAdventureQuest = DeserializeCompletedQuest(data);
                CompletedListUpdated(mCompletedAdventureQuest, oldcompleteddata);
                break;
            case "completedSubline":
                oldcompleteddata = mCompletedSublineQuest.CloneJson();
                mCompletedSublineQuest = DeserializeCompletedQuest(data);
                CompletedListUpdated(mCompletedSublineQuest, oldcompleteddata);
                break;
            case "completedGuild":
                mCompletedGuildQuest = DeserializeCompletedQuest(data);
                break;
            case "completedSignboard":
                mCompletedSignboardQuest = DeserializeCompletedQuest(data);
                UpdateDailyQuestUI();
                break;
            case "completedEvent":
                oldcompleteddata = mCompletedEventQuest.CloneJson();
                mCompletedEventQuest = DeserializeCompletedQuest(data);
                CompletedListUpdated(mCompletedEventQuest, oldcompleteddata);
                break;
            case "trackingList":
                mTrackingList_UI = mTrackingList = DeserializeCompletedQuest(data);
                mQuestListHUD.UpdateTrackingList(mTrackingList, this);
                break;
            case "wonderfulList":
                mWonderfulList = DeserializeCompletedQuest(data);
                break;
            case "unlockQuestList":
                mUnlockQuestList = DeserializeCompletedQuest(data);
                UpdateUI();
                break;
            case "unlockSignboardList":
                mUnlockSignboardList = DeserializeCompletedQuest(data);
                UpdateDailyQuestUI();
                break;
            case "lockedAdventure":
                mLockedAdventureQuest = DeserializeCompletedQuest(data);
                break;
        }
    }

    public void DeserializeCollectionData(string field, string data, int index)
    {
        CurrentQuestData questdata = DeserializeQuestData(data);
        switch (field)
        {
            case "AdventureQuest":
                UpdateQuestData(questdata, GetOldQuestData(QuestType.Destiny, index), QuestType.Destiny, index);
                break;
            case "SublineQuest":
                UpdateQuestData(questdata, GetOldQuestData(QuestType.Sub, index), QuestType.Sub, index);
                break;
            case "GuildQuest":
                UpdateQuestData(questdata, GetOldQuestData(QuestType.Guild, index), QuestType.Guild, index);
                break;
            case "SignboardQuest":
                UpdateQuestData(questdata, GetOldQuestData(QuestType.Signboard, index), QuestType.Signboard, index);
                UpdateDailyQuestUI();
                break;
            case "EventQuest":
                UpdateQuestData(questdata, GetOldQuestData(QuestType.Event, index), QuestType.Event, index);
                break;
        }
    }

    private CurrentQuestData DeserializeQuestData(string data)
    {
        if (!string.IsNullOrEmpty(data) && data != "{}")
        {
            QuestDataStats questDataStats = JsonConvertDefaultSetting.DeserializeObject<QuestDataStats>(data);
            return new CurrentQuestData(questDataStats);
        }
        return null;
    }

    private List<int> DeserializeCompletedQuest(string data)
    {
        List<int> result = JsonConvertDefaultSetting.DeserializeObject<List<int>>(data);
        if (result == null)
        {
            return new List<int>();
        }
        return result;
    }

    public void QuestCompleted(int questid, bool result)
    {
        if (result)
        {
            QuestType type = QuestRepo.GetQuestTypeByQuestId(questid);
            switch(type)
            {
                case QuestType.Main:
                    mMainQuest = null;
                    break;
                case QuestType.Destiny:
                    if (mAdventureQuest.ContainsKey(questid))
                    {
                        mAdventureQuest.Remove(questid);
                    }
                    break;
                case QuestType.Sub:
                    if (mSublineQuest.ContainsKey(questid))
                    {
                        mSublineQuest.Remove(questid);
                    }
                    break;
                case QuestType.Guild:
                    if (mGuildQuest.ContainsKey(questid))
                    {
                        mGuildQuest.Remove(questid);
                    }
                    break;
                case QuestType.Signboard:
                    if (mSignboardQuest.ContainsKey(questid))
                    {
                        mSignboardQuest.Remove(questid);
                    }
                    break;
                case QuestType.Event:
                    if (mEventQuest.ContainsKey(questid))
                    {
                        mEventQuest.Remove(questid);
                    }
                    break;
            }
        }

        RemoveQuest(questid);

        Dictionary<string, string> param = new Dictionary<string, string>();
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        param.Add("questname", questJson == null ? "" : questJson.questname);
        string systemmsg = GUILocalizationRepo.GetLocalizedSysMsgByName(result ? "sys_QuestComplete_Success" : "sys_QuestComplete_Failed", param);
        UIManager.ShowSystemMessage(systemmsg, true);
    }

    public void QuestTriggered(int questid, bool result)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("questname", questJson.questname);
            string message = result ? GUILocalizationRepo.GetLocalizedSysMsgByName("sys_QuestTrigger_Success", param) : GUILocalizationRepo.GetLocalizedSysMsgByName("sys_QuestTrigger_Failed", param);
            UIManager.ShowSystemMessage(message, true);

            if (questJson.type == QuestType.Destiny && !UIManager.IsWidgetActived(HUDWidgetType.QuestStart))
            {
                UIManager.SetWidgetActive(HUDWidgetType.QuestStart, true);
                UIManager.GetWidget(HUDWidgetType.QuestStart).GetComponent<Hud_QuestStart>().Init(questJson);
            }
        }
    }

    private void UpdateQuestData(CurrentQuestData newdata, CurrentQuestData olddata, QuestType type, int index)
    {
        switch(type)
        {
            case QuestType.Main:
                mMainQuest = newdata;
                break;
            case QuestType.Destiny:
                UpdateQuestData(mAdventureQuest, newdata, type, index);
                CheckNewDestinyQuest(newdata, olddata);
                break;
            case QuestType.Sub:
                UpdateQuestData(mSublineQuest, newdata, type, index);
                break;
            case QuestType.Guild:
                UpdateQuestData(mGuildQuest, newdata, type, index);
                break;
            case QuestType.Signboard:
                UpdateQuestData(mSignboardQuest, newdata, type, index);
                break;
            case QuestType.Event:
                UpdateQuestData(mEventQuest, newdata, type, index);
                break;
        }
        
        if (mQuestListHUD == null)
        {
            mQuestListHUD = UIManager.GetWidget(HUDWidgetType.QuestList).GetComponent<HUD_QuestList>();
        }
        mQuestListHUD.UpdateQuestData(newdata, olddata, this);
        UpdateUI(newdata, olddata);
        TriggerQuestEvent(newdata);
        UpdateTriggerQuest(newdata, olddata);
        DisplayKillObjectiveMsg(newdata, olddata);
    }

    private void UpdateQuestData(Dictionary<int, CurrentQuestData> datalist, CurrentQuestData data, QuestType type, int index)
    {
        int questid = data == null ? GetQuestIdFromIndexMap(type, index) : data.QuestId;
        if (datalist.ContainsKey(questid))
        {
            datalist[questid] = data;
            if (data == null)
            {
                datalist.Remove(questid);
            }
        }
        else
        {
            if (data != null)
            {
                datalist.Add(data.QuestId, data);
            }
        }

        if (data == null)
        {
            if (mQuestIdByIndex[type].ContainsKey(index))
            {
                mQuestIdByIndex[type].Remove(index);
            }
        }
        else
        {
            if (!mQuestIdByIndex[type].ContainsKey(index))
            {
                mQuestIdByIndex[type].Add(index, questid);
            }
        }
    }

    private int GetQuestIdFromIndexMap(QuestType type, int index)
    {
        if (mQuestIdByIndex[type].ContainsKey(index))
        {
            return mQuestIdByIndex[type][index];
        }
        return -1;
    }

    private CurrentQuestData GetOldQuestData(QuestType type, int index)
    {
        int questid = GetQuestIdFromIndexMap(type, index);
        return GetQuestData(type, questid);
    }    

    private void UpdateUI(CurrentQuestData questData, CurrentQuestData oldquestData)
    {
        UI_Quest quest = UIManager.GetWindowGameObject(WindowType.Quest).GetComponent<UI_Quest>();
        if (quest != null)
        {
            quest.UpdateQuestData(questData, oldquestData);
        }
    }

    private void UpdateUI()
    {
        UI_Quest quest = UIManager.GetWindowGameObject(WindowType.Quest).GetComponent<UI_Quest>();
        if (quest != null)
        {
            quest.UpdateOngoingQuestData();
        }
    }

    public CurrentQuestData GetQuestData(QuestType type)
    {
        switch (type)
        {
            case QuestType.Main:
                return mMainQuest;
        }
        return null;
    }

    public Dictionary<int, CurrentQuestData> GetQuestDataList(QuestType type)
    {
        switch (type)
        {
            case QuestType.Main:
                return mMainQuest == null ? new Dictionary<int, CurrentQuestData>() : new Dictionary<int, CurrentQuestData>() { { mMainQuest.QuestId, mMainQuest } };
            case QuestType.Destiny:
                return mAdventureQuest;
            case QuestType.Sub:
                return mSublineQuest;
            case QuestType.Guild:
                return mGuildQuest;
            case QuestType.Signboard:
                return mSignboardQuest;
            case QuestType.Event:
                return mEventQuest;
        }
        return null;
    }

    public CurrentQuestData GetQuestData(QuestType type, int questid)
    {
        Dictionary<int, CurrentQuestData> quests = GetQuestDataList(type);
        if (quests.ContainsKey(questid))
        {
            return quests[questid];
        }
        return null;
    }

    private bool IsQuestOngoing(int questid)
    {
        QuestType type = QuestRepo.GetQuestTypeByQuestId(questid);
        return IsQuestOngoing(type, questid);
    }

    public bool IsQuestCompleted(int questid)
    {
        QuestType type = QuestRepo.GetQuestTypeByQuestId(questid);
        return IsQuestCompleted(type, questid);
    }

    private bool IsQuestCompleted(QuestType type, int questid)
    {
        switch(type)
        {
            case QuestType.Main:
                return mCompletedMainQuest.Contains(questid);
            case QuestType.Destiny:
                return mCompletedAdventureQuest.Contains(questid);
            case QuestType.Sub:
                return mCompletedSublineQuest.Contains(questid);
            case QuestType.Guild:
                return mCompletedGuildQuest.Contains(questid);
            case QuestType.Signboard:
                return mCompletedSignboardQuest.Contains(questid);
            case QuestType.Event:
                return mCompletedEventQuest.Contains(questid);
        }
        return false;
    }

    public bool IsQuestOngoing(QuestType type, int questid)
    {
        Dictionary<int, CurrentQuestData> quests = GetQuestDataList(type);
        if (quests.ContainsKey(questid))
        {
            return true;
        }
        return false;
    }

    public bool IsQuestShowable(int questid)
    {
        return IsQuestAvailable(questid, false);
    }

    public bool IsQuestAvailable(int questid, bool checkrequirement = true)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson == null)
        {
            return false;
        }

        if (questJson.type == QuestType.Guild || questJson.type == QuestType.Signboard)
        {
            return false;
        }

        if (questJson.frontquest > 0)
        {
            QuestType type = QuestRepo.GetQuestTypeByQuestId(questJson.frontquest);
            if (!IsQuestCompleted(type, questJson.frontquest))
            {
                return false;
            }
        }

        if (questJson.minlv > mPlayer.PlayerSynStats.Level)
        {
            return false;
        }

        if (IsQuestOngoing(questJson.type, questid))
        {
            return false;
        }

        if (IsQuestCompleted(questJson.type, questid))
        {
            return false;
        }

        if (checkrequirement)
        {
            List<QuestRequirementDetailJson> requirements = QuestRepo.GetRequirementByGroupId(questJson.requirementid);
            if (requirements == null)
            {
                return false;
            }
            else
            {
                foreach (QuestRequirementDetailJson requirement in requirements)
                {
                    if (!CheckRequirement(requirement))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private bool CheckRequirement(QuestRequirementDetailJson requirement)
    {
        switch (requirement.type)
        {
            case QuestRequirementType.Level:
                if (mPlayer.PlayerSynStats.Level >= requirement.para1)
                {
                    return true;
                }
                break;
            case QuestRequirementType.Item:
                if (mPlayer.clientItemInvCtrl.itemInvData.HasItem((ushort)requirement.para1, requirement.para2))
                {
                    return true;
                }
                break;
            case QuestRequirementType.Equipment:
                Equipment equipment = mPlayer.mEquipmentInvData.Slots.Where(o => o.ItemID == requirement.para1).First();
                if (equipment != null)
                {
                    if (equipment.UpgradeLevel >= requirement.para3)
                    {
                        return true;
                    }
                }
                break;
            case QuestRequirementType.Hero:
                if (requirement.para2 == 1 && mPlayer.HeroStats.IsHeroSummoned(requirement.para1))
                {
                    return true;
                }
                else if (requirement.para2 == 2 && mPlayer.HeroStats.IsHeroUnlocked(requirement.para1))
                {
                    return true;
                }
                break;
            case QuestRequirementType.Title:
                return true;
            case QuestRequirementType.SideEffect:
                if (requirement.para2 == 1 && mPlayer.HasSideEffect(requirement.para1))
                {
                    return true;
                }
                else if (requirement.para2 == 2 && !mPlayer.HasSideEffect(requirement.para1))
                {
                    return true;
                }
                break;
            case QuestRequirementType.Companian:
                if (requirement.para2 == 1 && requirement.para1 == GetCompanionId())
                {
                    return true;
                }
                else if (requirement.para2 == 2 && requirement.para1 != GetCompanionId())
                {
                    return true;
                }
                break;
            case QuestRequirementType.Clue:
                if (requirement.para2 == 1 && mPlayer.DestinyClueController.IsClueAlreadyUnlock(requirement.para1))
                {
                    return true;
                }
                else if (requirement.para2 == 2 && !mPlayer.DestinyClueController.IsClueAlreadyUnlock(requirement.para1))
                {
                    return true;
                }
                break;
            case QuestRequirementType.Job:
                if (requirement.para2 == 1 && mPlayer.PlayerSynStats.jobsect == requirement.para1)
                {
                    return true;
                }
                else if (requirement.para2 == 2 && mPlayer.PlayerSynStats.jobsect != requirement.para1)
                {
                    return true;
                }
                break;
            case QuestRequirementType.TimeClue:
                if (requirement.para2 == 1 && mPlayer.DestinyClueController.IsTimeClueAlreadyUnlock(requirement.para1))
                {
                    return true;
                }
                else if (requirement.para2 == 2 && !mPlayer.DestinyClueController.IsTimeClueAlreadyUnlock(requirement.para1))
                {
                    return true;
                }
                break;
        }
        return false;
    }

    public CurrentQuestData GetLatestQuestData(QuestType type)
    {
        Dictionary<int, CurrentQuestData> quests = GetQuestDataList(type);
        if (quests.Count > 0)
        {
            return quests.Values.Last();
        }
        return null;
    }

    public bool IsQuestCanSubmit(int questid)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null && questJson.replyid)
        {
            CurrentQuestData questData = GetQuestData(questJson.type, questid);
            if (questData != null && questData.Status == (byte)QuestStatus.CompletedAllObjective)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsObjectiveCompleted(int objectiveid, int count)
    {
        QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
        switch (objectiveJson.type)
        {
            case QuestObjectiveType.Kill:
            case QuestObjectiveType.RealmComplete:
            case QuestObjectiveType.PercentageKill:
            case QuestObjectiveType.Interact:
            case QuestObjectiveType.Choice:
                if (count >= objectiveJson.para2)
                {
                    return true;
                }
                return false;
            case QuestObjectiveType.Talk:
            case QuestObjectiveType.QuickTalk:
                if (count == 1)
                {
                    return true;
                }
                return false;
            case QuestObjectiveType.Empty:
            case QuestObjectiveType.MultipleObj:
                return true;
            default:
                return false;
        }
    }

    public bool IsMainQuestObjectiveCompleted(int questid, int groupid, int sequence)
    {
        ChapterJson selected = QuestRepo.GetChapterByQuestId(questid);
        int currentquestid = mMainQuest == null ? GetUnlockMainQuestId() : mMainQuest.QuestId;
        ChapterJson current = QuestRepo.GetChapterByQuestId(currentquestid);
        if (selected.groupid < current.groupid)
        {
            return true;
        }

        if (selected.groupid == current.groupid)
        {
            if (selected.sequence < current.sequence)
            {
                return true;
            }

            if (selected.sequence == current.sequence)
            {
                if (mMainQuest != null && groupid == mMainQuest.GroupdId)
                {
                    if (sequence < mMainQuest.MainObjective.SequenceNum)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool IsMainQuestObjectiveCompleted(int questid, int groupid, int sequence, int mainid, int subsequence)
    {
        ChapterJson selected = QuestRepo.GetChapterByQuestId(questid);
        ChapterJson current = QuestRepo.GetChapterByQuestId(mMainQuest.QuestId);
        if (selected.groupid < current.groupid)
        {
            return true;
        }

        if (selected.groupid == current.groupid)
        {
            if (selected.sequence < current.sequence)
            {
                return true;
            }

            if (selected.sequence == current.sequence)
            {
                if (groupid != mMainQuest.GroupdId)
                {
                    return true;
                }
                else if (groupid == mMainQuest.GroupdId)
                {
                    if (sequence < mMainQuest.MainObjective.SequenceNum)
                    {
                        return true;
                    }
                    else if (sequence == mMainQuest.MainObjective.SequenceNum)
                    {
                        if (subsequence < mMainQuest.SubObjective[mainid].SequenceNum)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public int GetObjectiveProgress(int questid, int objectiveid)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            return GetObjectiveProgress(questJson.type, questid, objectiveid);
        }
        return 0;
    }

    private int GetObjectiveProgress(QuestType type, int questid, int objectiveid)
    {
        CurrentQuestData questData = GetQuestData(type, questid);
        return GetObjectiveProgress(questData, objectiveid);
    }

    private int GetObjectiveProgress(CurrentQuestData questData, int objectiveid)
    {
        if (questData != null)
        {
            for (int i = 0; i < questData.MainObjective.ObjectiveIds.Count; i++)
            {
                if (questData.MainObjective.ObjectiveIds[i] == objectiveid)
                {
                    if (questData.MainObjective.ProgressCount.Count > i)
                    {
                        return questData.MainObjective.ProgressCount[i];
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            foreach(KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
            {
                for (int i = 0; i < entry.Value.ObjectiveIds.Count; i++)
                {
                    if (entry.Value.ObjectiveIds[i] == objectiveid)
                    {
                        if (entry.Value.ProgressCount.Count > i)
                        {
                            return entry.Value.ProgressCount[i];
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
        }
        return 0;
    }

    public int GetTalkId(int questid, int npcid, bool nexttalkid = false)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            if (IsQuestOngoing(questJson.type, questid))
            {
                CurrentQuestData questData = GetQuestData(questJson.type, questid);
                int talkid = GetObjectiveTalkId(questData.MainObjective, npcid, nexttalkid);
                if (talkid == -1)
                {
                    foreach(KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
                    {
                        talkid = GetObjectiveTalkId(entry.Value, npcid, nexttalkid);
                        if (talkid != -1)
                        {
                            break;
                        }
                    }
                }
                return talkid;
            }
            else
            {
                if (questJson.triggertype == QuestTriggerType.NPC && questJson.triggercaller == npcid)
                {
                    return questJson.starttalkid;
                }
            }
        }
        return -1;
    }

    private int GetObjectiveTalkId(CurrentObjectiveData objectiveData, int npcid, bool nexttalkid)
    {
        for (int i = 0; i < objectiveData.ObjectiveIds.Count; i++)
        {
            int objectiveid = objectiveData.ObjectiveIds[i];
            int progress = objectiveData.ProgressCount[i];
            if (progress > 0)
            {
                break;
            }
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
            if(objectiveJson != null)
            {
                if (objectiveJson.type == QuestObjectiveType.QuickTalk)
                {
                    return objectiveJson.para2;
                }
                else
                {
                    if (!nexttalkid && (objectiveJson.type == QuestObjectiveType.Talk || objectiveJson.type == QuestObjectiveType.Choice))
                    {
                        if (objectiveJson.para1 == npcid)
                        {
                            return objectiveJson.para2;
                        }
                    }
                }
            }
        }
        return -1;
    }

    public int GetInteractiveId(int questid, int npcid)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            CurrentQuestData questData = GetQuestData(questJson.type, questid);
            int interactiveid = GetObjectiveInteractiveId(questData.MainObjective, npcid);
            if (interactiveid == -1)
            {
                foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
                {
                    interactiveid = GetObjectiveInteractiveId(entry.Value, npcid);
                    if (interactiveid != -1)
                    {
                        break;
                    }
                }
            }
            return interactiveid;
        }
        return -1;
    }

    private int GetObjectiveInteractiveId(CurrentObjectiveData objectiveData, int npcid)
    {
        if (objectiveData.ObjectiveIds.Count == objectiveData.ProgressCount.Count)
        {
            for (int i = 0; i < objectiveData.ObjectiveIds.Count; i++)
            {
                int objectiveid = objectiveData.ObjectiveIds[i];
                int progress = objectiveData.ProgressCount[i];
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
                if (objectiveJson != null)
                {
                    if (objectiveJson.type == QuestObjectiveType.Interact && objectiveJson.para3 == npcid && progress < objectiveJson.para2)
                    {
                        return objectiveJson.para1;
                    }
                }
            }
        }
        return -1;
    }

    public bool CompletedAllQuest(List<int> questlist)
    {
        foreach(int questid in questlist)
        {
            if (!IsQuestCompleted(questid))
            {
                return false;
            }
        }
        return true;
    }

    public List<QuestJson> GetUnlockQuest(QuestType type)
    {
        List<QuestJson> questlist = new List<QuestJson>();
        foreach(int id in mUnlockQuestList)
        {
            QuestJson questJson = QuestRepo.GetQuestByID(id);
            if (questJson != null && questJson.type == type)
            {
                questlist.Add(questJson);
            }
        }
        return questlist;
    }

    #region Description
    public string DeserializedDescription(QuestType type, int questid, int objectiveid, bool completed, bool ongoing)
    {
        QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
        if (objectiveJson != null)
        {
            string description = objectiveJson.description;
            int progress = 0;
            if (completed)
            {
                progress = QuestRepo.GetObjectiveTargetCount(objectiveid);
            }
            if (ongoing)
            {
                progress = GetObjectiveProgress(type, questid, objectiveid);
            }
            description = DeserializedObjectiveDescription(description, objectiveJson, progress);
            return DeserializedRequirement(description, objectiveJson.requirementid);
        }
        return "";
    }

    public string DeserializedDescription(QuestType type, int questid, int mainid, List<int> subid, bool completed, bool ongoing)
    {
        QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(subid[0]);
        if (objectiveJson != null)
        {
            List<int> progresscount = new List<int>();
            for (int i = 0; i < subid.Count; i++)
            {
                progresscount.Add(0);
                if (completed)
                {
                    progresscount[i] = QuestRepo.GetObjectiveTargetCount(subid[i]);
                }
                if (ongoing)
                {
                    progresscount[i] = GetObjectiveProgress(type, questid, subid[i]);
                }
            }
            string description = objectiveJson.description;
            description = DeserializedObjectiveDescription(description, subid, progresscount);
            foreach (int id in subid)
            {
                objectiveJson = QuestRepo.GetQuestObjectiveByID(id);
                description = DeserializedRequirement(description, objectiveJson.requirementid);
            }
            return description;
        }
        return "";
    }

    public string DeserializedDescription(CurrentQuestData questData)
    {
        if (questData != null)
        {
            QuestStatus questStatus = (QuestStatus)questData.Status;
            if (questStatus != QuestStatus.CompletedAllObjective && questStatus != QuestStatus.CompletedWithEvent)
            {
                if (questData.MainObjective.ObjectiveIds.Count == 1)
                {
                    int mainid = questData.MainObjective.ObjectiveIds[0];
                    QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(mainid);
                    string description = "";
                    if (objectiveJson.type != QuestObjectiveType.MultipleObj)
                    {
                        description = objectiveJson.description;
                        description = DeserializedObjectiveDescription(description, objectiveJson, questData.MainObjective.ProgressCount[0]);
                        return DeserializedRequirement(description, objectiveJson.requirementid);
                    }
                    else
                    {
                        int subid = questData.SubObjective[mainid].ObjectiveIds[0];
                        QuestObjectiveJson subObjectiveJson = QuestRepo.GetQuestObjectiveByID(subid);
                        description = subObjectiveJson.description;
                        description = DeserializedObjectiveDescription(description, questData.SubObjective[mainid].ObjectiveIds, questData.SubObjective[mainid].ProgressCount);
                        foreach (int id in questData.SubObjective[mainid].ObjectiveIds)
                        {
                            subObjectiveJson = QuestRepo.GetQuestObjectiveByID(id);
                            description = DeserializedRequirement(description, subObjectiveJson.requirementid);
                        }
                        return description;
                    }
                }
            }
        }
        return "";
    }

    private string DeserializedObjectiveDescription(string description, QuestObjectiveJson objectiveJson, int progresscount)
    {
        description = DeserializedObjectiveTarget1(description, "%o[p1]%", objectiveJson.para1, objectiveJson.type);
        description = DeserializedObjectiveTarget2(description, "%o[p2]%", objectiveJson.para2, objectiveJson.type);
        description = DeserializedObjectiveTarget3(description, "%o[p3]%", objectiveJson.para3, objectiveJson.type);
        description = DeserializedProgressCount(description, "%pc%", progresscount);
        return description;
    }

    private string DeserializedObjectiveDescription(string description, List<int> objectiveid, List<int> progresscount)
    {
        Dictionary<int, QuestObjectiveJson> objectivesJson = new Dictionary<int, QuestObjectiveJson>();
        Dictionary<int, int> progress = new Dictionary<int, int>();

        for (int i = 0; i < objectiveid.Count; i++)
        {
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid[i]);
            objectivesJson.Add(objectiveid[i], objectiveJson);
            progress.Add(objectiveid[i], progresscount[i]);
        }

        if (objectiveid.Count >= 1)
        {
            description = objectivesJson[objectiveid[0]].description;
        }

        foreach (KeyValuePair<int, QuestObjectiveJson> entry in objectivesJson)
        {
            string replacement1 = "%o_" + entry.Key + "[p1]%";
            string replacement2 = "%o_" + entry.Key + "[p2]%";
            string replacement3 = "%o_" + entry.Key + "[p3]%";
            string replacement4 = "%pc_" + entry.Key + "%";
            QuestObjectiveJson objectiveJson = entry.Value;
            description = DeserializedObjectiveTarget1(description, replacement1, objectiveJson.para1, objectiveJson.type);
            description = DeserializedObjectiveTarget2(description, replacement2, objectiveJson.para2, objectiveJson.type);
            description = DeserializedObjectiveTarget3(description, replacement3, objectiveJson.para3, objectiveJson.type);
            description = DeserializedProgressCount(description, replacement4, progress[entry.Key]);
        }
        return description;
    }

    #region Deserialized Objective Description
    private string DeserializedObjectiveTarget1(string description, string replacement, int param, QuestObjectiveType type)
    {
        string targetname = "";
        bool ishyperlink = false;
        switch (type)
        {
            case QuestObjectiveType.Kill:
            case QuestObjectiveType.PercentageKill:
                targetname = CombatNPCRepo.GetNPCById(param).localizedname;
                ishyperlink = true;
                break;
            case QuestObjectiveType.Talk:
            case QuestObjectiveType.Choice:
            case QuestObjectiveType.QuickTalk:
                targetname = StaticNPCRepo.GetNPCById(param).localizedname;
                ishyperlink = true;
                break;
            case QuestObjectiveType.Interact:
            case QuestObjectiveType.RealmComplete:
            case QuestObjectiveType.Empty:
            case QuestObjectiveType.MultipleObj:
                targetname = "";
                ishyperlink = false;
                break;
            case QuestObjectiveType.Guide:
                targetname = StaticGuideRepo.GetNPCById(param).localizedname;
                ishyperlink = true;
                break;
        }

        if (ishyperlink)
        {
            string hyperlink = GenerateHyperlink(targetname, type, param);
            return description.Replace(replacement, hyperlink);
        }
        else
        {
            return description.Replace(replacement, targetname);
        }
    }

    private string DeserializedObjectiveTarget2(string description, string replacement, int param, QuestObjectiveType type)
    {
        string targetname = "";
        switch (type)
        {
            case QuestObjectiveType.Kill:
            case QuestObjectiveType.PercentageKill:
            case QuestObjectiveType.Interact:
            case QuestObjectiveType.RealmComplete:
                targetname = param.ToString();
                break;
            case QuestObjectiveType.Talk:
            case QuestObjectiveType.Choice:
            case QuestObjectiveType.Empty:
            case QuestObjectiveType.MultipleObj:
            case QuestObjectiveType.QuickTalk:
            case QuestObjectiveType.Guide:
                targetname = "";
                break;
        }
        
        return description.Replace(replacement, targetname);
    }

    private string DeserializedObjectiveTarget3(string description, string replacement, int param, QuestObjectiveType type)
    {
        string targetname = "";
        bool ishyperlink = false;
        switch (type)
        {
            case QuestObjectiveType.Kill:
            case QuestObjectiveType.PercentageKill:
            case QuestObjectiveType.Talk:
            case QuestObjectiveType.Choice:
            case QuestObjectiveType.QuickTalk:
            case QuestObjectiveType.RealmComplete:
            case QuestObjectiveType.Empty:
            case QuestObjectiveType.MultipleObj:
            case QuestObjectiveType.Guide:
                targetname = "";
                ishyperlink = false;
                break;
            case QuestObjectiveType.Interact:
                targetname = StaticNPCRepo.GetNPCById(param).localizedname;
                ishyperlink = true;
                break;
        }

        if (ishyperlink)
        {
            string hyperlink = GenerateHyperlink(targetname, type, param);
            return description.Replace(replacement, hyperlink);
        }
        else
        {
            return description.Replace(replacement, targetname);
        }
    }

    private string DeserializedProgressCount(string description, string replacement, int progresscount)
    {
        string result = DescriptionColor(progresscount.ToString(), Color.black);
        return description.Replace(replacement, result);
    }
    #endregion

    private string DeserializedRequirement(string description, int requirementid, Dictionary<int, int> progress = null)
    {
        List<QuestRequirementDetailJson> list = QuestRepo.GetRequirementByGroupId(requirementid);
        if (list == null)
        {
            return description;
        }

        if (progress == null)
        {
            progress = new Dictionary<int, int>();
        }

        Dictionary<int, QuestRequirementDetailJson> questRequirementDetailJsons = new Dictionary<int, QuestRequirementDetailJson>();
        foreach (QuestRequirementDetailJson requirementDetail in list)
        {
            questRequirementDetailJsons.Add(requirementDetail.requirementid, requirementDetail);
            if (!progress.ContainsKey(requirementDetail.requirementid))
            {
                progress.Add(requirementDetail.requirementid, CheckRequirementProgress(requirementDetail));
            }
        }
        if (list != null)
        {
            if (questRequirementDetailJsons.Count == 1)
            {
                return DeserializedRequirementDescription(list[0], description, progress);
            }
            else
            {
                return DeserializedRequirementDescription(questRequirementDetailJsons, description, progress);
            }
        }
        return description;
    }

    private string DeserializedRequirementDescription(QuestRequirementDetailJson requirementDetailJson, string description, Dictionary<int, int> progress)
    {
        description = DeserializedRequirementParam1(description, "%r[p1]%", requirementDetailJson.para1, requirementDetailJson.type);
        description = DeserializedRequirementParam2(description, "%r[p2]%", requirementDetailJson.para2, requirementDetailJson.type);
        description = DeserializedRequirementParam3(description, "%r[p3]%", requirementDetailJson.para3, requirementDetailJson.type);
        description = DeserializedRequirementProgress(description, requirementDetailJson, progress);
        return description;
    }

    private string DeserializedRequirementDescription(Dictionary<int, QuestRequirementDetailJson> requirementDetailJsons, string description, Dictionary<int, int> progress)
    {
        foreach (KeyValuePair<int, QuestRequirementDetailJson> entry in requirementDetailJsons)
        {
            QuestRequirementDetailJson requirementDetailJson = entry.Value;
            string replacement1 = "%r_" + entry.Key + "[p1]%";
            string replacement2 = "%r_" + entry.Key + "[p2]%";
            string replacement3 = "%r_" + entry.Key + "[p3]%";
            description = DeserializedRequirementParam1(description, replacement1, requirementDetailJson.para1, requirementDetailJson.type);
            description = DeserializedRequirementParam2(description, replacement2, requirementDetailJson.para2, requirementDetailJson.type);
            description = DeserializedRequirementParam3(description, replacement3, requirementDetailJson.para3, requirementDetailJson.type);
            description = DeserializedRequirementProgress(description, requirementDetailJson, progress);
        }

        return description;
    }

    #region Deserialized Requirement
    private string DeserializedRequirementParam1(string description, string replacement, int param, QuestRequirementType type)
    {
        string value = "";
        switch (type)
        {
            case QuestRequirementType.Level:
                value = param.ToString();
                break;
            case QuestRequirementType.Item:
            case QuestRequirementType.Equipment:
                value = GameRepo.ItemFactory.GetItemById(param).localizedname;
                break;
            case QuestRequirementType.Hero:
                value = HeroRepo.GetHeroById(param).localizedname;
                break;
            case QuestRequirementType.Title:
                value = "";
                break;
            case QuestRequirementType.SideEffect:
                value = SideEffectRepo.GetSideEffect(param).localizedname;
                break;
            case QuestRequirementType.Companian:
                value = StaticNPCRepo.GetNPCById(param).localizedname;
                break;
            case QuestRequirementType.Clue:
            case QuestRequirementType.Job:
            case QuestRequirementType.TimeClue:
                value = "";
                break;
        }
        string result = DescriptionColor(value, Color.black);
        return description.Replace(replacement, result);
    }

    private string DeserializedRequirementParam2(string description, string replacement, int param, QuestRequirementType type)
    {
        string value = "";
        switch (type)
        {
            case QuestRequirementType.Level:
            case QuestRequirementType.Equipment:
            case QuestRequirementType.Hero:
            case QuestRequirementType.Title:
            case QuestRequirementType.SideEffect:
            case QuestRequirementType.Companian:
            case QuestRequirementType.Clue:
            case QuestRequirementType.Job:
            case QuestRequirementType.TimeClue:
                value = "";
                break;
            case QuestRequirementType.Item:
                value = param.ToString();
                break;
        }
        string result = DescriptionColor(value, Color.black);
        return description.Replace(replacement, result);
    }

    private string DeserializedRequirementParam3(string description, string replacement, int param, QuestRequirementType type)
    {
        string value = "";
        switch (type)
        {
            case QuestRequirementType.Level:
            case QuestRequirementType.Item:
            case QuestRequirementType.Hero:
            case QuestRequirementType.Title:
            case QuestRequirementType.SideEffect:
            case QuestRequirementType.Companian:
            case QuestRequirementType.Clue:
            case QuestRequirementType.Job:
            case QuestRequirementType.TimeClue:
                value = "";
                break;
            case QuestRequirementType.Equipment:
                value = param.ToString();
                break;
        }
        string result = DescriptionColor(value, Color.black);
        return description.Replace(replacement, result);
    }

    private string DeserializedRequirementProgress(string description, QuestRequirementDetailJson requirementDetailJson, Dictionary<int, int> progress)
    {
        string replacement = "%r_" + requirementDetailJson.requirementid + "%";
        string value = "";
        switch (requirementDetailJson.type)
        {
            case QuestRequirementType.Level:
            case QuestRequirementType.Item:
            case QuestRequirementType.Equipment:
                value = progress[requirementDetailJson.requirementid].ToString();
                break;
            case QuestRequirementType.Hero:
            case QuestRequirementType.Title:
            case QuestRequirementType.SideEffect:
            case QuestRequirementType.Companian:
            case QuestRequirementType.Clue:
            case QuestRequirementType.Job:
            case QuestRequirementType.TimeClue:
                value = "";
                break;
        }
        string result = DescriptionColor(value, Color.black);
        return description.Replace(replacement, result);
    }

    private int CheckRequirementProgress(QuestRequirementDetailJson requirementDetailJson)
    {
        switch (requirementDetailJson.type)
        {
            case QuestRequirementType.Level:
                return mPlayer.PlayerSynStats.Level;
            case QuestRequirementType.Item:
                return mPlayer.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)requirementDetailJson.para1);
            case QuestRequirementType.Equipment:
            case QuestRequirementType.Hero:
                if (requirementDetailJson.para2 == 1 && mPlayer.HeroStats.IsHeroSummoned(requirementDetailJson.para1))
                {
                    return 1;
                }
                else if (requirementDetailJson.para2 == 2 && mPlayer.HeroStats.IsHeroUnlocked(requirementDetailJson.para1))
                {
                    return 1;
                }
                break;
            case QuestRequirementType.Title:
            case QuestRequirementType.SideEffect:
                if (requirementDetailJson.para2 == 1 && mPlayer.HasSideEffect(requirementDetailJson.para1))
                {
                    return 1;
                }
                else if (requirementDetailJson.para2 == 2 && !mPlayer.HasSideEffect(requirementDetailJson.para1))
                {
                    return 1;
                }
                break;
            case QuestRequirementType.Companian:
                if (requirementDetailJson.para2 == 1 && GetCompanionId() == requirementDetailJson.para1)
                {
                    return 1;
                }
                else if (requirementDetailJson.para2 == 2 && GetCompanionId() != requirementDetailJson.para1)
                {
                    return 1;
                }
                break;
            case QuestRequirementType.Clue:
                if (requirementDetailJson.para2 == 1 && mPlayer.DestinyClueController.IsClueAlreadyUnlock(requirementDetailJson.para1))
                {
                    return 1;
                }
                else if (requirementDetailJson.para2 == 2 && !mPlayer.DestinyClueController.IsClueAlreadyUnlock(requirementDetailJson.para1))
                {
                    return 1;
                }
                break;
            case QuestRequirementType.Job:
                return mPlayer.PlayerSynStats.jobsect;
            case QuestRequirementType.TimeClue:
                if (requirementDetailJson.para2 == 1 && mPlayer.DestinyClueController.IsTimeClueAlreadyUnlock(requirementDetailJson.para1))
                {
                    return 1;
                }
                else if (requirementDetailJson.para2 == 2 && !mPlayer.DestinyClueController.IsTimeClueAlreadyUnlock(requirementDetailJson.para1))
                {
                    return 1;
                }
                break;
        }
        return 0;
    }
    #endregion

    private string DescriptionColor(string text, Color color)
    {
        string colorcode = ColorUtility.ToHtmlStringRGB(color);
        return "<color=" + colorcode + ">" + text + "</color>";
    }

    private string GenerateHyperlink(string display, QuestObjectiveType type, int param)
    {
        return "<a name=\"" + (byte)type + ";" + param + ";1\">" + display + "</a>";
    }

    private string GenerateHyperlink(string display, QuestTriggerType type, int param)
    {
        return "<a name=\"" + (byte)type + ";" + param + ";0\">" + display + "</a>";
    }

    public string GetStartQuestDescription(QuestJson questJson)
    {
        string description = questJson.description;
        string replacement = "%tc%";
        string targetname = "";
        bool ishyperlink = false;
        switch (questJson.triggertype)
        {
            case QuestTriggerType.NPC:
                ishyperlink = true;
                targetname = StaticNPCRepo.GetNPCById(questJson.triggercaller).localizedname;
                break;
            case QuestTriggerType.Item:
                targetname = GameRepo.ItemFactory.GetItemById(questJson.triggercaller).localizedname;
                break;
            case QuestTriggerType.Level:
                targetname = questJson.triggercaller.ToString();
                break;
            case QuestTriggerType.Interact:
            case QuestTriggerType.Signboard:
                targetname = "";
                break;
            case QuestTriggerType.Hero:
                targetname = HeroRepo.GetHeroById(questJson.triggercaller).localizedname;
                break;
        }
        if (ishyperlink)
        {
            string hyperlink = GenerateHyperlink(targetname, questJson.triggertype, questJson.triggercaller);
            return description.Replace(replacement, hyperlink);
        }
        else
        {
            return description.Replace(replacement, targetname);
        }
    }
    #endregion

    public Dictionary<int, DateTime> GetMainObjectiveEndtime(CurrentQuestData questData)
    {
        Dictionary<int, DateTime> completetimes = new Dictionary<int, DateTime>();
        if (questData.MainObjective.CompleteTime.Count > 0)
        {
            for (int i = 0; i < questData.MainObjective.ObjectiveIds.Count; i++)
            {
                if (questData.MainObjective.CompleteTime.Count > i && 
                    questData.MainObjective.CompleteTime[i].GetType() == typeof(string) && 
                    questData.MainObjective.CompleteTime[i] != "null" && 
                    questData.MainObjective.CompleteTime[i] != "0")
                {
                    completetimes.Add(questData.MainObjective.ObjectiveIds[i], DateTime.ParseExact(questData.MainObjective.CompleteTime[i], "yyyy.MM.dd+HH:mm:ss", CultureInfo.InvariantCulture));
                }
            }
        }

        return completetimes;
    }

    public Dictionary<int, DateTime> GetSubObjectiveEndtime(CurrentQuestData questData)
    {
        Dictionary<int, DateTime> completetimes = new Dictionary<int, DateTime>();
        foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
        {
            if (entry.Value.CompleteTime.Count > 0)
            {
                for (int i = 0; i < entry.Value.ObjectiveIds.Count; i++)
                {
                    if (entry.Value.CompleteTime.Count > i && 
                        entry.Value.CompleteTime[i].GetType() == typeof(string) &&
                        entry.Value.CompleteTime[i] != "null" && 
                        entry.Value.CompleteTime[i] != "0")
                    {
                        completetimes.Add(entry.Value.ObjectiveIds[i], DateTime.ParseExact(entry.Value.CompleteTime[i], "yyyy.MM.dd+HH:mm:ss", CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        return completetimes;
    }

    public string ReplaceEndTime(string description, Dictionary<int, DateTime> mainendtime, Dictionary<int, DateTime> subendtime)
    {
        if (mainendtime != null && mainendtime.Count >= 1)
        {
            DateTime mainET = mainendtime.Values.First();
            double remainsec = mainET.Subtract(DateTime.Now).TotalSeconds;
            string time = "";
            if (remainsec > 0)
            {
                time = GUILocalizationRepo.GetShortLocalizedTimeString(remainsec);
            }
            else
            {
                string colorcode = ColorUtility.ToHtmlStringRGB(Color.red);
                description = "<color=#" + colorcode + ">" + GUILocalizationRepo.GetLocalizedString("quest_failed") + "</color>" + " " + description;
            }
            description = description.Replace("%t%", time);
        }
        if (subendtime != null && subendtime.Count >= 1)
        {
            KeyValuePair<int, DateTime> subET = subendtime.FirstOrDefault();
            double remainsec = subET.Value.Subtract(DateTime.Now).TotalSeconds;
            string replacement = "%t_" + subET.Key + "%";
            string time = "";
            if (remainsec > 0)
            {
                time = GUILocalizationRepo.GetShortLocalizedTimeString(remainsec);
            }
            else
            {
                string colorcode = ColorUtility.ToHtmlStringRGB(Color.red);
                description = "<color=#" + colorcode + ">" + GUILocalizationRepo.GetLocalizedString("quest_failed") + "</color>" + " " + description;
            }
            description = description.Replace(replacement, time);
        }
        return description;
    }

    public List<int> GetTrackingList()
    {
        if (TrackingListUpdate && mTrackingList_UI != null)
        {
            return mTrackingList_UI;
        }
        else
        {
            mTrackingList_UI = mTrackingList;
            return mTrackingList_UI;
        }
    }

    public bool IsQuestTracked(int questid)
    {
        return mTrackingList.Contains(questid);
    }

    public void UpdateTrackingList()
    {
        if (TrackingListUpdate)
        {
            TrackingListUpdate = false;
            RPCFactory.NonCombatRPC.UpdateTrakingList(JsonConvertDefaultSetting.SerializeObject(mTrackingList_UI));
            mTrackingList_UI = null;
        }
    }

    public List<int> GetWonderfulList()
    {
        return mWonderfulList;
    }

    #region Trigger Quest Event
    private void TriggerQuestEvent(CurrentQuestData questData)
    {
        if (questData == null)
        {
            return;
        }

        if (questData.Status == (byte)QuestStatus.NewQuestWithEvent || questData.Status == (byte)QuestStatus.NewObjectiveWithEvent || questData.Status == (byte)QuestStatus.CompletedWithEvent
            || questData.SubStatus == (byte)QuestStatus.NewQuestWithEvent || questData.SubStatus == (byte)QuestStatus.NewObjectiveWithEvent || questData.SubStatus == (byte)QuestStatus.CompletedWithEvent)
        {
            if (GameInfo.mQuestEventTriggered != -1 && GameInfo.mQuestEventTriggered != questData.QuestId)
            {
                GameInfo.mPendingQuestEventList.Add(questData.QuestId);
            }
            else if (GameInfo.mQuestEventTriggered == -1)
            {
                GameInfo.mQuestEventTriggered = questData.QuestId;
                GetAllEventList(questData);
            }
            else if (questData.QuestId == GameInfo.mQuestEventTriggered)
            {
                Debug.Log("Remaining Quest Event List : " + GameInfo.mQuestEventList.Count);
            }
        }
        else
        {
            AutoSubmitQuest(questData);
        }
    }

    private void GetAllEventList(CurrentQuestData questData)
    {
        if (questData.Status == (byte)QuestStatus.NewQuestWithEvent || questData.SubStatus == (byte)QuestStatus.NewQuestWithEvent)
        {
            QuestJson questJson = QuestRepo.GetQuestByID(GameInfo.mQuestEventTriggered);
            GameInfo.mQuestEventList = QuestRepo.GetQuestEvents(questJson.eventid, EventTimingType.StartQuest);
        }
        else if (questData.Status == (byte)QuestStatus.CompletedWithEvent || questData.SubStatus == (byte)QuestStatus.CompletedWithEvent)
        {
            QuestJson questJson = QuestRepo.GetQuestByID(GameInfo.mQuestEventTriggered);
            GameInfo.mQuestEventList = QuestRepo.GetQuestEvents(questJson.eventid, EventTimingType.CompleteQuest);
        }
        else if (questData.Status == (byte)QuestStatus.NewObjectiveWithEvent || questData.SubStatus == (byte)QuestStatus.NewObjectiveWithEvent)
        {
            List<int> objectivelist = new List<int>();
            objectivelist.AddRange(questData.MainObjective.ObjectiveIds);
            foreach(KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
            {
                objectivelist.AddRange(entry.Value.ObjectiveIds);
            }
            GameInfo.mQuestEventList = QuestRepo.GetQuestEvents(EventTimingType.CompleteObjective, objectivelist);
        }

        StartQuestEvent();
    }

    public IEnumerator OnPlayerReady()
    {
        yield return new WaitForSecondsRealtime(1);

        GameInfo.mWaitingQuestEvent = false;
        GameInfo.gLocalPlayer.QuestController.StartQuestEvent();
    }

    public void StartQuestEvent()
    {
        if (GameInfo.mWaitingQuestEvent)
        {
            return;
        }

        if (GameInfo.mQuestEventList.Count > 0)
        {
            QuestEventDetailJson questEvent = GameInfo.mQuestEventList.First();
            if (questEvent != null)
            {
                GameInfo.mQuestEventList.Remove(questEvent);
                TriggerQuestEvent(questEvent);
            }
        }
        else
        {
            RPCFactory.NonCombatRPC.UpdateQuestStatus(GameInfo.mQuestEventTriggered);
            StartNextQuestEvent();
        }
    }

    private void TriggerQuestEvent(QuestEventDetailJson questEvent)
    {
        switch (questEvent.type)
        {
            case QuestEventType.Cutscene:
                int delay = 0;
                int.TryParse(questEvent.para2, out delay);
                GameInfo.mWaitingQuestEvent = true;
                GameInfo.gCombat.StartCoroutine(mPlayer.PlayCutscene(questEvent.para1, delay, GameInfo.mQuestEventTriggered));
                break;
            case QuestEventType.Playmaker:
                break;
            case QuestEventType.Monster:
                int monsterid = -1;
                int monsterstatus = 0;
                int.TryParse(questEvent.para1, out monsterid);
                int.TryParse(questEvent.para2, out monsterstatus);
                NPCJson npcJson = CombatNPCRepo.GetNPCById(monsterid);
                if (npcJson != null)
                {
                    if (monsterstatus == 1 || monsterstatus == 2)
                    {
                        bool aggressive = monsterstatus == 2 ? true : false;
                        RPCFactory.NonCombatRPC.ConsoleSpawnPersonalMonster(npcJson.archetype, -1, aggressive);
                    }
                }
                break;
            case QuestEventType.Teleport:
                float x = 0, y = 0, z = 0;
                if (!string.IsNullOrEmpty(questEvent.para2))
                {
                    string[] pos = questEvent.para2.Split(',');
                    for (int i = 0; i < pos.Length; i++)
                    {
                        if (i == 0)
                        {
                            float.TryParse(pos[i], out x);
                        }
                        else if (i == 1)
                        {
                            float.TryParse(pos[i], out y);
                        }
                        else if (i == 2)
                        {
                            float.TryParse(pos[i], out z);
                        }
                    }
                }
                Vector3 position = new Vector3(x, y, z);
                GameInfo.mWaitingQuestEvent = true;
                RPCFactory.CombatRPC.OnTeleportToLevelAndPos(questEvent.para1, position.ToRPCPosition());
                break;
            case QuestEventType.SideEffect:
                int seid = -1;
                int.TryParse(questEvent.para1, out seid);
                if (GameInfo.mSideEffectQuestStatus.Count > 0)
                {
                    foreach (KeyValuePair<int, int> entry in GameInfo.mSideEffectQuestStatus)
                    {
                        RPCFactory.CombatRPC.RemoveSideBuff(entry.Value);
                    }
                    GameInfo.mSideEffectQuestStatus = new Dictionary<int, int>();
                }
                GameInfo.mSideEffectQuestStatus.Add(GameInfo.mQuestEventTriggered, seid);
                RPCFactory.NonCombatRPC.ApplyQuestEventBuff(questEvent.id);
                break;
            case QuestEventType.Companion:
                int companionid = -1;
                int.TryParse(questEvent.para1, out companionid);
                GameInfo.mCompanionQuestStatus = new Dictionary<int, int>();
                GameInfo.mCompanionQuestStatus.Add(GameInfo.mQuestEventTriggered, companionid);
                RPCFactory.NonCombatRPC.ApplyQuestEventCompanion(questEvent.id, GameInfo.mQuestEventTriggered);
                break;
            case QuestEventType.NPC:
                int npcid;
                bool status;
                int.TryParse(questEvent.para1, out npcid);
                status = questEvent.para2 == "1" ? true : false;
                StaticClientNPCAlwaysShow staticnpc = GameInfo.gCombat.mEntitySystem.GetStaticClientNPC(npcid);
                if (staticnpc != null)
                {
                    bool startupdisplay = staticnpc.StartUpDisplay;
                    if (startupdisplay != status)
                    {
                        if (!GameInfo.mNpcQuestStatus.ContainsKey(npcid))
                        {
                            GameInfo.mNpcQuestStatus.Add(npcid, new List<int>());
                        }
                        if (!GameInfo.mNpcQuestStatus[npcid].Contains(GameInfo.mQuestEventTriggered))
                        {
                            GameInfo.mNpcQuestStatus[npcid].Add(GameInfo.mQuestEventTriggered);
                        }
                    }
                    else
                    {
                        if (GameInfo.mNpcQuestStatus.ContainsKey(npcid))
                        {
                            GameInfo.mNpcQuestStatus.Remove(npcid);
                        }
                    }
                    staticnpc.UpdateDisplayStatus(status);
                }
                break;
            case QuestEventType.Realm:
                int realmid;
                bool realmstatus;
                int.TryParse(questEvent.para1, out realmid);
                realmstatus = questEvent.para2 == "1" ? true : false;
                GameInfo.mWaitingQuestEvent = true;
                if (realmstatus)
                {
                    RPCFactory.CombatRPC.CreateRealmByID(realmid, false, false, GameInfo.mQuestEventTriggered);
                }
                else
                {
                    string levelName = SceneManager.GetActiveScene().name;
                    if (RealmRepo.IsWorld(levelName))
                    {
                        GameInfo.mWaitingQuestEvent = false;
                    }
                    else
                    {
                        RPCFactory.CombatRPC.LeaveRealm();
                    }
                }
                break;
        }

        if (!GameUtils.IsEmptyString(questEvent.msg))
        {
            UIManager.ShowSystemMessage(questEvent.msg);
        }

        if (!GameInfo.mWaitingQuestEvent)
        {
            StartQuestEvent();
        }
    }

    public void RollBackQuestEvent(int questid)
    {
        NpcDisplayRollBack(questid);
        SideEffectRollBack(questid);
        CompanionRollBack(questid);
    }

    private void NpcDisplayRollBack(int questid)
    {
        List<int> npclist = new List<int>();
        foreach (KeyValuePair<int, List<int>> entry in GameInfo.mNpcQuestStatus)
        {
            if (entry.Value.Contains(questid))
            {
                npclist.Add(entry.Key);
            }
        }

        foreach(int npcid in npclist)
        {
            GameInfo.mNpcQuestStatus[npcid].Remove(questid);
            if (GameInfo.mNpcQuestStatus[npcid].Count <= 0)
            {
                StaticClientNPCAlwaysShow staticnpc = GameInfo.gCombat.mEntitySystem.GetStaticClientNPC(npcid);
                if (staticnpc != null)
                {
                    staticnpc.UpdateDisplayStatus(staticnpc.StartUpDisplay);
                }
            }
        }
    }

    private void SideEffectRollBack(int questid)
    {
        if (GameInfo.mSideEffectQuestStatus.ContainsKey(questid))
        {
            RPCFactory.CombatRPC.RemoveSideBuff(GameInfo.mSideEffectQuestStatus[questid]);
            GameInfo.mSideEffectQuestStatus = new Dictionary<int, int>();
        }
    }

    private void CompanionRollBack(int questid)
    {
        if (GameInfo.mCompanionQuestStatus.ContainsKey(questid))
        {
            RPCFactory.NonCombatRPC.ResetQuestEventCompanion(GameInfo.mCompanionQuestStatus[questid]);
            GameInfo.mCompanionQuestStatus = new Dictionary<int, int>();
        }
    }

    public void StartNextQuestEvent()
    {
        GameInfo.mQuestEventTriggered = -1;
        if (GameInfo.mPendingQuestEventList.Count > 0)
        {
            GameInfo.mQuestEventTriggered = GameInfo.mPendingQuestEventList[0];
            GameInfo.mPendingQuestEventList.Remove(GameInfo.mQuestEventTriggered);
            QuestJson questJson = QuestRepo.GetQuestByID(GameInfo.mQuestEventTriggered);
            if (questJson != null)
            {
                CurrentQuestData questData = GetQuestData(questJson.type, GameInfo.mQuestEventTriggered);
                if (questData != null)
                {
                    GetAllEventList(questData);
                }
            }
        }
    }

    private void UpdateNpcDisplayStatus()
    {
        foreach (KeyValuePair<int, List<int>> entry in GameInfo.mNpcQuestStatus)
        {
            StaticClientNPCAlwaysShow staticnpc = GameInfo.gCombat.mEntitySystem.GetStaticClientNPC(entry.Key);
            if (staticnpc != null)
            {
                staticnpc.ResetDisplayStatus();
            }
        }
    }

    public void OnCutSceneFinished()
    {
        GameInfo.mWaitingQuestEvent = false;
        StartQuestEvent();
    }

    public void OnLevelLoaded()
    {
        StartQuestEvent();
    }
    #endregion

    private void AutoSubmitQuest(CurrentQuestData questData)
    {
        QuestStatus status = (QuestStatus)questData.Status;
        if (status == QuestStatus.CompletedAllObjective)
        {
            QuestJson questJson = QuestRepo.GetQuestByID(questData.QuestId);
            if (questJson != null && !questJson.replyid)
            {
                UIManager.StartHourglass();
                RPCFactory.NonCombatRPC.CompleteQuest(questData.QuestId, questJson.replyid);

                if (!CutsceneManager.instance.IsPlaying())
                    BotStateController.Instance.Resume();
            }
        }
        else if ((status == QuestStatus.NewObjective || status == QuestStatus.NewQuest) && bInit)
        {
            mQuestDialogueController.CheckDialogueAvailableQuest(questData);
        }
    }

    public void CloseNpcTalk()
    {
        if (mQuestDialogueController.HasPendingDialogue())
        {
            mQuestDialogueController.StartNextDialogue();
        }
        else
        {
            UIManager.CloseDialog(WindowType.DialogNpcTalk);
            BotStateController.Instance.Resume();
        }
    }

    public void UpdateTriggerData(StaticClientNPCAlwaysShow staticArea)
    {
        if (mActivedTrigger == staticArea)
        {
            if (typeof(StaticAreaGhost) == mActivedTrigger.GetType())
            {
                int interactid = ((StaticAreaGhost)mActivedTrigger).GetInteractId();
                if (interactid != -1)
                {
                    int questid = ((StaticAreaGhost)mActivedTrigger).GetActivedQuestId();
                    if (!UIManager.IsWidgetActived(HUDWidgetType.QuestAction))
                    {
                        UIManager.SetWidgetActive(HUDWidgetType.QuestAction, true);
                    }
                    Hud_QuestAction questAction = UIManager.GetWidget(HUDWidgetType.QuestAction).GetComponent<Hud_QuestAction>();
                    questAction.Init(interactid, questid);
                }
                else
                {
                    UIManager.SetWidgetActive(HUDWidgetType.QuestAction, false);
                    BotStateController.Instance.Resume();
                }
            }
            else if (typeof(StaticGuideGhost) == mActivedTrigger.GetType())
            {
                ((StaticGuideGhost)mActivedTrigger).DoInteractAction();
            }
        }
    }

    public void OnEnterStaticArea(StaticClientNPCAlwaysShow staticArea)
    {
        if (!mTriggerArea.Contains(staticArea))
        {
            mTriggerArea.Add(staticArea);
        }
        OnTriggerChanged();
    }

    public void OnExitStaticArea(StaticClientNPCAlwaysShow staticArea)
    {
        if (mTriggerArea.Contains(staticArea))
        {
            mTriggerArea.Remove(staticArea);
        }
        OnTriggerChanged();
    }

    private void OnTriggerChanged()
    {
        mActivedTrigger = mTriggerArea.Count > 0 ? mTriggerArea.First() : null;

        if (mActivedTrigger != null)
        {
            if (typeof(StaticAreaGhost) == mActivedTrigger.GetType())
            {
                int interactid = ((StaticAreaGhost)mActivedTrigger).GetInteractId();
                int questid = ((StaticAreaGhost)mActivedTrigger).GetActivedQuestId();
                if (interactid != -1)
                {
                    if (!UIManager.IsWidgetActived(HUDWidgetType.QuestAction))
                    {
                        UIManager.SetWidgetActive(HUDWidgetType.QuestAction, true);
                    }
                    Hud_QuestAction questAction = UIManager.GetWidget(HUDWidgetType.QuestAction).GetComponent<Hud_QuestAction>();
                    questAction.Init(interactid, questid);
                }
                else
                {
                    UIManager.SetWidgetActive(HUDWidgetType.QuestAction, false);
                }
            }
            else if (typeof(StaticGuideGhost) == mActivedTrigger.GetType())
            {
                ((StaticGuideGhost)mActivedTrigger).DoInteractAction();
            }
        }
        else
        {
            UIManager.SetWidgetActive(HUDWidgetType.QuestAction, false);
        }
    }

    public void GetInteractData(int questid, int interactid, out QuestObjectiveJson objectiveJson, out int progress)
    {
        objectiveJson = null;
        progress = 0;

        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            CurrentQuestData questData = GetQuestData(questJson.type, questJson.questid);
            if (questData != null)
            {
                for (int i = 0; i < questData.MainObjective.ObjectiveIds.Count; i++)
                {
                    int objectiveid = questData.MainObjective.ObjectiveIds[i];
                    objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
                    if (objectiveJson != null && objectiveJson.type == QuestObjectiveType.Interact && objectiveJson.para1 == interactid)
                    {
                        if (questData.MainObjective.ProgressCount.Count >= i)
                        {
                            progress = questData.MainObjective.ProgressCount[i];
                            return;
                        }
                    }
                }

                foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
                {
                    for (int i = 0; i < entry.Value.ObjectiveIds.Count; i++)
                    {
                        int objectiveid = entry.Value.ObjectiveIds[i];
                        objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
                        if (objectiveJson != null && objectiveJson.type == QuestObjectiveType.Interact && objectiveJson.para1 == interactid)
                        {
                            if (entry.Value.ProgressCount.Count >= i)
                            {
                                progress = entry.Value.ProgressCount[i];
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    public void ActionInterupted()
    {
        if (UIManager.IsWidgetActived(HUDWidgetType.ProgressBar))
        {
            Hud_ProgressBar progressBar = UIManager.GetWidget(HUDWidgetType.ProgressBar).GetComponent<Hud_ProgressBar>();
            progressBar.ForceEnd();
        }
        UIManager.SetWidgetActive(HUDWidgetType.ProgressBar, false);
    }

    #region Quest Pathfinding
    public void ProcessObjectiveHyperLink(string linkinfo, int questid)
    {
        string[] objectiveinfo = linkinfo.Split(';');
        int targetid;
        byte type;
        int isobjective = 0;
        int.TryParse(objectiveinfo[1], out targetid);
        byte.TryParse(objectiveinfo[0], out type);
        int.TryParse(objectiveinfo[2], out isobjective);
        if (isobjective == 1)
        {
            ProceedQuestObjective((QuestObjectiveType)type, targetid, questid);
        }
       else
        {
            ProceedQuestTrigger((QuestTriggerType)type, targetid, questid);
        }
    }

    public void ProceedQuestObjective(QuestObjectiveType type, int targetid, int questid)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            switch (type)
            {
                case QuestObjectiveType.Kill:
                case QuestObjectiveType.PercentageKill:
                    ProceedToQuestTarget(targetid, questid, questJson.teleport, true, (byte)type);
                    break;
                case QuestObjectiveType.Talk:
                case QuestObjectiveType.Choice:
                case QuestObjectiveType.Interact:
                    ProceedToQuestTarget(targetid, questid, questJson.teleport, false, (byte)type);
                    break;
                case QuestObjectiveType.Guide:
                    ProceedToQuestTarget(targetid, questid, questJson.teleport, false, (byte)type);
                    break;
                case QuestObjectiveType.QuickTalk:
                    CurrentQuestData questData = GetQuestData(questJson.type, questid);
                    if (questData != null)
                    {
                        mQuestDialogueController.CheckDialogueAvailableQuest(questData);
                    }
                    break;
            }
        }
    }

    public void ProceedQuestTrigger(QuestTriggerType type, int targetid, int questid)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            switch (type)
            {
                case QuestTriggerType.NPC:
                    ProceedToQuestTarget(targetid, questid, questJson.teleport, false, (byte)type, false);
                    break;
                case QuestTriggerType.Interact:
                    break;
            }
        }
    }

    public void CheckQuestTeleportAction()
    {
        if (GameInfo.mTeleportAction != null)
        {
            if (GameInfo.mTeleportAction.IsObjective)
            {
                ProceedQuestObjective((QuestObjectiveType)GameInfo.mTeleportAction.ActionType, GameInfo.mTeleportAction.TargetId, GameInfo.mTeleportAction.QuestId);
            }
            else
            {
                ProceedQuestTrigger((QuestTriggerType)GameInfo.mTeleportAction.ActionType, GameInfo.mTeleportAction.TargetId, GameInfo.mTeleportAction.QuestId);
            }
            GameInfo.mTeleportAction = null;
        }
    }

    private void ProceedToQuestTarget(int targetId, int questId, bool teleport, bool isCombat, byte type, bool isobjective = true)
    {
        string levelName = SceneManager.GetActiveScene().name;
        string targetlevel = "";
        Vector3 targetpos = Vector3.zero;

        bool foundtarget = false;
        if (isCombat)
        {
            NPCJson npcJson = CombatNPCRepo.GetNPCById(targetId);
            if (npcJson != null)
                foundtarget = NPCPosMap.FindNearestMonster(npcJson.archetype, levelName, mPlayer.Position, 
                    ref targetlevel, ref targetpos);
        }
        else
        {
            if (type == (byte)QuestObjectiveType.Guide)
            {
                StaticGuideJson staticGuideJson = StaticGuideRepo.GetNPCById(targetId);
                if (staticGuideJson != null)
                    foundtarget = NPCPosMap.FindNearestStaticGuide(staticGuideJson.archetype, levelName, mPlayer.Position,
                        ref targetlevel, ref targetpos);
            }
            else
            {
                StaticNPCJson staticNPCJson = StaticNPCRepo.GetNPCById(targetId);
                if (staticNPCJson != null)
                    foundtarget = NPCPosMap.FindNearestStaticNPC(staticNPCJson.archetype, levelName, mPlayer.Position,
                        ref targetlevel, ref targetpos);
            }
        }

        if (!foundtarget)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CannotFindTarget"));
            return;
        }
        else
        {
            mPlayer.Idle();
            PartyFollowTarget.Pause(false);
            BotStateController.Instance.Quest();

            if (levelName == targetlevel)
            {
                if (isCombat)
                {
                    mPlayer.PathFindToTarget(targetpos, -1, 0, false, false, () => {
                        BotStateController.Instance.CombatQuest(questId, targetId);
                    });
                }
                else if (type == (byte)QuestObjectiveType.Guide)
                {
                    mPlayer.ProceedToTarget(targetpos, targetId, CallBackAction.None);
                }
                else
                {
                    mPlayer.ProceedToTarget(targetpos, targetId, CallBackAction.Interact);
                }
            }
            else
            {
                bool currentmapisworld = RealmRepo.IsWorld(levelName);
                bool nextmapisworld = RealmRepo.IsWorld(targetlevel);
                if ((currentmapisworld && !nextmapisworld) || (!currentmapisworld && nextmapisworld))
                {
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("qst_targetcannotteleport"));
                }
                else
                {
                    if (teleport)
                    {
                        GameInfo.mTeleportAction = new QuestTeleportAction(type, targetId, questId, isobjective);
                        RPCFactory.CombatRPC.OnTeleportToLevel(targetlevel);
                    }
                    else
                    {
                        BotController.TheDijkstra.DoRouter(levelName, targetlevel, out foundtarget);
                        if (foundtarget)
                        {
                            BotController.DestLevel = targetlevel;
                            BotController.DestMapPos = targetpos;
                            BotController.DestAction = isCombat ? ReachTargetAction.StartBot : ReachTargetAction.NPC_Interact;
                            BotController.DestArchtypeID = isCombat ? -1 : targetId;
                            GameInfo.gLocalPlayer.Bot.SeekingWithRouter();
                        }
                        else
                            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CannotFindTarget"));
                    }
                }
            }
        }
    }

    public bool GetObjectiveTarget(CurrentQuestData questData, out int targetid, out QuestObjectiveType type)
    {
        List<int> objectivelist = new List<int>();
        foreach(int objectiveid in questData.MainObjective.ObjectiveIds)
        {
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
            if (objectiveJson != null && objectiveJson.type != QuestObjectiveType.MultipleObj)
            {
                objectivelist.Add(objectiveid);
            }
        }
        foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
        {
            foreach (int objectiveid in entry.Value.ObjectiveIds)
            {
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
                if (objectiveJson != null && objectiveJson.type != QuestObjectiveType.MultipleObj)
                {
                    objectivelist.Add(objectiveid);
                }
            }
        }

        targetid = -1;
        type = QuestObjectiveType.Empty;

        if (objectivelist.Count == 1)
        {
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectivelist[0]);
            if (objectiveJson != null)
            {
                type = objectiveJson.type;
                switch (objectiveJson.type)
                {
                    case QuestObjectiveType.Kill:
                    case QuestObjectiveType.PercentageKill:
                        targetid = objectiveJson.para1;
                        break;
                    case QuestObjectiveType.Talk:
                    case QuestObjectiveType.Choice:
                    case QuestObjectiveType.QuickTalk:
                        targetid = objectiveJson.para1;
                        break;
                    case QuestObjectiveType.Interact:
                        targetid = objectiveJson.para3;
                        break;
                    case QuestObjectiveType.Guide:
                        targetid = objectiveJson.para1;
                        break;
                }
            }
            return true;
        }
        
        return false;
    }

    public bool GetQuestTriggerTarget(int questid, out int targetid, out QuestTriggerType type)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);

        targetid = -1;
        type = QuestTriggerType.Level;

        if (questJson != null)
        {
            type = questJson.triggertype;
            switch (questJson.triggertype)
            {
                case QuestTriggerType.NPC:
                    targetid = questJson.triggercaller;
                    break;
                case QuestTriggerType.Interact:
                    targetid = questJson.triggercaller;
                    break;
            }
            return true;
        }
        return false;
    }
    #endregion

    public bool IsQuestInProgressOrUnlockOrCompleted(QuestJson questJson)
    {
        CurrentQuestData questData = GetQuestData(questJson.type, questJson.questid);
        if (questData != null)
        {
            return true;
        }

        if (mUnlockQuestList.Contains(questJson.questid))
        {
            return true;
        }

        if (IsQuestCompleted(questJson.type, questJson.questid))
        {
            return true;
        }

        return false;
    }

    public QuestJson GetUnlockMainQuest()
    {
        foreach(int questid in mUnlockQuestList)
        {
            QuestJson questJson = QuestRepo.GetQuestByID(questid);
            if (questJson != null && questJson.type == QuestType.Main)
            {
                return questJson;
            }
        }
        return null;
    }

    private int GetUnlockMainQuestId()
    {
        foreach (int questid in mUnlockQuestList)
        {
            QuestJson questJson = QuestRepo.GetQuestByID(questid);
            if (questJson != null && questJson.type == QuestType.Main)
            {
                return questJson.questid;
            }
        }
        return -1;
    }

    #region Quest Kill System Message
    private string GetKillObjectiveDescription(int objid, int progress)
    {
        QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objid);
        if (objectiveJson != null)
        {
            string description = string.IsNullOrEmpty(objectiveJson.msg) || objectiveJson.msg == "#unlocalized#" ? objectiveJson.description : objectiveJson.msg;
            string monstername = CombatNPCRepo.GetNPCById(objectiveJson.para1).localizedname;
            description = description.Replace("%o[p1]%", monstername);
            description = description.Replace("%o[p2]%", objectiveJson.para2.ToString());
            description = description.Replace("%pc%", progress.ToString());
            return description;
        }
        return "";
    }

    private void DisplayKillObjectiveMsg(CurrentObjectiveData newObjectiveData, CurrentObjectiveData oldObjectiveData)
    {
        int oldstep = oldObjectiveData.SequenceNum;
        int newstep = newObjectiveData.SequenceNum;
        int oldobjcount = oldObjectiveData.ProgressCount.Count;
        int newobjcount = newObjectiveData.ProgressCount.Count;
        if (oldstep == newstep && oldobjcount == newobjcount)
        {
            for (int i = 0; i < newobjcount; i++)
            {
                int newprogress = newObjectiveData.ProgressCount[i];
                int oldprogress = oldObjectiveData.ProgressCount[i];
                int objid = newObjectiveData.ObjectiveIds[i];
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objid);
                if (newprogress > oldprogress && (objectiveJson.type == QuestObjectiveType.Kill || objectiveJson.type == QuestObjectiveType.PercentageKill))
                {
                    UIManager.ShowSystemMessage(GetKillObjectiveDescription(objid, newprogress));
                }
            }
        }
        else if (newstep > oldstep)
        {
            for (int i = 0; i < oldobjcount; i++)
            {
                int oldprogress = oldObjectiveData.ProgressCount[i];
                int objid = oldObjectiveData.ObjectiveIds[i];
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objid);
                if (oldprogress < objectiveJson.para2 && (objectiveJson.type == QuestObjectiveType.Kill || objectiveJson.type == QuestObjectiveType.PercentageKill))
                {
                    UIManager.ShowSystemMessage(GetKillObjectiveDescription(objid, oldprogress + 1));
                }
            }
        }
    }

    private void DisplayKillObjectiveMsg(Dictionary<int, CurrentObjectiveData> newSubObjective, Dictionary<int, CurrentObjectiveData> oldSubObjective)
    {
        int oldsubcount = oldSubObjective.Count;
        int newsubcount = newSubObjective.Count;
        if (oldsubcount > 0)
        {
            foreach (KeyValuePair<int, CurrentObjectiveData> entry in oldSubObjective)
            {
                CurrentObjectiveData oldObjectiveData = entry.Value;
                CurrentObjectiveData newObjectiveData = null;
                if (newSubObjective.TryGetValue(entry.Key, out newObjectiveData))
                {
                    DisplayKillObjectiveMsg(newObjectiveData, oldObjectiveData);
                }
                else
                {
                    int oldobjcount = oldObjectiveData.ProgressCount.Count;
                    for (int i = 0; i < oldobjcount; i++)
                    {
                        int oldprogress = oldObjectiveData.ProgressCount[i];
                        int objid = oldObjectiveData.ObjectiveIds[i];
                        QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objid);
                        if (oldprogress < objectiveJson.para2 && (objectiveJson.type == QuestObjectiveType.Kill || objectiveJson.type == QuestObjectiveType.PercentageKill))
                        {
                            UIManager.ShowSystemMessage(GetKillObjectiveDescription(objid, oldprogress + 1));
                        }
                    }
                }
            }
        }
    }

    private void DisplayKillObjectiveMsg(CurrentQuestData newQuestData, CurrentQuestData oldQuestData)
    {
        if (oldQuestData != null && newQuestData != null)
        {
            DisplayKillObjectiveMsg(newQuestData.MainObjective, oldQuestData.MainObjective);
            DisplayKillObjectiveMsg(newQuestData.SubObjective, oldQuestData.SubObjective);
        }
    }
    #endregion

    #region Signboard Quest
    public List<int> GetUnlockSignboardList()
    {
        return mUnlockSignboardList;
    }

    public int GetSignboardRewardBoost()
    {
        return mSignboardRewardBoost;
    }

    public int GetSignboardLimit()
    {
        return mSignboardLimit;
    }

    private void UpdateDailyQuestUI()
    {
        if (UIManager.IsWindowOpen(WindowType.DailyQuest))
        {
            UIManager.GetWindowGameObject(WindowType.DailyQuest).GetComponent<UI_DailyActivity>().UpdateDailyQuest();
        }
    }
    #endregion

    private void CheckNewDestinyQuest(CurrentQuestData newQuestData, CurrentQuestData oldQuestData)
    {
        if (newQuestData != null && (newQuestData.Status == (byte)QuestStatus.DestinyEffect || newQuestData.Status == (byte)QuestStatus.DestinyEffectWithEvent))
        {
            QuestJson questJson = QuestRepo.GetQuestByID(newQuestData.QuestId);
            if (questJson != null && questJson.showae)
            {
                if (!UIManager.IsWidgetActived(HUDWidgetType.DestinyStart))
                {
                    UIManager.SetWidgetActive(HUDWidgetType.DestinyStart, true);
                    UIManager.GetWidget(HUDWidgetType.DestinyStart).GetComponent<Hud_DestinyStart>().Init(newQuestData.QuestId);
                    RPCFactory.NonCombatRPC.UpdateQuestStatus(newQuestData.QuestId);
                }
            }
        }
    }

    private void UpdateTriggerQuest(CurrentQuestData newQuestData, CurrentQuestData oldQuestData)
    {
        if (mQuestTriggerController != null)
        {
            if (oldQuestData == null && newQuestData != null)
            {
                mQuestTriggerController.NewAcceptedQuest(newQuestData.QuestId);
            }
            if (newQuestData != null)
            {
                mQuestTriggerController.UpdateOngoingQuest(newQuestData, mPlayer);
            }
            mQuestTriggerController.RefreshNPCForQuest();
        }

        if (mQuestGuideController != null)
        {
            if (newQuestData != null)
            {
                mQuestGuideController.UpdateOngoingQuest(newQuestData);
            }
        }
    }

    public void RemoveQuest(int questid)
    {
        if (mQuestTriggerController != null)
        {
            mQuestTriggerController.NewCompletedQuest(questid);
            mQuestTriggerController.RefreshNPCForQuest();
            mQuestGuideController.NewCompletedQuest(questid);
        }
    }

    private void CompletedListUpdated(List<int> newData, List<int> oldData)
    {
        if (mQuestTriggerController != null)
        {
            List<int> added = new List<int>();
            List<int> removed = new List<int>();

            foreach (int questid in newData)
            {
                if (!oldData.Contains(questid))
                {
                    added.Add(questid);
                }
            }

            foreach (int questid in oldData)
            {
                if (!newData.Contains(questid))
                {
                    removed.Add(questid);
                }
            }

            mQuestTriggerController.AddedCompletedList(added);
            mQuestTriggerController.RemovedCompletedList(removed);
            mQuestTriggerController.RefreshNPCForQuest();
            mQuestGuideController.AddedCompletedList(added);
        }
    }

    public QuestTriggerController GetQuestTriggerController()
    {
        return mQuestTriggerController;
    }

    public void UpdateRequirementProgress(QuestRequirementType requirementType, int triggerid, PlayerGhost player)
    {
        if (mQuestRequirementController != null)
        {
            mQuestRequirementController.UpdateTriggerQuestRequirementProgress(requirementType, triggerid, player);
            mQuestRequirementController.UpdateObjectiveRequirementProgress(requirementType, triggerid, player);
        }
    }

    public bool IsObjectiveAvailable(int questid, int objectiveid)
    {
        bool result = false;
        QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
        if (objectiveJson != null)
        {
            int progress = GetObjectiveProgress(questid, objectiveid);
            if (progress < QuestRepo.GetObjectiveTargetCount(objectiveid))
            {
                if (mQuestRequirementController.GetObjectiveRequirementStatus(questid, objectiveid) == QuestRequirementStatus.Completed)
                {
                    result = true;
                }
            }
        }
        return result;
    }

    public bool IsQuestCanSubmitByObjective(int questid, int objectiveid)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            if (IsObjectiveAvailable(questid, objectiveid))
            {
                CurrentQuestData questData = GetQuestData(questJson.type, questid);
                if (questData != null)
                {
                    if (IsLastSequenceObjective(questid, questData.GroupdId, questData.MainObjective.SequenceNum))
                    {
                        int remaining = GetRemainingUncompleteObjectiveCount(questData);
                        if (remaining == 1 && !questJson.replyid)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private int GetRemainingUncompleteObjectiveCount(CurrentQuestData questData)
    {
        int count = 0;
        foreach (int objectiveid in questData.MainObjective.ObjectiveIds)
        {
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
            if (objectiveJson != null)
            {
                int progress = GetObjectiveProgress(questData, objectiveid);
                if (!IsObjectiveCompleted(objectiveid, progress))
                {
                    count += 1;
                }
            }
        }

        foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
        {
            foreach (int objectiveid in entry.Value.ObjectiveIds)
            {
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
                if (objectiveJson != null)
                {
                    int progress = GetObjectiveProgress(questData, objectiveid);
                    if (!IsObjectiveCompleted(objectiveid, progress))
                    {
                        count += 1;
                    }
                }
            }
        }

        return count;
    }

    private bool IsLastSequenceObjective(int questid, int groupid, int currentseq)
    {
        Dictionary<int, Dictionary<int, List<string>>> grouplist = QuestRepo.GetQuestObjectiveByQuestId(questid);
        if (grouplist.ContainsKey(groupid))
        {
            int totalseq = grouplist[groupid].Count - 1;
            if (currentseq == totalseq)
            {
                return true;
            }
        }
        return false;
    }

    public void AddNewDialogue(StaticNPCGhost npc, int talkid, int questid, bool ongoing)
    {
        if (ongoing)
        {
            mQuestDialogueController.OpenQuestDialogue(npc, talkid, questid);
        }
        else
        {
            mQuestDialogueController.OpenStartQuestDialogue(npc, talkid, questid);
        }
    }

    public void AddNewDialogue(StaticNPCGhost npc, List<int> questlist, Dictionary<int, int> functionlist, List<int> lockedlist)
    {
        mQuestDialogueController.OpenSelectionDialogue(npc, questlist, functionlist, lockedlist);
    }

    public void AddNewDialogue(StaticNPCGhost npc, bool completedall)
    {
        mQuestDialogueController.OpenCommonDialogue(npc, completedall);
    }

    public int GetObjectiveIdByTargetId(int questid, int targetid)
    {
        int objectiveid = -1;
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson !=null)
        {
            CurrentQuestData questData = GetQuestData(questJson.type, questid);
            if (questData != null)
            {
                objectiveid = GetObjectiveIdByTargetId(questData.MainObjective, targetid);
                if (objectiveid != -1)
                {
                    return objectiveid;
                }
                else
                {
                    foreach(KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
                    {
                        objectiveid = GetObjectiveIdByTargetId(entry.Value, targetid);
                        if (objectiveid != -1)
                        {
                            return objectiveid;
                        }
                    }
                }
            }
        }
        return -1;
    }

    private int GetObjectiveIdByTargetId(CurrentObjectiveData objectiveData, int targetid)
    {
        foreach (int objectiveid in objectiveData.ObjectiveIds)
        {
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
            if (objectiveJson != null && (objectiveJson.type == QuestObjectiveType.Kill || objectiveJson.type == QuestObjectiveType.PercentageKill) && objectiveJson.para1 == targetid)
            {
                return objectiveid;
            }
        }
        return -1;
    }

    public void UpdateCompanionData(string data)
    {
        if (data == "null")
        {
            GameInfo.mCompanionQuestStatus = new Dictionary<int, int>();
        }
        else
        {
            string[] values = data.Split(':');
            if (values.Length >= 2)
            {
                int companionid = -1;
                int questid = -1;
                int.TryParse(values[0], out companionid);
                int.TryParse(values[1], out questid);
                GameInfo.mCompanionQuestStatus = new Dictionary<int, int>();
                GameInfo.mCompanionQuestStatus.Add(questid, companionid);
            }
        }
    }

    public int GetCompanionId()
    {
        foreach (KeyValuePair<int, int> entry in GameInfo.mCompanionQuestStatus)
        {
            return entry.Value;
        }
        return -1;
    }

    public bool IsAdventureLocked(int questid)
    {
        return mLockedAdventureQuest.Contains(questid);
    }

    public QuestRequirementStatus GetQuestRequirementStatus(int questid)
    {
        return mQuestRequirementController.GetTriggerQuestRequirementStatus(questid);
    }

    public string GetRequirementText(int questid)
    {
        return mQuestRequirementController.GetRequirementText(questid);
    }
}
