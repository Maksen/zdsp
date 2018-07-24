using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using System.IO;
using AssetBundles;
using System;

public class ZealotBuild
{
    readonly static string[] BuildScenes = new string[] { "Assets/Scenes/SplashScreen.unity", "Assets/Scenes/Dialog_SplashLoadingScreen.unity" };

    [MenuItem("Build/Clear Webplayer AssetBundles Cache", false, 2000)]
    public static void ClearWebplayerAssetBundlesCache()
    {
        //Caching.CleanCache();
    }

    [MenuItem("Build/Player Settings", false, 2000)]
    public static void OpenPlayerSettings()
    {
        EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
    }

    [MenuItem("Build/Builder/Set Scenes Bundle Names", false, 10000)]
    public static void SetScenesBundleNames()
    {
        float count = 0f;
        EditorUtility.DisplayProgressBar("Setting Scenes Bundle Names", "Hold on...", 0f);

        foreach (var sceneSetting in EditorBuildSettings.scenes)
        {
            count++;
            EditorUtility.DisplayProgressBar("Setting Scenes Bundle Names", "Hold on...", count / EditorBuildSettings.scenes.Length);
            if (File.Exists(sceneSetting.path))
            {
                var assetimporter = AssetImporter.GetAtPath(sceneSetting.path);
                string sceneName = Path.GetFileNameWithoutExtension(sceneSetting.path);
                if (sceneSetting.enabled && (!BuildScenes.Contains(sceneSetting.path)))
                {
                    assetimporter.assetBundleName = "levels/" + sceneName.ToLower();
                }
                else
                {
                    assetimporter.assetBundleName = "";
                }
            }


            AssetDatabase.SaveAssets();
        }

        EditorUtility.ClearProgressBar();
        Debug.Log("Done setting scenes asset bundle names");
    }
    [MenuItem("Build/Builder/Set AssetContainer Bundle Names", false, 10000)]
    public static void SetAssetContainerBundleNames()
    {
        float count = 0f;
        EditorUtility.DisplayProgressBar("Setting AssetContainer Bundle Names", "Hold on...", 0f);

        List<string> preLoadable = new List<string>();
        List<string> sceneLoaded = new List<string>();
        List<string> notbuild = new List<string>();

        Dictionary<string, string> bundleNames = new Dictionary<string, string>();
        string[] files = Directory.GetFiles("Assets/AssetContainers/", "*.asset");
        foreach(string assetpath in files)
        {
            count++;
            EditorUtility.DisplayProgressBar("Setting AssetContainer Bundle Names", "Hold on...", count / files.Length);

            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetpath);
            if (asset != null && asset is IAssetContainer)
            {
                IAssetContainer container = asset as IAssetContainer;
                container.OnWillSaveAssets();

                ((IAssetContainer)asset).SetIndividualAssetBundleNames();

                string filename = Path.GetFileNameWithoutExtension(assetpath).ToLower();
                string bundleName = "";

                if (((IAssetContainer)asset).Build && !((IAssetContainer)asset).IndividualAssetBundle)
                {
                    if (((IAssetContainer)asset).Preload)
                        preLoadable.Add(asset.name);
                    else
                        sceneLoaded.Add(asset.name);

                    //bundle names are ALL LOWERCASE!
                    bundleName = "assetcontainers/" + filename;
                }
                else
                {
                    notbuild.Add(asset.name);
                }

                var assetimporter = AssetImporter.GetAtPath(assetpath);
                assetimporter.assetBundleName = bundleName;

                if (!string.IsNullOrEmpty(bundleName))
                    bundleNames.Add(bundleName, assetpath);
            }
        }

        count = 0f;
        EditorUtility.DisplayProgressBar("Verifying AssetContainer Bundle Names", "Hold on...", 0f);

