using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zealot.Repository;

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

    public void Init(WorldMapCountryMonster wmcm)
    {
        int id = wmcm.archetype;
        //mIcon.sprite = ClientUtils.LoadIcon();
        //mFrame.sprite = ;
    }

    public void OnClick()
    {
        //Auto-pilot player to monsterspawner and start bot
    }
}
