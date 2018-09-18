using System;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;

public class UI_DailyActivity : BaseWindowBehaviour
{
    [SerializeField]
    UI_DailyQuest DailyQuest;

    [SerializeField]
    UI_Donate Donate;

    public DefaultToggleInGroup TabController;   
    
    //3rd Tab worldboss
    public void Ret_GetWorldBossList(Dictionary<int, SpecialBossStatus> specialBossStatus)
    {
        GameObject content_boss = TabController.GetPageContent(2);
        if (content_boss.activeInHierarchy)
            content_boss.GetComponent<UI_SpecialBoss_Detail>().Init(specialBossStatus);
    }

    public void UpdateDailyQuest()
    {
        if (DailyQuest.gameObject.activeSelf)
        {
            DailyQuest.RefreshSignboard();
        }
    }

    public void UpdateDonate()
    {
        if (Donate.gameObject.activeSelf)
        {
            Donate.RefreshDonate();
        }
    }

    public void UpdateDonateData(string guid,int result)
    {
        if (Donate.gameObject.activeSelf)
        {
            Donate.UpdateDonateData(guid, result);
        }
    }
}

