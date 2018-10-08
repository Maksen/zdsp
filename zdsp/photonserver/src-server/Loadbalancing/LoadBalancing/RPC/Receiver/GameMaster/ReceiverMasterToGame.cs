using Photon.LoadBalancing.GameServer.NPCStore;
using Photon.LoadBalancing.ServerToServer;
using System;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.Common.RPC;
using Zealot.Server.Entities;
using Zealot.Server.Rules;

namespace Photon.LoadBalancing.GameServer
{
    public partial class GameApplication
    {
        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.ForceKick)]
        public void ForceKick(string userid, string reason, OutgoingGameToMasterPeer peer)
        {
            GameClientPeer clientpeer = GetUserPeerByUserid(userid);
            if (clientpeer != null)
            {
                RemoveCookie(clientpeer.UserId);
                clientpeer.Disconnect();
            }
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.ForceKickMutiple)]
        public void ForceKickMutiple(string[] userids, string reason, OutgoingGameToMasterPeer peer)
        {
            for (int index = 0; index < userids.Length; index++)
                ForceKick(userids[index], reason, peer);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.RegCookie)]
        public void RegCookie(string userid, string cookie, OutgoingGameToMasterPeer peer)
        {
            AddCookie(userid, cookie);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.Ret_TransferServer)]
        public void Ret_TransferServer(string userid, int serverid, string serverAddress, OutgoingGameToMasterPeer peer)
        {
            var user_peer = GetUserPeerByUserid(userid);
            if (user_peer != null)
            {
                var info = user_peer.mTransferServerInfo;
                if (info != null && info.serverid == serverid && info.charname == user_peer.mChar)
                {
                    //user_peer.ForceSaveCharacter();
                    user_peer.mTransferServerInfo = null;
                    user_peer.ZRPC.NonCombatRPC.Ret_TransferServer(serverid, serverAddress, user_peer);
                }
            }
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.EventDataUpdated)]
        public void EventDataUpdated(byte eventtype, string message, OutgoingGameToMasterPeer peer)
        {
            GMEventType type = (GMEventType)eventtype;

            switch (type)
            {
                case GMEventType.NPCShopDataUpdate:
                    NPCStoreManager.reset = true;
                    break;
            }
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.GMMessage)]
        public void GMMessage(string sessionid, string playerName, string reason, int mode, OutgoingGameToMasterPeer peer)
        {
            GameClientPeer clientpeer = GetCharPeer(playerName);
            if (clientpeer != null)
            {
                clientpeer.ZRPC.NonCombatRPC.GMMessage(reason, mode, clientpeer);
            }
            peer.ZRPC.GameToMasterRPC.GMResultBool(sessionid, true, peer);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.KickPlayer)]
        public void KickPlayer(string sessionid, string playerName, string reason, OutgoingGameToMasterPeer peer)
        {
            GameClientPeer clientpeer = GetCharPeer(playerName);
            if (clientpeer != null)
            {
                RemoveCookie(clientpeer.UserId);
                clientpeer.Disconnect();
            }
            peer.ZRPC.GameToMasterRPC.GMResultBool(sessionid, true, peer);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.MutePlayer)]
        public void MutePlayer(string sessionid, string playerName, string reason, string DT, OutgoingGameToMasterPeer peer)
        {
            GameClientPeer clientpeer = GetCharPeer(playerName);
            if (clientpeer != null)
                clientpeer.mDTMute = DateTime.Parse(DT);
            peer.ZRPC.GameToMasterRPC.GMResultBool(sessionid, true, peer);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.KickFromGuild)]
        public void KickFromGuild(string sessionid, string playerName, string reason, OutgoingGameToMasterPeer peer)
        {
            int result = GuildRules.KickFromGuildByGM(playerName);
            peer.ZRPC.GameToMasterRPC.GMResultString(sessionid, result.ToString(), peer);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.TeleportPlayer)]
        public void TeleportPlayer(string sessionid, string playerName, string reason, OutgoingGameToMasterPeer peer)
        {
            GameClientPeer clientpeer = GetCharPeer(playerName);
            Player player = clientpeer != null ? clientpeer.mPlayer : null;
            if (player == null)
                peer.ZRPC.GameToMasterRPC.GMResultString(sessionid, clientpeer == null ? "-1" : "-2", peer);
            else
            {
                clientpeer.TransferToCity(player.GetAccumulatedLevel());
                peer.ZRPC.GameToMasterRPC.GMResultString(sessionid, "1", peer);
            }
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.AddSystemMessage)]
        public void AddSystemMessage(int id, string message, string color, string starttime, string endtime, int interval, int repeat, int type, OutgoingGameToMasterPeer peer)
        {
            SystemMessageRules.AddMessage(id, message, color, starttime, endtime, interval, repeat, type);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.DeleteSystemMessage)]
        public void DeleteSystemMessage(int id, OutgoingGameToMasterPeer peer)
        {
            SystemMessageRules.RemoveMessage(id);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.LogUIDShift)]
        public void LogUIDShift(string userId, int oldLoginType, string oldLoginId, int newLoginType, string newLoginId, OutgoingGameToMasterPeer peer)
        {
            string message = string.Format("oldLoginType: {0} | oldLoginId: {1} | newLoginType: {2} | newLoginId: {3}",
                oldLoginType,
                oldLoginId,
                newLoginType,
                newLoginId);

            Zealot.Logging.Client.LogClasses.AccountShift accountShiftLog = new Zealot.Logging.Client.LogClasses.AccountShift();
            accountShiftLog.userId = userId.ToString();
            accountShiftLog.message = message;
            accountShiftLog.oldLoginType = oldLoginType;
            accountShiftLog.oldLoginId = oldLoginId;
            accountShiftLog.newLoginType = newLoginType;
            accountShiftLog.newLoginId = newLoginId;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(accountShiftLog);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.TongbaoCostBuffChange)]
        public void TongbaoCostBuffChange(OutgoingGameToMasterPeer peer)
        {
            TongbaoCostBuff.Init("gmtools");
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.GMItemMallStatusUpdate)]
        public void GMItemMallStatusUpdate(OutgoingGameToMasterPeer peer)
        {
            Task task = ItemMall.ItemMallManager.Instance.RefreshShopStatus();
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.ChangeGuildLeader)]
        public void ChangeGuildLeader(string sessionid, int guildid, string newleader, OutgoingGameToMasterPeer peer)
        {
            int success = GuildRules.GMChangeLeader(guildid, newleader);
            peer.ZRPC.GameToMasterRPC.GMResultString(sessionid, success.ToString(), peer);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.NotifyActivityChange)]
        public void NotifyActivityChange(OutgoingGameToMasterPeer peer)
        {
            Task task = GMActivityRules.UpdateConfigs();
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.GMAuctionChange)]
        public void GMAuctionChange(OutgoingGameToMasterPeer peer)
        {
            Task task = AuctionRules.InitAuctionItemsFromDB();
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.GMTopUpActivityUpdate)]
        public async Task GMTopUpActivityUpdate(OutgoingGameToMasterPeer peer)
        {
            TopUp.TopUpManager topUpManager = await TopUp.TopUpManager.InstanceAsync();
            var ignoreAwait = topUpManager.UpdateTopUpActivityAsync();
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.ExchangeRateUpdate)]
        public void ExchangeRateUpdate(OutgoingGameToMasterPeer peer)
        {
            var ignoreAwait = CurrencyExchangeRules.UpdateRate();
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.GMGetArenaRank)]
        public void GMGetArenaRank(string seesionid, string player, OutgoingGameToMasterPeer peer)
        {
            int rank = LadderRules.GetArenaRank(player);
            peer.ZRPC.GameToMasterRPC.GMResultString(seesionid, (rank + 1).ToString(), peer);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.GMResetArenaRank)]
        public void GMResetArenaRank(string seesionid, string player, OutgoingGameToMasterPeer peer)
        {
            LadderRules.ResetPlayerRank(player);
            peer.ZRPC.GameToMasterRPC.GMResultBool(seesionid, true, peer);
        }        
    }
}