using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIAssetContainer : ScriptableObject
{
    private Dictionary<string, UnityEngine.Object> AssetMap = new Dictionary<string, UnityEngine.Object>();
    public List<ExportedAsset> ExportedAssets;

    void OnEnable()
    {

#if UNITY_EDITOR
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
#else
        if(Application.isPlaying)
#endif
        {
            if (ExportedAssets != null)
            {
                foreach (ExportedAsset expAsset in ExportedAssets)
                    AddAssetToMap(expAsset);
            }
        }
    }

    private bool AddAssetToMap(ExportedAsset expAsset)
    {
        if (expAsset != null && expAsset.asset != null)
        {
            string assetname = expAsset.assetPath;
            if (AssetMap.ContainsKey(assetname))
            {
                Debug.LogFormat("UIAssetContainer [{0}] has duplicate asset [{1}]", this.name, assetname);
                return false;
            }

            AssetMap.Add(assetname, (GameObject)expAsset.asset);
            return true;
        }
        return false;
    }

    public T GetAssetByPath<T>(string assetPath) where T :UnityEngine.Object
    {
        if (AssetMap.ContainsKey(assetPath))
        {
            return AssetMap[assetPath] as T;
        }
        return default(T);
    }

#if UNITY_EDITOR
    public bool AddAsset(string path, UnityEngine.Object asset)
    {
        if (ExportedAssets == null)
            ExportedAssets = new List<ExportedAsset>();

        if (asset == null || ExportedAssets.FirstOrDefault(x => x.assetPath == path) != null)
            return false;

        ExportedAsset ea = new ExportedAsset { assetPath = path, asset = asset };
        ExportedAssets.Add(ea);

        return true;
    }

    public void Clear()
    {
        if (ExportedAssets != null)
            ExportedAssets.Clear();
    }
#endif
}
