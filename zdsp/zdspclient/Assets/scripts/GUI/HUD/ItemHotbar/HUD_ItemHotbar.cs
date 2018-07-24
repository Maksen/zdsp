using UnityEngine;

public class HUD_ItemHotbar : MonoBehaviour
{
    [SerializeField]
    ItemHotbarButton[] itemHotbarButtons = null;

    // Use this for initialization
    void Awake()
    {
        int len = itemHotbarButtons.Length;
        for (int i = 0; i < len; ++i)
            itemHotbarButtons[i].QuickSlotIdx = (byte)i;
    }

    public void SetItemToSlot(int slotIdx, int itemId)
    {
        itemHotbarButtons[slotIdx].SetItem(itemId);
    }
}
