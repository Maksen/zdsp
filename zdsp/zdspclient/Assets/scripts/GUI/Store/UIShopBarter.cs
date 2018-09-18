using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class UIShopBarter : UIShop
{	
	void Start ()
    {
        storetype = NPCStoreInfo.StoreType.Barter;
        SortIcons();
    }

    override public void init(NPCStoreInfo store)
    {
        List<NPCStoreInfo.StandardItem> arg = new List<NPCStoreInfo.StandardItem>();

        foreach (var item in store.inventory)
        {
            arg.Add(item.Value);
        }

        init(store.NameCT, arg.ToArray());
    }

    public override void init(string storename, NPCStoreInfo.StandardItem[] newitemlist)
    {
        shopname.text = storename;
        storetype = NPCStoreInfo.StoreType.Barter;

        detailswindow.gameObject.SetActive(false);
        ClearShopList();

        if (GameInfo.gLocalPlayer != null)
        {
            heldcurrency.text = GameInfo.gLocalPlayer.SecondaryStats.Money.ToString();
            heldauctioncurrency.text = GameInfo.gLocalPlayer.SecondaryStats.Gold.ToString();
        }

        if (itemlistparent != null && itemicon_prefab != null && newitemlist != null && newitemlist.Length > 0)
        {
            currentitemlist = new List<ShopItem>();

            foreach (var item in newitemlist)
            {
                var solditem = item;
                item.data = GameRepo.ItemFactory.GetInventoryItem(item.ItemID);

                if (item.data == null)
                {
                    Debug.LogWarning("Item id: " + item.ItemID.ToString() + " not found");
                    continue;
                }

				if (item.Show == false) continue;
				if (System.DateTime.Now > item.EndTime || System.DateTime.Now < item.StartTime)
					continue;

				var icon = Instantiate(itemicon_prefab, itemlistparent);

                var shopitem = icon.GetComponent<ShopItem>();
                shopitem.itemname.text = item.data.JsonObject.name;
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
                    case BagType.Socket:
                        ((GameIcon_DNA)itemicon).InitWithToolTipView(itemId, 0, 0);
                        break;
                }

                currentitemlist.Add(shopitem);
            }
        }
    }
}
