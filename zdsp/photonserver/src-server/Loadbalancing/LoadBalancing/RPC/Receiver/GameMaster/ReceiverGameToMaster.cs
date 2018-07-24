using Photon.LoadBalancing.MasterServer.GameServer;
using Zealot.Common;
using Zealot.Common.RPC;

namespace Photon.LoadBalancing.MasterServer
{
    public partial class MasterGame
    {
        [RPCMethod(RPCCategory.GameToMaster, (byte)GameToMasterRPCMethods.RegularServerStatusUpdate)]
        public void RegularServerStatusUpdate(int roomupdatedur, byte cpuusage, IncomingGamePeer peer)
        {
            GMServerStatus status;
            if (GameServerStatus.ServerListStatus.TryGetValue(peer.Serverconfig.id, out status))
            {
                status.roomupdatedur = roomupdatedur;
                status.cpuusage = cpuusage;
            }
        }

        [RPCMethod(RPCCategory.GameToMaster, (byte)GameToMasterRPCMethods.RegUser)]
        public void RegUser(string userid, IncomingGamePeer peer)
        {
            peer.Serverconfig.onlinePlayers++;
            IncomingGamePeer old_peer;
            if (UserMap.TryGetValue(userid, out old_peer))
            {
                if (old_peer != null && old_peer != peer)
                    old_peer.ZRPC.MasterToGameRPC.ForceKick(userid, "relogin", old_peer);
            }
            UserMap[userid] = peer;
            log.DebugFormat("RegUser {0}", userid);
        }

        [RPCMethod(RPCCategory.GameToMaster, (byte)GameToMasterRPCMethods.UnRegUser)]
        public void UnRegUser(string userid, IncomingGamePeer peer)
        {
            peer.Serverconfig.onlinePlayers--;
            IncomingGamePeer old_peer;
            if (UserMap.TryGetValue(userid, out old_peer))
            {
                if (old_peer == null || old_peer == peer)
                    UserMap.Remove(userid);
            }
        }

        [RPCMethod(RPCCategory.GameToMaster, (byte)GameToMasterRPCMethods.TransferServer)]
        public void TransferServer(string userid, string cookie, int serverid, IncomingGamePeer peer)
        {
            var game_peer = GetPeerByServerId(serverid);
            if (game_peer != null && game_peer.Serverconfig.serverline == peer.Serverconfig.serverline) //make sure same serverline
            {
                game_peer.ZRPC.MasterToGameRPC.RegCookie(userid, cookie, game_peer);
                peer.ZRPC.MasterToGameRPC.Ret_TransferServer(userid, serverid, game_peer.Serverconfig.ipAddr, peer);
            }
            else
                peer.ZRPC.MasterToGameRPC.Ret_TransferServer(userid, serverid, "", peer);
        }

        [RPCMethod(RPCCategory.GameToMaster, (byte)GameToMasterRPCMethods.GMResultBool)]
        public void GMResultBool(string sessionid, bool result, IncomingGamePeer peer)
        {
            var gmPeer = MasterApplication.Instance.GMPeer;
            gmPeer.ZRPC.MasterToGMRPC.GMResultBool(sessionid, result, gmPeer);
        }

        [RPCMethod(RPCCategory.GameToMaster, (byte)GameToMasterRPCMethods.GMResultString)]
        public void GMResultString(string sessionid, string result, IncomingGamePeer peer)
        {
            var gmPeer = MasterApplication.Instance.GMPeer;
            gmPeer.ZRPC.MasterToGMRPC.GMResultString(sessionid, result, gmPeer);
        }
    }
}