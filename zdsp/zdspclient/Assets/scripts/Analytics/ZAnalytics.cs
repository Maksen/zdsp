using UnityEngine.Analytics;
using System;
using System.Collections.Generic;

public class ZAnalytics : MonoSingleton<ZAnalytics>
{
    public void LoginUser(string userId)
    {
#if ZANALYTICS
        Analytics.SetUserId(userId);
#endif
    }

    public void LoginPlayer(int playerChar)
    {
    }

    public void OnLobbyLoaded()
    {
    }

    public void OnLevelChanged(int rank, int level)
    {
        int progressLevel = rank * 1000 + level;
    }

    public void OnNewTutorial(int tutorialId, int step)
    {
#if ZANALYTICS
        if (step == 1)
        {
            //int stepnumber = TutorialRepo.GetTutorialStepNumber(tutorialId);
            //if (stepnumber != 0)
            //{
            //    Analytics.CustomEvent("Tutorial", new Dictionary<string, object>
            //    {
            //        {"Tutorial Step", stepnumber }
            //    });
            //}
        }
#endif
    }

    public void TestCustomEvent()
    {
#if ZANALYTICS
        Analytics.CustomEvent("Custom Test", new Dictionary<string, object>
        {
            {"Time", DateTime.Now.Ticks },
            {"Test", "Test String" },
        });
#endif
    }

    public void OnStartRealm(int realmId)
    {

    }

    public void OnEndRealm(int realmId)
    {

    }

    public void OnVIPChanged(byte vipLevel)
    {

    }

    public void ResetCharacterPoints()
    {

    }

    public void OnUseItem(int itemid, int count)
    {

    }

    public void OnPurchaseItem(int itemid, int count)
    {

    }
}
