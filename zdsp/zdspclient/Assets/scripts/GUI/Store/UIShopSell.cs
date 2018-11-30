using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public abstract class UIShop : MonoBehaviour
{
    //public NPCStoreInfo.ItemStoreType storetype;

    public Transform itemlistparent = null;
    protected GameObject itemicon_prefab = null;
    protected UIShopDetails detailswindow;    
    public Text shopname, heldcurrency, heldauctioncurrency;

    public int id;

    protected List<ShopItem> currentitemlist = null;
    protected ShopItem current_item_selection = null;

    abstract public void init(NPCStoreInfo store);
    abstract public void init(string storename, NPCStoreInfo.StandardItem[] newitemlist);

    public void UpdateCurrencyDisplay()
    {
        if (GameInfo.gLocalPlayer != null)
        {
            heldcurrency.text = GameInfo.gLocalPlayer.SecondaryStats.Money.ToString();
            heldauctioncurrency.text = GameInfo.gLocalPlayer.SecondaryStats.Gold.ToString();
        }
    }

    public void SignalTransactionStatus(string message)
    {
        UIManager.OpenOkDialog(message, null);

        RPCFactory.NonCombatRPC.NPCStoreInit(id);
        RPCFactory.NonCombatRPC.NPCStoreGetPlayerTransactions(id);

        if (message == GUILocalizationRepo.GetLocalizedSysMsgByName("Transaction success"))
        {
            var item = current_item_selection.itemdata;

            if (item.DailyOrWeekly != NPCStoreInfo.Frequency.Unlimited)
                item.Remaining -= detailswindow.last_purchase_quantity;

            detailswindow.init(current_item_selection);
        }
    }

    Dictionary<string, NPCStoreInfo.Transaction> playertransactions = null;
    public void UpdateTransactions(Dictionary<string, NPCStoreInfo.Transaction> transactions)
    {
        playertransactions = transactions;

        if (playertransactions == null)
        {
            // signal null transaction history returned
            return;
        }

        foreach (var item in currentitemlist)
        {
            var storeitem = item.itemdata;
            //if (storeitem.Type == NPCStoreInfo.ItemStoreType.Normal)
            {
                var itemkey = storeitem.Key();

				if (transactions.ContainsKey(itemkey))
				{
					storeitem.Remaining = transactions[itemkey].remaining;

					if(detailswindow != null) detailswindow.UpdateSelectedItemRemaining();
					//switch (storetype)
					//{
					//	case NPCStoreInfo.StoreType.Normal:
					//		((UIShopSellDetails)detailswindow).UpdateSelectedItemRemaining();
					//		break;

					//	case NPCStoreInfo.StoreType.Barter:
					//		((UIShopBarterDetails)detailswindow).UpdateSelectedItemRemaining();
					//		break;
					//}
				}
            }
        }
    }

    protected void ClearShopList()
    {
        if (currentitemlist != null)
        {
            foreach (var item in currentitemlist)
            {
                Destroy(item.gameObject);
            }

            currentitemlist = null;
        }
    }

    ShopItem GetItemFromEnableScript(GameObject selecteditem, bool toggleoff = true)
    {
        ShopItem ret = null;
        foreach (var item in currentitemlist)
        {
            if (item.selectionEnabled.gameObject != selecteditem)
            {
                if (toggleoff) item.GetComponent<Toggle>().isOn = false;
            }
            else
            {
                ret = item;
            }
        }

        return ret;
    }

    protected virtual void OnItemSelected(GameObject selecteditem)
    {
        var item = GetItemFromEnableScript(selecteditem, true);
        
        if (item.selectionEnabled.gameObject.activeSelf)
        {
            ItemSortJson itemSortJson = GameRepo.ItemFactory.GetItemSortById(item.itemdata.data.JsonObject.itemsort);
            GameObject itemprefab = ClientUtils.LoadGameIcon(itemSortJson.gameicontype);

            current_item_selection = item;

            detailswindow.itemiconprefab = itemprefab;
            detailswindow.gameObject.SetActive(true);
            detailswindow.selecteditem = current_item_selection.itemdata;

            var animator = detailswindow.gameObject.GetComponent<Animator>();
            animator.enabled = true;
            switch (item.itemdata.Type)
            {
                case NPCStoreInfo.ItemStoreType.Normal:
                    ((UIShopSellDetails)detailswindow).init(item);                            
                    animator.Play("ShopSellItemDetail_Open");
                    break;

                case NPCStoreInfo.ItemStoreType.Barter:
                    ((UIShopBarterDetails)detailswindow).init(item);
                    animator.Play("ShopBarterItemDetail_Open");
                    break;
            }                                                            
        }    
    }

    protected void OnItemDeselected(GameObject selecteditem)
    {
        var item = GetItemFromEnableScript(selecteditem, false);

        var animator = detailswindow.gameObject.GetComponent<Animator>();
        animator.enabled = true;
        detailswindow.gameObject.SetActive(true);
        switch (item.itemdata.Type)
        {
            case NPCStoreInfo.ItemStoreType.Normal:
                animator.Play("ShopSellItemDetail_Close");
                break;

            case NPCStoreInfo.ItemStoreType.Barter:
                animator.Play("ShopBarterItemDetail_Close");
                break;
        }        
    }

    public void RequestShopInfo(int shopid)
    {
        id = shopid;
        GameInfo.gUIShop = this;
        GameInfo.gUIShop.id = id;

        RPCFactory.NonCombatRPC.NPCStoreInit(id);
        RPCFactory.NonCombatRPC.NPCStoreGetPlayerTransactions(id);
    }
};

