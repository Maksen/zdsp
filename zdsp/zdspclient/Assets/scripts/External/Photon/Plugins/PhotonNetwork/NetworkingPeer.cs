// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NetworkingPeer.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking (PUN)
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Zealot.Common;
using Hashtable = ExitGames.Client.Photon.Hashtable;

#region Enums

/// <summary>
/// Detailed connection / networking peer state.
/// PUN implements a loadbalancing and authentication workflow "behind the scenes", so
/// some states will automatically advance to some follow up state. Those states are
/// commented with "(will-change)".
/// </summary>
/// \ingroup publicApi
public enum ClientState
{
    /// <summary>Not running. Only set before initialization and first use.</summary>
    Uninitialized,

    /// <summary>Created and available to connect.</summary>
    PeerCreated,

    /// <summary>Not used at the moment.</summary>
    Queued,

    /// <summary>The application is authenticated. PUN usually joins the lobby now.</summary>
    /// <remarks>(will-change) Unless AutoJoinLobby is false.</remarks>
    Authenticated,

    /// <summary>Client is in the lobby of the Master Server and gets room listings.</summary>
    /// <remarks>Use Join, Create or JoinRandom to get into a room to play.</remarks>
    JoinedLobby,

    /// <summary>Disconnecting.</summary>
    /// <remarks>(will-change)</remarks>
    DisconnectingFromMasterserver,

    /// <summary>Connecting to game server (to join/create a room and play).</summary>
    /// <remarks>(will-change)</remarks>
    ConnectingToGameserver,

    /// <summary>Similar to Connected state but on game server. Still in process to join/create room.</summary>
    /// <remarks>(will-change)</remarks>
    ConnectedToGameserver,

    /// <summary>In process to join/create room (on game server).</summary>
    /// <remarks>(will-change)</remarks>
    Joining,

    /// <summary>Final state of a room join/create sequence. This client can now exchange events / call RPCs with other clients.</summary>
    Joined,

    /// <summary>Leaving a room.</summary>
    /// <remarks>(will-change)</remarks>
    Leaving,

    /// <summary>Workflow is leaving the game server and will re-connect to the master server.</summary>
    /// <remarks>(will-change)</remarks>
    DisconnectingFromGameserver,

    /// <summary>Workflow is connected to master server and will establish encryption and authenticate your app.</summary>
    /// <remarks>(will-change)</remarks>
    ConnectingToMasterserver,

    /// <summary>Same Queued but coming from game server.</summary>
    /// <remarks>(will-change)</remarks>
    QueuedComingFromGameserver,

    /// <summary>PUN is disconnecting. This leads to Disconnected.</summary>
    /// <remarks>(will-change)</remarks>
    Disconnecting,

    /// <summary>No connection is setup, ready to connect. Similar to PeerCreated.</summary>
    Disconnected,

    /// <summary>Final state for connecting to master without joining the lobby (AutoJoinLobby is false).</summary>
    ConnectedToMaster,

    /// <summary>Client connects to the NameServer. This process includes low level connecting and setting up encryption. When done, state becomes ConnectedToNameServer.</summary>
    ConnectingToNameServer,

    /// <summary>Client is connected to the NameServer and established enctryption already. You should call OpGetRegions or ConnectToRegionMaster.</summary>
    ConnectedToNameServer,

    /// <summary>When disconnecting from a Photon NameServer.</summary>
    /// <remarks>(will-change)</remarks>
    DisconnectingFromNameServer,

    /// <summary>When connecting to a Photon Server, this state is intermediate before you can call any operations.</summary>
    /// <remarks>(will-change)</remarks>
    Authenticating,

    DisconnectingForTransferServer
}

/// <summary>
/// Summarizes the cause for a disconnect. Used in: OnConnectionFail and OnFailedToConnectToPhoton.
/// </summary>
/// <remarks>Extracted from the status codes from ExitGames.Client.Photon.StatusCode.</remarks>
/// <seealso cref="PhotonNetworkingMessage"/>
/// \ingroup publicApi
public enum DisconnectCause
{
    /// <summary>Server actively disconnected this client.
    /// Possible cause: The server's user limit was hit and client was forced to disconnect (on connect).</summary>
    DisconnectByServerUserLimit = StatusCode.DisconnectByServerUserLimit,

    /// <summary>Connection could not be established.
    /// Possible cause: Local server not running.</summary>
    ExceptionOnConnect = StatusCode.ExceptionOnConnect,

    /// <summary>Timeout disconnect by server (which decided an ACK was missing for too long).</summary>
    DisconnectByServerTimeout = StatusCode.DisconnectByServer,

    /// <summary>Server actively disconnected this client.
    /// Possible cause: Server's send buffer full (too much data for client).</summary>
    DisconnectByServerLogic = StatusCode.DisconnectByServerLogic,

    /// <summary>Some exception caused the connection to close.</summary>
    Exception = StatusCode.Exception,

    /// <summary>(32767) The Photon Cloud rejected the sent AppId. Check your Dashboard and make sure the AppId you use is complete and correct.</summary>
    InvalidAuthentication = ErrorCode.InvalidAuthentication,

    /// <summary>(32757) Authorization on the Photon Cloud failed because the concurrent users (CCU) limit of the app's subscription is reached.</summary>
    MaxCcuReached = ErrorCode.MaxCcuReached,

    /// <summary>(32756) Authorization on the Photon Cloud failed because the app's subscription does not allow to use a particular region's server.</summary>
    InvalidRegion = ErrorCode.InvalidRegion,

    /// <summary>The security settings for client or server don't allow a connection (see remarks).</summary>
    /// <remarks>
    /// A common cause for this is that browser clients read a "crossdomain" file from the server.
    /// If that file is unavailable or not configured to let the client connect, this exception is thrown.
    /// Photon usually provides this crossdomain file for Unity.
    /// If it fails, read:
    /// http://doc.exitgames.com/photon-server/PolicyApp
    /// </remarks>
    SecurityExceptionOnConnect = StatusCode.SecurityExceptionOnConnect,

    /// <summary>Timeout disconnect by client (which decided an ACK was missing for too long).</summary>
    DisconnectByClientTimeout = StatusCode.TimeoutDisconnect,

    /// <summary>Exception in the receive-loop.
    /// Possible cause: Socket failure.</summary>
    InternalReceiveException = StatusCode.ExceptionOnReceive,

    /// <summary>(32753) The Authentication ticket expired. Handle this by connecting again (which includes an authenticate to get a fresh ticket).</summary>
    AuthenticationTicketExpired = 32753,
}

