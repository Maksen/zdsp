using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScrollRectAnimation : MonoBehaviour
{

    [SerializeField]
    Animator animator = null;
    [SerializeField]
    string animationName = null;
    [SerializeField]
    ScrollRect scrollRect = null;

    [SerializeField]
    bool invert = true;

    [SerializeField, Header("false for Horizontal")]
    bool isVertical = true;

    // Use this for initialization
    void Start()
    {
        var clip = GetAnimationClip(animationName);
        scrollRect.onValueChanged.AddListener((Vector2 vec) =>
        {
            float value;

            if (isVertical)
                value = scrollRect.verticalNormalizedPosition;
            else
                value = scrollRect.horizontalNormalizedPosition;

            value /= clip.length;

            if (invert)
                value = 1.0f - value;

            clip.SampleAnimation(animator.gameObject, value);

            animator.enabled = false;
        });
    }


    public AnimationClip GetAnimationClip(string name)
    {
        if (!animator) return null; // no animator

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == name)
            {
                return clip;
            }
        }
        return null; // no clip by that name
    }

}
