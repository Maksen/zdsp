using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class ItemMallRepo
    {
        //Kopio
        public static Dictionary<int, ItemMall_Item> mItemMap;

        public static Dictionary<ItemMallShoppingType, int> treasurePoints;
        
        static ItemMallRepo()
        {
            mItemMap = new Dictionary<int, ItemMall_Item>();
            treasurePoints = new Dictionary<ItemMallShoppingType, int>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mItemMap.Clear();
            treasurePoints.Clear();

            foreach (var value in gameData.ItemMallItem)
            {
                if (value.Value.online)
                {
                    var item = new ItemMall_Item(value.Value);
                    mItemMap.Add(value.Value.id, item);
                }
            }


            foreach (var data in gameData.ShopItemMapTreasure)
            {
                treasurePoints.Add(data.Value.treasuretype, data.Value.points);
            }
        }
        
        public static ItemMall_Item GetItemByID(int shopItemID)
        {
            return mItemMap[shopItemID];
        }
        
        public static CurrencyType GetSellCurrency(ItemMallCategory cat, ItemMallShoppingType type)
        {
            //hard code!
            if(type == ItemMallShoppingType.Bind)
            return CurrencyType.LockGold;
            else 
                return CurrencyType.Gold;
        }
        
        public static int GetTreausePoint(ItemMallShoppingType type)
        {
            if (treasurePoints.ContainsKey(type) == false)
                return 0;
            else
                return treasurePoints[type];
        }
        public static bool IsTreasureUnlock(ItemMallShoppingType type, int amt)
        {
            if (treasurePoints.ContainsKey(type) == false)
                return true;
            else
                return amt >= treasurePoints[type];
        }
    }
}
