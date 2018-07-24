using Zealot.Entities;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;

namespace Zealot.Server.Entities
{
    public class MessageBroadcaster : IServerEntity
    {
        public MessageBroadcasterJson mPropertyInfos;
        public GameLogic mInstance;

        public MessageBroadcaster(MessageBroadcasterJson info, GameLogic instance)
        {
            mPropertyInfos = info;
            mInstance = instance;
        }

        public ServerEntityJson GetPropertyInfos()
        {
            return mPropertyInfos;
        }

        public void InstanceStartUp()
        {           
        }

        public void SendMessage(string message, object[] parameters)
        {
            if (message == "")
                return;

            string _parameters = string.Format("{0};{1}", mPropertyInfos.emergency ? 1 : 0, message);
            switch (mPropertyInfos.type)
            {
                case 0: //world
                    GameApplication.Instance.BroadcastMessage(BroadcastMessageType.MessageBroadcaster, _parameters);
                    break;
                case 1: //room
                    mInstance.ZRPC.CombatRPC.BroadcastMessageToClient((byte)BroadcastMessageType.MessageBroadcaster, _parameters, mInstance.mRoom);
                    break;
                case 2: //personal
                    if (parameters != null && parameters.Length >= 1)
                    {
                        Player _player = parameters[0] as Player;
                        if (_player != null)
                        {
                            GameClientPeer _peer = _player.Slot;
                            _peer.ZRPC.CombatRPC.BroadcastMessageToClient((byte)BroadcastMessageType.MessageBroadcaster, _parameters, _peer);
                        }
                    }
                    break;
            }
        }

        #region Triggers
        public void SendMessage1(IServerEntity sender, object[] parameters = null)
        {
            if(mPropertyInfos.messages.Length >= 1)
                SendMessage(mPropertyInfos.messages[0], parameters);  
        }

        public void SendMessage2(IServerEntity sender, object[] parameters = null)
        {
            if (mPropertyInfos.messages.Length >= 2)
                SendMessage(mPropertyInfos.messages[1], parameters); 
        }

        public void SendMessage3(IServerEntity sender, object[] parameters = null)
        {
            if (mPropertyInfos.messages.Length >= 3)
                SendMessage(mPropertyInfos.messages[2], parameters); 
        }

        public void SendMessage4(IServerEntity sender, object[] parameters = null)
        {
            if (mPropertyInfos.messages.Length >= 4)
                SendMessage(mPropertyInfos.messages[3], parameters); 
        }

        public void SendMessage5(IServerEntity sender, object[] parameters = null)
        {
            if (mPropertyInfos.messages.Length >= 5)
                SendMessage(mPropertyInfos.messages[4], parameters); 
        }
        #endregion
    }
}
