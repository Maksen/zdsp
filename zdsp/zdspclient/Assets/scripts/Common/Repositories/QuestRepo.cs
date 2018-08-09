using System;
using System.Collections.Generic;
using System.Linq;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class QuestRepo
    {
        public static int MainStartQuestRefId = 1;
        public static int MaxMainQuest = 1;
        public static int MaxAdventureQuest = 50;
        public static int MaxSublineQuest = 20;
        public static int MaxGuildQuest = 10;
        public static int MaxSignboardQuest = 10;
        public static int MaxEventQuest = 10;
        public static int MaxTrackingCount = 20;
        public static Dictionary<int, int> mChapterGroupByQuestIdMap;
        public static Dictionary<int, List<ChapterJson>> mChaptherGroupMap;
        public static Dictionary<int, QuestJson> mQuestIdMap;
        public static Dictionary<int, QuestObjectiveJson> mQuestObjectiveIdMap;
        public static Dictionary<int, QuestTalkDetailJson> mQuestTalkDetailIdMap;
        public static Dictionary<int, int> mQuestTalkDetailIdByTalkId;
        public static Dictionary<int, List<QuestSelectDetailJson>> mQuestSelectDetailMap;
        public static Dictionary<int, QuestSelectDetailJson> mQuestSelectDetailIdMap;
        public static Dictionary<int, QuestInteractiveDetailJson> mQuestInteractiveDetailIdMap;
        public static Dictionary<int, List<QuestRequirementDetailJson>> mQuestRequirementDetailMap;
        public static Dictionary<int, QuestRequirementDetailJson> mQuestRequirementIdMap;
        public static Dictionary<int, List<QuestEventDetailJson>> mQuestEventDetailMap;
        public static Dictionary<int, WonderfulJson> mWonderfulMap;
        public static Dictionary<int, List<int>> mWonderfulByQuestIdMap;
        public static Dictionary<int, List<QuestDestinyJson>> mDestinyDetailGroupMap;
        public static Dictionary<int, QuestDestinyJson> mDestinyDetailIdMap;
        public static Dictionary<int, List<QuestSignboardJson>> mSignboardGroupMap;
        public static Dictionary<int, QuestSignboardJson> mSignboardDetailMap;
        public static Dictionary<int, int> mSignboardIdByQuestId;
        public static Dictionary<int, SignboardLimitJson> mDailyLimitDetailMap;

        static QuestRepo()
        {
            mChapterGroupByQuestIdMap = new Dictionary<int, int>();
            mChaptherGroupMap = new Dictionary<int, List<ChapterJson>>();
            mQuestIdMap = new Dictionary<int, QuestJson>();
            mQuestObjectiveIdMap = new Dictionary<int, QuestObjectiveJson>();
            mQuestTalkDetailIdMap = new Dictionary<int, QuestTalkDetailJson>();
            mQuestTalkDetailIdByTalkId = new Dictionary<int, int>();
            mQuestSelectDetailMap = new Dictionary<int, List<QuestSelectDetailJson>>();
            mQuestSelectDetailIdMap = new Dictionary<int, QuestSelectDetailJson>();
            mQuestInteractiveDetailIdMap = new Dictionary<int, QuestInteractiveDetailJson>();
            mQuestRequirementDetailMap = new Dictionary<int, List<QuestRequirementDetailJson>>();
            mQuestRequirementIdMap = new Dictionary<int, QuestRequirementDetailJson>();
            mQuestEventDetailMap = new Dictionary<int, List<QuestEventDetailJson>>();
            mWonderfulMap = new Dictionary<int, WonderfulJson>();
            mWonderfulByQuestIdMap = new Dictionary<int, List<int>>();
            mDestinyDetailGroupMap = new Dictionary<int, List<QuestDestinyJson>>();
            mDestinyDetailIdMap = new Dictionary<int, QuestDestinyJson>();
            mSignboardGroupMap = new Dictionary<int, List<QuestSignboardJson>>();
            mSignboardDetailMap = new Dictionary<int, QuestSignboardJson>();
            mSignboardIdByQuestId = new Dictionary<int, int>();
            mDailyLimitDetailMap = new Dictionary<int, SignboardLimitJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mChapterGroupByQuestIdMap.Clear();
            mChaptherGroupMap.Clear();
            mQuestIdMap.Clear();
            mQuestObjectiveIdMap.Clear();
            mQuestTalkDetailIdMap.Clear();
            mQuestTalkDetailIdByTalkId.Clear();
            mQuestSelectDetailMap.Clear();
            mQuestSelectDetailIdMap.Clear();
            mQuestInteractiveDetailIdMap.Clear();
            mQuestRequirementDetailMap.Clear();
            mQuestRequirementIdMap.Clear();
            mQuestEventDetailMap.Clear();
            mWonderfulMap.Clear();
            mWonderfulByQuestIdMap.Clear();
            mDestinyDetailGroupMap.Clear();
            mDestinyDetailIdMap.Clear();
            mSignboardGroupMap.Clear();
            mSignboardDetailMap.Clear();
            mSignboardIdByQuestId.Clear();
            mDailyLimitDetailMap.Clear();

            foreach (KeyValuePair<int, ChapterJson> entry in gameData.Chapter)
            {
                if (!mChaptherGroupMap.ContainsKey(entry.Value.groupid))
                {
                    mChaptherGroupMap.Add(entry.Value.groupid, new List<ChapterJson>());
                }

                mChapterGroupByQuestIdMap.Add(entry.Value.questid, entry.Value.groupid);
                mChaptherGroupMap[entry.Value.groupid].Add(entry.Value);
            }
            
            foreach(KeyValuePair<int, QuestJson> entry in gameData.Quest)
            {
                mQuestIdMap.Add(entry.Value.questid, entry.Value);
            }

            foreach (KeyValuePair<int, QuestObjectiveJson> entry in gameData.QuestObjective)
            {
                mQuestObjectiveIdMap.Add(entry.Value.objid, entry.Value);
            }
            
            foreach (KeyValuePair<int, QuestTalkDetailJson> entry in gameData.QuestTalkDetail)
            {
                mQuestTalkDetailIdMap.Add(entry.Value.talkid, entry.Value);
                mQuestTalkDetailIdByTalkId.Add(entry.Key, entry.Value.talkid);
            }

            foreach (KeyValuePair<int, QuestSelectDetailJson> entry in gameData.QuestSelectDetail)
            {
                mQuestSelectDetailIdMap.Add(entry.Key, entry.Value);

                if (!mQuestSelectDetailMap.ContainsKey(entry.Value.groupid))
                {
                    mQuestSelectDetailMap.Add(entry.Value.groupid, new List<QuestSelectDetailJson>());
                }

                mQuestSelectDetailMap[entry.Value.groupid].Add(entry.Value);
            }

            foreach (KeyValuePair<int, QuestInteractiveDetailJson> entry in gameData.QuestInteractiveDetail)
            {
                mQuestInteractiveDetailIdMap.Add(entry.Value.interactiveid, entry.Value);
            }

            foreach (KeyValuePair<int, QuestRequirementDetailJson> entry in gameData.QuestRequirementDetail)
            {
                if (!mQuestRequirementDetailMap.ContainsKey(entry.Value.groupid))
                {
                    mQuestRequirementDetailMap.Add(entry.Value.groupid, new List<QuestRequirementDetailJson>());
                }

                mQuestRequirementDetailMap[entry.Value.groupid].Add(entry.Value);
                mQuestRequirementIdMap.Add(entry.Value.requirementid, entry.Value);
            }

            foreach(KeyValuePair<int, WonderfulJson> entry in gameData.Wonderful)
            {
                mWonderfulMap.Add(entry.Key, entry.Value);
                List<int> ids = GetWonderfulQuestIds(entry.Value);
                foreach (int id in ids)
                {
                    if (!mWonderfulByQuestIdMap.ContainsKey(id))
                    {
                        mWonderfulByQuestIdMap.Add(id, new List<int>());
                    }
                    mWonderfulByQuestIdMap[id].Add(entry.Key);
                }
            }

            foreach (KeyValuePair<int, QuestEventDetailJson> entry in gameData.QuestEventDetail)
            {
                if (!mQuestEventDetailMap.ContainsKey(entry.Value.groupid))
                {
                    mQuestEventDetailMap.Add(entry.Value.groupid, new List<QuestEventDetailJson>());
                }
                mQuestEventDetailMap[entry.Value.groupid].Add(entry.Value);
            }

            foreach(KeyValuePair<int, QuestDestinyJson> entry in gameData.QuestDestiny)
            {
                mDestinyDetailIdMap.Add(entry.Key, entry.Value);
                if (!mDestinyDetailGroupMap.ContainsKey(entry.Value.groupid))
                {
                    mDestinyDetailGroupMap.Add(entry.Value.groupid, new List<QuestDestinyJson>());
                }
                mDestinyDetailGroupMap[entry.Value.groupid].Add(entry.Value);
            }

            foreach (KeyValuePair<int, QuestSignboardJson> entry in gameData.QuestSignboard)
            {
                mSignboardDetailMap.Add(entry.Value.signboardid, entry.Value);
                if (!mSignboardGroupMap.ContainsKey(entry.Value.groupid))
                {
                    mSignboardGroupMap.Add(entry.Value.groupid, new List<QuestSignboardJson>());
                }
                mSignboardIdByQuestId.Add(entry.Value.questid, entry.Value.signboardid);
                mSignboardGroupMap[entry.Value.groupid].Add(entry.Value);
            }

            foreach (KeyValuePair<int, SignboardLimitJson> entry in gameData.SignboardLimit)
            {
                mDailyLimitDetailMap.Add(entry.Key, entry.Value);
            }

            MainStartQuestRefId = GameConstantRepo.GetConstantInt("MainQuest_Id");
        }

        public static ChapterJson GetChapterByQuestId(int questid)
        {
            int groupid;
            if (mChapterGroupByQuestIdMap.TryGetValue(questid, out groupid))
            {
                return mChaptherGroupMap[groupid].Where(o => o.questid == questid).First();
            }
            return null;
        }

        public static string GetChapterProgress(int groupid, int questid)
        {
            int max = mChaptherGroupMap[groupid].Count;
            List<ChapterJson> list = GetQuestsInChapter(groupid);
            int progress = list.FindIndex(x => x.questid == questid);
            return progress + " \\ " + max; 
        }

        public static List<ChapterJson> GetQuestsInChapter(int groupid)
        {
            return mChaptherGroupMap[groupid].OrderBy(o => o.sequence).ToList();
        }

        public static Dictionary<int, List<ChapterJson>> GetChapters()
        {
            return mChaptherGroupMap;
        }

        public static QuestJson GetQuestByID(int questid)
        {
            if (mQuestIdMap.ContainsKey(questid))
                return mQuestIdMap[questid];
            return null;
        }

        public static void GetQuestByType(QuestType questType, out Dictionary<int, QuestJson> questlist)
        {
            questlist = new Dictionary<int, QuestJson>();
            foreach (KeyValuePair<int,QuestJson> quest in mQuestIdMap)
            {
                if (quest.Value.type == questType)
                {
                    questlist.Add(quest.Key, quest.Value);
                }
            }
        }

        public static int GetNextQuestByGroup(int questid, int groupid)
        {
            QuestJson questJson = GetQuestByID(questid);
            if (questJson != null)
            {
                string[] ids = questJson.nextquest.Split(';');
                if (ids.Length > groupid)
                {
                    string id = ids[groupid];
                    if (!string.IsNullOrEmpty(id) && id != "#unnamed#")
                    {
                        return int.Parse(id);
                    }
                }
            }
            return -1;
        }

        public static bool CheckQuestEventByQuestId(int questid, bool complete)
        {
            QuestJson questJson = GetQuestByID(questid);
            if (questJson != null)
            {
                List<QuestEventDetailJson> eventlist = GetQuestEventListById(questJson.eventid);
                if (eventlist != null)
                {
                    foreach (QuestEventDetailJson data in eventlist)
                    {
                        if (!complete && data.timing == EventTimingType.StartQuest)
                        {
                            return true;
                        }

                        if (complete && data.timing == EventTimingType.CompleteQuest)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool CheckQuestEventByObjectiveId(int objectiveid)
        {
            QuestObjectiveJson objectiveJson = GetQuestObjectiveByID(objectiveid);
            if (objectiveJson != null)
            {
                List<QuestEventDetailJson> eventlist = GetQuestEventListById(objectiveJson.eventid);
                if (eventlist != null)
                {
                    foreach (QuestEventDetailJson data in eventlist)
                    {
                        if (data.timing == EventTimingType.CompleteObjective && data.objectiveid == objectiveid)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static Dictionary<int, Dictionary<int, List<string>>> GetQuestObjectiveByQuestId(int questid)
        {
            Dictionary<int, Dictionary<int, List<string>>> objectivelist = new Dictionary<int, Dictionary<int, List<string>>>();
            QuestJson questJson = GetQuestByID(questid);
            string objectivedata = "";
            if (questJson != null)
            {
                if (!string.IsNullOrEmpty(questJson.objgroup) || !string.IsNullOrEmpty(questJson.objectiveid))
                {
                    if (!string.IsNullOrEmpty(questJson.objectiveid))
                    {
                        objectivedata = questJson.objectiveid;
                    }
                    else if (!string.IsNullOrEmpty(questJson.objgroup))
                    {
                        objectivedata = questJson.objgroup;
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(objectivedata))
            {
                List<string> groups = GetObjectiveGroupList(objectivedata);
                for (int i = 0; i < groups.Count; i++)
                {
                    List<string> idgroups = GetObjectiveIdGroup(groups[i]);
                    Dictionary<int, List<string>> idgroup = new Dictionary<int, List<string>>();
                    for (int j = 0; j < idgroups.Count; j++)
                    {
                        idgroup.Add(j, GetObjectiveIdList(idgroups[j]));
                    }
                    objectivelist.Add(i, idgroup);
                }
            }
            return objectivelist;
        }

        private static List<string> GetObjectiveGroupList(string objectives)
        {
            List<string> grouplist = new List<string>();
            string[] groups = objectives.Split(';');
            for (int i = 0; i < groups.Length; i++)
            {
                if (!string.IsNullOrEmpty(groups[i]))
                {
                    grouplist.Add(groups[i]);
                }
            }
            return grouplist;
        }

        private static List<string> GetObjectiveIdGroup(string objectives)
        {
            List<string> idgroup = new List<string>();
            string[] group = objectives.Split(',');
            for (int i = 0; i < group.Length; i++)
            {
                if (!string.IsNullOrEmpty(group[i]))
                {
                    idgroup.Add(group[i]);
                }
            }
            return idgroup;
        }

        private static List<string> GetObjectiveIdList(string objectives)
        {
            List<string> idlist = new List<string>();
            string[] ids = objectives.Split('|');
            for (int i = 0; i < ids.Length; i++)
            {
                if (!string.IsNullOrEmpty(ids[i]))
                {
                    idlist.Add(ids[i]);
                }
            }
            return idlist;
        }

        public static QuestObjectiveJson GetQuestObjectiveByID(int objectiveid)
        {
            if (mQuestObjectiveIdMap.ContainsKey(objectiveid))
                return mQuestObjectiveIdMap[objectiveid];
            return null;
        }

        public static QuestTalkDetailJson GetQuestTalkByID(int talkid)
        {
            if (mQuestTalkDetailIdMap.ContainsKey(talkid))
                return mQuestTalkDetailIdMap[talkid];
            return null;
        }

        public static List<QuestSelectDetailJson> GetSelectionByGroupId(int groupid)
        {
            if (mQuestSelectDetailMap.ContainsKey(groupid))
                return mQuestSelectDetailMap[groupid];
            return null;
        }

        public static QuestSelectDetailJson GetSelectionById(int id)
        {
            if (mQuestSelectDetailIdMap.ContainsKey(id))
                return mQuestSelectDetailIdMap[id];
            return null;
        }

        public static bool CheckCorrectAnswer(int talkid, int answerid)
        {
            QuestTalkDetailJson talkJson = GetQuestTalkByID(talkid);
            if (talkJson != null)
            {
                List<QuestSelectDetailJson> questSelects = GetSelectionByGroupId(talkJson.selectionid);
                if (questSelects != null)
                {
                    foreach(QuestSelectDetailJson selection in questSelects)
                    {
                        if (selection.id == answerid)
                        {
                            if ((selection.actiontype == QuestSelectionActionType.SubmitObjective || selection.actiontype == QuestSelectionActionType.Job) && selection.isanswer)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static QuestSelectDetailJson GetSelectionByTalkId(int talkid, int answerid)
        {
            QuestTalkDetailJson talkJson = GetQuestTalkByID(talkid);
            if (talkJson != null)
            {
                List<QuestSelectDetailJson> questSelects = GetSelectionByGroupId(talkJson.selectionid);
                if (questSelects != null)
                {
                    foreach (QuestSelectDetailJson selection in questSelects)
                    {
                        if (selection.id == answerid)
                        {
                            return selection;
                        }
                    }
                }
            }
            return null;
        }

        public static QuestInteractiveDetailJson GetQuestInteractiveByID(int interactiveid)
        {
            if (mQuestInteractiveDetailIdMap.ContainsKey(interactiveid))
                return mQuestInteractiveDetailIdMap[interactiveid];
            return null;
        }

        public static List<QuestRequirementDetailJson> GetRequirementByGroupId(int groupid)
        {
            if (mQuestRequirementDetailMap.ContainsKey(groupid))
                return mQuestRequirementDetailMap[groupid];
            return null;
        }

        public static List<int> GetPreviousObjectiveId(int questid, int group, int seqnum)
        {
            Dictionary<int, Dictionary<int, List<string>>> grouplist = GetQuestObjectiveByQuestId(questid);
            int previousseqnum = seqnum - 1;
            if (grouplist.ContainsKey(group))
            {
                Dictionary<int, List<string>> objectives = grouplist[group];
                if (objectives.ContainsKey(previousseqnum))
                {
                    return objectives[previousseqnum].Select(int.Parse).ToList();
                }
            }
            return new List<int>();
        }

        public static NextObjectiveData GetNextObjectiveId(int questid, int group, int seqnum)
        {
            Dictionary<int, Dictionary<int, List<string>>> grouplist = GetQuestObjectiveByQuestId(questid);
            int nextseqnum = seqnum + 1;
            if (grouplist.ContainsKey(group))
            {
                Dictionary<int, List<string>> objectives = grouplist[group];
                if (nextseqnum > objectives.Count)
                {
                    return new NextObjectiveData("Error");
                }
                else if (nextseqnum == objectives.Count)
                {
                    return new NextObjectiveData("End");
                }
                else
                {
                    return new NextObjectiveData(objectives[nextseqnum].Select(int.Parse).ToList());
                }
            }
            else
            {
                return new NextObjectiveData("Error");
            }
        }

        public static List<int> GetObjectiveId(int questid, int group, int seqnum)
        {
            Dictionary<int, Dictionary<int, List<string>>> grouplist = GetQuestObjectiveByQuestId(questid);
            if (grouplist.ContainsKey(group))
            {
                Dictionary<int, List<string>> objectives = grouplist[group];
                if (objectives.ContainsKey(seqnum))
                {
                    return objectives[seqnum].Select(int.Parse).ToList();
                }
            }
            return null;
        }

        public static NextObjectiveData GetNextSubObjectiveId(int mainid, int seqnum)
        {
            Dictionary<int, List<string>> idlist = GetSubObjectiveList(mainid);
            int nextseqnum = seqnum + 1;
            if (nextseqnum > idlist.Count)
            {
                return new NextObjectiveData("Error");
            }
            else if (nextseqnum == idlist.Count)
            {
                return new NextObjectiveData("End");
            }
            else
            {
                return new NextObjectiveData(idlist[nextseqnum].Select(int.Parse).ToList());
            }
        }

        public static Dictionary<int, List<int>> GetSubObjective(int objectiveid)
        {
            Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();
            Dictionary<int, List<string>> list = GetSubObjectiveList(objectiveid);
            foreach (KeyValuePair<int, List<string>> entry in list)
            {
                result.Add(entry.Key, entry.Value.Select(int.Parse).ToList());
            }
            return result;
        }

        public static int GetSubObjectiveSeqNum(int objectiveid)
        {
            Dictionary<int, List<int>> objectivelist = GetSubObjective(objectiveid);
            foreach (KeyValuePair<int, List<int>> entry in objectivelist)
            {
                if (entry.Value.Contains(objectiveid))
                {
                    return entry.Key;
                }
            }
            return 0;
        }

        private static Dictionary<int, List<string>> GetSubObjectiveList(int objectiveid)
        {
            Dictionary<int, List<string>> subobjectivelist = new Dictionary<int, List<string>>();
            QuestObjectiveJson objectiveJson = GetQuestObjectiveByID(objectiveid);
            if (!string.IsNullOrEmpty(objectiveJson.para4) && objectiveJson.type == QuestObjectiveType.MultipleObj)
            {
                List<string> groups = GetObjectiveIdGroup(objectiveJson.para4);
                for (int i = 0; i < groups.Count; i++)
                {
                    subobjectivelist.Add(i, new List<string>());
                    List<string> ids = GetObjectiveIdList(groups[i]);
                    for (int j = 0; j < ids.Count; j++)
                    {
                        subobjectivelist[i].Add(ids[j]);
                    }
                }
            }
            return subobjectivelist;
        }

        public static List<int> GetSubObjectiveId(int mainid, int seqnum)
        {
            Dictionary<int, List<string>> idlist = GetSubObjectiveList(mainid);
            if (idlist.ContainsKey(seqnum))
            {
                return idlist[seqnum].Select(int.Parse).ToList();
            }
            return null;
        }

        public static List<int> GetObjectiveByGroup(int questid, int group, int seqnum)
        {
            Dictionary<int, Dictionary<int, List<string>>> objectivelist = GetQuestObjectiveByQuestId(questid);
            return objectivelist[group][seqnum].Select(int.Parse).ToList();
        }        

        public static QuestType GetQuestTypeByQuestId(int questid)
        {
            return GetQuestByID(questid).type;
        }

        public static QuestObjectiveType GetObjectiveTypeByObjectiveId(int objectiveid)
        {
            return GetQuestObjectiveByID(objectiveid).type;
        }

        public static int GetObjectiveTargetCount(int objectiveid)
        {
            QuestObjectiveJson objectiveJson = GetQuestObjectiveByID(objectiveid);
            switch(objectiveJson.type)
            {
                case QuestObjectiveType.Kill:
                    return objectiveJson.para2;
                case QuestObjectiveType.Talk:
                    return 1;
                case QuestObjectiveType.RealmComplete:
                    return objectiveJson.para2;
                case QuestObjectiveType.PercentageKill:
                    return objectiveJson.para2;
                case QuestObjectiveType.Choice:
                    return objectiveJson.para2;
                case QuestObjectiveType.Interact:
                    return objectiveJson.para2;
                case QuestObjectiveType.MultipleObj:
                    return 1;
                case QuestObjectiveType.Empty:
                    return 1;
                case QuestObjectiveType.QuickTalk:
                    return 1;
                default:
                    return 0;
            }
        }

        public static bool MultiQuestRewardGroup(int questid)
        {
            QuestJson questJson = GetQuestByID(questid);
            string[] rewardlist = questJson.reward.Split(',');
            return rewardlist.Length > 1;
        }

        public static int GetQuestReward(int questid, int group)
        {
            QuestJson questJson = GetQuestByID(questid);
            if (string.IsNullOrEmpty(questJson.reward) || questJson.reward == "#unnamed#")
            {
                return -1;
            }

            string[] rewardlist = questJson.reward.Split(',');
            if (rewardlist.Length > group)
            {
                return Convert.ToInt32(rewardlist[group]);
            }
            else
            {
                return -1;
            }
        }

        public static List<int> GetQuestRewardList(int questid)
        {
            QuestJson questJson = GetQuestByID(questid);
            string[] rewardlist = questJson.reward.Split(',');
            return rewardlist.Select(int.Parse).ToList();
        }

        public static int GetMaxQuestCountByType(QuestType questType)
        {
            switch (questType)
            {
                case QuestType.Main:
                    return MaxMainQuest;
                case QuestType.Destiny:
                    return MaxAdventureQuest;
                case QuestType.Sub:
                    return MaxSublineQuest;
                case QuestType.Guild:
                    return MaxGuildQuest;
                case QuestType.Signboard:
                    return MaxSignboardQuest;
                case QuestType.Event:
                    return MaxEventQuest;
                default:
                    return 0;
            }
        }

        public static long GetObjectiveEndTime(int objectiveid)
        {
            return GetQuestObjectiveByID(objectiveid).time * 1000;
        }

        public static List<int> GetUnlockQuest(int level, QuestTriggerType type, int callerid)
        {
            if (type == QuestTriggerType.Level)
            {
                return mQuestIdMap.Where(o => o.Value.minlv <= level && o.Value.type != QuestType.Main && o.Value.triggertype == type && o.Value.triggercaller <= callerid).Select(o => o.Key).ToList();
            }
            else
            {
                return mQuestIdMap.Where(o => o.Value.minlv <= level && o.Value.type != QuestType.Main && o.Value.triggertype == type && o.Value.triggercaller == callerid).Select(o => o.Key).ToList();
            }
        }

        public static List<int> GetUnlockQuestList(int level)
        {
            return mQuestIdMap.Where(o => o.Value.minlv <= level).Select(o => o.Key).ToList();
        }

        public static QuestRequirementDetailJson GetQuestRequirementyById(int id)
        {
            if (mQuestRequirementIdMap.ContainsKey(id))
            {
                return mQuestRequirementIdMap[id];
            }
            return null;
        }

        public static Dictionary<int, WonderfulJson>  GetWonderful()
        {
            return mWonderfulMap;
        }

        public static int GetWonderfulCount()
        {
            return mWonderfulMap.Count;
        }

        public static List<int> GetWonderfulByQuestId(int questid)
        {
            if (mWonderfulByQuestIdMap.ContainsKey(questid))
            {
                return mWonderfulByQuestIdMap[questid];
            }
            return null;
        }

        public static List<int> GetUnlockWonderfulQuestList(int id)
        {
            if(mWonderfulMap.ContainsKey(id))
            {
                return GetWonderfulQuestIds(mWonderfulMap[id]);
            }
            return null;
        }

        private static List<int> GetWonderfulQuestIds(WonderfulJson wonderfulJson)
        {
            List<int> result = new List<int>();
            string[] ids = wonderfulJson.questid.Split(',');
            foreach(string id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    int idresult;
                    if (int.TryParse(id, out idresult))
                    {
                        result.Add(idresult);
                    }
                }
            }
            return result;
        }

        public static List<QuestEventDetailJson> GetQuestEventListById(int groupid)
        {
            if (mQuestEventDetailMap.ContainsKey(groupid))
            {
                return mQuestEventDetailMap[groupid];
            }
            return null;
        }

        public static QuestEventDetailJson GetQuestEvent(int groupid, EventTimingType timingType, int objectiveid)
        {
            List<QuestEventDetailJson> questEvents = GetQuestEventListById(groupid);
            foreach(QuestEventDetailJson questevent in questEvents)
            {
                if (questevent.timing == timingType && ((timingType == EventTimingType.CompleteObjective && questevent.objectiveid == objectiveid) || timingType != EventTimingType.CompleteObjective))
                {
                    return questevent;
                }
            }
            return null;
        }

        public static QuestEventDetailJson GetQuestEventById(int eventid)
        {
            foreach(KeyValuePair<int, List<QuestEventDetailJson>> entry in mQuestEventDetailMap)
            {
                foreach(QuestEventDetailJson eventdata in entry.Value)
                {
                    if (eventdata.id == eventid)
                    {
                        return eventdata;
                    }
                }
            }
            return null;
        }

        public static List<int> GetDestinyGroup()
        {
            return mDestinyDetailGroupMap.Keys.ToList();
        }

        public static QuestDestinyJson GetDestinyByGroupId(int groupid)
        {
            List<QuestDestinyJson> questDestinies = GetDestinyListByGroupId(groupid);
            if (questDestinies != null && questDestinies.Count > 0)
            {
                return questDestinies[0];
            }
            return null;
        }

        public static List<QuestDestinyJson> GetDestinyListByGroupId(int groupid)
        {
            if (mDestinyDetailGroupMap.ContainsKey(groupid))
            {
                return mDestinyDetailGroupMap[groupid];
            }
            return null;
        }

        public static QuestJson GetNextBlockQuestJson(int groupid, int row, int column)
        {
            List<QuestDestinyJson> destinyJsons = GetDestinyListByGroupId(groupid);
            foreach(QuestDestinyJson destinyJson in destinyJsons)
            {
                if (destinyJson.uicolumn == column && destinyJson.uirow == row)
                {
                    QuestJson questJson = GetQuestByID(destinyJson.questid);
                    return questJson;
                }
            }
            return null;
        }

        public static QuestDestinyJson GetDestinyById(int id)
        {
            if (mDestinyDetailIdMap.ContainsKey(id))
            {
                return mDestinyDetailIdMap[id];
            }
            return null;
        }

        public static QuestDestinyJson GetDestinyByQuestId(int questid)
        {
            return mDestinyDetailIdMap.Values.Where(o => o.questid == questid).First();
        }

        public static int GetPriorityQuestId(List<int> questlist)
        {
            QuestJson selectedquest = null;
            foreach (int questid in questlist)
            {
                QuestJson questJson = GetQuestByID(questid);
                if (selectedquest == null)
                {
                    selectedquest = questJson;
                }
                else
                {
                    int p1 = GetQuestPriorityNumber(selectedquest.type);
                    int p2 = GetQuestPriorityNumber(questJson.type);
                    if (p2 < p1)
                    {
                        selectedquest = questJson;
                    }
                }
            }

            if (selectedquest != null)
            {
                return selectedquest.questid;
            }
            return -1;
        }

        private static int GetQuestPriorityNumber(QuestType type)
        {
            if (type == QuestType.Main)
            {
                return 0;
            }
            else if (type == QuestType.Destiny)
            {
                return 1;
            }
            else if (type == QuestType.Event)
            {
                return 2;
            }
            else if (type == QuestType.Sub)
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        public static List<int> GetSignboardGroupByLevel(int level)
        {
            List<int> group = new List<int>();
            foreach (KeyValuePair<int, List<QuestSignboardJson>> entry in mSignboardGroupMap)
            {
                if (entry.Value.Count > 0)
                {
                    QuestSignboardJson signboardJson = entry.Value[0];
                    if (level >= signboardJson.lvlmin && level <= signboardJson.lvlmax)
                    {
                        group.Add(entry.Key);
                    }
                }
            }
            return group;
        }

        public static List<QuestSignboardJson> GetSignboardQuestByGroup(int groupid)
        {
            if (mSignboardGroupMap.ContainsKey(groupid))
            {
                return mSignboardGroupMap[groupid];
            }
            return null;
        }

        public static QuestSignboardJson GetSignboardQuestBySignboardId(int signboardid)
        {
            if (mSignboardDetailMap.ContainsKey(signboardid))
            {
                return mSignboardDetailMap[signboardid];
            }
            return null;
        }

        public static int GetSignboardIdByQuestId(int questid)
        {
            if (mSignboardIdByQuestId.ContainsKey(questid))
            {
                return mSignboardIdByQuestId[questid];
            }
            return -1;
        }

        public static int GetSignboardDailyLimit(int level)
        {
            foreach (KeyValuePair<int, SignboardLimitJson> entry in mDailyLimitDetailMap)
            {
                if (level >= entry.Value.lvlmin && level <= entry.Value.lvlmax)
                {
                    return entry.Value.dailylimit;
                }
            }
            return 0;
        }
    }
}