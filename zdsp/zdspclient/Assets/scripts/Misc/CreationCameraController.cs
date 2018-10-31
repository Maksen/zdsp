using UnityEngine;
using Zealot.Common;

public class CreationCameraController : MonoBehaviour
{
    [SerializeField]
    Animator CharacterAnimator;

    private int mType;

    public void Init()
    {
        mType = 3;
    }

    public void ChangeCamera(int type, Gender gender)
    {
        if (mType != type)
        {
            string animation = GetAppearanceName(mType) + "To" + GetAppearanceName(type);
            animation = gender == Gender.Male ? animation : "Female_" + animation;
            mType = type;
            CharacterAnimator.Play(animation);
        }
    }

    public void SetCamera(Gender gender)
    {
        string animation = GetAppearanceName(mType);
        animation = gender == Gender.Male ? "Male" + animation : "Female" + animation;
        CharacterAnimator.Play(animation);
    }

    private string GetAppearanceName(int type)
    {
        switch (type)
        {
            case 0:
                return "Hair";
            case 1:
                return "Face";
            case 2:
                return "Body";
            default:
                return "Outfit";
        }
    }
}
