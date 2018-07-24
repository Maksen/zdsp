//#define _ENABLE_GET_PROPERTY_

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zealot.Client.Entities;
using Zealot.Repository;

public class UI_CharacterInfoTabTwo : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField]
    GameObject mSecondaryStatsPrefab;
    [SerializeField]
    List<UI_CharacterInfo_StatPlusMinus> mPlusMinusLst = new List<UI_CharacterInfo_StatPlusMinus>();
    List<UI_CharacterInfo_StatsReview> mSecondaryStatLst = new List<UI_CharacterInfo_StatsReview>();

    [Header("Content Header")]
    [SerializeField]
    GameObject mPlusMinusHead;
    [SerializeField]
    GameObject mSecondaryStatsHead;

    [Header("Remaining Stats Points")]
    [SerializeField]
    Text mRemainStatsValue;
    int mRemainStatsValueInt = 0;

    public void Awake()
    {
        for (int i = 0; i < mPlusMinusLst.Count; ++i)
        {
            mPlusMinusLst[i].mIncCallback = IncreaseStatCallback;
            mPlusMinusLst[i].mDncCallback = DecreaseStatCallback;
            mPlusMinusLst[i].mNoStatPtCallback = HasNoStatsPointRemaining;
        }

        GameObject obj = null;
        UI_CharacterInfo_StatsReview cisr = null;
        int max = (int)ClientUtils.CharacterSecondaryStats.NUM_SECONDARY_STATS;
        for (int i = 0; i < max; ++i)
        {
            obj = Instantiate(mSecondaryStatsPrefab, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(mSecondaryStatsHead.transform, false);
            obj.SetActive(false);

            cisr = obj.GetComponent<UI_CharacterInfo_StatsReview>();
            mSecondaryStatLst.Add(cisr);
        }

        //Localization
        mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.STR].TTName = mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.STR].Name;
        mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.AGI].TTName = mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.AGI].Name;
        mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.DEX].TTName = mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.DEX].Name;
        mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.CON].TTName = mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.CON].Name;
        mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.INT].TTName = mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.INT].Name;

        //Localize secondary stats
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.WEAPONATK_STR].Name  = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.WEAPONATK_STR);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.WEAPONATK_DEX].Name  = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.WEAPONATK_DEX);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.WEAPONATK_INT].Name  = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.WEAPONATK_INT);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.IGNORE_DEF].Name     = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.IGNORE_DEF);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.ATK_SPD].Name        = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.ATK_SPD);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.SKILL_CAST_SPD].Name = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.SKILL_CAST_SPD);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.HIT].Name            = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.HIT);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.FLEE].Name           = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.FLEE);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.CLASS_MAX_HP].Name   = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.CLASS_MAX_HP);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.CLASS_HP_REGEN].Name = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.CLASS_HP_REGEN);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.CLASS_MAX_SP].Name   = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.CLASS_MAX_SP);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.CLASS_SP_REGEN].Name = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.CLASS_SP_REGEN);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.RECOVERY_BONUS].Name = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.RECOVERY_BONUS);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.DEF].Name            = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.DEF);
        mSecondaryStatLst[(int)ClientUtils.CharacterSecondaryStats.ELEM_DEF].Name       = ClientUtils.GetLocalizedStatsName(ClientUtils.CharacterSecondaryStats.ELEM_DEF);
    }

    public void OnEnable()
    {
        SetStatsField();
    }
    public void OnDisable()
    {
       
    }

    /// <summary>
    /// Call this when character info window has a popup and now return back to focus
    /// </summary>
    public void OnRegainWindowContext()
    {
        SetStatsField();
    }

    #region Stat Cancel/Confirm button feedback
    public void OnConfirmStatsAllocation()
    {
        int totalspent = GameInfo.gLocalPlayer.LocalCombatStats.StatsPoint - mRemainStatsValueInt;
        if (totalspent == 0)
            return;

        //Notify server about decision
        RPCFactory.NonCombatRPC.CharacterInfoSpendStatsPoints(mPlusMinusLst[0].NewBaseValue - mPlusMinusLst[0].BaseValue,
                                                              mPlusMinusLst[1].NewBaseValue - mPlusMinusLst[1].BaseValue,
                                                              mPlusMinusLst[2].NewBaseValue - mPlusMinusLst[2].BaseValue,
                                                              mPlusMinusLst[3].NewBaseValue - mPlusMinusLst[3].BaseValue,
                                                              mPlusMinusLst[4].NewBaseValue - mPlusMinusLst[4].BaseValue);
    }
    public void OnConfirmStatsAllocation_ServerFeedback(int retVal)
    {
        if (retVal == -1)
        {
            //Error msg
            return;
        }

        for (int i = 0; i < mPlusMinusLst.Count; ++i)
        {
            mPlusMinusLst[i].SetStats();
        }

        //Clear affected stats
        for (int i = 0; i < mSecondaryStatLst.Count; ++i)
        {
            mSecondaryStatLst[i].Value = "";
            mSecondaryStatLst[i].gameObject.SetActive(false);
        }
    }
    public void OnCancelStatsAllocation()
    {
        //Return remaining stats point to original value
        mRemainStatsValueInt = GameInfo.gLocalPlayer.LocalCombatStats.StatsPoint;
        mRemainStatsValue.text = mRemainStatsValueInt.ToString();

        for (int i = 0; i < mPlusMinusLst.Count; ++i)
        {
            mPlusMinusLst[i].ResetStats();
        }

        //Clear affected stats
        for (int i = 0; i < mSecondaryStatLst.Count; ++i)
        {
            mSecondaryStatLst[i].Value = "";
            mSecondaryStatLst[i].gameObject.SetActive(false);
        }
    }
    #endregion

    #region Stat callback function
    private void IncreaseStatCallback(ClientUtils.CharacterBasicStats e, int statVal)
    {
        //Subtract total stats point left
        mRemainStatsValueInt--;
        mRemainStatsValue.text = mRemainStatsValueInt.ToString();

        //Set interactable to false for all buttons if remain stat value == 0
        if (HasNoStatsPointRemaining())
        {
            bool haspt = !HasNoStatsPointRemaining();
            for (int i = 0; i < mPlusMinusLst.Count; ++i)
            {
                mPlusMinusLst[i].PlusInteractable = haspt;
            }
        }

        //Hide secondary stat if value is zero
        if (statVal <= 0)
        {
            ClearSecondaryStatsDisplay(e, statVal);
            return;
        }

        //Calculate and display the result after adding stat
        Dictionary<ClientUtils.CharacterSecondaryStats, float> secDic2 = ClientUtils.GetSecStatsByStats(e, statVal);
        foreach (KeyValuePair<ClientUtils.CharacterSecondaryStats, float> pair in secDic2)
        {
            mSecondaryStatLst[(int)pair.Key].Value = pair.Value.ToString();
            mSecondaryStatLst[(int)pair.Key].gameObject.SetActive(true);
        }
    }
    private void DecreaseStatCallback(ClientUtils.CharacterBasicStats e, int statVal)
    {
        //Add total stats point left
        mRemainStatsValueInt++;
        mRemainStatsValue.text = mRemainStatsValueInt.ToString();

        //Set interactable to true for all buttons if remain stat previous value == 0
        if (mRemainStatsValueInt-1 == 0)
        {
            bool haspt = !HasNoStatsPointRemaining();
            for (int i = 0; i < mPlusMinusLst.Count; ++i)
            {
                mPlusMinusLst[i].PlusInteractable = haspt;
            }
        }

        //Hide secondary stat if value is zero
        if (statVal <= 0)
        {
            ClearSecondaryStatsDisplay(e, statVal);
            return;
        }

        //Calculate and display the result after adding stat
        Dictionary<ClientUtils.CharacterSecondaryStats, float> secDic2 = ClientUtils.GetSecStatsByStats(e, statVal);
        foreach (KeyValuePair<ClientUtils.CharacterSecondaryStats, float> pair in secDic2)
        {
            mSecondaryStatLst[(int)pair.Key].Value = pair.Value.ToString();
            mSecondaryStatLst[(int)pair.Key].gameObject.SetActive(true);
        }
    }
    private void ClearSecondaryStatsDisplay(ClientUtils.CharacterBasicStats e, int statVal)
    {
        Dictionary<ClientUtils.CharacterSecondaryStats, float> secDic1 = ClientUtils.GetSecStatsByStats(e, statVal);
        foreach (KeyValuePair<ClientUtils.CharacterSecondaryStats, float> pair in secDic1)
        {
            mSecondaryStatLst[(int)pair.Key].Value = "";
            mSecondaryStatLst[(int)pair.Key].gameObject.SetActive(false);
        }
    }
    private bool HasNoStatsPointRemaining()
    {
        return (mRemainStatsValueInt <= 0);
    }
    #endregion

    private void SetStatsField()
    {
        if (GameInfo.gLocalPlayer == null)
            return;

        var lcs = GameInfo.gLocalPlayer.LocalCombatStats;
        if (mPlusMinusLst != null && mPlusMinusLst.Count > 0)
        {
            //Update stats according to localcombatstats
            mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.STR].BaseValue = lcs.Strength;
            mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.STR].BonusValue = 0;

            mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.AGI].BaseValue = lcs.Agility;
            mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.AGI].BonusValue = 0;

            mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.CON].BaseValue = lcs.Constitution;
            mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.CON].BonusValue = 0;

            mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.INT].BaseValue = lcs.Intelligence;
            mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.INT].BonusValue = 0;

            mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.DEX].BaseValue = lcs.Dexterity;
            mPlusMinusLst[(int)ClientUtils.CharacterBasicStats.DEX].BonusValue = 0;
        }

        //Button reset
        OnCancelStatsAllocation();

        //Localization
        mRemainStatsValueInt = GameInfo.gLocalPlayer.LocalCombatStats.StatsPoint;   //Hack
        mRemainStatsValue.text = mRemainStatsValueInt.ToString();
    }
}
