using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
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

    [SerializeField]
    GameObject mPartyLeaderIcon;

    [SerializeField]
    Image mPortraitIcon;

    public void UpdateHPBar(int value, int max)
    {
        mHPBar.Max = max;
        mHPBar.Value = value;
    }

    public void UpdateMPBar(int value, int max)
    {
        mMPBar.Max = max;
        mMPBar.Value = value;
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
        float xpFloatVal = (float)ss.experience / CharacterLevelRepo.GetExpByLevel(pss.Level + 1);
        int xpIntVal = Mathf.FloorToInt(xpFloatVal * 100);

        mEXPPercent.text = xpIntVal.ToString() + "%";
    }

    public void UpdateLevel(int level)
    {
        mLevel.text = level.ToString();
    }

    public void SetPartyLeader(bool isLeader)
    {
        mPartyLeaderIcon.SetActive(isLeader);
    }

    public void UpdatePortrait(byte jobSect)
    {
        mPortraitIcon.sprite = ClientUtils.LoadIcon(JobSectRepo.GetJobPortraitPath((JobType)jobSect));
    }
}