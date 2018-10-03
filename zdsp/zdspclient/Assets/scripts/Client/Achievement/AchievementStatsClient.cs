using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;

public class AchievementStatsClient : AchievementStats
{
    private GameObject windowObj;
    //private UI_Achievement uiAchieve;

    public void Init()
    {
        //windowObj = UIManager.GetWindowGameObject(WindowType.Achievement);
        //uiAchieve = windowObj.GetComponent<UI_Achievement>();
    }

    public void UpdateCollections(byte idx, string value)
    {
        if (string.IsNullOrEmpty(value))
            return;
        Debug.Log("col index: " + idx + "-" + value);

        string[] colArray = value.Split('|');
        for (int i = 0; i < colArray.Length; ++i)
        {
            string[] colData = colArray[i].Split(';');
            int id = int.Parse(colData[0]);
            DateTime date = DateTime.ParseExact(colData[1], "yyyy/MM/dd", CultureInfo.InvariantCulture);
            string photodesc = "";
            bool stored = false;
            if (colData.Length > 2)
            {
                int isStored;
                if (int.TryParse(colData[2], out isStored) && isStored == 1)
                    stored = true;
                else
                    photodesc = colData[2];
            }

            CollectionElement elem = GetCollectionById(id);
            if (elem != null) // existing, updated stored
            {
                elem.Stored = stored;
            }
            else
            {
                elem = new CollectionElement(id, date, false, photodesc, stored, idx);
                collectionsDict.Add(id, elem);
            }
        }
    }

    public void UpdateAchievements(byte idx, string value)
    {
        if (string.IsNullOrEmpty(value))
            return;
        Debug.Log("ach index: " + idx + " - " + value);

        string[] achArray = value.Split('|');
        for (int i = 0; i < achArray.Length; ++i)
        {
            string[] achData = achArray[i].Split(';');
            int id = int.Parse(achData[0]);
            int count = int.Parse(achData[1]);

            AchievementElement elem = GetAchievementById(id);
            if (elem != null)  // existing, update count
            {
                if (!elem.IsCompleted())
                    elem.Count = count;
            }
            else // new, add to dict
            {
                AchievementObjective obj = AchievementRepo.GetAchievementObjectiveById(id);
                if (obj != null)
                {
                    elem = new AchievementElement(id, count, obj.completeCount, false, obj.slotIdx);
                    achievementsDict.Add(id, elem);
                }
            }
        }
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
                AchievementType type = (AchievementType)int.Parse(rcData[0]);
                int id = int.Parse(rcData[1]);
                AchievementRewardClaim claim = new AchievementRewardClaim(type, id);
                claimsList.Add(claim);
            }
        }
    }

    public bool HasUnclaimedRewards()
    {
        return claimsList.Count > 0;
    }

    public void OnClaimAllRewards(string claimedRewards)
    {
        //Debug.Log("claimed rewards: " + claimedRewards);
    }

    public void UpdateLatestRecords(AchievementType type, string value)
    {
        //Debug.Log(type + " record: " + value);
        List<AchievementRecord> list = type == AchievementType.Collection ? latestCollectionsList : latestAchievementList;
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
