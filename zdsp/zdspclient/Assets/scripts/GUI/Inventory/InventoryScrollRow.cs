using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zealot.Common;

public class InvRowIcon
{
    public BagType kind = BagType.Any;
    public GameObject gameIcon = null;
}

public class InventoryScrollRow : MonoBehaviour
{
    [SerializeField]
    GameObject[] prefabGameicons = null;

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
                Destroy(ChildList[i].gameIcon);
                ChildList[i].gameIcon = null;
            }
            ChildList.Clear();
        }
    }

    public void Init(UI_Inventory uiInventory, int rowIndex, int cellsPerRow)
    {
        Clear();

        List<InvDisplayItem> displayItemList = uiInventory.DisplayItemList;
        BagType currInvTab = uiInventory.CurrentInventoryTab;
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
            IInventoryItem item = invDisplayItem.item;
            int originSlotId = invDisplayItem.originSlotId;
            GameObject gameIcon = null;
            if (currInvTab == BagType.Any && item == null)
            {
                gameIcon = Instantiate(prefabGameicons[0]);
                gameIcon.GetComponent<GameIcon_EmptySlot>().SetLock(realIdx >= unlockSlotCnt);
            }
            else
            {
                UnityAction callback = null;
                if (invSellPanel.gameObject.activeInHierarchy)
                    callback = () => invSellPanel.OnOpenDialogItemSellUse(realIdx);
                else if (invQuickSlot.gameObject.activeInHierarchy)
                    callback = () => invQuickSlot.OnSetItemToSlot(originSlotId);
                else
                    callback = () => uiInventory.OnClicked_InventoryItem(originSlotId, item);

                BagType bagType = item.JsonObject.bagtype;
                gameIcon = Instantiate(prefabGameicons[(int)bagType]);
                switch (bagType)
                {
                    case BagType.Equipment:
                        Equipment eq = item as Equipment;
                        gameIcon.GetComponent<GameIcon_Equip>().Init(eq.ItemID, 0, 0, eq.UpgradeLevel, false, false, false, callback);
                        break;
                    case BagType.Consumable:
                    case BagType.Material:
                        gameIcon.GetComponent<GameIcon_MaterialConsumable>().Init(item.ItemID, item.StackCount, false, false, callback);
                        break;
                    case BagType.DNA:
                        gameIcon.GetComponent<GameIcon_DNA>().Init(item.ItemID, 0, 0, callback);
                        break;
                }
            }

            gameIcon.transform.SetParent(parent, false);
            ChildList.Add(new InvRowIcon { kind = (item != null) ? item.JsonObject.bagtype : BagType.Any, gameIcon = gameIcon });
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
            int realIdx = startIndex + i;
            InvDisplayItem invDisplayItem = displayItemList[realIdx];
            int originSlotId = invDisplayItem.originSlotId;
            UnityAction callback = null;
            if (invSellPanel.gameObject.activeInHierarchy)
                callback = () => invSellPanel.OnOpenDialogItemSellUse(realIdx);
            else if (invQuickSlot.gameObject.activeInHierarchy)
                callback = () => invQuickSlot.OnSetItemToSlot(originSlotId);
            else
                callback = () => uiInventory.OnClicked_InventoryItem(originSlotId, invDisplayItem.item);

            BagType kind = ChildList[i].kind;
            GameObject gameIcon = ChildList[i].gameIcon;
            switch (kind)
            {
                case BagType.Equipment:
                    gameIcon.GetComponent<GameIcon_Equip>().SetClickCallback(callback);
                    break;
                case BagType.Consumable:
                case BagType.Material:
                    gameIcon.GetComponent<GameIcon_MaterialConsumable>().SetClickCallback(callback);
                    break;
                case BagType.DNA:
                    gameIcon.GetComponent<GameIcon_DNA>().SetClickCallback(callback);
                    break;
            }
        }
    }

    [System.Obsolete("Used in swap method, only works when there is only one kind of gameobject")]
    public void SetParent(Transform parent)
    {
        InventoryScrollRow parentInvRow = parent.gameObject.GetComponent<InventoryScrollRow>();
        int count = ChildList.Count;
        for (int i = 0; i < count; ++i)
        {
            InvRowIcon child = ChildList[i];
            parentInvRow.ChildList.Add(child);
            child.gameIcon.transform.SetParent(parent, false);
        }
        ChildList.Clear();
    }
}
