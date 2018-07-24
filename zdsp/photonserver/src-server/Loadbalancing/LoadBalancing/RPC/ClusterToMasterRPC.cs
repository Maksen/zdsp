using Photon.LoadBalancing.MasterServer;
using Photon.LoadBalancing.Operations;
using Zealot.Common.RPC;

namespace Zealot.RPC
{
    public class ClusterToMasterRPC : ServerRPCBase
    {
        public ClusterToMasterRPC() :
            base(typeof(ClusterToMasterRPC), (byte)OperationCode.ClusterToMaster, true)
        {
            SetMainContext(typeof(MasterCluster), RPCCategory.ClusterToMaster);
        }
    }
}