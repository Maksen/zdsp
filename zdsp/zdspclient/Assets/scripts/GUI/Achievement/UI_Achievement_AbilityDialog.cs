using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class UI_Achievement_AbilityDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataParent;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] ScrollRect scrollRect;

    private Dictionary<EffectType, ValuePair<float, float>> buffsMap = new Dictionary<EffectType, ValuePair<float, float>>();

    public override void OnOpenWindow()
    {
        base.OnOpenWindow();
        PlayerGhost player = GameInfo.gLocalPlayer;

        // from achievement level
        AchievementLevel levelInfo = AchievementRepo.GetAchievementLevelInfo(player.PlayerSynStats.AchievementLevel);
        if (levelInfo != null)
        {
            List<SideEffectJson> passiveSEs = levelInfo.sideEffects;
            for (int i = 0; i < passiveSEs.Count; ++i)
                AddToBuffMap(passiveSEs[i]);
        }

        // from collections claimed and stored
        var collections = player.AchievementStats.GetCollectionsDict();
        foreach (var elem in collections)
        {
            CollectionObjective obj = AchievementRepo.GetCollectionObjectiveById(elem.Key);
            if (obj != null)
            {
                if (obj.rewardType == AchievementRewardType.SideEffect && obj.rewardId > 0 && elem.Value.Claimed)
                    AddToBuffMap(SideEffectRepo.GetSideEffect(obj.rewardId));
                if ((obj.type == CollectionType.Fashion || obj.type == CollectionType.Relic) && elem.Value.Stored)
                {
                    for (int i = 0; i < obj.storeSEs.Count; ++i)
                        AddToBuffMap(obj.storeSEs[i]);
                }
            }
        }

        // from achievements claimed
        var achievements = player.AchievementStats.GetAchievementsDict();
        foreach (var elem in achievements)
        {
            AchievementObjective obj = AchievementRepo.GetAchievementObjectiveById(elem.Key);
            if (obj != null)
            {
                if (obj.rewardType == AchievementRewardType.SideEffect && obj.rewardId > 0 && elem.Value.Claimed)
                    AddToBuffMap(SideEffectRepo.GetSideEffect(obj.rewardId));
            }
        }

        Populate();
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

    private void Populate()
    {
        int length = SideEffectUtils.buffTypeGroups.Length;
        for (int i = 0; i < length; ++i)
        {
            int child = 0;
            GameObject lastChild = null;
            List<EffectType> currentGrp = SideEffectUtils.buffTypeGroups[i];
            for (int j = 0; j < currentGrp.Count; ++j)
            {
                EffectType effectType = currentGrp[j];
                ValuePair<float, float> pair;
                if (buffsMap.TryGetValue(effectType, out pair))
                {
                    if (pair.Item1 > 0)
                    {
                        if (child % 2 == 0)
                        {
                            lastChild = ClientUtils.CreateChild(dataParent, dataPrefab);
                            lastChild.GetComponent<Achievement_AbilityData>().SetLeftData(effectType.ToString(), pair.Item1, false);
                            child++;
                        }
                        else
                        {
                            lastChild.GetComponent<Achievement_AbilityData>().SetRightData(effectType.ToString(), pair.Item1, false);
                            child++;
                        }
                    }

                    if (pair.Item2 > 0)
                    {
                        if (child % 2 == 0)
                        {
                            lastChild = ClientUtils.CreateChild(dataParent, dataPrefab);
                            lastChild.GetComponent<Achievement_AbilityData>().SetLeftData(effectType.ToString(), pair.Item2, true);
                            child++;
                        }
                        else
                        {
                            lastChild.GetComponent<Achievement_AbilityData>().SetRightData(effectType.ToString(), pair.Item2, true);
                            child++;
                        }
                    }
                }
            }
        }
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        ClientUtils.DestroyChildren(dataParent);
        scrollRect.verticalNormalizedPosition = 1f;
        buffsMap.Clear();
    }
}