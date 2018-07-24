using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class RealmData
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "daily")]
        public int DailyEntry { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "extra")]
        public int ExtraEntry { get; set; }
    }

    public class DungeonStoryData
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "daily")]
        public int DailyEntry { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "extra")]
        public int ExtraEntry { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "dailyextra")]
        public int DailyExtraEntry { get; set; }

        [JsonProperty(PropertyName = "starcompleted")]
        public bool[] StarCompleted { get; set; }

        [DefaultValue(false)]
        [JsonProperty(PropertyName = "starcollected")]
        public string StarCollected { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class RealmInventoryData
    {
        #region serializable properties

        [JsonProperty(PropertyName = "story")]
        public List<DungeonStoryData> DungeonStory = new List<DungeonStoryData>();

        [JsonProperty(PropertyName = "daily")]
        public List<RealmData> DungeonDaily = new List<RealmData>();

        [JsonProperty(PropertyName = "special")]
        public List<RealmData> DungeonSpecial = new List<RealmData>();
        
        [JsonProperty(PropertyName = "worldboss")]
        public List<RealmData> WorldBoss = new List<RealmData>();

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "elitemap")]
        public int EliteMapTime { get; set; }
        #endregion
    }
}