/// <summary>Available server (types) for internally used field: server.</summary>
/// <remarks>Photon uses 3 different roles of servers: Name Server, Master Server and Game Server.</remarks>
public enum ServerConnection
{
    /// <summary>This server is where matchmaking gets done and where clients can get lists of rooms in lobbies.</summary>
    MasterServer,
    /// <summary>This server handles a number of rooms to execute and relay the messages between players (in a room).</summary>
    GameServer,
    /// <summary>This server is used initially to get the address (IP) of a Master Server for a specific region. Not used for Photon OnPremise (self hosted).</summary>
    NameServer
}

#endregion

/// <summary>
/// Implements Photon LoadBalancing used in PUN.
/// This class is used internally by PhotonNetwork and not intended as public API.
/// </summary>
internal class NetworkingPeer : LoadBalancingPeer, IPhotonPeerListener
{
    /// <summary>Combination of GameVersion+"_"+PunVersion. Separates players per app by version.</summary>
    protected internal string AppVersion
    {
        get { return PhotonNetwork.gameVersion; }
    }

    /// <summary>Contains the AppId for the Photon Cloud (ignored by Photon Servers).</summary>
    protected internal string AppId;

    /// <summary>
    /// A user's authentication values used during connect for Custom Authentication with Photon (and a custom service/community).
    /// Set these before calling Connect if you want custom authentication.
    /// </summary>
    public AuthenticationValues AuthValues { get; set; }

    /// <summary>Internally used cache for the server's token. Identifies a user/session and can be used to rejoin.</summary>
    private string tokenCache;

    /// <summary>Enables the new Authentication workflow</summary>
    public AuthModeOption AuthMode = AuthModeOption.Auth;

    /// <summary>Defines how the communication gets encrypted.</summary>
    public EncryptionMode EncryptionMode = EncryptionMode.PayloadEncryption;


    ///<summary>Simplifies getting the token for connect/init requests, if this feature is enabled.</summary>
    private string TokenForInit
    {
        get
        {
            if (this.AuthMode == AuthModeOption.Auth)
            {
                return null;
            }
            return (this.AuthValues != null) ? this.AuthValues.Token : null;
        }
    }

    /// <summary>True if this client uses a NameServer to get the Master Server address.</summary>
    public bool IsUsingNameServer { get; protected internal set; }

    /// <summary>Name Server Host Name for Photon Cloud. Without port and without any prefix.</summary>
    #if !UNITY_EDITOR && UNITY_SWITCH
    public const string NameServerHost = "nameserver-eu.cloudapp.net";//set to "ns.exitgames.com" after Nintendo has fixed the traffic manager bug in their dns-resolver for which this is a workaround
    #else
    public const string NameServerHost = "ns.exitgames.com";
    #endif

    /// <summary>Name Server for HTTP connections to the Photon Cloud. Includes prefix and port.</summary>
    public const string NameServerHttp = "http://ns.exitgamescloud.com:80/photon/n";

    /// <summary>Name Server port per protocol (the UDP port is different than TCP, etc).</summary>
    private static readonly Dictionary<ConnectionProtocol, int> ProtocolToNameServerPort = new Dictionary<ConnectionProtocol, int>() { { ConnectionProtocol.Udp, 5058 }, { ConnectionProtocol.Tcp, 4533 }, { ConnectionProtocol.WebSocket, 9093 }, { ConnectionProtocol.WebSocketSecure, 19093 } }; //, { ConnectionProtocol.RHttp, 6063 } };

    /// <summary>Name Server Address for Photon Cloud (based on current protocol). You can use the default values and usually won't have to set this value.</summary>
    public string NameServerAddress { get { return this.GetNameServerAddress(); } }

    /// <summary>Your Master Server address. In PhotonCloud, call ConnectToRegionMaster() to find your Master Server.</summary>
    /// <remarks>
    /// In the Photon Cloud, explicit definition of a Master Server Address is not best practice.
    /// The Photon Cloud has a "Name Server" which redirects clients to a specific Master Server (per Region and AppId).
    /// </remarks>
    public string MasterServerAddress { get; protected internal set; }

    /// <summary>The game server's address for a particular room. In use temporarily, as assigned by master.</summary>
    public string GameServerAddress { get; protected internal set; }

    /// <summary>The server this client is currently connected or connecting to.</summary>
    /// <remarks>
    /// Each server (NameServer, MasterServer, GameServer) allow some operations and reject others.
    /// </remarks>
    protected internal ServerConnection Server { get; private set; }

    public ClientState State { get; internal set; }

    public bool IsInitialConnect = false;

    /// <summary>Internally used to trigger OpAuthenticate when encryption was established after a connect.</summary>
    private bool didAuthenticate;

    /// <summary>A list of region names for the Photon Cloud. Set by the result of OpGetRegions().</summary>
    /// <remarks>Put a "case OperationCode.GetRegions:" into your OnOperationResponse method to notice when the result is available.</remarks>
    public List<Region> AvailableRegions { get; protected internal set; }

    /// <summary>The cloud region this client connects to. Set by ConnectToRegionMaster().</summary>
    public CloudRegionCode CloudRegion { get; protected internal set; }

    /// <summary>Internally used to flag if the message queue was disabled by a "scene sync" situation (to re-enable it).</summary>
    protected internal bool loadingLevelAndPausedNetwork = false;

    // Connection setup caching for reconnection
    private string cachedServerAddress;
    private string cachedApplicationName;
    private ServerConnection cachedServerType;

    public NetworkingPeer(string playername, ConnectionProtocol connectionProtocol) : base(connectionProtocol)
    {
        CloudRegion = CloudRegionCode.none;
        this.Listener = this;
        this.LimitOfUnreliableCommands = 40;
        this.State = ClientState.PeerCreated;
    }

    /// <summary>
    /// Gets the NameServer Address (with prefix and port), based on the set protocol (this.UsedProtocol).
    /// </summary>
    /// <returns>NameServer Address (with prefix and port).</returns>
    private string GetNameServerAddress()
    {
        #if RHTTP
        if (currentProtocol == ConnectionProtocol.RHttp)
        {
            return NameServerHttp;
        }
        #endif

        ConnectionProtocol currentProtocol = this.TransportProtocol;
        int protocolPort = 0;
        ProtocolToNameServerPort.TryGetValue(currentProtocol, out protocolPort);

        string protocolPrefix = string.Empty;
        if (currentProtocol == ConnectionProtocol.WebSocket)
        {
            protocolPrefix = "ws://";
        }
        else if (currentProtocol == ConnectionProtocol.WebSocketSecure)
        {
            protocolPrefix = "wss://";
        }

        if (PhotonNetwork.UseAlternativeUdpPorts && this.TransportProtocol == ConnectionProtocol.Udp)
        {
            protocolPort = 27000;
        }
        string result = string.Format("{0}{1}:{2}", protocolPrefix, NameServerHost, protocolPort);

        //Debug.Log("NameServer: " + result);
        return result;
    }

#region Operations and Connection Methods

