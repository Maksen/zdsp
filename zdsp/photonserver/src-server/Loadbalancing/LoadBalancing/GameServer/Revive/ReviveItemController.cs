using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.Common.Datablock;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.Entities;

namespace Photon.LoadBalancing.GameServer
{
    public enum ReviveResult
    {
        Success,
        Failed
    }

    public class ReviveItemRequest
    {
        private int _requestID;
        private string _requestorName;
        private string _requesteeName;
        private int _itemID;

        public ReviveItemRequest(int requestId, string requestor, string requestee, int itemId)
        {
            _requestID = requestId;
            _requestorName = requestor;
            _requesteeName = requestee;
            _itemID = itemId;
        }

        public string GetRequestor()
        {
            return _requestorName;
        }

        public string GetRequestee()
        {
            return _requesteeName;
        }

        public int GetRequestID()
        {
            return _requestID;
        }

        public int GetItemID()
        {
            return _itemID;
        }

        public bool Contains(string name)
        {
            return _requestorName == name || _requesteeName == name;
        }
    }

    public class ReviveItemSession
    {
        private int                 _sessionID;
        private string              _requestorName;
        private string              _requesteeName;
        private int                 _itemID;
        //private ReviveItemInfoStats      _reviveItemInfoStats;
        //private ReviveItemInventoryData  _reviveItemInventoryData;
        private bool                _deductRequestSent;
        private bool                _deductConfirmed;
        private GameTimer           _deductTimer;
        private bool                _reviveRequestSent;
        private bool                _reviveConfirmed;
        private GameTimer           _reviveTimer;
        private bool                _complete;

        public ReviveItemSession(int sessionId, string requestor, string requestee, int itemId)
        {
            _sessionID          = sessionId;
            _requestorName      = requestor;
            _requesteeName      = requestee;
            _itemID             = itemId;
            //_reviveItemInfoStats     = new ReviveItemInfoStats();
            //_reviveItemInventoryData = new ReviveItemInventoryData();
            //_reviveItemInventoryData.InitDefault();
            _deductRequestSent  = false;
            _deductConfirmed    = false;
            _deductTimer        = null;
            _reviveRequestSent  = false;
            _reviveConfirmed    = false;
            _reviveTimer        = null;
            _complete           = false;
        }

        public string GetRequestor()
        {
            return _requestorName;
        }

        public string GetRequestee()
        {
            return _requesteeName;
        }

        public int GetSessionID()
        {
            return _sessionID;
        }

        public int GetItemID()
        {
            return _itemID;
        }

        public bool DeductRequestSent()
        {
            return _deductRequestSent;
        }

        public void SendDeductRequest()
        {
            _deductRequestSent = true;
        }

        public bool DeductItemConfirmed()
        {
            return _deductConfirmed;
        }

        public void ConfirmDeductItem()
        {
            _deductConfirmed = true;
            StopDeductTimer();
        }

        public GameTimer DeductTimer()
        {
            return _deductTimer;
        }

        public void StartDeductTimer(GameClientPeer peer)
        {
            if(_deductTimer == null)
            {
                _deductTimer = peer.mPlayer.mInstance.SetTimer(10000, (arg) =>
                {
                    //peer.DeductReviveItem(_sessionID, _itemID);
                    GameClientPeer requestorPeer = GameApplication.Instance.GetCharPeer(_requestorName);
                    GameClientPeer requesteePeer = GameApplication.Instance.GetCharPeer(_requesteeName);

                    if(requestorPeer != null)
                    {
                        requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_DeductItemTimedOut"), "", false, requestorPeer);
                    }
                    if(requesteePeer != null)
                    {
                        requesteePeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_DeductItemTimedOut"), "", false, requesteePeer);
                    }
                    StopDeductTimer();
                    GameApplication.Instance.ReviveItemController.CancelReviveItem(_sessionID);
                }, null);
            }
        }

        public void StopDeductTimer()
        {
            if (_deductTimer != null)
            {
                _deductTimer = null;
            }
        }

