using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using UnityEngine;
using Zealot.Entities;
using Zealot.Repository;

public class GameLoader : MonoSingleton<GameLoader>
{
    public enum Language { En, Sc, Tc };

    public Language languageVariant = Language.Tc;

    public SplashLoadingScreen splashScreen { get; private set; }
    public SplashScreenSettings splashScreenSettings { get; private set; }

    public GameVersion gameVersion = null;
    ServerSettings serverSetting = null;
    PatchManager PatchManager = null;

    public Coroutine loadingCoroutine { get; private set; }

    enum TypeOfLoading
    {
        Init,
        Patch,
        GameDB,
        DownloadMovie,
        Asset,
        Effect,
        Scene,
        Num
    }

    public bool LoadedLevelData { get { return false; } }

    void Awake()
    {
        if (PatchManager == null)
            PatchManager = new PatchManager();

        AssetBundleManager.Initialize();

        ShowSystemInfo();

        splashScreenSettings = Resources.Load<SplashScreenSettings>("SplashScreenSettings");

        string langVariant = languageVariant.ToString().ToLower();

        //set variant
        AssetBundleManager.ActiveVariants = new string[] { langVariant };

        gameVersion = Resources.Load<GameVersion>("GameVersion");
        if (gameVersion == null)
        {
            Debug.Log("gameVersion is null at Awake");
        }

        serverSetting = (ServerSettings)Resources.Load(PhotonNetwork.serverSettingsAssetFile, typeof(ServerSettings));
        if (serverSetting == null)
        {
            Debug.Log("serversetting is null at Awake");
        }
    }

    public void Init(SplashLoadingScreen splashScreen)
    {
        this.splashScreen = splashScreen;
    }

    private void ShowSystemInfo()
    {
        Debug.Log("## System memory: " + SystemInfo.systemMemorySize.ToString() + " MB");
        Debug.Log("## Graphics API: " + SystemInfo.graphicsDeviceVersion);
    }

    // Use this for initialization
    IEnumerator Start()
    {
        yield return StartCoroutine(StartLoading(new List<TypeOfLoading>() {
            TypeOfLoading.Init,
            TypeOfLoading.Patch,
            TypeOfLoading.GameDB,
            TypeOfLoading.DownloadMovie,
            TypeOfLoading.Asset,
            TypeOfLoading.Effect,
            TypeOfLoading.Scene
        }, null));
    }


    public void StartSync()
    {
        SyncInit(new List<TypeOfLoading>() {
            TypeOfLoading.Init,
            TypeOfLoading.Patch,
            TypeOfLoading.GameDB,
            TypeOfLoading.DownloadMovie,
            TypeOfLoading.Asset,
            TypeOfLoading.Effect,
            TypeOfLoading.Scene
        }, null);
    }

    void SyncInit(List<TypeOfLoading> list_typeofLoading, Action callbackAfterLoading)
    {
        IEnumerator loading = StartLoading(list_typeofLoading, callbackAfterLoading);
        while (loading.MoveNext()) ;
    }

