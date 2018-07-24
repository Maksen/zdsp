using AssetBundles;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIWindowLoader : MonoSingleton<UIWindowLoader>
{
    struct WindowAyncLoadOp
    {
        public string bundleName;
        public string assetName;
        public AssetBundleLoadAssetOperation request;
        public Action<GameObject> callback;

        public WindowAyncLoadOp(string bundleName, string assetName, AssetBundleLoadAssetOperation request, Action<GameObject> callback)
        {
            this.bundleName = bundleName;
            this.assetName = assetName;
            this.request = request;
            this.callback = callback;
        }
    }

    private Dictionary<string, WindowAyncLoadOp> mLoadingWindows = new Dictionary<string, WindowAyncLoadOp>();
    private Dictionary<string, WindowAyncLoadOp> mCachedLoaders = new Dictionary<string, WindowAyncLoadOp>();

    public void LoadUIWindow(string windowname, Action<GameObject> callback = null)
    {
        string assetContainerName = string.Format("UI_AsyncWindows");
        string assetName = string.Format("Assets/UI_PiLiQ/Scenes/{0}/P_{0}/UI_{0}.prefab", windowname);
        string loadpath = AssetLoader.GetLoadString(assetContainerName, assetName);
        StartCoroutine(AssetLoader.Instance.LoadAsyncCoroutine(loadpath, callback, true));
    }
}
