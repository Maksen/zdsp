using Photon.LoadBalancing.GameServer;
using Photon.LoadBalancing.GameServer.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Repository;

namespace Zealot.Server.Rules
{
    public static class StoreRules
    {
        public static void StoreInit(GameClientPeer peer)
        {
            foreach (var setting in StoreRepo.mStoreSetting)
            {
                int cat = setting.Value.category;
                if (isCatAvailable(cat, peer))
                    InitCategory(peer, cat);
                else
                    peer.CharacterData.StoreInventory.list_store[cat] = null;
                //else
               //     peer.mPlayer.StoreSynStats.list_store[cat] = null; //remove from character data if hero no longer exist
            }
        }

        static void InitCategory(GameClientPeer peer, int cat)
        {
            if (peer == null || cat < 0 || cat >= peer.CharacterData.StoreInventory.list_store.Count)
                return;

            StoreCategory sc = peer.CharacterData.StoreInventory.list_store[cat];
            DateTime nextRef;
            if (sc != null)
            {
                nextRef = StoreRepo.GetNextRefresh(sc.category);
                sc.nextRefresh = (sc.nextRefresh >= nextRef) ? nextRef : sc.nextRefresh;
            }

            bool needRefresh = (sc == null || sc.nextRefresh <= DateTime.Now);
            //bool needRefresh = true; //Uncomment to refresh on every open page
            if (needRefresh)
            {
                StoreCategory store = new StoreCategory();
                store.category = cat;
                var list = GetItemList(store.category, peer);
                store.list_storeitem = new List<Store_Item>();
                foreach (var shopid in list)
                {
                    Store_Item item = new Store_Item(shopid, 0);
                    store.list_storeitem.Add(item);
                }
                store.nextRefresh = StoreRepo.GetNextRefresh(cat);
                store.refreshCount = 0;
                peer.CharacterData.StoreInventory.list_store[cat] = store;
            }
        }

        /// <summary>
        /// Meant for adding/deleting items to store category's inventory upon prereq value changes
        /// Also do a full refresh if needed
        /// </summary>
        /// <param name="cat"></param>
        /// <param name="peer"></param>
        public static void UpdateRefreshCategoryFree(int cat, GameClientPeer peer)
        {
            if (peer == null || cat < 0 || cat >= peer.CharacterData.StoreInventory.list_store.Count)
                return;

            //If store category doesnt exist, make one
            //Do a full refresh if necessary, if not.. just add/remove store category's inventory
            StoreCategory sc = peer.CharacterData.StoreInventory.list_store[cat];
            bool needCreation = (sc == null);
            bool needRefresh = (sc != null && sc.nextRefresh <= DateTime.Now);
            if (needCreation)
                InitCategory(peer, cat);
            else if (needRefresh)
                Refresh(cat, peer, true);
            else
                Update(cat, peer);
        }

        #region Update
        static void Update(int cat, GameClientPeer peer)
        {
            if (peer == null || cat < 0 || cat >= peer.CharacterData.StoreInventory.list_store.Count)
                return;

            UpdateItemList(cat, peer);
        }
        #endregion

        #region refresh
        public static void RefreshCategory(int cat, GameClientPeer peer)
        {
            int count = GetStoreCategoryTime(cat, peer);
            int cost = StoreRepo.GetRefreshPrice(cat, count);
            var isDeductSuccessful = peer.mPlayer.DeductCurrency(CurrencyType.Gold, cost, true, "Store_Refresh");
            if (isDeductSuccessful == true)
                Refresh(cat, peer);
        }

        static void Refresh(int cat, GameClientPeer peer, bool freeRefresh = false)
        {
            if (peer == null || cat < 0 || cat >= peer.CharacterData.StoreInventory.list_store.Count)
                return;

            //var store = JsonConvert.DeserializeObject<StoreCategory>((string)peer.mPlayer.StoreSynStats.list_store[cat]);
            StoreCategory store = peer.CharacterData.StoreInventory.list_store[cat];
            var list = GetItemList(cat, peer);
            store.list_storeitem = new List<Store_Item>();
            foreach (var shopid in list)
            {
                Store_Item item = new Store_Item(shopid, 0);
                store.list_storeitem.Add(item);
            }

            if (!freeRefresh)
            {
                ++store.refreshCount;
                //Log
                Log_Refresh(store.category, store.refreshCount, peer);
            }
                
            //peer.mPlayer.StoreSynStats.list_store[cat] = JsonConvert.SerializeObject(store);
            peer.CharacterData.StoreInventory.list_store[cat] = store; 
        }
        #endregion

