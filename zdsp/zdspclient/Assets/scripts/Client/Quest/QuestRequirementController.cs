using System.Collections.Generic;
using Zealot.Common;
using Kopio.JsonContracts;
using Zealot.Client.Entities;
using Zealot.Repository;
using System.Linq;

public enum QuestRequirementStatus
{
    Incompleted,
    Completed,
    Error,
}

public class QuestRequirementData
{
    public int QuestId;
    public QuestRequirementType Type;
    public QuestRequirementDetailJson QuestRequirementDetailJson;
    public int RequirementGroup;
    public int RequirementId;
    public int TriggerId;
    public int RequireProgress;
    public int Progress;

    public QuestRequirementData(int questid, QuestRequirementDetailJson requirementJson, int progress)
    {
        QuestId = questid;
        Type = requirementJson.type;
        QuestRequirementDetailJson = requirementJson;
        RequirementGroup = requirementJson.groupid;
        RequirementId = requirementJson.requirementid;
        TriggerId = requirementJson.para1;
        RequireProgress = GetRequireProgress(requirementJson);
        Progress = progress;
    }

    public virtual int GetRequireProgress(QuestRequirementDetailJson requirementJson)
    {
        switch(requirementJson.type)
        {
            case QuestRequirementType.Level:
                return requirementJson.para1;
            case QuestRequirementType.Item:
                return requirementJson.para2;
            case QuestRequirementType.Equipment:
                return requirementJson.para2;
            case QuestRequirementType.Hero:
                return requirementJson.para2;
            case QuestRequirementType.Title:
                return requirementJson.para2;
            case QuestRequirementType.SideEffect:
                return requirementJson.para2;
            case QuestRequirementType.Companian:
                return requirementJson.para2;
            case QuestRequirementType.Clue:
                return requirementJson.para2;
            case QuestRequirementType.Job:
                return requirementJson.para1;
            case QuestRequirementType.TimeClue:
                return requirementJson.para2;
            default:
                return 0;
        }
    }

    public virtual bool FullfillRequirement()
    {
        return Progress >= RequireProgress;
    }

