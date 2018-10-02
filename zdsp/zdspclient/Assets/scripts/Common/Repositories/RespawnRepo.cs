using Kopio.JsonContracts;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
    public class RespawnRepo
    {
        public static Dictionary<int, RespawnJson> respawnIDJsonMap; // RespawnID -> Json

        static RespawnRepo()
        {
            respawnIDJsonMap = new Dictionary<int, RespawnJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            foreach(KeyValuePair<int, RespawnJson> entry in gameData.Respawn)
            {
                int respawnID = entry.Value.respawnid;
                if(respawnID > 0 && !respawnIDJsonMap.ContainsKey(respawnID))
                {
                    respawnIDJsonMap.Add(respawnID, entry.Value);
                }
            }
        }

        public static RespawnJson GetRespawnDataByID(int respawnId)
        {
            if(respawnIDJsonMap.ContainsKey(respawnId))
            {
                return respawnIDJsonMap[respawnId];
            }

            return null;
        }

        public static List<ItemInfo> GetItemListByID(int respawnId)
        {
            RespawnJson respawnData = GetRespawnDataByID(respawnId);
            if(respawnData == null)
            {
                return new List<ItemInfo>();
            }

            string itemListStr = respawnData.deductitem;
            if (string.IsNullOrEmpty(itemListStr))
            {
                return new List<ItemInfo>();
            }

            List<ItemInfo> itemList = new List<ItemInfo>();

            List<string> itemListDataList = itemListStr.Split('|').ToList();
            if(itemListDataList != null)
            {
                for(int i = 0; i < itemListDataList.Count; ++i)
                {
                    int itemid = 0;
                    int amount = 0;
                    List<string> itemDataList = itemListDataList[i].Split(';').ToList();
                    if (int.TryParse(itemDataList[0], out itemid) && int.TryParse(itemDataList[1], out amount))
                    {
                        ItemInfo newItem = new ItemInfo();
                        newItem.itemId = (ushort)itemid;
                        newItem.stackCount = amount;
                        itemList.Add(newItem);
                    }
                }
            }

            return itemList;
        }

        public static List<CurrencyInfo> GetCurrencyListByID(int respawnId)
        {
            RespawnJson respawnData = GetRespawnDataByID(respawnId);
            if (respawnData == null)
            {
                return new List<CurrencyInfo>();
            }

            string currencyListStr = respawnData.deductcurrency;
            if (string.IsNullOrEmpty(currencyListStr))
            {
                return new List<CurrencyInfo>();
            }

            List<CurrencyInfo> currencyList = new List<CurrencyInfo>();

            List<string> currencyListDataList = currencyListStr.Split('|').ToList();
            if(currencyListDataList != null)
            {
                for(int i = 0; i < currencyListDataList.Count; ++i)
                {
                    int currencyid = 0;
                    int amount = 0;
                    List<string> currencyDataList = currencyListDataList[i].Split(';').ToList();
                    if (int.TryParse(currencyDataList[0], out currencyid) && int.TryParse(currencyDataList[1], out amount))
                    {
                        CurrencyInfo newCurrency = new CurrencyInfo();
                        newCurrency.currencyType = (CurrencyType)currencyid;
                        newCurrency.amount = amount;
                        currencyList.Add(newCurrency);
                    }
                }
            }

            return currencyList;
        }
    }
}
