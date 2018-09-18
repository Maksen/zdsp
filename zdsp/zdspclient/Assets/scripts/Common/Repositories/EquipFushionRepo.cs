using System.Collections.Generic;
using Kopio.JsonContracts;
using System;
using System.Linq;
using Zealot.Common;
using System.Text;

namespace Zealot.Repository
{
    public class EquipFushionRepo
    {
        private static Dictionary<int, ElementalStoneJson> elementalStoneMap;
        private static Dictionary<int, ItemSortJson> itemSortMap;
        private static Dictionary<int, FushionPartsListJson> convertMap;
        private static Dictionary<int, string> typesNameMap;
        private static Dictionary<int, EquipFushionSideEffectJson> equipFushionSideEffectMap;
        private static Dictionary<string, List<EquipFushionSideEffectJson>> equipFushionSideEffectSort;
        private static Dictionary<int, EquipFushionCostJson> equipFushionCostMap;
        private static Dictionary<int, EquipFushionAdditionRuleJson> equipFushionAdditionRuleMap;
        private static Dictionary<int, EquipFushionSellJson> equipFushionSellMap;
        private static Dictionary<int, EquipFushionStoreJson> equipFushionStoreMap;

        static EquipFushionRepo()
        {
            elementalStoneMap = new Dictionary<int, ElementalStoneJson>();
            itemSortMap = new Dictionary<int, ItemSortJson>();
            convertMap = new Dictionary<int, FushionPartsListJson>();
            typesNameMap = new Dictionary<int, string>();
            equipFushionSideEffectMap = new Dictionary<int, EquipFushionSideEffectJson>();
            equipFushionSideEffectSort = new Dictionary<string, List<EquipFushionSideEffectJson>>();
            equipFushionCostMap = new Dictionary<int, EquipFushionCostJson>();
            equipFushionAdditionRuleMap = new Dictionary<int, EquipFushionAdditionRuleJson>();
            equipFushionSellMap = new Dictionary<int, EquipFushionSellJson>();
            equipFushionStoreMap = new Dictionary<int, EquipFushionStoreJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            foreach (KeyValuePair<int, ElementalStoneJson> entry in gameData.ElementalStone)
            {
                int stoneId = entry.Value.itemid;

                if (!elementalStoneMap.ContainsKey(stoneId))
                {
                    elementalStoneMap.Add(stoneId, entry.Value);
                }
            }

            foreach (KeyValuePair<int, ItemSortJson> entry in gameData.ItemSort)
            {
                int itemId = entry.Value.id;
                if (!itemSortMap.ContainsKey(itemId))
                {
                    itemSortMap.Add(itemId, entry.Value);
                }
            }

            foreach (KeyValuePair<int, FushionPartsListJson> entry in gameData.FushionPartsList)
            {
                int convertId = entry.Value.sort_id;
                if (!convertMap.ContainsKey(convertId))
                {
                    convertMap.Add(convertId, entry.Value);
                }

                int typeId = entry.Value.type_id;
                if (!typesNameMap.ContainsKey(typeId))
                {
                    typesNameMap.Add(typeId, entry.Value.type_name);
                }
            }

            foreach (KeyValuePair<int, EquipFushionSideEffectJson> entry in gameData.EquipFushionSideEffect)
            {
                string effectName = entry.Value.group_id;

                if (!equipFushionSideEffectSort.ContainsKey(effectName))
                {
                    equipFushionSideEffectSort.Add(effectName, new List<EquipFushionSideEffectJson>());
                }
                equipFushionSideEffectSort[effectName].Add(entry.Value);

                int effectId = entry.Value.id;

                if (!equipFushionSideEffectMap.ContainsKey(effectId))
                {
                    equipFushionSideEffectMap.Add(effectId, entry.Value);
                }
            }

            foreach (KeyValuePair<int, EquipFushionCostJson> entry in gameData.EquipFushionCost)
            {
                int rarity = entry.Value.stone_rarity;

                if (!equipFushionCostMap.ContainsKey(rarity))
                {
                    equipFushionCostMap.Add(rarity, entry.Value);
                }
            }

            foreach (KeyValuePair<int, EquipFushionAdditionRuleJson> entry in gameData.EquipFushionAdditionRule)
            {
                int rarity = entry.Value.se_rarity;

                if (!equipFushionAdditionRuleMap.ContainsKey(rarity))
                {
                    equipFushionAdditionRuleMap.Add(rarity, entry.Value);
                }
            }

            foreach (KeyValuePair<int, EquipFushionSellJson> entry in gameData.EquipFushionSell)
            {
                int rarity = entry.Value.sell_rarity;

                if (!equipFushionSellMap.ContainsKey(rarity))
                {
                    equipFushionSellMap.Add(rarity, entry.Value);
                }
            }

            foreach (KeyValuePair<int, EquipFushionStoreJson> entry in gameData.EquipFushionStore)
            {
                int id = entry.Value.stone_id;

                if (!equipFushionStoreMap.ContainsKey(id))
                {
                    equipFushionStoreMap.Add(id, entry.Value);
                }
            }
        }

