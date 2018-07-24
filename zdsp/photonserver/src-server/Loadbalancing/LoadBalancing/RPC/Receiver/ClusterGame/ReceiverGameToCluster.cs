using Photon.LoadBalancing.ClusterServer.GameServer;
using Zealot.Common.RPC;

namespace Photon.LoadBalancing.ClusterServer
{
    public partial class ClusterServer
    {
        [RPCMethod(RPCCategory.GameToCluster, (byte)GameToClusterRPCMethods.RegChar)]
        public void RegChar(string charname, string userid, int level, IncomingGameServerPeer peer)
        {
            CharDetails charDetail;
            if (CharMap.TryGetValue(charname, out charDetail))
            {
                charDetail.peer = peer;
            }
            else
            {
                CharacterSyncData charSyncData = new CharacterSyncData { lvl = level, server = peer.Serverconfig.servername };
                charDetail = new CharDetails { peer = peer, data = charSyncData };
                CharMap.Add(charname, charDetail);
            }
            var broadcast_to_peers = GetBroadcastPeers(peer.Serverconfig.serverline, peer);
            if (broadcast_to_peers.Count > 0)
                peer.ZRPC.ClusterToGameRPC.RegChar(charname, level, peer.Serverconfig.servername, broadcast_to_peers);
        }

        [RPCMethod(RPCCategory.GameToCluster, (byte)GameToClusterRPCMethods.UnRegChar)]
        public void UnRegChar(string charname, string userid, IncomingGameServerPeer peer)
        {
            if (CharMap.Remove(charname))
            {
                var broadcast_to_peers = GetBroadcastPeers(peer.Serverconfig.serverline, peer);
                if (broadcast_to_peers.Count > 0)
                    peer.ZRPC.ClusterToGameRPC.UnRegChar(charname, broadcast_to_peers);
            }
        }
    }
}