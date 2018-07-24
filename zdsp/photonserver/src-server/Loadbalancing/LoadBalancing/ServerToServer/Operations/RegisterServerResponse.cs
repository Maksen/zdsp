// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegisterGameServerResponse.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the RegisterServerResponse type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Photon.LoadBalancing.ServerToServer.Operations
{
    using System.Collections;
    using Photon.SocketServer;
    using Photon.SocketServer.Rpc;
    public class RegisterServerResponse : DataContract
    {
        #region Constructors and Destructors
        public RegisterServerResponse(IRpcProtocol protocol, OperationResponse response)
            : base(protocol, response.Parameters)
        {
        }
        public RegisterServerResponse()
        {
        }
        #endregion
        #region Properties
        //[DataMember(Code = 4, IsOptional = true)]
        //public Hashtable AuthList { get; set; }
        //[DataMember(Code = 1, IsOptional = true)]
        //public byte[] ExternalAddress { get; set; }
        //[DataMember(Code = 2)]
        //public byte[] InternalAddress { get; set; }
        //[DataMember(Code = 5, IsOptional = true)]
        //public byte[] SharedKey { get; set; }
        #endregion
    }
}