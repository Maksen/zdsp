using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Audio;
using Zealot.Common;
using Zealot.Repository;

public class UI_Login : MonoBehaviour
{
    [SerializeField]
    string announcementServerURL = "https://piliq.zealotdigital.com.tw/topnews.aspx";
    [SerializeField]
    Text txtServerName = null;
    [SerializeField]
    Text txtGameVersion = null;

    const string secretKey = "3h5j3k2fv1";

    public string CachedPass { get; set; }

    // Localized System message
    public string RetMsgSignUpSuccess { get; protected set; }
    public string RetMsgUserAlreadyExist { get; protected set; }
    public string RetMsgRePasswordMismatch { get; protected set; }
    public string RetMsgUserDoesNotExist { get; protected set; }
    public string RetMsgUserOrPassFailed { get; protected set; }

    public string SysMsgOpAuthenticate { get; protected set; }
    public string SysMsgConnectingGameServer { get; protected set; }
    public string SysMsgUpdatingServerList { get; protected set; }
    public string SysMsgLoadingLobby { get; protected set; }

    void Awake()
    {
        GameInfo.gUILogin = this;
        NetworkHandler.AnnouncementServerURL = announcementServerURL;
        GameVersion gameVersion = GameLoader.Instance.gameVersion;
#if ZEALOT_DEVELOPMENT && !UNITY_EDITOR
        Zealot.RemoteLog.SetFileID("");
#endif
    }

    // Use this for initialization
    void Start()
    {
        bool isLoginDataValid = GameInfo.gLogin.GetLoginData();
        LoginData loginData = LoginData.Instance;

        if (!loginData.HasReadLicense)
            UIManager.OpenDialog(WindowType.DialogLicenseAgreement);

        // Establish connection and send device ID if is a new device
        // Send empty string if has existing login data
        string newDeviceId = isLoginDataValid ? "" : loginData.DeviceId;
        GameInfo.gLogin.ConnectToPhotonServer(LoginAuthType.EstablishConnection.ToString(), newDeviceId);

        CachedPass = "";
        StartCoroutine(InitLocalizeText());

        InitAudioSettings();
        OpenDialogAnnouncement();
    }

    void OnDestroy()
    {
        GameInfo.gUILogin = null;
    }

    IEnumerator InitLocalizeText()
    {
        yield return null;

        GameVersion gameVersion = GameLoader.Instance.gameVersion;
        string versionNumber = (gameVersion != null) ? gameVersion.ServerVersion : "";
        txtGameVersion.text = string.Format("{0}：{1}", GUILocalizationRepo.GetLocalizedString("login_Version"), versionNumber.Trim());

        // Localize system message
        RetMsgSignUpSuccess = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_SignUpSuccess", null);
        RetMsgUserAlreadyExist = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_UserAlreadyExist", null);
        RetMsgRePasswordMismatch = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_RePasswordMismatch", null);
        RetMsgUserDoesNotExist = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_UserDoesNotExist", null);
        RetMsgUserOrPassFailed = GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Login_UserOrPassFailed", null);

        SysMsgOpAuthenticate = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Login_OpAuthenticate", null);
        SysMsgConnectingGameServer = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Login_ConnectingGameServer", null);
        SysMsgUpdatingServerList = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Login_UpdatingServerList", null);
        SysMsgLoadingLobby = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Login_LoadingLobby", null);
    }

    void InitAudioSettings()
    {
        float volumeScale = GameSettings.MusicVolume / 100;
        Music.Instance.SetVolume(volumeScale);

        volumeScale = GameSettings.SoundFXEnabled ? GameSettings.SoundFXVolume / 100 : 0;
        SoundFX.Instance.SetVolume(volumeScale);
    }

    public void SetLoginDataPass(LoginAuthType loginType, string password)
    {
        if (loginType == LoginAuthType.Username && !string.IsNullOrEmpty(password))
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
        if (LoginData.Instance.LoginType == (short)LoginAuthType.Username &&
            !string.IsNullOrEmpty(LoginData.Instance.EncryptedPass) && 
            !string.IsNullOrEmpty(LoginData.Instance.IV))
        {
            password = Login.DecryptTxt(LoginData.Instance.EncryptedPass, secretKey, Convert.FromBase64String(LoginData.Instance.IV));
            return true;
        }   
        return false;
    }

