using UnityEngine;
using UnityEngine.UI;

public class UI_Cutscene : BaseWindowBehaviour
{
    [SerializeField]
    Button Skip;
    
    public void Init(bool active)
    {
        Skip.gameObject.SetActive(active);
    }

    public void OnClickSkip()
    {
        if (GameInfo.gCombat != null && GameInfo.gCombat.CutsceneManager != null)
        {
            GameInfo.gCombat.CutsceneManager.SkipCutscene();
        }
    }
}
