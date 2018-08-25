using Kopio.JsonContracts;
using System.Collections.Generic;

namespace Zealot.Repository
{
    public static class ExchangeShopRepo
    {
        public static Dictionary<int, ExchangeShopItemJson> mExchangeShopItemMap;   //ex id => item json
        public static Dictionary<int, ExchangeShopCategoryJson> mExchangeShopClassMap; //categoryid => class json

        public static Dictionary<ExchangeShopCategoryJson, List<ExchangeShopItemJson>> mExchangeShopInventoryMap; //class json => list<item json>
        public static Dictionary<int, List<ExchangeShopItemJson>> mExchangeShopInventoryMap2; //categoryid => list<item json>
        public static Dictionary<int, int> mExchangeShopItemToClassMap; //item id => categoryid

        //ex id => Dic<itemreq_id #, item count>. This stores the total item count for req items, should there any item id equal to each other
        public static Dictionary<int, Dictionary<int, int>> mExchangeShopItemReqTotalCountMap;

        static ExchangeShopRepo()
        {
            mExchangeShopItemMap = new Dictionary<int, ExchangeShopItemJson>();
            mExchangeShopClassMap = new Dictionary<int, ExchangeShopCategoryJson>();

            mExchangeShopInventoryMap = new Dictionary<ExchangeShopCategoryJson, List<ExchangeShopItemJson>>();
            mExchangeShopInventoryMap2 = new Dictionary<int, List<ExchangeShopItemJson>>();
            mExchangeShopItemToClassMap = new Dictionary<int, int>();

            mExchangeShopItemReqTotalCountMap = new Dictionary<int, Dictionary<int, int>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mExchangeShopItemMap.Clear();
            mExchangeShopClassMap.Clear();
            mExchangeShopInventoryMap.Clear();
            mExchangeShopInventoryMap2.Clear();
            mExchangeShopItemToClassMap.Clear();
            mExchangeShopItemReqTotalCountMap.Clear();

            Dictionary<int, ExchangeShopCategoryJson> exShopClass = gameData.ExchangeShopCategory;
            Dictionary<int, ExchangeShopItemJson> exShopItem = gameData.ExchangeShopItem;

            //Create all the different item class
            foreach (var iclass in exShopClass)
            {
                //create item class, and its item list
                mExchangeShopClassMap.Add(iclass.Value.categoryid, iclass.Value);
                mExchangeShopInventoryMap.Add(iclass.Value, new List<ExchangeShopItemJson>());
                mExchangeShopInventoryMap2.Add(iclass.Value.categoryid, new List<ExchangeShopItemJson>());

                //load all apporpiate items into the item list
                foreach (var item in exShopItem)
                {
                    if (item.Value.categoryid == iclass.Value.categoryid)
                    {
                        mExchangeShopInventoryMap[iclass.Value].Add(item.Value);
                        mExchangeShopInventoryMap2[iclass.Value.categoryid].Add(item.Value);
                    }
                }
            }

            //Load mExchangeShopItemMap
            foreach (var item in exShopItem)
            {
                mExchangeShopItemMap.Add(item.Value.exid, item.Value);
                mExchangeShopItemToClassMap.Add(item.Value.exid, item.Value.categoryid);



                //Create a new exchange shop recipe entry
                mExchangeShopItemReqTotalCountMap.Add(item.Value.exid, new Dictionary<int, int>());

                //Add the first req item
                if (item.Value.item1_id > 0)
                    mExchangeShopItemReqTotalCountMap[item.Value.exid].Add(item.Value.item1_id, item.Value.item1_count);

                //Check if 2nd req item id == 1st req item id
                if (item.Value.item2_id > 0)
                {
                    if (mExchangeShopItemReqTotalCountMap[item.Value.exid].ContainsKey(item.Value.item2_id) == false)
                    {
                        mExchangeShopItemReqTotalCountMap[item.Value.exid].Add(item.Value.item2_id, item.Value.item2_count);
                    }
                    else
                    {
                        mExchangeShopItemReqTotalCountMap[item.Value.exid][item.Value.item2_id] += item.Value.item2_count;
                    }
                }

                //Check if 3rd req item id == 1st/2nd req item id
                if (item.Value.item3_id > 0)
                {
                    if (mExchangeShopItemReqTotalCountMap[item.Value.exid].ContainsKey(item.Value.item3_id) == false)
                    {
                        mExchangeShopItemReqTotalCountMap[item.Value.exid].Add(item.Value.item3_id, item.Value.item3_count);
                    }
                    else
                    {
                        mExchangeShopItemReqTotalCountMap[item.Value.exid][item.Value.item3_id] += item.Value.item3_count;
                    }
                }

                //Check if 4th req item id == 1st/2nd/3rd req item id
                if (item.Value.item4_id > 0)
                {
                    if (mExchangeShopItemReqTotalCountMap[item.Value.exid].ContainsKey(item.Value.item4_id) == false)
                    {
                        mExchangeShopItemReqTotalCountMap[item.Value.exid].Add(item.Value.item4_id, item.Value.item4_count);
                    }
                    else
                    {
                        mExchangeShopItemReqTotalCountMap[item.Value.exid][item.Value.item4_id] += item.Value.item4_count;
                    }
                }
            }
        }

    }
}
