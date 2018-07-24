using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateScriptableObject]
public class TextureContainer : BaseAssetContainer
{
    public override string ContainerType { get { return "Texture"; } }

    public List<Texture> TextureList;

    private Dictionary<string, Texture> textureAssets = new Dictionary<string, Texture>();

    protected override void OnEnable()
    {
        base.OnEnable();

        if (TextureList == null)
            TextureList = new List<Texture>();

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
        if (expAsset != null)
        {
            string assetname = expAsset.assetPath;
            if (textureAssets.ContainsKey(assetname))
            {
                Debug.LogFormat("TextureContainer [{0}] has duplicate texture [{1}]", this.name, assetname);
                return false;
            }

            textureAssets.Add(assetname, (Texture)expAsset.asset);
            return true;
        }
        return false;
    }

    public override T GetAssetByPath<T>(string assetPath)
    {
        if (textureAssets.ContainsKey(assetPath))
            return textureAssets[assetPath] as T;

        return null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// called by editor to add into list
    /// </summary>
    /// <typeparam name="T">Texture</typeparam>
    public override bool AddAsset<T>(UnityEngine.Object asset)
    {
        var obj = asset as Texture;
        string assetname = asset.name;

        if (!CanAddAsset<Texture>(obj))
        {
            Debug.LogFormat("TextureContainer [{0}]: failed to add [{1}]", this.name, assetname);
            return false;
        }

        if (!TextureList.Contains(obj))
        {
            TextureList.Add(obj);
            return true;
        }
        else
            Debug.LogFormat("TextureContainer [{0}] already contains asset [{1}]", this.name, assetname);

        return false;
    }

    /// <summary>
    /// removes any null or duplicated assets from list
    /// </summary>
    /// <typeparam name="T">Texture</typeparam>
    public override void ReorganiseList<T>()
    {
        if (TextureList.Count > 0)
        {
            List<Texture> tempList = TextureList.ToList();
            TextureList.Clear();
            foreach (Texture item in tempList)
            {
                if (item != null && !TextureList.Contains(item))
                {
                    TextureList.Add(item);
                }
            }

            TextureList = SortAsset(TextureList);
        }
    }

    /// <summary>
    /// extends the capacity of the list and fills with null
    /// </summary>
    /// <typeparam name="T">Texture</typeparam>
    public override void AddSlots<T>(int slots)
    {
        TextureList.AddRange(new Texture[slots]);
    }

    public override void OnWillSaveAssets()
    {
        ExportedAssets.Clear();
        foreach (Texture texture in TextureList)
        {
            ExportedAsset exp = new ExportedAsset();
            if (allowAbsolutePath)
                exp.assetPath = AssetDatabase.GetAssetPath(texture);
            else
                exp.assetPath = AssetDatabase.GetAssetPath(texture).Replace("Assets/" + containerAssetsPath + "/", "");

            exp.asset = texture;
            ExportedAssets.Add(exp);
        }
    }

    public override List<UnityEngine.Object> VerifySlots()
    {
        List<UnityEngine.Object> removeList = new List<UnityEngine.Object>();

        foreach (var texture in TextureList)
        {
            if (texture != null && !CanAddAsset<Texture>(texture))
                removeList.Add(texture);
        }

        foreach (var texture in removeList)
        {
            TextureList.Remove((Texture)texture);
        }

        return removeList;
    }
#endif
}

