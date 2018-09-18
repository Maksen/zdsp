using Kopio.JsonContracts;
using Photon.LoadBalancing.GameServer;
using Photon.LoadBalancing.GameServer.Mail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.Repository;
using Zealot.Server.Entities;

namespace Zealot.Server.Rules
{
    public static class LootRules
    {
        public static LimitedItemInventory mLimitedItemInventory = new LimitedItemInventory();

        public static void Init()
        {
            List<Dictionary<string, object>> records = GameApplication.dbRepository.LimitedItem.GetRecords();
            if (records.Count > 0)
                mLimitedItemInventory = LimitedItemInventory.DeserializeFromDB((string)records[0]["payload"]);
        }

        public static void AddLimitItem(List<ItemInfo> itemsToAdd)
        {
            int count = itemsToAdd.Count;
            for (int index = 0; index < count; ++index)
                AddLimitItem(itemsToAdd[index].itemId, itemsToAdd[index].stackCount);
        }

        //return count added.
        public static int AddLimitItem(int itemid, int stackCount)
        {
            LimitedItemJson json = LootRepo.GetLimitedItemInfo(itemid);
            if (json == null)
                return stackCount;
            int maxAmt = json.amt;
            if (maxAmt == 0)
                return 0;
            DateTime today = DateTime.Today;
            LimitedItemData record;
            if (mLimitedItemInventory.records.TryGetValue(itemid, out record))
            {
                int amountNow = record.amount;
                switch (json.cycle)
                {
                    case LimitItemCycle.Day:
                        if (record.lastUpdateDate != today)
                            amountNow = 0;
                        break;
                    case LimitItemCycle.Week:
                        if (record.lastUpdateDate.AddDays(-AdjustDayOfWeek(record.lastUpdateDate.DayOfWeek)) !=
                            today.AddDays(-AdjustDayOfWeek(today.DayOfWeek)))
                            amountNow = 0;
                        break;
                    case LimitItemCycle.Month:
                        if (record.lastUpdateDate.Year != today.Year || record.lastUpdateDate.Month != today.Month)
                            amountNow = 0;
                        break;
                }
                
                if (amountNow >= maxAmt)
                    return 0;
                else
                {
                    record.lastUpdateDate = today;
                    mLimitedItemInventory.IsDirty = true;
                    record.amount = amountNow + stackCount;
                    int overCount = record.amount - maxAmt;
                    if (overCount > 0)
                    {
                        record.amount = maxAmt;
                        return stackCount - overCount;
                    }
                    else
                        return stackCount;
                }
            }
            else
            {
                int added = Math.Min(stackCount, maxAmt);
                mLimitedItemInventory.IsDirty = true;
                mLimitedItemInventory.records.Add(itemid, new LimitedItemData() { amount = added, lastUpdateDate = today });
                return added;
            }
        }

        //check can add how many count, but not modify amount
        public static int CanAddLimitItem(int itemid, int stackCount, out bool findLimitItem)
        {
            findLimitItem = false;
            LimitedItemJson json = LootRepo.GetLimitedItemInfo(itemid);
            if (json == null)
                return stackCount;
            int maxAmt = json.amt;
            if (maxAmt == 0)
                return 0;
            DateTime today = DateTime.Today;
            LimitedItemData record;
            if (mLimitedItemInventory.records.TryGetValue(itemid, out record))
            {
                int amountNow = record.amount;
                switch (json.cycle)
                {
                    case LimitItemCycle.Day:
                        if (record.lastUpdateDate != today)
                            amountNow = 0;
                        break;
                    case LimitItemCycle.Week:
                        if (record.lastUpdateDate.AddDays(-AdjustDayOfWeek(record.lastUpdateDate.DayOfWeek)) !=
                            today.AddDays(-AdjustDayOfWeek(today.DayOfWeek)))
                            amountNow = 0;
                        break;
                    case LimitItemCycle.Month:
                        if (record.lastUpdateDate.Year != today.Year || record.lastUpdateDate.Month != today.Month)
                            amountNow = 0;
                        break;
                }

                if (amountNow >= maxAmt)
                    return 0;
                else
                {
                    findLimitItem = true;
                    int overCount = amountNow + stackCount - maxAmt;
                    if (overCount > 0)
                        return stackCount - overCount;
                    else
                        return stackCount;
                }
            }
            else
            {
                findLimitItem = true;
                return Math.Min(stackCount, maxAmt);
            }
        }

