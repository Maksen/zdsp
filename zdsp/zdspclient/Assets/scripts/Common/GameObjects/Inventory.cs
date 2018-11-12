using Kopio.JsonContracts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Zealot.Repository;

namespace Zealot.Common
{
    // Corresponds to array index in equipment inventory
    public enum EquipmentSlot
    {
        Helm,
        Body,
        Back,
        Neck,
        Weapon,
        Boots,
        Accessory1,
        Accessory2,
        Ring1,
        Ring2,

        MAXSLOTS
    }

    public enum FashionSlot
    {
        Helm,
        Body,
        Weapon,
        Back,
        Bathrobe,

        MAXSLOTS
    }

    public enum AppearanceSlot
    {
        HairStyle,
        HairColor,
        MakeUp,
        SkinColor,

        MAXSLOTS
    }

    public enum EquipmentSlotType
    {
        Equipped,
        Bag,
    }

    public enum EquipmentPropertyType
    {
        Upgrade = 0,
        Plus,
        UpTier,
        FosterRequestValue,
        Foster,
        FosterUpdateMax,
        FosterRemoveMax,
    }

    public enum EquipPropertyType
    {
        Upgrade = 0,
        Reform,
        Recycle,
    }

    public enum InventorySlot
    {
        COLLECTION_SIZE = 100,
        MAXSLOTS = 200
    }

    public enum InvReturnCode
    {
        Full,
        AddSuccess,
        AddFailed,
        UseFailed,
        UseSuccess,
        ItemCoolDown,
        UnEquipFailed,
        UnEquipSuccess,
        EquipFailed,
        EquipSuccess,
        SellFailed,
        SellSuccess,
        RemoveFailed,
        RemoveSuccess,
        MaxHp,
        MaxCurrency,
        OverLevel,
        BelowLevel,
    }

    public enum OpenSlotRetCode
    {
        Success,
        Fail_Gold,
        Fail_AutoOpen,
        Fail_AllOpened,
    }

    public enum UsingChestCode
    {
        Fail,
        Success,
        NotEnoughSpace,
        WrongKey
    }

    public class ItemInfo
    {
        public ushort itemId;
        public int stackCount;
    }

    public class CurrencyInfo
    {
        public CurrencyType currencyType;
        public int amount;
    }

    public class ItemSlotInfo
    {
        public int stackTotal;
        public List<int> slotIds;

        public ItemSlotInfo()
        {
            stackTotal = 0;
            slotIds = new List<int>();
        }

        public void Add(int slotid, int stack)
        {
            int count = slotIds.Count;
            for (int index = 0; index < count; ++index)
            {
                int _slotid = slotIds[index];
                if (slotid < _slotid)
                {
                    slotIds.Insert(index, slotid);
                    stackTotal += stack;
                    return;
                }
                else if (slotid == _slotid)
                {
                    stackTotal += stack;
                    return;
                }
            }
            slotIds.Add(slotid);
            stackTotal += stack;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ItemInventoryData
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "unlockslotcnt")]
        public int UnlockedSlotCount { get; set; }

        [JsonProperty(PropertyName = "slots")]
        public List<IInventoryItem> Slots = new List<IInventoryItem>();

        public int NumSlots { get { return Slots.Count; } }
        protected int maxSlotsSize = (int)InventorySlot.MAXSLOTS;

        public void InitDefault()
        {
            Slots = new List<IInventoryItem>(new IInventoryItem[maxSlotsSize]);
            if (UnlockedSlotCount == 0)
                UnlockedSlotCount = GameConstantRepo.ItemInvStartingSlotCount;
        }

        public void ValidateDefault()
        {
            if (Slots.Count != maxSlotsSize)
            {
                List<IInventoryItem> OldSlots = Slots;
                InitDefault();
                for (int i = 0; i < maxSlotsSize; ++i)
                {
                    if (i == OldSlots.Count)
                        break;
                    Slots[i] = OldSlots[i];
                }
            }
        }

        public virtual void ClearItemInventory()
        {
            int count = Slots.Count;
            for (int i = 0; i < count; ++i)
                Slots[i] = null;
        }

