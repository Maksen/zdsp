using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class GameIcon_Base : MonoBehaviour
{
    [SerializeField]
    protected Button button = null;

    [SerializeField]
    protected Image imgIcon = null;

    [SerializeField]
    protected GameIconCmpt_Rarity itemRarity = null;

    [SerializeField]
    protected GameObject newDot = null;

    public IInventoryItem InventoryItem { get; private set; }

    UnityAction onClickIconCallback = null;

    protected void Init(int itemId, bool isNew = false)
    {
        InventoryItem = GameRepo.ItemFactory.GetInventoryItem(itemId);
        if (InventoryItem == null)
            return;

        ItemBaseJson itemBaseJson = InventoryItem.JsonObject;
        Sprite sprite = ClientUtils.LoadIcon(itemBaseJson.iconspritepath);
        if (sprite != null)
            imgIcon.sprite = sprite;

        itemRarity.SetRarity(InventoryItem.ItemSortJson.gameicontype, itemBaseJson.rarity);
        newDot.SetActive(isNew);
    }

    protected void Init(CurrencyType currencyType, bool isNew = false)
    {
        Sprite sprite = ClientUtils.LoadCurrencyIcon(currencyType);
        if (sprite != null)
            imgIcon.sprite = sprite;

        newDot.SetActive(isNew);
    }

    protected void OnClickShowItemToolTip(int stackCount)
    {
        if (InventoryItem != null)
        {
            InventoryItem.StackCount = stackCount;
            ItemUtils.OpenDialogItemDetailToolTip(InventoryItem);
        }
    }

    public void SetClickCallback(UnityAction callback)
    {
        if (onClickIconCallback != null)
            button.onClick.RemoveListener(onClickIconCallback);
        onClickIconCallback = callback;
        if (onClickIconCallback != null)
            button.onClick.AddListener(onClickIconCallback);
    }

    public void SetButtonEnable(bool isEnable)
    {
        button.enabled = isEnable;
    }
}
