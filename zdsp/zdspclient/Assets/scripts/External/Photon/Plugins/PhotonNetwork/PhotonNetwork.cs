// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhotonNetwork.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System.Diagnostics;
using UnityEngine;
using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Hashtable = ExitGames.Client.Photon.Hashtable;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif


/// <summary>
/// The main class to use the PhotonNetwork plugin.
/// This class is static.
/// </summary>
/// \ingroup publicApi
public static class PhotonNetwork
{
    /// <summary>Version number of PUN. Also used in GameVersion to separate client version from each other.</summary>
    public const string versionPUN = "1.88";

    /// <summary>Version string for your this build. Can be used to separate incompatible clients. Sent during connect.</summary>
    /// <remarks>This is only sent when you connect so that is also the place you set it usually (e.g. in ConnectUsingSettings).</remarks>
    public static string gameVersion { get; set; }

    /// <summary>
    /// This Monobehaviour allows Photon to run an Update loop.
    /// </summary>
    internal static readonly PhotonHandler photonMono;

    /// <summary>
    /// Photon peer class that implements LoadBalancing in PUN.
    /// Primary use is internal (by PUN itself).
    /// </summary>
    internal static NetworkingPeer networkingPeer;

    /// <summary>
    /// The maximum number of assigned PhotonViews <i>per player</i> (or scene). See the [General Documentation](@ref general) topic "Limitations" on how to raise this limitation.
    /// </summary>
    public static readonly int MAX_VIEW_IDS = 1000; // VIEW & PLAYER LIMIT CAN BE EASILY CHANGED, SEE DOCS


    /// <summary>Name of the PhotonServerSettings file (used to load and by PhotonEditor to save new files).</summary>
    internal const string serverSettingsAssetFile = "PhotonServerSettings";

    /// <summary>Serialized server settings, written by the Setup Wizard for use in ConnectUsingSettings.</summary>
    public static ServerSettings PhotonServerSettings = (ServerSettings)Resources.Load(PhotonNetwork.serverSettingsAssetFile, typeof(ServerSettings));

    /// <summary>Currently used server address (no matter if master or game server).</summary>
    public static string ServerAddress { get { return (networkingPeer != null) ? networkingPeer.ServerAddress : "<not connected>"; } }

    /// <summary>Currently used Cloud Region (if any). As long as the client is not on a Master Server or Game Server, the region is not yet defined.</summary>
    public static CloudRegionCode CloudRegion { get { return (networkingPeer != null && connected && Server!=ServerConnection.NameServer) ? networkingPeer.CloudRegion : CloudRegionCode.none; } }

    /// <summary>
    /// False until you connected to Photon initially. True in offline mode, while connected to any server and even while switching servers.
    /// </summary>
    public static bool connected
    {
        get
        {
            if (offlineMode)
            {
                return true;
            }

            if (networkingPeer == null)
            {
                return false;
            }

            return !networkingPeer.IsInitialConnect && networkingPeer.State != ClientState.PeerCreated && networkingPeer.State != ClientState.Disconnected && networkingPeer.State != ClientState.Disconnecting && networkingPeer.State != ClientState.ConnectingToNameServer;
        }
    }

    /// <summary>
    /// True when you called ConnectUsingSettings (or similar) until the low level connection to Photon gets established.
    /// </summary>
    public static bool connecting
    {
        get { return networkingPeer.IsInitialConnect && !offlineMode; }
    }

    /// <summary>
    /// A refined version of connected which is true only if your connection to the server is ready to accept operations like join, leave, etc.
    /// </summary>
    public static bool connectedAndReady
    {
        get
        {
            // connected property will check offlineMode and networkingPeer being null
            if (!connected)
            {
                return false;
            }

            if (offlineMode)
            {
                return true;
            }

            switch (connectionStateDetailed)
            {
                case ClientState.PeerCreated:
                case ClientState.Disconnected:
                case ClientState.Disconnecting:
                case ClientState.Authenticating:
                case ClientState.ConnectingToGameserver:
                case ClientState.ConnectingToMasterserver:
                case ClientState.ConnectingToNameServer:
                case ClientState.Joining:
                    return false;   // we are not ready to execute any operations
            }

            return true;
        }
    }

    /// <summary>
    /// Simplified connection state
    /// </summary>
    public static ConnectionState connectionState
    {
        get
        {
            if (offlineMode)
            {
                return ConnectionState.Connected;
            }

            if (networkingPeer == null)
            {
                return ConnectionState.Disconnected;
            }

            switch (networkingPeer.PeerState)
            {
                case PeerStateValue.Disconnected:
                    return ConnectionState.Disconnected;
                case PeerStateValue.Connecting:
                    return ConnectionState.Connecting;
                case PeerStateValue.Connected:
                    return ConnectionState.Connected;
                case PeerStateValue.Disconnecting:
                    return ConnectionState.Disconnecting;
                case PeerStateValue.InitializingApplication:
                    return ConnectionState.InitializingApplication;
            }

            return ConnectionState.Disconnected;
        }
    }

    /// <summary>
    /// Detailed connection state (ignorant of PUN, so it can be "disconnected" while switching servers).
    /// </summary>
    /// <remarks>
    /// In OfflineMode, this is ClientState.Joined (after create/join) or it is ConnectedToMaster in all other cases.
    /// </remarks>
    public static ClientState connectionStateDetailed
    {
        get
        {
            if (offlineMode)
            {
                return ClientState.ConnectedToMaster;
            }

            if (networkingPeer == null)
            {
                return ClientState.Disconnected;
            }

            return networkingPeer.State;
        }
    }

    /// <summary>The server (type) this client is currently connected or connecting to.</summary>
    /// <remarks>Photon uses 3 different roles of servers: Name Server, Master Server and Game Server.</remarks>
    public static ServerConnection Server { get { return (PhotonNetwork.networkingPeer != null) ? PhotonNetwork.networkingPeer.Server : ServerConnection.NameServer; } }

    /// <summary>
    /// A user's authentication values used during connect.
    /// </summary>
    /// <remarks>
    /// Set these before calling Connect if you want custom authentication.
    /// These values set the userId, if and how that userId gets verified (server-side), etc..
    ///
    /// If authentication fails for any values, PUN will call your implementation of OnCustomAuthenticationFailed(string debugMsg).
    /// See: PhotonNetworkingMessage.OnCustomAuthenticationFailed
    /// </remarks>
    public static AuthenticationValues AuthValues
    {
        get { return (networkingPeer != null) ? networkingPeer.AuthValues : null; }
        set { if (networkingPeer != null) networkingPeer.AuthValues = value; }
    }

    /// <summary>
    /// Network log level. Controls how verbose PUN is.
    /// </summary>
    public static PhotonLogLevel logLevel = PhotonLogLevel.ErrorsOnly;

