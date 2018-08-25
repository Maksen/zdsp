using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Zealot.ClientSpawners;
using Zealot.Spawners;
using Zealot.Repository;

class SceneNPCTools
{
    public static void CompileSceneAssets(bool showProgressBar, Dictionary<string,string> errorList = null)
    {
        var scene = SceneManager.GetActiveScene();

        var gamedb = EditorUtils.GetGameDB();
        if (gamedb == null)
            return;

        float progress = 0f;
        if (showProgressBar)
            EditorUtility.DisplayProgressBar("Compile Level Assets", "Hold on...", progress);

        var entityGO = GameObject.Find("Entity");
        if(entityGO == null)
        {
            Debug.LogFormat("Entity prefab not found");
            EditorUtility.ClearProgressBar();

            if (errorList != null)
                errorList[scene.name] = "Entity prefab not found";

            return;
        }

        GameObject entitiesPrefab = null;
        for (int i = 0; i < entityGO.transform.childCount; i++)
        {
            var child = entityGO.transform.GetChild(i).gameObject;
            if(child.name.StartsWith("Entities_"))
            {
                entitiesPrefab = child;
                break;               
            }
        }

        if(entitiesPrefab == null)
        {
            Debug.LogFormat("Entities_ prefab not found");
            EditorUtility.ClearProgressBar();

            if (errorList != null)
                errorList[scene.name] = "Entities_ prefab not found";

            return;
        }
        var sourcePrefab = PrefabUtility.GetPrefabParent(entitiesPrefab);

        LevelRepo.Init(gamedb);
        RealmRepo.Init(gamedb);
        CombatNPCRepo.Init(gamedb);
        StaticNPCRepo.Init(gamedb);
        RealmNPCGroupRepo.Init(gamedb);
        
        #region Add Containers
        //add scene npc container
        var results = UnityEngine.Object.FindObjectsOfType<SceneNPCContainer>();
        foreach (var go in results)
            GameObject.DestroyImmediate(go.gameObject);

        var gameObject = new GameObject();
        gameObject.name = "SceneNPCContainer";
        var npcContainer = gameObject.AddComponent<SceneNPCContainer>();
        gameObject.transform.SetParent(entitiesPrefab.transform);

        //add scene asset container
        var findresults = UnityEngine.Object.FindObjectsOfType<SceneAssetContainer>();
        foreach (var go in findresults)
            GameObject.DestroyImmediate(go.gameObject);

        var sceneAssetGO = new GameObject();
        sceneAssetGO.name = "SceneAssetContainer";
        var sceneAssetContainer = sceneAssetGO.AddComponent<SceneAssetContainer>();
        sceneAssetGO.transform.SetParent(entitiesPrefab.transform);
        #endregion        

        progress = 0.1f;
        if (showProgressBar)
            EditorUtility.DisplayProgressBar("Compile Level Assets", "Finding all spawners...", progress);

        LevelJson levelJson = LevelRepo.GetInfoByName(scene.name);
        int levelID = levelJson != null ? levelJson.id : -1;
        List<int> realmIds = new List<int>();

        List<string> modelpaths = new List<string>();
        var monsterpaths = GetSceneSpawners(gamedb, levelID);
        modelpaths.AddRange(monsterpaths.Keys);        

        progress = 0.2f;
        if (showProgressBar)
            EditorUtility.DisplayProgressBar("Compile Level Assets", "Finding all client spawners...", progress);

        var staticNPCs = GetSceneClientSpawners();
        modelpaths.AddRange(staticNPCs);

        progress = 0.3f;
        if (showProgressBar)
            EditorUtility.DisplayProgressBar("Compile Level Assets", "Linking model prefabs...", progress);

        List<string> missingNPCEffects = new List<string>();
        List<string> missingAssets = new List<string>();
        for (int i = 0; i < modelpaths.Count; i++)
        {
            progress = 0.3f + ((float)i / modelpaths.Count * 0.5f);
            if (showProgressBar)
                EditorUtility.DisplayProgressBar("Compile Level Assets", "Linking model prefabs...", progress);

            string modelpath = modelpaths[i];
            if (!string.IsNullOrEmpty(modelpath))
            {
                GameObject modelprefab = null;
                if (modelpath.StartsWith("Assets/"))
                    modelprefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelpath);              
                else
                    modelprefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/" + modelpath);

                if (modelprefab == null)
                    missingAssets.Add(modelpath);
                else
                {
                    //if (modelprefab.GetComponent<NPCEffect>() == null && monsterpaths.ContainsKey(modelpath)
                    //    && monsterpaths[modelpath] != typeof(GateSpawner) && monsterpaths[modelpath] != typeof(ObjectSpawner))
                    //{
                    //    missingNPCEffects.Add(modelpath);
                    //}
                    npcContainer.AddModelPrefab(modelprefab);
                }
            }
        }

