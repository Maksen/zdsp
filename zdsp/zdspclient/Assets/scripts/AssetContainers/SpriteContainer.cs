using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateScriptableObject]
public class SpriteContainer : BaseAssetContainer
{
    public override string ContainerType { get { return "Sprite"; } }

    public List<Sprite> SpriteList;

    private Dictionary<string, Sprite> spriteAssets = new Dictionary<string, Sprite>();

    protected override void OnEnable()
    {
        base.OnEnable();
        Init();
    }

    public void Init()
    {
        if (spriteAssets.Count > 0)
            return;

        if (SpriteList == null)
            SpriteList = new List<Sprite>();

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
            if (spriteAssets.ContainsKey(assetname))
            {
                Debug.LogFormat("SpriteContainer [{0}] has duplicate sprite [{1}]", this.name, assetname);
                return false;
            }

            spriteAssets.Add(assetname, (Sprite)expAsset.asset);
            return true;
        }
        return false;
    }

    public override T GetAssetByPath<T>(string assetPath)
    {
        if (spriteAssets.ContainsKey(assetPath))
            return spriteAssets[assetPath] as T;

        return null;
    }


#if UNITY_EDITOR
    /// <summary>
    /// called by editor to add into list
    /// </summary>
    /// <typeparam name="T">Sprite</typeparam>
    public override bool AddAsset<T>(UnityEngine.Object asset)
    {
        var obj = asset as Sprite;
        string assetname = asset.name;

        if (!CanAddAsset<Sprite>(obj))
        {
            Debug.LogFormat("SpriteContainer [{0}]: failed to add [{1}]", this.name, assetname);
            return false;
        }

        if (!SpriteList.Contains(obj))
        {
            SpriteList.Add(obj);
            return true;
        }
        else
            Debug.LogFormat("SpriteContainer [{0}] already contains asset [{1}]", this.name, assetname);

        return false;
    }

    /// <summary>
    /// removes any null or duplicated assets from list
    /// </summary>
    /// <typeparam name="T">Sprite</typeparam>
    public override void ReorganiseList<T>()
    {
        if (SpriteList.Count > 0)
        {
            List<Sprite> tempList = SpriteList.ToList();
            SpriteList.Clear();
            foreach (Sprite item in tempList)
            {
                if (item != null && !SpriteList.Contains(item))
                {
                    SpriteList.Add(item);
                }
            }

            SpriteList = SortAsset(SpriteList);
        }
    }

    /// <summary>
    /// extends the capacity of the list and fills with null
    /// </summary>
    /// <typeparam name="T">Sprite</typeparam>
    public override void AddSlots<T>(int slots)
    {
        SpriteList.AddRange(new Sprite[slots]);
    }

    public override void OnWillSaveAssets()
    {
        ExportedAssets.Clear();
        foreach (Sprite sprite in SpriteList)
        {
            ExportedAsset exp = new ExportedAsset();
            if (allowAbsolutePath)
                exp.assetPath = AssetDatabase.GetAssetPath(sprite);
            else
                exp.assetPath = AssetDatabase.GetAssetPath(sprite).Replace("Assets/" + containerAssetsPath + "/", "");

            exp.asset = sprite;
            ExportedAssets.Add(exp);
        }
    }

    public override List<UnityEngine.Object> VerifySlots()
    {
        List<UnityEngine.Object> removeList = new List<UnityEngine.Object>();

        foreach (var sprite in SpriteList)
        {
            if (sprite != null && !CanAddAsset<Sprite>(sprite))
                removeList.Add(sprite);
        }

        foreach (var sprite in removeList)
        {
            SpriteList.Remove((Sprite)sprite);
        }

        return removeList;
    }
#endif
}
