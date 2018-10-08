// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IncomingGameServerPeer.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the IncomingGameServerPeer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Photon.LoadBalancing.MasterServer.GameManager
{
    #region using directives
    using System;
    using ExitGames.Logging;
    using Photon.LoadBalancing.ServerToServer.Events;
    using Photon.LoadBalancing.ServerToServer.Operations;
    using Photon.SocketServer;
    using Photon.SocketServer.ServerToServer;
    using OperationCode = Photon.LoadBalancing.ServerToServer.Operations.OperationCode;
    using Zealot.RPC;
    using ErrorCode = Photon.Common.ErrorCode;
    using Zealot.Common.RPC;
    using Photon.LoadBalancing.MasterServer.GameServer;
    #endregion

    public class IncomingGMPeer : InboundS2SPeer
    {
        #region Constants and Fields
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private readonly MasterApplication application;
        public ZRPC ZRPC;

        #endregion
        #region Constructors and Destructors
        public IncomingGMPeer(InitRequest initRequest, MasterApplication application)
            : base(initRequest)
        {
            this.application = application;
            ZRPC = new ZRPC();
            log.InfoFormat("GM server connection from {0}:{1} established (id={2})", this.RemoteIP, this.RemotePort, this.ConnectionId);
        }
        #endregion
        #region Properties
        public string Key { get; protected set; }
        #endregion
        #region Public Methods
        #endregion
        #region Methods
        public bool IsReady()
        {
            return !string.IsNullOrEmpty(Key);
        }

        protected virtual OperationResponse HandleRegisterServerRequest(OperationRequest request)
        {
            var registerRequest = new RegisterGameServer(this.Protocol, request);
            if (registerRequest.IsValid == false)
            {
                string msg = registerRequest.GetErrorMessage();
                log.ErrorFormat("RegisterGameServer contract error: {0}", msg);
                return new OperationResponse(request.OperationCode) { DebugMessage = msg, ReturnCode = (short)ErrorCode.OperationInvalid };
            }
            var contract = new RegisterServerResponse();
            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    "Received register gmpeer request: Address={0}, TcpPort={1}",
                    registerRequest.GameServerAddress,
                    registerRequest.TcpPort);
            }
            this.Key = string.Format("{0}-{1}-{2}", registerRequest.GameServerAddress, registerRequest.UdpPort, registerRequest.TcpPort);
            //this.application.GameServers.OnConnect(this);
            return new OperationResponse(request.OperationCode, contract);
        }

        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            if (log.IsInfoEnabled)
            {
                log.InfoFormat("OnDisconnect: game server connection closed (connectionId={0}, Key={1}, reason={2})", this.ConnectionId, Key, reasonCode);
            }
            if (application.GMPeer == this)
                application.GMPeer = null;
        }
        protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
        {
        }
        protected override void OnOperationRequest(OperationRequest request, SendParameters sendParameters)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("OnOperationRequest: pid={0}, op={1}", this.ConnectionId, request.OperationCode);
            }

            application.executionFiber.Enqueue(() =>
            {
                OperationResponse response = null;
                switch (request.OperationCode)
                {
                    default:
                        response = new OperationResponse(request.OperationCode) { ReturnCode = -1, DebugMessage = "Unknown operation code" };
                        break;
                    case ServerEventCode.GMRPC:
                        ZRPC.MasterToGMRPC.OnCommandServer(this, this, request, sendParameters);
                        break;
                    case OperationCode.RegisterGameServer:
                        {
                            response = IsReady()
                                            ? new OperationResponse(request.OperationCode) { ReturnCode = -1, DebugMessage = "already registered" }
                                            : this.HandleRegisterServerRequest(request);
                            break;
                        }
                }
                if (response != null)
                    this.SendOperationResponse(response, sendParameters);
            });
        }
        protected override void OnOperationResponse(OperationResponse opResponse, SendParameters sendParameters)
        {
        }
        #endregion

        #region MASTERTOGMRPC
        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.GetServerList)]
        //public void GetServerList(string session, PeerBase peer)
        //{
        //}

        [RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.GetAllServerStatus)]
        public void GetAllServerStatus(string session, PeerBase peer)
        {
            string status = application.mMasterCluster.ClusterServers.ToString();
            ZRPC.MasterToGMRPC.GMResultString(session, status, peer);
        }

        [RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.BroadcastToGameServers)]
        public void BroadcastToGameServers(string sessionid, byte eventtype, string message, string serverlist, PeerBase peer)
        {
            bool result = false;
            if (serverlist == "All")
            {
                foreach (var gameserver in application.mMasterGame.GameServersByServerId)
                {
                    if (gameserver.Value.Connected)
                    {
                        gameserver.Value.ZRPC.MasterToGameRPC.EventDataUpdated(eventtype, message, gameserver.Value);
                    }
                }

                return;
            }
            int serverid = 0;
            string[] servers = serverlist.Split(';');
            if (servers.Length > 0)
            {
                for (int i = 0; i < servers.Length; i++)
                {
                    if (!string.IsNullOrEmpty(servers[i]) )
                    {
                        IncomingGamePeer targetpeer = application.GetServerPeerById(int.Parse(servers[i]));
                        if (targetpeer != null && targetpeer.Connected)
                        {
                            targetpeer.ZRPC.MasterToGameRPC.EventDataUpdated(eventtype, message, targetpeer);
                        }
                    }
                }
                result = true;
            }
            ZRPC.MasterToGMRPC.GMResultBool(sessionid, result, peer);
        }

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.GMMessage)]
        //public void GMMessage(string sessionid, string player, string message, int mode, int serverid, PeerBase peer)
        //{
        //    IncomingGameServerPeer targetpeer = this.application.GetServerPeerById(serverid);
        //    if (targetpeer != null && targetpeer.Connected)
        //        targetpeer.ZRPC.MasterToGameRPC.GMMessage(sessionid, player, message, mode, targetpeer);
        //    else
        //        ZRPC.MasterToGMRPC.GMResultBool(sessionid, false, peer);
        //}

        [RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.KickPlayer)]
        public void KickPlayer(string sessionid, string player, string reason, int serverid, PeerBase peer)
        {
            IncomingGamePeer targetpeer = this.application.GetServerPeerById(serverid);
            if (targetpeer != null && targetpeer.Connected)
                targetpeer.ZRPC.MasterToGameRPC.ForceKick(player, reason, targetpeer);
            else
                ZRPC.MasterToGMRPC.GMResultBool(sessionid, false, peer);
        }

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.MutePlayer)]
        //public void MutePlayer(string sessionid, string player, string reason, string DT, int serverid, PeerBase peer)
        //{
        //    IncomingGameServerPeer targetpeer = this.application.GetServerPeerById(serverid);
        //    if (targetpeer != null && targetpeer.Connected)
        //        targetpeer.ZRPC.MasterToGameRPC.MutePlayer(sessionid, player, reason, DT, targetpeer);
        //    else
        //        ZRPC.MasterToGMRPC.GMResultBool(sessionid, false, peer);
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.KickFromGuild)]
        //public void KickFromGuild(string sessionid, string player, string reason, int serverid, PeerBase peer)
        //{
        //    IncomingGameServerPeer targetpeer = this.application.GetServerPeerById(serverid);
        //    if (targetpeer != null && targetpeer.Connected)
        //        targetpeer.ZRPC.MasterToGameRPC.KickFromGuild(sessionid, player, reason, targetpeer);
        //    else
        //        ZRPC.MasterToGMRPC.GMResultBool(sessionid, false, peer);
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.TeleportPlayer)]
        //public void TeleportPlayer(string sessionid, string player, string reason, int serverid, PeerBase peer)
        //{
        //    IncomingGameServerPeer targetpeer = this.application.GetServerPeerById(serverid);
        //    if (targetpeer != null && targetpeer.Connected)
        //    {
        //        targetpeer.ZRPC.MasterToGameRPC.TeleportPlayer(sessionid, player, reason, targetpeer);
        //    }
        //    else
        //        ZRPC.MasterToGMRPC.GMResultBool(sessionid, false, peer);
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.SendSystemMessage)]
        //public void SendSystemMessage(string sessionid, int id, string message, string color, string starttime, string endtime, int interval, int repeat, string serverlist, int type, PeerBase peer)
        //{
        //    bool result = false;
        //    string[] servers = serverlist.Split(';');
        //    if (servers.Length > 0)
        //    {
        //        for (int i = 0; i < servers.Length; i++)
        //        {
        //            if (!string.IsNullOrEmpty(servers[i]))
        //            {
        //                IncomingGameServerPeer targetpeer = this.application.GetServerPeerById(int.Parse(servers[i]));
        //                if (targetpeer != null && targetpeer.Connected)
        //                {
        //                    targetpeer.ZRPC.MasterToGameRPC.AddSystemMessage(id, message, color, starttime, endtime, interval, repeat, type, targetpeer);
        //                }
        //            }
        //        }
        //        result = true;
        //    }
        //    ZRPC.MasterToGMRPC.GMResultBool(sessionid, result, peer);
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.DeleteSystemMessage)]
        //public void DeleteSystemMessage(string sessionid, int id, string serverlist, PeerBase peer)
        //{
        //    bool result = false;
        //    string[] servers = serverlist.Split(';');
        //    if (servers.Length > 0)
        //    {
        //        for (int i = 0; i < servers.Length; i++)
        //        {
        //            if (!string.IsNullOrEmpty(servers[i]))
        //            {
        //                IncomingGameServerPeer targetpeer = this.application.GetServerPeerById(int.Parse(servers[i]));
        //                if (targetpeer != null && targetpeer.Connected)
        //                {
        //                    targetpeer.ZRPC.MasterToGameRPC.DeleteSystemMessage(id, targetpeer);
        //                }
        //            }
        //        }
        //        result = true;
        //    }
        //    ZRPC.MasterToGMRPC.GMResultBool(sessionid, result, peer);
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.TongbaoCostBuffChange)]
        //public void TongbaoCostBuffChange(string sessionid, PeerBase peer)
        //{
        //    bool result = false;
        //    Dictionary<int, IncomingGameServerPeer> lists = this.application.GetAllServerPeers();
        //    foreach (KeyValuePair<int, IncomingGameServerPeer> entry in lists)
        //    {
        //        IncomingGameServerPeer targetpeer = entry.Value;
        //        if (targetpeer != null && targetpeer.Connected)
        //        {
        //            targetpeer.ZRPC.MasterToGameRPC.TongbaoCostBuffChange(targetpeer);
        //        }
        //        result = true;
        //    }

        //    ZRPC.MasterToGMRPC.GMResultBool(sessionid, result, peer);
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.GetLevelData)]
        //public void GetLevelData(string sessionid, PeerBase peer)
        //{            
        //    ZRPC.MasterToGMRPC.GMResultString(sessionid, LevelRepo.GetLevelDataString(), peer);
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.GMItemMallStatusUpdate)]
        //public void GMItemMallStatusUpdate(string sessionid, PeerBase peer)
        //{
        //    bool result = false;
        //    var lists = this.application.GetAllServerPeers();
        //    foreach (var entry in lists)
        //    {
        //        IncomingGameServerPeer targetpeer = entry.Value;
        //        if (targetpeer != null && targetpeer.Connected)
        //            targetpeer.ZRPC.MasterToGameRPC.GMItemMallStatusUpdate(targetpeer);
        //        result = true;
        //    }

        //    ZRPC.MasterToGMRPC.GMResultBool(sessionid, result, peer);
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.ChangeGuildLeader)]
        //public void ChangeGuildLeader(string sessionid, int guildid, string newleader, int serverid, PeerBase peer)
        //{
        //    IncomingGameServerPeer targetpeer = this.application.GetServerPeerById(serverid);
        //    if (targetpeer != null && targetpeer.Connected)
        //        targetpeer.ZRPC.MasterToGameRPC.ChangeGuildLeader(sessionid, guildid, newleader, targetpeer);
        //    else
        //        ZRPC.MasterToGMRPC.GMResultString(sessionid, "-1", peer);
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.NotifyActivityChange)]
        //public void NotifyActivityChange(string sessionid, PeerBase peer)
        //{
        //    bool result = false;
        //    var lists = this.application.GetAllServerPeers();
        //    foreach (var entry in lists)
        //    {
        //        IncomingGameServerPeer targetpeer = entry.Value;
        //        if (targetpeer != null && targetpeer.Connected)
        //        {
        //            targetpeer.ZRPC.MasterToGameRPC.NotifyActivityChange(targetpeer);
        //        }
        //        result = true;
        //    }

        //    ZRPC.MasterToGMRPC.GMResultBool(sessionid, result, peer);
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.GMAuctionChange)]
        //public void GMAuctionChange(string sessionid, PeerBase peer)
        //{
        //    bool result = false;
        //    var lists = this.application.GetAllServerPeers();
        //    foreach (var entry in lists)
        //    {
        //        IncomingGameServerPeer targetpeer = entry.Value;
        //        if (targetpeer != null && targetpeer.Connected)
        //        {
        //            targetpeer.ZRPC.MasterToGameRPC.GMAuctionChange(targetpeer);
        //        }
        //        result = true;
        //    }

        //    ZRPC.MasterToGMRPC.GMResultBool(sessionid, result, peer);
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.GMTopUpActivityUpdate)]
        //public void GMTopUpActivityUpdate(string sessionid, PeerBase peer)
        //{
        //    bool result = false;
        //    var lists = this.application.GetAllServerPeers();
        //    foreach (var entry in lists)
        //    {
        //        IncomingGameServerPeer targetpeer = entry.Value;
        //        if (targetpeer != null && targetpeer.Connected)
        //        {
        //            targetpeer.ZRPC.MasterToGameRPC.GMTopUpActivityUpdate(targetpeer);
        //        }
        //        result = true;
        //    }

        //    ZRPC.MasterToGMRPC.GMResultBool(sessionid, result, peer);
        //}


        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.ExchangeRateUpdate)]
        //public void ExchangeRateUpdate(string sessionid, PeerBase peer)
        //{
        //    bool result = false;
        //    var lists = this.application.GetAllServerPeers();
        //    foreach (var entry in lists)
        //    {
        //        IncomingGameServerPeer targetpeer = entry.Value;
        //        if (targetpeer != null && targetpeer.Connected)
        //        {
        //            targetpeer.ZRPC.MasterToGameRPC.ExchangeRateUpdate(targetpeer);
        //        }
        //        result = true;
        //    }

        //    ZRPC.MasterToGMRPC.GMResultBool(sessionid, result, peer);
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.GMGetArenaRank)]
        //public void GMGetArenaRank(string sessionid, string player, int serverid, PeerBase peer)
        //{
        //    IncomingGameServerPeer targetpeer = this.application.GetServerPeerById(serverid);
        //    if (targetpeer != null && targetpeer.Connected)
        //    {
        //        targetpeer.ZRPC.MasterToGameRPC.GMGetArenaRank(sessionid, player, targetpeer);
        //    }
        //    else
        //        ZRPC.MasterToGMRPC.GMResultString(sessionid, "-1", peer); //fail
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.GMResetArenaRank)]
        //public void GMResetArenaRank(string sessionid, string player, int serverid, PeerBase peer)
        //{
        //    IncomingGameServerPeer targetpeer = this.application.GetServerPeerById(serverid);
        //    if (targetpeer != null && targetpeer.Connected)
        //    {
        //        targetpeer.ZRPC.MasterToGameRPC.GMResetArenaRank(sessionid, player, targetpeer);
        //    }
        //    else
        //        ZRPC.MasterToGMRPC.GMResultBool(sessionid, false, peer); //fail
        //}

        //[RPCMethod(RPCCategory.MasterToGMRPC, (byte)GMMasterRPCMethods.GMNotifyTalentModifierChanged)]
        //public void GMNotifyTalentModifierChanged(string sessionid, PeerBase peer)
        //{
        //    bool result = false;
        //    var lists = this.application.GetAllServerPeers();
        //    foreach (var entry in lists)
        //    {
        //        IncomingGameServerPeer targetpeer = entry.Value;
        //        if (targetpeer != null && targetpeer.Connected)
        //        {
        //            targetpeer.ZRPC.MasterToGameRPC.GMNotifyTalentModifierChanged(targetpeer);
        //        }
        //        result = true;
        //    }

        //    ZRPC.MasterToGMRPC.GMResultBool(sessionid, result, peer);
        //}

        #endregion
    }
}