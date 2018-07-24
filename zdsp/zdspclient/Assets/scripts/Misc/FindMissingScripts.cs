using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class FindMissingScripts
#if UNITY_EDITOR
    : EditorWindow
#endif
{
#if UNITY_EDITOR
    [MenuItem("Window/FindMissingScripts")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FindMissingScripts));
    }


    public void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts in selected prefabs"))
        {
            FindInSelected();
        }
    }
    private static void FindInSelected()
    {
        GameObject[] go = Selection.gameObjects;
        int go_count = 0, components_count = 0, missing_count = 0;
        foreach (GameObject g in go)
        {
            Transform[] allChildren = g.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                go_count++;
                Component[] components = child.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    components_count++;
                    if (components[i] == null)
                    {
                        missing_count++;
                        string s = child.name;
                        Transform t = child.transform;
                        while (t.parent != null)
                        {
                            s = t.parent.name + "/" + s;
                            t = t.parent;
                        }
                        Debug.Log(s + " has an empty script attached in position: " + i, child);
                    }
                }
            }
        }

        Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
    }

#endif
}