        #region New store inventory
        static List<int> GetItemList(int cat, GameClientPeer peer)
        {
            var shopSetting = StoreRepo.GetStoreSetting(cat);
            if (shopSetting == null)
                return new List<int>();

            switch (shopSetting.shopType)
            {
                case UIStoreLinkType.MoneyStore:
                    return GetItemList_Money(cat, peer);
                case UIStoreLinkType.GuildStore:
                    return GetItemList_Guild(cat, peer);
                default:
                    return GetItemList_Normal(cat, peer);
            }
        }

        private static List<int> GetItemList_Normal(int cat, GameClientPeer peer)
        {
            List<int> ret_List = new List<int>();
            int shelveCount = Math.Min(StoreRepo.mStoreSetting[cat].CommodityCount, StoreRepo.dic_shortStoreProducts[cat].Count);
            //Loop all shelves
            for (int i = 1; i <= shelveCount; ++i)
            {
                List<StoreRepo.ShortStoreProduct> shelf = StoreRepo.GetProductOnShelve(cat, i);
                shelf.RemoveAll(item => isItemAvailable(item.storeID, peer) == false);

                //Roll items from shelf list
                if (shelf.Count > 1)
                {
                    int total = 0;
                    foreach (var storeItem in shelf)
                    {
                        total += storeItem.probability;
                    }
                    var random = GameUtils.GetRandomGenerator();
                    int rand = random.Next(total);

                    total = 0;//reset
                    foreach (var storeItem in shelf)
                    {
                        total += storeItem.probability;
                        if (total >= rand)
                        {
                            ret_List.Add(storeItem.storeID);
                            break;
                        }
                    }
                }
                else if (shelf.Count == 1)
                {
                    ret_List.Add(shelf[0].storeID);
                }
            }

            return ret_List;
        }

        private static List<int> GetItemList_Guild(int cat, GameClientPeer peer)
        {
            List<int> ret_List = new List<int>();
            var datalist = StoreRepo.dic_shortStoreProducts[cat];
            
            //Guild related
            var guild = GuildRules.GetGuildById(peer.mPlayer.SecondaryStats.guildId);
            if (guild == null)
                return ret_List;

            int guildShelfLimit = (int)guild.GetGuildTechStats(GuildTechType.Shop); //return 0 if cannot find stats
            int maxCount = (datalist.Count < guildShelfLimit) ? datalist.Count : guildShelfLimit;

            for (int i = 1; i <= maxCount; ++i)
            {
                List<StoreRepo.ShortStoreProduct> shelf = StoreRepo.GetProductOnShelve(cat, i);
                shelf.RemoveAll(item => isItemAvailable(item.storeID, peer) == false);
                if (shelf.Count > 1)
                {
                    int total = 0;
                    foreach (var storeItem in shelf)
                    {
                        total += storeItem.probability;
                    }
                    var random = GameUtils.GetRandomGenerator();
                    int rand = random.Next(total);

                    total = 0;//reset
                    foreach (var storeItem in shelf)
                    {
                        total += storeItem.probability;
                        if (total >= rand)
                        {
                            ret_List.Add(storeItem.storeID);
                            break;
                        }
                    }
                }
                else if (shelf.Count == 1)
                {
                    ret_List.Add(shelf[0].storeID);
                }
                //else do nth
            }
            return ret_List;
        }

        private static List<int> GetItemList_Money(int cat, GameClientPeer peer)
        {
            List<int> ret_List = new List<int>();
            var datalist = StoreRepo.dic_shortStoreProducts[cat];

            int vipLevel = 0;
            int vipShelfLimit = 5; //VIPRepo.GetVIPPrivilege("Store", vipLevel);
            int maxCount = (datalist.Count < vipShelfLimit) ? datalist.Count : vipShelfLimit;

            for (int i = 1; i <= maxCount; ++i)
            {
                List<StoreRepo.ShortStoreProduct> shelf = StoreRepo.GetProductOnShelve(cat, i);
                shelf.RemoveAll(item => isItemAvailable(item.storeID, peer) == false);
                if (shelf.Count > 1)
                {
                    int total = 0;
                    foreach (var storeItem in shelf)
                    {
                        total += storeItem.probability;
                    }
                    var random = GameUtils.GetRandomGenerator();
                    int rand = random.Next(total);

                    total = 0;//reset
                    foreach (var storeItem in shelf)
                    {
                        total += storeItem.probability;
                        if (total >= rand)
                        {
                            ret_List.Add(storeItem.storeID);
                            break;
                        }
                    }
                }
                else if (shelf.Count == 1)
                {
                    ret_List.Add(shelf[0].storeID);
                }
                //else do nth
            }
            return ret_List;
        }
        #endregion

