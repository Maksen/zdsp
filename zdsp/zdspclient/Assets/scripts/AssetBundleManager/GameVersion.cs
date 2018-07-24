using UnityEngine;
using System;
using Zealot.Common;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LoginServerIp : byte
{
    Private = 0,    //for self testing
    Staging,        //for development
    Live,           //public live server
    PublicTest      //public verify and testing server
}

/// <summary>
/// For version control. Can be updated automatically from build tools in the future
/// </summary>
/// <remarks>Only 1 instance of should reside in Resources folder</remarks>
[CreateScriptableObject]
public class GameVersion : ScriptableObject
{
    [Header("Android")]
    [SerializeField]
    [ReadOnly]
    private string androidGameVersion;
    
    [Header("IOS")]
    [SerializeField]
    [ReadOnly]
    private string iOSGameVersion;
        
    [Header("My Card Android")]
    [SerializeField]
    [ReadOnly]
    private string mycardGameVersion;
    
    [Header("Patch Server Link")]
    [SerializeField]
    [ReadOnly]
    private string privateWebServerRootPath = "http://piliqstg.zealotdigital.com.tw/piliqpatch/";
    //sg = "http://bdmobile.zealotdigital.com.tw/piliq/"
    //tw = "http://piliqstg.zealotdigital.com.tw/piliqpatch/"
    //live = "http://piliqdemo2patch.zealotdigital.com.tw/piliqpatch/"
    //piliqdemo3.zealotdigital.com.tw 
    //http://piliqdemo3patch.zealotdigital.com.tw/piliqpatch3/ 
    
    public string WebServerRootPath
    {
        get
        {
            return privateWebServerRootPath;
        }
    }

    public string WebServerVersionPath
    {
        get { return WebServerRootPath + AssetBundles.Utility.GetPlatformName() + "/gameversion.txt"; }
    }
    public string WebServerConfigPath(LoginServerIp serverType)
    {
        return WebServerRootPath + AssetBundles.Utility.GetPlatformName() + "/" + serverType.ToString() + "gameConfig.txt";
    }

    public string ServerWithPatchVersion { get { return string.Format("{0}|{1}", GetVersion(), AssetBundleNumber); } }
    public string ServerVersion { get { return GetVersion(); } }

    private string _assetBundleRootPath;
    public string AssetBundlePath { get { return _assetBundleRootPath +"/"+ AssetBundleNumber; } }
    public void SetAssetBundleRootPath(string path)
    {
        _assetBundleRootPath = path;
        Debug.Log("_assetBundleRootPath :" + path);
    }

    private string _assetBundleNumber;
    public string AssetBundleNumber { get { return _assetBundleNumber; } }
    public void SetAssetBundleNumber(string num)
    {
        _assetBundleNumber = num;
        Debug.Log("assetbundleNum :" + num);
    }

    /// <summary>
    /// Called in-game to get client game version. Do not use for building installers.
    /// </summary>
    /// <returns></returns>
    public string GetVersion()
    {
#if UNITY_IOS
        return iOSGameVersion;
#elif MYCARD
        return mycardGameVersion;
#else
        return androidGameVersion;
#endif
    }

    public static ClientPlatform GetClientPlatform()
    {
#if UNITY_IOS
        return ClientPlatform.iOS;
#elif MYCARD
        return ClientPlatform.MyCard;
#else
        return ClientPlatform.Android;
#endif
    }

    ////TO BE REPLACED!
    //const string mycardURL = "http://myherogo.zld.com.tw/myherogo/mycard/index.html";
    //const string androidURL = "market://details?id=com.zealotdigital.zdgohero";
    //const string iosURL = "itms-apps://itunes.apple.com/app/wo-de-ying-xiong-menggo/id1158778280";   //https://itunes.apple.com/us/app/wo-de-ying-xiong-menggo/id1158778280

#if UNITY_EDITOR
    public void UpdateGameVersion(BuildTarget buildtarget)
    {
        var gameVersion = PlayerSettings.bundleVersion;
        switch (buildtarget)
        {
            case BuildTarget.Android:
#if MYCARD
                mycardGameVersion = gameVersion;
#else
                androidGameVersion = gameVersion;
#endif
                break;

            case BuildTarget.iOS:
                iOSGameVersion = gameVersion;
                break;
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public string EditorGetGameVersion(BuildTarget buildtarget)
    {
        switch (buildtarget)
        {
            case BuildTarget.Android:
#if MYCARD
                return mycardGameVersion;
#else
                return androidGameVersion;
#endif

            case BuildTarget.iOS:
                return iOSGameVersion;
        }

        return "";
    }
#endif
}

//public class GameVersionCreator
//{
//    [MenuItem("ZDGUI/Create GameVersion")]
//    public static void CreateGameVersion()
//    {
//        var scriptableObject = ScriptableObject.CreateInstance<GameVersion>();
//        AssetDatabase.CreateAsset(scriptableObject, "Assets/Resources/GameVersion.asset");
//        AssetDatabase.SaveAssets();
//    }
//}