    public void SetSelectedGameServer(int serverId)
    {
        Login login = GameInfo.gLogin;
        Dictionary<int, ServerInfo> serverInfoRefDict = login.ServerInfoRefDict;
        if (serverInfoRefDict.ContainsKey(serverId))
        {
            ServerInfo serverInfo = serverInfoRefDict[serverId];
            login.SelectedServerInfo = serverInfo;
            string world = login.ServerLineRefDict[serverInfo.ServerLine].displayName;
            txtServerName.text = ClientUtils.GetServerNameWithColor(serverInfo.ServerLoad, world, serverInfo.ServerName);
            if (login.IsConnectingToGameServer)
                login.ConnectToSelectedGameServerSetup();
        }
        else
            login.SelectedServerInfo = null;
    }

    public void ParseServersInfoStr(string serversInfoStr)
    {
        Login login = GameInfo.gLogin;
        int selectedServerId = (login.SelectedServerInfo != null) ? login.SelectedServerInfo.Id : LoginData.Instance.ServerId;

        bool isSelectedServerValid = false;
        Dictionary<int, ServerInfo> serverInfoRefDict = login.ServerInfoRefDict;
        serverInfoRefDict.Clear();
        if (!string.IsNullOrEmpty(serversInfoStr))
        {      
            string[] serversInfoStrArray = serversInfoStr.Split(';');
            List<ServerLine> serverLineList = JsonConvertDefaultSetting.DeserializeObject<ServerLineList>(serversInfoStrArray[0]).list;
            Dictionary<int, ServerLine> serverLineRefDict = login.ServerLineRefDict;
            serverLineRefDict.Clear();
            int count = serverLineList.Count;
            for (int index = 0; index < count; ++index)
            {
                ServerLine serverLine = serverLineList[index];
                serverLineRefDict[serverLine.serverLineId] = serverLine;
            }

            count = serversInfoStrArray.Length;
            for (int index = 1; index < count; ++index)
            {
                string[] serverConfigAndLoad = serversInfoStrArray[index].Split('|');
                if (serverConfigAndLoad.Length != 2)
                    continue;
                string serverConfigStr = serverConfigAndLoad[0];
                if (!string.IsNullOrEmpty(serverConfigStr))
                {
                    ServerLoad serverload = (ServerLoad)byte.Parse(serverConfigAndLoad[1]);
                    ServerConfig serverConfig = ServerConfig.Deserialize(serverConfigStr);
                    int serverId = serverConfig.id;
                    ServerInfo serverInfo = new ServerInfo(serverId, serverConfig.ipAddr, serverConfig.servername, serverConfig.serverline, serverload);
                    serverInfoRefDict[serverId] = serverInfo;
                    if (selectedServerId == serverId) // Selected server Id is valid
                        isSelectedServerValid = true;
                }
            }
        }
        SetSelectedGameServer(isSelectedServerValid ? selectedServerId : 0);

        GameObject gameobjServerSelection = UIManager.GetWindowGameObject(WindowType.DialogServerSelection);
        if (gameobjServerSelection != null)
            gameobjServerSelection.GetComponent<UI_DialogServerSelection>().InitServerBuckets(serverInfoRefDict);
    }

    public void OpenOkDialogLoginFailed(bool isUserNotExist)
    {
        string retCodeStr = isUserNotExist ? RetMsgUserDoesNotExist : RetMsgUserOrPassFailed;
        UIManager.OpenOkDialog(retCodeStr, null);
    }

    private void OpenDialogAnnouncement()
    {
        //UIManager.OpenDialog(WindowType.DialogAnnouncement);
    }

    public void OpenDialogServerSelection(bool isUpdatingServerList = true)
    {
        Login login = GameInfo.gLogin;
        if (login.ReconnectWhenDisconnected("ServerList"))
        {
            UIManager.StartHourglass(10.0f, SysMsgUpdatingServerList);
            return;
        }

        if (isUpdatingServerList)
            login.CustomAuthWithServer(OperationCode.GetServerList, LoginData.Instance.DeviceId);
        else // Recieved GetServerList result, open dialog
            UIManager.OpenDialog(WindowType.DialogServerSelection);
    }

    public void OnClickServerSelection()
    {
        OpenDialogServerSelection();
    }

    public void OnClickAnywhereToStart()
    {
        LoginAuthType type = (LoginAuthType)LoginData.Instance.LoginType;
        switch (type)
        {
            case LoginAuthType.Device:
                string loginId = LoginData.Instance.LoginId = LoginData.Instance.DeviceId;
                GameInfo.gLogin.OnLogin(type, loginId, loginId);
                break;
            case LoginAuthType.Username:
                string pass = "";
                if (TryGetLoginDataPass(out pass))
                    GameInfo.gLogin.OnLogin(type, LoginData.Instance.LoginId, pass);
                break;
            case LoginAuthType.Facebook:
                GetComponent<FBLogin>().CallFBLogin();
                break;
        }
    }

    public void OnClickSettings()
    {
        UIManager.OpenDialog(WindowType.DialogSettings);
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
