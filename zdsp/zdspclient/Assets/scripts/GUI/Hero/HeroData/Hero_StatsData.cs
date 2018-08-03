using UnityEngine;
using UnityEngine.UI;

public class Hero_StatsData : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text valueText;
    [SerializeField] GameObject colonObj;

    public void Init(string statname, float value)
    {
        colonObj.SetActive(true);
        valueText.gameObject.SetActive(true);
        nameText.text = statname;
        valueText.text = value.ToString();
    }

    public void Init(string statname, string value)
    {
        colonObj.SetActive(true);
        valueText.gameObject.SetActive(true);
        nameText.text = statname;
        valueText.text = value;
    }

    public void Init(string text)
    {
        colonObj.SetActive(false);
        valueText.gameObject.SetActive(false);
        nameText.text = text;
    }
}
