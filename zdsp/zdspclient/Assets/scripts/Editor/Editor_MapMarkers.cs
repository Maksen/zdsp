using UnityEngine;
using UnityEditor;
using Zealot.Spawners;

public class Editor_MapMarkers
{
    static string Tag = "MonsterSpawnerMarker";
    [MenuItem("GameObject/Map/Add MonsterSpawnerMarker %&w")]
    public static void AddMonsterSpawnerMarker() {        
        GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/MonsterSpawnerMarker.prefab", typeof(GameObject)) as GameObject;
        GameObject mk = GameObject.Instantiate(prefab);
        mk.tag = Tag;
        GameObject mkref = Selection.activeGameObject;
        if (mkref!= null)
        {
            mk.transform.SetParent(mkref.transform.parent, false);
            mk.transform.position = mkref.transform.position;
            mkref.transform.SetParent(mk.transform, true);                    
            MonsterSpawnerMarker msm = mk.GetComponent<MonsterSpawnerMarker>();
            mk.name = mkref.name + "_Marker";
            if (msm != null)
            {
                MonsterSpawner msp = mkref.GetComponent<MonsterSpawner>();
                if (msp != null)
                {
                    //copy arch from spawner to marker
                    msm.archetype = msp.archetype;
                    msm.archetypeGroup = msp.archetypeGroup;
                }
                else
                {
                    SpecialBossSpawner specialBossSpawner = mkref.GetComponent<SpecialBossSpawner>();
                    if (specialBossSpawner != null)
                    {
                        /*
                        var gamedb = EditorUtils.GetGameDB();                       
                        SpecialBossRepo.Init(gamedb);
                        SpecialBossJson specialBossJson = SpecialBossRepo.GetInfoByName(specialBossSpawner.bossName);
                        if (specialBossJson != null)
                        {
                            NPCRepo.Init(gamedb);
                            NPCJson npcJson = NPCRepo.GetArchetypeById(specialBossJson.archetype);
                            if (npcJson != null)
                                msm.archetype = npcJson.archetype;
                        }*/
                    }
                }
            }
        }        
    }

    [MenuItem("GameObject/Map/Select all MonsterSpawnerMarker %&q")]
    public static void SelectAllMonsterSpawnerMarkers()
    {
        GameObject[] mks = GameObject.FindGameObjectsWithTag(Tag);
        Selection.objects = mks;
    }

    [MenuItem("GameObject/Map/Delete all MonsterSpawnerMarker")]
    public static void ClearAllMonsterSpawnerMarkers()
    {
        if (EditorUtility.DisplayDialog("Delete all MonsterSpawnerMarker",
                                        "Do you want to delete all MonsterSpawnerMakers in the scene?",
                                        "Yes",
                                        "Cancel"))
        {
            GameObject[] mks = GameObject.FindGameObjectsWithTag(Tag);
            for (int i = 0; i < mks.Length; i++)
            {
                GameObject.DestroyImmediate(mks[i]);
            }
        }
    }
}

