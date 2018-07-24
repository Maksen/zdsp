using Photon.LoadBalancing.GameServer;

namespace Photon.LoadBalancing.ServerToServer
{
    public class ClusterServerConnection : ServerConnectionBase
    {
        public ClusterServerConnection(GameApplication controller, string address, int port, int connectRetryIntervalSeconds, string ApplicationId)
            : base(controller, address, port, connectRetryIntervalSeconds, ApplicationId)
        {
        }

        public override OutgoingServerPeer CreateServerPeer()
        {
            OutgoingClusterServerPeer peer = new OutgoingClusterServerPeer(this);
            ((GameApplication)Application).clusterPeer = peer;
            return peer;
        }
    }
}
