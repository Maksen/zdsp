using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;
using Zealot.Audio;

public class VoicePendingInfo
{
    public string clipName;
    public int duration;
    public MessageType msgType;

    public VoicePendingInfo(string clipName, int duration, MessageType msgType)
    {
        this.clipName = clipName;
        this.duration = duration;
        this.msgType = msgType;
    }
}

[RequireComponent(typeof(AudioSource))]
public class VoiceChatManager : MonoSingleton<VoiceChatManager>
{
    float mAudioVolume;
    bool mIsMute = false;

    AudioSource audioSource = null;  
    int mTotalPendingDuration = 0;
    int mPendingDurationLimit = 60;
    List<VoicePendingInfo> mPendingVoiceList = new List<VoicePendingInfo>();

    void Awake()
    {
        mPendingDurationLimit = GameConstantRepo.GetConstantInt("VoiceChat_PendingLimit", 60);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    protected override void OnDestroy()
    {
        audioSource = null;
        base.OnDestroy();
    }

    public bool IsPlaying()
    {
        return (audioSource != null) ? audioSource.isPlaying : false;
    }

    public void PlayVoice()
    {
        if (audioSource != null)
        {
            if (GameSettings.SoundFXEnabled)
                SoundFX.Instance.MuteSound(true);

            if (GameSettings.MusicEnabled)
                Music.Instance.PauseMusic();

            audioSource.Play();
        }
    }

    public void StopPlaying()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    public string GetClipName()
    {
        return (audioSource != null && audioSource.clip != null) ? audioSource.clip.name : "";
    }

    public void SetAudioClip(AudioClip clip, string clipName)
    {
        if (audioSource != null)
        {
            audioSource.clip = clip;
            if (audioSource.clip != null)
                audioSource.clip.name = clipName;
        }
    }

    public int GetAudioState()
    {
        return (audioSource != null && audioSource.clip != null) ? (int)audioSource.clip.loadState : -1;
    }

    public void MuteAudioListener()
    {
        mAudioVolume = AudioListener.volume;
        AudioListener.volume = 0.0f;
        mIsMute = true;
    }

    public void RestoreAudioListener()
    {
        if (mIsMute)
        {
            AudioListener.volume = mAudioVolume;
            mIsMute = false;
        }
    }

    public void OnPlayVoiceEnd()
    {
        if (GameSettings.SoundFXEnabled)
            SoundFX.Instance.MuteSound(false);

        if (GameSettings.MusicEnabled)
            Music.Instance.UnpauseMusic();
    }

    public void EnqueueVoice(string voice, int duration, MessageType msgType)
    {
        if (mTotalPendingDuration >= mPendingDurationLimit || mTotalPendingDuration+duration >= mPendingDurationLimit + 5)
            return;
        VoicePendingInfo info = new VoicePendingInfo(voice, duration, msgType);
        mPendingVoiceList.Add(info);
        mTotalPendingDuration += duration;
    }

    public string DequeueVoice()
    {
        if (mPendingVoiceList.Count > 0)
        {
            VoicePendingInfo info = mPendingVoiceList[0];
            mPendingVoiceList.RemoveAt(0);
            mTotalPendingDuration -= info.duration;
            return info.clipName;
        }
        return null;
    }

    public void RemoveVoice(string name)
    {
        for (int index = mPendingVoiceList.Count-1; index >= 0; --index)
        {
            if (mPendingVoiceList[index].clipName == name)
            {
                mTotalPendingDuration -= mPendingVoiceList[index].duration;
                mPendingVoiceList.RemoveAt(index);
                break;
            }
        }
    }

    public void RemoveAllVoiceByChannel(MessageType msgType)
    {
        for (int index = mPendingVoiceList.Count-1; index >= 0; --index)
        {
            if (mPendingVoiceList[index].msgType == msgType)
            {
                mTotalPendingDuration -= mPendingVoiceList[index].duration;
                mPendingVoiceList.RemoveAt(index);
            }
        }
    }

    public void RemoveVoiceAll()
    {
        mTotalPendingDuration = 0;
        mPendingVoiceList.Clear();
    }

    public void CleanUp()
    {
        StopPlaying();
        RestoreAudioListener();
        RemoveVoiceAll();
    }
}
