using ExitGames.Concurrency.Fibers;
using Kopio.JsonContracts;
using Photon.LoadBalancing.GameServer;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Repository;
using Zealot.Server.Entities;

namespace Zealot.Server.Rules
{
    public static class QuestRules
    {
        private static int serverId;
        private static readonly PoolFiber executionFiber;

        static QuestRules()
        {
            executionFiber = GameApplication.Instance.executionFiber;
        }

        public static void Init()
        {
            serverId = GameApplication.Instance.GetMyServerline();
        }

        /*
        Kill/Percentage Kill Type - param1(MonsterId), param2(Killed Count)
        Talk Type - param1(NpcId), param2(Interact Count)
        Choice Type - param1(NpcId), param2(Interact Count), param3(SelectionId), param4(Question Talk Id)
        Interact Type - param1(InteractId),param2(Success?)
        Realm Type - param1(RealmId), param2(Completed Count)
        Multiple Type - param2(Completed?)
        */
        public static bool UpdateObjectiveStatus(ref QuestObjectiveData objectiveData, Player player, int param1, int param2, int param3 = -1, int param4 = -1)
        {
            bool success = false;
            QuestJson questJson = QuestRepo.GetQuestByID(objectiveData.QuestId);
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveData.ObjectiveId);
            if (!questJson.isopen)
            {
                return success;
            }

            long current = player.GetSynchronizedTime();
            if (objectiveData.EndTime != 0 && objectiveData.EndTime <= current)
            {
                return success;
            }

            switch (objectiveJson.type)
            {
                case QuestObjectiveType.Kill:
                    if (objectiveJson.para1 == param1)
                    {
                        success = true;
                    }
                    break;
                case QuestObjectiveType.Talk:
                    if (objectiveJson.para1 == param1)
                    {
                        success = true;
                    }
                    break;
                case QuestObjectiveType.RealmComplete:
                    if (objectiveJson.para1 == param1)
                    {
                        success = true;
                    }
                    break;
                case QuestObjectiveType.PercentageKill:
                    int chance = GameUtils.RandomInt(0, 100);
                    if (objectiveJson.para1 == param1 && chance <= objectiveJson.para3)
                    {
                        success = true;
                    }
                    break;
                case QuestObjectiveType.Choice:
                    bool isanswer = QuestRepo.CheckCorrectAnswer(param4, param3);
                    if (objectiveJson.para1 == param1 && isanswer)
                    {
                        success = true;
                    }
                    break;
                case QuestObjectiveType.Interact:
                    QuestInteractiveDetailJson questInteractiveDetail = QuestRepo.GetQuestInteractiveByID(param1);
                    if (questInteractiveDetail.type == QuestInteractiveType.EndQuest)
                    {
                        if (param2 > 0)
                        {
                            success = true;
                        }
                    }
                    break;
                case QuestObjectiveType.MultipleObj:
                case QuestObjectiveType.Empty:
                case QuestObjectiveType.QuickTalk:
                    success = true;
                    break;
            }

            if (success)
            {
                objectiveData.Count += param2;
            }
            
            bool req_success = UpdateRequirementProgress(ref objectiveData, player);
            if (!success && req_success)
            {
                success = true;
            }

            return success;
        }

        #region Check Objective Progression
        public static bool IsObjectivesCompleted(int questid, List<int> idlist, List<int> progresscount, Player player)
        {
            bool success = false;
            QuestJson questJson = QuestRepo.GetQuestByID(questid);
            if (!questJson.isopen)
            {
                return success;
            }

            int requirementcheck = 0;
            for (int i = 0; i < idlist.Count; i++)
            {
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(idlist[i]);
                if (RequirementListChecking(objectiveJson.requirementid, player))
                {
                    requirementcheck += 1;
                }
            }
            if (requirementcheck >= idlist.Count)
            {
                success = true;
            }

            if (success)
            {
                int completedCount = 0;
                for (int i = 0; i < idlist.Count; i++)
                {
                    if (IsObjectiveCompleted(idlist[i], progresscount[i]))
                    {
                        completedCount += 1;
                    }
                }
                if (completedCount >= idlist.Count)
                {
                    success = true;
                }
                else
                {
                    success = false;
                }
            }

            if (success)
            {
                int recovercheck = 0;
                for (int i = 0; i < idlist.Count; i++)
                {
                    QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(idlist[i]);
                    if (RecoverListChecking(objectiveJson.requirementid, player))
                    {
                        recovercheck += 1;
                    }
                }
                if (recovercheck >= idlist.Count)
                {
                    success = true;
                }
                else
                {
                    success = false;
                }
            }

            return success;
        }

