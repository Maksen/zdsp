using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AvatarPartContainer))]
[CanEditMultipleObjects]
public class AssetContainerEditor : BaseAssetContainerEditor
{
    private bool addMaterialSlots = false;
    private int _newMaterialSlots;

    private bool addMeshSlots = false;
    private int _newMeshSlots;

    const string materialExt = ".mat";
    const string meshExt = ".fbx";

    public override void OnInspectorGUI()
    {
        DrawContainerType();

        EditorGUILayout.BeginHorizontal();        
        GUILayout.Label(string.Format("Materials ({0})", materialExt), extStyle);
        GUILayout.Label(string.Format("Meshes ({0})", meshExt), extStyle);
        EditorGUILayout.EndHorizontal();

        serializedObject.Update();

        AvatarPartContainer assetContainer = (AvatarPartContainer)target;

        SerializedProperty list1 = serializedObject.FindProperty("MaterialList");
        SerializedProperty list2 = serializedObject.FindProperty("MeshList");

        DrawAddSubfolder();

        EditorGUI.BeginChangeCheck();

        DrawContainerPropertiesToggle();

        #region Material Box
        EditorGUILayout.BeginVertical(areaStyle);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Add Folder", addFolderTooltip), btnStyle))
        {
            OnAddFolder<Material>(materialExt);
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
            if(IsKeyPressed("AddMaterialSlots", KeyCode.Return))
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

        EditorGUILayout.PropertyField(list1, true);
        EditorGUILayout.EndVertical();
        #endregion

        #region Mesh Box
        EditorGUILayout.BeginVertical(areaStyle);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Add Folder", addFolderTooltip), btnStyle))
        {
            OnAddFolder<Mesh>(meshExt);
        }
        if (GUILayout.Button(new GUIContent("Reorganise", reorganiseTooltip), btnStyle))
        {
            OnReorganise<Mesh>();
        }
        if (GUILayout.Button("Add Slots", btnStyle))
        {
            addMeshSlots = !addMeshSlots;
        }
        EditorGUILayout.EndHorizontal();

        if (addMeshSlots)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("AddMeshSlots");
            _newMeshSlots = EditorGUILayout.IntField("Slots to add: ", _newMeshSlots);
            if (IsKeyPressed("AddMeshSlots", KeyCode.Return))
            {
                if (_newMeshSlots > 0)
                {
                    OnAddSlots<Mesh>(_newMeshSlots);
                }
                _newMeshSlots = 0;
                addMeshSlots = false;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.PropertyField(list2, true);
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