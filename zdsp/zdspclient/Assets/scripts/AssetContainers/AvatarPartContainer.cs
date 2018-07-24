using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateScriptableObject]
public class AvatarPartContainer : BaseAssetContainer
{
    public override string ContainerType { get { return "AvatarPart"; } }

    public List<Material> MaterialList;
    public List<Mesh> MeshList;

    private Dictionary<string, Material> materialAssets = new Dictionary<string, Material>();
    private Dictionary<string, Mesh> meshAssets = new Dictionary<string, Mesh>();

    private Dictionary<Type, object> assetListMap = new Dictionary<Type, object>();
    private Dictionary<Type, object> assetLists = new Dictionary<Type, object>();

    protected override void OnEnable()
    {
        base.OnEnable();

        if (MaterialList == null)
            MaterialList = new List<Material>();
        if (MeshList == null)
            MeshList = new List<Mesh>();

#if UNITY_EDITOR
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
#else
        if(Application.isPlaying)
#endif
        {
            foreach (ExportedAsset expAsset in ExportedAssets)
            {
                var asset = expAsset.asset;
                if (asset is Mesh)
                {
                    AddAssetToMap(meshAssets, expAsset);
                }
                else if (asset is Material)
                {
                    AddAssetToMap(materialAssets, expAsset);
                }
            }
        }

        assetListMap.Add(typeof(Material), materialAssets);
        assetListMap.Add(typeof(Mesh), meshAssets);

        assetLists.Add(typeof(Material), MaterialList);
        assetLists.Add(typeof(Mesh), MeshList);
        
    }

    private bool AddAssetToMap<T>(Dictionary<string, T> assets, ExportedAsset expAsset) where T : UnityEngine.Object
    {
        if (expAsset != null)
        {
            string assetname = expAsset.assetPath;
            if (assets.ContainsKey(assetname))
            {
                Debug.LogFormat("AvatarPartContainer [{0}] has duplicate asset [{1}]", this.name, assetname);
                return false;
            }

            assets.Add(assetname, (T)expAsset.asset);
            return true;
        }
        return false;
    }

    public override T GetAssetByPath<T>(string assetPath)
    {
        if (assetListMap.ContainsKey(typeof(T)))
        {
            var container = assetListMap[typeof(T)] as Dictionary<string, T>;

            if (container.ContainsKey(assetPath))
                return container[assetPath];
        }

        return null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// called by editor to add into list
    /// </summary>
    /// <typeparam name="T">Material or Mesh</typeparam>
    public override bool AddAsset<T>(UnityEngine.Object asset)
    {
        if (assetLists.ContainsKey(typeof(T)))
        {
            var assetMap = assetListMap[typeof(T)] as Dictionary<string, T>;
            var container = assetLists[typeof(T)] as List<T>;
            
            var obj = asset as T;
            string assetname = asset.name;

            if (!CanAddAsset<T>(obj))
            {
                Debug.LogFormat("AssetContainer [{0}]: failed to add [{1}]", this.name, assetname);
                return false;
            }

            if (!container.Contains(obj))
            {
                container.Add(obj);
                return true;
            }
            else
                Debug.LogFormat("AssetContainer [{0}] already contains asset [{1}]", this.name, assetname);
        }
        return false;
    }

    /// <summary>
    /// removes any null or duplicated assets from list
    /// </summary>
    /// <typeparam name="T">Material or Mesh</typeparam>
    public override void ReorganiseList<T>()
    {
        if (assetLists.ContainsKey(typeof(T)))
        {
            var container = assetLists[typeof(T)] as List<T>;
            if (container.Count > 0)
            {
                List<T> tempList = container.ToList();
                container.Clear();
                foreach (T item in tempList)
                {
                    if (item != null && !container.Contains(item))
                    {
                        container.Add(item);
                    }
                }

                container = SortAsset(container);
            }
        }
    }

    /// <summary>
    /// extends the capacity of the list and fills with null
    /// </summary>
    /// <typeparam name="T">Material or Mesh</typeparam>
    public override void AddSlots<T>(int slots)
    {
        if (assetLists.ContainsKey(typeof(T)))
        {
            var container = assetLists[typeof(T)] as List<T>;
            container.AddRange(new T[slots]);
        }
    }

    /// <summary>
    /// Recomile mapping of assets to their respective paths
    /// </summary>
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

        foreach (Mesh mesh in MeshList)
        {
            ExportedAsset exp = new ExportedAsset();
            if (allowAbsolutePath)
                exp.assetPath = AssetDatabase.GetAssetPath(mesh);
            else
                exp.assetPath = AssetDatabase.GetAssetPath(mesh).Replace("Assets/" + containerAssetsPath + "/", "");

            exp.asset = mesh;
            ExportedAssets.Add(exp);
        }
    }

    public override List<UnityEngine.Object> VerifySlots()
    {
        List<UnityEngine.Object> removeList = new List<UnityEngine.Object>();

        List<Material> remMaterial = new List<Material>();
        foreach (var material in MaterialList)
        {
            if (material != null && !CanAddAsset<Material>(material))
            {
                remMaterial.Add(material);
                removeList.Add(material);
            }
        }
        foreach (var material in remMaterial)
        {
            MaterialList.Remove(material);
        }

        List<Mesh> remMesh = new List<Mesh>();
        foreach (var mesh in MeshList)
        {
            if (mesh != null && !CanAddAsset<Mesh>(mesh))
            {
                remMesh.Add(mesh);
                removeList.Add(mesh);
            }
        }
        foreach (var mesh in remMesh)
        {
            MeshList.Remove(mesh);
        }

        return removeList;
    }
#endif
}