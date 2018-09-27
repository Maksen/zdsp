using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.Entities;

namespace Photon.LoadBalancing.GameServer
{
    public class AchievementStatsServer : AchievementStats
    {
        private GameClientPeer peer;
        private Player player;

        private StringBuilder sb = new StringBuilder();

        public void Init(Player _player, GameClientPeer _peer, AchievementInvData invData)
        {
            player = _player;
            peer = _peer;

            ParseCollections(invData.Collections);
            ParseAchievements(invData.Achievements);
            RewardClaims = invData.RewardClaims;
            ParseRewardClaims();
            LatestCollections = invData.LatestCollections;
            LatestAchievements = invData.LatestAchievements;
            ParseLatestRecords(latestCollectionsList, LatestCollections);
            ParseLatestRecords(latestAchievementList, LatestAchievements);
        }

        private void ParseCollections(string value)
        {
            collectionsDict.Clear();
            if (!string.IsNullOrEmpty(value))
            {
                List<StringBuilder> sblist = new List<StringBuilder>();
                int length = Enum.GetNames(typeof(CollectionType)).Length;
                for (int i = 0; i < length; ++i)
                    sblist.Add(new StringBuilder());

                string[] colArray = value.Split('|');
                for (int i = 0; i < colArray.Length; ++i)
                {
                    string[] colData = colArray[i].Split(';');
                    int id = int.Parse(colData[0]);
                    DateTime date = DateTime.ParseExact(colData[1], "yyyy/MM/dd", CultureInfo.InvariantCulture);
                    string photodesc = "";
                    if (colData.Length > 2)
                        photodesc = colData[2];
                    CollectionObjective obj = AchievementRepo.GetCollectionObjectiveById(id);
                    if (obj != null)
                    {
                        CollectionElement elem = new CollectionElement(id, date, photodesc);
                        collectionsDict.Add(id, elem);
                        int index = (int)obj.type;
                        sblist[index].AppendFormat("{0}|", colArray[i]);
                    }
                }

                for (int i = 0; i < length; ++i)
                    Collections[i] = sblist[i].ToString().TrimEnd('|');
            }
        }

        private void ParseAchievements(string value)
        {
            achievementsDict.Clear();
            if (!string.IsNullOrEmpty(value))
            {
                List<StringBuilder> sblist = new List<StringBuilder>();
                int length = AchievementRepo.achievementMainTypes.Count;
                for (int i = 0; i < length; ++i)
                    sblist.Add(new StringBuilder());

                string[] achArray = value.Split('|');
                for (int i = 0; i < achArray.Length; ++i)
                {
                    string[] achData = achArray[i].Split(';');
                    int id = int.Parse(achData[0]);
                    int count = int.Parse(achData[1]);
                    AchievementObjective obj = AchievementRepo.GetAchievementObjectiveById(id);
                    if (obj != null)
                    {
                        AchievementElement elem = new AchievementElement(id, count, obj.completeCount, obj.slotIdx);
                        achievementsDict.Add(id, elem);
                        sblist[obj.slotIdx].AppendFormat("{0}|", achArray[i]);
                    }
                }

                for (int i = 0; i < length; ++i)
                    Achievements[i] = sblist[i].ToString().TrimEnd('|');
            }
        }

        private void ParseRewardClaims()
        {
            claimsList.Clear();
            if (!string.IsNullOrEmpty(RewardClaims))
            {
                string[] rcArray = RewardClaims.Split('|');
                for (int i = 0; i < rcArray.Length; ++i)
                {
                    string[] rcData = rcArray[i].Split(';');
                    AchievementType type = (AchievementType)int.Parse(rcData[0]);
                    int id = int.Parse(rcData[1]);
                    AchievementRewardClaim claim = new AchievementRewardClaim(type, id);
                    claimsList.Add(claim);
                }
            }
        }

        private void ParseLatestRecords(List<AchievementRecord> list, string value)
        {
            list.Clear();
            if (!string.IsNullOrEmpty(value))
            {
                string[] rArray = value.Split('|');
                for (int i = 0; i < rArray.Length; ++i)
                {
                    string[] rData = rArray[i].Split(';');
                    int id = int.Parse(rData[0]);
                    DateTime date = DateTime.ParseExact(rData[1], "yyyy/MM/dd", CultureInfo.InvariantCulture);
                    AchievementRecord record = new AchievementRecord(id, date);
                    list.Add(record);
                }
            }
        }

