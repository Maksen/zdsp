using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneNPCContainer : MonoBehaviour
{
    [Serializable]
    struct SceneNPCAsset
    {
        public string assetPath;
        public UnityEngine.Object asset;
    }

    public List<GameObject> assets;

    [SerializeField]
    List<SceneNPCAsset> ExportedAssets;

    private Dictionary<string, GameObject> assetsByPath;

    void Awake()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
#else
        if(Application.isPlaying)
#endif
        {            
            assetsByPath = new Dictionary<string, GameObject>();
            foreach(var ea in ExportedAssets)
            {
                if(ea.asset != null)
                    assetsByPath.Add(ea.assetPath, (GameObject)ea.asset);
            }
            AssetManager.RegisterSceneNPC(this);
        }
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        if (ExportedAssets == null)
            ExportedAssets = new List<SceneNPCAsset>();

        ExportedAssets.Clear();
        if (assets != null)
        {
            foreach (var go in assets)
            {
                if (go != null)
                {
                    ExportedAssets.Add(new SceneNPCAsset
                    {
                        assetPath = AssetDatabase.GetAssetPath(go).Remove(0, 7),
                        asset = go
                    });
                }
            }
        }
    }

    public void AddModelPrefab(GameObject model)
    {
        if (assets == null)
            assets = new List<GameObject>();

        if (!assets.Contains(model))
            assets.Add(model);
    }
#endif

    public GameObject GetNPCPrefab(string path)
    {
        if (assetsByPath.ContainsKey(path))
            return assetsByPath[path];

        return null;
    }
}
