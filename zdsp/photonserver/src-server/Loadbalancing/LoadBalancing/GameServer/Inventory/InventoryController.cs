using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.Rules;
using Zealot.Server.Entities;
using Zealot.Server.SideEffects;

public class InvRetval
{
    public InvReturnCode retCode;
    public Dictionary<int, int> invSlot = new Dictionary<int, int>();
    public Dictionary<int, ushort> equipSlot = new Dictionary<int, ushort>();

    public void SetInvSlot(int slotIndex, int amount)
    {
        if (amount == 0)
            return;
        if (invSlot.ContainsKey(slotIndex))
            invSlot[slotIndex] += amount;
        else
            invSlot.Add(slotIndex, amount);
    }
}

namespace Photon.LoadBalancing.GameServer
{
    public class ItemInventoryController
    {
        //private ItemUIDGenerator mUIDGenerator;
        public ItemInventoryServerData mInvData;
        private EquipmentInventoryData mEquipInvData;
        private ItemSortInventoryData mItemSortInvData;
        private GameClientPeer mSlot;

        // At these slots, player are unable to meet the requirement of the equipment and failed to wear them.
        HashSet<EquipmentSlot> mEquipmentRequirementsFailed = new HashSet<EquipmentSlot>();

        public enum OpenNewSlotType
        {
            DEFAULT,
            FREE,
            VIP_AUTOOPEN,
        }

        public ItemInventoryController(GameClientPeer peer)
        {
            int connID = peer.ConnectionId;
            //mUIDGenerator = new ItemUIDGenerator(1, connID);
            mSlot = peer;
            InitFromData();
        }

        public void InitFromData()
        {
            CharacterData characterData = mSlot.CharacterData;
            ItemInventoryData itemInvData = characterData.ItemInventory;
            mInvData = new ItemInventoryServerData();
            mInvData.Init(itemInvData);

            mEquipInvData = characterData.EquipmentInventory;
            mItemSortInvData = characterData.ItemSortInventory;
        }

        //public string GenerateItemUID()
        //{
        //    return mUIDGenerator.GenerateItemUID();
        //}

        public int GetEmptySlotCount()
        {
            return mInvData.GetEmptySlotCount();
        }

        public Dictionary<int, IInventoryItem> GetItemsByItemType(ItemType type)
        {
            return mInvData.GetItemsByItemType(type);
        }

        public int GetItemStackCountByItemId(ushort itemId)
        {
            return mInvData.GetTotalStackCountByItemId(itemId);
        }

        public bool HasItem(ushort itemId, int stackCount)
        {
            return mInvData.HasItem(itemId, stackCount);
        }

        public void LogItem(string from, int itemid, int count, bool add)
        {
            ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(itemid);
            if (itemJson != null && itemJson.log)
            {
                if (!add)
                    count = -count;
                string message = string.Format("source:{0}|id:{1}|amt:{2}", from, itemid, count);
                Zealot.Logging.Client.LogClasses.ItemChange itemChangeLog = new Zealot.Logging.Client.LogClasses.ItemChange();
                itemChangeLog.userId = mSlot.mUserId;
                itemChangeLog.charId = mSlot.GetCharId();
                itemChangeLog.message = message;
                itemChangeLog.source = from;
                itemChangeLog.itemid = itemid;
                itemChangeLog.amt = count;
                var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(itemChangeLog);
            }
        }

        public InvRetval AddItemsToInventory(IInventoryItem item, bool isNew, string from)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.AddFailed;
            ItemInfo additem = new ItemInfo() { itemId = item.ItemID, stackCount = item.StackCount };
            bool canAdd = mInvData.CanAdd(new List<ItemInfo>() { additem });
            if (canAdd)
            {
                if (AddItems(item, ref retval, isNew))
                {
                    OnAddItems(retval.invSlot);
                    SyncInvData(retval.invSlot);
                    retval.retCode = InvReturnCode.AddSuccess;
                    LogItem(from, item.ItemID, item.StackCount, true);
                    UpdateCollectItemAchievement(item.ItemID, item.StackCount);
                }
                else
                    RevertAdd(retval.invSlot);
            }
            else
            {
                retval.retCode = InvReturnCode.Full;
            }
            return retval;
        }

