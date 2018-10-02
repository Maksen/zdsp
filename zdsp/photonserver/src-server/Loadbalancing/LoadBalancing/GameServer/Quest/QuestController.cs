using Kopio.JsonContracts;
using Zealot.Common.Entities;
using Zealot.Common;
using Zealot.Repository;
using Zealot.Server.Entities;
using System.Collections.Generic;
using Zealot.Server.Rules;
using System.Linq;
using System;

namespace Photon.LoadBalancing.GameServer
{
    public class QuestSynStatsServer : QuestSynStats
    {
    }

    public enum ObjectiveStatus : byte
    {
        Incomplete = 0,
        Completed = 1,
    }

    public class QuestObjectiveData
    {
        public int QuestId { get; set; }
        public int OrderNum { get; set; }
        public int ObjectiveId { get; set; }
        public QuestType QuestType { get; set; }
        public int Count { get; set; }
        public Dictionary<int, int> Requirement { get; set; }
        public long EndTime { get; set; }
        public ObjectiveStatus Status { get; set; }
        public int MainObjectiveId { get; set; }

        public QuestObjectiveData(CurrentQuestData questData, int objectiveid, int ordernum, int count, long endtime, ObjectiveStatus status, int mainobjectiveid = -1)
        {
            QuestId = questData.QuestId;
            OrderNum = ordernum;
            ObjectiveId = objectiveid;
            QuestType = (QuestType)questData.QuestType;
            Count = count;
            EndTime = endtime;
            Status = status;
            MainObjectiveId = mainobjectiveid;
            Requirement = GetRequirementCount(questData);
        }

