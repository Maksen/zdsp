using UnityEngine;
using UnityEngine.UI;

public class UI_Party_RequestBase : MonoBehaviour
{
    [SerializeField] protected Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] protected Image portraitImage;

    protected void InitBase(string name, int charlevel)
    {
        nameText.text = name;
        levelText.text = charlevel.ToString();
    }
}