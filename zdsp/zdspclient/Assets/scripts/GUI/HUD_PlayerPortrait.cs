using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Zealot.Common.Entities;
using Zealot.Repository;

public class HUD_PlayerPortrait : MonoBehaviour
{
    [SerializeField]
    UI_ProgressBarC mHPBar;
    [SerializeField]
    UI_ProgressBarC mMPBar;
    [SerializeField]
    Text mEXPPercent;
    [SerializeField]
    Text mLevel;

    public void Awake()
    {
    }

    public void UpdateHPMPBar()
    {
        if (GameInfo.gLocalPlayer == null)
        {
            Debug.LogError("HUD_PlayerPortrait.UpdateHPMPBar: GameInfo.gLocalPlayer is null?!");
            return;
        }

        //PlayerSynStats pss = GameInfo.gLocalPlayer.PlayerSynStats;
        LocalCombatStats lcs = GameInfo.gLocalPlayer.LocalCombatStats;
        mHPBar.Max = lcs.HealthMax;
        mHPBar.Value = lcs.Health;
        mMPBar.Max = lcs.ManaMax;
        mMPBar.Value = lcs.Mana;
    }
    public void UpdateExp()
    {
        if (GameInfo.gLocalPlayer == null)
        {
            Debug.LogError("HUD_PlayerPortrait.UpdateExp: GameInfo.gLocalPlayer is null?!");
            return;
        }

        PlayerSynStats pss = GameInfo.gLocalPlayer.PlayerSynStats;
        SecondaryStats ss = GameInfo.gLocalPlayer.SecondaryStats;
        float xpFloatVal = (float)ss.experience / CharacterLevelRepo.GetExpByLevel(pss.Level);
        int xpIntVal = Mathf.FloorToInt(xpFloatVal * 100);

        mEXPPercent.text = xpIntVal.ToString() + "%";
    }
    public void UpdateLevel()
    {
        if (GameInfo.gLocalPlayer == null)
        {
            Debug.LogError("HUD_PlayerPortrait.UpdateLevel: GameInfo.gLocalPlayer is null?!");
            return;
        }

        PlayerSynStats pss = GameInfo.gLocalPlayer.PlayerSynStats;

        mLevel.text = pss.Level.ToString();
    }
}