    /// <summary>
    /// Assets loading coroutine, updates load status and progress bar
    /// </summary>
    /// <returns></returns>
    IEnumerator StartLoading(List<TypeOfLoading> list_typeofLoading, Action callbackAfterLoading)
    {
        //reset var
        int currPhase = 0;

        int totalPhrase = list_typeofLoading.Count - 1;
        list_typeofLoading.Sort();
#if !UNITY_EDITOR
        ProfilerUtils.LogString("[Profiler] Begin: ");
#endif

        foreach (var loadingPhrase in list_typeofLoading)
        {
            switch (loadingPhrase)
            {
                case TypeOfLoading.Init:
                    ProfilerUtils.LogString("[Profiler] Init Begin: ");
                    splashScreen.SetDescriptionText(splashScreenSettings.checkUpdates);
#if !UNITY_EDITOR && USE_ASSETBUNDLE
                    splashScreen.SetDownloadProgress(0f);
                    yield return StartCoroutine(VerifyBuildNumber());
                    splashScreen.SetDownloadProgress(0.5f);
                    yield return StartCoroutine(InitializeBuild());
                    splashScreen.SetDownloadProgress(1.0f);
#endif
                    while (!Caching.ready)
                        yield return null;

                    ProfilerUtils.LogString("[Profiler] Init End: ");
                    break;
                case TypeOfLoading.Patch:
                    {
                        ProfilerUtils.LogString("[Profiler] Patch Begin ");
                        splashScreen.SetDescriptionText(splashScreenSettings.checkUpdates);
#if !UNITY_EDITOR || USE_ASSETBUNDLE
                        //destroy previous manifest before loading new one
                        AssetBundleManager.AssetBundleManifestObject = null;
                        yield return StartCoroutine(InitializeManifest());

                        splashScreen.SetDownloadProgress(0f);
                        yield return PatchManager.StartPatch();
                        AssetBundleManager.UnloadAssetBundleManifest();
                        ProfilerUtils.LogString("[Profiler] Patch End: ");
#endif
                    }
                    break;
                case TypeOfLoading.GameDB:
                    {
                        Debug.LogWarning("GameDB ");
                        yield return StartCoroutine(InitPiliQGameDB());
                        ProfilerUtils.LogString("[Profiler] GameDB End: ");
                        yield return StartCoroutine(InitLevelData());
                        ProfilerUtils.LogString("[Profiler] LevelDB End: ");
                    }
                    break;

                case TypeOfLoading.DownloadMovie:
                    {
                        Debug.LogWarning("DownloadMovie ");
#if USE_ASSETBUNDLE
                        splashScreen.SetDescriptionText(splashScreenSettings.downloadMovie);
                        splashScreen.SetDownloadProgress(0f);
                        yield return StartCoroutine(DownloadAllMovies());
                        ProfilerUtils.LogString("[Profiler] Movie End: ");
#endif
                    }
                    break;

                case TypeOfLoading.Asset:
                    {
                        Debug.LogWarning("Asset ");
#if !UNITY_EDITOR || USE_ASSETBUNDLE
                        splashScreen.SetDescriptionText(splashScreenSettings.loadingAssets);
                        splashScreen.SetDownloadProgress(0f);

                        yield return StartCoroutine(LoadAssetContainers((curr)=> { LoadingProgress(curr, 0, 50); }));
                        ProfilerUtils.LogString("[Profiler] Asset End: ");
#endif
                    }
                    break;
                case TypeOfLoading.Effect:
                    {
                        yield return StartCoroutine(EfxSystem.Instance.PreloadedEffects((curr) => { LoadingProgress(curr, 50, 100); }));
                    }
                    break;
                case TypeOfLoading.Scene:
                    {
                        Debug.LogWarning("Scene ");
                        yield return StartCoroutine(SceneLoader.Instance.LoadLevelOp("UI_LoginHierarchy", DestroySplash));
                        ProfilerUtils.LogString("[Profiler] Scene End: ");
                    }
                    break;
            }
        }

        //progress = 100;
        yield return new WaitForSeconds(0.5f);//do all the on load and look nicer on loading screen


        if (callbackAfterLoading != null)
            callbackAfterLoading.Invoke();

    }

    void LoadingProgress(float curr, int begin, int end)
    {
        float value = (float)(begin + curr * (end - begin)) / 100.0f;
        splashScreen.SetDownloadProgress(value);
    }

    void DestroySplash()
    {
        Destroy(splashScreen);
        splashScreen = null;

        splashScreenSettings = null;
    }

