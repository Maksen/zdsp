using Newtonsoft.Json;

namespace Zealot.Common
{
    public partial class EquipFushionInventoryData
    {
        #region serializable properties
        [JsonProperty(PropertyName = "FinishedFushion")]
        public bool EndOfFushion;

        [JsonProperty(PropertyName = "EquipFushionCoin")]
        public int CoinSlot;
        #endregion

        public EquipFushionInventoryData()
        {

        }

        public void InitDefault ()
        {
            if (CoinSlot == 0)
            {
                EndOfFushion = false;
                CoinSlot = 0;
            }
        }
    }
}