        public bool ReviveRequestSent()
        {
            return _reviveRequestSent;
        }

        public void SendReviveRequest()
        {
            _reviveRequestSent = true;
        }

        public bool ReviveConfirmed()
        {
            return _reviveConfirmed;
        }

        public void ConfirmRevive()
        {
            _reviveConfirmed = true;
            StopReviveTimer();
        }

        public GameTimer ReviveTimer()
        {
            return _reviveTimer;
        }

        public void StartReviveTimer(GameClientPeer peer)
        {
            if (_reviveTimer == null)
            {
                _reviveTimer = peer.mPlayer.mInstance.SetTimer(10000, (arg) =>
                {
                    //peer.RevivePlayer(_sessionID);
                    GameClientPeer requestorPeer = GameApplication.Instance.GetCharPeer(_requestorName);
                    GameClientPeer requesteePeer = GameApplication.Instance.GetCharPeer(_requesteeName);

                    if (requestorPeer != null)
                    {
                        requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_RespawnTimedOut"), "", false, requestorPeer);
                    }
                    if (requesteePeer != null)
                    {
                        requesteePeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_RespawnTimedOut"), "", false, requesteePeer);
                    }
                    StopReviveTimer();
                    GameApplication.Instance.ReviveItemController.CancelReviveItem(_sessionID);
                }, null);
            }
        }

        public void StopReviveTimer()
        {
            if (_reviveTimer != null)
            {
                _reviveTimer = null;
            }
        }

        public bool Contains(string name)
        {
            return _requestorName == name || _requesteeName == name;
        }

        public void Complete()
        {
            _complete = true;
        }

        public bool IsComplete()
        {
            return _deductConfirmed && _reviveConfirmed;
        }
    }

    public class ReviveItemController
    {
        private Dictionary<int, ReviveItemSession> _reviveItemSessions;
        private Dictionary<int, ReviveItemRequest> _reviveItemRequests;
        private List<int> _reviveItemSessionCleanup;
        private List<int> _reviveItemRequestCleanup;

        public ReviveItemController()
        {
            _reviveItemSessions = new Dictionary<int, ReviveItemSession>();
            _reviveItemRequests = new Dictionary<int, ReviveItemRequest>();
            _reviveItemSessionCleanup = new List<int>();
            _reviveItemRequestCleanup = new List<int>();
        }

        public ReviveItemSession GetSession(int sessionId)
        {
            return GetReviveItemSession(sessionId);
        }

        public void RequestReviveItem(string requestor, string requestee, int itemId)
        {
            GameClientPeer requestorPeer = GameApplication.Instance.GetCharPeer(requestor);
            GameClientPeer requesteePeer = GameApplication.Instance.GetCharPeer(requestee);
            if (ReviveItemRequestExists(requestor))
            {
                requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_PlayerAlreadyRequesting"), "", false, requestorPeer);
                return;
            }
            if (ReviveItemRequestExists(requestee))
            {
                requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_RequesteeAlreadyRequesting"), "", false, requestorPeer);
                return;
            }
            if (ReviveItemSessionExists(requestee))
            {
                requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_AlreadyReviving"), "", false, requestorPeer);
                return;
            }

            if (requestorPeer == null || requesteePeer == null)
            {
                // One or both players have disconnected!
                if (requestorPeer != null)
                {
                    requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_OtherPlayerDisconnected"), "", false, requestorPeer);
                }
                if (requesteePeer != null)
                {
                    requesteePeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_OtherPlayerDisconnected"), "", false, requesteePeer);
                }

                return;
            }

            Player requestorPlayer = requestorPeer.mPlayer;
            Player requesteePlayer = requesteePeer.mPlayer;

            if (requestorPlayer == null || requesteePlayer == null)
            {
                // One or both players have disconnected!
                if (requestorPlayer != null)
                {
                    requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_OtherPlayerDisconnected"), "", false, requestorPeer);
                }
                if (requesteePlayer != null)
                {
                    requesteePeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_OtherPlayerDisconnected"), "", false, requesteePeer);
                }

                return;
            }

            float distance = (requestorPlayer.Position - requesteePlayer.Position).magnitude;
            float maxDist = 0f;
            float.TryParse(GameConstantRepo.GetConstant("ReviveDistance"), out maxDist);

            if (requestorPlayer.mInstance != requesteePlayer.mInstance)
            {
                requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_RequesteeNotInSameMap"), "", false, requestorPeer);
                requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_RequestorNotInSameMap"), "", false, requestorPeer);

                return;
            }
            else if (distance > maxDist)
            {
                requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_RequesteeTooFar"), "", false, requestorPeer);

                return;
            }

            int newRequestId = _reviveItemRequests.Count + 1;
            ReviveItemRequest newReviveItemRequest = new ReviveItemRequest(newRequestId, requestor, requestee, itemId);
            _reviveItemRequests.Add(newRequestId, newReviveItemRequest);

            SendRquest(newRequestId, requestee, requestor);
        }

