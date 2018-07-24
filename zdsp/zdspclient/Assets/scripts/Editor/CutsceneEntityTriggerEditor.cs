using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CutsceneEntityTrigger))]
public class CutsceneEntityTriggerEditor : Editor
{
    CutsceneEntityTrigger cutsceneEntityTriggerScript;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        cutsceneEntityTriggerScript = (CutsceneEntityTrigger)target;
        
        if (cutsceneEntityTriggerScript.triggerColliderType == TriggerColliderType.Sphere)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Sphere radius", GUILayout.Width(70));
            cutsceneEntityTriggerScript.radius = EditorGUILayout.FloatField(cutsceneEntityTriggerScript.radius);
            GUILayout.EndHorizontal();

            EditorUtility.SetDirty(cutsceneEntityTriggerScript);
        }

        if (cutsceneEntityTriggerScript.triggerColliderType == TriggerColliderType.Box)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Box size", GUILayout.Width(70));
            cutsceneEntityTriggerScript.boxSize = EditorGUILayout.Vector3Field("size", cutsceneEntityTriggerScript.boxSize);
            GUILayout.EndHorizontal();

            EditorUtility.SetDirty(cutsceneEntityTriggerScript);
        }
    }
}
