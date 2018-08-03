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
                bool reset = false;
                switch (json.cycle)
                {
                    case LimitItemCycle.Day:
                        if (record.lastUpdateDate != today)
                            reset = true;
                        break;
                    case LimitItemCycle.Week:
                        if (record.lastUpdateDate.AddDays(-AdjustDayOfWeek(record.lastUpdateDate.DayOfWeek)) !=
                            today.AddDays(-AdjustDayOfWeek(today.DayOfWeek)))
                            reset = true;
                        break;
                    case LimitItemCycle.Month:
                        if (record.lastUpdateDate.Year != today.Year || record.lastUpdateDate.Month != today.Month)
                            reset = true;
                        break;
                }   
                if (reset)
                    record.amount = 0;
                
                if (record.amount >= maxAmt)
                    return 0;
                else
                {
                    record.lastUpdateDate = today;
                    mLimitedItemInventory.IsDirty = true;
                    record.amount = record.amount + stackCount;
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
                mLimitedItemInventory.records.Add(itemid, new LimitedItemData() { amount = added, lastUpdateDate = today });
                return added;
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
        public static void GenerateLootItem(Player player, List<LootItem> itemList, MonsterClass monsterClass, int monsterLvl, LootItemDisplayInventory displayInventory)
        {
            int itemListCount = itemList.Count;
            if (itemListCount == 0)
                return;

            int pid = player.GetPersistentID();
            int lootCorrectionPercent = 100;
            LootCorrectionJson lootCorrectionJson = LootRepo.GetLootCorrection(monsterLvl - player.GetAccumulatedLevel());
            if (lootCorrectionJson != null)
            {
                switch (monsterClass)
                {
                    case MonsterClass.Normal:
                        lootCorrectionPercent = lootCorrectionJson.normalmonster;
                        break;
                    case MonsterClass.Mini:
                        lootCorrectionPercent = lootCorrectionJson.boss;
                        break;
                }
            }
            bool noBattleTime = player.Slot.CharacterData.BattleTime <= 0;

            List<ItemInfo> items = new List<ItemInfo>();
            Dictionary<CurrencyType, int> currencyAdded = new Dictionary<CurrencyType, int>();
            for (int index = 0; index < itemListCount; index++)
            {
                LootItem lootItem = itemList[index];
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
                int itemid = lootItem.itemid;
                if (itemid > 0)
                {
                    int addedCount = AddLimitItem(itemid, stackCount);
                    if (addedCount == 0) 
                        continue; //limiteditem hit max amount
                    items.Add(new ItemInfo() { itemId = (ushort)itemid, stackCount = addedCount });
                    if (displayInventory != null)
                        displayInventory.Add(pid, itemid);
                }
                else if (lootItem.currencyType != CurrencyType.None)
                {
                    if (currencyAdded.ContainsKey(lootItem.currencyType))
                        currencyAdded[lootItem.currencyType] += stackCount;
                    else
                        currencyAdded[lootItem.currencyType] = stackCount;
                    if (displayInventory != null)
                        displayInventory.Add(pid, -1);
                }
            }
            var retValue = player.Slot.mInventory.AddItemsIntoInventory(items, true, "Loot");
            if (retValue.retCode != InvReturnCode.AddSuccess)
            {
                // If cant add to bag, send mail
                MailObject mailObj = new MailObject();
                mailObj.rcvName = player.Name;
                mailObj.mailName = "Loot";
                List<IInventoryItem> list_Attachment = new List<IInventoryItem>();
                foreach (var item in items)
                    list_Attachment.Add(GameRules.GenerateItem(item.itemId, null, item.stackCount));

                mailObj.lstAttachment = list_Attachment;
                mailObj.dicCurrencyAmt = currencyAdded;
                MailManager.Instance.SendMail(mailObj);
            }
            else
            {
                // Add currency
                foreach (var currency in currencyAdded)
                    player.AddCurrency(currency.Key, currency.Value, "Loot");
            }
        }

        public static void GenerateLootItem_SendMail(string playerName, List<LootItem> itemList, LootItemDisplayInventory displayInventory)
        {
            int itemListCount = itemList.Count;
            if (itemListCount == 0)
                return;

            List<ItemInfo> items = new List<ItemInfo>();
            Dictionary<CurrencyType, int> currencyAdded = new Dictionary<CurrencyType, int>();
            for (int index = 0; index < itemListCount; index++)
            {
                LootItem lootItem = itemList[index];
                int stackCount = lootItem.GetAmount();
                if (stackCount == 0)
                    continue;
                int itemid = lootItem.itemid;
                if (itemid > 0)
                {
                    int addedCount = AddLimitItem(itemid, stackCount);
                    if (addedCount == 0)
                        continue; //limiteditem hit max amount
                    items.Add(new ItemInfo() { itemId = (ushort)itemid, stackCount = addedCount });
                    if (displayInventory != null)
                        displayInventory.Add(0, itemid);
                }
                else if (lootItem.currencyType != CurrencyType.None)
                {
                    if (currencyAdded.ContainsKey(lootItem.currencyType))
                        currencyAdded[lootItem.currencyType] += stackCount;
                    else
                        currencyAdded[lootItem.currencyType] = stackCount;
                    if (displayInventory != null)
                        displayInventory.Add(0, -1);
                }
            }

            // If cant add to bag, send mail
            MailObject mailObj = new MailObject();
            mailObj.rcvName = playerName;
            mailObj.mailName = "Loot";
            List<IInventoryItem> list_Attachment = new List<IInventoryItem>();
            foreach (var item in items)
                list_Attachment.Add(GameRules.GenerateItem(item.itemId, null, item.stackCount));

            mailObj.lstAttachment = list_Attachment;
            mailObj.dicCurrencyAmt = currencyAdded;
            MailManager.Instance.SendMail(mailObj);
        }
        #endregion
    }
}
