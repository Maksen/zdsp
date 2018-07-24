#pragma warning disable 1587
/// \file
/// <summary>ScriptableObject defining a server setup. An instance is created as <b>PhotonServerSettings</b>.</summary>
#pragma warning restore 1587

using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public class Region
{
    public CloudRegionCode Code;
    /// <summary>Unlike the CloudRegionCode, this may contain cluster information.</summary>
    public string Cluster;
    public string HostAndPort;
    public int Ping;

    public Region(CloudRegionCode code)
    {
        this.Code = code;
        this.Cluster = code.ToString();
    }

    public Region(CloudRegionCode code, string regionCodeString, string address)
    {
        this.Code = code;
        this.Cluster = regionCodeString;
        this.HostAndPort = address;
    }

    public static CloudRegionCode Parse(string codeAsString)
    {
        if (codeAsString == null)
        {
            return CloudRegionCode.none;
        }

        int slash = codeAsString.IndexOf('/');
        if (slash > 0)
        {
            codeAsString = codeAsString.Substring(0, slash);
        }
        codeAsString = codeAsString.ToLower();

        if (Enum.IsDefined(typeof(CloudRegionCode), codeAsString))
        {
            return (CloudRegionCode)Enum.Parse(typeof(CloudRegionCode), codeAsString);
        }

        return CloudRegionCode.none;
    }


    internal static CloudRegionFlag ParseFlag(CloudRegionCode region)
    {
        if (Enum.IsDefined(typeof(CloudRegionFlag), region.ToString()))
        {
            return (CloudRegionFlag)Enum.Parse(typeof(CloudRegionFlag), region.ToString());
        }

        return (CloudRegionFlag)0;
    }

    [Obsolete]
    internal static CloudRegionFlag ParseFlag(string codeAsString)
    {
        codeAsString = codeAsString.ToLower();

        CloudRegionFlag code = 0;
        if (Enum.IsDefined(typeof(CloudRegionFlag), codeAsString))
        {
            code = (CloudRegionFlag)Enum.Parse(typeof(CloudRegionFlag), codeAsString);
        }

        return code;
    }

    public override string ToString()
    {
        return string.Format("'{0}' \t{1}ms \t{2}", this.Cluster, this.Ping, this.HostAndPort);
    }
}


/// <summary>
/// Collection of connection-relevant settings, used internally by PhotonNetwork.ConnectUsingSettings.
/// </summary>
[Serializable]
public class ServerSettings : ScriptableObject
{
    public enum HostingOption { NotSet = 0, PhotonCloud = 1, SelfHosted = 2, OfflineMode = 3, BestRegion = 4 }

    public string AppID = "";
    public string VoiceAppID = "";
    public string ChatAppID = "";

    public HostingOption HostType = HostingOption.NotSet;

    public CloudRegionCode PreferredRegion;
    public CloudRegionFlag EnabledRegions = (CloudRegionFlag)(-1);

    public ConnectionProtocol Protocol = ConnectionProtocol.Udp;
    //public string ServerAddress = "";
    public string ServerAddressPrivate = "";        
    public string ServerAddressStaging = "";        
    public string ServerAddressLive = "";           
    public string ServerAddressPublicTest = "";     

    public int ServerPort = 5055;
    public int VoiceServerPort = 5055;  // Voice only uses UDP

    [HideInInspector]
    public bool JoinLobby;
    [HideInInspector]
    public bool EnableLobbyStatistics;
    public PhotonLogLevel PunLogging = PhotonLogLevel.ErrorsOnly;
    public DebugLevel NetworkLogging = DebugLevel.ERROR;

	public bool RunInBackground = true;
    [HideInInspector]
    public List<string> RpcList = new List<string>();   // set by scripts and or via Inspector

    [HideInInspector]
    public bool DisableAutoOpenWizard;

    // custom server values (not used for PhotonCloud)
    public string ServerAddress
    {
        get
        {
#if !UNITY_EDITOR && USE_ASSETBUNDLE
            return _serverAddress;
#else
            LoginServerIp type = LoginServerType;
            if (type == LoginServerIp.Staging)
                return ServerAddressStaging;
            else if (type == LoginServerIp.Live)
                return ServerAddressLive;
            else if (type == LoginServerIp.PublicTest)
                return ServerAddressPublicTest;
            else
                return ServerAddressPrivate;
#endif
        }
    }

    public void UseCloudBestRegion(string cloudAppid)
    {
        this.HostType = HostingOption.BestRegion;
        this.AppID = cloudAppid;
    }

    public void UseCloud(string cloudAppid)
    {
        this.HostType = HostingOption.PhotonCloud;
        this.AppID = cloudAppid;
    }

    public void UseCloud(string cloudAppid, CloudRegionCode code)
    {
        this.HostType = HostingOption.PhotonCloud;
        this.AppID = cloudAppid;
        this.PreferredRegion = code;
    }

    public void UseMyServer(string serverAddress, int serverPort, string application)
    {
        this.HostType = HostingOption.SelfHosted;
        this.AppID = (application != null) ? application : "master";

        this.ServerAddressPrivate = serverAddress;
        this.ServerPort = serverPort;
    }

    private string _serverAddress;
    [SerializeField, ReadOnly]
    private LoginServerIp _loginServerType = LoginServerIp.Private;
    public LoginServerIp LoginServerType
    {
        get
        {
#if UNITY_EDITOR
            //refer to LoginServerSelector.kEditorPrefLogin
            string kEditorPrefLogin = "LoginServerIp";
            if (EditorPrefs.HasKey(kEditorPrefLogin))
            {
                return (LoginServerIp)EditorPrefs.GetInt(kEditorPrefLogin);
            }
            else
            {
                return LoginServerIp.Private;
            }
#else
            return _loginServerType;
#endif
        }

        private set
        {
            _loginServerType = value;
        }
    }

    public void SetLoginServerTypeForBuild(LoginServerIp type)
    {
        LoginServerType = type;
    }

    /// <summary>Checks if a string is a Guid by attempting to create one.</summary>
    /// <param name="val">The potential guid to check.</param>
    /// <returns>True if new Guid(val) did not fail.</returns>
    public static bool IsAppId(string val)
    {
        try
        {
            new Guid(val);
        }
        catch
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Gets the best region code in preferences.
    /// This composes the PhotonHandler, since its Internal and can not be accessed by the custom inspector
    /// </summary>
    /// <value>The best region code in preferences.</value>
    public static CloudRegionCode BestRegionCodeInPreferences
    {
        get { return PhotonHandler.BestRegionCodeInPreferences; }
    }

    public static void ResetBestRegionCodeInPreferences()
    {
        PhotonHandler.BestRegionCodeInPreferences = CloudRegionCode.none;
    }

    public override string ToString()
    {
        return "ServerSettings: " + HostType + " " + ServerAddress;
    }

    public void SetServerAddress(string add)
    {
        _serverAddress = add;
    }
}
