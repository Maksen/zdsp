using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class UIShopBarter : UIShop
{	
	void Start ()
    {
        storetype = NPCStoreInfo.StoreType.Barter;
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
                int itemId = item.ItemID;
                IInventoryItem invItem = GameRepo.ItemFactory.GetInventoryItem(itemId);
                if (invItem == null)
                {
                    Debug.LogWarningFormat("Item id: {0} not found", itemId);
                    continue;
                }
                item.data = invItem;

                if (item.Show == false) continue;
				if (System.DateTime.Now > item.EndTime || System.DateTime.Now < item.StartTime)
					continue;

                if (item.Type != NPCStoreInfo.ItemStoreType.Barter)
                    continue;

				var icon = Instantiate(itemicon_prefab, itemlistparent, false);
                ItemBaseJson itemJson = invItem.JsonObject;
                var shopitem = icon.GetComponent<ShopItem>();
                shopitem.itemname.text = itemJson.name;
                shopitem.selectionEnabled.onEnabled = OnItemSelected;
                shopitem.selectionEnabled.onDisabled = OnItemDeselected;
                shopitem.itemdata = item;

                ItemGameIconType iconType = invItem.ItemSortJson.gameicontype;
                GameObject gameIcon = Instantiate(ClientUtils.LoadGameIcon(iconType));
                gameIcon.transform.SetParent(shopitem.itemicon_parent, false);
                ClientUtils.InitGameIcon(gameIcon, invItem, itemId, iconType, invItem.StackCount, true);

                currentitemlist.Add(shopitem);
            }
        }
    }
}
