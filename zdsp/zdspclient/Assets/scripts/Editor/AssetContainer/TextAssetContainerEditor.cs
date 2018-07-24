using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextAssetContainer))]
[CanEditMultipleObjects]
public class TextAssetContainerEditor : BaseAssetContainerEditor
{
    private bool addTextAssetSlots = false;
    private int _newTextAssetSlots;

    const string textAssetTypes = ".json;.txt;";

    public override void OnInspectorGUI()
    {
        DrawContainerType();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("TextAssets ({0})", textAssetTypes), extStyle);
        EditorGUILayout.EndHorizontal();

        serializedObject.Update();

        TextAssetContainer assetContainer = (TextAssetContainer)target;

        SerializedProperty list = serializedObject.FindProperty("TextAssetList");

        DrawAddSubfolder();

        EditorGUI.BeginChangeCheck();

        DrawContainerPropertiesToggle();

        #region TextAsset Box
        EditorGUILayout.BeginVertical(areaStyle);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Add Folder", addFolderTooltip), btnStyle))
        {
            OnAddFolder<TextAsset>(textAssetTypes);
        }
        if (GUILayout.Button(new GUIContent("Reorganise", reorganiseTooltip), btnStyle))
        {
            OnReorganise<TextAsset>();
        }
        if (GUILayout.Button("Add Slots", btnStyle))
        {
            addTextAssetSlots = !addTextAssetSlots;
        }
        EditorGUILayout.EndHorizontal();

        if (addTextAssetSlots)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("AddTextAssetSlots");
            _newTextAssetSlots = EditorGUILayout.IntField("Slots to add: ", _newTextAssetSlots);
            if (IsKeyPressed("AddTextAssetSlots", KeyCode.Return))
            {
                if (_newTextAssetSlots > 0)
                {
                    OnAddSlots<TextAsset>(_newTextAssetSlots);
                }
                _newTextAssetSlots = 0;
                addTextAssetSlots = false;
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
}
