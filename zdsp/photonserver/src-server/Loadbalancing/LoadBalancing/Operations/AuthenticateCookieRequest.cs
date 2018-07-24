// --------------------------------------------------------------------------------------------------------------------
// chensheng 9/4/2018
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.LoadBalancing.Operations
{
    #region using directives
    using Photon.SocketServer;
    using Photon.SocketServer.Rpc;
    #endregion

    public class AuthenticateCookieRequest : Operation
    {
        #region Constructors and Destructors

        public AuthenticateCookieRequest(IRpcProtocol protocol, OperationRequest operationRequest)
            : base(protocol, operationRequest)
        {
        }

        #endregion
        #region Serialized Properties
        [DataMember(Code = (byte)ParameterCode.CookieId, IsOptional = false)]
        public string Cookie { get; set; }

        [DataMember(Code = (byte)ParameterCode.UniqueId, IsOptional = false)]
        public string UserId { get; set; }
        #endregion
    }
}