using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

using UnityEditor;
using UnityEngine;

public abstract class BaseAssetContainerEditor : Editor
{
    protected const string addFolderTooltip = "Add files inside selected folder, excluding subfolders";
    protected const string reorganiseTooltip = "Removes empty asset and duplicates from list";
    protected const string exportManifestTooltip = "Exports .csv containing the list of assets and their respective paths";
    protected const string exportPackageTooltip = "AutoSave Assets and Export container and dependencies into unity package";

    protected GUIStyle btnStyle;
    protected GUIStyle commonBtnStyle;
    protected GUIStyle areaStyle;
    protected GUIStyle extStyle;
    protected GUIStyle nameStyle;

    protected IAssetContainer assetContainer;

    bool addSubfolder = true;
    const string addSubfolderTooltip = "True to include all files in subfolders when using Add Folder";

    protected virtual void OnEnable()
    {
        assetContainer = (IAssetContainer)target;
        SetStyles();
    }

    protected virtual void SetStyles()
    {
        btnStyle = new GUIStyle(EditorStyles.miniButton);
        btnStyle.margin = new RectOffset(5, 5, 2, 2);
        btnStyle.fontSize = 10;

        commonBtnStyle = new GUIStyle(EditorStyles.miniButton) { margin = new RectOffset(20, 20, 20, 2) };
        commonBtnStyle.fontSize = 12;
        commonBtnStyle.fixedHeight = 20;

        areaStyle = new GUIStyle(EditorStyles.helpBox);
        areaStyle.padding = new RectOffset(0, 0, 20, 20);

        extStyle = new GUIStyle(EditorStyles.boldLabel);
        extStyle.fontSize = 12;

        nameStyle = new GUIStyle(EditorStyles.boldLabel);
        nameStyle.fontSize = 14;
        nameStyle.normal.textColor = Color.blue;
    }

    protected void DrawContainerType()
    {
        GUILayout.Label(assetContainer.ContainerType + " Container", nameStyle);
    }

    protected void DrawContainerPropertiesToggle()
    {
        SerializedProperty build = serializedObject.FindProperty("build");
        EditorGUILayout.PropertyField(build, new GUIContent("Build", "true to include asset container in build"));

        SerializedProperty preload = serializedObject.FindProperty("preload");
        EditorGUILayout.PropertyField(preload, new GUIContent("Preload", "true to preload asset container during startup"));

        SerializedProperty individualAssetBundle = serializedObject.FindProperty("individualAssetBundle");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Individual AssetBundles", "true to set individual bundlename for each asset"), GUILayout.Width(150));
        EditorGUILayout.PropertyField(individualAssetBundle, GUIContent.none);
        EditorGUILayout.EndHorizontal();

        SerializedProperty allowAbsolutePath = serializedObject.FindProperty("allowAbsolutePath");
        EditorGUILayout.PropertyField(allowAbsolutePath, new GUIContent("Allow Absolute Path", "true to allow include asset from different folders"));

        SerializedProperty addSubFolder = serializedObject.FindProperty("addSubFolder");
        EditorGUILayout.PropertyField(addSubFolder, new GUIContent("Include Subfolders", addSubfolderTooltip));
    }

    protected void DrawAddSubfolder()
    {
       
    }

    protected void DrawCommonGUI()
    {
        EditorGUILayout.BeginVertical();

        if (GUILayout.Button(new GUIContent("Save Assets", "Save all assets"), commonBtnStyle))
        {
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button(new GUIContent("Save & Export Manifest", exportManifestTooltip), commonBtnStyle))
        {
            OnExportManifest();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button(new GUIContent("Save and EXPORT PACKAGE", exportPackageTooltip), commonBtnStyle))
        {
            OnExportPackage();
        }

        //if (GUILayout.Button("COMMIT DATABASE", commonBtnStyle))
        //{
        //    OnCommit();
        //}
        EditorGUILayout.EndVertical();

    }

