using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;
using Zealot.Audio;

public class UI_Login : MonoBehaviour
{
    #region Variables
    public string AnnouncementServerURL = "https://piliq.zealotdigital.com.tw/topnews.aspx";
    // Editor Linked Gameobjects
    [SerializeField]
    Text txtAccountName = null;
    [SerializeField]
    Text txtServerName = null;
    [SerializeField]
    Text txtVersion = null;

    const string secretKey = "3h5j3k2fv1";

    public string CachedPass { get; set; }
    Dictionary<int, ServerInfo> serverListDict = new Dictionary<int, ServerInfo>();
    public Dictionary<int, ServerLine> serverLineDict = new Dictionary<int, ServerLine>();

    // Localized System message
    public string RetRePasswordMismatchStr { get; protected set; }
    public string RetUserAlreadyExistStr { get; protected set; }
    public string RetGameServerFullStr { get; protected set; }
    public string RetAuthCookieFailedStr { get; protected set; }
    public string RetUserDoesNotExistStr { get; protected set; }
    public string RetUserOrPassFailedStr { get; protected set; }
    public string RetSignUpSuccessStr { get; protected set; }
    public string RetUserFreezed { get; protected set; }

    //public string RetUIShiftFailedStr { get; protected set; }
    //public string RetUsernameAccReqStr { get; protected set; }
    //public string RetSameNewAndOldPassStr { get; protected set; }
    //public string RetUIShiftAccExistStr { get; protected set; }
    //public string RetInvalidEmailStr { get; protected set; }

    public string SysOpAuthenticate { get; protected set; }
    public string SysConnectingGameServer { get; protected set; }
    public string SysConnectedToGameServer { get; protected set; }
    public string SysLoadingLobby { get; protected set; }
    public string SysInvalidClientVer { get; protected set; }
    public string SysUpdatingServerList { get; protected set; }

    #endregion

    void Awake()
    {
        GameInfo.gUILogin = this;
        NetworkHandler.AnnouncementServerURL = AnnouncementServerURL;
        GameVersion gameVersion = GameLoader.Instance.gameVersion;
#if ZEALOT_DEVELOPMENT && !UNITY_EDITOR
        Zealot.RemoteLog.SetFileID("");
#endif
    }

    // Use this for initialization
    void Start()
    {
        if (GameInfo.firstViewMovie)
        {
            //UIManager.OpenDialog(WindowType.DialogSceneMovie, 
               // (window) => { window.GetComponent<DialogMovie>().StartPlay(GameLoader.MovieOpening, null); });
            GameInfo.firstViewMovie = false;
        }

        bool isLoginDataValid = GameInfo.gLogin.GetLoginData();
        LoginData loginData = LoginData.Instance;

        OpenDialogLicenseAgreement();
        // Establish connection and get device ID
        if (isLoginDataValid) // Has existing login data
        {
            GameInfo.gLogin.ConnectToPhotonServer(LoginType.EstablishConnection.ToString(), "");
            txtAccountName.text = loginData.LoginId;
        }
        else // New device login
        {
            GameInfo.gLogin.ConnectToPhotonServer(LoginType.EstablishConnection.ToString(), loginData.DeviceId);
        }

        CachedPass = "";
        StartCoroutine(InitLocalizeText());

        InitAudioSettings();
        OpenDialogAnnouncement();
    }

    void OnDestroy()
    {
        GameInfo.gUILogin = null;
        if (serverListDict != null)
        {
            serverListDict.Clear();
            serverListDict = null;
        }
    }