        progress = 0.8f;
        if (showProgressBar)
            EditorUtility.DisplayProgressBar("Compile Level Assets", "Adding scene UI assets...", progress);

        //grab maps no need for this project       
        //Sprite mapsprite = null;
        //if (levelJson != null)
        //{
        //    string mappath = @"Assets/UI_PiLiQ/AsyncIcons/Map/map_" + scene.name + ".png";
        //    mapsprite = AssetDatabase.LoadAssetAtPath<Sprite>(mappath);
        //}

        //if (mapsprite != null)
        //    sceneAssetContainer.mapIcon = mapsprite;
        //else
        //    missingAssets.Add("missing Map sprite");

        progress = 0.8f;
        if (showProgressBar)
            EditorUtility.DisplayProgressBar("Compile Level Assets", "Validating...", progress);

        npcContainer.OnValidate();
        sceneAssetContainer.OnValidate();

        PrefabUtility.ReplacePrefab(entitiesPrefab, sourcePrefab, ReplacePrefabOptions.ConnectToPrefab);
        
        string prefabPath = AssetDatabase.GetAssetPath(sourcePrefab);
        var assetimporter = AssetImporter.GetAtPath(prefabPath);
        if(assetimporter != null)
        {
            if (EditorBuildSettings.scenes.FirstOrDefault(x => x.enabled && x.path == scene.path) != null)
                assetimporter.assetBundleName = string.Format("entities/{0}", scene.name.ToLower());
            else
                assetimporter.assetBundleName = "";
        }

        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();

