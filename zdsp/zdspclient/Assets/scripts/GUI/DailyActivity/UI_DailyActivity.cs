using System;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;

public class UI_DailyActivity : BaseWindowBehaviour
{
    public DefaultToggleInGroup TabController;   
    
    //3rd Tab worldboss
    public void Ret_GetWorldBossList(Dictionary<int, SpecialBossStatus> specialBossStatus)
    {
        GameObject content_boss = TabController.GetPageContent(2);
        if (content_boss.activeInHierarchy)
            content_boss.GetComponent<UI_SpecialBoss_Detail>().Init(specialBossStatus);
    }
}

