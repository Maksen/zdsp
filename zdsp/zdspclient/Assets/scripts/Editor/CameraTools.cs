using UnityEngine;
using UnityEditor;

public class CameraTools 
{
    [MenuItem("Tools/AddCameraTool")]
    private static void AddCameraTool()
    {
        if (Application.isPlaying)
        {
            Debug.Log("Adding Camera Tool");
            Object prefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Tools/Scenes_camera.prefab", typeof(GameObject));
            GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            Debug.Log("Done");
        }
        else
        {
            Debug.LogError("Editor is not playing");
        }
    }

}

//#if UNITY_EDITOR
//[CustomEditor(typeof(CameraTools))]
//public class CameraToolsEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();

//        CameraTools myScript = (CameraTools)target;
//        if (GUILayout.Button("Create CameraTools"))
//        {
//            myScript.AddCameraTool();
//        }
//    }
//}
//#endif