using System;
using UnityEngine;

public class UI_Hero : BaseWindowBehaviour
{
    [SerializeField] UI_Hero_Info uiHeroInfo;

    public override void OnOpenWindow()
    {
        base.OnOpenWindow();
        uiHeroInfo.Init();
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
    }

    public void OnSummonedHeroChanged()
    {
        uiHeroInfo.OnSummonedHeroChanged();
    }

    public void OnHeroAdded(Hero hero)
    {
        uiHeroInfo.OnHeroAdded(hero);
    }

    internal void OnHeroUpdated(Hero oldHero, Hero newHero)
    {
        uiHeroInfo.OnHeroUpdated(oldHero, newHero);
    }
}