    public string GetRequirementText()
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        switch(Type)
        {
            case QuestRequirementType.Level:
                param.Add("level", RequireProgress.ToString());
                return GUILocalizationRepo.GetLocalizedString("quest_lvl_requirement", param);
            case QuestRequirementType.Item:
                param.Add("item", GetRequirementName());
                param.Add("count", RequireProgress.ToString());
                return GUILocalizationRepo.GetLocalizedString("quest_item_requirement", param);
            case QuestRequirementType.Equipment:
                param.Add("item", GetRequirementName());
                param.Add("count", RequireProgress.ToString());
                return GUILocalizationRepo.GetLocalizedString("quest_equipment_requirement", param);
            case QuestRequirementType.Hero:
                if (QuestRequirementDetailJson.para2 == 1)
                {
                    param.Add("hero", GetRequirementName());
                    return GUILocalizationRepo.GetLocalizedString("quest_herounlock_requirement", param);
                }
                else
                {
                    param.Add("hero", GetRequirementName());
                    return GUILocalizationRepo.GetLocalizedString("quest_herouse_requirement", param);
                }
            case QuestRequirementType.Title:
                param.Add("title", GetRequirementName());
                return GUILocalizationRepo.GetLocalizedString("quest_title_requirement", param);
            case QuestRequirementType.SideEffect:
                param.Add("sideeffect", GetRequirementName());
                return GUILocalizationRepo.GetLocalizedString("quest_se_requirement", param);
            case QuestRequirementType.Companian:
                param.Add("npc", GetRequirementName());
                return GUILocalizationRepo.GetLocalizedString("quest_companion_requirement", param);
            case QuestRequirementType.Clue:
                param.Add("clue", GetRequirementName());
                return GUILocalizationRepo.GetLocalizedString("quest_clue_requirement", param);
            case QuestRequirementType.Job:
                param.Add("job", GetRequirementName());
                return GUILocalizationRepo.GetLocalizedString("quest_job_requirement", param);
            case QuestRequirementType.TimeClue:
                param.Add("clue", GetRequirementName());
                return GUILocalizationRepo.GetLocalizedString("quest_timeclue_requirement", param);
            default:
                return "";
        }
    }

    private string GetRequirementName()
    {
        switch (Type)
        {
            case QuestRequirementType.Level:
                return "";
            case QuestRequirementType.Item:
            case QuestRequirementType.Equipment:
                ItemBaseJson itemBaseJson = GameRepo.ItemFactory.GetItemById(QuestRequirementDetailJson.para1);
                return itemBaseJson == null ? "" : itemBaseJson.localizedname;
            case QuestRequirementType.Hero:
                HeroJson heroJson = HeroRepo.GetHeroById(QuestRequirementDetailJson.para1);
                return heroJson == null ? "" : heroJson.localizedname;
            case QuestRequirementType.Title:
                return "";
            case QuestRequirementType.SideEffect:
                SideEffectJson sideEffectJson = SideEffectRepo.GetSideEffect(QuestRequirementDetailJson.para1);
                return sideEffectJson == null ? "" : sideEffectJson.localizedname;
            case QuestRequirementType.Companian:
                StaticNPCJson staticNPCJson = StaticNPCRepo.GetNPCById(QuestRequirementDetailJson.para1);
                return staticNPCJson == null ? "" : staticNPCJson.localizedname;
            case QuestRequirementType.Clue:
                DestinyClueJson destinyClueJson = DestinyClueRepo.GetDestinyClueById(QuestRequirementDetailJson.para1);
                QuestJson destinyQuestJson = QuestRepo.GetQuestByID(destinyClueJson == null ? -1 : destinyClueJson.questid);
                return destinyQuestJson == null ? "" : destinyQuestJson.questname;
            case QuestRequirementType.Job:
                return JobSectRepo.GetJobLocalizedName((JobType)QuestRequirementDetailJson.para1);
            case QuestRequirementType.TimeClue:
                TimeClueJson timeClueJson = DestinyClueRepo.GetTimeClueById(QuestRequirementDetailJson.para1);
                QuestJson timeQuestJson = QuestRepo.GetQuestByID(timeClueJson == null ? -1 : timeClueJson.questid);
                return timeQuestJson == null ? "" : timeQuestJson.questname;
            default:
                return "";
        }
    }
}

public class ObjectiveRequirementData : QuestRequirementData
{
    public int ObjectiveId;

    public ObjectiveRequirementData(int questid, int objectiveid, QuestRequirementDetailJson requirementJson, int progress) : base (questid, requirementJson, progress)
    {
        ObjectiveId = objectiveid;
    }
}

public class QuestRequirementController
{
    private QuestClientController mQuestController;

    //trigger quest requirement
    private Dictionary<QuestRequirementType, Dictionary<int, List<QuestRequirementData>>> mStartQuestIdByRequirementType;
    private Dictionary<int, List<QuestRequirementType>> mRequirementTypeByStartQuestId;
    private Dictionary<int, QuestRequirementStatus> mStartQuestRequirementStatus;

    //ongoing quest requirement
    private Dictionary<QuestRequirementType, Dictionary<int, Dictionary<int, List<ObjectiveRequirementData>>>> mQuestIdByRequirementType;
    private Dictionary<int, Dictionary<int, List<QuestRequirementType>>> mRequirementTypeByQuestId;
    private Dictionary<int, Dictionary<int, QuestRequirementStatus>> mQuestRequirementStatus;
    private Dictionary<int, List<int>> mQuestGroupProgress;

