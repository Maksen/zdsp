using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;
using System;

namespace Zealot.Common
{
    // Send client use json class
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PackageLotteryItemsData
    {
        [JsonProperty(PropertyName = "ids")]
        public int[] ids;
        [JsonProperty(PropertyName = "counts")]
        public int[] counts;
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PackagePointRewardsData
    {
        [JsonProperty(PropertyName = "point")]
        public int point;
        [JsonProperty(PropertyName = "ids")]
        public int[] ids;
        [JsonProperty(PropertyName = "counts")]
        public int[] counts;
    }

    // save DB characterdata class
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class LotteryInfo
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "id")]
        public int lotteryId { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "free")]
        public int freeTicket { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "point")]
        public int point { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "lotterycount")]
        public int lotteryCount { get; set; }

        [JsonProperty(PropertyName = "rewardpoints")]
        public List<int> rewardPoints { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "lastupdatetime")]
        public long lastUpdateDateTicks { get; set; }

        private DateTime lastUpdateDateTime;
        public DateTime LastUpdateDate
        {
            get
            {
                return lastUpdateDateTime;
            }
            set
            {
                lastUpdateDateTime = value;
                lastUpdateDateTicks = value.Ticks;
            }
        }

        public LotteryInfo()
        {
            rewardPoints = new List<int>();
        }

        public int pointRewardCount;
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class LotteryInventoryData
    {
        #region serializable properties
        // Resets everyday
        [JsonProperty(PropertyName = "lotterydata")]
        public List<LotteryInfo> lotteryDatas = new List<LotteryInfo>();
        #endregion

        public LotteryInventoryData() { }

        public void InitDefault()
        {
            //if (lotteryDatas.Count == 0)
            //    lotteryDatas.Add(new LotteryInfo());

            //LotteryInfo info = lotteryDatas[0];
            //info.lotteryId = 0;
            //info.freeTicket = 0;
            //info.point = 0;
            //info.lotteryCount = 0;
            //info.LastUpdateDate = new DateTime(0);
            //info.rewardPoints = new List<int>();
            //info.pointRewardCount = 0;
        }
    }
}
