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
            {
                mHeroesDict.Remove(hero.HeroId);  // remove for debug only
                Debug.Log("Remove hero: " + hero.HeroId);
            }
        }
    }

    private void AddHero(string str)
    {
        Debug.Log("Add hero: " + str);
        Hero hero = Hero.ToObject(str);
        hero.HeroJson = HeroRepo.GetHeroById(hero.HeroId);
        mHeroesDict[hero.HeroId] = hero;

        if (windowObj.activeInHierarchy)
            uiHero.OnHeroAdded(hero);
    }

    private void UpdateHero(Hero oldHero, string str)
    {
        Debug.Log("Update hero: " + str);
        Hero newHero = Hero.ToObject(str);
        newHero.HeroJson = oldHero.HeroJson;
        mHeroesDict[newHero.HeroId] = newHero;

        // todo: check whether can put back active check when handle unlock skin item
        uiHero.OnHeroUpdated(oldHero, newHero);
    }

    public void UpdateExplorations(string value)
    {
        Debug.Log("Update explorations: " + value);
        var newExplorationDict = JsonConvertDefaultSetting.DeserializeObject<Dictionary<int, ExploreMapData>>(value);
        foreach (var map in newExplorationDict)
        {
            ExploreMapData newMap = map.Value;
            if (!explorationsDict.ContainsKey(map.Key))  // current not in dict so is new map
            {
                newMap.MapData = HeroRepo.GetExplorationMapById(map.Key);
                explorationsDict.Add(map.Key, newMap);
            }
            else // update existing map
            {
                newMap.MapData = explorationsDict[map.Key].MapData;
                explorationsDict[map.Key] = newMap;
            }
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
}