using UnityEngine;
using System;
using Zealot.Common;
//using GooglePlayGames;
//using GooglePlayGames.BasicApi;

public class GoogleLogin : Photon.MonoBehaviour 
{
    private string serverAuthCode = "";
    private Action AuthCallBack;

	// Use this for initialization
	void Awake () 
	{
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
        /*PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        // enables saving game progress.
        //.EnableSavedGames()
        // registers a callback to handle game invitations received while the game is not running.
        //.WithInvitationDelegate(< callback method >)
        // registers a callback for turn based match notifications received while the
        // game is not running.
        //.WithMatchDelegate(< callback method >)
        // require access to a player's Google+ social graph (usually not needed)
        //.RequireGooglePlus()
        .AddOauthScope("profile")
        .Build();

        PlayGamesPlatform.InitializeInstance(config);
        // recommended for debugging:
        PlayGamesPlatform.DebugLogEnabled = true;
        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();*/
#endif
        Init();
        AuthCallBack = OnGoogleLoggedIn;
    }

    private void Init()
    {
        serverAuthCode = "";
    }

    public void OnClickedGoogleLogin()
    {
        Init();
        AuthCallBack = OnGoogleLoggedIn;
        Social.localUser.Authenticate(OnGoogleLoginCB);
    }

    public void OnClickedGoogleUIDShift()
    {
        Init();
        AuthCallBack = OnGoogleUIDShift;
        Social.localUser.Authenticate(OnGoogleLoginCB);
    }

    private void OnGoogleLoginCB(bool success)
    {
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
        /*Debug.Log(string.Format("Callback Result: {0}", success));
        if(success)
        {
            bool isAuth = PlayGamesPlatform.Instance.IsAuthenticated();
            if (isAuth)
            {
                Debug.Log("PlayGamesPlatform Authenticated success.");
                Debug.Log(string.Format("Google Id: {0}", PlayGamesPlatform.Instance.GetUserId()));
                //Debug.Log(string.Format("Access Token: {0}", PlayGamesPlatform.Instance.GetAccessToken()));
                PlayGamesPlatform.Instance.GetServerAuthCode(OnServerAuthCode);
            }
        }*/
#endif
    }

    /*private void OnServerAuthCode(CommonStatusCodes statusCode, string authCode)
    {
        Debug.Log(string.Format("CommonStatusCode: {0}, Auth Code: {1}", statusCode.ToString(), authCode));
        serverAuthCode = authCode;
        AuthCallBack();
    }*/

    private bool IsAuthCodeNullorEmpty()
    {
        if(string.IsNullOrEmpty(serverAuthCode))
        {
            Debug.LogError("AuthCode is Null or Empty.");
            return true;
        }
        return false;
    }

    public void OnGoogleUIDShift()
    {
        /*if(IsAuthCodeNullorEmpty())
            return;

        Login loginCmpt = GetComponent<Login>();
        if(loginCmpt.ReconnectWhenDisconnected("UIDShift_"+LoginType.Google.ToString()))
            return;

        loginCmpt.GetLoginData();
        LoginData loginData = LoginData.Instance;
        loginCmpt.CustomAuthWithServerUIDShift((LoginType)loginData.LoginType, loginData.LoginId, LoginType.Google, 
                                               serverAuthCode, loginData.DeviceId);*/
    }

    public void OnGoogleLoggedIn()
	{
        if(IsAuthCodeNullorEmpty())
            return;

        Login loginCmpt = GetComponent<Login>();
        if(loginCmpt.ReconnectWhenDisconnected(LoginAuthType.Google.ToString()))
            return;

        loginCmpt.GetLoginData();
        LoginData loginData = LoginData.Instance;
        loginCmpt.DefaultAuthWithServer(LoginAuthType.Google.ToString(),
                                        serverAuthCode, loginData.DeviceId); // Auth with server
	}
}
