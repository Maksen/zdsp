using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class UI_Achievement_RewardsDialog : BaseWindowBehaviour
{
    [SerializeField] AchievementRewardsScrollView rewardsListScrollView;
    [SerializeField] AchievementCollatedRewardsScrollView claimedRewardsScrollView;

    private Dictionary<CurrencyType, int> currencyAdded = new Dictionary<CurrencyType, int>();
    private Dictionary<int, int> itemsAdded = new Dictionary<int, int>();
    private Dictionary<EffectType, ValuePair<float, float>> buffsMap = new Dictionary<EffectType, ValuePair<float, float>>();

    public void InitRewardsList(List<AchievementRewardClaim> rewardList)
    {
        rewardsListScrollView.gameObject.SetActive(true);
        claimedRewardsScrollView.gameObject.SetActive(false);

        rewardsListScrollView.Populate(rewardList);
    }

    public void RefreshRewardsList(List<AchievementRewardClaim> rewardList)
    {
        if (rewardList.Count > 0)
            rewardsListScrollView.Refresh();
        else
            rewardsListScrollView.Clear();
    }

    public void InitCollatedRewardsList(List<AchievementRewardClaim> rewardList)
    {
        rewardsListScrollView.gameObject.SetActive(false);
        claimedRewardsScrollView.gameObject.SetActive(true);

        Dictionary<EffectType, string> seIconPaths = new Dictionary<EffectType, string>();

        for (int i = 0; i < rewardList.Count; ++i)
        {
            var reward = rewardList[i];
            BaseAchievementObjective obj = AchievementRepo.GetObjectiveByTypeAndId(reward.ClaimType, reward.Id);
            if (obj != null)
            {
                // add exp
                if (currencyAdded.ContainsKey(CurrencyType.AExp))
                    currencyAdded[CurrencyType.AExp] += obj.exp;
                else
                    currencyAdded.Add(CurrencyType.AExp, obj.exp);

                if (obj.rewardType == AchievementRewardType.Currency)
                {
                    CurrencyType currencyType = (CurrencyType)obj.rewardId;
                    if (currencyAdded.ContainsKey(currencyType))
                        currencyAdded[currencyType] += obj.rewardCount;
                    else
                        currencyAdded.Add(currencyType, obj.rewardCount);
                }
                else if (obj.rewardType == AchievementRewardType.Item)
                {
                    int itemId = obj.rewardId;
                    if (itemsAdded.ContainsKey(itemId))
                        itemsAdded[itemId] += obj.rewardCount;
                    else
                        itemsAdded.Add(itemId, obj.rewardCount);
                }
                else if (obj.rewardType == AchievementRewardType.SideEffect)
                {
                    SideEffectJson se = SideEffectRepo.GetSideEffect(obj.rewardId);
                    if (se != null)
                    {
                        AddToBuffMap(se);
                        if (!seIconPaths.ContainsKey(se.effecttype) && !string.IsNullOrEmpty(obj.rewardIconPath))
                            seIconPaths.Add(se.effecttype, obj.rewardIconPath);
                    }
                }
            }
        }

        List<AchievementReward> collatedList = new List<AchievementReward>();
        foreach (var item in currencyAdded)
            collatedList.Add(new AchievementReward(AchievementRewardType.Currency, (int)item.Key, item.Value, ""));
        foreach (var item in itemsAdded)
            collatedList.Add(new AchievementReward(AchievementRewardType.Item, item.Key, item.Value, ""));
        foreach (var item in buffsMap)
        {
            string iconpath;
            seIconPaths.TryGetValue(item.Key, out iconpath);
            if (item.Value.Item1 > 0) // abs
            {
                int key = (int)item.Key;
                collatedList.Add(new AchievementReward(AchievementRewardType.SideEffect, key, item.Value.Item1, iconpath));
            }
            if (item.Value.Item2 > 0)  // relative
            {
                int key = (int)item.Key;
                collatedList.Add(new AchievementReward(AchievementRewardType.SideEffect, key, item.Value.Item2, iconpath));
            }
        }

        claimedRewardsScrollView.Populate(collatedList);
    }

    private void AddToBuffMap(SideEffectJson se)
    {
        if (se == null)
            return;

        if (buffsMap.ContainsKey(se.effecttype))
        {
            if (se.isrelative)
                buffsMap[se.effecttype].Item2 += se.max;
            else
                buffsMap[se.effecttype].Item1 += se.max;
        }
        else
        {
            if (se.isrelative)
                buffsMap.Add(se.effecttype, new ValuePair<float, float>(0, se.max));
            else
                buffsMap.Add(se.effecttype, new ValuePair<float, float>(se.max, 0));
        }
    }

    public void OnClickClaimAllRewards()
    {
        RPCFactory.CombatRPC.ClaimAllAchievementRewards();
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        rewardsListScrollView.Clear();
        claimedRewardsScrollView.Clear();
        currencyAdded.Clear();
        itemsAdded.Clear();
        buffsMap.Clear();
    }
}