    public QuestRequirementController(QuestClientController questController)
    {
        mQuestController = questController;
        mStartQuestIdByRequirementType = new Dictionary<QuestRequirementType, Dictionary<int, List<QuestRequirementData>>>();
        mRequirementTypeByStartQuestId = new Dictionary<int, List<QuestRequirementType>>();
        mStartQuestRequirementStatus = new Dictionary<int, QuestRequirementStatus>();

        mQuestIdByRequirementType = new Dictionary<QuestRequirementType, Dictionary<int, Dictionary<int, List<ObjectiveRequirementData>>>>();
        mRequirementTypeByQuestId = new Dictionary<int, Dictionary<int, List<QuestRequirementType>>>();
        mQuestRequirementStatus = new Dictionary<int, Dictionary<int, QuestRequirementStatus>>();
        mQuestGroupProgress = new Dictionary<int, List<int>>();
    }

    public void InitOngoingQuestData(CurrentQuestData questData, PlayerGhost player)
    {
        QuestStatus questStatus = (QuestStatus)questData.Status;
        if (questStatus == QuestStatus.NewQuest || questStatus == QuestStatus.NewObjective)
        {
            if (AddQuestGroupProgress(questData))
            {
                AddObjectivesRequirement(questData, player);
            }
        }
    }

    public void UpdateOngoingQuestData(CurrentQuestData questData, PlayerGhost player)
    {
        QuestStatus questStatus = (QuestStatus)questData.Status;
        if (questStatus == QuestStatus.NewQuest || questStatus == QuestStatus.NewObjective)
        {
            if (AddQuestGroupProgress(questData))
            {
                RemoveObjectiveRequirement(questData.QuestId);
                AddObjectivesRequirement(questData, player);
            }
        }
    }

    public void DeleteOngoingQuestData(int questid)
    {
        RemoveObjectiveRequirement(questid);
    }

    #region Quest Objective Requirement
    private void RemoveObjectiveRequirement(int questid)
    {
        Dictionary<int, List<QuestRequirementType>> requirementTypes = GetObjectiveRequirementCheckList(questid);
        foreach(KeyValuePair<int, List<QuestRequirementType>> entry in requirementTypes)
        {
            foreach (QuestRequirementType requirementType in entry.Value)
            {
                RemoveObjectiveRequirementData(requirementType, questid);
            }
        }

        RemoveObjectiveRequirementTrackList(questid);
        RemoveObjectiveRequirementStatus(questid);
        RemoveQuestGroupProgress(questid);
    }

    private void AddObjectivesRequirement(CurrentQuestData questData, PlayerGhost player)
    {
        AddObjectiveRequirement(questData.QuestId, questData.MainObjective, player);

        foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
        {
            AddObjectiveRequirement(questData.QuestId, entry.Value, player);
        }
    }

    private void AddObjectiveRequirement(int questid, CurrentObjectiveData objectiveData, PlayerGhost player)
    {
        foreach (int objectiveid in objectiveData.ObjectiveIds)
        {
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
            if (objectiveJson != null)
            {
                List<QuestRequirementDetailJson> requirementList = QuestRepo.GetRequirementByGroupId(objectiveJson.requirementid);
                if (requirementList != null)
                {
                    foreach (QuestRequirementDetailJson requirementJson in requirementList)
                    {
                        int progress = 0;
                        UpdateProgress(requirementJson.type, requirementJson.para1, requirementJson.para2, player, ref progress);
                        ObjectiveRequirementData requirementData = new ObjectiveRequirementData(questid, objectiveid, requirementJson, progress);
                        AddObjectiveRequirementData(requirementJson.type, questid, objectiveid, requirementData);
                        AddObjectiveRequiremenTrackList(requirementJson.type, questid, objectiveid);
                    }
                }
            }

            CheckObjectiveRequirement(questid, objectiveid);
        }
    }

    private void AddObjectiveRequirementData(QuestRequirementType requirementType, int questid, int objectiveid, ObjectiveRequirementData requirementData)
    {
        if (!mQuestIdByRequirementType.ContainsKey(requirementType))
        {
            mQuestIdByRequirementType.Add(requirementType, new Dictionary<int, Dictionary<int, List<ObjectiveRequirementData>>>());
        }

        if (!mQuestIdByRequirementType[requirementType].ContainsKey(questid))
        {
            mQuestIdByRequirementType[requirementType].Add(questid, new Dictionary<int, List<ObjectiveRequirementData>>());
        }

        if (!mQuestIdByRequirementType[requirementType][questid].ContainsKey(objectiveid))
        {
            mQuestIdByRequirementType[requirementType][questid].Add(objectiveid, new List<ObjectiveRequirementData>());
        }

        mQuestIdByRequirementType[requirementType][questid][objectiveid].Add(requirementData);
    }

