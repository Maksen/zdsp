using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GuildRankData
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "lvl")]
        public byte Level { get; set; }

        [JsonProperty(PropertyName = "lead")]
        public string Leader { get; set; }

        [JsonProperty(PropertyName = "ptige")]
        public int ActivePrestige { get; set; }

        public GuildRankData(string name, byte level, string leader, int prestige)
        {
            Name = name;
            Level = level;
            Leader = leader;
            ActivePrestige = prestige;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CountryData
    {
        #region serializable properties
        [JsonProperty(PropertyName = "type")]
        public byte Type { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "guild")]
        public string KingGuildName { get; set; }

        [JsonProperty(PropertyName = "rank")]
        public List<GuildRankData> GuildRanks = new List<GuildRankData>();

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "score")]
        public Int64 CombatScore { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "prestige")]
        public Int64 Prestige { get; set; }

        #endregion
        public CountryData()
        {
            KingGuildName = "";
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CountryInventoryData
    {
        [JsonProperty(PropertyName = "king")]
        public CharacterCreationData KingData { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "guild")]
        public string KingGuildName { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "ktype")]
        public byte KingCountryType { get; set; }

        [JsonProperty(PropertyName = "dt")]
        public DateTime LastUpdate { get; set; }

        [JsonProperty(PropertyName = "country")]
        public List<CountryData> CountryDatas = new List<CountryData>(); //index = CountryType - 1

        public CountryInventoryData()
        {
            KingGuildName = "";
        }

        public string SerializeForDB(bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            //jsonSetting.Converters.Add(new DBInventoryItemConverter());
            return JsonConvert.SerializeObject(this, formatting, jsonSetting);
        }

        public static CountryInventoryData DeserializeFromDB(string data)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new DBInventoryItemConverter());
            return JsonConvert.DeserializeObject<CountryInventoryData>(data, jsonSetting);
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CountryReportData
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "type")]
        public byte Type { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "param")]
        public string Parameters { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CountryReportInventoryData
    {
        [JsonProperty(PropertyName = "reports")]
        public List<CountryReportData> ReportDatas = new List<CountryReportData>(); //max 50

        [JsonProperty(PropertyName = "announce")]
        public List<string> BaZhuAnnounce = new List<string>();

        public string reports = "";
        public bool IsDirty = false;
        public bool SaveToDB = false;

        public string SerializeForDB(bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(this, formatting, jsonSetting);
        }

        public static CountryReportInventoryData DeserializeFromDB(string data)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.DeserializeObject<CountryReportInventoryData>(data, jsonSetting);
        }

        public void AddReport(CountryReportType type, string parameters)
        {         
            CountryReportData report = new CountryReportData();
            report.Type = (byte)type;
            report.Parameters = parameters;          
            if (ReportDatas.Count == 50)
                ReportDatas.RemoveAt(0);
            ReportDatas.Add(report);
            IsDirty = true;
            SaveToDB = true;
        }

        public void EditAnnounce(int index, string message)
        {
            BaZhuAnnounce[index] = message;
            IsDirty = true;
            SaveToDB = true;
        }
    }
}
