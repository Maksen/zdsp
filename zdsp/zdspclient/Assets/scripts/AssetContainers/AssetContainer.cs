using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class ExportedAsset
{
    public string assetPath;
    public UnityEngine.Object asset;
}

public interface IAssetContainer
{
    string Name { get; }
    string ContainerType { get; }
    string ContainerPath { get; }

    bool Preload { get; set; }
    bool Build { get; set; }
    bool IndividualAssetBundle { get; set; }
    bool AllowAbsolutePath { get; set; }
    bool AddSubFolder { get; set; }

    T GetAssetByPath<T>(string assetname) where T : UnityEngine.Object;
    List<ExportedAsset> GetExportedAssets();

#if UNITY_EDITOR
    bool AddAsset<T>(UnityEngine.Object asset) where T : UnityEngine.Object;
    void ReorganiseList<T>() where T : UnityEngine.Object;
    void AddSlots<T>(int slots) where T : UnityEngine.Object;

    List<UnityEngine.Object> VerifySlots();
    void OnWillSaveAssets();
    bool HasAsset(string assetPath);
    void SetIndividualAssetBundleNames();
    void LogAssetDetail(int index);
#endif
}

public abstract class BaseAssetContainer : ScriptableObject, IAssetContainer
{
    public string Name { get { return this.name; } }
    public abstract string ContainerType { get; }

    [SerializeField]
    protected bool preload;
    public bool Preload
    {
        get { return preload; }
        set { preload = value; }
    }

    [SerializeField]
    protected bool build;
    public bool Build
    {
        get { return build; }
        set { build = value; }
    }

    [SerializeField]
    protected bool individualAssetBundle;
    public bool IndividualAssetBundle
    {
        get { return individualAssetBundle; }
        set { individualAssetBundle = value; }
    }

    [HideInInspector]
    public string containerAssetsPath;
    public string ContainerPath { get { return containerAssetsPath; } }

    [SerializeField]
    protected bool allowAbsolutePath;
    public bool AllowAbsolutePath
    {
        get { return allowAbsolutePath; }
        set { allowAbsolutePath = value; }
    }

    [SerializeField]
    protected bool addSubFolder;
    public bool AddSubFolder
    {
        get { return addSubFolder; }
        set { addSubFolder = value; }
    }

    /// <summary>
    /// Used as a container to map assetpaths to assets. Hidden in inspector
    /// </summary>
    public List<ExportedAsset> ExportedAssets;
    public List<ExportedAsset> GetExportedAssets() { return ExportedAssets; }

    protected virtual void OnEnable()
    {
        if (ExportedAssets == null)
            ExportedAssets = new List<ExportedAsset>();
    }

    public abstract T GetAssetByPath<T>(string assetname) where T : UnityEngine.Object;

#if UNITY_EDITOR
    public abstract bool AddAsset<T>(UnityEngine.Object asset) where T : UnityEngine.Object;
    public abstract void AddSlots<T>(int slots) where T : UnityEngine.Object;
    public abstract void OnWillSaveAssets();
    public abstract void ReorganiseList<T>() where T : UnityEngine.Object;
    public abstract List<UnityEngine.Object> VerifySlots();
    protected virtual bool CanAddAsset<T>(UnityEngine.Object asset) where T : UnityEngine.Object
    {
        string assetPath = AssetDatabase.GetAssetPath(asset);

        if (!assetPath.Equals(string.Empty) && assetPath.StartsWith("Assets/" + containerAssetsPath + "/"))
        {
            return true;
        }
        else if (allowAbsolutePath && assetPath.StartsWith("Assets/"))
        {
            return true;
        }
        return false;
    }

    protected List<T> SortAsset<T>(List<T> allasset)
    {
        return allasset.OrderBy(o =>
        {
            UnityEngine.Object i = o as UnityEngine.Object;
            return i.name;
        }
        ).ToList();
    }
    public virtual bool HasAsset(string assetPath)
    {
        foreach (ExportedAsset expAsset in ExportedAssets)
        {
            if (expAsset.assetPath == assetPath)
            {
                return expAsset.asset != null;
            }
        }
        return false;
    }
    protected string GetExportedAssetBundleName(string containerName, string assetPath)
    {
        return containerName + "/" + assetPath.Replace(System.IO.Path.GetExtension(assetPath), "");
    }

    public void SetIndividualAssetBundleNames()
    {
        if (this.IndividualAssetBundle)
        {
            foreach (var ea in ExportedAssets)
            {
                if (ea.asset != null)
                {
                    var assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ea.asset));
                    assetImporter.assetBundleName = GetExportedAssetBundleName(Name, ea.assetPath);
                }
            }
        }
    }

    public void LogAssetDetail(int index)
    {
        if (ExportedAssets.Count > index)
        {
            string msg = string.Format("ContainerName:<color=#ff0000ff>{0}</color> \nAssetPath:<color=#ff0000ff>{1}</color>\nFullPath:<color=#ff0000ff>{0}/{1}</color>", Name, ExportedAssets[index].assetPath);
            Debug.Log(msg);
        }
        else
        {
            Debug.LogError("LogAssetDetail error: Try Save Assets");
        }
    }

    public virtual void UpdateAndRefreshContainer()
    {

    }
#endif
}