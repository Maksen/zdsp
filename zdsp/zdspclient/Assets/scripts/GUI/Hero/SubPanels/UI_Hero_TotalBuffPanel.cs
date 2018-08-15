using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common.Entities;
using Zealot.Repository;

public class UI_Hero_TotalBuffPanel : MonoBehaviour
{
    [SerializeField] Transform dataParent;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] ScrollRect scrollRect;

    private Dictionary<int, float> sdgMap = new Dictionary<int, float>();  // sdg id -> amount

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
                    // add up the amount if have previous same type sideeffect 
                    if (sdgMap.ContainsKey(se.sdg))
                        sdgMap[se.sdg] += se.max; // for passive se min == max, just use max for consistency
                    else
                        sdgMap.Add(se.sdg, se.max);
                }
            }
        }

        PopulateList();
    }

    private void PopulateList()
    {
        foreach (var sdg in sdgMap)
        {
            GameObject obj = ClientUtils.CreateChild(dataParent, dataPrefab);
            string sdgText = SDGRepo.GetDescriptionByID(sdg.Key);
            obj.GetComponent<Hero_Text>().SetText(SDGRepo.ReplaceText(sdgText, "max", sdg.Value));
        }
    }

    public void Clear()
    {
        sdgMap.Clear();
        ClientUtils.DestroyChildren(dataParent);
        scrollRect.verticalNormalizedPosition = 1f;
    }
}