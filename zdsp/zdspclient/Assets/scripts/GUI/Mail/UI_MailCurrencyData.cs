using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_MailCurrencyData : MonoBehaviour
{
    [SerializeField]
    Image mCurrencyIcon;
    [SerializeField]
    Text mCurrencyValue;

    CurrencyType mCurType;

    public string currencyValue
    {
        get { return mCurrencyValue.text; }
    }
    public CurrencyType currencyType
    {
        get { return mCurType; }
    }

    public void SetValue(CurrencyType type, int amt)
    {
        mCurType = type;
        mCurrencyIcon.sprite = ClientUtils.LoadCurrencyIcon(type);
        mCurrencyValue.text = amt.ToString();
    }

    public void SetValue(CurrencyType type, string amt)
    {
        mCurType = type;
        mCurrencyIcon.sprite = ClientUtils.LoadCurrencyIcon(type);
        mCurrencyValue.text = amt;
    }
}
