using System.Collections.Generic;
using Kopio.JsonContracts;
using System;
using System.Linq;
using Zealot.Common;
using System.Text;

namespace Zealot.Repository
{
    public class EquipFusionRepo
    {
        private static Dictionary<int, ElementalStoneJson> elementalStoneMap;
        private static Dictionary<int, ItemSortJson> itemSortMap;
        private static Dictionary<int, FusionPartsListJson> convertMap;
        private static Dictionary<int, string> typesNameMap;
        private static Dictionary<int, EquipFusionSideEffectJson> equipFusionSideEffectMap;
        private static Dictionary<string, List<EquipFusionSideEffectJson>> equipFusionSideEffectSort;
        private static Dictionary<int, EquipFusionCostJson> equipFusionCostMap;
        private static Dictionary<int, EquipFusionAdditionRuleJson> equipFusionAdditionRuleMap;
        private static Dictionary<int, EquipFusionSellJson> equipFusionSellMap;
        private static Dictionary<int, EquipFusionStoreJson> equipFusionStoreMap;

        static EquipFusionRepo()
        {
            elementalStoneMap = new Dictionary<int, ElementalStoneJson>();
            itemSortMap = new Dictionary<int, ItemSortJson>();
            convertMap = new Dictionary<int, FusionPartsListJson>();
            typesNameMap = new Dictionary<int, string>();
            equipFusionSideEffectMap = new Dictionary<int, EquipFusionSideEffectJson>();
            equipFusionSideEffectSort = new Dictionary<string, List<EquipFusionSideEffectJson>>();
            equipFusionCostMap = new Dictionary<int, EquipFusionCostJson>();
            equipFusionAdditionRuleMap = new Dictionary<int, EquipFusionAdditionRuleJson>();
            equipFusionSellMap = new Dictionary<int, EquipFusionSellJson>();
            equipFusionStoreMap = new Dictionary<int, EquipFusionStoreJson>();
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

            foreach (KeyValuePair<int, FusionPartsListJson> entry in gameData.FusionPartsList)
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

            foreach (KeyValuePair<int, EquipFusionSideEffectJson> entry in gameData.EquipFusionSideEffect)
            {
                string effectName = entry.Value.group_id;

                if (!equipFusionSideEffectSort.ContainsKey(effectName))
                {
                    equipFusionSideEffectSort.Add(effectName, new List<EquipFusionSideEffectJson>());
                }
                equipFusionSideEffectSort[effectName].Add(entry.Value);

                int effectId = entry.Value.id;

                if (!equipFusionSideEffectMap.ContainsKey(effectId))
                {
                    equipFusionSideEffectMap.Add(effectId, entry.Value);
                }
            }

            foreach (KeyValuePair<int, EquipFusionCostJson> entry in gameData.EquipFusionCost)
            {
                int rarity = entry.Value.stone_rarity;

                if (!equipFusionCostMap.ContainsKey(rarity))
                {
                    equipFusionCostMap.Add(rarity, entry.Value);
                }
            }

            foreach (KeyValuePair<int, EquipFusionAdditionRuleJson> entry in gameData.EquipFusionAdditionRule)
            {
                int rarity = entry.Value.se_rarity;

                if (!equipFusionAdditionRuleMap.ContainsKey(rarity))
                {
                    equipFusionAdditionRuleMap.Add(rarity, entry.Value);
                }
            }

            foreach (KeyValuePair<int, EquipFusionSellJson> entry in gameData.EquipFusionSell)
            {
                int rarity = entry.Value.sell_rarity;

                if (!equipFusionSellMap.ContainsKey(rarity))
                {
                    equipFusionSellMap.Add(rarity, entry.Value);
                }
            }

            foreach (KeyValuePair<int, EquipFusionStoreJson> entry in gameData.EquipFusionStore)
            {
                int id = entry.Value.stone_id;

                if (!equipFusionStoreMap.ContainsKey(id))
                {
                    equipFusionStoreMap.Add(id, entry.Value);
                }
            }
        }

        public static ElementalStoneJson GetStoneJson (int id)
        {
            if (elementalStoneMap.ContainsKey(id))
            {
                return elementalStoneMap[id];
            }
            return null;
        }

