using System.Collections.Generic;
using Zealot.Common.Entities;
using Zealot.Client.Entities;
using Zealot.Repository;
using Kopio.JsonContracts;
using Zealot.Common;
using System;
using System.Linq;

public class QuestObjectiveTriggerData
{
    public int QuestId;
    public int ObjectiveId;

    public QuestObjectiveTriggerData(int questid, int objectiveid)
    {
        QuestId = questid;
        ObjectiveId = objectiveid;
    }
}

public class QuestTriggerController
{
    //list for accept quest
    private Dictionary<int, List<int>> mLockedListByNpc;
    private Dictionary<int, List<int>> mAvailableListByNpc;
    private Dictionary<int, List<int>> mUnavailableListByNpc;
    private Dictionary<int, int> mNpcIdByQuestId;

    //list for doing quest
    private Dictionary<int, Dictionary<int, List<QuestObjectiveTriggerData>>> mOngoingListByNpc;
    private Dictionary<int, Dictionary<int, List<QuestObjectiveTriggerData>>> mUncompleteListByNpc;
    private Dictionary<int, Dictionary<int, int>> mNpcListByQuestId;
    private Dictionary<int, List<int>> mQuestGroupProgress;

    //npc list for refresh
    private List<int> mNpcList;

    //list for empty objective list
    private Dictionary<int, List<int>> mOngoingEmptyObjective;

    private QuestClientController mQuestController;
    private QuestRequirementController mRequirementController;
    private ClientEntitySystem mEntitySystem;

    public QuestTriggerController(QuestClientController questController, QuestRequirementController requirementController, ClientEntitySystem entitySystem)
    {
        mQuestController = questController;
        mRequirementController = requirementController;
        mEntitySystem = entitySystem;
        mLockedListByNpc = new Dictionary<int, List<int>>();
        mAvailableListByNpc = new Dictionary<int, List<int>>();
        mUnavailableListByNpc = new Dictionary<int, List<int>>();
        mNpcIdByQuestId = new Dictionary<int, int>();

        mOngoingListByNpc = new Dictionary<int, Dictionary<int, List<QuestObjectiveTriggerData>>>();
        mUncompleteListByNpc = new Dictionary<int, Dictionary<int, List<QuestObjectiveTriggerData>>>();
        mNpcListByQuestId = new Dictionary<int, Dictionary<int, int>>();
        mQuestGroupProgress = new Dictionary<int, List<int>>();

        mNpcList = new List<int>();

        mOngoingEmptyObjective = new Dictionary<int, List<int>>();

        InitAvailableList(entitySystem);
        InitOngoingQuest();
        CheckEmptyObjectiveList();
        RefreshNPCForQuest();
    }

