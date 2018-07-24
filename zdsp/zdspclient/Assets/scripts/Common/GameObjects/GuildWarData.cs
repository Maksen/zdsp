using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Zealot.Repository;

namespace Zealot.Common
{
    public enum GuildWarReadyState : byte
    {
         NotReady,
         BetQuater,
         WarQuater,
         BetSemiFinal,
         WarSemiFinal,
         BetFinal,
         WarFinal
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GuildWarData
    {
        #region serializable properties
        [DefaultValue(true)]
        [JsonProperty(PropertyName = "iconfree")]
        public bool GuildIconFree { get; set; }

        
        #endregion

        public bool IsDirty;

        public GuildWarData()
        {
            GuildIconFree = true;
        }

        #region Json Serialization
        public string SerializeForDB(bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };

            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(this, formatting, jsonSetting);
        }

        public static GuildData DeserializeFromDB(string guildData)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.DeserializeObject<GuildData>(guildData, jsonSetting);
        }
        #endregion
    }
}