    /// <summary>
    /// The minimum difference that a Vector2 or Vector3(e.g. a transforms rotation) needs to change before we send it via a PhotonView's OnSerialize/ObservingComponent.
    /// </summary>
    /// <remarks>
    /// Note that this is the sqrMagnitude. E.g. to send only after a 0.01 change on the Y-axix, we use 0.01f*0.01f=0.0001f. As a remedy against float inaccuracy we use 0.000099f instead of 0.0001f.
    /// </remarks>
    public static float precisionForVectorSynchronization = 0.000099f;

    /// <summary>
    /// The minimum angle that a rotation needs to change before we send it via a PhotonView's OnSerialize/ObservingComponent.
    /// </summary>
    public static float precisionForQuaternionSynchronization = 1.0f;

    /// <summary>
    /// The minimum difference between floats before we send it via a PhotonView's OnSerialize/ObservingComponent.
    /// </summary>
    public static float precisionForFloatSynchronization = 0.01f;

    /// <summary>
    /// While enabled, the MonoBehaviours on which we call RPCs are cached, avoiding costly GetComponents<MonoBehaviour>() calls.
    /// </summary>
    /// <remarks>
    /// RPCs are called on the MonoBehaviours of a target PhotonView. Those have to be found via GetComponents.
    ///
    /// When set this to true, the list of MonoBehaviours gets cached in each PhotonView.
    /// You can use photonView.RefreshRpcMonoBehaviourCache() to manually refresh a PhotonView's
    /// list of MonoBehaviours on demand (when a new MonoBehaviour gets added to a networked GameObject, e.g.).
    /// </remarks>
    public static bool UseRpcMonoBehaviourCache;

    /// <summary>
    /// While enabled (true), Instantiate uses PhotonNetwork.PrefabCache to keep game objects in memory (improving instantiation of the same prefab).
    /// </summary>
    /// <remarks>
    /// Setting UsePrefabCache to false during runtime will not clear PrefabCache but will ignore it right away.
    /// You could clean and modify the cache yourself. Read its comments.
    /// </remarks>
    public static bool UsePrefabCache = true;

    /// <summary>
    /// Keeps references to GameObjects for frequent instantiation (out of memory instead of loading the Resources).
    /// </summary>
    /// <remarks>
    /// You should be able to modify the cache anytime you like, except while Instantiate is used. Best do it only in the main-Thread.
    /// </remarks>
    public static Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>();

    /// <summary>
    /// If not null, this is the (exclusive) list of GameObjects that get called by PUN SendMonoMessage().
    /// </summary>
    /// <remarks>
    /// For all callbacks defined in PhotonNetworkingMessage, PUN will use SendMonoMessage and
    /// call FindObjectsOfType() to find all scripts and GameObjects that might want a callback by PUN.
    ///
    /// PUN callbacks are not very frequent (in-game, property updates are most frequent) but
    /// FindObjectsOfType is time consuming and with a large number of GameObjects, performance might
    /// suffer.
    ///
    /// Optionally, SendMonoMessageTargets can be used to supply a list of target GameObjects. This
    /// skips the FindObjectsOfType() but any GameObject that needs callbacks will have to Add itself
    /// to this list.
    ///
    /// If null, the default behaviour is to do a SendMessage on each GameObject with a MonoBehaviour.
    /// </remarks>
    public static HashSet<GameObject> SendMonoMessageTargets;


    /// <summary>
    /// Defines which classes can contain PUN Callback implementations.
    /// </summary>
    /// <remarks>
    /// This provides the option to optimize your runtime for speed.<br/>
    /// The more specific this Type is, the fewer classes will be checked with reflection for callback methods.
    /// </remarks>
    public static Type SendMonoMessageTargetType = typeof(Photon.MonoBehaviour);

    /// <summary>
    /// Can be used to skip starting RPCs as Coroutine, which can be a performance issue.
    /// </summary>
    public static bool StartRpcsAsCoroutine = true;

    /// <summary>
    /// Offline mode can be set to re-use your multiplayer code in singleplayer game modes.
    /// When this is on PhotonNetwork will not create any connections and there is near to
    /// no overhead. Mostly usefull for reusing RPC's and PhotonNetwork.Instantiate
    /// </summary>
    public static bool offlineMode
    {
        get
        {
            return isOfflineMode;
        }

        set
        {
            if (value == isOfflineMode)
            {
                return;
            }

            if (value && connected)
            {
                Debug.LogError("Can't start OFFLINE mode while connected!");
                return;
            }

            if (networkingPeer.PeerState != PeerStateValue.Disconnected)
            {
                networkingPeer.Disconnect(); // Cleanup (also calls OnLeftRoom to reset stuff)
            }
            isOfflineMode = value;
            if (isOfflineMode)
            {
                GameInfo.gLogin.OnConnectedToMaster();
            }
        }
    }

    private static bool isOfflineMode = false;

    /// <summary>
    /// Defines how many times per second PhotonNetwork should send a package. If you change
    /// this, do not forget to also change 'sendRateOnSerialize'.
    /// </summary>
    /// <remarks>
    /// Less packages are less overhead but more delay.
    /// Setting the sendRate to 50 will create up to 50 packages per second (which is a lot!).
    /// Keep your target platform in mind: mobile networks are slower and less reliable.
    /// </remarks>
    public static int sendRate
    {
        get
        {
            return 1000 / sendInterval;
        }

        set
        {
            sendInterval = 1000 / value;
            if (photonMono != null)
            {
                photonMono.updateInterval = sendInterval;
            }

            if (value < sendRateOnSerialize)
            {
                // sendRateOnSerialize needs to be <= sendRate
                sendRateOnSerialize = value;
            }
        }
    }

    /// <summary>
    /// Defines how many times per second OnPhotonSerialize should be called on PhotonViews.
    /// </summary>
    /// <remarks>
    /// Choose this value in relation to PhotonNetwork.sendRate. OnPhotonSerialize will create updates and messages to be sent.<br/>
    /// A lower rate takes up less performance but will cause more lag.
    /// </remarks>
    public static int sendRateOnSerialize
    {
        get
        {
            return 1000 / sendIntervalOnSerialize;
        }

        set
        {
            if (value > sendRate)
            {
                Debug.LogError("Error: Can not set the OnSerialize rate higher than the overall SendRate.");
                value = sendRate;
            }

            sendIntervalOnSerialize = 1000 / value;
            if (photonMono != null)
            {
                photonMono.updateIntervalOnSerialize = sendIntervalOnSerialize;
            }
        }
    }

    private static int sendInterval = 50; // in miliseconds.

    private static int sendIntervalOnSerialize = 100; // in miliseconds. I.e. 100 = 100ms which makes 10 times/second

