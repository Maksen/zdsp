using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UIWidgets;

using System.Collections;
using System.Collections.Generic;

public class UI_CharacterInfo_StatPlusMinus : MonoBehaviour
{
    static float holdDelay = 0.4f;
    static float holdChangeDelay = 0.1f;

    public delegate void StatsIncCallback(ClientUtils.CharacterBasicStats e, int statVal);
    public delegate void StatsDncCallback(ClientUtils.CharacterBasicStats e, int statVal);
    public delegate bool HasStatsPoints();

    [Header("UI Display")]
    [SerializeField]
    Text mName;
    [SerializeField]
    Text mValue;
    [SerializeField]
    string mBaseBonusOperator = "+";

    [Header("Buttons")]
    [SerializeField]
    ButtonAdvanced mBtnPlus;
    [SerializeField]
    ButtonAdvanced mBtnMinus;

    [Header("Stat Type")]
    [SerializeField]
    ClientUtils.CharacterBasicStats mStatType;

    [SerializeField]
    Text mTooltipName;
    [SerializeField]
    Text mTooltipValue;

    int mIntBaseValue = 0;
    int mTempIntBaseValue = 0; //value for user to see when adding/subtracting
    int mIntBonusValue = 0;
    string mBaseValue;
    string mBonusValue;
    Coroutine minusCoroutine = null;
    Coroutine plusCoroutine = null;
    string mTooltipVal;
    string mTooltipPercent = " (NA%)";

    public StatsIncCallback mIncCallback;
    public StatsDncCallback mDncCallback;
    public HasStatsPoints mNoStatPtCallback;

    #region Property
    public string Name
    {
        get { return mName.text; }
        set { mName.text = value; }
    }
    public int BaseValue
    {
        get
        {
            return mIntBaseValue;
        }
        set
        {
            mTempIntBaseValue = mIntBaseValue = value;
            mBaseValue = value.ToString();
            mValue.text = mBaseValue + mBaseBonusOperator + mBonusValue;

            TTVal = (value + mIntBonusValue).ToString();
        }
    }
    public int BonusValue
    {
        set
        {
            mIntBonusValue = value;
            mBonusValue = value.ToString();
            mValue.text = mBaseValue + mBaseBonusOperator + mBonusValue;

            TTVal = (mIntBaseValue + value).ToString();
        }
    }
    public ClientUtils.CharacterBasicStats StatType
    {
        set { mStatType = value; }
    }
    public bool PlusInteractable
    {
        get { return mBtnPlus.IsInteractable(); }
        set { mBtnPlus.interactable = value; }
    }
    public string TTName
    {
        set { mTooltipName.text = value; }
    }
    public string TTVal
    {
        set
        {
            mTooltipVal = value;
            mTooltipValue.text = mTooltipVal + mTooltipPercent;
        }
    }
    public string TTPercent
    {
        set
        {
            mTooltipPercent = " (" + value + "%)";
            mTooltipValue.text = mTooltipVal + mTooltipPercent;
        }
    }
    public string CombineName
    {
        get { return mName.text; }
        set
        {
            mName.text = value;
            mTooltipName.text = value;
        }
    }

    public int NewBaseValue
    {
        get { return mTempIntBaseValue; }
    }
    #endregion

    public void Awake()
    {
        mBtnPlus.onClick.AddListener(OnClick_StatsPlus);
        mBtnMinus.onClick.AddListener(OnClick_StatsMinus);

        mBtnPlus.onPointerDown.AddListener(OnDown_StatsPlus);
        mBtnMinus.onPointerDown.AddListener(OnDown_StatsMinus);

        mBtnPlus.onPointerUp.AddListener(OnUp_StatsPlus);
        mBtnMinus.onPointerUp.AddListener(OnUp_StatsMinus);
    }

    public void OnEnable()
    {
        mBtnPlus.interactable = !mNoStatPtCallback();
    }

    public void SetStats()
    {
        BaseValue = mTempIntBaseValue;
        mBtnMinus.interactable = false;
        mBtnPlus.interactable = !mNoStatPtCallback();
    }
    public void ResetStats()
    {
        BaseValue = mIntBaseValue;
        mBtnMinus.interactable = false;
        mBtnPlus.interactable = !mNoStatPtCallback();
    }

    #region Button function
    public void OnClick_StatsMinus()
    {
        //Turn off minus button
        if (mTempIntBaseValue == mIntBaseValue)
            return;

        DecrementStats();
    }
    public void OnClick_StatsPlus()
    {
        //Check if still have leftover stats points
        //Turn off plus button
        if (mNoStatPtCallback())
            return;

        IncrementStats();
    }
    public void OnDown_StatsMinus(PointerEventData edata)
    {
        if (minusCoroutine != null)
            StopCoroutine(minusCoroutine);
        minusCoroutine = StartCoroutine(MinusDownCoroutine());
    }
    public void OnDown_StatsPlus(PointerEventData edata)
    {
        if (plusCoroutine != null)
            StopCoroutine(plusCoroutine);
        plusCoroutine = StartCoroutine(PlusDownCoroutine());
    }
    public void OnUp_StatsMinus(PointerEventData edata)
    {
        if (minusCoroutine != null)
            StopCoroutine(minusCoroutine);
        minusCoroutine = null;
    }
    public void OnUp_StatsPlus(PointerEventData edata)
    {
        if (plusCoroutine != null)
            StopCoroutine(plusCoroutine);
        plusCoroutine = null;
    }
    #endregion

    IEnumerator MinusDownCoroutine()
    {
        yield return new WaitForSeconds(holdDelay);
        while (mTempIntBaseValue != mIntBaseValue)
        {
            DecrementStats();
            yield return new WaitForSeconds(holdChangeDelay);
        }
    }
    IEnumerator PlusDownCoroutine()
    {
        yield return new WaitForSeconds(holdDelay);
        while (mNoStatPtCallback() == false)
        {
            IncrementStats();
            yield return new WaitForSeconds(holdChangeDelay);
        }
    }

    private void IncrementStats()
    {
        mTempIntBaseValue++;
        mBaseValue = mTempIntBaseValue.ToString();
        mValue.text = mBaseValue + mBaseBonusOperator + mBonusValue;

        //Reflect on secondary stats reflection window
        mIncCallback(mStatType, mTempIntBaseValue - mIntBaseValue);

        //Turn on minus button if not turned on
        if (mBtnMinus.interactable == false)
            mBtnMinus.interactable = true;

        //Turn off plus button if no more points
        if (mNoStatPtCallback())
            mBtnPlus.interactable = false;
    }
    private void DecrementStats()
    {
        mTempIntBaseValue--;
        mBaseValue = mTempIntBaseValue.ToString();
        mValue.text = mBaseValue + mBaseBonusOperator + mBonusValue;

        //reflect on secondary stats reflection window
        mDncCallback(mStatType, mTempIntBaseValue - mIntBaseValue);

        //Turn off minus button if hit limit
        if (mTempIntBaseValue == mIntBaseValue)
            mBtnMinus.interactable = false;

        //Turn on plus button if not turned on
        if (mBtnPlus.interactable == false)
            mBtnPlus.interactable = true;
    }
}
