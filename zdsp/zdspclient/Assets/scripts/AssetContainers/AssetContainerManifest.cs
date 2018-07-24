using System;
using System.Collections.Generic;
using UnityEngine;

public class AssetContainerManifest : ScriptableObject
{
    public List<string> PreLoadable;
    public List<string> SceneLoaded;

    void OnEnable()
    {
        if (PreLoadable == null)
            PreLoadable = new List<string>();

        if (SceneLoaded == null)
            SceneLoaded = new List<string>();
    }

    public void AddAssetContainer(string containerName, bool preload)
    {
        if(preload)
        {
            if (!PreLoadable.Contains(containerName))
                PreLoadable.Add(containerName);
        }
        else
        {
            if (!SceneLoaded.Contains(containerName))
                SceneLoaded.Add(containerName);
        }
    }
}