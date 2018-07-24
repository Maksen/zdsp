using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class UI_CharacterInfo_BasicStats : MonoBehaviour
{
    [SerializeField]
    Text _mStatsName;
    [SerializeField]
    Text _mStatValue;

    string _mStatValuePrefix = "";
    string _mStatValuePostfix = "";

    [SerializeField]
    Text _mTooltipName;
    [SerializeField]
    Text _mTooltipValue;

    string _mTooltipVal;
    string _mTooltipPercent = " "; //used to show stats percentage at the end

    public string Name
    {
        get { return _mStatsName.text; }
        set { _mStatsName.text = value; }
    }
    public int Val
    {
        get
        {
            _mStatValue.text = _mStatValue.text.Replace(_mStatValuePostfix, "");
            _mStatValue.text = _mStatValue.text.Replace(_mStatValuePrefix, "");
            int res;
            if (int.TryParse(_mStatValue.text, out res))
            {
                Debug.Log("UI_CharacterInfo_BasicStats StatValue get property: parse failed.");
                return -1;
            }
            return res;
        }
        set
        {
            value = Mathf.Max(0, value);
            _mStatValue.text = _mStatValuePrefix + value.ToString() + _mStatValuePostfix;
        }
    }
    public string ValPrefix
    {
        get { return _mStatValuePrefix; }
        set
        {
            if (_mStatValuePrefix.Length != 0)
                _mStatValue.text = _mStatValue.text.Replace(_mStatValuePrefix, value);
            _mStatValuePrefix = value;
        }
    }
    public string ValPostfix
    {
        get { return _mStatValuePostfix; }
        set
        {
            if (_mStatValuePostfix.Length != 0)
                _mStatValue.text = _mStatValue.text.Replace(_mStatValuePostfix, "");
            _mStatValuePostfix = value;
            _mStatValue.text += _mStatValuePostfix;
        }
    }
    public string TTName
    {
        set { _mTooltipName.text = value; }
    }
    public string TTVal
    {
        set
        {
            _mTooltipVal = value;
            _mTooltipValue.text = _mTooltipVal + _mTooltipPercent;
        }
    }
    public string TTPercent
    {
        set
        {
            _mTooltipPercent = " (" + value + "%)";
            _mTooltipValue.text = _mTooltipVal + _mTooltipPercent;
        }
    }
    public string CombineName
    {
        get { return _mStatsName.text; }
        set
        {
            _mStatsName.text = value;
            _mTooltipName.text = value;
        }
    }
    public int CombineVal
    {
        set
        {
            Val = value;
            TTVal = value.ToString();
        }
    }
    public int CombineValPercent
    {
        set
        {
            Val = value;
            TTVal = "";
            TTPercent = value.ToString();
        }
    }
}
