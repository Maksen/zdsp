using UnityEngine;
using UnityEngine.UI;

public class UI_Cutscene_Skip : BaseWindowBehaviour
{
    [SerializeField]
    Button Skip;

    public static UI_Cutscene_Skip instance = null;
    UI_Cutscene_Skip()
    {
        instance = this;
    }

    public void Init(bool active)
    {
        // Skip.gameObject.SetActive(active);
    }

    public void OnClickSkip()
    {
        if (CutsceneManager.instance != null)
            CutsceneManager.instance.SkipCutscene();

        gameObject.SetActive(false);
    }
}
