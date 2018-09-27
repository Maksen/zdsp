using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_MailAttachmentData : MonoBehaviour
{
    public Text mItemCount;                     //how many item of this type
    GameIcon_Base mItemIcon;
    [HideInInspector]
    public ItemType mItemType;

    [Header("Parent")]
    [SerializeField]
    GameObject mIconParent;

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
        int stackCount = item.StackCount;
        mItemCount.text = "X" + stackCount.ToString();
        mItemType = item.JsonObject.itemtype;

        ItemGameIconType iconType = item.ItemSortJson.gameicontype;
        GameObject gameIcon = Instantiate(ClientUtils.LoadGameIcon(iconType));
        gameIcon.transform.SetParent(mIconParent.transform, false);
        ClientUtils.InitGameIcon(gameIcon, item, item.ItemID, iconType, stackCount, true);

        mItemIcon = gameIcon.GetComponent<GameIcon_Base>();

        //enabled = flag;
        mItemCount.enabled = flag;
    }
}
