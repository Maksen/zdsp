using UnityEngine;
using UnityEngine.UI;

public class MeridianToggleData : MonoBehaviour {

    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Text typeName;
    [SerializeField]
    private GameObject typeLevel;
    [SerializeField]
    private Text effectName;
    [SerializeField]
    private Text effectValue;


    public void Init(string mAnimator, string mTypeName, int mPartLevel, string mEffectName, string mEffectValue)
    {
        animator.Play(mAnimator);
        typeName.text = mTypeName;
        if (mPartLevel == 0)
            typeLevel.SetActive(false);
        else
        {
            typeLevel.SetActive(true);
            typeLevel.GetComponent<Text>().text = string.Format("{0}{1}", "LV", mPartLevel.ToString());
        }
        effectName.text = mEffectName;
        effectValue.text = mEffectValue;
    }

    public void PlayAnime(string animation)
    {
        animator.Play(animation);
    }

    public void EffectValueChange(int value)
    {
        effectValue.text = value.ToString();
    }
}
