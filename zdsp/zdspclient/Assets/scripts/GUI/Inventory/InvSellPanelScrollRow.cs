using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;

public class InvSellPanelScrollRow : MonoBehaviour
{
    [SerializeField]
    GameObject[] prefabGameicons = null;

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
            BagType bagType = invItem.JsonObject.bagtype;
            GameObject gameIcon = Instantiate(prefabGameicons[(int)bagType-1]);
            switch (bagType)
            {
                case BagType.Equipment:
                    Equipment eq = invItem as Equipment;
                    gameIcon.GetComponent<GameIcon_Equip>().Init(eq.ItemID, 0, 0, eq.UpgradeLevel, false, false, false, 
                        () => { uiInvSellPanel.RemoveFromSellPanelByIndex(realIdx); });
                    break;
                case BagType.Consumable:
                case BagType.Material:
                    gameIcon.GetComponent<GameIcon_MaterialConsumable>().Init(invItem.ItemID, invDisplayItem.DisplayStackCount, false, false,
                        () => { uiInvSellPanel.RemoveFromSellPanelByIndex(realIdx); });
                    break;
                case BagType.DNA:
                    gameIcon.GetComponent<GameIcon_DNA>().Init(invItem.ItemID, 0, 0,
                        () => { uiInvSellPanel.RemoveFromSellPanelByIndex(realIdx); });
                    break;
            }

            gameIcon.transform.SetParent(parent, false);
            ChildList.Add(gameIcon);
        }
    }
}