        public Dictionary<int, IInventoryItem> GetItemsByItemType(ItemType type)
        {
            Dictionary<int, IInventoryItem> ret = new Dictionary<int, IInventoryItem>();
            for (int index = 0; index < UnlockedSlotCount; ++index)
            {
                IInventoryItem element = Slots[index];
                if (element != null && element.JsonObject.itemtype == type)
                    ret.Add(index, element);
            }
            return ret;
        }

        public virtual IInventoryItem GetItemByItemId(ushort itemId)
        {
            for (int index = 0; index < UnlockedSlotCount; ++index)
            {
                IInventoryItem element = Slots[index];
                if (element != null && element.ItemID == itemId)
                    return element;
            }
            return null;
        }

        public virtual Dictionary<int, IInventoryItem> GetItemsByItemId(ushort itemId)
        {
            Dictionary<int, IInventoryItem> ret = new Dictionary<int, IInventoryItem>();
            for (int index = 0; index < UnlockedSlotCount; ++index)
            {
                IInventoryItem element = Slots[index];
                if (element != null && element.ItemID == itemId)
                    ret.Add(index, element);
            }
            return ret;
        }

        public bool IsItemExist(int slotIdx, ushort itemId)
        {
            return Slots[slotIdx] != null && Slots[slotIdx].ItemID == itemId;
        }

        public virtual int GetTotalStackCountByItemId(ushort itemId)
        {
            int count = 0;
            for (int index = 0; index < UnlockedSlotCount; ++index)
            {
                IInventoryItem element = Slots[index];
                if (element != null && element.ItemID == itemId)
                    count += element.StackCount;
            }
            return count;
        }

        public virtual bool HasItem(ushort itemId, int stackCount)
        {
            if (stackCount <= 0)
                return true;
            int count = 0;
            for (int index = 0; index < UnlockedSlotCount; ++index)
            {
                IInventoryItem element = Slots[index];
                if (element != null && element.ItemID == itemId)
                {
                    count += element.StackCount;
                    if (count >= stackCount)
                        return true;
                }
            }
            return false;
        }

        public virtual bool HasItem(ushort itemId)
        {
            return GetItemByItemId(itemId) != null;
        }

        public void SetSlotItem(int idx, IInventoryItem item)
        {
            Slots[idx] = item;
        }

        public virtual void ItemRemoveBySlotId(int idx, int stackCount)
        {
            if (Slots[idx] != null && Slots[idx].StackCount == 0)
                Slots[idx] = null;
        }

        public virtual int GetEmptySlotCount()
        {
            int count = 0;
            for (int index = 0; index < UnlockedSlotCount; ++index)
            {
                if (Slots[index] == null)
                    ++count;
            }
            return count;
        }

        public virtual bool HasEmptySlot()
        {
            return GetEmptySlotIndex() >= 0;
        }

        public virtual int GetEmptySlotIndex()
        {
            for (int index = 0; index < UnlockedSlotCount; ++index)
            {
                if (Slots[index] == null)
                    return index;
            }
            return -1;
        }

        public virtual int GetAvailableSlotByItemId(ushort itemId)
        {
            for (int index = 0; index < UnlockedSlotCount; ++index)
            {
                IInventoryItem invItem = Slots[index];
                if (invItem != null && invItem.ItemID == itemId && invItem.StackCount < invItem.MaxStackCount)
                    return index;
            }
            return -1;
        }

        public virtual bool IsExistSlotAvailableByItemId(ushort itemId, int stackCount, int maxStackCount)
        {
            for (int index = 0; index < UnlockedSlotCount; ++index)
            {
                IInventoryItem invItem = Slots[index];
                if (invItem != null && invItem.ItemID == itemId)
                {
                    stackCount -= (maxStackCount - invItem.StackCount);
                    if (stackCount <= 0)
                        return true;
                }
            }
            return false;
        }

        public int GetStackcountOnSlot(int slotIdx)
        {
            return Slots[slotIdx] == null ? 0 : Slots[slotIdx].StackCount;
        }

        public virtual Dictionary<int, IInventoryItem> GetStackableSlot(ushort itemId)
        {
            Dictionary<int, IInventoryItem> ret = new Dictionary<int, IInventoryItem>();
            ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(itemId);            
            if (itemJson != null)
            {
                ItemSortJson itemSortJson = GameRepo.ItemFactory.GetItemSortById(itemJson.itemsort);
                if (itemSortJson != null && itemSortJson.maxstackcount > 1)
                {
                    for (int index = 0; index < UnlockedSlotCount; ++index)
                    {
                        IInventoryItem invItem = Slots[index];
                        if (invItem != null && invItem.ItemID == itemId && invItem.StackCount < invItem.MaxStackCount)
                            ret.Add(index, invItem);
                    }
                }
            }
            return ret;
        }

