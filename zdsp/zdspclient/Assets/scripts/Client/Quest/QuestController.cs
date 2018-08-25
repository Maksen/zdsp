using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
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

    //Subline Quest
    private Dictionary<int, CurrentQuestData> mSublineQuest;
    private List<int> mCompletedSublineQuest;

    //Subline Quest
    private Dictionary<int, CurrentQuestData> mGuildQuest;
    private List<int> mCompletedGuildQuest;

    //Signboard Quest
    private Dictionary<int, CurrentQuestData> mSignboardQuest;
    private List<int> mCompletedSignboardQuest;

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
    private int mQuestEventTriggered;
    private List<int> mPendingQuestEventList;
    private Dictionary<int, List<int>> mNPCListByQuestId;
    private Dictionary<QuestType, Dictionary<int, int>> mQuestIdByIndex;
    private bool bInit = false;
    private List<StaticAreaGhost> mTriggerArea;
    private StaticAreaGhost mActivedTrigger;
    private HUD_QuestList mQuestListHUD;

    public QuestClientController(PlayerGhost player)
    {
        mCompletedMainQuest = new List<int>();

        mAdventureQuest = new Dictionary<int, CurrentQuestData>();
        mCompletedAdventureQuest = new List<int>();

        mSublineQuest = new Dictionary<int, CurrentQuestData>();
        mCompletedSublineQuest = new List<int>();

        mGuildQuest = new Dictionary<int, CurrentQuestData>();
        mCompletedGuildQuest = new List<int>();

        mSignboardQuest = new Dictionary<int, CurrentQuestData>();
        mCompletedSignboardQuest = new List<int>();

        mEventQuest = new Dictionary<int, CurrentQuestData>();
        mCompletedEventQuest = new List<int>();

        mTrackingList = new List<int>();
        mWonderfulList = new List<int>();
        mUnlockQuestList = new List<int>();
        mUnlockSignboardList = new List<int>();

        mQuestEventTriggered = -1;
        mPendingQuestEventList = new List<int>();
        mNPCListByQuestId = new Dictionary<int, List<int>>();

        mPlayer = player;
        TrackingListUpdate = false;

        mQuestIdByIndex = new Dictionary<QuestType, Dictionary<int, int>>();
        foreach (QuestType type in Enum.GetValues(typeof(QuestType)))
        {
            mQuestIdByIndex.Add(type, new Dictionary<int, int>());
        }
        mTriggerArea = new List<StaticAreaGhost>();
        mActivedTrigger = null;
    }

    public void Init()
    {
        UpdateAvailableQuest();
        bInit = true;
        UpdateNpcDisplayStatus();
    }

    public void DeserializeData(string field, string data, string olddata)
    {
        if (string.IsNullOrEmpty(data))
            return;

        switch (field)
        {
            case "mainQuest":
                CurrentQuestData questData = DeserializeQuestData(data);
                UpdateQuestData(questData, mMainQuest, QuestType.Main, -1);
                break;
            case "completedMain":
                mCompletedMainQuest = DeserializeCompletedQuest(data);
                UpdateAvailableQuest();
                break;
            case "completedAdventure":
                mCompletedAdventureQuest = DeserializeCompletedQuest(data);
                UpdateAvailableQuest();
                break;
            case "completedSubline":
                mCompletedSublineQuest = DeserializeCompletedQuest(data);
                UpdateAvailableQuest();
                break;
            case "completedGuild":
                mCompletedGuildQuest = DeserializeCompletedQuest(data);
                break;
            case "completedSignboard":
                mCompletedSignboardQuest = DeserializeCompletedQuest(data);
                break;
            case "completedEvent":
                mCompletedEventQuest = DeserializeCompletedQuest(data);
                UpdateAvailableQuest();
                break;
            case "trackingList":
                mTrackingList = DeserializeCompletedQuest(data);
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
            case "signboardQuest":
                UpdateQuestData(questdata, GetOldQuestData(QuestType.Signboard, index), QuestType.Signboard, index);
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
        if (UIManager.IsWindowOpen(WindowType.DialogNpcTalk))
        {
            CloseNpcTalk();
        }

        if (mNPCListByQuestId.ContainsKey(questid))
        {
            mNPCListByQuestId.Remove(questid);
        }

        List<StaticClientNPCAlwaysShow> npclist = ((ClientEntitySystem)GameInfo.gLocalPlayer.EntitySystem).GetVisibleStaticNPC();
        foreach (StaticClientNPCAlwaysShow npc in npclist)
        {
            npc.UpdateAvailableQuestList();
            List<int> questlist = GetQuestListByNPCId(npc.mArchetypeId);
            npc.UpdateOngoingQuest(questlist);
        }

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
        UpdateAvailableQuest();
        ManageNPCQuest(newdata);
        TriggerQuestEvent(newdata);
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

    private void UpdateAvailableQuest()
    {
        List<StaticClientNPCAlwaysShow> npclist = ((ClientEntitySystem)GameInfo.gLocalPlayer.EntitySystem).GetVisibleStaticNPC();
        foreach (StaticClientNPCAlwaysShow npc in npclist)
        {
            npc.UpdateAvailableQuestList();
        }
    }

    private void ManageNPCQuest(CurrentQuestData questData)
    {
        if (questData  == null)
        {
            return;
        }

        bool reordernpc = true;
        QuestStatus status = (QuestStatus)questData.Status;
        if (status == QuestStatus.CompletedAllObjective || status == QuestStatus.CompletedWithEvent)
        {
            reordernpc = false;
        }

        if (questData.MainObjective == null)
        {
            reordernpc = false;
        }

        List<int> npclist = new List<int>();
        if (reordernpc)
        {
            for (int i = 0; i < questData.MainObjective.ObjectiveIds.Count; i++)
            {
                int objectiveid = questData.MainObjective.ObjectiveIds[i];
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
                if (objectiveJson.type == QuestObjectiveType.Talk || objectiveJson.type == QuestObjectiveType.Choice)
                {
                    if (questData.MainObjective.ProgressCount[i] == 0)
                    {
                        npclist.Add(objectiveJson.para1);
                    }
                }
                else if (objectiveJson.type == QuestObjectiveType.Interact)
                {
                    if (questData.MainObjective.ProgressCount[i] < objectiveJson.para2)
                    {
                        npclist.Add(objectiveJson.para3);
                    }
                }
            }

            foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
            {
                for (int i = 0; i < entry.Value.ObjectiveIds.Count; i++)
                {
                    int objectiveid = entry.Value.ObjectiveIds[i];
                    QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
                    if (objectiveJson.type == QuestObjectiveType.Talk || objectiveJson.type == QuestObjectiveType.Choice)
                    {
                        if (entry.Value.ProgressCount[i] == 0)
                        {
                            npclist.Add(objectiveJson.para1);
                        }
                    }
                    else if (objectiveJson.type == QuestObjectiveType.Interact)
                    {
                        if (entry.Value.ProgressCount[i] < objectiveJson.para2)
                        {
                            npclist.Add(objectiveJson.para3);
                        }
                    }
                }
            }
        }

        List<int> oldnpclist = new List<int>();
        if (mNPCListByQuestId.ContainsKey(questData.QuestId))
        {
            oldnpclist = mNPCListByQuestId[questData.QuestId];
            mNPCListByQuestId[questData.QuestId] = npclist;
        }
        else
        {
            mNPCListByQuestId.Add(questData.QuestId, npclist);
        }

        if (GameInfo.gLocalPlayer != null)
        {
            foreach (int npcid in npclist)
            {
                if (oldnpclist.Contains(npcid))
                {
                    oldnpclist.Remove(npcid);
                }
                StaticClientNPCAlwaysShow staticnpc = ((ClientEntitySystem)GameInfo.gLocalPlayer.EntitySystem).GetStaticClientNPC(npcid);
                if (staticnpc != null)
                {
                    List<int> questlist = GetQuestListByNPCId(npcid);
                    staticnpc.UpdateOngoingQuest(questlist);
                }
            }

            foreach (int npcid in oldnpclist)
            {
                StaticClientNPCAlwaysShow staticnpc = ((ClientEntitySystem)GameInfo.gLocalPlayer.EntitySystem).GetStaticClientNPC(npcid);
                if (staticnpc != null)
                {
                    staticnpc.RemoveOngoingQuest(questData.QuestId);
                }
            }
        }
    }

    public List<int> GetQuestListByNPCId(int npcid)
    {
        List<int> questlist = new List<int>();
        foreach(KeyValuePair<int, List<int>> entry in mNPCListByQuestId)
        {
            if (entry.Value.Contains(npcid))
            {
                questlist.Add(entry.Key);
            }
        }
        return questlist;
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

    public bool CheckIsInteractNpc(int objectiveid, int npcid)
    {
        QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
        if (objectiveJson == null)
        {
            return false;
        }

        if (objectiveJson.type == QuestObjectiveType.Talk || objectiveJson.type == QuestObjectiveType.Choice)
        {
            if (objectiveJson.para1 == npcid)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsQuestAvailable(int questid)
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

        List<QuestRequirementDetailJson> requirements = QuestRepo.GetRequirementByGroupId(questJson.requirementid);
        if (requirements == null)
        {
            return false;
        }
        else
        {
            foreach(QuestRequirementDetailJson requirement in requirements)
            {
                if (!CheckRequirement(requirement))
                {
                    return false;
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
                if (requirement.para2 == 1 && requirement.para1 == mPlayer.PlayerSynStats.QuestCompanionId)
                {
                    return true;
                }
                else if (requirement.para2 == 2 && requirement.para1 != mPlayer.PlayerSynStats.QuestCompanionId)
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
        CurrentQuestData questData = GetQuestData(questJson.type, questid);
        Dictionary<int, Dictionary<int, List<string>>> grouplist = QuestRepo.GetQuestObjectiveByQuestId(questid);
        if (questData != null)
        {
            if (grouplist.ContainsKey(questData.GroupdId))
            {
                int totalseq = grouplist[questData.GroupdId].Count - 1;
                if (questData.MainObjective.SequenceNum == totalseq && questJson.replyid)
                {
                    if (mNPCListByQuestId.ContainsKey(questid))
                    {
                        List<int> npclist = mNPCListByQuestId[questid];
                        if (npclist.Count == 1)
                        {
                            return CanCompleteThroughNPC(questData);
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool CanCompleteThroughNPC(CurrentQuestData questData)
    {
        List<CurrentObjectiveData> objectiveDatas = new List<CurrentObjectiveData>();
        objectiveDatas.Add(questData.MainObjective);
        foreach(KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
        {
            objectiveDatas.Add(entry.Value);
        }
        return CanObjectivesBeComplete(objectiveDatas);
    }

    private bool CanObjectivesBeComplete(List<CurrentObjectiveData> objectiveDatas)
    {
        Dictionary<CurrentObjectiveData, List<int>> interactid = new Dictionary<CurrentObjectiveData, List<int>>();
        foreach(CurrentObjectiveData objectiveData in objectiveDatas)
        {
            for (int i = 0; i < objectiveData.ObjectiveIds.Count; i++)
            {
                int objectiveid = objectiveData.ObjectiveIds[i];
                int progress = objectiveData.ProgressCount[i];
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
                if (objectiveJson.type == QuestObjectiveType.Choice || objectiveJson.type == QuestObjectiveType.Talk)
                {
                    if (!interactid.ContainsKey(objectiveData))
                    {
                        interactid.Add(objectiveData, new List<int>());
                    }
                    interactid[objectiveData].Add(i);
                }
                else
                {
                    if (!IsObjectiveCompleted(objectiveid, progress))
                    {
                        return false;
                    }
                }
            }
        }

        if (interactid.Count ==0 )
        {
            return false;
        }
        else if (interactid.Count > 1)
        {
            int total = 0;
            int completion = 0;
            foreach(KeyValuePair<CurrentObjectiveData, List<int>> entry in interactid)
            {
                foreach(int id in entry.Value)
                {
                    int objectiveid = entry.Key.ObjectiveIds[id];
                    int progress = entry.Key.ProgressCount[id];
                    QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
                    total += 1;
                    if (IsObjectiveCompleted(objectiveid, progress))
                    {
                        completion += 1;
                    }
                }
            }
            if (completion + 1 != total)
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckObjectiveRequirement(int requirementid)
    {
        int count = 0;
        List<QuestRequirementDetailJson> requirements = QuestRepo.GetRequirementByGroupId(requirementid);
        if (requirements != null)
        {
            foreach (QuestRequirementDetailJson requirement in requirements)
            {
                if (CheckRequirement(requirement))
                {
                    count += 1;
                }
            }
            if (count < requirements.Count)
            {
                return false;
            }
        }
        return true;
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

    private int GetMainObjectiveProgress(QuestType type, int questid, int objectiveid)
    {
        CurrentQuestData currentQuest = null;
        switch (type)
        {
            case QuestType.Main:
                if (mMainQuest.QuestId == questid)
                {
                    currentQuest = mMainQuest;
                }
                break;
            case QuestType.Destiny:
                currentQuest = mAdventureQuest[questid];
                break;
            case QuestType.Sub:
                currentQuest = mSublineQuest[questid];
                break;
            case QuestType.Guild:
                currentQuest = mGuildQuest[questid];
                break;
            case QuestType.Signboard:
                currentQuest = mSignboardQuest[questid];
                break;
            case QuestType.Event:
                currentQuest = mEventQuest[questid];
                break;
        }

        if (currentQuest != null)
        {
            for (int i = 0; i < currentQuest.MainObjective.ObjectiveIds.Count; i++)
            {
                if (currentQuest.MainObjective.ObjectiveIds[i] == objectiveid)
                {
                    return currentQuest.MainObjective.ProgressCount[i];
                }
            }
        }
        return 0;
    }

    private int GetSubObjectiveProgress(QuestType type, int questid, int mainid, int objectiveid)
    {
        CurrentQuestData currentQuest = null;
        switch (type)
        {
            case QuestType.Main:
                if (mMainQuest.QuestId == questid)
                {
                    currentQuest = mMainQuest;
                }
                break;
            case QuestType.Destiny:
                currentQuest = mAdventureQuest[questid];
                break;
            case QuestType.Sub:
                currentQuest = mSublineQuest[questid];
                break;
            case QuestType.Guild:
                currentQuest = mGuildQuest[questid];
                break;
            case QuestType.Signboard:
                currentQuest = mSignboardQuest[questid];
                break;
            case QuestType.Event:
                currentQuest = mEventQuest[questid];
                break;
        }

        if (currentQuest != null)
        {
            for (int i = 0; i < currentQuest.SubObjective[mainid].ObjectiveIds.Count; i++)
            {
                if (currentQuest.SubObjective[mainid].ObjectiveIds[i] == objectiveid)
                {
                    return currentQuest.SubObjective[mainid].ProgressCount[i];
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
                progress = GetMainObjectiveProgress(type, questid, objectiveid);
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
                    progresscount[i] = GetSubObjectiveProgress(type, questid, mainid, subid[i]);
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
        string replacement = "%r_" + requirementDetailJson.id + "%";
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
                if (requirementDetailJson.para2 == 1 && mPlayer.PlayerSynStats.QuestCompanionId == requirementDetailJson.para1)
                {
                    return 1;
                }
                else if (requirementDetailJson.para2 == 2 && mPlayer.PlayerSynStats.QuestCompanionId != requirementDetailJson.para1)
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

    public Dictionary<int, long> GetMainObjectiveEndtime(CurrentQuestData questData)
    {
        Dictionary<int, long> completetimes = new Dictionary<int, long>();
        if (questData.MainObjective.CompleteTime.Count > 0)
        {
            for (int i = 0; i < questData.MainObjective.ObjectiveIds.Count; i++)
            {
                if (questData.MainObjective.CompleteTime[i] > 0)
                {
                    completetimes.Add(questData.MainObjective.ObjectiveIds[i], questData.MainObjective.CompleteTime[i]);
                }
            }
        }

        return completetimes;
    }

    public Dictionary<int, long> GetSubObjectiveEndtime(CurrentQuestData questData)
    {
        Dictionary<int, long> completetimes = new Dictionary<int, long>();
        foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
        {
            if (entry.Value.CompleteTime.Count > 0)
            {
                for (int i = 0; i < entry.Value.ObjectiveIds.Count; i++)
                {
                    if (entry.Value.CompleteTime[i] > 0)
                    {
                        completetimes.Add(entry.Value.ObjectiveIds[i], entry.Value.CompleteTime[i]);
                    }
                }
            }
        }

        return completetimes;
    }

    public string ReplaceEndTime(string description, Dictionary<int, long> mainendtime, Dictionary<int, long> subendtime)
    {
        long currenttime = GameInfo.GetSynchronizedTime();
        if (mainendtime.Count >= 1)
        {
            long mainET = mainendtime.Values.First();
            long remaintime = mainET - currenttime;
            string time = "0";
            if (remaintime > 0)
            {
                time = GUILocalizationRepo.GetShortLocalizedTimeString(remaintime);
            }
            description = description.Replace("%t%", time);
        }
        if (subendtime.Count >= 1)
        {
            foreach (KeyValuePair<int, long> entry in subendtime)
            {
                long remaintime = entry.Value - currenttime;
                string replacement = "%t_" + entry.Key + "%";
                string time = "0";
                if (remaintime > 0)
                {
                    time = GUILocalizationRepo.GetShortLocalizedTimeString(remaintime);
                    
                }
                description = description.Replace(replacement, time);
            }
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

        if (questData.Status == (byte)QuestStatus.NewQuestWithEvent || questData.Status == (byte)QuestStatus.NewObjectiveWithEvent || questData.Status == (byte)QuestStatus.CompletedWithEvent)
        {
            if (mQuestEventTriggered != -1)
            {
                mPendingQuestEventList.Add(questData.QuestId);
            }
            else
            {
                mQuestEventTriggered = questData.QuestId;
                ActiveQuestEvent();
            }
        }
        else
        {
            AutoProceed(questData);
        }
    }

    private void ActiveQuestEvent()
    {
        QuestType questType = QuestRepo.GetQuestTypeByQuestId(mQuestEventTriggered);
        CurrentQuestData questData = GetQuestData(questType, mQuestEventTriggered);
        int objectiveid = -1;
        if (questData != null)
        {
            int eventgroupid = -1;
            EventTimingType timingType = EventTimingType.CompleteObjective;
            if (questData.Status == (byte)QuestStatus.NewQuestWithEvent)
            {
                QuestJson questJson = QuestRepo.GetQuestByID(mQuestEventTriggered);
                eventgroupid = questJson.eventid;
                timingType = EventTimingType.StartQuest;
            }
            else if (questData.Status == (byte)QuestStatus.CompletedWithEvent)
            {
                QuestJson questJson = QuestRepo.GetQuestByID(mQuestEventTriggered);
                eventgroupid = questJson.eventid;
                timingType = EventTimingType.CompleteQuest;
            }
            else if (questData.Status == (byte)QuestStatus.NewObjectiveWithEvent)
            {
                eventgroupid = GetEventId(questData, out objectiveid);
                timingType = EventTimingType.CompleteObjective;
            }

            if (eventgroupid != -1)
            {
                TriggerQuestEventByType(eventgroupid, timingType, objectiveid);
            }
        }
    }

    private int GetEventId(CurrentQuestData questData, out int objectiveid)
    {
        foreach(int objid in questData.MainObjective.ObjectiveIds)
        {
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objid);
            if (objectiveJson != null && objectiveJson.eventid > 0)
            {
                objectiveid = objid;
                return objectiveJson.eventid;
            }
        }
        objectiveid = -1;
        return -1;
    }

    private void TriggerQuestEventByType(int eventgroupid, EventTimingType timingType, int objectiveid)
    {
        bool canstartnextevent = false;
        QuestEventDetailJson questEvent = QuestRepo.GetQuestEvent(eventgroupid, timingType, objectiveid);
        if (questEvent != null)
        {
            switch(questEvent.type)
            {
                case QuestEventType.Cutscene:
                    int delay = 0;
                    int.TryParse(questEvent.para2, out delay);
                    GameInfo.gCombat.StartCoroutine(mPlayer.PlayCutscene(questEvent.para1, delay, mQuestEventTriggered));
                    RPCFactory.NonCombatRPC.UpdateQuestStatus(mQuestEventTriggered);
                    canstartnextevent = true;
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
                            RPCFactory.NonCombatRPC.ConsoleSpawnPersonalMonster(npcJson.archetype, -1, aggressive, mQuestEventTriggered);
                        }
                    }
                    break;
                case QuestEventType.Teleport:
                    int x = 0, y = 0, z = 0;
                    if (!string.IsNullOrEmpty(questEvent.para2))
                    {
                        string[] pos = questEvent.para2.Split(',');
                        for (int i = 0; i < pos.Length; i++)
                        {
                            if (i ==0)
                            {
                                int.TryParse(pos[i], out x);
                            }
                            else if (i == 1)
                            {
                                int.TryParse(pos[i], out y);
                            }
                            else if(i == 2)
                            {
                                int.TryParse(pos[i], out z);
                            }
                        }
                    }
                    Vector3 position = new Vector3(x, y, z);
                    RPCFactory.CombatRPC.OnTeleportToLevelAndPos(questEvent.para1, position.ToRPCPosition(), mQuestEventTriggered);
                    canstartnextevent = true;
                    break;
                case QuestEventType.SideEffect:
                    int seid = -1;
                    int.TryParse(questEvent.para1, out seid);
                    if (GameInfo.mSideEffectQuestStatus.Count > 0)
                    {
                        foreach(KeyValuePair<int, int> entry in GameInfo.mSideEffectQuestStatus)
                        {
                            RPCFactory.CombatRPC.RemoveSideBuff(entry.Value);
                        }
                        GameInfo.mSideEffectQuestStatus = new Dictionary<int, int>();
                    }
                    GameInfo.mSideEffectQuestStatus.Add(mQuestEventTriggered, seid);
                    RPCFactory.NonCombatRPC.ApplyQuestEventBuff(questEvent.id, mQuestEventTriggered);
                    break;
                case QuestEventType.Companion:
                    int companionid = -1;
                    int.TryParse(questEvent.para1, out companionid);
                    if (GameInfo.mCompanionQuestStatus.Count > 0)
                    {
                        foreach (KeyValuePair<int, int> entry in GameInfo.mCompanionQuestStatus)
                        {
                            RPCFactory.NonCombatRPC.ResetQuestEventCompanion(entry.Value);
                        }
                        GameInfo.mCompanionQuestStatus = new Dictionary<int, int>();
                    }
                    GameInfo.mCompanionQuestStatus.Add(mQuestEventTriggered, companionid);
                    RPCFactory.NonCombatRPC.ApplyQuestEventCompanion(questEvent.id, mQuestEventTriggered);
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
                            if (!GameInfo.mNpcQuestStatus[npcid].Contains(mQuestEventTriggered))
                            {
                                GameInfo.mNpcQuestStatus[npcid].Add(mQuestEventTriggered);
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
                    RPCFactory.NonCombatRPC.UpdateQuestStatus(mQuestEventTriggered);
                    canstartnextevent = true;
                    break;
            }
            if (!string.IsNullOrEmpty(questEvent.msg))
            {
                UIManager.ShowSystemMessage(questEvent.msg);
            }
        }

        if (canstartnextevent)
        {
            StartNextQuestEvent();
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
        mQuestEventTriggered = -1;
        if (mPendingQuestEventList.Count > 0)
        {
            mQuestEventTriggered = mPendingQuestEventList[0];
            mPendingQuestEventList.Remove(mQuestEventTriggered);
            ActiveQuestEvent();
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
    #endregion

    private void AutoProceed(CurrentQuestData questData)
    {
        QuestStatus status = (QuestStatus)questData.Status;
        if (status == QuestStatus.NewObjective || status == QuestStatus.NewQuest)
        {
            int talkid = GetTalkId(questData.QuestId, -1);
            if (talkid != -1)
            {
                if (UIManager.IsWindowOpen(WindowType.DialogNpcTalk))
                {
                    UIManager.GetWindowGameObject(WindowType.DialogNpcTalk).GetComponent<UI_Dialogue>().UpdateTalkId(talkid);
                }
                else
                {
                    UIManager.CloseAllWindows();
                    UIManager.OpenDialog(WindowType.DialogNpcTalk, (window) => window.GetComponent<UI_Dialogue>().Init(-1, talkid, questData.QuestId, true));
                }
            }
            else
            {
                if (UIManager.IsWindowOpen(WindowType.DialogNpcTalk))
                {
                    CloseNpcTalk();
                }
            }
            AutoProceedObjective(questData);
        }
        else if (status == QuestStatus.CompletedAllObjective)
        {
            QuestJson questJson = QuestRepo.GetQuestByID(questData.QuestId);
            if (questJson != null)
            {
                RPCFactory.NonCombatRPC.CompleteQuest(questData.QuestId, questJson.replyid);
            }
        }
        else
        {
            if (UIManager.IsWindowOpen(WindowType.DialogNpcTalk))
            {
                CloseNpcTalk();
            }
        }
    }

    private void AutoProceedObjective(CurrentQuestData questData)
    {
        List<int> objectivelist = new List<int>();

        foreach(int mainid in questData.MainObjective.ObjectiveIds)
        {
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(mainid);
            if (objectiveJson != null && objectiveJson.type == QuestObjectiveType.Empty)
            {
                objectivelist.Add(mainid);
            }
        }

        foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
        {
            foreach (int subid in entry.Value.ObjectiveIds)
            {
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(subid);
                if (objectiveJson != null && objectiveJson.type == QuestObjectiveType.Empty)
                {
                    objectivelist.Add(subid);
                }
            }
        }
        
        if (objectivelist.Count > 0)
        {
            RPCFactory.NonCombatRPC.SubmitEmptyObjective(questData.QuestId);
        }
    }

    public void CloseNpcTalk()
    {
        UIManager.CloseDialog(WindowType.DialogNpcTalk);
    }

    public void UpdateTriggerData(StaticAreaGhost staticArea)
    {
        if (mActivedTrigger == staticArea)
        {
            int interactid = mActivedTrigger.GetInteractId();
            if (interactid != -1)
            {
                int questid = mActivedTrigger.GetActivedQuestId();
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
    }

    public void OnEnterStaticArea(StaticAreaGhost staticArea)
    {
        if (!mTriggerArea.Contains(staticArea))
        {
            mTriggerArea.Add(staticArea);
        }
        OnTriggerChanged();
    }

    public void OnExitStaticArea(StaticAreaGhost staticArea)
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
            int interactid = mActivedTrigger.GetInteractId();
            int questid = mActivedTrigger.GetActivedQuestId();
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

    public void ProcessObjectiveHyperLink(string linkinfo, int questid)
    {
        string[] objectiveinfo = linkinfo.Split(';');
        int targetid;
        byte type;
        int.TryParse(objectiveinfo[1], out targetid);
        byte.TryParse(objectiveinfo[0], out type);
        ProceedQuestObjective((QuestObjectiveType)type, targetid, questid);
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
                    ProceedToQuestTarget(targetid, questid, questJson.teleport, false, (byte)type);
                    break;
                case QuestObjectiveType.Talk:
                case QuestObjectiveType.Choice:
                case QuestObjectiveType.Interact:
                    ProceedToQuestTarget(targetid, questid, questJson.teleport, true, (byte)type);
                    break;
                case QuestObjectiveType.QuickTalk:
                    CurrentQuestData questData = GetQuestData(questJson.type, questid);
                    if (questData != null)
                    {
                        AutoProceed(questData);
                    }
                    break;
            }
        }
    }

    public void ProceedQuestTrigger(QuestTriggerType type, int targetid, int questid)
    {
        switch(type)
        {
            case QuestTriggerType.NPC:
                ProceedToQuestTarget(targetid, questid, false, true, (byte)type, false);
                break;
            case QuestTriggerType.Interact:
                break;
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
            StaticNPCJson staticNPCJson = StaticNPCRepo.GetNPCById(targetId);
            if (staticNPCJson != null)
                foundtarget = NPCPosMap.FindNearestStaticNPC(staticNPCJson.archetype, levelName, mPlayer.Position, 
                    ref targetlevel, ref targetpos);
        }

        if (!foundtarget)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CannotFindTarget"));
            return;
        }
        else
        {
            mPlayer.ForceIdle();
            mPlayer.Bot.StopBot();

            if (levelName == targetlevel)
            {
                if (isCombat)
                {
                    mPlayer.PathFindToTarget(targetpos, -1, 0, false, true, () => {
                        if (GameSettings.AutoBotEnabled)
                            mPlayer.Bot.StartBot();
                    });
                }
                else
                    mPlayer.ProceedToTarget(targetpos, targetId, CallBackAction.Interact);
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
                        BotController.DestMonsterOrNPC = isCombat ? ReachTargetAction.StartBot : ReachTargetAction.NPC_Interact;
                        BotController.DestArchtypeID = isCombat ? -1 : targetId;
                        GameInfo.gLocalPlayer.Bot.SeekingWithRouter();
                    }
                    else
                        UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CannotFindTarget"));
                }
            }
        }
    }

    public bool HaveMultipleTarget(CurrentQuestData questData)
    {
        List<int> objectivelist = new List<int>();
        objectivelist.AddRange(questData.MainObjective.ObjectiveIds);
        foreach(KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
        {
            objectivelist.AddRange(entry.Value.ObjectiveIds);
        }

        if (objectivelist.Count <= 1)
        {
            return false;
        }
        else
        {
            int targetcount = 0;
            foreach(int objectiveid in objectivelist)
            {
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
                if (objectiveJson != null)
                {
                    if (objectiveJson.type != QuestObjectiveType.Empty && objectiveJson.type != QuestObjectiveType.MultipleObj && objectiveJson.type != QuestObjectiveType.RealmComplete)
                    {
                        targetcount += 1;
                    }
                }
            }
            return targetcount > 1 ? true : false;
        }
    }

    public bool GetObjectiveTarget(CurrentQuestData questData, out int targetid, out QuestObjectiveType type)
    {
        List<int> objectivelist = new List<int>();
        objectivelist.AddRange(questData.MainObjective.ObjectiveIds);
        foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
        {
            objectivelist.AddRange(entry.Value.ObjectiveIds);
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

    private void DisplayKillObjectiveMsg(CurrentQuestData newQuestData, CurrentQuestData oldQuestData)
    {
        if (oldQuestData != null)
        {
            List<int> oldobjectivelist = new List<int>();
            List<int> oldprogresslist = new List<int>();
            oldobjectivelist.AddRange(oldQuestData.MainObjective.ObjectiveIds);
            oldprogresslist.AddRange(oldQuestData.MainObjective.ProgressCount);
            foreach (KeyValuePair<int, CurrentObjectiveData> entry in oldQuestData.SubObjective)
            {
                oldobjectivelist.AddRange(entry.Value.ObjectiveIds);
                oldprogresslist.AddRange(entry.Value.ProgressCount);
            }

            Dictionary<int, int> olddata = new Dictionary<int, int>();
            for (int i = 0; i < oldobjectivelist.Count; i++)
            {
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(oldobjectivelist[i]);
                if (objectiveJson != null && (objectiveJson.type == QuestObjectiveType.Kill || objectiveJson.type == QuestObjectiveType.PercentageKill))
                {
                    olddata.Add(oldobjectivelist[i], oldprogresslist[i]);
                }
            }

            foreach (KeyValuePair<int, int> entry in olddata)
            {
                UIManager.ShowSystemMessage(GetKillObjectiveDescription(entry.Key, entry.Value + 1));
            }
        }
    }
}
