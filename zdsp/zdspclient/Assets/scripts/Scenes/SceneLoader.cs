using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Zealot.Repository;
using AssetBundles;

public class SceneLoader : MonoSingleton<SceneLoader>
{
    private bool isCombatHierarchyLoaded;

    public void LoadLevel(string levelname, UnityAction afterLoad = null)
    {
        Debug.LogFormat("LoadLevel [{0}]", levelname);
        StartCoroutine(LoadLevelOp(levelname, afterLoad));
    }

    public IEnumerator LoadLevelOp(string levelname, UnityAction afterLoad)
    {
        string levelLowercase = levelname.ToLower();
        AssetManager.PreLevelLoad(levelname);
        string[] defaultLevels = { "ui_loginhierarchy", "lobby", "ui_createchar" }; // Here all small case
        bool isCombatHierarchy = string.Compare(levelname, "UI_CombatHierarchy", true) == 0;
        LoadSceneMode mode = isCombatHierarchy ? LoadSceneMode.Additive : LoadSceneMode.Single;

#if UNITY_EDITOR && !USE_ASSETBUNDLE
        var request = SceneManager.LoadSceneAsync(levelname, mode);
        while (!request.isDone)
        {
            if (UIManager.LoadingScreen != null)
            {
                float progress = request.progress;
                if (Array.IndexOf(defaultLevels, levelLowercase) < 0 && !isCombatHierarchyLoaded)
                    progress *= 0.5f;
                if (isCombatHierarchy)
                    progress = 0.5f + progress;
                UIManager.LoadingScreen.SetLoadingScreenProgress(progress);
            }

            yield return null;
        }
#else
        string bundleName = "levels/" + levelLowercase;
        AssetBundleLoadLevelOperation request = AssetBundleManager.LoadLevelAsync(bundleName, levelname, mode == LoadSceneMode.Additive);
        if (request == null)
            yield break;

        StartCoroutine(request);
        while (!request.IsDone())
        {
            if (UIManager.LoadingScreen != null)
            {
                float progress = request.Progress;
                if (Array.IndexOf(defaultLevels, levelLowercase) < 0 && !isCombatHierarchyLoaded)
                    progress *= 0.5f;
                if (isCombatHierarchy)
                    progress = 0.5f + progress;
                UIManager.LoadingScreen.SetLoadingScreenProgress(progress);
            }

            yield return null;
        }

        yield return null;
        AssetBundleManager.UnloadAssetBundle(bundleName);
#endif
        if (mode == LoadSceneMode.Additive)
        {
            SceneManager.MergeScenes(SceneManager.GetSceneByName(levelname), SceneManager.GetActiveScene());
        }

        OnLevelLoaded(levelname);

        if (isCombatHierarchy || isCombatHierarchyLoaded)
        {
            if (UIManager.LoadingScreen != null)
                UIManager.LoadingScreen.SetLoadingScreenProgress(1.0f);
        }
        else if (Array.IndexOf(defaultLevels, levelLowercase) < 0 && !isCombatHierarchyLoaded)
        {
            LoadUICombat();
        }

        if (afterLoad != null)
            afterLoad.Invoke();

        Debug.LogFormat("## LoadLevel [{0}] mem [{1}]", levelname, UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong());
    }

    void OnLevelLoaded(string levelName)
    {
        UIManager.StopHourglass();

        switch (levelName.ToLower())
        {
            case "lobby":
            case "ui_createchar":
                //ClientUtils.PlayMusic(GameSettings.MusicEnabled);
                PhotonNetwork.networkingPeer.NewSceneLoaded();
                break;

            case "ui_loginhierarchy":
                PhotonNetwork.networkingPeer.NewSceneLoaded();
                //ClientUtils.PlayMusic(GameSettings.MusicEnabled);
                break;

            case "ui_combathierarchy":
                {
                    string loadedScene = "";
                    for (int i = 0; i < SceneManager.sceneCount; ++i)
                    {
                        string sceneName = SceneManager.GetSceneAt(i).name;
                        if (!sceneName.Equals("ui_combathierarchy", StringComparison.OrdinalIgnoreCase) &&
                            !sceneName.Equals("ui_loginhierarchy", StringComparison.OrdinalIgnoreCase))
                        {
                            loadedScene = sceneName;
                            break;
                        }
                    }
                    SetupLevel(loadedScene);
                    PhotonNetwork.networkingPeer.NewSceneLoaded();
                    UIManager.UIHierarchy.SetupEventSystem();
                    isCombatHierarchyLoaded = true;
                }
                break;

            default:
                {
                    GameInfo.gClientState = PiliClientState.Combat;

                    var CombatmainPrefab = AssetLoader.Instance.Load<GameObject>(AssetLoader.GetLoadString("Prefabs_preloadcontainer", "CombatMain.prefab"));
                    GameObject main = Instantiate(CombatmainPrefab);

                    var CombatcameraPrefab = AssetLoader.Instance.Load<GameObject>(AssetLoader.GetLoadString("Prefabs_preloadcontainer", "CombatCamera.prefab"));
                    GameObject cam = Instantiate(CombatcameraPrefab);

                    main.GetComponentInChildren<ClientMain>().InitPlayerCamera(cam);

                    if (isCombatHierarchyLoaded)
                    {
                        SetupLevel(levelName);
                        PhotonNetwork.networkingPeer.NewSceneLoaded();
                    }
                }
                break;
        }
    }

    // Setup current level for map, additional setup for realm at enterrealm
    void SetupLevel(string levelName)
    {
        UIManager.OnAllWidgets();
        //ClientUtils.PlayMusic(GameSettings.MusicEnabled);
    }

    void LoadUICombat()
    {
        LoadLevel("UI_CombatHierarchy");
    }

    public void OnCombatHierarchyDestroyed()
    {
        isCombatHierarchyLoaded = false;
    }
}
