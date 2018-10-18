using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateScriptableObject]
public class PrefabContainer : BaseAssetContainer
{
    public override string ContainerType { get { return "Prefab"; } }

    public List<GameObject> PrefabList;
    private Dictionary<string, GameObject> prefabAssets = new Dictionary<string, GameObject>();

    protected override void OnEnable()
    {
        base.OnEnable();

        if (PrefabList == null)
            PrefabList = new List<GameObject>();

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
            if (prefabAssets.ContainsKey(assetname))
            {
                Debug.LogFormat("PrefabContainer [{0}] has duplicate prefab [{1}]", this.name, assetname);
                return false;
            }
            
            prefabAssets.Add(assetname, (GameObject)expAsset.asset);
            return true;             
        }
        return false;
    }

    public override T GetAssetByPath<T>(string assetPath)
    {
        if (prefabAssets.ContainsKey(assetPath))
        {
            return prefabAssets[assetPath] as T;
        }

        return default(T);
    }


#if UNITY_EDITOR
    /// <summary>
    /// called by editor to add into list
    /// </summary>
    /// <typeparam name="T">GameObject</typeparam>
    public override bool AddAsset<T>(UnityEngine.Object asset)
    {
        var obj = asset as GameObject;
        string assetname = asset.name;

        if (!CanAddAsset<T>(obj))
        {
            Debug.LogFormat("PrefabContainer [{0}]: failed to add [{1}]", this.name, assetname);
            return false;
        }

        if (!PrefabList.Contains(obj))
        {
            PrefabList.Add(obj);
            return true;
        }
        else
            Debug.LogFormat("PrefabContainer [{0}] already contains prefab [{1}]", this.name, assetname);

        return false;
    }

    /// <summary>
    /// removes any null or duplicated assets from list
    /// </summary>
    /// <typeparam name="T">GameObject</typeparam>
    public override void ReorganiseList<T>()
    {
        if (PrefabList.Count > 0)
        {
            List<GameObject> tempList = PrefabList.ToList();
            List<UnityEngine.Object> tempobjlist = new List<UnityEngine.Object>();
            PrefabList.Clear();

            foreach (GameObject gameobj in tempList)
            {
                if (gameobj != null && IsPrefab(gameobj) && !PrefabList.Contains(gameobj))
                {
                    PrefabList.Add(gameobj);
                    tempobjlist.Add(gameobj);
                }
            }

            SortAsset(tempobjlist);
        }
    }

    /// <summary>
    /// extends the capacity of the list and fills with null
    /// </summary>
    /// <typeparam name="T">GameObject</typeparam>
    public override void AddSlots<T>(int slots)
    {
        PrefabList.AddRange(new GameObject[slots]);
    }

    /// <summary>
    /// Recomile mapping of assets to their respective paths
    /// </summary>
    public override void OnWillSaveAssets()
    {
        ExportedAssets.Clear();
        foreach (GameObject gameobj in PrefabList)
        {
            ExportedAsset exp = new ExportedAsset();
            if (allowAbsolutePath)
                exp.assetPath = AssetDatabase.GetAssetPath(gameobj);
            else
                exp.assetPath = AssetDatabase.GetAssetPath(gameobj).Replace("Assets/" + containerAssetsPath + "/", "");

            exp.asset = gameobj;
            ExportedAssets.Add(exp);
        }
    }

    /// <summary>
    /// Verifies assets in container and returns list of objects that do not belong to the correct folder
    /// </summary>
    public override List<UnityEngine.Object> VerifySlots()
    {
        List<UnityEngine.Object> removeList = new List<UnityEngine.Object>();

        foreach (var prefab in PrefabList)
        {
            if (prefab != null && !CanAddAsset<GameObject>(prefab))
                removeList.Add(prefab);
        }

        foreach (var prefab in removeList)
        {
            PrefabList.Remove((GameObject)prefab);
        }

        return removeList;
    }

    /// <summary>
    /// Can add if asset belongs to the sub folder the asset container is created from
    /// </summary>
    protected override bool CanAddAsset<T>(UnityEngine.Object asset)
    {
        var result = base.CanAddAsset<T>(asset);

        string assetPath = AssetDatabase.GetAssetPath(asset);
        GameObject go = asset as GameObject;

        if (IsPrefab(go) && result)
        {
            return true;
        }
        return false;
    }

    private static bool IsPrefab(GameObject go)
    {
        return (PrefabUtility.GetPrefabParent(go) == null && PrefabUtility.GetPrefabObject(go) != null);
    }

    public override void UpdateAndRefreshContainer()
    {
        string[] extensionArray = ".prefab".Split(';');
        string path = "Assets/" + containerAssetsPath;
        var files = Directory.GetFiles(path, "*.*", AddSubFolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            foreach (string extension in extensionArray)
            {
                if (file.EndsWith(extension) && !file.Contains("@"))
                {
                    string assetPath = file.Replace(Application.dataPath, "").Replace('\\', '/');
                    GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (asset != null)
                        AddAsset<GameObject>(asset);
                    break;
                }
            }
        }
        EditorUtility.SetDirty(this);
    }
#endif
}