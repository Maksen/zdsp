using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zealot.Common;

public class ChatroomBagScrollRow : MonoBehaviour
{
    List<InvRowIcon> ChildList { get; set; }

    // Use this for initialization
    void Awake()
    {
        ChildList = new List<InvRowIcon>();
    }

    void OnDestroy()
    {
        ChildList = null;
    }

    public void Clear()
    {
        int count = ChildList.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; ++i)
            {
                Destroy(ChildList[i].GameIcon);
                ChildList[i].GameIcon = null;
            }
            ChildList.Clear();
        }
    }

    public void Init(HUD_Chatroom hudChatroom, int rowIndex, int cellsPerRow)
    {
        Clear();

        List<InvDisplayItem> displayItemList = hudChatroom.DisplayItemList;
        int totalCount = displayItemList.Count;
        int unlockSlotCnt = GameInfo.gLocalPlayer.SecondaryStats.UnlockedSlotCount;

        Transform parent = gameObject.transform;
        int realStartIdx = rowIndex * cellsPerRow;
        for (int i = 0; i < cellsPerRow; ++i)
        {
            int realIdx = realStartIdx + i;
            if (realIdx >= totalCount)
                break;

            // Init game icon
            InvDisplayItem invDisplayItem = displayItemList[realIdx];
            IInventoryItem invItem = invDisplayItem.InvItem;
            if (invItem == null)
                continue;

            UnityAction callback = () => hudChatroom.OnClickInventoryItem(invItem);
            ItemGameIconType iconType = invItem.ItemSortJson.gameicontype;
            GameObject gameIcon = Instantiate(ClientUtils.LoadGameIcon(iconType));
            gameIcon.transform.SetParent(parent, false);
            switch (iconType)
            {
                case ItemGameIconType.Equipment:
                    Equipment eq = invItem as Equipment;
                    gameIcon.GetComponent<GameIcon_Equip>().Init(eq.ItemID, 0, eq.ReformStep, eq.UpgradeLevel, false,
                        eq.IsNew, false, callback);
                    break;
                case ItemGameIconType.Consumable:
                case ItemGameIconType.Material:
                    gameIcon.GetComponent<GameIcon_MaterialConsumable>().Init(invItem.ItemID, invItem.StackCount, 
                        false, invItem.IsNew, false, callback);
                    break;
                case ItemGameIconType.DNA:
                    gameIcon.GetComponent<GameIcon_DNA>().Init(invItem.ItemID, 0, 0, invItem.IsNew, callback);
                    break;
            }
               
            ChildList.Add(new InvRowIcon { IconType = (int)iconType, GameIcon = gameIcon });
        }
    }
}
