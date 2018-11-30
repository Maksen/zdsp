using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

public class ArtTools
{
    /// <summary>
    /// Creates a prefab based on current selected gameobject in hierarchy.
    /// </summary>
    [MenuItem("GameObject/!!! Art - Make Prefab", false, 0)]
    public static void CreateModelPrefab()
    {
        var selected = Selection.activeGameObject;
        if (selected != null)
        {
            string currentScene = EditorSceneManager.GetActiveScene().path;
            string currentFolder = "Assets";
            if (!string.IsNullOrEmpty(currentScene))
            {
                string sceneName = Path.GetFileNameWithoutExtension(currentScene);
                currentFolder = Path.GetDirectoryName(currentScene).Replace('\\', '/');
            }

            string targetFolder = EditorUtility.OpenFolderPanel("Select Folder", currentFolder, "");
            if (!string.IsNullOrEmpty(targetFolder))
            {
                int index = targetFolder.IndexOf("Assets/");
                string relativePath = index != -1 ? targetFolder.Substring(index) : "Assets";
                string targetPath = relativePath + "/" + selected.name + ".prefab";

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
                if (prefab == null)
                {
                    prefab = PrefabUtility.CreatePrefab(targetPath, selected);
                    Debug.Log("Created new prefab from gameobject: " + targetPath);
                }
                //if gameobject is already a prefab
                else if (PrefabUtility.GetPrefabType(selected) != PrefabType.None && PrefabUtility.FindPrefabRoot(selected) != null)
                {
                    prefab = PrefabUtility.ReplacePrefab(selected, prefab, ReplacePrefabOptions.ConnectToPrefab);
                    Debug.Log("Replaced prefab of prefab: " + targetPath);
                }
                else
                {
                    prefab = PrefabUtility.ReplacePrefab(selected, prefab, ReplacePrefabOptions.ReplaceNameBased);
                    Debug.Log("Replaced prefab of gameobject: " + targetPath);
                }

                Selection.activeObject = prefab;

                AssetDatabase.SaveAssets();
            }
            else
                Debug.Log("Invalid folder or no folders found");
        }
    }
}
