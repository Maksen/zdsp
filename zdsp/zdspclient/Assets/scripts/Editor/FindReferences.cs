using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Text;

public class FindPro
{
    [MenuItem("CONTEXT/Component/Find References/Find references in all scenes")]
    private static void FindReferences(MenuCommand data)
    {
        Object context = data.context;
        if (context)
        {
            var comp = context as Component;
            if (comp)
                FindReferencesTo(comp);
        }
    }

    [MenuItem("CONTEXT/Component/Find References/Find references in UI scenes")]
    private static void FindReferencesInUI(MenuCommand data)
    {
        Object context = data.context;
        if (context)
        {
            var comp = context as Component;
            if (comp)
                FindReferencesTo(comp, "Assets/UI");
        }
    }

    [MenuItem("Assets/Find References/Find references in all scenes")]
    private static void FindReferencesToAsset(MenuCommand data)
    {
        var selected = Selection.activeObject;
        if (selected)
            FindReferencesTo(selected);
    }

    [MenuItem("Assets/Find References/Find references in UI scenes")]
    private static void FindReferencesToAssetInUI(MenuCommand data)
    {
        var selected = Selection.activeObject;
        if (selected)
            FindReferencesTo(selected, "Assets/UI");
    }

    private static void FindReferencesTo(Object to, string filter = "")
    {
        var references = new Dictionary<string, List<string>>();
        //var referencedBy = new List<Object>();
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            string[] scenes = SceneManager.GetAllScenes().Select(x => x.path).ToArray();

            var allAssets = AssetDatabase.GetAllAssetPaths();
            var scenePaths = allAssets.Where(x => Path.GetExtension(x) == ".unity").ToArray();

            if (filter.Length > 0)
                scenePaths = scenePaths.Where(x => x.StartsWith(filter)).ToArray();

            float count = 0;
            float total = scenePaths.Length;
            EditorUtility.DisplayProgressBar("Finding References", "", 0f);
            foreach (string scenepath in scenePaths)
            {
                count++;
                if (!EditorUtility.DisplayCancelableProgressBar("Finding References", "find in scene " + scenepath, count / total))
                {
                    try
                    {
                        EditorSceneManager.OpenScene(scenepath);

                        var sceneObjects = AllSceneObjects();
                        foreach (var sceneobj in sceneObjects)
                        {
                            var go = sceneobj.gameObject;

                            if (PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
                            {
                                if (PrefabUtility.GetPrefabParent(go) == to)
                                {
                                    //Debug.Log(string.Format("[{2}] referenced by {0}, {1}", go.name, go.GetType(), scenepath), go);
                                    //referencedBy.Add(go);

                                    if (!references.ContainsKey(scenepath))
                                        references.Add(scenepath, new List<string>());
                                    references[scenepath].Add("[" + go.name + "] : " + go.GetType());
                                }
                            }

                            var components = go.GetComponents<Component>();
                            for (int i = 0; i < components.Length; i++)
                            {
                                var c = components[i];
                                if (!c) continue;

                                var so = new SerializedObject(c);
                                var sp = so.GetIterator();

                                while (sp.NextVisible(true))
                                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                                    {
                                        if (sp.objectReferenceValue == to)
                                        {
                                            //Debug.Log(string.Format("[{2}] referenced by {0}, {1}", c.name, c.GetType(), scenepath), c);
                                            //referencedBy.Add(c.gameObject);

                                            if (!references.ContainsKey(scenepath))
                                                references.Add(scenepath, new List<string>());
                                            references[scenepath].Add("[" + c.name + "] : " + c.GetType());
                                        }
                                    }
                            }
                        }
                    }
                    catch(System.Exception ex)
                    {
                        //error when finding reference in scene
                    }
                }
                else
                    break;
            }

            bool firstscene = true;
            foreach (string scenepath in scenes)
            {
                if (scenepath != string.Empty)
                {
                    EditorSceneManager.OpenScene(scenepath, firstscene ? OpenSceneMode.Single : OpenSceneMode.Additive);
                    firstscene = false;
                }
            }
        }

        EditorUtility.ClearProgressBar();

        if (references.Count > 0)
        {
            //Selection.objects = referencedBy.ToArray();

            StringBuilder sb = new StringBuilder();
            foreach(var kvp in references)
            {
                sb.AppendLine("Scene [" + kvp.Key + "]");
                foreach (string entry in kvp.Value)
                {
                    sb.AppendLine("\t" + entry);
                }
                sb.AppendLine();
            }

            //EditorUtility.DisplayDialog("Found References", sb.ToString(), "OK");

            var window = EditorWindow.GetWindow<FoundReferencesWindow>(true, "Found References", true);
            window.Text = sb.ToString();
            window.ShowPopup();

            Debug.Log(sb.ToString());
        }
        else Debug.Log("no references in scene");
    }

    private static IEnumerable<GameObject> SceneRoots()
    {
        var prop = new HierarchyProperty(HierarchyType.GameObjects);
        var expanded = new int[0];
        while (prop.Next(expanded))
        {
            yield return prop.pptrValue as GameObject;
        }
    }

    private static IEnumerable<Transform> AllSceneObjects()
    {
        var queue = new Queue<Transform>();

        foreach (var root in SceneRoots())
        {
            var tf = root.transform;
            yield return tf;
            queue.Enqueue(tf);
        }

        while (queue.Count > 0)
        {
            foreach (Transform child in queue.Dequeue())
            {
                yield return child;
                queue.Enqueue(child);
            }
        }
    }
}

public class FoundReferencesWindow : EditorWindow
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