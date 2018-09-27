using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class HUD_RealmExit : MonoBehaviour
{
    // Editor Linked Gameobjects
    [SerializeField]
    Text txtTimer = null;

    RealmState realmState = RealmState.Preparation;
    float timeLeft = 0;

    public void Init(int elapsedTime)
    {      
        realmState = GameInfo.mRealmState;
        switch (realmState)
        {
            case RealmState.Preparation:
                timeLeft = GameInfo.mRealmInfo.preparation - elapsedTime;
                break;
            case RealmState.Started:
                timeLeft = GameInfo.mRealmInfo.timelimit - elapsedTime;
                break;
        }        
        if (timeLeft <= 0)
            OnTimesUp();
        UIManager.SetWidgetActive(HUDWidgetType.RealmExit, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            SetTimerText(timeLeft);
        }
        else if (timeLeft < 0)
            OnTimesUp();
    }

    public void OnTimesUp()
    {    
        timeLeft = 0;
        switch (realmState)
        {
            case RealmState.Created:
            case RealmState.Preparation:
                GameInfo.mRealmState = realmState = RealmState.Started;
                RealmJson mRealmInfo = GameInfo.mRealmInfo;
                timeLeft = (mRealmInfo != null) ? mRealmInfo.timelimit : 0;
                break;
            case RealmState.Started:
                GameInfo.mRealmState = realmState = RealmState.Ended;
                break;
        }
        SetTimerText(timeLeft);
    }

    public void SetTimerText(float timeLeft)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds((timeLeft > 0) ? timeLeft+1 : timeLeft);
        txtTimer.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

    public void OnMissionCompleted(int countdown)
    {
        realmState = RealmState.Ended;
        timeLeft = countdown;
    }

    public void OnClickExitRealm()
    {
        UIManager.OpenYesNoDialog(GUILocalizationRepo.GetLocalizedString("dun_LeaveRealmConfirm"), 
            () => RPCFactory.CombatRPC.LeaveRealm(), null);
    }
}
