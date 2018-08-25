using System;
using UnityEngine;

public static class ZealotExtensions
{
    public static bool HasState(this Animator animator, string state)
    {
        return animator.HasState(0, Animator.StringToHash(state));
    }

    public static AnimationClip GetClip(this Animator animator, string clipname)
    {
        return Array.Find(animator.runtimeAnimatorController.animationClips, clip => clip.name == clipname);
    }

    public static bool IsPlaying(this Animator animator, string state)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(state) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }

    public static void PlayFromStart(this Animator animator, string state)
    {
        animator.Play(state, -1, 0);
    }
}