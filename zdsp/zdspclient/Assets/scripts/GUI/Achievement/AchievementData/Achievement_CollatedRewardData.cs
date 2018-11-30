using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Achievement_CollatedRewardData : MonoBehaviour
{
    [SerializeField] Transform iconSlot;
    [SerializeField] Image iconImage;
    [SerializeField] Text rewardNameText;
    [SerializeField] Text rewardAmtText;

    public void Init(CollatedAchievementReward reward)
    {
        ResetData();
        switch (reward.rewardType)
        {
            case AchievementRewardType.Item:
                IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(reward.rewardId);
                if (item != null)
                {
                    ItemGameIconType iconType = item.ItemSortJson.gameicontype;
                    GameObject iconPrefab = ClientUtils.LoadGameIcon(iconType);
                    GameObject itemIcon = ClientUtils.CreateChild(iconSlot, iconPrefab);
                    ClientUtils.InitGameIcon(itemIcon, item, item.ItemID, iconType, (int)reward.rewardCount, false);
                    rewardNameText.text = item.JsonObject.localizedname;
                    rewardAmtText.text = "x" + reward.rewardCount;
                }
                break;
            case AchievementRewardType.Currency:
                iconImage.gameObject.SetActive(true);
                CurrencyType currencyType = (CurrencyType)reward.rewardId;
                iconImage.sprite = ClientUtils.LoadCurrencyIcon(currencyType);
                rewardNameText.text = ClientUtils.GetLocalizedCurrencyName(currencyType);
                rewardAmtText.text = "x" + reward.rewardCount;
                break;
            case AchievementRewardType.SideEffect:
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = ClientUtils.LoadIcon(reward.iconPath);
                rewardNameText.text = SideEffectUtils.GetEffectTypeLocalizedName((EffectType)reward.rewardId);
                rewardAmtText.text = "+" + reward.rewardCount;
                break;
        }
    }

    private void ResetData()
    {
        ClientUtils.DestroyChildren(iconSlot);
        iconImage.gameObject.SetActive(false);
    }
}