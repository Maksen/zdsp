using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_DialogAccountRegister : MonoBehaviour
{
    [SerializeField]
    InputField inputfieldUsername = null;
    [SerializeField]
    InputField inputfieldPassword = null;
    [SerializeField]
    InputField inputfieldConfirmPassword = null;
    [SerializeField]
    Image imgVerifyCheck = null;
    [SerializeField]
    Sprite[] imgVerify = null;

    bool showVerify = false;
    float currTimer = 0;

    void OnEnable()
    {
        inputfieldUsername.text = "";
        inputfieldPassword.text = "";
        inputfieldConfirmPassword.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (showVerify)
        {
            if (currTimer >= 0)
                currTimer -= Time.deltaTime;
            else
            {
                showVerify = false;
                imgVerifyCheck.gameObject.SetActive(false);
            }
        }
    }

    public void SetVerifyCheck(bool isValid)
    {
        currTimer = 3;
        showVerify = true;
        imgVerifyCheck.gameObject.SetActive(true);
        imgVerifyCheck.sprite = isValid ? imgVerify[1] : imgVerify[0];
        //if (!isValid)
        //    UIManager.ShowSystemMessage(GameInfo.gUILogin.RetUserAlreadyExistStr);
    }

    public void OnClickVerifyUsername()
    {
        Login login = GameInfo.gLogin;
        if (login.ReconnectWhenDisconnected("VerifyLoginIdRegister"))
            return;

        string loginId = inputfieldUsername.text;
        if (string.IsNullOrEmpty(loginId))
            return;
        string pattern = @"^[a-zA-Z0-9_]{3,12}$"; //alphanumeric, 3 to 12 characters long
        Regex rgx = new Regex(pattern);
        if (!rgx.IsMatch(loginId))
        {
            SetVerifyCheck(false);
            return;
        }

        login.CustomAuthWithServer(OperationCode.VerifyLoginId, LoginType.Username.ToString(), loginId, "VerifyLoginIdRegister");
    }

    public void OnClickSignUp()
    {
        Login login = GameInfo.gLogin;
        if (login.ReconnectWhenDisconnected("Register"))
            return;

        string loginId = inputfieldUsername.text;
        string pass = inputfieldPassword.text;
        string confirmPass = inputfieldConfirmPassword.text;
        if (string.IsNullOrEmpty(loginId) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(confirmPass))
            return;
        else if (!pass.Equals(confirmPass))
        {
            UIManager.OpenOkDialog(GameInfo.gUILogin.RetMsgRePasswordMismatch, null);
            return;
        }

        GameInfo.gUILogin.CachedPass = pass;
        login.CustomAuthWithServer(OperationCode.Register, loginId, pass, LoginData.Instance.DeviceId);
    }
}
