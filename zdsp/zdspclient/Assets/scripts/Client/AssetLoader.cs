using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_EDITOR || USE_ASSETBUNDLE
using AssetBundles;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AssetLoader : MonoSingleton<AssetLoader>
{
    [SerializeField]
    float delay = 0;//fake delay

    const int MaxPrefabCache = 128; //assuming we dont load this amount of objects in 1 go
    const float ClearAssetsInterval = 120f;
    const float ClearPrefabInterval = 60f;
    float clearAssetsElapsed = 0f;
    float clearPrefabElapsed = 0f;

    Dictionary<string, AssetLoadOperation> prefabMap = new Dictionary<string, AssetLoadOperation>();
    Dictionary<string, AssetLoadOperation> assetMap = new Dictionary<string, AssetLoadOperation>();

    Dictionary<string, bool> stillLoading = new Dictionary<string, bool>();

    public IEnumerator Preload(System.Action<float> progress)
    {
#if !UNITY_EDITOR || USE_ASSETBUNDLE
        int count = 0;
        var manifest = Resources.Load<AssetContainerManifest>("AssetContainerManifest");
        Debug.Log("manifest.PreLoadable.Count : " + manifest.PreLoadable.Count);
        //do loading for preload asset (increase ref count in AssetBundleManager)
        foreach (string containerName in manifest.PreLoadable)
        {
            var containerRequest = AssetBundleManager.LoadAssetAsync(string.Concat("assetcontainers/", containerName.ToLower()), containerName, typeof(BaseAssetContainer));
            if (containerRequest != null)
            {
                ++count;
                yield return containerRequest;

                float value = (float)count / manifest.PreLoadable.Count;
                if (progress != null)
                    progress(value);

                var assetContainer = containerRequest.GetAsset<BaseAssetContainer>();

                AssetManager.OnLoadedAssetContainerFromBundle(containerName, assetContainer);
            }
        }
#endif
        yield return null;
    }

    public void LoadAsync<T>(string loadpath, System.Action<T> afterLoad_callback, bool removeCBOnLevelChanged = true) where T : UnityEngine.Object
    {
        string container, assetpath;
        GetBundleAssetFromBundle(loadpath, out container, out assetpath);
        LoadAsync(container, assetpath, afterLoad_callback, removeCBOnLevelChanged);
    }
    public void LoadAsync<T>(string loadpath, System.Action<T, LoadedAssetInfo> afterLoad_callback, bool removeCBOnLevelChanged = true) where T : UnityEngine.Object
    {
        string container, assetpath;
        GetBundleAssetFromBundle(loadpath, out container, out assetpath);
        LoadedAssetInfo info = new LoadedAssetInfo { container = container, assetPath = assetpath };
        LoadAsync(container, assetpath, info, afterLoad_callback, removeCBOnLevelChanged);
    }

    public void LoadAsync<T>(string loadpath, int skillgroupid, System.Action<T, LoadedAssetInfo> afterLoad_callback, bool removeCBOnLevelChanged = true) where T : UnityEngine.Object
    {
        string container, assetpath;
        GetBundleAssetFromBundle(loadpath, out container, out assetpath);
        LoadedAssetInfo info = new LoadedAssetInfo { skillgroupid = skillgroupid };
        LoadAsync(container, assetpath, info, afterLoad_callback, removeCBOnLevelChanged);
    }

    /// <summary>
    ///  Async loads asset.
    /// </summary>
    void LoadAsync<T>(string containerName, string assetName, System.Action<T> afterLoad_callback, bool removeCBOnLevelChanged) where T : UnityEngine.Object
    {
        StartCoroutine(LoadAsyncCoroutine(containerName, assetName, afterLoad_callback, removeCBOnLevelChanged));
    }

    void LoadAsync<T>(string containerName, string assetName, LoadedAssetInfo info, System.Action<T, LoadedAssetInfo> afterLoad_callback, bool removeCBOnLevelChanged) where T : UnityEngine.Object
    {
        StartCoroutine(LoadAsyncCoroutine(containerName, assetName, delegate (T obj) {
            afterLoad_callback(obj, info);
        }, removeCBOnLevelChanged));

    }

    /// <summary>
    ///  Async loads asset. For Method that requires control over the coroutine. 
    ///     Eg. wait for the load to complete then do the next step at loading screen
    /// </summary>
    IEnumerator LoadAsyncCoroutine<T>(string containerName, string assetName, System.Action<T> afterLoad_callback, bool removeCBOnLevelChanged) where T : UnityEngine.Object
    {
        string bundlePath = string.Format("{0}/{1}", containerName, assetName);

        bool isGameObject = typeof(T) == typeof(GameObject);
        var assetTable = isGameObject ? prefabMap : assetMap;
        if (assetTable.ContainsKey(bundlePath))
        {
            var loadop = assetTable[bundlePath];
            loadop.SetLastAccessed();
            loadop.GetLoadedAsset<T>(afterLoad_callback, removeCBOnLevelChanged);
            yield return null;
        }
        else
        {
            var loadop = new AssetLoadOperation(bundlePath, containerName, assetName);

            assetTable.Add(bundlePath, loadop);

            //if is prefab and assetTable is full, do cleaning on next cycle
            if (isGameObject && assetTable.Count >= MaxPrefabCache)
                clearPrefabElapsed += ClearPrefabInterval;

            //not prefab, just unload after callback
            if (!isGameObject)
                loadop.GetLoadedAsset<T>(new System.Action<T>((asset) => CleanUpAssetCache(bundlePath)), false);
            loadop.GetLoadedAsset<T>(afterLoad_callback, removeCBOnLevelChanged);
            stillLoading[bundlePath] = false;
            yield return loadop.StartLoad<T>();
        }
    }
    public IEnumerator LoadAsyncCoroutine<T>(string loadpath, System.Action<T> afterLoad_callback, bool removeCBOnLevelChanged) where T : UnityEngine.Object
    {
        string container, assetpath;
        GetBundleAssetFromBundle(loadpath, out container, out assetpath);
        return LoadAsyncCoroutine(container, assetpath, afterLoad_callback, removeCBOnLevelChanged);
    }
    public IEnumerator LoadAsyncContainer<T>(string containerName, System.Action<T> afterLoad_callback, bool removeCBOnLevelChanged) where T : BaseAssetContainer
    {
        return LoadAsyncCoroutine("assetcontainers", containerName, afterLoad_callback, removeCBOnLevelChanged);
    }

    ///// <summary>
    ///// Loads only preloaded assets synchronously.
    ///// </summary>
    //public T Load<T>(string path) where T : UnityEngine.Object
    //{
    //    string containerName;
    //    string assetName;
    //    GetBundleAssetFromBundle(path, out containerName, out assetName);
    //    return Load<T>(containerName, assetName);
    //}

    /// <summary>
    /// Loads only preloaded assets synchronously.
    /// </summary>
    T Load<T>(string containerName, string assetName) where T : UnityEngine.Object
    {
#if UNITY_EDITOR && !USE_ASSETBUNDLE
        var containerPath = string.Format("Assets/AssetContainers/{0}.asset", containerName);
        BaseAssetContainer container = AssetDatabase.LoadAssetAtPath<BaseAssetContainer>(containerPath);
        if (container != null && container.Preload)
        {
            return container.GetAssetByPath<T>(assetName);
        }
        else
        {
            Debug.LogError("AssetLoader.Load : container is not found or is not Preloaded");
            return null;
        }
#else
        if (AssetManager.LoadContainers.ContainsKey(containerName))//preloaded 
        {
            T asset = AssetManager.LoadAsset<T>(containerName, assetName) as T;
            return asset;
        }
        else
        {
            Debug.LogWarning("Resource is not preloaded " + containerName + "/" + assetName + ". Need to async load.");
            return null;
        }
#endif
    }

    public T Load<T>(string loadpath) where T : UnityEngine.Object
    {
        string container, assetpath;
        GetBundleAssetFromBundle(loadpath, out container, out assetpath);
        return Load<T>(container, assetpath);
    }

    void GetBundleAssetFromBundle(string path, out string containerName, out string assetPath)
    {
        containerName = "";
        assetPath = "";
        int seperator = path.IndexOf('/');
        if (seperator > -1)
        {
            containerName = path.Substring(0, seperator);
            assetPath = path.Substring(seperator + 1);
        }
        else
        {
            containerName = "";
            assetPath = path;
        }
    }

    public static string GetLoadString(string containerName, string assetName)
    {
        return string.Format("{0}/{1}", containerName, assetName);
    }

#region Cache handling
    void Update()
    {
        float elapsed = Time.deltaTime;
        //ClearAssetCache(elapsed);
        ClearPrefabCache(elapsed);
    }

    void ClearAssetCache(float elapsed)
    {
        //don't clean up
        clearAssetsElapsed += elapsed;
        if (clearAssetsElapsed > ClearAssetsInterval)
        {
            Debug.LogFormat("[AssetCache] {0} items", assetMap.Count);
            //OnClearAssetCache();
            clearAssetsElapsed = 0f;
        }
    }

    void CleanUpAssetCache(string bundlePath)
    {
        Debug.LogFormat("[Clear Prefab Cache] {0}", bundlePath);
        assetMap[bundlePath].OnCleanup();
        assetMap.Remove(bundlePath);
    }

    void OnClearAssetCache()
    {
        foreach (var asset in assetMap)
        {
            asset.Value.OnCleanup();
        }
        assetMap.Clear();
        Debug.LogFormat("[OnClearAssetCache] {0} items", assetMap.Count);
    }

    void ClearPrefabCache(float elapsed)
    {
        clearPrefabElapsed += elapsed;
        if (clearPrefabElapsed > ClearPrefabInterval)
        {
            Debug.LogFormat("[StillLoading] {0} items", stillLoading.Count);
            //if(stillLoading.Count > 0)
            //{
            //    foreach(string name in stillLoading.Keys)
            //        Debug.LogFormat("@ {0}", name);
            //}

            Debug.LogFormat("[PrefabCache] {0} items", prefabMap.Count);

            //use last accessed time for now, can implement one that manages cache by size in future
            ClearPrefabMapByLastAccessed();

            clearPrefabElapsed = 0f;
        }
    }

    List<string> toRemovePrefabs = new List<string>(MaxPrefabCache);
    void ClearPrefabMapByLastAccessed()
    {
        float maxPrefabLifespan = ClearPrefabInterval;

        foreach (var kvp in prefabMap)
        {
            string bundlePath = kvp.Key;
            if (stillLoading.ContainsKey(bundlePath))
                continue;
            var loadOp = kvp.Value;
            float lastAccessed = loadOp.TimeSinceLastAccessed();

            if (lastAccessed >= maxPrefabLifespan)
                toRemovePrefabs.Add(bundlePath);
        }

        if (toRemovePrefabs.Count > 0)
        {
            for (int i = 0; i < toRemovePrefabs.Count; i++)
            {
                string bundlePath = toRemovePrefabs[i];
                CleanUpPrefabCache(bundlePath);
            }
        }

        toRemovePrefabs.Clear();
    }

    void CleanUpPrefabCache(string bundlePath)
    {
        //Debug.LogFormat("[Clear Prefab Cache] {0}", bundlePath);
        prefabMap[bundlePath].OnCleanup();
        prefabMap.Remove(bundlePath);
    }

    public void OnLevelChanged()
    {
        foreach (var loadop in prefabMap.Values)
            loadop.OnLevelChanged();
        foreach (var loadop in assetMap.Values)
            loadop.OnLevelChanged();
    }
#endregion

    bool mapDirty = false;

    /// <summary>
    /// Called by maps when new textures are applied so that old resources can be released.
    /// </summary>
    /// <remarks>
    /// Can only call <see cref="Resources.UnloadUnusedAssets"/> after all assetbundle loading have finished, 
    /// else it seems like it may affect loading and pink textures may appear.
    /// </remarks>
    public void MapChange()
    {
        if (CanUnloadAssets())
        {
            mapDirty = false;
            StartCoroutine(UnloadUnusedAssets());
        }
        else
            mapDirty = true;
    }

    bool CanUnloadAssets()
    {
        return stillLoading.Count == 0;
    }

    public void FinishLoad(string bundlePath)
    {
        stillLoading.Remove(bundlePath);
        if (mapDirty && CanUnloadAssets())
        {
            mapDirty = false;
            StartCoroutine(UnloadUnusedAssets());
        }
    }

    IEnumerator UnloadUnusedAssets()
    {
        var asyncOp = Resources.UnloadUnusedAssets();//this is not so expensive as previously thought
        while (!asyncOp.isDone)
        {
            yield return null;
        }
    }
}
