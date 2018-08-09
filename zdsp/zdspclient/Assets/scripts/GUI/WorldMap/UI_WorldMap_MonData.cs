using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_WorldMap_MonData : MonoBehaviour
{
    [SerializeField]
    Image mFrame;
    [SerializeField]
    Image mIcon;

    public Sprite Frame
    {
        set { mFrame.sprite = value; }
    }
    public Sprite IconSprite
    {
        set { mIcon.sprite = value; }
    }

    public void OnClick()
    {

    }
}