    protected IEnumerator InitializeBuild()
    {
        string path = gameVersion.WebServerConfigPath(serverSetting.LoginServerType);//Buddha/Android/BuildNumber

        WWW www = new WWW(path);
        yield return www;
        if (www.error == null)
        {
            int cacheSize;

            Debug.LogFormat("InitializeBuild buildpath = {0}", path);

            var deserizaliseText = www.text.Split('|');
            if (deserizaliseText.Length >= 4)
            {
                if (int.TryParse(deserizaliseText[0], out cacheSize))
                    Caching.maximumAvailableDiskSpace = ConvertMegabytesToBytes(cacheSize);
                Debug.LogFormat("InitializeBuild maximumAvailableDiskSpace {0}MB", cacheSize);

                gameVersion.SetAssetBundleRootPath(deserizaliseText[1]);

                gameVersion.SetAssetBundleNumber(deserizaliseText[2]);

                serverSetting.SetServerAddress(deserizaliseText[3]);

            }
            else
            {
                Debug.LogFormat("InitializeBuild missing configs");
            }
            www.Dispose();
            www = null;
        }
        else
        {
            Debug.LogErrorFormat("www error = {0}", www.error);
        }

        //if same continue
        yield return null;
    }

//    [Obsolete]
//    protected IEnumerator InitializeBuild2()
//    {
//        var gameVersion = Resources.Load<GameVersion>("GameVersion");
//        if (gameVersion == null)
//        {
//            Debug.Log("gameVersion is null at InitializeBuild");
//            yield break;
//        }

//        string buildNumber = "";
//        string buildpath = gameVersion.WebServerRootPath;
//        int cacheSize;

//        buildpath = buildpath + Utility.GetPlatformName() + "/gameversion";     //Buddha/Android/BuildNumber or Buddha/MyCard/Android/BuildNumber 

//        Debug.LogFormat("InitializeBuild buildpath = {0}", buildpath);

//        WWW www = new WWW(buildpath);
//        yield return www;
//        if (www.error == null)
//        {
//            var deserizaliseText = www.text.Split('|');
//            buildNumber = deserizaliseText[0];
//            if (deserizaliseText.Length >= 2 && int.TryParse(deserizaliseText[1], out cacheSize))
//            {
//                Caching.maximumAvailableDiskSpace = ConvertMegabytesToBytes(cacheSize);
//                Debug.LogFormat("InitializeBuild maximumAvailableDiskSpace {0}MB", cacheSize);
//            }
//            else
//            {
//                Debug.LogFormat("InitializeBuild maximumAvailableDiskSpace fail to retrieve");
//            }
//            www.Dispose();
//            www = null;
//            Debug.LogFormat("InitializeBuild buildNumber", buildNumber);
//        }

//        Debug.LogFormat("InitializeBuild gameVersion", gameVersion);

//        //return  0 if = , return  1 if left > right , return -1 if right > left
//        int compare = VersionCompare(buildNumber, gameVersion.GetVersion());

//        if (compare > 0)//build number > gameversion 
//        {
//            Instance.splashScreen.ShowBuildVersionErrorDialog(() =>
//            {
//#if UNITY_ANDROID
//#if MYCARD
//                Application.OpenURL("http://buddha.zealotdigital.com.tw/index.aspx");
//#else
//                Application.OpenURL("market://details?id=com.zealotdigital.buddhamobile");
//#endif
//#elif (UNITY_IPHONE || UNITY_IOS)
//                Application.OpenURL("itms-apps://itunes.apple.com/app/idYOUR_ID");
//#endif
//            });

//            // stop player from continuing until download latest version & restart game
//            while (true)
//                yield return null; 
//        }
//        else if (compare < 0)//build number < gameversion 
//        {
//            if(serverSetting.LoginServerType == LoginServerIp.Live)
//                serverSetting.SetLoginServerTypeForBuild(LoginServerIp.PublicTest);
//        }
//        //if same continue

//        yield return null;
//    }

    // Initialize the downloading url and AssetBundleManifest object.
    protected IEnumerator InitializeManifest()
    {
#if USE_ASSETBUNDLE
        string path = gameVersion.AssetBundlePath ;
#endif

#if USE_ASSETBUNDLE
        //to test downloading
        AssetBundleManager.SetSourceAssetBundleURL(path);
#else
        //Use the following code if AssetBundles are embedded in the project for example via StreamingAssets folder etc:
        //AssetBundleManager.SetSourceAssetBundleURL(Application.dataPath + "/");
        //Or customize the URL based on your deployment or configuration
#if UNITY_EDITOR
        AssetBundleManager.SetSourceAssetBundleDirectory("/AssetBundles/Android/Android/");
#else
        AssetBundleManager.SetSourceAssetBundleDirectory("/AssetBundles/");
#endif
#endif

        bool downloadedManifest = false;
        while (!downloadedManifest)
        {
            // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
            var request = AssetBundleManager.LoadManifest();

            if (request != null)
            {
                yield return StartCoroutine(request);

                if (request.HasDownloadError())
                {
                    bool waitForResume = true;

                    while (waitForResume)
                    {
                        var errorCode = PatchManager.GetErrorCode(request.GetError());
                        GameLoader.Instance.splashScreen.ShowResumePatchDialog(true, (int)errorCode, () => waitForResume = false);
                        yield return null;
                    }
                }
                else
                    downloadedManifest = true;
            }
        }
    }

