using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_BattleTime : MonoBehaviour {

    public Text battleTimeTxt;

    public void UpdateBattleTime(float battleTime)
    {
        
        battleTimeTxt.text = string.Format("{0}分鐘", battleTime);
    }
}
