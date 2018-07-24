using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class NewServerEventInventoryData
    {
        [JsonProperty(PropertyName = "level")]
        public List<int> LevelRewardCollected = new List<int>(); //list of NewServerActivity ids with type = NewServerActivityType.Level

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "boss")]
        public int BossSlay { get; set; }

        [DefaultValue(false)]
        [JsonProperty(PropertyName = "bossreward")]
        public bool BossSlayClaimed { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "topuptotal")]
        public int TopupTotal { get; set; }

        [DefaultValue(false)]
        [JsonProperty(PropertyName = "topuptotalreward")]
        public bool TopupTotalClaimed { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "topuptoday")]
        public int TopupToday { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "consume")]
        public int Consumption { get; set; }

        [DefaultValue(false)]
        [JsonProperty(PropertyName = "consumereward")]
        public bool ConsumptionClaimed { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class NewServerEventRankData
    {
        [DefaultValue("")]
        [JsonProperty(PropertyName = "name")]
        public string PlayerName { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "credit")]
        public int Credit { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class NewServerEventLevelReward
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "id")]
        public int ID { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "reward")]
        public int RewardCollected { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class NewServerEventRecords
    {
        [JsonProperty(PropertyName = "level")]
        public List<NewServerEventLevelReward> LevelRewards = new List<NewServerEventLevelReward>();

        [JsonProperty(PropertyName = "boss")]
        public List<NewServerEventRankData> BossSlay = new List<NewServerEventRankData>(); //top 10 boss slayer and value

        [JsonProperty(PropertyName = "topuptotal")]
        public List<NewServerEventRankData> TopupTotal = new List<NewServerEventRankData>(); //top 10 topup player name and value

        [JsonProperty(PropertyName = "consume")]
        public List<NewServerEventRankData> ConsumptionTotal = new List<NewServerEventRankData>(); //top 10 consumption player name and value

        [JsonProperty(PropertyName = "topuptoday")]
        public List<NewServerEventRankData> TopupToday = new List<NewServerEventRankData>(); //top 5 topup today player name and value

        [JsonProperty(PropertyName = "rebate")]
        public List<NewServerEventRankData> Rebates = new List<NewServerEventRankData>(); //record players with unclaimed rebates

        [JsonProperty(PropertyName = "dt")]
        public DateTime LastRebateReset { get; set; }

        public bool IsDirty = false;

        public string SerializeForDB(bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            jsonSetting.Converters.Add(new DBInventoryItemConverter());
            return JsonConvert.SerializeObject(this, formatting, jsonSetting);
        }

        public static NewServerEventRecords DeserializeFromDB(string data)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new DBInventoryItemConverter());
            return JsonConvert.DeserializeObject<NewServerEventRecords>(data, jsonSetting);
        }  
    }
}
