using Kopio.JsonContracts;
using Newtonsoft.Json;
using Photon.LoadBalancing.GameServer.Mail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.Repository;
using Zealot.Server.Entities;
using Zealot.Server.Rules;


namespace Photon.LoadBalancing.GameServer.ItemMall
{
    public class ItemMallManager
    {
        private static volatile ItemMallManager instance;
        private static object syncRoot = new object();

        private JsonSerializerSettings jsonSetting;

        Dictionary<int, ItemMall_Item> mItemMap_gm;

        public Dictionary<ItemMallCategory, List<string>> ItemMall_ItemInfo_String = new Dictionary<ItemMallCategory, List<string>>();
        const int MAX_SIZE = 100;

        public List<bool> IsShopOpen = new List<bool>(); //IsShopOpen[ItemMallCategory] = isOpen
        public List<DateTime> ShopLastUpdated = new List<DateTime>();

        private ItemMallManager()
        {
            jsonSetting = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            IsShopOpen = new List<bool>();
            ShopLastUpdated = new List<DateTime>();
            for (int i = 0; i <= (int)ItemMallCategory.WeaponFashion; ++i)
            {
                IsShopOpen.Add(true);
                ShopLastUpdated.Add(DateTime.MinValue);
                ItemMall_ItemInfo_String.Add((ItemMallCategory)i, new List<string>());
            }
        }

