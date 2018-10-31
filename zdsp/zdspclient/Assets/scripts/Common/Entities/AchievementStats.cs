using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using Zealot.Common.Datablock;
using Zealot.Repository;

namespace Zealot.Common.Entities
{
    public class AchievementStats : LocalObject
    {
        private string _rewardClaims;
        private string _latestCollections;
        private string _latestAchievements;

        public Dictionary<int, CollectionElement> GetCollectionsDict() { return collectionsDict; }
        protected Dictionary<int, CollectionElement> collectionsDict; // key: id

        public Dictionary<int, AchievementElement> GetAchievementsDict() { return achievementsDict; }
        protected Dictionary<int, AchievementElement> achievementsDict; // key: id

        public List<AchievementRewardClaim> claimsList;
        public List<AchievementRecord> latestCollectionsList;
        public List<AchievementRecord> latestAchievementList;

        public AchievementStats() : base(LOTYPE.AchievementStats)
        {
            _rewardClaims = "";
            _latestCollections = "";
            _latestAchievements = "";

            collectionsDict = new Dictionary<int, CollectionElement>();
            achievementsDict = new Dictionary<int, AchievementElement>();
            claimsList = new List<AchievementRewardClaim>();
            latestCollectionsList = new List<AchievementRecord>();
            latestAchievementList = new List<AchievementRecord>();

            Collections = new CollectionHandler<object>(Enum.GetNames(typeof(CollectionType)).Length);
            Collections.SetParent(this, "Collections");

            Achievements = new CollectionHandler<object>(Enum.GetNames(typeof(AchievementType)).Length);
            Achievements.SetParent(this, "Achievements");
        }

        public CollectionHandler<object> Collections { get; set; }
        public CollectionHandler<object> Achievements { get; set; }

        public string RewardClaims
        {
            get { return _rewardClaims; }
            set { OnSetAttribute("RewardClaims", value); _rewardClaims = value; }
        }

        public string LatestCollections
        {
            get { return _latestCollections; }
            set { OnSetAttribute("LatestCollections", value); _latestCollections = value; }
        }

        public string LatestAchievements
        {
            get { return _latestAchievements; }
            set { OnSetAttribute("LatestAchievements", value); _latestAchievements = value; }
        }

        public BaseAchievementElement GetElementByTypeAndId(AchievementKind type, int id)
        {
            if (type == AchievementKind.Collection)
                return GetCollectionById(id);
            else
                return GetAchievementById(id);
        }

        public CollectionElement GetCollectionById(int id)
        {
            CollectionElement elem;
            collectionsDict.TryGetValue(id, out elem);
            return elem;
        }

        public AchievementElement GetAchievementById(int id)
        {
            AchievementElement elem;
            achievementsDict.TryGetValue(id, out elem);
            return elem;
        }

        public bool IsAchievementCompletedAndClaimed(int id)
        {
            AchievementElement elem = GetAchievementById(id);
            if (elem != null)
                return elem.IsCompleted() && elem.Claimed;
            return false;
        }

        public int GetLISAFunctionValue(LISAFunction function, int achievementLevel)
        {
            List<LISAExternalFunctionJson> list = AchievementRepo.GetExternalFunctionsByFunctionType(function);
            if (list != null)
            {
                int value = 0;
                for (int i = 0; i < list.Count; ++i)
                {
                    LISAExternalFunctionJson data = list[i];
                    if (data.triggertype == LISAFunctionTriggerType.AchievementLV)
                    {
                        if (achievementLevel >= data.triggervalue && data.functionvalue > value)
                            value = data.functionvalue;
                    }
                    else
                    {
                        if (IsAchievementCompletedAndClaimed(data.triggervalue) && data.functionvalue > value)
                            value = data.functionvalue;
                    }
                }
                return value;
            }
            return 0;
        }
    }
}