        public static int AdjustDayOfWeek(DayOfWeek dayOfweek)
        {
            if (dayOfweek == DayOfWeek.Sunday)
                return 7;
            return (int)dayOfweek;
        }

        public static async Task SaveLimitedItemsToDB()
        {
            string records = "";
            if (mLimitedItemInventory.IsDirty)
            {
                records = mLimitedItemInventory.SerializeForDB();
                mLimitedItemInventory.IsDirty = false;
            }
            if (records != "")
            {
                var saved = await GameApplication.dbRepository.LimitedItem.Insert_Update(records);
                if (!saved)
                    mLimitedItemInventory.IsDirty = true; //if save fail, make it dirty so that it can trigger save again.
            }
        }

        #region Generate Loot
        public static void GenerateLootItems(Player player, List<LootItem> lootItemList, MonsterType monsterType, int monsterLvl, 
            LootItemDisplayInventory displayInventory)
        {
            int lootItemListCount = lootItemList.Count;
            if (lootItemListCount == 0)
                return;

            int lootCorrectionPercent = 100;
            LootCorrectionJson lootCorrectionJson = LootRepo.GetLootCorrection(monsterLvl - player.GetAccumulatedLevel());
            if (lootCorrectionJson != null)
            {
                switch (monsterType)
                {
                    case MonsterType.Normal:
                        lootCorrectionPercent = lootCorrectionJson.normalmonster;
                        break;
                    case MonsterType.MiniBoss:
                        lootCorrectionPercent = lootCorrectionJson.boss;
                        break;
                }
            }

            bool noBattleTime = player.Slot.CharacterData.BattleTime <= 0;
            bool isDisplayingLoot = (displayInventory != null);
            int pid = player.GetPersistentID();
            List<ItemInfo> itemList = new List<ItemInfo>();
            Dictionary<CurrencyType, int> currencyToAdd = new Dictionary<CurrencyType, int>();
            for (int index = 0; index < lootItemListCount; ++index)
            {
                LootItem lootItem = lootItemList[index];
                if (!lootItem.ignorelv)
                {
                    if (lootCorrectionPercent == 0 || (lootCorrectionPercent < 100 && lootCorrectionPercent < GameUtils.RandomInt(1, 100)))
                        continue;
                }
                if (!lootItem.ignoretime && noBattleTime)
                    continue;
                int stackCount = lootItem.GetAmount();
                if (stackCount == 0)
                    continue;
                int itemId = lootItem.itemid;
                if (itemId > 0)
                {
                    int addCount = AddLimitItem(itemId, stackCount);
                    if (addCount == 0)
                        continue; // limiteditem hit max amount
                    itemList.Add(new ItemInfo() { itemId = (ushort)itemId, stackCount = addCount });
                    if (isDisplayingLoot)
                        displayInventory.Add(pid, itemId);
                }
                else if (lootItem.currencyType != CurrencyType.None)
                {
                    if (currencyToAdd.ContainsKey(lootItem.currencyType))
                        currencyToAdd[lootItem.currencyType] += stackCount;
                    else
                        currencyToAdd[lootItem.currencyType] = stackCount;
                    if (isDisplayingLoot)
                        displayInventory.Add(pid, -1);
                }
            }

            var retValue = player.Slot.mInventory.AddItemsIntoInventory(itemList, true, "Loot");
            if (retValue.retCode != InvReturnCode.AddSuccess)
            {
                // If can't add to inventory, send mail
                List<IInventoryItem> itemsToAdd = new List<IInventoryItem>();
                foreach (var item in itemList)
                    itemsToAdd.Add(GameRules.GenerateItem(item.itemId, null, item.stackCount));
                GameRules.SendMailWithAttachment(player.Name, "Loot", itemsToAdd, currencyToAdd);
            }
            else
            {
                // Add currency
                foreach (var currency in currencyToAdd)
                    player.AddCurrency(currency.Key, currency.Value, "Loot");
            }
        }

        // Doesn't check LootCorrection, battle time, no displayloot
        public static void GenerateLootItems(List<int> grpIds, Dictionary<int, int> itemsToAdd, Dictionary<CurrencyType, int> currencyToAdd)
        {
            if (grpIds.Count == 0)
                return;

            List<LootItem> lootItemList = LootRepo.RandomItems(grpIds);
            GenerateLootItems(lootItemList, itemsToAdd, currencyToAdd, 0, null);
        }

