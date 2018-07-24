using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Zealot.Repository;

namespace Zealot.Common
{
    /// <summary>
    /// Stores the number of times player can still exchange for items in UI_ExchangeShop
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ExchangeShopInventory
    {
        [JsonProperty(PropertyName = "lstexno")]
        public Dictionary<int, int> map_exchangeLeft = new Dictionary<int, int>();

        public void InitDefault()
        {
            map_exchangeLeft = new Dictionary<int, int>();

            NewDayReset();
        }

        public void NewDayReset()
        {
            //Grab the exchange item data
            var itemMap = ExchangeShopRepo.mExchangeShopItemMap;

            //First initialization
            if (map_exchangeLeft.Count == 0)
            {
                foreach (var e in itemMap)
                {
                    map_exchangeLeft.Add(e.Value.exid, e.Value.daily_exchange);
                }
            }
            else
            {
                foreach (var e in itemMap)
                {
                    //if new entry for exchangeshop
                    if (!map_exchangeLeft.ContainsKey(e.Value.exid))
                        map_exchangeLeft.Add(e.Value.exid, e.Value.daily_exchange);

                    //if item does reset daily, refresh exchanges
                    else if (e.Value.daily_exchange_everyday)
                        map_exchangeLeft[e.Value.exid] = e.Value.daily_exchange;
                }
            }
        }

        public void UpdateInventoryList()
        {
            var itemMap = ExchangeShopRepo.mExchangeShopItemMap;

            foreach (var e in itemMap)
            {
                //if new entry
                if (!map_exchangeLeft.ContainsKey(e.Value.exid))
                    map_exchangeLeft.Add(e.Value.exid, e.Value.daily_exchange);
            }
        }

        public void SaveToExchangeShopInventory(string datamap)
        {
            map_exchangeLeft.Clear();
            map_exchangeLeft = JsonConvert.DeserializeObject<Dictionary<int, int>>(datamap);
        }

        public bool NeedReset()
        {
            return false;
        }
    }
}
