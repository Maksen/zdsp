using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Kopio.JsonContracts;
using Zealot.Repository;
using Zealot.Client.Entities;

public class UI_DonateData : MonoBehaviour
{
    [SerializeField]
    Transform ItemIcon;

    [SerializeField]
    Text ItemName;

    [SerializeField]
    Text StackCount;

    [SerializeField]
    Transform RewardContent;

    [SerializeField]
    Button RewardButton;

    [SerializeField]
    GameObject MaterialIcon;

    [SerializeField]
    GameObject ConsumableIcon;

    [SerializeField]
    GameObject EquipIcon;

    private DonateOrderData mDonateOrderData;
    private string mGuid;

    public void Init(DonateOrderData data, PlayerGhost player)
    {
        mGuid = data.Guid;
        mDonateOrderData = data;
        DonateJson donateJson = DonateRepo.GetDonateById(data.Id);
        if (donateJson != null)
        {
            ItemBaseJson itemBaseJson = GameRepo.ItemFactory.GetItemById(donateJson.donateitemid);
            if (itemBaseJson != null)
            {
                int amount = ((data.Count * donateJson.increase) + donateJson.amount);
                ItemName.text = itemBaseJson.localizedname;
                StackCount.text = amount + "/" + player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)donateJson.donateitemid);
                GenerateItemIcon(itemBaseJson, ItemIcon, 1);
            }
            Reward reward = RewardListRepo.GetRewardByGrpIDJobID(donateJson.reward, player.PlayerSynStats.jobsect);
            if (reward != null)
            {
                GenerateRewardItem(reward);
            }
        }
    }

    private void GenerateRewardItem(Reward reward)
    {
        foreach (RewardItem item in reward.itemRewardLst)
        {
            ItemBaseJson itemBaseJson = GameRepo.ItemFactory.GetItemById(item.itemId);
            if (itemBaseJson != null)
            {
                GenerateItemIcon(itemBaseJson, RewardContent, item.count);
            }
        }
    }

    private void GenerateItemIcon(ItemBaseJson itemJson, Transform parent, int amount)
    {
        GameObject icon = null;
        switch (itemJson.bagtype)
        {
            case BagType.Equipment:
                icon = Instantiate(EquipIcon);
                icon.GetComponent<GameIcon_Equip>().InitWithoutCallback(itemJson.itemid, 0, 0, 0);
                break;
            case BagType.Consumable:
                icon = Instantiate(ConsumableIcon);
                icon.GetComponent<GameIcon_MaterialConsumable>().InitWithoutCallback(itemJson.itemid, amount);
                break;
            case BagType.Material:
                icon = Instantiate(MaterialIcon);
                icon.GetComponent<GameIcon_MaterialConsumable>().InitWithoutCallback(itemJson.itemid, amount);
                break;
            case BagType.Socket:
                break;
        }
        icon.transform.SetParent(parent, false);
        int itemid = itemJson.itemid;
        icon.GetComponent<Button>().onClick.AddListener(() => OnClickItem(itemid, amount));
    }

    public void OnClickItem(int itemid, int count)
    {
        IInventoryItem inventoryItem = GameRepo.ItemFactory.GetInventoryItem(itemid);
        inventoryItem.StackCount = count;
        UIManager.OpenDialog(WindowType.DialogItemDetail, (window) => {
            window.GetComponent<UI_DialogItemDetailToolTip>().InitTooltip(inventoryItem);
        });
    }

    public void OnClickDonate()
    {
        UIManager.StartHourglass();
        RPCFactory.NonCombatRPC.DonateItem(mGuid);
    }
}