    IEnumerator InitLocalizeText()
    {
        yield return null;

        GameVersion gameVersion = GameLoader.Instance.gameVersion;
        string versionNumber = (gameVersion != null) ? gameVersion.ServerVersion : "";
        txtVersion.text = string.Format("{0}: {1}", GUILocalizationRepo.GetLocalizedString("login_Version"), versionNumber.Trim());
        // Localize system message
        RetRePasswordMismatchStr = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_RePasswordMismatch", null);
        RetUserAlreadyExistStr = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_UserAlreadyExist", null);
        RetGameServerFullStr = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_GameServerFull", null);
        RetAuthCookieFailedStr = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_AuthCookieFailed", null);
        RetUserDoesNotExistStr = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_UserDoesNotExist", null);
        RetUserOrPassFailedStr = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_UserOrPassFailed", null);
        RetSignUpSuccessStr = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_SignUpSuccess", null);
        RetUserFreezed = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_UserFreezed");
        //RetUIShiftFailedStr = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_UIShiftFailed", null);
        //RetUsernameAccReqStr = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_UsernameAccReq", null);
        //RetSameNewAndOldPassStr = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_SameNewAndOldPass", null);
        //RetUIShiftAccExistStr = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_UIShiftAccExist", null);
        //RetInvalidEmailStr = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_InvalidEmail", null);

        SysOpAuthenticate = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Login_OpAuthenticate", null);
        SysConnectingGameServer = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Login_ConnectingGameServer", null);
        SysConnectedToGameServer = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Login_ConnectedToGameServer", null);
        SysLoadingLobby = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Login_LoadingLobby", null);
        SysInvalidClientVer = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_InvalidClientVersion", null);
        SysUpdatingServerList = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Login_UpdatingServerList", null);
    }

    void InitAudioSettings()
    {
        float volumeScale = GameSettings.MusicVolume / 100;
        Music.Instance.SetVolume(volumeScale);

        volumeScale = GameSettings.SoundFXEnabled ? GameSettings.SoundFXVolume / 100 : 0;
        SoundFX.Instance.SetVolume(volumeScale);
    }

    public void SetAccountName(string accountName)
    {
        txtAccountName.text = accountName;
    }

