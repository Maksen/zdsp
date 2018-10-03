using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Zealot.Common.Entities;

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
        public bool Claimed { get; set; }  // server use only
        public string PhotoDesc { get; set; }
        public bool Stored { get; set; }
        public int SlotIdx { get; set; } // server use only

        public CollectionElement(int id, DateTime date, bool claim, string photodesc, bool store, int idx)
        {
            Id = id;
            CollectDate = date;
            Claimed = claim;
            PhotoDesc = photodesc;
            Stored = store;
            SlotIdx = idx;
        }

        public string ToClientString()
        {
            if (Stored)
                return string.Format("{0};{1};1", Id, CollectDate.ToString("yyyy/MM/dd"));
            else if (!string.IsNullOrEmpty(PhotoDesc))
                return string.Format("{0};{1};{2}", Id, CollectDate.ToString("yyyy/MM/dd"), PhotoDesc);
            else
                return string.Format("{0};{1}", Id, CollectDate.ToString("yyyy/MM/dd"));
        }
    }

    public class AchievementElement
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public int CompleteCount { get; set; }
        public bool Claimed { get; set; } // server use only
        public int SlotIdx { get; set; } // server use only

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

        public bool IsCompleted()
        {
            return Count >= CompleteCount;
        }

        public string ToClientString()
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
    }
}