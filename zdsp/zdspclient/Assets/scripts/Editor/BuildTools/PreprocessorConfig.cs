using System.Linq;

using UnityEngine;
using UnityEditor;

public class PreprocessorConfig
{
    static BuildTargetGroup[] targetGroups = { BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.Standalone };

    const string zealotDevelopmentMenu = "Build/Preprocessor/Zealot Development Config";

    [MenuItem(zealotDevelopmentMenu, false, 20000)]
    public static void BuildSetDevelopment()
    {
        var matchedGroups = targetGroups.ToDictionary(x => x, null);
        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (matchedGroups.ContainsKey(group))
            {
                bool isRemoveDevelopment = false;
#if ZEALOT_DEVELOPMENT
                isRemoveDevelopment = true;
#else
                isRemoveDevelopment = false;
#endif
                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                //var strArray = strSettings.Split(';');

                if(isRemoveDevelopment == true)
                {
                    strSettings = strSettings.Replace("ZEALOT_DEVELOPMENT;", "");
                }
                else
                {
                    strSettings = "ZEALOT_DEVELOPMENT;" + strSettings;
                }
                //strSettings = string.Join(";", strArray.Where(x => !x.Contains("ZEALOT_RELEASE") && !x.Contains("ZEALOT_DEVELOPMENT")).ToArray());
                //strSettings = "ZEALOT_DEVELOPMENT;" + strSettings;

                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
            }
        }

        Debug.Log("Setting build to Development.");