public class UIShopSell : UIShop
{
    public UIShopBarterDetails BarterDetails;
    public UIShopSellDetails SellDetails;

    public GameObject soldicon_prefab = null;
    public GameObject bartericon_prefab = null;

    void Start ()
    {
        //storetype = NPCStoreInfo.ItemStoreType.Normal;    
	}

    override public void init(NPCStoreInfo store)
    {
        List<NPCStoreInfo.StandardItem> arg = new List<NPCStoreInfo.StandardItem>();

        foreach (var item in store.inventory)
        {
            arg.Add((NPCStoreInfo.StandardItem)item.Value);
        }        

        init(store.NameCT, arg.ToArray());
    }

    override protected void OnItemSelected(GameObject selecteditem)
    {
        ShopItem shopitem = null;

        foreach (var item in currentitemlist)
        {
            if (item.selectionEnabled.gameObject == selecteditem)
            {
                shopitem = item;
            }
        }

        switch (shopitem.itemdata.Type)
        {
            case NPCStoreInfo.ItemStoreType.Barter:
                detailswindow = BarterDetails;
                SellDetails.gameObject.SetActive(false);
                break;

            default:
                detailswindow = SellDetails;
                BarterDetails.gameObject.SetActive(false);
                break;
        }

        base.OnItemSelected(selecteditem);
    }

    override public void init(string storename, NPCStoreInfo.StandardItem[] newitemlist)
    {
        shopname.text = storename;
        //storetype = NPCStoreInfo.ItemStoreType.Normal;

        //detailswindow.gameObject.SetActive(false);
        BarterDetails.gameObject.SetActive(false);
        SellDetails.gameObject.SetActive(false);

        ClearShopList();
        UpdateCurrencyDisplay();

        if (itemlistparent != null && newitemlist != null && newitemlist.Length > 0)
        {            
            currentitemlist = new List<ShopItem>();

            foreach (var item in newitemlist)
            {
                if (item.Show == false)
                    continue;

                if (System.DateTime.Now > item.EndTime || System.DateTime.Now < item.StartTime)
                    continue;

                int itemId = item.ItemID;
                IInventoryItem invItem = GameRepo.ItemFactory.GetInventoryItem(itemId);
                if (invItem == null)
                {
                    Debug.LogWarningFormat("Item id: {0} not found", itemId);
                    continue;
                }               
                var solditem = item;
                    
                item.data = invItem;

                switch (item.Type)
                {
                    case NPCStoreInfo.ItemStoreType.Barter:
                        itemicon_prefab = bartericon_prefab;
                        break;

                    case NPCStoreInfo.ItemStoreType.Normal:
                        itemicon_prefab = soldicon_prefab;
                        break;
                }

                var icon = Instantiate(itemicon_prefab, itemlistparent, false);
                var shopitem = icon.GetComponent<ShopItem>();
                ItemBaseJson itemJson = invItem.JsonObject;
                shopitem.itemname.text = itemJson.name;                      
                shopitem.selectionEnabled.onEnabled = OnItemSelected;
                shopitem.selectionEnabled.onDisabled = OnItemDeselected;
                shopitem.itemdata = item;

                ItemGameIconType iconType = invItem.ItemSortJson.gameicontype;
                GameObject gameIcon = Instantiate(ClientUtils.LoadGameIcon(iconType));
                gameIcon.transform.SetParent(shopitem.itemicon_parent, false);
                ClientUtils.InitGameIcon(gameIcon, invItem, itemId, iconType, invItem.StackCount, true);

                if (item.Type == NPCStoreInfo.ItemStoreType.Normal)
                {
                    shopitem.price.text = solditem.DiscountedPrice().ToString();
                    shopitem.currencyicon.type = (CurrencyType)solditem.SoldType;                    
                }

                currentitemlist.Add(shopitem);
            }
        }        
    }        
}
