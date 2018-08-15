using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Video;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateScriptableObject]
public class VideoClipContainer : BaseAssetContainer
{
    public override string ContainerType { get { return "MovieClip"; } }

    public List<VideoClip> VideoClipList;

    private Dictionary<string, VideoClip> videoClipAssets = new Dictionary<string, VideoClip>();

    protected override void OnEnable()
    {
        base.OnEnable();
        Init();
    }

    public void Init()
    {
        if (videoClipAssets.Count > 0)
            return;

        if (VideoClipList == null)
            VideoClipList = new List<VideoClip>();

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
            if (videoClipAssets.ContainsKey(assetname))
            {
                Debug.LogFormat("VideoClipContainer [{0}] has duplicate VideoClip [{1}]", this.name, assetname);
                return false;
            }

            videoClipAssets.Add(assetname, (VideoClip)expAsset.asset);
            return true;
        }
        return false;
    }

    public override T GetAssetByPath<T>(string assetPath)
    {
        if (videoClipAssets.ContainsKey(assetPath))
            return videoClipAssets[assetPath] as T;

        return null;
    }


#if UNITY_EDITOR
    /// <summary>
    /// called by editor to add into list
    /// </summary>
    /// <typeparam name="T">AudioClip</typeparam>
    public override bool AddAsset<T>(UnityEngine.Object asset)
    {
        var obj = asset as VideoClip;
        string assetname = asset.name;

        if (!CanAddAsset<VideoClip>(obj))
        {
            Debug.LogFormat("VideoClipContainer [{0}]: failed to add [{1}]", this.name, assetname);
            return false;
        }

        if (!VideoClipList.Contains(obj))
        {
            VideoClipList.Add(obj);
            return true;
        }
        else
            Debug.LogFormat("VideoClipContainer [{0}] already contains asset [{1}]", this.name, assetname);

        return false;
    }

    /// <summary>
    /// removes any null or duplicated assets from list
    /// </summary>
    /// <typeparam name="T">AudioClip</typeparam>
    public override void ReorganiseList<T>()
    {
        if (VideoClipList.Count > 0)
        {
            List<VideoClip> tempList = VideoClipList.ToList();
            VideoClipList.Clear();
            foreach (VideoClip item in tempList)
            {
                if (item != null && !VideoClipList.Contains(item))
                {
                    VideoClipList.Add(item);
                }
            }

            VideoClipList = SortAsset(VideoClipList);
        }
    }

    /// <summary>
    /// extends the capacity of the list and fills with null
    /// </summary>
    /// <typeparam name="T">AudioClip</typeparam>
    public override void AddSlots<T>(int slots)
    {
        VideoClipList.AddRange(new VideoClip[slots]);
    }

    public override void OnWillSaveAssets()
    {
        ExportedAssets.Clear();
        foreach (VideoClip videoclip in VideoClipList)
        {
            ExportedAsset exp = new ExportedAsset();
            if (allowAbsolutePath)
                exp.assetPath = AssetDatabase.GetAssetPath(videoclip);
            else
                exp.assetPath = AssetDatabase.GetAssetPath(videoclip).Replace("Assets/" + containerAssetsPath + "/", "");

            exp.asset = videoclip;
            ExportedAssets.Add(exp);
        }
    }

    public override List<UnityEngine.Object> VerifySlots()
    {
        List<UnityEngine.Object> removeList = new List<UnityEngine.Object>();

        foreach (var videoclip in VideoClipList)
        {
            if (videoclip != null && !CanAddAsset<VideoClip>(videoclip))
                removeList.Add(videoclip);
        }

        foreach (var videoclip in removeList)
        {
            VideoClipList.Remove((VideoClip)videoclip);
        }

        return removeList;
    }
#endif
}
