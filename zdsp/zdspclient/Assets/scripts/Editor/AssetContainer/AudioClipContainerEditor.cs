using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioClipContainer))]
[CanEditMultipleObjects]
public class AudioClipContainerEditor : BaseAssetContainerEditor
{
    private bool addAudioClipSlots = false;
    private int _newAudioClipSlots;

    const string audioClipTypes = ".wav;.mp3;.ogg";

    public override void OnInspectorGUI()
    {
        DrawContainerType();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("AudioClips ({0})", audioClipTypes), extStyle);
        EditorGUILayout.EndHorizontal();

        serializedObject.Update();

        AudioClipContainer assetContainer = (AudioClipContainer)target;

        SerializedProperty list = serializedObject.FindProperty("AudioClipList");
       

        DrawAddSubfolder();

        EditorGUI.BeginChangeCheck();

        DrawContainerPropertiesToggle();

        #region AudioClip Box
        EditorGUILayout.BeginVertical(areaStyle);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Add Folder", addFolderTooltip), btnStyle))
        {
            OnAddFolder<AudioClip>(audioClipTypes);
        }
        if (GUILayout.Button(new GUIContent("Reorganise", reorganiseTooltip), btnStyle))
        {
            OnReorganise<AudioClip>();
        }
        if (GUILayout.Button("Add Slots", btnStyle))
        {
            addAudioClipSlots = !addAudioClipSlots;
        }
        EditorGUILayout.EndHorizontal();

        if (addAudioClipSlots)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("AddAudioClipSlots");
            _newAudioClipSlots = EditorGUILayout.IntField("Slots to add: ", _newAudioClipSlots);
            if (IsKeyPressed("AddAudioClipSlots", KeyCode.Return))
            {
                if (_newAudioClipSlots > 0)
                {
                    OnAddSlots<AudioClip>(_newAudioClipSlots);
                }
                _newAudioClipSlots = 0;
                addAudioClipSlots = false;
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
