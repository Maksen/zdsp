using Zealot.Common;

namespace Photon.LoadBalancing.GameServer
{
    public class PowerUpController
    {
        private GameClientPeer _peer;

        public PowerUpController(GameClientPeer peer)
        {
            _peer = peer;
        }

        public int GetExp()
        {
            return PowerUpInventoryData.EXP_GIVE;
        }
    }
}
