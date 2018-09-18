using System.Collections.Generic;
using System;
using Zealot.Common.Entities;

namespace Zealot.Common
{
    public partial class EquipmentCraftInventoryData
    {
        public void InitFormInventory(EquipmentCraftStats equipmentCraftStats)
        {
            if (EquipmentCraftSlots.Count <= 0)
            {
                Console.Write("void InitFromStats(EquipmentCraftStats equipmentCraftStats) error: EquipmentCraftSlots empty");
                return;
            }

            equipmentCraftStats.finishedCraft = EquipmentCrafted;

            foreach (KeyValuePair<int, int> entry in EquipmentCraftSlots)
            {
                if (equipmentCraftStats.equipmentCraftList.ContainsKey(entry.Key))
                {
                    equipmentCraftStats.equipmentCraftList[entry.Key] = entry.Value;
                } else
                {
                    equipmentCraftStats.equipmentCraftList.Add(entry.Key, entry.Value);
                }
            }
        }

        public void InitFormStats(EquipmentCraftStats equipmentCraftStats)
        {
            if (EquipmentCraftSlots.Count <= 0)
            {
                Console.Write("void InitFromStats(EquipmentCraftStats equipmentCraftStats) error: EquipmentCraftSlots empty");
                return;
            }

            EquipmentCrafted = equipmentCraftStats.finishedCraft;

            foreach (KeyValuePair<int, int> entry in equipmentCraftStats.equipmentCraftList)
            {
                if (equipmentCraftStats.equipmentCraftList.ContainsKey(entry.Key))
                {
                    EquipmentCraftSlots[entry.Key] = entry.Value;
                }
                else
                {
                    EquipmentCraftSlots.Add(entry.Key, entry.Value);
                }
            }
        }

        public void SaveToInventoryData(EquipmentCraftStats equipmentCraftStats)
        {
            InitFormStats(equipmentCraftStats);
        }
    }
}