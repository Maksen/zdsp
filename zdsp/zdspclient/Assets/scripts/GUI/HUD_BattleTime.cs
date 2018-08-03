using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class HUD_BattleTime : MonoBehaviour {

    public Text battleTimeTxt;

    public void UpdateBattleTime(int battleTime)
    {
        if (battleTime > 0)
            battleTimeTxt.text = string.Format("{0}{1}", Mathf.CeilToInt(battleTime / 60.0f), GUILocalizationRepo.GetLocalizedString("time_minute"));
        else
            battleTimeTxt.text = 0 + GUILocalizationRepo.GetLocalizedString("time_minute");
    }
}
