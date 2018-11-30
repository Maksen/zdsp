using Newtonsoft.Json;

namespace Zealot.Common
{
    public partial class EquipFusionInventoryData
    {
        #region serializable properties
        [JsonProperty(PropertyName = "FinishedFusion")]
        public bool EndOfFusion;

        [JsonProperty(PropertyName = "EquipFusionCoin")]
        public int CoinSlot;

        [JsonProperty(PropertyName = "FusionItemSort")]
        public int FusionItemSort;

        [JsonProperty(PropertyName = "FusionData")]
        public string FusionData;
        #endregion

        public EquipFusionInventoryData()
        {
            EndOfFusion = false;
            FusionItemSort = 0;
            FusionData = string.Empty;
        }

        public void InitDefault ()
        {
            EndOfFusion = false;
            FusionItemSort = 0;
            FusionData = string.Empty;
        }
    }
}