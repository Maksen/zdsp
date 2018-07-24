using Photon.LoadBalancing.ClusterServer;

namespace Photon.LoadBalancing.ServerToServer
{
    public class MasterServerConnection : ServerConnectionBase
    {
        public MasterServerConnection(ClusterApplication controller, string address, int port, int connectRetryIntervalSeconds, string ApplicationId)
            : base(controller, address, port, connectRetryIntervalSeconds, ApplicationId)
        {
        }

        public override OutgoingServerPeer CreateServerPeer()
        {
            OutgoingMasterServerPeer peer = new OutgoingMasterServerPeer(this);
            ((ClusterApplication)Application).masterPeer = peer;
            return peer;
        }
    }
}
