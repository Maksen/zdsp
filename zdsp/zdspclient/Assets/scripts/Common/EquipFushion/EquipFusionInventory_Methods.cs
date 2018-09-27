using System;
using Zealot.Common.Entities;

namespace Zealot.Common
{
    public partial class EquipFusionInventoryData
    {
        public void InitFromInventory(EquipFusionStats equipFusionStats)
        {
            equipFusionStats.EquipFusionCoin = CoinSlot;
        }

        public void InitFromStats(EquipFusionStats equipFusionStats)
        {
            CoinSlot = equipFusionStats.EquipFusionCoin;
        }

        public void SaveToInventoryData(EquipFusionStats equipFusionStats)
        {
            InitFromStats(equipFusionStats);
        }
    }
}