    /// <summary>
    /// Can be used to pause dispatching of incoming evtents (RPCs, Instantiates and anything else incoming).
    /// </summary>
    /// <remarks>
    /// While IsMessageQueueRunning == false, the OnPhotonSerializeView calls are not done and nothing is sent by
    /// a client. Also, incoming messages will be queued until you re-activate the message queue.
    ///
    /// This can be useful if you first want to load a level, then go on receiving data of PhotonViews and RPCs.
    /// The client will go on receiving and sending acknowledgements for incoming packages and your RPCs/Events.
    /// This adds "lag" and can cause issues when the pause is longer, as all incoming messages are just queued.
    /// </remarks>
    public static bool isMessageQueueRunning
    {
        get
        {
            return m_isMessageQueueRunning;
        }

        set
        {
            if (value) PhotonHandler.StartFallbackSendAckThread();
            networkingPeer.IsSendingOnlyAcks = !value;
            m_isMessageQueueRunning = value;
        }
    }

    /// <summary>Backup for property isMessageQueueRunning.</summary>
    private static bool m_isMessageQueueRunning = true;

    /// <summary>
    /// Used once per dispatch to limit unreliable commands per channel (so after a pause, many channels can still cause a lot of unreliable commands)
    /// </summary>
    public static int unreliableCommandsLimit
    {
        get
        {
            return networkingPeer.LimitOfUnreliableCommands;
        }

        set
        {
            networkingPeer.LimitOfUnreliableCommands = value;
        }
    }

    /// <summary>
    /// Photon network time, synched with the server.
    /// </summary>
    /// <remarks>
    /// v1.55<br/>
    /// This time value depends on the server's Environment.TickCount. It is different per server
    /// but inside a Room, all clients should have the same value (Rooms are on one server only).<br/>
    /// This is not a DateTime!<br/>
    ///
    /// Use this value with care: <br/>
    /// It can start with any positive value.<br/>
    /// It will "wrap around" from 4294967.295 to 0!
    /// </remarks>
    public static double time
    {
        get
        {
            uint u = (uint)ServerTimestamp;
            double t = u;
            return t / 1000;
        }
    }

    /// <summary>
    /// The current server's millisecond timestamp.
    /// </summary>
    /// <remarks>
    /// This can be useful to sync actions and events on all clients in one room.
    /// The timestamp is based on the server's Environment.TickCount.
    ///
    /// It will overflow from a positive to a negative value every so often, so
    /// be careful to use only time-differences to check the time delta when things
    /// happen.
    ///
    /// This is the basis for PhotonNetwork.time.
    /// </remarks>
    public static int ServerTimestamp
    {
        get
        {
            if (offlineMode)
            {
                if (UsePreciseTimer && startupStopwatch != null && startupStopwatch.IsRunning)
                {
                    return (int)startupStopwatch.ElapsedMilliseconds;
                }
                return Environment.TickCount;
            }

            return networkingPeer.ServerTimeInMilliSeconds;
        }
    }

	/// <summary>If true, PUN will use a Stopwatch to measure time since start/connect. This is more precise than the Environment.TickCount used by default.</summary>
    private static bool UsePreciseTimer = false;
    static Stopwatch startupStopwatch;

    /// <summary>
    /// Defines how many seconds PUN keeps the connection, after Unity's OnApplicationPause(true) call. Default: 60 seconds.
    /// </summary>
    /// <remarks>
    /// It's best practice to disconnect inactive apps/connections after a while but to also allow users to take calls, etc..
    /// We think a reasonable backgroung timeout is 60 seconds.
    ///
    /// To handle the timeout, implement: OnDisconnectedFromPhoton(), as usual.
    /// Your application will "notice" the background disconnect when it becomes active again (running the Update() loop).
    ///
    /// If you need to separate this case from others, you need to track if the app was in the background
    /// (there is no special callback by PUN).
    ///
    /// A value below 0.1 seconds will disable this timeout (careful: connections can be kept indefinitely).
    ///
    ///
    /// Info:
    /// PUN is running a "fallback thread" to send ACKs to the server, even when Unity is not calling Update() regularly.
    /// This helps keeping the connection while loading scenes and assets and when the app is in the background.
    ///
    /// Note:
    /// Some platforms (e.g. iOS) don't allow to keep a connection while the app is in background.
    /// In those cases, this value does not change anything, the app immediately loses connection in background.
    ///
    /// Unity's OnApplicationPause() callback is broken in some exports (Android) of some Unity versions.
    /// Make sure OnApplicationPause() gets the callbacks you'd expect on the platform you target!
    /// Check PhotonHandler.OnApplicationPause(bool pause), to see the implementation.
    /// </remarks>
    public static float BackgroundTimeout = 60.0f;

    /// <summary>
    /// Are we the master client?
    /// </summary>
    public static bool isMasterClient
    {
        get
        {
            return true;
        }
    }

    /// <summary>Is true while being in a room (connectionStateDetailed == ClientState.Joined).</summary>
    /// <remarks>
    /// Many actions can only be executed in a room, like Instantiate or Leave, etc.
    /// You can join a room in offline mode, too.
    /// </remarks>
    public static bool inRoom
    {
        get
        {
            // in offline mode, you can be in a room too and connectionStateDetailed then returns Joined like on online mode!
            return connectionStateDetailed == ClientState.Joined;
        }
    }

    /// <summary>
    /// Enables or disables the collection of statistics about this client's traffic.
    /// </summary>
    /// <remarks>
    /// If you encounter issues with clients, the traffic stats are a good starting point to find solutions.
    /// Only with enabled stats, you can use GetVitalStats
    /// </remarks>
    public static bool NetworkStatisticsEnabled
    {
        get
        {
            return networkingPeer.TrafficStatsEnabled;
        }

        set
        {
            networkingPeer.TrafficStatsEnabled = value;
        }
    }

    /// <summary>
    /// Count of commands that got repeated (due to local repeat-timing before an ACK was received).
    /// </summary>
    /// <remarks>
    /// If this value increases a lot, there is a good chance that a timeout disconnect will happen due to bad conditions.
    /// </remarks>
    public static int ResentReliableCommands
    {
        get { return networkingPeer.ResentReliableCommands; }
    }

    /// <summary>Crc checks can be useful to detect and avoid issues with broken datagrams. Can be enabled while not connected.</summary>
    public static bool CrcCheckEnabled
    {
        get { return networkingPeer.CrcEnabled; }
        set
        {
            if (!connected && !connecting)
            {
                networkingPeer.CrcEnabled = value;
            }
            else
            {
                Debug.Log("Can't change CrcCheckEnabled while being connected. CrcCheckEnabled stays " + networkingPeer.CrcEnabled);
            }
        }
    }

    /// <summary>If CrcCheckEnabled, this counts the incoming packages that don't have a valid CRC checksum and got rejected.</summary>
    public static int PacketLossByCrcCheck
    {
        get { return networkingPeer.PacketLossByCrc; }
    }

    /// <summary>Defines the number of times a reliable message can be resent before not getting an ACK for it will trigger a disconnect. Default: 5.</summary>
    /// <remarks>Less resends mean quicker disconnects, while more can lead to much more lag without helping. Min: 3. Max: 10.</remarks>
    public static int MaxResendsBeforeDisconnect
    {
        get { return networkingPeer.SentCountAllowance; }
        set
        {
            if (value < 3) value = 3;
            if (value > 10) value = 10;
            networkingPeer.SentCountAllowance = value;
        }
    }

