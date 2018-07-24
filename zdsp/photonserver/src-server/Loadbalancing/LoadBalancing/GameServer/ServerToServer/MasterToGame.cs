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
    #endregion
    public partial class GameApplication : ApplicationBase
    {
        #region MASTERTOGAMERPC
        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.BroadcastCookie)]
        public void BroadcastCookie(string cookie, string userid, PeerBase peer)
        {
            RegCookie(userid, cookie);
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.EventDataUpdated)]
        public void EventDataUpdated(byte eventtype, PeerBase peer)
        {
            GMEventType type = (GMEventType)eventtype;
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.GMMessage)]
        public void GMMessage(string sessionid, string playerName, string reason, int mode, PeerBase peer)
        {
            GameClientPeer clientpeer = GetCharPeer(playerName);
            if (clientpeer != null)
            {
                clientpeer.ZRPC.NonCombatRPC.GMMessage(reason, mode, clientpeer);
            }
            (peer as OutgoingMasterServerPeer).ZRPC.GameToMasterRPC.GMResultBool(sessionid, true, peer);
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.KickPlayer)]
        public void KickPlayer(string sessionid, string playerName, string reason, PeerBase peer)
        {
            GameClientPeer clientpeer = GetCharPeer(playerName);
            if (clientpeer != null)
            {
                //clientpeer.ZRPC.NonCombatRPC.GMMessage(reason, clientpeer);
                clientpeer.Disconnect();
            }
            (peer as OutgoingMasterServerPeer).ZRPC.GameToMasterRPC.GMResultBool(sessionid, true, peer);
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.MutePlayer)]
        public void MutePlayer(string sessionid, string playerName, string reason, string DT, PeerBase peer)
        {
            GameClientPeer clientpeer = GetCharPeer(playerName);
            if (clientpeer != null)
                clientpeer.mDTMute = DateTime.Parse(DT);
            (peer as OutgoingMasterServerPeer).ZRPC.GameToMasterRPC.GMResultBool(sessionid, true, peer);
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.KickFromGuild)]
        public void KickFromGuild(string sessionid, string playerName, string reason, PeerBase peer)
        {
            int result = GuildRules.KickFromGuildByGM(playerName);
            (peer as OutgoingMasterServerPeer).ZRPC.GameToMasterRPC.GMResultString(sessionid, result.ToString(), peer);
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.TeleportPlayer)]
        public void TeleportPlayer(string sessionid, string playerName, string reason, PeerBase peer)
        {
            GameClientPeer clientpeer = GetCharPeer(playerName);
            Player player = clientpeer != null ? clientpeer.mPlayer : null;
            if (player == null)
                (peer as OutgoingMasterServerPeer).ZRPC.GameToMasterRPC.GMResultString(sessionid, clientpeer == null ? "-1" : "-2", peer);
            else
            {
                clientpeer.TransferToCity(player.GetAccumulatedLevel());
                (peer as OutgoingMasterServerPeer).ZRPC.GameToMasterRPC.GMResultString(sessionid, "1", peer);
            }
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.AddSystemMessage)]
        public void AddSystemMessage(int id, string message, string color, string starttime, string endtime, int interval, int repeat, int type, PeerBase peer)
        {
            SystemMessageRules.AddMessage(id, message, color, starttime, endtime, interval, repeat, type);
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.DeleteSystemMessage)]
        public void DeleteSystemMessage(int id, PeerBase peer)
        {
            SystemMessageRules.RemoveMessage(id);
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.LogUIDShift)]
        public void LogUIDShift(string userId, int oldLoginType, string oldLoginId, int newLoginType, string newLoginId, PeerBase peer)
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

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.TongbaoCostBuffChange)]
        public void TongbaoCostBuffChange(PeerBase peer)
        {
            TongbaoCostBuff.Init("gmtools");
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.GMItemMallStatusUpdate)]
        public void GMItemMallStatusUpdate(PeerBase peer)
        {
            Task task = ItemMall.ItemMallManager.Instance.RefreshShopStatus();
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.ChangeGuildLeader)]
        public void ChangeGuildLeader(string sessionid, int guildid, string newleader, PeerBase peer)
        {
            int success = GuildRules.GMChangeLeader(guildid, newleader);
            (peer as OutgoingMasterServerPeer).ZRPC.GameToMasterRPC.GMResultString(sessionid, success.ToString(), peer);
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.NotifyActivityChange)]
        public void NotifyActivityChange(PeerBase peer)
        {
            Task task = GMActivityRules.UpdateConfigs();
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.GMAuctionChange)]
        public void GMAuctionChange(PeerBase peer)
        {
            Task task = AuctionRules.InitAuctionItemsFromDB();
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.GMTopUpActivityUpdate)]
        public async Task GMTopUpActivityUpdate(PeerBase peer)
        {
            TopUp.TopUpManager topUpManager = await TopUp.TopUpManager.InstanceAsync();
            var ignoreAwait = topUpManager.UpdateTopUpActivityAsync();
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.ExchangeRateUpdate)]
        public void ExchangeRateUpdate(PeerBase peer)
        {
            var ignoreAwait = CurrencyExchangeRules.UpdateRate();
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.GMGetArenaRank)]
        public void GMGetArenaRank(string seesionid, string player, PeerBase peer)
        {
            int rank = LadderRules.GetArenaRank(player);
            (peer as OutgoingMasterServerPeer).ZRPC.GameToMasterRPC.GMResultString(seesionid, (rank + 1).ToString(), peer);
        }

        [RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.GMResetArenaRank)]
        public void GMResetArenaRank(string seesionid, string player, PeerBase peer)
        {
            LadderRules.ResetPlayerRank(player);
            (peer as OutgoingMasterServerPeer).ZRPC.GameToMasterRPC.GMResultBool(seesionid, true, peer);
        }
        #endregion
    }
}