using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using UnityEditorInternal;

public class PrefabContextMenu : EditorWindow
{
    [MenuItem("Assets/Reload FBX Prefab", false, 20000)]
    static void ReloadFBXPrefab()
    {
        foreach (var guid in Selection.assetGUIDs)
        {
            string assetFilePath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadMainAssetAtPath(assetFilePath) as GameObject;
            SkinnedMeshRenderer render;
            if (assetFilePath.EndsWith(".prefab") && prefab && (render = prefab.GetComponentInChildren<SkinnedMeshRenderer>()))
            {
                string fbxFileName = render.gameObject.name;
                string[] guids = AssetDatabase.FindAssets(fbxFileName);
                string fbxFilePath = "";
                foreach (var meshGuid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(meshGuid);
                    if (path.EndsWith(fbxFileName + ".fbx"))
                    {
                        fbxFilePath = path;
                        break;
                    }
                }
                if (fbxFilePath.Length == 0)
                {
                    Debug.Log("Missing file " + fbxFileName + ".fbx, reload failed.");
                    continue;
                }

                var prototypePrefab = (GameObject)AssetDatabase.LoadAssetAtPath(fbxFilePath, typeof(GameObject));
                GameObject clonePrefab = Instantiate(prototypePrefab);
                CopyComponent(prefab, clonePrefab);
                GameObject origRoot = prefab.transform.Find("root").gameObject;
                GameObject newRoot = clonePrefab.transform.Find("root").gameObject;
                CloneChildGameObject(origRoot, newRoot);
                Debug.Log("Reload " + prefab.name + " success.");
                PrefabUtility.ReplacePrefab(clonePrefab, prefab, ReplacePrefabOptions.ReplaceNameBased);
                AssetDatabase.Refresh();
                GameObject.DestroyImmediate(clonePrefab);
            }
        }
    }

    static private void CopyComponent(GameObject src, GameObject dest)
    {
        Component[] coms = src.GetComponents<Component>();
        foreach (var com in coms)
        {
            var destCom = dest.GetComponent(com.GetType());
            if (!destCom)
                destCom = dest.AddComponent(com.GetType());
            
            ComponentUtility.CopyComponent(com);
            ComponentUtility.PasteComponentValues(destCom);
        }
    }

    static private void CloneChildGameObject(GameObject src, GameObject dest)
    {
        CopyComponent(src, dest);
        for (int i = 0; i < src.transform.childCount; i++)
        {
            GameObject srcChildGameObject = src.transform.GetChild(i).gameObject;
            var transform = dest.transform.Find(srcChildGameObject.name);
            GameObject destChildGameObject;
            if (transform)
                destChildGameObject = transform.gameObject;
            else
            {
                destChildGameObject = new GameObject(srcChildGameObject.name);
                destChildGameObject.transform.parent = dest.transform;
            }
            CopyComponent(srcChildGameObject, destChildGameObject);
            CloneChildGameObject(srcChildGameObject, destChildGameObject);
        }
    }

}
