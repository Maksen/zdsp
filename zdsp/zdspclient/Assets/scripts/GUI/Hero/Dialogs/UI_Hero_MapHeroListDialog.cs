using Kopio.JsonContracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_Hero_MapHeroListDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataParent;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] ScrollRect scrollRect;

    private int index;
    private Action<int, int> OnSelectCallback;

    public void Init(ExplorationMapJson mapData, int index, Action<int, int> selectCallback)
    {
        this.index = index;
        OnSelectCallback = selectCallback;
        HeroStatsClient heroStats = GameInfo.gLocalPlayer.HeroStats;
        var heroes = heroStats.GetHeroesDict().Values;
        List<Hero> sortedHeroList = heroes.Where(x => !x.IsAway).OrderBy(x => x, new HeroComparer(mapData)).ToList();
        for (int i = 0; i < sortedHeroList.Count; i++)
        {
            Hero hero = sortedHeroList[i];
            GameObject obj = ClientUtils.CreateChild(dataParent, dataPrefab);
            obj.GetComponent<Hero_MapHeroListData>().Init(hero, OnHeroSelected, mapData);
        }
    }

    private void OnHeroSelected(int heroId)
    {
        if (OnSelectCallback != null)
            OnSelectCallback(index, heroId);
        GetComponent<UIDialog>().ClickClose();
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        ClientUtils.DestroyChildren(dataParent);
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private class HeroComparer : IComparer<Hero>
    {
        public ExplorationMapJson mapJson { get; set; }

        public HeroComparer(ExplorationMapJson map)
        {
            mapJson = map;
        }

        public int Compare(Hero x, Hero y)
        {
            if ((x.Level < mapJson.reqherolevel || x.TrustLevel < mapJson.reqherotrust)
                && (y.Level < mapJson.reqherolevel || y.TrustLevel < mapJson.reqherotrust))
                return x.HeroId.CompareTo(y.HeroId);
            else if (x.Level >= mapJson.reqherolevel && x.TrustLevel >= mapJson.reqherotrust
                && (y.Level < mapJson.reqherolevel || y.TrustLevel < mapJson.reqherotrust))
                return -1;
            else if ((x.Level < mapJson.reqherolevel || x.TrustLevel < mapJson.reqherotrust)
                && (y.Level >= mapJson.reqherolevel && y.TrustLevel >= mapJson.reqherotrust))
                return 1;
            else if (x.GetNoOfMapCriteriaMet(mapJson) == y.GetNoOfMapCriteriaMet(mapJson))
                return x.HeroId.CompareTo(y.HeroId);
            else
                return y.GetNoOfMapCriteriaMet(mapJson).CompareTo(x.GetNoOfMapCriteriaMet(mapJson));
        }
    }
}
