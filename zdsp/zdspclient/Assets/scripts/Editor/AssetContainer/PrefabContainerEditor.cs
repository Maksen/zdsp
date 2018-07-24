using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrefabContainer))]
[CanEditMultipleObjects]
class PrefabContainerEditor : BaseAssetContainerEditor
{
    private bool addPrefabSlots = false;
    private int _newPrefabSlots;

    const string prefabExt = ".prefab";

    public override void OnInspectorGUI()
    {
        DrawContainerType();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Prefabs ({0})", prefabExt), extStyle);
        EditorGUILayout.EndHorizontal();

        serializedObject.Update();

        PrefabContainer assetContainer = (PrefabContainer)target;

        SerializedProperty list = serializedObject.FindProperty("PrefabList");

        DrawAddSubfolder();

        EditorGUI.BeginChangeCheck();

        DrawContainerPropertiesToggle();

        #region Prefab Box
        EditorGUILayout.BeginVertical(areaStyle);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Add Folder", addFolderTooltip), btnStyle))
        {
            OnAddFolder<GameObject>(prefabExt);
        }
        if (GUILayout.Button(new GUIContent("Reorganise", reorganiseTooltip), btnStyle))
        {
            OnReorganise<GameObject>();
        }
        if (GUILayout.Button("Add Slots", btnStyle))
        {
            addPrefabSlots = !addPrefabSlots;
        }
        EditorGUILayout.EndHorizontal();

        if (addPrefabSlots)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("AddPrefabSlots");
            _newPrefabSlots = EditorGUILayout.IntField("Slots to add: ", _newPrefabSlots);
            if (IsKeyPressed("AddPrefabSlots", KeyCode.Return))
            {
                if (_newPrefabSlots > 0)
                {
                    OnAddSlots<GameObject>(_newPrefabSlots);
                }
                _newPrefabSlots = 0;
                addPrefabSlots = false;
            }
            EditorGUILayout.EndHorizontal();
        }

        Show(list);
        EditorGUILayout.EndVertical();
        #endregion

        if (EditorGUI.EndChangeCheck())
        {
            if (serializedObject.ApplyModifiedProperties())
            {
                OnApplyModifiedProperties();
            }           
        }

        DrawCommonGUI();
    }

    protected override void OnExportPackage()
    {
        PrefabContainer prefabContaier = assetContainer as PrefabContainer;
        AssetDatabase.SaveAssets();

        Dictionary<string, object> filemap = new Dictionary<string, object>();
        
        string assetpath = AssetDatabase.GetAssetPath(target);        
        string[] dependencies = AssetDatabase.GetDependencies(new string[] { assetpath });

        foreach(string dependency in dependencies)
        {
            if(!filemap.ContainsKey(dependency) && !dependency.EndsWith(".cs"))
            {
                filemap.Add(dependency, null);
            }
        }

        foreach (GameObject prefab in prefabContaier.PrefabList)
        {
            if (prefab != null)
            {
                string prefabPath = AssetDatabase.GetAssetPath(prefab);
                string[] prefabdep = AssetDatabase.GetDependencies(new string[] { prefabPath });

                foreach (string dependency in prefabdep)
                {
                    if (!filemap.ContainsKey(dependency) && !dependency.EndsWith(".cs"))
                    {
                        filemap.Add(dependency, null);
                    }
                }
            }
        }

        string[] filetoexport = filemap.Keys.Where(x => !x.EndsWith(".cs")).ToArray();

        string savepath = EditorUtility.SaveFilePanel("Export Container Package", "", "", "unitypackage");
        if (!savepath.Equals(String.Empty))
        {
            AssetDatabase.ExportPackage(filetoexport, savepath);
            Debug.LogFormat("Exported Container Package to [{0}]", savepath);
        }

    }
}