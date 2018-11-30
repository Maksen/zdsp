using Photon.LoadBalancing.Operations;
using Photon.LoadBalancing.GameServer;
using Zealot.Common.RPC;

namespace Zealot.RPC
{
    public class MasterToGameRPC : ServerRPCBase
    {
        public MasterToGameRPC() :
            base(typeof(MasterToGameRPC), (byte)OperationCode.MasterToGame, true)
        {
            SetMainContext(typeof(GameApplication), RPCCategory.MasterToGame);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.ForceKick)]
        public void ForceKick(string userid, string reason, object target)
        {
            ProxyMethod("ForceKick", userid, reason, target);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.ForceKickMutiple)]
        public void ForceKickMutiple(string[] userids, string reason, object target)
        {
            ProxyMethod("ForceKickMutiple", userids, reason, target);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.RegCookie)]
        public void RegCookie(string userid, string cookie, object target)
        {
            ProxyMethod("RegCookie", userid, cookie, target);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.Ret_TransferServer)]
        public void Ret_TransferServer(string userid, int serverid, string serverAddress, object target)
        {
            ProxyMethod("Ret_TransferServer", userid, serverid, serverAddress, target);
        }

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.EventDataUpdated)]
        public void EventDataUpdated(byte eventtype, string msg, object target)
        {
            ProxyMethod("EventDataUpdated", eventtype, msg, target);
        }

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.LogUIDShift)]
        //public void LogUIDShift(string userId, int oldLoginType, string oldLoginId, int newLoginType, string newLoginId, object target)
        //{
        //    ProxyMethod("LogUIDShift", userId, oldLoginType, oldLoginId, newLoginType, newLoginId, target);
        //}

        [RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.GMMessage)]
        public void GMMessage(string seesionid, string player, string message, int mode, object target)
        {
            ProxyMethod("GMMessage", seesionid, player, message, mode, target);
        }

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.KickPlayer)]
        //public void KickPlayer(string seesionid, string player, string reason, object target)
        //{
        //    ProxyMethod("KickPlayer", seesionid, player, reason, target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.MutePlayer)]
        //public void MutePlayer(string seesionid, string player, string reason, string DT, object target)
        //{
        //    ProxyMethod("MutePlayer", seesionid, player, reason, DT, target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.KickFromGuild)]
        //public void KickFromGuild(string seesionid, string player, string reason, object target)
        //{
        //    ProxyMethod("KickFromGuild", seesionid, player, reason, target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.TeleportPlayer)]
        //public void TeleportPlayer(string seesionid, string player, string reason, object target)
        //{
        //    ProxyMethod("TeleportPlayer", seesionid, player, reason, target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.AddSystemMessage)]
        //public void AddSystemMessage(int id, string message, string color, string starttime, string endtime, int interval, int repeat, int type, object target)
        //{
        //    ProxyMethod("AddSystemMessage", id, message, color, starttime, endtime, interval, repeat, type, target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.DeleteSystemMessage)]
        //public void DeleteSystemMessage(int id, object target)
        //{
        //    ProxyMethod("DeleteSystemMessage", id, target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.TongbaoCostBuffChange)]
        //public void TongbaoCostBuffChange(object target)
        //{
        //    ProxyMethod("TongbaoCostBuffChange", target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.GMItemMallStatusUpdate)]
        //public void GMItemMallStatusUpdate(object target)
        //{
        //    ProxyMethod("GMItemMallStatusUpdate", target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.ChangeGuildLeader)]
        //public void ChangeGuildLeader(string seesionid, int guildid, string newleader, object target)
        //{
        //    ProxyMethod("ChangeGuildLeader", seesionid, guildid, newleader, target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.NotifyActivityChange)]
        //public void NotifyActivityChange(object target)
        //{
        //    ProxyMethod("NotifyActivityChange", target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.GMAuctionChange)]
        //public void GMAuctionChange(object target)
        //{
        //    ProxyMethod("GMAuctionChange", target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.GMTopUpActivityUpdate)]
        //public void GMTopUpActivityUpdate(object target)
        //{
        //    ProxyMethod("GMTopUpActivityUpdate", target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.ExchangeRateUpdate)]
        //public void ExchangeRateUpdate(object target)
        //{
        //    ProxyMethod("ExchangeRateUpdate", target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.GMGetArenaRank)]
        //public void GMGetArenaRank(string seesionid, string player, object target)
        //{
        //    ProxyMethod("GMGetArenaRank", seesionid, player, target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.GMResetArenaRank)]
        //public void GMResetArenaRank(string seesionid, string player, object target)
        //{
        //    ProxyMethod("GMResetArenaRank", seesionid, player, target);
        //}

        //[RPCMethod(RPCCategory.MasterToGameRPC, (byte)MasterToGameRPCMethods.GMNotifyTalentModifierChanged)]
        //public void GMNotifyTalentModifierChanged(object target)
        //{
        //    ProxyMethod("GMNotifyTalentModifierChanged", target);
        //}


        //[RPCMethod(RPCCategory.MasterToGame, (byte)MasterToGameRPCMethods.Ret_GetServerList)]
        //public void Ret_GetServerList(string info, object target)
        //{
        //    ProxyMethod("Ret_GetServerList", info, target);
        //}
    }
}
