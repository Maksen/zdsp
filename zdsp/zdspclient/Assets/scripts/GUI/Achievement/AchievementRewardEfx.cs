using UnityEngine;

public class AchievementRewardEfx : MonoBehaviour
{
    [SerializeField] Animator animator;

    private UI_Achievement parent;

    public void SetParent(UI_Achievement myParent)
    {
        parent = myParent;
    }

    public void Play(int index)
    {
        gameObject.SetActive(true);
        animator.Play("FxSequence_" + index);
    }

    public void OnFlyToProgressBar()
    {
        parent.UpdateLevelProgressFromClient();
    }
}