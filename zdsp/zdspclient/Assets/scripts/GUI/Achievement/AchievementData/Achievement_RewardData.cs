using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Achievement_RewardData : MonoBehaviour
{
    [SerializeField] Transform iconSlot;
    [SerializeField] Image iconImage;
    [SerializeField] Text achievementNameText;
    [SerializeField] Text aexpNameText;
    [SerializeField] Text aexpAmtText;
    [SerializeField] Text rewardNameText;
    [SerializeField] Text rewardAmtText;

    private AchievementRewardClaim achReward;

    private void Awake()
    {
        aexpNameText.text = ClientUtils.GetCurrencyLocalizedName(CurrencyType.AExp);
    }

    public void Init(AchievementRewardClaim reward)
    {
        achReward = reward;

        BaseAchievementObjective obj = AchievementRepo.GetObjectiveByTypeAndId(reward.ClaimType, reward.Id);
        if (obj != null)
        {
            achievementNameText.text = obj.localizedName;
            aexpAmtText.text = "x" + obj.exp;
            SetReward(obj);
        }
    }

    private void SetReward(BaseAchievementObjective obj)
    {
        ResetData();
        switch (obj.rewardType)
        {
            case AchievementRewardType.None:
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = ClientUtils.LoadCurrencyIcon(CurrencyType.AExp);
                break;
            case AchievementRewardType.Item:
                IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(obj.rewardId);
                if (item != null)
                {
                    ItemGameIconType iconType = item.ItemSortJson.gameicontype;
                    GameObject iconPrefab = ClientUtils.LoadGameIcon(iconType);
                    GameObject itemIcon = ClientUtils.CreateChild(iconSlot, iconPrefab);
                    ClientUtils.InitGameIcon(itemIcon, item, item.ItemID, iconType, obj.rewardCount, false);
                    rewardNameText.text = item.JsonObject.localizedname;
                    rewardAmtText.text = "x" + obj.rewardCount;
                    rewardNameText.transform.parent.gameObject.SetActive(true);
                }
                break;
            case AchievementRewardType.Currency:
                iconImage.gameObject.SetActive(true);
                CurrencyType currencyType = (CurrencyType)obj.rewardId;
                iconImage.sprite = ClientUtils.LoadCurrencyIcon(currencyType);
                rewardNameText.text = ClientUtils.GetCurrencyLocalizedName(currencyType);
                rewardAmtText.text = "x" + obj.rewardCount;
                rewardNameText.transform.parent.gameObject.SetActive(true);

                break;
            case AchievementRewardType.SideEffect:
                SideEffectJson se = SideEffectRepo.GetSideEffect(obj.rewardId);
                if (se != null)
                {
                    iconImage.gameObject.SetActive(true);
                    iconImage.sprite = ClientUtils.LoadIcon(obj.rewardIconPath);
                    rewardNameText.text = se.effecttype.ToString(); // todo: jm to change to localize
                    rewardAmtText.text = "+" + (se.isrelative ? string.Format("{0}%", se.max) : se.max.ToString());
                    rewardNameText.transform.parent.gameObject.SetActive(true);
                }
                break;
        }
    }

    private void ResetData()
    {
        ClientUtils.DestroyChildren(iconSlot);
        iconImage.gameObject.SetActive(false);
        rewardNameText.transform.parent.gameObject.SetActive(false);
    }

    public void OnClickClaim()
    {
        RPCFactory.CombatRPC.ClaimAchievementReward((byte)achReward.ClaimType, achReward.Id);
    }
}