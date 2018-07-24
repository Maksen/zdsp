// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationCode.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the OperationCode type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.LoadBalancing.Operations
{
    public enum OperationCode : byte
    {
        // operation codes inherited from lite
        //Join = 255 (Join is not used the in load balancing project)
        Leave = Photon.Hive.Operations.OperationCode.Leave,
        RaiseEvent = Photon.Hive.Operations.OperationCode.RaiseEvent,
        SetProperties = Photon.Hive.Operations.OperationCode.SetProperties,
        GetProperties = Photon.Hive.Operations.OperationCode.GetProperties,
        Ping = Photon.Hive.Operations.OperationCode.Ping,
        ChangeGroups = Photon.Hive.Operations.OperationCode.ChangeGroups,

        // operation codes in load the balancing project
        Authenticate = Photon.Hive.Operations.OperationCode.Authenticate, 
        JoinLobby = Photon.Hive.Operations.OperationCode.JoinLobby, 
        LeaveLobby = Photon.Hive.Operations.OperationCode.LeaveLobby, 
        CreateGame = Photon.Hive.Operations.OperationCode.CreateGame, 
        JoinGame = Photon.Hive.Operations.OperationCode.JoinGame, 
        JoinRandomGame = Photon.Hive.Operations.OperationCode.JoinRandomGame, 
        // CancelJoinRandomGame = 224, currently not used 
        DebugGame = Photon.Hive.Operations.OperationCode.DebugGame,
        FindFriends = Photon.Hive.Operations.OperationCode.FindFriends,
        LobbyStats = Photon.Hive.Operations.OperationCode.LobbyStats,
        Rpc = Photon.Hive.Operations.OperationCode.Rpc,

        // zealot used
        ConnectGameSetup = Photon.Hive.Operations.OperationCode.ConnectGameSetup,
        VerifyLoginId = Photon.Hive.Operations.OperationCode.VerifyLoginId,
        AuthenticateCookie = Photon.Hive.Operations.OperationCode.AuthenticateCookie,
        GetServerList = Photon.Hive.Operations.OperationCode.GetServerList,
        Register = Photon.Hive.Operations.OperationCode.Register,
        CheckMail = Photon.Hive.Operations.OperationCode.CheckMail,
        BanThisPlayer = Photon.Hive.Operations.OperationCode.BanThisPlayer,
        QATools = Photon.Hive.Operations.OperationCode.QATools,

        Combat = Photon.Hive.Operations.OperationCode.Combat,
        //Inventory = 2,
        //Quest = 3,
        //Social = 4,
        //Guild = 5,
        Action = Photon.Hive.Operations.OperationCode.Action,
        Lobby = Photon.Hive.Operations.OperationCode.Lobby,
        LocalObject = Photon.Hive.Operations.OperationCode.LocalObject,
        UnreliableCombat = Photon.Hive.Operations.OperationCode.UnreliableCombat,
        NonCombat = Photon.Hive.Operations.OperationCode.NonCombat,

        GMToMasterRPC = Photon.Hive.Operations.OperationCode.GMToMasterRPC,
        MasterToGame = Photon.Hive.Operations.OperationCode.MasterToGame,
        GameToMaster = Photon.Hive.Operations.OperationCode.GameToMaster,
        ClusterToGame = Photon.Hive.Operations.OperationCode.ClusterToGame,
        GameToCluster = Photon.Hive.Operations.OperationCode.GameToCluster,
        ClusterToMaster = Photon.Hive.Operations.OperationCode.ClusterToMaster,
        MasterToCluster = Photon.Hive.Operations.OperationCode.MasterToCluster,
    }
}