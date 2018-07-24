using AssetBundles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundleWindow : EditorWindow
{
    GUIContent prevContent;

    bool isBuilding = false;

    bool startBuild = false;
    BuildTarget mBuildTarget = BuildTarget.Android;

    AssetBundleManifest oldManifest;
    AssetBundleManifest newManifest;

    bool generateSizeInfo = true;

    public string TargetPath { get; set; }

    GUIStyle boldstyle;

    void OnEnable()
    {
        prevContent = titleContent;

        if (boldstyle == null)
        {
            boldstyle = new GUIStyle(EditorStyles.textField) { fontStyle = FontStyle.Bold };
        }
    }

    void OnInspectorUpdate()
    {
        if (startBuild)
        {
            startBuild = false;
            LoadOldManifest(mBuildTarget);
        }
    }

    void StartBuild(BuildTarget target)
    {
        isBuilding = true;
        mBuildTarget = target;
        startBuild = true;
    }

    public void OnGUI()
    {
        if (isBuilding)
        {
            titleContent = new GUIContent("Building. Hold On....");
        }
        else
            titleContent = prevContent;

        EditorGUILayout.LabelField("Build Asset Bundles");
        generateSizeInfo = EditorGUILayout.ToggleLeft("Generate AssetBundle Size File (.json)", generateSizeInfo);

        EditorGUILayout.Separator();

        GUI.enabled = !isBuilding;
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            EditorGUILayout.BeginVertical("Box");

            if (GUILayout.Button("BUILD Android Asset Bundle"))
            {
                StartBuild(BuildTarget.Android);
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("CLEAN UNUSED Android AssetBundles"))
            {
                CleanUnusedAssetBundles(BuildTarget.Android);
            }
            if (GUILayout.Button("CLEAN Android AssetBundles"))
            {
                CleanAssetBundles(BuildTarget.Android);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
        {
            EditorGUILayout.BeginVertical("Box");
            if (GUILayout.Button("BUILD Windows64 Asset Bundle"))
            {
                StartBuild(BuildTarget.StandaloneWindows64);
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("CLEAN UNUSED Windows64 AssetBundles"))
            {
                CleanUnusedAssetBundles(BuildTarget.StandaloneWindows64);
            }
            if (GUILayout.Button("CLEAN Windows64 AssetBundles"))
            {
                CleanAssetBundles(BuildTarget.StandaloneWindows64);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            EditorGUILayout.BeginVertical("Box");

            if (GUILayout.Button("BUILD iOS Asset Bundle"))
            {
                StartBuild(BuildTarget.iOS);
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("CLEAN UNUSED iOS AssetBundles"))
            {
                CleanUnusedAssetBundles(BuildTarget.iOS);
            }
            if (GUILayout.Button("CLEAN iOS AssetBundles"))
            {
                CleanAssetBundles(BuildTarget.iOS);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
        }


        EditorGUILayout.BeginVertical("Box");
        if (GUILayout.Button("Open Assetbundle Folder"))
        {
            OpenInFileBrowser.Open(Application.dataPath + "/../" + TargetPath);
        }

        if (GUILayout.Button("Show Manifest Diff"))
        {
            ShowManifestDiff();
        }

        if (GUILayout.Button("Export Manifest Diff"))
        {
            ExportManifestDiff();
        }
        EditorGUILayout.EndVertical();
        GUI.enabled = true;
    }

    public void LoadOldManifest()
    {
        LoadOldManifest(BuildTarget.Android);
    }
    public void LoadOldManifest(BuildTarget buildtarget)
    {
        string platformName = Utility.GetPlatformForAssetBundles(buildtarget);

        string folderPath = "";
        folderPath = Application.dataPath + "/../" + AssetBundleTools.GetAssetBundleFolder(TargetPath, buildtarget);

        string path = "file://" + Path.Combine(folderPath, platformName);

        bool canBuild = false;
        AssetBundle bundle = null;

        if (Directory.Exists(folderPath))
        {
            WWW www = new WWW(path);
            if (www.error == null)
            {
                bundle = www.assetBundle;
                www.Dispose();
                www = null;

                if (bundle != null)
                {
                    oldManifest = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest");
                }

                canBuild = true;
            }
        }
        else
            canBuild = true;

        if (canBuild)
        {
            BuildAssetBundles(buildtarget);

            if (bundle != null)
                bundle.Unload(false);
        }
        isBuilding = false;
    }

    void BuildAssetBundles(BuildTarget buildtarget)
    {

        var failedNames = CheckNames();
        if (!string.IsNullOrEmpty(failedNames))
        {
            if (!EditorUtility.DisplayDialog("Build Failed", string.Format("The following names have spaces\n{0}\n\nCONTINUE BUILD?", failedNames), "OK", "Cancel"))
                return;
        }

        string outputPath = AssetBundleTools.GetAssetBundleFolder(TargetPath, buildtarget);
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        newManifest = BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, buildtarget);

        if (newManifest != null)
        {
            if (generateSizeInfo)
                GenerateAssetBundleSizeInfo(newManifest, outputPath, buildtarget);

            RenameAssetBundleManifest(outputPath, buildtarget);

            Debug.Log("Done Building AssetBundles.");
        }
        else
        {
            Debug.LogError("Error Building AssetBundles.");
        }
    }

    void CleanAssetBundles(BuildTarget buildtarget)
    {
        string outputPath = Path.Combine(TargetPath, Utility.GetPlatformForAssetBundles(buildtarget));
        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }

        Directory.CreateDirectory(outputPath);

        Debug.LogFormat("Deleted Assetbundle [{0}]", outputPath);
    }

    void CleanUnusedAssetBundles(BuildTarget buildtarget)
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
        List<string> removed = new List<string>();

        string outputPath = Path.Combine(TargetPath, Utility.GetPlatformForAssetBundles(buildtarget));
        if (Directory.Exists(outputPath))
        {
            string platformManifest = Utility.GetPlatformForAssetBundles(buildtarget);
            var bundleNames = AssetDatabase.GetAllAssetBundleNames().ToDictionary(x => x, x => true);
            string[] files = Directory.GetFiles(outputPath, "*.*", SearchOption.AllDirectories);
            foreach (string filepath in files)
            {
                if (Path.GetExtension(filepath) == ".manifest")
                    continue;

                string path = filepath.Replace(outputPath + "\\", "").Replace("\\", "/");
                if (path == platformManifest)
                    continue;

                if (!bundleNames.ContainsKey(path))
                    removed.Add(filepath);
            }

            foreach (var path in removed)
            {
                FileUtil.DeleteFileOrDirectory(path);
                FileUtil.DeleteFileOrDirectory(path + ".manifest");
            }
        }

        if (removed.Count > 0)
        {
            EditorUtility.DisplayDialog("Deleted Asset Bundles", string.Join(Environment.NewLine, removed.ToArray()), "Ok");
        }
        else
            EditorUtility.DisplayDialog("Deleted Asset Bundles", "Nothing deleted", "Ok");
    }

    void ShowManifestDiff()
    {
        StringBuilder sb = new StringBuilder();

        var missing = new Dictionary<string, Hash128>();
        var rebuilt = new Dictionary<string, Hash128>();
        var newbundles = new Dictionary<string, Hash128>();

        if (DiffManifest(out missing, out rebuilt, out newbundles))
        {
            if (missing.Count > 0)
            {
                sb.AppendFormat("Removed bundles:{1}{0}{1}{1}", string.Join(Environment.NewLine, missing.Keys.ToArray()), Environment.NewLine);
            }

            if (newbundles.Count > 0)
            {
                sb.AppendFormat("New bundles:{1}{0}{1}{1}", string.Join(Environment.NewLine, newbundles.Keys.ToArray()), Environment.NewLine);
            }

            if (rebuilt.Count > 0)
            {
                sb.AppendFormat("Rebuilt:{1}{0}", string.Join(Environment.NewLine, rebuilt.Keys.ToArray()), Environment.NewLine);
            }
            else
                sb.AppendLine("No rebuilds");

            var window = EditorWindow.GetWindow<DiffAssetBundleManifestResultWindow>(true, "Build Results", true);
            window.Text = sb.ToString();
            window.ShowPopup();
        }
        else
        {
            EditorUtility.DisplayDialog("Build results", "No Diff. All new AssetBundles", "OK");
        }
    }

    void ExportManifestDiff()
    {
        string savepath = EditorUtility.SaveFilePanel("Export Manifest Diff", "", "", "txt");
        if (savepath != string.Empty)
        {
            StringBuilder sb = new StringBuilder();

            var missing = new Dictionary<string, Hash128>();
            var rebuilt = new Dictionary<string, Hash128>();
            var newbundles = new Dictionary<string, Hash128>();

            if (DiffManifest(out missing, out rebuilt, out newbundles))
            {
                if (missing.Count > 0)
                {
                    sb.AppendFormat("Removed bundles:{1}{0}{1}{1}", string.Join(Environment.NewLine, missing.Keys.ToArray()), Environment.NewLine);
                }

                if (newbundles.Count > 0)
                {
                    sb.AppendFormat("New bundles:{1}{0}{1}{1}", string.Join(Environment.NewLine, newbundles.Keys.ToArray()), Environment.NewLine);
                }

                if (rebuilt.Count > 0)
                {
                    sb.AppendFormat("Rebuilt:{1}{0}", string.Join(Environment.NewLine, rebuilt.Keys.ToArray()), Environment.NewLine);
                }
                else
                    sb.AppendLine("No rebuilds");

                try
                {
                    File.WriteAllText(savepath, sb.ToString());
                    Debug.LogFormat("Exported Manifest Diff: [{0}]", savepath);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    EditorUtility.DisplayDialog("Failed Exporting", "Export failed. Check if file is in use", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Build results", "No Diff. All new AssetBundles", "OK");
            }
        }
    }

    bool DiffManifest(out Dictionary<string, Hash128> missing, out Dictionary<string, Hash128> rebuilt, out Dictionary<string, Hash128> newbundles)
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

    string CheckNames()
    {
        StringBuilder sb = new StringBuilder();
        var bundleNames = AssetDatabase.GetAllAssetBundleNames();
        foreach (var name in bundleNames)
        {
            if (name.Contains(" "))
            {
                sb.AppendLine(name);
            }
        }

        return sb.ToString();
    }

    void RenameAssetBundleManifest(string folderPath, BuildTarget buildtarget)
    {
        var platformName = Utility.GetPlatformForAssetBundles(buildtarget);
        string bundleVersionName = platformName;
        string source = Path.Combine(folderPath, bundleVersionName);
        string sourceManifest = source + ".manifest";

        string dest = Path.Combine(folderPath, platformName);
        string destManifest = dest + ".manifest";

        try
        {
            if (string.Compare(sourceManifest, destManifest) != 0)
            {
                FileUtil.ReplaceFile(source, dest);
                FileUtil.ReplaceFile(sourceManifest, destManifest);

                FileUtil.DeleteFileOrDirectory(source);
                FileUtil.DeleteFileOrDirectory(sourceManifest);
            }

        }
        catch (Exception ex)
        {
            Debug.LogError("[RenameAssetBundleManifest] failed : " + ex.ToString());

        }
    }

    void GenerateAssetBundleSizeInfo(AssetBundleManifest manifest, string outputPath, BuildTarget buildtarget)
    {
        var platformName = Utility.GetPlatformForAssetBundles(buildtarget);

        var bundleNames = manifest.GetAllAssetBundles();
        var assetbundleSizeInfo = new AssetBundleSizeInfo();

        for (int i = 0; i < bundleNames.Length; i++)
        {
            string bundleName = bundleNames[i];
            var path = Path.Combine(outputPath, bundleName);
            if (File.Exists(path))
            {
                var length = new System.IO.FileInfo(path).Length;
                assetbundleSizeInfo.AddBundleSize(bundleName, length);
            }
        }

        string json = AssetBundleSizeInfo.Serialize(assetbundleSizeInfo);

        try
        {
            var filepath = Path.Combine(outputPath, AssetBundleSizeInfo.FileName);
            File.WriteAllText(filepath, json);
        }
        catch (Exception ex)
        {
            Debug.LogFormat("[GenerateAssetBundleSizeInfo] error: {0}", ex.ToString());
        }
    }
}

public class DiffAssetBundleManifestResultWindow : EditorWindow
{
    public string Text;
    Vector2 scrollPos = Vector2.zero;

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height), GUILayout.Width(position.width));
        EditorGUILayout.TextArea(Text);
        EditorGUILayout.EndScrollView();
    }
}