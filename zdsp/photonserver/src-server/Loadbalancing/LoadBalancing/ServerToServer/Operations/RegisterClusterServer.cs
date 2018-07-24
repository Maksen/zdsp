// --------------------------------------------------------------------------------------------------------------------
// chensheng 2018/03/19
// --------------------------------------------------------------------------------------------------------------------
namespace Photon.LoadBalancing.ServerToServer.Operations
{
    #region using directives
    using Photon.SocketServer;
    using Photon.SocketServer.Rpc;
    #endregion
    /// <summary>
    ///   Defines the parameters which should be send from game server instances to 
    ///   register at the master application.
    /// </summary>
    public class RegisterClusterServer : Operation
    {
        #region Constructors and Destructors
        /// <summary>
        ///   Initializes a new instance of the <see cref = "RegisterClusterServer" /> class.
        /// </summary>
        /// <param name = "rpcProtocol">
        ///   The rpc Protocol.
        /// </param>
        /// <param name = "operationRequest">
        ///   The operation request.
        /// </param>
        public RegisterClusterServer(IRpcProtocol rpcProtocol, OperationRequest operationRequest)
            : base(rpcProtocol, operationRequest)
        {
        }

        public RegisterClusterServer()
        {
        }
        #endregion
        #region Properties
        [DataMember(Code = 1, IsOptional = false)]
        public string GameServerAddress { get; set; }

        [DataMember(Code = 2, IsOptional = false)]
        public int TcpPort { get; set; }
        #endregion
    }
}