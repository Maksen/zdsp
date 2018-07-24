using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

using UnityEditor;
using UnityEngine;

public class AssetDependencyTool
{
    static string[] filterFolders = new string[]
    {
        "Assets/UI_PiLiQ/Sound/",
        "Assets/UI_PiLiQ/Textures/Atlas_ETC2_1/",
        "Assets/UI_PiLiQ/Textures/Atlas_ETC2_2/",
        "Assets/UI_PiLiQ/Textures/Atlas_ETC2_3/",
        "Assets/UI_PiLiQ/Textures/Atlas_RBG_1/",
        "Assets/UI_PiLiQ/Textures/Atlas_RBG_2/",
        "Assets/UI_PiLiQ/Textures/Atlas_RBG_3/",
        "Assets/UI_PiLiQ/Textures/emoji/",
        "Assets/UI_PiLiQ/Textures/Misc/",
        "Assets/UI_PiLiQ/BitmapFonts/",
        "Assets/UI_PiLiQ/Icons/",
        "Assets/UI_PiLiQ/Widgets/",
        "Assets/Prefabs/",
        "Assets/Shaders/Zealot_Art/UI/",
        "Assets/Shaders/effect/ParticlesAdditiveCulled.shader",
        
        //No idea why it's used. Resize to tiny textures
        "Assets/External/CNControls/NotImportant/Materials/",
        //"Assets/External/UIWidgets/SampleAssets/Sprites/NewSample"
    };

    static string[] filterExts = new string[]
    {
        ".cs",
        ".dll"
    };


    [MenuItem("Assets/Dump Asset Dependencies &X", false, 90000)]
    public static void DumpAssetDependencies()
    {
        var selections = Selection.objects;
        var alldependencies = new Dictionary<string, object>();

        for (int selIdx = 0; selIdx < selections.Length; selIdx++)
        {
            var selected = selections[selIdx];
            if (selected != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(selected);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    var files = GetFiles(assetPath).Where(x => !x.EndsWith(".meta")).ToList();
                    for (int idx = 0; idx < files.Count; idx++)
                    {
                        var dependencies = AssetDatabase.GetDependencies(files[idx]);
                        for (int i = 0; i < dependencies.Length; i++)
                        {
                            string path = dependencies[i];
                            if (!alldependencies.ContainsKey(path) && !path.Equals(files[idx])
                                && !filterExts.Any(x => path.EndsWith(x)) && !filterFolders.Any(filter => path.StartsWith(filter)))
                                alldependencies.Add(path, null);
                        }
                    }
                }
            }
        }

        if (alldependencies.Count > 0)
        {
            var ordered = alldependencies.Keys.OrderBy(x => x).ToArray();
            if (ordered.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < ordered.Length; i++)
                {
                    sb.AppendLine(ordered[i]);
                }

                try
                {
                    File.WriteAllText("assetdependencies.txt", sb.ToString());
                    Debug.LogWarningFormat("<color=blue>Exported dependencies to [{0}] </color>" + UnityEngine.Random.Range(-100000f, 100000f), "assetdependencies.txt");
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
        else
        {
            Debug.LogWarningFormat("<color=brown>Nothing exported, all safe to use </color> " + UnityEngine.Random.Range(-100000f, 100000f), "assetdependencies.txt");
        }
    }

    public static string GetSelectedPathOrFallback()
    {
        string path = "Assets";

        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }

    /// <summary>
    /// Recursively gather all files under the given path including all its subfolders.
    /// </summary>
    static IEnumerable<string> GetFiles(string path)
    {
        if (!Directory.Exists(path))
            yield return path;
        else
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }
    }
}

