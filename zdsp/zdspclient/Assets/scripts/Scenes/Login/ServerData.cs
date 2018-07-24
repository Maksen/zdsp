using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zealot.Common;

public class ServerData : MonoBehaviour
{
    [SerializeField]
    Text txtServerName = null;

    public void Init(ServerInfo serverInfo, ToggleGroup toggleGrp, UnityAction<bool> onValueChanged = null)
    {    
        txtServerName.text = string.Format("<color={0}>{1}</color>", ClientUtils.GetServerStatusColor(serverInfo.serverLoad), serverInfo.serverName);

        Toggle toggle = GetComponent<Toggle>();
        toggle.group = toggleGrp;
        toggle.onValueChanged.AddListener(onValueChanged);
    }
}
