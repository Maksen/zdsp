using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using Newtonsoft.Json;
using Zealot.Entities;
using Zealot.Spawners;
using System.Xml;
using System;

public class Exporter : EditorWindow
{
    const string PREFIX = "Assets//GameData//Levels//";
    const string EXT = ".json";

    [MenuItem("Tools/Export Current Level", false, 90000)]
    static void exportCurrentLevelAll()
    {
        exportCurrentLevelSafeZoneID();
        exportCurrentLevel();
        OnExportCurrentNavData();
        SceneNPCTools.CompileSceneAssets(true);
    }

    static void exportCurrentLevel()
    {
        bool isSuccess = true;
        List<string> pathList = new List<string>();
        XmlNodeList nodes = ZealotEditorConfig.GetConfigNodeList("/config/export/levels/level");
        if (nodes != null)
        {
            foreach (XmlNode node in nodes)
                pathList.Add(node.Attributes["path"].Value);
        }
     
        string scenepath = EditorApplication.currentScene;
        //Debug.Log("sp" + scenepath);
        //Debug.Log("sp2 : " + EditorSceneManager.GetActiveScene().GetHashCode());  
        if (scenepath.Length == 0)
        {
            Debug.LogWarning("No active Scene is opened for export!");
            return;
        }

        string filename = Path.GetFileNameWithoutExtension(scenepath) + EXT;
        string path = PREFIX + filename;
        using (FileStream fs = File.Open(path, FileMode.Create))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                jsonSetting.Converters.Add(new Vector3Converter());
                LevelInfo linfo = new LevelInfo();
                
                if (exportSpawners(linfo))
                {
                    string output = JsonConvert.SerializeObject(linfo, Newtonsoft.Json.Formatting.Indented, jsonSetting);
                    sw.Write(output);

                    Debug.Log("Level: " + path + " exported successfully!");
                }
                else
                {
                    isSuccess = false;
                }

                if (exportSafeZoneTrigger() == false)
                {
                    return;
                }
            }
        }

        if (isSuccess == false)
            return;

        foreach(string dirpath in pathList)
        {
            if (!Directory.Exists(dirpath))
            {
                Directory.CreateDirectory(dirpath);
            }
            string targetpath = Path.Combine(dirpath, filename);
            File.Copy(path, targetpath, true);
            Debug.Log("copied level data to [" + targetpath + "]");
        }   
    }

    private static bool exportSafeZoneTrigger()
    {
        bool isSuccess = true;
        SafeZoneTrigger[] safezone = GameObject.FindObjectsOfType(typeof(SafeZoneTrigger)) as SafeZoneTrigger[];
        Collider[] mycol;
        int mask = 0; 
        mask |= (1 << LayerMask.NameToLayer("SafeZoneLayer"));
        for (int i=0;i<safezone.Length;i++)
        {
            if(safezone[i].myAreaType == SafeZoneTrigger.SafeZoneAreaType.Sphere)
            {
                
                mycol = Physics.OverlapSphere(safezone[i].transform.position, safezone[i].safeZoneRadius, mask);
                if (mycol.Length > 1)//mean overlap, more than 1 is because it detect ownself as well
                {
                    EditorUtility.DisplayDialog("Error",
                        safezone[i].name + " is touching " + mycol[1].name, "Ok");
                    
                    isSuccess = false;
                    break;
                }
            }
            else if (safezone[i].myAreaType == SafeZoneTrigger.SafeZoneAreaType.Box)
            {
                mycol = Physics.OverlapBox(safezone[i].transform.position, safezone[i].boxSize / 2, Quaternion.identity, mask);
                if (mycol.Length > 1)//mean overlap, more than 1 is because it detect ownself as well
                {
                    EditorUtility.DisplayDialog("Error",
                        safezone[i].name + " is touching " + mycol[1].name, "Ok");
                    
                    isSuccess = false;
                    break;
                }
            }
        }

        for (int i = 0; i < safezone.Length; i++)
        {
            DestroyImmediate(safezone[i].GetComponent<Collider>());
            EditorUtility.SetDirty(safezone[i]);
        }

        return isSuccess;
    }

    static bool exportSpawners(LevelInfo linfo)
    {
        ServerEntity[] points = GameObject.FindObjectsOfType(typeof(ServerEntity)) as ServerEntity[];
        Dictionary<string, Dictionary<int, ServerEntityJson>> spawners = new Dictionary<string, Dictionary<int, ServerEntityJson>>();
        foreach (ServerEntity o in points)
        {
            ServerEntityJson jsonclass = o.GetJson();
            System.Type t = jsonclass.GetType();
            if (!spawners.ContainsKey(t.Name))
                spawners.Add(t.Name, new Dictionary<int, ServerEntityJson>());

            try
            {
                o.EntityId = jsonclass.ObjectID;
                spawners[t.Name].Add(jsonclass.ObjectID, jsonclass);
                EditorUtility.SetDirty(o);
            }
            catch(Exception e)
            {
                EditorUtility.DisplayDialog("Error",
                       e.Message, "Ok");
               
               return false;
            }
            
        }
        linfo.mEntities = spawners;

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("Success writing spawners..");
        
        return true;
    }

    private static void ExportNavData(string path, bool showDialog)
    {
        if (!string.IsNullOrEmpty(path))
        {
            if (AstarPath.active == null)
            {
                AstarPath.active = FindObjectOfType(typeof(AstarPath)) as AstarPath;
                if (AstarPath.active == null)
                {
                    return;
                }
            }

            //if (AstarPathEditor.activeEditor == null) return;

            if (!Application.isPlaying && (AstarPath.active.astarData.graphs == null || AstarPath.active.astarData.graphTypes == null))
            {
                EditorUtility.DisplayProgressBar("Scanning", "Deserializing", 0);
                AstarPath.active.astarData.DeserializeGraphs();
            }

            EditorUtility.DisplayProgressBar("Scanning", "Scanning...", 0);

            try
            {
                OnScanStatus info = progress => EditorUtility.DisplayProgressBar("Scanning", progress.description, progress.progress);
                AstarPath.active.ScanLoop(info);

            }
            catch (System.Exception e)
            {
                Debug.LogError("There was an error generating the graphs:\n" + e + "\n\nIf you think this is a bug, please contact me on arongranberg.com (post a comment)\n");
                if (showDialog)
                    EditorUtility.DisplayDialog("Error Generating Graphs", "There was an error when generating graphs, check the console for more info", "Ok");

                throw e;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            //uint checksum;
            Pathfinding.Serialization.SerializeSettings settings = Pathfinding.Serialization.SerializeSettings.All;
            byte[] bytes = null;
            //uint ch = 0;

            // Add a work item since we cannot be sure that pathfinding (or graph updates)
            // is not running at the same time
            AstarPath.active.AddWorkItem(new AstarPath.AstarWorkItem(force =>
            {
                var sr = new Pathfinding.Serialization.AstarSerializer(AstarPath.active.astarData, settings);
                sr.OpenSerialize();
                AstarPath.active.astarData.SerializeGraphsPart(sr);
                //sr.SerializeEditorSettings(AstarPathEditor.activeEditor.GetGraphEditors());
                bytes = sr.CloseSerialize();
                //ch = sr.GetChecksum();
                return true;
            }));

            // Make sure the above work item is executed immediately
            AstarPath.active.FlushWorkItems();
            //checksum = ch;

            Pathfinding.Serialization.AstarSerializer.SaveToFile(path, bytes);

            if (showDialog)
                EditorUtility.DisplayDialog("Done Saving", "Done saving nav graph data.", "Ok");
            Debug.Log("Navdata: " + path + " exported successfully!");
        }
    }

    static void exportCurrentLevelSafeZoneID()
    {
        SafeZoneTrigger[] safezone = GameObject.FindObjectsOfType(typeof(SafeZoneTrigger)) as SafeZoneTrigger[];     
        for (int i = 0; i < safezone.Length; i++)
            safezone[i].SetMySafeZoneFlag(true);
    }

    public static void OnExportCurrentNavData(bool showDialog = true)
    {
        bool canexport = false;
        GameObject astar = GameObject.Find("A*");
        canexport = (astar != null && astar.activeInHierarchy);

        if (canexport)
        {
            string levelname = Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
            string navpath = @"..\piliclient\Assets\GameData\Navdata\" + levelname + ".bytes";
            List<string> copyPaths = new List<string>();

            XmlNodeList nodes = ZealotEditorConfig.GetConfigNodeList("/config/export/navdatas/navdata");
            if (nodes != null)
            {
                bool firstpath = true;
                foreach (XmlNode node in nodes)
                {
                    if (firstpath)
                        navpath = Path.Combine(node.Attributes["path"].Value, levelname + ".bytes");
                    else
                        copyPaths.Add(Path.Combine(node.Attributes["path"].Value, levelname + ".bytes"));

                    firstpath = false;
                }
            }

            try
            {
                ExportNavData(navpath, showDialog);

                foreach (string copypath in copyPaths)
                {
                    File.Copy(navpath, copypath, true);
                    Debug.Log("Navdata copy to: " + copypath);
                }
            }
            catch(System.Exception ex)
            {
                //will not copy if there are errors exporting navdata
                throw ex;
            }
            
        }
        else if (showDialog)
        {
            EditorUtility.DisplayDialog("Cannot export NavData", "Active A* cannot be found in scene.\nPlease make sure A* has been generated and activated", "Ok");
        }
    }
}

