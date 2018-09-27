using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zealot.Common;

public class InvSellPanelScrollRow : MonoBehaviour
{
    List<GameObject> ChildList = null;

    // Use this for initialization
    void Awake()
    {
        ChildList = new List<GameObject>();
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
                Destroy(ChildList[i]);
                ChildList[i] = null;
            }
            ChildList.Clear();
        }
    }

    public void Init(UI_Inventory_SellPanel uiInvSellPanel, int rowIndex, int cellsPerRow)
    {
        Clear();

        List<InvDisplayItem> sellItemList = uiInvSellPanel.SellItemList;
        int totalCount = sellItemList.Count;

        Transform parent = gameObject.transform;
        int realStartIdx = rowIndex * cellsPerRow;
        for (int i = 0; i < cellsPerRow; ++i)
        {
            int realIdx = realStartIdx + i;
            if (realIdx >= totalCount)
                break;

            InvDisplayItem invDisplayItem = sellItemList[realIdx];
            IInventoryItem invItem = invDisplayItem.InvItem;
            UnityAction callback = () => uiInvSellPanel.RemoveFromSellPanelByIndex(realIdx);

            ItemGameIconType iconType = invItem.ItemSortJson.gameicontype;
            GameObject gameIcon = Instantiate(ClientUtils.LoadGameIcon(iconType));
            gameIcon.transform.SetParent(parent, false);
            switch (iconType)
            {
                case ItemGameIconType.Equipment:
                    Equipment eq = invItem as Equipment;
                    gameIcon.GetComponent<GameIcon_Equip>().Init(eq.ItemID, 0, eq.ReformStep, eq.UpgradeLevel, false, eq.IsNew, false, callback);
                    break;
                case ItemGameIconType.Consumable:
                case ItemGameIconType.Material:
                    gameIcon.GetComponent<GameIcon_MaterialConsumable>().Init(invItem.ItemID, invDisplayItem.DisplayStackCount, 
                        false, invItem.IsNew, false, callback);
                    break;
                case ItemGameIconType.DNA:
                    gameIcon.GetComponent<GameIcon_DNA>().Init(invItem.ItemID, 0, 0, invItem.IsNew, callback);
                    break;
            }
           
            ChildList.Add(gameIcon);
        }
    }
}