    private List<ObjectiveRequirementData> GetObjectiveRequirementData(QuestRequirementType requirementType, int questid, int objectiveid)
    {
        if (mQuestIdByRequirementType.ContainsKey(requirementType))
        {
            if (mQuestIdByRequirementType[requirementType].ContainsKey(questid))
            {
                if (mQuestIdByRequirementType[requirementType][questid].ContainsKey(objectiveid))
                {
                    return mQuestIdByRequirementType[requirementType][questid][objectiveid];
                }
            }
        }
        return new List<ObjectiveRequirementData>();
    }

    private Dictionary<int, Dictionary<int, List<ObjectiveRequirementData>>> GetObjectiveRequirementData(QuestRequirementType requirementType)
    {
        if (mQuestIdByRequirementType.ContainsKey(requirementType))
        {
            return mQuestIdByRequirementType[requirementType];
        }
        return new Dictionary<int, Dictionary<int, List<ObjectiveRequirementData>>>();
    }

    private void RemoveObjectiveRequirementData(QuestRequirementType requirementType, int questid)
    {
        if (mQuestIdByRequirementType.ContainsKey(requirementType))
        {
            if (mQuestIdByRequirementType[requirementType].ContainsKey(questid))
            {
                mQuestIdByRequirementType[requirementType].Remove(questid);
            }
        }
    }

    private void AddObjectiveRequiremenTrackList(QuestRequirementType requirementType, int questid, int objectiveid)
    {
        if (!mRequirementTypeByQuestId.ContainsKey(questid))
        {
            mRequirementTypeByQuestId.Add(questid, new Dictionary<int, List<QuestRequirementType>>());
        }

        if (!mRequirementTypeByQuestId[questid].ContainsKey(objectiveid))
        {
            mRequirementTypeByQuestId[questid].Add(objectiveid, new List<QuestRequirementType>());
        }

        if (!mRequirementTypeByQuestId[questid][objectiveid].Contains(requirementType))
        {
            mRequirementTypeByQuestId[questid][objectiveid].Add(requirementType);
        }
    }

    private List<QuestRequirementType> GetObjectiveRequirementCheckList(int questid, int objectiveid)
    {
        if (mRequirementTypeByQuestId.ContainsKey(questid))
        {
            if (mRequirementTypeByQuestId[questid].ContainsKey(objectiveid))
            {
                return mRequirementTypeByQuestId[questid][objectiveid];
            }
        }
        return new List<QuestRequirementType>();
    }

    private Dictionary<int, List<QuestRequirementType>> GetObjectiveRequirementCheckList(int questid)
    {
        if (mRequirementTypeByQuestId.ContainsKey(questid))
        {
            return mRequirementTypeByQuestId[questid];
        }
        return new Dictionary<int, List<QuestRequirementType>>();
    }

    private void RemoveObjectiveRequirementTrackList(int questid)
    {
        if (mRequirementTypeByQuestId.ContainsKey(questid))
        {
            mRequirementTypeByQuestId.Remove(questid);
        }
    }

    private void AddObjectiveRequirementStatus(int questid, int objectiveid, QuestRequirementStatus status)
    {
        if (!mQuestRequirementStatus.ContainsKey(questid))
        {
            mQuestRequirementStatus.Add(questid, new Dictionary<int, QuestRequirementStatus>());
        }

        if (!mQuestRequirementStatus[questid].ContainsKey(objectiveid))
        {
            mQuestRequirementStatus[questid].Add(objectiveid, status);
        }
        else
        {
            mQuestRequirementStatus[questid][objectiveid] = status;
        }
    }

