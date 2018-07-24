using System;
using System.Text;
using System.Collections.Generic;
using System.Timers;
using System.Globalization;
using Photon.LoadBalancing.GameServer;

namespace Zealot.Server.Rules
{
    public class SystemMessageData
    {
        public int id;
        public string message;
        public string color;
        public DateTime startdt;
        public DateTime enddt;
        public int interval;
        public int repeat;
        public int type;
        public Timer timer;
        public int current;

        public SystemMessageData(int id, string message, string color, DateTime startdt, DateTime enddt, int interval, int repeat, int type)
        {
            this.id = id;
            this.message = message;
            this.color = color;
            this.startdt = startdt;
            this.enddt = enddt;
            this.interval = interval;
            this.repeat = repeat;
            this.type = type;
            timer = null;
            current = 0;
        }
    }

    public static class SystemMessageRules
    {
        private enum MessageColorList
        {
            Black = 0,
            Blue = 1,
            Cyan = 2,
            Green = 3,
            Gold = 4,
            Indigo = 5,
            Orange = 6,
            Pink = 7,
            Red = 8,
            White = 9,
            Yellow = 10,
        }

        public static Dictionary<int, SystemMessageData> mMessageList;

        public static void Init()
        {
            mMessageList = new Dictionary<int, SystemMessageData>();
            List<Dictionary<string, object>> messages = GameApplication.dbGM.SystemMessageRepo.GetMessage(GameApplication.Instance.GetMyServerId());
            if (messages.Count > 0)
            {
                foreach(Dictionary<string , object> message in messages)
                {
                    int id = (int)message["ID"];
                    string color = ((MessageColorList)message["MessageColor"]).ToString();
                    string messagetext = (string)message["Message"];
                    DateTime startdt = Convert.ToDateTime(message["BroadcastDT"].ToString());
                    int interval = (int)message["Interval"];
                    int repeat = (int)message["Repeat"];
                    int minutes = repeat * interval;
                    DateTime enddt = Convert.ToDateTime(message["EndDt"].ToString());
                    int type = (int)message["MessageType"];

                    if (Convert.ToDateTime(enddt) > DateTime.Now)
                    {
                        SystemMessageData newdata = new SystemMessageData(id, messagetext, color, startdt, enddt, interval, repeat, type);
                        mMessageList.Add(id, newdata);
                        UpdateMessageTimer(id);
                    }
                }
            }
        }

        private static void UpdateMessageTimer(int id)
        {
            if (!mMessageList.ContainsKey(id))
                return;

            SystemMessageData msgdata = mMessageList[id];
            if (msgdata.current > msgdata.repeat)
            {
                RemoveMessage(msgdata.id);
            }
            else
            {
                for(int i=msgdata.current;i<=msgdata.repeat;i++)
                {
                    DateTime broadcasttime = msgdata.startdt.AddMinutes(msgdata.interval * i);
                    if (broadcasttime < DateTime.Now)
                        continue;

                    msgdata.current = i;
                    double ms = (broadcasttime - DateTime.Now).TotalMilliseconds;
                    Timer timer = new Timer(ms);
                    timer.Elapsed += delegate { BroadcastMessage(msgdata.id); };
                    timer.AutoReset = false;
                    timer.Start();
                    msgdata.timer = timer;
                    break;
                }
            }
        }

        public static void AddMessage(int id, string message, string color, string startdt, string enddt, int interval, int repeat, int type)
        {
            DateTime sdt = DateTime.ParseExact(startdt, "dd/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
            DateTime edt = DateTime.ParseExact(enddt, "dd/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
            SystemMessageData newdata = new SystemMessageData(id, message, color, sdt, edt, interval, repeat, type);
            if (mMessageList.ContainsKey(id))
                mMessageList[id] = newdata;
            else
                mMessageList.Add(id, newdata);

            BroadcastMessage(id);
        }

        public static void RemoveMessage(int id)
        {
            SystemMessageData data = null;
            if (mMessageList.TryGetValue(id, out data))
            {
                mMessageList.Remove(id);
            }
        }

        private static void BroadcastMessage(int id)
        {
            if (!mMessageList.ContainsKey(id))
                return;

            SystemMessageData msgdata = mMessageList[id];
            if (msgdata.timer != null)
            {
                msgdata.timer.Stop();
                msgdata.timer = null;
            }
            

            StringBuilder sb = new StringBuilder();
            sb.Append("<color=");
            sb.Append(GetColorByName(msgdata.color));
            sb.Append(">");
            sb.Append(msgdata.message);
            sb.Append("</color>");
            
            //GameApplication.Instance.BroadcastMessage(msgdata.type == 0 ? BroadcastMessageType.GMSystemMessage : BroadcastMessageType.EmergencySystemMessage, sb.ToString());
            msgdata.current += 1;
            UpdateMessageTimer(id);
        }

        private static string GetColorByName(string color)
        {
            switch (color)
            {
                case "Black":
                    return "#000000";
                case "Blue":
                    return "#0000FF";
                case "Cyan":
                    return "#00FFFF";
                case "Green":
                    return "#008000";
                case "Gold":
                    return "#FFD700";
                case "Indigo":
                    return "#4B0082";
                case "Orange":
                    return "#FFA500";
                case "Pink":
                    return "#FFC0CB";
                case "Red":
                    return "#FF0000";
                case "White":
                    return "#FFFFFF";
                case "Yellow":
                    return "#FFFF00";
                default:
                    return "#000000";
            }
        }
    }
}
