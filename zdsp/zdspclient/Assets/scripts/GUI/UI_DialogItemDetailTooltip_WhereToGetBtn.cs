using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class UI_DialogItemDetailTooltip_WhereToGetBtn : MonoBehaviour
{
    [SerializeField]
    Image mImg;
    [SerializeField]
    Text mName;
    [SerializeField]
    Text mLevelName;
    [SerializeField]
    Text mMapName;
    [SerializeField]
    Button mBtn;

    public Sprite Portrait
    {
        set { mImg.sprite = value; }
    }
    public string Name
    {
        set { mName.text = value; }
        get { return mName.text; }
    }
    public string LevelName
    {
        set
        {
            mLevelName.text = value;
            mLevelName.gameObject.SetActive(!string.IsNullOrEmpty(mLevelName.text));
        }
        get { return mLevelName.text; }
    }
    public string MapName
    {
        set
        {
            mMapName.text = value;
            mMapName.gameObject.SetActive(!string.IsNullOrEmpty(mMapName.text));
        }
        get { return mMapName.text; }
    }
    public bool HideButton
    {
        get { return mBtn.gameObject.GetActive(); }
        set { mBtn.gameObject.SetActive(!value); }
    }
    public UnityAction ButtonAction
    {
        set
        {
            HideButton = false;
            mBtn.onClick.RemoveAllListeners();
            mBtn.onClick.AddListener(value);
        }
    }

    public void Clear()
    {
        Portrait = null;
        Name = "";
        LevelName = "";
        MapName = "";

        gameObject.SetActive(false);
    }
}
