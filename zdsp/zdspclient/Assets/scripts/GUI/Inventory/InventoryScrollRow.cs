using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zealot.Common;

public class InvRowIcon
{
    public int IconType = 0;
    public GameObject GameIcon = null;
}

public class InventoryScrollRow : MonoBehaviour
{
    [SerializeField]
    GameObject prefabIconEmptyslot = null;

    List<InvRowIcon> ChildList { get; set; }

    // Use this for initialization
    void Awake()
    {
        ChildList = new List<InvRowIcon>();
    }

    void OnDestroy()
    {
        ChildList = null;
    }

    public void Clear()
    {
        int count = ChildList.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; ++i)
            {
                Destroy(ChildList[i].GameIcon);
                ChildList[i].GameIcon = null;
            }
            ChildList.Clear();
        }
    }

    public void Init(UI_Inventory uiInventory, int rowIndex, int cellsPerRow)
    {
        Clear();

        List<InvDisplayItem> displayItemList = uiInventory.DisplayItemList;
        BagTabType currInvTab = uiInventory.CurrentInventoryTab;
        int totalCount = displayItemList.Count;
        int unlockSlotCnt = GameInfo.gLocalPlayer.SecondaryStats.UnlockedSlotCount;
        UI_Inventory_SellPanel invSellPanel = uiInventory.InvSellPanel;
        UI_Inventory_QuickSlot invQuickSlot = uiInventory.InvQuickSlot;

        Transform parent = gameObject.transform;
        int realStartIdx = rowIndex * cellsPerRow;
        for (int i = 0; i < cellsPerRow; ++i)
        {
            int realIdx = realStartIdx + i;
            if (realIdx >= totalCount)
                break;

            // Init game icon
            InvDisplayItem invDisplayItem = displayItemList[realIdx];
            IInventoryItem invItem = invDisplayItem.InvItem;
            GameObject gameIcon = null;
            if (currInvTab == BagTabType.All && invItem == null)
            {
                gameIcon = Instantiate(prefabIconEmptyslot);
                gameIcon.GetComponent<GameIcon_EmptySlot>().SetLock(realIdx >= unlockSlotCnt);
            }
            else
            {
                UnityAction callback = null;
                if (invSellPanel.gameObject.activeInHierarchy)
                    callback = () => invSellPanel.OnOpenDialogItemSellUse(realIdx);
                else if (invQuickSlot.gameObject.activeInHierarchy)
                    callback = () => invQuickSlot.OnSetItemToSlot(invDisplayItem.OriginSlotId);
                else
                    callback = () => uiInventory.OnClicked_InventoryItem(invDisplayItem.OriginSlotId, invItem);

                ItemGameIconType iconType = invItem.ItemSortJson.gameicontype;
                gameIcon = Instantiate(ClientUtils.LoadGameIcon(iconType));
                switch (iconType)
                {
                    case ItemGameIconType.Equipment:
                        Equipment eq = invItem as Equipment;
                        gameIcon.GetComponent<GameIcon_Equip>().Init(eq.ItemID, 0, eq.ReformStep, eq.UpgradeLevel, false, 
                            eq.IsNew, false, callback);
                        break;
                    case ItemGameIconType.Consumable:
                    case ItemGameIconType.Material:
                        gameIcon.GetComponent<GameIcon_MaterialConsumable>().Init(invItem.ItemID, 
                            invDisplayItem.DisplayStackCount, false, invItem.IsNew, false, callback);
                        break;
                    case ItemGameIconType.DNA:
                        gameIcon.GetComponent<GameIcon_DNA>().Init(invItem.ItemID, 0, 0, invItem.IsNew, callback);
                        break;
                }
            }

            gameIcon.transform.SetParent(parent, false);
            ChildList.Add(new InvRowIcon { IconType = (invItem != null) ? (int)invItem.ItemSortJson.gameicontype : -1, GameIcon = gameIcon });
        }
    }

    public void UpdateCallback(UI_Inventory uiInventory, int startIndex)
    {
        List<InvDisplayItem> displayItemList = uiInventory.DisplayItemList;
        UI_Inventory_SellPanel invSellPanel = uiInventory.InvSellPanel;
        UI_Inventory_QuickSlot invQuickSlot = uiInventory.InvQuickSlot;

        int count = ChildList.Count;
        for (int i = 0; i < count; ++i)
        {
            InvRowIcon invRowIcon = ChildList[i];
            if (invRowIcon.IconType != -1)
            {
                int realIdx = startIndex + i;
                InvDisplayItem invDisplayItem = displayItemList[realIdx];

                UnityAction callback = null;
                if (invSellPanel.gameObject.activeInHierarchy)
                    callback = () => invSellPanel.OnOpenDialogItemSellUse(realIdx);
                else if (invQuickSlot.gameObject.activeInHierarchy)
                    callback = () => invQuickSlot.OnSetItemToSlot(invDisplayItem.OriginSlotId);
                else
                    callback = () => uiInventory.OnClicked_InventoryItem(invDisplayItem.OriginSlotId, invDisplayItem.InvItem);
            
                ItemGameIconType iconType = (ItemGameIconType)invRowIcon.IconType;
                switch (iconType)
                {
                    case ItemGameIconType.Equipment:
                        invRowIcon.GameIcon.GetComponent<GameIcon_Equip>().SetClickCallback(callback);
                        break;
                    case ItemGameIconType.Consumable:
                    case ItemGameIconType.Material:
                        invRowIcon.GameIcon.GetComponent<GameIcon_MaterialConsumable>().SetClickCallback(callback);
                        break;
                    case ItemGameIconType.DNA:
                        invRowIcon.GameIcon.GetComponent<GameIcon_DNA>().SetClickCallback(callback);
                        break;
                }
            }
        }
    }

    [System.Obsolete("Used in swap method, only works when there is only one type of gameobject as child")]
    public void SetParent(Transform parent)
    {
        InventoryScrollRow parentInvRow = parent.gameObject.GetComponent<InventoryScrollRow>();
        int count = ChildList.Count;
        for (int i = 0; i < count; ++i)
        {
            InvRowIcon child = ChildList[i];
            parentInvRow.ChildList.Add(child);
            child.GameIcon.transform.SetParent(parent, false);
        }
        ChildList.Clear();
    }
}
