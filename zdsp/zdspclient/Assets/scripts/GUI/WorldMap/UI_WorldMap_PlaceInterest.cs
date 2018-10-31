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
    UnityAction<Sprite> spriteCallback = null;
    UnityAction<int> placeinterestSelectCallback = null;
    string mPlaceInterestName;
    Sprite mAreaSprite = null;
    int mPlaceInterestIndex; //Index to PlaceInterest data in kopio

    public Sprite IconSprite
    {
        set { mIcon.sprite = value; }
    }
    public string Name
    {
        set { mName.text = value; }
    }
    public int PIIndex
    {
        get { return mPlaceInterestIndex; }
    }

    public void Init(WorldMapCountryPlaceInterest wmcpi, UnityAction<string> _selectCallback, UnityAction<Sprite> _spriteCallback, UnityAction<int> _placeinterestSelectCallback)
    {
        mName.text = wmcpi.name;
        mPlaceInterestName = LevelRepo.GetInfoById(wmcpi.levelID).unityscene;
        mAreaSprite = ClientUtils.LoadIcon(wmcpi.iconPath);
        mPlaceInterestIndex = wmcpi.interestID;
        selectCallback = _selectCallback;
        spriteCallback = _spriteCallback;
        placeinterestSelectCallback = _placeinterestSelectCallback;
    }

    public void OnClick()
    {
        //Tell UI_WorldMapCountry that this place/interest is selected
        selectCallback(mPlaceInterestName);
        spriteCallback(mAreaSprite);
        placeinterestSelectCallback(mPlaceInterestIndex);
    }
}
