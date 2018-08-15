using System;
using System.Collections;
using System.Collections.Generic;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UIShopDetails : MonoBehaviour
{
    public Transform itemicon_parent;
    public GameObject itemiconprefab;
    public GameIcon_Base itemicon;

    public Text itemname, itemtype;
    public Text dailyorweekly;
    public Text limitamountvalue;
    public Text purchasequantity;
    public Spinner purchasequantitywidget;
    public List<CurrencyIcon> currencyicons;
    public Text unitcost;
    public Text totalcost;
    public Text discount;
    public Text unitsowned;

    public GameObject description;
    public GameObject description_value;
    public GameObject description_multiline;
    public GameObject description_line;
    List<GameObject> description_objects = new List<GameObject>();

    public Button buybutton;

    public NPCStoreInfo.Item selecteditem;

    // Use this for initialization
    void Start ()
    {
        purchasequantitywidget.onValueChanged.AddListener(delegate { UpdateTotalCost(); });
        
        if (discount != null) discount.text = "沒折扣";
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void UpdateItemsOwned(ShopItem item)
    {
        if (GameInfo.gLocalPlayer == null || item == null)
        {
            unitsowned.text = "0";
            return;
        }

        var inv = GameInfo.gLocalPlayer.clientItemInvCtrl.itemInvData.Slots;
        unitsowned.text = "";
        foreach (var playeritem in inv)
        {
            if (playeritem!= null && playeritem.ItemID == item.itemdata.ItemID)
            {
                unitsowned.text = playeritem.StackCount.ToString();
                break;
            }
        }
        if (unitsowned.text.Length == 0)
            unitsowned.text = "0";
    }

    public void init(ShopItem item)
    {
        UpdateItemsOwned(item);

        itemname.text = item.itemname.text;
        itemtype.text = item.itemdata.data.JsonObject.itemtype.ToString();
        //description.text = item.itemdata.data.JsonObject.description;

        foreach (var obj in description_objects)
        {
            Destroy(obj);
        }
        description_objects.Clear();

        var desc = item.itemdata.data.JsonObject.description;
        if (desc.Length > 0)
        {
            var newline = Instantiate(description_multiline, description.transform);
            newline.GetComponentInChildren<Text>().text = desc;
            description_objects.Add(newline);
        }
        description_objects.Add(Instantiate(description_line, description.transform));

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

                itemicon.Init(item.itemdata.data.ItemID);
            }

            foreach (var currencyicon in currencyicons)
            {
                currencyicon.type = item.currencyicon.type;
            }
        }

        buybutton.onClick.RemoveAllListeners();
        buybutton.onClick.AddListener(new UnityEngine.Events.UnityAction(BuySelectedItem));
    }

    void UpdateTotalCost()
    {
        if (selecteditem.Type == NPCStoreInfo.ItemStoreType.Normal)
        {
            var standarditem = (NPCStoreInfo.StandardItem)selecteditem;
            totalcost.text = (purchasequantitywidget.Value * standarditem.SoldValue).ToString();
        }

        buybutton.enabled = true;
        buybutton.interactable = purchasequantitywidget.Value > 0;
    }

    public void UpdateSelectedItemRemaining()
    {
        if (selecteditem != null)
        {
            var item = NPCStoreInfo.StandardItem.GetFromBase(selecteditem);

            purchasequantitywidget.Max = item.Remaining;
            purchasequantitywidget.Value = 0;
            UpdateTotalCost();
        }
    }

    public int last_purchase_quantity = 0;
    public void BuySelectedItem()
    {
        last_purchase_quantity = purchasequantitywidget.Value;
        RPCFactory.NonCombatRPC.NPCStoreBuy(selecteditem.StoreID, selecteditem.ItemListID, purchasequantitywidget.Value, GameInfo.gLocalPlayer.Name);
    }
}
