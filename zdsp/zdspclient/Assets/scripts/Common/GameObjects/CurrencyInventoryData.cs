using Newtonsoft.Json;
using System.ComponentModel;
using Zealot.Common.Entities;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CurrencyInventoryData
    {
        #region serializable properties
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "money")]
        public int Money { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "gold")]
        public int Gold { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "bgold")]
        public int BindGold { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "lottpts")]
        public int LotteryPoints { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "honor")]
        public int Honor { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "contribute")]
        public int GuildContribute { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "fundtoday")]
        public int GuildFundToday { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "fundtotal")]
        public long GuildFundTotal { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "viplvl")]
        public byte VIPLevel { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "vippts")]
        public int VIPPoints { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "battlecoin")]
        public int BattleCoin { get; set; }
        #endregion

        public void SaveToInventoryData(SecondaryStats secondaryStats, PlayerSynStats playerSynStats)
        {
            Money = secondaryStats.Money;
            Gold = secondaryStats.Gold;
            BindGold = secondaryStats.bindgold;
            LotteryPoints = secondaryStats.lotterypoints;
            Honor = secondaryStats.honor;
            GuildContribute = secondaryStats.contribute;
            VIPLevel = playerSynStats.vipLvl;
            VIPPoints = secondaryStats.vippoints;          
            BattleCoin = secondaryStats.battlecoin;
        }
    }
}
