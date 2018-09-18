using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_MailAttachmentData : MonoBehaviour
{
    public Image mItemSlot;                     //item window frame
    public Text mItemCount;                     //how many item of this type
    GameIcon_Equip mItemIcon;
    [HideInInspector]
    public ItemType mItemType;

    void OnDestroy()
    {

    }

    void OnClickedIcon_MailAttachment(IInventoryItem item)
    {
        //Data are supposed to get from repository
        //string name = "Red Potion";
        //string restrict = "None";
        //string stats = "Recovers 45 hp.\nIncrease max hp by 1.\n";

        UIManager.OpenDialog(WindowType.DialogItemDetail, (GameObject window) =>
        {
            window.GetComponent<UI_DialogItemDetailToolTip>().InitTooltip(item);
        });
    }

    public void SetItem(IInventoryItem item, bool flag=true)
    {
        //Note: cannot support 10 digits of 9 (9999999999)
        mItemCount.text = "X" + item.StackCount.ToString();
        mItemType = item.JsonObject.itemtype;

        Equipment eq = item as Equipment;
        mItemIcon.InitWithToolTipView(eq.JsonObject.itemid, 0, eq.ReformStep, eq.UpgradeLevel);

        //enabled = flag;
        mItemSlot.enabled = flag;
        mItemCount.enabled = flag;
    }
}
