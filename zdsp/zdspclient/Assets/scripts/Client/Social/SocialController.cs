using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using System;

public class SocialController {

    public static void AddSystemMessage(string msgName, string args)
    {
        string message = GUILocalizationRepo.GetLocalizedSysMsgByName(msgName, GameUtils.FormatString(args));
        UIManager.ShowSystemMessage(message);
    }
    public static void OpenOkDialog(string msgName, string args, Action callback = null)
    {
        string message = GUILocalizationRepo.GetLocalizedSysMsgByName(msgName, GameUtils.FormatString(args));
        UIManager.OpenOkDialog(message, callback);
    }

    PlayerGhost player;
    SocialStats stats;
    public SocialController(PlayerGhost player)
    {
        this.player = player;
        this.stats = player.SocialStats;


         var cm= Component.FindObjectOfType<ClientMain>();
        if(cm!=null)
        {
            var tool = cm.GetComponent<SocialTestTool>();
            if (tool == null)
                cm.gameObject.AddComponent<SocialTestTool>();
        }
    }
    public void OnValueChanged(string field, object value, object oldvalue)
    {
        if(field=="msg")
        {
            
        }
    }
    public void OnNewlyAdded()
    {

    }
    public void DD()
    {

    }
}
