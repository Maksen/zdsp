using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using Zealot.Repository;

public class UI_WorldMap : MonoBehaviour
{
    [SerializeField]
    GameObject mCountryDataPrefab;
    [SerializeField]
    GameObject mCountryParent;
    [SerializeField]
    Image mMapAreaViewImg;
    [SerializeField]
    GameObject mPlaceInterestMarkerPrefab;
    [SerializeField]
    GameObject mPlaceInterestMarkerParent;
    [SerializeField]
    ToggleGroup mCountryHighlightToggleGrp;

    [Header("Country Selected GameObject")]
    #region Country Selected GameObject
    [SerializeField]
    List<GameObject> mBigCountryList = new List<GameObject>();
    [SerializeField]
    UI_MoveToObj mMoveToObjClass;
    [SerializeField]
    List<GameObject> mCamPosList = new List<GameObject>();
    List<GameObject> mInterestMarkerList = new List<GameObject>();
    #endregion

    public void Awake()
    {
        List<WorldMapCountry> worldMapLst = MapRepo.mWorldMapLst;

        //Create all the country
        for (int i = 0; i < worldMapLst.Count; ++i)
        {
            GameObject obj = Instantiate(mCountryDataPrefab, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(mCountryParent.transform, false);

            UI_WorldMap_Country wmcc = obj.GetComponent<UI_WorldMap_Country>();
            GameObject bigCountry = mBigCountryList[i];
            GameObject camPos = mCamPosList[i + 1]; //mCamPosList[0] is zoom-out cam position
            wmcc.Init(worldMapLst[i], ()=> {
                ZoomToCountry(bigCountry, camPos);    
            },
            (sprite)=>
            {
                SetAreaPreview(sprite);
            },
            (lst)=>
            {
                ShowInterestMarker(lst);
            },
            (index)=>
            {
                HighlightInterestMarker(index);
            });

            Toggle tg = obj.GetComponent<Toggle>();
            tg.group = mCountryHighlightToggleGrp;
        }

        //Create all interest map marker
        List<WorldMapInterestMarkerPos> markerPostLst = MapRepo.mWorldMapInterestMarkLst;
        for (int i = 0; i < markerPostLst.Count; ++i)
        {
            Vector3 pos = new Vector3(markerPostLst[i].x, markerPostLst[i].y, markerPostLst[i].z);
            GameObject obj = Instantiate(mPlaceInterestMarkerPrefab, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(mPlaceInterestMarkerParent.transform, false);
            obj.transform.localPosition = pos;
            obj.SetActive(false);

            mInterestMarkerList.Add(obj);
        }
    }

    private void ZoomToCountry(GameObject selectedCountry, GameObject camPos)
    {
        if (selectedCountry == null || camPos == null)
            return;

        for (int i = 0; i < mBigCountryList.Count; ++i)
        {
            mBigCountryList[i].SetActive(false);
        }

        selectedCountry.SetActive(true);
        mMoveToObjClass.MoveTo(camPos.transform);

        //Turn on visibility of country's map marker

    }

    private void SetAreaPreview(Sprite areaSprite)
    {
        mMapAreaViewImg.sprite = areaSprite;
    }

    private void ShowInterestMarker(List<int> interestIDLst)
    {
        //Hide all place interest map markers
        mInterestMarkerList.ForEach((obj)=>
        {
            obj.SetActive(false);
        });

        //Show that country's place interest map markers
        for (int i = 0; i < interestIDLst.Count; ++i)
        {
            mInterestMarkerList[interestIDLst[i]-1].SetActive(true);
        }
    }

    private void HighlightInterestMarker(int index)
    {
        //Index out of bounds
        ///Kopio index start from 1
        if (index <= 0 || index > mInterestMarkerList.Count)
            return;

        //Toggle off particle system
        mInterestMarkerList.ForEach((obj) =>
        {
            ParticleSystem ps1 = obj.GetComponentInChildren<ParticleSystem>(true);
            if (ps1 == null)
            {
                Debug.LogError("UI_WorldMap.ShowInterestMarker: Walaoeh!! Cannot find particle system in prefab");
                return;
            }
            ps1.gameObject.SetActive(false);
        });

        //Toggle particle system
        ParticleSystem ps = mInterestMarkerList[index-1].GetComponentInChildren<ParticleSystem>(true);
        if (ps == null)
        {
            Debug.LogError("UI_WorldMap.ShowInterestMarker: Walaoeh!! Cannot find particle system in prefab");
            return;
        }
        ps.gameObject.SetActive(true);
    }
}
