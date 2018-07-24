using System;
using System.Text;

using UnityEngine;
using UnityEditor;
using System.IO;
using AssetBundles;
using System.Collections.Generic;
using System.Linq;

public class AssetBundleTools
{
    static public string AssetBundlePath
    {
        get { return "AssetBundles"; }
    }

    [MenuItem("Build/Asset Bundles/Export Asset Bundles Manifest", false, 1000)]
    public static void ListAssetBundles()
    {
        string filepath = EditorUtility.SaveFilePanel("Export AssetBundle Manifest", "", "", ".csv");
        if (filepath != string.Empty)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Bundle Name, Asset Path");
            var bundleNames = AssetDatabase.GetAllAssetBundleNames();
            foreach (string bundleName in bundleNames)
            {
                bool firstrow = true;
                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                foreach (string assetPath in assetPaths)
                {
                    if (firstrow)
                        sb.AppendLine(bundleName + ", " + assetPath);
                    else
                        sb.AppendLine(" , " + assetPath);

                    firstrow = false;
                }
            }

            try
            {
                File.WriteAllText(filepath, sb.ToString());
                Debug.LogFormat("Exported Manifest: [{0}]", filepath);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Failed exporting manifest", "Export failed. Check if file is in use", "OK");
            }
        }
    }

    [MenuItem("Build/Asset Bundles/List Asset Path", false, 1000)]
    public static void ListAssetPath()
    {
        string filepath = Application.dataPath + "/AssetBundlePath.csv";
        try
        {
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
                Debug.LogFormat("File Deleted: [{0}]", filepath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Delete manifest failed", "Delete failed. Check if file is in use", "OK");
            return;//stop here
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Bundle Name, Asset Path");

        var containersPath = string.Format("{0}/AssetContainers", Application.dataPath);
        var info = new DirectoryInfo(containersPath);
        var fileInfo = info.GetFiles();
        int progress = 0;
        foreach (var file in fileInfo)
        {
            if (file.Name.EndsWith("asset"))//container files end with asset in this folder
            {
                EditorUtility.DisplayProgressBar("Reading file", "Reading :" + file.Name, ++progress / fileInfo.Length);
                string containerPath = string.Format("Assets/AssetContainers/{0}",file.Name);
                BaseAssetContainer container = AssetDatabase.LoadAssetAtPath<BaseAssetContainer>(containerPath);
                foreach (var asset in container.ExportedAssets)
                {
                    string loadPath = string.Format("{0}/{1}", container.Name, asset.assetPath);
                    string assetLocation = AssetDatabase.GetAssetPath(asset.asset);
                    sb.AppendLine(loadPath + ", " + assetLocation);
                }
            }
        }
        EditorUtility.ClearProgressBar();

        try
        {
            File.WriteAllText(filepath, sb.ToString());
            Debug.LogFormat("Exported Manifest: [{0}]", filepath);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Failed exporting manifest", "Export failed. Check if file is in use", "OK");
        }
    }

    [MenuItem("Build/Asset Bundles/List Unused Bundles", false, 1000)]
    public static void ListUnusedAssetBundles()
    {
        var unusedBundles = AssetDatabase.GetUnusedAssetBundleNames();

        EditorUtility.DisplayDialog("Unused Bundles", string.Join("\n", unusedBundles), "OK");
    }

    [MenuItem("Build/Asset Bundles/Dump AssetBundle Dependencies", false, 3000)]
    public static void DumpAssetBundleDependencies()
    {
        if (Selection.activeObject != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var assetImporter = AssetImporter.GetAtPath(assetPath);
                if (assetImporter == null || string.IsNullOrEmpty(assetImporter.assetBundleName))
                {
                    Debug.LogFormat("selected object [{0}] does not have asset bundle", assetPath);
                    return;
                }

                string bundleName = assetImporter.assetBundleName;
                string platformName = AssetBundles.Utility.GetPlatformForAssetBundles(BuildTarget.Android);

                string folderPath = Application.dataPath + "/../" + AssetBundleTools.GetAssetBundleFolder(AssetBundlePath, BuildTarget.Android);
                string path = "file://" + Path.Combine(folderPath, platformName);

                WWW www = new WWW(path);

                if (www.error == null)
                {
                    AssetBundle bundle = www.assetBundle;
                    www.Dispose();
                    www = null;

                    if (bundle != null)
                    {
                        var manifest = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest");
                        var results = manifest.GetAllDependencies(bundleName);

                        StringBuilder sb = new StringBuilder();
                        foreach (var line in results)
                            sb.AppendLine(line);

                        Debug.LogFormat("Exporting dependencies for [{0}]", assetPath);
                        try
                        {
                            File.WriteAllText("bundledependencies.txt", sb.ToString());
                            Debug.LogFormat("Exported dependencies to [{0}]", "bundledependencies.txt");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }

                        bundle.Unload(false);
                        return;
                    }
                    else
                    {
                        Debug.Log("assetbundle manifest not found");
                        return;
                    }
                }
                else
                {
                    www.Dispose();
                    www = null;
                    return;
                }
            }
        }

        Debug.Log("No valid asset object selected");
    }

    [MenuItem("Build/Asset Bundles/Build AssetBundles from Selection", false, 1000)]
    private static void BuildBundlesFromSelection()
    {
        // Get all selected *assets*
        var assets = Selection.objects.Where(o => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o))).ToArray();

        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
        HashSet<string> processedBundles = new HashSet<string>();

        // Get asset bundle names from selection
        foreach (var o in assets)
        {
            var assetPath = AssetDatabase.GetAssetPath(o);
            var importer = AssetImporter.GetAtPath(assetPath);

            if (importer == null)
            {
                continue;
            }

            // Get asset bundle name & variant
            var assetBundleName = importer.assetBundleName;
            var assetBundleVariant = importer.assetBundleVariant;
            var assetBundleFullName = string.IsNullOrEmpty(assetBundleVariant) ? assetBundleName : assetBundleName + "." + assetBundleVariant;

            // Only process assetBundleFullName once. No need to add it again.
            if (processedBundles.Contains(assetBundleFullName))
            {
                continue;
            }

            processedBundles.Add(assetBundleFullName);

            AssetBundleBuild build = new AssetBundleBuild();

            build.assetBundleName = assetBundleName;
            build.assetBundleVariant = assetBundleVariant;
            build.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleFullName);

            assetBundleBuilds.Add(build);
        }

        BuildScript.BuildAssetBundles(assetBundleBuilds.ToArray());
    }
    static public string GetAssetBundleFolder(string targetPath, BuildTarget buildtarget)
    {
        var platformName = Utility.GetPlatformForAssetBundles(buildtarget);
        var platformPath = Path.Combine(targetPath, platformName);
        return Path.Combine(platformPath, platformName);
    }

    static public string GetOldAssetBundleFolder(string targetPath, BuildTarget buildtarget,string oldVersion)
    {
        var platformName = Utility.GetPlatformForAssetBundles(buildtarget);
        var platformPath = Path.Combine(targetPath, platformName);
        return Path.Combine(platformPath, platformName + "_" + oldVersion);
    }

    static public bool DiffManifest(AssetBundleManifest oldManifest, AssetBundleManifest newManifest, out Dictionary<string, Hash128> missing, out Dictionary<string, Hash128> rebuilt, out Dictionary<string, Hash128> newbundles)
    {
        if (oldManifest == null)
        {
            missing = rebuilt = newbundles = null;
            return false;
        }

        Dictionary<string, Hash128> oldhash = oldManifest.GetAllAssetBundles().ToDictionary<string, string, Hash128>(x => x, x => oldManifest.GetAssetBundleHash(x));
        Dictionary<string, Hash128> newhash = newManifest.GetAllAssetBundles().ToDictionary<string, string, Hash128>(x => x, x => newManifest.GetAssetBundleHash(x));

        missing = oldhash.Where(kvp => !newhash.ContainsKey(kvp.Key)).ToDictionary(x => x.Key, x => x.Value);
        rebuilt = newhash.Where(kvp => oldhash.ContainsKey(kvp.Key) && !oldhash[kvp.Key].Equals(kvp.Value)).ToDictionary(x => x.Key, x => x.Value);
        newbundles = newhash.Where(kvp => !oldhash.ContainsKey(kvp.Key)).ToDictionary(x => x.Key, x => x.Value);

        return true;
    }
}