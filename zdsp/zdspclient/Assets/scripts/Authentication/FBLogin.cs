using UnityEngine;
using System;
using System.Collections.Generic;
using Zealot.Common;
using Facebook.Unity;

public class FBLogin : Photon.MonoBehaviour 
{
    // Use this for initialization
    void Start() 
	{
        if(Application.platform != RuntimePlatform.WindowsPlayer)
            CallFBInit();
        else
            Debug.LogWarning("Unable to init Facebook in Unity WindowsPlayer.");
    }

    #region FB.Init() example

    private void CallFBInit()
	{
		if (!FB.IsInitialized)
			FB.Init(InitCallback, OnHideUnity);
		else {
			InitCallback();
		}
	}

	private void InitCallback()
	{
#if(!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
		if (FB.IsInitialized) {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
			// ...
		} else {
			Debug.Log("Failed to Initialize the Facebook SDK");
		}
#endif
	}
	
	private void OnHideUnity(bool isGameShown)
	{
		if (!isGameShown) {
			// Pause the game - we will need to hide
			Time.timeScale = 0;
		} else {
			// Resume the game - we're getting focus again
			Time.timeScale = 1;
		}
	}

#endregion

    #region FB.Login() example
    public void CallFBLogin()
    {
        CallFBAction(OnFBLoggedIn, AuthCallback);
    }

    public void CallFBUIDShift()
    {
        CallFBAction(OnFBUIDShift, AuthCallbackUIDShift);
    }

	public void CallFBAction(InitDelegate MethodCallback, FacebookDelegate<ILoginResult> ResultCallback)
	{
        if(FB.IsLoggedIn)
        {            
            // AccessToken class will have session details
            var aToken = AccessToken.CurrentAccessToken;
			// Print current access token's User ID
			Debug.Log(aToken.UserId);
			// Print current access token's granted permissions
			foreach (string perm in aToken.Permissions) {
				Debug.Log(perm);
			}
            MethodCallback();
		}
        else
        {
		    // Login to facebook
		    var perms = new List<string>(){"public_profile", "email", "user_friends"};
            FB.LogInWithReadPermissions(perms, ResultCallback);

            // Temporary disable buttons so that it won't be clicked
            //Button[] buttons = GameInfo.gUILogin.gameobjButtonSwitchAcc.GetComponentsInChildren<Button>();
            //buttons[0].enabled = false;
            //buttons[1].enabled = false;
        }  
	}
	
	private void CallFBLoginForPublish()
	{
		// It is generally good behavior to split asking for read and publish
		// permissions rather than ask for them all at once.
		// In your own game, consider postponing this call until the moment
		// you actually need it.
		//FB.LogInWithReadPermissions("publish_actions", LoginCallback);
	}

	private void AuthCallback(ILoginResult result)
    {
        // Enable other buttons after getting result
        //Button[] buttons = GameInfo.gUILogin.gameobjButtonSwitchAcc.GetComponentsInChildren<Button>();
        //buttons[0].enabled = true;
        //buttons[1].enabled = true;

        if (result.Cancelled)
        {
            Debug.Log("Facebook user action: cancelled");
            return;
        }   
		if(FB.IsLoggedIn)
        {            
            // AccessToken class will have session details
            var aToken = AccessToken.CurrentAccessToken;
			// Print current access token's User ID
			Debug.Log(aToken.UserId);
			// Print current access token's granted permissions
			foreach (string perm in aToken.Permissions) {
				Debug.Log(perm);
			}
            OnFBLoggedIn();
		}
        else
			Debug.Log("Facebook is not logged in.");
	}

    private void AuthCallbackUIDShift(ILoginResult result)
    {
        // Enable other buttons after getting result
        //Button[] buttons = GameInfo.gUILogin.gameobjButtonSwitchAcc.GetComponentsInChildren<Button>();
        //buttons[0].enabled = true;
        //buttons[1].enabled = true;

        if (result.Cancelled)
        {
            Debug.Log("Facebook user action: cancelled");
            return;
        }   
		if(FB.IsLoggedIn)   
            OnFBUIDShift();
        else
			Debug.Log("Facebook is not logged in.");
	}

	/*void LoginCallback(FBResult result)
	{
		if (result.Error != null)
			lastResponse = "Error Response:\n" + result.Error;
		else if (!FB.IsLoggedIn) 
			lastResponse = "Login cancelled by Player";
		else 
			lastResponse = "Login was successful!";
        LogManager.DebugLog(lastResponse);
        
        // Enable other buttons 
        buttonEasyLogin.GetComponent<Button>().enabled = true;        
        buttonUsernameLogin.GetComponent<Button>().enabled = true;
		if(FB.IsLoggedIn)
			OnFBLoggedIn();
	}*/

    public void OnFBUIDShift()
    {
        /*Login loginCmpt = GetComponent<Login>();
        if(loginCmpt.ReconnectWhenDisconnected("UIDShift_"+LoginType.Facebook.ToString()))
            return;

        loginCmpt.GetLoginData();
        LoginData loginData = LoginData.Instance;
        loginCmpt.CustomAuthWithServerUIDShift((LoginType)loginData.LoginType, loginData.LoginId, LoginType.Facebook, 
                                               AccessToken.CurrentAccessToken.TokenString, loginData.DeviceId);*/
    }

	public void OnFBLoggedIn()
	{
        Login loginCmpt = GetComponent<Login>();
        if(loginCmpt.ReconnectWhenDisconnected(LoginType.Facebook.ToString()))
            return;

        loginCmpt.GetLoginData();
        LoginData loginData = LoginData.Instance;
        loginCmpt.DefaultAuthWithServer(LoginType.Facebook.ToString(),
                                        AccessToken.CurrentAccessToken.TokenString, 
                                        loginData.DeviceId); // Auth with server
	}
	
	public void CallFBLogout()
	{
		FB.LogOut();
	}
    #endregion
}