    /// <summary>In case of network loss, reliable messages can be repeated quickly up to 3 times.</summary>
    /// <remarks>
    /// When reliable messages get lost more than once, subsequent repeats are delayed a bit
    /// to allow the network to recover.<br/>
    /// With this option, the repeats 2 and 3 can be sped up. This can help avoid timeouts but
    /// also it increases the speed in which gaps are closed.<br/>
    /// When you set this, increase PhotonNetwork.MaxResendsBeforeDisconnect to 6 or 7.
    /// </remarks>
    public static int QuickResends
    {
        get { return networkingPeer.QuickResendAttempts; }
        set
        {
            if (value < 0) value = 0;
            if (value > 3) value = 3;
            networkingPeer.QuickResendAttempts = (byte)value;
        }
    }

    /// <summary>Switch to alternative ports for a UDP connection to the Public Cloud.</summary>
    /// <remarks>
    /// This should be used when a customer has issues with connection stability. Some players
    /// reported better connectivity for Steam games. The effect might vary, which is why the 
    /// alternative ports are not the new default.
    /// 
    /// The alternative (server) ports are 27000 up to 27003. 
    /// 
    /// The values are appplied by replacing any incoming server-address string accordingly.
    /// You only need to set this to true though.
    /// 
    /// This value does not affect TCP or WebSocket connections.
    /// </remarks>
    public static bool UseAlternativeUdpPorts { get; set; }


    /// <summary>
    /// Defines the delegate usable in OnEventCall.
    /// </summary>
    /// <remarks>Any eventCode &lt; 200 will be forwarded to your delegate(s).</remarks>
    /// <param name="eventCode">The code assigend to the incoming event.</param>
    /// <param name="content">The content the sender put into the event.</param>
    /// <param name="senderId">The ID of the player who sent the event. It might be 0, if the "room" sent the event.</param>
    public delegate void EventCallback(byte eventCode, object content, int senderId);

    /// <summary>Register your RaiseEvent handling methods here by using "+=".</summary>
    /// <remarks>Any eventCode &lt; 200 will be forwarded to your delegate(s).</remarks>
    /// <see cref="RaiseEvent"/>
    public static EventCallback OnEventCall;


    internal static int lastUsedViewSubId = 0;  // each player only needs to remember it's own (!) last used subId to speed up assignment
    internal static int lastUsedViewSubIdStatic = 0;  // per room, the master is able to instantiate GOs. the subId for this must be unique too
    internal static List<int> manuallyAllocatedViewIds = new List<int>();

    /// <summary>
    /// Static constructor used for basic setup.
    /// </summary>
    static PhotonNetwork()
    {
        #if UNITY_EDITOR
        if (PhotonServerSettings == null)
        {
            // create PhotonServerSettings
            CreateSettings();
        }

        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            //Debug.Log(string.Format("PhotonNetwork.ctor() Not playing {0} {1}", UnityEditor.EditorApplication.isPlaying, UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode));
            return;
        }

        // This can happen when you recompile a script IN play made
        // This helps to surpress some errors, but will not fix breaking
        PhotonHandler[] photonHandlers = GameObject.FindObjectsOfType(typeof(PhotonHandler)) as PhotonHandler[];
        if (photonHandlers != null && photonHandlers.Length > 0)
        {
            Debug.LogWarning("Unity recompiled. Connection gets closed and replaced. You can connect as 'new' client.");
            foreach (PhotonHandler photonHandler in photonHandlers)
            {
                //Debug.Log("Handler: " + photonHandler + " photonHandler.gameObject: " + photonHandler.gameObject);
                photonHandler.gameObject.hideFlags = 0;
                GameObject.DestroyImmediate(photonHandler.gameObject);
                Component.DestroyImmediate(photonHandler);
            }
        }
        #endif

		if (PhotonServerSettings != null)
		{
			Application.runInBackground = PhotonServerSettings.RunInBackground;
		}

        // Set up a MonoBehaviour to run Photon, and hide it
        GameObject photonGO = new GameObject();
        photonMono = (PhotonHandler)photonGO.AddComponent<PhotonHandler>();
        photonGO.name = "PhotonMono";
        photonGO.hideFlags = HideFlags.HideInHierarchy;


        // Set up the NetworkingPeer and use protocol of PhotonServerSettings
        ConnectionProtocol protocol = PhotonNetwork.PhotonServerSettings.Protocol;
        networkingPeer = new NetworkingPeer(string.Empty, protocol);
        networkingPeer.QuickResendAttempts = 2;
        networkingPeer.SentCountAllowance = 7;


        #if UNITY_XBOXONE
        networkingPeer.AuthMode = AuthModeOption.Auth;
        #endif

        if (UsePreciseTimer)
        {
            Debug.Log("Using Stopwatch as precision timer for PUN.");
            startupStopwatch = new Stopwatch();
            startupStopwatch.Start();
            networkingPeer.LocalMsTimestampDelegate = () => (int)startupStopwatch.ElapsedMilliseconds;
        }

