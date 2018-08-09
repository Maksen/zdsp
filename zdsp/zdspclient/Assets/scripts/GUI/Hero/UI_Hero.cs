using System;
using UnityEngine;

public class UI_Hero : BaseWindowBehaviour
{
    [SerializeField] UI_Hero_Info uiHeroInfo;

    public override void OnRegister()
    {
        base.OnRegister();
        uiHeroInfo.Setup();
    }

    public override void OnOpenWindow()
    {
        base.OnOpenWindow();
        uiHeroInfo.Init();
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        uiHeroInfo.CleanUp();
    }

    public void OnSummonedHeroChanged()
    {
        uiHeroInfo.OnSummonedHeroChanged();
    }

    public void OnHeroAdded(Hero hero)
    {
        uiHeroInfo.OnHeroAdded(hero);
    }

    public void OnHeroUpdated(Hero oldHero, Hero newHero)
    {
        uiHeroInfo.OnHeroUpdated(oldHero, newHero);
    }

    public void OnInterestRandomSpinResult(byte interest)
    {
        uiHeroInfo.OnInterestRandomSpinResult(interest);
    }
}
