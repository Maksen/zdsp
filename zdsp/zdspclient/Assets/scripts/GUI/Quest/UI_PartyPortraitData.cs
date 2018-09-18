using UnityEngine;
using UnityEngine.UI;

public class UI_PartyPortraitData : MonoBehaviour
{
    [SerializeField]
    Image MemberIcon;

    public void Init(Sprite sprite)
    {
        MemberIcon.sprite = sprite;
    }
}
