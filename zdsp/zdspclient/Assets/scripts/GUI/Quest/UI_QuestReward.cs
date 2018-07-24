using UnityEngine;
using System.Collections.Generic;
using Kopio.JsonContracts;
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
            switch(itemJson.itemtype)
            {
                case ItemType.Material:
                    mRewards.Add(GenerateMaterialIcon(item.id, item.count));
                    break;
                case ItemType.Equipment:
                    mRewards.Add(GenerateEquipIcon(item.id));
                    break;
                case ItemType.PotionFood:
                case ItemType.LuckyPick:
                case ItemType.Henshin:
                case ItemType.Features:
                case ItemType.DNA:
                case ItemType.Relic:
                case ItemType.QuestItem:
                case ItemType.MercenaryItem:
                case ItemType.PetItem:
                    mRewards.Add(GenerateConsumableIcon(item.id, item.count));
                    break;
            }
        }
    }

    private void Clear()
    {
        foreach(GameObject reward in mRewards)
        {
            GameObject.Destroy(reward);
        }
        mRewards = new List<GameObject>();
    }

    private GameObject GenerateMaterialIcon(int itemid, int stackcount)
    {
        GameObject icon = Instantiate(MaterialIcon);
        icon.GetComponent<GameIcon_MaterialConsumable>().Init(itemid, stackcount, false);
        icon.transform.SetParent(transform, false);
        return icon;
    }

    private GameObject GenerateConsumableIcon(int itemid, int stackcount)
    {
        GameObject icon = Instantiate(ConsumableIcon);
        icon.GetComponent<GameIcon_MaterialConsumable>().Init(itemid, stackcount, false);
        icon.transform.SetParent(transform, false);
        return icon;
    }

    private GameObject GenerateEquipIcon(int itemid)
    {
        GameObject icon = Instantiate(EquipIcon);
        icon.GetComponent<GameIcon_Equip>().Init(itemid, 0, 0, 0, false, false);
        icon.transform.SetParent(transform, false);
        return icon;
    }
}
