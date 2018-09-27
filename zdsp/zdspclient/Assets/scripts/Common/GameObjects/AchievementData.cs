using Newtonsoft.Json;
using System;

namespace Zealot.Common
{
    public static class AchievementData
    {
        public static readonly int MAX_RECORDS = 3;
    }

    public enum AchievementType
    {
        Collection,
        Achievement
    }

    public class CollectionElement
    {
        public int Id { get; set; }
        public DateTime CollectDate { get; set; }
        public string PhotoDesc { get; set; }

        public CollectionElement(int id, DateTime date, string photodesc = "")
        {
            Id = id;
            CollectDate = date;
            PhotoDesc = photodesc;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(PhotoDesc))
                return string.Format("{0};{1}", Id, CollectDate.ToString("yyyy/MM/dd"));
            else
                return string.Format("{0};{1};{2}", Id, CollectDate.ToString("yyyy/MM/dd"), PhotoDesc);
        }
    }

    public class AchievementElement
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public int CompleteCount { get; set; }
        public int SlotIdx { get; set; }

        public AchievementElement(int id, int count, int completecount, int idx)
        {
            Id = id;
            Count = count;
            CompleteCount = completecount;
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

        public bool IsCompleted()
        {
            return Count >= CompleteCount;
        }

        public override string ToString()
        {
            return string.Format("{0};{1}", Id, Count);
        }
    }

    public class AchievementRewardClaim
    {
        public AchievementType ClaimType { get; set; }
        public int Id { get; set; }

        public AchievementRewardClaim(AchievementType type, int id)
        {
            ClaimType = type;
            Id = id;
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
        [JsonProperty(PropertyName = "collect")]
        public string Collections { get; set; }

        [JsonProperty(PropertyName = "achieve")]
        public string Achievements { get; set; }

        [JsonProperty(PropertyName = "claim")]
        public string RewardClaims { get; set; }

        [JsonProperty(PropertyName = "ltc")]
        public string LatestCollections { get; set; }

        [JsonProperty(PropertyName = "lta")]
        public string LatestAchievements { get; set; }
    }
}