        var window = EditorWindow.GetWindowWithRect<ReconfigPreprocWindow>(new Rect(50, 50, 400, 300), true, "Re-Configuring ZEALOT_DEVELOPMENT", true);
        window.Init("ZEALOT_DEVELOPMENT");


    }

    [MenuItem(zealotDevelopmentMenu, true, 20000)]
    public static bool CanSetDevConfig()
    {
#if ZEALOT_DEVELOPMENT
        Menu.SetChecked(zealotDevelopmentMenu, true);
#else
        Menu.SetChecked(zealotDevelopmentMenu, false);
       
#endif
        return true;
    }
    
    #region My Card
    const string myCardLiveConfigMenu = "Build/Preprocessor/MyCard Config/Live";
    [MenuItem(myCardLiveConfigMenu, false, 20000)]
    public static void ToggleMyCardConfig()
    {
#if MYCARD_LIVE
        bool insert = false;
#else
        bool insert = true;
#endif

        var matchedGroups = targetGroups.ToDictionary(x => x, null);
        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (matchedGroups.ContainsKey(group))
            {
                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                var strArray = strSettings.Split(';').ToList();

                if (insert)
                {
                    if (strArray.IndexOf("MYCARD_LIVE") < 0)
                    {
                        strArray.Add("MYCARD_LIVE");
                    }

                    int indexConfig = strArray.IndexOf("MYCARD_TEST");
                    if (indexConfig >= 0)
                    {
                        strArray.RemoveAt(indexConfig);
                    }
                }
                else
                {
                    int indexConfig = strArray.IndexOf("MYCARD_LIVE");
                    if (indexConfig >= 0)
                    {
                        strArray.RemoveAt(indexConfig);
                    }
                }
                strSettings = string.Join(";", strArray.ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
            }
        }

        if (insert)
            Debug.Log("Setting build to MYCARD_LIVE.");
        else
            Debug.Log("Setting build remove MYCARD_LIVE.");

        var window = EditorWindow.GetWindowWithRect<ReconfigPreprocWindow>(new Rect(50, 50, 400, 300), true, "Re-Configuring MYCARD_LIVE", true);
        window.Init((insert ? "insert" : "remove") + " MYCARD_LIVE");
    }

    [MenuItem(myCardLiveConfigMenu, true, 20000)]
    public static bool ToggleMyCardConfigVerify()
    {
#if MYCARD_LIVE
        Menu.SetChecked(myCardLiveConfigMenu, true);
        Menu.SetChecked(myCardTestConfigMenu, false);
#elif MYCARD_TEST
        Menu.SetChecked(myCardLiveConfigMenu, false);
        Menu.SetChecked(myCardTestConfigMenu, true);
#else
        Menu.SetChecked(myCardLiveConfigMenu, false);
        Menu.SetChecked(myCardTestConfigMenu, false);
#endif
        return true;
    }

    const string myCardTestConfigMenu = "Build/Preprocessor/MyCard Config/Test";
    [MenuItem(myCardTestConfigMenu, false, 20000)]
    public static void ToggleMyCardSandboxConfig()
    {
#if MYCARD_TEST
        bool insert = false;
#else
        bool insert = true;
#endif

        var matchedGroups = targetGroups.ToDictionary(x => x, null);
        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (matchedGroups.ContainsKey(group))
            {
                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                var strArray = strSettings.Split(';').ToList();

                if (insert)
                {
                    if (strArray.IndexOf("MYCARD_TEST") < 0)
                    {
                        strArray.Add("MYCARD_TEST");
                    }

                    int indexConfig = strArray.IndexOf("MYCARD_LIVE");
                    if (indexConfig >= 0)
                    {
                        strArray.RemoveAt(indexConfig);
                    }
                }
                else
                {
                    int indexConfig = strArray.IndexOf("MYCARD_TEST");
                    if (indexConfig >= 0)
                    {
                        strArray.RemoveAt(indexConfig);
                    }
                }
                strSettings = string.Join(";", strArray.ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
            }
        }

        if (insert)
            Debug.Log("Setting build to MYCARD_TEST.");
        else
            Debug.Log("Setting build remove MYCARD_TEST.");

        var window = EditorWindow.GetWindowWithRect<ReconfigPreprocWindow>(new Rect(50, 50, 400, 300), true, "Re-Configuring MYCARD_TEST", true);
        window.Init((insert ? "insert" : "remove") + " MYCARD_TEST");
    }

    [MenuItem(myCardTestConfigMenu, true, 20000)]
    public static bool ToggleMyCardTestConfigVerify()
    {
#if MYCARD_TEST
        Menu.SetChecked(myCardTestConfigMenu, true);
        Menu.SetChecked(myCardLiveConfigMenu, false);
#elif MYCARD_LIVE
        Menu.SetChecked(myCardTestConfigMenu, false);
        Menu.SetChecked(myCardLiveConfigMenu, true);
#else
        Menu.SetChecked(myCardTestConfigMenu, false);
        Menu.SetChecked(myCardLiveConfigMenu, false);
#endif
        return true;
    }
    #endregion

    #region Use Assetbundle
    const string useAssetBundleMenu = "Build/Preprocessor/UseAssetBundle Config";
    [MenuItem(useAssetBundleMenu, false, 20000)]
    public static void ToggleUseAssetBundle()
    {
#if USE_ASSETBUNDLE
        bool insert = false;
#else
        bool insert = true;
#endif

        var matchedGroups = targetGroups.ToDictionary(x => x, null);
        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (matchedGroups.ContainsKey(group))
            {
                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                var strArray = strSettings.Split(';').ToList();

                if (insert)
                {
                    if (strArray.IndexOf("USE_ASSETBUNDLE") < 0)
                    {
                        strArray.Add("USE_ASSETBUNDLE");
                    }
                }
                else
                {
                    int indexConfig = strArray.IndexOf("USE_ASSETBUNDLE");
                    if (indexConfig >= 0)
                    {
                        strArray.RemoveAt(indexConfig);
                    }
                }
                strSettings = string.Join(";", strArray.ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
            }
        }

        if (insert)
            Debug.Log("Setting build to UseAssetBundle.");
        else
            Debug.Log("Setting build remove UseAssetBundle.");

        var window = EditorWindow.GetWindowWithRect<ReconfigPreprocWindow>(new Rect(50, 50, 400, 300), true, "Re-Configuring USE_ASSETBUNDLE", true);
        window.Init((insert? "insert" : "remove") + " USE_ASSETBUNDLE");
    }

    [MenuItem(useAssetBundleMenu, true, 20000)]
    public static bool ToggleUseAssetBundleVerify()
    {
#if USE_ASSETBUNDLE

        Menu.SetChecked(useAssetBundleMenu, true);
#else
        Menu.SetChecked(useAssetBundleMenu, false);
#endif
        return true;
    }
    #endregion
    

    //    #region Zealot Console Commands
    //    const string zealotConsoleCommandMenu = "Build/Preprocessor/ZealotConsoleCommand Config";
    //    [MenuItem(zealotConsoleCommandMenu, false, 20000)]
    //    public static void ToggleZealotConsoleCommand()
    //    {
    //#if ZEALOT_CONSOLECOMMAND
    //        bool insert = false;
    //#else
    //        bool insert = true;
    //#endif

    //        var matchedGroups = targetGroups.ToDictionary(x => x, null);
    //        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
    //        {
    //            if (matchedGroups.ContainsKey(group))
    //            {
    //                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
    //                var strArray = strSettings.Split(';').ToList();

    //                if (insert)
    //                {
    //                    if (strArray.IndexOf("ZEALOT_CONSOLECOMMAND") < 0)
    //                    {
    //                        int indexZealotConfig = strArray.FindIndex(x => x.Contains("ZEALOT_"));
    //                        if (indexZealotConfig >= 0)
    //                        {
    //                            strArray.Insert(indexZealotConfig + 1, "ZEALOT_CONSOLECOMMAND");
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    int indexConfig = strArray.IndexOf("ZEALOT_CONSOLECOMMAND");
    //                    if (indexConfig >= 0)
    //                    {
    //                        strArray.RemoveAt(indexConfig);
    //                    }
    //                }
    //                strSettings = string.Join(";", strArray.ToArray());
    //                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
    //            }
    //        }

    //        if (insert)
    //            Debug.Log("Setting build to use ZEALOT_CONSOLECOMMAND.");
    //        else
    //            Debug.Log("Setting build remove ZEALOT_CONSOLECOMMAND.");

    //        var window = EditorWindow.GetWindowWithRect<ReconfigPreprocWindow>(new Rect(50, 50, 400, 300), true, "Re-Configuring ZEALOT_CONSOLECOMMAND", true);
    //        window.Init((insert ? "insert" : "remove") + " ZEALOT_CONSOLECOMMAND");
    //    }

    //    [MenuItem(zealotConsoleCommandMenu, true, 20000)]
    //    public static bool ToggleZealotConsoleCommandVerify()
    //    {
    //#if ZEALOT_CONSOLECOMMAND

    //        Menu.SetChecked(zealotConsoleCommandMenu, true);
    //#else
    //        Menu.SetChecked(zealotConsoleCommandMenu, false);
    //#endif
    //        return true;
    //    }
    //    #endregion

    #region Zealot Simplifier Chinese
    const string zealotSCMenu = "Build/Preprocessor/ZealotSimplifiedChinese Config";
    [MenuItem(zealotSCMenu, false, 20000)]
    public static void ToggleZealotSimplifiedChinese()
    {
#if ZEALOT_SimplifiedChinese
        bool insert = false;
#else
        bool insert = true;
#endif

        var matchedGroups = targetGroups.ToDictionary(x => x, null);
        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (matchedGroups.ContainsKey(group))
            {
                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                var strArray = strSettings.Split(';').ToList();

                if (insert)
                {
                    if (strArray.IndexOf("ZEALOT_SimplifiedChinese") < 0)
                    {
                        int indexZealotConfig = strArray.FindIndex(x => x.Contains("ZEALOT_"));
                        if (indexZealotConfig >= 0)
                        {
                            strArray.Insert(indexZealotConfig + 1, "ZEALOT_SimplifiedChinese");
                        }
                    }
                }
                else
                {
                    int indexConfig = strArray.IndexOf("ZEALOT_SimplifiedChinese");
                    if (indexConfig >= 0)
                    {
                        strArray.RemoveAt(indexConfig);
                    }
                }
                strSettings = string.Join(";", strArray.ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
            }
        }

        if (insert)
            Debug.Log("Setting build to use ZEALOT_SimplifiedChinese.");
        else
            Debug.Log("Setting build remove ZEALOT_SimplifiedChinese.");

        var window = EditorWindow.GetWindowWithRect<ReconfigPreprocWindow>(new Rect(50, 50, 400, 300), true, "Re-Configuring ZEALOT_SimplifiedChinese", true);
        window.Init((insert ? "insert" : "remove") + " ZEALOT_SimplifiedChinese");
    }

    [MenuItem(zealotSCMenu, true, 20000)]
    public static bool ToggleZealotSimplifiedChineseVerify()
    {
#if ZEALOT_SimplifiedChinese
        Menu.SetChecked(zealotSCMenu, true);
#else
        Menu.SetChecked(zealotSCMenu, false);
#endif
        return true;
    }
    #endregion


    [MenuItem("Build/Preprocessor/Show Config Window")]
    public static void ShowConfigWindow()
    {
        var window = EditorWindow.GetWindow<PreprocessorConfigWindow>();
        window.Init();
    }
}

