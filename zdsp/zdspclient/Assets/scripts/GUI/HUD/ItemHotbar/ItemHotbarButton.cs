using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Common.Datablock;
using Zealot.Repository;

public class ItemHotbarButton : MonoBehaviour
{
    [SerializeField]
    GameObject gameObjImgPlus = null;

    [SerializeField]
    Image imgIcon = null;

    public byte QuickSlotIdx { private get; set; }

    public void SetItem(int itemId)
    {
        if (itemId > 0)
        {
            IInventoryItem invItem = GameRepo.ItemFactory.GetInventoryItem(itemId);
            if (invItem == null)
                return;

            Sprite sprite = ClientUtils.LoadIcon(invItem.JsonObject.iconspritepath);
            if (sprite != null)
                imgIcon.sprite = sprite;
        }

        gameObjImgPlus.SetActive(itemId == 0);
        imgIcon.gameObject.SetActive(itemId > 0);     
    }

    public void OnClickUseItem()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
        {
            CollectionHandler<object> itemHotbarCollection = player.ItemHotbarStats.ItemHotbar;
            int itemId = (int)itemHotbarCollection[QuickSlotIdx];
            if (itemId > 0)
                RPCFactory.CombatRPC.UseItemHotbar(QuickSlotIdx);
            else
            {
                GameObject windowObj = UIManager.GetWindowGameObject(WindowType.Inventory);
                windowObj.GetComponent<UI_Inventory>().InitOnEnable = false;
                UIManager.OpenWindow(WindowType.Inventory, 
                    (window) => window.GetComponent<UI_Inventory>().Init(BagType.Consumable));
            }
        }
    }
}
