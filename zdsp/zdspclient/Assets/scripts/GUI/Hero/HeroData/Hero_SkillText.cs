using UnityEngine;
using UnityEngine.UI;

public class Hero_SkillText : MonoBehaviour
{
    [SerializeField] Text skillText;
	
    public void SetText(string text)
    {
        skillText.text = text;
    }
}