        public string CollectionsToString()
        {
            foreach (var item in collectionsDict)
            {
                sb.Append(item.Key);
                sb.AppendFormat(";{0}", item.Value.CollectDate.ToString("yyyy/MM/dd"));
                if (!string.IsNullOrEmpty(item.Value.PhotoDesc))
                    sb.AppendFormat(";{0}", item.Value.PhotoDesc);
                sb.Append("|");
            }
            string collectionString = sb.ToString().TrimEnd('|');
            sb.Clear();
            return collectionString;
        }

        public string AchievementsToString()
        {
            foreach (var item in achievementsDict)
                sb.AppendFormat("{0};{1}|", item.Key, item.Value.Count);
            string achString = sb.ToString().TrimEnd('|');
            sb.Clear();
            return achString;
        }

        private void UpdateRewardClaimsString()
        {
            for (int i = 0; i < claimsList.Count; ++i)
                sb.AppendFormat("{0};{1}|", (int)claimsList[i].ClaimType, claimsList[i].Id);
            RewardClaims = sb.ToString().TrimEnd('|');
            sb.Clear();
        }

        private void UpdateRecordsString(AchievementType type)
        {
            if (type == AchievementType.Collection)
            {
                for (int i = 0; i < latestCollectionsList.Count; ++i)
                    sb.AppendFormat("{0};{1}|",latestCollectionsList[i].Id, latestCollectionsList[i].CompleteDate.ToString("yyyy/MM/dd"));
                LatestCollections = sb.ToString().TrimEnd('|');
            }
            else
            {
                for (int i = 0; i < latestAchievementList.Count; ++i)
                    sb.AppendFormat("{0};{1}|", latestAchievementList[i].Id, latestAchievementList[i].CompleteDate.ToString("yyyy/MM/dd"));
                LatestAchievements = sb.ToString().TrimEnd('|');
            }
            sb.Clear();
        }

        public void UpdateCollection(CollectionType objType, int target)
        {
            CollectionObjective obj = AchievementRepo.GetCollectionObjectiveByKey(objType, target);
            if (obj != null && !collectionsDict.ContainsKey(obj.id))
            {
                string info = "";
                if (objType == CollectionType.Photo)
                {
                    if (player.IsInParty())
                        info = AchievementRepo.GetRandomPhotoDescription(player.PartyStats.MemberCount(false));
                    else
                        info = AchievementRepo.GetRandomPhotoDescription(1);
                }
                DateTime now = DateTime.Now;
                CollectionElement elem = new CollectionElement(obj.id, now, info);
                collectionsDict.Add(obj.id, elem);

                int index = (int)objType;
                string oldString = (string)Collections[index];
                Collections[index] = string.IsNullOrEmpty(oldString) ? elem.ToString() : oldString + "|" + elem.ToString();

                AddToRewardClaims(AchievementType.Collection, obj.id);
                UpdateRewardClaimsString();
                AddToLatestRecords(AchievementType.Collection, obj.id, now);
                UpdateRecordsString(AchievementType.Collection);
            }
        }

        public void UpdateAchievement(AchievementObjectiveType objType, string target = "-1", int count = 1, bool increment = true, bool debug = false)
        {
            if (count <= 0 && !debug)
                return;

            bool hasNewlyCompleted = false;
            int dirtySlot = -1;
            List<AchievementObjective> achList = AchievementRepo.GetAchievementObjectivesByKey(objType, target);
            if (achList != null)
            {
                int listCount = achList.Count;
                for (int i = 0; i < listCount; ++i)
                {
                    AchievementObjective obj = achList[i];
                    AchievementElement elem = GetAchievementById(obj.id);
                    bool isAlreadyCompleted = false;
                    if (elem != null) // existing achievement
                    {
                        isAlreadyCompleted = elem.IsCompleted();
                        if (isAlreadyCompleted)
                            continue;
                        elem.UpdateCount(count, increment);
                        dirtySlot = obj.slotIdx;  // every objective in group should have same slot                        
                    }
                    else  // new achievement
                    {
                        elem = new AchievementElement(obj.id, count, obj.completeCount, obj.slotIdx);
                        achievementsDict.Add(obj.id, elem);
                        dirtySlot = obj.slotIdx;  // every objective in group should have same slot
                    }

                    if (!isAlreadyCompleted && elem.IsCompleted())  // just completed this achievement so can claim
                    {
                        AddToRewardClaims(AchievementType.Achievement, obj.id);
                        AddToLatestRecords(AchievementType.Achievement, obj.id, DateTime.Now);
                        hasNewlyCompleted = true;
                    }
                }
            }

            if (dirtySlot != -1)  // rebuild dirty slot in collectionhandler
            {
                foreach (var item in achievementsDict)
                {
                    if (item.Value.SlotIdx == dirtySlot)
                        sb.AppendFormat("{0};{1}|", item.Key, item.Value.Count);
                }
                Achievements[dirtySlot] = sb.ToString().TrimEnd('|');
                sb.Clear();
            }

            if (hasNewlyCompleted) // has completed new ones so need update string
            {
                UpdateRewardClaimsString();
                UpdateRecordsString(AchievementType.Achievement);
            }
        }

