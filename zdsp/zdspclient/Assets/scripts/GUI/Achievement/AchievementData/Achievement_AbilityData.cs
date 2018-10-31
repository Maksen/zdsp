using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class Achievement_AbilityData : MonoBehaviour
{
    [SerializeField] Text leftNameText;
    [SerializeField] Text leftValueText;
    [SerializeField] GameObject rightColumnObj;
    [SerializeField] Text rightNameText;
    [SerializeField] Text rightValueText;
    [SerializeField] GameObject lineSeparatorObj;

    public void SetLeftData(EffectType effectType, float value, bool showPercent)
    {
        leftNameText.text = SideEffectUtils.GetEffectTypeLocalizedName(effectType);
        leftValueText.text = "+" + (showPercent ? string.Format("{0}%", value) : value.ToString());
        rightColumnObj.SetActive(false);
    }

    public void SetRightData(EffectType effectType, float value, bool showPercent)
    {
        rightNameText.text = SideEffectUtils.GetEffectTypeLocalizedName(effectType);
        rightValueText.text = "+" + (showPercent ? string.Format("{0}%", value) : value.ToString());
        rightColumnObj.SetActive(true);
    }

    public void SetLineSeparator(bool value)
    {
        lineSeparatorObj.SetActive(value);
    }
}