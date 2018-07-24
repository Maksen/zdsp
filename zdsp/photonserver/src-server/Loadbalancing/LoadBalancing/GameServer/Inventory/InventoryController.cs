using System;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.Rules;
using Zealot.Server.Entities;

public class InvRetval
{
    public InvReturnCode retCode;
    public Dictionary<int, int> invSlot = new Dictionary<int, int>();
    public Dictionary<int, ushort> equipSlot = new Dictionary<int, ushort>();

    public void SetInvSlot(int index, int stackCount)
    {
        if (stackCount == 0)
            return;
        if (invSlot.ContainsKey(index))
            invSlot[index] += stackCount;
        else
            invSlot.Add(index, stackCount);
    }
}

namespace Photon.LoadBalancing.GameServer
{
    public class ItemInventoryController
    {
        //private ItemUIDGenerator mUIDGenerator;
        public ItemInventoryServerData mInvData;
        private EquipmentInventoryData mEquipInvData;
        private ItemKindData mItemKindData;
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
            if (itemInvData.Slots.Capacity == 0)
                itemInvData.InitDefault();
            mInvData = new ItemInventoryServerData();
            mInvData.Init(itemInvData);

            mEquipInvData = characterData.EquipmentInventory;
            if (mEquipInvData.Slots.Capacity == 0)
                mEquipInvData.InitDefault();

            mItemKindData = characterData.ItemKindInv;
        }

        //public string GenerateItemUID()
        //{
        //    return mUIDGenerator.GenerateItemUID();
        //}

