using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_Hero_MapEfficiencyDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataParent;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] ScrollRect scrollRect;

    public void Init(ExplorationMapJson mapData, List<int> heroIds)
    {
        for (int i = 0; i < heroIds.Count; i++)
        {
            Hero hero = GameInfo.gLocalPlayer.HeroStats.GetHero(heroIds[i]);
            if (hero == null) // should not happen
                continue;
            GameObject obj = ClientUtils.CreateChild(dataParent, dataPrefab);
            obj.GetComponent<Hero_MapEfficiencyData>().Init(hero, mapData);
        }
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        ClientUtils.DestroyChildren(dataParent);
        scrollRect.verticalNormalizedPosition = 1f;
    }
}