using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;

public class AchievementStatsClient : AchievementStats
{
    private GameObject windowObj;
    private UI_Achievement uiAchieve;
    private PlayerGhost player;

    public void Init()
    {
        windowObj = UIManager.GetWindowGameObject(WindowType.Achievement);
        uiAchieve = windowObj.GetComponent<UI_Achievement>();
        player = GameInfo.gLocalPlayer;

        InitCollections();
        InitAchievements();
    }

    private void InitCollections()
    {
        for (int idx = 0; idx < Collections.Count; ++idx)
        {
            if (Collections[idx] == null)
                continue;
            string info = (string)Collections[idx];
            if (string.IsNullOrEmpty(info))
                continue;

            string[] colArray = info.Split('|');
            for (int i = 0; i < colArray.Length; ++i)
            {
                string[] colData = colArray[i].Split(';');
                int id = int.Parse(colData[0]);
                DateTime date = DateTime.ParseExact(colData[1], "yyyy/MM/dd", CultureInfo.InvariantCulture);
                bool claimed = int.Parse(colData[2]) == 1;
                string photodesc = "";
                bool stored = false;
                if (colData.Length > 3)
                {
                    int isStored;
                    if (int.TryParse(colData[3], out isStored) && isStored == 1)
                        stored = true;
                    else
                        photodesc = colData[3];
                }

                CollectionElement elem = new CollectionElement(id, date, claimed, photodesc, stored, idx);
                collectionsDict.Add(id, elem);
            }
        }
    }

    public void UpdateCollections(byte idx, string value)
    {
        if (string.IsNullOrEmpty(value))
            return;
        //Debug.Log("col index: " + idx + "-" + value);

        string[] colArray = value.Split('|');
        for (int i = 0; i < colArray.Length; ++i)
        {
            string[] colData = colArray[i].Split(';');
            int id = int.Parse(colData[0]);
            DateTime date = DateTime.ParseExact(colData[1], "yyyy/MM/dd", CultureInfo.InvariantCulture);
            bool claimed = int.Parse(colData[2]) == 1;
            string photodesc = "";
            bool stored = false;
            if (colData.Length > 3)
            {
                int isStored;
                if (int.TryParse(colData[3], out isStored) && isStored == 1)
                    stored = true;
                else
                    photodesc = colData[3];
            }

            CollectionElement elem = GetCollectionById(id);
            if (elem != null) // existing, updated stored, claimed
            {
                elem.Claimed = claimed;
                elem.Stored = stored;
            }
            else // new completed
            {
                elem = new CollectionElement(id, date, claimed, photodesc, stored, idx);
                collectionsDict.Add(id, elem);

                UIManager.ShowAchievementMessage(AchievementKind.Collection, id);
            }
        }

        if (windowObj != null && windowObj.activeInHierarchy)
            uiAchieve.UpdateCollectionProgress();

        GameObject dialog = UIManager.GetWindowGameObject(WindowType.DialogCollection);
        if (dialog != null && dialog.activeInHierarchy)
            dialog.GetComponent<UI_Achievement_CollectionDialog>().RefreshObjectiveList();
    }

    private void InitAchievements()
    {
        for (int idx = 0; idx < Achievements.Count; ++idx)
        {
            if (Achievements[idx] == null)
                continue;
            string info = (string)Achievements[idx];
            if (string.IsNullOrEmpty(info))
                continue;

            string[] achArray = info.Split('|');
            for (int i = 0; i < achArray.Length; ++i)
            {
                string[] achData = achArray[i].Split(';');
                int id = int.Parse(achData[0]);
                int count = int.Parse(achData[1]);
                bool claimed = int.Parse(achData[2]) == 1;

                AchievementObjective obj = AchievementRepo.GetAchievementObjectiveById(id);
                if (obj != null)
                {
                    AchievementElement elem = new AchievementElement(id, count, obj.completeCount, claimed, idx);
                    achievementsDict.Add(id, elem);
                }
            }
        }
    }

    public void UpdateAchievements(byte idx, string value)
    {
        if (string.IsNullOrEmpty(value))
            return;
        //Debug.Log("ach index: " + idx + " - " + value);

        string[] achArray = value.Split('|');
        for (int i = 0; i < achArray.Length; ++i)
        {
            string[] achData = achArray[i].Split(';');
            int id = int.Parse(achData[0]);
            int count = int.Parse(achData[1]);
            bool claimed = int.Parse(achData[2]) == 1;

            AchievementElement elem = GetAchievementById(id);
            bool isAlreadyCompleted = false;

            if (elem != null)  // existing, update count
            {
                elem.Claimed = claimed;
                isAlreadyCompleted = elem.IsCompleted();
                if (isAlreadyCompleted)
                    continue;
                elem.Count = count;
            }
            else // new, add to dict
            {
                AchievementObjective obj = AchievementRepo.GetAchievementObjectiveById(id);
                if (obj != null)
                {
                    elem = new AchievementElement(id, count, obj.completeCount, claimed, idx);
                    achievementsDict.Add(id, elem);
                }
            }

            if (!isAlreadyCompleted && elem.IsCompleted()) // just completed this achievement
                UIManager.ShowAchievementMessage(AchievementKind.Achievement, id);
        }

        if (windowObj != null && windowObj.activeInHierarchy)
            uiAchieve.UpdateAchievementProgress();
    }

