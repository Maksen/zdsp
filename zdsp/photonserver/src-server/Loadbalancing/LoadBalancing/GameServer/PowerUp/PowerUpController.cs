using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
