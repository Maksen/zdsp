using UnityEngine;
using UnityEngine.UI;

public class Achievement_AbilityData : MonoBehaviour
{
    [SerializeField] Text leftNameText;
    [SerializeField] Text leftValueText;
    [SerializeField] GameObject rightColumnObj;
    [SerializeField] Text rightNameText;
    [SerializeField] Text rightValueText;

    public void SetLeftData(string name, float value, bool showPercent)
    {
        leftNameText.text = name;
        leftValueText.text = "+" + (showPercent ? string.Format("{0}%", value) : value.ToString());
        rightColumnObj.SetActive(false);
    }

    public void SetRightData(string name, float value, bool showPercent)
    {
        rightNameText.text = name;
        rightValueText.text = "+" + (showPercent ? string.Format("{0}%", value) : value.ToString());
        rightColumnObj.SetActive(true);
    }
}