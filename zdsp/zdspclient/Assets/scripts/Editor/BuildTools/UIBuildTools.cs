using UnityEditor;

class UIBuildTools
{
    /// <summary>
    /// Compiles npc models into Assets/UI/Scenes/Activity/ActivityModels.asset
    /// </summary>
    [MenuItem("Tools/Compile UI_Activity Assets", false, 60000)]
    public static void CompileUIActivityAssets()
    {
        /*
        var gamedb = EditorUtils.GetGameDB();
        if (gamedb == null)
            return;

        //NPCRepo.Init(gamedb);

        List<string> modelPaths = new List<string>();

        //compile model prefab assets
        float count = 0f;
        int specialBossCount = gamedb.SpecialBoss.Values.Count;
        EditorUtility.DisplayProgressBar("Compiling UI Activity Assets", "Retrieving archetype info", 0f);

        foreach (var bossinfo in gamedb.SpecialBoss.Values)
        {
            count++;
            EditorUtility.DisplayProgressBar("Compiling UI Activity Assets", "Retrieving archetype info", 0f + (count / specialBossCount) * 0.4f);

            var archetypeInfo = NPCRepo.GetArchetypeById(bossinfo.archetype);
            if(archetypeInfo != null)
            {
                if (!modelPaths.Contains(archetypeInfo.modelprefab))
                    modelPaths.Add(archetypeInfo.modelprefab);
            }
        }

        count = 0f;
        int modelCount = modelPaths.Count;
        EditorUtility.DisplayProgressBar("Compiling UI Activity Assets", "Importing assets", 0.5f);

        if (modelPaths.Count > 0)
        {
            foreach (string path in modelPaths)
            {
                count++;
                EditorUtility.DisplayProgressBar("Compiling UI Activity Assets", "Importing assets", 0.5f + (count / modelCount) * 0.4f);

                var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(string.Concat("Assets/", path));

                if (modelPrefab != null)
                {
                    string prefabName = Path.GetFileNameWithoutExtension(path);
                    var assetimporter = AssetImporter.GetAtPath("Assets/" + path);
                    if (assetimporter != null)
                        assetimporter.assetBundleName = "activity_models/" + prefabName.ToLower();
                }
            }
        }

        AssetDatabase.SaveAssets();

        EditorUtility.ClearProgressBar();*/
    }

    //[MenuItem("Tools/Compile UI Window Manifest", false, 60000)]
    //public static void CompileUIWindowManifest()
    //{
    //    EditorUtility.DisplayProgressBar("Compiling UI Window Manifest", "Reading asset bundle paths", 0f);
    //    List<WinPanel> preLoaded = new List<WinPanel>();
    //    List<WinPanel> asyncLoaded = new List<WinPanel>();

    //    float count = 0f;
    //    //get all scenes from assetbundles
    //    var bundleNames = AssetDatabase.GetAllAssetBundleNames().Where(x=>x.StartsWith("p/"));
    //    foreach(string bundlename in bundleNames)
    //    {
    //        var assetpaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundlename);
    //        foreach (var path in assetpaths)
    //        {
    //            count++;
    //            EditorUtility.DisplayProgressBar("Compiling UI Window Manifest", "Reading asset bundle paths", (float)(count/bundlename.Length));
    //            GameObject windowprefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
    //            if (windowprefab != null)
    //            {
    //                WindowSubscriber sub = windowprefab.GetComponent<WindowSubscriber>();
    //                if (sub != null)
    //                {
    //                    if (sub.AsyncLoad)
    //                        asyncLoaded.Add(sub.WinPanel);
    //                    else
    //                        preLoaded.Add(sub.WinPanel);
    //                }
    //            }                
    //        }
    //    }

    //    EditorUtility.DisplayProgressBar("Compiling UI Window Manifest", "Saving manifest", 1f);
    //    var manifest = AssetDatabase.LoadAssetAtPath<UIWindowManifest>("Assets/GameData/UIWindowManifest.asset");
    //    if (manifest == null)
    //    {
    //        manifest = ScriptableObject.CreateInstance<UIWindowManifest>();
    //        AssetDatabase.CreateAsset(manifest, "Assets/GameData/UIWindowManifest.asset");
    //    }
    //    manifest.PreLoadable = preLoaded;
    //    manifest.AsyncLoaded = asyncLoaded;
    //    EditorUtility.SetDirty(manifest);
    //    AssetDatabase.SaveAssets();

    //    EditorUtility.ClearProgressBar();   
    //}
}