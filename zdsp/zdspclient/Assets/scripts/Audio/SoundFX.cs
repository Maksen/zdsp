using UnityEngine;

namespace Zealot.Audio
{
    [RequireComponent(typeof(AudioListener))]
    [RequireComponent(typeof(AudioSource))]
    public class SoundFX : MonoBehaviour
    {
        public static SoundFX Instance
        {
            get
            {
                if (instance == null)
                {
                    var objects = FindObjectsOfType<SoundFX>();
                    if (objects.Length == 0)
                    {
                        var go = new GameObject("SoundFX");
                        instance = go.AddComponent<SoundFX>();
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

        private static SoundFX instance;
        private AudioSource source;
        private float maxVolume;
        private bool isPlaying;

        public const string assetContainerName = "UI_ZDSP_Sound";

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            source = GetComponent<AudioSource>();
            source.loop = false;
            maxVolume = source.volume;
            isPlaying = false;
        }

        void Start() { }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void SetVolume(float volumeScale)
        {
            source.volume = volumeScale * maxVolume;
        }

        public float GetVolume()
        {
            return source.volume;
        }

        public void PlayOneShot(AudioClip clip)
        {
            Instance.PlaySoundOneShot(clip);
        }

        public void PlayOneShot(AudioClip clip, float volume)
        {
            Instance.PlaySoundOneShot(clip, volume);
        }

        public void PlayOneShot(string clipName)
        {
            Instance.PlaySoundOneShot(clipName);
        }

        private void PlaySoundOneShot(AudioClip clip)
        {
            if (clip != null)
                source.PlayOneShot(clip);
        }

        private void PlaySoundOneShot(AudioClip clip, float volume)
        {
            if (clip != null)
                source.PlayOneShot(clip, volume);
        }

        private void PlaySoundOneShot(string clipName)
        {
            AudioClip clip = AssetLoader.Instance.Load<AudioClip>(AssetLoader.GetLoadString(assetContainerName, clipName));
            if (clip != null)
                source.PlayOneShot(clip);
        }

        public void Play(AudioClip clip)
        {
            Instance.PlaySound(clip);
        }

        public void Play(AudioClip clip, float volume)
        {
            Instance.PlaySound(clip, volume);
        }

        public void Play(string name)
        {
            Instance.PlaySound(name);
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                source.clip = clip;
                source.Play();
                CancelInvoke("ResetIsPlaying");
                isPlaying = true;
                Invoke("ResetIsPlaying", clip.length);
            }
        }

        private void PlaySound(AudioClip clip, float volume)
        {
            if (clip != null)
            {
                source.volume = volume;
                source.clip = clip;
                source.Play();
                CancelInvoke("ResetIsPlaying");
                isPlaying = true;
                Invoke("ResetIsPlaying", clip.length);
            }
        }

        private void PlaySound(string name)
        {
            AudioClip clip = AssetLoader.Instance.Load<AudioClip>(AssetLoader.GetLoadString(assetContainerName, name));
            if (clip != null)
            {
                source.clip = clip;
                source.Play();
                CancelInvoke("ResetIsPlaying");
                isPlaying = true;
                Invoke("ResetIsPlaying", clip.length);
            }
        }

        public void StopSound()
        {
            source.Stop();
        }

        public void MuteSound(bool val)
        {
            source.mute = val;
        }

        public void SetLoop(bool loop)
        {
            source.loop = loop;
        }

        public bool IsPlaying()
        {
            return isPlaying;
        }

        private void ResetIsPlaying()
        {
            isPlaying = false;
            source.clip = null;
        }

        public void CleanUp()
        {
            StopSound();
            if (isPlaying)
            {
                CancelInvoke("ResetIsPlaying");
                ResetIsPlaying();
            }
        }

        public AudioSource GetAudioSource()
        {
            return source;
        }
    }
}
