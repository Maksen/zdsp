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
public class AudioClipContainer : BaseAssetContainer
{
    public override string ContainerType { get { return "AudioClip"; } }

    public List<AudioClip> AudioClipList;

    private Dictionary<string, AudioClip> audioClipAssets = new Dictionary<string, AudioClip>();

    protected override void OnEnable()
    {
        base.OnEnable();
        Init();
    }

    public void Init()
    {
        if (audioClipAssets.Count > 0)
            return;

        if (AudioClipList == null)
            AudioClipList = new List<AudioClip>();

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
            if (audioClipAssets.ContainsKey(assetname))
            {
                Debug.LogFormat("AudioClipContainer [{0}] has duplicate AudioClip [{1}]", this.name, assetname);
                return false;
            }

            audioClipAssets.Add(assetname, (AudioClip)expAsset.asset);
            return true;
        }
        return false;
    }

    public override T GetAssetByPath<T>(string assetPath)
    {
        if (audioClipAssets.ContainsKey(assetPath))
            return audioClipAssets[assetPath] as T;

        return null;
    }


#if UNITY_EDITOR
    /// <summary>
    /// called by editor to add into list
    /// </summary>
    /// <typeparam name="T">AudioClip</typeparam>
    public override bool AddAsset<T>(UnityEngine.Object asset)
    {
        var obj = asset as AudioClip;
        string assetname = asset.name;

        if (!CanAddAsset<AudioClip>(obj))
        {
            Debug.LogFormat("AudioClipContainer [{0}]: failed to add [{1}]", this.name, assetname);
            return false;
        }

        if (!AudioClipList.Contains(obj))
        {
            AudioClipList.Add(obj);
            return true;
        }
        else
            Debug.LogFormat("AudioClipContainer [{0}] already contains asset [{1}]", this.name, assetname);

        return false;
    }

    /// <summary>
    /// removes any null or duplicated assets from list
    /// </summary>
    /// <typeparam name="T">AudioClip</typeparam>
    public override void ReorganiseList<T>()
    {
        if (AudioClipList.Count > 0)
        {
            List<AudioClip> tempList = AudioClipList.ToList();
            AudioClipList.Clear();
            foreach (AudioClip item in tempList)
            {
                if (item != null && !AudioClipList.Contains(item))
                {
                    AudioClipList.Add(item);
                }
            }

            AudioClipList = SortAsset(AudioClipList);
        }
    }

    /// <summary>
    /// extends the capacity of the list and fills with null
    /// </summary>
    /// <typeparam name="T">AudioClip</typeparam>
    public override void AddSlots<T>(int slots)
    {
        AudioClipList.AddRange(new AudioClip[slots]);
    }

    public override void OnWillSaveAssets()
    {
        ExportedAssets.Clear();
        foreach (AudioClip audioclip in AudioClipList)
        {
            ExportedAsset exp = new ExportedAsset();
            if (allowAbsolutePath)
                exp.assetPath = AssetDatabase.GetAssetPath(audioclip);
            else
                exp.assetPath = AssetDatabase.GetAssetPath(audioclip).Replace("Assets/" + containerAssetsPath + "/", "");

            exp.asset = audioclip;
            ExportedAssets.Add(exp);
        }
    }

    public override List<UnityEngine.Object> VerifySlots()
    {
        List<UnityEngine.Object> removeList = new List<UnityEngine.Object>();

        foreach (var audioclip in AudioClipList)
        {
            if (audioclip != null && !CanAddAsset<AudioClip>(audioclip))
                removeList.Add(audioclip);
        }

        foreach (var audioclip in removeList)
        {
            AudioClipList.Remove((AudioClip)audioclip);
        }

        return removeList;
    }

    public override void UpdateAndRefreshContainer()
    {
        string[] extensionArray = ".wav;.mp3;.ogg".Split(';');
        string path = "Assets/" + containerAssetsPath;
        var files = Directory.GetFiles(path, "*.*", AddSubFolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            foreach (string extension in extensionArray)
            {
                if (file.EndsWith(extension) && !file.Contains("@"))
                {
                    string assetPath = file.Replace(Application.dataPath, "").Replace('\\', '/');
                    AudioClip asset = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                    if (asset != null)
                        AddAsset<AudioClip>(asset);
                    break;
                }
            }
        }
        EditorUtility.SetDirty(this);
    }
#endif
}