        #region update store inventory
        static void UpdateItemList(int cat, GameClientPeer peer)
        {
            var shopSetting = StoreRepo.GetStoreSetting(cat);
            if (shopSetting == null)
                return;

            switch (shopSetting.shopType)
            {
                case UIStoreLinkType.MoneyStore:
                    UpdateItemList_Money(cat, peer);
                    break;
                case UIStoreLinkType.GuildStore:
                    UpdateItemList_Guild(cat, peer);
                    break;
                default:
                    return;
            }
        }

        private static void UpdateItemList_Money(int cat, GameClientPeer peer)
        {
            List<int> ret_List = new List<int>();
            var datalist = StoreRepo.dic_shortStoreProducts[cat];
            
            int vipLevel = 0;
            int vipShelfLimit = 5; // VIPRepo.GetVIPPrivilege("Store", vipLevel);
            int maxCount = (datalist.Count < vipShelfLimit) ? datalist.Count : vipShelfLimit;
            int curCount = peer.CharacterData.StoreInventory.list_store[cat].list_storeitem.Count;

            for (int i = curCount+1; i <= maxCount; ++i)
            {
                List<StoreRepo.ShortStoreProduct> shelf = StoreRepo.GetProductOnShelve(cat, i);
                shelf.RemoveAll(item => isItemAvailable(item.storeID, peer) == false);
                if (shelf.Count > 1)
                {
                    int total = 0;
                    foreach (var storeItem in shelf)
                    {
                        total += storeItem.probability;
                    }
                    var random = GameUtils.GetRandomGenerator();
                    int rand = random.Next(total);

                    total = 0;//reset
                    foreach (var storeItem in shelf)
                    {
                        total += storeItem.probability;
                        if (total >= rand)
                        {
                            ret_List.Add(storeItem.storeID);
                            break;
                        }
                    }
                }
                else if (shelf.Count == 1)
                {
                    ret_List.Add(shelf[0].storeID);
                }
                //else do nth
            }

            StoreCategory store = peer.CharacterData.StoreInventory.list_store[cat];
            //If appending
            if (vipShelfLimit > curCount)
            {
                foreach (var shopid in ret_List)
                {
                    Store_Item item = new Store_Item(shopid, 0);
                    store.list_storeitem.Add(item);
                }
            }
            //if removing
            else if (vipShelfLimit < curCount)
            {
                //Remove from back
                int lastIndex = store.list_storeitem.Count - 1;
                int amtToRemove = curCount - vipShelfLimit;
                int newLastIndex = lastIndex - amtToRemove;
                
                store.list_storeitem.RemoveRange(newLastIndex, amtToRemove);
            }

            peer.CharacterData.StoreInventory.list_store[cat] = store;
        }

        private static void UpdateItemList_Guild(int cat, GameClientPeer peer)
        {
            List<int> ret_List = new List<int>();
            var datalist = StoreRepo.dic_shortStoreProducts[cat];

            //if no guild, let store init handle deletion
            var guild = GuildRules.GetGuildById(peer.mPlayer.SecondaryStats.guildId);
            if (guild == null)
                return;

            int guildShelfLimit = (int)guild.GetGuildTechStats(GuildTechType.Shop); //return 0 if cannot find stats
            int maxCount = (datalist.Count < guildShelfLimit) ? datalist.Count : guildShelfLimit;
            int curCount = peer.CharacterData.StoreInventory.list_store[cat].list_storeitem.Count;

            for (int i = curCount+1; i <= maxCount; ++i)
            {
                List<StoreRepo.ShortStoreProduct> shelf = StoreRepo.GetProductOnShelve(cat, i);
                shelf.RemoveAll(item => isItemAvailable(item.storeID, peer) == false);
                if (shelf.Count > 1)
                {
                    int total = 0;
                    foreach (var storeItem in shelf)
                    {
                        total += storeItem.probability;
                    }
                    var random = GameUtils.GetRandomGenerator();
                    int rand = random.Next(total);

                    total = 0;//reset
                    foreach (var storeItem in shelf)
                    {
                        total += storeItem.probability;
                        if (total >= rand)
                        {
                            ret_List.Add(storeItem.storeID);
                            break;
                        }
                    }
                }
                else if (shelf.Count == 1)
                {
                    ret_List.Add(shelf[0].storeID);
                }
                //else do nth
            }

            StoreCategory store = peer.CharacterData.StoreInventory.list_store[cat];
            //If appending
            if (guildShelfLimit > curCount)
            {
                foreach (var shopid in ret_List)
                {
                    Store_Item item = new Store_Item(shopid, 0);
                    store.list_storeitem.Add(item);
                }
            }
            //if removing
            else if (guildShelfLimit < curCount)
            {
                //Remove from back
                int lastIndex = store.list_storeitem.Count - 1;
                int amtToRemove = curCount - guildShelfLimit;
                int newLastIndex = lastIndex - amtToRemove;

                store.list_storeitem.RemoveRange(newLastIndex, amtToRemove);
            }

            peer.CharacterData.StoreInventory.list_store[cat] = store;
        }
        #endregion

