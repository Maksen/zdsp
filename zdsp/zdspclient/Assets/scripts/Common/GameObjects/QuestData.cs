using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Zealot.Repository;

namespace Zealot.Common
{
    public enum QuestStatus : byte
    {
        NewQuest = 0,
        NewQuestWithEvent = 1,
        NewObjective = 2,
        NewObjectiveWithEvent = 3,
        Ongoing = 4,
        CompletedAllObjective = 5,
        CompletedWithEvent = 6,
        Error = 7,
        Non = 8,
        Failed = 9,
    }

    public enum UpdateObjectiveType
    {
        Kill = 0,
        NPC = 1,
        Realm = 2,
        Interact = 3,
        Empty = 4,
    }

    public class CurrentObjectiveData
    {
        public List<int> ObjectiveIds { get; set; }
        public int SequenceNum { get; set; }
        public List<long> CompleteTime { get; set; }
        public List<int> ProgressCount { get; set; }
        public Dictionary<int, int> RequirementProgress { get; set; }

        public CurrentObjectiveData()
        {
            ObjectiveIds = new List<int>();
            SequenceNum = -1;
            CompleteTime = new List<long>();
            ProgressCount = new List<int>();
            RequirementProgress = new Dictionary<int, int>();
        }

        public CurrentObjectiveData(QuestObjectiveDataStats stats, int questid, int groupid)
        {
            ObjectiveIds = QuestRepo.GetObjectiveId(questid, groupid, stats.seqNum);
            SequenceNum = stats.seqNum;
            CompleteTime = stats.completeTime;
            ProgressCount = stats.progressCount;
            RequirementProgress = stats.requirementProgress;
        }

        public CurrentObjectiveData(QuestObjectiveDataStats stats, int mainid)
        {
            ObjectiveIds = QuestRepo.GetSubObjectiveId(mainid, stats.seqNum);
            SequenceNum = stats.seqNum;
            CompleteTime = stats.completeTime;
            ProgressCount = stats.progressCount;
            RequirementProgress = stats.requirementProgress;
        }
    }

    public class CurrentQuestData
    {
        public int QuestId { get; set; }
        public byte QuestType { get; set; }
        public int GroupdId { get; set; }
        public CurrentObjectiveData MainObjective { get; set; }
        public Dictionary<int, CurrentObjectiveData> SubObjective { get; set; }
        public byte Status { get; set; }
        public byte SubStatus { get; set; }

        public CurrentQuestData() { }

        public CurrentQuestData(QuestType type)
        {
            QuestId = type  == Common.QuestType.Main ? QuestRepo.MainStartQuestRefId : - 1;
            QuestType = (byte)type;
            GroupdId = -1;
            MainObjective = new CurrentObjectiveData();            
            SubObjective = new Dictionary<int, CurrentObjectiveData>();
            Status = (byte)QuestStatus.NewQuest;
        }

        public CurrentQuestData(QuestDataStats stats)
        {
            QuestId = stats.questId;
            QuestType = stats.questType;
            GroupdId = stats.groupId;
            MainObjective = new CurrentObjectiveData(stats.mainObjective, stats.questId, stats.groupId);
            SubObjective = new Dictionary<int, CurrentObjectiveData>();
            foreach (KeyValuePair<int, QuestObjectiveDataStats> entry in stats.subObjective)
            {
                SubObjective.Add(entry.Key, new CurrentObjectiveData(entry.Value, entry.Key));
            }
            Status = stats.status;
        }
    }

    public class NextObjectiveData
    {
        public string ReturnCode { get; set; }
        public List<int> ObjectiveList { get; set; }

        public NextObjectiveData(string returncode)
        {
            ReturnCode = returncode;
            ObjectiveList = null;
        }

        public NextObjectiveData(List<int> objectivelist)
        {
            ReturnCode = "Success";
            ObjectiveList = objectivelist;
        }
    }

    public class CompletedQuestData
    {
        public int QuestId { get; set; }
        public byte QuestType { get; set; }

