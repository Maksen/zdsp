using Candlelight.UI;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Chatbox : MonoBehaviour
{
    [SerializeField]
    Text txtPlayername = null;

    [SerializeField]
    HyperText hypertxtMessage = null;

    public void Init(string playerName, string chatMessage)
    {
        if (txtPlayername != null)
            txtPlayername.text = playerName;

        hypertxtMessage.text = chatMessage;
        hypertxtMessage.ClickedLink.AddListener((a, b) => { GameInfo.gCombat.OnClickHyperText(a, b); });
    }
}
