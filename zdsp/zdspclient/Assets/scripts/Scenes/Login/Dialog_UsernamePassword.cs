using ExitGames.Client.Photon;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class Dialog_UsernamePassword : MonoBehaviour
{
    // Editor Linked Gameobjects
    [SerializeField]
    GameObject gamobjSelectAuthPlatform = null;
    [SerializeField]
    GameObject gamobjUsernameSignin = null;
    [SerializeField]
    GameObject gamobjUsernameSignup = null;

    [Header("Username Sign In")]
    [SerializeField]
    InputField inputfieldSigninUsername = null;
    [SerializeField]
    InputField inputfieldSigninPassword = null;
    [SerializeField]
    Image imgVerifyCheck = null;
    [SerializeField]
    Sprite[] imgVerify = null;

    [Header("Username Sign Up")]
    [SerializeField]
    InputField inputfieldSignupUsername = null;
    [SerializeField]
    InputField inputfieldSignupPassword = null;
    [SerializeField]
    InputField inputfieldSignupConfirmPassword = null;

    bool showVerify = true;
    float currTimer = 0;

    // Use this for initialization
    void Start()
    {
        Init();
    }

    void OnDisable()
    {
        Init();
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

    void Init()
    {
        gamobjSelectAuthPlatform.SetActive(true);
        gamobjUsernameSignin.SetActive(false);
        gamobjUsernameSignup.SetActive(false);

        InitInputfieldSignIn("", "");
        InitInputfieldSignUp("", "", "");
    }

    public void InitInputfieldSignIn(string username, string password)
    {
        inputfieldSigninUsername.text = username;
        inputfieldSigninPassword.text = password;
    }

    public void InitInputfieldSignUp(string username, string password, string confirmPassword)
    {
        inputfieldSignupUsername.text = username;
        inputfieldSignupPassword.text = password;
        inputfieldSignupConfirmPassword.text = confirmPassword;
    }

    public bool TryGetInputfieldSignIn(out string signinUsername, out string signinPassword)
    {
        signinUsername = inputfieldSigninUsername.text;
        signinPassword = inputfieldSigninPassword.text;
        if (string.IsNullOrEmpty(signinUsername) || string.IsNullOrEmpty(signinPassword))
            return false;

        return true;
    }

    public void SetVerifyCheck(bool isValid)
    {
        currTimer = 3;
        showVerify = true;
        imgVerifyCheck.gameObject.SetActive(true);
        imgVerifyCheck.sprite = isValid ? imgVerify[1] : imgVerify[0];
        if (!isValid)
            UIManager.ShowSystemMessage(GameInfo.gUILogin.RetUserAlreadyExistStr);
    }

    public void OnClickOpenUsernameSignIn()
    {
        gamobjSelectAuthPlatform.SetActive(false);
        gamobjUsernameSignin.SetActive(true);
        gamobjUsernameSignup.SetActive(false);
    }

    public void OnClickOpenUsernameSignUp()
    {
        gamobjSelectAuthPlatform.SetActive(false);
        gamobjUsernameSignin.SetActive(false);
        gamobjUsernameSignup.SetActive(true);
    }

    public void OnClickFacebookSignIn()
    {
    }

    public void OnClickGoogleSignIn()
    {
    }

    public void OnClickUsernameSignIn()
    {
        string loginId = inputfieldSigninUsername.text;
        string pass = inputfieldSigninPassword.text;
        if (string.IsNullOrEmpty(loginId) || string.IsNullOrEmpty(pass))
            return;

        GameInfo.gLogin.OnLogin(LoginType.Username, loginId, pass);
    }

    public void OnClickUsernameVerify()
    {
        Login login = GameInfo.gLogin;
        if (login.ReconnectWhenDisconnected("VerifyLoginIdRegister"))
            return;

        string loginId = inputfieldSignupUsername.text;
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

    public void OnClickUsernameSignUp()
    {
        Login login = GameInfo.gLogin;
        if (login.ReconnectWhenDisconnected("Register"))
            return;

        string loginId = inputfieldSignupUsername.text;
        string pass = inputfieldSignupPassword.text;
        string confirmPass = inputfieldSignupConfirmPassword.text;
        if (string.IsNullOrEmpty(loginId) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(confirmPass))
            return;
        if (pass != confirmPass)
        {
            UIManager.OpenOkDialog(GameInfo.gUILogin.RetRePasswordMismatchStr, null);
            return;
        }

        GameInfo.gUILogin.CachedPass = pass;
        login.CustomAuthWithServer(OperationCode.Register, loginId, pass, LoginData.Instance.DeviceId);
    }

    public void OnClickBackToSelectPlatform()
    {
        Init();
    }
}
