using UnityEngine;
using UnityEngine.Playables;

public class CharacterCreationManager : MonoBehaviour
{
    [SerializeField]
    GameObject Cutscene;

    [SerializeField]
    GameObject UITrigger;

    private void Awake()
    {
        GameInfo.gCharacterCreationManager = this;
    }

    public void PlayCutscene()
    {
        if (Cutscene != null)
        {
            Cutscene.SetActive(true);
            Cutscene.GetComponent<PlayableDirector>().Play();
        }
    }

    public void StopCutScene()
    {
        if (Cutscene != null && UITrigger != null)
        {
            Cutscene.SetActive(false);
            UITrigger.SetActive(false);
        }
    }
}