    protected void OnApplyModifiedProperties()
    {
        List<UnityEngine.Object> removedObjects = assetContainer.VerifySlots();
        if (removedObjects.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Following files do not belong to path under asset container: ");
            foreach (var asset in removedObjects)
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                sb.AppendLine(assetPath);
            }

            Debug.Log(sb.ToString());
            //EditorUtility.DisplayDialog("Cannot Add Assets", sb.ToString(), "OK");
        }
    }

    protected void OnExportManifest()
    {
        AssetDatabase.SaveAssets();

        string savepath = EditorUtility.SaveFilePanel("Export Container Manifest", "", "", "csv");
        if (!savepath.Equals(string.Empty))
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("GameDB Path,Unity Path,Name");

            foreach (ExportedAsset expAsset in assetContainer.GetExportedAssets())
            {
                if (expAsset != null)
                {
                    if (expAsset.asset == null)
                        sb.AppendFormat("{0},{1},{2}{3}", "null", expAsset.assetPath, "null", Environment.NewLine);
                    else
                    {
                        sb.AppendFormat("{0},{1},{2}{3}", target.name + "/" + expAsset.assetPath, expAsset.assetPath, expAsset.asset.name, Environment.NewLine);
                    }
                }
            }

            try
            {
                File.WriteAllText(savepath, sb.ToString());
                Debug.LogFormat("Exported Manifest: [{0}]", savepath);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Failed exporting manifest", "Export failed. Check if file is in use", "OK");
            }
        }
    }

    protected void OnCommit()
    {
        
    }

    protected virtual void OnExportPackage()
    {
        AssetDatabase.SaveAssets();
        string assetpath = AssetDatabase.GetAssetPath(target);
        string[] dependencies = AssetDatabase.GetDependencies(new string[] { assetpath });

        string[] exportPaths = dependencies.Where(x => !x.EndsWith(".cs")).ToArray();

        string savepath = EditorUtility.SaveFilePanel("Export Container Package", "", "", "unitypackage");
        if (!savepath.Equals(string.Empty))
        {
            AssetDatabase.ExportPackage(exportPaths, savepath);
            Debug.LogFormat("Exported Container Package to [{0}]", savepath);
        }
    }

    protected void OnAddFolder<T>(string filter) where T : UnityEngine.Object
    {
        string targetPath = assetContainer.ContainerPath;
        int lastSeparator = targetPath.LastIndexOf('/') + 1;

        var path = EditorUtility.OpenFolderPanel("Select Folder To Add", "Assets/" + targetPath.Substring(0, lastSeparator), targetPath.Substring(lastSeparator));
        if (!path.Equals(string.Empty))
        {
            string[] extensionArray = filter.Split(';');

            var files = Directory.GetFiles(path, "*.*", addSubfolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                foreach (string extension in extensionArray)
                {
                    if (file.EndsWith(extension) && !file.Contains("@"))
                    {
                        string assetPath = "Assets" + file.Replace(Application.dataPath, "").Replace('\\', '/');
                        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                        if (asset != null)
                            assetContainer.AddAsset<T>(asset);
                        break;
                    }
                }
            }
            EditorUtility.SetDirty(target);
        }
    }

    protected void OnReorganise<T>() where T : UnityEngine.Object
    {
        assetContainer.ReorganiseList<T>();
        EditorUtility.SetDirty(target);
    }

    protected void OnAddSlots<T>(int slots) where T : UnityEngine.Object
    {
        assetContainer.AddSlots<T>(slots);
        EditorUtility.SetDirty(target);
    }

    protected static bool IsKeyPressed(string controlName, KeyCode key)
    {
        if (GUI.GetNameOfFocusedControl() == controlName)
        {
            Event e = Event.current;
            if (e.type == EventType.KeyUp && e.keyCode == key)
                return true;
        }
        return false;
    }

    protected void Show(SerializedProperty list)
    {
        if (!list.isArray)
        {
            EditorGUILayout.HelpBox(list.name + " is neither an array nor a list!", MessageType.Error);
            return;
        }

        EditorGUILayout.PropertyField(list);
        EditorGUI.indentLevel += 1;
        if (list.isExpanded)
        {
            EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
            for (int i = 0; i < list.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
                if (GUILayout.Button("Detail"))
                {
                    assetContainer.LogAssetDetail(i);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUI.indentLevel -= 1;
    }
    
}