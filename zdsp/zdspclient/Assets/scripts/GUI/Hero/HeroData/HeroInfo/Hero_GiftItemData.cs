using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Hero_GiftItemData : MonoBehaviour
{
    [SerializeField] Transform iconTransform;
    [SerializeField] GameObject iconPrefab;
    [SerializeField] Text itemNameText;
    [SerializeField] Text itemDescText;

    private Action<int> OnSendItemCallback;
    private int itemId;
    private GameIcon_MaterialConsumable item;

    public void Init(int itemid, int itemcount, Action<int> callback)
    {
        itemId = itemid;
        if (item == null)
        {
            GameObject icon = ClientUtils.CreateChild(iconTransform, iconPrefab);
            item = icon.GetComponent<GameIcon_MaterialConsumable>();
        }
        item.InitWithoutCallback(itemid, itemcount);
        ItemBaseJson itemBaseJson = item.InventoryItem.JsonObject;
        itemNameText.text = itemBaseJson.localizedname;
        itemDescText.text = itemBaseJson.description;
        OnSendItemCallback = callback;
    }

    public void OnClickSend()
    {
        if (OnSendItemCallback != null)
            OnSendItemCallback(itemId);
    }

    public void OnUsed(int countLeft)
    {
        item.SetStackCount(countLeft);
    }
}