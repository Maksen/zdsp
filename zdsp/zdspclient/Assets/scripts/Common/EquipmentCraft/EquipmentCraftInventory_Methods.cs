using System.Collections.Generic;
using System;
using Zealot.Common.Entities;

namespace Zealot.Common
{
    public partial class EquipmentCraftInventoryData
    {
        public void InitFormInventory(EquipmentCraftStats equipmentCraftStats)
        {
            if (achievementRequireList.Count <= 0)
            {
                Console.Write("void InitFromInventory(EquipmentCraftStats equipmentCraftStats) error: achievementRequireList empty");
                return;
            }

            equipmentCraftStats.finishedCraft = EquipmentCrafted;

            foreach (KeyValuePair<int, int> entry in achievementRequireList)
            {
                if (equipmentCraftStats.achievementRequireList.ContainsKey(entry.Key))
                {
                    equipmentCraftStats.achievementRequireList[entry.Key] = entry.Value;
                } else
                {
                    equipmentCraftStats.achievementRequireList.Add(entry.Key, entry.Value);
                }
            }
        }

        public void InitFormStats(EquipmentCraftStats equipmentCraftStats)
        {
            if (achievementRequireList.Count <= 0)
            {
                Console.Write("void InitFromStats(EquipmentCraftStats equipmentCraftStats) error: achievementRequireList empty");
                return;
            }

            EquipmentCrafted = equipmentCraftStats.finishedCraft;

            foreach (KeyValuePair<int, int> entry in equipmentCraftStats.achievementRequireList)
            {
                if (equipmentCraftStats.achievementRequireList.ContainsKey(entry.Key))
                {
                    achievementRequireList[entry.Key] = entry.Value;
                }
                else
                {
                    achievementRequireList.Add(entry.Key, entry.Value);
                }
            }
        }

        public void SaveToInventoryData(EquipmentCraftStats equipmentCraftStats)
        {
            InitFormStats(equipmentCraftStats);
        }
    }
}