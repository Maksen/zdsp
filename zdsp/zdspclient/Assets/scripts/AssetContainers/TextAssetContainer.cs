using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateScriptableObject]
public class TextAssetContainer : BaseAssetContainer
{
    public override string ContainerType { get { return "Text"; } }

    public List<TextAsset> TextAssetList;

    private Dictionary<string, TextAsset> textAssetAssets = new Dictionary<string, TextAsset>();

    protected override void OnEnable()
    {
        base.OnEnable();
        Init();
    }

    public void Init()
    {
        if (textAssetAssets.Count > 0)
            return;

        if (TextAssetList == null)
            TextAssetList = new List<TextAsset>();

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
            if (textAssetAssets.ContainsKey(assetname))
            {
                Debug.LogFormat("TextAssetContainer [{0}] has duplicate TextAsset [{1}]", this.name, assetname);
                return false;
            }

            textAssetAssets.Add(assetname, (TextAsset)expAsset.asset);
            return true;
        }
        return false;
    }

    public override T GetAssetByPath<T>(string assetPath)
    {
        if (textAssetAssets.ContainsKey(assetPath))
            return textAssetAssets[assetPath] as T;

        return null;
    }


#if UNITY_EDITOR
    /// <summary>
    /// called by editor to add into list
    /// </summary>
    /// <typeparam name="T">TextAsset</typeparam>
    public override bool AddAsset<T>(UnityEngine.Object asset)
    {
        var obj = asset as TextAsset;
        string assetname = asset.name;

        if (!CanAddAsset<TextAsset>(obj))
        {
            Debug.LogFormat("TextAssetContainer [{0}]: failed to add [{1}]", this.name, assetname);
            return false;
        }

        if (!TextAssetList.Contains(obj))
        {
            TextAssetList.Add(obj);
            return true;
        }
        else
            Debug.LogFormat("TextAssetContainer [{0}] already contains asset [{1}]", this.name, assetname);

        return false;
    }

    /// <summary>
    /// removes any null or duplicated assets from list
    /// </summary>
    /// <typeparam name="T">TextAsset</typeparam>
    public override void ReorganiseList<T>()
    {
        if (TextAssetList.Count > 0)
        {
            List<TextAsset> tempList = TextAssetList.ToList();
            TextAssetList.Clear();
            foreach (TextAsset item in tempList)
            {
                if (item != null && !TextAssetList.Contains(item))
                {
                    TextAssetList.Add(item);
                }
            }
            TextAssetList = SortAsset(TextAssetList);
        }
    }

    /// <summary>
    /// extends the capacity of the list and fills with null
    /// </summary>
    /// <typeparam name="T">TextAsset</typeparam>
    public override void AddSlots<T>(int slots)
    {
        TextAssetList.AddRange(new TextAsset[slots]);
    }

    public override void OnWillSaveAssets()
    {
        ExportedAssets.Clear();
        foreach (TextAsset textasset in TextAssetList)
        {
            ExportedAsset exp = new ExportedAsset();
            if (allowAbsolutePath)
                exp.assetPath = AssetDatabase.GetAssetPath(textasset);
            else
                exp.assetPath = AssetDatabase.GetAssetPath(textasset).Replace("Assets/" + containerAssetsPath + "/", "");

            exp.asset = textasset;
            ExportedAssets.Add(exp);
        }
    }

    public override List<UnityEngine.Object> VerifySlots()
    {
        List<UnityEngine.Object> removeList = new List<UnityEngine.Object>();

        foreach (var textasset in TextAssetList)
        {
            if (textasset != null && !CanAddAsset<TextAsset>(textasset))
                removeList.Add(textasset);
        }

        foreach (var textasset in removeList)
        {
            TextAssetList.Remove((TextAsset)textasset);
        }

        return removeList;
    }

    public override void UpdateAndRefreshContainer()
    {
        string[] extensionArray = ".json;.txt".Split(';');
        string path = "Assets/" + containerAssetsPath;
        var files = Directory.GetFiles(path, "*.*", AddSubFolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            foreach (string extension in extensionArray)
            {
                if (file.EndsWith(extension) && !file.Contains("@"))
                {
                    string assetPath = file.Replace(Application.dataPath, "").Replace('\\', '/');
                    TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                    if (asset != null)
                        AddAsset<TextAsset>(asset);
                    break;
                }
            }
        }
        EditorUtility.SetDirty(this);
    }
#endif
}