    public void UpdateRewardClaims()
    {
        //Debug.Log("claimable rewards: " + RewardClaims);
        claimsList.Clear();
        if (!string.IsNullOrEmpty(RewardClaims))
        {
            string[] rcArray = RewardClaims.Split('|');
            for (int i = 0; i < rcArray.Length; ++i)
            {
                string[] rcData = rcArray[i].Split(';');
                AchievementKind type = (AchievementKind)int.Parse(rcData[0]);
                int id = int.Parse(rcData[1]);
                AchievementRewardClaim claim = new AchievementRewardClaim(type, id);
                claimsList.Add(claim);
            }
        }

        bool hasUnclaimed = HasUnclaimedRewards();

        if (windowObj != null && windowObj.activeInHierarchy)
            uiAchieve.SetClaimRewardButton(hasUnclaimed);

        GameObject dialog = UIManager.GetWindowGameObject(WindowType.DialogAchievementRewards);
        if (dialog != null && dialog.activeInHierarchy)
            dialog.GetComponent<UI_Achievement_RewardsDialog>().SetClaimAllButton(hasUnclaimed);
    }

    public bool HasUnclaimedRewards()
    {
        return claimsList.Count > 0;
    }

    public void OnClaimReward(string claimedReward)
    {
        //Debug.Log("OnClaimReward: " + claimedReward);
        if (!string.IsNullOrEmpty(claimedReward))
        {
            string[] rcData = claimedReward.Split(';');
            AchievementKind type = (AchievementKind)int.Parse(rcData[0]);
            int id = int.Parse(rcData[1]);
            AchievementRewardClaim claim = new AchievementRewardClaim(type, id);

            GameObject dialog = UIManager.GetWindowGameObject(WindowType.DialogAchievementRewards);
            if (dialog != null && dialog.activeInHierarchy)
                dialog.GetComponent<UI_Achievement_RewardsDialog>().OnClaimedReward(claim);
        }
    }

    public void OnClaimAllRewards(string claimedRewards)
    {
        //Debug.Log("claimed rewards: " + claimedRewards);
        if (!string.IsNullOrEmpty(claimedRewards))
        {
            List<AchievementRewardClaim> claimedRewardsList = new List<AchievementRewardClaim>();
            string[] rcArray = claimedRewards.Split('|');
            for (int i = 0; i < rcArray.Length; ++i)
            {
                string[] rcData = rcArray[i].Split(';');
                AchievementKind type = (AchievementKind)int.Parse(rcData[0]);
                int id = int.Parse(rcData[1]);
                AchievementRewardClaim claim = new AchievementRewardClaim(type, id);
                claimedRewardsList.Add(claim);
            }

            GameObject dialog = UIManager.GetWindowGameObject(WindowType.DialogAchievementRewards);
            if (dialog != null && dialog.activeInHierarchy)
                dialog.GetComponent<UI_Achievement_RewardsDialog>().OnClaimedAllRewards(claimedRewardsList);
        }
    }

    public void UpdateLatestRecords(AchievementKind type, string value)
    {
        //Debug.Log(type + " record: " + value);
        List<AchievementRecord> list = type == AchievementKind.Collection ? latestCollectionsList : latestAchievementList;
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

    public void OnUpdateAchievementLevel()
    {
        if (windowObj != null)
            uiAchieve.OnAchievementLevelUp();
    }

    public int GetTotalCompletedCollectionsCount()
    {
        return collectionsDict.Count;
    }

    public int GetCollectionCountByType(int index)
    {
        return collectionsDict.Values.Count(x => x.SlotIdx == index);
    }

    public int GetTotalCompletedAchievementsCount()
    {
        return achievementsDict.Values.Count(x => x.IsCompleted());
    }

    public int GetAchievementCountByType(int index)
    {
        return achievementsDict.Values.Count(x => x.IsCompleted() && x.SlotIdx == index);
    }

    public void GetLastCompletedAndNextAchievement(List<AchievementObjective> objList, out AchievementObjective last, out AchievementObjective next)
    {
        last = next = null;
        if (objList == null || objList.Count == 0)
            return;
        int lastIndex = -1;
        for (int i = 0; i < objList.Count; ++i)
        {
            if (IsAchievementCompleted(objList[i].id))
                lastIndex = i;
            else
                break;
        }
        if (lastIndex != -1)
            last = objList[lastIndex];
        if (lastIndex + 1 < objList.Count)
            next = objList[lastIndex + 1];
    }

    public void OnChangeLISATier()
    {
        if (windowObj != null && windowObj.activeInHierarchy)
            uiAchieve.UpdateAvatarModel(CurrentLISATier);
    }

    public bool CanUnlockLISATier()
    {
        int highestTier = AchievementRepo.GetHighestLISATierByLevel(player.PlayerSynStats.AchievementLevel);
        return highestTier != HighestUnlockedTier;
    }

    public void OnUnlockLISATier()
    {
        if (windowObj != null && windowObj.activeInHierarchy)
            uiAchieve.UpdateTierProgress();
    }

    public CollectStatus GetCollectionObjectiveStatus(CollectionObjective obj)
    {
        CollectionElement elem = GetCollectionById(obj.id);
        if (elem == null)
            return CollectStatus.Locked;
        else
        {
            CollectStatus status = CollectStatus.Unlocked;
            switch (obj.type)
            {
                case CollectionType.Fashion:
                case CollectionType.Relic:
                    if (elem.Stored)
                        status = CollectStatus.Stored;
                    else
                        status = player.clientItemInvCtrl.itemInvData.HasItem((ushort)obj.targetId) ? CollectStatus.HaveItem : CollectStatus.NoItem;
                    break;
            }
            return status;
        }
    }

#if ZEALOT_DEVELOPMENT

    public void ClearCollectionsDict()
    {
        collectionsDict.Clear();
    }

    public void ClearAchievementDict()
    {
        achievementsDict.Clear();
    }

#endif
}