using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using Zealot.Repository;

public class UI_WorldMap_PlaceInterest : MonoBehaviour
{
    [SerializeField]
    Image mIcon;
    [SerializeField]
    Text mName;

    UnityAction<string> selectCallback = null;
    string mPlaceInterestName;

    public Sprite IconSprite
    {
        set { mIcon.sprite = value; }
    }
    public string Name
    {
        set { mName.text = value; }
    }

    public void Init(WorldMapCountryPlaceInterest wmcpi, UnityAction<string> _selectCallback)
    {
        mName.text = wmcpi.name;
        mPlaceInterestName = LevelRepo.GetInfoById(wmcpi.levelID).unityscene;
        selectCallback = _selectCallback;
    }

    public void OnClick()
    {
        //Tell UI_WorldMapCountry that this place/interest is selected
        selectCallback(mPlaceInterestName);
    }
}
