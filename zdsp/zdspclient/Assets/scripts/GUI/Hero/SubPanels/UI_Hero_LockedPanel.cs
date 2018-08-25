using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Hero_LockedPanel : MonoBehaviour
{
    [SerializeField] Transform itemSlotTransform;
    [SerializeField] GameObject itemIconPrefab;
    [SerializeField] Text itemCountText;

    private GameIcon_MaterialConsumable unlockItem;
    private Action OnClickCallback;
    private Color origColor = Color.clear;
    private string heroName;
    private int bindItemId, unbindItemId;
    private bool showSpendConfirmation;

    public void Init(string heroName, string itemIdStr, int itemCount, Action clickCallback)
    {
        this.heroName = heroName;
        if (origColor == Color.clear)
            origColor = itemCountText.color;

        if (unlockItem == null)
        {
            GameObject icon = ClientUtils.CreateChild(itemSlotTransform, itemIconPrefab);
            unlockItem = icon.GetComponent<GameIcon_MaterialConsumable>();
        }

        string[] itemids = itemIdStr.Split(';');
        if (itemids.Length > 0 && int.TryParse(itemids[0], out bindItemId))
        {
            unlockItem.InitWithToolTipView(bindItemId, 1);
            int bindCount = GameInfo.gLocalPlayer.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)bindItemId);
            int unbindCount = 0;
            if (itemids.Length > 1 && int.TryParse(itemids[1], out unbindItemId))
                unbindCount = GameInfo.gLocalPlayer.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)unbindItemId);
            int totalCount = bindCount + unbindCount;
            bool enough = totalCount >= itemCount;
            itemCountText.text = "x" + itemCount;
            itemCountText.color = enough ? origColor : Color.red;
            showSpendConfirmation = enough && bindCount < itemCount;  // show confirmation whether want to spend unbind
        }

        OnClickCallback = clickCallback;
    }

    public void OnClick()
    {
        if (showSpendConfirmation)
        {
            IInventoryItem bindItem = unlockItem.inventoryItem;
            IInventoryItem unbindItem = GameRepo.ItemFactory.GetInventoryItem(unbindItemId);
            if (bindItem != null && unbindItem != null)
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("hero", heroName);
                param.Add("bind", bindItem.JsonObject.localizedname);
                param.Add("unbind", unbindItem.JsonObject.localizedname);
                string message = GUILocalizationRepo.GetLocalizedString("hro_confirmUseUnbindToUnlock", param);
                UIManager.OpenYesNoDialog(message, OnConfirm);
            }
        }
        else
        {
            OnConfirm();
        }
    }

    private void OnConfirm()
    {
        if (OnClickCallback != null)
            OnClickCallback();
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}