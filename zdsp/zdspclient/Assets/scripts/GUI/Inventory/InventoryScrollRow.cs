using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zealot.Common;

public class InvRowIcon
{
    public BagType bagType = BagType.Any;
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
            int originSlotId = invDisplayItem.OriginSlotId;
            IInventoryItem invItem = invDisplayItem.InvItem;
            GameObject gameIcon = null;            
            if (currInvTab == BagType.Any && invItem == null)
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
                    callback = () => uiInventory.OnClicked_InventoryItem(originSlotId, invItem);

                BagType bagType = invItem.JsonObject.bagtype;
                gameIcon = Instantiate(prefabGameicons[(int)bagType]);
                switch (bagType)
                {
                    case BagType.Equipment:
                        Equipment eq = invItem as Equipment;
                        if(eq != null)
                            gameIcon.GetComponent<GameIcon_Equip>().Init(eq.ItemID, 0, 0, eq.UpgradeLevel, false, false, eq.IsNew, false, callback);

                        Relic relic = invItem as Relic;
                        if(relic != null)
                            gameIcon.GetComponent<GameIcon_Equip>().Init(relic.ItemID, 0, 0, 0, false, false, eq.IsNew, false, callback);
                        break;
                    case BagType.Consumable:
                    case BagType.Material:
                        gameIcon.GetComponent<GameIcon_MaterialConsumable>().Init(invItem.ItemID, invDisplayItem.DisplayStackCount, false, invItem.IsNew, false, callback);
                        break;
                    case BagType.DNA:
                        gameIcon.GetComponent<GameIcon_DNA>().Init(invItem.ItemID, 0, 0, invItem.IsNew, callback);
                        break;
                }
            }

            gameIcon.transform.SetParent(parent, false);
            ChildList.Add(new InvRowIcon { bagType = (invItem != null) ? invItem.JsonObject.bagtype : BagType.Any, gameIcon = gameIcon });
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
            int originSlotId = invDisplayItem.OriginSlotId;

            UnityAction callback = null;
            if (invSellPanel.gameObject.activeInHierarchy)
                callback = () => invSellPanel.OnOpenDialogItemSellUse(realIdx);
            else if (invQuickSlot.gameObject.activeInHierarchy)
                callback = () => invQuickSlot.OnSetItemToSlot(originSlotId);
            else
                callback = () => uiInventory.OnClicked_InventoryItem(originSlotId, invDisplayItem.InvItem);

            BagType bagType = ChildList[i].bagType;
            GameObject gameIcon = ChildList[i].gameIcon;
            switch (bagType)
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

    [System.Obsolete("Used in swap method, only works when there is only one type of gameobject as child")]
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
