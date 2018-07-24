using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;

public class OpenPartyWindowButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnOpenClick);
    }

    private void OnOpenClick()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
        {
            bool hasParty = player.PlayerSynStats.Party > 0;
            UIManager.OpenWindow(WindowType.Party, (window) => window.GetComponent<UI_Party>().SetUp(hasParty));
        }
    }
}
