namespace Photon.LoadBalancing.ServerToServer
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using ExitGames.Logging;
    using Photon.SocketServer;

    public abstract class ServerConnectionBase : IDisposable
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public readonly ApplicationBase Application;
        public readonly string Address;
        public readonly int Port;
        private readonly int connectRetryIntervalSeconds;
        private byte isReconnecting;
        private Timer reconnectTimer;

        private OutgoingServerPeer peer;
        private string ApplicationId = "";

        protected ServerConnectionBase(ApplicationBase controller, string address, int port, int connectRetryIntervalSeconds, string ApplicationId)
        {
            this.Application = controller;
            this.Address = address;
            this.Port = port;
            this.connectRetryIntervalSeconds = connectRetryIntervalSeconds;
            this.ApplicationId = ApplicationId;
        }

        public IPEndPoint EndPoint { get; private set; }

        public OutgoingServerPeer GetPeer()
        {
            return this.peer;
        }

        public abstract OutgoingServerPeer CreateServerPeer();

        public void Initialize()
        {
            this.Connect();
        }

        public SendResult SendEventIfRegistered(IEventData eventData, SendParameters sendParameters)
        {
            var masterPeer = this.peer;
            if (masterPeer == null || masterPeer.IsRegistered == false)
            {
                return SendResult.Disconnected;
            }

            return masterPeer.SendEvent(eventData, sendParameters);
        }

        public SendResult SendEvent(IEventData eventData, SendParameters sendParameters)
        {
            var masterPeer = this.peer;
            if (masterPeer == null || masterPeer.Connected == false)
            {
                return SendResult.Disconnected;
            }

            return masterPeer.SendEvent(eventData, sendParameters);
        }

        public void Connect()
        {
            if (this.reconnectTimer != null)
            {
                this.reconnectTimer.Dispose();
                this.reconnectTimer = null;
            }

            // check if the photon application is shuting down
            if (this.Application.Running == false)
            {
                return;
            }

            try
            {
                this.UpdateEndpoint();
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("MasterServer endpoint for address {0} updated to {1}", this.Address, this.EndPoint);
                }

                this.Connect(this.EndPoint);
            }
            catch(Exception e)
            {
                log.Error(e);
                if (this.isReconnecting == 1)
                {
                    this.Reconnect();
                }
                else
                {
                    throw;
                }
            }
        }

        public void Reconnect()
        {
            if (this.Application.Running == false)
            {
                return;
            }

            Thread.VolatileWrite(ref this.isReconnecting, 1);
            this.reconnectTimer = new Timer(o => this.Connect(), null, this.connectRetryIntervalSeconds * 1000, System.Threading.Timeout.Infinite);
        }

        public void Connect(IPEndPoint endPoint)
        {
            if (this.Application.Running == false)
            {
                return;
            }
            if (this.peer == null)
            {
                this.peer = this.CreateServerPeer();
            }
            if (this.peer.ConnectTcp(endPoint, ApplicationId))
            {
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat("Connecting to server at {0}", endPoint);
                }
            }
            else
            {
                log.WarnFormat("connection refused - is the process shutting down ?");
            }
        }

        public void UpdateEndpoint()
        {
            IPAddress masterAddress;
            if (!IPAddress.TryParse(this.Address, out masterAddress))
            {
                var hostEntry = Dns.GetHostEntry(this.Address);
                if (hostEntry.AddressList == null || hostEntry.AddressList.Length == 0)
                {
                    throw new ExitGames.Configuration.ConfigurationException(
                        "MasterIPAddress setting is neither an IP nor an DNS entry: " + this.Address);
                }

                masterAddress = hostEntry.AddressList.First(address => address.AddressFamily == AddressFamily.InterNetwork);

                if (masterAddress == null)
                {
                    throw new ExitGames.Configuration.ConfigurationException(
                        "MasterIPAddress does not resolve to an IPv4 address! Found: "
                        + string.Join(", ", hostEntry.AddressList.Select(a => a.ToString()).ToArray()));
                }
            }

            this.EndPoint = new IPEndPoint(masterAddress, this.Port);
        }

        public void OnConnectionEstablished(object responseObject)
        {
            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Master connection established: address:{0}", this.Address);
            }
            Thread.VolatileWrite(ref this.isReconnecting, 0);
        }

        public void OnConnectionFailed(int errorCode, string errorMessage) 
        {
            if (this.isReconnecting == 0)
            {
                log.ErrorFormat(
                    "Master connection failed: address={0}, errorCode={1}, msg={2}", 
                    this.EndPoint,
                    errorCode, 
                    errorMessage);
            }
            else if (log.IsWarnEnabled)
            {
                log.WarnFormat(
                    "Master connection failed: address={0}, errorCode={1}, msg={2}",
                    this.EndPoint,
                    errorCode,
                    errorMessage);
            }

            this.Reconnect();
        }

        public void Dispose()
        {
            var timer = this.reconnectTimer;
            if (timer != null)
            {
                timer.Dispose();
                this.reconnectTimer = null;
            }

            if (peer != null)
            {
                peer.Disconnect();
                peer.Dispose();
                peer = null;
            }
        }
    }
}
