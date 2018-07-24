using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct SceneAsset
{
    public string assetPath;
    public UnityEngine.Object asset;
}

public class SceneAssetContainer : MonoBehaviour
{
    public Sprite mapIcon;
    public List<Sprite> spriteList;

    //[HideInInspector]
    [SerializeField]
    List<SceneAsset> ExportedAssets;

    private Dictionary<string, UnityEngine.Object> assetsByPath;

    void Awake()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
#else
        if(Application.isPlaying)
#endif
        {
            assetsByPath = new Dictionary<string, UnityEngine.Object>();
            foreach (var ea in ExportedAssets)
            {
                if (ea.asset != null)
                    assetsByPath.Add(ea.assetPath, (UnityEngine.Object)ea.asset);
            }
            AssetManager.RegisterSceneAssets(this);
        }
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        if (ExportedAssets == null)
            ExportedAssets = new List<SceneAsset>();

        ExportedAssets.Clear();
        if (spriteList != null)
        {
            foreach (var sprite in spriteList)
            {
                if (sprite != null)
                {
                    ExportedAssets.Add(new SceneAsset
                    {
                        assetPath = AssetDatabase.GetAssetPath(sprite).Remove(0, 7),
                        asset = sprite
                    });
                }
            }
        }
    }

    public void AddSprite(Sprite sprite)
    {
        if (sprite == null)
            return;

        if (spriteList == null)
            spriteList = new List<Sprite>();

        spriteList.Add(sprite);
    }
#endif

    public T GetSceneAsset<T>(string path) where T : UnityEngine.Object
    {
        if (assetsByPath.ContainsKey(path))
            return (T)assetsByPath[path];

        return null;
    }
}
