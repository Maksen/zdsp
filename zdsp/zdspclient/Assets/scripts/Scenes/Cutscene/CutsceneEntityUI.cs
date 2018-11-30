using UnityEngine;
using System.Collections;

public class CutsceneEntityUI : MonoBehaviour
{
    void Start()
    {

    }

    public void PlayUICutscene()
    {
        if(CutsceneManager.instance == null)
        {
            return;
        }

        CutsceneManager.instance.PlayUICutscene();
        gameObject.SetActive(false);
    }
}