        #region isTrue
        static bool isItemAvailable(int storeID, GameClientPeer peer)
        {
            var player = peer.mPlayer;
            var product = StoreRepo.GetProduct(storeID);
            if (product == null)
                return false;

            bool haveHero = product.heroID <= 0;
            bool levelMet = product.levelreq == -1 || product.levelreq <= player.PlayerSynStats.Level;
            return haveHero && levelMet;
        }

        static bool isCatAvailable(int cat, GameClientPeer peer)
        {
            var player = peer.mPlayer;
            var setting = StoreRepo.GetStoreSetting(cat);
            if (setting == null)
                return false;

            int lvlreq = StoreRepo.mStoreSetting[cat].LvlReq;
            bool catAvailable = false;
            bool isEnoughLv = (peer.mPlayer.PlayerSynStats.Level >= lvlreq);
            switch (setting.shopType)
            {
                case UIStoreLinkType.MoneyStore:
                case UIStoreLinkType.Lottery:
                case UIStoreLinkType.WuLing:
                case UIStoreLinkType.WuMen:
                    catAvailable = isEnoughLv;
                    break;
                case UIStoreLinkType.GuildStore:
                    catAvailable = (player.SecondaryStats.guildId > 0 && isEnoughLv);
                    //catAvailable = true;
                    break;
                case UIStoreLinkType.NotApplicable:
                    //catAvailable = isEnoughLv && player.SecondaryStats.guildId > 0 && HeroesHouseRules.HeroesList.ContainsKey(setting.heroID) && HeroesHouseRules.HeroesList[setting.heroID].currentGuildId == player.SecondaryStats.guildId;
                    break;
            }
            return catAvailable;

            //bool haveHero = setting.heroID <= 0 || player.SecondaryStats.guildId > 0 && HeroesHouseRules.HeroesList.ContainsKey(setting.heroID) && HeroesHouseRules.HeroesList[setting.heroID].currentGuildId == player.SecondaryStats.guildId;
            //return haveHero;

            //return false;
        }

        static bool isItemInList(int category, int shelf, int StoreID, GameClientPeer peer)
        {
            if (peer == null || category < 0 || category >= peer.CharacterData.StoreInventory.list_store.Count ||
                shelf <= 0 || shelf > peer.CharacterData.StoreInventory.list_store[category].list_storeitem.Count)
                return false;

            StoreCategory store = peer.CharacterData.StoreInventory.list_store[category];
            return store.list_storeitem[shelf - 1].storeID == StoreID;

            //StoreCategory store = JsonConvert.DeserializeObject<StoreCategory>((string)peer.mPlayer.StoreSynStats.list_store[category]);
            //return store.list_storeitem[shelf-1].storeID == StoreID;
        }
        #endregion

