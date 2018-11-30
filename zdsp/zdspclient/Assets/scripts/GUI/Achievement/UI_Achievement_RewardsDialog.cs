using Kopio.JsonContracts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Achievement_RewardsDialog : BaseWindowBehaviour
{
    [SerializeField] AchievementRewardsScrollView rewardsListScrollView;
    [SerializeField] AchievementCollatedRewardsScrollView claimedRewardsScrollView;
    [SerializeField] GameObject rewardEfxPrefab;
    [SerializeField] Transform rewardEfxParent;
    [SerializeField] Button claimBtn;
    [SerializeField] GameObject skipBtn;
    [SerializeField] GameObject closeBtn;

    private Dictionary<CurrencyType, int> currencyAdded = new Dictionary<CurrencyType, int>();
    private Dictionary<int, int> itemsAdded = new Dictionary<int, int>();
    private Dictionary<EffectType, ValuePair<float, float>> buffsMap = new Dictionary<EffectType, ValuePair<float, float>>();
    private List<CollatedAchievementReward> collatedList = new List<CollatedAchievementReward>();
    private List<AchievementRewardInfo> displayList = new List<AchievementRewardInfo>();
    private List<GameObject> efxObjList;
    private UI_Achievement parent;
    private int lastEfxIndex;
    private Coroutine efxCoroutine;
    private WaitForSeconds efxWait = new WaitForSeconds(0.5f);

    private void Awake()
    {
        efxObjList = ObjMgr.Instance.InitGameObjectPool(rewardEfxParent, rewardEfxPrefab, rewardEfxPrefab.transform.localPosition, rewardEfxPrefab.transform.localScale, 10);
        rewardsListScrollView.InitScrollView(this);
        claimedRewardsScrollView.InitScrollView();
    }

    public void InitRewardsList(List<AchievementRewardClaim> rewardList, UI_Achievement myParent)
    {
        if (parent == null)
        {
            parent = myParent;
            for (int i = 0; i < efxObjList.Count; ++i)
                efxObjList[i].GetComponent<AchievementRewardEfx>().SetParent(parent);
        }

        rewardsListScrollView.gameObject.SetActive(true);
        claimedRewardsScrollView.gameObject.SetActive(false);

        displayList.Clear();
        for (int i = 0; i < rewardList.Count; ++i)
            displayList.Add(new AchievementRewardInfo(rewardList[i], false));

        rewardsListScrollView.Populate(displayList);
    }

    public void OnClaimedReward(AchievementRewardClaim claimedReward)
    {
        int foundIndex = displayList.FindIndex(x => x.rewardClaim.ClaimType == claimedReward.ClaimType && x.rewardClaim.Id == claimedReward.Id);
        if (foundIndex != -1)
        {
            AchievementRewardInfo claimed = displayList[foundIndex];
            claimed.claimed = true;
            rewardsListScrollView.Refresh(foundIndex);
            parent.AddRewardQueue(claimedReward);
        }
    }

    public void OnClaimedAllRewards(List<AchievementRewardClaim> rewardList)
    {
        rewardsListScrollView.GoToTop();

        for (int i = 0; i < displayList.Count; ++i)
            displayList[i].claimed = true;
        rewardsListScrollView.RefreshAll();  // disable all claim button

        BuildCollatedList(rewardList);

        claimBtn.gameObject.SetActive(false);
        skipBtn.SetActive(true);
        closeBtn.SetActive(false);
        StartCoroutine(LatePlayEfx(rewardList));
    }

    private IEnumerator LatePlayEfx(List<AchievementRewardClaim> rewardList)
    {
        yield return null;
        efxCoroutine = StartCoroutine(PlayClaimAllEfx(rewardList));
    }

    private IEnumerator PlayClaimAllEfx(List<AchievementRewardClaim> rewardList)
    {
        AchievementRewardClaim reward = rewardList[0];
        rewardList.RemoveAt(0);

        int foundIndex = displayList.FindIndex(x => x.rewardClaim.ClaimType == reward.ClaimType && x.rewardClaim.Id == reward.Id);
        if (foundIndex != -1)
        {
            AchievementRewardInfo claimed = displayList[foundIndex];
            rewardsListScrollView.PlayEfx(foundIndex);
            parent.AddRewardQueue(reward);
        }

        yield return efxWait;

        if (rewardList.Count > 0)
            efxCoroutine = StartCoroutine(PlayClaimAllEfx(rewardList));
        else
            StartCoroutine(DelayShowCollatedList());
    }

    private IEnumerator DelayShowCollatedList()
    {
        yield return efxWait;
        ShowCollatedRewardsList();
    }

    private void ShowCollatedRewardsList()
    {
        rewardsListScrollView.gameObject.SetActive(false);
        claimedRewardsScrollView.gameObject.SetActive(true);
        claimedRewardsScrollView.Populate(collatedList);
        closeBtn.SetActive(true);
    }

    private void BuildCollatedList(List<AchievementRewardClaim> rewardList)
    {
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
                        SideEffectUtils.AddToBuffDict(buffsMap, se);
                        if (!seIconPaths.ContainsKey(se.effecttype) && !string.IsNullOrEmpty(obj.rewardIconPath))
                            seIconPaths.Add(se.effecttype, obj.rewardIconPath);
                    }
                }
            }
        }

        foreach (var item in currencyAdded)
            collatedList.Add(new CollatedAchievementReward(AchievementRewardType.Currency, (int)item.Key, item.Value, ""));
        foreach (var item in itemsAdded)
            collatedList.Add(new CollatedAchievementReward(AchievementRewardType.Item, item.Key, item.Value, ""));
        foreach (var item in buffsMap)
        {
            string iconpath;
            seIconPaths.TryGetValue(item.Key, out iconpath);
            if (item.Value.Item1 > 0) // abs
            {
                int key = (int)item.Key;
                collatedList.Add(new CollatedAchievementReward(AchievementRewardType.SideEffect, key, item.Value.Item1, iconpath));
            }
            if (item.Value.Item2 > 0)  // relative
            {
                int key = (int)item.Key;
                collatedList.Add(new CollatedAchievementReward(AchievementRewardType.SideEffect, key, item.Value.Item2, iconpath));
            }
        }
    }

    public void OnClickClaimAllRewards()
    {
        RPCFactory.CombatRPC.ClaimAllAchievementRewards();
    }

    public void OnClickSkipEfx()
    {
        if (efxCoroutine != null)
        {
            StopCoroutine(efxCoroutine);
            efxCoroutine = null;
        }

        ShowCollatedRewardsList();
        parent.ForceUpdateLevelProgress();
    }

    public void SetClaimAllButton(bool interactable)
    {
        claimBtn.interactable = interactable;
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();

        rewardsListScrollView.Clear();
        claimedRewardsScrollView.Clear();
        currencyAdded.Clear();
        itemsAdded.Clear();
        buffsMap.Clear();
        collatedList.Clear();
        displayList.Clear();
        ObjMgr.Instance.ResetContainerObject(efxObjList);
        parent.ForceUpdateLevelProgress();
        if (efxCoroutine != null)
        {
            StopCoroutine(efxCoroutine);
            efxCoroutine = null;
        }
        claimBtn.gameObject.SetActive(true);
        claimBtn.interactable = true;
        skipBtn.SetActive(false);
    }

    public void PlayRewardEffect()
    {
        GameObject efxObj = ObjMgr.Instance.GetContainerObject(efxObjList);
        if (efxObj != null)
        {
            AchievementRewardEfx efx = efxObj.GetComponent<AchievementRewardEfx>();
            lastEfxIndex %= 5;
            efx.Play(++lastEfxIndex);
        }
    }
}