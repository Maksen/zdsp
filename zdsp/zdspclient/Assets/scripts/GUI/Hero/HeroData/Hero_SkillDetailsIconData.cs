using UnityEngine;
using UnityEngine.UI;

public class Hero_SkillDetailsIconData : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Image frameImage;
	
    public void Init(Sprite sprite, bool isPassive)
    {
        iconImage.sprite = sprite;
        string framePath = isPassive ?  "UI_ZDSP_Icons/Skill/SkillFrame_Passive.tif" : "UI_ZDSP_Icons/Skill/SkillFrame_Active.tif";
        frameImage.sprite = ClientUtils.LoadIcon(framePath);
    }
}
