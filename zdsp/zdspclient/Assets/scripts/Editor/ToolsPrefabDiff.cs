#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ToolsPrefabDiff : EditorWindow
{
    GUIContent prevContent;
    Vector2 scrollPosition;

    public IEnumerable<PropertyModification> Values { get; set; }
    public GameObject parentSelected { get; set; }
    public IEnumerable<PropertyModification>[] allValues { get; set; }
    public GameObject[] allobj { get; set; }
    void OnEnable()
    {
        prevContent = titleContent;
    }

    public void OnGUI2()
    {
        EditorGUILayout.BeginVertical();

        GUILayout.Space(5);

        GUILayout.Label("GameObject:" + parentSelected.name, GUILayout.Height(50));

        GUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false);

        IEnumerable<PropertyModification> filterList = Values
                                                        .GroupBy(x => ((x.target as Component) != null) ? RootPath((x.target as Component).transform) + (x.target as Component).GetType().Name : "")
                                                        .Select(group => group.First());

        EditorGUILayout.Separator();
        foreach (var v in filterList)
        {
            //v.target is the prefab~ component
            //parentSelect is the object in scene!
            var comp = v.target as Component;
            if (comp != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(v.target.GetType().Name + " : " + RootPath((v.target as Component).transform));

                if (GUILayout.Button("Revert", GUILayout.Width(100)))
                {
                    //Button Pressed

                    Transform child;//in scene
                    if (parentSelected.name == comp.gameObject.name)
                        child = parentSelected.transform;
                    else
                        child = parentSelected.transform.Find(RootPath(comp.transform));

                    var success = PrefabUtility.ResetToPrefabState(child.GetComponent(v.target.GetType().Name));
                    if (success)
                    {
                        //re-get values
                        Values = PrefabUtility.GetPropertyModifications(parentSelected);
                    }
                }

                if (GUILayout.Button("Select", GUILayout.Width(100)))
                {
                    Transform child;//in scene
                    if (parentSelected.name == comp.gameObject.name)
                        child = parentSelected.transform;
                    else
                        child = parentSelected.transform.Find(RootPath(comp.transform));

                    Selection.activeGameObject = child.gameObject;
                }

                EditorGUILayout.EndHorizontal();

            }
        }
        EditorGUILayout.Separator();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        //Bottom
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Revert All Transform"))
        {
            foreach (var v in filterList)
            {
                var comp = v.target as Component;
                if (comp != null)
                {
                    if (comp.GetType().Name == "RectTransform" || comp.GetType().Name == "Transform")
                    {
                        Transform child;//in scene
                        if (parentSelected.name == comp.gameObject.name)
                            child = parentSelected.transform;
                        else
                            child = parentSelected.transform.Find(RootPath(comp.transform));

                        PrefabUtility.ResetToPrefabState(child.GetComponent(v.target.GetType().Name));

                    }
                }
            }

            //re-get values
            Values = PrefabUtility.GetPropertyModifications(parentSelected);
        }
        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;


    }

    public void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false);
        for (int i=0;i<allobj.Length;i++)
        {
            var obj = allobj[i];
            var value = allValues[i];
            EditorGUILayout.BeginVertical();

            GUILayout.Space(1);

            GUILayout.Label("GameObject:" + obj.name, GUILayout.Height(20));
            IEnumerable<PropertyModification> filterList = value
                                                            .GroupBy(x => ((x.target as Component) != null) ? RootPath((x.target as Component).transform) + (x.target as Component).GetType().Name : "")
                                                            .Select(group => group.First());

            EditorGUILayout.Separator();
            foreach (var v in filterList)
            {
                //v.target is the prefab~ component
                //parentSelect is the object in scene!
                var comp = v.target as Component;
                if (comp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(v.target.GetType().Name + " : " + RootPath((v.target as Component).transform));

                    if (GUILayout.Button("Revert", GUILayout.Width(100)))
                    {
                        //Button Pressed

                        Transform child;//in scene
                        if (obj.name == comp.gameObject.name)
                            child = obj.transform;
                        else
                            child = obj.transform.Find(RootPath(comp.transform));

                        var success = PrefabUtility.ResetToPrefabState(child.GetComponent(v.target.GetType().Name));
                        if (success)
                        {
                            //re-get values
                            value = PrefabUtility.GetPropertyModifications(obj);
                        }
                    }

                    if (GUILayout.Button("Select", GUILayout.Width(100)))
                    {
                        Transform child;//in scene
                        if (obj.name == comp.gameObject.name)
                            child = obj.transform;
                        else
                            child = obj.transform.Find(RootPath(comp.transform));

                        Selection.activeGameObject = child.gameObject;
                    }

                    EditorGUILayout.EndHorizontal();

                }
            }
            EditorGUILayout.Separator();

            
            EditorGUILayout.EndVertical();

            ////Bottom
            //EditorGUILayout.BeginHorizontal();
            //if (GUILayout.Button("Revert All Transform"))
            //{
            //    foreach (var v in filterList)
            //    {
            //        var comp = v.target as Component;
            //        if (comp != null)
            //        {
            //            if (comp.GetType().Name == "RectTransform" || comp.GetType().Name == "Transform")
            //            {
            //                Transform child;//in scene
            //                if (obj.name == comp.gameObject.name)
            //                    child = obj.transform;
            //                else
            //                    child = obj.transform.Find(RootPath(comp.transform));

            //                PrefabUtility.ResetToPrefabState(child.GetComponent(v.target.GetType().Name));

            //            }
            //        }
            //    }

            //    //re-get values
            //    value = PrefabUtility.GetPropertyModifications(obj);
            //}
            //EditorGUILayout.EndHorizontal();

        }
        revertallprefab();
        EditorGUILayout.EndScrollView();
        GUI.enabled = true;


    }

    void revertallprefab()
    {
        if (GUILayout.Button("Revert All Transform"))
        {

            for (int i = 0; i < allobj.Length; i++)
            {
                var obj = allobj[i];
                var value = allValues[i];

                IEnumerable<PropertyModification> filterList = value
                                                                .GroupBy(x => ((x.target as Component) != null) ? RootPath((x.target as Component).transform) + (x.target as Component).GetType().Name : "")
                                                                .Select(group => group.First());
                EditorGUILayout.Separator();


                ////Bottom
                EditorGUILayout.BeginHorizontal();

                foreach (var v in filterList)
                {
                    var comp = v.target as Component;
                    if (comp != null)
                    {
                        if (comp.GetType().Name == "RectTransform" || comp.GetType().Name == "Transform")
                        {
                            Transform child;//in scene
                            if (obj.name == comp.gameObject.name)
                                child = obj.transform;
                            else
                                child = obj.transform.Find(RootPath(comp.transform));
                            PrefabUtility.ResetToPrefabState(child.GetComponent(v.target.GetType().Name));

                        }
                    }
                }

                //re-get values
                value = PrefabUtility.GetPropertyModifications(obj);

                EditorGUILayout.EndHorizontal();
            }
        }
    }
    string RootPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null && t.root != t.parent)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

}

#endif