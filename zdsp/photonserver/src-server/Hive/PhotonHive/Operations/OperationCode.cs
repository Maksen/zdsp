// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationCode.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Codes of operations (defining their type, parameters incoming from clients and return values).
//   These codes match events (in parts).
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.Hive.Operations
{
    /// <summary>
    ///   Defines the operation codes used by the Lite application.
    ///   These codes match events (in parts).
    /// </summary>
    public enum OperationCode : byte
    {
        /// <summary>
        ///   The operation code for the <see cref="JoinGameRequest">join</see> operation.
        /// </summary>
        Join = 255, 

        /// <summary>
        ///   Operation code for the <see cref="LeaveRequest">leave</see> operation.
        /// </summary>
        Leave = 254, 

        /// <summary>
        ///   Operation code for the <see cref="RaiseEventRequest">raise event</see> operation.
        /// </summary>
        RaiseEvent = 253, 

        /// <summary>
        ///   Operation code for the <see cref="SetPropertiesRequest">set properties</see> operation.
        /// </summary>
        SetProperties = 252, 

        /// <summary>
        ///   Operation code for the <see cref="GetPropertiesRequest">get properties</see> operation.
        /// </summary>
        GetProperties = 251, 
        
        /// <summary>
        ///   Operation code for the ping operation.
        /// </summary>
        Ping = 249,

        /// <summary>
        ///   Operation code for the <see cref="ChangeGroups" /> operation.
        /// </summary>
        ChangeGroups = 248,

        // operation codes in load the balancing project
        ConnectGameSetup = 235,
        VerifyLoginId = 234,
        AuthenticateCookie = 233,
        GetServerList = 232,
        Register = 231,
        Authenticate = 230,
        JoinLobby = 229,
        LeaveLobby = 228,
        CreateGame = 227,
        JoinGame = 226,
        JoinRandomGame = 225,

        CheckMail = 224,
        // CancelJoinRandomGame = 224, currently not used 
        DebugGame = 223,
        FindFriends = 222,
        LobbyStats = 221,

        // Rpc call to external server
        Rpc = 219,
     
        BanThisPlayer = 216,
        QATools = 99,

        Combat = 1,
        //Inventory = 2,
        //Quest = 3,
        //Social = 4,
        //Guild = 5,
        Action = 6,
        Lobby = 7,
        LocalObject = 8,
        UnreliableCombat = 9,
        NonCombat = 10,

        GMToMasterRPC = 29,
        MasterToGame = 30,
        GameToMaster = 31,
        ClusterToGame = 32,
        GameToCluster = 33,
        ClusterToMaster = 34,
        MasterToCluster = 35,
    }
}