        private void AddToRewardClaims(AchievementType type, int id)
        {
            AchievementRewardClaim newClaim = new AchievementRewardClaim(type, id);
            claimsList.Add(newClaim);
        }

        private void AddToLatestRecords(AchievementType type, int id, DateTime date)
        {
            AchievementRecord newRecord = new AchievementRecord(id, date);
            if (type == AchievementType.Collection)
            {
                latestCollectionsList.Add(newRecord);
                if (latestCollectionsList.Count > AchievementData.MAX_RECORDS) // remove oldest record if more then max limit
                    latestCollectionsList.RemoveAt(0);
            }
            else
            {
                latestAchievementList.Add(newRecord);
                if (latestAchievementList.Count > AchievementData.MAX_RECORDS) // remove oldest record if more then max limit
                    latestAchievementList.RemoveAt(0);
            }
        }

        public void ClaimReward(AchievementType type, int id)
        {
            AchievementRewardClaim claim = claimsList.Find(x => x.ClaimType == type && x.Id == id);
            if (claim != null)
            {
                claimsList.Remove(claim);
                UpdateRewardClaimsString();
            }
        }

        public void ClaimAllRewards()
        {
            for (int i = 0; i < claimsList.Count; ++i)
            {
            }
        }

#if DEBUG
        public void ConsoleResetCollections()
        {
            collectionsDict.Clear();
            Collections.ResetAll();
        }

        public void ConsoleGetAllCollections(int objtype)
        {
            if (objtype == -1)
            {
                ConsoleResetCollections();
                foreach (var obj in AchievementRepo.collectionObjectives.Values)
                {
                    CollectionElement elem = new CollectionElement(obj.id, DateTime.Now, "");
                    collectionsDict.Add(obj.id, elem);

                    int index = (int)obj.type;
                    string oldString = (string)Collections[index];
                    Collections[index] = string.IsNullOrEmpty(oldString) ? elem.ToString() : oldString + "|" + elem.ToString();
                    AddToRewardClaims(AchievementType.Collection, obj.id);
                }
            }
            else
            {
                var list = AchievementRepo.GetCollectionObjectivesByType((CollectionType)objtype);
                foreach (var obj in list)
                {
                    if (GetCollectionById(obj.id) == null)
                    {
                        CollectionElement elem = new CollectionElement(obj.id, DateTime.Now, "");
                        collectionsDict.Add(obj.id, elem);
                        string oldString = (string)Collections[objtype];
                        Collections[objtype] = string.IsNullOrEmpty(oldString) ? elem.ToString() : oldString + "|" + elem.ToString();
                        AddToRewardClaims(AchievementType.Collection, obj.id);
                    }
                }
            }
            UpdateRewardClaimsString();
        }

        public void ConsoleResetAchievements()
        {
            achievementsDict.Clear();
            Achievements.ResetAll();
        }

        public void ConsoleGetAllAchievements()
        {
            ConsoleResetAchievements();

            foreach (var obj in AchievementRepo.achievementObjectives.Values)
            {
                AchievementElement elem = new AchievementElement(obj.id, obj.completeCount, obj.completeCount, obj.slotIdx);
                achievementsDict.Add(obj.id, elem);
                string oldString = (string)Achievements[obj.slotIdx];
                Achievements[obj.slotIdx] = string.IsNullOrEmpty(oldString) ? elem.ToString() : oldString + "|" + elem.ToString();
                AddToRewardClaims(AchievementType.Achievement, obj.id);
            }
            UpdateRewardClaimsString();
        }
#endif
    }
}