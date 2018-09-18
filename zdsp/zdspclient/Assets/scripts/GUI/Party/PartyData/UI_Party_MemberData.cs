using System;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class UI_Party_MemberData : UI_Party_RequestBase
{
    [SerializeField] GameObject leaderIcon;

    public void Init(string charName, int charLvl, int portraitId, bool isLeader)
    {
        InitBase(charName, charLvl);
        leaderIcon.SetActive(isLeader);
        int jobTypeCount = Enum.GetNames(typeof(JobType)).Length;
        if (portraitId > jobTypeCount)  // is a hero
        {
            int heroId = portraitId - jobTypeCount;
            var heroData = HeroRepo.GetHeroById(heroId);
            if (heroData != null)
                portraitImage.sprite = ClientUtils.LoadIcon(heroData.smallportraitpath);
        }
        else  // is player
        {
            portraitImage.sprite = ClientUtils.LoadIcon(JobSectRepo.GetJobPortraitPath((JobType)portraitId));
        }
    }
}