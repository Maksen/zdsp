using System;
using Zealot.Common.Entities;

namespace Zealot.Common
{
    public partial class EquipFushionInventoryData
    {
        public void InitFromInventory(EquipFushionStats equipFushionStats)
        {
            if (CoinSlot < 0)
            {
                Console.Write("void InitFromStats(EquipFushionStats equipFushionStats) error: StoneInfoSlots empty");
                return;
            }

            equipFushionStats.FinishedFushion = EndOfFushion;
            equipFushionStats.EquipFushionCoin = CoinSlot;
        }

        public void InitFromStats(EquipFushionStats equipFushionStats)
        {
            if (CoinSlot < 0)
            {
                Console.Write("void InitFromStats(EquipFushionStats equipFushionStats) error: StoneInfoSlots empty");
                return;
            }

            EndOfFushion = equipFushionStats.FinishedFushion;
            CoinSlot = equipFushionStats.EquipFushionCoin;
        }

        public void SaveToInventoryData(EquipFushionStats equipFushionStats)
        {
            InitFromStats(equipFushionStats);
        }
    }
}