        public CompletedQuestData(int questid, QuestType type)
        {
            QuestId = questid;
            QuestType = (byte)type;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class QuestObjectiveDataStats
    {
        [JsonProperty(PropertyName = "SequnceNum")]
        public int seqNum;

        [JsonProperty(PropertyName = "CompleteTime")]
        public List<long> completeTime;

        [JsonProperty(PropertyName = "ProgressCount")]
        public List<int> progressCount;

        [JsonProperty(PropertyName = "RequirementProgress")]
        public Dictionary<int, int> requirementProgress;

        public QuestObjectiveDataStats() { }

        public QuestObjectiveDataStats(CurrentObjectiveData objectiveData)
        {
            seqNum = objectiveData.SequenceNum;
            completeTime = objectiveData.CompleteTime;
            progressCount = objectiveData.ProgressCount;
            requirementProgress = objectiveData.RequirementProgress;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class QuestDataStats
    {
        [JsonProperty(PropertyName = "QuestId")]
        public int questId;

        [JsonProperty(PropertyName = "QuestType")]
        public byte questType;

        [JsonProperty(PropertyName = "GroupId")]
        public int groupId;

        [JsonProperty(PropertyName = "MainObjective")]
        public QuestObjectiveDataStats mainObjective;

        [JsonProperty(PropertyName = "SubObjective")]
        public Dictionary<int, QuestObjectiveDataStats> subObjective;

        [JsonProperty(PropertyName = "Status")]
        public byte status;

        public QuestDataStats() { }

        public QuestDataStats(CurrentQuestData questData)
        {
            questId = questData.QuestId;
            questType = questData.QuestType;
            groupId = questData.GroupdId;
            mainObjective = new QuestObjectiveDataStats(questData.MainObjective);
            subObjective = new Dictionary<int, QuestObjectiveDataStats>();
            if (questData.SubObjective.Count > 0)
            {
                foreach(KeyValuePair<int, CurrentObjectiveData> subobjectivedata in questData.SubObjective)
                {
                    subObjective.Add(subobjectivedata.Key, new QuestObjectiveDataStats(subobjectivedata.Value));
                }
            }
            status = questData.Status;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class QuestInventoryData
    {
        [JsonProperty(PropertyName = "MainQuest")]
        public string MainQuest { get; set; }

        [JsonProperty(PropertyName = "CompletedMain")]
        public string CompletedMain { get; set; }

        [JsonProperty(PropertyName = "AdventureQuest")]
        public string AdventureQuest { get; set; }

        [JsonProperty(PropertyName = "CompletedAdventure")]
        public string CompletedAdventure { get; set; }

        [JsonProperty(PropertyName = "SublineQuest")]
        public string SublineQuest { get; set; }

        [JsonProperty(PropertyName = "CompletedSubline")]
        public string CompletedSubline { get; set; }

        [JsonProperty(PropertyName = "GuildQuest")]
        public string GuildQuest { get; set; }

        [JsonProperty(PropertyName = "CompletedGuild")]
        public string CompletedGuild { get; set; }

        [JsonProperty(PropertyName = "SignboardQuest")]
        public string SignboardQuest { get; set; }

        [JsonProperty(PropertyName = "CompletedSignboard")]
        public string CompletedSignboard { get; set; }

        [JsonProperty(PropertyName = "EventQuest")]
        public string EventQuest { get; set; }

        [JsonProperty(PropertyName = "CompletedEvent")]
        public string CompletedEvent { get; set; }

        [JsonProperty(PropertyName = "TrackingList")]
        public string TrackingList { get; set; }

        [JsonProperty(PropertyName = "UnlockWonderful")]
        public string UnlockWonderful { get; set; }

        [JsonProperty(PropertyName = "UnlockQuest")]
        public string UnlockQuest { get; set; }

        public QuestInventoryData() {
            MainQuest = "";
            CompletedMain = "";
            AdventureQuest = "";
            CompletedAdventure = "";
            SublineQuest = "";
            CompletedSubline = "";
            GuildQuest = "";
            CompletedGuild = "";
            SignboardQuest = "";
            CompletedSignboard = "";
            EventQuest = "";
            CompletedEvent = "";
            TrackingList = "";
            UnlockWonderful = "";
            UnlockQuest = "";
        }

        public void DeserializeSingleQuestData(QuestType type, ref CurrentQuestData questData)
        {
            string QuestData = "";
            switch (type)
            {
                case QuestType.Main:
                    QuestData = MainQuest;
                    break;
            }

            if (!string.IsNullOrEmpty(QuestData))
            {
                questData = (CurrentQuestData)DeserializeQuestData(QuestData, false);
            }
            else
            {
                questData = null;
            }
        }

        public void DeserializeQuestDataList(QuestType type, ref Dictionary<int, CurrentQuestData> questDatas)
        {
            string QuestData = "";
            switch(type)
            {
                case QuestType.Destiny:
                    QuestData = AdventureQuest;
                    break;
                case QuestType.Sub:
                    QuestData = SublineQuest;
                    break;
                case QuestType.Guild:
                    QuestData = GuildQuest;
                    break;
                case QuestType.Signboard:
                    QuestData = SignboardQuest;
                    break;
                case QuestType.Event:
                    QuestData = EventQuest;
                    break;
            }

            if (!string.IsNullOrEmpty(QuestData))
            {
                questDatas = (Dictionary<int, CurrentQuestData>)DeserializeQuestData(QuestData, true);
            }
            else
            {
                questDatas = new Dictionary<int, CurrentQuestData>();
            }
        }

        private object DeserializeQuestData(string data, bool isList)
        {
            if (isList)
            {
                return JsonConvertDefaultSetting.DeserializeObject<Dictionary<int, CurrentQuestData>>(data);
            }
            else
            {
                return JsonConvertDefaultSetting.DeserializeObject<CurrentQuestData>(data);
            }
        }

        public List<int> DeserializeCompletedQuest(QuestType type)
        {
            string CompletedData = "";
            switch (type)
            {
                case QuestType.Main:
                    CompletedData = CompletedMain == null ? "" : CompletedMain;
                    break;
                case QuestType.Destiny:
                    CompletedData = CompletedAdventure == null ? "" : CompletedAdventure;
                    break;
                case QuestType.Sub:
                    CompletedData = CompletedSubline == null ? "" : CompletedSubline;
                    break;
                case QuestType.Guild:
                    CompletedData = CompletedGuild == null ? "" : CompletedGuild;
                    break;
                case QuestType.Signboard:
                    CompletedData = CompletedSignboard == null ? "" : CompletedSignboard;
                    break;
                case QuestType.Event:
                    CompletedData = CompletedEvent == null ? "" : CompletedEvent;
                    break;
            }
            List<int> result = JsonConvertDefaultSetting.DeserializeObject<List<int>>(CompletedData);
            if (result == null)
            {
                return new List<int>();
            }
            return result;
        }

        public List<int> DeseralizeTraclingList()
        {
            return JsonConvertDefaultSetting.DeserializeObject<List<int>>(TrackingList);
        }

        public List<int> DeseralizeWonderfulList()
        {
            return JsonConvertDefaultSetting.DeserializeObject<List<int>>(UnlockWonderful);
        }

        public List<int> DeseralizeUnlockQuestList()
        {
            return JsonConvertDefaultSetting.DeserializeObject<List<int>>(UnlockQuest);
        }

        public string SerailizeTrackingList(List<int> trackinglist)
        {
            TrackingList = JsonConvertDefaultSetting.SerializeObject(trackinglist);
            return TrackingList;
        }

        public string SerailizeWonderfulList(List<int> wonderfullist)
        {
            UnlockWonderful = JsonConvertDefaultSetting.SerializeObject(wonderfullist);
            return UnlockWonderful;
        }

        public string SerailizeUnlockQuestList(List<int> questlist)
        {
            UnlockQuest = JsonConvertDefaultSetting.SerializeObject(questlist);
            return UnlockQuest;
        }

        public void SerializeQuestData(object questData, QuestType type)
        {
            switch (type)
            {
                case QuestType.Main:
                    MainQuest = JsonConvertDefaultSetting.SerializeObject(questData);
                    break;
                case QuestType.Destiny:
                    AdventureQuest = JsonConvertDefaultSetting.SerializeObject(questData);
                    break;
                case QuestType.Sub:
                    SublineQuest = JsonConvertDefaultSetting.SerializeObject(questData);
                    break;
                case QuestType.Guild:
                    GuildQuest = JsonConvertDefaultSetting.SerializeObject(questData);
                    break;
                case QuestType.Signboard:
                    SignboardQuest = JsonConvertDefaultSetting.SerializeObject(questData);
                    break;
                case QuestType.Event:
                    EventQuest = JsonConvertDefaultSetting.SerializeObject(questData);
                    break;
            }
        }

        public void SerializeCompletedQuest(object questData, QuestType type)
        {
            if (questData == null)
            {
                questData = "";
            }

            switch (type)
            {
                case QuestType.Main:
                    CompletedMain = JsonConvertDefaultSetting.SerializeObject(questData);
                    break;
                case QuestType.Destiny:
                    CompletedAdventure = JsonConvertDefaultSetting.SerializeObject(questData);
                    break;
                case QuestType.Sub:
                    CompletedSubline = JsonConvertDefaultSetting.SerializeObject(questData);
                    break;
                case QuestType.Guild:
                    CompletedGuild = JsonConvertDefaultSetting.SerializeObject(questData);
                    break;
                case QuestType.Signboard:
                    CompletedSignboard = JsonConvertDefaultSetting.SerializeObject(questData);
                    break;
                case QuestType.Event:
                    CompletedEvent = JsonConvertDefaultSetting.SerializeObject(questData);
                    break;
            }
        }
    }
}
