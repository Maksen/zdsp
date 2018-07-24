using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MaterialContainer))]
[CanEditMultipleObjects]
public class MaterialContainerEditor : BaseAssetContainerEditor
{
    private bool addMaterialSlots = false;
    private int _newMaterialSlots;

    const string materialTypes = ".mat";

    public override void OnInspectorGUI()
    {
        DrawContainerType();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Materials ({0})", materialTypes), extStyle);
        EditorGUILayout.EndHorizontal();

        serializedObject.Update();

        MaterialContainer assetContainer = (MaterialContainer)target;

        SerializedProperty list = serializedObject.FindProperty("MaterialList");

        DrawAddSubfolder();

        EditorGUI.BeginChangeCheck();

        DrawContainerPropertiesToggle();

        #region Material Box
        EditorGUILayout.BeginVertical(areaStyle);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Add Folder", addFolderTooltip), btnStyle))
        {
            OnAddFolder<Material>(materialTypes);
        }
        if (GUILayout.Button(new GUIContent("Reorganise", reorganiseTooltip), btnStyle))
        {
            OnReorganise<Material>();
        }
        if (GUILayout.Button("Add Slots", btnStyle))
        {
            addMaterialSlots = !addMaterialSlots;
        }
        EditorGUILayout.EndHorizontal();

        if (addMaterialSlots)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("AddMaterialSlots");
            _newMaterialSlots = EditorGUILayout.IntField("Slots to add: ", _newMaterialSlots);
            if (IsKeyPressed("AddMaterialSlots", KeyCode.Return))
            {
                if (_newMaterialSlots > 0)
                {
                    OnAddSlots<Material>(_newMaterialSlots);
                }
                _newMaterialSlots = 0;
                addMaterialSlots = false;
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
