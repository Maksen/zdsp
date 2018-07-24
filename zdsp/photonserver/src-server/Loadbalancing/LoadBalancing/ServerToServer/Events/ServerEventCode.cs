// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServerEventCode.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the ServerEventCode type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Photon.LoadBalancing.ServerToServer.Events
{
    public class ServerEventCode
    {
        public const byte InitInstance = 0;
        public const byte UpdateServer = 1;
        public const byte UpdateAppStats = 2;
        public const byte UpdateGameState = 3;
        public const byte RemoveGameState = 4;
        public const byte AuthenticateUpdate = 10;
        public const byte GMRPC = 21;
        public const byte MasterToGame = 30;
        public const byte GameToMaster = 31;
        public const byte ClusterToGame = 32;
        public const byte GameToCluster = 33;
        public const byte ClusterToMaster = 34;
        public const byte MasterToCluster = 35;
        public const byte GameManagerToGame = 40;
    }

    //public enum ServerEventCode
    //{
    //    ////InitInstance = 0, 
    //    UpdateServer = 1, 
    //    UpdateAppStats = 2, 
    //    UpdateGameState = 3, 
    //    RemoveGameState = 4, 
    //    AuthenticateUpdate = 10,
    //    MasterToGame = 30,
    //    GameManagerToGame = 40
    //}
}