    public override bool Connect(string serverAddress, string applicationName)
    {
        Debug.LogError("Avoid using this directly. Thanks.");
        return false;
    }

    /// <summary>Can be used to reconnect to the master server after a disconnect.</summary>
    /// <remarks>Common use case: Press the Lock Button on a iOS device and you get disconnected immediately.</remarks>
    public bool ReconnectToMaster()
    {
        if (this.AuthValues == null)
        {
            Debug.LogWarning("ReconnectToMaster() with AuthValues == null is not correct!");
            this.AuthValues = new AuthenticationValues();
        }
        this.AuthValues.Token = this.tokenCache;

        return this.Connect(this.MasterServerAddress, ServerConnection.MasterServer);
    }

    public bool Connect(string serverAddress, ServerConnection type)
    {
        if (PhotonHandler.AppQuits)
        {
            Debug.LogWarning("Ignoring Connect() because app gets closed. If this is an error, check PhotonHandler.AppQuits.");
            return false;
        }

        if (this.State == ClientState.Disconnecting)
        {
            Debug.LogError("Connect() failed. Can't connect while disconnecting (still). Current state: " + PhotonNetwork.connectionStateDetailed);
            return false;
        }

        this.cachedServerType = type;
        this.cachedServerAddress = serverAddress;
        this.cachedApplicationName = string.Empty;

        this.SetupProtocol(type);

        // connect might fail, if the DNS name can't be resolved or if no network connection is available
        bool connecting = base.Connect(serverAddress, this.AppId, this.TokenForInit);
        if (connecting)
        {
            switch (type)
            {
                case ServerConnection.NameServer:
                    State = ClientState.ConnectingToNameServer;
                    break;
                case ServerConnection.MasterServer:
                    State = ClientState.ConnectingToMasterserver;
                    break;
                case ServerConnection.GameServer:
                    State = ClientState.ConnectingToGameserver;
                    break;
            }
        }
        return connecting;
    }

    bool _isReconnecting;

    /// <summary>
    /// Reconnect this instance. Uses the exact same settings as the developer used to connect using cached properties ( serverAddress, ApplicationName and Protocol Type)
    /// This is used for switching from UDP to TCP on udp connection timeout.
    /// </summary>
    bool Reconnect()
    {
        _isReconnecting = true;

        PhotonNetwork.SwitchToProtocol(PhotonNetwork.PhotonServerSettings.Protocol);
        this.SetupProtocol(this.cachedServerType);

        bool connecting = base.Connect(cachedServerAddress, cachedApplicationName, this.TokenForInit);

        if (connecting)
        {
            switch (this.cachedServerType)
            {
            case ServerConnection.NameServer:
                State = ClientState.ConnectingToNameServer;
                break;
            case ServerConnection.MasterServer:
                State = ClientState.ConnectingToMasterserver;
                break;
            case ServerConnection.GameServer:
                State = ClientState.ConnectingToGameserver;
                break;
            }
        }

        return connecting;
    }

    /// <summary>
    /// Connects to the NameServer for Photon Cloud, where a region-list can be fetched.
    /// </summary>
    /// <see cref="OpGetRegions"/>
    /// <returns>If the workflow was started or failed right away.</returns>
    public bool ConnectToNameServer()
    {
        if (PhotonHandler.AppQuits)
        {
            Debug.LogWarning("Ignoring Connect() because app gets closed. If this is an error, check PhotonHandler.AppQuits.");
            return false;
        }

        this.IsUsingNameServer = true;
        this.CloudRegion = CloudRegionCode.none;

        if (this.State == ClientState.ConnectedToNameServer)
        {
            return true;
        }

        this.SetupProtocol(ServerConnection.NameServer);

        this.cachedServerType = ServerConnection.NameServer;
        cachedServerAddress = this.NameServerAddress;
        cachedApplicationName = "ns";

        if (!base.Connect(this.NameServerAddress, "ns", this.TokenForInit))
        {
            return false;
        }

        this.State = ClientState.ConnectingToNameServer;
        return true;
    }

    /// <summary>
    /// Connects you to a specific region's Master Server, using the Name Server to find the IP.
    /// </summary>
    /// <returns>If the operation could be sent. If false, no operation was sent.</returns>
    public bool ConnectToRegionMaster(CloudRegionCode region)
    {
        if (PhotonHandler.AppQuits)
        {
            Debug.LogWarning("Ignoring Connect() because app gets closed. If this is an error, check PhotonHandler.AppQuits.");
            return false;
        }

        this.IsUsingNameServer = true;
        this.CloudRegion = region;

        if (this.State == ClientState.ConnectedToNameServer)
        {
            return this.CallAuthenticate();
        }

        this.cachedServerType = ServerConnection.NameServer;
        cachedServerAddress = this.NameServerAddress;
        cachedApplicationName = "ns";

        this.SetupProtocol(ServerConnection.NameServer);
        if (!base.Connect(this.NameServerAddress, "ns", this.TokenForInit))
        {
            return false;
        }

        this.State = ClientState.ConnectingToNameServer;
        return true;
    }

