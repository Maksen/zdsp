using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;

public class UI_Hero_TotalBuffPanel : MonoBehaviour
{
    [SerializeField] Transform dataParent;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] ScrollRect scrollRect;

    private Dictionary<EffectType, ValuePair<float, float>> buffMap = new Dictionary<EffectType, ValuePair<float, float>>();

    public void Init()
    {
        Clear();

        HeroStatsClient heroStats = GameInfo.gLocalPlayer.HeroStats;
        List<HeroBond> allBondsList = HeroRepo.heroBondsList;
        for (int i = 0; i < allBondsList.Count; i++)
        {
            HeroBond bond = allBondsList[i];
            HeroBondJson highestLevelData = bond.GetHighestFulfilledLevel(heroStats);
            if (highestLevelData != null)
            {
                foreach (SideEffectJson se in highestLevelData.sideeffects.Values)
                {
                    AddToBuffMap(se);
                }
            }
        }

        PopulateList();
    }

    private void AddToBuffMap(SideEffectJson se)
    {
        if (se == null)
            return;

        if (buffMap.ContainsKey(se.effecttype))
        {
            if (se.isrelative)
                buffMap[se.effecttype].Item2 += se.max;
            else
                buffMap[se.effecttype].Item1 += se.max;
        }
        else
        {
            if (se.isrelative)
                buffMap.Add(se.effecttype, new ValuePair<float, float>(0, se.max));
            else
                buffMap.Add(se.effecttype, new ValuePair<float, float>(se.max, 0));
        }
    }

    private void PopulateList()
    {
        int length = SideEffectUtils.buffTypeGroups.Length;
        for (int i = 0; i < length; ++i)
        {
            List<EffectType> currentGrp = SideEffectUtils.buffTypeGroups[i];
            for (int j = 0; j < currentGrp.Count; ++j)
            {
                EffectType effectType = currentGrp[j];
                ValuePair<float, float> pair;
                if (buffMap.TryGetValue(effectType, out pair))
                {
                    if (pair.Item1 > 0)
                    {
                        GameObject obj = ClientUtils.CreateChild(dataParent, dataPrefab);
                        string text = string.Format("{0} +{1}", effectType.ToString(), pair.Item1); // todo: jm localize
                        obj.GetComponent<Hero_Text>().SetText(text);
                    }

                    if (pair.Item2 > 0)
                    {
                        GameObject obj = ClientUtils.CreateChild(dataParent, dataPrefab);
                        string text = string.Format("{0} +{1}%", effectType.ToString(), pair.Item2);
                        obj.GetComponent<Hero_Text>().SetText(text);
                    }
                }
            }
        }
    }

    public void Clear()
    {
        buffMap.Clear();
        ClientUtils.DestroyChildren(dataParent);
        scrollRect.verticalNormalizedPosition = 1f;
    }
}