using Kopio.JsonContracts;
using System;
using System.Linq;
using System.Collections.Generic;
using Zealot.Common;

namespace Zealot.Repository
{
    public class PowerUpUtilities
    {
        public static int RandomWeightOrder(List<int> probability)
        {
            int totalWeight = 0;
            for (int i = 0; i < probability.Count; ++i)
            {
                totalWeight += probability[i];
            }
            if (totalWeight == 0)
            {
                return -1;
            }

            Random rnd = GameUtils.GetRandomGenerator();
            int target = rnd.Next(0, totalWeight);
            int compare = 0;

            for (int i = 0; i < probability.Count; ++i)
            {
                int addition = probability[i] + compare;
                if (probability[i] == 0)
                {
                    continue;
                }
                if (target >= compare && addition > target)
                {
                    return i;
                }
                else
                {
                    compare = addition;
                }
            }
            return -1;
        }

        public static List<ItemInfo> ConvertMaterialFormat(string material)
        {
            List<ItemInfo> materialList = new List<ItemInfo>();
            string matStrList = material;
            List<string> Split_List = matStrList.Split(';').ToList();
            for (int i = 0; i < Split_List.Count; ++i)
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
                else
                {
                    continue;
                }
            }
            return materialList;
        }
    }

    public class PowerUpRepo
    {
        #region PartsPowerUp
        static Dictionary<PowerUpPartsType, Dictionary<int, PowerUpJson>> partsLvlMap;
        static Dictionary<PowerUpPartsType, Dictionary<int, SideEffectJson>> partsEffectMap;
        static Dictionary<int, int> partsTypeConverter;
        #endregion

        #region MeridianPowerUp
        static Dictionary<int, Dictionary<int, MeridianUnlockListJson>> meridianUnlockSort;
        static Dictionary<int, Dictionary<int, MeridianExpListJson>> meridianExpSort;
        #endregion

        static PowerUpRepo()
        {
            #region PartsPowerUp
            partsLvlMap = new Dictionary<PowerUpPartsType, Dictionary<int, PowerUpJson>>();
            partsEffectMap = new Dictionary<PowerUpPartsType, Dictionary<int, SideEffectJson>>();
            partsTypeConverter = new Dictionary<int, int>();
            #endregion

            #region MeridianPowerUp
            meridianUnlockSort = new Dictionary<int, Dictionary<int, MeridianUnlockListJson>>();
            meridianExpSort = new Dictionary<int, Dictionary<int, MeridianExpListJson>>();
            #endregion
        }

        public static void Init(GameDBRepo gameData)
        {
            #region PartsPowerUp
            foreach (KeyValuePair<int, PowerUpJson> entry in gameData.PowerUp)
            {
                PowerUpPartsType part = entry.Value.part;
                int level = entry.Value.power;
                if(!partsLvlMap.ContainsKey(part))
                {
                    partsLvlMap.Add(part, new Dictionary<int, PowerUpJson>());
                }

                partsLvlMap[part].Add(level, entry.Value);
            }

            foreach (KeyValuePair<int, PowerUpPartsListJson> entry in gameData.PowerUpPartsList)
            {
                int parts = entry.Value.part;
                int powerUpParts = entry.Value.poweruppart;

                if (!partsTypeConverter.ContainsKey(parts))
                {
                    partsTypeConverter.Add(parts, powerUpParts);
                }
            }
            #endregion

            #region MeridianPowerUp
            foreach (KeyValuePair<int, MeridianUnlockListJson> entry in gameData.MeridianUnlockList)
            {
                int type = entry.Value.mltype;
                if (!meridianUnlockSort.ContainsKey(type))
                {
                    meridianUnlockSort.Add(type, new Dictionary<int, MeridianUnlockListJson>());
                }
                meridianUnlockSort[type].Add(entry.Value.mlrank, entry.Value);
            }

            foreach (KeyValuePair<int, MeridianExpListJson> entry in gameData.MeridianExpList)
            {
                int type = entry.Value.mltype;
                if (!meridianExpSort.ContainsKey(type))
                {
                    meridianExpSort.Add(type, new Dictionary<int, MeridianExpListJson>());
                }
                meridianExpSort[type].Add(entry.Value.mlrank, entry.Value);
            }
            #endregion
        }

        #region PartsPowerUp
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
                    return PowerUpUtilities.ConvertMaterialFormat(partsLvlMap[part][level].material);
                }
            }
            return null;
        }

        public static int PartsTypeValue (int partsType)
        {
            if (partsTypeConverter.ContainsKey(partsType))
            {
                return partsTypeConverter[partsType];
            }
            return -1;
        }
        #endregion

        #region MeridianPowerUp
        public static MeridianUnlockListJson GetMeridianUnlockByTypesLevel(int type, int level)
        {
            if (meridianUnlockSort.ContainsKey(type))
            {
                if (meridianUnlockSort[type].ContainsKey(level))
                {
                    return meridianUnlockSort[type][level];
                }
            }
            return null;
        }

        public static MeridianExpListJson GetMeridianExpByTypesLevel(int type, int level)
        {
            if (meridianExpSort.ContainsKey(type))
            {
                if (meridianExpSort[type].ContainsKey(level))
                {
                    return meridianExpSort[type][level];
                }
            }
            return null;
        }

        public static List<ItemInfo> GetMeridianUnlockMaterial(int type, int level)
        {
            if (meridianUnlockSort.ContainsKey(type))
            {
                if (meridianUnlockSort[type].ContainsKey(level))
                {
                    return PowerUpUtilities.ConvertMaterialFormat(meridianUnlockSort[type][level].item);
                }
            }
            return null;
        }

        public static List<ItemInfo> GetMeridianExpMaterial(int type, int level)
        {
            if (meridianExpSort.ContainsKey(type))
            {
                if (meridianExpSort[type].ContainsKey(level))
                {
                    return PowerUpUtilities.ConvertMaterialFormat(meridianExpSort[type][level].item);
                }
            }
            return null;
        }

        public static int MeridianCriticalExp (int type, int level)
        {
            MeridianExpListJson expData = meridianExpSort[type][level];
            int ctr1 = expData.multiple1;
            int ctr2 = expData.multiple2;
            int ctr3 = expData.multiple3;
            int ctrDefault = 100000 - ctr1 - ctr2 - ctr3;
            List<int> randomMath = new List<int>() { ctr1, ctr2, ctr3, ctrDefault };

            switch (PowerUpUtilities.RandomWeightOrder(randomMath))
            {
                case 0:
                    return expData.crt1;
                case 1:
                    return expData.crt2;
                case 2:
                    return expData.crt3;
                case 3:
                    return 1;
                default:
                    break;
            }
            return 1;
        }
        #endregion
    }
}
