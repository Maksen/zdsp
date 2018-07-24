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
    //public class Item
    //{
    //    public string name;

    //    public int id;
    //    public int value;
    //    public char soldtype;
    //    public int soldvalue;
    //    public float discount;
    //    public int sortnumber;
    //    public float probability;
    //    public int levelrequired;
    //    public DateTime starttime;
    //    public DateTime endtime;
    //    public int excount;
    //    public char dailyorweekly;
    //    public IInventoryItem data;
    //};

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

    public void init(List<NPCStoreInfo.StandardItem> newitemlist, NPCStoreInfo.StoreType store_type)
    {
        // GameInfo.gLocalPlayer;

        storetype = store_type;

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
}
