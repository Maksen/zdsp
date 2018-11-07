using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Achievement_ObjectiveDataBase : MonoBehaviour
{
    [SerializeField] Text achNameText;
    [SerializeField] Text achDescText;
    [SerializeField] Image trophyIconImage;
    [SerializeField] UI_ProgressBarC progressBar;
    [SerializeField] Image aexpIconImage;
    [SerializeField] Text aexpAmtText;
    [SerializeField] Transform itemIconSlot;
    [SerializeField] Image rewardImage;
    [SerializeField] Text rewardNameText;
    [SerializeField] Text rewardAmtText;

    protected virtual void Awake()
    {
        SetProgressBarMaxText(GUILocalizationRepo.GetLocalizedString("ach_filter_completed"));
    }

    public virtual void Init(AchievementObjective obj, int count)
    {
        trophyIconImage.sprite = LoadTrophyIcon(obj.json.trophytype);
        achNameText.text = obj.localizedName.Replace("{count}", obj.completeCount.ToString());
        achDescText.text = obj.json.localizeddescription.Replace("{count}", obj.completeCount.ToString());
        SetReward(obj);
        progressBar.Max = obj.completeCount;
        progressBar.Value = count;
        progressBar.Refresh();
    }

    public void UpdateProgress(int count)
    {
        if (progressBar.Value != count)
        {
            progressBar.Value = count;
            progressBar.Refresh();
        }
    }

    protected void SetProgressBarMaxText(string text)
    {
        progressBar.MaxText = text;
    }

    private void SetReward(BaseAchievementObjective obj)
    {
        aexpIconImage.sprite = ClientUtils.LoadCurrencyIcon(CurrencyType.AExp);
        aexpAmtText.text = "x" + obj.exp;

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
        //todo: jm to fill in path
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