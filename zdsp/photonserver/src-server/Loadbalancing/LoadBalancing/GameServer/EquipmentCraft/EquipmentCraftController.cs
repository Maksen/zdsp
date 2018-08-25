using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photon.LoadBalancing.GameServer
{
    public class EquipmentCraftController
    {
        private GameClientPeer _peer;

        public EquipmentCraftController (GameClientPeer peer)
        {
            _peer = peer;
        }
    }
}
