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

        if (discount != null) discount.text = GUILocalizationRepo.GetLocalizedString("discount") + ": 0";
    }

    public void init(ShopItem item)
    {
        if (item.itemdata.Type == NPCStoreInfo.ItemStoreType.Normal)
        {
            var standarditem = item.itemdata;

            purchasequantitywidget.Value = 0;
            totalcost.text = "0";
            unitcost.text = standarditem.DiscountedPrice().ToString();

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
                    limitamountvalue.text = GUILocalizationRepo.GetLocalizedString("unlimited");
                    //limitamountvalue.text = "無限";
                    break;
            }

            purchasequantitywidget.Value = 0;

            discount.text = string.Format("{0}: {1:0}%", GUILocalizationRepo.GetLocalizedString("discount"), item.itemdata.Discount);

            if (itemiconprefab != null)
            {
                if (itemicon != null)
                    Destroy(itemicon.gameObject);

                itemicon = Instantiate(itemiconprefab, itemicon_parent, false).GetComponent<GameIcon_Base>();
                IInventoryItem invItem = item.itemdata.data;
                ClientUtils.InitGameIcon(itemicon.gameObject, invItem, invItem.ItemID, invItem.ItemSortJson.gameicontype, invItem.StackCount, true);             
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
            var cost = purchasequantitywidget.Value * standarditem.DiscountedPrice();
            totalcost.text = cost.ToString();
        }
        base.UpdateTotalCost();
    }
}
