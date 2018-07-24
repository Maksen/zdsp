using UnityEngine;
using System.Collections.Generic;
using System;
using Zealot.Audio;

[Serializable]
public class AnimationSound
{
    public string animationClipName;
    public AudioClip audioClip;
}

[RequireComponent(typeof(Animation))]
public class PlayAnimationWithSound : MonoBehaviour
{
    public AudioClip talk;

    public List<AnimationSound> soundList;

    private void PlayClip(AudioClip audioClip)
    {
        if (!SoundFX.Instance.IsPlaying())
            SoundFX.Instance.Play(audioClip);
    }

    public void PlayAnimation(string clip)
    {
        Animator animator = GetComponent<Animator>();   
        if (!animator.HasState(clip))
        {
            Debug.Log("Animation clip: " + clip + " does not exist");
            return;
        }

        if (!animator.IsPlaying(clip))
        {
            animator.CrossFade(clip, 0.2f);

            for (int index = 0; index < soundList.Count; index++)
            {
                if (soundList[index].animationClipName == clip)
                {
                    PlayClip(soundList[index].audioClip);
                    return;
                }
            }

            if (clip == "talk")
                PlayClip(talk);
        }
    }
}
