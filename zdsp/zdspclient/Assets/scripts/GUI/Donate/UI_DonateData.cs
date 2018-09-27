using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
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
        ItemSortJson itemSortJson = GameRepo.ItemFactory.GetItemSortById(itemJson.itemsort);
        GameObject gameIcon = Instantiate(ClientUtils.LoadGameIcon(itemSortJson.gameicontype));
        gameIcon.transform.SetParent(parent, false);
        ClientUtils.InitGameIcon(gameIcon, null, itemJson.itemid, itemSortJson.gameicontype, amount, true);
    }

    public void OnClickDonate()
    {
        UIManager.StartHourglass();
        RPCFactory.NonCombatRPC.DonateItem(mGuid);
    }
}