        // Local player
        CustomTypes.Register();
    }

    /// <summary>
    /// While offline, the network protocol can be switched (which affects the ports you can use to connect).
    /// </summary>
    /// <remarks>
    /// When you switch the protocol, make sure to also switch the port for the master server. Default ports are:
    /// TCP: 4530
    /// UDP: 5055
    ///
    /// This could look like this:<br/>
    /// Connect(serverAddress, <udpport|tcpport>, appID, gameVersion)
    ///
    /// Or when you use ConnectUsingSettings(), the PORT in the settings can be switched like so:<br/>
    /// PhotonNetwork.PhotonServerSettings.ServerPort = 4530;
    ///
    /// The current protocol can be read this way:<br/>
    /// PhotonNetwork.networkingPeer.UsedProtocol
    ///
    /// This does not work with the native socket plugin of PUN+ on mobile!
    /// </remarks>
    /// <param name="cp">Network protocol to use as low level connection. UDP is default. TCP is not available on all platforms (see remarks).</param>
    public static void SwitchToProtocol(ConnectionProtocol cp)
    {
        // Debug.Log("SwitchToProtocol: " + cp + " PhotonNetwork.connected: " + PhotonNetwork.connected);
        networkingPeer.TransportProtocol = cp;
    }


    /// <summary>Connect to Photon as configured in the editor (saved in PhotonServerSettings file).</summary>
    /// <remarks>
    /// This method will disable offlineMode (which won't destroy any instantiated GOs) and it
    /// will set isMessageQueueRunning to true.
    ///
    /// Your server configuration is created by the PUN Wizard and contains the AppId and
    /// region for Photon Cloud games and the server address if you host Photon yourself.
    /// These settings usually don't change often.
    ///
    /// To ignore the config file and connect anywhere call: PhotonNetwork.ConnectToMaster.
    ///
    /// To connect to the Photon Cloud, a valid AppId must be in the settings file (shown in the Photon Cloud Dashboard).
    /// https://www.photonengine.com/dashboard
    ///
    /// Connecting to the Photon Cloud might fail due to:
    /// - Invalid AppId (calls: OnFailedToConnectToPhoton(). check exact AppId value)
    /// - Network issues (calls: OnFailedToConnectToPhoton())
    /// - Invalid region (calls: OnConnectionFail() with DisconnectCause.InvalidRegion)
    /// - Subscription CCU limit reached (calls: OnConnectionFail() with DisconnectCause.MaxCcuReached. also calls: OnPhotonMaxCccuReached())
    ///
    /// More about the connection limitations:
    /// http://doc.exitgames.com/en/pun
    /// </remarks>
    /// <param name="gameVersion">This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).</param>
    public static bool ConnectUsingSettings(string gameVersion)
    {
        if (networkingPeer.PeerState != PeerStateValue.Disconnected)
        {
            Debug.LogWarning("ConnectUsingSettings() failed. Can only connect while in state 'Disconnected'. Current state: " + networkingPeer.PeerState);
            return false;
        }
        if (PhotonServerSettings == null)
        {
            Debug.LogError("Can't connect: Loading settings failed. ServerSettings asset must be in any 'Resources' folder as: " + serverSettingsAssetFile);
            return false;
        }
        if (PhotonServerSettings.HostType == ServerSettings.HostingOption.NotSet)
        {
            Debug.LogError("You did not select a Hosting Type in your PhotonServerSettings. Please set it up or don't use ConnectUsingSettings().");
            return false;
        }

		// only apply Settings if logLevel is default ( see ServerSettings.cs), else it means it's been set programmatically
		if (PhotonNetwork.logLevel == PhotonLogLevel.ErrorsOnly)
		{
        	PhotonNetwork.logLevel = PhotonServerSettings.PunLogging;
		}

		// only apply Settings if logLevel is default ( see ServerSettings.cs), else it means it's been set programmatically
		if (PhotonNetwork.networkingPeer.DebugOut == DebugLevel.ERROR)
		{
        	PhotonNetwork.networkingPeer.DebugOut = PhotonServerSettings.NetworkLogging;
		}


        SwitchToProtocol(PhotonServerSettings.Protocol);
        networkingPeer.SetApp(PhotonServerSettings.AppID, gameVersion);

        if (PhotonServerSettings.HostType == ServerSettings.HostingOption.OfflineMode)
        {
            offlineMode = true;
            return true;
        }

        if (offlineMode)
        {
            // someone can set offlineMode in code and then call ConnectUsingSettings() with non-offline settings. Warning for that case:
            Debug.LogWarning("ConnectUsingSettings() disabled the offline mode. No longer offline.");
        }

        offlineMode = false; // Cleanup offline mode
        isMessageQueueRunning = true;
        networkingPeer.IsInitialConnect = true;

        if (PhotonServerSettings.HostType == ServerSettings.HostingOption.SelfHosted)
        {
            networkingPeer.IsUsingNameServer = false;
            networkingPeer.MasterServerAddress = (PhotonServerSettings.ServerPort == 0) ? PhotonServerSettings.ServerAddress : PhotonServerSettings.ServerAddress + ":" + PhotonServerSettings.ServerPort;

            return networkingPeer.Connect(networkingPeer.MasterServerAddress, ServerConnection.MasterServer);
        }

        if (PhotonServerSettings.HostType == ServerSettings.HostingOption.BestRegion)
        {
            return ConnectToBestCloudServer(gameVersion);
        }

        return networkingPeer.ConnectToRegionMaster(PhotonServerSettings.PreferredRegion);
    }

    /// <summary>Connect to a Photon Master Server by address, port, appID and game(client) version.</summary>
    /// <remarks>
    /// To connect to the Photon Cloud, a valid AppId must be in the settings file (shown in the Photon Cloud Dashboard).
    /// https://www.photonengine.com/dashboard
    ///
    /// Connecting to the Photon Cloud might fail due to:
    /// - Invalid AppId (calls: OnFailedToConnectToPhoton(). check exact AppId value)
    /// - Network issues (calls: OnFailedToConnectToPhoton())
    /// - Invalid region (calls: OnConnectionFail() with DisconnectCause.InvalidRegion)
    /// - Subscription CCU limit reached (calls: OnConnectionFail() with DisconnectCause.MaxCcuReached. also calls: OnPhotonMaxCccuReached())
    ///
    /// More about the connection limitations:
    /// http://doc.exitgames.com/en/pun
    /// </remarks>
    /// <param name="masterServerAddress">The server's address (either your own or Photon Cloud address).</param>
    /// <param name="port">The server's port to connect to.</param>
    /// <param name="appID">Your application ID (Photon Cloud provides you with a GUID for your game).</param>
    /// <param name="gameVersion">This client's version number. Users are separated by gameversion (which allows you to make breaking changes).</param>
    public static bool ConnectToMaster(string masterServerAddress, int port, string appID, string gameVersion)
    {
        if (networkingPeer.PeerState != PeerStateValue.Disconnected)
        {
            Debug.LogWarning("ConnectToMaster() failed. Can only connect while in state 'Disconnected'. Current state: " + networkingPeer.PeerState);
            return false;
        }

        if (offlineMode)
        {
            offlineMode = false; // Cleanup offline mode
            Debug.LogWarning("ConnectToMaster() disabled the offline mode. No longer offline.");
        }

        if (!isMessageQueueRunning)
        {
            isMessageQueueRunning = true;
            Debug.LogWarning("ConnectToMaster() enabled isMessageQueueRunning. Needs to be able to dispatch incoming messages.");
        }

        networkingPeer.SetApp(appID, gameVersion);
        networkingPeer.IsUsingNameServer = false;
        networkingPeer.IsInitialConnect = true;
        networkingPeer.MasterServerAddress = (port == 0) ? masterServerAddress : masterServerAddress + ":" + port;

        return networkingPeer.Connect(networkingPeer.MasterServerAddress, ServerConnection.MasterServer);
    }

	/// <summary>Can be used to reconnect to the master server after a disconnect.</summary>
	/// <remarks>
	/// After losing connection, you can use this to connect a client to the region Master Server again.
	/// Cache the room name you're in and use ReJoin(roomname) to return to a game.
	/// Common use case: Press the Lock Button on a iOS device and you get disconnected immediately.
	/// </remarks>
    public static bool Reconnect()
    {
        if (string.IsNullOrEmpty(networkingPeer.MasterServerAddress))
        {
            Debug.LogWarning("Reconnect() failed. It seems the client wasn't connected before?! Current state: " + networkingPeer.PeerState);
            return false;
        }

        if (networkingPeer.PeerState != PeerStateValue.Disconnected)
        {
            Debug.LogWarning("Reconnect() failed. Can only connect while in state 'Disconnected'. Current state: " + networkingPeer.PeerState);
            return false;
        }

        if (offlineMode)
        {
            offlineMode = false; // Cleanup offline mode
            Debug.LogWarning("Reconnect() disabled the offline mode. No longer offline.");
        }

        if (!isMessageQueueRunning)
        {
            isMessageQueueRunning = true;
            Debug.LogWarning("Reconnect() enabled isMessageQueueRunning. Needs to be able to dispatch incoming messages.");
        }

        networkingPeer.IsUsingNameServer = false;
        networkingPeer.IsInitialConnect = false;
        return networkingPeer.ReconnectToMaster();
    }

    /// <summary>
    /// Connect to the Photon Cloud region with the lowest ping (on platforms that support Unity's Ping).
    /// </summary>
    /// <remarks>
    /// Will save the result of pinging all cloud servers in PlayerPrefs. Calling this the first time can take +-2 seconds.
    /// The ping result can be overridden via PhotonNetwork.OverrideBestCloudServer(..)
    /// This call can take up to 2 seconds if it is the first time you are using this, all cloud servers will be pinged to check for the best region.
    ///
    /// The PUN Setup Wizard stores your appID in a settings file and applies a server address/port.
    /// To connect to the Photon Cloud, a valid AppId must be in the settings file (shown in the Photon Cloud Dashboard).
    /// https://www.photonengine.com/dashboard
    ///
    /// Connecting to the Photon Cloud might fail due to:
    /// - Invalid AppId (calls: OnFailedToConnectToPhoton(). check exact AppId value)
    /// - Network issues (calls: OnFailedToConnectToPhoton())
    /// - Invalid region (calls: OnConnectionFail() with DisconnectCause.InvalidRegion)
    /// - Subscription CCU limit reached (calls: OnConnectionFail() with DisconnectCause.MaxCcuReached. also calls: OnPhotonMaxCccuReached())
    ///
    /// More about the connection limitations:
    /// http://doc.exitgames.com/en/pun
    /// </remarks>
    /// <param name="gameVersion">This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).</param>
    /// <returns>If this client is going to connect to cloud server based on ping. Even if true, this does not guarantee a connection but the attempt is being made.</returns>
    public static bool ConnectToBestCloudServer(string gameVersion)
    {
        if (networkingPeer.PeerState != PeerStateValue.Disconnected)
        {
            Debug.LogWarning("ConnectToBestCloudServer() failed. Can only connect while in state 'Disconnected'. Current state: " + networkingPeer.PeerState);
            return false;
        }

        if (PhotonServerSettings == null)
        {
            Debug.LogError("Can't connect: Loading settings failed. ServerSettings asset must be in any 'Resources' folder as: " + PhotonNetwork.serverSettingsAssetFile);
            return false;
        }

        if (PhotonServerSettings.HostType == ServerSettings.HostingOption.OfflineMode)
        {
            return PhotonNetwork.ConnectUsingSettings(gameVersion);
        }

        networkingPeer.IsInitialConnect = true;
        networkingPeer.SetApp(PhotonServerSettings.AppID, gameVersion);

        CloudRegionCode bestFromPrefs = PhotonHandler.BestRegionCodeInPreferences;
        if (bestFromPrefs != CloudRegionCode.none)
        {
            Debug.Log("Best region found in PlayerPrefs. Connecting to: " + bestFromPrefs);
            return networkingPeer.ConnectToRegionMaster(bestFromPrefs);
        }

        bool couldConnect = PhotonNetwork.networkingPeer.ConnectToNameServer();
        return couldConnect;
    }


    /// <summary>
    /// Connects to the Photon Cloud region of choice.
    /// </summary>
    public static bool ConnectToRegion(CloudRegionCode region, string gameVersion)
    {
        if (networkingPeer.PeerState != PeerStateValue.Disconnected)
        {
            Debug.LogWarning("ConnectToRegion() failed. Can only connect while in state 'Disconnected'. Current state: " + networkingPeer.PeerState);
            return false;
        }

        if (PhotonServerSettings == null)
        {
            Debug.LogError("Can't connect: ServerSettings asset must be in any 'Resources' folder as: " + PhotonNetwork.serverSettingsAssetFile);
            return false;
        }

        if (PhotonServerSettings.HostType == ServerSettings.HostingOption.OfflineMode)
        {
            return PhotonNetwork.ConnectUsingSettings(gameVersion);
        }

        networkingPeer.IsInitialConnect = true;
        networkingPeer.SetApp(PhotonServerSettings.AppID, gameVersion);

        if (region != CloudRegionCode.none)
        {
            Debug.Log("ConnectToRegion: " + region);
            return networkingPeer.ConnectToRegionMaster(region);
        }

        return false;
    }

    /// <summary>Overwrites the region that is used for ConnectToBestCloudServer(string gameVersion).</summary>
    /// <remarks>
    /// This will overwrite the result of pinging all cloud servers.<br/>
    /// Use this to allow your users to save a manually selected region in the player preferences.<br/>
    /// Note: You can also use PhotonNetwork.ConnectToRegion to (temporarily) connect to a specific region.
    /// </remarks>
    public static void OverrideBestCloudServer(CloudRegionCode region)
    {
        PhotonHandler.BestRegionCodeInPreferences = region;
    }

    /// <summary>Pings all cloud servers again to find the one with best ping (currently).</summary>
    public static void RefreshCloudServerRating()
    {
        throw new NotImplementedException("not available at the moment");
    }


    /// <summary>
    /// Resets the traffic stats and re-enables them.
    /// </summary>
    public static void NetworkStatisticsReset()
    {
        networkingPeer.TrafficStatsReset();
    }


    /// <summary>
    /// Only available when NetworkStatisticsEnabled was used to gather some stats.
    /// </summary>
    /// <returns>A string with vital networking statistics.</returns>
    public static string NetworkStatisticsToString()
    {
        if (networkingPeer == null || offlineMode)
        {
            return "Offline or in OfflineMode. No VitalStats available.";
        }

        return networkingPeer.VitalStatsToString(false);
    }

    /// <summary>
    /// Used for compatibility with Unity networking only. Encryption is automatically initialized while connecting.
    /// </summary>
    [Obsolete("Used for compatibility with Unity networking only. Encryption is automatically initialized while connecting.")]
    public static void InitializeSecurity()
    {
        return;
    }

    /// <summary>
    /// Helper function which is called inside this class to erify if certain functions can be used (e.g. RPC when not connected)
    /// </summary>
    /// <returns></returns>
    private static bool VerifyCanUseNetwork()
    {
        if (connected)
        {
            return true;
        }

        Debug.LogError("Cannot send messages when not connected. Either connect to Photon OR use offline mode!");
        return false;
    }

    /// <summary>
    /// Makes this client disconnect from the photon server, a process that leaves any room and calls OnDisconnectedFromPhoton on completion.
    /// </summary>
    /// <remarks>
    /// When you disconnect, the client will send a "disconnecting" message to the server. This speeds up leave/disconnect
    /// messages for players in the same room as you (otherwise the server would timeout this client's connection).
    /// When used in offlineMode, the state-change and event-call OnDisconnectedFromPhoton are immediate.
    /// Offline mode is set to false as well.
    /// Once disconnected, the client can connect again. Use ConnectUsingSettings.
    /// </remarks>
    public static void Disconnect()
    {
        if (offlineMode)
        {
            offlineMode = false;
            networkingPeer.State = ClientState.Disconnecting;
            networkingPeer.OnStatusChanged(StatusCode.Disconnect);
            return;
        }

        if (networkingPeer == null)
        {
            return; // Surpress error when quitting playmode in the editor
        }

        networkingPeer.Disconnect();
    }

    /// <summary>To authenticate cookie between client and game server.</summary>
    public static bool AuthenticateCookie(string userid, string cookie, int serverid)
    {
        if (string.IsNullOrEmpty(cookie))
        {
            Debug.LogError("Cookie is null or empty. Check whether cookie pass in correctly.");
            return false;
        }
        Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
        opParameters.Add(ParameterCode.CookieId, cookie);
        opParameters.Add(ParameterCode.UniqueId, userid);
        return networkingPeer.OpAuthenticateCookie(opParameters);
    }

    // this is the method to connect to a single gameserver
    public static void ConnectGameServer(string gameserveraddr)
    {
        networkingPeer.OpConnectGameServer(gameserveraddr);
    }

    /// <summary>
    /// Sends fully customizable events in a room. Events consist of at least an EventCode (0..199) and can have content.
    /// </summary>
    /// <remarks>
    /// To receive the events someone sends, register your handling method in PhotonNetwork.OnEventCall.
    ///
    /// Example:
    /// private void OnEventHandler(byte eventCode, object content, int senderId)
    /// { Debug.Log("OnEventHandler"); }
    ///
    /// PhotonNetwork.OnEventCall += this.OnEventHandler;
    ///
    /// With the senderId, you can look up the PhotonPlayer who sent the event.
    /// It is best practice to assign a eventCode for each different type of content and action. You have to cast the content.
    ///
    /// The eventContent is optional. To be able to send something, it must be a "serializable type", something that
    /// the client can turn into a byte[] basically. Most basic types and arrays of them are supported, including
    /// Unity's Vector2, Vector3, Quaternion. Transforms or classes some project defines are NOT supported!
    /// You can make your own class a "serializable type" by following the example in CustomTypes.cs.
    ///
    ///
    /// The RaiseEventOptions have some (less intuitive) combination rules:
    /// If you set targetActors (an array of PhotonPlayer.ID values), the receivers parameter gets ignored.
    /// When using event caching, the targetActors, receivers and interestGroup can't be used. Buffered events go to all.
    /// When using cachingOption removeFromRoomCache, the eventCode and content are actually not sent but used as filter.
    /// </remarks>
    /// <param name="eventCode">A byte identifying the type of event. You might want to use a code per action or to signal which content can be expected. Allowed: 0..199.</param>
    /// <param name="eventContent">Some serializable object like string, byte, integer, float (etc) and arrays of those. Hashtables with byte keys are good to send variable content.</param>
    /// <param name="sendReliable">Makes sure this event reaches all players. It gets acknowledged, which requires bandwidth and it can't be skipped (might add lag in case of loss).</param>
    /// <param name="options">Allows more complex usage of events. If null, RaiseEventOptions.Default will be used (which is fine).</param>
    /// <returns>False if event could not be sent</returns>
    public static bool RaiseEvent(byte eventCode, object eventContent, bool sendReliable, RaiseEventOptions options)
    {
        if (!inRoom || eventCode >= 200)
        {
            Debug.LogWarning("RaiseEvent() failed. Your event is not being sent! Check if your are in a Room and the eventCode must be less than 200 (0..199).");
            return false;
        }

        return networkingPeer.OpRaiseEvent(eventCode, eventContent, sendReliable, options);
    }

    #if PHOTON_LIB_MIN_4_1_2
    public static bool RaiseEvent(byte eventCode, object eventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
    {
        if (!inRoom || eventCode >= 200)
        {
            Debug.LogWarning("RaiseEvent() failed. Your event is not being sent! Check if your are in a Room and the eventCode must be less than 200 (0..199).");
            return false;
        }

        return networkingPeer.OpRaiseEvent(eventCode, eventContent, raiseEventOptions, sendOptions);
    }
    #endif 

    /// <summary>
    /// The current roundtrip time to the photon server.
    /// </summary>
    /// <returns>Roundtrip time (to server and back).</returns>
    public static int GetPing()
    {
        return networkingPeer.RoundTripTime;
    }

    /// <summary>Refreshes the server timestamp (async operation, takes a roundtrip).</summary>
    /// <remarks>Can be useful if a bad connection made the timestamp unusable or imprecise.</remarks>
    public static void FetchServerTimestamp()
    {
        if (networkingPeer != null)
        {
            networkingPeer.FetchServerTimestamp();
        }
    }

    /// <summary>
    /// Can be used to immediately send the RPCs and Instantiates just called, so they are on their way to the other players.
    /// </summary>
    /// <remarks>
    /// This could be useful if you do a RPC to load a level and then load it yourself.
    /// While loading, no RPCs are sent to others, so this would delay the "load" RPC.
    /// You can send the RPC to "others", use this method, disable the message queue
    /// (by isMessageQueueRunning) and then load.
    /// </remarks>
    public static void SendOutgoingCommands()
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        while (networkingPeer.SendOutgoingCommands())
        {
        }
    }

    /// <summary>Finds the GameObjects with Components of a specific type (using FindObjectsOfType).</summary>
    /// <param name="type">Type must be a Component</param>
    /// <returns>HashSet with GameObjects that have a specific type of Component.</returns>
    public static HashSet<GameObject> FindGameObjectsWithComponent(Type type)
    {
        HashSet<GameObject> objectsWithComponent = new HashSet<GameObject>();

        Component[] targetComponents = (Component[]) GameObject.FindObjectsOfType(type);
        for (int index = 0; index < targetComponents.Length; index++)
        {
            if (targetComponents[index] != null)
            {
                objectsWithComponent.Add(targetComponents[index].gameObject);
            }
        }

        return objectsWithComponent;
    }

    /// <summary>Wraps loading a level to pause the network mesage-queue. Optionally syncs the loaded level in a room.</summary>
    /// <remarks>
    /// While loading levels, it makes sense to not dispatch messages received by other players.
    /// This method takes care of that by setting PhotonNetwork.isMessageQueueRunning = false and enabling
    /// the queue when the level was loaded.
    ///
    /// To sync the loaded level in a room, set PhotonNetwork.automaticallySyncScene to true.
    /// The Master Client of a room will then sync the loaded level with every other player in the room.
    ///
    /// You should make sure you don't fire RPCs before you load another scene (which doesn't contain
    /// the same GameObjects and PhotonViews). You can call this in OnJoinedRoom.
    ///
    /// This uses Application.LoadLevel.
    /// </remarks>
    /// <param name='levelName'>
    /// Name of the level to load. Make sure it's available to all clients in the same room.
    /// </param>
    public static void LoadLevel(string levelName)
    {
        PhotonNetwork.isMessageQueueRunning = false;
        networkingPeer.loadingLevelAndPausedNetwork = true;
        SceneLoader.Instance.LoadLevel(levelName);
    }


    /// <summary>
    /// This operation makes Photon call your custom web-service by name (path) with the given parameters.
    /// </summary>
    /// <remarks>
    /// This is a server-side feature which must be setup in the Photon Cloud Dashboard prior to use.<br/>
    /// See the Turnbased Feature Overview for a short intro.<br/>
    /// http://doc.photonengine.com/en/turnbased/current/getting-started/feature-overview
    ///<br/>
    /// The Parameters will be converted into JSon format, so make sure your parameters are compatible.
    ///
    /// See PhotonNetworkingMessage.OnWebRpcResponse on how to get a response.
    ///
    /// It's important to understand that the OperationResponse only tells if the WebRPC could be called.
    /// The content of the response contains any values your web-service sent and the error/success code.
    /// In case the web-service failed, an error code and a debug message are usually inside the
    /// OperationResponse.
    ///
    /// The class WebRpcResponse is a helper-class that extracts the most valuable content from the WebRPC
    /// response.
    /// </remarks>
    /// <example>
    /// Example callback implementation:<pre>
    ///
    /// public void OnWebRpcResponse(OperationResponse response)
    /// {
    ///     WebRpcResponse webResponse = new WebRpcResponse(operationResponse);
    ///     if (webResponse.ReturnCode != 0) { //...
    ///     }
    ///
    ///     switch (webResponse.Name) { //...
    ///     }
    ///     // and so on
    /// }</pre>
    /// </example>
    public static bool WebRpc(string name, object parameters)
    {
        return networkingPeer.WebRpc(name, parameters);
    }

    public static void SetNetworkState(ClientState state)
    {
        networkingPeer.State = state;
    }

