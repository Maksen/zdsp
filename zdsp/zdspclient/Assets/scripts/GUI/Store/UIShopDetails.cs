using System.Collections.Generic;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public abstract class UIShopDetails : MonoBehaviour 
{
    public Transform itemicon_parent;

    [HideInInspector]
    public GameObject itemiconprefab;

    [HideInInspector]
    public GameIcon_Base itemicon;

    public Text itemname, itemtype;
    public Text dailyorweekly;
    public Text limitamountvalue;
    public Spinner purchasequantitywidget;
    
    public Text unitsowned;

    public GameObject description;
    public GameObject description_value;
    public GameObject description_multiline;
    public GameObject description_line;
    List<GameObject> description_objects = new List<GameObject>();

    public Button buybutton;

    public NPCStoreInfo.Item selecteditem;

    // Use this for initialization
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int GetOwnedItemCount(int id)
    {
        if (GameInfo.gLocalPlayer == null)
        {
            return 0;
        }

        var inv = GameInfo.gLocalPlayer.clientItemInvCtrl.itemInvData;

        int numowned = 0;
        for (int i = 0; i < inv.Slots.Count; ++i)
        {
            var playeritem = inv.Slots[i];

            if (playeritem != null && playeritem.ItemID == id)
            {
                numowned += inv.GetStackcountOnSlot(i);
            }
        }

        return numowned;
    }

    void UpdateItemsOwned(ShopItem item)
    {        
        unitsowned.text = GetOwnedItemCount(item.itemdata.ItemID).ToString();
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
        
        buybutton.onClick.RemoveAllListeners();
        buybutton.onClick.AddListener(new UnityEngine.Events.UnityAction(BuySelectedItem));
    }
    
    public int last_purchase_quantity = 0;
    public void BuySelectedItem()
    {
        last_purchase_quantity = purchasequantitywidget.Value;
        RPCFactory.NonCombatRPC.NPCStoreBuy(selecteditem.StoreID, selecteditem.ItemListID, purchasequantitywidget.Value, GameInfo.gLocalPlayer.Name);
    }

    public void UpdateSelectedItemRemaining()
    {
        if (selecteditem != null)
        {
            purchasequantitywidget.Max = selecteditem.Remaining;
            purchasequantitywidget.Value = 0;
            UpdateTotalCost();
        }
    }

    protected void UpdateTotalCost()
    {        
        buybutton.enabled = true;
        buybutton.interactable = purchasequantitywidget.Value > 0;
    }
}
