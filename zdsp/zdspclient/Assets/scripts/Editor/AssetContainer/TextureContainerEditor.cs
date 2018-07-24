using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextureContainer))]
[CanEditMultipleObjects]
public class TextureContainerEditor : BaseAssetContainerEditor
{
    private bool addTextureSlots = false;
    private int _newTextureSlots;

    const string textureTypes = ".png;.tga";

    public override void OnInspectorGUI()
    {
        DrawContainerType();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Textures ({0})", textureTypes), extStyle);
        EditorGUILayout.EndHorizontal();

        serializedObject.Update();

        TextureContainer assetContainer = (TextureContainer)target;

        SerializedProperty list = serializedObject.FindProperty("TextureList");

        DrawAddSubfolder();

        EditorGUI.BeginChangeCheck();

        DrawContainerPropertiesToggle();

        #region Texture Box
        EditorGUILayout.BeginVertical(areaStyle);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Add Folder", addFolderTooltip), btnStyle))
        {
            OnAddFolder<Texture>(textureTypes);
        }
        if (GUILayout.Button(new GUIContent("Reorganise", reorganiseTooltip), btnStyle))
        {
            OnReorganise<Texture>();
        }
        if (GUILayout.Button("Add Slots", btnStyle))
        {
            addTextureSlots = !addTextureSlots;
        }
        EditorGUILayout.EndHorizontal();

        if (addTextureSlots)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("AddTextureSlots");
            _newTextureSlots = EditorGUILayout.IntField("Slots to add: ", _newTextureSlots);
            if (IsKeyPressed("AddTextureSlots", KeyCode.Return))
            {
                if (_newTextureSlots > 0)
                {
                    OnAddSlots<Texture>(_newTextureSlots);
                }
                _newTextureSlots = 0;
                addTextureSlots = false;
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

