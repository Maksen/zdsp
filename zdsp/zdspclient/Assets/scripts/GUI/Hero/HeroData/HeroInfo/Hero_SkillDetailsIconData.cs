using UnityEngine;
using UnityEngine.UI;

public class Hero_SkillDetailsIconData : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Image frameImage;
    [SerializeField] Sprite[] frameSprite;  // 0 - active, 1 - passive

    public void Init(Sprite sprite, bool isPassive)
    {
        iconImage.sprite = sprite;
        frameImage.sprite = isPassive ? frameSprite[1] : frameSprite[0];
    }
}