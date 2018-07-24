using Photon.LoadBalancing.Operations;
using Zealot.Common.RPC;
using Photon.LoadBalancing.ClusterServer;

namespace Zealot.RPC
{
    public class MasterToClusterRPC : ServerRPCBase
    {
        public MasterToClusterRPC() :
            base(typeof(MasterToClusterRPC), (byte)OperationCode.MasterToCluster, true)
        {
            SetMainContext(typeof(ClusterApplication), RPCCategory.MasterToCluster);
        }
    }
}