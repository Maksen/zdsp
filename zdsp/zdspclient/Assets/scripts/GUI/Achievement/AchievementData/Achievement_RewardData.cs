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
    [SerializeField] Text aexpAmtText;
    [SerializeField] Text rewardNameText;
    [SerializeField] Text rewardAmtText;
    [SerializeField] Button button;
    [SerializeField] GameObject claimEfx;

    private AchievementRewardInfo achReward;
    private UI_Achievement_RewardsDialog parent;
    private AchievementRewardsScrollView scrollView;

    public void Init(AchievementRewardInfo reward, UI_Achievement_RewardsDialog myParent, AchievementRewardsScrollView scroll)
    {
        parent = myParent;
        scrollView = scroll;
        achReward = reward;
        BaseAchievementObjective obj = AchievementRepo.GetObjectiveByTypeAndId(reward.rewardClaim.ClaimType, reward.rewardClaim.Id);
        if (obj != null)
        {
            achievementNameText.text = obj.localizedName.Replace("{count}", obj.completeCount.ToString());
            aexpAmtText.text = "x" + obj.exp;
            SetReward(obj);
        }
        button.interactable = !reward.claimed;
    }

    public void UpdateDataAndPlayEfx(AchievementRewardInfo reward)
    {
        achReward = reward;
        if (reward.claimed && button.interactable)
        {
            button.interactable = false;
            claimEfx.SetActive(true);
        }
    }

    public void UpdateData(AchievementRewardInfo reward)
    {
        achReward = reward;
        if (reward.claimed && button.interactable)
        {
            button.interactable = false;
        }
    }

    public void PlayClaimEfx()
    {
        claimEfx.SetActive(true);
    }

    // triggered by GUIAnimEvent OnFinished
    public void OnClaimEfxFinished()
    {
        claimEfx.SetActive(false);
        parent.PlayRewardEffect();
        scrollView.RemoveData(achReward);
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
                rewardNameText.text = ClientUtils.GetLocalizedCurrencyName(currencyType);
                rewardAmtText.text = "x" + obj.rewardCount;
                rewardNameText.transform.parent.gameObject.SetActive(true);

                break;
            case AchievementRewardType.SideEffect:
                SideEffectJson se = SideEffectRepo.GetSideEffect(obj.rewardId);
                if (se != null)
                {
                    iconImage.gameObject.SetActive(true);
                    iconImage.sprite = ClientUtils.LoadIcon(obj.rewardIconPath);
                    rewardNameText.text = SideEffectUtils.GetEffectTypeLocalizedName(se.effecttype);
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
        RPCFactory.CombatRPC.ClaimAchievementReward((byte)achReward.rewardClaim.ClaimType, achReward.rewardClaim.Id);
    }

    public AchievementRewardInfo GetRewardInfo()
    {
        return achReward;
    }
}