        public async Task ServerInit_ItemMall()
        {
            mItemMap_gm = new Dictionary<int, ItemMall_Item>();

            mItemMap_gm.Clear();

            await UpdateGmToolsData();

            var listStatus = await PrepareShopStatus();
            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                IsShopOpen = listStatus.Item1;
                ShopLastUpdated = listStatus.Item2;
            });
        }

        private async Task<List<ItemMall_Item>> PrepareItem(int category)
        {
            List<ItemMall_Item> retValue = new List<ItemMall_Item>();

            List<Dictionary<string, object>> sv_lstItemJson = await GameApplication.dbGM.ItemMall.GetItemMallItem(category);

            foreach (var item in sv_lstItemJson)
            {
                ItemMall_Item newItem = new ItemMall_Item(item);
                retValue.Add(newItem);
            }
            return retValue;
        }

        private async Task<Tuple<List<bool>, List<DateTime>>> PrepareShopStatus()
        {
            var shopOpen = new List<bool>();
            var shopLastUpdated = new List<DateTime>();
            for (int i = 0; i <= (int)ItemMallCategory.WeaponFashion; ++i)
            {
                shopOpen.Add(true);//default all true
                shopLastUpdated.Add(DateTime.MinValue);
            }

            List<Dictionary<string, object>> sv_lstShopStatus = await GameApplication.dbGM.ItemMall.GetShopStatus();

            foreach (var item in sv_lstShopStatus)
            {
                int cat = (int)item["ItemCategory"];
                shopOpen[cat] = (bool)item["IsOpen"];

                var dt = new DateTime((long)item["LastUpdated"]);
                shopLastUpdated[cat] = dt;
            }
            return new Tuple<List<bool>, List<DateTime>>(shopOpen, shopLastUpdated);
        }

        private void SetJsonSettingForDB()
        {
            jsonSetting.Converters.Clear();

            jsonSetting.Converters.Add(new DBInventoryItemConverter());
        }

        private void SetJsonSettingForClient()
        {
            jsonSetting.Converters.Clear();

            jsonSetting.Converters.Add(new ClientInventoryItemConverter());
        }

        private bool HasExceededLimit(GameClientPeer playerPeer, int id, bool isGM, int stackToPurchase)
        {
            CharacterData charData = playerPeer.CharacterData;
            ItemMallInventoryData itemMallInvData = charData.ItemMallInventory;
            List<ItemMallLimitEntry> lstItemMallData = itemMallInvData.lstItemMallData;

            ItemMallLimitEntry entry = lstItemMallData.Find(x => x.id == (ushort)id && x.isGM == isGM);

            if (entry != null)
            {
                var shopItem = GetItem(id, isGM);
                if (shopItem == null)
                {
                    return false;//item not found
                }

                var limitCount = shopItem.limitcount;//-1 = no limit
                if (limitCount != -1 && limitCount < entry.stackPurchased + stackToPurchase)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetItemPurchaseLimit(GameClientPeer playerPeer, int shopItemId, int dailyLimit)
        {
            if (dailyLimit == 0)
            {
                return int.MaxValue;
            }

            CharacterData charData = playerPeer.CharacterData;
            ItemMallInventoryData itemMallInvData = charData.ItemMallInventory;
            List<ItemMallLimitEntry> lstItemMallData = itemMallInvData.lstItemMallData;

            ItemMallLimitEntry entry = lstItemMallData.Find(x => x.id == (ushort)shopItemId);


            if (entry == null)
            {
                return dailyLimit;
            }
            else
            {
                int purchaseLimit = dailyLimit - entry.stackPurchased;

                return Math.Max(0, purchaseLimit);
            }
        }

        private void UpdateDailyLimit(GameClientPeer playerPeer, byte category, int id, int stackToBuy, bool isGM)
        {
            CharacterData charData = playerPeer.CharacterData;
            ItemMallInventoryData itemMallInvData = charData.ItemMallInventory;
            List<ItemMallLimitEntry> lstItemMallData = itemMallInvData.lstItemMallData;

            ItemMallLimitEntry entry = lstItemMallData.Find(x => x.id == (ushort)id && x.isGM == isGM);
            var shopItem = GetItem(id, isGM);
            if (entry == null && shopItem != null && shopItem.limitcount != -1)
            {
                ItemMallLimitEntry newEntry = new ItemMallLimitEntry();
                newEntry.id = id;
                newEntry.stackPurchased = stackToBuy;
                newEntry.isGM = isGM;
                itemMallInvData.lstItemMallData.Add(newEntry);
            }
            else
            {
                if (entry != null && entry.stackPurchased != -1)
                    entry.stackPurchased += stackToBuy;
            }
        }

        private void SendItemToMail(string charName, IInventoryItem item)
        {
            MailObject mailObj = new MailObject();
            mailObj.rcvName = charName;
            //mailObj.mailName = "Mail Test";
            mailObj.mailName = "Item Mall Mail";

            mailObj.lstAttachment.Add(item);

            MailManager.Instance.SendMail(mailObj);
        }

        private bool IsWithinPurchaseTime(DateTime begin, DateTime end)
        {
            if (!((DateTime.Now < begin) || (DateTime.Now > end)))
            {
                return false;
            }
            return true;
        }

        private bool IsCategoryShopOpen(ItemMallCategory cat)
        {
            return IsShopOpen[(int)cat];
        }

        private void EFLog_Purchase(GameClientPeer playerPeer,
                ItemMallCategory category, IInventoryItem iInvenItem, int totalCost,
                int goldBefore,int bindGoldBefore , int quantityBefore, int quantityAfter)
        {
            string playerName = playerPeer.mPlayer.Name;

            string categoryString = Enum.GetName(typeof(ItemMallCategory), category);
            
            int itemId = iInvenItem.ItemID;
            int quantityPurchased = iInvenItem.StackCount;
            int goldAfter = playerPeer.mPlayer.SecondaryStats.Gold;
            int bindGoldAfter = playerPeer.mPlayer.SecondaryStats.bindgold;
            
            string message = string.Format(@"itemCategory = {0} |itemId = {1} | quantityPurchased = {2} | totalCost = {3}",
                categoryString,
                itemId,
                quantityPurchased,
                totalCost);

            Zealot.Logging.Client.LogClasses.ItemMall itemMallLog = new Zealot.Logging.Client.LogClasses.ItemMall();
            itemMallLog.userId = playerPeer.mUserId;
            itemMallLog.charId = playerPeer.GetCharId();
            itemMallLog.message = message;

            itemMallLog.itemCategory = categoryString;
            itemMallLog.itemId = itemId;
            itemMallLog.quantityBefore = quantityBefore;
            itemMallLog.quantityPurchased = quantityPurchased;
            itemMallLog.quantityAfter = quantityAfter;
            itemMallLog.goldBefore = goldBefore;
            itemMallLog.goldAfter = goldAfter;
            itemMallLog.bindGoldBefore = bindGoldBefore;
            itemMallLog.bindGoldAfter = bindGoldAfter;
            itemMallLog.totalCost = totalCost;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(itemMallLog);
        }

        public static ItemMallManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new ItemMallManager();
                        }
                    }
                }

                return instance;
            }
        }

        public int PurchaseItem(GameClientPeer playerPeer, int id, bool isGm, int stackToBuy, long ticks)
        {
            
            // 1. Check daily limit
            // 2. Check is Data Sync
            // 3. Check VIP Requirement
            // 4. Check total cost
            // 5. Add items to inven
            // 6. Update daily limit
            // 7. Save
            Player player = playerPeer.mPlayer;
            bool hasExceededLimit = HasExceededLimit(playerPeer, id, isGm, stackToBuy);
            if (hasExceededLimit)
            {
                return (int)ItemMallReturnCode.PurchaseItem_Fail_PurchaseLimitExceed;
            }
            else//hasExceededLimit == false
            {
                int cost;
                int itemId;
                int stackCount;

                bool isDeductSuccessful = false;

                ItemMall_Item mallItem = GetItem(id, isGm);
                if (mallItem == null)
                {
                    return (int)ItemMallReturnCode.PurchaseItem_Fail_ShopNotSync;
                }

                itemId = mallItem.itemid;
                stackCount = mallItem.stackcount;

                var dt = new DateTime(ticks);
                if (IsCategoryShopOpen(mallItem.category) == false)
                {
                    return (int)ItemMallReturnCode.PurchaseItem_Fail_ShopNotOpen;
                }
                if (dt < ShopLastUpdated[(int)mallItem.category])
                {
                    return (int)ItemMallReturnCode.PurchaseItem_Fail_ShopNotSync;
                }
                if (IsWithinPurchaseTime(mallItem.beginDate, mallItem.endDate) == true)
                {
                    return (int)ItemMallReturnCode.PurchaseItem_Fail_InvalidDateTime;
                }
                if (CanPurchaseVIP(mallItem, player.PlayerSynStats.vipLvl) == false)
                {
                    return (int)ItemMallReturnCode.PurchaseItem_Fail_VIPLevel;
                }


                cost = mallItem.price * stackToBuy;

                if (mallItem.shoppingtype == ItemMallShoppingType.Bind)
                    isDeductSuccessful = player.IsCurrencySufficient(CurrencyType.Gold, cost, true);//use bind gold
                else
                    isDeductSuccessful = player.IsCurrencySufficient(CurrencyType.Gold, cost, false);//use gold

                if (isDeductSuccessful == false)
                {
                    return (int)ItemMallReturnCode.PurchaseItem_Fail_InsufficientCurrency;
                }

                int actualAmtToBuy = stackCount * stackToBuy;
                IInventoryItem item = GameRules.GenerateItem(itemId, null, actualAmtToBuy, true);
                int beforeStackcount = playerPeer.mInventory.GetItemStackCountByItemId((ushort)itemId);
                InvRetval invRetVal = playerPeer.mInventory.AddItemsIntoInventory((ushort)itemId, actualAmtToBuy, true, "ItemMall");
                int afterStackcount = playerPeer.mInventory.GetItemStackCountByItemId((ushort)itemId);
                switch (invRetVal.retCode)
                {
                    case InvReturnCode.AddSuccess:
                        int goldBefore = player.SecondaryStats.Gold;
                        int bindGoldBefore = player.SecondaryStats.bindgold;
                        if (mallItem.shoppingtype == ItemMallShoppingType.Bind)
                            player.DeductGold(cost, true, true, "ItemMall_Buy");//use bind gold
                        else
                            player.DeductGold(cost, false, true, "ItemMall_Buy");//use gold

                        EFLog_Purchase(playerPeer, mallItem.category, item, cost,goldBefore,bindGoldBefore,beforeStackcount,afterStackcount);

                        UpdateDailyLimit(playerPeer, (byte)mallItem.category, id, stackToBuy, isGm);

                        return (int)ItemMallReturnCode.PurchaseItem_Success;

                    case InvReturnCode.Full:
                        return (int)ItemMallReturnCode.PurchaseItem_Fail_BagFull;
                    //EFLog_Purchase(playerPeer, mallItem.category, item, cost);
                    //UpdateDailyLimit(playerPeer, (byte)mallItem.category, id, stackToBuy);
                    //SendItemToMail(playerPeer.mChar, item);
                    //return (int)ItemMallReturnCode.PurchaseItem_Success_SentToMail;

                    case InvReturnCode.AddFailed:
                    default:
                        return (int)ItemMallReturnCode.PurchaseItem_Fail_UnknownInventoryReturnCode;
                }

                //playerPeer.SaveCharacter();
            }
        }

        public async Task RefreshShopStatus()
        {
            var listStatus = await PrepareShopStatus();
            GameApplication.Instance.executionFiber.Enqueue(async () =>
            {
                List<int> needUpdate = new List<int>();

                for (int i = 0; i <= (int)ItemMallCategory.WeaponFashion; ++i)
                {
                    if (listStatus.Item1[i] == true && IsShopOpen[i] == false)//if is newly open
                    {
                        needUpdate.Add(i);
                    }
                }
                IsShopOpen = listStatus.Item1;
                ShopLastUpdated = listStatus.Item2;

                if (needUpdate.Count > 0)
                {
                    foreach (var reqUpdate in needUpdate)
                        await UpdateGmToolsData(reqUpdate);
                }
            });
        }

        private ItemMall_PurchaseLimit GeneratePurchaseLimit(GameClientPeer playerPeer, bool isGMTools)
        {

            ItemMall_PurchaseLimit retValue = new ItemMall_PurchaseLimit();

            retValue.isGMTools = isGMTools;

            retValue.dicLimit = new Dictionary<int, int>();

            Dictionary<int, ItemMall_Item> dic;
            if (isGMTools)
                dic = mItemMap_gm;
            else
                dic = ItemMallRepo.mItemMap;

            playerPeer.CharacterData.ItemMallInventory.lstItemMallData.RemoveAll(item => item.isGM == isGMTools && (GetItem(item.id, isGMTools) == null || GetItem(item.id, isGMTools).endDate < DateTime.Today));

            foreach (var item in dic.Values)
            {
                int limit = GetItemPurchaseLimit(playerPeer, item.shopItemID, item.limitcount);
                retValue.dicLimit.Add(item.shopItemID, limit);
            }

            return retValue;
        }

        //bool RemoveOutdatedEntry(ItemMallLimitEntry entry,bool isGMTools)
        //{
        //    if (isGMTools == entry.isGM)
        //    {

        //    }
        //}

        public string GetPurchaseLimit(string charName, bool isGMTools)
        {
            GameClientPeer playerPeer = GameApplication.Instance.GetCharPeer(charName);

            if (playerPeer == null)
            {
                return null;
            }

            ItemMall_PurchaseLimit retValue = GeneratePurchaseLimit(playerPeer, isGMTools);

            SetJsonSettingForClient();
            string serializedItemMallString = JsonConvert.SerializeObject(retValue, Formatting.None, jsonSetting);
            return serializedItemMallString;
        }

        public string ItemMallInit_Client_MallData(GameClientPeer playerPeer)
        {
            ItemMall_InitMall retValue = new ItemMall_InitMall();

            retValue.purchaseLimit_GM = new ItemMall_PurchaseLimit();
            retValue.purchaseLimit_Kopio = new ItemMall_PurchaseLimit();

            retValue.purchaseLimit_GM = GeneratePurchaseLimit(playerPeer, true);
            retValue.purchaseLimit_Kopio = GeneratePurchaseLimit(playerPeer, false);

            var invMall = playerPeer.CharacterData.ItemMallInventory;
            retValue._endtime_tortoise = invMall.itemMallTreasureTiming.endtime_tortoise;
            retValue._endtime_tiger = invMall.itemMallTreasureTiming.endtime_tiger;
            retValue._endtime_dragon = invMall.itemMallTreasureTiming.endtime_dragon;
            retValue._endtime_phoenix = invMall.itemMallTreasureTiming.endtime_phoenix;

            retValue.isShopOpen = IsShopOpen;

            SetJsonSettingForClient();
            string serializedItemMallString = JsonConvert.SerializeObject(retValue, Formatting.None, jsonSetting);
            return serializedItemMallString;
        }

        public List<string> ItemMallInit_Client_ItemData(int category, long lastGrab)
        {
            if (IsShopOpen[category] && lastGrab < ShopLastUpdated[category].Ticks)
                return ItemMall_ItemInfo_String[(ItemMallCategory)category];

            return null;
        }

        public async Task UpdateGmToolsData(int category = 0)
        {
            //init item list
            Dictionary<ItemMallCategory, List<ItemMall_Item>> dic_ItemList = new Dictionary<ItemMallCategory, List<ItemMall_Item>>();
            if (category == 0)
            {
                for (ItemMallCategory i = 0; i <= ItemMallCategory.WeaponFashion; ++i)
                {
                    dic_ItemList.Add(i, new List<ItemMall_Item>());
                }
            }
            else
            {
                dic_ItemList.Add((ItemMallCategory)category, new List<ItemMall_Item>());
            }

            //Update server data
            var lst_Item = await PrepareItem(category);

            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                foreach (var item in lst_Item)
                {
                    ItemMall_Item insertItem = new ItemMall_Item();

                    insertItem.shopItemID = item.shopItemID;//the id is already added with
                    insertItem.name = item.name;
                    insertItem.itemid = item.itemid;
                    insertItem.price = item.price;
                    insertItem.stackcount = item.stackcount;
                    insertItem.limitcount = item.limitcount;
                    insertItem.category = item.category;
                    insertItem.shoppingtype = item.shoppingtype;
                    insertItem.viplevel = item.viplevel;
                    insertItem.beginDate = item.beginDate;
                    insertItem.endDate = item.endDate;
                    insertItem.sortnumber = item.sortnumber;
                    insertItem.online = item.online;
                    insertItem.showTime = item.showTime;
                    insertItem.showLimited = item.showLimited;
                    insertItem.showVIP = item.showVIP;
                    insertItem.shoppingjob = item.shoppingjob;
                    insertItem.isGM = true;

                    //add or edit
                    mItemMap_gm[insertItem.shopItemID] = insertItem;

                    dic_ItemList[item.category].Add(insertItem);
                }

                foreach (var catItemList in dic_ItemList)
                {
                    ItemMall_ItemInfo_String[catItemList.Key].Clear();
                    var list = catItemList.Value;
                    int start = 0;
                    do
                    {
                        ItemMall_InitItemInfo ret = new ItemMall_InitItemInfo();
                        int end = start + MAX_SIZE > list.Count ? list.Count : start + MAX_SIZE;
                        ret.gmItems = list.GetRange(start, end - start);
                        string res = JsonConvert.SerializeObject(ret, Formatting.None, jsonSetting);
                        ItemMall_ItemInfo_String[catItemList.Key].Add(res);
                        start += MAX_SIZE;
                    } while (start < list.Count);
                }

                dic_ItemList.Clear();
            }
            );
        }

        public void UpdateTiming_Charged(GameClientPeer player, int amt)
        {
            CharacterData charData = player.CharacterData;
            ItemMallInventoryData itemMallInvData = charData.ItemMallInventory;
            DateTime newEndTime = DateTime.Now + TimeSpan.FromDays(2);

            if (ItemMallRepo.IsTreasureUnlock(ItemMallShoppingType.Tortoise, amt))
                itemMallInvData.itemMallTreasureTiming.SetNewEndTime(ItemMallShoppingType.Tortoise, newEndTime);

            if (ItemMallRepo.IsTreasureUnlock(ItemMallShoppingType.Phoenix, amt))
                itemMallInvData.itemMallTreasureTiming.SetNewEndTime(ItemMallShoppingType.Phoenix, newEndTime);

            if (ItemMallRepo.IsTreasureUnlock(ItemMallShoppingType.Tiger, amt))
                itemMallInvData.itemMallTreasureTiming.SetNewEndTime(ItemMallShoppingType.Tiger, newEndTime);

            if (ItemMallRepo.IsTreasureUnlock(ItemMallShoppingType.Dragon, amt))
                itemMallInvData.itemMallTreasureTiming.SetNewEndTime(ItemMallShoppingType.Dragon, newEndTime);
        }

        bool CanPurchaseVIP(ItemMall_Item item, int playerVIP)
        {
            return item.viplevel == -1 || item.viplevel <= playerVIP;
        }

        private ItemMall_Item GetItem(int shopID, bool isGM)
        {
            if (isGM)//gm
            {
                if (mItemMap_gm.ContainsKey(shopID))
                    return mItemMap_gm[shopID];
                else
                    return null;
            }
            else
            {
                return ItemMallRepo.GetItemByID(shopID);
            }
        }
    }
}
