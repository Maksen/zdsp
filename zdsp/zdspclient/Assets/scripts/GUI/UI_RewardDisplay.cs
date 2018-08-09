using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class UI_RewardDisplay : MonoBehaviour
{
    public Transform Content;
    public GameObject[] GameIconPrefab;

    public void Init(List<RewardItem> itemInfoList)
    {
        Clear();
        if (itemInfoList != null)
        {
            int count = itemInfoList.Count;
            for (int index = 0; index < count; index++)
                CreateIcon(itemInfoList[index]);
        }
    }

    void CreateIcon(RewardItem itemInfo)
    {
        var itemJson = GameRepo.ItemFactory.GetItemById(itemInfo.id);
        if (itemJson == null)
            return;
        BagType bagType = itemJson.bagtype;
        int prefab_index = (int)bagType - 1;
        if (prefab_index >= 0 && prefab_index < GameIconPrefab.Length)
        {
            GameObject gameIcon = Instantiate(GameIconPrefab[prefab_index]);
            gameIcon.transform.SetParent(Content, false);
            switch (bagType)
            {
                case BagType.Equipment:
                    gameIcon.GetComponent<GameIcon_Equip>().InitWithTooltipViewOnly(itemJson.itemid);
                    break;
                case BagType.Consumable:
                case BagType.Material:
                    gameIcon.GetComponent<GameIcon_MaterialConsumable>().InitWithTooltipViewOnly(itemJson.itemid, itemInfo.count);
                    break;
                case BagType.DNA:
                    gameIcon.GetComponent<GameIcon_DNA>().InitWithTooltipViewOnly(itemJson.itemid, 0, 0);
                    break;
            }
        }
    }

    void Clear()
    {
        foreach (Transform child in Content)
            Destroy(child.gameObject);
    }
}

