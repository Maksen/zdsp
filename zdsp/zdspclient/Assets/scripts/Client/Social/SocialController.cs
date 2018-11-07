using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Common.Entities;

public class SocialController {
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
