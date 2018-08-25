using System.Collections.Generic;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UIShopSellDetails : UIShopDetails
{
    public List<CurrencyIcon> currencyicons;
    public Text unitcost;
    public Text totalcost;
    public Text discount;

    void Start()
    {
        purchasequantitywidget.onValueChanged.AddListener(delegate { UpdateTotalCost(); });

        if (discount != null) discount.text = "沒折扣";
    }

    public void init(ShopItem item)
    {
        if (item.itemdata.Type == NPCStoreInfo.ItemStoreType.Normal)
        {
            var standarditem = (NPCStoreInfo.StandardItem)item.itemdata;

            purchasequantitywidget.Value = 0;
            totalcost.text = "0";
            unitcost.text = standarditem.SoldValue.ToString();

            switch (standarditem.DailyOrWeekly)
            {
                case NPCStoreInfo.Frequency.Daily:
                    purchasequantitywidget.Max = standarditem.Remaining;
                    dailyorweekly.text = GUILocalizationRepo.mLocalizedStrIdMap[291].localizedval;
                    limitamountvalue.text = standarditem.Remaining.ToString() + " / " + standarditem.ExCount.ToString();

                    selecteditem = item.itemdata;
                    UpdateSelectedItemRemaining();
                    break;

                case NPCStoreInfo.Frequency.Weekly:
                    purchasequantitywidget.Max = standarditem.Remaining;
                    dailyorweekly.text = GUILocalizationRepo.mLocalizedStrIdMap[292].localizedval;
                    limitamountvalue.text = standarditem.Remaining.ToString() + " / " + standarditem.ExCount.ToString();

                    selecteditem = item.itemdata;
                    UpdateSelectedItemRemaining();
                    break;

                case NPCStoreInfo.Frequency.Unlimited:
                    purchasequantitywidget.Max = 99;
                    dailyorweekly.text = GUILocalizationRepo.mLocalizedStrIdMap[291].localizedval;
                    limitamountvalue.text = "無限";
                    break;
            }

            purchasequantitywidget.Value = 0;

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
                    case BagType.DNA:
                        ((GameIcon_DNA)itemicon).InitWithToolTipView(itemId, 0, 0);
                        break;
                }
            }

            foreach (var currencyicon in currencyicons)
            {
                currencyicon.type = item.currencyicon.type;
            }
        }

        base.init(item);
    }

    new void UpdateTotalCost()
    {
        if (selecteditem.Type == NPCStoreInfo.ItemStoreType.Normal)
        {
            var standarditem = (NPCStoreInfo.StandardItem)selecteditem;
            totalcost.text = (purchasequantitywidget.Value * standarditem.SoldValue).ToString();
        }
        base.UpdateTotalCost();
    }
}
