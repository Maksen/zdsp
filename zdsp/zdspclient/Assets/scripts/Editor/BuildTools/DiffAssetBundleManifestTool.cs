using AssetBundles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class DiffAssetBundleManifestTool : EditorWindow
{
    [MenuItem("Build/Builder/Diff Asset Bundles Manifest", false, 10101)]
    public static void ShowDiffAssetBundleManifestWindow()
    {
        var window = EditorWindow.GetWindowWithRect<DiffAssetBundleManifestTool>(new Rect(50, 50, 500, 150), true, "Diff Asset Bundles Manifest", true);
        window.ShowPopup();
    }

    GUIStyle commonBtnStyle;
    string oldManifestPath;
    string newManifestPath;

    void OnEnable()
    {
        if (commonBtnStyle == null)
        {
            commonBtnStyle = new GUIStyle(EditorStyles.miniButton) { margin = new RectOffset(100, 100, 30, 2) };
            commonBtnStyle.fixedHeight = 30;
        }
    }

    void OnGUI()
    {
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Old Manifest: ", GUILayout.Width(80));
        oldManifestPath = EditorGUILayout.TextField(oldManifestPath, GUILayout.Width(370));
        if(GUILayout.Button("...", GUILayout.Width(30)))
        {
            GetManifestPath(out oldManifestPath);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("New Manifest: ", GUILayout.Width(80));
        newManifestPath = EditorGUILayout.TextField(newManifestPath, GUILayout.Width(370));
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            GetManifestPath(out newManifestPath);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        if (GUILayout.Button("Diff Manifest", commonBtnStyle))
            DiffManifest();
    }

    void DiffManifest()
    {
        if (!string.IsNullOrEmpty(oldManifestPath) && !string.IsNullOrEmpty(newManifestPath))
        {
            AssetBundleManifest oldManifest = null;
            AssetBundleManifest newManifest = null;

            WWW www = new WWW("file://" + oldManifestPath);
            if (www.error == null)
            {
                var bundle = www.assetBundle;
                www.Dispose();
                www = null;

                if (bundle != null)
                {
                    oldManifest = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest");
                    bundle.Unload(false);
                }
            }

            www = new WWW("file://" + newManifestPath);
            if (www.error == null)
            {
                var bundle = www.assetBundle;
                www.Dispose();
                www = null;

                if (bundle != null)
                {
                    newManifest = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest");
                    bundle.Unload(false);
                }
            }

            if(oldManifest != null && newManifest != null)
            {
                StringBuilder sb = new StringBuilder();
                var missing = new Dictionary<string, Hash128>();
                var rebuilt = new Dictionary<string, Hash128>();
                var newbundles = new Dictionary<string, Hash128>();

                AssetBundleTools.DiffManifest(oldManifest, newManifest, out missing, out rebuilt, out newbundles);

                long patchFileSize = 0;
                string directoryName = System.IO.Path.GetDirectoryName(newManifestPath);
                string fileURL = "file://" + System.IO.Path.Combine(directoryName, "bundleinfo.json");
                www = new WWW(fileURL);
                while (!www.isDone)
                {

                }
                AssetBundleSizeInfo assetBundleSizeInfo = AssetBundleSizeInfo.Deserialize(www.text);
                if (missing.Count > 0)
                {
                    sb.AppendFormat("Removed bundles:{1}{0}{1}{1}", string.Join(Environment.NewLine, missing.Keys.ToArray()), Environment.NewLine);
                }

                if (newbundles.Count > 0)
                {
                    sb.AppendFormat("New bundles:{1}{0}{1}{1}", string.Join(Environment.NewLine, newbundles.Keys.ToArray()), Environment.NewLine);
                    foreach(var kvp in newbundles)
                    {
                        patchFileSize += assetBundleSizeInfo.GetBundleSize(kvp.Key);
                    }
                }

                sb.AppendLine("Rebuilt:");
                if (rebuilt.Count > 0)
                {
                    //sb.AppendFormat("Rebuilt:{1}{0}", string.Join(Environment.NewLine, rebuilt.Keys.ToArray()), Environment.NewLine);
                    foreach (var kvp in rebuilt)
                    {                      
                        long size = assetBundleSizeInfo.GetBundleSize(kvp.Key);
                        patchFileSize += size;
                        sb.AppendFormat("{0} size = {1}KB {2}", kvp.Key, size/1024f, Environment.NewLine);
                    }
                }
                else
                    sb.AppendLine("No rebuilds");
                
                string patchSizeMega = ((patchFileSize / 1024f) / 1024f).ToString("N2");
                sb.AppendFormat("{1}{1}Total Patch Size:{0}M", patchSizeMega, Environment.NewLine);
                var window = EditorWindow.GetWindow<DiffAssetBundleManifestResultWindow>(true, "Diff Results", true);
                window.Text = sb.ToString();
                window.ShowPopup();
            }
            else
                Debug.Log("Invalid assetbundle manifests");
        }
        else
            Debug.Log("Invalid manifest file paths");
    }

    void GetManifestPath(out string path)
    {
        path = EditorUtility.OpenFilePanel("Select AssetBundle Manifest", AssetBundleTools.AssetBundlePath, "");
    }
}