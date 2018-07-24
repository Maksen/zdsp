// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationCode.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the OperationCode type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Photon.LoadBalancing.ServerToServer.Operations
{
    public class OperationCode
    {
        public const byte RegisterGameServer = 1;
        public const byte ResponseToGameManager = 2;
        public const byte RegisterClusterServer = 3;
        public const byte GMRPC = 21;
    }

    //public enum OperationCode : short
    //{
    //    RegisterGameServer = 1,         
    //}
}