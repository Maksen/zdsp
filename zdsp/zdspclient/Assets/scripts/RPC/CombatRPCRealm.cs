using UnityEngine;
using System;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;
using Zealot.Common.RPC;
using Zealot.Client.Entities;
using Zealot.Repository;

public partial class ClientMain : MonoBehaviour
{
    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.EnterRealm)]
    public void EnterRealm(int realmId, byte realmState, int elapsed)
    {
        RealmJson mRealmInfo = RealmRepo.GetInfoById(realmId);
        //LevelJson LevelInfo = LevelRepo.GetInfoById(mRealmInfo.level);
        GameInfo.mRealmInfo = mRealmInfo;
        if (mRealmInfo == null)
            return;
        IsRealmInfoReady = true;
        GameInfo.mRealmState = (RealmState)realmState;
        PlayerGhost player = GameInfo.gLocalPlayer;
        RealmType realmType = mRealmInfo.type;
        GameObject gameObj = null;
        //if (realmType != RealmType.RealmWorld)
        //{
        //    UIManager.SetWidgetActive(HUDWidgetType.Map, false);
        //    UIManager.SetWidgetActive(HUDWidgetType.Personal, false);
        //    UIManager.SetWidgetActive(HUDWidgetType.Spending, false);
        //    UIManager.SetWidgetActive(HUDWidgetType.MainMenu, false);
        //    UIManager.SetWidgetActive(HUDWidgetType.Party, false);
        //    UIManager.SetWidgetActive(HUDWidgetType.Bidding_GuildWar_FactionWar_Party, false);
        //    UIManager.SetWidgetActive(HUDWidgetType.VIP, false);
        //    UIManager.SetWidgetActive(HUDWidgetType.TopUp, false);
        //    UIManager.SetWidgetActive(HUDWidgetType.WelfareFirstTopUp, false);
             
        //}
    }

    private void ArenaPrepareCounddown(object arg)
    {
        if (GameInfo.gLocalPlayer == null)
            return;
        int countdown = (int)arg;
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("countdown", countdown.ToString());
        UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_ArenaPrepareCountdown", parameters));
        countdown--;
        if (countdown > 0)
            mTimers.SetTimer(1000, ArenaPrepareCounddown, countdown);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.StartCountDown)]
    public void StartCountDown(int count)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("countdown", count.ToString());
        UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_GameStartCountDown", parameters));
    }
}
