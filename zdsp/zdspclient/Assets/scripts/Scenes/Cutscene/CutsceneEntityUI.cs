using UnityEngine;
using System.Collections;

public class CutsceneEntityUI : MonoBehaviour
{
    void Start()
    {

    }

    public void PlayUICutscene()
    {
        if(GameInfo.gCombat.CutsceneManager == null)
        {
            return;
        }

        GameInfo.gCombat.CutsceneManager.PlayUICutscene();
        gameObject.SetActive(false);
    }
}
