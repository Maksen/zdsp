using UnityEngine;
using UnityEngine.UI;

public class Hero_Text : MonoBehaviour
{
    [SerializeField] Text valueText;

    public void SetText(string text)
    {
        valueText.text = text;
    }
}