        public virtual void ItemInventorySorting()
        {
            int listlen = Slots.Count;
            Slots.RemoveAll(o => o == null);
            MergeItemStackCount(); //O(n2)

            int currentlistlen = Slots.Count;
            if (currentlistlen < maxSlotsSize)
            {
                List<IInventoryItem> emptyslots = new List<IInventoryItem>(new IInventoryItem[(listlen - currentlistlen)]);
                Slots.AddRange(emptyslots);
            }
        }

        public void MergeItemStackCount()
        {
            List<IInventoryItem> newOrderredSlots = Slots.OrderBy(o => o.SortOrder).ThenBy(o => o.ItemID).ToList();
            Slots.Clear();
            foreach (IInventoryItem item in newOrderredSlots)
            {
                if (Slots.Count > 0)
                {
                    bool newf = true;
                    foreach (IInventoryItem useableitem in Slots)
                    {
                        if (useableitem.ItemID == item.ItemID)
                        {
                            if (useableitem.StackCount < useableitem.MaxStackCount)
                            {
                                ushort remain = (ushort)(useableitem.MaxStackCount - useableitem.StackCount);
                                newf = false;
                                if (remain >= item.StackCount)
                                {
                                    useableitem.StackCount += item.StackCount;
                                    break;
                                }
                                else
                                {
                                    ushort newCount = (ushort)(item.StackCount - remain);
                                    useableitem.StackCount = useableitem.MaxStackCount;
                                    IInventoryItem newitem = item;
                                    newitem.StackCount = newCount;
                                    Slots.Add(newitem);
                                    break;
                                }
                            }
                        }
                    }
                    if (newf)
                    {
                        Slots.Add(item);
                    }
                }
                else
                    Slots.Add(item);
            }
        }

