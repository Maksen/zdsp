using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Repository;

public class UI_QuestReward : MonoBehaviour
{
    private List<GameObject> mRewards;

    public void Init(Reward reward)
    {
        if (reward == null || reward.itemRewardLst == null)
            return;

        mRewards = new List<GameObject>();

        foreach (RewardItem item in reward.itemRewardLst)
        {
            int itemId = item.itemId;
            ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(itemId);
            if (itemJson != null)
            {
                ItemSortJson itemSortJson = GameRepo.ItemFactory.GetItemSortById(itemJson.itemsort);
                GameObject gameIcon = Instantiate(ClientUtils.LoadGameIcon(itemSortJson.gameicontype));
                gameIcon.transform.SetParent(transform, false);
                ClientUtils.InitGameIcon(gameIcon, null, itemId, itemSortJson.gameicontype, item.count, true);
                mRewards.Add(gameIcon);
            }
        }
    }

    void OnDisable()
    {
        //Clear();
    }

    private void Clear()
    {
        if (mRewards != null)
        {
            foreach (GameObject reward in mRewards)
                Destroy(reward);

            mRewards.Clear();
        }
    }
}
