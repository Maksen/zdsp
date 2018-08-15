using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

[CustomEditor(typeof(VideoClipContainer))]
[CanEditMultipleObjects]
public class VideoClipContainerEditor : BaseAssetContainerEditor
{
    private bool addVideoClipSlots = false;
    private int _newVideoClipSlots;

    const string videoClipTypes = ".mp4";

    public override void OnInspectorGUI()
    {
        DrawContainerType();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("VideoClips ({0})", videoClipTypes), extStyle);
        EditorGUILayout.EndHorizontal();

        serializedObject.Update();

        VideoClipContainer assetContainer = (VideoClipContainer)target;

        SerializedProperty list = serializedObject.FindProperty("VideoClipList");


        DrawAddSubfolder();

        EditorGUI.BeginChangeCheck();

        DrawContainerPropertiesToggle();

        #region VideoClip Box
        EditorGUILayout.BeginVertical(areaStyle);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Add Folder", addFolderTooltip), btnStyle))
        {
            OnAddFolder<VideoClip>(videoClipTypes);
        }
        if (GUILayout.Button(new GUIContent("Reorganise", reorganiseTooltip), btnStyle))
        {
            OnReorganise<AudioClip>();
        }
        if (GUILayout.Button("Add Slots", btnStyle))
        {
            addVideoClipSlots = !addVideoClipSlots;
        }
        EditorGUILayout.EndHorizontal();

        if (addVideoClipSlots)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("AddVideoClipSlots");
            _newVideoClipSlots = EditorGUILayout.IntField("Slots to add: ", _newVideoClipSlots);
            if (IsKeyPressed("AddVideoClipSlots", KeyCode.Return))
            {
                if (_newVideoClipSlots > 0)
                {
                    OnAddSlots<AudioClip>(_newVideoClipSlots);
                }
                _newVideoClipSlots = 0;
                addVideoClipSlots = false;
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
