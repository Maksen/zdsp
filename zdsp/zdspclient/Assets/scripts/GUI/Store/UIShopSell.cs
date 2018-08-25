using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public abstract class UIShop : MonoBehaviour
{
    public class IconTypes
    {
        public static string[] types = new string[] { "equip", "material", "consumable", "buffdebuff", "DNA" };
        public static string Equip = types[0];
        public static string Material = types[1];
        public static string Consumable = types[2];
        public static string BuffDebuff = types[3];
        public static string DNA = types[4];
    }

    public NPCStoreInfo.StoreType storetype;

    public Transform itemlistparent = null;
    public GameObject itemicon_prefab = null;
    public UIShopDetails detailswindow;    
    public List<GameObject> GameIconPrefabs;
    public Text shopname, heldcurrency, heldauctioncurrency;

    public int id;

    protected List<ShopItem> currentitemlist = null;
    protected ShopItem current_item_selection = null;

    abstract public void init(NPCStoreInfo store);
    abstract public void init(string storename, NPCStoreInfo.StandardItem[] newitemlist);

    public Dictionary<string, GameObject> SortedGameIcons = new Dictionary<string, GameObject>();
    protected void SortIcons()
    {
        if (GameIconPrefabs.Count > 0)
        {
            foreach (var icon in GameIconPrefabs)
            {
                foreach (var type in IconTypes.types)

                    if (icon.name.ToLower().Contains(type.ToLower()))
                    {
                        SortedGameIcons.Add(type, icon);
                    }
            }
        }
    }

    public void UpdateCurrencyDisplay()
    {
        if (GameInfo.gLocalPlayer != null)
        {
            heldcurrency.text = GameInfo.gLocalPlayer.SecondaryStats.money.ToString();
            heldauctioncurrency.text = GameInfo.gLocalPlayer.SecondaryStats.gold.ToString();
        }
    }

    public void SignalTransactionStatus(string message)
    {
        UIManager.OpenOkDialog(message, null);

        RPCFactory.NonCombatRPC.NPCStoreInit(id);
        RPCFactory.NonCombatRPC.NPCStoreGetPlayerTransactions(id);

        if (message == "Transaction success")
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
            if (storeitem.Type == NPCStoreInfo.ItemStoreType.Normal)
            {
                var itemkey = storeitem.Key();

				if (transactions.ContainsKey(itemkey))
				{
					storeitem.Remaining = transactions[itemkey].remaining;

					detailswindow.UpdateSelectedItemRemaining();
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

    protected GameObject GetItemIconPrefab(ItemType type)
    {
        GameObject itemprefab = null;

        switch (type)
        {
            case ItemType.Equipment:
                itemprefab = SortedGameIcons[IconTypes.Equip];
                break;

            case ItemType.Material:
                itemprefab = SortedGameIcons[IconTypes.Material];
                break;

            case ItemType.PotionFood:
                itemprefab = SortedGameIcons[IconTypes.Consumable];
                break;

            case ItemType.DNA:
                itemprefab = SortedGameIcons[IconTypes.DNA];
                break;

            default:
                itemprefab = SortedGameIcons[IconTypes.Consumable];
                break;
        }

        return itemprefab;
    }

    protected void OnItemSelected(GameObject selecteditem)
    {
        foreach (var item in currentitemlist)
        {
            if (item.selectionEnabled.gameObject != selecteditem)
            {
                item.GetComponent<Toggle>().isOn = false;
            }
            else
            {
                if (item.selectionEnabled.gameObject.activeSelf)
                {
                    GameObject itemprefab = GetItemIconPrefab(item.itemdata.data.JsonObject.itemtype);                    

                    current_item_selection = item;

                    detailswindow.itemiconprefab = itemprefab;
                    detailswindow.gameObject.SetActive(true);
                    detailswindow.selecteditem = current_item_selection.itemdata;

                    var animator = detailswindow.gameObject.GetComponent<Animator>();
                    animator.enabled = true;
                    switch (storetype)
                    {
                        case NPCStoreInfo.StoreType.Normal:
                            ((UIShopSellDetails)detailswindow).init(item);                            
                            animator.Play("ShopSellItemDetail_Open");
                            break;

                        case NPCStoreInfo.StoreType.Barter:
                            ((UIShopBarterDetails)detailswindow).init(item);
                            animator.Play("ShopBarterItemDetail_Open");
                            break;
                    }                                                            
                }
            }
        }
    }

    protected void OnItemDeselected(GameObject selecteditem)
    {
        foreach (var item in currentitemlist)
        {
            if (item.selectionEnabled.gameObject.activeSelf)
                return;
        }

        var animator = detailswindow.gameObject.GetComponent<Animator>();
        animator.enabled = true;
        detailswindow.gameObject.SetActive(true);
        switch (storetype)
        {
            case NPCStoreInfo.StoreType.Normal:
                animator.Play("ShopSellItemDetail_Close");
                break;

            case NPCStoreInfo.StoreType.Barter:
                animator.Play("ShopBarterItemDetail_Close");
                break;
        }        
    }
};

public class UIShopSell : UIShop
{
    void Start ()
    {
        storetype = NPCStoreInfo.StoreType.Normal;

        SortIcons();        
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

    override public void init(string storename, NPCStoreInfo.StandardItem[] newitemlist)
    {
        shopname.text = storename;
        storetype = NPCStoreInfo.StoreType.Normal;

        detailswindow.gameObject.SetActive(false);
        ClearShopList();
        UpdateCurrencyDisplay();

        if (itemlistparent != null && newitemlist != null && newitemlist.Length > 0)
        {            
            currentitemlist = new List<ShopItem>();

            foreach (var item in newitemlist)
            {                
                var solditem = (NPCStoreInfo.StandardItem)item;
                item.data = GameRepo.ItemFactory.GetInventoryItem(item.ItemID);

                if (item.data == null)
                {
                    Debug.LogWarning("Item id: " + item.ItemID.ToString() + " not found");
                    continue;
                }

				if (item.Show == false) continue;

				var icon = Instantiate(itemicon_prefab, itemlistparent);

                var shopitem = icon.GetComponent<ShopItem>();
                shopitem.itemname.text = item.data.JsonObject.name;
                shopitem.price.text = solditem.SoldValue.ToString();
                shopitem.currencyicon.type = (CurrencyType)solditem.SoldType;
                shopitem.selectionEnabled.onEnabled = OnItemSelected;
                shopitem.selectionEnabled.onDisabled = OnItemDeselected;
                shopitem.itemdata = item;

                var iconprefab = GetItemIconPrefab(item.data.JsonObject.itemtype);
                var itemicon = Instantiate(iconprefab, shopitem.itemicon_parent).GetComponent<GameIcon_Base>();
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

                currentitemlist.Add(shopitem);
            }
        }        
    }        
}