        public int GetEmptySlotCount()
        {
            return mInvData.GetEmptySlotCount();
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

        public InvRetval AddItemsIntoInventory(IInventoryItem item, bool isNew, string from)
        {
            InvRetval retval = new InvRetval();
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
                }
                else
                {
                    RevertAdd(retval.invSlot);
                    retval.retCode = InvReturnCode.AddFailed;
                }
            }
            else
            {
                retval.retCode = InvReturnCode.Full;
            }
            return retval;
        }

        public InvRetval AddItemsIntoInventory(List<IInventoryItem> items, bool isNew, string from)
        {
            InvRetval retval = new InvRetval();
            List<ItemInfo> additems = new List<ItemInfo>();
            foreach (IInventoryItem item in items)
            {
                ItemInfo _iteminfo = additems.Find(x => x.itemId == item.ItemID);
                if (_iteminfo != null)
                    _iteminfo.stackCount += item.StackCount;
                else
                    additems.Add(new ItemInfo() { itemId = item.ItemID, stackCount = item.StackCount });
            }
            bool canAdd = mInvData.CanAdd(additems);
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
                    for (int index = 0; index < items.Count; index++)
                    {
                        LogItem(from, items[index].ItemID, items[index].StackCount, true);
                    }
                    OnAddItems(retval.invSlot);
                    SyncInvData(retval.invSlot);
                    retval.retCode = InvReturnCode.AddSuccess;      
                }
                else
                {
                    RevertAdd(retval.invSlot);
                    retval.retCode = InvReturnCode.AddFailed;
                }
            }
            else
            {
                retval.retCode = InvReturnCode.Full;
            }
            return retval;
        }

        public InvRetval AddItemsIntoInventory(ushort itemid, int stackcount, bool isNew, string from)
        {
            InvRetval retval = new InvRetval();
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
                }
                else
                {
                    RevertAdd(retval.invSlot);
                    retval.retCode = InvReturnCode.AddFailed;
                }
            }
            else
            {
                retval.retCode = InvReturnCode.Full;
            }
            return retval;
        }

        public InvRetval AddItemsIntoInventory(List<ItemInfo> items, bool isNew, string from)
        {
            InvRetval retval = new InvRetval();
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
                    for (int index = 0; index < items.Count; ++index)
                    {
                        LogItem(from, items[index].itemId, items[index].stackCount, true);
                    }
                    OnAddItems(retval.invSlot);
                    SyncInvData(retval.invSlot);
                    retval.retCode = InvReturnCode.AddSuccess;
                }
                else
                {
                    RevertAdd(retval.invSlot);
                    retval.retCode = InvReturnCode.AddFailed;
                }
            }
            else
            {
                retval.retCode = InvReturnCode.Full;
            }
            return retval;
        }

        private bool AddItems(ushort itemId, int stackCount, ref InvRetval retval, bool isNew = true)
        {
            int added;
            bool slotexist = true;
            int index;
            ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(itemId);
            if (itemJson == null)
                return false;
            if (itemJson.bagtype != BagType.Equipment)
            {
                while (stackCount > 0 && slotexist)
                {
                    index = mInvData.GetAvailableSlotsByItemId(itemId);
                    if (index == -1)
                        slotexist = false;
                    else
                    {
                        added = 0;
                        AddItem(index, ref stackCount, ref added, isNew);
                        retval.SetInvSlot(index, added);
                    }
                }
            }

            bool emptyslot = mInvData.GetEmptySlotCount() > 0;
            while (stackCount > 0 && emptyslot)
            {
                index = mInvData.GetEmptySlotIndex();
                if (index == -1)
                    emptyslot = false;
                else
                {
                    IInventoryItem newitem = GameRules.GenerateItem(itemId, mSlot, 1);
                    if (newitem == null)
                        return false;

                    if (newitem.MaxStackCount >= stackCount)
                    {
                        newitem.StackCount = stackCount;
                        if (isNew)
                            newitem.SetNewItem();
                        stackCount = 0;
                    }
                    else
                    {
                        newitem.StackCount = newitem.MaxStackCount;
                        if (isNew)
                            newitem.SetNewItem();
                        stackCount -= newitem.MaxStackCount;
                    }
                    mInvData.SetSlotItem(index, newitem);
                    retval.SetInvSlot(index, newitem.StackCount);
                }
            }

            if (stackCount > 0)
                return false;
            else
                return true;
        }

        private bool AddItems(IInventoryItem item, ref InvRetval retval, bool isNew = true)
        {
            Dictionary<int, IInventoryItem> slots = mInvData.GetStackableSlot(item.ItemID);
            if (slots.Count == 0)
            {
                if (mInvData.GetEmptySlotCount() > 0)
                {
                    int index = mInvData.GetEmptySlotIndex();
                    if (index == -1)
                        return false;
                    if (isNew)
                        item.SetNewItem();
                    mInvData.SetSlotItem(index, item);
                    retval.SetInvSlot(index, item.StackCount);
                }
            }
            else
            {
                int added = 0;
                int stackcount = item.StackCount;
                foreach (KeyValuePair<int, IInventoryItem> slot in slots)
                {
                    int index = slot.Key;
                    IInventoryItem oitem = slot.Value;
                    ushort available = (ushort)(oitem.MaxStackCount - oitem.StackCount);
                    if (available >= stackcount)
                    {
                        oitem.StackCount += stackcount;
                        added = stackcount;
                        if (isNew)
                            oitem.SetNewItem();
                        stackcount = 0;
                    }
                    else
                    {
                        oitem.StackCount += available;
                        added = available;
                        if (isNew)
                            oitem.SetNewItem();
                        stackcount -= available;
                    }                    
                    retval.SetInvSlot(index, added);

                    if (stackcount == 0)
                        break;
                }

                if (stackcount > 0 && mInvData.GetEmptySlotCount() > 0)
                {
                    int index = mInvData.GetEmptySlotIndex();
                    if (index == -1)
                        return false;

                    item.StackCount = stackcount;
                    mInvData.SetSlotItem(index, item);
                    retval.SetInvSlot(index, item.StackCount);
                }
            }
            return true;
        }

        private void AddItem(int slotId, ref int stackcount, ref int added, bool isNew = true)
        {
            IInventoryItem item = mInvData.Slots[slotId];
            if (item == null)
                return;

            if (item.StackCount < item.MaxStackCount)
            {
                ushort remain = (ushort)(item.MaxStackCount - item.StackCount);
                if (remain >= stackcount)
                {
                    item.StackCount += stackcount;
                    added = stackcount;
                    stackcount = 0;
                }
                else
                {
                    item.StackCount += remain;
                    added = remain;
                    stackcount -= remain;
                }
                if (isNew)
                    item.SetNewItem();
            }
        }

        public InvRetval SwapEquipmentToInventory(int slotid)
        {
            InvRetval retval = new InvRetval();
            Equipment equipitem = mEquipInvData.GetEquipmentBySlotId(slotid);
            if (equipitem == null)
            {
                retval.retCode = InvReturnCode.UnEquipFailed;
                return retval;
            }
            else
            {
                int index = mInvData.GetEmptySlotIndex();
                if (index == -1)
                {
                    retval.retCode = InvReturnCode.Full;
                    return retval;
                }
                else
                {
                    mEquipInvData.SetEquipmentToSlot(slotid, null);
                    retval.equipSlot[slotid] = 1;

                    mInvData.SetSlotItem(index, equipitem);
                    retval.SetInvSlot(index, 1);

                    SyncInvData(retval.invSlot);
                    SyncEquipmentData(retval.equipSlot, false);
                    UpdateEquipmentCombatStats(false, index, slotid);
                    retval.retCode = InvReturnCode.UnEquipSuccess;
                }
            }

            return retval;
        }

        public InvRetval SwapEquipmentFromInventory(int slotId)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.EquipFailed;

            Equipment equipitem = mInvData.Slots[slotId] as Equipment;
            if (equipitem == null || equipitem.EquipmentJson.fashionsuit)
                return retval;

            EquipmentSlot _equipSlot = mEquipInvData.FindAEquipSlotToSwap(equipitem);
            if (_equipSlot == EquipmentSlot.MAXSLOTS)
                return retval;

            int equipSlot = (int)_equipSlot;
            Equipment destItem = mEquipInvData.GetEquipmentBySlotId(equipSlot);
            if (destItem != null)
                mInvData.SetSlotItem(slotId, destItem);
            else
                mInvData.SetSlotItem(slotId, null);
            retval.SetInvSlot(slotId, 1);

            mEquipInvData.SetEquipmentToSlot(equipSlot, equipitem);
            retval.equipSlot[equipSlot] = 1;

            SyncInvData(retval.invSlot);
            SyncEquipmentData(retval.equipSlot, false);
            UpdateEquipmentCombatStats(true, slotId, equipSlot);
            retval.retCode = InvReturnCode.EquipSuccess;

            return retval;
        }

        public InvRetval SwapFashionToInventory(int slotId)
        {
            InvRetval retval = new InvRetval();
            Equipment equipitem = mEquipInvData.GetFashionSlot(slotId);
            if (equipitem == null)
            {
                retval.retCode = InvReturnCode.UnEquipFailed;
                return retval;
            }
            else
            {
                int index = mInvData.GetEmptySlotIndex();
                if (index == -1)
                {
                    retval.retCode = InvReturnCode.Full;
                    return retval;
                }
                else
                {
                    mEquipInvData.SetFashionToSlot(slotId, null);
                    retval.equipSlot[slotId] = 1;

                    mInvData.SetSlotItem(index, equipitem);
                    retval.SetInvSlot(index, 1);

                    SyncInvData(retval.invSlot);
                    SyncEquipmentData(retval.equipSlot, true);
                    retval.retCode = InvReturnCode.UnEquipSuccess;
                }
            }

            return retval;
        }

        public InvRetval SwapFashionFromInventory(int slotId)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.EquipFailed;

            Equipment equipitem = mInvData.Slots[slotId] as Equipment;
            if (equipitem == null)
                return retval;

            FashionSlot _equipSlot = InventoryHelper.GetFashionSlotByPartType(equipitem.EquipmentJson.partstype);
            if (_equipSlot == FashionSlot.MAXSLOTS)
                return retval;

            int equipSlot = (int)_equipSlot;
            Equipment destItem = mEquipInvData.GetFashionSlot(equipSlot);
            if (destItem != null)
                mInvData.SetSlotItem(slotId, destItem);
            else
                mInvData.SetSlotItem(slotId, null);
            retval.SetInvSlot(slotId, 1);

            mEquipInvData.SetFashionToSlot(equipSlot, equipitem);
            retval.equipSlot[equipSlot] = 1;

            SyncInvData(retval.invSlot);
            SyncEquipmentData(retval.equipSlot, true);
            retval.retCode = InvReturnCode.EquipSuccess;

            return retval;
        }

        public InvRetval UseItemInInventory(int slotId, ushort useAmount)
        {
            InvRetval retval = new InvRetval();
            IInventoryItem item = mInvData.Slots[slotId];
            if (item == null || mSlot.mPlayer == null || item.StackCount < useAmount)
            {
                retval.retCode = InvReturnCode.UseFailed;
                return retval;
            }

            if (!CanUse(item, ref retval))
                return retval;

            switch (item.JsonObject.itemtype)
            {
                case ItemType.Equipment:
                    retval = useAmount == 0 ? SwapFashionFromInventory(slotId) : SwapEquipmentFromInventory(slotId);
                    break;       
                case ItemType.PotionFood:
                //    Player player = mSlot.mPlayer;
                //    int health = player.GetHealth();
                //    if (health > 0)
                //    {
                //        if (health == player.GetHealthMax())
                //            retval.retcode = InvReturnCode.MaxHp;
                //        else
                //        {
                //            var potionItem = item as PotionItem;
                //            bool iscd = IsItemUnderCoolDown(potionItem.ItemType, potionItem.CooldownInSec);
                //            if (iscd)
                //                retval.retcode = InvReturnCode.ItemCoolDown;
                //            else
                //            {
                //                useAmount = 1; //potion got cooldown.
                                retval = UseItems(slotId, useAmount);
                //            }
                //        }
                //    }
                //    else
                //        retval.retcode = InvReturnCode.UseFailed;
                    break;
                default:
                    {
                        retval.retCode = InvReturnCode.UseFailed;
                        break;
                    }
            }

            return retval;
        }

        //*** From Inventory Bag ***//
        public InvRetval UseItems(int slotId, ushort useAmount)
        {
            InvRetval retval = new InvRetval();
            IInventoryItem item = mInvData.Slots[slotId];
            if (item == null || item.JsonObject.bagtype != BagType.Consumable || mSlot.mPlayer == null || item.StackCount < useAmount)
            {
                retval.retCode = InvReturnCode.UseFailed;
                return retval;
            }

            if (!CanUse(item, ref retval))
                return retval;

            //if (item.ItemType == ItemType.OpenWindow)
            //{
            //    retval.retcode = InvReturnCode.UseSuccess;
            //    mSlot.ZRPC.CombatRPC.OpenUIWindow((byte)(item as OpenWindowItem).linkUI, slotId, mSlot);
            //    return retval;
            //}
            //else if (item.ItemType == ItemType.SocketGemItem)
            //{
            //    retval.retcode = InvReturnCode.UseSuccess;
            //    mSlot.ZRPC.CombatRPC.OpenUIWindow((byte)LinkUIType.Equipment_Gem_Equip, -1,mSlot);
            //    return retval;
            //}

            bool success = true;
            if (!DeductItems(item.ItemID, slotId, useAmount, ref retval))
                success = false;
            if (success)
            {
                bool used = false;
                switch (item.JsonObject.itemtype)
                {
                    case ItemType.PotionFood:
                //        var potionItem = item as PotionItem;
                //        int healAbsolute = mSlot.mPlayer.AddHealthPercentage(potionItem.Heal);
                //        int cooldownReduction = (int)mSlot.mPlayer.SkillPassiveStats.GetField(SkillPassiveFieldName.Potion_CD);
                //        float realcooldown = potionItem.CooldownInSec * (1 - cooldownReduction * 0.01f); //really need a varaiant cooldown for different item at Server?
                //        UpdateItemLastUsedDT(potionItem.ItemType, potionItem.ItemID, true, (int)realcooldown);
                        used = true;

                //        //Tell player about healing
                //        {
                //            AttackResult ar = new AttackResult(mSlot.mPlayer.GetPersistentID(), healAbsolute);
                //            ar.IsHeal = true;
                //            mSlot.mPlayer.QueueDmgResult(ar);
                //        }
                        break;
                //    case (ItemType.Currency):
                //        var currencyItem = item as CurrencyItem;
                //        if (mSlot.mPlayer.IsCurrencyAddable(currencyItem.CurrencyType, currencyItem.Amount * useAmount))
                //        {
                //            if(currencyItem.CurrencyType == CurrencyType.GuildContribution || currencyItem.CurrencyType == CurrencyType.GuildGold)
                //            {
                //                if (mSlot.mPlayer.SecondaryStats.guildId > 0)
                //                {
                //                    used = true;
                //                }
                //                else
                //                {
                //                    retval.retcode = InvReturnCode.UseFailed;
                //                    used = false;
                //                    break;
                //                }
                //            }
                //            else
                //                used = true;

                //            mSlot.mPlayer.AddCurrency(currencyItem.CurrencyType, currencyItem.Amount * useAmount, "CurrencyItem");
                //        }
                //        else
                //        {
                //            retval.retcode = InvReturnCode.MaxCurrency;
                //            used = false;
                //        }
                //        break;
                //    case (ItemType.Ticket):
                //        var ticketItem = item as TicketItem;
                //        if (ticketItem.realmType == RealmType.ActivityGuildSMBoss)
                //            mSlot.mPlayer.SecondaryStats.GuildSMBossExtraEntry += ticketItem.amountAdded * useAmount;
                //        else
                //            mSlot.mPlayer.RealmStats.AddExtraEntry(ticketItem.realmType, ticketItem.sequence, ticketItem.amountAdded * useAmount, ticketItem.dungeonType);

                //        int count = ticketItem.amountAdded * useAmount;
                //        Dictionary<string, string> dic = new Dictionary<string, string>();
                //        string instanceName = RealmRepo.GetDungeonName(ticketItem.realmType, ticketItem.sequence, ticketItem.dungeonType);
                //        dic.Add("name", instanceName);
                //        dic.Add("num", count.ToString());
                //        mSlot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_TicketAddTime", GameUtils.FormatString(dic), false, mSlot);
                //        used = true;
                //        break;
                //    case (ItemType.GuildQuest):
                //        var GuildQuestItem = item as GuildQuestItem;
                //        mSlot.mPlayer.AddGuildQuestRefreshTimes(GuildQuestItem.amountAdded * useAmount);
                //        used = true;
                //        break;
                //    case (ItemType.PetItem):
                //        used = true;
                //        break;
                //    case (ItemType.PetEgg):
                //        PetEgg petegg = item as PetEgg;
                //        if (petegg != null)
                //            used = mSlot.mPlayer.mPet.AddPet(petegg.petid, useAmount);
                //        if (!used)
                //            retval.retcode = InvReturnCode.PetBagFull;
                //        break;
                }
                if (used)
                {
                    RemoveEmptyItem(retval.invSlot);
                    LogItem("UseItem", item.ItemID, useAmount, false);
                    SyncInvData(retval.invSlot);
                    retval.retCode = InvReturnCode.UseSuccess;
                }
                else
                {
                    RevertRemove(retval.invSlot);
                }
            }
            else
            {
                RevertRemove(retval.invSlot);
                retval.retCode = InvReturnCode.UseFailed;
            }
            return retval;
        }

        public InvRetval DeductItem(ushort itemId, ushort useAmount, string from)
        {      
            InvRetval retval = new InvRetval();
            if (useAmount <= 0)
            {
                retval.retCode = InvReturnCode.UseSuccess;
                return retval;
            }
            retval.retCode = InvReturnCode.UseFailed;
            IInventoryItem item = mInvData.GetItemByItemId(itemId);
            if (item != null)
            {
                if (item.JsonObject.bagtype == BagType.Consumable)
                    retval = UseToolItems(itemId, useAmount, from);
                else
                {
                    if (mInvData.HasItem(itemId, useAmount))
                    {
                        if (!DeductItems(itemId, -1, useAmount, ref retval))
                            RevertRemove(retval.invSlot);
                        else
                        {
                            RemoveEmptyItem(retval.invSlot);
                            LogItem(from, itemId, useAmount, false);
                            SyncInvData(retval.invSlot);
                            retval.retCode = InvReturnCode.UseSuccess;
                        }
                    }
                }
            }
            return retval;
        }

        //*** For craft, upgrade, et... ***//
        public InvRetval UseToolItems(List<ItemInfo> items, string from)
        {
            InvRetval retval = new InvRetval();
            bool success = true;
            for (int index = 0; index < items.Count; index++)
            {
                ItemInfo iteminfo = items[index];
                if (!mInvData.HasItem(iteminfo.itemId, iteminfo.stackCount) || !DeductItems(iteminfo.itemId, -1, iteminfo.stackCount, ref retval))
                {
                    success = false;
                    break;
                }
            }

            if (success)
            {
                RemoveEmptyItem(retval.invSlot);
                for (int index = 0; index < items.Count; index++)
                {
                    LogItem(from, items[index].itemId, items[index].stackCount, false);
                }
                SyncInvData(retval.invSlot);
                retval.retCode = InvReturnCode.UseSuccess;
            }
            else
            {
                RevertRemove(retval.invSlot);
                retval.retCode = InvReturnCode.UseFailed;
            }

            return retval;
        }

        public InvRetval UseToolItems(ushort itemId, ushort useAmount, string from)
        {
            InvRetval retval = new InvRetval();
            retval.retCode = InvReturnCode.UseFailed;
            if (!mInvData.HasItem(itemId, useAmount))
                return retval;

            if (!DeductItems(itemId, -1, useAmount, ref retval))
                RevertRemove(retval.invSlot);
            else
            {
                RemoveEmptyItem(retval.invSlot);
                LogItem(from, itemId, useAmount, false);
                SyncInvData(retval.invSlot);
                retval.retCode = InvReturnCode.UseSuccess;
            }

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

        //warehouse related
        public InvRetval RemoveInvItem(int slotId, string from)
        {
            InvRetval retval = new InvRetval();
            IInventoryItem item = mInvData.Slots[slotId];
            if (item != null)
            {
                int stackcount = item.StackCount;
                if (DeductItems(item.ItemID, slotId, stackcount, ref retval))
                {
                    LogItem(from, item.ItemID, stackcount, false);
                    RemoveEmptyItem(retval.invSlot);
                    SyncInvData(retval.invSlot);
                    retval.retCode = InvReturnCode.RemoveSuccess;
                }
                else
                {
                    RevertRemove(retval.invSlot);
                    retval.retCode = InvReturnCode.RemoveFailed;
                }
            }         
            return retval;
        }

        public bool DeductItems(ushort itemId, int slotId, int stackCount, ref InvRetval retval)
        {
            if (stackCount == 0)
                return true;

            int removed = 0;
            if (slotId >= 0)
            {
                RemoveItem(slotId, ref stackCount, ref removed);
                retval.SetInvSlot(slotId, removed);
            }
            else
            {
                int index = 0;
                Dictionary<int, IInventoryItem> stackableSlot = mInvData.FindItemByItemId(itemId); 
                foreach(var kvp in stackableSlot)
                {
                    index = kvp.Key;
                    removed = 0;
                    RemoveItem(index, ref stackCount, ref removed);
                    retval.SetInvSlot(index, removed);
                    if (stackCount == 0)
                        return true;
                }
            }
            return stackCount == 0;
        }

        public void RemoveItem(int slotId, ref int stackCount, ref int removed)
        {
            IInventoryItem item = mInvData.Slots[slotId];
            if (item == null)
                return;

            if (item.StackCount >= stackCount)
            {
                item.StackCount -= stackCount;
                removed = stackCount;
                stackCount = 0;
            }
            else
            {
                stackCount -= item.StackCount;
                removed = item.StackCount;
                item.StackCount = 0;
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
                    if (!DeductItems(item.ItemID, slotId, amount, ref retval))
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
                    continue;//skip

                var item = mInvData.Slots[slotId];
                if (item == null || item.JsonObject.bagtype != BagType.Consumable/* || item.itemtype != ItemType.Currency*/)
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
                if (!DeductItems(item.ItemID, slotId, item.StackCount, ref retval))
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

        public UpdateRetval UpdateEquipmentProperties(ushort value, EquipPropertyType ptype, bool isEquipped, int slotID, List<int> buffList = null, bool isMultiSel = false, float currIncr = 0f, float nextIncr = 0f)
        {
            Equipment equipment = isEquipped ? mEquipInvData.Slots[slotID] as Equipment : mInvData.Slots[slotID] as Equipment;

            PlayerCombatStats combatStats = (PlayerCombatStats)mSlot.mPlayer.CombatStats;
            //combatStats.ComputeEquippedCombatStats(equipment, gemGroup, mSlot.mPlayer.EquipInvStats.selectedGemGroup, false); //CombatStats: take off equip
            if(ptype == EquipPropertyType.Upgrade)
                combatStats.ComputeEquippedCombatStats(equipment, currIncr, false);

            switch(ptype)
            {
                case EquipPropertyType.Upgrade:
                    equipment.UpgradeLevel = value;
                    equipment.EncodeItem();
                    break;
                case EquipPropertyType.Reform:
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
        
        public InvRetval SellItem(int slotId, ushort amount)
        {
            InvRetval retval = new InvRetval();
            IInventoryItem item = mInvData.Slots[slotId];
            if (item == null || item.StackCount < amount || item.JsonObject.sellprice == 0)
            {
                retval.retCode = InvReturnCode.SellFailed;
                return retval;
            }

            if (!DeductItems(item.ItemID, slotId, amount, ref retval) || !SellItemForMoney(item.JsonObject.sellprice * amount))
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
            foreach (KeyValuePair<int, int> slot in items)
            {
                player.UpdateInventoryStats(slot.Key, itemSlots[slot.Key]);
            }
        }

        public void ClearItemInventory()
        {
            mInvData.ClearItemInventory();
            UpdateInvStats();
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

        public void NewDayReset()
        {
            mItemKindData.NewDayReset();
        }

        public ItemKindData GetItemKindData()
        {
            return mItemKindData;
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
                IInventoryItem oitem = mInvData.Slots[slot.Key];
                oitem.StackCount += slot.Value;
                //mInvData.SetSlotItem(slot.Key, oitem);
            }
        }

        public void RemoveEmptyItem(Dictionary<int, int> items)
        {
            foreach (KeyValuePair<int, int> slot in items)
            {
                mInvData.ItemRemoveBySlotId(slot.Key, slot.Value);
            }
        }

        public void OnAddItems(Dictionary<int, int> items)
        {
            foreach (KeyValuePair<int, int> slot in items)
            {
                mInvData.OnAddItem(slot.Key, slot.Value);
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

        private bool IsItemUnderCoolDown(int itemKind, int seconds)
        {
            IItemData data = mItemKindData.GetDataByItemKindId(itemKind);
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
                data = mItemKindData.GetDataByItemKindId(itemtype);
                if (data == null)
                {
                    data = new IItemData(itemtype, 0, 0, now);
                    idx = mItemKindData.GetEmptySlot();
                }
                else
                {
                    data.LastUsedt = now;
                    idx = mItemKindData.GetIndexByItemKindId(itemtype);
                }
            }
            else
            {
                data = mItemKindData.GetDataByItemId(itemId);
                if (data == null)
                {
                    data = new IItemData(0, itemId, 0, now);
                    idx = mItemKindData.GetEmptySlot();
                }
                else
                {
                    data.LastUsedt = now;
                    idx = mItemKindData.GetIndexByItemId(itemId);
                }
            }
            mItemKindData.SetDataBySlotId(idx, data);
        }

        private bool SellItemForMoney(int amtToAdd)
        {
            if (mSlot.mPlayer == null)
                return false;

            mSlot.mPlayer.AddCurrency(CurrencyType.Money, amtToAdd, "SellItemForMoney");
            return true;
        }

        private bool CanUse(IInventoryItem item, ref InvRetval retval)
        {
            int lvl = mSlot.mPlayer.GetAccumulatedLevel();
            if (lvl < item.JsonObject.requirelvl)
            {
                retval.retCode = InvReturnCode.BelowLevel;
                return false;
            }

            //if (lvl >= item.LimitLvl && item.LimitLvl != 0)
            //{
            //    retval.retcode = InvReturnCode.OverLevel;
            //    return false;
            //}
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

        public void SetItemToHotbar(byte idx, int slotId)
        {
            if (slotId >= 0)
            {
                IInventoryItem invItem = mInvData.Slots[slotId];
                if (invItem != null)
                {
                    ItemType itemType = invItem.JsonObject.itemtype;
                    if (itemType == ItemType.PotionFood || (itemType == ItemType.Material && ((MaterialJson)invItem.JsonObject).mattype == MaterialType.Special))
                        mSlot.mPlayer.UpdateItemHotbar(idx, invItem.ItemID);
                }
            }
            else // Remove from item hotbar slot
                mSlot.mPlayer.UpdateItemHotbar(idx, 0);
        }

        public void UseItemInHotbar(byte index)
        {
            int itemId = (int)mSlot.mPlayer.ItemHotbarStats.ItemHotbar[index];
            if (itemId != 0)
            {
                int slotId = mInvData.GetLeastStackCountSlotIdxByItemId((ushort)itemId);
                if (slotId != -1)
                    UseItemInInventory(slotId, 1);
            }
        }
    }
}
