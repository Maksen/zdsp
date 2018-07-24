// --------------------------------------------------------------------------------------------------------------------
// chensheng 2018/04/18
// GameServer to MasterServer
// --------------------------------------------------------------------------------------------------------------------
using Photon.LoadBalancing.GameServer;

namespace Photon.LoadBalancing.ServerToServer
{
    public class GameToMasterConnection : ServerConnectionBase
    {
        public GameToMasterConnection(GameApplication controller, string address, int port, int connectRetryIntervalSeconds, string ApplicationId)
            : base(controller, address, port, connectRetryIntervalSeconds, ApplicationId)
        {
        }

        public override OutgoingServerPeer CreateServerPeer()
        {
            OutgoingGameToMasterPeer peer = new OutgoingGameToMasterPeer(this);
            ((GameApplication)Application).masterPeer = peer;
            return peer;
        }
    }
}
