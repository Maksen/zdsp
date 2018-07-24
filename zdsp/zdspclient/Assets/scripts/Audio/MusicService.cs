using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zealot.Audio
{
    public class MusicService : MonoBehaviour
    {
        public bool loadFromScene = false;
        public string audioClip;
        public AudioClip audioClip_scene;
        public bool fadeIn = false;
        public float fadeInDuration;
        public float fadeOutDuration;

        public void FadeInMusic(AudioClip clip)
        {
            Music.Instance.FadeInMusic(clip, fadeInDuration, fadeOutDuration);
        }

        public void FadeInMusic(string name)
        {
            Music.Instance.FadeInMusic(name, fadeInDuration, fadeOutDuration);
        }

        public void PlayMusicOnce(AudioClip clip)
        {
            Music.Instance.Reset();
            Music.Instance.PlayMusicOnce(clip);
        }

        public void PlayMusicOnce(string name)
        {
            Music.Instance.Reset();
            Music.Instance.PlayMusicOnce(name);
        }

        public void PlayMusicLoop(AudioClip clip)
        {
            Music.Instance.Reset();
            Music.Instance.PlayMusicLoop(clip);
        }

        public void PlayMusicLoop(string name)
        {
            Music.Instance.Reset();
            Music.Instance.PlayMusicLoop(name);
        }

        public void Play()
        {
            if (loadFromScene && audioClip_scene != null)
            {
                if (fadeIn)
                    FadeInMusic(audioClip_scene);
                else
                    PlayMusicLoop(audioClip_scene);
            }
            else if (!loadFromScene && !string.IsNullOrEmpty(audioClip))
            {
                if (fadeIn)
                    FadeInMusic(audioClip);
                else
                    PlayMusicLoop(audioClip);
            };
        }

#if UNITY_EDITOR
        public List<ExportedAsset> GetAudioClips()
        {
            var container = AssetManager.GetAssetContainer(Music.assetContainerName);
            return container.GetExportedAssets();
        }

        public string GetAudioClipName()
        {
            return audioClip;
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MusicService))]
    [CanEditMultipleObjects]
    public class MusicServiceEditor : Editor
    {
        SerializedProperty loadFromScene;
        SerializedProperty audioClip;
        SerializedProperty audioClip_scene;
        SerializedProperty fadeIn;
        SerializedProperty fadeInDuration;
        SerializedProperty fadeOutDuration;

        List<string> clipOptions = new List<string> { "None" };
        List<string> clipPaths = new List<string> { "" };
        string clipName;
        int selectedClipIndex;

        public void OnEnable()
        {
            loadFromScene = serializedObject.FindProperty("loadFromScene");
            audioClip = serializedObject.FindProperty("audioClip");
            audioClip_scene = serializedObject.FindProperty("audioClip_scene");
            clipName = ((MusicService)target).GetAudioClipName();
            OnUpdateAudioClipsDropDown();
            fadeIn = serializedObject.FindProperty("fadeIn");
            fadeInDuration = serializedObject.FindProperty("fadeInDuration");
            fadeOutDuration = serializedObject.FindProperty("fadeOutDuration");
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

            EditorGUILayout.PropertyField(fadeIn);
            if (fadeIn.boolValue)
            {
                EditorGUILayout.PropertyField(fadeInDuration);
                EditorGUILayout.PropertyField(fadeOutDuration);
            }

            if (loadFromScene.boolValue)
                EditorGUILayout.HelpBox("Loading from Scene.", MessageType.Info);
            else
                EditorGUILayout.HelpBox("Loading via AssetBundle.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnUpdateAudioClipsDropDown()
        {
            List<ExportedAsset> clips = ((MusicService)target).GetAudioClips();
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