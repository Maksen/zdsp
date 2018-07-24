using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class BossKillData
    {
        #region serializable properties
        [JsonProperty(PropertyName = "dmg")]
        public List<BossKillDmgRecord> dmgRecords = new List<BossKillDmgRecord>(); //max count of 50;

        [JsonProperty(PropertyName = "score")]
        public List<BossKillScoreRecord> scoreRecords = new List<BossKillScoreRecord>(); //max count of 50;
        #endregion

        public string SerializeForDB(bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(this, formatting, jsonSetting);
        }

        public static BossKillData DeserializeFromDB(string data)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.DeserializeObject<BossKillData>(data, jsonSetting);
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class BossKillDmgRecord
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "score")]
        public int Score { get; set; }
    }


    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class BossKillScoreRecord
    {
        [JsonProperty(PropertyName = "name")]
        public List<string> Name = new List<string>(); //list of team members

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "score")]
        public long Score { get; set; }
    }
}