        //verify, in case bundle name is used by other assets
        foreach (string bundleName in bundleNames.Keys)
        {
            count++;
            EditorUtility.DisplayProgressBar("Verifying AssetContainer Bundle Names", "Hold on...", count / bundleNames.Keys.Count);

            var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            if(assetPaths.Length != 1)
            {
                string msg = string.Format("AssetBundle [{0}] should not have more than 1 asset inside", bundleName);
                Debug.LogError(msg);
                EditorUtility.DisplayDialog("Set AssetContainer Bundle Names Failed", msg, "OK");
            }
            else if(assetPaths[0] != bundleNames[bundleName])
            {
                string msg = string.Format("AssetBundle [{0}] has an invalid path", bundleName);
                Debug.LogError(msg);
                EditorUtility.DisplayDialog("Set AssetContainer Bundle Names Failed", msg, "OK");
            }
        }

        CompileAssetBundleManifest(preLoadable, sceneLoaded);

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Finish setting bundle names", "The following asset containers are not in build:\n" + string.Join(Environment.NewLine, notbuild.ToArray()), "Ok");
        Debug.Log("Done setting AssetContainers asset bundle names");
    }

    private static void CompileAssetBundleManifest(List<string> preloadable, List<string> sceneloaded)
    {
        var manifest = AssetDatabase.LoadAssetAtPath<AssetContainerManifest>("Assets/Resources/AssetContainerManifest.asset");
        if(manifest == null)
        {
            manifest = ScriptableObject.CreateInstance<AssetContainerManifest>();
            AssetDatabase.CreateAsset(manifest, "Assets/Resources/AssetContainerManifest.asset");
        }

        manifest.PreLoadable = preloadable;
        manifest.SceneLoaded = sceneloaded;

        EditorUtility.SetDirty(manifest);
    }

    [MenuItem("Build/Builder/Set AssetContainer Individual AssetBundle Names", false, 90000)]
    public static void SetAssetContainerIndividualBundleNames()
    {
        float count = 0f;
        EditorUtility.DisplayProgressBar("Setting Individual Bundle Names", "Hold on...", 0f);

        string[] files = Directory.GetFiles("Assets/AssetContainers/", "*.asset");
        foreach (string assetpath in files)
        {
            count++;
            EditorUtility.DisplayProgressBar("Setting Individual Bundle Names", "Hold on...", count / files.Length);

            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetpath);
            if (asset != null && asset is IAssetContainer)
            {
                ((IAssetContainer)asset).SetIndividualAssetBundleNames();
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();

        Debug.Log("Done Setting Individual Bundle Names");
    }

    [MenuItem("Build/Builder/Build Asset Bundles", false, 10100)]
    public static void BuildAssetBundles()
    {
        string assetbundlePath = AssetBundleTools.AssetBundlePath;

#if (MYCARD_LIVE || MYCARD_TEST)
        //assetbundlePath += "/MyCard";
#endif

        if (!Directory.Exists(assetbundlePath))
            Directory.CreateDirectory(assetbundlePath);

        var window = EditorWindow.GetWindow<BuildAssetBundleWindow>(true, "Build AssetBundles", true);
        window.TargetPath = assetbundlePath;
        window.ShowPopup();
    }

    //[MenuItem("Build/Builder/Build Android Player (.apk)", false, 10200)]
    public static void BuildAndroidPlayer(BuildOptions buildOptions, LoginServerIp loginServerIp)
    {
        BuildTarget buildTarget = BuildTarget.Android;

        try
        {
            //BuildOptions buildOptions = GetBuildOptions();
            string savepath = EditorUtility.SaveFilePanel("Build Android", "", "", "apk");
            if (savepath != string.Empty)
            {
                ZealotPreBuild(buildTarget, savepath, loginServerIp);
                BuildPipeline.BuildPlayer(BuildScenes, savepath, buildTarget, buildOptions);
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Failed building android player", e.ToString(), "OK");
        }

        ZealotPostBuild(buildTarget);
    }

    //[MenuItem("Build/Builder/Build Windows Player (.exe)", false, 10300)]
    public static void BuildWindowsPlayer(BuildOptions buildOptions, LoginServerIp loginServerIp)
    {
        BuildTarget buildTarget = BuildTarget.StandaloneWindows;

        try
        {
            //BuildOptions buildOptions = GetBuildOptions();
            string savepath = EditorUtility.SaveFilePanel("Build Windows Player", "", "", "exe");
            if (savepath != string.Empty)
            {
                ZealotPreBuild(buildTarget, savepath, loginServerIp);
                BuildPipeline.BuildPlayer(BuildScenes, savepath, buildTarget, buildOptions);
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Failed building windows player", e.ToString(), "OK");
        }

        ZealotPostBuild(buildTarget);
    }

    //[MenuItem("Build/Builder/Build iOS Player (.app)", false, 10400)]
    public static void BuildiOSPlayer(BuildOptions buildOptions, LoginServerIp loginServerIp)
    {
        BuildTarget buildTarget = BuildTarget.iOS;

        try
        {
            //BuildOptions buildOptions = GetBuildOptions();
            string savepath = EditorUtility.SaveFilePanel("Build iOS", "", "", "app");
            if (savepath != string.Empty)
            {
                ZealotPreBuild(buildTarget, savepath, loginServerIp);
                BuildPipeline.BuildPlayer(BuildScenes, savepath, buildTarget, buildOptions);
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Failed building android player", e.ToString(), "OK");
        }

        ZealotPostBuild(buildTarget);
    }

    private static void ZealotPreBuild(BuildTarget buildtarget, string outputFile, LoginServerIp loginServerIp)
    {
#if USE_ASSETBUNDLE
        CleanStreamingAssets();
#endif

        //create streaming assetsfolder
        if (!Directory.Exists("Assets/StreamingAssets"))
        {
            Directory.CreateDirectory("Assets/StreamingAssets");
        }
        UpdateBuildInfo(outputFile, loginServerIp);
        PhotonNetwork.PhotonServerSettings.SetLoginServerTypeForBuild(loginServerIp);
        CopyMoviesToStreamingAssets();
        CopyAssetBundlesToStreamingAssets(buildtarget);
        MoveResourcesFolders(true);
        
        //update game version string to player setting strin
        var gameVersion = Resources.Load<GameVersion>("GameVersion");
        gameVersion.UpdateGameVersion(buildtarget);
    }
    
    private static void ZealotPostBuild(BuildTarget buildtarget)
    {
        MoveResourcesFolders(false);
        DeleteFilesFromStreamingAssets();
    }

    /// <summary>
    /// Move various Resources to temp folders to prevent unnecessary assets in build
    /// </summary>
    /// <param name="movetoTemp">true to move to temp folders (prebuild), false to move back from temp (postbuild)</param>
    private static void MoveResourcesFolders(bool movetoTemp)
    {
        string[] resourceFolders =
        {
            "Assets/External/Cinema Suite/About/",
            "Assets/External/Cinema Suite/Cinema Director/System/",
            "Assets/External/Cinema Suite/Cinema Pro Cams/Example Scene/Materials/"
        };

        string tmpResourceFolder = "TempResources_tmp";

        for (int i = 0; i < resourceFolders.Length; i++)
        {
            if (movetoTemp)
            {
                if (AssetDatabase.IsValidFolder(resourceFolders[i] + "Resources"))
                {
                    if (AssetDatabase.IsValidFolder(resourceFolders[i] + tmpResourceFolder))
                        EditorUtils.DeleteDirectory(resourceFolders[i] + tmpResourceFolder + "/");

                    AssetDatabase.MoveAsset(resourceFolders[i] + "Resources", resourceFolders[i] + tmpResourceFolder);
                }
                else
                    Debug.LogErrorFormat("{0} is not a valid resource folder", resourceFolders[i] + "Resources");
            }
            else
            {
                if (AssetDatabase.IsValidFolder(resourceFolders[i] + tmpResourceFolder))
                {
                    string res = AssetDatabase.MoveAsset(resourceFolders[i] + tmpResourceFolder, resourceFolders[i] + "Resources");
                    if(!string.IsNullOrEmpty(res))
                    {
                        //delete temp folder
                        EditorUtils.DeleteDirectory(resourceFolders[i] + tmpResourceFolder + "/");
                    }
                }
            }
        }
    }

    private static BuildOptions GetBuildOptions()
    {
        BuildOptions options = BuildOptions.ShowBuiltPlayer;

#if ZEALOT_DEVELOPMENT
        options |= BuildOptions.Development;
#endif

        return options;
    }

#region StreamingAssets helpers
    private static bool AssetBundleFolderExists(BuildTarget buildtarget)
    {
        string sourcePath = AssetBundleTools.GetAssetBundleFolder(AssetBundleTools.AssetBundlePath, buildtarget);
        return Directory.Exists(sourcePath);
    }

    private static void CleanStreamingAssets()
    {
#if USE_ASSETBUNDLE
        FileUtil.DeleteFileOrDirectory("Assets/StreamingAssets");
#endif
    }

    private static void CopyAssetBundlesToStreamingAssets(BuildTarget buildtarget)
    {
#if !USE_ASSETBUNDLE
        string targetPath = AssetBundleTools.AssetBundlePath;


        //temp copy assetbundles to streaming assets        
        string sourcePath = AssetBundleTools.GetAssetBundleFolder(targetPath, buildtarget);

        EditorUtils.DirectoryCopy(sourcePath, "Assets/StreamingAssets/AssetBundles/", true);

        AssetDatabase.SaveAssets();
        Debug.Log("Copied assetbundles to streaming assets");
#endif
    }

    private static void CopyMoviesToStreamingAssets()
    {
#if !USE_ASSETBUNDLE
        string moviesPath = "Assets/Movies";

        DirectoryInfo dir = new DirectoryInfo(moviesPath);
        if (!dir.Exists)
            return;

        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            if (Path.GetExtension(file.Name).Equals(".mp4"))
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine("Assets/StreamingAssets", file.Name);

                // Copy the file.
                file.CopyTo(temppath, true);
            }
        }
#endif
    }

    private static void DeleteFilesFromStreamingAssets()
    {
#if !USE_ASSETBUNDLE
        //delete assetbundles from streamingassets
        EditorUtils.DeleteDirectory(Path.Combine("Assets/StreamingAssets/", AssetBundleTools.AssetBundlePath));

        Debug.Log("Deleted assetbundles in streaming assets");
#endif
    }
    #endregion

    private static void UpdateBuildInfo(string outputFile, LoginServerIp loginServerIp)
    {
        string info_path = Application.dataPath + "/StreamingAssets/game_info.txt";

        using (StreamWriter game_info = new StreamWriter(info_path))
        {
            game_info.Write(string.Format("{0} {1} {2}{3}", Path.GetFileName(outputFile), SystemInfo.deviceName, System.DateTime.Now.ToString("yyyyMMdd_HH:mm"), Environment.NewLine));
            game_info.Write(string.Format("login {0}", (int)loginServerIp));
            game_info.Close();
        }
    }


#region command line

    static void BuildAssetBundlesAuto(BuildTarget target)
    {
        string assetbundlePath = AssetBundleTools.AssetBundlePath;

#if (MYCARD_LIVE || MYCARD_TEST)
        //assetbundlePath += "/MyCard";
#endif

        if (!Directory.Exists(assetbundlePath))
            Directory.CreateDirectory(assetbundlePath);

        BuildAssetBundleWindow window = new BuildAssetBundleWindow();
        window.TargetPath = assetbundlePath;
        window.LoadOldManifest(target);//load old manifest then build assetbundle
    }
    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }

    private static bool HasArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name)
                return true;
        }
        return false;
    }

    private static void RefreshAsset()
    {
        AssetDatabase.Refresh();
    }

    private static void SetPreprocessor()
    {
        string useassetbundlevalue = GetArg("-UseAssetBundle");
        if (useassetbundlevalue == "yes")
        {

#if !USE_ASSETBUNDLE
            TogglePreprocessor("USE_ASSETBUNDLE");
#endif

        }
        else if (useassetbundlevalue == "no")
        {
#if USE_ASSETBUNDLE
                TogglePreprocessor("USE_ASSETBUNDLE");
#endif
        }


        string Configuration = GetArg("-Configuration");
        if (Configuration == "Debug")
        {
#if !ZEALOT_DEVELOPMENT
            TogglePreprocessor("ZEALOT_DEVELOPMENT");
#endif
        }
        else if (Configuration == "Release")
        {
#if ZEALOT_DEVELOPMENT
            TogglePreprocessor("ZEALOT_DEVELOPMENT");
#endif
        }

        string mycard = GetArg("-mycard");

        if (mycard == "yes")
        {
            if (Configuration == "Debug")
            {
#if MYCARD_LIVE
            TogglePreprocessor("MYCARD_LIVE");//remove my card live when it is debug
#endif
#if !MYCARD_TEST
                TogglePreprocessor("MYCARD_TEST");//add my card test when it is debug
#endif
            }
            else if (Configuration == "Release")
            {
#if !MYCARD_LIVE
            TogglePreprocessor("MYCARD_LIVE");//add my card live when it is release
#endif
#if MYCARD_TEST
                TogglePreprocessor("MYCARD_TEST");//remove my card test when it is release
#endif
            }
        }
        else if (mycard == "no")
        {
#if MYCARD_LIVE
            TogglePreprocessor("MYCARD_LIVE");//remove them
#elif MYCARD_TEST
            TogglePreprocessor("MYCARD_TEST");//remove them
#endif
        }
    }
    private static void PerformBuild()
    {
        BuildTarget buildTarget = BuildTarget.Android;

        bool buildPlayer = true;//HasArg("-BuildPlayer");
        string buildplayervalue = GetArg("-BuildPlayer");
        if(buildplayervalue == "yes")
        {
            buildPlayer = true;
        }
        else if(buildplayervalue == "no")
        {
            buildPlayer = false;
        }
        

        string buildassetbundlevalue = GetArg("-BuildAssetBundle");
        bool buildAssetBundle = false;
        if(buildassetbundlevalue == "yes")
        {
            buildAssetBundle = true;
        }
        else if (buildassetbundlevalue == "no")
        {
            buildAssetBundle = false;
        }

        string extension = ".apk";
        string platform = GetArg("-Platform");
        if(platform == "iOS")
        {
            buildTarget = BuildTarget.iOS;
            extension = ".app";
        }
        else if(platform == "Android")
        {
            buildTarget = BuildTarget.Android;
            extension = ".apk";
        }
		else if(platform == "Windows")
        {
            buildTarget = BuildTarget.StandaloneWindows;
            extension = ".exe";
        }

        //if (HasArg("-iOS"))
        //{
        //    buildTarget = BuildTarget.iOS;
        //}
        //else if (HasArg("-Android"))
        //{
        //    buildTarget = BuildTarget.Android;
        //}
        //mycard?

        //yyyyMMddTHHmm YearMonthDayTHourMinute
        string identifier = DateTime.Now.Date.ToString("yyyyMMdd") + "T" + DateTime.Now.ToLocalTime().Hour.ToString("00") + DateTime.Now.ToLocalTime().Minute.ToString("00");

        string folder = Application.dataPath + "/../build/" + platform + "/" + identifier;//Path.GetDirectoryName(Application.dataPath) + "/DailyClientBuild/Latest";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        if (buildAssetBundle)
        {
            SetScenesBundleNames();

            SetAssetContainerBundleNames();

            BuildAssetBundlesAuto(buildTarget);
           // string from = AssetBundleTools.GetAssetBundleFolder(AssetBundleTools.AssetBundlePath, buildTarget);
           // string to = Path.GetFullPath(folder) + "/Android";
          //  FileUtil.DeleteFileOrDirectory(to);
          //  FileUtil.CopyFileOrDirectory(from, to);
        }

        if (buildPlayer)
        {
            LoginServerIp serverConnected = LoginServerIp.Staging;
            string servertype = GetArg("-ServerType");
            if(servertype == "Private")
            {
                serverConnected = LoginServerIp.Private;
            }
            else if (servertype == "Staging")
            {
                serverConnected = LoginServerIp.Staging;
            }
            else if (servertype == "Live")
            {
                serverConnected = LoginServerIp.Live;
            }            

            string gameVersion = GetArg("-ServerVersion");
            if (gameVersion != PlayerSettings.bundleVersion && !string.IsNullOrEmpty(gameVersion))
                PlayerSettings.bundleVersion = gameVersion;

            string buildVersionString = GetArg("-BuildNumber");
            int buildVersion;
            bool hasBuildIncluded = int.TryParse(buildVersionString, out buildVersion);
            if (hasBuildIncluded && buildVersion != PlayerSettings.Android.bundleVersionCode)
                PlayerSettings.Android.bundleVersionCode = buildVersion;

            if (buildVersionString != PlayerSettings.iOS.buildNumber && !string.IsNullOrEmpty(buildVersionString))
                PlayerSettings.iOS.buildNumber = buildVersionString;

            string savepath = "";
            //zdsp_20171218T1427_1.0.0_1_private_debug.apk
            
			string mycard = GetArg("-mycard");
			string mycardname = "";
			if(mycard == "yes")
				mycardname = "_mycard";
#if ZEALOT_DEVELOPMENT
            savepath = folder + "/zdsp_" + identifier + "_" + gameVersion + "_" + buildVersionString + "_" + servertype + "_Debug" + mycardname + extension;
#else
			savepath = folder + "/zdsp_" + identifier + "_" + gameVersion + "_" + buildVersionString + "_" + servertype + "_Release" + mycardname + extension;
#endif

            ZealotPreBuild(buildTarget, savepath, serverConnected);


            BuildOptions option = GetBuildOptions();

            BuildPipeline.BuildPlayer(BuildScenes, savepath, buildTarget, option);

            ZealotPostBuild(buildTarget);
        }

       // FileUtil.CopyFileOrDirectory(folder, Path.GetDirectoryName(Application.dataPath) + "/DailyBuild/" + identifier);
    }

    static BuildTargetGroup[] targetGroups = { BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.Standalone };
    public static void TogglePreprocessor(string preprocessor)
    {
        var matchedGroups = targetGroups.ToDictionary(x => x, null);
        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (matchedGroups.ContainsKey(group))
            {
                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

                int index = strSettings.IndexOf(preprocessor, 0);
                if(index < 0)//cannot find
                {
                    strSettings = preprocessor + ";" + strSettings;

                    // MYCARD preprocessor requires a toggle between LIVE and TEST.
                    // When LIVE is active, TEST must be turned off and vice versa.
                    if (preprocessor == "MYCARD_LIVE")
                    {
                        // Remove MYCARD_TEST if found.
                        int myCardTestIndex = strSettings.IndexOf("MYCARD_TEST", 0);
                        if (myCardTestIndex >= 0)
                        {
                            strSettings = strSettings.Replace("MYCARD_TEST;", "");
                        }
                    }
                    else if (preprocessor == "MYCARD_TEST")
                    {
                        // Remove MYCARD_TEST if found.
                        int myCardLiveIndex = strSettings.IndexOf("MYCARD_LIVE", 0);
                        if (myCardLiveIndex >= 0)
                        {
                            strSettings = strSettings.Replace("MYCARD_LIVE;", "");
                        }
                    }
                }
                else
                {
                    strSettings = strSettings.Replace(preprocessor + ";", "");
                }
                
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
            }
        }
    }
    #endregion
}