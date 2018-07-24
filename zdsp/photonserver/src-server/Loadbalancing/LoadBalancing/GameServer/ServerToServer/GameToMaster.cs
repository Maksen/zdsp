namespace Photon.LoadBalancing.GameServer
{
    #region using directives
    using Photon.SocketServer.ServerToServer;
    using Zealot.Common;
    #endregion
    public partial class OutgoingMasterServerPeer : OutboundS2SPeer
    {
        #region GAMETOMASTERRPC
        public void RegularServerStatusUpdate(GMServerStatus status)
        {
            if (IsRegistered)
                this.ZRPC.GameToMasterRPC.RegularServerStatusUpdate(status.id, status.ccu, status.pcu, status.roomupdatedur, status.cpuusage,
                    status.money, status.moneyDiff, status.gold, status.goldDiff, status.bindgold, status.bindgoldDiff, this);
        }
        #endregion        
    }
}