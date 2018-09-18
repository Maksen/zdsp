using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class UI_RewardDisplay : MonoBehaviour
{
    public Transform Content;
    public GameObject[] GameIconPrefab;

    public void Init(List<RewardItem> rewardItemList)
    {
        Clear();
        if (rewardItemList != null)
        {
            int count = rewardItemList.Count;
            for (int index = 0; index < count; ++index)
                CreateGameIcon(rewardItemList[index]);
        }
    }

    void OnDisable()
    {
        Clear();
    }

    public void Clear()
    {
        foreach (Transform child in Content)
            Destroy(child.gameObject);
    }

    void CreateGameIcon(RewardItem rewardItem)
    {
        var itemJson = GameRepo.ItemFactory.GetItemById(rewardItem.itemId);
        if (itemJson == null)
            return;
        BagType bagType = itemJson.bagtype;
        int prefab_index = (int)bagType-1;
        if (prefab_index >= 0 && prefab_index < GameIconPrefab.Length)
        {
            GameObject gameIcon = Instantiate(GameIconPrefab[prefab_index]);
            gameIcon.transform.SetParent(Content, false);
            switch (bagType)
            {
                case BagType.Equipment:
                    gameIcon.GetComponent<GameIcon_Equip>().InitWithToolTipView(itemJson.itemid, 0, 0, 0);
                    break;
                case BagType.Consumable:
                case BagType.Material:
                    gameIcon.GetComponent<GameIcon_MaterialConsumable>().InitWithToolTipView(itemJson.itemid, rewardItem.count);
                    break;
                case BagType.Socket:
                    gameIcon.GetComponent<GameIcon_DNA>().InitWithToolTipView(itemJson.itemid, 0, 0);
                    break;
            }
        }
    }
}

