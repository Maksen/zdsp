using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Audio;

public class UI_AudioClueMessage : MonoBehaviour
{
    [SerializeField]
    Button Play;

    private AudioClip mAudioClip;
    private float mAudioDuration;
    private AudioSource mAudioSource;

    public void Init(string audioclip, AudioSource audioSource)
    {
        mAudioSource = audioSource;
        mAudioClip = ClientUtils.LoadAudio(audioclip);
        PlaySound();
    }

    private void PlaySound()
    {
        if (mAudioClip != null)
        {
            SoundFX.Instance.MuteSound(true);
            Play.interactable = false;
            mAudioDuration = mAudioClip.length;
            mAudioSource.PlayOneShot(mAudioClip);
            mAudioSource.loop = false;
            StartCoroutine(WaitSoundEnd());
        }
    }

    private IEnumerator WaitSoundEnd()
    {
        yield return new WaitForSeconds(mAudioDuration);
        StopPlay();
    }

    public void OnClickPlay()
    {
        PlaySound();
    }

    private void StopPlay()
    {
        SoundFX.Instance.MuteSound(false);
        Play.interactable = true;
        mAudioDuration = 0;
        if (mAudioSource != null)
        {
            mAudioSource.Stop();
        }
    }

    public void Clean()
    {
        StopPlay();
        mAudioClip = null;
        mAudioSource = null;
    }
}
