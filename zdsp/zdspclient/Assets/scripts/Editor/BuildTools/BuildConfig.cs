using System.Linq;

using UnityEngine;
using UnityEditor;
using System;

public class BuildConfig
{
    static BuildTargetGroup[] targetGroups = {BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.Standalone};

    //[MenuItem("Build/Preprocessor/Set Development Config", false, 20000)]
    //public static void BuildSetDevelopment()
    //{
    //    var matchedGroups = targetGroups.ToDictionary(x => x, null);
    //    foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
    //    {
    //        if(matchedGroups.ContainsKey(group))
    //        {
    //            string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
    //            var strArray = strSettings.Split(';');

    //            strSettings = string.Join(";", strArray.Where(x => !x.Contains("ZEALOT_")).ToArray());
    //            strSettings = "ZEALOT_DEVELOPMENT;" + strSettings;

    //            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
    //        }
    //    }

    //    Debug.Log("Setting build to Development.");
    //}

    //[MenuItem("Build/Preprocessor/Set Release Config", false, 20000)]
    //public static void BuildSetRelease()
    //{
    //    var matchedGroups = targetGroups.ToDictionary(x => x, null);
    //    foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
    //    {
    //        if (matchedGroups.ContainsKey(group))
    //        {
    //            string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
    //            var strArray = strSettings.Split(';');

    //            strSettings = string.Join(";", strArray.Where(x => !x.Contains("ZEALOT_")).ToArray());
    //            strSettings = "ZEALOT_RELEASE;" + strSettings;

    //            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
    //        }
    //    }

    //    Debug.Log("Setting build to Release.");
    //}

//    [MenuItem("Build/Preprocessor/Set UseAssetBundle Config", false, 20000)]
//    public static void BuildSetUseAssetBundle()
//    {
//        var matchedGroups = targetGroups.ToDictionary(x => x, null);
//        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
//        {
//            if (matchedGroups.ContainsKey(group))
//            {
//                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
//                var strArray = strSettings.Split(';').ToList();

//                if (strArray.IndexOf("USE_ASSETBUNDLE") < 0)
//                {
//                    int indexZealotConfig = strArray.FindIndex(x => x.Contains("ZEALOT_"));
//                    if (indexZealotConfig >= 0)
//                    {
//                        strArray.Insert(indexZealotConfig + 1, "USE_ASSETBUNDLE");
//                        strSettings = string.Join(";", strArray.ToArray());
//                    }

//                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
//                }
//            }
//        }

//        Debug.Log("Setting build to UseAssetBundle.");
//    }

//    [MenuItem("Build/Preprocessor/Remove UseAssetBundle Config", false, 20000)]
//    public static void BuildRemoveUseAssetBundle()
//    {
//        var matchedGroups = targetGroups.ToDictionary(x => x, null);
//        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
//        {
//            if (matchedGroups.ContainsKey(group))
//            {
//                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
//                var strArray = strSettings.Split(';').ToList();

//                int indexConfig = strArray.IndexOf("USE_ASSETBUNDLE");
//                if (indexConfig >= 0)
//                {
//                    strArray.RemoveAt(indexConfig);
//                    strSettings = string.Join(";", strArray.ToArray());

//                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
//                }
//            }
//        }

//        Debug.Log("Setting build remove UseAssetBundle.");
//    }


//    [MenuItem("Build/Preprocessor/Set UseAssetBundle Config", true)]
//    public static bool ShowSetUseAssetBundle()
//    {
//#if USE_ASSETBUNDLE
//        return false;
//#else
//        return true;
//#endif
//    }

//    [MenuItem("Build/Preprocessor/Remove UseAssetBundle Config", true)]
//    public static bool ShowRemoveUseAssetBundle()
//    {
//#if USE_ASSETBUNDLE
//        return true;
//#else
//        return false;
//#endif
//    }


