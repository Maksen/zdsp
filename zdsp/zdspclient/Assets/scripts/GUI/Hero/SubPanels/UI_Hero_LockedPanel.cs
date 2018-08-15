using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Hero_LockedPanel : MonoBehaviour
{
    [SerializeField] Transform itemSlotTransform;
    [SerializeField] GameObject itemIconPrefab;
    [SerializeField] Text itemCountText;

    private GameIcon_MaterialConsumable unlockItem;
    private Action OnClickCallback;
    private Color origColor = Color.clear;

    public void Init(int itemId, int itemCount, Action clickCallback)
    {
        if (origColor == Color.clear)
            origColor = itemCountText.color;

        if (unlockItem == null)
        {
            GameObject icon = ClientUtils.CreateChild(itemSlotTransform, itemIconPrefab);
            unlockItem = icon.GetComponent<GameIcon_MaterialConsumable>();
        }

        unlockItem.InitWithTooltipViewOnly(itemId, 1);
        bool hasEnoughItem = GameInfo.gLocalPlayer.clientItemInvCtrl.itemInvData.HasItem((ushort)itemId, itemCount);
        itemCountText.text = "x" + itemCount;
        itemCountText.color = hasEnoughItem ? origColor : Color.red;

        OnClickCallback = clickCallback;
    }

    public void OnClick()
    {
        if (OnClickCallback != null)
            OnClickCallback();
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}