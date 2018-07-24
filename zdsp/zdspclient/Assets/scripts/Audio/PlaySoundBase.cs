using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zealot.Audio
{
    public class PlaySoundBase : MonoBehaviour
    {
        public bool loadFromScene = false;
        public string audioClip;
        public AudioClip audioClip_scene;
        public bool playOneShot = true;

        protected void PlayAudioClip()
        {
            if (loadFromScene && audioClip_scene != null)
            {
                if (playOneShot)
                    SoundFX.Instance.PlayOneShot(audioClip_scene);
                else
                    SoundFX.Instance.Play(audioClip_scene);
            }
            else if (!loadFromScene && !string.IsNullOrEmpty(audioClip))
            {
                if (playOneShot)
                    SoundFX.Instance.PlayOneShot(audioClip);
                else
                    SoundFX.Instance.Play(audioClip);
            }
        }

#if UNITY_EDITOR
        public List<ExportedAsset> GetAudioClips()
        {
            var container = AssetManager.GetAssetContainer(SoundFX.assetContainerName);
            return container.GetExportedAssets();
        }

        public string GetAudioClipName()
        {
            return audioClip;
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PlaySoundBase), true)]
    [CanEditMultipleObjects]
    public class PlaySoundBaseEditor : Editor
    {
        SerializedProperty loadFromScene;
        SerializedProperty audioClip;
        SerializedProperty audioClip_scene;
        SerializedProperty playOneShot;

        List<string> clipOptions = new List<string> { "None" };
        List<string> clipPaths = new List<string> { "" };
        string clipName;
        int selectedClipIndex;

        private void OnEnable()
        {
            loadFromScene = serializedObject.FindProperty("loadFromScene");
            audioClip = serializedObject.FindProperty("audioClip");
            audioClip_scene = serializedObject.FindProperty("audioClip_scene");
            clipName = ((PlaySoundBase)target).GetAudioClipName();
            OnUpdateAudioClipsDropDown();
            playOneShot = serializedObject.FindProperty("playOneShot");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(loadFromScene);

            if (loadFromScene.boolValue)
            {
                EditorGUILayout.PropertyField(audioClip_scene, new GUIContent("Audio Clip"));
            }
            else
            {
                selectedClipIndex = EditorGUILayout.Popup("Audio Clip", selectedClipIndex, clipOptions.ToArray());
                audioClip.stringValue = clipPaths[selectedClipIndex];
            }

            EditorGUILayout.PropertyField(playOneShot);

            if (loadFromScene.boolValue)
                EditorGUILayout.HelpBox("Loading from Scene.", MessageType.Info);
            else
                EditorGUILayout.HelpBox("Loading via AssetBundle", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnUpdateAudioClipsDropDown()
        {
            List<ExportedAsset> clips = ((PlaySoundBase)target).GetAudioClips();
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i] != null)
                {
                    clipOptions.Add(clips[i].asset.name);
                    clipPaths.Add(clips[i].assetPath);
                    if (clips[i].assetPath == clipName)
                        selectedClipIndex = i + 1;
                }
            }
        }
    }
#endif
}