    [MenuItem("Build/Set Development Config", true)]
    [MenuItem("Build/Set Release Config", true)]
    public static bool IsMasterCD()
    {
        return ZealotEditorConfig.HasMasterCDConfig();
    }


    #region ZAnalytics
    [MenuItem("Build/Preprocessor/Set ZAnalytics Config", false, 30000)]
    public static void BuildSetZAnalytics()
    {
        var matchedGroups = targetGroups.ToDictionary(x => x, null);
        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (matchedGroups.ContainsKey(group))
            {
                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                var strArray = strSettings.Split(';').ToList();

                if (strArray.IndexOf("ZANALYTICS") < 0)
                {
                    int indexZealotConfig = strArray.FindIndex(x => x.Contains("ZEALOT_"));

                    strArray.Insert(indexZealotConfig + 1, "ZANALYTICS");
                    strSettings = string.Join(";", strArray.ToArray());                

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
                }
            }
        }

        Debug.Log("Setting build to ZANALYTICS.");
    }

    [MenuItem("Build/Preprocessor/Remove ZAnalytics Config", false, 30000)]
    public static void BuildRemoveZAnalytics()
    {
        var matchedGroups = targetGroups.ToDictionary(x => x, null);
        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (matchedGroups.ContainsKey(group))
            {
                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                var strArray = strSettings.Split(';').ToList();

                int indexConfig = strArray.IndexOf("ZANALYTICS");
                if (indexConfig >= 0)
                {
                    strArray.RemoveAt(indexConfig);
                    strSettings = string.Join(";", strArray.ToArray());

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
                }
            }
        }

        Debug.Log("Setting build remove ZANALYTICS.");
    }

    [MenuItem("Build/Preprocessor/Set ZAnalytics Config", true)]
    public static bool ShowSetZAnalytics()
    {
#if ZANALYTICS
        return false;
#else
        return true;
#endif
    }

    [MenuItem("Build/Preprocessor/Remove ZAnalytics Config", true)]
    public static bool ShowRemoveZAnalytics()
    {
#if ZANALYTICS
        return true;
#else
        return false;
#endif
    }
    #endregion
    
    //    [MenuItem("Build/Preprocessor/Set UseAssetBundle Config", false, 20000)]
    //    public static void BuildSetUseAssetBundle()
    //    {
    //        var matchedGroups = targetGroups.ToDictionary(x => x, null);
    //        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
    //        {
    //            if (matchedGroups.ContainsKey(group))
    //            {
    //                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
    //                var strArray = strSettings.Split(';').ToList();

    //                if (strArray.IndexOf("USE_ASSETBUNDLE") < 0)
    //                {
    //                    int indexZealotConfig = strArray.FindIndex(x => x.Contains("ZEALOT_"));
    //                    if (indexZealotConfig >= 0)
    //                    {
    //                        strArray.Insert(indexZealotConfig + 1, "USE_ASSETBUNDLE");
    //                        strSettings = string.Join(";", strArray.ToArray());
    //                    }

    //                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
    //                }
    //            }
    //        }

    //        Debug.Log("Setting build to UseAssetBundle.");
    //    }

    //    [MenuItem("Build/Preprocessor/Remove UseAssetBundle Config", false, 20000)]
    //    public static void BuildRemoveUseAssetBundle()
    //    {
    //        var matchedGroups = targetGroups.ToDictionary(x => x, null);
    //        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
    //        {
    //            if (matchedGroups.ContainsKey(group))
    //            {
    //                string strSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
    //                var strArray = strSettings.Split(';').ToList();

    //                int indexConfig = strArray.IndexOf("USE_ASSETBUNDLE");
    //                if (indexConfig >= 0)
    //                {
    //                    strArray.RemoveAt(indexConfig);
    //                    strSettings = string.Join(";", strArray.ToArray());

    //                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, strSettings);
    //                }
    //            }
    //        }

    //        Debug.Log("Setting build remove UseAssetBundle.");
    //    }
}