        public static List<EquipFushionSideEffectJson> GetGemSideEffect(string GemEffect)
        {
            if (equipFushionSideEffectSort.ContainsKey(GemEffect))
            {
                return equipFushionSideEffectSort[GemEffect];
            }
            return null;
        }

        public static int ConvertStoneType (int sortId)
        {
            if (itemSortMap.ContainsKey(sortId))
            {
                return convertMap[itemSortMap[sortId].itemsortid].type_id;
            }
            return 0;
        }
        
        public static string StoneTypeGetName(int type)
        {
            if (typesNameMap.ContainsKey(type))
            {
                return typesNameMap[type];
            }
            return "";
        }

        public static Dictionary<int, ItemSortJson> Look()
        {
            return itemSortMap;
        }

        #region RandomGem
        public static string RandomGemEffect(int GemId)
        {
            if (elementalStoneMap.ContainsKey(GemId))
            {
                List<string> statsLis = new List<string>();

                do
                {
                    statsLis.Clear();
                    statsLis.Add(RandomGemEffectBySideEffectId(elementalStoneMap[GemId].se1group));
                    statsLis.Add(RandomGemEffectBySideEffectId(elementalStoneMap[GemId].se2group));
                    statsLis.Add(RandomGemEffectBySideEffectId(elementalStoneMap[GemId].se3group));
                } while (statsLis[0] == "0|0,0|0,0|0,0|0,0|0,0|" &&
                         statsLis[1] == "0|0,0|0,0|0,0|0,0|0,0|" &&
                         statsLis[2] == "0|0,0|0,0|0,0|0,0|0,0|");

                statsLis = SortEffectList(statsLis);

                StringBuilder bind = new StringBuilder();
                bind.Append(statsLis[0]);
                bind.Append(statsLis[1]);
                bind.Append(statsLis[2]);
                bind.Remove(bind.Length - 1, 1);

                return bind.ToString();
            }
            return null;
        }

        static string RandomGemEffectBySideEffectId(string GemEffect)
        {
            if (equipFushionSideEffectSort.ContainsKey(GemEffect))
            {
                List<EquipFushionSideEffectJson> sortData = new List<EquipFushionSideEffectJson>();
                sortData = equipFushionSideEffectSort[GemEffect];
                List<int> prob = new List<int>();

                for (int i = 0; i < sortData.Count; ++i)
                {
                    prob.Add(sortData[i].weight);
                }

                int order = RandomUtility(prob);

                if(order == -1)
                {
                    Console.Write("Kopio weight is error, please write probably weight");
                    return "0|0,0|0,0|0,0|0,0|0,0|";
                }

                return GetSideEffect(sortData[order].id, sortData[order].sideeffectid);
            }
            return "0|0,0|0,0|0,0|0,0|0,0|";
        }

        static string GetSideEffect(int EffectId, string EffectString)
        {
            if (EffectString == "-1")
            {
                return "0|0,0|0,0|0,0|0,0|0,0|";
            }
            List<int> numValue = EffectString.Split(';').Select(int.Parse).ToList();
            List<SideEffectJson> sideEffect = new List<SideEffectJson>();

            for (int i = 0; i < numValue.Count; ++i)
            {
                sideEffect.Add(SideEffectRepo.GetSideEffect(numValue[i]));
            }

            StringBuilder valList = new StringBuilder();

            valList.Append(EffectId.ToString());
            valList.Append("|");

            for (int i = 0; i < numValue.Count; ++i)
            {
                Random rnd = new Random();
                int val = rnd.Next((int)sideEffect[i].min, (int)sideEffect[i].max + 1);

                valList.Append(numValue[i]);
                valList.Append(",");
                valList.Append(val.ToString());
                valList.Append("|");
            }

            for (int i = numValue.Count; i < 5; ++i)
            {
                valList.Append("0,0|");
            }
            return valList.ToString();
        }
        #endregion

