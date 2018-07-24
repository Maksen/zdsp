namespace Photon.LoadBalancing.GameServer
{
    #region using directives
    using System;
    using System.Threading.Tasks;
    using Photon.SocketServer;
    using Zealot.Server.Rules;
    using Zealot.Common;
    using Zealot.Common.RPC;
    using Zealot.Server.Entities;
    using ServerToServer;
    using System.Collections.Generic;
    using System.Linq;
    #endregion
    public partial class GameApplication : ApplicationBase
    {
        #region ClusterToGame
        [RPCMethod(RPCCategory.ClusterToGame, (byte)ClusterToGameRPCMethods.RegChar)]
        public void RegChar(string charName, int level, string servername, OutgoingClusterServerPeer peer)
        {
            CreateCharDatablock(charName, level, servername, null, false);
        }

        [RPCMethod(RPCCategory.ClusterToGame, (byte)ClusterToGameRPCMethods.UnRegChar)]
        public void UnRegChar(string charName, OutgoingClusterServerPeer peer)
        {
            DeleteCharDatablock(charName, true, false);
        }

        [RPCMethod(RPCCategory.ClusterToGame, (byte)ClusterToGameRPCMethods.RegCharMultiple)]
        public void RegCharMultiple(string onlineChars, string servername, OutgoingClusterServerPeer peer)
        {
            //online_chars contains online chars in servername.
            Dictionary<string, RegCharInfo> online_chars = JsonConvertDefaultSetting.DeserializeObject<Dictionary<string, RegCharInfo>>(onlineChars);
            foreach(var kvp in online_chars)
                CreateCharDatablock(kvp.Key, kvp.Value.lvl, servername, null, false);
            List<string> _delete_char = new List<string>();
            string _charname;
            foreach(var kvp in mCharOnlineMap)
            {
                _charname = kvp.Key;
                if (kvp.Value.server == servername && !online_chars.ContainsKey(_charname))
                    _delete_char.Add(_charname);
            }
            for (int index = 0; index < _delete_char.Count; index++)
                DeleteCharDatablock(_delete_char[index], true, false);
        }

        [RPCMethod(RPCCategory.ClusterToGame, (byte)ClusterToGameRPCMethods.OnConnectedSyncData)]
        public void OnConnectedSyncData(string data, OutgoingClusterServerPeer peer)
        {
            //online_chars contains online chars in other game servers within same serverline.
            Dictionary<string, CharacterSyncData> online_chars = JsonConvertDefaultSetting.DeserializeObject<Dictionary<string, CharacterSyncData>>(data);
            foreach (var kvp in online_chars)
                CreateCharDatablock(kvp.Key, kvp.Value.lvl, kvp.Value.server, null, false);
            List<string> _delete_char = new List<string>();
            string _charname;
            string _myserver = MyServerConfig.servername;
            foreach (var kvp in mCharOnlineMap)
            {
                _charname = kvp.Key;
                if (kvp.Value.server != _myserver && !online_chars.ContainsKey(_charname))
                    _delete_char.Add(_charname);
            }
            for (int index = 0; index < _delete_char.Count; index++)
                DeleteCharDatablock(_delete_char[index], true, false);
        }
        #endregion
    }
}