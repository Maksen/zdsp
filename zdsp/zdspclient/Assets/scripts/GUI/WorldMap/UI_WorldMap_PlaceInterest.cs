using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_WorldMap_PlaceInterest : MonoBehaviour
{
    [SerializeField]
    Image mIcon;
    [SerializeField]
    Text mName;

    public Sprite IconSprite
    {
        set { mIcon.sprite = value; }
    }
    public string Name
    {
        set { mName.text = value; }
    }

    public void OnClick()
    {

    }
}
