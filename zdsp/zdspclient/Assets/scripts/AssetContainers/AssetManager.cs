using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class AssetManager
{
    static Dictionary<string, IAssetContainer> levelContainers = new Dictionary<string, IAssetContainer>();

    static Dictionary<string, IAssetContainer> loadedContainers = new Dictionary<string, IAssetContainer>();
    static public Dictionary<string, BaseAssetContainer> LoadContainers
    {
        get { return loadedContainers.ToDictionary(x => x.Key, x => x.Value as BaseAssetContainer); }
    }

    static string currentLevel = "";
    static SceneNPCContainer _SceneNPCContainer;
    static SceneAssetContainer _SceneAssetContainer;

    public static string LoadPiliQGameData()
    {
#if UNITY_EDITOR
        var gameDataTA = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GameData/GameRepo/gamedata.json");
        return gameDataTA.text;
#else
        //should be loaded in splashscreen
        return "";
#endif
    }

    public static Dictionary<string, string> LoadLevelData()
    {
#if UNITY_EDITOR
        Dictionary<string, string> levellist = new Dictionary<string, string>();
        var files = Directory.GetFiles("Assets/GameData/Levels", "*.json");
        foreach (string filepath in files)
        {            
            var levelTA = AssetDatabase.LoadAssetAtPath<TextAsset>(string.Format("Assets/GameData/Levels/{0}", Path.GetFileName(filepath)));
            levellist.Add(Path.GetFileNameWithoutExtension(filepath), levelTA.text);
        }
        return levellist;

#else
        //should be loaded in splashscreen
        return null;
#endif
    }

    /// <summary>
    /// Loads unity object from given path name
    /// </summary>
    /// <typeparam name="T">Asset type</typeparam>
    /// <param name="path">container/assetpath e.g. Effects_npc/mon_01/atk1.prefab</param>
    public static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        string[] assetNames = path.Split('/');
        int seperator = path.IndexOf('/');
        if (seperator > -1)
        {
            string containerName = path.Substring(0, seperator);
            string assetPath = path.Substring(seperator + 1);
            IAssetContainer assetcontainer = GetAssetContainer(containerName);

            if (assetcontainer != null)
                return assetcontainer.GetAssetByPath<T>(assetPath);
        }
        return null;
    }

    public static T LoadAsset<T>(string containerName, string assetPath) where T : UnityEngine.Object
    {
        IAssetContainer assetcontainer = GetAssetContainer(containerName);
        if (assetcontainer != null)
            return assetcontainer.GetAssetByPath<T>(assetPath);

        return null;
    }

    public static void OnLoadedAssetContainerFromBundle(string containerName, BaseAssetContainer container)
    {
        loadedContainers[container.Name] = container;
    }

    #region AssetContainer Helpers
    public static IAssetContainer GetAssetContainer(string containerName)
    {
        if (loadedContainers.ContainsKey(containerName))
            return loadedContainers[containerName];
        else
            return LoadAssetContainer(containerName);
    }

    private static IAssetContainer LoadAssetContainer(string containerName)
    {
        //TODO: Handle loading from asset bundles
#if UNITY_EDITOR && !USE_ASSETBUNDLE
        string containerPath = Path.Combine("Assets/AssetContainers", containerName + ".asset");
        return AssetDatabase.LoadAssetAtPath<BaseAssetContainer>(containerPath);

#elif ZEALOT_DEVELOPMENT
        //release to load from asset bundle
        if (loadedContainers.ContainsKey(containerName))
            return loadedContainers[containerName];
        else
            return null;
#else
        return Resources.Load(containerName) as IAssetContainer;
#endif
    }

    #endregion

    #region Scene NPC
    public static void RegisterSceneNPC(SceneNPCContainer sceneNPCContainer)
    {
        _SceneNPCContainer = sceneNPCContainer;
    }

    public static void RegisterSceneAssets(SceneAssetContainer sceneAssetContainer)
    {
        _SceneAssetContainer = sceneAssetContainer;
    }

    public static void PreLevelLoad(string levelname)
    {
        if (!levelname.Equals("UI_CombatHierarchy", StringComparison.OrdinalIgnoreCase))
        {
            _SceneNPCContainer = null;
            _SceneAssetContainer = null;
            currentLevel = levelname;
        }
    }

    public static GameObject LoadSceneNPC(string path)
    {
        if(_SceneNPCContainer != null)
        {
            return _SceneNPCContainer.GetNPCPrefab(path);
        }
        return null;
    }

    public static T LoadSceneAsset<T>(string path) where T :UnityEngine.Object
    {
        if (_SceneAssetContainer != null)
        {
            return _SceneAssetContainer.GetSceneAsset<T>(path);

        }
        return null;
    }

    public static Sprite LoadSceneMapIcon()
    {
        if (_SceneAssetContainer != null)
        {
            return _SceneAssetContainer.mapIcon;

        }
        return null;
    }
    #endregion
}
