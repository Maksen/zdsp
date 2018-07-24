using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

public class UI_CharacterInfo_Tooltip : MonoBehaviour
{
    [SerializeField]
    Text mName;
    [SerializeField]
    Text mValue;

    string mVal = "";
    string mPercent = "";

    public string Name
    {
        set { mName.text = value; }
    }
    public string Value
    {
        set
        {
            mVal = value;
            mValue.text = mVal + mPercent;
        }
    }
    public string Percent
    {
        set
        {
            mPercent = " (" + value + "%)";
            mValue.text = mVal + mPercent;
        }
    }
}
