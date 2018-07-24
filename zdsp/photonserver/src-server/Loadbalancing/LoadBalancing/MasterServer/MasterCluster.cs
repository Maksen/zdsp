using Photon.LoadBalancing.MasterServer.Cluster;
using System.Collections.Generic;

namespace Photon.LoadBalancing.MasterServer
{
    public partial class MasterCluster
    {
        public Dictionary<string, IncomingClusterServerPeer> ClusterServers; //IncomingClusterServerPeer.key <- IncomingClusterServerPeer

        public MasterCluster()
        {
            ClusterServers = new Dictionary<string, IncomingClusterServerPeer>();
        }

        public void OnConnect(IncomingClusterServerPeer peer)
        {
            string key = peer.Key;
            IncomingClusterServerPeer oldPeer;
            if (ClusterServers.TryGetValue(key, out oldPeer))
            {
                if (oldPeer != peer)
                {
                    oldPeer.Disconnect();
                    oldPeer.Dispose();
                }
            }
            ClusterServers[key] = peer;       
        }

        public void OnDisconnect(IncomingClusterServerPeer peer)
        {
            string key = peer.Key;
            ClusterServers.Remove(key);
        }
    }
}