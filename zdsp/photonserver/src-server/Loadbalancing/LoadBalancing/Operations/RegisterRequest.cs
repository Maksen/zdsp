// --------------------------------------------------------------------------------------------------------------------
// chensheng 9/4/2018
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.LoadBalancing.Operations
{
    #region using directives
    using Photon.SocketServer;
    using Photon.SocketServer.Rpc;
    #endregion

    public class RegisterRequest : Operation
    {
        #region Constructors and Destructors

        public RegisterRequest(IRpcProtocol protocol, OperationRequest operationRequest)
            : base(protocol, operationRequest)
        {
        }

        #endregion
        #region Serialized Properties
        [DataMember(Code = (byte)ParameterCode.LoginType, IsOptional = false)]
        public string LoginType { get; set; }

        [DataMember(Code = (byte)ParameterCode.LoginId, IsOptional = false)]
        public string LoginId { get; set; }

        [DataMember(Code = (byte)ParameterCode.DeviceId, IsOptional = false)]
        public string DeviceId { get; set; }
        #endregion
    }
}