    private void RemoveObjectiveRequirementStatus(int questid)
    {
        if (mQuestRequirementStatus.ContainsKey(questid))
        {
            mQuestRequirementStatus.Remove(questid);
        }
    }

    public QuestRequirementStatus GetObjectiveRequirementStatus(int questid, int objectiveid)
    {
        if (mQuestRequirementStatus.ContainsKey(questid))
        {
            if (mQuestRequirementStatus[questid].ContainsKey(objectiveid))
            {
                return mQuestRequirementStatus[questid][objectiveid];
            }
        }
        return QuestRequirementStatus.Error;
    }

    private bool AddQuestGroupProgress(CurrentQuestData questData)
    {
        List<int> objectivelist = new List<int>();
        objectivelist.AddRange(questData.MainObjective.ObjectiveIds);
        foreach(KeyValuePair<int, CurrentObjectiveData>  entry in questData.SubObjective)
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
    #endregion

    private void CheckObjectiveRequirement(int questid, int objectiveid)
    {
        List<QuestRequirementType> requirementTypes = GetObjectiveRequirementCheckList(questid, objectiveid);
        List<ObjectiveRequirementData> requirementDatas = new List<ObjectiveRequirementData>();
        foreach (QuestRequirementType requirementType in requirementTypes)
        {
            requirementDatas.AddRange(GetObjectiveRequirementData(requirementType, questid, objectiveid));
        }

        bool result = AllObjectiveRequirementFullfill(requirementDatas);
        AddObjectiveRequirementStatus(questid, objectiveid, result ? QuestRequirementStatus.Completed : QuestRequirementStatus.Incompleted);
    }

    private bool AllObjectiveRequirementFullfill(List<ObjectiveRequirementData> requirementDatas)
    {
        foreach (ObjectiveRequirementData requirementData in requirementDatas)
        {
            if (!requirementData.FullfillRequirement())
            {
                return false;
            }
        }
        return true;
    }

    public void UpdateObjectiveRequirementProgress(QuestRequirementType requirementType, int triggerid, PlayerGhost player)
    {
        Dictionary<int, List<int>> questlist = new Dictionary<int, List<int>>();
        Dictionary<int, Dictionary<int, List<ObjectiveRequirementData>>> requirementDatas = GetObjectiveRequirementData(requirementType);
        foreach (KeyValuePair<int, Dictionary<int, List<ObjectiveRequirementData>>> questentry in requirementDatas)
        {
            foreach (KeyValuePair<int, List<ObjectiveRequirementData>> objectiveentry in questentry.Value)
            {
                foreach (ObjectiveRequirementData requirementData in objectiveentry.Value)
                {
                    if (triggerid == -1 || requirementData.TriggerId == triggerid)
                    {
                        UpdateRequirementDataProgress(requirementData, player);
                        QuestRequirementStatus status = GetObjectiveRequirementStatus(requirementData.QuestId, requirementData.ObjectiveId);
                        if ((requirementData.FullfillRequirement() && status == QuestRequirementStatus.Incompleted) || (!requirementData.FullfillRequirement() && status == QuestRequirementStatus.Completed))
                        {
                            if (!questlist.ContainsKey(requirementData.QuestId))
                            {
                                questlist.Add(requirementData.QuestId, new List<int>());
                            }

                            if (!questlist[requirementData.QuestId].Contains(requirementData.ObjectiveId))
                            {
                                questlist[requirementData.QuestId].Add(requirementData.ObjectiveId);
                            }
                        }
                    }
                }
            }
        }
        CheckObjectiveRequirementByQuestId(questlist);
    }

    private void CheckObjectiveRequirementByQuestId(Dictionary<int, List<int>> questlist)
    {
        Dictionary<int, List<int>> refreshquest = new Dictionary<int, List<int>>();
        foreach (KeyValuePair<int, List<int>> entry in questlist)
        {
            int questid = entry.Key;
            foreach (int objectiveid in entry.Value)
            {
                List<ObjectiveRequirementData> requirementDatas = new List<ObjectiveRequirementData>();
                List<QuestRequirementType> requirementTypes = GetObjectiveRequirementCheckList(questid, objectiveid);
                foreach (QuestRequirementType requirementType in requirementTypes)
                {
                    requirementDatas.AddRange(GetObjectiveRequirementData(requirementType, questid, objectiveid));
                }
                QuestRequirementStatus status = GetObjectiveRequirementStatus(questid, objectiveid);
                bool result = AllRequirementFullfill(requirementDatas);
                if ((status == QuestRequirementStatus.Incompleted && result) || (status == QuestRequirementStatus.Completed && !result))
                {
                    if (!refreshquest.ContainsKey(questid))
                    {
                        refreshquest.Add(questid, new List<int>());
                    }
                    if (!refreshquest[questid].Contains(objectiveid))
                    {
                        refreshquest[questid].Add(objectiveid);
                    }
                    AddObjectiveRequirementStatus(questid, objectiveid, result ? QuestRequirementStatus.Completed : QuestRequirementStatus.Incompleted);
                }
            }
        }

        UpdateQuestObjectiveTrigger(refreshquest);
    }

    private void UpdateQuestObjectiveTrigger(Dictionary<int, List<int>> questlist)
    {
        mQuestController.GetQuestTriggerController().RecheckFromRequirementController(questlist);
    }

    #region Trigger Quest Requirement
    private void AddTriggerQuestRequirementData(QuestRequirementType requirementType, int questid, QuestRequirementData requirementData)
    {
        if (!mStartQuestIdByRequirementType.ContainsKey(requirementType))
        {
            mStartQuestIdByRequirementType.Add(requirementType, new Dictionary<int, List<QuestRequirementData>>());
        }

        if (!mStartQuestIdByRequirementType[requirementType].ContainsKey(questid))
        {
            mStartQuestIdByRequirementType[requirementType].Add(questid, new List<QuestRequirementData>());
        }

        mStartQuestIdByRequirementType[requirementType][questid].Add(requirementData);
    }

    private Dictionary<int, List<QuestRequirementData>> GetTriggerQuestRequirementList(QuestRequirementType requirementType)
    {
        if (mStartQuestIdByRequirementType.ContainsKey(requirementType))
        {
            return mStartQuestIdByRequirementType[requirementType];
        }
        return new Dictionary<int, List<QuestRequirementData>>();
    }

    private List<QuestRequirementData> GetTriggerQuestRequirementData(QuestRequirementType requirementType, int questid)
    {
        if (mStartQuestIdByRequirementType.ContainsKey(requirementType))
        {
            if (mStartQuestIdByRequirementType[requirementType].ContainsKey(questid))
            {
                return mStartQuestIdByRequirementType[requirementType][questid];
            }
        }
        return new List<QuestRequirementData>();
    }

    private void AddTriggerQuestRequirementTrackList(QuestRequirementType requirementType, int questid)
    {
        if (!mRequirementTypeByStartQuestId.ContainsKey(questid))
        {
            mRequirementTypeByStartQuestId.Add(questid, new List<QuestRequirementType>());
        }

        if (!mRequirementTypeByStartQuestId[questid].Contains(requirementType))
        {
            mRequirementTypeByStartQuestId[questid].Add(requirementType);
        }
    }

    private List<QuestRequirementType> GetTriggerQuestRequirementCheckList(int questid)
    {
        if (mRequirementTypeByStartQuestId.ContainsKey(questid))
        {
            return mRequirementTypeByStartQuestId[questid];
        }
        return new List<QuestRequirementType>();
    }

    private void AddTriggerQuestRequirementStatus(int questid, QuestRequirementStatus status)
    {
        if (!mStartQuestRequirementStatus.ContainsKey(questid))
        {
            mStartQuestRequirementStatus.Add(questid, status);
        }
        else
        {
            mStartQuestRequirementStatus[questid] = status;
        }
    }

    public QuestRequirementStatus GetTriggerQuestRequirementStatus(int questid)
    {
        if (mStartQuestRequirementStatus.ContainsKey(questid))
        {
            return mStartQuestRequirementStatus[questid];
        }
        return QuestRequirementStatus.Error;
    }
    #endregion

    public void AddTriggerQuestRequirement(int questid, PlayerGhost player)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        if (questJson != null)
        {
            List<QuestRequirementDetailJson> requirementList = QuestRepo.GetRequirementByGroupId(questJson.requirementid);
            List<QuestRequirementData> addedrequirementdata = new List<QuestRequirementData>();
            foreach (QuestRequirementDetailJson requirementJson in requirementList)
            {
                int progress = 0;
                if (player != null)
                {
                    UpdateProgress(requirementJson.type, requirementJson.para1, requirementJson.para2, player, ref progress);
                }
                QuestRequirementData requirementData = new QuestRequirementData(questid, requirementJson, progress);
                AddTriggerQuestRequirementData(requirementJson.type, questid, requirementData);
                AddTriggerQuestRequirementTrackList(requirementJson.type, questid);
                addedrequirementdata.Add(requirementData);
            }

            bool result = AllRequirementFullfill(addedrequirementdata);
            AddTriggerQuestRequirementStatus(questid, result ? QuestRequirementStatus.Completed : QuestRequirementStatus.Incompleted);
        }
    }

