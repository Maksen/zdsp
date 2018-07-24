using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class InvQuickConsumableButton : MonoBehaviour
{
    [SerializeField]
    Image imgIcon = null;

    [SerializeField]
    Button buttonAddItem = null;

    [SerializeField]
    Button buttonRemoveItem = null;

    public UI_Inventory_QuickSlot InvQuickSlot { private get; set; }

    public byte QuickSlotIdx { private get; set; }

    int slotItemId = 0; // Current Item Id

    public void SetItem(int itemId)
    {
        SetIconVisible(itemId > 0);

        if (itemId > 0 && itemId != slotItemId)
        {
            IInventoryItem invItem = GameRepo.ItemFactory.GetInventoryItem(itemId);
            if (invItem == null)
                return;

            Sprite sprite = ClientUtils.LoadIcon(invItem.JsonObject.iconspritepath);
            if (sprite != null)
                imgIcon.sprite = sprite;

            slotItemId = itemId;
        }  
    }

    public void SetIconVisible(bool value)
    {
        imgIcon.gameObject.SetActive(value);
        buttonAddItem.gameObject.SetActive(!value);
        buttonRemoveItem.gameObject.SetActive(value);
    }

    public void OnClickAddItem()
    {
        InvQuickSlot.SelectedQuickSlotIdx = QuickSlotIdx;
    }

    public void OnClickRemoveItem()
    {
        RPCFactory.CombatRPC.SetItemHotbar(QuickSlotIdx, -1);
    }
}
