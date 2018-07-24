using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

public class UIEditorTools
{
    /// <summary>
    /// Creates a prefab based on current selected gameobject in hierarchy.
    /// </summary>
    public static void CreateUIPrefab()
    {
        var clone = GameObject.Instantiate(Selection.activeGameObject);
        clone.name = Selection.activeGameObject.name;
        var selected = clone;

        if (selected != null)
        {
            string currentScene = EditorSceneManager.GetActiveScene().path;
            string sceneName = Path.GetFileNameWithoutExtension(currentScene);
            string currentFolder = Path.GetDirectoryName(currentScene).Replace('\\', '/'); ;

            string targetFolder = string.Empty;
            string[] subfolders = AssetDatabase.GetSubFolders(currentFolder);
            foreach(string subfolder in subfolders)
            {
                if (Path.GetFileName(subfolder).StartsWith("P_"))
                {
                    targetFolder = subfolder;
                    break;
                }
            }

            if(targetFolder == string.Empty)
            {
                Debug.Log("Invalid folder or no folders found");
                return;
            }

            string targetPath = targetFolder + "/" + selected.name + ".prefab";

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
            GameObject.DestroyImmediate(clone);
        }
    }

    [MenuItem("Tools/Toggle Inspector Lock %l")] // Ctrl + L
    static void ToggleInspectorLock()
    {
        ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }
}