        #region Purchase
        public static StoreReturnCode StorePurchase(int storeID, int stackToBuy, int shelveNo, out int oCurr1, out int oCurr2, GameClientPeer peer)
        {
            StoreReturnCode retcode;
            var product = StoreRepo.GetProduct(storeID);
            oCurr1 = oCurr2 = -1;

            bool isInList = isItemInList(product.storeOrder, shelveNo, storeID, peer);
            if (isInList == false)
                return StoreReturnCode.PurchaseItem_Fail_NotInList;

            var cType = StoreRepo.GetCurrencyType(storeID);
            bool allowBind = true;
            //if (cType == CurrencyType.LockGold)
            //    allowBind = true;
            int cost = product.totalPrice * stackToBuy;
            if (peer.mPlayer.IsCurrencySufficient(cType, cost, allowBind) == false)
                return StoreReturnCode.PurchaseItem_Fail_InsufficientCurrency;

            int actualAmtToBuy = product.itemCount * stackToBuy;
            IInventoryItem item = GameRules.GenerateItem(product.itemID, null, actualAmtToBuy);
            InvRetval invRetVal = peer.mInventory.AddItemsToInventory((ushort)product.itemID, actualAmtToBuy, true, "Store_Buy");
            switch (invRetVal.retCode)
            {
                case InvReturnCode.AddSuccess:
                    soldOutItem(product, shelveNo, peer);

                    peer.mPlayer.DeductCurrency(cType, cost, allowBind, "Store_Buy");
                    //Rare item notification broadcast
                    RareItemNotificationRules.CheckNotification(product.itemID, peer.mPlayer.PlayerSynStats.name);

                    retcode = StoreReturnCode.PurchaseItem_Success;
                    break;

                case InvReturnCode.AddFailed:
                case InvReturnCode.Full:
                    retcode = StoreReturnCode.PurchaseItem_Fail_NoBagSpace;
                    break;

                default:
                    retcode = StoreReturnCode.PurchaseItem_Fail_UnknownInventoryReturnCode;
                    break;
            }

            //Return currency in proper order
            if (cType == CurrencyType.LockGold || cType == CurrencyType.Gold)
            {
                oCurr1 = peer.mPlayer.GetCurrencyAmt(CurrencyType.LockGold);
                oCurr2 = peer.mPlayer.GetCurrencyAmt(CurrencyType.Gold);
            }
            else
            {
                oCurr1 = peer.mPlayer.GetCurrencyAmt(cType);
            }
            return retcode;
        }

        static void soldOutItem(StoreRepo.StoreProduct product, int shelveNo, GameClientPeer peer)
        {
            if (product == null || peer == null)
                return;
            if (product.storeOrder < 0 || product.storeOrder >= peer.CharacterData.StoreInventory.list_store.Count)
                return;
            if (!product.isValidShelve(shelveNo))
                return;

            StoreCategory cate = peer.CharacterData.StoreInventory.list_store[product.storeOrder];
            cate.list_storeitem[shelveNo - 1].isSold = 1;

            //Logging
            var cType = StoreRepo.GetCurrencyType(product.storeID);
            Log_PurchaseItem(product.storeOrder, shelveNo, product.itemID, product.itemCount, cType, product.totalPrice, peer);
        }

        static void SendItemToMail(string charName, IInventoryItem item)
        {
            MailObject mailObj = new MailObject();
            mailObj.rcvName = charName;
            mailObj.mailName = "Store Mail";

            mailObj.lstAttachment.Add(item);

            MailManager.Instance.SendMail(mailObj);
        }
        #endregion

        #region misc
        static int GetStoreCategoryTime(int category, GameClientPeer peer)
        {
            if (category < 0 || category >= peer.CharacterData.StoreInventory.list_store.Count)
                return -1;

            return peer.CharacterData.StoreInventory.list_store[category].refreshCount;

            //var store = JsonConvert.DeserializeObject<StoreCategory>((string)peer.mPlayer.StoreSynStats.list_store[category]);
            //return store.refreshCount;
        }
        #endregion

        #region Log
        static void Log_PurchaseItem(int storeOrder, int shelveNo, int itemID, int itemCount, CurrencyType ctype, int price, GameClientPeer peer)
        {
            Zealot.Logging.Client.LogClasses.StoreBuy log = new Logging.Client.LogClasses.StoreBuy();

            //if peer disconnect or invalid?
            if (peer == null || peer.mPlayer == null)
                return;

            log.userId = peer.mUserId;
            log.charId = peer.GetCharId();
            log.message = "";

            log.storeOrder = storeOrder;
            log.shelveNo = shelveNo;
            log.itemID = itemID;
            log.itemCount = itemCount;
            log.currencyType = ctype.ToString();
            log.itemPrice = price;

            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(log);
        }

        static void Log_Refresh(int storeOrder, int refreshCount, GameClientPeer peer)
        {
            Zealot.Logging.Client.LogClasses.StoreRefresh log = new Logging.Client.LogClasses.StoreRefresh();

            //if peer disconnect or invalid?
            if (peer == null || peer.mPlayer == null)
                return;

            log.userId = peer.mUserId;
            log.charId = peer.GetCharId();
            log.message = "";

            log.recvDate = DateTime.Now;
            log.storeOrder = storeOrder;
            log.storeRefreshCount = refreshCount;

            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(log);
        }
        #endregion
    }
}