        #region GetSlotsGem
        public static List<string> DecodeEffect(string EffectString)
        {
            List<string> effectGroup = EffectString.Split('|').ToList();
            List<string> effectLis = new List<string>();

            int loopCount = 0;

            if(effectGroup[0] == "0")
            {
                return new List<string>();
            }
            else if(effectGroup[6] == "0")
            {
                loopCount = 1;
                for (int i = 0; i < 6; ++i)
                {
                    effectLis.Add(effectGroup[i]);
                }
            }
            else if(effectGroup[12] == "0")
            {
                loopCount = 2;
                for (int i = 0; i < 12; ++i)
                {
                    effectLis.Add(effectGroup[i]);
                }
            }
            else
            {
                loopCount = 3;
                for (int i = 0; i < 18; ++i)
                {
                    effectLis.Add(effectGroup[i]);
                }
            }

            List<string> lis = new List<string>();

            for (int i = 0; i < loopCount; ++i)
            {
                StringBuilder bind = new StringBuilder();
                for (int j = 1; j < 6; ++j)
                {
                    int order = i * 6 + j;

                    if (effectLis[order] != "0,0")
                    {
                        List<string> devideLis = effectLis[order].Split(',').ToList();
                        bind.Append(SideEffectRepo.GetSideEffect(int.Parse(devideLis[0])).localizedname);
                        bind.Append("+");
                        bind.Append(devideLis[1]);
                        bind.Append(", ");
                    }
                    else
                    {
                        bind.Remove(bind.ToString().Length - 2, 2);
                        lis.Add(bind.ToString());
                        break;
                    }

                    if(j == 5)
                    {
                        bind.Remove(bind.ToString().Length - 2, 2);
                        lis.Add(bind.ToString());
                    }
                }
            }
            return lis;
        }

        public static int GetTotalCurrencyCount(int GemId1, int GemId2, int GemId3)
        {
            if(elementalStoneMap.ContainsKey(GemId1) && elementalStoneMap.ContainsKey(GemId2) && elementalStoneMap.ContainsKey(GemId3))
            {
                int totalCurrency = 0, rarity = 0;
                rarity = (int)elementalStoneMap[GemId1].rarity;
                totalCurrency += equipFushionCostMap[rarity].cost_currency;
                rarity = (int)elementalStoneMap[GemId2].rarity;
                totalCurrency += equipFushionCostMap[rarity].cost_currency;
                rarity = (int)elementalStoneMap[GemId3].rarity;
                totalCurrency += equipFushionCostMap[rarity].cost_currency;
                return totalCurrency;
            }
            return -1;
        }
        #endregion

        #region GetAdditionRule
        static int AdditionWeightSearch (int id)
        {
            if (equipFushionSideEffectMap.ContainsKey(id))
            {
                int rarity = 0;
                rarity = equipFushionSideEffectMap[id].se_rarity;
                return equipFushionAdditionRuleMap[rarity].weight;
            }
            return -1;
        }
        #endregion

        #region RandomEquipFushion
        public static string RandomSideEffectPutEquip (List<ElementalStone> SlotsStoneEffect)
        {
            //!!!this part probably rewrite or add random type
            List<string> stoneEffectList = new List<string>() { SlotsStoneEffect[0].FushionData, SlotsStoneEffect[1].FushionData, SlotsStoneEffect[2].FushionData };

            StringBuilder bind = new StringBuilder();
            bind.Append(GetAttributeInGemEffect(stoneEffectList[0]));
            bind.Append(GetAttributeInGemEffect(stoneEffectList[1]));
            bind.Append(GetAttributeInGemEffect(stoneEffectList[2]));
            bind.Remove(bind.Length - 1, 1);
            //!!!this part probably rewrite or add random type
            return bind.ToString();
        }

        static string GetAttributeInGemEffect (string effect)
        {
            List<string> effectList = effect.Split('|').ToList();
            List<int> weight = new List<int>();

            for (int i = 0; i < 3; ++i)
            {
                int effectId = int.Parse(effectList[i * 6]);
                if (effectId != 0)
                {
                    weight.Add(AdditionWeightSearch(effectId));
                }
                else
                {
                    break;
                }
            }

            int order = 0;
            do
            {
                order = RandomUtility(weight);
            } while (effectList[order * 6] == "0");

            int startLength = order * 6;
            int maxLenght = (order + 1) * 6;

            StringBuilder bind = new StringBuilder();
            bind.Append(effectList[startLength]);
            bind.Append("|");
            for (int i = startLength + 1; i < maxLenght; ++i)
            {
                bind.Append(effectList[i]);
                bind.Append("|");
            }

            return bind.ToString();
        }
        #endregion

        static int RandomUtility (List<int> probability)
        {
            int totalWeight = 0;
            for (int i = 0; i < probability.Count; ++i)
            {
                totalWeight += probability[i];
            }
            if(totalWeight == 0)
            {
                return -1;
            }

            Random rnd = new Random();
            int target = rnd.Next(0, totalWeight);
            int compare = 0;

            for (int i = 0; i < probability.Count; ++i)
            {
                int addition = probability[i] + compare;
                if(probability[i] == 0)
                {
                    continue;
                }
                if(target >= compare && addition > target)
                {
                    return i;
                } else
                {
                    compare = addition;
                }
            }
            return -1;
        }
        
        static List<string> SortEffectList (List<string> effectString)
        {
            List<string> lis = effectString;
            for (int i = 2; i > -1; --i)
            {
                if(lis[i] == "0|0,0|0,0|0,0|0,0|0,0|")
                {
                    lis.RemoveAt(i);
                    lis.Add("0|0,0|0,0|0,0|0,0|0,0|");
                }
            }
            return lis;
        }
    }
}