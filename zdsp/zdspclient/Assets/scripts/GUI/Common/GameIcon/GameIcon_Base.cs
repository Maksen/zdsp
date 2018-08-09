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

    public IInventoryItem inventoryItem { get; private set; }

    UnityAction onClickIconCallback = null;

    public void Init(int itemId)
    {
        inventoryItem = GameRepo.ItemFactory.GetInventoryItem(itemId);
        if (inventoryItem == null)
            return;

        ItemBaseJson itemBaseJson = inventoryItem.JsonObject;
        Sprite sprite = ClientUtils.LoadIcon(itemBaseJson.iconspritepath);
        if (sprite != null)
            imgIcon.sprite = sprite;

        itemRarity.SetRarity(itemBaseJson.bagtype, itemBaseJson.rarity);
    }

    public virtual void OnClickShowTooltipViewOnly()
    {
        UIManager.OpenDialog(WindowType.DialogItemDetail, (window) => {
            window.GetComponent<HUD_ItemDetailToolTip>().InitTooltip(inventoryItem);
        });
    }

    public void SetClickCallback(UnityAction callback)
    {
        if (onClickIconCallback != null)
            button.onClick.RemoveListener(onClickIconCallback);
        onClickIconCallback = callback;
        if (onClickIconCallback != null)
            button.onClick.AddListener(onClickIconCallback);
    }
}
