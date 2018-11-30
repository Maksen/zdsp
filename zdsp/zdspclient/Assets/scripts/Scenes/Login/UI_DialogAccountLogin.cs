using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_DialogAccountLogin : MonoBehaviour
{
    [SerializeField]
    InputField inputfieldUsername = null;
    [SerializeField]
    InputField inputfieldPassword = null;

    void OnEnable()
    {
        SetInputfieldSignIn("", "");
    }

    public void SetInputfieldSignIn(string username, string password)
    {
        inputfieldUsername.text = username;
        inputfieldPassword.text = password;
    }

    public bool TryGetInputfieldSignIn(out string username, out string password)
    {
        username = inputfieldUsername.text;
        password = inputfieldPassword.text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return false;

        return true;
    }

    public void OnClickOpenDialogAccountRegister()
    {
        UIManager.OpenDialog(WindowType.DialogAccountRegister);
    }

    public void OnClickSignIn()
    {
        string loginId = inputfieldUsername.text;
        string pass = inputfieldPassword.text;
        if (string.IsNullOrEmpty(loginId) || string.IsNullOrEmpty(pass))
            return;

        GameInfo.gLogin.OnLogin(LoginAuthType.Username, loginId, pass);
    }
}