        public static void GenerateLootItems_SendMail(string playerName, List<LootItem> lootItemList, LootItemDisplayInventory displayInventory)
        {
            Dictionary<int, int> itemsToAdd = new Dictionary<int, int>();
            Dictionary<CurrencyType, int> currencyToAdd = new Dictionary<CurrencyType, int>();
            if (GenerateLootItems(lootItemList, itemsToAdd, currencyToAdd, 0, displayInventory))
            {
                // If can't add to inventory, send mail
                List<IInventoryItem> itemList = GetInvItemListToAdd(itemsToAdd, true);
                GameRules.SendMailWithAttachment(playerName, "Loot", itemList, currencyToAdd);
            }
        }

        public static bool GenerateLootItems(List<LootItem> lootItemList, Dictionary<int, int> itemsToAdd, Dictionary<CurrencyType, int> currencyToAdd,
            int pid, LootItemDisplayInventory displayInventory)
        {
            int lootItemListCount = lootItemList.Count;
            if (lootItemListCount == 0)
                return false;

            bool isAddingCurrency = currencyToAdd != null, isDisplayingLoot = (displayInventory != null);
            for (int index = 0; index < lootItemListCount; ++index)
            {
                LootItem lootItem = lootItemList[index];
                int stackCount = lootItem.GetAmount();
                if (stackCount == 0)
                    continue;
                int itemId = lootItem.itemid;
                if (itemId > 0)
                {
                    if (itemsToAdd.ContainsKey(itemId))
                        itemsToAdd[itemId] += stackCount;
                    else
                        itemsToAdd[itemId] = stackCount;
                    if (isDisplayingLoot)
                        displayInventory.Add(pid, itemId);
                }
                else if (lootItem.currencyType != CurrencyType.None)
                {
                    if (isAddingCurrency)
                    {
                        if (currencyToAdd.ContainsKey(lootItem.currencyType))
                            currencyToAdd[lootItem.currencyType] += stackCount;
                        else
                            currencyToAdd[lootItem.currencyType] = stackCount;
                        if (isDisplayingLoot)
                            displayInventory.Add(pid, -1);
                    }
                }
            }
            return true;
        }

        public static List<ItemInfo> GetItemInfoListToAdd(Dictionary<int, int> itemsToAdd, bool checkLimitItem, out bool findLimitItem)
        {
            List<ItemInfo> itemList = new List<ItemInfo>();
            findLimitItem = false;
            if (checkLimitItem)
            {
                foreach (var kvp in itemsToAdd)
                {
                    bool found;
                    int addedCount = CanAddLimitItem(kvp.Key, kvp.Value, out found);
                    if (addedCount > 0)
                    {
                        if (found)
                            findLimitItem = true;
                        itemList.Add(new ItemInfo { itemId = (ushort)kvp.Key, stackCount = addedCount });
                    }
                }
            }
            else
            {
                foreach (var kvp in itemsToAdd)
                    itemList.Add(new ItemInfo { itemId = (ushort)kvp.Key, stackCount = kvp.Value });
            }
            return itemList;
        }

        public static List<ItemInfo> GetItemInfoListToAdd(Dictionary<int, int> itemsToAdd, bool checkLimitItem)
        {
            List<ItemInfo> itemList = new List<ItemInfo>();
            if (checkLimitItem)
            {
                foreach (var kvp in itemsToAdd)
                {
                    int addedCount = AddLimitItem(kvp.Key, kvp.Value);
                    if (addedCount > 0)
                        itemList.Add(new ItemInfo { itemId = (ushort)kvp.Key, stackCount = addedCount });
                }
            }
            else
            {
                foreach (var kvp in itemsToAdd)
                    itemList.Add(new ItemInfo { itemId = (ushort)kvp.Key, stackCount = kvp.Value });
            }
            return itemList;
        }

        public static List<IInventoryItem> GetInvItemListToAdd(Dictionary<int, int> itemsToAdd, bool checkLimitItem)
        {
            List<IInventoryItem> itemList = new List<IInventoryItem>();
            if (checkLimitItem)
            {
                foreach (var kvp in itemsToAdd)
                {
                    int addedCount = AddLimitItem(kvp.Key, kvp.Value);
                    if (addedCount > 0)
                        itemList.Add(GameRules.GenerateItem(kvp.Key, null, addedCount));
                }
            }
            else
            {
                foreach (var kvp in itemsToAdd)
                    itemList.Add(GameRules.GenerateItem(kvp.Key, null, kvp.Value));
            }
            return itemList;
        }
        #endregion
    }
}
