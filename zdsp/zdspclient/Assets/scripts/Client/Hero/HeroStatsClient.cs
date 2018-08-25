using Kopio.JsonContracts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;

public class HeroStatsClient : HeroStats
{
    private GameObject windowObj;
    private UI_Hero uiHero;
    private PlayerGhost localPlayer;
    private bool isInit = true;

    public void Init()
    {
        localPlayer = GameInfo.gLocalPlayer;

        windowObj = UIManager.GetWindowGameObject(WindowType.Hero);
        uiHero = windowObj.GetComponent<UI_Hero>();

        for (int i = 0; i < heroes.Count; i++)
        {
            if (heroes[i] != null)
            {
                string info = (string)heroes[i];
                AddHero(info);
            }
        }

        ParseExplorationsString();
        ParseExploredString();

        isInit = false;
    }

    public void UpdateHeroesList(byte idx, string value)
    {
        Hero hero = mHeroesDict.Values.FirstOrDefault(x => x.SlotIdx == idx);
        if (hero == null)  // currently not in list so is new hero
        {
            if (!string.IsNullOrEmpty(value))
                AddHero(value);
        }
        else
        {
            if (!string.IsNullOrEmpty(value))  // hero info updated
                UpdateHero(hero, value);
            else
                mHeroesDict.Remove(hero.HeroId);  // remove for debug only
        }
    }

    private void AddHero(string str)
    {
        //Debug.Log("Add hero: " + str);
        Hero hero = Hero.ToObject(str);
        hero.HeroJson = HeroRepo.GetHeroById(hero.HeroId);
        mHeroesDict[hero.HeroId] = hero;

        if (!isInit)
        {
            if (windowObj.activeInHierarchy)
                uiHero.OnHeroAdded(hero);
        }
    }

    private void UpdateHero(Hero oldHero, string str)
    {
        //Debug.Log("Update hero: " + str);
        Hero newHero = Hero.ToObject(str);
        newHero.HeroJson = oldHero.HeroJson;
        mHeroesDict[newHero.HeroId] = newHero;

        if (windowObj.activeInHierarchy)
            uiHero.OnHeroUpdated(oldHero, newHero);
    }

    public void UpdateExplorations(string value)
    {
        //Debug.Log("Update explorations: " + value);
        if (!string.IsNullOrEmpty(value))
        {
            var newExplorationDict = JsonConvertDefaultSetting.DeserializeObject<Dictionary<int, ExploreMapData>>(value);
            foreach (var map in newExplorationDict)
            {
                explorationsDict[map.Key] = map.Value;  // add or update existing map
            }

            int mapIdToRemove = 0;
            foreach (var map in explorationsDict)
            {
                if (!newExplorationDict.ContainsKey(map.Key))  // existing map is not in new dict so is removed
                {
                    mapIdToRemove = map.Key;
                    break;
                }
            }
            if (mapIdToRemove > 0)
                explorationsDict.Remove(mapIdToRemove);
        }
        else
            explorationsDict.Clear();

        if (windowObj.activeInHierarchy)
            uiHero.OnExplorationsUpdated();
    }

    public void UpdateExploredMaps()
    {
        //Debug.Log("Update explored maps: " + Explored);
        ParseExploredString();
    }

    public bool IsHeroSummoned(int heroId)
    {
        return SummonedHeroId == heroId;
    }

    public void OnSummonedHeroChanged()
    {
        uiHero.OnSummonedHeroChanged();
    }

    public void OnInterestRandomSpinResult(byte interest)
    {
        uiHero.OnInterestRandomSpinResult(interest);
    }

    public bool IsMapCompleted(int mapId)
    {
        ExploreMapData map = GetExploringMap(mapId);
        if (map != null)
            return map.Completed;
        return false;
    }

    public int GetFulfilledChestCount(ExplorationMapJson mapData, List<int> heroIds)
    {
        List<Hero> list = new List<Hero>();
        for (int i = 0; i < heroIds.Count; i++)
        {
            Hero hero = GetHero(heroIds[i]);
            if (hero != null)
                list.Add(hero);
        }

        int count = 1;  // minimum 1
        if (IsChestRequirementFulfilled(mapData.chestreqtype1, mapData.chestreqvalue1, list))
            count++;
        if (IsChestRequirementFulfilled(mapData.chestreqtype2, mapData.chestreqvalue2, list))
            count++;
        if (IsChestRequirementFulfilled(mapData.chestreqtype3, mapData.chestreqvalue3, list))
            count++;
        return count;
    }
}