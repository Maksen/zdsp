using Newtonsoft.Json;
using System;
using Zealot.Repository;

namespace Zealot.Common
{
    public static class AchievementData
    {
        public static readonly int MAX_RECORDS = 3;
    }

    public enum AchievementKind
    {
        Collection,
        Achievement
    }

    public abstract class BaseAchievementElement
    {
        public int Id { get; set; }
        public bool Claimed { get; set; }
        public int SlotIdx { get; set; } // server use only

        public virtual bool IsCompleted()
        {
            return true;
        }

        public virtual string ToClientString()
        {
            return "";
        }
    }

    public class CollectionElement : BaseAchievementElement
    {
        public DateTime CollectDate { get; set; }
        public string PhotoDesc { get; set; }
        public bool Stored { get; set; }

        public CollectionElement(int id, DateTime date, bool claim, string photodesc, bool store, int idx)
        {
            Id = id;
            CollectDate = date;
            Claimed = claim;
            PhotoDesc = photodesc;
            Stored = store;
            SlotIdx = idx;
        }

        public override string ToClientString()
        {
            if (Stored)
                return string.Format("{0};{1};{2};1", Id, CollectDate.ToString("yyyy/MM/dd"), Claimed ? 1 : 0);
            else if (!string.IsNullOrEmpty(PhotoDesc))
                return string.Format("{0};{1};{2};{3}", Id, CollectDate.ToString("yyyy/MM/dd"), Claimed ? 1 : 0, PhotoDesc);
            else
                return string.Format("{0};{1};{2}", Id, CollectDate.ToString("yyyy/MM/dd"), Claimed ? 1 : 0);
        }
    }

    public class AchievementElement : BaseAchievementElement
    {
        public int Count { get; set; }
        public int CompleteCount { get; set; }

        public AchievementElement(int id, int count, int completecount, bool claim, int idx)
        {
            Id = id;
            Count = count;
            CompleteCount = completecount;
            Claimed = claim;
            SlotIdx = idx;

#if DEBUG
            if (count == -1)
                Count = CompleteCount;
#endif
            Count = Math.Min(CompleteCount, Count);
        }

        public void UpdateCount(int newCount, bool increment)
        {
            if (increment)  // increment count by newCount
            {
                Count += newCount;
            }
            else  // set count to newCount
            {
                if (newCount > Count)
                    Count = newCount;
            }
#if DEBUG
            if (newCount == -1)
                Count = CompleteCount;
#endif
            Count = Math.Min(CompleteCount, Count);
        }

        public override bool IsCompleted()
        {
            return Count >= CompleteCount;
        }

        public override string ToClientString()
        {
            return string.Format("{0};{1};{2}", Id, Count, Claimed ? 1 : 0);
        }
    }

    public class AchievementRewardClaim
    {
        public AchievementKind ClaimType { get; set; }
        public int Id { get; set; }

        public AchievementRewardClaim(AchievementKind type, int id)
        {
            ClaimType = type;
            Id = id;
        }
    }

    // client use
    public struct AchievementReward
    {
        public AchievementRewardType rewardType;
        public int rewardId;
        public float rewardCount;
        public string iconPath;

        public AchievementReward(AchievementRewardType type, int id, float count, string path)
        {
            rewardType = type;
            rewardId = id;
            rewardCount = count;
            iconPath = path;
        }
    }

    public class AchievementRecord
    {
        public int Id { get; set; }

        public DateTime CompleteDate { get; set; }

        public AchievementRecord(int id, DateTime date)
        {
            Id = id;
            CompleteDate = date;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class AchievementInvData
    {
        [JsonProperty(PropertyName = "alv")]
        public int AchievementLevel { get; set; }

        [JsonProperty(PropertyName = "aexp")]
        public int AchievementExp { get; set; }

        [JsonProperty(PropertyName = "col")]
        public string Collections { get; set; }

        [JsonProperty(PropertyName = "ach")]
        public string Achievements { get; set; }

        [JsonProperty(PropertyName = "rwd")]
        public string RewardClaims { get; set; }

        [JsonProperty(PropertyName = "ltc")]
        public string LatestCollections { get; set; }

        [JsonProperty(PropertyName = "lta")]
        public string LatestAchievements { get; set; }

        [JsonProperty(PropertyName = "ct")]
        public string CompletedTargets { get; set; }

        [JsonProperty(PropertyName = "ctr")]
        public int CurrentTier { get; set; }

        [JsonProperty(PropertyName = "htr")]
        public int HighestUnlockedTier { get; set; }

        public static JsonSerializerSettings jsonSetting;

        public AchievementInvData()
        {
            jsonSetting = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            AchievementLevel = 1;
            CurrentTier = 1;
            HighestUnlockedTier = 1;
        }

        public string SerializeForDB()
        {
            return JsonConvert.SerializeObject(this, jsonSetting);
        }

        public static AchievementInvData DeserializeFromDB(string invData)
        {
            return JsonConvert.DeserializeObject<AchievementInvData>(invData, jsonSetting);
        }
    }


    // Client use
    public class AchievementInfo
    {
        public AchievementObjective objective;
        public int count;

        public AchievementInfo(AchievementObjective obj, int currCount)
        {
            objective = obj;
            count = currCount;
        }

        public bool IsCompleted()
        {
            return count >= objective.completeCount;
        }
    }

}