    protected IEnumerator InitPiliQGameDB()
    {
        string gameText = "";
        // Load asset from assetBundle.
        ProfilerUtils.LogString("[Profiler] InitPiliQGameDB Begin:");
        string loadpath = AssetLoader.GetLoadString("GameData_GameRepo", "gamedata.json");
        yield return AssetLoader.Instance.LoadAsyncCoroutine<TextAsset>(loadpath,
            (TextAsset gameDB) =>
            {
                ProfilerUtils.LogString("[Profiler] Gamedata textAsset:");
                if (!string.IsNullOrEmpty(gameDB.text))
                {
                    gameText = gameDB.text;
                }
            }, false);
        GameRepo.SetItemFactory(new ClientItemFactory());
        GameRepo.InitClient(gameText);

        EfxSystem.Instance.InitFromGameDB();
        ProfilerUtils.LogString("[Profiler] EfxSystem:");
    }

    void KopioErrorHandler(Exception exception)
    {
        if (exception != null)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog("Load GameDB Error", "!!!Mismatched GameDB Hash.Update jsoncontracts and gamedata again.", "OK");
#endif
            throw exception;
        }
    }

    private IEnumerator InitLevelData()
    {
        yield return AssetLoader.Instance.LoadAsyncContainer("GameData_Levels.asset", (BaseAssetContainer assetContainer) =>
        {

            var list_level = assetContainer.GetExportedAssets();
            Dictionary<string, string> levelData = new Dictionary<string, string>();
            for (int i = 0; i < list_level.Count; i++)
            {
                var levelname = list_level[i].asset.name;
                var levelTextAsset = list_level[i].asset as TextAsset;
                levelData.Add(levelname, levelTextAsset.text);
            }

            LevelReader.InitClient(levelData);
        }, false);
    }

    private IEnumerator LoadAssetContainers(Action<float> loadingProgress)
    {
        long memBeforeLoad = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();

        var manifest = Resources.Load<AssetContainerManifest>("AssetContainerManifest");
        float count = 0f;
        float total = manifest.PreLoadable.Count;
        foreach (string containerName in manifest.PreLoadable)
        {
            var containerRequest = AssetBundleManager.LoadAssetAsync(string.Concat("assetcontainers/", containerName.ToLower()), containerName, typeof(BaseAssetContainer));
            if (containerRequest != null)
            {
                yield return containerRequest;

                var assetContainer = containerRequest.GetAsset<BaseAssetContainer>();

                AssetManager.OnLoadedAssetContainerFromBundle(containerName, assetContainer);
            }
            else {
                yield return null;
            }

            ProfilerUtils.LogString("[Profiler] Asset [" + containerName + "] End: ");
            count++;
            loadingProgress(count / total);
        }

        long memAfterLoad = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
        long memDiff = memAfterLoad - memBeforeLoad;

        Debug.LogFormat("## AssetContainers size [{0}] -> [{1}] = [{2}]", memBeforeLoad, memAfterLoad, memDiff);
    }

    long ConvertMegabytesToBytes(long megabytes)
    {
        return (megabytes * 1024) * 1024;
    }
    
    int VersionCompare(string left, string right)
    {
        //return  0 if = , return  1 if left>right , return -1 if right>left

        if (string.IsNullOrEmpty(left) && string.IsNullOrEmpty(right))
            return 0;

        if (string.IsNullOrEmpty(left))
            return -1;

        if (string.IsNullOrEmpty(right))
            return 1;

        var leftarr = left.Split('.');
        var rightarr = right.Split('.');

        int tmp = 0;
        int leftTotal = 0, rightTotal = 0;

        if (leftarr.Length >= 1 && int.TryParse(leftarr[0], out tmp))
        {
            leftTotal += tmp * 100 * 100;
        }
        if (leftarr.Length >= 2 && int.TryParse(leftarr[1], out tmp))
        {
            leftTotal += tmp * 100;
        }
        if (leftarr.Length >= 3 && int.TryParse(leftarr[2], out tmp))
        {
            leftTotal += tmp;
        }


        if (rightarr.Length >= 1 && int.TryParse(rightarr[0], out tmp))
        {
            rightTotal += tmp * 100 * 100;
        }
        if (rightarr.Length >= 2 && int.TryParse(rightarr[1], out tmp))
        {
            rightTotal += tmp * 100;
        }
        if (rightarr.Length >= 3 && int.TryParse(rightarr[2], out tmp))
        {
            rightTotal += tmp;
        }


        //return  0 if =
        //return  1 if left>right
        //return -1 if right>left]
        if (leftTotal > rightTotal)
            return 1;
        if (rightTotal > leftTotal)
            return -1;
        else
            return 0;
    }

    protected IEnumerator VerifyBuildNumber()
    {
        string path = gameVersion.WebServerVersionPath;     //PiliQ/Android/gameversion 

        WWW www = new WWW(path);
        yield return www;
        if (www.error == null)
        {
            Debug.LogFormat("path = {0}", path);
            string buildNumber = "";

            var deserizaliseText = www.text.Split('|');
            buildNumber = deserizaliseText[0];
            string downloadSite = deserizaliseText[1];
            Debug.LogFormat("InitializeBuild gameVersion", gameVersion);

            //return  0 if = , return  1 if left > right , return -1 if right > left
            int compare = VersionCompare(buildNumber, gameVersion.GetVersion());

            if (compare > 0 || serverSetting.LoginServerType == LoginServerIp.Private)//build number > gameversion 
            {
                Instance.splashScreen.ShowBuildVersionErrorDialog(() =>
                {
                    Application.OpenURL(downloadSite);
                });

                // stop player from continuing until download latest version & restart game
                while (true)
                    yield return null;
            }
            else if (compare < 0)//build number < gameversion 
            {
                if (serverSetting.LoginServerType == LoginServerIp.Live || serverSetting.LoginServerType == LoginServerIp.Staging)
                    serverSetting.SetLoginServerTypeForBuild(LoginServerIp.PublicTest);
            }
            

            www.Dispose();
            www = null;
        }
        else
        {
            Debug.LogErrorFormat("www error = {0}", www.error);
        }
        //if same continue
        yield return null;
    }