        public bool CanAdd(List<ItemInfo> itemsToAdd)
        {
            int emptySlot = GetEmptySlotCount();
            int itemsToAddCount = itemsToAdd.Count;
            if (emptySlot >= itemsToAddCount)
                return true;

            for (int index = 0; index < itemsToAddCount; ++index)
            {
                ItemInfo item = itemsToAdd[index];
                ushort itemId = item.itemId;
                ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(itemId);
                if (itemJson == null)
                    continue;
                ItemSortJson itemSortJson = GameRepo.ItemFactory.GetItemSortById(itemJson.itemsort);
                if (itemSortJson.maxstackcount == 1 || !IsExistSlotAvailableByItemId(itemId, item.stackCount, itemSortJson.maxstackcount))
                {
                    if (--emptySlot < 0)
                        return false;
                }
            }
            return true;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ItemInventoryServerData : ItemInventoryData
    {
        private Dictionary<int, ItemSlotInfo> itemSlotMap; // itemId <- ItemSlotInfo
        private int emptySlots; // Number of empty slots

        public ItemInventoryServerData()
        {
            itemSlotMap = new Dictionary<int, ItemSlotInfo>();
            emptySlots = 0;
        }

        public void Init(ItemInventoryData data)
        {
            Slots = data.Slots;
            UnlockedSlotCount = data.UnlockedSlotCount;
            RebuildSlotMap();
        }

        public override void ClearItemInventory()
        {
            base.ClearItemInventory();
            itemSlotMap.Clear();
            emptySlots = UnlockedSlotCount;
        }

        private void RebuildSlotMap()
        {
            itemSlotMap.Clear();
            emptySlots = 0;
            for (int idx = 0; idx < UnlockedSlotCount; ++idx)
            {
                IInventoryItem item = Slots[idx];
                if (item == null)
                    ++emptySlots;
                else
                {
                    int itemId = item.ItemID;
                    if (!itemSlotMap.ContainsKey(itemId))
                        itemSlotMap.Add(itemId, new ItemSlotInfo());
                    itemSlotMap[itemId].Add(idx, item.StackCount);
                }
            }
        }

        public void OnUnlockedSlotUpdated(int unlockedSlotCount)
        {
            emptySlots += unlockedSlotCount - UnlockedSlotCount;
            UnlockedSlotCount = unlockedSlotCount;
        }

        public int OnAddItem(int slotIdx, int stackCount)
        {
            IInventoryItem item = Slots[slotIdx];         
            if (item.StackCount == stackCount)
                --emptySlots;
            int itemId = item.ItemID;
            if (!itemSlotMap.ContainsKey(itemId))
                itemSlotMap.Add(itemId, new ItemSlotInfo());
            itemSlotMap[itemId].Add(slotIdx, stackCount);
            return itemId;
        }

        public void OnRemoveItem(int slotIdx, int itemId, int stackCount)
        {
            if (itemSlotMap.ContainsKey(itemId))
            {
                ItemSlotInfo slotInfo = itemSlotMap[itemId];
                slotInfo.stackTotal -= stackCount;
                if (Slots[slotIdx] == null)
                {
                    List<int> slotIds = slotInfo.slotIds;
                    if (slotIds.Remove(slotIdx) && slotIds.Count == 0)
                        itemSlotMap.Remove(itemId);
                }
            }
        }

        public override IInventoryItem GetItemByItemId(ushort itemId)
        {
            return itemSlotMap.ContainsKey(itemId) ? Slots[itemSlotMap[itemId].slotIds[0]] : null;
        }

        public override Dictionary<int, IInventoryItem> GetItemsByItemId(ushort itemId)
        {
            Dictionary<int, IInventoryItem> ret = new Dictionary<int, IInventoryItem>();
            if (itemSlotMap.ContainsKey(itemId))
            {
                List<int> slotIdxs = itemSlotMap[itemId].slotIds;
                int slotIdxCount = slotIdxs.Count;
                for (int index = 0; index < slotIdxCount; ++index)
                {
                    int slotIdx = slotIdxs[index];
                    ret.Add(slotIdx, Slots[slotIdx]);
                }
            }
            return ret;
        }

        public int GetLeastStackCountSlotIdxByItemId(ushort itemId)
        {
            ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(itemId);
            if (itemJson == null)
                return -1;
            ItemSortJson itemSortJson = GameRepo.ItemFactory.GetItemSortById(itemJson.itemsort);
            if (itemSortJson == null)
                return -1;

            int slotIdx = -1, leastStackCount = itemSortJson.maxstackcount;
            if (itemSlotMap.ContainsKey(itemId))
            {
                List<int> itemSlotIdxs = itemSlotMap[itemId].slotIds;
                int count = itemSlotIdxs.Count;
                for (int i = 0; i < count; ++i)
                {
                    int currSlotId = itemSlotIdxs[i];
                    int currStackCnt = Slots[currSlotId].StackCount;
                    if (currStackCnt < leastStackCount)
                    {
                        leastStackCount = currStackCnt;
                        slotIdx = currSlotId;
                    }
                }
            }
            return slotIdx;
        }

        public override int GetTotalStackCountByItemId(ushort itemId)
        {
            return itemSlotMap.ContainsKey(itemId) ? itemSlotMap[itemId].stackTotal : 0;
        }

        public override bool HasItem(ushort itemId, int stackCount)
        {
            return GetTotalStackCountByItemId(itemId) >= stackCount;
        }

        public override bool HasItem(ushort itemId)
        {
            return itemSlotMap.ContainsKey(itemId);
        }

        public override void ItemRemoveBySlotId(int idx, int stackCount)
        {
            if (Slots[idx] == null)
                return;

            IInventoryItem item = Slots[idx];
            if (item.StackCount == 0)
            {
                Slots[idx] = null;
                ++emptySlots;
            }
            OnRemoveItem(idx, item.ItemID, stackCount);
        }

        public override int GetEmptySlotCount()
        {
            return emptySlots;
        }

        public override bool HasEmptySlot()
        {
            return emptySlots > 0;
        }

        public override int GetEmptySlotIndex()
        {
            if (emptySlots > 0)
            {
                for (int index = 0; index < UnlockedSlotCount; ++index)
                    if (Slots[index] == null)
                        return index;
            }
            return -1;
        }

        public override int GetAvailableSlotByItemId(ushort itemId)
        {
            if (itemSlotMap.ContainsKey(itemId))
            {
                List<int> slotIdxs = itemSlotMap[itemId].slotIds;
                int slotIdxCount = slotIdxs.Count;
                for (int index = 0; index < slotIdxCount; ++index)
                {
                    int slotIdx = slotIdxs[index];
                    IInventoryItem invItem = Slots[slotIdx];
                    if (invItem != null && invItem.ItemID == itemId && invItem.StackCount < invItem.MaxStackCount)
                        return slotIdx;
                }
            }
            return -1;
        }

        public override bool IsExistSlotAvailableByItemId(ushort itemId, int stackCount, int maxstack)
        {
            ItemSlotInfo itemSlotInfo;
            if (itemSlotMap.TryGetValue(itemId, out itemSlotInfo))
                return (itemSlotInfo.slotIds.Count * maxstack - itemSlotInfo.stackTotal) >= stackCount;

            return false;
        }

        public override Dictionary<int, IInventoryItem> GetStackableSlot(ushort itemId)
        {
            Dictionary<int, IInventoryItem> ret = new Dictionary<int, IInventoryItem>();
            ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(itemId);         
            if (itemJson != null)
            {
                ItemSortJson itemSortJson = GameRepo.ItemFactory.GetItemSortById(itemJson.itemsort);
                if (itemSortJson != null && itemSortJson.maxstackcount > 1)
                {
                    if (itemSlotMap.ContainsKey(itemId))
                    {
                        List<int> slotIdxs = itemSlotMap[itemId].slotIds;
                        int slotIdxCount = slotIdxs.Count;
                        for (int index = 0; index < slotIdxCount; ++index)
                        {
                            int slotIdx = slotIdxs[index];
                            IInventoryItem invItem = Slots[slotIdx];
                            if (invItem != null && invItem.ItemID == itemId && invItem.StackCount < invItem.MaxStackCount)
                                ret.Add(slotIdx, invItem);
                        }
                    }
                }
            }
            return ret;
        }

        public override void ItemInventorySorting()
        {
            base.ItemInventorySorting();
            RebuildSlotMap();
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class EquipmentInventoryData
    {
        [JsonProperty(PropertyName = "eqslots")]
        public List<Equipment> Slots = new List<Equipment>();

        [JsonProperty(PropertyName = "fsslots")]
        public List<Equipment> FashionSlots = new List<Equipment>();

        [JsonProperty(PropertyName = "appearance")]
        public List<int> AppearanceSlots = new List<int>();

        [JsonProperty(PropertyName = "helm")]
        public bool HideHelm = false;

        private int mEquipmentSlotSize = (int)EquipmentSlot.MAXSLOTS;
        private int mFashionSlotSize = (int)FashionSlot.MAXSLOTS;
        private int mAppearanceSlotSize = (int)AppearanceSlot.MAXSLOTS;

        public void InitDefault()
        {
            Slots = new List<Equipment>(new Equipment[mEquipmentSlotSize]);
            FashionSlots = new List<Equipment>(new Equipment[mFashionSlotSize]);
            AppearanceSlots = new List<int>(new int[mAppearanceSlotSize]);
        }

        public void ValidateDefault()
        {
            if (Slots.Count != mEquipmentSlotSize)
            {
                List<Equipment> temp = Slots;
                Slots = new List<Equipment>(new Equipment[mEquipmentSlotSize]);
                for (int i = 0; i < mEquipmentSlotSize; ++i)
                {
                    if (i == temp.Count)
                        break;
                    Slots[i] = temp[i];
                }
            }
            if (FashionSlots.Count != mFashionSlotSize)
            {
                List<Equipment> temp = FashionSlots;
                FashionSlots = new List<Equipment>(new Equipment[mFashionSlotSize]);
                for (int i = 0; i < mFashionSlotSize; ++i)
                {
                    if (i == temp.Count)
                        break;
                    FashionSlots[i] = temp[i];
                }
            }
            if (AppearanceSlots.Count != mAppearanceSlotSize)
            {
                List<int> temp = AppearanceSlots;
                AppearanceSlots = new List<int>(new int[mAppearanceSlotSize]);
                for (int i = 0; i < mAppearanceSlotSize; ++i)
                {
                    if (i == temp.Count)
                        break;
                    AppearanceSlots[i] = temp[i];
                }
            }
        }

        public void ClearEquipmentInventory()
        {
            int count = Slots.Count;
            for (int i = 0; i < count; ++i)
                Slots[i] = null;

            count = FashionSlots.Count;
            for (int i = 0; i < count; ++i)
                FashionSlots[i] = null;
        }

        public Equipment GetEquipmentBySlotId(int idx)
        {
            if (idx < mEquipmentSlotSize)
                return Slots[idx];
            return null;
        }

        public void SetEquipmentToSlot(int slotIdx, Equipment equipment)
        {
            Slots[slotIdx] = equipment;
        }

        public Equipment GetFashionSlot(int slotIdx)
        {
            if (slotIdx < mFashionSlotSize)
                return FashionSlots[slotIdx];
            return null;
        }

        public void SetFashionToSlot(int slotIdx, Equipment equipment)
        {
            FashionSlots[slotIdx] = equipment;
        }

        public bool IsFashionSlotEmpty(int slotIdx)
        {
            return FashionSlots[slotIdx] == null;
        }

        public EquipmentSlot FindAEquipSlotToSwap(Equipment equipitem, int equipSlotId = -1)
        {
            switch (equipitem.EquipmentJson.partstype)
            {
                case PartsType.Sword:
                case PartsType.Blade:
                case PartsType.Lance:
                case PartsType.Hammer:
                case PartsType.Fan:
                case PartsType.Xbow:
                case PartsType.Dagger:
                case PartsType.Sanxian:
                    return EquipmentSlot.Weapon;
                case PartsType.Helm:
                    return EquipmentSlot.Helm;
                case PartsType.Body:
                    return EquipmentSlot.Body;
                case PartsType.Wing:
                    return EquipmentSlot.Back;
                case PartsType.Boots:
                    return EquipmentSlot.Boots;
                case PartsType.Ring:
                    switch (equipSlotId)
                    {
                        default:
                            return (Slots[(int)EquipmentSlot.Ring1] != null && Slots[(int)EquipmentSlot.Ring2] == null) ? EquipmentSlot.Ring2 : EquipmentSlot.Ring1;
                        case 0:
                            return EquipmentSlot.Ring1;
                        case 1:
                            return EquipmentSlot.Ring2;
                    }
                case PartsType.Jewelry:
                    return EquipmentSlot.Neck;
                case PartsType.Accessory:
                    switch (equipSlotId)
                    {
                        default:
                            return (Slots[(int)EquipmentSlot.Accessory1] != null && Slots[(int)EquipmentSlot.Accessory2] == null) ? EquipmentSlot.Accessory2 : EquipmentSlot.Accessory1;
                        case 0:
                            return EquipmentSlot.Accessory1;
                        case 1:
                            return EquipmentSlot.Accessory2;
                    }
            }
            return EquipmentSlot.MAXSLOTS;
        }

        public Equipment GetEquipmentByItemId(ushort itemId)
        {
            foreach(Equipment equipment in Slots)
                if (equipment.ItemID == itemId)
                    return equipment;

            return null;
        }

        public void SetAppearanceToSlot(int slotIdx, int appearanceid)
        {
            AppearanceSlots[slotIdx] = appearanceid;
        }

        public int GetAppearanceSlot(int slotIdx)
        {
            if (slotIdx < mAppearanceSlotSize)
                return AppearanceSlots[slotIdx];
            return -1;
        }
    }

    public static class InventoryHelper
    {
        public static FashionSlot GetFashionSlotByPartType(PartsType type)
        {
            switch(type)
            {
                case PartsType.Sword:
                case PartsType.Blade:
                case PartsType.Lance:
                case PartsType.Hammer:
                case PartsType.Fan:
                case PartsType.Xbow:
                case PartsType.Dagger:
                case PartsType.Sanxian:
                    return FashionSlot.Weapon;
                case PartsType.Helm:
                    return FashionSlot.Helm;
                case PartsType.Body:
                    return FashionSlot.Body;
                case PartsType.Wing:
                    return FashionSlot.Back;
            }
            return FashionSlot.MAXSLOTS;
        }

        public static bool HasFashionSlot(PartsType type)
        {
            return GetFashionSlotByPartType(type) != FashionSlot.MAXSLOTS;
        }
    }
}
