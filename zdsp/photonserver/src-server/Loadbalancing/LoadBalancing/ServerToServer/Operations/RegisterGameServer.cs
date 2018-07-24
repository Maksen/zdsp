// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegisterGameServer.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the parameters which should be send from game server instances to
//   register at the master application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Photon.LoadBalancing.ServerToServer.Operations
{
    #region using directives
    using Photon.SocketServer;
    using Photon.SocketServer.Rpc;
    #endregion
    /// <summary>
    ///   Defines the parameters which should be send from game server instances to 
    ///   register at the cluster application.
    /// </summary>
    public class RegisterGameServer : Operation
    {
        #region Constructors and Destructors
        public RegisterGameServer(IRpcProtocol rpcProtocol, OperationRequest operationRequest)
            : base(rpcProtocol, operationRequest)
        {
        }

        public RegisterGameServer()
        {
        }
        #endregion
        #region Properties
        [DataMember(Code = 1, IsOptional = false)]
        public string GameServerAddress { get; set; }

        [DataMember(Code = 2, IsOptional = true)]
        public int? TcpPort { get; set; }

        [DataMember(Code = 3, IsOptional = true)]
        public int? UdpPort { get; set; }

        [DataMember(Code = 4, IsOptional = true)]
        public int? WebSocketPort { get; set; }

        [DataMember(Code = 5, IsOptional = true)]
        public string ServerConfig { get; set; }

        [DataMember(Code = 6, IsOptional = true)]
        public string[] OnlineUsers { get; set; }

        [DataMember(Code = 7, IsOptional = true)]
        public string OnlineChars { get; set; } //serialized
        #endregion
    }
}