    public void UpdateTriggerQuestRequirementProgress(QuestRequirementType requirementType, int triggerid, PlayerGhost player)
    {
        List<int> questlist = new List<int>();
        Dictionary<int, List<QuestRequirementData>> requirementDatas = GetTriggerQuestRequirementList(requirementType);
        foreach (KeyValuePair<int, List<QuestRequirementData>> entry in requirementDatas)
        {
            foreach (QuestRequirementData requirementData in entry.Value)
            {
                if (triggerid == -1 || requirementData.TriggerId == triggerid)
                {
                    UpdateRequirementDataProgress(requirementData, player);
                    QuestRequirementStatus status = GetTriggerQuestRequirementStatus(requirementData.QuestId);
                    if ((requirementData.FullfillRequirement() && status == QuestRequirementStatus.Incompleted) || (!requirementData.FullfillRequirement() && status == QuestRequirementStatus.Completed))
                    {
                        if (!questlist.Contains(requirementData.QuestId))
                        {
                            questlist.Add(requirementData.QuestId);
                        }
                    }
                }
            }
        }
        CheckTriggerRequirementByQuestId(questlist);
    }

    private void CheckTriggerRequirementByQuestId(List<int> questlist)
    {
        List<int> refreshquest = new List<int>();
        foreach(int questid in questlist)
        {
            List<QuestRequirementData> requirementDatas = new List<QuestRequirementData>();
            List<QuestRequirementType> requirementTypes = GetTriggerQuestRequirementCheckList(questid);
            foreach (QuestRequirementType requirementType in requirementTypes)
            {
                requirementDatas.AddRange(GetTriggerQuestRequirementData(requirementType, questid));
            }
            QuestRequirementStatus status = GetTriggerQuestRequirementStatus(questid);
            bool result = AllRequirementFullfill(requirementDatas);
            if ((status == QuestRequirementStatus.Incompleted && result) || (status == QuestRequirementStatus.Completed && !result))
            {
                refreshquest.Add(questid);
                AddTriggerQuestRequirementStatus(questid, result ? QuestRequirementStatus.Completed : QuestRequirementStatus.Incompleted);
            }
        }

        UpdateQuestTrigger(refreshquest);
    }

