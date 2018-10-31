using UnityEngine;

public class UI_DialogAccountLoginType : MonoBehaviour
{
    public void OnClickFacebookSignIn()
    {
    }

    public void OnClickGoogleSignIn()
    {
    }

    public void OnClickOpenDialogAccountLogin()
    {
        UIManager.OpenDialog(WindowType.DialogAccountLogin);
    }
}
