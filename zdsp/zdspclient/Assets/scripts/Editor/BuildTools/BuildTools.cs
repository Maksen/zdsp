using Kopio.JsonContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zealot.Repository;

public class BuildTools
{
    //[MenuItem("Build/Verify GameDB Asset Paths", false, 1000)]
    //public static void VerifyGameDBAssetPaths()
    //{
    //    TextAsset gamedb = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GameData/GameRepo/Buddha/gamedata.json");
    //    if (gamedb != null)
    //    {
    //        KopioAssetManager.VerifyGameDBAssets(gamedb.ToString());
    //    }
    //}

    [MenuItem("Build/Export All LevelData and NavData", false, 1000)]
    public static void ExportLevels()
    {
        var editorScenes = EditorBuildSettings.scenes;

        GameDBRepo gameData = EditorUtils.GetGameDB();
        if (gameData != null)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                List<string> scenes = new List<string>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene != null)
                        scenes.Add(scene.path);
                }

                EditorUtility.DisplayProgressBar("Export Scenes", "Exporting..", 0);

                LevelRepo.Init(gameData);
                var levelList = LevelRepo.mNameMap;
                int levelCount = levelList.Count;
                float count = 0f;

                foreach (var editorScene in editorScenes)
                {
                    if(editorScene.enabled)
                    {
                        string sceneName = Path.GetFileNameWithoutExtension(editorScene.path);
                        if (levelList.ContainsKey(sceneName))
                        {
                            try
                            {
                                count++;

                                Debug.Log("Loading: " + sceneName);
                                if (EditorSceneManager.OpenScene(editorScene.path).isLoaded)
                                {
                                    EditorApplication.ExecuteMenuItem("Window/Zealot Tools/Export Current Level");
                                    Exporter.OnExportCurrentNavData(false);
                                    EditorSceneManager.SaveOpenScenes();
                                }

                                EditorUtility.DisplayProgressBar("Export Scenes", "Exporting " + sceneName, (float)(count / levelCount));
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
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

                EditorUtility.ClearProgressBar();
            }
        }
        else
            Debug.Log("Game DB not found");
    }

    [MenuItem("Build/Player Settings", false, 2000)]
    public static void OpenPlayerSettings()
    {
        EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
    }

    [MenuItem("Build/Clear Webplayer AssetBundles Cache", false, 2000)]
    public static void ClearWebplayerAssetBundlesCache()
    {
        //Caching.CleanCache();
        Caching.ClearCache();
    }

    [MenuItem("Build/Verify Build Levels", false, 20000)]
    public static void VerifyBuildLevels()
    {
        string[] defaultscenes = { "SplashScreen.unity", "Dialog_SplashScreenLoading.unity", "Login.unity", "lobby.unity", "UI_CombatHierarchy.unity" };

        TextAsset gamedb = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GameData/GameRepo/gamedata.json");
        if (gamedb != null)
        {
            List<string> invalidScenes = new List<string>();
            List<string> disabledScenes = new List<string>();

            GameDBRepo gameData = JsonConvert.DeserializeObject<GameDBRepo>(gamedb.text);
            var lvlList = gameData.Level.Values.ToDictionary(x => x.unityscene, x => false);

            foreach (var sceneSetting in EditorBuildSettings.scenes)
            {
                string sceneName = Path.GetFileNameWithoutExtension(sceneSetting.path);
                if (defaultscenes.Any(x => sceneSetting.path.Contains(x)))
                {
                    //default scene
                    if (!sceneSetting.enabled)
                        disabledScenes.Add(sceneName);
                }
                else
                {
                    if (File.Exists(sceneSetting.path) && lvlList.ContainsKey(sceneName))
                    {
                        lvlList[sceneName] = true;
                        if (!sceneSetting.enabled)
                            disabledScenes.Add(sceneName);
                    }
                    else
                        invalidScenes.Add(sceneName);
                }
            }

            EditorUtility.DisplayDialog("Verify Build Levels", 
                string.Format("These scenes are not enabled:\n{0}\n\nThese scenes are invalid:\n{1}", string.Join("\n", disabledScenes.ToArray()), string.Join("\n", invalidScenes.ToArray())), "OK");
        }
    }
    

    [MenuItem("Build/Verify Build Levels", true)]
    public static bool IsMasterCD()
    {
        return ZealotEditorConfig.HasMasterCDConfig();
    }

    #region Export Packages

    [MenuItem("Build/Export Packages/Scripts only", true)]
    [MenuItem("Build/Export Packages/Script and UI for Art", true)]
    public static bool ShowExportPackagesMenu()
    {
        return false;
    }

    /// <summary>
    /// [deprecated]
    /// build package to sync scripts and UI folder from MasterCD to Art.
    /// </summary>
    [MenuItem("Build/Export Packages/Script and UI for Art", false, 60000)]
    public static void ExportScriptAndUIForArt()
    {
        string savepath = EditorUtility.SaveFilePanel("Export Scripts and UI Package for Art", "", "", "unitypackage");
        if (!savepath.Equals(string.Empty))
        {
            EditorUtility.DisplayProgressBar("Export scripts and UI", "Exporting..", 0);
            float count = 0f;


            Dictionary<string, object> exportFiles = new Dictionary<string, object>();

            string scriptsPath = Path.Combine(Application.dataPath, "scripts");
            var scriptfiles = Directory.GetFiles(scriptsPath, "*.*", SearchOption.AllDirectories);

            string uiPath = Path.Combine(Application.dataPath, "ui");
            var uifiles = Directory.GetFiles(uiPath, "*.*", SearchOption.AllDirectories);

            foreach (string filepath in scriptfiles)
            {
                if (Path.GetExtension(filepath) != ".meta" && Path.GetExtension(filepath) != ".bak" && Path.GetExtension(filepath) != ".orig")
                {
                    string assetPath = "Assets" + filepath.Replace(Application.dataPath, "").Replace('\\', '/');

                    if (!exportFiles.ContainsKey(assetPath))
                        exportFiles.Add(assetPath, null);

                    var dependencies = AssetDatabase.GetDependencies(new string[] { assetPath });
                    foreach (string dependency in dependencies)
                    {
                        if (!exportFiles.ContainsKey(dependency))
                        {
                            exportFiles.Add(dependency, null);
                        }
                    }
                }
                count++;
                EditorUtility.DisplayProgressBar("Export scripts and UI", "Exporting Scripts..", count / (uifiles.Length + scriptfiles.Length));
            }


            foreach (string filepath in uifiles)
            {
                if (Path.GetExtension(filepath) != ".meta" && Path.GetExtension(filepath) != ".bak" && Path.GetExtension(filepath) != ".orig")
                {
                    string assetPath = "Assets" + filepath.Replace(Application.dataPath, "").Replace('\\', '/');
                    if (!exportFiles.ContainsKey(assetPath))
                        exportFiles.Add(assetPath, null);

                    /*
                    var dependencies = AssetDatabase.GetDependencies(assetPath);
                    foreach (string dependency in dependencies)
                    {
                        if (!exportFiles.ContainsKey(dependency))
                        {
                            exportFiles.Add(dependency, null);
                        }
                    }
                    */
                }
                count++;
                EditorUtility.DisplayProgressBar("Export scripts and UI", "Exporting UI..", count / (uifiles.Length + scriptfiles.Length));
            }

            AssetDatabase.ExportPackage(exportFiles.Keys.ToArray(), savepath);
            EditorUtility.ClearProgressBar();
            Debug.LogFormat("Exported Art Package to [{0}]", savepath);
        }
    }

    /// <summary>
    /// [deprecated]
    /// build package to sync scripts from MasterCD to Art.
    /// </summary>
    [MenuItem("Build/Export Packages/Scripts only", false, 60000)]
    public static void ExportScriptsOnly()
    {
        string savepath = EditorUtility.SaveFilePanel("Export Scripts only Package", "", "", "unitypackage");
        if (!savepath.Equals(string.Empty))
        {
            EditorUtility.DisplayProgressBar("Export scripts", "Exporting..", 0);

            Dictionary<string, object> exportFiles = new Dictionary<string, object>();

            string scriptsPath = Path.Combine(Application.dataPath, "scripts");
            var files = Directory.GetFiles(scriptsPath, "*.*", SearchOption.AllDirectories);

            float count = 0f;
            int filescount = files.Length;

            foreach (string filepath in files)
            {
                if (Path.GetExtension(filepath) != ".meta" && Path.GetExtension(filepath) != ".bak" && Path.GetExtension(filepath) != ".orig")
                {
                    string assetPath = "Assets" + filepath.Replace(Application.dataPath, "").Replace('\\', '/');
                    if (!exportFiles.ContainsKey(assetPath))
                        exportFiles.Add(assetPath, null);

                    var dependencies = AssetDatabase.GetDependencies(new string[] { assetPath });
                    foreach (string dependency in dependencies)
                    {
                        if (!exportFiles.ContainsKey(dependency))
                        {
                            exportFiles.Add(dependency, null);
                        }
                    }
                }
                count++;
                EditorUtility.DisplayProgressBar("Export scripts", "Exporting..", count / filescount);
            }

            AssetDatabase.ExportPackage(exportFiles.Keys.ToArray(), savepath, ExportPackageOptions.IncludeDependencies);
            EditorUtility.ClearProgressBar();
            Debug.LogFormat("Exported Scripts Package to [{0}]", savepath);
        }
    }    
    #endregion
}

public class ScanShaders: EditorWindow
{
    List<string> lstPath;
    string[] arrPath;

    Vector2 scrollPosition;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Build/Scan Shaders", false, 20000)]
    static void Init()
    {
        var window = GetWindow(typeof (ScanShaders));
        window.Show();
    }
    
    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        GUILayout.Space(5);

        GUILayout.Label("Shader Listing:");

        GUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false);

        GUILayout.Label(string.Join(Environment.NewLine, lstPath.ToArray()));

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    void OnEnable()
    {
        lstPath = new List<string>();

        arrPath = AssetDatabase.GetAllAssetPaths();
        foreach (string path in arrPath.Where(s => s.EndsWith(".mat", StringComparison.OrdinalIgnoreCase)))
        {
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (lstPath.Contains(m.shader.name) == false)
            {
                lstPath.Add(m.shader.name);
            }
        }

        lstPath.Sort();
    }
}