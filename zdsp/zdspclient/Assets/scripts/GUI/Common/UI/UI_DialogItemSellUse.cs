using System;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_DialogItemSellUse : MonoBehaviour
{
    [SerializeField]
    Text txtItemName = null;

    [SerializeField]
    Transform parentGameIconSlot = null;

    [SerializeField]
    Button buttonYesToAll = null;

    [SerializeField]
    Spinner spinnerInput = null;

    GameObject gameIcon = null;

    Action<int> onClickYesCallback = null;
    Action onClickYesToAllCallback = null;

    public void Init(InvDisplayItem invDisplayItem, Action<int> yesCallback, Action yesToAllCallback = null)
    {
        IInventoryItem invItem = invDisplayItem.InvItem;
        if (invItem == null)
            return;

        ItemGameIconType iconType = invItem.ItemSortJson.gameicontype;
        gameIcon = Instantiate(ClientUtils.LoadGameIcon(iconType));
        gameIcon.transform.SetParent(parentGameIconSlot, false);
        ClientUtils.InitGameIcon(gameIcon, invItem, invItem.ItemID, iconType, invDisplayItem.DisplayStackCount, false);

        txtItemName.text = invItem.JsonObject.localizedname;
        spinnerInput.Max = invDisplayItem.DisplayStackCount;
        spinnerInput.Value = 1;

        onClickYesCallback = yesCallback;
        onClickYesToAllCallback = yesToAllCallback;
        buttonYesToAll.gameObject.SetActive(onClickYesToAllCallback != null);
    }

    void OnDisable()
    {
        if (gameIcon != null)
            Destroy(gameIcon);
        gameIcon = null;

        onClickYesCallback = null;
        onClickYesToAllCallback = null;
    }

    public void OnClickYesButton()
    {
        if (onClickYesCallback != null)
            onClickYesCallback(spinnerInput.Value);
        GetComponent<UIDialog>().ClickClose();
    }

    public void OnClickYesToAllButton()
    {
        if (onClickYesToAllCallback != null)
            onClickYesToAllCallback();
        GetComponent<UIDialog>().ClickClose();
    }
}
