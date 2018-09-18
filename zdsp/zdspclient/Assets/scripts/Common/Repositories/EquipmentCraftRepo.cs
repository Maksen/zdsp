using System.Collections.Generic;
using Kopio.JsonContracts;
using System;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
    public class EquipmentCraftRepo
    {
        //int partsCount = Enum.GetNames(typeof(PartsType)).Length;

        private static Dictionary<int, EquipmentCraftJson> equipmentMap;
        private static Dictionary<PartsType, List<Equipment>> equipmentPartSort;

        public static Dictionary<PartsType, string> equipmentTypesName = new Dictionary<PartsType, string> {
            { PartsType.Sword, "劍" },
            { PartsType.Blade, "刃" },
            { PartsType.Lance, "槍" },
            { PartsType.Hammer, "槌" },
            { PartsType.Fan, "扇" },
            { PartsType.Xbow, "弩" },
            { PartsType.Dagger, "匕首" },
            { PartsType.Sanxian, "古箏" },
            { PartsType.Helm, "頭盔" },
            { PartsType.Body, "身體" },
            { PartsType.Wing, "翅膀" },
            { PartsType.Boots, "鞋子" },
            { PartsType.Bathrobe, "長袍" },
            { PartsType.Ring, "戒指" },
            { PartsType.Jewelry, "珠寶" },
            { PartsType.Accessory, "配飾" }
        };

        static EquipmentCraftRepo()
        {
            equipmentMap = new Dictionary<int, EquipmentCraftJson>();
            equipmentPartSort = new Dictionary<PartsType, List<Equipment>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            foreach (KeyValuePair<int, EquipmentCraftJson> entry in gameData.EquipmentCraft)
            {
                int id = entry.Value.eq_id;

                if (!equipmentMap.ContainsKey(id))
                {
                    equipmentMap.Add(id, entry.Value);
                }
            }
            
            foreach (KeyValuePair<int, EquipmentCraftJson> entry in equipmentMap)
            {
                Equipment equipment = GameRepo.ItemFactory.GetInventoryItem(entry.Value.eq_id) as Equipment;
                PartsType eq_Type = equipment.EquipmentJson.partstype;

                if (!equipmentPartSort.ContainsKey(eq_Type))
                {
                    List<Equipment> dataMap = new List<Equipment>();
                    dataMap.Add(equipment);
                    equipmentPartSort.Add(eq_Type, dataMap);
                }
                else
                {
                    equipmentPartSort[eq_Type].Add(equipment);
                }
            }
        }

        public static List<PartsType> EquipmentPartsType()
        {
            List<PartsType> list = new List<PartsType>(equipmentPartSort.Keys);
            return list;
        }

        public static List<Equipment> EquipmentPartList(PartsType type)
        {
            List<Equipment> list = new List<Equipment>();
            if (equipmentPartSort.ContainsKey(type))
            {
                list = equipmentPartSort[type];
                return list;
            }
            return null;
        }

        public static List<EquipmentJson> EquipmentJsonPartList(PartsType type)
        {
            List<EquipmentJson> list = new List<EquipmentJson>();
            if (equipmentPartSort.ContainsKey(type))
            {
                foreach (Equipment entry in equipmentPartSort[type])
                {
                    list.Add(entry.EquipmentJson);
                }
                return list;
            }
            return null;
        }

        public static List<string> GetEquipmentName (PartsType type)
        {
            List<string> list = new List<string>();
            if (equipmentPartSort.ContainsKey(type)){
                foreach (Equipment entry in equipmentPartSort[type])
                {
                    list.Add(entry.EquipmentJson.localizedname);
                }
                return list;
            }
            return null;
        }
        
        public static List<ItemInfo> GetEquipmentMaterial(int id)
        {
            if (equipmentMap.ContainsKey(id))
            {
                List<ItemInfo> materialList = new List<ItemInfo>();
                string dividePart = equipmentMap[id].material;
                List<string> Split_List = dividePart.Split(';').ToList();
                for (int i = 0; i < Split_List.Count; i++)
                {
                    List<string> EndSplit_List = Split_List[i].Split('|').ToList();

                    int ItemId = 0;
                    int ItemCount = 0;
                    
                    if (int.TryParse(EndSplit_List[0], out ItemId) && int.TryParse(EndSplit_List[1], out ItemCount))
                    {
                        ItemInfo myinfo = new ItemInfo();
                        myinfo.itemId = Convert.ToUInt16(EndSplit_List[0]);
                        myinfo.stackCount = Convert.ToUInt16(EndSplit_List[1]);
                        materialList.Add(myinfo);
                    }
                }
                return materialList;
            }

            return null;
        }

        public static List<int> GetCurrency (int id)
        {
            if (equipmentMap.ContainsKey(id))
            {
                List<int> lis = equipmentMap[id].currency.Split(';').Select(Int32.Parse).ToList();
                return lis;
            }
            return null;
        }
    }
}