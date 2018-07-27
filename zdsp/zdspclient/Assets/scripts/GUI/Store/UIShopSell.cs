using System;
using System.Collections;
using System.Collections.Generic;
using UIAddons;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UIShopSell : MonoBehaviour
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

    public Transform itemlistparent = null;
    public GameObject itemicon_prefab = null;
    public UIShopDetails detailswindow;
    public Dictionary<string, GameObject> SortedGameIcons = new Dictionary<string, GameObject>();
    public List<GameObject> GameIconPrefabs;
    public Text heldcurrency, heldauctioncurrency;

    List<ShopItem> currentitemlist = null;

    NPCStoreInfo.StoreType storetype;    

    // Use this for initialization
    void Start ()
    {
        if (GameIconPrefabs.Count > 0)
        {
            foreach (var icon in GameIconPrefabs)
            {
                foreach(var type in IconTypes.types)

                if (icon.name.ToLower().Contains(type.ToLower()))
                {
                    SortedGameIcons.Add(type, icon);
                }
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void init(Dictionary<int, NPCStoreInfo.StandardItem> newitemlist, NPCStoreInfo.StoreType store_type)
    {
        List<NPCStoreInfo.StandardItem> arg = new List<NPCStoreInfo.StandardItem>();

        foreach (var item in newitemlist)
        {
            arg.Add(item.Value);
        }

        init(arg, store_type);
    }

    public void init(List<NPCStoreInfo.StandardItem> newitemlist, NPCStoreInfo.StoreType store_type)
    {
        storetype = store_type;

        if (GameInfo.gLocalPlayer != null)
        {
            heldcurrency.text = GameInfo.gLocalPlayer.SecondaryStats.money.ToString();
            heldauctioncurrency.text = GameInfo.gLocalPlayer.SecondaryStats.gold.ToString();
        }

        if (itemlistparent != null && itemicon_prefab != null && newitemlist != null && newitemlist.Count > 0)
        {
            ClearShopList();
            currentitemlist = new List<ShopItem>();

            foreach (var item in newitemlist)
            {                
                var solditem = (NPCStoreInfo.StandardItem)item;
                item.data = GameRepo.ItemFactory.GetInventoryItem(item.ItemID);

                var icon = Instantiate(itemicon_prefab, itemlistparent);

                var shopitem = icon.GetComponent<ShopItem>();
                shopitem.itemname.text = item.data.JsonObject.name;
                shopitem.price.text = solditem.SoldValue.ToString();
                shopitem.currencyicon.type = (CurrencyType)solditem.SoldType;
                shopitem.selectionEnabled.onEnabled = OnItemSelected;
                shopitem.selectionEnabled.onDisabled = OnItemDeselected;
                shopitem.itemicon.Init(item.data.ItemID);
                shopitem.itemdata = item;
                
                currentitemlist.Add(shopitem);
            }
        }
    }

    void ClearShopList()
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

    void OnItemSelected(GameObject selecteditem)
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
                    GameObject itemprefab = null;

                    switch (item.itemdata.data.JsonObject.itemtype)
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

                    detailswindow.itemiconprefab = itemprefab;
                    detailswindow.gameObject.SetActive(true);
                    detailswindow.init(item);                    

                    var animator = detailswindow.gameObject.GetComponent<Animator>();

                    animator.enabled = true;
                    animator.Play("ShopSellItemDetail_Open");
                }
            }
        }
    }

    void OnItemDeselected(GameObject selecteditem)
    {
        foreach (var item in currentitemlist)
        {
            if (item.selectionEnabled.gameObject.activeSelf)
                return;
        }

        detailswindow.gameObject.SetActive(true);
        var animator = detailswindow.gameObject.GetComponent<Animator>();

        animator.enabled = true;
        animator.Play("ShopSellItemDetail_Close");        
    }

    public void SignalBuySuccess()
    {
        UIManager.OpenOkDialog("Transaction success", null);
    }

    Dictionary<string, NPCStoreInfo.Transaction> playertransactions = null;
    public void UpdateTransactions(Dictionary<string, NPCStoreInfo.Transaction> transactions)
    {
        playertransactions = transactions;

        foreach (var item in currentitemlist)
        {
            var storeitem = item.itemdata;
            if (storeitem.Type == NPCStoreInfo.ItemStoreType.Normal)
            {
                var itemkey = storeitem.Key();

                if (transactions.ContainsKey(itemkey))
                {
                    ((NPCStoreInfo.StandardItem)storeitem).Remaining = transactions[itemkey].remaining;
                    detailswindow.UpdateSelectedItemRemaining();
                }
            }
        }
    }
}