    private bool AllRequirementFullfill(List<QuestRequirementData> requirementDatas)
    {
        foreach (QuestRequirementData requirementData in requirementDatas)
        {
            if (!requirementData.FullfillRequirement())
            {
                return false;
            }
        }
        return true;
    }

    private bool AllRequirementFullfill(List<ObjectiveRequirementData> requirementDatas)
    {
        foreach (ObjectiveRequirementData requirementData in requirementDatas)
        {
            if (!requirementData.FullfillRequirement())
            {
                return false;
            }
        }
        return true;
    }

    private void UpdateQuestTrigger(List<int> questlist)
    {
        mQuestController.GetQuestTriggerController().RecheckFromRequirementController(questlist);
    }

    private void UpdateProgress(QuestRequirementType requirementType, int triggerid, int triggertype, PlayerGhost player, ref int progress)
    {
        switch (requirementType)
        {
            case QuestRequirementType.Level:
                progress = player.PlayerSynStats.Level;
                break;
            case QuestRequirementType.Item:
                progress = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)triggerid);
                break;
            case QuestRequirementType.Equipment:
                Equipment equipment = player.mEquipmentInvData.GetEquipmentByItemId((ushort)triggerid);
                if (equipment != null)
                {
                    progress = equipment.UpgradeLevel;
                }
                else
                {
                    progress = 0;
                }
                break;
            case QuestRequirementType.Hero:
                if (player.HeroStats.IsHeroSummoned(triggerid) && triggertype == 1)
                {
                    progress = 1;
                }
                else if (player.HeroStats.IsHeroUnlocked(triggerid) && triggertype == 2)
                {
                    progress = 2;
                }
                else
                {
                    progress = 0;
                }
                break;
            case QuestRequirementType.Title:
                break;
            case QuestRequirementType.SideEffect:
                if (player.HasSideEffect(triggerid) && triggertype == 1)
                {
                    progress = 1;
                }
                else if (!player.HasSideEffect(triggerid) && triggertype == 2)
                {
                    progress = 2;
                }
                else
                {
                    progress = 0;
                }
                break;
            case QuestRequirementType.Companian:
                if (mQuestController.GetCompanionId() == triggerid && triggertype == 1)
                {
                    progress = 1;
                }
                else if (mQuestController.GetCompanionId() != triggerid && triggertype == 2)
                {
                    progress = 2;
                }
                else
                {
                    progress = 0;
                }
                break;
            case QuestRequirementType.Clue:
                if (player.DestinyClueController.IsClueAlreadyUnlock(triggerid) && triggertype == 1)
                {
                    progress = 1;
                }
                else if (!player.DestinyClueController.IsClueAlreadyUnlock(triggerid) && triggertype == 2)
                {
                    progress = 2;
                }
                else
                {
                    progress = 0;
                }
                break;
            case QuestRequirementType.Job:
                progress = player.PlayerSynStats.jobsect;
                break;
            case QuestRequirementType.TimeClue:
                if (player.DestinyClueController.IsTimeClueAlreadyUnlock(triggerid) && triggertype == 1)
                {
                    progress = 1;
                }
                else if (!player.DestinyClueController.IsTimeClueAlreadyUnlock(triggerid) && triggertype == 2)
                {
                    progress = 2;
                }
                else
                {
                    progress = 0;
                }
                break;
        }
    }

    private void UpdateRequirementDataProgress(QuestRequirementData requirementData, PlayerGhost player)
    {
        int progress = 0;
        UpdateProgress(requirementData.Type, requirementData.TriggerId, requirementData.RequireProgress, player, ref progress);
        requirementData.Progress = progress;
    }
    
    public string GetRequirementText(int questid)
    {
        QuestJson questJson = QuestRepo.GetQuestByID(questid);
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("questname", questJson == null ? "" : questJson.questname);
        string result = GUILocalizationRepo.GetLocalizedString("quest_requirement", param);
        List<QuestRequirementType> requirementTypes = GetTriggerQuestRequirementCheckList(questid);
        foreach (QuestRequirementType requiremnettype in requirementTypes)
        {
            List<QuestRequirementData> requirementDatas = GetTriggerQuestRequirementData(requiremnettype, questid);
            foreach(QuestRequirementData data in requirementDatas)
            {
                if (!data.FullfillRequirement())
                {
                    result += data.GetRequirementText() + " ";
                }
            }
        }
        return result;
    }
}
