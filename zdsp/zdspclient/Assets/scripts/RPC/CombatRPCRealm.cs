using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.RPC;
using Zealot.Repository;

public partial class ClientMain : MonoBehaviour
{
    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.EnterRealm)]
    public void EnterRealm(int realmId, byte realmState, int elapsed, long serverNowTick)
    {
        RealmJson mRealmInfo = RealmRepo.GetInfoById(realmId);       
        //LevelJson LevelInfo = LevelRepo.GetInfoById(mRealmInfo.level);
        GameInfo.mRealmInfo = mRealmInfo;
        GameInfo.mRealmState = (RealmState)realmState;
        if (mRealmInfo == null)
            return;
        IsRealmInfoReady = true;       
        RealmType realmType = mRealmInfo.type;  
        switch (realmType)
        {
            case RealmType.Dungeon:
                DateTime dtServerNow = new DateTime(serverNowTick);
                int actualElapsed = elapsed + (int)(DateTime.Now - dtServerNow).TotalSeconds;
                UIManager.GetWidget(HUDWidgetType.RealmExit).GetComponent<HUD_RealmExit>().Init(actualElapsed);
                break;
        }
    }

    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OnMissionCompleted)]
    public void OnMissionCompleted(bool success, int countdown)
    {
        GameInfo.mRealmState = RealmState.Ended;
        GameInfo.gLocalPlayer.Bot.StopBot();
        UIManager.GetWidget(HUDWidgetType.RealmExit).GetComponent<HUD_RealmExit>().OnMissionCompleted(countdown);
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