        public void AcceptReviveItemRequest(int requestId)
        {
            ReviveItemRequest request = GetReviveItemRequest(requestId);
            if (request != null)
            {
                int sessionId = StartSession(request.GetRequestor(), request.GetRequestee(), request.GetItemID());
                GameClientPeer requestorPeer = GameApplication.Instance.GetCharPeer(request.GetRequestor());
                GameClientPeer requesteePeer = GameApplication.Instance.GetCharPeer(request.GetRequestee());

                if (requestorPeer == null || requesteePeer == null)
                {
                    // One or both players have disconnected!
                    if (requestorPeer != null)
                    {
                        requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_OtherPlayerDisconnected"), "", false, requestorPeer);
                    }
                    if (requesteePeer != null)
                    {
                        requesteePeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_OtherPlayerDisconnected"), "", false, requesteePeer);
                    }

                    return;
                }

                Player requestorPlayer = requestorPeer.mPlayer;
                Player requesteePlayer = requesteePeer.mPlayer;

                if (requestorPlayer == null || requesteePlayer == null)
                {
                    // One or both players have disconnected!
                    if (requestorPlayer != null)
                    {
                        requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_OtherPlayerDisconnected"), "", false, requestorPeer);
                    }
                    if (requesteePlayer != null)
                    {
                        requesteePeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_OtherPlayerDisconnected"), "", false, requesteePeer);
                    }

                    return;
                }

                float distance = (requestorPlayer.Position - requesteePlayer.Position).magnitude;
                float maxDist = 0f;
                float.TryParse(GameConstantRepo.GetConstant("ReviveDistance"), out maxDist);

                if (requestorPlayer.mInstance != requesteePlayer.mInstance)
                {
                    requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_RequesteeNotInSameMap"), "", false, requestorPeer);
                    requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_RequestorNotInSameMap"), "", false, requestorPeer);

                    return;
                }
                else if (distance > maxDist)
                {
                    requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_RequesteeTooFar"), "", false, requestorPeer);

                    return;
                }

                //requestorPeer.ConfirmAcceptReviveItem(sessionId, request.GetRequestee(), "requestor");
                //requesteePeer.ConfirmAcceptReviveItem(sessionId, request.GetRequestor(), "requestee");

                EndRequest(requestId);
            }
        }

        public void RejectReviveItemRequest(int requestId)
        {
            ReviveItemRequest request = GetReviveItemRequest(requestId);
            if (request != null)
            {
                GameClientPeer requestorPeer = GameApplication.Instance.GetCharPeer(request.GetRequestor());
                requestorPeer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("reviveItem_Rejected"), "", false, requestorPeer);

                EndRequest(requestId);
            }
        }

