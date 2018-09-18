using System;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

class UIShopBarterDetails : UIShopDetails
{
    public GameObject RequiredBarterItemPrefab;
    public Transform RequiredWindowParent;
    private List<GameObject> RequirementEntries = new List<GameObject>();

    public void init(ShopItem item)
    {
        if (item.itemdata.Type == NPCStoreInfo.ItemStoreType.Barter || item.itemdata.required_items.Count > 0)
        {
            var barteritem = item.itemdata;
            purchasequantitywidget.Value = 0;

            switch (barteritem.DailyOrWeekly)
            {
                case NPCStoreInfo.Frequency.Daily:
                    purchasequantitywidget.Max = barteritem.Remaining;
                    dailyorweekly.text = GUILocalizationRepo.mLocalizedStrIdMap[291].localizedval;
                    limitamountvalue.text = barteritem.Remaining.ToString() + " / " + barteritem.ExCount.ToString();

                    selecteditem = item.itemdata;
                    UpdateSelectedItemRemaining();
                    break;

                case NPCStoreInfo.Frequency.Weekly:
                    purchasequantitywidget.Max = barteritem.Remaining;
                    dailyorweekly.text = GUILocalizationRepo.mLocalizedStrIdMap[292].localizedval;					
                    limitamountvalue.text = barteritem.Remaining.ToString() + " / " + barteritem.ExCount.ToString();

                    selecteditem = item.itemdata;
                    UpdateSelectedItemRemaining();
                    break;

                case NPCStoreInfo.Frequency.Unlimited:
                    purchasequantitywidget.Max = 99;
                    dailyorweekly.text = GUILocalizationRepo.mLocalizedStrIdMap[291].localizedval;
                    limitamountvalue.text = GUILocalizationRepo.GetLocalizedString("unlimited");
					//limitamountvalue.text = "無限";
					break;
            }                        

            if (itemiconprefab != null)
            {
                if (itemicon != null)
                    Destroy(itemicon.gameObject);

                itemicon = Instantiate(itemiconprefab, itemicon_parent).GetComponent<GameIcon_Base>();
                IInventoryItem invItem = itemicon.inventoryItem;
                BagType bagType = invItem.JsonObject.bagtype;
                int itemId = invItem.JsonObject.itemid;
                switch (bagType)
                {
                    case BagType.Equipment:
                        ((GameIcon_Equip)itemicon).InitWithToolTipView(itemId, 0, 0, 0);
                        break;
                    case BagType.Consumable:
                    case BagType.Material:
                        ((GameIcon_MaterialConsumable)itemicon).InitWithToolTipView(itemId, invItem.StackCount);
                        break;
                    case BagType.Socket:
                        ((GameIcon_DNA)itemicon).InitWithToolTipView(itemId, 0, 0);
                        break;
                }
            }

            // Determine maximum amount buyable by barter
            List<int> requirements_fulfilled = new List<int>();
            ClearRequirementDisplay();
            foreach (var req in barteritem.required_items)
            {
                var reqitem = GameRepo.ItemFactory.GetInventoryItem(req.ReqItemID);
                if (reqitem == null)
                    continue;

                var entry = Instantiate(RequiredBarterItemPrefab, RequiredWindowParent).GetComponent<BarterRequirement>();
                entry.itemname.text = reqitem.JsonObject.localizedname;

                var reqowned = GetOwnedItemCount(req.ReqItemID);
                entry.required_mytotal.text = reqowned.ToString() + " / " + req.ReqItemValue.ToString();

                RequirementEntries.Add(entry.gameObject);
                requirements_fulfilled.Add(reqowned / req.ReqItemValue);
            }

            int maxstacksbuyable = purchasequantitywidget.Max;
            foreach (var ele in requirements_fulfilled)
            {
                maxstacksbuyable = Math.Min(maxstacksbuyable, ele);
            }

            purchasequantitywidget.Max = maxstacksbuyable;
        }

        base.init(item);
    }

    void ClearRequirementDisplay()
    {
        foreach (var entry in RequirementEntries)
        {
            Destroy(entry);
        }
        RequirementEntries.Clear();
    }
}

