using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteContainer))]
[CanEditMultipleObjects]
public class SpriteContainerEditor : BaseAssetContainerEditor
{
    private bool addSpriteSlots = false;
    private int _newSpriteSlots;

    const string spriteTypes = ".png;.tif";

    public override void OnInspectorGUI()
    {
        DrawContainerType();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Sprites ({0})", spriteTypes), extStyle);
        EditorGUILayout.EndHorizontal();

        serializedObject.Update();

        SpriteContainer assetContainer = (SpriteContainer)target;

        SerializedProperty list = serializedObject.FindProperty("SpriteList");

        DrawAddSubfolder();

        EditorGUI.BeginChangeCheck();

        DrawContainerPropertiesToggle();

        #region Sprite Box
        EditorGUILayout.BeginVertical(areaStyle);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Add Folder", addFolderTooltip), btnStyle))
        {
            OnAddFolder<Sprite>(spriteTypes);
        }
        if (GUILayout.Button(new GUIContent("Reorganise", reorganiseTooltip), btnStyle))
        {
            OnReorganise<Sprite>();
        }
        if (GUILayout.Button("Add Slots", btnStyle))
        {
            addSpriteSlots = !addSpriteSlots;
        }
        EditorGUILayout.EndHorizontal();

        if (addSpriteSlots)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("AddSpriteSlots");
            _newSpriteSlots = EditorGUILayout.IntField("Slots to add: ", _newSpriteSlots);
            if (IsKeyPressed("AddSpriteSlots", KeyCode.Return))
            {
                if (_newSpriteSlots > 0)
                {
                    OnAddSlots<Sprite>(_newSpriteSlots);
                }
                _newSpriteSlots = 0;
                addSpriteSlots = false;
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