public class PreprocessorConfigWindow : EditorWindow
{
    public int selBuildSetting = 0;
    public string[] buildSetting = new string[] { "Develop", "Release", "None - should not be used" };

    public int selLangSetting = 0;
    public string[] langSetting = new string[] { "Simplified Chinese - PRC", "Traditional Chinese - TW" };

    public bool useAssetBundle;

    public void Init()
    {
        //set settings
#if ZEALOT_DEVELOPMENT
        selBuildSetting = 0;
#else
        selBuildSetting = 1;
#endif

#if USE_ASSETBUNDLE
        useAssetBundle = true;
#else
        useAssetBundle = false;
#endif

#if ZEALOT_SimplifiedChinese
        selLangSetting = 0;
#else
        selLangSetting = 1;
#endif

        //show window
        Show();
    }

    void Apply(string setting, bool insert)
    { 
        BuildTargetGroup[] targetGroups = { BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.Standalone };
        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (targetGroups.Contains(group))
            {
                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group); 
                var strArray = strSettings.Split(';').ToList();

                if (insert)
                {
                    if (strArray.IndexOf(setting) < 0)
                    {
                        strArray.Add(setting);
                    }
                }
                else
                {
                    int indexConfig = strArray.IndexOf(setting);
                    if (indexConfig >= 0)
                    {
                        strArray.RemoveAt(indexConfig);
                    }
                }
                strSettings = string.Join(";", strArray.ToArray()); 
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
            }
        }
    }

    void Apply()
    {
        Apply("ZEALOT_DEVELOPMENT", selBuildSetting == 0);

        Apply("USE_ASSETBUNDLE", useAssetBundle);

        Apply("ZEALOT_SimplifiedChinese", selLangSetting == 0);

        var window = EditorWindow.GetWindowWithRect<ReconfigPreprocWindow>(new Rect(50, 50, 400, 300), true, "Re-Configuring", true);
        window.Init("");
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Build Setting");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Separator();
        selBuildSetting = GUILayout.SelectionGrid(selBuildSetting, buildSetting, buildSetting.Count());
        EditorGUILayout.Separator();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Language Setting");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        selLangSetting = GUILayout.SelectionGrid(selLangSetting, langSetting, langSetting.Count());
        EditorGUILayout.Separator();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();
        useAssetBundle = EditorGUILayout.Toggle("Use Asset Bundle : ", useAssetBundle);
        EditorGUILayout.Separator();


        Rect rect = new Rect(2, position.size.y - 30, position.size.x - 5, 30);
        GUILayout.BeginArea(rect);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply"))
        {
            Apply();
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();

    }
}

public class ReconfigPreprocWindow : EditorWindow
{
    public string Config;
    Vector2 scrollPos = Vector2.zero;
    GUIStyle centeredStyle;

    public void Init(string config)
    {
        centeredStyle = new GUIStyle();
        centeredStyle.alignment = TextAnchor.UpperCenter;
        centeredStyle.fontStyle = FontStyle.Bold;

        Config = config;

        this.minSize = new Vector2(400, 300);

        ShowPopup();
        Focus();
    }

    void OnGUI()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Setting " + Config, centeredStyle);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Please Wait...");

        Rect rect = new Rect(2, position.size.y - 30, position.size.x - 5, 30);
        GUILayout.BeginArea(rect);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("This window will close automatically after compilation");
        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
    }

    void Update()
    {
        if (!EditorApplication.isCompiling)
            this.Close();
    }
}