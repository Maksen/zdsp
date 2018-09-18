using Kopio.JsonContracts;
using UnityEngine;
using System.Collections.Generic;
using Zealot.Repository;
using Zealot.Common;
using UnityEngine.UI;

public class UI_QuestReward : MonoBehaviour
{
    [SerializeField]
    GameObject MaterialIcon = null;
    [SerializeField]
    GameObject ConsumableIcon = null;
    [SerializeField]
    GameObject EquipIcon = null;

    private List<GameObject> mRewards;

    public void Init(Reward reward)
    {
        if (reward == null || reward.itemRewardLst == null)
            return;

        mRewards = new List<GameObject>();

        foreach (RewardItem item in reward.itemRewardLst)
        {
            ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(item.itemId);
            GameObject icon = null;
            switch(itemJson.bagtype)
            {
                case BagType.Equipment:
                    icon = Instantiate(EquipIcon);
                    icon.GetComponent<GameIcon_Equip>().InitWithoutCallback(item.itemId, 0, 0, 0);
                    break;
                case BagType.Consumable:
                    icon = Instantiate(ConsumableIcon);
                    icon.GetComponent<GameIcon_MaterialConsumable>().InitWithoutCallback(item.itemId, item.count);
                    break;
                case BagType.Material:
                    icon = Instantiate(MaterialIcon);
                    icon.GetComponent<GameIcon_MaterialConsumable>().InitWithoutCallback(item.itemId, item.count);
                    break;
                case BagType.Socket:
                    break;
            }
            icon.transform.SetParent(transform, false);
            RewardItem rewardItem = item;
            icon.GetComponent<Button>().onClick.AddListener(() => OnClickItem(rewardItem));
            mRewards.Add(icon);
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

    public void OnClickItem(RewardItem item)
    {
        IInventoryItem inventoryItem = GameRepo.ItemFactory.GetInventoryItem(item.itemId);
        inventoryItem.StackCount = item.count;
        UIManager.OpenDialog(WindowType.DialogItemDetail, (window) => {
            window.GetComponent<UI_DialogItemDetailToolTip>().InitTooltip(inventoryItem);
        });
    }
}
