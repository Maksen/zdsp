using System;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_DialogItemSellUse : MonoBehaviour
{
    [SerializeField]
    GameObject[] prefabGameicons = null;

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

        BagType bagType = invItem.JsonObject.bagtype;
        gameIcon = Instantiate(prefabGameicons[(int)bagType-1]);
        gameIcon.transform.SetParent(parentGameIconSlot, false);
        switch (bagType)
        {
            case BagType.Equipment:
                Equipment eq = invItem as Equipment;
                gameIcon.GetComponent<GameIcon_Equip>().InitWithoutCallback(eq.ItemID, 0, 0, eq.UpgradeLevel);
                break;
            case BagType.Consumable:
            case BagType.Material:
                gameIcon.GetComponent<GameIcon_MaterialConsumable>().InitWithoutCallback(invItem.ItemID, invDisplayItem.DisplayStackCount);
                break;
            case BagType.Socket:
                gameIcon.GetComponent<GameIcon_DNA>().InitWithoutCallback(invItem.ItemID, 0, 0);
                break;
        }

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
