using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zealot.Common;

public class ServerData : MonoBehaviour
{
    [SerializeField]
    Toggle toggle = null;
    [SerializeField]
    GameObject gameobjRecommended = null;
    [SerializeField]
    Text txtServerName = null;
    [SerializeField]
    Image[] imgJobSlots = null;

    public void Init(ServerInfo serverInfo, ServerLine serverLineInfo, ToggleGroup toggleGrp, UnityAction<bool> onValueChanged = null)
    {
        gameobjRecommended.SetActive(serverLineInfo.recommended);
        txtServerName.text = ClientUtils.GetServerNameWithColor(serverInfo.serverLoad, serverLineInfo.displayName, serverInfo.serverName);

        toggle.group = toggleGrp;
        toggle.onValueChanged.AddListener(onValueChanged);
    }
}
