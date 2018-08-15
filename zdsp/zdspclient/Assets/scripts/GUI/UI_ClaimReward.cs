using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Zealot.Repository;
using Kopio.JsonContracts;
using Zealot.Common;

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

public class UI_ClaimReward : BaseWindowBehaviour
{
    [SerializeField]
    Text Title;

    [SerializeField]
    Transform ItemContent;

    [SerializeField]
    GameObject EquipmentData;

    [SerializeField]
    GameObject MaterialData;

    [SerializeField]
    GameObject DnaData;

    [SerializeField]
    Button Claim;

    [SerializeField]
    Text ClaimButtonText;

    List<RewardItem> mRewardList;
    private Dictionary<int, GameObject> mRewardItem;
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
        mRewardItem = new Dictionary<int, GameObject>();

        foreach(RewardItem reward in mRewardList)
        {
            ItemBaseJson itemJson = GameRepo.ItemFactory.GetItemById(reward.id);
            BagType bagType = itemJson.bagtype;
            GameObject itemobj = null;
            switch (bagType)
            {
                case BagType.Equipment:
                    itemobj = Instantiate(EquipmentData);
                    itemobj.GetComponent<GameIcon_Equip>().InitWithTooltipViewOnly(itemJson.itemid);
                    break;
                case BagType.DNA:
                    itemobj = Instantiate(DnaData);
                    itemobj.GetComponent<GameIcon_DNA>().InitWithTooltipViewOnly(itemJson.itemid, 0, 0);
                    break;
                default:
                    itemobj = Instantiate(MaterialData);
                    itemobj.GetComponent<GameIcon_MaterialConsumable>().InitWithTooltipViewOnly(itemJson.itemid, reward.count);
                    break;
                
            }
            itemobj.transform.SetParent(ItemContent, false);
            mRewardItem.Add(reward.id, itemobj);
        }
    }



    private void Clean()
    {
        if (mRewardItem != null)
        {
            foreach(KeyValuePair<int, GameObject> entry in mRewardItem)
            {
                Destroy(entry.Value);
            }
            mRewardItem = new Dictionary<int, GameObject>();
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
