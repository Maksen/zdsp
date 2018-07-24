using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class UI_Inventory_SellPanel : MonoBehaviour
{
    [SerializeField]
    InvSellPanelScrollView sellPanelScrollView = null;

    [SerializeField]
    Toggle[] toggleCheckboxRarity = null;

    [SerializeField]
    Text txtTotalSellVal = null;

    public UI_Inventory UIInventory { private get; set; }

    public List<IInventoryItem> SellItemList { get; set; }

    public List<Dictionary<int, int>> RefList = null; // Dict: originSlotId -> amount

    Dictionary<int, int> sellAmtToSlotIdDict = null;

    int totalSellPrice = 0;

    // Use this for initialization
    void Awake()
    {
        SellItemList = new List<IInventoryItem>();
        RefList = new List<Dictionary<int, int>>();
        sellAmtToSlotIdDict = new Dictionary<int, int>();
    }

    void OnEnable()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
            sellPanelScrollView.InitScrollView(this);

        for (int i = 0; i < 3; ++i)
            toggleCheckboxRarity[i].isOn = false;

        totalSellPrice = 0;
        txtTotalSellVal.text = "0";

        UIInventory.ButtonPowerup.interactable = false;
    }

    void OnDisable()
    {
        sellPanelScrollView.Clear();
        SellItemList.Clear();
        RefList.Clear();

        UIInventory.ButtonPowerup.interactable = true;
    }

    public void RefreshSellPanel()
    {
        sellPanelScrollView.PopulateRows();
        txtTotalSellVal.text = totalSellPrice.ToString();
    }

    public void UpdateSellListFromRefList()
    {
        int refListCount = RefList.Count;
        for (int i = refListCount-1; i >= 0; --i)
        {
            int stackCount = 0;
            if (RefList[i].Count > 0)
            {
                Dictionary<int, int>.ValueCollection refDictVals = RefList[i].Values;
                foreach (int stack in refDictVals)
                    stackCount += stack;
            }
            if (stackCount == 0)
            {
                SellItemList.RemoveAt(i);
                RefList.RemoveAt(i);
            }
            else
                SellItemList[i].StackCount = stackCount;
        }
    }

    public void AddItemToSellList(IInventoryItem item, int itemId, int maxStackCnt, int originSlotId, int amtToAdd)
    {
        int sellItemListCnt = SellItemList.Count;
        for (int i = 0; i < sellItemListCnt; ++i)
        {
            if (amtToAdd == 0)
                break;

            IInventoryItem sellItem = SellItemList[i];
            int currStackCnt = sellItem.StackCount;
            if (sellItem.ItemID == itemId && currStackCnt < maxStackCnt)
            {
                int amtCanAdd = amtToAdd;
                if (currStackCnt + amtToAdd > maxStackCnt)
                    amtCanAdd = maxStackCnt - currStackCnt;

                amtToAdd -= amtCanAdd; // Leftover to next slot
                Dictionary<int, int> refDict = RefList[i];
                if (refDict.ContainsKey(originSlotId))
                    refDict[originSlotId] += amtCanAdd;
                else
                    refDict.Add(originSlotId, amtCanAdd);

                SellItemList[i].StackCount += amtCanAdd;             
                totalSellPrice += sellItem.JsonObject.sellprice * amtCanAdd;
            }
        }

        // If there is still leftover to add
        if (amtToAdd > 0)
        {
            RefList.Add(new Dictionary<int, int>() { { originSlotId, amtToAdd } });
            IInventoryItem newItem = GameRepo.ItemFactory.GetInventoryItemCopy(item);
            newItem.StackCount = amtToAdd;
            SellItemList.Add(newItem);
            totalSellPrice += newItem.JsonObject.sellprice * amtToAdd;
        }
    }

    public void AddItemToSellPanel(int displayItemIdx, int amtToAdd)
    {
        if (amtToAdd == 0)
            return;

        InvDisplayItem invDisplayItem = UIInventory.DisplayItemList[displayItemIdx];
        IInventoryItem displayItem = invDisplayItem.item;
        displayItem.StackCount -= amtToAdd; // Remove amount from displayItem

        AddItemToSellList(displayItem, displayItem.ItemID, displayItem.MaxStackCount, invDisplayItem.originSlotId, amtToAdd);

        if (displayItem.StackCount == 0)
            UIInventory.RemoveFromDisplayItemList(displayItemIdx);
        UIInventory.UpdateVisibleInvRows();
    }

    public void AddItemtoSellPanelByRarity(ItemRarity rarity)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
        {
            List<IInventoryItem> invItemList = player.clientItemInvCtrl.itemInvData.Slots;
            int itemListCnt = invItemList.Count;
            for (int i = 0; i < itemListCnt; ++i)
            {
                IInventoryItem item = invItemList[i];
                if (item != null && item.JsonObject.rarity == rarity)
                {
                    int amtLeftToAdd = item.StackCount, maxStackCnt = item.MaxStackCount;
                    int refListCnt = RefList.Count;
                    for (int j = 0; j < refListCnt; ++j)
                    {
                        Dictionary<int, int> refDict = RefList[j];
                        if (refDict.ContainsKey(i))
                            amtLeftToAdd -= refDict[i];
                    }

                    AddItemToSellList(item, item.ItemID, item.MaxStackCount, i, amtLeftToAdd);
                }
            }

            UIInventory.RefreshRight(player);
        }
    }

    public void RemoveFromSellPanelByIndex(int index)
    {
        RefList.RemoveAt(index);
        IInventoryItem sellItem = SellItemList[index];
        totalSellPrice -= sellItem.JsonObject.sellprice * sellItem.StackCount;
        SellItemList.RemoveAt(index);

        UIInventory.UpdateVisibleInvRows();
    }

    public void RemoveAllFromSellpanel()
    {
        RefList.Clear();
        SellItemList.Clear();
        UIInventory.UpdateVisibleInvRows();
    }

    public void RemoveFromSellPanelByRarity(ItemRarity rarity)
    {
        int sellItemCnt = SellItemList.Count;
        for (int i = 0; i < sellItemCnt; ++i)
        {
            if (SellItemList[i].JsonObject.rarity == rarity)
            {
                RefList.RemoveAt(i);
                IInventoryItem sellItem = SellItemList[i];
                totalSellPrice -= sellItem.JsonObject.sellprice * sellItem.StackCount;
                SellItemList.RemoveAt(i);
                sellItemCnt = SellItemList.Count;
                --i;
            }
        }

        UIInventory.UpdateVisibleInvRows();
    }

    public void OnValueChangedSelectAllCommon(bool value)
    {
        if (value) AddItemtoSellPanelByRarity(ItemRarity.Common);
        else RemoveFromSellPanelByRarity(ItemRarity.Common);
    }

    public void OnValueChangedSelectAllUncommon(bool value)
    {
        if (value) AddItemtoSellPanelByRarity(ItemRarity.Uncommon);
        else RemoveFromSellPanelByRarity(ItemRarity.Uncommon);
    }

    public void OnValueChangedSelectAllRare(bool value)
    {
        if (value) AddItemtoSellPanelByRarity(ItemRarity.Rare);
        else RemoveFromSellPanelByRarity(ItemRarity.Rare);
    }

    public void OnClickSellItems()
    {
        sellAmtToSlotIdDict.Clear();

        int refListCount = RefList.Count;
        if (refListCount > 0)
        {
            for (int i = 0; i < refListCount; ++i)
            {
                Dictionary<int, int> refDict = RefList[i];
                foreach (KeyValuePair<int, int> kvp in refDict)
                {
                    int slotId = kvp.Key;
                    if (sellAmtToSlotIdDict.ContainsKey(slotId))
                        sellAmtToSlotIdDict[slotId] += kvp.Value;
                    else
                        sellAmtToSlotIdDict.Add(slotId, kvp.Value);
                }
            }
            RPCFactory.CombatRPC.MassSellItems(sellAmtToSlotIdDict);
        }
    }

    public void OnClickClosePanel()
    {
        gameObject.SetActive(false);
        RemoveAllFromSellpanel();     
    }

    public void OnOpenDialogItemSellUse(int displayItemIdx)
    {
        UIManager.OpenDialog(WindowType.DialogItemSellUse, (GameObject window) => {
            window.GetComponent<UI_DialogItemSellUse>().Init(UIInventory.DisplayItemList[displayItemIdx].item,
                (int amount) => {
                    AddItemToSellPanel(displayItemIdx, amount);
                });
        });
    }
}