        public InvRetval AddItemsToInventory(List<IInventoryItem> items, bool isNew, string from)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.AddFailed;
            List<ItemInfo> addItems = new List<ItemInfo>();
            foreach (IInventoryItem item in items)
            {
                ItemInfo _iteminfo = addItems.Find(x => x.itemId == item.ItemID);
                if (_iteminfo != null)
                    _iteminfo.stackCount += item.StackCount;
                else
                    addItems.Add(new ItemInfo() { itemId = item.ItemID, stackCount = item.StackCount });
            }
            bool canAdd = mInvData.CanAdd(addItems);
            if (canAdd)
            {
                bool success = true;
                foreach (IInventoryItem item in items)
                {
                    if (!AddItems(item, ref retval, isNew))
                    {
                        success = false;
                        break;
                    }
                }

                if (success)
                {
                    OnAddItems(retval.invSlot);
                    SyncInvData(retval.invSlot);
                    retval.retCode = InvReturnCode.AddSuccess;
                    for (int index = 0; index < items.Count; ++index)
                    {
                        LogItem(from, items[index].ItemID, items[index].StackCount, true);
                        UpdateCollectItemAchievement(items[index].ItemID, items[index].StackCount);
                    }
                }
                else
                    RevertAdd(retval.invSlot);
            }
            else
            {
                retval.retCode = InvReturnCode.Full;
            }
            return retval;
        }

        public InvRetval AddItemsToInventory(ushort itemid, int stackcount, bool isNew, string from)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.AddFailed;
            if (itemid == 0 || stackcount == 0)
            {
                retval.retCode = InvReturnCode.AddSuccess;
                return retval;
            }

            ItemInfo additem = new ItemInfo() { itemId = itemid, stackCount = stackcount};
            bool canAdd = mInvData.CanAdd(new List<ItemInfo>() { additem });
            if (canAdd)
            {
                if (AddItems(itemid, stackcount, ref retval, isNew))
                {
                    OnAddItems(retval.invSlot);
                    SyncInvData(retval.invSlot);
                    retval.retCode = InvReturnCode.AddSuccess;
                    LogItem(from, itemid, stackcount, true);
                    UpdateCollectItemAchievement(itemid, stackcount);
                }
                else
                    RevertAdd(retval.invSlot);
            }
            else
            {
                retval.retCode = InvReturnCode.Full;
            }
            return retval;
        }

        public InvRetval AddItemsToInventory(List<ItemInfo> items, bool isNew, string from)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.AddFailed;
            if (items.Count == 0)
            {
                retval.retCode = InvReturnCode.AddSuccess;
                return retval;
            }

            bool canAdd = mInvData.CanAdd(items);
            if (canAdd)
            {
                bool success = true;
                foreach (ItemInfo item in items)
                {
                    if (!AddItems(item.itemId, item.stackCount, ref retval, isNew))
                    {
                        success = false;
                        break;
                    }
                }

                if (success)
                {
                    OnAddItems(retval.invSlot);
                    SyncInvData(retval.invSlot);
                    retval.retCode = InvReturnCode.AddSuccess;
                    for (int index = 0; index < items.Count; ++index)
                    {
                        LogItem(from, items[index].itemId, items[index].stackCount, true);
                        UpdateCollectItemAchievement(items[index].itemId, items[index].stackCount);
                    }
                }
                else
                    RevertAdd(retval.invSlot);
            }
            else
            {
                retval.retCode = InvReturnCode.Full;
            }
            return retval;
        }

        public void AddItemsToInventoryMailIfFail(List<ItemInfo> items, Dictionary<CurrencyType, int> currencyToAdd, string from)
        {
            Player player = mSlot.mPlayer;
            if (player == null)
                return;

            var retValue = AddItemsToInventory(items, true, from);
            if (retValue.retCode != InvReturnCode.AddSuccess)
            {
                // If can't add to inventory, send mail
                List<IInventoryItem> itemsToAdd = new List<IInventoryItem>();
                foreach (ItemInfo item in items)
                    itemsToAdd.Add(GameRules.GenerateItem(item.itemId, null, item.stackCount));
                GameRules.SendMailWithAttachment(player.Name, from, itemsToAdd, currencyToAdd);
            }
            else if (currencyToAdd != null) // Add items success, add currency
            {
                foreach (var currency in currencyToAdd)
                    player.AddCurrency(currency.Key, currency.Value, from);
            }
        }

        private bool AddItems(ushort itemId, int stackCount, ref InvRetval retval, bool isNew = true)
        {       
            ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(itemId);
            if (itemJson == null)
                return false;

            int slotId = -1;
            ItemSortJson itemSortJson = GameRepo.ItemFactory.GetItemSortById(itemJson.itemsort);
            if (itemSortJson.maxstackcount > 1)
            {
                while (stackCount > 0)
                {
                    slotId = mInvData.GetAvailableSlotByItemId(itemId);
                    if (slotId == -1)
                        break;
                    else
                        AddItem(mInvData.Slots[slotId], slotId, ref stackCount, ref retval, isNew);
                }
            }

            while (stackCount > 0)
            {
                slotId = mInvData.GetEmptySlotIndex();
                if (slotId == -1)
                    break;
                else
                {
                    IInventoryItem newitem = GameRules.GenerateItem(itemId, mSlot);
                    if (newitem == null)
                        return false;
                    if (stackCount <= newitem.MaxStackCount)
                    {
                        newitem.StackCount = stackCount;
                        stackCount = 0;
                    }
                    else
                    {
                        newitem.StackCount = newitem.MaxStackCount;
                        stackCount -= newitem.MaxStackCount;
                    }
                    if (isNew)
                        newitem.SetNewItem();
                    mInvData.SetSlotItem(slotId, newitem);
                    retval.SetInvSlot(slotId, newitem.StackCount);
                }
            }

            return stackCount == 0; // Fail if stackcount > 0
        }

        private bool AddItems(IInventoryItem item, ref InvRetval retval, bool isNew = true)
        {
            int stackCount = item.StackCount;
            Dictionary<int, IInventoryItem> slots = mInvData.GetStackableSlot(item.ItemID);
            if (slots.Count > 0)
            {                
                foreach (KeyValuePair<int, IInventoryItem> slot in slots)
                {
                    if (stackCount == 0)
                        break;
                    AddItem(slot.Value, slot.Key, ref stackCount, ref retval, isNew);
                }
                item.StackCount = stackCount;
            }

            if (stackCount > 0)
            {
                if (stackCount > item.MaxStackCount)
                    return false;
                int slotId = mInvData.GetEmptySlotIndex();
                if (slotId == -1)
                    return false;
                if (isNew)
                    item.SetNewItem();
                mInvData.SetSlotItem(slotId, item);
                retval.SetInvSlot(slotId, item.StackCount);
            }
            return true;
        }

        private void AddItem(IInventoryItem invItem, int slotId, ref int stackCount, ref InvRetval retval, bool isNew)
        {
            if (invItem == null)
                return;

            ushort remaining = (ushort)(invItem.MaxStackCount - invItem.StackCount);
            if (remaining >= stackCount)
            {
                invItem.StackCount += stackCount;
                retval.SetInvSlot(slotId, stackCount);
                stackCount = 0; // Has added all
            }
            else
            {
                invItem.StackCount += remaining;
                retval.SetInvSlot(slotId, remaining);
                stackCount -= remaining;
            }
            if (isNew)
                invItem.SetNewItem();
        }

        public void OnAddItems(Dictionary<int, int> items)
        {
            foreach (KeyValuePair<int, int> slot in items)
            {
                int itemId = mInvData.OnAddItem(slot.Key, slot.Value);

                // Update Destiny Clue
                mSlot.DestinyClueController.TriggerClueCondition(ClueCondition.Item, itemId);
            }
        }

        public InvRetval SwapEquipmentToInventory(int eqSlotId)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.UnEquipFailed;
            Equipment equipItem = mEquipInvData.GetEquipmentBySlotId(eqSlotId);
            if (equipItem == null)
                return retval;

            int invSlotId = mInvData.GetEmptySlotIndex();
            if (invSlotId == -1)
            {
                retval.retCode = InvReturnCode.Full;
                return retval;
            }

            mInvData.SetSlotItem(invSlotId, equipItem);
            retval.SetInvSlot(invSlotId, 1);

            mEquipInvData.SetEquipmentToSlot(eqSlotId, null);
            retval.equipSlot[eqSlotId] = 1;

            SyncInvData(retval.invSlot);
            SyncEquipmentData(retval.equipSlot, false);
            UpdateEquipmentCombatStats(false, invSlotId, eqSlotId);
            // Update sideeffects
            UpdateEquipmentSideEffect(mSlot.mPlayer, equipItem.EquipmentJson, false);
            retval.retCode = InvReturnCode.UnEquipSuccess;

            return retval;
        }

        public InvRetval SwapEquipmentFromInventory(int invSlotId)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.EquipFailed;
            IInventoryItem item = mInvData.Slots[invSlotId];
            if (item == null || !CanUse(item, ref retval))
                return retval;
            Equipment equipItem = item as Equipment;
            if (equipItem.EquipmentJson.fashionsuit)
                return retval;

            EquipmentSlot _equipSlot = mEquipInvData.FindAEquipSlotToSwap(equipItem);
            if (_equipSlot == EquipmentSlot.MAXSLOTS)
                return retval;

            int equipSlot = (int)_equipSlot;
            Equipment destItem = mEquipInvData.GetEquipmentBySlotId(equipSlot);
            if (destItem != null)
            {
                UpdateEquipmentSideEffect(mSlot.mPlayer, destItem.EquipmentJson, false);
                mInvData.SetSlotItem(invSlotId, destItem);
            }
            else
                mInvData.SetSlotItem(invSlotId, null);
            retval.SetInvSlot(invSlotId, 1);

            mEquipInvData.SetEquipmentToSlot(equipSlot, equipItem);
            retval.equipSlot[equipSlot] = 1;

            SyncInvData(retval.invSlot);
            SyncEquipmentData(retval.equipSlot, false);
            if(equipItem.EquipmentJson.equiptype == EquipmentType.Weapon)
                mSlot.mPlayer.UpdateBasicAttack(equipItem.EquipmentJson);
            UpdateEquipmentCombatStats(true, invSlotId, equipSlot);
            // Update sideeffects
            UpdateEquipmentSideEffect(mSlot.mPlayer, equipItem.EquipmentJson, true);
            retval.retCode = InvReturnCode.EquipSuccess;

            return retval;
        }

        public void UpdateEquipmentSideEffect(Player player, EquipmentJson equipmentJson, bool isEquipping)
        {              
            List<int> seIds = new List<int>();
            string seStr = equipmentJson.basese;
            if (!string.IsNullOrEmpty(seStr))
            {
                string[] seStrIds = seStr.Split(';');
                int length = seStrIds.Length;
                for (int i = 0; i < length; ++i)
                {
                    int id;
                    if (int.TryParse(seStrIds[i], out id))
                        seIds.Add(id);
                }
            }
            //seStr = equipmentJson.extrase;
            //if (!string.IsNullOrEmpty(seStr))
            //{
            //    string[] seStrIds = seStr.Split(';');
            //    int length = seStrIds.Length;
            //    for (int i = 0; i < length; ++i)
            //    {
            //        int id;
            //        if (int.TryParse(seStrIds[i], out id))
            //        {
            //            if (id != -1)
            //                seIds.Add(id);
            //        }
            //    }
            //}

            if (seIds.Count > 0)
                SideEffectsUtils.ClientUpdateEquipmentSideEffects(player, seIds, equipmentJson.id, isEquipping);
        }

        public InvRetval SwapFashionToInventory(int eqSlotId)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.UnEquipFailed;
            Equipment equipItem = mEquipInvData.GetFashionSlot(eqSlotId);
            if (equipItem == null)
                return retval;

            int invSlotId = mInvData.GetEmptySlotIndex();
            if (invSlotId == -1)
            {
                retval.retCode = InvReturnCode.Full;
                return retval;
            }

            mInvData.SetSlotItem(invSlotId, equipItem);
            retval.SetInvSlot(invSlotId, 1);

            mEquipInvData.SetFashionToSlot(eqSlotId, null);
            retval.equipSlot[eqSlotId] = 1;

            SyncInvData(retval.invSlot);
            SyncEquipmentData(retval.equipSlot, true);
            retval.retCode = InvReturnCode.UnEquipSuccess;

            return retval;
        }

        public InvRetval SwapFashionFromInventory(int invSlotId)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.EquipFailed;
            IInventoryItem item = mInvData.Slots[invSlotId];
            if (item == null || !CanUse(item, ref retval))
                return retval;
            Equipment equipItem = mInvData.Slots[invSlotId] as Equipment;
            if (!equipItem.EquipmentJson.fashionsuit)
                return retval;

            FashionSlot _equipSlot = InventoryHelper.GetFashionSlotByPartType(equipItem.EquipmentJson.partstype);
            if (_equipSlot == FashionSlot.MAXSLOTS)
                return retval;

            int equipSlot = (int)_equipSlot;
            Equipment destItem = mEquipInvData.GetFashionSlot(equipSlot);
            if (destItem != null)
                mInvData.SetSlotItem(invSlotId, destItem);
            else
                mInvData.SetSlotItem(invSlotId, null);
            retval.SetInvSlot(invSlotId, 1);

            mEquipInvData.SetFashionToSlot(equipSlot, equipItem);
            retval.equipSlot[equipSlot] = 1;

            SyncInvData(retval.invSlot);
            SyncEquipmentData(retval.equipSlot, true);
            retval.retCode = InvReturnCode.EquipSuccess;

            return retval;
        }

        public InvRetval UseItemInInventory(int slotId, int useAmount)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.UseFailed;
            IInventoryItem item = mInvData.Slots[slotId];
            if (item == null || mSlot.mPlayer == null || item.StackCount < useAmount)
                return retval;

            ItemBaseJson itemJson = item.JsonObject;
            switch (itemJson.itemtype)
            {
                case ItemType.Equipment:
                    retval = useAmount == 0 ? SwapFashionFromInventory(slotId) : SwapEquipmentFromInventory(slotId);
                    break;
                default:
                    retval = UseItem(slotId, useAmount);
                    break;
            }
            return retval;
        }

        //*** Use from Inventory Bag ***//
        public InvRetval UseItem(int slotId, int useAmount, string from = "")
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.UseFailed;
            IInventoryItem item = mInvData.Slots[slotId];
            if (item == null || item.StackCount < useAmount || !item.JsonObject.useable) 
                return retval;
            Player player = mSlot.mPlayer;
            if (player == null || !player.IsAlive() || !CanUse(item, ref retval))
                return retval;

            bool success = true;
            if (!mInvData.HasItem(item.ItemID, useAmount) || !DeductItem(item.ItemID, slotId, useAmount, ref retval))
                success = false;
            if (success)
            {
                bool usedSuccess = false;
                ItemBaseJson itemJson = item.JsonObject;
                switch (itemJson.itemtype)
                {
                    case ItemType.PotionFood:
                        //var potionItem = item as PotionItem;
                        //int healAbsolute = mSlot.mPlayer.AddHealthPercentage(potionItem.Heal);
                        //int cooldownReduction = (int)mSlot.mPlayer.SkillPassiveStats.GetField(SkillPassiveFieldName.Potion_CD);
                        //float realcooldown = potionItem.CooldownInSec * (1 - cooldownReduction * 0.01f); //really need a varaiant cooldown for different item at Server?
                        //UpdateItemLastUsedDT(potionItem.ItemType, potionItem.ItemID, true, (int)realcooldown);
                        ////Tell player about healing
                        //{
                        //    AttackResult ar = new AttackResult(mSlot.mPlayer.GetPersistentID(), healAbsolute);
                        //    ar.IsHeal = true;
                        //    mSlot.mPlayer.QueueDmgResult(ar);
                        //}
                        PotionFoodJson potionFood = (PotionFoodJson)itemJson;
                        int sideffectid = -1;
                        if (int.TryParse(potionFood.seid, out sideffectid))
                        {
                            if (SideEffectRepo.mIdMap.ContainsKey(sideffectid))
                            {
                                var sedata = SideEffectRepo.mIdMap[sideffectid];
                                var se = SideEffectFactory.CreateSideEffect(sedata);
                                if (se != null)
                                    se.Apply(mSlot.mPlayer, mSlot.mPlayer, true);
                                usedSuccess = true;
                            }
                        }
                        break;
                    case ItemType.LuckyPick:
                        LuckyPickJson luckyPick = (LuckyPickJson)itemJson;                       
                        Dictionary<CurrencyType, int> currencyToAdd = new Dictionary<CurrencyType, int>();
                        string[] currencyList = luckyPick.currency.Split(';');
                        int length = currencyList.Length;
                        if (length > 0)
                        {
                            for (int i = 0; i < length; ++i)
                            {
                                string[] currencyStr = currencyList[i].Split('|');
                                if (currencyStr.Length == 2)
                                {
                                    int type = 0, amt = 0;
                                    if (int.TryParse(currencyStr[0], out type))
                                        if (int.TryParse(currencyStr[1], out amt))
                                        {
                                            CurrencyType currencyType = (CurrencyType)type;
                                            if (currencyToAdd.ContainsKey(currencyType))
                                                currencyToAdd[currencyType] += amt;
                                            else
                                                currencyToAdd[currencyType] = amt;
                                        }
                                }
                            }
                        }
                        Dictionary<int, int> itemsToAdd = new Dictionary<int, int>();
                        List<int> lootGrpIds = GameUtils.ParseStringToIntList(luckyPick.luckypickgroup, ';');
                        if (lootGrpIds.Count > 0)
                            LootRules.GenerateLootItems(lootGrpIds, itemsToAdd, currencyToAdd);
                        List<ItemInfo> itemInfoList = LootRules.GetItemInfoListToAdd(itemsToAdd, true);
                        AddItemsToInventoryMailIfFail(itemInfoList, currencyToAdd, "LuckyPick");
                        usedSuccess = true;
                        break;
                    case ItemType.MercenaryItem:
                        HeroItemJson heroItemJson = (HeroItemJson)itemJson;
                        int heroId;
                        if (int.TryParse(heroItemJson.heroid, out heroId) && heroId > 0)
                        {
                            if (heroItemJson.heroitemtype == HeroItemType.Gift && heroItemJson.ischangelike == 0)
                            {
                                mSlot.mPlayer.HeroStats.AddHeroTrust(heroId, item.ItemID, false);
                                usedSuccess = true;
                            }
                            else if (heroItemJson.heroitemtype == HeroItemType.HeroSkin)
                            {
                                mSlot.mPlayer.HeroStats.UnlockHeroSkin(heroId, item.ItemID);
                                usedSuccess = true;
                            }
                        }
                        break;
                    //case (ItemType.Currency):
                    //    var currencyItem = item as CurrencyItem;
                    //    if (mSlot.mPlayer.IsCurrencyAddable(currencyItem.CurrencyType, currencyItem.Amount * useAmount))
                    //    {
                    //        if (currencyItem.CurrencyType == CurrencyType.GuildContribution || currencyItem.CurrencyType == CurrencyType.GuildGold)
                    //        {
                    //            if (mSlot.mPlayer.SecondaryStats.guildId > 0)
                    //            {
                    //                usedSuccess = true;
                    //            }
                    //            else
                    //            {
                    //                retval.retCode = InvReturnCode.UseFailed;
                    //                usedSuccess = false;
                    //                break;
                    //            }
                    //        }
                    //        else
                    //            usedSuccess = true;

                    //        mSlot.mPlayer.AddCurrency(currencyItem.CurrencyType, currencyItem.Amount * useAmount, "CurrencyItem");
                    //    }
                    //    else
                    //    {
                    //        retval.retCode = InvReturnCode.MaxCurrency;
                    //        usedSuccess = false;
                    //    }
                    //    break;
                }
                if (usedSuccess)
                {
                    RemoveEmptyItem(retval.invSlot);                 
                    SyncInvData(retval.invSlot);
                    retval.retCode = InvReturnCode.UseSuccess;
                    LogItem(string.IsNullOrEmpty(from) ? "UseItem" : from, item.ItemID, useAmount, false);
                }
                else
                    RevertRemove(retval.invSlot);
            }
            else
                RevertRemove(retval.invSlot);

            return retval;
        }

        //*** For craft, upgrade, etc... ***//
        public InvRetval DeductItems(List<ItemInfo> items, string from)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.UseFailed;
            bool success = true;
            int count = items.Count;
            for (int index = 0; index < count; ++index)
            {
                ItemInfo iteminfo = items[index];
                if (!mInvData.HasItem(iteminfo.itemId, iteminfo.stackCount) || !DeductItem(iteminfo.itemId, -1, iteminfo.stackCount, ref retval))
                {
                    success = false;
                    break;
                }
            }

            if (success)
            {
                RemoveEmptyItem(retval.invSlot);
                SyncInvData(retval.invSlot);
                retval.retCode = InvReturnCode.UseSuccess;
                for (int index = 0; index < items.Count; ++index)
                    LogItem(from, items[index].itemId, items[index].stackCount, false);
            }
            else
                RevertRemove(retval.invSlot);

            return retval;
        }

        public InvRetval DeductItems(ushort itemId, int amount, string from)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.UseFailed;
            if (amount <= 0 || !mInvData.HasItem(itemId, amount))
                return retval;

            if (DeductItem(itemId, -1, amount, ref retval))
            {
                RemoveEmptyItem(retval.invSlot);
                SyncInvData(retval.invSlot);
                retval.retCode = InvReturnCode.UseSuccess;
                LogItem(from, itemId, amount, false);
            }
            else
                RevertRemove(retval.invSlot);

            return retval;
        }

        public InvRetval RemoveEquipmentItems(List<ushort> slotIds)
        {
            InvRetval retval = new InvRetval();
            foreach (ushort slotId in slotIds)
            {
                IInventoryItem item = mInvData.Slots[slotId];
                if (item == null)
                {
                    retval.retCode = InvReturnCode.RemoveFailed;
                    return retval;
                }
            }

            foreach (ushort slotId in slotIds)
            {
                IInventoryItem item = mInvData.Slots[slotId];
                LogItem("Equip", item.ItemID, item.StackCount, false);
                item.StackCount = 0;
                retval.SetInvSlot(slotId, 1);          
            }
            RemoveEmptyItem(retval.invSlot);
            SyncInvData(retval.invSlot);
            retval.retCode = InvReturnCode.RemoveSuccess;

            return retval;
        }

        // warehouse related
        public InvRetval RemoveInventoryItem(int slotId, string from)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.RemoveFailed;
            IInventoryItem item = mInvData.Slots[slotId];
            if (item != null)
            {
                int stackcount = item.StackCount;
                if (DeductItem(item.ItemID, slotId, stackcount, ref retval))
                {
                    RemoveEmptyItem(retval.invSlot);
                    SyncInvData(retval.invSlot);
                    retval.retCode = InvReturnCode.RemoveSuccess;
                    LogItem(from, item.ItemID, stackcount, false);
                }
                else
                    RevertRemove(retval.invSlot);
            }
            return retval;
        }

        private bool DeductItem(ushort itemId, int slotId, int stackCount, ref InvRetval retval)
        {
            if (stackCount == 0)
                return true;

            if (slotId >= 0)
            {
                RemoveItem(slotId, ref stackCount, ref retval);
            }
            else
            {
                Dictionary<int, IInventoryItem> invItems = mInvData.GetItemsByItemId(itemId); 
                foreach(var kvp in invItems)
                {
                    if (stackCount == 0)
                        return true;
                    RemoveItem(kvp.Key, ref stackCount, ref retval);
                }
            }

            return stackCount == 0; // Fail if stackcount > 0
        }

        private void RemoveItem(int slotId, ref int stackCount, ref InvRetval retval)
        {
            IInventoryItem invItem = mInvData.Slots[slotId];
            if (invItem == null)
                return;

            if (invItem.StackCount >= stackCount)
            {
                invItem.StackCount -= stackCount;
                retval.SetInvSlot(slotId, stackCount);
                stackCount = 0;
            }
            else
            {
                stackCount -= invItem.StackCount;
                retval.SetInvSlot(slotId, invItem.StackCount);
                invItem.StackCount = 0;
            }
        }
        
        public InvRetval MassSellItems(Dictionary<int, int> sellAmtToSlotIdDict)
        {
            InvRetval retval = new InvRetval();
            List<IInventoryItem> invSlots = mInvData.Slots;
            int totalSellAmount = 0;
            foreach (KeyValuePair<int, int> kvp in sellAmtToSlotIdDict)
            {
                int slotId = kvp.Key;
                IInventoryItem item = invSlots[slotId];
                if (item != null)
                {
                    int amount = kvp.Value;
                    if (!DeductItem(item.ItemID, slotId, amount, ref retval) || item.JsonObject.sellprice == -1)
                        RevertRemove(retval.invSlot);
                    else
                        totalSellAmount += item.JsonObject.sellprice * amount;
                }
            }

            if (!SellItemForMoney(totalSellAmount))
            {
                RevertRemove(retval.invSlot);
                retval.retCode = InvReturnCode.SellFailed;
                return retval;
            }
            else
            {
                //foreach (var item in items.Values)
                //{
                //    LogItem("Sell", item.ItemID, item.StackCount, false);
                //}
                RemoveEmptyItem(retval.invSlot);
                SyncInvData(retval.invSlot);
                retval.retCode = InvReturnCode.SellSuccess;
                return retval;
            }
        }

        public InvRetval MassUseCurrencyItems(List<int> slots,out Dictionary<CurrencyType, int> currency)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.UseSuccess;
            currency = new Dictionary<CurrencyType, int>();
            if (mSlot.mPlayer == null)
            {
                retval.retCode = InvReturnCode.UseFailed;
                return retval;
            }

            Dictionary<int, IInventoryItem> items = new Dictionary<int, IInventoryItem>();
            //checking
            foreach (int slotId in slots)
            {
                if (slotId < 0 || slotId > mInvData.Slots.Count)
                    continue; //skip

                IInventoryItem item = mInvData.Slots[slotId];
                if (item == null || item.ItemSortJson.bagtabtype != BagTabType.Consumable)
                {
                    retval.retCode = InvReturnCode.UseFailed;
                    return retval;
                }

                if (!CanUse(item, ref retval))
                    return retval;
                
                //var currencyItem = item as CurrencyItem;
                //if (currencyItem.CurrencyType == CurrencyType.GuildContribution)
                //{
                //    if (mSlot.mPlayer.SecondaryStats.guildId <= 0)
                //    {
                //        retval.retcode = InvReturnCode.UseFailed;
                //        return retval;
                //    }
                //}

                //var currAmt = 0;
                //if (currency.ContainsKey(currencyItem.CurrencyType))
                //    currAmt = currency[currencyItem.CurrencyType];
                //currAmt += currencyItem.Amount * currencyItem.StackCount;
                //if (mSlot.mPlayer.IsCurrencyAddable(currencyItem.CurrencyType, currAmt))
                //    currency[currencyItem.CurrencyType] = currAmt;
                //else
                //{
                //    retval.retcode = InvReturnCode.MaxCurrency;
                //    return retval;
                //}
            }

            //action
            bool success = true;
            foreach (int slotId in slots)
            {
                if (slotId < 0 || slotId > mInvData.Slots.Count)
                    continue;//skip
                var item = mInvData.Slots[slotId];
                if (!DeductItem(item.ItemID, slotId, item.StackCount, ref retval))
                {
                    success = false;
                    break;
                }
            }
            if (success)
            {
                RemoveEmptyItem(retval.invSlot);
                SyncInvData(retval.invSlot);
                retval.retCode = InvReturnCode.UseSuccess;

                foreach (var cur in currency)
                {
                    mSlot.mPlayer.AddCurrency(cur.Key, cur.Value, "CurrencyItem");
                }
            }
            else
            {
                RevertRemove(retval.invSlot);
                retval.retCode = InvReturnCode.UseFailed;
            }
            
            return retval;
        }

        public InvRetval SellItem(int slotId, ushort amount)
        {
            InvRetval retval = new InvRetval();
            IInventoryItem item = mInvData.Slots[slotId];
            if (item == null || item.StackCount < amount || item.JsonObject.sellprice == 0)
            {
                retval.retCode = InvReturnCode.SellFailed;
                return retval;
            }

            if (!DeductItem(item.ItemID, slotId, amount, ref retval) || !SellItemForMoney(item.JsonObject.sellprice * amount))
            {
                RevertRemove(retval.invSlot);
                retval.retCode = InvReturnCode.SellFailed;
                return retval;
            }
            else
            {
                LogItem("Sell", item.ItemID, amount, false);
                RemoveEmptyItem(retval.invSlot);
                SyncInvData(retval.invSlot);
                retval.retCode = InvReturnCode.SellSuccess;
                return retval;
            }
        }

        private bool CanUse(IInventoryItem item, ref InvRetval retval)
        {
            int lvl = mSlot.mPlayer.GetAccumulatedLevel();
            if (lvl < item.JsonObject.requirelvl)
            {
                retval.retCode = InvReturnCode.BelowLevel;
                return false;
            }
            //Use item daily + weekly limit
            if (mSlot.CharacterData.ItemLimitData.UseItem(item) == false)
            {
                return false;
            }
            return true;
        }

        public void InvSortItem()
        {
            mInvData.ItemInventorySorting();
            UpdateInvStats();
        }

        public void SyncInvData(Dictionary<int, int> items)
        {
            Player player = mSlot.mPlayer;
            if (player == null)
                return;

            List<IInventoryItem> itemSlots = mInvData.Slots;
            Dictionary<int, int>.KeyCollection SlotIds = items.Keys;
            foreach (int idx in SlotIds)
            {
                player.UpdateInventoryStats(idx, itemSlots[idx]);
            }
        }

        public void UpdateInvStats()
        {
            Player player = mSlot.mPlayer;
            if (player == null)
                return;

            List<IInventoryItem> itemSlots = mInvData.Slots;
            int count = itemSlots.Count;
            for (int idx = 0; idx < count; ++idx)
            {
                player.UpdateInventoryStats(idx, itemSlots[idx]);
            }
        }

        public void ClearItemInventory()
        {
            mInvData.ClearItemInventory();
            UpdateInvStats();
        }

        public void RemoveEmptyItem(Dictionary<int, int> items)
        {
            foreach (KeyValuePair<int, int> slot in items)
            {
                mInvData.ItemRemoveBySlotId(slot.Key, slot.Value);
            }
        }

        public void RevertAdd(Dictionary<int, int> items)
        {
            foreach (KeyValuePair<int, int> slot in items)
            {
                IInventoryItem oitem = mInvData.Slots[slot.Key];
                oitem.StackCount -= slot.Value;
                if (oitem.StackCount <= 0)
                    mInvData.SetSlotItem(slot.Key, null);
                else if(oitem.IsNew)
                    oitem.ResetNewItem();
            }
        }

        public void RevertRemove(Dictionary<int, int> items)
        {
            foreach (KeyValuePair<int, int> slot in items)
            {
                IInventoryItem invItem = mInvData.Slots[slot.Key];
                invItem.StackCount += slot.Value;
                //mInvData.SetSlotItem(slot.Key, oitem);
            }
        }

        private void SyncEquipmentData(Dictionary<int, ushort> items, bool isFashion)
        {
            Player player = mSlot.mPlayer;
            if (player == null)
                return;

            if (isFashion)
            {
                foreach (KeyValuePair<int, ushort> slot in items)
                    player.UpdateFashionStats(slot.Key, mEquipInvData.GetFashionSlot(slot.Key));
            }
            else
            {
                foreach (KeyValuePair<int, ushort> slot in items)
                    player.UpdateEquipmentStats(slot.Key, mEquipInvData.GetEquipmentBySlotId(slot.Key));
            }

            if (player.IsInParty())
                player.PartyStats.SetDirty(player.Name);
        }

        public UpdateRetval UpdateEquipmentProperties(ushort value, EquipPropertyType ptype, bool isEquipped, int slotID, ushort selection = ushort.MaxValue, List<int> buffList = null, bool isMultiSel = false, float currIncr = 0f, float nextIncr = 0f, List<int> reformSEAdd = null, List<int> recycleSERemove = null)
        {
            Equipment equipment = isEquipped ? mEquipInvData.Slots[slotID] as Equipment : mInvData.Slots[slotID] as Equipment;

            PlayerCombatStats combatStats = (PlayerCombatStats)mSlot.mPlayer.CombatStats;
            //combatStats.ComputeEquippedCombatStats(equipment, gemGroup, mSlot.mPlayer.EquipInvStats.selectedGemGroup, false); //CombatStats: take off equip
            if (ptype == EquipPropertyType.Upgrade)
                combatStats.ComputeEquippedCombatStats(equipment, currIncr, false);

            switch (ptype)
            {
                case EquipPropertyType.Upgrade:
                    equipment.UpgradeLevel = value;
                    equipment.EncodeItem();
                    break;
                case EquipPropertyType.Reform:
                    equipment.ReformStep = value;
                    equipment.AddSelection(value, selection);
                    equipment.EncodeItem();
                    break;
                case EquipPropertyType.Recycle:
                    equipment.RemoveSelection(equipment.ReformStep);
                    equipment.ReformStep = value;
                    equipment.EncodeItem();
                    break;
            }

            //switch (ptype)
            //{
            //    case EquipPropertyType.Upgrade:
            //        equipment.UpgradeLevel = value;
            //        equipment.BasicAttributeVal = string.IsNullOrEmpty(attributeVal) == false ? uint.Parse(attributeVal) : 0;
            //        equipment.EncodeItem();
            //        break;
            //    case EquipPropertyType.Surmount:
            //        equipment.SurmountLevel = value;
            //        equipment.BasicAttributeVal = string.IsNullOrEmpty(attributeVal) == false ? uint.Parse(attributeVal) : 0;
            //        equipment.EncodeItem();
            //        break;
            //    case EquipPropertyType.Slotting:
            //        if(!string.IsNullOrEmpty(gemAttribute))
            //        {
            //            List<ushort> slotGemIDs = equipment.DecodeGemIDs();
            //            List<ushort> slotGemAttributes = equipment.DecodeGemAttributes();
            //            int slotPos = (gemGrp * gemGrpSize) + gemSlot;
            //            int currentGemID = slotGemIDs[slotPos];
            //            int currentGemAttrib = slotGemAttributes[slotPos];
            //            if (currentGemID > 0 && currentGemAttrib > 0)
            //            {
            //                // Return slotted gem to inventory
            //                SocketGemItem gem = GameRules.GenerateItem(currentGemID, mSlot) as SocketGemItem;
            //                InvRetval res = AddItemsIntoInventory(gem, false, "GemUnequip");

            //                if (res.retcode == InvReturnCode.AddFailed)
            //                {
            //                    mSlot.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryAddFailed"), "", false, mSlot);

            //                    return UpdateRetval.AddFailed;
            //                }

            //                if (res.retcode == InvReturnCode.Full)
            //                {
            //                    mSlot.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mSlot);

            //                    return UpdateRetval.AddFailed;
            //                }
            //            }

            //            slotGemIDs[slotPos] = gemID;
            //            slotGemAttributes[slotPos] = /*(ushort)EquipmentRepo.GetIntBySideEffectGroupId(gemAttribute)*/ushort.Parse(gemAttribute);

            //            equipment.EncodeGemData(slotGemIDs, slotGemAttributes);
            //        }
            //        else
            //        {
            //            return UpdateRetval.SlottingFailed;
            //        }
            //        break;
            //    case EquipPropertyType.Unslotting:
            //        List<ushort> unslotGemIDs = equipment.DecodeGemIDs();
            //        List<ushort> unslotGemAttributes = equipment.DecodeGemAttributes();
            //        int unslotPos = (gemGrp * gemGrpSize) + gemSlot;
            //        SocketGemItem unslotGem1 = GameRules.GenerateItem(unslotGemIDs[unslotPos], mSlot) as SocketGemItem;
            //        InvRetval res1 = AddItemsIntoInventory(unslotGem1, false, "GemUnequip");

            //        if (res1.retcode == InvReturnCode.AddFailed)
            //        {
            //            mSlot.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryAddFailed"), "", false, mSlot);

            //            return UpdateRetval.AddFailed;
            //        }

            //        if (res1.retcode == InvReturnCode.Full)
            //        {
            //            mSlot.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_BagInventoryFull"), "", false, mSlot);

            //            return UpdateRetval.AddFailed;
            //        }

            //        unslotGemIDs[unslotPos] = 0;
            //        unslotGemAttributes[unslotPos] = 0;

            //        equipment.EncodeGemData(unslotGemIDs, unslotGemAttributes);
            //        break;
            //    case EquipPropertyType.ChangeGem:
            //        List<ushort> gemIDs = equipment.DecodeGemIDs();
            //        List<ushort> gemAttributes = equipment.DecodeGemAttributes();
            //        int changePos = (gemGrp * gemGrpSize) + gemSlot;
            //        SocketGemItem changeGem = GameRules.GenerateItem(gemIDs[changePos], mSlot) as SocketGemItem;

            //        gemIDs[changePos] = gemID;
            //        gemAttributes[changePos] = ushort.Parse(gemAttribute);

            //        equipment.EncodeGemData(gemIDs, gemAttributes);
            //        break;
            //}

            if (isEquipped)
            {
                Dictionary<int, ushort> updatedEquipment = new Dictionary<int, ushort>();
                updatedEquipment.Add(slotID, equipment.ItemID);

                SyncEquipmentData(updatedEquipment, false);
            }
            else
            {
                Dictionary<int, int> updatedEquipment = new Dictionary<int, int>();
                updatedEquipment.Add(slotID, equipment.ItemID);

                SyncInvData(updatedEquipment);
            }

            //combatStats.ComputeEquippedCombatStats(equipment, gemGroup, mSlot.mPlayer.EquipInvStats.selectedGemGroup, true);//CombatStats: add back
            if (ptype == EquipPropertyType.Upgrade)
                combatStats.ComputeEquippedCombatStats(equipment, nextIncr, true, buffList); //CombatStats: add back

            return UpdateRetval.Success;
        }

        private void UpdateEquipmentCombatStats(bool added, int invslot, int equipslot)
        {
            //if (mSlot.mPlayer == null)
            //    return;

            //EquipmentItem item;
            //if (added)
            //    item = mEquipInvData.GetItemBySlotId(equipslot);
            //else
            //    item = mInvData.GetItembySlotId(invslot) as EquipmentItem;

            ////IsEqReqFailedAtIndex return T if combatStats Had Alr Been Affected
            //var combatStatsHasAlrAffected = IsEqReqFailedAtIndex((EquipmentSlot)equipslot);

            ////dont update if stats had already been affected and removing equipement
            //if (added || !combatStatsHasAlrAffected)
            //{
            //    mSlot.mPlayer.UpdateEquipmentCombatStats(item, added);
            //}
        }

        public void NewDayReset()
        {
            mItemSortInvData.NewDayReset();
        }

        public ItemSortInventoryData GetItemSortInvData()
        {
            return mItemSortInvData;
        }

        private bool IsItemUnderCoolDown(int itemSort, int seconds)
        {
            IItemData data = mItemSortInvData.GetDataByItemSortId(itemSort);
            if (data == null)
                return false;

            return DateTime.Now < data.LastUsedt;
        }

        private void UpdateItemLastUsedDT(ushort itemtype, ushort itemId, bool global, int cooldowntime)
        {
            int idx;
            IItemData data;
            DateTime now = DateTime.Now.AddSeconds(cooldowntime);
            if (global)
            {
                data = mItemSortInvData.GetDataByItemSortId(itemtype);
                if (data == null)
                {
                    data = new IItemData(itemtype, 0, 0, now);
                    idx = mItemSortInvData.GetEmptySlot();
                }
                else
                {
                    data.LastUsedt = now;
                    idx = mItemSortInvData.GetIndexByItemSortId(itemtype);
                }
            }
            else
            {
                data = mItemSortInvData.GetDataByItemId(itemId);
                if (data == null)
                {
                    data = new IItemData(0, itemId, 0, now);
                    idx = mItemSortInvData.GetEmptySlot();
                }
                else
                {
                    data.LastUsedt = now;
                    idx = mItemSortInvData.GetIndexByItemId(itemId);
                }
            }
            mItemSortInvData.SetDataBySlotId(idx, data);
        }

        private bool SellItemForMoney(int amtToAdd)
        {
            if (mSlot.mPlayer == null)
                return false;

            mSlot.mPlayer.AddCurrency(CurrencyType.Money, amtToAdd, "SellItemForMoney");
            return true;
        }

        private void UpdateItemBoundStatus(List<Dictionary<int, ushort>> items)
        {
            //foreach (Dictionary<int, ushort> item in items)
            //{
            //    foreach (KeyValuePair<int, ushort> slot in item)
            //    {
            //        EquipmentItem equipitem = mEquipInvData.GetItemBySlotId(slot.Key);
            //        mEquipInvData.SetSlotItem(slot.Key, equipitem);
            //    }
            //}
        }

        public void OnUnlockedNewSlot(int newUnlockedSlot)
        {
            mInvData.OnUnlockedSlotUpdated(newUnlockedSlot);
            mSlot.CharacterData.ItemInventory.UnlockedSlotCount = newUnlockedSlot;
        }

        #region Equipment Requirement
        public void SyncEquipmentRequirement()
        {
            //called when Player join game,and when Player reset stats
            //mEquipmentRequirementsFailed is empty when player join game
            //for (int i = 0; i < (int)EquipmentSlot.MAXSLOTS; i++)
            //{
            //    if (mEquipInvData.Slots[i] != null)
            //    {
            //        SyncEquipmentRequirement((EquipmentSlot)i);
            //    }
            //}
        }
        
        public void SyncEquipmentRequirement(EquipmentSlot slotIndex)
        {
        }

        public void RemoveEquipmentRequirementsFailed(EquipmentSlot slotIndex)
        {
            mEquipmentRequirementsFailed.Remove(slotIndex);
        }

        public int GetEquipmentReqFailedCount()
        {
            return mEquipmentRequirementsFailed.Count;
        }

        public bool IsEqReqFailedAtIndex(EquipmentSlot slotIndex)
        {
            //if it contains the slot index means, can't wear that equipment at index
            return mEquipmentRequirementsFailed.Contains(slotIndex);
        }
        #endregion

        public void SetItemToHotbar(byte index, int slotId)
        {
            if (slotId >= 0)
            {
                IInventoryItem invItem = mInvData.Slots[slotId];
                if (invItem != null && invItem.ItemSortJson.bagtabtype == BagTabType.Consumable)
                    mSlot.mPlayer.UpdateItemHotbar(index, invItem.ItemID);
            }
            else // Remove from item hotbar slot
                mSlot.mPlayer.UpdateItemHotbar(index, 0);
        }

        public InvRetval UseItemInHotbar(byte index)
        {       
            int itemId = (int)mSlot.mPlayer.ItemHotbarStats.ItemHotbar[index];
            if (itemId == 0)
                return new InvRetval() { retCode = InvReturnCode.UseFailed };

            int slotId = mInvData.GetLeastStackCountSlotIdxByItemId((ushort)itemId);
            if (slotId == -1)
                return new InvRetval() { retCode = InvReturnCode.UseFailed };

            InvRetval retval = UseItemInInventory(slotId, 1);
            if (mInvData.GetLeastStackCountSlotIdxByItemId((ushort)itemId) == -1)
                mSlot.mPlayer.UpdateItemHotbar(index, 0);
            return retval;
        }

        public void UpdateEquipFusion(int slotID, string value)
        {
            Equipment equip = mInvData.Slots[slotID] as Equipment;
            equip.FusionEffect = value;
            equip.EncodeItem();
            Dictionary<int, int> refreshEquipment = new Dictionary<int, int>();
            refreshEquipment.Add(slotID, equip.JsonObject.itemid);
            SyncInvData(refreshEquipment);
        }

        private void UpdateCollectItemAchievement(int itemId, int itemCount)
        {
            IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(itemId);
            if (item != null)
            {
                ItemType itemType = item.JsonObject.itemtype;
                if (itemType == ItemType.Equipment)
                {
                    Equipment equipItem = item as Equipment;
                    if (equipItem != null && equipItem.EquipmentJson.fashionsuit)
                        mSlot.mPlayer.AchievementStats.UpdateCollection(CollectionType.Fashion, itemId);
                }
                else if (itemType == ItemType.Relic)
                {
                    mSlot.mPlayer.AchievementStats.UpdateCollection(CollectionType.Relic, itemId);
                }
            }

            mSlot.mPlayer.UpdateAchievement(AchievementObjectiveType.CollectItem, itemId.ToString(), false, itemCount);
        }
    }
}
