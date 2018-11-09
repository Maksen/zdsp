using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;

public class Achievement_SEText : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text valueText;

    private Color origColor;

    private void Awake()
    {
        origColor = nameText.color;
    }

    public void SetSEText(SideEffectJson sejson, bool isPositive, bool isActive)
    {
        nameText.text = SideEffectUtils.GetEffectTypeLocalizedName(sejson.effecttype);
        string sign = isPositive ? "+" : "-";
        valueText.text = sign + (sejson.isrelative ? string.Format("{0}%", sejson.max) : sejson.max.ToString());
        nameText.color = valueText.color = isActive ? origColor : ClientUtils.ColorGray;
    }
}