using ExitGames.Logging;
using Photon.LoadBalancing.ClusterServer.GameServer;
using Photon.LoadBalancing.ServerToServer;
using Photon.LoadBalancing.ServerToServer.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zealot.Common;

namespace Photon.LoadBalancing.ClusterServer
{
    public class CharDetails
    {
        public IncomingGameServerPeer peer;
        public CharacterSyncData data;
    }

    public partial class ClusterServer
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public ClusterApplication Application;
        public Dictionary<int, Dictionary<int, IncomingGameServerPeer>> GameServersByLine; //serverline <- serverid <- IncomingGameServerPeer
        public Dictionary<int, IncomingGameServerPeer> GameServersByServerId;
        public Dictionary<int, DateTime> DCGameServers; //serverid <- dc datetime.
        public Dictionary<string, CharDetails> CharMap;//charname <- CharDetails

        public ClusterServer(ClusterApplication application)
        {
            Application = application;
            GameServersByLine = new Dictionary<int, Dictionary<int, IncomingGameServerPeer>>();
            GameServersByServerId = new Dictionary<int, IncomingGameServerPeer>();
            DCGameServers = new Dictionary<int, DateTime>();
            CharMap = new Dictionary<string, CharDetails>();
        }

        #region Methods
        public void OnConnect(RegisterGameServer request, IncomingGameServerPeer peer)
        {
            Dictionary<string, RegCharInfo> online_chars = JsonConvertDefaultSetting.DeserializeObject<Dictionary<string, RegCharInfo>>(request.OnlineChars); 
            ServerConfig serverConfig = peer.Serverconfig;
            int serverid = serverConfig.id;
            int serverline = serverConfig.serverline;
            bool isDCServer = DCGameServers.Remove(serverid); //previous dc server

            if (isDCServer)
            {
                //inform other servers within serverline i got these online chars. 
                peer.ZRPC.ClusterToGameRPC.RegCharMultiple(request.OnlineChars, serverConfig.servername, GetBroadcastPeers(serverline));
            }

            //inform me all other server online chars within serverline.
            Dictionary<string, CharacterSyncData> _online_char_data = new Dictionary<string, CharacterSyncData>();
            foreach (var kvp in CharMap)
            {
                if (online_chars.ContainsKey(kvp.Key))
                    continue;
                if (kvp.Value.peer.Serverconfig.serverline == serverline)
                    _online_char_data.Add(kvp.Key, kvp.Value.data);
            }
            peer.ZRPC.ClusterToGameRPC.OnConnectedSyncData(JsonConvertDefaultSetting.SerializeObject(_online_char_data), peer);

            //add to server map
            if (!GameServersByLine.ContainsKey(serverline))
                GameServersByLine.Add(serverline, new Dictionary<int, IncomingGameServerPeer>());
            var serversInSameServerline = GameServersByLine[serverline];
            IncomingGameServerPeer oldPeer;
            if (serversInSameServerline.TryGetValue(serverid, out oldPeer))
            {
                if (oldPeer != peer)
                {
                    oldPeer.Disconnect();
                    oldPeer.Dispose();
                }
            }
            serversInSameServerline[serverid] = peer;
            GameServersByServerId[serverid] = peer;

            if (isDCServer) //previous dc server
            {
                string _charname = "";
                CharDetails _chardetail;
                foreach(var kvp in online_chars)
                {
                    _charname = kvp.Key;
                    if (CharMap.TryGetValue(_charname, out _chardetail))
                    {
                        _chardetail.peer = peer;
                        _chardetail.data.lvl = kvp.Value.lvl;
                        _chardetail.data.server = serverConfig.servername;
                    }
                    else
                    {
                        CharacterSyncData charSyncData = new CharacterSyncData { lvl = kvp.Value.lvl, server = serverConfig.servername };
                        CharMap.Add(_charname, new CharDetails { peer = peer, data = charSyncData });
                    }
                }
            }
        }

        public void OnDisconnect(IncomingGameServerPeer peer)
        {
            int serverid = peer.Serverconfig.id;
            int serverline = peer.Serverconfig.serverline;
            Dictionary<int, IncomingGameServerPeer> serversInSameServerline;
            if (GameServersByLine.TryGetValue(serverline, out serversInSameServerline))
            {
                serversInSameServerline.Remove(serverid);
                GameServersByServerId.Remove(serverid);
            }

            //remove user and char records and insert to dc record.
            List<string> _char_to_remove = new List<string>();
            foreach(var kvp in CharMap)
            {
                if (kvp.Value.peer == peer)
                    _char_to_remove.Add(kvp.Key);
            }
            for (int index = 0; index < _char_to_remove.Count; index++)
                CharMap.Remove(_char_to_remove[index]);

            DCGameServers[serverid] = DateTime.Now;
        }

        public List<IncomingGameServerPeer> GetBroadcastPeers(int serverline, IncomingGameServerPeer excludePeer)
        {
            Dictionary<int, IncomingGameServerPeer> peers;
            if (GameServersByLine.TryGetValue(serverline, out peers))
            {
                if (excludePeer == null)
                    return peers.Values.ToList();
                return peers.Values.Where(x => x != excludePeer).ToList();
            }
            return new List<IncomingGameServerPeer>();
        }

        public Dictionary<int, IncomingGameServerPeer> GetBroadcastPeers(int serverline)
        {
            Dictionary<int, IncomingGameServerPeer> peers;
            if (GameServersByLine.TryGetValue(serverline, out peers))
                return peers;
            return new Dictionary<int, IncomingGameServerPeer>();
        }

        public IncomingGameServerPeer GetPeerByServerId(int serverid)
        {
            IncomingGameServerPeer peer = null;
            GameServersByServerId.TryGetValue(serverid, out peer);
            return peer;
        }
        #endregion
    }
}