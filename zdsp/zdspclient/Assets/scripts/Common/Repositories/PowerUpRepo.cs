using Kopio.JsonContracts;
using System;
using System.Linq;
using System.Collections.Generic;
using Zealot.Common;

namespace Zealot.Repository
{
    public class PowerUpRepo
    {
        public static Dictionary<PowerUpPartsType, Dictionary<int, PowerUpJson>> partsLvlMap;
        public static Dictionary<PowerUpPartsType, Dictionary<int, SideEffectJson>> partsEffectMap;

        static PowerUpRepo()
        {
            partsLvlMap = new Dictionary<PowerUpPartsType, Dictionary<int, PowerUpJson>>();
            partsEffectMap = new Dictionary<PowerUpPartsType, Dictionary<int, SideEffectJson>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            foreach(KeyValuePair<int, PowerUpJson> entry in gameData.PowerUp)
            {
                PowerUpPartsType part = entry.Value.part;
                int level = entry.Value.power;
                if(!partsLvlMap.ContainsKey(part))
                {
                    Dictionary<int, PowerUpJson> levelMap = new Dictionary<int, PowerUpJson>();
                    partsLvlMap.Add(part, levelMap);
                }

                partsLvlMap[part].Add(level, entry.Value);
            }
        }

        
        public static PowerUpJson GetPowerUpByPartsLevel(PowerUpPartsType part, int level)
        {
            if(partsLvlMap.ContainsKey(part))
            {
                if(partsLvlMap[part].ContainsKey(level))
                {
                    return partsLvlMap[part][level];
                }
            }
            
            return null;
        }

        
        public static List<ItemInfo> GetPowerUpMaterialByPartsEffect(PowerUpPartsType part, int level)
        {
            if (partsLvlMap.ContainsKey(part))
            {
                if (partsLvlMap[part].ContainsKey(level))
                {
                    List<ItemInfo> materialList = new List<ItemInfo>();
                    string matStrList = partsLvlMap[part][level].material;
                    List<string> Split_List = matStrList.Split(';').ToList();
                    for (int i = 0; i < Split_List.Count; i++)
                    {
                        List<string> EndSplit_List = Split_List[i].Split('|').ToList();

                        int ItemId = 0;
                        int ItemCount = 0;

                        //若兩個String皆可轉換成Int
                        if (int.TryParse(EndSplit_List[0], out ItemId) && int.TryParse(EndSplit_List[1], out ItemCount))
                        {
                            ItemInfo myinfo = new ItemInfo();
                            myinfo.itemId = Convert.ToUInt16(EndSplit_List[0]);
                            myinfo.stackCount = Convert.ToUInt16(EndSplit_List[1]);
                            materialList.Add(myinfo);
                        }
                    } //end for
                    return materialList;
                }
            }

            return null;
        }
    }
}
