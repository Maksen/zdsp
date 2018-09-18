using System;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Photon.LoadBalancing.GameServer
{
    public class EquipFushionController
    {
        private GameClientPeer _peer;

        public EquipFushionController(GameClientPeer peer)
        {
            _peer = peer;
        }
    }
}
