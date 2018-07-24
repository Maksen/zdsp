using Photon.SocketServer;

namespace Zealot.Server.EventMessage
{
    public class RPCBroadcastData
    {
        public EventData EventData { get; private set; }
        public SendParameters SendParameters { get; private set; }

        public RPCBroadcastData(EventData eventData, SendParameters sendparam)
        {
            EventData = eventData;
            SendParameters = sendparam;
        }
    }
}