        if (showProgressBar)
        {
            if(missingAssets.Count > 0 || missingNPCEffects.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("MISSING ASSETS:");
                sb.AppendLine(string.Join(Environment.NewLine, missingAssets.ToArray()));
                sb.AppendLine();
                //sb.AppendLine("Prefabs without NPCEffect component:");
                //sb.AppendLine(string.Join(Environment.NewLine, missingNPCEffects.ToArray()));

                EditorUtility.DisplayDialog("Missing Assets", sb.ToString(), "OK");
            }
            Debug.Log("Finish compile scene npc container");
        }
    }

    [MenuItem("Build/Compile All Level Assets", false, 1000)]
    public static void CompileAllLevelsAssets()
    {
        var editorScenes = EditorBuildSettings.scenes;

        GameDBRepo gameData = EditorUtils.GetGameDB();
        if (gameData != null)
        {
            Dictionary<string, string> errorList = new Dictionary<string, string>();
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                List<string> scenes = new List<string>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene != null)
                        scenes.Add(scene.path);
                }
                
                EditorUtility.DisplayProgressBar("Compile All Levels Assets", "Compiling..", 0);

                LevelRepo.Init(gameData);
                var levelList = LevelRepo.mNameMap;
                int levelCount = levelList.Count;
                float count = 0f;

                foreach (var editorScene in editorScenes)
                {
                    if (editorScene.enabled)
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
                                    CompileSceneAssets(false, errorList);
                                }

                                EditorUtility.DisplayProgressBar("Compile All Levels Assets", "Compiling " + sceneName, (float)(count / levelCount));
                            }
                            catch (Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }

                foreach(var kvp in errorList)
                {
                    Debug.LogErrorFormat("[{0}] failed to compile assets: {1}", kvp.Key, kvp.Value);
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

    private static List<string> GetSceneClientSpawners()
    {
        List<string> results = new List<string>();
        ClientSpawnerBase[] clientSpawners = UnityEngine.Object.FindObjectsOfType(typeof(ClientSpawnerBase)) as ClientSpawnerBase[];
        foreach (var spawner in clientSpawners)
        {
            string archetype = "";
            if (spawner is StaticNPCSpawner)
                archetype = ((StaticNPCSpawner)spawner).archetype;

            string modelpath = StaticNPCRepo.GetModelPrefabPathByArchetype(archetype);
            if (!string.IsNullOrEmpty(modelpath))
                results.Add(modelpath);
        }
        return results.Distinct().ToList();
    }

    private static Dictionary<string, Type> GetSceneSpawners(GameDBRepo gamedb, int levelID)
    {
        Dictionary<string, Type> results = new Dictionary<string, Type>();
        ServerEntity[] points = UnityEngine.Object.FindObjectsOfType(typeof(ServerEntity)) as ServerEntity[];
        foreach (var spawner in points)
        {
            Type spawnerType = spawner.GetType();
            if (spawner is GoblinSpawner)
            {
                string archetype = ((GoblinSpawner)spawner).archetype;
                var npcinfo = CombatNPCRepo.GetNPCByArchetype(archetype);
                if (npcinfo != null && !string.IsNullOrEmpty(npcinfo.modelprefabpath))
                {
                    results[npcinfo.modelprefabpath] = typeof(GoblinSpawner);
                }
            }
            else if (spawner is MonsterSpawner)
            {
                var monsterspawner = spawner as MonsterSpawner;
                if (!string.IsNullOrEmpty(monsterspawner.archetype))
                {
                    var npcinfo = CombatNPCRepo.GetNPCByArchetype(monsterspawner.archetype);
                    if (npcinfo != null)
                    {
                        if(!string.IsNullOrEmpty(npcinfo.modelprefabpath))
                            results[npcinfo.modelprefabpath] = typeof(MonsterSpawner);
                    }
                }
                else if (!string.IsNullOrEmpty(monsterspawner.archetypeGroup))
                {
                    foreach (var realminfo in RealmRepo.mIdMap.Values)
                    {
                        if (realminfo.level == levelID)
                        {
                            var npcinfo = RealmNPCGroupRepo.GetNPCByGroupNameAndRealmId(monsterspawner.archetypeGroup, realminfo.id);
                            if (npcinfo != null && !string.IsNullOrEmpty(npcinfo.modelprefabpath))
                            {
                                results[npcinfo.modelprefabpath] = typeof(MonsterSpawner);
                            }
                        }
                    }
                }
            }
            else if (spawner is SpecialBossSpawner)
            {
                string bossName = ((SpecialBossSpawner)spawner).bossName;
                var specialBossInfo = gamedb.SpecialBoss.Values.FirstOrDefault(x => x.name == bossName);
                if (specialBossInfo != null)
                {
                    var npcinfo = CombatNPCRepo.GetNPCById(specialBossInfo.archetypeid);
                    if (npcinfo != null && !string.IsNullOrEmpty(npcinfo.modelprefabpath))
                    {
                        results[npcinfo.modelprefabpath] = typeof(SpecialBossSpawner);
                    }
                }
            }
            else if (spawner is SMBossSpawner)
            {
                var smbossspawner = spawner as SMBossSpawner;
                if (!string.IsNullOrEmpty(smbossspawner.archetype))
                {
                    var npcinfo = CombatNPCRepo.GetNPCByArchetype(smbossspawner.archetype);
                    if (npcinfo != null)
                    {
                        if (!string.IsNullOrEmpty(npcinfo.modelprefabpath))
                            results[npcinfo.modelprefabpath] = typeof(SMBossSpawner);
                    }
                }
            }
            else if (spawnerType == typeof(GateSpawner))
            {
                var gateprefab = ((GateSpawner)spawner).prefab;
                if(gateprefab != null)
                {
                    results[AssetDatabase.GetAssetPath(gateprefab)] = typeof(GateSpawner);
                }
            }
            else if (spawnerType == typeof(PersonalMonsterSpawner))
            {
                string archetype = ((PersonalMonsterSpawner)spawner).archetype;
                var npcinfo = CombatNPCRepo.GetNPCByArchetype(archetype);
                if (npcinfo != null && !string.IsNullOrEmpty(npcinfo.modelprefabpath))
                {
                    results[npcinfo.modelprefabpath] = typeof(PersonalMonsterSpawner);
                }
            }
            else if (spawnerType == typeof(ObjectSpawner))
            {
                var prefab = ((ObjectSpawner)spawner).prefab;
                if (prefab != null)
                {
                    results[AssetDatabase.GetAssetPath(prefab)] = typeof(ObjectSpawner);
                }
            }
        }
        return results;
    }
}
