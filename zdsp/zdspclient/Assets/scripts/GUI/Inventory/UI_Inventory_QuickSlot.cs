using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Common.Datablock;

public class UI_Inventory_QuickSlot : MonoBehaviour
{
    [SerializeField]
    InvQuickConsumableButton[] invQuickSlots = null;

    public UI_Inventory UIInventory { private get; set; }

    public byte SelectedQuickSlotIdx {  get; set; }

    // Use this for initialization
    void Awake()
    {
        int len = invQuickSlots.Length;
        for (int i = 0; i < len; ++i)
        {
            InvQuickConsumableButton quickSlot = invQuickSlots[i];
            quickSlot.InvQuickSlot = this;
            quickSlot.QuickSlotIdx = (byte)i;
        }
    }

    void OnEnable()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
        {
            CollectionHandler<object> itemHotbarCollection = player.ItemHotbarStats.ItemHotbar;
            int itemHotbarSlotCnt = itemHotbarCollection.Count;
            for (int i = 0; i < itemHotbarSlotCnt; ++i)
            {
                int itemId = (int)itemHotbarCollection[i];
                invQuickSlots[i].SetItem(itemId);
            }
            UIInventory.UpdateScrollViewCallback();
        }
    }

    void OnDisable()
    {
        if (UIInventory.CurrentInventoryTab == BagType.Consumable)
            UIInventory.UpdateScrollViewCallback();
    }

    public void SetItemToSlot(int slotIdx, int itemId)
    {
        invQuickSlots[slotIdx].SetItem(itemId);
    }

    public void OnSetItemToSlot(int slotIdx)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
        {
            IInventoryItem invItem = player.clientItemInvCtrl.itemInvData.Slots[slotIdx];
            if (invItem == null)
                return;

            CollectionHandler<object> itemHotbarCollection = player.ItemHotbarStats.ItemHotbar;
            int itemHotbarSlotCnt = itemHotbarCollection.Count;
            int idxToSet = -1, itemIdToAdd = invItem.ItemID;
            for (int i = 0; i < itemHotbarSlotCnt; ++i)
            {
                int itemId = (int)itemHotbarCollection[i];
                if (itemId == itemIdToAdd)
                {
                    idxToSet = -1;
                    break;
                }
                else if (itemId == 0 && idxToSet == -1)
                    idxToSet = i;
            }
            if (idxToSet != -1)
            {
                idxToSet = ((int)itemHotbarCollection[SelectedQuickSlotIdx] == 0) ? SelectedQuickSlotIdx : idxToSet;
                RPCFactory.CombatRPC.SetItemHotbar((byte)idxToSet, slotIdx);
            }
            SelectedQuickSlotIdx = 0;
        }
    }
}