        private static bool IsObjectiveCompleted(int objectiveid, int count)
        {
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
            switch(objectiveJson.type)
            {
                case QuestObjectiveType.Kill:
                case QuestObjectiveType.RealmComplete:
                case QuestObjectiveType.PercentageKill:
                case QuestObjectiveType.Interact:
                    if (count >= objectiveJson.para2)
                    {
                        return true;
                    }
                    return false;
                case QuestObjectiveType.Talk:
                case QuestObjectiveType.QuickTalk:
                case QuestObjectiveType.Choice:
                case QuestObjectiveType.Empty:
                case QuestObjectiveType.MultipleObj:
                    if (count >= 1)
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }
        #endregion

        public static CurrentQuestData StartNextMainObjective(CurrentQuestData questData, long currenttime, Player player)
        {
            NextObjectiveData objectiveData = QuestRepo.GetNextObjectiveId(questData.QuestId, questData.GroupdId, questData.MainObjective.SequenceNum);
            if (objectiveData.ReturnCode == "End")
            {
                questData.MainObjective.ProgressCount = new List<int>();
                questData.MainObjective.CompleteTime = new List<long>();
                questData.MainObjective.RequirementProgress = new Dictionary<int, int>();
                if (QuestRepo.CheckQuestEventByQuestId(questData.QuestId, true))
                {
                    questData.Status = (byte)QuestStatus.CompletedWithEvent;
                }
                else
                {
                    questData.Status = (byte)QuestStatus.CompletedAllObjective;
                }
            }
            else if (objectiveData.ReturnCode == "Error")
            {
                questData.Status = (byte)QuestStatus.Error;
            }
            else
            {
                List<int> objectiveids = questData.MainObjective.ObjectiveIds;
                questData.MainObjective.ObjectiveIds = objectiveData.ObjectiveList;
                questData.MainObjective.SequenceNum += 1;
                questData.MainObjective.ProgressCount = new List<int>(questData.MainObjective.ObjectiveIds.Count);
                questData.MainObjective.RequirementProgress = new Dictionary<int, int>();
                for (int i = 0; i < questData.MainObjective.ObjectiveIds.Count; i++)
                {
                    questData.MainObjective.ProgressCount.Add(0);
                    Dictionary<int, int> requirements = GenerateRequirementProgress(questData.MainObjective.ObjectiveIds[i], player);
                    foreach (KeyValuePair<int, int> requirement in requirements)
                    {
                        questData.MainObjective.RequirementProgress.Add(requirement.Key, requirement.Value);
                    }
                }
                questData.MainObjective.CompleteTime = GetObjectiveEndTime(objectiveData.ObjectiveList, currenttime);
                bool eventresult = false;
                foreach (int objectiveid in objectiveids)
                {
                    eventresult = QuestRepo.CheckQuestEventByObjectiveId(objectiveid);
                    if (eventresult)
                    {
                        break;
                    }
                }
                questData.Status = eventresult ? (byte)QuestStatus.NewObjectiveWithEvent : (byte)QuestStatus.NewObjective;

                questData.SubObjective = new Dictionary<int, CurrentObjectiveData>();
                for (int i = 0; i < questData.MainObjective.ObjectiveIds.Count; i++)
                {
                    int id = questData.MainObjective.ObjectiveIds[i];
                    CurrentObjectiveData subobjective = new CurrentObjectiveData();
                    subobjective.ObjectiveIds = QuestRepo.GetSubObjectiveId(id, 0);
                    if (subobjective.ObjectiveIds != null && subobjective.ObjectiveIds.Count > 0)
                    {
                        subobjective.SequenceNum = 0;
                        subobjective.ProgressCount = new List<int>(subobjective.ObjectiveIds.Count);
                        subobjective.RequirementProgress = new Dictionary<int, int>();
                        for (int j = 0; j < subobjective.ObjectiveIds.Count; j++)
                        {
                            subobjective.ProgressCount.Add(0);
                            Dictionary<int, int> requirements = GenerateRequirementProgress(subobjective.ObjectiveIds[i], player);
                            foreach (KeyValuePair<int, int> requirement in requirements)
                            {
                                subobjective.RequirementProgress.Add(requirement.Key, requirement.Value);
                            }
                        }
                        subobjective.CompleteTime = GetObjectiveEndTime(subobjective.ObjectiveIds, currenttime);
                        questData.SubObjective.Add(id, subobjective);
                    }
                }
            }
            return questData;
        }

        public static int StartNextSubObjective(int mainid, ref CurrentObjectiveData objectiveData, long currenttime, Player player)
        {
            List<int> objectiveids = objectiveData.ObjectiveIds;
            NextObjectiveData newobjectiveData = QuestRepo.GetNextSubObjectiveId(mainid, objectiveData.SequenceNum);
            if (newobjectiveData.ReturnCode == "End")
            {
                objectiveData.ObjectiveIds = new List<int>();
                objectiveData.ProgressCount = new List<int>();
                objectiveData.CompleteTime = new List<long>();
                bool eventresult = false;
                foreach (int objectiveid in objectiveids)
                {
                    eventresult = QuestRepo.CheckQuestEventByObjectiveId(objectiveid);
                    if (eventresult)
                    {
                        return 1;
                    }
                }
                return 2;
            }
            else if (newobjectiveData.ReturnCode == "Error")
            {
                return 0;
            }
            else
            {
                objectiveData.ObjectiveIds = newobjectiveData.ObjectiveList;
                objectiveData.SequenceNum += 1;
                objectiveData.ProgressCount = new List<int>(objectiveData.ObjectiveIds.Count);
                objectiveData.RequirementProgress = new Dictionary<int, int>();
                for (int i = 0; i < objectiveData.ObjectiveIds.Count; i++)
                {
                    objectiveData.ProgressCount.Add(0);
                    Dictionary<int, int> requirements = GenerateRequirementProgress(objectiveData.ObjectiveIds[i], player);
                    foreach (KeyValuePair<int, int> requirement in requirements)
                    {
                        objectiveData.RequirementProgress.Add(requirement.Key, requirement.Value);
                    }
                }
                objectiveData.CompleteTime = GetObjectiveEndTime(newobjectiveData.ObjectiveList, currenttime);
                bool eventresult = false;
                foreach (int objectiveid in objectiveids)
                {
                    eventresult = QuestRepo.CheckQuestEventByObjectiveId(objectiveid);
                    if (eventresult)
                    {
                        return 3;
                    }
                }
                return 4;
            }
        }

        //Check Accept Quest Requirement
        public static bool AcceptQuest(int questid, int callerid, List<int> completedquest, int questcount, Player player)
        {
            bool canAccept = false;
            QuestJson questJson = QuestRepo.GetQuestByID(questid);

            if (!questJson.isopen)
            {
                return canAccept;
            }

            if (questcount >= QuestRepo.GetMaxQuestCountByType(questJson.type))
            {
                return canAccept;
            }

            if (player.PlayerSynStats.Level < questJson.minlv)
            {
                return canAccept;
            }
             
            if (questJson.type != QuestType.Main)
            {
                if (completedquest.Contains(questid))
                {
                    return canAccept;
                }
            }
            
            switch(questJson.triggertype)
            {
                case QuestTriggerType.NPC:
                case QuestTriggerType.Item:
                case QuestTriggerType.Interact:
                case QuestTriggerType.Signboard:
                case QuestTriggerType.Hero:
                    if (questJson.triggercaller != callerid)
                    {
                        return canAccept;
                    }
                    break;
                case QuestTriggerType.Level:
                    if (callerid < questJson.triggercaller)
                    {
                        return canAccept;
                    }
                    break;
            }

            canAccept = RequirementListChecking(questJson.requirementid, player);

            if (canAccept)
            {
                canAccept = RecoverListChecking(questJson.requirementid, player);
            }
            return canAccept;
        }

        public static CurrentQuestData StartNewQuest(int questid, int group, long currentime, Player player)
        {
            QuestJson questJson = QuestRepo.GetQuestByID(questid);
            if (!questJson.isopen)
            {
                return null;
            }

            CurrentQuestData questData = new CurrentQuestData(questJson.type);
            questData.QuestId = questJson.questid;

            questData.MainObjective = new CurrentObjectiveData();
            questData.GroupdId = group;
            questData.MainObjective.ObjectiveIds = QuestRepo.GetObjectiveByGroup(questid, group, 0);
            questData.MainObjective.SequenceNum = 0;
            questData.MainObjective.ProgressCount = new List<int>(questData.MainObjective.ObjectiveIds.Count);
            questData.MainObjective.RequirementProgress = new Dictionary<int, int>();
            for (int i = 0; i < questData.MainObjective.ObjectiveIds.Count; i++)
            {
                questData.MainObjective.ProgressCount.Add(0);
                Dictionary<int, int> requirements = GenerateRequirementProgress(questData.MainObjective.ObjectiveIds[i], player);
                foreach(KeyValuePair<int, int>requirement in requirements)
                {
                    questData.MainObjective.RequirementProgress.Add(requirement.Key, requirement.Value);
                }
            }
            questData.MainObjective.CompleteTime = GetObjectiveEndTime(questData.MainObjective.ObjectiveIds, currentime);
            if (QuestRepo.CheckQuestEventByQuestId(questid, false))
            {
                questData.Status = (byte)QuestStatus.NewQuestWithEvent;
            }
            else
            {
                questData.Status = (byte)QuestStatus.NewQuest;
            }

            questData.SubObjective = new Dictionary<int, CurrentObjectiveData>();
            for (int i = 0; i < questData.MainObjective.ObjectiveIds.Count; i++)
            {
                int id = questData.MainObjective.ObjectiveIds[i];
                CurrentObjectiveData objectiveData = new CurrentObjectiveData();
                objectiveData.ObjectiveIds = QuestRepo.GetSubObjectiveId(id, 0);
                if (objectiveData.ObjectiveIds != null && objectiveData.ObjectiveIds.Count > 0)
                {
                    objectiveData.SequenceNum = 0;
                    objectiveData.ProgressCount = new List<int>(objectiveData.ObjectiveIds.Count);
                    objectiveData.RequirementProgress = new Dictionary<int, int>();
                    for (int j = 0; j < objectiveData.ObjectiveIds.Count; j++)
                    {
                        objectiveData.ProgressCount.Add(0);
                        Dictionary<int, int> requirements = GenerateRequirementProgress(objectiveData.ObjectiveIds[i], player);
                        foreach (KeyValuePair<int, int> requirement in requirements)
                        {
                            if (!objectiveData.RequirementProgress.ContainsKey(requirement.Key))
                            {
                                objectiveData.RequirementProgress.Add(requirement.Key, requirement.Value);
                            }
                        }
                    }
                    objectiveData.CompleteTime = GetObjectiveEndTime(objectiveData.ObjectiveIds, currentime);
                    questData.SubObjective.Add(id, objectiveData);
                }
            }
            questData.SubStatus = (byte)QuestStatus.Non;
            return questData;
        }

        private static Dictionary<int, int> GenerateRequirementProgress(int objectiveid, Player player)
        {
            Dictionary<int, int> progress = new Dictionary<int, int>();
            QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(objectiveid);
            if (objectiveJson != null)
            {
                List<QuestRequirementDetailJson> requirementDetailJsons = QuestRepo.GetRequirementByGroupId(objectiveJson.requirementid);
                if (requirementDetailJsons != null)
                {
                    foreach (QuestRequirementDetailJson requirementDetailJson in requirementDetailJsons)
                    {
                        if (requirementDetailJson.type == QuestRequirementType.Level)
                        {
                            progress.Add(requirementDetailJson.requirementid, player.PlayerSynStats.Level);
                        }
                        else if (requirementDetailJson.type == QuestRequirementType.Item)
                        {
                            progress.Add(requirementDetailJson.requirementid, player.Slot.mInventory.GetItemStackCountByItemId((ushort)requirementDetailJson.para1));
                        }
                        else if (requirementDetailJson.type == QuestRequirementType.Equipment)
                        {
                            progress.Add(requirementDetailJson.requirementid, 0);
                        }
                        else if (requirementDetailJson.type == QuestRequirementType.Hero)
                        {
                            int heroprogress = 0;
                            if (requirementDetailJson.para2 == 1 && player.HeroStats.IsHeroSummoned(requirementDetailJson.para1))
                            {
                                heroprogress = 1;
                            }
                            else if (requirementDetailJson.para2 == 2 && player.HeroStats.IsHeroSummoned(requirementDetailJson.para1))
                            {
                                heroprogress = 1;
                            }
                            progress.Add(requirementDetailJson.requirementid, heroprogress);
                        }
                        else if (requirementDetailJson.type == QuestRequirementType.Title)
                        {
                            progress.Add(requirementDetailJson.requirementid, 0);
                        }
                        else if (requirementDetailJson.type == QuestRequirementType.SideEffect)
                        {
                            progress.Add(requirementDetailJson.requirementid, 0);
                        }
                        else if (requirementDetailJson.type == QuestRequirementType.Companian)
                        {
                            progress.Add(requirementDetailJson.requirementid, 0);
                        }
                        else if (requirementDetailJson.type == QuestRequirementType.Clue)
                        {
                            progress.Add(requirementDetailJson.requirementid, 0);
                        }
                        else if (requirementDetailJson.type == QuestRequirementType.Outfit)
                        {
                            progress.Add(requirementDetailJson.requirementid, 0);
                        }
                        else if (requirementDetailJson.type == QuestRequirementType.Job)
                        {
                            progress.Add(requirementDetailJson.requirementid, player.PlayerSynStats.jobsect);
                        }
                    }
                }
            }
            return progress;
        }

        private static bool UpdateRequirementProgress(ref QuestObjectiveData objectiveData, Player player)
        {
            bool updated = false;
            List<int> idlist = objectiveData.Requirement.Keys.ToList();
            foreach (int id in idlist)
            {
                int result = UpdateRequirementProgressCount(id, player);
                if (result != objectiveData.Requirement[id])
                {
                    updated = true;
                }
                objectiveData.Requirement[id] = result;
            }
            return updated;
        }

        private static int UpdateRequirementProgressCount(int requirementid, Player player)
        {
            QuestRequirementDetailJson questRequirement = QuestRepo.GetQuestRequirementyById(requirementid);
            if (questRequirement != null)
            {
                switch(questRequirement.type)
                {
                    case QuestRequirementType.Level:
                        return player.PlayerSynStats.Level;
                    case QuestRequirementType.Item:
                        return player.Slot.mInventory.GetItemStackCountByItemId((ushort)questRequirement.para1);
                    case QuestRequirementType.Equipment:
                        return 0;
                    case QuestRequirementType.Hero:
                        if (questRequirement.para2 == 1 && player.HeroStats.IsHeroSummoned(questRequirement.para1))
                        {
                            return 1;
                        }
                        else if (questRequirement.para2 == 2 && player.HeroStats.IsHeroUnlocked(questRequirement.para1))
                        {
                            return 1;
                        }
                        return 0;
                    case QuestRequirementType.Title:
                        return 0;
                    case QuestRequirementType.SideEffect:
                        return 0;
                    case QuestRequirementType.Companian:
                        return 0;
                    case QuestRequirementType.Clue:
                        return 0;
                    case QuestRequirementType.Outfit:
                        return 0;
                    case QuestRequirementType.Job:
                        return player.PlayerSynStats.jobsect;
                }
            }
            return 0;
        }

        private static List<long> GetObjectiveEndTime(List<int> objectiveid, long currenttime)
        {
            List<long> endtime = new List<long>(objectiveid.Count);
            for (int i = 0; i < objectiveid.Count; i++)
            {
                long time = QuestRepo.GetObjectiveEndTime(objectiveid[i]);
                if (time > 0)
                {
                    endtime.Add(currenttime + time);
                }
                else
                {
                    endtime.Add(0);
                }
            }
            return endtime;
        }

        public static bool RollBackQuestObjective(Player player, long currentime, CurrentQuestData questData, ref CurrentQuestData newQuestData)
        {
            int mainObjectiveId = -1;
            foreach(int objid in questData.MainObjective.ObjectiveIds)
            {
                if (objid != -1)
                {
                    mainObjectiveId = objid;
                    break;
                }
            }

            if (mainObjectiveId != -1)
            {
                QuestObjectiveJson objectiveJson = QuestRepo.GetQuestObjectiveByID(mainObjectiveId);
                if (objectiveJson != null && objectiveJson.type == QuestObjectiveType.MultipleObj)
                {
                    newQuestData = questData;
                    newQuestData.MainObjective.RequirementProgress = new Dictionary<int, int>();
                    for (int i = 0; i < questData.MainObjective.ObjectiveIds.Count; i++)
                    {
                        newQuestData.MainObjective.ProgressCount[i] = 0;
                        Dictionary<int, int> requirements = GenerateRequirementProgress(newQuestData.MainObjective.ObjectiveIds[i], player);
                        foreach (KeyValuePair<int, int> requirement in requirements)
                        {
                            newQuestData.MainObjective.RequirementProgress[requirement.Key] = requirement.Value;
                        }
                    }
                    newQuestData.MainObjective.CompleteTime = GetObjectiveEndTime(questData.MainObjective.ObjectiveIds, currentime);

                    int seqnum = QuestRepo.GetSubObjectiveSeqNum(objectiveJson.para1);
                    newQuestData.SubObjective = new Dictionary<int, CurrentObjectiveData>();
                    for (int i = 0; i < newQuestData.MainObjective.ObjectiveIds.Count; i++)
                    {
                        int id = newQuestData.MainObjective.ObjectiveIds[i];
                        CurrentObjectiveData objectiveData = new CurrentObjectiveData();
                        objectiveData.ObjectiveIds = QuestRepo.GetSubObjectiveId(id, seqnum);
                        if (objectiveData.ObjectiveIds != null && objectiveData.ObjectiveIds.Count > 0)
                        {
                            objectiveData.SequenceNum = seqnum;
                            objectiveData.ProgressCount = new List<int>(objectiveData.ObjectiveIds.Count);
                            objectiveData.RequirementProgress = new Dictionary<int, int>();
                            for (int j = 0; j < objectiveData.ObjectiveIds.Count; j++)
                            {
                                objectiveData.ProgressCount.Add(0);
                                Dictionary<int, int> requirements = GenerateRequirementProgress(objectiveData.ObjectiveIds[i], player);
                                foreach (KeyValuePair<int, int> requirement in requirements)
                                {
                                    objectiveData.RequirementProgress.Add(requirement.Key, requirement.Value);
                                }
                            }
                            objectiveData.CompleteTime = GetObjectiveEndTime(objectiveData.ObjectiveIds, currentime);
                            newQuestData.SubObjective.Add(id, objectiveData);
                        }
                    }
                    newQuestData.SubStatus = (byte)QuestStatus.Non;
                }
                return true;
            }
            return false;
        }

        #region Requirement Check
        private static bool RequirementListChecking(int requirementid, Player player)
        {
            List<QuestRequirementDetailJson> requirementList = QuestRepo.GetRequirementByGroupId(requirementid);
            if (requirementList != null)
            {
                int passcount = 0;
                foreach (QuestRequirementDetailJson requirement in requirementList)
                {
                    bool result = CheckRequirement(requirement, player);
                    passcount = result ? passcount + 1 : passcount;
                }

                if (passcount >= requirementList.Count)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CheckRequirement(QuestRequirementDetailJson requirement, Player player)
        {
            switch(requirement.type)
            {
                case QuestRequirementType.Level:
                    if (player.PlayerSynStats.Level >= requirement.para1)
                    {
                        return true;
                    }
                    break;
                case QuestRequirementType.Item:
                    if (player.Slot.mInventory.HasItem((ushort)requirement.para1, requirement.para2))
                    {
                        return true;
                    }
                    break;
                case QuestRequirementType.Equipment:
                    Equipment equipment = player.Slot.CharacterData.EquipmentInventory.Slots.Where(o => o.ItemID == requirement.para1).First();
                    if (equipment != null)
                    {
                        if (equipment.UpgradeLevel >= requirement.para3)
                        {
                            return true;
                        }
                    }
                    break;
                case QuestRequirementType.Hero:
                    if (requirement.para2 == 1 && player.HeroStats.IsHeroSummoned(requirement.para1))
                    {
                        return true;
                    }
                    else if (requirement.para2 == 2 && player.HeroStats.IsHeroUnlocked(requirement.para1))
                    {
                        return true;
                    }
                    break;
                case QuestRequirementType.Title:
                    return true;
                case QuestRequirementType.SideEffect:
                    return true;
                case QuestRequirementType.Companian:
                    return true;
                case QuestRequirementType.Clue:
                    return true;
                case QuestRequirementType.Outfit:
                    return true;
                case QuestRequirementType.Job:
                    if (requirement.para2 == 1 && player.PlayerSynStats.jobsect == requirement.para1)
                    {
                        return true;
                    }
                    else if (requirement.para2 == 2 && player.PlayerSynStats.jobsect != requirement.para1)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        public static bool CanAddIntoUnlock(QuestJson questJson, int jobsec)
        {
            List<QuestRequirementDetailJson> requirementList = QuestRepo.GetRequirementByGroupId(questJson.requirementid);
            if (requirementList != null)
            {
                foreach (QuestRequirementDetailJson requirement in requirementList)
                {
                    if (requirement.type == QuestRequirementType.Job)
                    {
                        if (requirement.para2 == 1 && requirement.para1 != jobsec)
                        {
                            return false;
                        }
                        else if (requirement.para2 == 2 && requirement.para1 == jobsec)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        #endregion

        #region Requirement Type Recover Check
        private static bool RecoverListChecking(int requirementid, Player player)
        {
            List<QuestRequirementDetailJson> requirementList = QuestRepo.GetRequirementByGroupId(requirementid);
            if (requirementList != null)
            {
                int passcount = 0;
                foreach (QuestRequirementDetailJson requirement in requirementList)
                {
                    bool result = CheckRecover(requirement, player);
                    passcount = result ? passcount + 1 : passcount;
                }

                if (passcount >= requirementList.Count)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CheckRecover(QuestRequirementDetailJson requirement, Player player)
        {
            switch (requirement.type)
            {
                case QuestRequirementType.Level:
                case QuestRequirementType.Equipment:
                    return true;
                case QuestRequirementType.Item:
                    if (requirement.para3 > 0)
                    {
                        InvRetval retval = player.Slot.mInventory.DeductItem((ushort)requirement.para1, (ushort)requirement.para2, "Quest");
                        if (retval.retCode == InvReturnCode.UseFailed)
                        {
                            return false;
                        }
                    }
                    return true;
                case QuestRequirementType.Hero:
                    return true;
                case QuestRequirementType.Title:
                    return true;
                case QuestRequirementType.SideEffect:
                    return true;
                case QuestRequirementType.Companian:
                    return true;
                case QuestRequirementType.Clue:
                    return true;
                case QuestRequirementType.Outfit:
                    return true;
                case QuestRequirementType.Job:
                    return true;
            }
            return false;
        }
    #endregion        

        public static string SerializedCompletedList(List<int> completedList)
        {
            return JsonConvertDefaultSetting.SerializeObject(completedList);
        }

        public static string SerializedQuestStats(CurrentQuestData questData)
        {
            return SerializedQuestStats(questData == null ? new QuestDataStats() : new QuestDataStats(questData));
        }

        public static Dictionary<int, string> SerializedQuestStats(Dictionary<int, CurrentQuestData> questData)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            foreach (KeyValuePair<int, CurrentQuestData> entry in questData)
            {
                result.Add(entry.Key, SerializedQuestStats(new QuestDataStats(entry.Value)));
            }
            return result;
        }

        public static string SerializedQuestStats(QuestDataStats questDataStats)
        {
            return JsonConvertDefaultSetting.SerializeObject(questDataStats);
        }
    }
}