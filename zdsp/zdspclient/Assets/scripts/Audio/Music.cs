using System;
using System.Collections;
using UnityEngine;

namespace Zealot.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class Music : MonoBehaviour
    {
        public static Music Instance
        {
            get
            {
                if (instance == null)
                {
                    var objects = FindObjectsOfType<Music>();
                    if (objects.Length == 0)
                    {
                        var go = new GameObject("Music");
                        instance = go.AddComponent<Music>();
                        DontDestroyOnLoad(go);
                    }
                    else if (objects.Length >= 1)
                    {
                        instance = objects[0];
                        DontDestroyOnLoad(instance.gameObject);
                        for (int i = 1; i < objects.Length; i++)
                            DestroyImmediate(objects[i].gameObject);
                    }
                }
                return instance;
            }
        }

        private static Music instance;
        private AudioSource source;
        private float maxVolume;
        private float volume;
        private bool isFadingOut;
        private float timer;
        private Coroutine coroutiner;

        public const string assetContainerName = "Musics";

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            source = GetComponent<AudioSource>();
            maxVolume = source.volume;
            volume = maxVolume;
        }

        private void Start() { }

        public void SetVolume(float volumeScale)
        {
            volume = volumeScale * maxVolume;
            source.volume = volume;
        }

        public void PauseMusic()
        {
            source.Pause();
        }

        public void UnpauseMusic()
        {
            source.UnPause();
        }

        public void StopMusic()
        {
            source.Stop();
        }

        public void SetMusic(AudioClip clip)
        {
            source.clip = clip;
        }

        public void PlayMusicOnce()
        {
            if (source != null)
            {
                source.loop = false;
                source.Play();
            }
        }

        public void PlayMusicOnce(AudioClip clip)
        {
            if (source != null)
            {
                source.Stop();
                source.loop = false;
                source.clip = clip;
                source.Play();
            }
        }

        public void PlayMusicOnce(string name)
        {
            if (source != null)
            {
                source.Stop();
                source.loop = false;
                AssetLoader.Instance.LoadAsync<AudioClip>(AssetLoader.GetLoadString(assetContainerName, name), (clip) =>
                {
                    source.clip = clip;
                    source.Play();
                }, false);
            }
        }

        public void PlayMusicLoop()
        {
            if (source != null)
            {
                source.Stop();
                source.loop = true;
                source.Play();
            }
        }

        public void PlayMusicLoop(AudioClip clip)
        {
            if (source != null)
            {
                source.Stop();
                source.loop = true;
                source.clip = clip;
                source.Play();
            }
        }

        public void PlayMusicLoop(string name)
        {
            if (source != null)
            {
                source.Stop();
                source.loop = true;
                AssetLoader.Instance.LoadAsync<AudioClip>(AssetLoader.GetLoadString(assetContainerName, name), (clip) =>
                {
                    source.clip = clip;
                    source.Play();
                }, false);
            }
        }

        private IEnumerator FadeOut(float currentVol, float duration, Action afterFadeOutCallback = null)
        {
            while (source.volume > 0.0f)
            {
                source.volume = Mathf.SmoothStep(currentVol, 0.0f, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }

            source.Stop();
            source.volume = 0;
            isFadingOut = false;
            if (afterFadeOutCallback != null)
                afterFadeOutCallback();
        }

        public void FadeOutMusic(float duration, Action afterFadeOutCallback = null)
        {
            // stop fading in if any
            if (coroutiner != null)
                StopCoroutine(coroutiner);
            timer = 0;
            isFadingOut = true;
            coroutiner = StartCoroutine(FadeOut(source.volume, duration, afterFadeOutCallback));
        }

        private IEnumerator FadeIn(float currentVol, float duration)
        {
            while (source.volume < volume)
            {
                source.volume = Mathf.SmoothStep(currentVol, volume, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }

            source.volume = volume;
        }

        private void StartFadeIn(AudioClip clip, float duration)
        {
            isFadingOut = false;
            if (coroutiner != null)
                StopCoroutine(coroutiner);
            source.clip = clip;
            PlayMusicLoop();
            timer = 0;
            coroutiner = StartCoroutine(FadeIn(source.volume, duration));
        }

        public void FadeInMusic(string name, float fadeInDuration, float fadeOutDuration)
        {
            if (source.isPlaying && !isFadingOut)  // currently has music playing, fade out old music first
            {
                AssetLoader.Instance.LoadAsync<AudioClip>(AssetLoader.GetLoadString(assetContainerName, name), (clip) =>
                {
                    FadeOutMusic(fadeOutDuration, () => StartFadeIn(clip, fadeInDuration));
                }, false);
            }
            else if (source.isPlaying && isFadingOut) // currently music is fading out, stop fade out and start fade in
            {
                AssetLoader.Instance.LoadAsync<AudioClip>(AssetLoader.GetLoadString(assetContainerName, name), (clip) =>
                {
                    StartFadeIn(clip, fadeInDuration);
                }, false);
            }
            else  // no music playing, immediately fade in
            {
                source.volume = 0;
                AssetLoader.Instance.LoadAsync<AudioClip>(AssetLoader.GetLoadString(assetContainerName, name), (clip) =>
                {
                    StartFadeIn(clip, fadeInDuration);
                }, false);
            }
        }

        public void FadeInMusic(AudioClip clip, float fadeInDuration, float fadeOutDuration)
        {
            if (source.isPlaying && !isFadingOut)  // currently has music playing, fade out old music first
            {
                FadeOutMusic(fadeOutDuration, () => StartFadeIn(clip, fadeInDuration));
            }
            else if (source.isPlaying && isFadingOut) // currently music is fading out, stop fade out and start fade in
            {
                StartFadeIn(clip, fadeInDuration);
            }
            else  // no music playing, immediately fade in
            {
                source.volume = 0;
                StartFadeIn(clip, fadeInDuration);
            }
        }

        public bool IsPlaying(string name)
        {
            if (source.clip != null)
            {
                if (source.clip.name == name && source.isPlaying)
                    return true;
            }

            return false;
        }

        public bool IsPlaying()
        {
            return source.isPlaying;
        }

        public void Reset()
        {
            if (coroutiner != null)
                StopCoroutine(coroutiner);
            isFadingOut = false;
            source.Stop();
            source.clip = null;
            source.volume = volume;
            source.loop = true;
        }
    }
}
