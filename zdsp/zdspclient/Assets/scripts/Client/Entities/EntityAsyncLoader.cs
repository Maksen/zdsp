using AssetBundles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Client.Entities;

/// <summary>
/// deprecated class, npc are now loaded from scene npc containers
/// </summary>
class EntityAsyncLoader : MonoBehaviour
{
    struct AssetAyncLoadOp
    {
        public string bundleName;
        public string assetPath;
        public BaseClientEntity entity;
        public AssetBundleLoadAssetOperation request;

        public AssetAyncLoadOp(string bundleName, string assetPath, BaseClientEntity entity, AssetBundleLoadAssetOperation request)
        {
            this.bundleName = bundleName;
            this.assetPath = assetPath;
            this.entity = entity;
            this.request = request;
        }
    }

    private List<AssetAyncLoadOp> mLoadingClientEntities = new List<AssetAyncLoadOp>();
    private Dictionary<int, AssetAyncLoadOp> mLoadingNetEntities = new Dictionary<int, AssetAyncLoadOp>();
    private Dictionary<string, int> mLoadingContainers = new Dictionary<string, int>();

    public void OnRemoveEntity(BaseNetEntityGhost entity)
    {
        int pid = entity.GetPersistentID();
        if (mLoadingNetEntities.ContainsKey(pid))
        {
#if !UNITY_EDITOR || USE_ASSETBUNDLE
            var loadop = mLoadingNetEntities[pid];
            RemoveLoadingContainer(loadop.bundleName);
#endif
            mLoadingNetEntities.Remove(pid);
        }
    }

    public void LoadNetEntityAnimObj(BaseNetEntityGhost entity, string path)
    {
        int persistentID = entity.GetPersistentID();
        if (!mLoadingNetEntities.ContainsKey(persistentID))
        {
            string[] assetNames = path.Split('/');
            int seperator = path.IndexOf('/');
            if (seperator > -1)
            {
                string containerName = path.Substring(0, seperator);
                string assetPath = path.Substring(seperator + 1);
                string bundleName = string.Format("assetcontainers/{0}", containerName.ToLower());

#if UNITY_EDITOR && !USE_ASSETBUNDLE

                var requestInfo = new AssetAyncLoadOp { bundleName = bundleName, assetPath = assetPath, entity = entity, request = null };
                mLoadingNetEntities.Add(persistentID, requestInfo);

#else
                AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(bundleName, containerName, typeof(BaseAssetContainer));
                if (request != null)
                {
                    if (mLoadingContainers.ContainsKey(bundleName))
                        mLoadingContainers[bundleName]++;
                    else
                        mLoadingContainers[bundleName] = 1;

                    var coroutine = StartCoroutine(request);
                    var requestInfo = new AssetAyncLoadOp { bundleName = bundleName, assetPath = assetPath, entity = entity, request = request };

                    mLoadingNetEntities.Add(persistentID, requestInfo);
                }
#endif
            }
        }
    }

    public void LoadClientEntityAnimObj(StaticClientNPC entity, string path)
    {
        string[] assetNames = path.Split('/');
        int seperator = path.IndexOf('/');
        if (seperator > -1)
        {
            string containerName = path.Substring(0, seperator);
            string assetPath = path.Substring(seperator + 1);
            string bundleName = string.Format("assetcontainers/{0}", containerName.ToLower());

#if UNITY_EDITOR && !USE_ASSETBUNDLE

            var requestInfo = new AssetAyncLoadOp { bundleName = bundleName, assetPath = assetPath, entity = entity, request = null };
            mLoadingClientEntities.Add(requestInfo);

#else
            AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(bundleName, containerName, typeof(BaseAssetContainer));
            if (request != null)
            {
                if (mLoadingContainers.ContainsKey(bundleName))
                    mLoadingContainers[bundleName]++;
                else
                    mLoadingContainers[bundleName] = 1;

                var coroutine = StartCoroutine(request);
                var requestInfo = new AssetAyncLoadOp { bundleName = bundleName, assetPath = assetPath, entity = entity, request = request };

                mLoadingClientEntities.Add(requestInfo);
            }
#endif
        }
    }

    private void RemoveLoadingContainer(string bundleName)
    {
        if (!mLoadingContainers.ContainsKey(bundleName) || --mLoadingContainers[bundleName] <= 0)
        {
            AssetBundleManager.UnloadAssetBundle(bundleName);
            mLoadingContainers.Remove(bundleName);
        }
    }

    void Update()
    {
        int removeKey = -1;

        foreach(var kvp in mLoadingNetEntities)
        {
#if UNITY_EDITOR && !USE_ASSETBUNDLE

            var requestInfo = kvp.Value;
            var asset = AssetManager.LoadAsset<GameObject>(requestInfo.bundleName.Replace("assetcontainers/", ""), requestInfo.assetPath);
            requestInfo.entity.OnAnimObjLoaded(asset);

            removeKey = kvp.Key;
            break;
#else
            if (kvp.Value.request.IsDone())
            {
                var requestInfo = kvp.Value;
                var container = kvp.Value.request.GetAsset<BaseAssetContainer>();

                requestInfo.entity.OnAnimObjLoaded(container.GetAssetByPath<GameObject>(requestInfo.assetPath));
                removeKey = kvp.Key;

                RemoveLoadingContainer(requestInfo.bundleName);

#if USE_CACHING
                cachedAssets[requestInfo.assetPath] = asset;
#endif
                break;
            }
#endif
        }

        if (removeKey > 0)
            mLoadingNetEntities.Remove(removeKey);

        int removeIdx = -1;
        for(int i=0; i < mLoadingClientEntities.Count; i++)
        {
            var loadOp = mLoadingClientEntities[i];

#if UNITY_EDITOR && !USE_ASSETBUNDLE

            var asset = AssetManager.LoadAsset<GameObject>(loadOp.bundleName.Replace("assetcontainers/", ""), loadOp.assetPath);
            loadOp.entity.OnAnimObjLoaded(asset);

            removeIdx = i;
            break;
#else
            if (loadOp.request.IsDone())
            {
                var container = loadOp.request.GetAsset<BaseAssetContainer>();
                loadOp.entity.OnAnimObjLoaded(container.GetAssetByPath<GameObject>(loadOp.assetPath));

                removeIdx = i;
                RemoveLoadingContainer(loadOp.bundleName);

                break;
            }
#endif
        }

        if (removeIdx >= 0)
            mLoadingClientEntities.RemoveAt(removeIdx);
    }
}
