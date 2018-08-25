using Kopio.JsonContracts;
using System.Collections.Generic;
using System.Linq;
using System;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class StoreRepo
    {
        public class StoreSetting
        {
            public int category;//StoreOrder
            public string localizedname;
            public int heroID;
            public UIStoreLinkType shopType;
            public int CommodityCount;
            public bool OnRefresh;
            public TimeSpan RefreshTime1;//2359
            public TimeSpan RefreshTime2;
            public TimeSpan RefreshTime3;
            public CurrencyType CurrencyType;
            public int LvlReq;
            public StoreSetting(StoreSetJson json)
            {
                category = json.storeorder;
                localizedname = json.localizedname;
                heroID = json.heroid;
                shopType = json.shoptype;
                CommodityCount = json.commoditycount;
                OnRefresh = json.refresh;
                CurrencyType = json.currencytype;
                RefreshTime1 = RefreshTime2 =  RefreshTime3 = TimeSpan.Zero;
                LvlReq = json.openlv;

                string s_hr = json.refreshtime1.Substring(0, 2);
                string s_min = json.refreshtime1.Substring(2, 2);
                int hr=0, min = 0;
                bool success = int.TryParse(s_hr, out hr) && int.TryParse(s_min, out min);
                if (success)
                {
                    RefreshTime1 = new TimeSpan(hr, min, 0);
                }

                if (json.refreshtime2.Length > 0)
                {
                    s_hr = json.refreshtime2.Substring(0, 2);
                    s_min = json.refreshtime2.Substring(2, 2);
                    hr = 0;
                    min = 0;
                    success = int.TryParse(s_hr, out hr) && int.TryParse(s_min, out min);
                    if (success)
                    {
                        RefreshTime2 = new TimeSpan(hr, min, 0);
                    }
                }

                if (json.refreshtime3.Length > 0)
                {
                    s_hr = json.refreshtime3.Substring(0, 2);
                    s_min = json.refreshtime3.Substring(2, 2);
                    hr = 0;
                    min = 0;
                    success = int.TryParse(s_hr, out hr) && int.TryParse(s_min, out min);
                    if (success)
                    {
                        RefreshTime3 = new TimeSpan(hr, min, 0);
                    }
                }
                
            }
        }

        public class StoreProduct
        {
            public int storeID;
            public int storeOrder;
            public int itemID;
            public int itemCount;
            public int totalPrice;
            public int heroID;
            public string shelveNumber;
            public int probability;
            public int levelreq;
            public List<int> shelveNumberList;
            public StoreProduct(ProductSettingJson json)
            {
                storeID = json.id;
                storeOrder = json.storeorder;
                itemID = json.itemid;
                itemCount = json.itemcount;
                totalPrice = json.totalprice;
                heroID = json.heroid;
                shelveNumber = json.shelvesnumber;
                probability = json.probability;
                levelreq = json.levelreq;

                //Generate all valid shelve number
                shelveNumberList = new List<int>();
                List<string> shelfNoStrList = shelveNumber.Split(';').ToList();
                int x = 0;
                for (int i = 0; i < shelfNoStrList.Count; ++i)
                {
                    if (int.TryParse(shelfNoStrList[i], out x))
                        shelveNumberList.Add(x);
                }
            }
            public bool isValidShelve(int shelve)
            {
                return shelveNumberList.Contains(shelve);
            }
        }

        public struct ShortStoreProduct
        {
            public int storeID;
            public int probability;

            public ShortStoreProduct(int _storeID, int _probability)
            {
                storeID = _storeID;
                probability = _probability;
            }
        }

        //mStoreSetting[cat]
        public static Dictionary<int, StoreSetting> mStoreSetting;

        //dic_Products[shopid] = product
        public static Dictionary<int, StoreProduct> dic_Products;

        //dic_RefreshCost[cat] = list of price
        public static Dictionary<int, List<int>> dic_RefreshCost;

        //dic_storeProducts[cat] = list<StoreProduct>
        public static Dictionary<int, List<StoreProduct>> dic_storeProducts; //change to int if ex.

        //dic_shortStoreProducts[cat][shelfNum] = short product
        public static Dictionary<int, Dictionary<int, List<ShortStoreProduct>>> dic_shortStoreProducts;

        //dic_HeroIDToStoreSetting[heroid] = storeorder
        public static Dictionary<int, int> dic_HeroIDToStoreSetting;

        //dic_ShopTypeToStoreSetting[shoptype] = storeorder
        public static Dictionary<UIStoreLinkType, int> dic_ShopTypeToStoreSetting;

        public static void Init(GameDBRepo gameData)
        {
            mStoreSetting = new Dictionary<int, StoreSetting>();
            dic_Products = new Dictionary<int, StoreProduct>();
            dic_RefreshCost = new Dictionary<int, List<int>>();
            dic_HeroIDToStoreSetting = new Dictionary<int, int>();
            dic_ShopTypeToStoreSetting = new Dictionary<UIStoreLinkType, int>();
            
            foreach (var setting in gameData.StoreSet)
            {
                mStoreSetting.Add(setting.Value.storeorder, new StoreSetting(setting.Value));

                if (setting.Value.heroid > 0)
                    dic_HeroIDToStoreSetting.Add(setting.Value.heroid, setting.Value.storeorder);

                if (setting.Value.shoptype != UIStoreLinkType.NotApplicable)
                    dic_ShopTypeToStoreSetting.Add(setting.Value.shoptype, setting.Value.storeorder);
            }
            foreach (var product in gameData.ProductSetting)
            {
                var storeproduct = new StoreProduct(product.Value);
                dic_Products.Add(storeproduct.storeID, storeproduct);
                AddToDicData(storeproduct);
            }
            foreach (var refresh in gameData.StoreRefresh)
            {
                if (!dic_RefreshCost.ContainsKey(refresh.Value.storeorder))
                    dic_RefreshCost.Add(refresh.Value.storeorder, new List<int>());

                dic_RefreshCost[refresh.Value.storeorder].Add(refresh.Value.refreshprice);
            }
        }

        static void AddToDicData(StoreProduct product)
        {
            //dic_products[cat][shelfNum] = product
            var cat = product.storeOrder;
            List<int> shelveNoList = product.shelveNumberList;

            //dic_storeProducts[cat] = list<StoreProduct>
            if (dic_storeProducts == null)
            {
                dic_storeProducts = new Dictionary<int, List<StoreProduct>>();
            }
            if (dic_storeProducts.ContainsKey(cat) == false)
            {
                dic_storeProducts.Add(cat, new List<StoreProduct>());
            }
            if (dic_storeProducts[cat].Contains(product) == false)
            {
                dic_storeProducts[cat].Add(product);
            }

            //dic_shortStoreProducts[cat][shelfNum] = short product
            if (dic_shortStoreProducts == null)
            {
                dic_shortStoreProducts = new Dictionary<int, Dictionary<int, List<ShortStoreProduct>>>();
            }
            if (dic_shortStoreProducts.ContainsKey(cat) == false)
            {
                dic_shortStoreProducts.Add(cat, new Dictionary<int, List<ShortStoreProduct>>());
            }
            
            //Loop to add shelve list and short product
            for (int i = 0; i < shelveNoList.Count; ++i)
            {
                if (dic_shortStoreProducts[cat].ContainsKey(shelveNoList[i]) == false)
                {
                    dic_shortStoreProducts[cat].Add(shelveNoList[i], new List<ShortStoreProduct>());
                }

                dic_shortStoreProducts[cat][shelveNoList[i]].Add(new ShortStoreProduct(product.storeID, 
                                                                                        product.probability));
            }
        }

        public static StoreProduct GetProduct(int storeID)
        {
            if (dic_Products.ContainsKey(storeID))
                return dic_Products[storeID];
            return null;
        }

        public static StoreSetting GetStoreSetting(int category)
        {
            if (mStoreSetting.ContainsKey(category))
                return mStoreSetting[category];
            return null;
        }

        public static DateTime GetNextRefresh(int category)
        {
            var setting = GetStoreSetting(category);
            if (setting != null)
            {
                DateTime rf1 = DateTime.Today.Date + setting.RefreshTime1;//3
                DateTime rf2 = DateTime.Today.Date + setting.RefreshTime2;//6
                DateTime rf3 = DateTime.Today.Date + setting.RefreshTime3;//9

                //4 case
                if (DateTime.Now > rf3)
                {
                    return DateTime.Today.Date + TimeSpan.FromDays(1) + setting.RefreshTime1;
                }
                else if (DateTime.Now > rf2)
                {
                    return rf3;
                }
                else if (DateTime.Now > rf1)
                {
                    return rf2;
                }
                else
                {
                    return rf1;
                }

            }
            else
                return DateTime.MaxValue;
        }

        public static bool shouldRefresh(int category, DateTime prevtime, DateTime nowtime)
        {
            var setting = GetStoreSetting(category);
            if (setting == null)
                return false;

            DateTime rf1 = DateTime.Today.Date + setting.RefreshTime1;//3
            DateTime rf2 = DateTime.Today.Date + setting.RefreshTime2;//6
            DateTime rf3 = DateTime.Today.Date + setting.RefreshTime3;//9

            //If reset time is later than previous interval time
            //If reset time is earlier than or equals to right now
            if (DateTime.Compare(rf1, prevtime) >= 0 && DateTime.Compare(rf1, nowtime) <= 0)
                return true;
            else if (DateTime.Compare(rf2, prevtime) >= 0 && DateTime.Compare(rf2, nowtime) <= 0)
                return true;
            else if (DateTime.Compare(rf3, prevtime) >= 0 && DateTime.Compare(rf3, nowtime) <= 0)
                return true;

            return false;
        }

        public static CurrencyType GetCurrencyType(int storeID)
        {
            var product = GetProduct(storeID);
            if (product != null)
            {
                var storeSet = GetStoreSetting(product.storeOrder);
                if (storeSet != null)
                    return storeSet.CurrencyType;//gold and bind gold special case!
            }
            return CurrencyType.None;
        }

        public static int GetRefreshPrice(int category, int time)
        {
            if (dic_RefreshCost.ContainsKey(category))
            {
                var list_price = dic_RefreshCost[category];
                if (time < list_price.Count && time >= 0)
                    return list_price[time];
                else
                    return list_price.Last();
            }
            return int.MaxValue;
        }

        public static int GetStoreOrder(int heroID)
        {
            if (dic_HeroIDToStoreSetting.ContainsKey(heroID))
            {
                return dic_HeroIDToStoreSetting[heroID];
            }

            return -1;
        }

        public static int GetStoreOrder(UIStoreLinkType slt)
        {
            if (dic_ShopTypeToStoreSetting.ContainsKey(slt))
            {
                return dic_ShopTypeToStoreSetting[slt];
            }

            return -1;
        }

        public static List<ShortStoreProduct> GetProductOnShelve(int cat, int shelveNo)
        {
            if (!dic_shortStoreProducts.ContainsKey(cat) || !dic_shortStoreProducts[cat].ContainsKey(shelveNo))
                return new List<ShortStoreProduct>();

            return new List<ShortStoreProduct>( dic_shortStoreProducts[cat][shelveNo] );
        }
    }
}