        private Dictionary<int, int> GetRequirementCount(CurrentQuestData questData)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(ObjectiveId);
            List<QuestRequirementDetailJson> requirementDetailJsons = QuestRepo.GetRequirementByGroupId(objectiveJson.requirementid);
            if (requirementDetailJsons != null)
            {
                foreach(QuestRequirementDetailJson requirement in requirementDetailJsons)
                {
                    if (MainObjectiveId == -1)
                    {
                        if (questData.MainObjective.RequirementProgress.ContainsKey(requirement.requirementid))
                        {
                            result.Add(requirement.requirementid, questData.MainObjective.RequirementProgress[requirement.requirementid]);
                        }
                        else
                        {
                            result.Add(requirement.requirementid, 0);
                        }
                    }
                    else
                    {
                        Dictionary<int, int> subrequirement = questData.SubObjective[MainObjectiveId].RequirementProgress;
                        result.Add(requirement.requirementid, subrequirement[requirement.requirementid]);
                    }
                }
            }
            return result;
        }
    }

    public class QuestController
    {
        private Player mPlayer;

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
        private int mSignboardRewardBoost;
        private int mSignboardLimit;

        //Event Quest
        private Dictionary<int, CurrentQuestData> mEventQuest;
        private List<int> mCompletedEventQuest;

        //Quest Tracking List
        private List<int> mTrackingList;

        //Wonderful Unlock List
        private List<int> mWonderfulList;

        //Quest Unlock List
        private List<int> mUnlockQuestList;

        //Signboard Unlock List
        private List<int> mUnlockSignboardList;

        //Companion Id
        private int mCompanionId;

        private Dictionary<QuestObjectiveType, Dictionary<int, QuestObjectiveData>> mObjectiveListByObjectiveType;
        private List<CompletedQuestData> mCompletedQuestList;
        private Dictionary<QuestType, List<int>> mUpdateList;
        private bool mSignboardResetOnDay;
        
        public QuestController(Player player)
        {
            mPlayer = player;
            mCompletedQuestList = new List<CompletedQuestData>();
            mUpdateList = new Dictionary<QuestType, List<int>>();
            mTrackingList = new List<int>();
            mObjectiveListByObjectiveType = new Dictionary<QuestObjectiveType, Dictionary<int, QuestObjectiveData>>();
            foreach (QuestObjectiveType type in Enum.GetValues(typeof(QuestObjectiveType)))
            {
                mObjectiveListByObjectiveType.Add(type, new Dictionary<int, QuestObjectiveData>());
            }
        }

        public void InitFromData(CharacterData characterData)
        {
            characterData.QuestInventory.DeserializeSingleQuestData(QuestType.Main, ref mMainQuest);
            mCompletedMainQuest = characterData.QuestInventory.DeserializeCompletedQuest(QuestType.Main);

            characterData.QuestInventory.DeserializeQuestDataList(QuestType.Destiny, ref mAdventureQuest);
            mCompletedAdventureQuest = characterData.QuestInventory.DeserializeCompletedQuest(QuestType.Destiny);

            characterData.QuestInventory.DeserializeQuestDataList(QuestType.Sub, ref mSublineQuest);
            mCompletedSublineQuest = characterData.QuestInventory.DeserializeCompletedQuest(QuestType.Sub);

            characterData.QuestInventory.DeserializeQuestDataList(QuestType.Guild, ref mGuildQuest);
            mCompletedGuildQuest = characterData.QuestInventory.DeserializeCompletedQuest(QuestType.Guild);

            characterData.QuestInventory.DeserializeQuestDataList(QuestType.Signboard, ref mSignboardQuest);
            mCompletedSignboardQuest = characterData.QuestInventory.DeserializeCompletedQuest(QuestType.Signboard);

            characterData.QuestInventory.DeserializeQuestDataList(QuestType.Event, ref mEventQuest);
            mCompletedEventQuest = characterData.QuestInventory.DeserializeCompletedQuest(QuestType.Event);

            mTrackingList = characterData.QuestInventory.DeseralizeTraclingList();
            mTrackingList = mTrackingList == null ? new List<int>() : mTrackingList;

            mWonderfulList = characterData.QuestInventory.DeseralizeWonderfulList();
            mWonderfulList = mWonderfulList == null ? new List<int>() : mWonderfulList;

            mUnlockQuestList = characterData.QuestInventory.DeseralizeUnlockQuestList();
            mUnlockQuestList = mUnlockQuestList == null ? new List<int>() : mUnlockQuestList;
            if (characterData.QuestInventory.IsDefaultData())
            {
                InitDefaultMainQuest();
            }

            mUnlockSignboardList = characterData.QuestInventory.DeseralizeUnlockSignboardList();
            mUnlockSignboardList = mUnlockSignboardList == null ? QuestRules.GetSignboardDataByLevel(characterData.ProgressLevel) : mUnlockSignboardList;
            mSignboardResetOnDay = GameConstantRepo.GetConstantInt("SignboardResetOnDay") == 1 ? true : false;
            mSignboardRewardBoost = characterData.QuestInventory.SignboardRewardBoost;
            mSignboardLimit = characterData.QuestInventory.SignboardLimit;

            mCompanionId = characterData.QuestInventory.CompanionId;

            InitObjectiveData();
        }

        private void InitDefaultMainQuest()
        {
            mMainQuest = null;
            TriggerNewQuest(QuestRepo.MainStartQuestRefId, 0, 0, false);
        }

        public void InitQuestStats( ref QuestSynStats questSynStats)
        {
            questSynStats.mainQuest = QuestRules.SerializedQuestStats(mMainQuest);
            questSynStats.completedMain = QuestRules.SerializedCompletedList(mCompletedMainQuest);

            questSynStats.adventureQuest = QuestRules.SerializedQuestStats(mAdventureQuest);
            questSynStats.completedAdventure = QuestRules.SerializedCompletedList(mCompletedAdventureQuest);

            questSynStats.sublineQuest = QuestRules.SerializedQuestStats(mSublineQuest);
            questSynStats.completedSubline = QuestRules.SerializedCompletedList(mCompletedSublineQuest);

            questSynStats.guildQuest = QuestRules.SerializedQuestStats(mGuildQuest);
            questSynStats.completedGuild = QuestRules.SerializedCompletedList(mCompletedGuildQuest);

            questSynStats.signboardQuest = QuestRules.SerializedQuestStats(mSignboardQuest);
            questSynStats.completedSignboard = QuestRules.SerializedCompletedList(mCompletedSignboardQuest);
            questSynStats.signboardRewardBoost = mSignboardRewardBoost;
            questSynStats.signboardLimit = mSignboardLimit;

            questSynStats.eventQuest = QuestRules.SerializedQuestStats(mEventQuest);
            questSynStats.completedEvent = QuestRules.SerializedCompletedList(mCompletedEventQuest);

            questSynStats.trackingList = QuestRules.SerializedCompletedList(mTrackingList);
            questSynStats.wonderfulList = QuestRules.SerializedCompletedList(mWonderfulList);
            questSynStats.unlockQuestList = QuestRules.SerializedCompletedList(mUnlockQuestList);
            questSynStats.unlockSignboardList = QuestRules.SerializedCompletedList(mUnlockSignboardList);
        }

        private List<int> GetUnlockQuestList()
        {
            int level = mPlayer.PlayerSynStats.Level;
            List<int> idlist = QuestRepo.GetUnlockQuest(level, QuestTriggerType.Level, level);
            return FilterQuestList(idlist);
        }

        private void InitObjectiveData()
        {
            QuestObjectiveRollBack(mMainQuest, false);
            RegroupQuestDataByObjectiveType(mMainQuest);

            QuestObjectiveRollBack(mAdventureQuest, false);
            RegroupQuestDataByObjectiveType(mAdventureQuest);

            QuestObjectiveRollBack(mSublineQuest, false);
            RegroupQuestDataByObjectiveType(mSublineQuest);

            QuestObjectiveRollBack(mGuildQuest, false);
            RegroupQuestDataByObjectiveType(mGuildQuest);

            QuestObjectiveRollBack(mSignboardQuest, false);
            RegroupQuestDataByObjectiveType(mSignboardQuest);

            QuestObjectiveRollBack(mEventQuest, false);
            RegroupQuestDataByObjectiveType(mEventQuest);

            SyncQuestStats();
        }

        public void ResetOnNewDay()
        {
            RemoveQuestFromCompletedList(QuestRepeatType.OncePerDay);
            if (mSignboardResetOnDay)
            {
                ResetSignboardList();
            }
        }

        private void RemoveQuestFromCompletedList(QuestRepeatType repeattype)
        {
            foreach (QuestType type in Enum.GetValues(typeof(QuestType)))
            {
                List<int> completedlist = GetCompletedList(type);
                List<int> questidtoremove = new List<int>();
                foreach(int questid in completedlist)
                {
                    QuestJson questJson = QuestRepo.GetQuestByID(questid);
                    if (questJson != null && questJson.repeat == repeattype)
                    {
                        questidtoremove.Add(questid);
                    }
                }

                if (questidtoremove.Count > 0)
                {
                    foreach(int questid in questidtoremove)
                    {
                        completedlist.Remove(questid);
                    }
                    string result = QuestRules.SerializedCompletedList(completedlist);
                    mPlayer.QuestStats.UpdateCompletedList(type, result);
                }
            }
        }

        private void ResetSignboardList()
        {
            AutoCompleteSignboardQuest();
            mUnlockSignboardList = QuestRules.GetSignboardDataByLevel(mPlayer.GetAccumulatedLevel());
            mPlayer.QuestStats.unlockSignboardList = QuestRules.SerializedCompletedList(mUnlockSignboardList);
            mSignboardRewardBoost = 100;
            mPlayer.QuestStats.signboardRewardBoost = mSignboardRewardBoost;
            mSignboardLimit = QuestRepo.GetSignboardDailyLimit(mPlayer.GetAccumulatedLevel());
            mPlayer.QuestStats.signboardLimit = mSignboardLimit;
        }

        private void AutoCompleteSignboardQuest()
        {
            Dictionary<int, CurrentQuestData> signboardquest = mSignboardQuest.CloneJson();
            foreach (KeyValuePair<int, CurrentQuestData> entry in signboardquest)
            {
                CurrentQuestData questData = entry.Value;
                if (questData.Status == (byte)QuestStatus.CompletedAllObjective || questData.Status == (byte)QuestStatus.CompletedWithEvent)
                {
                    CompleteQuest(questData.QuestId, true);
                }
                else
                {
                    DeleteQuest(questData.QuestId);
                }
            }
        }

        private void ResetSignboardListOnLevelUp()
        {
            int level = mPlayer.PlayerSynStats.Level;
            List<int> data = QuestRules.CheckSignboardChangeGroup(level);
            if (data != null)
            {
                List<int> signboardlist = new List<int>();
                foreach(int signboardid in mUnlockSignboardList)
                {
                    QuestSignboardJson signboardJson = QuestRepo.GetSignboardQuestBySignboardId(signboardid);
                    if (signboardJson != null)
                    {
                        if (IsQuestCompleted(signboardJson.questid))
                        {
                            signboardlist.Add(signboardid);
                        }
                        if (GetQuestDataById(signboardJson.questid, QuestType.Signboard) != null)
                        {
                            signboardlist.Add(signboardid);
                        }
                    }
                }

                foreach(int newsignboardid in data)
                {
                    if (!signboardlist.Contains(newsignboardid))
                    {
                        signboardlist.Add(newsignboardid);
                    }
                }

                mUnlockSignboardList = signboardlist;
                mPlayer.QuestStats.unlockSignboardList = QuestRules.SerializedCompletedList(mUnlockSignboardList);
                mSignboardLimit = QuestRepo.GetSignboardDailyLimit(level);
                mPlayer.QuestStats.signboardLimit = mSignboardLimit;
            }
        }

        #region Generate Objective Data From Quest Data For Progress Checking
        private void RegroupQuestDataByObjectiveType(Dictionary<int, CurrentQuestData> questDatas)
        {
            foreach(KeyValuePair<int, CurrentQuestData> entry in questDatas)
            {
                RegroupQuestDataByObjectiveType(entry.Value);
            }
        }

        private void RegroupQuestDataByObjectiveType(CurrentQuestData questData)
        {
            AddNewObjectiveData(questData);
        }
        #endregion

        #region Generate, Update, Delete Objective Data Methods
        private void AddNewObjectiveData(CurrentQuestData questData)
        {
            if (questData == null)
                return;

            int i = 0;
            foreach (int id in questData.MainObjective.ObjectiveIds)
            {
                QuestObjectiveType type = QuestRepo.GetObjectiveTypeByObjectiveId(id);
                InsertObjectiveData(questData, id, i, type);
                if (type == QuestObjectiveType.MultipleObj)
                {
                    int j = 0;
                    foreach (int subid in questData.SubObjective[id].ObjectiveIds)
                    {
                        QuestObjectiveType subtype = QuestRepo.GetObjectiveTypeByObjectiveId(subid);
                        InsertSubObjectiveData(questData, id, subid, j, subtype);
                        j++;
                    }
                }
                i++;
            }
        }

        private void InsertObjectiveData(CurrentQuestData questData, int objectiveid, int order, QuestObjectiveType type)
        {
            int progresscount = GetObjectiveProgressCount(questData, order);
            ObjectiveStatus status = GetObjectiveStatus(progresscount, objectiveid);
            if (status == ObjectiveStatus.Completed)
                return;
            long endtime = GetObjectiveEndtime(questData, order);
            if (mObjectiveListByObjectiveType[type].ContainsKey(objectiveid))
            {
                mObjectiveListByObjectiveType[type][objectiveid] = new QuestObjectiveData(questData, objectiveid, order, progresscount, endtime, status);
            }
            else
            {
                mObjectiveListByObjectiveType[type].Add(objectiveid, new QuestObjectiveData(questData, objectiveid, order, progresscount, endtime, status));
            }
        }

        private void InsertSubObjectiveData(CurrentQuestData questData, int mainid, int subid, int order, QuestObjectiveType type)
        {
            int progresscount = GetSubObjectiveProgressCount(questData, mainid, order);
            ObjectiveStatus status = GetObjectiveStatus(progresscount, subid);
            if (status == ObjectiveStatus.Completed)
                return;
            long endtime = GetSubObjectiveEndtime(questData, mainid, order);
            if (mObjectiveListByObjectiveType[type].ContainsKey(subid))
            {
                mObjectiveListByObjectiveType[type][subid] = new QuestObjectiveData(questData, subid, order, progresscount, endtime, status, mainid);
            }
            else
            {
                mObjectiveListByObjectiveType[type].Add(subid, new QuestObjectiveData(questData, subid, order, progresscount, endtime, status, mainid));
            }
        }

        private void DeleteObjectivesData(CurrentQuestData questData)
        {
            foreach(int mainid in questData.MainObjective.ObjectiveIds)
            {
                DeleteObjectiveData(mainid);
            }
            foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
            {
                foreach (int subid in entry.Value.ObjectiveIds)
                {
                    DeleteObjectiveData(subid);
                }
            }
        }

        private void DeleteObjectiveData(int objectiveid)
        {
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
            mObjectiveListByObjectiveType[objectiveJson.type].Remove(objectiveid);
        }
        #endregion

        private int GetObjectiveProgressCount(CurrentQuestData questData, int order)
        {
            List<int> countlist = questData.MainObjective.ProgressCount;
            if (countlist.Count <= order)
            {
                return 0;
            }
            else
            {
                return countlist[order];
            }
        }

        private int GetSubObjectiveProgressCount(CurrentQuestData questData, int mainid, int order)
        {
            List<int> countlist = questData.SubObjective[mainid].ProgressCount;
            if (countlist.Count <= order)
            {
                return 0;
            }
            else
            {
                return countlist[order];
            }
        }

        private ObjectiveStatus GetObjectiveStatus(int count, int objectiveid)
        {
            if (count >= QuestRepo.GetObjectiveTargetCount(objectiveid))
            {
                return ObjectiveStatus.Completed;
            }
            return ObjectiveStatus.Incomplete;
        }

        private long GetObjectiveEndtime(CurrentQuestData questData, int order)
        {
            if (questData.MainObjective.CompleteTime.Contains(order))
            {
                return questData.MainObjective.CompleteTime[order];
            }
            return 0;
        }

        private long GetSubObjectiveEndtime(CurrentQuestData questData, int mainid, int order)
        {
            if (questData.SubObjective.ContainsKey(mainid))
            {
                if (questData.SubObjective[mainid].CompleteTime.Contains(order))
                {
                    return questData.SubObjective[mainid].CompleteTime[order];
                }
            }
            return 0;
        }

        private List<int> GetCompletedList(QuestType questType)
        {
            switch(questType)
            {
                case QuestType.Main:
                    return mCompletedMainQuest;
                case QuestType.Destiny:
                    return mCompletedAdventureQuest;
                case QuestType.Sub:
                    return mCompletedSublineQuest;
                case QuestType.Guild:
                    return mCompletedGuildQuest;
                case QuestType.Signboard:
                    return mCompletedSignboardQuest;
                case QuestType.Event:
                    return mCompletedEventQuest;
                default:
                    return null;
            }
        }

        private bool IsQuestCompleted(int questid)
        {
            foreach (QuestType type in Enum.GetValues(typeof(QuestType)))
            {
                List<int> idlist = GetCompletedList(type);
                if (idlist.Contains(questid))
                {
                    return true;
                }
            }
            return false;
        }

        private List<int> GetOngoingQuestList(QuestType questType)
        {
            switch (questType)
            {
                case QuestType.Destiny:
                    return mAdventureQuest.Select(o => o.Key).ToList();
                case QuestType.Sub:
                    return mSublineQuest.Select(o => o.Key).ToList();
                case QuestType.Guild:
                    return mGuildQuest.Select(o => o.Key).ToList();
                case QuestType.Signboard:
                    return mSignboardQuest.Select(o => o.Key).ToList();
                case QuestType.Event:
                    return mEventQuest.Select(o => o.Key).ToList();
                default:
                    return null;
            }
        }

        private int GetCurrentQuestCount(QuestType questType)
        {
            switch (questType)
            {
                case QuestType.Main:
                    return mMainQuest == null? 0 : 1;
                case QuestType.Destiny:
                    return mAdventureQuest.Count;
                case QuestType.Sub:
                    return mSublineQuest.Count;
                case QuestType.Guild:
                    return mGuildQuest.Count;
                case QuestType.Signboard:
                    return mSignboardQuest.Count;
                case QuestType.Event:
                    return mEventQuest.Count;
                default:
                    return 0;
            }
        }

        private bool CheckEmptySlot(QuestType questType)
        {
            int current = GetCurrentQuestCount(questType);
            int max = QuestRepo.GetMaxQuestCountByType(questType);
            return current < max;
        }

        private CurrentQuestData GetQuestDataById(int questid, QuestType type)
        {
            switch(type)
            {
                case QuestType.Main:
                    if (mMainQuest != null && mMainQuest.QuestId == questid)
                    {
                        return mMainQuest;
                    }
                    break;
                case QuestType.Destiny:
                    if (mAdventureQuest != null && mAdventureQuest.ContainsKey(questid))
                    {
                        return mAdventureQuest[questid];
                    }
                    break;
                case QuestType.Sub:
                    if (mSublineQuest != null && mSublineQuest.ContainsKey(questid))
                    {
                        return mSublineQuest[questid];
                    }
                    break;
                case QuestType.Guild:
                    if (mGuildQuest != null && mGuildQuest.ContainsKey(questid))
                    {
                        return mGuildQuest[questid];
                    }
                    break;
                case QuestType.Signboard:
                    if (mSignboardQuest != null && mSignboardQuest.ContainsKey(questid))
                    {
                        return mSignboardQuest[questid];
                    }
                    break;
                case QuestType.Event:
                    if (mEventQuest != null && mEventQuest.ContainsKey(questid))
                    {
                        return mEventQuest[questid];
                    }
                    break;
            }
            return null;
        }

        private CurrentQuestData GetQuestDataById(int questid)
        {
            CurrentQuestData questData = null;
            foreach (QuestType type in Enum.GetValues(typeof(QuestType)))
            {
                questData = GetQuestDataById(questid, type);
                if (questData != null)
                {
                    break;
                }
            }

            return questData;
        }

        private QuestObjectiveData GetObjectiveDataById(int objectiveid)
        {
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
            if (objectiveJson != null)
            {
                Dictionary<int, QuestObjectiveData> datalist = mObjectiveListByObjectiveType[objectiveJson.type];
                if (datalist.ContainsKey(objectiveid))
                {
                    return datalist[objectiveid];
                }
            }
            return null;
        }

        #region Generate, Update, Delete Quest Data Methods
        private void AddNewQuest(CurrentQuestData questData)
        {
            QuestType questType = (QuestType)questData.QuestType;
            if (questType == (byte)QuestType.Main)
            {
                mMainQuest = questData;
            }
            else
            {
                Dictionary<int, CurrentQuestData> questlist = new Dictionary<int, CurrentQuestData>();
                switch(questType)
                {
                    case QuestType.Destiny:
                        questlist = mAdventureQuest;
                        break;
                    case QuestType.Sub:
                        questlist = mSublineQuest;
                        break;
                    case QuestType.Guild:
                        questlist = mGuildQuest;
                        break;
                    case QuestType.Signboard:
                        questlist = mSignboardQuest;
                        break;
                    case QuestType.Event:
                        questlist = mEventQuest;
                        break;
                    default:
                        return;
                }

                questlist.Add(questData.QuestId, questData);
            }
            AddNewObjectiveData(questData);

            if (!mUpdateList.ContainsKey(questType))
            {
                mUpdateList.Add(questType, new List<int>());
            }
            mUpdateList[questType].Add(questData.QuestId);
        }

        private CurrentQuestData UpdateQuestProgressCount(QuestObjectiveData objectiveData)
        {
            CurrentQuestData questData = GetQuestDataById(objectiveData.QuestId, objectiveData.QuestType);
            if (questData != null)
            {
                if (objectiveData.MainObjectiveId == -1)
                {
                    if (questData.MainObjective.ObjectiveIds[objectiveData.OrderNum] == objectiveData.ObjectiveId)
                    {
                        if (questData.MainObjective.ProgressCount.Count > objectiveData.OrderNum)
                        {
                            questData.MainObjective.ProgressCount[objectiveData.OrderNum] = objectiveData.Count;
                        }
                        else
                        {
                            List<int> progresscount = new List<int>(questData.MainObjective.ObjectiveIds.Count);
                            for (int i = 0; i < questData.MainObjective.ProgressCount.Count; i++)
                            {
                                progresscount[i] = questData.MainObjective.ProgressCount[i];
                            }
                            progresscount[objectiveData.OrderNum] = objectiveData.Count;
                            questData.MainObjective.ProgressCount = progresscount;
                        }
                    }
                }
                else
                {
                    if (questData.SubObjective[objectiveData.MainObjectiveId].ObjectiveIds[objectiveData.OrderNum] == objectiveData.ObjectiveId)
                    {
                        if (questData.SubObjective[objectiveData.MainObjectiveId].ProgressCount.Count > objectiveData.OrderNum)
                        {
                            questData.SubObjective[objectiveData.MainObjectiveId].ProgressCount[objectiveData.OrderNum] = objectiveData.Count;
                        }
                        else
                        {
                            List<int> progresscount = new List<int>(questData.SubObjective[objectiveData.MainObjectiveId].ObjectiveIds.Count);
                            for (int i = 0; i < questData.SubObjective[objectiveData.MainObjectiveId].ProgressCount.Count; i++)
                            {
                                progresscount[i] = questData.SubObjective[objectiveData.MainObjectiveId].ProgressCount[i];
                            }
                            progresscount[objectiveData.OrderNum] = objectiveData.Count;
                            questData.SubObjective[objectiveData.MainObjectiveId].ProgressCount = progresscount;
                        }
                    }
                }
            }
            return questData;
        }

        private void UpdateQuestData(CurrentQuestData questData)
        {
            if (questData.Status == (byte)QuestStatus.Error)
                return;

            if (questData.Status == (byte)QuestStatus.NewObjective)
                AddNewObjectiveData(questData);

            QuestType questType = (QuestType)questData.QuestType;
            switch (questType)
            {
                case QuestType.Main:
                    mMainQuest = questData;
                    break;
                case QuestType.Destiny:
                    if (mAdventureQuest.ContainsKey(questData.QuestId))
                    {
                        mAdventureQuest[questData.QuestId] = questData;
                    }
                    break;
                case QuestType.Sub:
                    if (mSublineQuest.ContainsKey(questData.QuestId))
                    {
                        mSublineQuest[questData.QuestId] = questData;
                    }
                    break;
                case QuestType.Guild:
                    if (mGuildQuest.ContainsKey(questData.QuestId))
                    {
                        mGuildQuest[questData.QuestId] = questData;
                    }
                    break;
                case QuestType.Signboard:
                    if (mSignboardQuest.ContainsKey(questData.QuestId))
                    {
                        mSignboardQuest[questData.QuestId] = questData;
                    }
                    break;
                case QuestType.Event:
                    if (mEventQuest.ContainsKey(questData.QuestId))
                    {
                        mEventQuest[questData.QuestId] = questData;
                    }
                    break;
            }

            if (!mUpdateList.ContainsKey(questType))
            {
                mUpdateList.Add(questType, new List<int>());
            }
            mUpdateList[questType].Add(questData.QuestId);
        }

        private void ResetQuestData(CurrentQuestData questData)
        {
            if (questData.Status == (byte)QuestStatus.Error)
                return;

            CurrentQuestData newQuestData = QuestRules.StartNewQuest(questData.QuestId, 0, mPlayer.GetSynchronizedTime(), mPlayer);
            DeleteObjectivesData(questData);
            AddNewObjectiveData(newQuestData);
            UpdateQuestData(newQuestData);
        }

        private void DeleteQuestData(CurrentQuestData questData, bool forceDelete = false, int nextid = -1)
        {
            if (questData.Status == (byte)QuestStatus.Error || forceDelete)
            {
                QuestType questType = (QuestType)questData.QuestType;
                switch (questType)
                {
                    case QuestType.Main:
                        mMainQuest = null;
                        break;
                    case QuestType.Destiny:
                        if (mAdventureQuest.ContainsKey(questData.QuestId))
                        {
                            mAdventureQuest.Remove(questData.QuestId);
                        }
                        break;
                    case QuestType.Sub:
                        if (mSublineQuest.ContainsKey(questData.QuestId))
                        {
                            mSublineQuest.Remove(questData.QuestId);
                        }
                        break;
                    case QuestType.Guild:
                        if (mGuildQuest.ContainsKey(questData.QuestId))
                        {
                            mGuildQuest.Remove(questData.QuestId);
                        }
                        break;
                    case QuestType.Signboard:
                        if (mSignboardQuest.ContainsKey(questData.QuestId))
                        {
                            mSignboardQuest.Remove(questData.QuestId);
                        }
                        break;
                    case QuestType.Event:
                        if (mEventQuest.ContainsKey(questData.QuestId))
                        {
                            mEventQuest.Remove(questData.QuestId);
                        }
                        break;
                }

                DeleteObjectivesData(questData);

                if (!mUpdateList.ContainsKey(questType))
                {
                    mUpdateList.Add(questType, new List<int>());
                }
                mUpdateList[questType].Add(questData.QuestId);
            }
        }
        #endregion

        public bool TriggerNewQuest(int questid, int callerid, int group = 0, bool sync = true)
        {
            bool unlockquestchanged = false;
            QuestJson questJson = QuestRepo.GetQuestByID(questid);
            if (questJson != null)
            {
                QuestType questType = questJson.type;
                bool signboardstatus = true;
                if(questType == QuestType.Signboard && GetSignboardProgress() >= mSignboardLimit)
                {
                    signboardstatus = false;
                }

                if (CheckEmptySlot(questType) && signboardstatus)
                {
                    bool canAccept = QuestRules.AcceptQuest(questid, callerid, GetCompletedList(questType), GetCurrentQuestCount(questType), mPlayer);
                    CurrentQuestData existQuestData = GetQuestDataById(questid);
                    if (canAccept && existQuestData == null)
                    {
                        CurrentQuestData questData = QuestRules.StartNewQuest(questid, group, mPlayer.GetSynchronizedTime(), mPlayer);
                        if (questType == QuestType.Destiny)
                        {
                            if (questData.Status == (byte)QuestStatus.NewQuest)
                            {
                                questData.Status = (byte)QuestStatus.DestinyEffect;
                            }
                            else if (questData.Status == (byte)QuestStatus.NewQuestWithEvent)
                            {
                                questData.Status = (byte)QuestStatus.DestinyEffectWithEvent;
                            }
                        }
                        AddNewQuest(questData);
                        AddToTracking(questid);
                        if (sync)
                        {
                            SyncQuestStats();
                        }
                        if (mUnlockQuestList.Contains(questid))
                        {
                            mUnlockQuestList.Remove(questid);
                            SyncUnlockQuestList();
                        }
                        if (mPlayer.Slot != null)
                        {
                            mPlayer.Slot.ZRPC.NonCombatRPC.Ret_TriggerQuest(questid, true, mPlayer.Slot);
                        }
                        return true;
                    }
                }
                else
                {
                    if (questJson.triggertype == QuestTriggerType.Level || questJson.triggertype == QuestTriggerType.Hero)
                    {
                        if (!mUnlockQuestList.Contains(questid) && QuestRules.CanAddIntoUnlock(questJson, mPlayer.PlayerSynStats.jobsect))
                        {
                            mUnlockQuestList.Add(questid);
                            unlockquestchanged = true;
                        }
                    }
                    else
                    {
                        mPlayer.Slot.ZRPC.NonCombatRPC.Ret_TriggerQuest(questid, false, mPlayer.Slot);
                    }
                }
            }
            if (unlockquestchanged)
            {
                SyncUnlockQuestList();
            }
            return false;
        }

        /*
        Kill Type - param1(MonsterId), param2(Killed Count)
        Npc Type - param1(NpcId), param2(Interact Count), param3(SelectionId)
        Realm Type - param1(RealmId), param2(Completed Count)
        Interact Type - param1(InteractId),param2(Success?)
        */
        private void UpdateObjectiveStatus(UpdateObjectiveType type, int param1, int param2, int param3, int questid = -1, int talkid = -1)
        {
            List<QuestObjectiveData> objectiveDatas = new List<QuestObjectiveData>();
            List<QuestObjectiveData> updatedDatas = new List<QuestObjectiveData>();
            Dictionary<int, CurrentQuestData> subQuestData = new Dictionary<int, CurrentQuestData>();
            Dictionary<int, CurrentQuestData> updatedQuestData = new Dictionary<int, CurrentQuestData>();
            Dictionary<int, CurrentQuestData> questForDelete = new Dictionary<int, CurrentQuestData>();
            Dictionary<int, CurrentQuestData> questForUpdate = new Dictionary<int, CurrentQuestData>();
            List<int> objectiveIdForDelete = new List<int>();

            switch (type)
            {
                case UpdateObjectiveType.Kill:
                    objectiveDatas.AddRange(mObjectiveListByObjectiveType[QuestObjectiveType.Kill].Values.ToList());
                    objectiveDatas.AddRange(mObjectiveListByObjectiveType[QuestObjectiveType.PercentageKill].Values.ToList());
                    break;
                case UpdateObjectiveType.NPC:
                    objectiveDatas.AddRange(mObjectiveListByObjectiveType[QuestObjectiveType.Talk].Values.ToList());
                    objectiveDatas.AddRange(mObjectiveListByObjectiveType[QuestObjectiveType.Choice].Values.ToList());
                    objectiveDatas.AddRange(mObjectiveListByObjectiveType[QuestObjectiveType.QuickTalk].Values.ToList());
                    break;
                case UpdateObjectiveType.Realm:
                    objectiveDatas.AddRange(mObjectiveListByObjectiveType[QuestObjectiveType.RealmComplete].Values.ToList());
                    break;
                case UpdateObjectiveType.Interact:
                    objectiveDatas.AddRange(mObjectiveListByObjectiveType[QuestObjectiveType.Interact].Values.ToList());
                    break;
                case UpdateObjectiveType.Empty:
                    objectiveDatas.AddRange(mObjectiveListByObjectiveType[QuestObjectiveType.Empty].Values.ToList());
                    break;
            }

            if (questid != -1 && objectiveDatas.Count > 0)
            {
                QuestObjectiveData odata = objectiveDatas.Where(o => o.QuestId == questid).First();
                if (odata != null)
                {
                    objectiveDatas = new List<QuestObjectiveData>();
                    objectiveDatas.Add(odata);
                }
            }

            //Update Objective Progress
            foreach (QuestObjectiveData objectivedata in objectiveDatas)
            {
                if (objectivedata.Status == ObjectiveStatus.Completed)
                    continue;

                QuestObjectiveData data = objectivedata;
                if (QuestRules.UpdateObjectiveStatus(ref data, mPlayer, param1, param2, param3, talkid))
                {
                    updatedDatas.Add(data);
                }
            }

            //Update Sub Objective Progress Count
            foreach (QuestObjectiveData objectivedata in updatedDatas)
            {
                if (objectivedata.MainObjectiveId == -1)
                    continue;

                CurrentQuestData questData = UpdateQuestProgressCount(objectivedata);
                if (questData != null)
                {
                    subQuestData.Add(questData.QuestId, questData);
                }
            }

            //Update Main Objective Progress Count
            foreach (QuestObjectiveData objectivedata in updatedDatas)
            {
                CurrentQuestData questData = UpdateQuestProgressCount(objectivedata);
                if (questData != null)
                {
                    if (subQuestData.ContainsKey(questData.QuestId))
                    {
                        subQuestData[questData.QuestId].MainObjective = questData.MainObjective;
                        updatedQuestData.Add(questData.QuestId, subQuestData[questData.QuestId]);
                    }
                    else
                    {
                        updatedQuestData.Add(questData.QuestId, questData);
                    }
                }
            }

            //Check Sub And Main Objective Completation
            foreach (KeyValuePair<int, CurrentQuestData> entry in updatedQuestData)
            {
                int i = 0;
                CurrentQuestData questdata = entry.Value;
                bool updateMain = true;

                // Sub Objective
                foreach(KeyValuePair<int, CurrentObjectiveData> subentry in questdata.SubObjective)
                {
                    if (QuestRules.IsObjectivesCompleted(questdata.QuestId, subentry.Value.ObjectiveIds, subentry.Value.ProgressCount, mPlayer))
                    {
                        objectiveIdForDelete.AddRange(subentry.Value.ObjectiveIds);
                        CurrentObjectiveData objectiveData = subentry.Value;
                        int result = QuestRules.StartNextSubObjective(subentry.Key, ref objectiveData, mPlayer.GetSynchronizedTime(), mPlayer);
                        if (result == 0)
                        {
                            if (!questForDelete.ContainsKey(questdata.QuestId))
                            {
                                questForDelete.Add(questdata.QuestId, questdata);
                                continue;
                            }
                        }
                        else
                        {
                            if (result == 1 || result == 2)
                            {
                                questdata.MainObjective.ProgressCount[i] += 1;
                                questdata.SubStatus = result == 1 ? (byte)QuestStatus.NewObjectiveWithEvent : (byte)QuestStatus.NewObjective;
                            }
                            else
                            {
                                questdata.SubObjective[subentry.Key] = objectiveData;
                                questdata.SubStatus = result == 3 ? (byte)QuestStatus.NewObjectiveWithEvent : (byte)QuestStatus.NewObjective;
                                updateMain = false;
                            }
                        }
                    }
                    i++;
                }

                if (updateMain)
                {
                    if (QuestRules.IsObjectivesCompleted(questdata.QuestId, questdata.MainObjective.ObjectiveIds, questdata.MainObjective.ProgressCount, mPlayer))
                    {
                        objectiveIdForDelete.AddRange(questdata.MainObjective.ObjectiveIds);
                        CurrentQuestData newQuestData = QuestRules.StartNextMainObjective(questdata, mPlayer.GetSynchronizedTime(), mPlayer);
                        if (newQuestData != null)
                        {
                            if (newQuestData.Status == (byte)QuestStatus.Error)
                            {
                                questForDelete.Add(newQuestData.QuestId, newQuestData);
                            }
                            else
                            {
                                questForUpdate.Add(newQuestData.QuestId, newQuestData);
                            }
                        }
                    }
                    else
                    {
                        questForUpdate.Add(questdata.QuestId, questdata);
                    }
                }
                else
                {
                    questForUpdate.Add(questdata.QuestId, questdata);
                }                
            }

            foreach (KeyValuePair<int, CurrentQuestData> questdata in questForDelete)
            {
                DeleteQuestData(questdata.Value);
            }

            foreach (int objectiveid in objectiveIdForDelete)
            {
                DeleteObjectiveData(objectiveid);
            }

            if (questForUpdate.ContainsKey(questid) && type == UpdateObjectiveType.NPC)
            {
                CurrentQuestData questData = questForUpdate[questid];
                QuestSelectDetailJson selectJson = QuestRepo.GetSelectionByTalkId(talkid, param3);
                if (selectJson != null && selectJson.actiontype == QuestSelectionActionType.Job)
                {
                    mPlayer.UpdateJobSect((byte)selectJson.actionid);
                }
            }

            foreach (KeyValuePair<int, CurrentQuestData> questdata in questForUpdate)
            {
                UpdateQuestData(questdata.Value);
            }

            SyncQuestStats();
        }

        public bool CompleteQuest(int questid, bool replyid)
        {
            QuestJson questJson = QuestRepo.GetQuestByID(questid);
            QuestType questType = questJson.type;
            CurrentQuestData questData = GetQuestDataById(questid, questType);
            int nextquest = QuestRepo.GetNextQuestByGroup(questData.QuestId, questData.GroupdId);
            if (questData == null || !questJson.isopen)
            {
                return false;
            }

            if ((QuestStatus)questData.Status != QuestStatus.CompletedAllObjective)
            {
                return false;
            }

            if (questJson.replyid  != replyid)
            {
                return false;
            }

            int rewardgroup = QuestRepo.GetQuestReward(questid, questData.GroupdId);
            if (rewardgroup != -1)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("questid", questJson.questid.ToString());
                float expboost = GetExpBoost(questJson.type);
                GameRules.GiveReward_CheckBagSlotThenMail(mPlayer, new List<int>() { rewardgroup }, "quest_mailname", parameters, true, true, "Quest_Reward", expboost);
                UpdateExpBoost(questJson);
            }

            //unlock feature

            if (questJson.repeat != QuestRepeatType.AlwaysRepeat)
            {
                List<int> completedlist = GetCompletedList(questType);
                if (completedlist != null)
                {
                    completedlist.Add(questData.QuestId);
                }
                mCompletedQuestList.Add(new CompletedQuestData(questData.QuestId, questType));
            }
            DeleteQuestData(questData, true);

            RemoveFromTracking(questData.QuestId);

            List<int> wonderfullist = QuestRepo.GetWonderfulByQuestId(questData.QuestId);
            if (wonderfullist != null)
            {
                foreach (int wonderfulid in wonderfullist)
                {
                    int passcount = 0;
                    List<int> unlockquestids = QuestRepo.GetUnlockWonderfulQuestList(wonderfulid);
                    foreach (int unlockquestid in unlockquestids)
                    {
                        if (IsQuestCompleted(unlockquestid))
                        {
                            passcount += 1;
                        }
                    }
                    if (passcount >= unlockquestids.Count)
                    {
                        if (!mWonderfulList.Contains(wonderfulid))
                        {
                            mWonderfulList.Add(wonderfulid);
                        }
                    }
                }
            }
            
            if (nextquest > 0)
            {
                if (!mUnlockQuestList.Contains(nextquest))
                {
                    mUnlockQuestList.Add(nextquest);
                    AddToTracking(nextquest);
                    SyncUnlockQuestList();
                }
            }

            AutoAssignNewQuest();

            SyncQuestStats(true);

            mPlayer.DestinyClueController.TriggerClueCondition(ClueCondition.Quest, questid);

            UpdateQuestCompleteAchievement(questType, questid);

            return true;
        }

        private void AutoAssignNewQuest()
        {
            List<QuestJson> questJsons = OrderAcceptableQuestList(mUnlockQuestList, false);

            bool changed = false;
            foreach (QuestJson questJson in questJsons)
            {
                bool result = TriggerNewQuest(questJson.questid, mPlayer.PlayerSynStats.Level);
                if (result)
                {
                    changed = true;
                    mUnlockQuestList.Remove(questJson.questid);
                }
            }

            if (changed)
            {
                SyncUnlockQuestList();
            }
        }

        public void KillCheck(int monsterid, int count)
        {
            UpdateObjectiveStatus(UpdateObjectiveType.Kill, monsterid, count, 0);
        }

        public void NpcCheck(int questid, int npcid, int choice, int talkid)
        {
            UpdateObjectiveStatus(UpdateObjectiveType.NPC, npcid, 1, choice, questid, talkid);
        }

        public void RealmCheck(int realmid, int count)
        {
            UpdateObjectiveStatus(UpdateObjectiveType.Realm, realmid, count, 0);
        }

        public bool InteractCheck(int interactid, int questid)
        {
            QuestInteractiveDetailJson interactiveJson = QuestRepo.GetQuestInteractiveByID(interactid);
            if (interactiveJson != null)
            {
                if (interactiveJson.type == QuestInteractiveType.StartQuest)
                {
                    TriggerNewQuest(interactiveJson.para, interactid);
                }
                else
                {
                    UpdateObjectiveStatus(UpdateObjectiveType.Interact, interactid, 1, 0, questid);
                }
            }
            return true;
        }

        public void ConditionCheck(QuestRequirementType type)
        {
            UpdateObjectiveStatus(UpdateObjectiveType.Empty, 0, 1, 0);

            if (type == QuestRequirementType.Level)
            {
                List<int> questlist = GetUnlockQuestList();
                TriggerQuestByLevel(questlist);
                ResetSignboardListOnLevelUp();
            }
        }

        private void TriggerQuestByLevel(List<int> questlist)
        {
            List<QuestJson> questJsons = OrderAcceptableQuestList(questlist, true);

            foreach(QuestJson questJson in questJsons)
            {
                TriggerNewQuest(questJson.questid, mPlayer.PlayerSynStats.Level);
            }
        }

        private List<QuestJson> OrderAcceptableQuestList(List<int> idlist, bool order)
        {
            List<QuestJson> questJsons = new List<QuestJson>();
            foreach (int id in idlist)
            {
                questJsons.Add(QuestRepo.GetQuestByID(id));
            }
            questJsons = order ? questJsons.OrderBy(o => o.minlv).ToList() : questJsons;
            return questJsons;
        }

        private List<int> FilterQuestList(List<int> idlist)
        {
            foreach (QuestType type in Enum.GetValues(typeof(QuestType)))
            {
                if (type == QuestType.Main)
                    break;
                List<int> completedlist = GetCompletedList(type);
                List<int> ongoindlist = GetOngoingQuestList(type);

                foreach (int id in idlist)
                {
                    if (completedlist != null && completedlist.Contains(id))
                    {
                        idlist.Remove(id);
                    }
                    if (ongoindlist != null && ongoindlist.Contains(id))
                    {
                        idlist.Remove(id);
                    }
                }
            }
            return idlist;
        }

        private void UpdateQuestStats(QuestType type, CurrentQuestData questData, int questid)
        {
            if (type == QuestType.Main)
            {
                mPlayer.QuestStats.mainQuest = questData == null ? "" : QuestRules.SerializedQuestStats(questData);
            }
            else
            {
                string result = questData == null ? "" : QuestRules.SerializedQuestStats(questData);
                mPlayer.QuestStats.UpdateStats(type, questid, result);
            }
        }

        private void SyncQuestStats(bool completed = false)
        {
            foreach(KeyValuePair<QuestType, List<int>> entry in mUpdateList)
            {
                foreach(int id in entry.Value)
                {
                    CurrentQuestData questData = GetQuestDataById(id, entry.Key);
                    UpdateQuestStats(entry.Key, questData, id);
                }
            }
            foreach(CompletedQuestData entry in mCompletedQuestList)
            {
                QuestType type = (QuestType)entry.QuestType;
                List<int> completedList = GetCompletedList(type);
                string result = QuestRules.SerializedCompletedList(completedList);
                mPlayer.QuestStats.UpdateCompletedList(type, result);
            }
            mPlayer.QuestStats.trackingList = QuestRules.SerializedCompletedList(mTrackingList);
            mCompletedQuestList = new List<CompletedQuestData>();
            mUpdateList = new Dictionary<QuestType, List<int>>();
            if (completed)
            {
                mPlayer.QuestStats.wonderfulList = QuestRules.SerializedCompletedList(mWonderfulList);
            }
        }
        
        private void SyncUnlockQuestList()
        {
            mPlayer.QuestStats.unlockQuestList = QuestRules.SerializedCompletedList(mUnlockQuestList);
        }

        public void SaveQuestInventory(QuestInventoryData questInventory)
        {
            questInventory.SerializeQuestData(mMainQuest, QuestType.Main);
            questInventory.SerializeQuestData(mAdventureQuest, QuestType.Destiny);
            questInventory.SerializeQuestData(mSublineQuest, QuestType.Sub);
            questInventory.SerializeQuestData(mGuildQuest, QuestType.Guild);
            questInventory.SerializeQuestData(mSignboardQuest, QuestType.Signboard);
            questInventory.SerializeQuestData(mEventQuest, QuestType.Event);

            questInventory.SerializeCompletedQuest(mCompletedMainQuest, QuestType.Main);
            questInventory.SerializeCompletedQuest(mCompletedAdventureQuest, QuestType.Destiny);
            questInventory.SerializeCompletedQuest(mCompletedSublineQuest, QuestType.Sub);
            questInventory.SerializeCompletedQuest(mCompletedGuildQuest, QuestType.Guild);
            questInventory.SerializeCompletedQuest(mCompletedSignboardQuest, QuestType.Signboard);
            questInventory.SerializeCompletedQuest(mCompletedEventQuest, QuestType.Event);

            questInventory.SerailizeTrackingList(mTrackingList);
            questInventory.SerailizeWonderfulList(mWonderfulList);
            questInventory.SerailizeUnlockQuestList(mUnlockQuestList);
            questInventory.SerailizeUnlockSignboardList(mUnlockSignboardList);
            
            questInventory.CompanionId = mCompanionId;
            questInventory.SignboardRewardBoost = mSignboardRewardBoost;
            questInventory.SignboardLimit = mSignboardLimit;
        }

        private void AddToTracking(int questid)
        {
            if (mTrackingList.Count < QuestRepo.MaxTrackingCount)
            {
                if (!mTrackingList.Contains(questid))
                {
                    mTrackingList.Add(questid);
                }
            }
        }

        private void RemoveFromTracking(int questid)
        {
            if (mTrackingList.Contains(questid))
            {
                mTrackingList.Remove(questid);
            }
        }

        public void UpdateTrackingList(string trackingdata)
        {
            List<int> trackinglist = JsonConvertDefaultSetting.DeserializeObject<List<int>>(trackingdata);
            mTrackingList = trackinglist;
            mPlayer.QuestStats.trackingList = QuestRules.SerializedCompletedList(mTrackingList);
        }

        public bool DeleteQuest(int questid)
        {
            CurrentQuestData questData = GetQuestDataById(questid);
            QuestJson questJson = QuestRepo.GetQuestByID(questid);
            bool success = false;
            if (questData != null && questJson != null)
            {
                if (questJson.candelete)
                {
                    DeleteQuestData(questData, true);
                    RemoveFromTracking(questData.QuestId);
                    SyncQuestStats();
                    success = true;
                }
            }
            return success;
        }

        public bool ResetQuest(int questid)
        {
            CurrentQuestData questData = GetQuestDataById(questid);
            QuestJson questJson = QuestRepo.GetQuestByID(questid);
            bool success = false;
            if (questData != null && questJson != null)
            {
                if (questJson.canreset)
                {
                    ResetQuestData(questData);
                    SyncQuestStats();
                    success = true;
                }
            }
            return success;
        }

        public void UpdateQuestEventStatus(int questid)
        {
            CurrentQuestData questData = GetQuestDataById(questid);
            if (questData != null)
            {
                if (questData.Status == (byte)QuestStatus.CompletedWithEvent)
                {
                    questData.Status = (byte)QuestStatus.CompletedAllObjective;
                }
                else if (questData.Status == (byte)QuestStatus.NewObjectiveWithEvent)
                {
                    questData.Status = (byte)QuestStatus.NewObjective;
                }
                else if (questData.Status == (byte)QuestStatus.NewQuestWithEvent)
                {
                    questData.Status = (byte)QuestStatus.NewQuest;
                }
                else if (questData.Status == (byte)QuestStatus.DestinyEffectWithEvent)
                {
                    questData.Status = (byte)QuestStatus.NewQuestWithEvent;
                }
                else if (questData.Status == (byte)QuestStatus.DestinyEffect)
                {
                    questData.Status = (byte)QuestStatus.NewQuest;
                }
                questData.SubStatus = (byte)QuestStatus.Non;
                UpdateQuestData(questData);
                SyncQuestStats();
            }
        }

        public void FailQuest(int questid)
        {
            CurrentQuestData questData = GetQuestDataById(questid);
            if (questData != null)
            {
                if (QuestObjectiveRollBack(questData))
                {
                    SyncQuestStats();
                }
            }
        }

        private void QuestObjectiveRollBack(Dictionary<int, CurrentQuestData>questDatas, bool UpdateObjectiveData = true)
        {
            foreach(KeyValuePair<int , CurrentQuestData> entry in questDatas)
            {
                QuestObjectiveRollBack(entry.Value, UpdateObjectiveData);
            }
        }

        private bool QuestObjectiveRollBack(CurrentQuestData questData, bool UpdateObjectiveData = true)
        {
            if (questData != null && questData.SubObjective.Count > 1)
            {
                CurrentQuestData newQuestData = null;
                if (QuestRules.RollBackQuestObjective(mPlayer, mPlayer.GetSynchronizedTime(), questData, ref newQuestData))
                {
                    if (UpdateObjectiveData)
                    {
                        DeleteObjectivesData(questData);
                        AddNewObjectiveData(newQuestData);
                    }
                    UpdateQuestData(newQuestData);
                    return true;
                }
            }
            return false;
        }

        public void SubmiteEmptyObjective(int questid)
        {
            UpdateObjectiveStatus(UpdateObjectiveType.Empty, 0, 1, 0, questid);
        }

        public void ApplyEventSE(int eventid, int questid)
        {
            QuestEventDetailJson questEvent = QuestRepo.GetQuestEventById(eventid);
            if (questEvent.type == QuestEventType.SideEffect)
            {
                int seid = -1;
                bool status = false;
                int.TryParse(questEvent.para1, out seid);
                status = questEvent.para2 == "1" ? true : false;
                if (status)
                {
                    GameRules.ApplySideEffect(seid, mPlayer, mPlayer);
                }
                else
                {
                    mPlayer.RemoveSideEffect(seid);
                }
            }

            UpdateQuestEventStatus(questid);
        }

        public void UpdateCompanion(int eventid, int questid)
        {
            QuestEventDetailJson questEvent = QuestRepo.GetQuestEventById(eventid);
            if (questEvent.type == QuestEventType.Companion)
            {
                int npcid = -1;
                bool status = false;
                int.TryParse(questEvent.para1, out npcid);
                status = questEvent.para2 == "1" ? true : false;
                if (status)
                {
                    mCompanionId = npcid;
                }
                else
                {
                    mCompanionId = -1;
                }
                mPlayer.PlayerSynStats.QuestCompanionId = mCompanionId;
            }

            UpdateQuestEventStatus(questid);
        }

        public void RemoveCompanionId(int companionid)
        {
            if (mCompanionId == companionid)
            {
                mCompanionId = -1;
                mPlayer.PlayerSynStats.QuestCompanionId = mCompanionId;
            }
        }

        public int GetQuestCompanionId()
        {
            return mCompanionId;
        }

        private float GetExpBoost(QuestType type)
        {
            return type == QuestType.Signboard ? mSignboardRewardBoost / 100.0f : 1.0f;
        }

        private void UpdateExpBoost(QuestJson questJson)
        {
            if (questJson.type == QuestType.Signboard)
            {
                int signboardid = QuestRepo.GetSignboardIdByQuestId(questJson.questid);
                QuestSignboardJson signboardJson = QuestRepo.GetSignboardQuestBySignboardId(signboardid);
                if (signboardJson != null)
                {
                    mSignboardRewardBoost += signboardJson.expplus;
                    mPlayer.QuestStats.signboardRewardBoost = mSignboardRewardBoost;
                }
            }
        }

        private int GetSignboardProgress()
        {
            int progress = 0;
            foreach(int signboardid in mUnlockSignboardList)
            {
                QuestSignboardJson signboardJson = QuestRepo.GetSignboardQuestBySignboardId(signboardid);
                if (signboardJson != null)
                {
                    bool completed = IsQuestCompleted(signboardJson.questid);
                    bool ongoing = GetQuestDataById(signboardJson.questid, QuestType.Signboard) == null ? false : true;
                    if (completed || ongoing)
                    {
                        progress += 1;
                    }
                }
            }
            return progress;
        }

        public bool HasQuestBeenTriggered(int questId)
        {
            return GetQuestDataById(questId) != null || IsQuestCompleted(questId);
        }

        private void UpdateQuestCompleteAchievement(QuestType questType, int questId)
        {
            string target = questId.ToString();
            switch (questType)
            {
                case QuestType.Main:
                    mPlayer.UpdateAchievement(AchievementObjectiveType.MainQuest, target, true);
                    mPlayer.UpdateAchievement(AchievementObjectiveType.MainQuest, target, false);
                    break;
                case QuestType.Destiny:
                    mPlayer.UpdateAchievement(AchievementObjectiveType.DestinyQuest, target, true);
                    mPlayer.UpdateAchievement(AchievementObjectiveType.DestinyQuest, target, false);
                    break;
                case QuestType.Sub:
                    mPlayer.UpdateAchievement(AchievementObjectiveType.SubQuest, target, true);
                    mPlayer.UpdateAchievement(AchievementObjectiveType.SubQuest, target, false);
                    break;
                case QuestType.Guild:
                    mPlayer.UpdateAchievement(AchievementObjectiveType.GuildQuest, target, true);
                    mPlayer.UpdateAchievement(AchievementObjectiveType.GuildQuest, target, false);
                    break;
                case QuestType.Signboard:
                    mPlayer.UpdateAchievement(AchievementObjectiveType.QuestBoard);
                    break;
                case QuestType.Event:
                    break;
                default:
                    break;
            }
        }

        #region Development
        public void UpdateQuestProgress(QuestType questType)
        {
            CurrentQuestData questData = null;
            switch (questType)
            {
                case QuestType.Main:
                    questData = mMainQuest;
                    break;
            }

            List<QuestObjectiveData> objectiveDatas = new List<QuestObjectiveData>();
            foreach (int id in questData.MainObjective.ObjectiveIds)
            {
                QuestObjectiveData objectiveData = GetObjectiveDataById(id);
                if (objectiveData == null)
                {
                    break;
                }
                objectiveDatas.Add(objectiveData);

                foreach (KeyValuePair<int, CurrentObjectiveData> entry in questData.SubObjective)
                {
                    foreach(int subid in entry.Value.ObjectiveIds)
                    {
                        QuestObjectiveData subobjectiveData = GetObjectiveDataById(subid);
                        objectiveDatas.Add(subobjectiveData);
                    }
                }
            }

            foreach(QuestObjectiveData objectiveData in objectiveDatas)
            {
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveData.ObjectiveId);
                if(objectiveJson.type == QuestObjectiveType.Kill || objectiveJson.type == QuestObjectiveType.PercentageKill)
                {
                    KillCheck(objectiveJson.para1, 1);
                }
                else if (objectiveJson.type == QuestObjectiveType.Talk || objectiveJson.type == QuestObjectiveType.Choice || objectiveJson.type == QuestObjectiveType.Interact)
                {
                    //NpcCheck(objectiveJson.para1, objectiveJson.para3);
                }
                else if (objectiveJson.type == QuestObjectiveType.RealmComplete)
                {
                    RealmCheck(objectiveJson.para1, 1);
                }
            }
        }
        #endregion
    }
}
