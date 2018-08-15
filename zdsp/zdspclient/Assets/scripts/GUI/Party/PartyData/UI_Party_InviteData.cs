using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Party_InviteData : UI_Party_RequestBase
{
    [SerializeField] Text jobText;
    [SerializeField] Button button;
    [SerializeField] GameObject spacer;

    private UnityAction<string> inviteAction;
    private UnityAction<int> inviteHeroAction;
    private int heroId;

    public void Init(string name, int charlevel, JobType job, int portrait, UnityAction<string> action)
    {
        InitBase(name, charlevel);
        portraitImage.sprite = ClientUtils.LoadIcon("UI_ZDSP_Icons/Portraits/zzz_Test.png");  // todo: temp
        jobText.text = JobSectRepo.GetJobLocalizedName(job);
        inviteAction = action;
    }

    public void InitHero(Hero hero, UnityAction<int> action)
    {
        InitBase(hero.HeroJson.localizedname, hero.Level);
        portraitImage.sprite = ClientUtils.LoadIcon(hero.HeroJson.smallportraitpath);
        jobText.text = "";
        spacer.SetActive(false);
        inviteHeroAction = action;
        heroId = hero.HeroId;
    }

    public void OnClickInvite()
    {
        if (inviteAction != null)
        {
            inviteAction(nameText.text);
            button.interactable = false;
        }
        else if (inviteHeroAction != null)
        {
            inviteHeroAction(heroId);
            button.interactable = false;
        }
    }
}