        private void CancelReviveItemRequest(int requestId)
        {
            ReviveItemRequest request = GetReviveItemRequest(requestId);

            if (request == null)
            {
                // ReviveItem Session not found!
                return;
            }

            GameClientPeer requestorPeer = GameApplication.Instance.GetCharPeer(request.GetRequestor());
            GameClientPeer requesteePeer = GameApplication.Instance.GetCharPeer(request.GetRequestee());
            if (requestorPeer != null)
            {
                requestorPeer.ConfirmCancelReviveItem();
            }
            if (requesteePeer != null)
            {
                requesteePeer.ConfirmCancelReviveItem();
            }

            EndRequest(request.GetRequestID());
        }

        public void CancelReviveItem(int sessionId)
        {
            ReviveItemSession session = GetReviveItemSession(sessionId);

            if (session == null)
            {
                // ReviveItem Session not found!
                return;
            }

            GameClientPeer requestorPeer = GameApplication.Instance.GetCharPeer(session.GetRequestor());
            GameClientPeer requesteePeer = GameApplication.Instance.GetCharPeer(session.GetRequestee());
            if (requestorPeer != null)
            {
                requestorPeer.ConfirmCancelReviveItem();
            }
            if (requesteePeer != null)
            {
                requesteePeer.ConfirmCancelReviveItem();
            }

            EndSession(session);
        }

        public void ConfirmItemDeducted(int sessionId)
        {
            ReviveItemSession session = GetReviveItemSession(sessionId);

            if (session == null)
            {
                // No such reviveItem session exists!
                return;
            }

            session.ConfirmDeductItem();
        }

        public void ConfirmRevived(int sessionId)
        {
            ReviveItemSession session = GetReviveItemSession(sessionId);

            if (session == null)
            {
                // No such reviveItem session exists!
                return;
            }

            session.ConfirmRevive();
        }

        public void UpdateReviveItemConfirmed(int sessionId, string playerName, bool isConfirmed)
        {
            //ReviveItemSession session = GetReviveItemSession(sessionId);

            //if (session == null)
            //{
            //    // No such reviveItem session exists!
            //    return;
            //}

            //if (playerName == session.GetRequestor())
            //{
            //    session.UpdateRequestorConfirm(isConfirmed);
            //}
            //else if (playerName == session.GetRequestee())
            //{
            //    session.UpdateRequesteeConfirm(isConfirmed);
            //}
        }

        public void Update()
        {
            lock (_reviveItemRequests)
            {
                foreach (KeyValuePair<int, ReviveItemRequest> entry in _reviveItemRequests)
                {
                    ReviveItemRequest request = entry.Value;

                    GameClientPeer requestorPeer = GameApplication.Instance.GetCharPeer(request.GetRequestor());
                    GameClientPeer requesteePeer = GameApplication.Instance.GetCharPeer(request.GetRequestee());

                    // Case: When one or both players have disconnected
                    if (requestorPeer == null || requesteePeer == null)
                    {
                        CancelReviveItemRequest(request.GetRequestID());

                        continue;
                    }
                }
            }

            lock (_reviveItemSessions)
            {
                foreach (KeyValuePair<int, ReviveItemSession> entry in _reviveItemSessions)
                {
                    ReviveItemSession session = entry.Value;

                    GameClientPeer requestorPeer = GameApplication.Instance.GetCharPeer(session.GetRequestor());
                    GameClientPeer requesteePeer = GameApplication.Instance.GetCharPeer(session.GetRequestee());

                    // Case: When one or both players have disconnected
                    if (requestorPeer == null || requesteePeer == null)
                    {
                        CancelReviveItem(session.GetSessionID());

                        continue;
                    }

                    if (session.DeductRequestSent() == false)
                    {
                        session.StartDeductTimer(requestorPeer);
                        session.SendDeductRequest();
                        requestorPeer.DeductReviveItem(session.GetSessionID(), session.GetItemID());
                    }
                    else
                    {
                        if (session.DeductItemConfirmed() == true && session.DeductTimer() != null)
                        {
                            session.StopDeductTimer();
                        }
                    }

                    if (session.DeductItemConfirmed() == true)
                    {
                        if (session.ReviveRequestSent() == false)
                        {
                            session.StartReviveTimer(requesteePeer);
                            session.SendReviveRequest();
                            requesteePeer.RevivePlayer(session.GetSessionID());
                        }
                        else
                        {
                            if (session.ReviveConfirmed() == true && session.ReviveTimer() != null)
                            {
                                session.StopReviveTimer();
                            }
                        }
                    }

                    if(session.IsComplete())
                    {
                        EndSession(session);
                    }
                }

                // Cleanup
                for (int i = 0; i < _reviveItemSessionCleanup.Count; ++i)
                {
                    ReviveItemSession session = null;
                    int sessionId = _reviveItemSessionCleanup[i];

                    if (_reviveItemSessions.ContainsKey(sessionId))
                    {
                        session = _reviveItemSessions[sessionId];
                    }

                    if (session != null && session.IsComplete())
                    {
                        _reviveItemSessions.Remove(sessionId);
                    }
                }

                _reviveItemSessionCleanup.Clear();

                for (int i = 0; i < _reviveItemRequestCleanup.Count; ++i)
                {
                    ReviveItemRequest request = null;
                    int requestId = _reviveItemRequestCleanup[i];

                    if (_reviveItemRequests.ContainsKey(requestId))
                    {
                        request = _reviveItemRequests[requestId];
                    }

                    if (request != null)
                    {
                        _reviveItemRequests.Remove(requestId);
                    }
                }

                _reviveItemRequestCleanup.Clear();
            }
        }

