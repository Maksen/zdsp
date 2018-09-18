using Kopio.JsonContracts;
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

    public List<InvDisplayItem> SellItemList { get; set; }
    public List<Dictionary<int, int>> SellRefList = null; // Dict: originSlotId -> amountToSell

    int totalSellPrice = 0;

    // Use this for initialization
    void Awake()
    {
        SellItemList = new List<InvDisplayItem>();
        SellRefList = new List<Dictionary<int, int>>();
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

        UIInventory.ButtonSell.interactable = false;
        UIInventory.ButtonPowerup.interactable = false;
    }

    void OnDisable()
    {
        sellPanelScrollView.Clear();
        ClearSellPanelList();

        UIInventory.ButtonSell.interactable = true;
        UIInventory.ButtonPowerup.interactable = true;
    }

    public void UpdateSellListFromRefList()
    {
        int refListCount = SellRefList.Count;
        for (int i = refListCount-1; i >= 0; --i)
        {
            int stackCount = 0;
            Dictionary<int, int> sellRefDict = SellRefList[i];
            if (sellRefDict.Count > 0)
            {
                foreach (KeyValuePair<int, int> kvp in sellRefDict)
                    stackCount += kvp.Value;
            }
            if (stackCount == 0)
            {
                SellRefList.RemoveAt(i);
                SellItemList.RemoveAt(i);              
            }
            else
                SellItemList[i].DisplayStackCount = stackCount;
        }

        sellPanelScrollView.PopulateRows();
        txtTotalSellVal.text = totalSellPrice.ToString();
    }

    public void AddItemToSellList(IInventoryItem invItem, int originSlotId, int amtToAdd)
    {
        int itemId = invItem.ItemID;
        int maxStackCnt = invItem.MaxStackCount;
        int itemSellPrice = invItem.JsonObject.sellprice;

        int sellItemListCnt = SellItemList.Count;
        for (int i = 0; i < sellItemListCnt; ++i)
        {
            if (amtToAdd == 0)
                break;

            InvDisplayItem sellItem = SellItemList[i];
            int sellStackCnt = sellItem.DisplayStackCount;
            if (sellItem.InvItem.ItemID == itemId && sellStackCnt < maxStackCnt)
            {
                int amtCanAdd = amtToAdd;
                if (sellStackCnt + amtToAdd > maxStackCnt)
                    amtCanAdd = maxStackCnt - sellStackCnt;

                amtToAdd -= amtCanAdd; // Leftover to next slot
                Dictionary<int, int> refDict = SellRefList[i];
                if (refDict.ContainsKey(originSlotId))
                    refDict[originSlotId] += amtCanAdd;
                else
                    refDict.Add(originSlotId, amtCanAdd);

                SellItemList[i].DisplayStackCount += amtCanAdd;             
                totalSellPrice += itemSellPrice * amtCanAdd;
            }
        }

        // If there is still leftover to add
        if (amtToAdd > 0)
        {
            SellRefList.Add(new Dictionary<int, int>() { { originSlotId, amtToAdd } });
            SellItemList.Add(new InvDisplayItem { OriginSlotId = originSlotId, InvItem = invItem,
                                                  OriginStackCount = invItem.StackCount,
                                                  DisplayStackCount = amtToAdd }); ;
            totalSellPrice += itemSellPrice * amtToAdd;
        }
    }

    public void ClearSellPanelList()
    {
        SellRefList.Clear();
        SellItemList.Clear();
    }

    public void AddItemToSellPanel(int displayItemIdx, int amtToSell)
    {
        if (amtToSell == 0)
            return;

        List<InvDisplayItem> displayItemList = UIInventory.DisplayItemList;
        InvDisplayItem invDisplayItem = displayItemList[displayItemIdx];
        invDisplayItem.DisplayStackCount -= amtToSell; // Remove amount from inv displayItem stackcount
        AddItemToSellList(invDisplayItem.InvItem, invDisplayItem.OriginSlotId, amtToSell);
        if (invDisplayItem.DisplayStackCount == 0 && UIInventory.CurrentInventoryTab != BagType.Any)
            displayItemList.RemoveAt(displayItemIdx);

        UIInventory.UpdateVisibleInvRows(false);
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
                IInventoryItem invItem = invItemList[i];
                if (invItem != null && invItem.JsonObject.rarity == rarity)
                {
                    int amtLeftToAdd = invItem.StackCount, maxStackCnt = invItem.MaxStackCount;
                    int sellRefListCnt = SellRefList.Count;
                    for (int j = 0; j < sellRefListCnt; ++j)
                    {
                        Dictionary<int, int> refDict = SellRefList[j];
                        if (refDict.ContainsKey(i))
                            amtLeftToAdd -= refDict[i];
                    }
                    AddItemToSellList(invItem, i, amtLeftToAdd);
                }
            }

            UIInventory.RefreshRight(player);
        }
    }

    public void RemoveFromSellPanelByIndex(int index)
    {
        SellRefList.RemoveAt(index);
        InvDisplayItem sellItem = SellItemList[index];
        totalSellPrice -= sellItem.InvItem.JsonObject.sellprice * sellItem.DisplayStackCount;
        SellItemList.RemoveAt(index);

        UIInventory.UpdateVisibleInvRows(false);
    }

    public void RemoveFromSellPanelByRarity(ItemRarity rarity)
    {
        int sellItemCnt = SellItemList.Count;
        for (int i = sellItemCnt-1; i >= 0; --i)
        {
            InvDisplayItem sellItem = SellItemList[i];
            ItemBaseJson itemJson = sellItem.InvItem.JsonObject;
            if (itemJson.rarity == rarity)
            {
                SellRefList.RemoveAt(i);
                totalSellPrice -= itemJson.sellprice * sellItem.DisplayStackCount;
                SellItemList.RemoveAt(i);
                sellItemCnt = SellItemList.Count;
            }
        }

        UIInventory.UpdateVisibleInvRows(false);
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
        int sellRefListCount = SellRefList.Count;
        if (sellRefListCount > 0)
        {
            string message = GUILocalizationRepo.GetLocalizedString("inv_ConfirmSell");
            UIManager.OpenYesNoDialog(message, () =>
            {
                Dictionary<int, int> sellAmtToSlotIdDict = new Dictionary<int, int>();
                for (int i = 0; i < sellRefListCount; ++i)
                {
                    Dictionary<int, int> refDict = SellRefList[i];
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

                gameObject.SetActive(false); // Close sell panel when sell items
            });
        }
    }

    public void OnClickClosePanel()
    {
        gameObject.SetActive(false);
        ClearSellPanelList();
        UIInventory.UpdateVisibleInvRows(false);
    }

    public void OnOpenDialogItemSellUse(int displayItemIdx)
    {
        UIManager.OpenDialog(WindowType.DialogItemSellUse, (GameObject window) => {
            window.GetComponent<UI_DialogItemSellUse>().Init(UIInventory.DisplayItemList[displayItemIdx],
                (int amount) => { AddItemToSellPanel(displayItemIdx, amount); });
        });
    }
}
