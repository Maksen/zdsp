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

    public void Init(int itemId, int itemCount, Action clickCallback)
    {
        if (unlockItem == null)
        {
            GameObject icon = ClientUtils.CreateChild(itemSlotTransform, itemIconPrefab);
            unlockItem = icon.GetComponent<GameIcon_MaterialConsumable>();
        }

        unlockItem.Init(itemId, 1);
        itemCountText.text = "x" + itemCount;
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