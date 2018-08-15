using System;
using UnityEngine;
using System.Collections;
using AssetBundles;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class LoadedAssetInfo
{
    public string container;
    public string assetPath;
    public int skillgroupid;
}

public class AssetLoadOperation
{
    readonly string bundlePath;
    readonly string containerName;
    readonly string assetName;

    float lastAccessed = Time.realtimeSinceStartup;

    UnityEngine.Object loadedAsset;

    Action<UnityEngine.Object> onLoaded;
    List<Action<UnityEngine.Object>> onLoadedLevelNoChangeCallback;
    //add all map of assset container down in this dictionary if want to load it fast. (not through the big asset container)
    static readonly Dictionary<string, string> containerNameToPathMap = new Dictionary<string, string>
    {
        //{"Effects_piliq_heroskill", "Assets/Effects_piliq/heroskill/" },
        //{"Effects_piliq_boss", "Assets/Effects_piliq/boss/" },
        //{"Effects_piliq_characters", "Assets/Effects_piliq/characters/" },
        //{"Effects_piliq_monster", "Assets/Effects_piliq/monster/" },
        //{"Effects_piliq_snpc", "Assets/Effects_piliq/snpc/" },
        //{"Effects_piliq_pet", "Assets/Effects_piliq/pet/" }
    };
    public AssetLoadOperation(string bundlePath, string containerName, string assetName)
    {
        this.bundlePath = bundlePath;
        this.containerName = containerName;
        this.assetName = assetName;
        onLoadedLevelNoChangeCallback = new List<Action<UnityEngine.Object>>();
    }

    public IEnumerator StartLoad<T>() where T : UnityEngine.Object
    {
        T loadedObj = null;
        //Debug.LogFormat ("AssetLoadOperation StartLoad {0}", bundlePath);
        if (AssetManager.LoadContainers.ContainsKey(containerName))//preloaded
        {
            string loadpath = AssetLoader.GetLoadString(containerName, assetName);
            loadedObj = AssetLoader.Instance.Load<T>(loadpath) as T;
        }
        else
        {
#if UNITY_EDITOR && !USE_ASSETBUNDLE
            if (string.Compare(containerName, "assetcontainers") != 0)
            {
                if (containerNameToPathMap.ContainsKey(containerName))
                {
                    var assetfullpath = containerNameToPathMap[containerName] + assetName;
                    //Debug.Log("loading " + assetfullpath);
                    loadedObj = AssetDatabase.LoadAssetAtPath<T>(assetfullpath);
                }
                else
                {
                    var containerPath = string.Format("Assets/AssetContainers/{0}.asset", containerName);

                    BaseAssetContainer container = AssetDatabase.LoadAssetAtPath<BaseAssetContainer>(containerPath);
                    if (container != null)
                    {
                        loadedObj = container.GetAssetByPath<T>(assetName);
                    }
                    else
                    {
                        Debug.LogError("AssetLoadOperation.StartLoad : container is not found | " + containerName);
                    }
                }
            }
            else
            {
                var containerPath = string.Format("Assets/AssetContainers/{0}", assetName);
                BaseAssetContainer container = AssetDatabase.LoadAssetAtPath<BaseAssetContainer>(containerPath);
                if (container != null)
                {
                    loadedObj = container as T;
                }
                else
                {
                    Debug.LogError("AssetLoadOperation.StartLoad : container is not found | " + containerName);
                }
            }

            yield return null;
#else
            AssetBundleLoadAssetOperation request = null;
            string bundleName = (containerName + "/" + assetName).ToLower();
            string assetFileName = assetName.Split('.')[0];
            int index1 = assetFileName.LastIndexOf('/');
            if (index1 != -1)
            {
                assetFileName = assetFileName.Substring(index1 + 1);
            
            }
            request = AssetBundleManager.LoadAssetAsync(bundleName.Split('.')[0], assetFileName, typeof(T));

            if (request != null)
            {

                yield return AssetLoader.Instance.StartCoroutine(request);

                loadedObj = request.GetAsset<T>();

                AssetBundleManager.UnloadAssetBundle(bundleName.Split('.')[0]);

            }
            else
			{
                //Debug.LogFormat ("AssetLoadOperation request null {0}", bundlePath);
                yield return null;
			}
#endif
        }

        //Debug.LogFormat ("AssetLoadOperation Finish {0}", bundlePath);
        AssetLoader.Instance.FinishLoad(bundlePath);
        if (loadedObj != null)
        {
            OnAssetLoaded(loadedObj);
        }
        else
        {
            Debug.LogWarningFormat("[AssetLoadOperation] Null asset, [{0}]", bundlePath);
            OnAssetLoaded(null);
        }
    }


    public void GetLoadedAsset<T>(Action<T> callback, bool removeCBOnLevelChanged) where T : UnityEngine.Object
    {
        SetLastAccessed();
        if (loadedAsset != null)
        {
            callback(loadedAsset as T);
        }
        else
        {
            if (removeCBOnLevelChanged)
                onLoadedLevelNoChangeCallback.Add(new Action<UnityEngine.Object>((asset) => callback(asset as T)));
            else
                onLoaded += new Action<UnityEngine.Object>((asset) => callback(asset as T));
        }
    }

    public void GetInstantiatedAsset(Action<GameObject> callback, bool removeCBOnLevelChanged)
    {
        SetLastAccessed();
        if (loadedAsset != null)
        {
            callback(InstantiateAsset(loadedAsset as GameObject));
        }
        else
        {
            if (removeCBOnLevelChanged)
                onLoadedLevelNoChangeCallback.Add((asset) => callback(InstantiateAsset(asset as GameObject)));
            else
                onLoaded += (asset) => callback(InstantiateAsset(asset as GameObject));
        }
    }

    GameObject InstantiateAsset(GameObject prefab)
    {
        if (prefab != null)
        {
            var newObj = GameObject.Instantiate(prefab) as GameObject;
            return newObj;
        }
        return null;
    }

    public void SetLastAccessed()
    {
        lastAccessed = Time.realtimeSinceStartup;
    }

    public float TimeSinceLastAccessed()
    {
        return Time.realtimeSinceStartup - lastAccessed;
    }

    void OnAssetLoaded(UnityEngine.Object asset)
    {
        loadedAsset = asset;
        for (int index = 0; index < onLoadedLevelNoChangeCallback.Count; index++)
            onLoaded += onLoadedLevelNoChangeCallback[index];
        if (onLoaded != null)
            onLoaded(asset);
        onLoadedLevelNoChangeCallback.Clear();
        onLoaded = null;
    }

    public void OnCleanup()
    {
        onLoadedLevelNoChangeCallback.Clear();
        onLoaded = null;
        loadedAsset = null;
    }

    public void OnLevelChanged()
    {
        onLoadedLevelNoChangeCallback.Clear();
    }
}