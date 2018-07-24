using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

namespace Zealot
{
    public class Msg
    {
        public string id;
        public string msg;
        public string logtype;
    }

    public class RemoteLog : MonoSingleton<RemoteLog>
    {
        private WebSocket _wocketSocket = null;
        private int mainThreadID = -1;
        private bool _isConnect = false;
        private string _id = "";
        private string _url = "192.168.11.135:5478";

        void Start()
        {
            this.mainThreadID = Thread.CurrentThread.ManagedThreadId;
        }

        void Enable()
        {
            Instance.gameObject.SetActive(true);
        }

        void Disable()
        {
            Instance.gameObject.SetActive(false);
        }

        public static void EnableRemoteLog()
        {
            RemoteLog.Instance.Enable();
        }

        public static void DisableRemoteLog()
        {
            RemoteLog.Instance.Disable();
        }
        public void Close()
        {
            if (_wocketSocket != null && _isConnect)
            {
                _wocketSocket.Close();
            }
        }
        public string LocalIPAddress()
        {

            string localIP = "";
            try
            {
                IPHostEntry host;
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("RemoteLog error msg: " + e.ToString());
            }
            return localIP;
        }
        public static void Disconnect()
        {
            RemoteLog.Instance.Close();
        }
        public IEnumerator Connect(string url)
        {
            _url = string.Format("ws://{0}/Log", url);
            _isConnect = false;
            _id = (LocalIPAddress() != "") ? LocalIPAddress() + "_" + SystemInfo.deviceUniqueIdentifier : SystemInfo.deviceUniqueIdentifier;
            if (_wocketSocket == null)
            {
                _wocketSocket = new WebSocket(new Uri(_url));
            }
            yield return StartCoroutine(_wocketSocket.Connect());
            _isConnect = true;
        }

        public static void ConnectToServer(string url)
        {
            if (url != string.Empty)
            {
                RemoteLog.Instance.StartCoroutine(RemoteLog.Instance.Connect(url));
            }
        }

        public static RemoteLog SetFileID(string fileid)
        {
            RemoteLog.Instance.UpdateFileID(fileid);
            return RemoteLog.Instance;
        }
        public void UpdateFileID(string fileid = "")
        {
            _id = (LocalIPAddress() != "") ? LocalIPAddress() + "_" + SystemInfo.deviceUniqueIdentifier : SystemInfo.deviceUniqueIdentifier;
            if (fileid != "")
            {
                _id += "_" + fileid;
            }
        }

        void Update()
        {
            if (_wocketSocket != null)
            {
                if (_isConnect && _wocketSocket.error != null)
                {
                    _isConnect = false;
                    _wocketSocket.Close();
                    return;
                }
            }
            if (!Application.isPlaying)
            {
                if (_isConnect)
                {
                    _wocketSocket.Close();
                }
            }
        }

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
            Application.logMessageReceivedThreaded += HandleLogThreadCallback;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
            Application.logMessageReceivedThreaded -= HandleLogThreadCallback;
        }

        void HandleLog(string log, string track, LogType type)
        {
            Output(log, track, type);
        }

        void HandleLogThreadCallback(string log, string track, LogType type)
        {
            if (this.mainThreadID != Thread.CurrentThread.ManagedThreadId)
            {
                Output(log, track, type);
            }
        }

        private void Output(string log, string track, LogType type)
        {
            string logtype = type.ToString().ToLower();
            string text = string.Format("{0} {1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log);
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                if (GameInfo.gLocalPlayer != null)
                {
                    //
                    text += string.Format("\n{0} ,log_playername: {1} ,Top WindowType: {2} ,ServerType: {3}",
                        track, GameInfo.gLocalPlayer.Name, UIManager.GetTopWindowType(), PhotonNetwork.PhotonServerSettings.LoginServerType);
                    //text += "\n" + track + ", log_playername: " + GameInfo.gLocalPlayer.Name + ", Top WindowType: " + UIManager.GetTopWindowType();
                }

                else
                    text += "\n" + track;
            }

            if (_wocketSocket != null && _isConnect)
            {
                _wocketSocket.SendString(JsonUtility.ToJson(new Msg() { id = _id, msg = text, logtype = logtype }));
            }
        }
    }
}
