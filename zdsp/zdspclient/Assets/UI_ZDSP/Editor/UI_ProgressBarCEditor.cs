using UnityEditor;
using System.Collections.Generic;
using System;

[CanEditMultipleObjects]
[CustomEditor(typeof(UI_ProgressBarC), true)]
public class UI_ProgressBarCEditor : Editor {
    Dictionary<string, SerializedProperty> serializedProperties = new Dictionary<string, SerializedProperty>();

    string[] properties = new string[]{
        "_max",
        "_value",
        "type",
        "barImage",
        "typeA",
        "typeB",

        "BarText",
        "textType",
        "showMax",
        "_canExceedMax"
    };

    SerializedProperty progressbartype;
    protected void OnEnable()
    {
        progressbartype = serializedObject.FindProperty("_ProgressBarTypeFlagMask");
        Array.ForEach(properties, x => {
            var p = serializedObject.FindProperty(x);
            serializedProperties.Add(x, p);
        });
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //base.OnInspectorGUI();

        EditorGUILayout.Space();

        //progressbartype.intValue = (int)((UI_ProgressBarC.ProgressbarType)EditorGUILayout.EnumMaskField("Progess Bar Type", (UI_ProgressBarC.ProgressbarType)progressbartype.intValue));
        progressbartype.intValue = (int)((UI_ProgressBarC.ProgressbarType)EditorGUILayout.EnumFlagsField("Progess Bar Type", (UI_ProgressBarC.ProgressbarType)progressbartype.intValue));
        EditorGUILayout.PropertyField(serializedProperties["_canExceedMax"]);
        EditorGUILayout.PropertyField(serializedProperties["_max"]);
        EditorGUILayout.PropertyField(serializedProperties["_value"]);
        EditorGUILayout.PropertyField(serializedProperties["barImage"]);

        if (((UI_ProgressBarC.ProgressbarType)progressbartype.intValue & UI_ProgressBarC.ProgressbarType.TypeA) == UI_ProgressBarC.ProgressbarType.TypeA)
        {
            EditorGUILayout.PropertyField(serializedProperties["typeA"]);
        }


        if (((UI_ProgressBarC.ProgressbarType)progressbartype.intValue & UI_ProgressBarC.ProgressbarType.TypeB) == UI_ProgressBarC.ProgressbarType.TypeB)
        {
            EditorGUILayout.PropertyField(serializedProperties["typeB"]);
        }
        
        EditorGUILayout.PropertyField(serializedProperties["type"]);

        EditorGUI.indentLevel++;
        if (serializedProperties["type"].enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(serializedProperties["BarText"]);
            EditorGUILayout.PropertyField(serializedProperties["textType"]);
            EditorGUILayout.PropertyField(serializedProperties["showMax"]);
        }

       

            EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();

        Array.ForEach(targets, x => ((UI_ProgressBarC)x).Refresh());
    }
}
