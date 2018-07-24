using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;

public class GameLoadingScreen : MonoBehaviour
{
    [SerializeField]
    UI_ProgressBarC progressBar = null;
    [SerializeField]
    Transform contentSpineWallpaper = null;
    [SerializeField]
    bool activeOnStartup = false;

    Dictionary<int, GameObject> wallpaperDict = new Dictionary<int, GameObject>();
    GameObject currentWallpaper = null;
    int currentJob = 0;
    float currTimer = 0;

	void Awake()
    {
        UIManager.RegisterLoadingScreen(this);
        DontDestroyOnLoad(this.gameObject);
        gameObject.SetActive(activeOnStartup);
    }

    void OnEnable()
    {
        currentWallpaper = null;
        //currentJob = GameUtils.RandomInt(1, 4);
        //ShowWallpaperByJob(currentJob);
        currTimer = 10.5f;
    }

    void OnDisable()
    {
        CleanUp();
    }

    void OnDestroy()
    {
        CleanUp();
    }

    void CleanUp()
    {
        foreach (Transform child in contentSpineWallpaper)
            Destroy(child.gameObject);

        wallpaperDict.Clear();
    }

    //void Update()
    //{
    //    if (currTimer >= 0)
    //        currTimer -= Time.deltaTime;
    //    else
    //    {
    //        currentJob = (currentJob+1 > 4) ? 1 : currentJob+1;
    //        ShowWallpaperByJob(currentJob); 
    //        currTimer = 10.0f;
    //    }
    //}

    void ShowWallpaperByJob(int jobType)
    {
        if (currentWallpaper != null)
            currentWallpaper.SetActive(false);

        if (!wallpaperDict.ContainsKey(jobType))
        {
            GameObject prefab = AssetLoader.Instance.Load<GameObject>(GetSplinePathByJob((JobType)jobType));
            GameObject wallpaper = Instantiate(prefab);
            wallpaper.transform.SetParent(contentSpineWallpaper, false);
            wallpaperDict.Add(jobType, wallpaper);
        }
        currentWallpaper = wallpaperDict[jobType];
        currentWallpaper.SetActive(true);
    }

    string GetSplinePathByJob(JobType jobType)
    {
        switch (jobType)
        {
            case JobType.Warrior:  return "UI_PiLiQ_AncySpine/LoadingScreen/Spine_LoadingScreen_kn/Spine_LoadingScreen_kn.prefab";
            case JobType.Soldier:  return "UI_PiLiQ_AncySpine/LoadingScreen/Spine_LoadingScreen_sw/Spine_LoadingScreen_sw.prefab";
            case JobType.Tactician:  return "UI_PiLiQ_AncySpine/LoadingScreen/Spine_LoadingScreen_sp/Spine_LoadingScreen_sp.prefab";
            case JobType.Killer: return "UI_PiLiQ_AncySpine/LoadingScreen/Spine_LoadingScreen_ha/Spine_LoadingScreen_ha.prefab";
            default: return "";
        }
    }

    public void ShowLoadingScreen(bool val)
    {
        if (progressBar != null)
            progressBar.Value = 0;
        gameObject.SetActive(val);
    }

    public void SetLoadingScreenProgress(float progress)
    {
        if(progressBar!=null)
            progressBar.Value = (long)(progress * 100);
    }

    public void DestroyLoadingScreen()
    {
        Destroy(gameObject);
    }
}