    // this sets up the protocol to us, depending on auth-mode and or export.
    protected internal void SetupProtocol(ServerConnection serverType)
    {
        ConnectionProtocol protocolOverride = this.TransportProtocol;

        #if UNITY_XBOXONE
        this.AuthMode = AuthModeOption.Auth;
        if (this.AuthValues == null)
        {
            UnityEngine.Debug.LogError("UNITY_XBOXONE builds must set AuthValues. Set this before calling any Connect method. Refer to the online docs for guidance.");
            throw new Exception("UNITY_XBOXONE builds must set AuthValues.");
        }
        if (this.AuthValues.AuthPostData == null)
        {
            UnityEngine.Debug.LogError("UNITY_XBOXONE builds must use Photon's XBox Authentication and set the XSTS token by calling: PhotonNetwork.AuthValues.SetAuthPostData(xstsToken). Refer to the online docs for guidance.");
			throw new Exception("UNITY_XBOXONE builds must use Photon's XBox Authentication.");
        }
        if (this.AuthValues.AuthType != CustomAuthenticationType.Xbox)
        {
            UnityEngine.Debug.LogWarning("UNITY_XBOXONE builds must use AuthValues.AuthType \"CustomAuthenticationType.Xbox\". PUN sets this value now. Refer to the online docs to avoid this warning.");
            this.AuthValues.AuthType = CustomAuthenticationType.Xbox;
        }
        if (this.TransportProtocol != ConnectionProtocol.WebSocketSecure)
        {
            UnityEngine.Debug.LogWarning("UNITY_XBOXONE builds must use WSS (Secure WebSockets) as Transport Protocol. Changing to WSS from your selection: " + this.TransportProtocol);
            this.TransportProtocol = ConnectionProtocol.WebSocketSecure;
        }
        #endif

        if (this.AuthMode == AuthModeOption.AuthOnceWss)
        {
            if (serverType != ServerConnection.NameServer)
            {
                if (PhotonNetwork.logLevel >= PhotonLogLevel.ErrorsOnly)
                {
                    Debug.LogWarning("Using PhotonServerSettings.Protocol when leaving the NameServer (AuthMode is AuthOnceWss): " + PhotonNetwork.PhotonServerSettings.Protocol);
                }
                protocolOverride = PhotonNetwork.PhotonServerSettings.Protocol;
            }
            else
            {
                if (PhotonNetwork.logLevel >= PhotonLogLevel.ErrorsOnly)
                {
                    Debug.LogWarning("Using WebSocket to connect NameServer (AuthMode is AuthOnceWss).");
                }
                protocolOverride = ConnectionProtocol.WebSocketSecure;
            }
        }

        Type socketTcp = null;
        #if UNITY_XBOXONE
        socketTcp = Type.GetType("ExitGames.Client.Photon.SocketWebTcpNativeDynamic, Assembly-CSharp", false);
        if (socketTcp == null)
        {
            socketTcp = Type.GetType("ExitGames.Client.Photon.SocketWebTcpNativeDynamic, Assembly-CSharp-firstpass", false);
        }
        #else
        // to support WebGL export in Unity, we find and assign the SocketWebTcp class (if it's in the project).
        // alternatively class SocketWebTcp might be in the Photon3Unity3D.dll
        socketTcp = Type.GetType("ExitGames.Client.Photon.SocketWebTcp, Assembly-CSharp", false);
        if (socketTcp == null)
        {
            socketTcp = Type.GetType("ExitGames.Client.Photon.SocketWebTcp, Assembly-CSharp-firstpass", false);
        }
        #endif
        if (socketTcp != null)
        {
            this.SocketImplementationConfig[ConnectionProtocol.WebSocket] = socketTcp;
            this.SocketImplementationConfig[ConnectionProtocol.WebSocketSecure] = socketTcp;
        }


        #if UNITY_WEBGL
        if (this.TransportProtocol != ConnectionProtocol.WebSocket && this.TransportProtocol != ConnectionProtocol.WebSocketSecure)
        {
            Debug.Log("WebGL only supports WebSocket protocol. Overriding PhotonServerSettings.");
            protocolOverride = ConnectionProtocol.WebSocketSecure;
        }
        PhotonHandler.PingImplementation = typeof(PingHttp);
        #endif


        #if !UNITY_EDITOR && (UNITY_WINRT)
        // this automatically uses a separate assembly-file with Win8-style Socket usage (not possible in Editor)
        Debug.LogWarning("Using PingWindowsStore");
        PhotonHandler.PingImplementation = typeof(PingWindowsStore);    // but for ping, we have to set the implementation explicitly to Win 8 Store/Phone
        #endif


        #pragma warning disable 0162    // the library variant defines if we should use PUN's SocketUdp variant (at all)
        if (PhotonPeer.NoSocket)
        {
            #if !UNITY_EDITOR && (UNITY_PS3 || UNITY_ANDROID)
            this.SocketImplementationConfig[ConnectionProtocol.Udp] = typeof(SocketUdpNativeDynamic);
            PhotonHandler.PingImplementation = typeof(PingNativeDynamic);
            #elif !UNITY_EDITOR && (UNITY_IPHONE || UNITY_SWITCH)
            this.SocketImplementationConfig[ConnectionProtocol.Udp] = typeof(SocketUdpNativeStatic);
            PhotonHandler.PingImplementation = typeof(PingNativeStatic);
            #elif !UNITY_EDITOR && UNITY_WINRT
            // this automatically uses a separate assembly-file with Win8-style Socket usage (not possible in Editor)
            #else
            this.SocketImplementationConfig[ConnectionProtocol.Udp] = typeof(SocketUdp);
            PhotonHandler.PingImplementation = typeof(PingMonoEditor);
            #endif

            if (this.SocketImplementationConfig[ConnectionProtocol.Udp] == null)
            {
                Debug.Log("No socket implementation set for 'NoSocket' assembly. Please check your settings.");
            }
        }
        #pragma warning restore 0162

        if (PhotonHandler.PingImplementation == null)
        {
            PhotonHandler.PingImplementation = typeof(PingMono);
        }


        if (this.TransportProtocol == protocolOverride)
        {
            return;
        }


        if (PhotonNetwork.logLevel >= PhotonLogLevel.ErrorsOnly)
        {
            Debug.LogWarning("Protocol switch from: " + this.TransportProtocol + " to: " + protocolOverride + ".");
        }

        this.TransportProtocol = protocolOverride;
    }

    /// <summary>
    /// Complete disconnect from photon (and the open master OR game server)
    /// </summary>
    public override void Disconnect()
    {
        if (this.PeerState == PeerStateValue.Disconnected)
        {
            if (!PhotonHandler.AppQuits)
            {
                Debug.LogWarning(string.Format("Can't execute Disconnect() while not connected. Nothing changed. State: {0}", this.State));
            }
            return;
        }

        this.State = ClientState.Disconnecting;
        base.Disconnect();
    }

    public void TransferGameServer(string serverAddress)
    {
        GameServerAddress = serverAddress;
        this.State = ClientState.DisconnectingForTransferServer;
        base.Disconnect();
    }

    public bool ReconnectToGameServer()
    {
        if (this.cachedServerType == ServerConnection.GameServer)
            return Reconnect();
        return false;
    }

    private bool CallAuthenticate()
    {
        // once encryption is availble, the client should send one (secure) authenticate. it includes the AppId (which identifies your app on the Photon Cloud)
        AuthenticationValues auth = this.AuthValues ?? new AuthenticationValues();
        if (this.AuthMode == AuthModeOption.Auth)
        {
            return this.OpAuthenticate(this.AppId, this.AppVersion, auth, this.CloudRegion.ToString());
        }
        else
        {
            return this.OpAuthenticateOnce(this.AppId, this.AppVersion, auth, this.CloudRegion.ToString(), this.EncryptionMode, PhotonNetwork.PhotonServerSettings.Protocol);
        }
    }