#region Movie Handling 
    public const string MovieOpening = "open.mp4";
    public const string MovieComics = "comics.mp4";
    //public const string MovieGuild = "";

    private IEnumerator DownloadAllMovies()
    {
        GameLoader.Instance.splashScreen.SetDownloadProgress(0);
        yield return DownloadMovie(MovieOpening);
        GameLoader.Instance.splashScreen.SetDownloadProgress(0.5f);
        yield return DownloadMovie(MovieComics);
        GameLoader.Instance.splashScreen.SetDownloadProgress(1.0f);
        //yield return DownloadMovie(MovieGuild);
        //GameLoader.Instance.splashScreen.SetDownloadProgress(1.00f);
    }

    private IEnumerator DownloadMovie(string moviefile)
    {
        string moviePath = Application.persistentDataPath + "/" + moviefile;

        if (!System.IO.File.Exists(moviePath))
        {
            string url = gameVersion.WebServerRootPath + "Movies/" + moviefile;
            Debug.Log(string.Format("Movie URL: {0}", url));

            WWW www = new WWW(url);
            while (!www.isDone)
            {
                yield return null;
            }

            Debug.Log(string.Format("Downloaded movie: {0}", moviefile));
            System.IO.File.WriteAllBytes(moviePath, www.bytes);
        }
        yield return null;
    }

#endregion
}
