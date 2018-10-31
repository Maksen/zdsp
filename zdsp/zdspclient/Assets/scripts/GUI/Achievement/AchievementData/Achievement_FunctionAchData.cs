using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Achievement_FunctionAchData : Achievement_FunctionData
{
    [SerializeField] Image trophyIconImage;
    [SerializeField] Text achNameText;
    [SerializeField] Text achSubTypeText;
    [SerializeField] Text achDescText;
    [SerializeField] UI_ProgressBarC achProgressBar;
    [SerializeField] Image axepIconImage;
    [SerializeField] Text aexpAmtText;
    [SerializeField] Transform itemIconSlot;
    [SerializeField] Image rewardImage;
    [SerializeField] Text rewardNameText;
    [SerializeField] Text rewardAmtText;

    public override void Init(LISAExternalFunctionJson data, ToggleGroup toggleGroup, bool unlocked)
    {
        base.Init(data, toggleGroup, unlocked);
        InitAchievementDetails(data.triggervalue);
    }

    public override void Refresh()
    {
        AchievementStatsClient achStats = GameInfo.gLocalPlayer.AchievementStats;
        SetUnlocked(achStats.IsAchievementCompletedAndClaimed(triggerValue));
        AchievementElement elem = achStats.GetAchievementById(triggerValue);
        achProgressBar.Value = elem == null ? 0 : elem.Count;
        achProgressBar.Refresh();
    }

    private void InitAchievementDetails(int id)
    {
        AchievementObjective obj = AchievementRepo.GetAchievementObjectiveById(id);
        if (obj != null)
        {
            nameText.text = obj.localizedName;

            trophyIconImage.sprite = LoadTrophyIcon(obj.json.trophytype);
            achNameText.text = obj.localizedName;
            achSubTypeText.text = AchievementRepo.GetAchievementSubTypeLocalizedName(obj.subType);
            achDescText.text = obj.json.localizeddescription.Replace("{count}", obj.completeCount.ToString());
            axepIconImage.sprite = ClientUtils.LoadCurrencyIcon(CurrencyType.AExp);
            aexpAmtText.text = "x" + obj.exp;
            SetReward(obj);
            achProgressBar.Max = obj.completeCount;
            AchievementElement elem = GameInfo.gLocalPlayer.AchievementStats.GetAchievementById(id);
            achProgressBar.Value = elem == null ? 0 : elem.Count;
            achProgressBar.Refresh();
        }
    }

    private void SetReward(BaseAchievementObjective obj)
    {
        switch (obj.rewardType)
        {
            case AchievementRewardType.None:
                itemIconSlot.parent.gameObject.SetActive(false);
                break;
            case AchievementRewardType.Item:
                rewardImage.gameObject.SetActive(false);
                IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(obj.rewardId);
                if (item != null)
                {
                    ItemGameIconType iconType = item.ItemSortJson.gameicontype;
                    GameObject iconPrefab = ClientUtils.LoadGameIcon(iconType);
                    GameObject itemIcon = ClientUtils.CreateChild(itemIconSlot, iconPrefab);
                    ClientUtils.InitGameIcon(itemIcon, item, item.ItemID, iconType, obj.rewardCount, false);
                    rewardNameText.text = item.JsonObject.localizedname;
                    rewardAmtText.text = "x" + obj.rewardCount;
                }
                break;
            case AchievementRewardType.Currency:
                rewardImage.gameObject.SetActive(true);
                CurrencyType currencyType = (CurrencyType)obj.rewardId;
                rewardImage.sprite = ClientUtils.LoadCurrencyIcon(currencyType);
                rewardNameText.text = ClientUtils.GetCurrencyLocalizedName(currencyType);
                rewardAmtText.text = "x" + obj.rewardCount;
                break;
            case AchievementRewardType.SideEffect:
                rewardImage.gameObject.SetActive(true);
                SideEffectJson se = SideEffectRepo.GetSideEffect(obj.rewardId);
                if (se != null)
                {
                    rewardImage.sprite = ClientUtils.LoadIcon(obj.rewardIconPath);
                    rewardNameText.text = SideEffectUtils.GetEffectTypeLocalizedName(se.effecttype);
                    rewardAmtText.text = "+" + (se.isrelative ? string.Format("{0}%", se.max) : se.max.ToString());
                }
                break;
        }
    }

    private Sprite LoadTrophyIcon(AchievementTrophyType trophyType)
    {
        string path = "";
        switch (trophyType)
        {
            case AchievementTrophyType.Bronze:
                path = "";
                break;
            case AchievementTrophyType.Silver:
                path = "";
                break;
            case AchievementTrophyType.Gold:
                path = "";
                break;
        }
        return ClientUtils.LoadIcon(path);
    }
}