    /// <summary>
    /// Internally used only. Triggers OnStateChange with "Disconnect" in next dispatch which is the signal to re-connect (if at all).
    /// </summary>
    private void DisconnectToReconnect()
    {
        switch (this.Server)
        {
            case ServerConnection.NameServer:
                this.State = ClientState.DisconnectingFromNameServer;
                base.Disconnect();
                break;
            case ServerConnection.MasterServer:
                this.State = ClientState.DisconnectingFromMasterserver;
                base.Disconnect();
                break;
            case ServerConnection.GameServer:
                this.State = ClientState.DisconnectingFromGameserver;
                base.Disconnect();
                break;
        }
    }

    /// <summary>
    /// While on the NameServer, this gets you the list of regional servers (short names and their IPs to ping them).
    /// </summary>
    /// <returns>If the operation could be sent. If false, no operation was sent (e.g. while not connected to the NameServer).</returns>
    public bool GetRegions()
    {
        if (this.Server != ServerConnection.NameServer)
        {
            return false;
        }

        bool sent = this.OpGetRegions(this.AppId);
        if (sent)
        {
            this.AvailableRegions = null;
        }

        return sent;
    }

    public void OpConnectGameServer(string gameserveraddr)
    {
        if (this.Server == ServerConnection.GameServer)
        {
            Debug.Log("Still connected to GameServer, OpConnectGameServer Failed!");
            return;
        }
        else
        {
            this.GameServerAddress = gameserveraddr;
            this.DisconnectToReconnect();
        }
    }

    public override bool OpRaiseEvent(byte eventCode, object customEventContent, bool sendReliable, RaiseEventOptions raiseEventOptions)
    {
        if (PhotonNetwork.offlineMode)
        {
            return false;
        }

        return base.OpRaiseEvent(eventCode, customEventContent, sendReliable, raiseEventOptions);
    }


