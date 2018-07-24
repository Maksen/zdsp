using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Zealot.Common;
using Zealot.Repository;

public class ServerListData : MonoBehaviour
{
    // Editor Linked Gameobjects
    [SerializeField]
    Text txtServerlistName = null;

    public ServerLine serverLineInfo;

    public void Init(ServerLine serverLineInfo, ToggleGroup toggleGrp, UnityAction<bool> onValueChanged = null)
    {
        this.serverLineInfo = serverLineInfo;
        string text = serverLineInfo.displayName;
        if (serverLineInfo.recommend)
            text += " " + GUILocalizationRepo.GetLocalizedString("com_Recommend");
        txtServerlistName.text = text;

        Toggle toggle = GetComponent<Toggle>();
        toggle.group = toggleGrp;
        if (onValueChanged != null)
            toggle.onValueChanged.AddListener(onValueChanged);
    }
}
