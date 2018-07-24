using System;
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

        UpdateExplorations();
        UpdateExploredMaps();
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

        if (windowObj.activeInHierarchy)
            uiHero.OnHeroUpdated(oldHero, newHero);
    }

    public void UpdateExplorations()
    {
        //Debug.Log("Update explorations: " + Explorations);
        ParseExplorationsString();
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
}
