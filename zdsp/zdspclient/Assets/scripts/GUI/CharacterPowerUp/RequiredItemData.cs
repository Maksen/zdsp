using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Client.Entities;

public class RequiredItemData : MonoBehaviour
{
    public GameObject gameIconPrefab;
    public Transform  gameIconParent;

    public Text requiredAmount;

    int itemID;
    
	/// 根據消耗、擁有數量、需要數量做判斷
	
    public void InitCurrency(CurrencyType type, int invAmount, int reqAmount)
    {
        GameObject gameIconObj = Instantiate(gameIconPrefab);
        gameIconObj.transform.SetParent(gameIconParent, false);

        //GameIcon_MaterialConsumable gameIcon = gameIconObj.GetComponent<GameIcon_MaterialConsumable>();
        //gameIcon.Init(itemId, invAmount);
        requiredAmount.text = reqAmount.ToString();

        GameIcon_MaterialConsumable gameIcon = gameIconObj.GetComponent<GameIcon_MaterialConsumable>();
        gameIcon.Init(CurrencyType.Money, invAmount, false, false, false, OnClick);
        gameIcon.SetStackCount(invAmount);
        CompareMaterial(gameIconObj.transform.GetChild(2).GetComponent<Text>(), invAmount, reqAmount);
    }
    
	/// 根據ID、擁有數量、需要數量做判斷
	
    public bool InitMaterial(int itemId, int invAmount, int reqAmount)
    {
        GameObject gameIconObj = Instantiate(gameIconPrefab);
        gameIconObj.transform.GetChild(2).GetComponent<Text>().text = invAmount.ToString();
        gameIconObj.transform.SetParent(gameIconParent, false);
        itemID = itemId;

        GameIcon_MaterialConsumable gameIcon = gameIconObj.GetComponent<GameIcon_MaterialConsumable>();
        gameIcon.Init(itemId, invAmount, false, false, false, OnClick);
        gameIcon.SetFullStackCount(invAmount);
        requiredAmount.text = reqAmount.ToString();
        return CompareMaterial(gameIconObj.transform.GetChild(2).GetComponent<Text>(), invAmount, reqAmount);
    }

    bool CompareMaterial(Text colorText, int invAmount, int reqAmount)
    {
        if (invAmount >= reqAmount)
        {
            colorText.color = Color.white;
            return true;
        }
        else
        {
            colorText.color = ClientUtils.ColorRed;
            return false;
        }
    }

    public void OnClick()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        var _item = player.clientItemInvCtrl.itemInvData.GetItemByItemId((ushort)itemID);
        UIManager.OpenDialog(WindowType.DialogItemDetail, (window) => {
            OnClicked_InitTooltip(window.GetComponent<UI_DialogItemDetailToolTip>(), _item);
        });
    }

    private void OnClicked_InitTooltip(UI_DialogItemDetailToolTip component, IInventoryItem item)
    {
        component.InitTooltip(item);
        List<ItemDetailsButton> _buttons = new List<ItemDetailsButton>();
        component.SetButtonCallback(_buttons);
    }
}
