using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_DialogSettings_Account : MonoBehaviour
{
    [SerializeField]
    Text txtUID = null;

    public void Init()
    {
        if (!string.IsNullOrEmpty(LoginData.Instance.LoginId))
            txtUID.text = LoginData.Instance.LoginId;
    }

    public void OnClickLogout()
    {
        UIManager.OpenDialog(WindowType.DialogAccountLoginType);
        UIManager.CloseDialog(WindowType.DialogSettings);
    }
}
