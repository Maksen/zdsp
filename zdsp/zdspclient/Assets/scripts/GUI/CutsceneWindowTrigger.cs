using UnityEngine;

public class CutsceneWindowTrigger : MonoBehaviour
{
    public void OnEnable()
    {
        OpenCharacterCreationUI();
    }

    private void OpenCharacterCreationUI()
    {
        GameInfo.gLobby.StartCharacterCreation();
    }
}
