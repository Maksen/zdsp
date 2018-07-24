using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SafeZoneTrigger))]
public class SafeZoneEditor : Editor {

    SafeZoneTrigger myScript;
    protected void OnEnable()
    {
        myScript = (SafeZoneTrigger)target;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (myScript.myAreaType == SafeZoneTrigger.SafeZoneAreaType.Sphere)
        {
          
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("sphere radius", GUILayout.Width(70));
            myScript.safeZoneRadius = EditorGUILayout.FloatField(myScript.safeZoneRadius);
            GUILayout.EndHorizontal();

            EditorUtility.SetDirty(myScript);
        }

   
        if (myScript.myAreaType == SafeZoneTrigger.SafeZoneAreaType.Box)
        {
          
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("box size", GUILayout.Width(70));
            myScript.boxSize = EditorGUILayout.Vector3Field("size",myScript.boxSize);
            GUILayout.EndHorizontal();

            EditorUtility.SetDirty(myScript);
        }
    }
}
