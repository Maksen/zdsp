using UnityEngine;
using UnityEngine.UI;

public class Hero_StatsData : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text valueText;

    public void Init(string statname, int value)
    {
        nameText.text = statname;
        valueText.text = value.ToString();
    }

    public void Init(string statname, string value)
    {
        nameText.text = statname;
        valueText.text = value;  
    }
}
