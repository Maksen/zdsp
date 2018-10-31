using UnityEngine;

public class GameLoadingScreen : MonoBehaviour
{
    [SerializeField]
    UI_ProgressBarC progressBar = null;
    [SerializeField]
    Transform contentWallpaper = null;

    //Dictionary<int, GameObject> wallpaperDict = new Dictionary<int, GameObject>();
    //GameObject currentWallpaper = null;
    //int currentJob = 0;
    //float currTimer = 0;

    void Awake()
    {
        OnAwake();
    }

    public void OnAwake()
    {
        if (UIManager.LoadingScreen == null)
        {
            UIManager.RegisterLoadingScreen(this);
            DontDestroyOnLoad(gameObject);
        }
    }

    void OnDisable()
    {
        CleanUp();
    }

    void OnDestroy()
    {
        CleanUp();
    }

    public void DestroyLoadingScreen()
    {
        Destroy(gameObject);
    }

    void CleanUp()
    {
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
}