        private void SendRquest(int requestId, string requestee, string requestor)
        {
            GameClientPeer requesteePeer = GameApplication.Instance.GetCharPeer(requestee);
            if (requesteePeer != null)
            {
                requesteePeer.RequestReviveItem(requestor, requestId);
            }
        }

        private int StartSession(string requestor, string requestee, int itemId)
        {
            int newSessionId = _reviveItemSessions.Count + 1;
            ReviveItemSession newReviveItemSession = new ReviveItemSession(newSessionId, requestor, requestee, itemId);
            _reviveItemSessions.Add(newSessionId, newReviveItemSession);

            return newSessionId;
        }

        private void EndSession(ReviveItemSession session)
        {
            session.Complete();
            _reviveItemSessionCleanup.Add(session.GetSessionID());
        }

        private void EndRequest(int requestId)
        {
            _reviveItemRequestCleanup.Add(requestId);
        }

        private ReviveItemRequest GetReviveItemRequest(int requestId)
        {
            ReviveItemRequest reviveItemRequest = null;
            if (_reviveItemRequests.ContainsKey(requestId))
            {
                reviveItemRequest = _reviveItemRequests[requestId];
            }

            // ReviveItem Session not found!
            return reviveItemRequest;
        }

        private bool ReviveItemRequestExists(string name)
        {
            foreach (KeyValuePair<int, ReviveItemRequest> entry in _reviveItemRequests)
            {
                if (entry.Value.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ReviveItemSessionExists(string name)
        {
            foreach (KeyValuePair<int, ReviveItemSession> entry in _reviveItemSessions)
            {
                if (entry.Value.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }

        private ReviveItemSession GetReviveItemSession(int sessionId)
        {
            ReviveItemSession reviveItemSession = null;
            if (_reviveItemSessions.ContainsKey(sessionId))
            {
                reviveItemSession = _reviveItemSessions[sessionId];
            }

            // ReviveItem Session not found!
            return reviveItemSession;
        }

        private void ConfirmCompleteReviveItem(ReviveItemSession session)
        {
            GameClientPeer requestorPeer = GameApplication.Instance.GetCharPeer(session.GetRequestor());
            GameClientPeer requesteePeer = GameApplication.Instance.GetCharPeer(session.GetRequestee());

            //session.CompleteReviveItem(requestorPeer, requesteePeer);

            requestorPeer.ConfirmCompleteReviveItem();
            requesteePeer.ConfirmCompleteReviveItem();

            EndSession(session);
        }
    }
}
