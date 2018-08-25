using Kopio.JsonContracts;
using UnityEngine;
using System.Collections.Generic;
using Zealot.Repository;
using Zealot.Common;

public class UI_QuestReward : MonoBehaviour
{
    [SerializeField]
    GameObject MaterialIcon;

    [SerializeField]
    GameObject ConsumableIcon;

    [SerializeField]
    GameObject EquipIcon;

    private List<GameObject> mRewards;

    public void Init(Reward reward)
    {
        if (reward == null || reward.itemRewardLst == null)
            return;

        mRewards = new List<GameObject>();

        foreach (RewardItem item in reward.itemRewardLst)
        {
            ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(item.id);
            GameObject icon = null;
            switch(itemJson.bagtype)
            {
                case BagType.Equipment:
                    icon = Instantiate(EquipIcon);
                    icon.GetComponent<GameIcon_Equip>().InitWithoutCallback(item.id, 0, 0, 0);
                    break;
                case BagType.Consumable:
                    icon = Instantiate(ConsumableIcon);
                    icon.GetComponent<GameIcon_MaterialConsumable>().InitWithoutCallback(item.id, item.count);
                    break;
                case BagType.Material:
                    icon = Instantiate(MaterialIcon);
                    icon.GetComponent<GameIcon_MaterialConsumable>().InitWithoutCallback(item.id, item.count);
                    break;
                case BagType.DNA:
                    break;
            }
            icon.transform.SetParent(transform, false);
            mRewards.Add(icon);
        }
    }

    void OnDisable()
    {
        Clear();
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
