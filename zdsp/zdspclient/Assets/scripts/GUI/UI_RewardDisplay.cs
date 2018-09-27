using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class UI_RewardDisplay : MonoBehaviour
{
    public Transform Content;

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
        ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(rewardItem.itemId);
        if (itemJson == null)
            return;

        ItemSortJson itemSortJson = GameRepo.ItemFactory.GetItemSortById(itemJson.itemsort);
        GameObject gameIcon = Instantiate(ClientUtils.LoadGameIcon(itemSortJson.gameicontype));
        gameIcon.transform.SetParent(Content, false);
        ClientUtils.InitGameIcon(gameIcon, null, itemJson.itemid, itemSortJson.gameicontype, rewardItem.count, true);
    }
}