    private void InitAvailableList(ClientEntitySystem entitySystem)
    {
        List<StaticClientNPCAlwaysShow> npclist = entitySystem.GetAllQuestRelatedNPC();
        foreach (StaticClientNPCAlwaysShow npc in npclist)
        {
            int npcid = npc.mArchetypeId;
            StaticNPCJson staticNPC = StaticNPCRepo.GetNPCById(npcid);
            if (staticNPC != null)
            {
                List<int> questlist = new List<int>();
                string[] ids = staticNPC.questid.Split(';');
                foreach (string id in ids)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        questlist.Add(int.Parse(id));
                    }
                }

                foreach(int questid in questlist)
                {
                    mRequirementController.AddTriggerQuestRequirement(questid, GameInfo.gLocalPlayer);
                    AddToNpcTrackList(npcid, questid);
                    CheckAvailableQuest(npcid, questid);
                }
                AddRefreshNPC(npcid);
            }
        }
    }

    private void CheckAvailableQuest(int npcid, int questid)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            bool ongoing = mQuestController.IsQuestOngoing(questJson.type, questJson.questid);
            bool available = mQuestController.IsQuestAvailable(questJson.questid);
            bool completed = mQuestController.IsQuestCompleted(questJson.questid);

            if (available && !completed)
            {
                AddToAvailableList(npcid, questJson.questid);
                RemoveFromUnavailableList(npcid, questJson.questid);
                RemoveFromLockedList(npcid, questJson.questid);
            }
            else if (ongoing)
            {
                AddToUnavailableList(npcid, questJson.questid);
                RemoveFromAvailableList(npcid, questJson.questid);
                RemoveFromLockedList(npcid, questJson.questid);
            }
            else
            {
                AddToLockedList(npcid, questJson.questid);
                RemoveFromAvailableList(npcid, questJson.questid);
                RemoveFromUnavailableList(npcid, questJson.questid);
            }
        }
    }

    private void AddToNpcTrackList(int npcid, int questid)
    {
        if (mNpcIdByQuestId.ContainsKey(questid))
        {
            mNpcIdByQuestId[questid] = npcid;
        }
        else
        {
            mNpcIdByQuestId.Add(questid, npcid);
        }
    }

    private int GetNpcIdByQuestId(int questid)
    {
        if (mNpcIdByQuestId.ContainsKey(questid))
        {
            return mNpcIdByQuestId[questid];
        }
        return -1;
    }

    private void AddToAvailableList(int npcid, int questid)
    {
        if (!mAvailableListByNpc.ContainsKey(npcid))
        {
            mAvailableListByNpc.Add(npcid, new List<int>());
        }

        if (!mAvailableListByNpc[npcid].Contains(questid))
        {
            mAvailableListByNpc[npcid].Add(questid);
        }
    }

    private void RemoveFromAvailableList(int npcid, int questid)
    {
        if (mAvailableListByNpc.ContainsKey(npcid))
        {
            mAvailableListByNpc[npcid].Remove(questid);
        }
    }

    private void AddToUnavailableList(int npcid, int questid)
    {
        if (!mUnavailableListByNpc.ContainsKey(npcid))
        {
            mUnavailableListByNpc.Add(npcid, new List<int>());
        }

        if (!mUnavailableListByNpc[npcid].Contains(questid))
        {
            mUnavailableListByNpc[npcid].Add(questid);
        }
    }

    private void RemoveFromUnavailableList(int npcid, int questid)
    {
        if (mUnavailableListByNpc.ContainsKey(npcid))
        {
            mUnavailableListByNpc[npcid].Remove(questid);
        }
    }

    private void AddToLockedList(int npcid, int questid)
    {
        if (!mLockedListByNpc.ContainsKey(npcid))
        {
            mLockedListByNpc.Add(npcid, new List<int>());
        }

        if (!mLockedListByNpc[npcid].Contains(questid))
        {
            mLockedListByNpc[npcid].Add(questid);
        }
    }

    private void RemoveFromLockedList(int npcid, int questid)
    {
        if (mLockedListByNpc.ContainsKey(npcid))
        {
            mLockedListByNpc[npcid].Remove(questid);
        }
    }

    private void AddToOngoingList(int npcid, int questid, int objectiveid)
    {
        if (!mOngoingListByNpc.ContainsKey(npcid))
        {
            mOngoingListByNpc.Add(npcid, new Dictionary<int, List<QuestObjectiveTriggerData>>());
        }

        if (!mOngoingListByNpc[npcid].ContainsKey(questid))
        {
            mOngoingListByNpc[npcid].Add(questid, new List<QuestObjectiveTriggerData>());
        }

        QuestObjectiveTriggerData objectiveTriggerData = new QuestObjectiveTriggerData(questid, objectiveid);
        mOngoingListByNpc[npcid][questid].Add(objectiveTriggerData);
    }

    private void RemoveFromOngoingList(int npcid, int questid)
    {
        if (mOngoingListByNpc.ContainsKey(npcid))
        {
            mOngoingListByNpc[npcid].Remove(questid);
        }
    }

    private void RemoveFromOngoingList(int npcid, int questid, int objectiveid)
    {
        if (mOngoingListByNpc.ContainsKey(npcid))
        {
            if (mOngoingListByNpc[npcid].ContainsKey(questid))
            {
                QuestObjectiveTriggerData objectiveTriggerData = mOngoingListByNpc[npcid][questid].Where(o => o.QuestId == questid && o.ObjectiveId == objectiveid).First();
                mOngoingListByNpc[npcid][questid].Remove(objectiveTriggerData);
            }
            mOngoingListByNpc[npcid].Remove(questid);
        }
    }

    private void AddToUncompletList(int npcid, int questid, int objectiveid)
    {
        if (!mUncompleteListByNpc.ContainsKey(npcid))
        {
            mUncompleteListByNpc.Add(npcid, new Dictionary<int, List<QuestObjectiveTriggerData>>());
        }

        if (!mUncompleteListByNpc[npcid].ContainsKey(questid))
        {
            mUncompleteListByNpc[npcid].Add(questid, new List<QuestObjectiveTriggerData>());
        }

        QuestObjectiveTriggerData objectiveTriggerData = new QuestObjectiveTriggerData(questid, objectiveid);
        mUncompleteListByNpc[npcid][questid].Add(objectiveTriggerData);
    }

    private void RemoveFromUncompleteList(int npcid, int questid)
    {
        if (mUncompleteListByNpc.ContainsKey(npcid))
        {
            mUncompleteListByNpc[npcid].Remove(questid);
        }
    }

    private void RemoveFromUncompleteList(int npcid, int questid, int objectiveid)
    {
        if (mUncompleteListByNpc.ContainsKey(npcid))
        {
            if (mUncompleteListByNpc[npcid].ContainsKey(questid))
            {
                QuestObjectiveTriggerData objectiveTriggerData = mUncompleteListByNpc[npcid][questid].Where(o => o.QuestId == questid && o.ObjectiveId == objectiveid).First();
                mUncompleteListByNpc[npcid][questid].Remove(objectiveTriggerData);
            }
            mUncompleteListByNpc[npcid].Remove(questid);
        }
    }

    private void AddToObjectiveNpcTrackList(int npcid, int questid, int objectiveid)
    {
        if (!mNpcListByQuestId.ContainsKey(questid))
        {
            mNpcListByQuestId.Add(questid, new Dictionary<int, int>());
        }
        else
        {
            Dictionary<int, int> questlist = mNpcListByQuestId[questid];
            foreach (KeyValuePair<int, int> entry in questlist)
            {
                int oldnpcid = entry.Value;
                CleanNpcTrackList(oldnpcid, questid);
            }
            mNpcListByQuestId[questid] = new Dictionary<int, int>();
        }

        if (!mNpcListByQuestId[questid].ContainsKey(objectiveid))
        {
            mNpcListByQuestId[questid].Add(objectiveid, npcid);
        }
        else
        {
            mNpcListByQuestId[questid][objectiveid] = npcid;
        }
    }

    private void RemoveQuestTrackList(int questid)
    {
        if (mNpcListByQuestId.ContainsKey(questid))
        {
            mNpcListByQuestId.Remove(questid);
        }
    }

    private void CleanNpcTrackList(int npcid, int questid)
    {
        RemoveFromOngoingList(npcid, questid);
        RemoveFromUncompleteList(npcid, questid);
        AddRefreshNPC(npcid);
    }

    private Dictionary<int, int> GetNpcTrackList(int questid)
    {
        if (mNpcListByQuestId.ContainsKey(questid))
        {
            return mNpcListByQuestId[questid];
        }
        return new Dictionary<int, int>();
    }

    private int GetNpcIdFromNpcTrackList(int questid, int objectiveid)
    {
        if (mNpcListByQuestId.ContainsKey(questid))
        {
            if (mNpcListByQuestId[questid].ContainsKey(objectiveid))
            {
                return mNpcListByQuestId[questid][objectiveid];
            }
        }
        return -1;
    }

    private void RemoveFromAllAvailableList(int npcid, int questid)
    {
        RemoveFromAvailableList(npcid, questid);
        AddToUnavailableList(npcid, questid);
        RemoveFromLockedList(npcid, questid);
    }

    private void AddRefreshNPC(int npcid)
    {
        if (!mNpcList.Contains(npcid))
        {
            mNpcList.Add(npcid);
        }
    }

    private bool AddQuestGroupProgress(CurrentQuestData questData)
    {
        List<int> objectivelist = new List<int>();
        objectivelist.AddRange(questData.MainObjective.ObjectiveIds);
        foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
        {
            objectivelist.AddRange(entry.Value.ObjectiveIds);
        }

        if (!mQuestGroupProgress.ContainsKey(questData.QuestId))
        {
            mQuestGroupProgress.Add(questData.QuestId, objectivelist);
            return true;
        }
        else
        {
            List<int> oldlist = mQuestGroupProgress[questData.QuestId];
            if (objectivelist.SequenceEqual(oldlist))
            {
                return false;
            }
            else
            {
                mQuestGroupProgress[questData.QuestId] = objectivelist;
                return true;
            }
        }
    }

    private void RemoveQuestGroupProgress(int questid)
    {
        if (mQuestGroupProgress.ContainsKey(questid))
        {
            mQuestGroupProgress.Remove(questid);
        }
    }

    private List<int> GetAvailableListByNpcId(int npcid)
    {
        if (mAvailableListByNpc.ContainsKey(npcid))
        {
            return mAvailableListByNpc[npcid];
        }
        return new List<int>();
    }

    private Dictionary<int, int> GetOngoingListByNpcId(int npcid)
    {
        Dictionary<int, int> questlist = new Dictionary<int, int>();
        if (mOngoingListByNpc.ContainsKey(npcid))
        {
            foreach(KeyValuePair<int, List<QuestObjectiveTriggerData>> entry in mOngoingListByNpc[npcid])
            {
                int questid = entry.Key;
                foreach (QuestObjectiveTriggerData triggerdata in entry.Value)
                {
                    if (!questlist.ContainsKey(triggerdata.QuestId))
                    {
                        questlist.Add(triggerdata.QuestId, triggerdata.ObjectiveId);
                    }
                }
            }
        }
        return questlist;
    }

    private void AddEmptyObjectiveList(int questid, List<int> objectivelist)
    {
        if (objectivelist.Count <= 0)
        {
            return;
        }

        if (mOngoingEmptyObjective.ContainsKey(questid))
        {
            mOngoingEmptyObjective[questid] = objectivelist;
        }
        else
        {
            mOngoingEmptyObjective.Add(questid, objectivelist);
        }
    }

    private void RemoveEmptyObjectiveList(int questid)
    {
        if (mOngoingEmptyObjective.ContainsKey(questid))
        {
            mOngoingEmptyObjective.Remove(questid);
        }
    }

    private bool IsEmptyObjectiveType(int questid, int objectiveid)
    {
        if (mOngoingEmptyObjective.ContainsKey(questid))
        {
            return mOngoingEmptyObjective[questid].Contains(objectiveid);
        }
        return false;
    }

    private List<int> GetEmptyObjectiveList(int questid)
    {
        if (mOngoingEmptyObjective.ContainsKey(questid))
        {
            return mOngoingEmptyObjective[questid];
        }
        return new List<int>();
    }

    //after accepted quest
    public void NewAcceptedQuest(int questid)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            if (questJson.triggertype == QuestTriggerType.NPC || questJson.triggertype == QuestTriggerType.Interact)
            {
                int npcid = questJson.triggercaller;
                RemoveFromAvailableList(npcid, questid);
                AddToUnavailableList(npcid, questid);
                AddRefreshNPC(npcid);
            }
        }
    }

    //after completed quest or removed quest
    public void NewCompletedQuest(int questid)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            DeleteOngoingQuest(questid);
            if (questJson.triggertype == QuestTriggerType.NPC || questJson.triggertype == QuestTriggerType.Interact)
            {
                if (!mQuestController.IsQuestCompleted(questid))
                {
                    int npcid = questJson.triggercaller;
                    if (mQuestController.IsQuestAvailable(questid))
                    {
                        RemoveFromUnavailableList(npcid, questid);
                        AddToAvailableList(npcid, questid);
                    }
                    else
                    {
                        RemoveFromUnavailableList(npcid, questid);
                        AddToLockedList(npcid, questid);
                    }
                    AddRefreshNPC(npcid);
                }
            }
            CheckNextQuest(questJson);
        }
    }

    //open next quest after completed quest
    private void CheckNextQuest(QuestJson questJson)
    {
        if (!GameUtils.IsEmptyString(questJson.nextquest))
        {
            List<int> nextquestlist = new List<int>();
            string[] ids = questJson.nextquest.Split(';');
            foreach (string id in ids)
            {
                if (!GameUtils.IsEmptyString(id))
                {
                    int questid = -1;
                    if (int.TryParse(id, out questid))
                    {
                        nextquestlist.Add(questid);
                    }
                }
            }

            foreach (int questid in nextquestlist)
            {
                int lockednpcid = GetNPCIdFromLockedList(questid);
                if (lockednpcid != -1)
                {
                    CheckAvailableQuest(lockednpcid, questid);
                    AddRefreshNPC(lockednpcid);
                }

                int uncavailablenpcid = GetNPCIdFromUnavailableList(questid);
                if (uncavailablenpcid != -1)
                {
                    CheckAvailableQuest(uncavailablenpcid, questid);
                    AddRefreshNPC(uncavailablenpcid);
                }
            }
        }
    }

    private int GetNPCIdFromLockedList(int questid)
    {
        foreach (KeyValuePair<int, List<int>> entry in mLockedListByNpc)
        {
            if (entry.Value.Contains(questid))
            {
                return entry.Key;
            }
        }
        return -1;
    }

    private int GetNPCIdFromUnavailableList(int questid)
    {
        foreach (KeyValuePair<int, List<int>> entry in mUnavailableListByNpc)
        {
            if (entry.Value.Contains(questid))
            {
                return entry.Key;
            }
        }
        return -1;
    }

    public void RefreshNPCForQuest()
    {
        if (mNpcList.Count > 0)
        {
            foreach(int npcid in mNpcList)
            {
                StaticClientNPCAlwaysShow staticnpc = mEntitySystem.GetStaticClientNPC(npcid);
                if (staticnpc != null)
                {
                    staticnpc.UpdateQuestList(GetAvailableListByNpcId(npcid), GetOngoingListByNpcId(npcid));
                }
            }
        }
        mNpcList = new List<int>();
    }

    private void InitOngoingQuest()
    {
        foreach (QuestType type in Enum.GetValues(typeof(QuestType)))
        {
            Dictionary<int, CurrentQuestData> questDatas = mQuestController.GetQuestDataList(type);
            foreach (KeyValuePair<int, CurrentQuestData> entry in questDatas)
            {
                int questid = entry.Key;
                AddQuestGroupProgress(entry.Value);
                mRequirementController.InitOngoingQuestData(entry.Value, GameInfo.gLocalPlayer);
                Dictionary<int, int> npclist = GetNewNPCList(entry.Value);
                foreach(KeyValuePair<int, int> npc in npclist)
                {
                    int npcid = npc.Value;
                    int objectiveid = npc.Key;
                    AddToObjectiveNpcTrackList(npcid, questid, objectiveid);
                    CheckObjectiveAvailable(questid, objectiveid, npcid);
                }

                GetEmptyObjectiveType(entry.Value);
            }
        }
    }

    public void UpdateOngoingQuest(CurrentQuestData questData, PlayerGhost player)
    {
        QuestStatus questStatus = (QuestStatus)questData.Status;
        if (questStatus == QuestStatus.NewQuest || questStatus == QuestStatus.NewObjective)
        {
            if (AddQuestGroupProgress(questData))
            {
                mRequirementController.UpdateOngoingQuestData(questData, player);
                Dictionary<int, int> npclist = GetNewNPCList(questData);
                foreach (KeyValuePair<int, int> npc in npclist)
                {
                    int npcid = npc.Value;
                    int objectiveid = npc.Key;
                    AddToObjectiveNpcTrackList(npcid, questData.QuestId, objectiveid);
                    CheckObjectiveAvailable(questData.QuestId, objectiveid, npcid);
                }
            }

            GetEmptyObjectiveType(questData);
        }
        CheckEmptyObjectiveList(questData.QuestId);
    }

    private void DeleteOngoingQuest(int questid)
    {
        RemoveEmptyObjectiveList(questid);
        mRequirementController.DeleteOngoingQuestData(questid);
        Dictionary<int, int> npclist = GetNpcTrackList(questid);
        foreach(KeyValuePair<int, int> entry in npclist)
        {
            int objectiveid = entry.Key;
            int npcid = entry.Value;
            CleanNpcTrackList(npcid, questid);
            RemoveQuestGroupProgress(questid);
            RemoveQuestTrackList(questid);
        }
    }

    private void CheckObjectiveAvailable(int questid, int objectiveid, int npcid)
    {
        bool result = mQuestController.IsObjectiveAvailable(questid, objectiveid);
        if (result)
        {
            AddToOngoingList(npcid, questid, objectiveid);
            RemoveFromUncompleteList(npcid, questid, objectiveid);
        }
        else
        {
            AddToUncompletList(npcid, questid, objectiveid);
            RemoveFromOngoingList(npcid, questid, objectiveid);
        }
        RemoveFromAllAvailableList(npcid, questid);
        AddRefreshNPC(npcid);
    }

    private Dictionary<int, int> GetNewNPCList(CurrentQuestData questData)
    {
        QuestStatus status = (QuestStatus)questData.Status;
        if (status == QuestStatus.CompletedAllObjective || status == QuestStatus.CompletedWithEvent)
        {
            return new Dictionary<int, int>();
        }
        else
        {
            Dictionary<int, int> npclist = new Dictionary<int, int>();
            GetNPCListFromObjectiveData(questData.MainObjective, ref npclist);
            foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
            {
                GetNPCListFromObjectiveData(entry.Value, ref npclist);
            }
            return npclist;
        }
    }

    private void GetNPCListFromObjectiveData(CurrentObjectiveData objectiveData, ref Dictionary<int, int> npclist)
    {
        for (int i = 0; i < objectiveData.ObjectiveIds.Count; i++)
        {
            int objectiveid = objectiveData.ObjectiveIds[i];
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
            if (objectiveJson.type == QuestObjectiveType.Talk || objectiveJson.type == QuestObjectiveType.Choice)
            {
                if (objectiveData.ProgressCount[i] == 0)
                {
                    if (npclist.ContainsKey(objectiveid))
                    {
                        npclist[objectiveid] = objectiveJson.para1;
                    }
                    else
                    {
                        npclist.Add(objectiveid, objectiveJson.para1);
                    }
                }
            }
            else if (objectiveJson.type == QuestObjectiveType.Interact)
            {
                if (objectiveData.ProgressCount[i] < objectiveJson.para2)
                {
                    if (npclist.ContainsKey(objectiveid))
                    {
                        npclist[objectiveid] = objectiveJson.para3;
                    }
                    else
                    {
                        npclist.Add(objectiveid, objectiveJson.para3);
                    }
                }
            }
        }
    }

    private void GetEmptyObjectiveType(CurrentQuestData questData)
    {
        List<int> objectivelist = new List<int>();
        QuestStatus status = (QuestStatus)questData.Status;
        if (status == QuestStatus.CompletedAllObjective || status == QuestStatus.CompletedWithEvent)
        {
            
        }
        else
        {
            GetEmptyObjectiveListFromObjectiveData(questData.MainObjective, ref objectivelist);
            foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
            {
                GetEmptyObjectiveListFromObjectiveData(entry.Value, ref objectivelist);
            }
        }

        AddEmptyObjectiveList(questData.QuestId, objectivelist);
    }

    private void GetEmptyObjectiveListFromObjectiveData(CurrentObjectiveData objectiveData, ref List<int> objectivelist)
    {
        for (int i = 0; i < objectiveData.ObjectiveIds.Count; i++)
        {
            int objectiveid = objectiveData.ObjectiveIds[i];
            int progress = objectiveData.ProgressCount[i];
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
            if (objectiveJson != null && objectiveJson.type == QuestObjectiveType.Empty && progress == 0)
            {
                if (!objectivelist.Contains(objectiveid))
                {
                    objectivelist.Add(objectiveid);
                }
            }
        }
    }

    //when new quest id added into completed quest list 
    public void AddedCompletedList(List<int> questlist)
    {
        RecheckNextQuestList(questlist);

        foreach (int questid in questlist)
        {
            int npcid = GetNpcIdByQuestId(questid);
            if (npcid != -1)
            {
                CheckAvailableQuest(npcid, questid);
                AddRefreshNPC(npcid);
            }
        }
    }

    //when quest id removed from completed quest list 
    public void RemovedCompletedList(List<int> questlist)
    {
        RecheckNextQuestList(questlist);

        foreach (int questid in questlist)
        {
            int npcid = GetNpcIdByQuestId(questid);
            if (npcid != -1)
            {
                CheckAvailableQuest(npcid, questid);
                AddRefreshNPC(npcid);
            }
        }
    }

    private void RecheckNextQuestList(List<int> questlist)
    {
        foreach (int questid in questlist)
        {
            QuestJson questJson = QuestRepo.GetQuestByID(questid);
            if (questJson != null)
            {
                CheckNextQuest(questJson);
            }
        }
    }

    public void RecheckFromRequirementController(List<int> questlist)
    {
        foreach (int questid in questlist)
        {
            int npcid = GetNpcIdByQuestId(questid);
            CheckAvailableQuest(npcid, questid);
            AddRefreshNPC(npcid);
        }
        RefreshNPCForQuest();
    }

    public void RecheckFromRequirementController(Dictionary<int, List<int>> questlist)
    {
        List<int> questtoproceed = new List<int>();
        foreach (KeyValuePair<int, List<int>> entry in questlist)
        {
            int questid = entry.Key;
            List<int> objectivetoproceed = new List<int>();
            foreach (int objectiveid in entry.Value)
            {
                int npcid = GetNpcIdFromNpcTrackList(questid, objectiveid);
                if (npcid != -1)
                {
                    CheckObjectiveAvailable(questid, objectiveid, npcid);
                    AddRefreshNPC(npcid);
                }
                else
                {
                    if (IsEmptyObjectiveType(questid, objectiveid))
                    {
                        objectivetoproceed.Add(objectiveid);
                    }
                }
            }

            if (objectivetoproceed.Count > 0)
            {
                questtoproceed.Add(questid);
            }
        }
        RefreshNPCForQuest();
        SubmitQuestWithEmptyObjective(questtoproceed);
    }

    private void CheckEmptyObjectiveList()
    {
        List<int> questtoproceed = new List<int>();
        foreach (KeyValuePair<int, List<int>> entry in mOngoingEmptyObjective)
        {
            if (CheckEmptyObjective(entry.Key))
            {
                questtoproceed.Add(entry.Key);
            }
        }

        if (questtoproceed.Count > 0)
        {
            SubmitQuestWithEmptyObjective(questtoproceed);
        }
    }

    private void CheckEmptyObjectiveList(int questid)
    {
        if (CheckEmptyObjective(questid))
        {
            SubmitQuestWithEmptyObjective(new List<int>() { questid });
        }
    }

    private bool CheckEmptyObjective(int questid)
    {
        List<int> objectivelist = GetEmptyObjectiveList(questid);
        List<int> result = new List<int>();
        foreach (int objectiveid in objectivelist)
        {
            if (mQuestController.IsObjectiveAvailable(questid, objectiveid))
            {
                result.Add(objectiveid);
            }
        }

        return result.Count > 0 ? true : false;
    }

    private void SubmitQuestWithEmptyObjective(List<int> questlist)
    {
        string questids = JsonConvertDefaultSetting.SerializeObject(questlist);
        RPCFactory.NonCombatRPC.SubmitEmptyObjective(questids);
    }
}
