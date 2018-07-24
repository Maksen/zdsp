using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Zealot.Common;

namespace Zealot.Common
{
    public enum LeaderboardType : byte
    {        
        Guild = 0,
        HeroHouse = 1,
        FactionWar = 2,
        GearScore = 3,
        HeroBook = 4,
        Gold = 5,
        Pet = 6,
        Arena = 7,
        FactionKill = 8,
        FactionDeath = 9,

        GuildRankAll = 100, //not show in leaderboard
        FactionRecommend = 101, //number of players for each faction
        WorldLevel = 102, //average progresslevel for top x players.
        WorldBossRecord = 103, //activity world boss leader board
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class LeaderBoardData
    {
        [JsonProperty("type")]
        public LeaderboardType lbType;

        [JsonProperty("rank")]
        public List<LeaderBoardRowData> lbRanking = new List<LeaderBoardRowData>();

        [JsonProperty("dt")]
        public DateTime lastupdate;

        public virtual string GetSerialized()
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.SerializeObject(this, Formatting.None, jsonSetting);
        }

        public static LeaderBoardData Deserialize(string lbData)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.DeserializeObject<LeaderBoardData>(lbData, jsonSetting);
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class LeaderBoardRowData
    {
        [DefaultValue(0)]
        [JsonProperty("sc")]
        public long score;

        [DefaultValue("")]
        [JsonProperty(PropertyName = "rn")]
        public string rankName;

        [DefaultValue(FactionType.None)]
        [JsonProperty(PropertyName = "fa")]
        public FactionType faction;

        [DefaultValue("")]
        [JsonProperty(PropertyName = "gd")]
        public string guild;

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "para1")]
        public int para1;

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "para2")]
        public int para2;

        public LeaderBoardRowData(long score, string name, FactionType faction, string guild, int para1, int para2)
        {
            this.score = score;
            this.rankName = name;
            this.faction = faction;
            this.guild = guild;
            this.para1 = para1;
            this.para2 = para2;
        }
    }
}
