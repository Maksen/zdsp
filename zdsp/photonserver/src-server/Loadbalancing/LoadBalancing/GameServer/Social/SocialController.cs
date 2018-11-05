using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photon.LoadBalancing.GameServer
{
    public class SocialController
    {
        private GameClientPeer m_Peer;
        public SocialController(GameClientPeer peer)
        {
            this.m_Peer = peer;
        }
    }
}
