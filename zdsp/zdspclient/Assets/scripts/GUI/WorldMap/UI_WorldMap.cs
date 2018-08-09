using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_WorldMap : MonoBehaviour
{
    [SerializeField]
    GameObject mCountryDataPrefab;
    [SerializeField]
    GameObject mCountryParent;
    [SerializeField]
    Image mMapAreaViewImg;

    public void Awake()
    {

    }

    public void Init()
    {
        
    }

    private UI_WorldMap_Country CreateCountryData()
    {
        GameObject obj = Instantiate(mCountryDataPrefab, Vector3.zero, Quaternion.identity);
        obj.transform.SetParent(mCountryParent.transform, false);

        return obj.GetComponent<UI_WorldMap_Country>();
    }
}
