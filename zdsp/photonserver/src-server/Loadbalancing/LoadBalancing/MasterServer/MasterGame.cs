using ExitGames.Logging;
using Photon.LoadBalancing.MasterServer.GameServer;
using Photon.LoadBalancing.ServerToServer.Operations;
using System;
using System.Collections.Generic;
using System.Text;
using Zealot.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Photon.LoadBalancing.MasterServer
{
    public partial class MasterGame
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public Dictionary<int, IncomingGamePeer> GameServersByServerId; //serverid <- IncomingGamePeer
        public Dictionary<int, Dictionary<int, IncomingGamePeer>> GameServersByLine; //serverline <- serverid <- IncomingGamePeer
        public ServerLineList ServerLineList;
        public GameServerStatus GameServerStatus; //to record previous status even IncomingClusterServerPeer disconnected.
        public Dictionary<string, IncomingGamePeer> UserMap;//userid <- IncomingGamePeer
        public Dictionary<int, DateTime> DCGameServers; //serverid <- dc datetime.
        public Dictionary<string, int> DCUsers; //userid <- serverid

        public MasterGame()
        {
            GameServersByServerId = new Dictionary<int, IncomingGamePeer>();
            GameServersByLine = new Dictionary<int, Dictionary<int, IncomingGamePeer>>();
            ServerLineList = new ServerLineList();
            GameServerStatus = new GameServerStatus();
            UserMap = new Dictionary<string, IncomingGamePeer>();
            DCGameServers = new Dictionary<int, DateTime>();
            DCUsers = new Dictionary<string, int>();
        }

        public void OnConnect(RegisterGameServer request, IncomingGamePeer peer)
        {
            string[] online_users = request.OnlineUsers;
            ServerConfig serverConfig = peer.Serverconfig;
            serverConfig.onlinePlayers = online_users.Length;
            int serverid = serverConfig.id;
            int serverline = serverConfig.serverline;
            bool isDCServer = DCGameServers.Remove(serverid); //previous dc server

            if (isDCServer)
            {
                //kick users in other game server within same serverline.
                if (online_users.Length > 0)
                    peer.ZRPC.MasterToGameRPC.ForceKickMutiple(online_users, "relogin", GameServersByServerId);

                //clean DCUsers properly.
                List<string> _key_to_remove = new List<string>();
                foreach (var kvp in DCUsers)
                {
                    if (kvp.Value == serverid)
                        _key_to_remove.Add(kvp.Key);
                }
                for (int index = 0; index < _key_to_remove.Count; index++)
                    DCUsers.Remove(_key_to_remove[index]);

                //rebuild UserMap
                for (int index = 0; index < online_users.Length; index++)
                    UserMap[online_users[index]] = peer;
            }

            //add to server map
            IncomingGamePeer oldPeer;
            if (GameServersByServerId.TryGetValue(serverid, out oldPeer))
            {
                if (oldPeer != peer)
                {
                    oldPeer.Disconnect();
                    oldPeer.Dispose();
                }
            }
            GameServersByServerId[serverid] = peer;

            Dictionary<int, IncomingGamePeer> serversInSameServerline;
            if (!GameServersByLine.TryGetValue(serverline, out serversInSameServerline))
            {
                serversInSameServerline = new Dictionary<int, IncomingGamePeer>();
                GameServersByLine.Add(serverline, serversInSameServerline);
                CheckNewServerLine(serverline);
            }
            serversInSameServerline[serverid] = peer;

            //add or update server status
            GMServerStatus gmServerStatus;
            Dictionary<int, GMServerStatus> ServerListStatus = GameServerStatus.ServerListStatus;
            if (!ServerListStatus.TryGetValue(serverid, out gmServerStatus))
            {
                gmServerStatus = new GMServerStatus(serverid);
                ServerListStatus.Add(serverid, gmServerStatus);
            }
            gmServerStatus.online = true;
            gmServerStatus.ccu = serverConfig.onlinePlayers;
        }

        public void OnDisconnect(IncomingGamePeer peer)
        {
            int serverid = peer.Serverconfig.id;
            int serverline = peer.Serverconfig.serverline;
            Dictionary<int, IncomingGamePeer> serversInSameServerline;
            if (GameServersByLine.TryGetValue(serverline, out serversInSameServerline))
            {
                if (serversInSameServerline.Remove(serverid))
                    GameServerStatus.ServerListStatus[serverid].online = false;
                GameServersByServerId.Remove(serverid);
            }

            //remove user and char records and insert to dc record.
            List<string> _user_to_remove = new List<string>();
            foreach (var kvp in UserMap)
            {
                if (kvp.Value == peer)
                {
                    _user_to_remove.Add(kvp.Key);
                    DCUsers[kvp.Key] = serverid;
                }
            }
            for (int index = 0; index < _user_to_remove.Count; index++)
                UserMap.Remove(_user_to_remove[index]);

            DCGameServers[serverid] = DateTime.Now;
        }

        private void CheckNewServerLine(int serverLine)
        {
            if (!ServerLineList.list.Exists(x => x.serverLineId == serverLine))
            {
                var task = MasterApplication.Instance.ServerLineSelectAllAsync();
            }
        }

        public string GetServerListJson()
        {
            JObject json = new JObject();
            json["serverLines"] = JToken.FromObject(this.ServerLineList.list);
            JArray servers = new JArray();

            json["gameServers"] = servers;
            byte gameserver = (byte)GameServerType.Game;
            foreach (var kvp in GameServersByServerId)
            {
                JArray item = new JArray();
                var config = kvp.Value.Serverconfig;
                if (config.servertype == gameserver)
                {
                    item.Add(JObject.FromObject(config));
                    item.Add(config.GetServerLoad(config.onlinePlayers).ToString());
                }
            }
            return json.ToString( Formatting.None);
        }


        public string GetSerializedServer()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0};", this.ServerLineList.serializeString);
            byte gameserver = (byte)GameServerType.Game;
            foreach (var kvp in GameServersByServerId)
            {
                var config = kvp.Value.Serverconfig;
                if (config.servertype == gameserver)
                    sb.AppendFormat("{0}|{1};", config.serializeString, (byte)config.GetServerLoad(config.onlinePlayers));
            }
            return sb.ToString().TrimEnd(';');
        }

        public IncomingGamePeer GetPeerByServerId(int serverid)
        {
            IncomingGamePeer peer;
            GameServersByServerId.TryGetValue(serverid, out peer);
            return peer;
        }

        public bool KickPlayer(string userid, string reason)
        {
            IncomingGamePeer peer;
            if (UserMap.TryGetValue(userid, out peer))
            {
                if (peer != null)
                    peer.ZRPC.MasterToGameRPC.ForceKick(userid, "relogin", peer);
                return true;
            }
            return false;
        }

        public string GetGameServerStatus(bool force)
        {
            DateTime now = DateTime.Now;
            if (force || (now - GameServerStatus.lastQueryDT).TotalMinutes > 9)
            {
                IncomingGamePeer _peer;
                if (GameServerStatus.lastQueryDT < now.Date) //new day
                {
                    foreach (var kvp in GameServerStatus.ServerListStatus.Values)
                        kvp.pcu = 0;
                }
                foreach (var kvp in GameServerStatus.ServerListStatus)
                {
                    if (GameServersByServerId.TryGetValue(kvp.Key, out _peer))
                        kvp.Value.Update(_peer.Serverconfig.onlinePlayers);
                }
            }
            return GameServerStatus.GetSerializeString(force);
        }
    }
}
