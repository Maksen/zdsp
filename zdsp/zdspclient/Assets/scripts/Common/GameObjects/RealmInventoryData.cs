using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Zealot.Common.Entities;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class RealmData
    {
        [JsonProperty(PropertyName = "seq")]
        public int Sequence { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "lootlimit")]
        public int LootRewardLimit { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class RealmInventoryData
    {
        #region serializable properties

        [JsonProperty(PropertyName = "story")]
        public List<RealmData> DungeonStory = new List<RealmData>();

        #endregion

        public void InitDefault()
        {
        }

        public void SaveToInventoryData(RealmStats realmStats)
        {
            DungeonStory.Clear();

            Dictionary<int, RealmInfo> dungeonStoryInfos = realmStats.GetDungeonStoryInfos();
            foreach (KeyValuePair<int, RealmInfo> entry in dungeonStoryInfos)
            {
                RealmInfo info = entry.Value;
                RealmData data = new RealmData() {
                    Sequence = info.Sequence,
                    LootRewardLimit = info.LootRewardLimit
                };
                DungeonStory.Add(data);
            }
        }
    }
}
