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
    ToggleGroup mCountryHighlightToggleGrp;

    [Header("Country Selected GameObject")]
    #region Country Selected GameObject
    [SerializeField]
    List<GameObject> mBigCountryList = new List<GameObject>();
    [SerializeField]
    UI_MoveToObj mMoveToObjClass;
    [SerializeField]
    List<GameObject> mCamPosList = new List<GameObject>();
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
            });

            Toggle tg = obj.GetComponent<Toggle>();
            tg.group = mCountryHighlightToggleGrp;
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
    }
}
