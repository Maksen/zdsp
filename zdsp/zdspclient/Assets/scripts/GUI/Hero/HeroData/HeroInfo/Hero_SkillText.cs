using UnityEngine;
using UnityEngine.UI;

public class Hero_SkillText : MonoBehaviour
{
    [SerializeField] Text valueText;

    public void SetText(string text)
    {
        valueText.text = text;
    }
}