    #if PHOTON_LIB_MIN_4_1_2
    public override bool OpRaiseEvent(byte eventCode, object customEventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
    {
        if (PhotonNetwork.offlineMode)
        {
            return false;
        }

        return base.OpRaiseEvent(eventCode, customEventContent, raiseEventOptions, sendOptions);
    }
    #endif

    #endregion

    #region Implementation of IPhotonPeerListener

    public void DebugReturn(DebugLevel level, string message)
    {
        if (level == DebugLevel.ERROR)
        {
            Debug.LogError(message);
        }
        else if (level == DebugLevel.WARNING)
        {
            Debug.LogWarning(message);
        }
        else if (level == DebugLevel.INFO && PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
        {
            Debug.Log(message);
        }
        else if (level == DebugLevel.ALL && PhotonNetwork.logLevel == PhotonLogLevel.Full)
        {
            Debug.Log(message);
        }
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        if (PhotonNetwork.networkingPeer.State == ClientState.Disconnecting)
        {
            if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
            {
                Debug.Log("OperationResponse ignored while disconnecting. Code: " + operationResponse.OperationCode);
            }
            return;
        }

        // extra logging for error debugging (helping developers with a bit of automated analysis)
        if (operationResponse.ReturnCode == 0)
        {
            if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
                Debug.Log(operationResponse.ToString());
        }

        // use the "secret" or "token" whenever we get it. doesn't really matter if it's in AuthResponse.
        Dictionary<byte, object> opParams = operationResponse.Parameters;
        switch (operationResponse.OperationCode)
        {
            case OperationCode.Authenticate:
            case OperationCode.AuthenticateOnce:
            {
                // ClientState oldState = this.State;
                if (operationResponse.ReturnCode != 0)
                {
                    if (operationResponse.ReturnCode == ErrorCode.InvalidOperation)
                    {
                        Debug.LogError(string.Format("If you host Photon yourself, make sure to start the 'Instance LoadBalancing' "+ this.ServerAddress));
                    }
                    else if (operationResponse.ReturnCode == ErrorCode.InvalidAuthentication)
                    {
                        //Debug.LogError(string.Format("The appId this client sent is unknown on the server (Cloud). Check settings. If using the Cloud, check account."));
                        GameInfo.gLogin.OnFailedToConnectToPhoton(DisconnectCause.InvalidAuthentication);
                    }
                    else if (operationResponse.ReturnCode == ErrorCode.CustomAuthenticationFailed)
                    {
                        //Debug.LogError(string.Format("Custom Authentication failed (either due to user-input or configuration or AuthParameter string format). Calling: OnCustomAuthenticationFailed()"));
                        //SendMonoMessage(PhotonNetworkingMessage.OnCustomAuthenticationFailed, operationResponse.DebugMessage);
                    }
                    else if (operationResponse.ReturnCode == ErrorCode.UsernameDoesNotExist)
                    {
                        //Debug.LogError("User name does not exist. Calling: OnLoginFailed()");
                        GameInfo.gLogin.OnLoginFailed(true);
                    }
                    else if (operationResponse.ReturnCode == ErrorCode.UserOrPassFailed)
                    {
                        //Debug.LogError("Username or password is wrong. Calling: OnLoginFailed()");
                        GameInfo.gLogin.OnLoginFailed(false);
                    }
                    else if (operationResponse.ReturnCode == ErrorCode.InvalidClientVer)
                    {
                        //Debug.LogError("Invalid Client Version");
                        GameInfo.gLogin.InvalidClientVer(false);
                    }
                    else if (operationResponse.ReturnCode == ErrorCode.UserBlocked)
                    {
                        GameInfo.gLogin.UserFreezed();
                    }
                    else
                    {
                        Debug.LogError(string.Format("Authentication failed: '{0}' Code: {1}", operationResponse.DebugMessage, operationResponse.ReturnCode));
                    }

                    this.State = ClientState.Disconnecting;
                    this.Disconnect();

                    if (operationResponse.ReturnCode == ErrorCode.MaxCcuReached)
                    {
                        if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
                            Debug.LogWarning(string.Format("Currently, the limit of users is reached for this title. Try again later. Disconnecting"));
                        GameInfo.gLogin.OnConnectionFail(DisconnectCause.MaxCcuReached);
                    }
                    else if (operationResponse.ReturnCode == ErrorCode.AuthenticationTicketExpired)
                    {
                        if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
                            Debug.LogError(string.Format("The authentication ticket expired. You need to connect (and authenticate) again. Disconnecting."));
                        GameInfo.gLogin.OnConnectionFail(DisconnectCause.AuthenticationTicketExpired);
                    }
                    break;
                }
                else
                {
                    if (this.Server == ServerConnection.MasterServer)
                    {
                        bool isEstablishConnection = false;
                        if (opParams.Count > 0 && opParams.ContainsKey(ParameterCode.LoginType))
                        {
                            LoginAuthType loginType = (LoginAuthType)opParams[ParameterCode.LoginType];
                            string loginID = "", cookieId = "", password = "", serverList = "", userid = "";
                            if (opParams.ContainsKey(ParameterCode.LoginId))
                                loginID = opParams[ParameterCode.LoginId] as string;
                            if (opParams.ContainsKey(ParameterCode.CookieId))
                                cookieId = opParams[ParameterCode.CookieId] as string;
                            if (opParams.ContainsKey(ParameterCode.UniqueId))
                                userid = opParams[ParameterCode.UniqueId] as string;
                            if (opParams.ContainsKey(ParameterCode.Password))
                                password = opParams[ParameterCode.Password] as string;
                            if (opParams.ContainsKey(ParameterCode.ServerList))
                                serverList = opParams[ParameterCode.ServerList] as string;

                            switch (loginType)
                            {
                                case LoginAuthType.EstablishConnection:
                                    GameInfo.gLogin.OnEstablishedConnection(loginID, serverList);
                                    isEstablishConnection = true;
                                    break;
                                case LoginAuthType.Device:
                                case LoginAuthType.Facebook:
                                case LoginAuthType.Google:
                                    GameInfo.gLogin.OnLoginSuccess(loginType.ToString(), loginID, cookieId, userid, serverList, "");
                                    break;
                                case LoginAuthType.Username:
                                    GameInfo.gLogin.OnLoginSuccess(loginType.ToString(), loginID, cookieId, userid, serverList, password);
                                    break;                               
                            }
                        }
                        if (!isEstablishConnection)
                        {
                            this.State = ClientState.ConnectedToMaster;
                            GameInfo.gLogin.OnConnectedToMaster();
                        }
                    }
                    else if (this.Server == ServerConnection.GameServer)
                        GameInfo.gLogin.OnConnectedToGameServer();
                }
                break;
            }

            case OperationCode.Register:
                {
                    string loginId = "";
                    if (opParams.ContainsKey(ParameterCode.LoginId))
                        loginId = opParams[ParameterCode.LoginId] as string;                  
                    GameInfo.gLogin.OnRegisterResult(operationResponse.ReturnCode == ErrorCode.Ok, loginId);
                    break;
                }

            case OperationCode.GetServerList:
                {
                    if (operationResponse.ReturnCode == ErrorCode.InvalidClientVer)
                    {
                        Debug.LogError("Invalid Client Version");
                        GameInfo.gLogin.InvalidClientVer(false);
                    }
                    else
                    {
                        string serverList = "";
                        if (opParams.ContainsKey(ParameterCode.ServerList))
                            serverList = opParams[ParameterCode.ServerList] as string;
                        GameInfo.gLogin.OnGetServerListResult(serverList);
                    }
                    break;
                }

            case OperationCode.AuthenticateCookie:
                {
                    string peerAppId = "";
                    if (opParams.Count > 0)
                    {
                        if (opParams.ContainsKey(ParameterCode.ApplicationId))
                            peerAppId = opParams[ParameterCode.ApplicationId] as string;
                        if (opParams.ContainsKey(ParameterCode.VoiceChatAddress))
                            GameInfo.mVoiceChatAddress = opParams[ParameterCode.VoiceChatAddress] as string;
                    }
                    // Also now check for game server full during cookie auth
                    GameInfo.gLogin.OnAuthenticatedCookie(operationResponse.ReturnCode, peerAppId);
                    break;
                }

            case OperationCode.ConnectGameSetup:
                {
                    GameInfo.gLogin.OnConnectGameSetup(operationResponse.ReturnCode);
                    break;
                }

            case OperationCode.VerifyLoginId:
                {
                    string loginId = "", requestType = "";
                    if (opParams.Count > 0)
                    {
                        if (opParams.ContainsKey(ParameterCode.LoginId))
                            loginId = opParams[ParameterCode.LoginId] as string;
                        if (opParams.ContainsKey(ParameterCode.DeviceId))
                            requestType = opParams[ParameterCode.DeviceId] as string;
                    }
                    GameInfo.gLogin.OnVerifiedLoginId(operationResponse.ReturnCode != ErrorCode.Ok, loginId, requestType);
                    break;
                }

            //case OperationCode.PasswordModify:
            //{
            //    string pass = "";
            //    if(opParams.ContainsKey(ParameterCode.Password))
            //        pass = opParams[ParameterCode.Password] as string;
            //    GameInfo.gLogin.OnPasswordModifyResult, operationResponse.ReturnCode, pass);
            //    break;
            //}

            //case OperationCode.UIDShift:
            //{
            //    LoginType loginType = LoginType.Device;
            //    string loginId="", pass = "";
            //    if(opParams.Count > 0)
            //    {
            //        if(opParams.ContainsKey(ParameterCode.LoginType))
            //            loginType = (LoginType)opParams[ParameterCode.LoginType];
            //        if(opParams.ContainsKey(ParameterCode.LoginId))
            //            loginId = opParams[ParameterCode.LoginId] as string;
            //        if(opParams.ContainsKey(ParameterCode.Password))
            //            pass = opParams[ParameterCode.Password] as string;
            //    }
            //    GameInfo.gLogin.OnUIDShiftResult(operationResponse.ReturnCode, loginType, loginId, pass);
            //    break;
            //}

            case OperationCode.RaiseEvent:
                // this usually doesn't give us a result. only if the caching is affected the server will send one.
                break;

            case OperationCode.WebRpc:
                SendZealotMessage(PhotonNetworkingMessage.OnWebRpcResponse, operationResponse);
                break;

            case OperationCode.Combat:
                SendZealotMessage(PhotonNetworkingMessage.OnCombatEvent, operationResponse);
                break;

            case OperationCode.NonCombat:
                SendZealotMessage(PhotonNetworkingMessage.OnNonCombatEvent, operationResponse);
                break;

            case OperationCode.UnreliableCombat:
                SendZealotMessage(PhotonNetworkingMessage.OnUnreliableCombatEvent, operationResponse);
                break;

            case OperationCode.Lobby:
                SendZealotMessage(PhotonNetworkingMessage.OnLobbyEvent, operationResponse);
                break;

            case OperationCode.Action:
                SendZealotMessage(PhotonNetworkingMessage.OnActionEvent, operationResponse);
                break;

            case OperationCode.LocalObject:
                SendZealotMessage(PhotonNetworkingMessage.OnLocalObjectEvent, operationResponse);
                break;

            default:
                Debug.LogWarning(string.Format("OperationResponse unhandled: {0}", operationResponse.ToString()));
                break;
        }

        //this.externalListener.OnOperationResponse(operationResponse);
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
            Debug.Log(string.Format("OnStatusChanged: {0} current State: {1}", statusCode.ToString(), this.State));

        switch (statusCode)
        {
            case StatusCode.Connect:
                if (this.State == ClientState.ConnectingToGameserver)
                {
                    if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
                        Debug.Log("Connected to gameserver.");

                    this.Server = ServerConnection.GameServer;
                    this.State = ClientState.ConnectedToGameserver;
                }

                if (this.State == ClientState.ConnectingToMasterserver)
                {
                    if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
                        Debug.Log("Connected to masterserver.");

                    this.Server = ServerConnection.MasterServer;
                    this.State = ClientState.Authenticating;  // photon v4 always requires OpAuthenticate. even self-hosted Photon Server

                    if (this.IsInitialConnect)
                    {
                        this.IsInitialConnect = false;  // after handling potential initial-connect issues with special messages, we are now sure we can reach a server
                        //SendMonoMessage(PhotonNetworkingMessage.OnConnectedToPhoton);
                    }
                }


                if (this.TransportProtocol != ConnectionProtocol.WebSocketSecure)
                {
                    if (this.Server == ServerConnection.NameServer || this.AuthMode == AuthModeOption.Auth)
                    {
                        if (!PhotonNetwork.offlineMode)
                            this.EstablishEncryption();
                    }
                }
                else
                {
                    if (DebugOut == DebugLevel.INFO)
                    {
                        Debug.Log("Skipping EstablishEncryption. Protocol is secure.");
                    }

                    goto case StatusCode.EncryptionEstablished;
                }
                break;

            case StatusCode.EncryptionEstablished:
                // reset flags
                _isReconnecting = false;
                if (this.AuthMode == AuthModeOption.AuthOnce || this.AuthMode == AuthModeOption.AuthOnceWss)
                {
                    // AuthMode "Once" means we only authenticate on the NameServer
                    Debug.Log("didAuthenticate " + didAuthenticate + " AuthMode "+ AuthMode);
                    break;
                }

                // we might need to authenticate automatically now, so the client can do anything at all
                if (!this.didAuthenticate)
                {
                    if (this.Server == ServerConnection.GameServer) //for game server we use AuthenticateCookie
                    {
                        this.didAuthenticate = true;
                        GameInfo.gLogin.OnConnectedToGameServer();
                    }
                    else
                        this.didAuthenticate = this.CallAuthenticate();
                    if (this.didAuthenticate)
                        this.State = ClientState.Authenticating;
                }
                break;

            case StatusCode.EncryptionFailedToEstablish:
                Debug.LogError("Encryption wasn't established: " + statusCode + ". Going to authenticate anyways.");
                //AuthenticationValues authV = this.AuthValues ?? new AuthenticationValues();
                //this.OpAuthenticate(this.AppId, this.AppVersion, authV, this.CloudRegion.ToString());     // TODO: check if there are alternatives
                this.Disconnect();
                break;

            case StatusCode.Disconnect:
                this.didAuthenticate = false;
                if (this.State == ClientState.DisconnectingFromMasterserver)
                {
                    if (this.Connect(this.GameServerAddress, ServerConnection.GameServer))
                    {
                        this.State = ClientState.ConnectingToGameserver;
                    }
                }
                else if (this.State == ClientState.DisconnectingFromGameserver)
                {
                    this.SetupProtocol(ServerConnection.MasterServer);
                    if (this.Connect(this.MasterServerAddress, ServerConnection.MasterServer))
                    {
                        this.State = ClientState.ConnectingToMasterserver;
                    }
                }
                else if(this.State == ClientState.DisconnectingForTransferServer)
                {
                    GameInfo.OnDisconnect();
                    if (this.Connect(this.GameServerAddress, ServerConnection.GameServer))
                    {
                        this.State = ClientState.ConnectingToGameserver;
                    }
                }
                else
                {
                    if (_isReconnecting)
                    {
                        return;
                    }

                    if (this.AuthValues != null)
                    {
                        this.AuthValues.Token = null;       // invalidate any custom auth secrets
                    }

                    this.IsInitialConnect = false;          // not "connecting" anymore
                    this.State = ClientState.PeerCreated;   // if we set another state here, we could keep clients from connecting in OnDisconnectedFromPhoton right here.
                    GameInfo.gLogin.OnDisconnectedFromPhoton();
                }
                break;

            case StatusCode.ExceptionOnConnect:
            case StatusCode.SecurityExceptionOnConnect:
                this.IsInitialConnect = false;

                this.State = ClientState.PeerCreated;
                if (this.AuthValues != null)
                {
                    this.AuthValues.Token = null;  // invalidate any custom auth secrets
                }

                DisconnectCause cause = (DisconnectCause)statusCode;
                GameInfo.gLogin.OnFailedToConnectToPhoton(cause);
                break;

            case StatusCode.Exception:
                if (this.IsInitialConnect)
                {
                    Debug.LogError("Exception while connecting to: " + this.ServerAddress + ". Check if the server is available.");
                    if (this.ServerAddress == null || this.ServerAddress.StartsWith("127.0.0.1"))
                    {
                        Debug.LogWarning("The server address is 127.0.0.1 (localhost): Make sure the server is running on this machine. Android and iOS emulators have their own localhost.");
                        if (this.ServerAddress == this.GameServerAddress)
                        {
                            Debug.LogWarning("This might be a misconfiguration in the game server config. You need to edit it to a (public) address.");
                        }
                    }

                    this.State = ClientState.PeerCreated;
                    cause = (DisconnectCause)statusCode;
                    this.IsInitialConnect = false;
                    GameInfo.gLogin.OnFailedToConnectToPhoton(cause);
                }
                else
                {
                    this.State = ClientState.PeerCreated;
                    cause = (DisconnectCause)statusCode;
                    GameInfo.gLogin.OnConnectionFail(cause);
                }

                this.Disconnect();
                break;

            case StatusCode.TimeoutDisconnect:
                if (this.IsInitialConnect)
                {
                    if (!_isReconnecting)
                    {
                        Debug.LogWarning(statusCode + " while connecting to: " + this.ServerAddress + ". Check if the server is available.");

                        this.IsInitialConnect = false;
                        cause = (DisconnectCause)statusCode;
                        GameInfo.gLogin.OnFailedToConnectToPhoton(cause);
                    }
                }
                else
                {
                    cause = (DisconnectCause)statusCode;
                    GameInfo.gLogin.OnConnectionFail(cause);
                }
                if (this.AuthValues != null)
                {
                    this.AuthValues.Token = null;  // invalidate any custom auth secrets
                }

                /* JF: we need this when reconnecting and joining.
                if (this.ServerAddress.Equals(this.GameServerAddress))
                {
                    this.GameServerAddress = null;
                }
                if (this.ServerAddress.Equals(this.MasterServerAddress))
                {
                    this.ServerAddress = null;
                }
                */

                this.Disconnect();
                break;

            case StatusCode.ExceptionOnReceive:
            case StatusCode.DisconnectByServer:
            case StatusCode.DisconnectByServerLogic:
            case StatusCode.DisconnectByServerUserLimit:
                if (this.IsInitialConnect)
                {
                    Debug.LogWarning(statusCode + " while connecting to: " + this.ServerAddress + ". Check if the server is available.");

                    this.IsInitialConnect = false;
                    cause = (DisconnectCause)statusCode;
                    GameInfo.gLogin.OnFailedToConnectToPhoton(cause);
                }
                else
                {
                    cause = (DisconnectCause)statusCode;
                    GameInfo.gLogin.OnConnectionFail(cause);
                }
                if (this.AuthValues != null)
                {
                    this.AuthValues.Token = null;  // invalidate any custom auth secrets
                }

                this.Disconnect();
                break;

            default:
                // this.mListener.serverErrorReturn(statusCode.value());
                Debug.LogError("Received unknown status code: " + statusCode);
                break;
        }

        //this.externalListener.OnStatusChanged(statusCode);
    }


    public void OnEvent(EventData photonEvent)
    {
        if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
            Debug.Log(string.Format("OnEvent: {0}", photonEvent.ToString()));

        switch (photonEvent.Code)
        {            
            case OperationCode.Combat:
                SendZealotMessage(PhotonNetworkingMessage.OnCombatEvent, photonEvent);
                break;

            case OperationCode.UnreliableCombat:
                SendZealotMessage(PhotonNetworkingMessage.OnUnreliableCombatEvent, photonEvent);
                break;

            case OperationCode.NonCombat:
                SendZealotMessage(PhotonNetworkingMessage.OnNonCombatEvent, photonEvent);
                break;

            case OperationCode.Action:
                SendZealotMessage(PhotonNetworkingMessage.OnActionEvent, photonEvent);
                break;

            case OperationCode.Lobby:
                SendZealotMessage(PhotonNetworkingMessage.OnLobbyEvent, photonEvent);
                break;

            case OperationCode.LocalObject:
                SendZealotMessage(PhotonNetworkingMessage.OnLocalObjectEvent, photonEvent);
                break;

            default:
                Debug.LogWarning("Warning: Unhandled event " + photonEvent.Code);
                break;
        }

        //this.externalListener.OnEvent(photonEvent);
    }
#endregion

    private void SetupEncryption(Dictionary<byte, object> encryptionData)
    {
        // this should not be called when authentication is done per server. this mode does not support the required "key-exchange via token"
        if (this.AuthMode == AuthModeOption.Auth)
        {
            if (DebugOut == DebugLevel.ERROR)
            {
                UnityEngine.Debug.LogWarning("SetupEncryption() called but ignored. Not XB1 compiled. EncryptionData: " + encryptionData.ToStringFull());
                return;
            }
        }

        // for AuthOnce and AuthOnceWss, we can keep the same secret across machines (for the session, basically)
        if (DebugOut == DebugLevel.INFO)
        {
            UnityEngine.Debug.Log("SetupEncryption() got called. "+encryptionData.ToStringFull());
        }

        var mode = (EncryptionMode)(byte)encryptionData[EncryptionDataParameters.Mode];
        switch (mode)
        {
            case EncryptionMode.PayloadEncryption:
                byte[] secret = (byte[])encryptionData[EncryptionDataParameters.Secret1];
                this.InitPayloadEncryption(secret);
                break;
            case EncryptionMode.DatagramEncryption:
                {
                    byte[] secret1 = (byte[])encryptionData[EncryptionDataParameters.Secret1];
                    byte[] secret2 = (byte[])encryptionData[EncryptionDataParameters.Secret2];
                    this.InitDatagramEncryption(secret1, secret2);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void SendZealotMessage(PhotonNetworkingMessage msgType, params object[] parameters)
    {
        object callParameter = (parameters != null && parameters.Length == 1) ? parameters[0] : parameters;
        if (GameInfo.gClientState == GameClientState.Combat)
            GameInfo.gCombat.OnZealotRPCEvent(msgType, callParameter as EventData);
        else if (GameInfo.gClientState == GameClientState.Lobby)
            GameInfo.gLobby.OnZealotRPCEvent(msgType, callParameter as EventData);
    }

    public static void SendMonoMessage(PhotonNetworkingMessage methodString, params object[] parameters)
    {
        HashSet<GameObject> objectsToCall;
        if (PhotonNetwork.SendMonoMessageTargets != null)
        {
            objectsToCall = PhotonNetwork.SendMonoMessageTargets;
        }
        else
        {
            objectsToCall = PhotonNetwork.FindGameObjectsWithComponent(PhotonNetwork.SendMonoMessageTargetType);
        }

        string methodName = methodString.ToString();
        object callParameter = (parameters != null && parameters.Length == 1) ? parameters[0] : parameters;
        foreach (GameObject gameObject in objectsToCall)
        {
            if (gameObject!=null)
            {
                gameObject.SendMessage(methodName, callParameter, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void NewSceneLoaded()
    {
        if (this.loadingLevelAndPausedNetwork)
        {
            this.loadingLevelAndPausedNetwork = false;
            PhotonNetwork.isMessageQueueRunning = true;
        }
    }

    public void SetApp(string appId, string gameVersion)
    {
        this.AppId = appId.Trim();
        if (!string.IsNullOrEmpty(gameVersion))
        {
            PhotonNetwork.gameVersion = gameVersion.Trim();
        }
    }

    public bool WebRpc(string uriPath, object parameters)
    {
        Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
        opParameters.Add(ParameterCode.UriPath, uriPath);
        opParameters.Add(ParameterCode.WebRpcParameters, parameters);

        return this.OpCustom(OperationCode.WebRpc, opParameters, true);
    }
}