    public void SetLoginDataPass(LoginType loginType, string password)
    {
        if (loginType == LoginType.Username && !string.IsNullOrEmpty(password))
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.GenerateIV();
            LoginData.Instance.EncryptedPass = Login.EncryptTxt(password, secretKey, des.IV);
            LoginData.Instance.IV = Convert.ToBase64String(des.IV);
        }
        else LoginData.Instance.EncryptedPass = LoginData.Instance.IV = "";
    }

    public bool TryGetLoginDataPass(out string password)
    {
        password = "";
        if (LoginData.Instance.LoginType == (short)LoginType.Username &&
            !string.IsNullOrEmpty(LoginData.Instance.EncryptedPass) && 
            !string.IsNullOrEmpty(LoginData.Instance.IV))
        {
            password = Login.DecryptTxt(LoginData.Instance.EncryptedPass, secretKey, 
                                        Convert.FromBase64String(LoginData.Instance.IV));
            return true;
        }   
        return false;
    }

    public void SetSelectedGameServer(int serverid)
    {
        if (serverListDict.ContainsKey(serverid))
        {
            ServerInfo serverInfo = serverListDict[serverid];
            GameInfo.gLogin.SelectedServerInfo = serverInfo;
            string world = serverLineDict[serverInfo.serverLine].displayName;          
            txtServerName.text = string.Format("<color={0}>{1} {2}</color>", ClientUtils.GetServerStatusColor(serverInfo.serverLoad), world, serverInfo.serverName);
            if (GameInfo.gLogin.IsConnectingToGameServer)
                GameInfo.gLogin.ConnectToSelectedGameServerSetup();
        }
        else
            GameInfo.gLogin.SelectedServerInfo = null;
    }

    void SetServerBuckets(Dictionary<int, ServerInfo> serverInfoList)
    {
        GameObject gameobjServerSelection = UIManager.GetWindowGameObject(WindowType.DialogServerSelection);
        if (gameobjServerSelection != null)
            gameobjServerSelection.GetComponent<Dialog_ServerSelection>().InitServerBuckets(serverListDict, serverLineDict);
    }

    public ServerInfo GetLastLogin()
    {
        int id = LoginData.Instance.ServerId;
        if (serverListDict.ContainsKey(id))
            return serverListDict[id];
        return null;
    }

    public void ParseServersInfoStr(string serversInfoStr)
    {
        serverListDict.Clear();      
        bool selectedServerExist = false;
        int selectedSvrId = (GameInfo.gLogin.SelectedServerInfo != null) ? GameInfo.gLogin.SelectedServerInfo.id : LoginData.Instance.ServerId;
        if (!string.IsNullOrEmpty(serversInfoStr))
        {
            string[] serversInfoStrArray = serversInfoStr.Split(';');
            var serverLineList = JsonConvertDefaultSetting.DeserializeObject<ServerLineList>(serversInfoStrArray[0]).list;
            serverLineDict.Clear();
            for (int index = 0; index < serverLineList.Count; index++)
                serverLineDict[serverLineList[index].serverline] = serverLineList[index];
            for (int index = 1; index < serversInfoStrArray.Length; index++)
            {
                string[] serverConfigAndLoad = serversInfoStrArray[index].Split('|');
                string serverConfigStr = serverConfigAndLoad[0];
                if (!string.IsNullOrEmpty(serverConfigStr))
                {
                    ServerLoad serverload = (ServerLoad)byte.Parse(serverConfigAndLoad[1]);
                    ServerConfig serverConfig = ServerConfig.Deserialize(serverConfigStr);
                    int serverid = serverConfig.id;
                    ServerInfo serverInfo = new ServerInfo(serverid, serverConfig.ipAddr, serverConfig.servername, serverConfig.serverline, serverload);
                    serverListDict.Add(serverid, serverInfo);
                    if (selectedSvrId == serverid) // Saved Id exist
                        selectedServerExist = true;
                }
            }
        }
        SetSelectedGameServer(selectedServerExist ? selectedSvrId : 0);
        SetServerBuckets(serverListDict);
    }

    void OpenDialogLicenseAgreement()
    {
        if (!LoginData.Instance.HasReadLicense)
            UIManager.OpenDialog(WindowType.DialogLicenseAgreement);
    }

    public void OpenOkDialogLoginFailed(bool isUserNotExist)
    {
        string retCodeStr = isUserNotExist ? RetUserDoesNotExistStr : RetUserOrPassFailedStr;
        UIManager.OpenOkDialog(retCodeStr, null);
    }

    public void OpenDialogServerSelection(bool isGettingServerList=true)
    {
        Login login = GameInfo.gLogin;
        if (login.ReconnectWhenDisconnected("ServerList"))
        {
            UIManager.StartHourglass(10.0f, SysUpdatingServerList);
            return;
        }

        if (isGettingServerList)
            login.CustomAuthWithServer(OperationCode.GetServerList, LoginData.Instance.DeviceId);
        else // Recieved GetServerList result, open dialog
            UIManager.OpenDialog(WindowType.DialogServerSelection);
    }

    private void OpenDialogAnnouncement()
    {
        //UIManager.OpenDialog(WindowType.DialogAnnouncement);
    }

    public void OnClickAnnouncement()
    {
        OpenDialogAnnouncement();
    }

    public void OnClickSwitchAccount()
    {
        UIManager.OpenDialog(WindowType.DialogUsernamePassword);
    }

    public void OnClickServerSelection()
    {
        OpenDialogServerSelection();
    }

    public void OnClickEnterGame()
    {
        LoginType type = (LoginType)LoginData.Instance.LoginType;
        switch (type)
        {
            case LoginType.Device:
                string loginId = LoginData.Instance.LoginId = LoginData.Instance.DeviceId;
                GameInfo.gLogin.OnLogin(type, loginId, loginId);
                break;
            case LoginType.Username:
                string pass = "";
                if (TryGetLoginDataPass(out pass))
                    GameInfo.gLogin.OnLogin(type, LoginData.Instance.LoginId, pass);
                break;
            case LoginType.Facebook:
                GetComponent<FBLogin>().CallFBLogin();
                break;
        }
    }

    /*public void SetUIActive(string parent, string child, bool isActive)
    {
        GameObject parentObj = GameObject.Find(parent);
        Transform[] transList = parentObj.GetComponentsInChildren<Transform>(true);
        foreach(Transform cmpt in transList)
        {
            GameObject obj = cmpt.gameObject;
            if(obj.name.Equals(child))
            {
                obj.SetActive(isActive);
                break;
            }
        }
    }*/
}

