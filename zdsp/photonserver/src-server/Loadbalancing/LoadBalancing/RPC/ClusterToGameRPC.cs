using Photon.LoadBalancing.Operations;
using Zealot.Common.RPC;
using Photon.LoadBalancing.GameServer;

namespace Zealot.RPC
{
    public class ClusterToGameRPC : ServerRPCBase
    {
        public ClusterToGameRPC() :
            base(typeof(ClusterToGameRPC), (byte)OperationCode.ClusterToGame, true)
        {
            SetMainContext(typeof(GameApplication), RPCCategory.ClusterToGame);
        }

        [RPCMethod(RPCCategory.ClusterToGame, (byte)ClusterToGameRPCMethods.RegChar)]
        public void RegChar(string charName, int level, string servername, object target)
        {
            ProxyMethod("RegChar", charName, level, servername, target);
        }

        [RPCMethod(RPCCategory.ClusterToGame, (byte)ClusterToGameRPCMethods.UnRegChar)]
        public void UnRegChar(string charName, object target)
        {
            ProxyMethod("UnRegChar", charName, target);
        }

        [RPCMethod(RPCCategory.ClusterToGame, (byte)ClusterToGameRPCMethods.RegCharMultiple)]
        public void RegCharMultiple(string onlineChars, string servername, object target)
        {
            ProxyMethod("RegCharMultiple", onlineChars, servername, target);
        }

        [RPCMethod(RPCCategory.ClusterToGame, (byte)ClusterToGameRPCMethods.OnConnectedSyncData)]
        public void OnConnectedSyncData(string data, object target)
        {
            ProxyMethod("OnConnectedSyncData", data, target);
        }     
    }
}