        public static List<EquipFusionSideEffectJson> GetGemSideEffect(string GemEffect)
        {
            if (equipFusionSideEffectSort.ContainsKey(GemEffect))
            {
                return equipFusionSideEffectSort[GemEffect];
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
            return string.Empty;
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
                } while (statsLis[0] == "0|0,0|0,0|" &&
                         statsLis[1] == "0|0,0|0,0|" &&
                         statsLis[2] == "0|0,0|0,0|");

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
            if (equipFusionSideEffectSort.ContainsKey(GemEffect))
            {
                List<EquipFusionSideEffectJson> sortData = new List<EquipFusionSideEffectJson>();
                sortData = equipFusionSideEffectSort[GemEffect];
                List<int> prob = new List<int>();

                for (int i = 0; i < sortData.Count; ++i)
                {
                    prob.Add(sortData[i].weight);
                }

                int order = PowerUpUtilities.RandomWeightOrder(prob);

                if(order == -1)
                {
                    //ElementalStone effect is illegal
                    return "0|0,0|0,0|";
                }

                return GetSideEffect(sortData[order].id, sortData[order].sideeffectid);
            }
            return "0|0,0|0,0|";
        }

        static string GetSideEffect(int EffectId, string EffectString)
        {
            if (EffectString == "-1")
            {
                return "0|0,0|0,0|";
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
                Random rnd = GameUtils.GetRandomGenerator();
                int val = rnd.Next((int)sideEffect[i].min, (int)sideEffect[i].max + 1);

                valList.Append(numValue[i]);
                valList.Append(",");
                valList.Append(val.ToString());
                valList.Append("|");
            }

            for (int i = numValue.Count; i < 2; ++i)
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
            List<string> lis = new List<string>();

            for (int i = 0; i < 3; ++i)
            {
                for (int j = 1; j < 3; ++j)
                {
                    int order = i * 3 + j;

                    if (effectGroup[order] != "0,0")
                    {
                        List<string> devideLis = effectGroup[order].Split(',').ToList();
                        StringBuilder bind = new StringBuilder(SideEffectRepo.GetSideEffect(int.Parse(devideLis[0])).localizedname);
                        bind.Append("+");
                        bind.Append(devideLis[1]);
                        lis.Add(bind.ToString());
                    }
                    else
                    {
                        lis.Add(string.Empty);
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
                totalCurrency += equipFusionCostMap[rarity].cost_currency;
                rarity = (int)elementalStoneMap[GemId2].rarity;
                totalCurrency += equipFusionCostMap[rarity].cost_currency;
                rarity = (int)elementalStoneMap[GemId3].rarity;
                totalCurrency += equipFusionCostMap[rarity].cost_currency;
                return totalCurrency;
            }
            return -1;
        }
        #endregion

        #region GetAdditionRule
        static int AdditionWeightSearch (int id)
        {
            if (equipFusionSideEffectMap.ContainsKey(id))
            {
                int rarity = 0;
                rarity = equipFusionSideEffectMap[id].se_rarity;
                return equipFusionAdditionRuleMap[rarity].weight;
            }
            return -1;
        }
        #endregion

        #region RandomEquipFusion
        public static string RandomSideEffectPutEquip (List<ElementalStone> SlotsStoneEffect)
        {
            //!!!this part probably rewrite or add random type
            List<string> stoneEffectList = new List<string>() { SlotsStoneEffect[0].FusionData, SlotsStoneEffect[1].FusionData, SlotsStoneEffect[2].FusionData };

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
                int effectId = int.Parse(effectList[i * 3]);
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
                order = PowerUpUtilities.RandomWeightOrder(weight);
            } while (effectList[order * 3] == "0");

            int startLength = order * 3;
            int maxLenght = (order + 1) * 3;

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
        
        static List<string> SortEffectList (List<string> effectString)
        {
            List<string> lis = effectString;
            for (int i = 2; i > -1; --i)
            {
                if(lis[i] == "0|0,0|0,0|")
                {
                    lis.RemoveAt(i);
                    lis.Add("0|0,0|0,0|");
                }
            }
            return lis;
        }

        public static List<string> BuildEquipStats(Equipment equip)
        {
            List<string> lis = new List<string>();
            lis.Add(equip.GetEquipmentName());
            lis.Add(equip.UpgradeLevel.ToString());
            StringBuilder st = new StringBuilder("+");
            st.Append(equip.ReformStep);
            lis.Add(st.ToString());
            lis.Add("階");
            return lis;
        }
    }
}