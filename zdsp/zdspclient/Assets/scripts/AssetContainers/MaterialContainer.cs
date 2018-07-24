using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateScriptableObject]
public class MaterialContainer : BaseAssetContainer
{
    public override string ContainerType { get { return "Material"; } }

    public List<Material> MaterialList;

    private Dictionary<string, Material> materialAssets = new Dictionary<string, Material>();

    protected override void OnEnable()
    {
        base.OnEnable();

        if (MaterialList == null)
            MaterialList = new List<Material>();

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
            if (materialAssets.ContainsKey(assetname))
            {
                Debug.LogFormat("MaterialContainer [{0}] has duplicate material [{1}]", this.name, assetname);
                return false;
            }

            materialAssets.Add(assetname, (Material)expAsset.asset);
            return true;
        }
        return false;
    }

    public override T GetAssetByPath<T>(string assetPath)
    {
        if (materialAssets.ContainsKey(assetPath))
            return materialAssets[assetPath] as T;

        return null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// called by editor to add into list
    /// </summary>
    /// <typeparam name="T">Material</typeparam>
    public override bool AddAsset<T>(UnityEngine.Object asset)
    {
        var obj = asset as Material;
        string assetname = asset.name;

        if (!CanAddAsset<Material>(obj))
        {
            Debug.LogFormat("MaterialContainer [{0}]: failed to add [{1}]", this.name, assetname);
            return false;
        }

        if (!MaterialList.Contains(obj))
        {
            MaterialList.Add(obj);
            return true;
        }
        else
            Debug.LogFormat("MaterialContainer [{0}] already contains asset [{1}]", this.name, assetname);

        return false;
    }

    /// <summary>
    /// removes any null or duplicated assets from list
    /// </summary>
    /// <typeparam name="T">Material</typeparam>
    public override void ReorganiseList<T>()
    {
        if (MaterialList.Count > 0)
        {
            List<Material> tempList = MaterialList.ToList();
            MaterialList.Clear();
            foreach (Material item in tempList)
            {
                if (item != null && !MaterialList.Contains(item))
                {
                    MaterialList.Add(item);
                }
            }

            MaterialList = SortAsset(MaterialList);
        }
    }

    /// <summary>
    /// extends the capacity of the list and fills with null
    /// </summary>
    /// <typeparam name="T">Texture</typeparam>
    public override void AddSlots<T>(int slots)
    {
        MaterialList.AddRange(new Material[slots]);
    }

    public override void OnWillSaveAssets()
    {
        ExportedAssets.Clear();
        foreach (Material material in MaterialList)
        {
            ExportedAsset exp = new ExportedAsset();
            if (allowAbsolutePath)
                exp.assetPath = AssetDatabase.GetAssetPath(material);
            else
                exp.assetPath = AssetDatabase.GetAssetPath(material).Replace("Assets/" + containerAssetsPath + "/", "");

            exp.asset = material;
            ExportedAssets.Add(exp);
        }
    }

    public override List<UnityEngine.Object> VerifySlots()
    {
        List<UnityEngine.Object> removeList = new List<UnityEngine.Object>();

        foreach (var material in MaterialList)
        {
            if (material != null && !CanAddAsset<Material>(material))
                removeList.Add(material);
        }

        foreach (var material in removeList)
        {
            MaterialList.Remove((Material)material);
        }

        return removeList;
    }
#endif
}
