using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class UI_CharacterInfo_StatsReview : MonoBehaviour
{
    [SerializeField]
    Text mTextObj;

    string mName;
    string mOP = "+";
    string mVal;

    public string Text
    {
        get { return mTextObj.text; }
    }
    public string Name
    {
        get { return mName; }
        set
        {
            mName = value;
            mTextObj.text = mName + mOP + mVal;
        }
    }
    public string OP
    {
        get { return mOP; }
        set
        {
            mOP = value;
            mTextObj.text = mName + mOP + mVal;
        }
    }
    public string Value
    {
        get { return mVal; }
        set
        {
            mVal = value;
            mTextObj.text = mName + mOP + mVal;
        }
    }
}
