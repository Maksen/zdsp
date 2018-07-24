using Photon.LoadBalancing.Operations;
using Zealot.Common.RPC;
using Photon.LoadBalancing.ClusterServer;

namespace Zealot.RPC
{
    public class GameToClusterRPC : ServerRPCBase
    {
        public GameToClusterRPC() :
            base(typeof(GameToClusterRPC), (byte)OperationCode.GameToCluster, true)
        {
            SetMainContext(typeof(ClusterServer), RPCCategory.GameToCluster);
        }

        [RPCMethod(RPCCategory.GameToCluster, (byte)GameToClusterRPCMethods.RegChar)]
        public void RegChar(string charName, string userid, int level, object target)
        {
            ProxyMethod("RegChar", charName, userid, level, target);
        }

        [RPCMethod(RPCCategory.GameToCluster, (byte)GameToClusterRPCMethods.UnRegChar)]
        public void UnRegChar(string charName, string userid, object target)
        {
            ProxyMethod("UnRegChar", charName, userid, target);
        }
    }
}