#if UNITY_EDITOR


    /// <summary>
    /// Finds the asset path base on its name or search query: https://docs.unity3d.com/ScriptReference/AssetDatabase.FindAssets.html
    /// </summary>
    /// <returns>The asset path.</returns>
    /// <param name="asset">Asset.</param>
    public static string FindAssetPath(string asset)
	{
		string[] guids = AssetDatabase.FindAssets (asset, null);
		if (guids.Length != 1)
		{
			return string.Empty;
		} else
		{
			return AssetDatabase.GUIDToAssetPath (guids [0]);
		}
	}


	/// <summary>
	/// Finds the pun asset folder. Something like Assets/Photon Unity Networking/Resources/
	/// </summary>
	/// <returns>The pun asset folder.</returns>
	public static string FindPunAssetFolder()
	{
		string _thisPath =	FindAssetPath("PhotonClasses");
		string _PunFolderPath = string.Empty;

		_PunFolderPath = GetParent(_thisPath,"Photon Unity Networking");

		if (_PunFolderPath != null)
		{
			return "Assets" + _PunFolderPath.Substring(Application.dataPath.Length)+"/";
		}

		return "Assets/Photon Unity Networking/";
	}

	/// <summary>
	/// Gets the parent directory of a path. Recursive Function, will return null if parentName not found
	/// </summary>
	/// <returns>The parent directory</returns>
	/// <param name="path">Path.</param>
	/// <param name="parentName">Parent name.</param>
	public static string GetParent(string path, string parentName)
	{
		var dir = new DirectoryInfo(path);

		if (dir.Parent == null)
		{
			return null;
		}

		if (string.IsNullOrEmpty(parentName))
		{
			return  dir.Parent.FullName;
		}

		if (dir.Parent.Name == parentName)
		{
			return dir.Parent.FullName;
		}

		return GetParent(dir.Parent.FullName, parentName);
	}


    [Conditional("UNITY_EDITOR")]
    public static void CreateSettings()
    {
        PhotonNetwork.PhotonServerSettings = (ServerSettings)Resources.Load(PhotonNetwork.serverSettingsAssetFile, typeof(ServerSettings));
        if (PhotonNetwork.PhotonServerSettings != null)
        {
            return;
        }

        // find out if ServerSettings can be instantiated (existing script check)
        ScriptableObject serverSettingTest = ScriptableObject.CreateInstance("ServerSettings");
        if (serverSettingTest == null)
        {
            Debug.LogError("missing settings script");
            return;
        }
        UnityEngine.Object.DestroyImmediate(serverSettingTest);


        // if still not loaded, create one
        if (PhotonNetwork.PhotonServerSettings == null)
        {
			string _PunResourcesPath = PhotonNetwork.FindPunAssetFolder();

			_PunResourcesPath += "Resources/";


			string serverSettingsAssetPath = _PunResourcesPath+ PhotonNetwork.serverSettingsAssetFile + ".asset";
			string settingsPath = Path.GetDirectoryName(serverSettingsAssetPath);
            if (!Directory.Exists(settingsPath))
            {
                Directory.CreateDirectory(settingsPath);
                AssetDatabase.ImportAsset(settingsPath);
            }

            PhotonNetwork.PhotonServerSettings = (ServerSettings)ScriptableObject.CreateInstance("ServerSettings");
            if (PhotonNetwork.PhotonServerSettings != null)
            {
				AssetDatabase.CreateAsset(PhotonNetwork.PhotonServerSettings, serverSettingsAssetPath);
            }
            else
            {
                Debug.LogError("PUN failed creating a settings file. ScriptableObject.CreateInstance(\"ServerSettings\") returned null. Will try again later.");
            }
        }
    }


    /// <summary>
    /// Internally used by Editor scripts, called on Hierarchy change (includes scene save) to remove surplus hidden PhotonHandlers.
    /// </summary>
    public static void InternalCleanPhotonMonoFromSceneIfStuck()
    {
        PhotonHandler[] photonHandlers = GameObject.FindObjectsOfType(typeof(PhotonHandler)) as PhotonHandler[];
        if (photonHandlers != null && photonHandlers.Length > 0)
        {
            Debug.Log("Cleaning up hidden PhotonHandler instances in scene. Please save it. This is not an issue.");
            foreach (PhotonHandler photonHandler in photonHandlers)
            {
                // Debug.Log("Removing Handler: " + photonHandler + " photonHandler.gameObject: " + photonHandler.gameObject);
                photonHandler.gameObject.hideFlags = 0;

                if (photonHandler.gameObject != null && photonHandler.gameObject.name == "PhotonMono")
                {
                    GameObject.DestroyImmediate(photonHandler.gameObject);
                }

                Component.DestroyImmediate(photonHandler);
            }
        }
    }
#endif

}
