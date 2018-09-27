using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public enum CollectRewardType
{
    DestinyClue,
}

public class CollectRewardData
{
    public int mCollectId;
    public CollectRewardType mType;

    public CollectRewardData(int collectid, CollectRewardType type)
    {
        mCollectId = collectid;
        mType = type;
    }
}

public class UI_DialogClaimReward : BaseWindowBehaviour
{
    [SerializeField]
    Text Title;

    [SerializeField]
    Transform ItemContent;

    [SerializeField]
    Button Claim;

    [SerializeField]
    Text ClaimButtonText;

    private List<RewardItem> mRewardList;
    private List<GameObject> mRewardItems;
    private CollectRewardData mCollectRewardData;

    public void Init(List<RewardItem> rewardlist, CollectRewardData data, string title = null, bool claimable = true)
    {
        mCollectRewardData = data;
        mRewardList = rewardlist;
        Title.text = string.IsNullOrEmpty(title) ? GUILocalizationRepo.GetLocalizedString("gtr_title_get_rewards") : GUILocalizationRepo.GetLocalizedString(title);
        UpdateRewardItem();
        Claim.interactable = claimable;
        ClaimButtonText.text = claimable ? GUILocalizationRepo.GetLocalizedString("gtr_collect") : GUILocalizationRepo.GetLocalizedString("gtr_collected");
    }

    private void UpdateRewardItem()
    {
        Clean();
        mRewardItems = new List<GameObject>();

        foreach(RewardItem reward in mRewardList)
        {
            int itemId = reward.itemId;
            ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(itemId);
            if (itemJson != null)
            {
                ItemSortJson itemSortJson = GameRepo.ItemFactory.GetItemSortById(itemJson.itemsort);
                GameObject gameIcon = Instantiate(ClientUtils.LoadGameIcon(itemSortJson.gameicontype));
                gameIcon.transform.SetParent(ItemContent, false);
                ClientUtils.InitGameIcon(gameIcon, null, itemId, itemSortJson.gameicontype, reward.count, true);
                mRewardItems.Add(gameIcon);
            }
        }
    }

    private void Clean()
    {
        if (mRewardItems != null)
        {
            foreach (GameObject obj in mRewardItems)
                Destroy(obj);

            mRewardItems.Clear();
        }
    }

    private void OnDisable()
    {
        mRewardList = null;
        Clean();
    }

    public void OnClickCollectReward()
    {
        UIManager.StartHourglass();
        if (mCollectRewardData.mType == CollectRewardType.DestinyClue)
        {
            RPCFactory.NonCombatRPC.CollectClueReward(mCollectRewardData.mCollectId);
        }
    }
}
