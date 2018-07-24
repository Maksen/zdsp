namespace Photon.LoadBalancing
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using Zealot.Common;
    #endregion

    public class GameServerStatus
    {
        public Dictionary<int, GMServerStatus> ServerListStatus; //serverid <- GMServerStatus
        public string serializeString = "";
        public DateTime lastQueryDT;

        public GameServerStatus()
        {
            ServerListStatus = new Dictionary<int, GMServerStatus>();
            serializeString = "";
            lastQueryDT = DateTime.Now.AddMinutes(-10);
        }

        public bool NeedRefreshData(bool force)
        {
            return force || (DateTime.Now - lastQueryDT).TotalMinutes > 9;
        }

        public string GetSerializeString(bool force)
        {
            DateTime now = DateTime.Now;
            if (force || (now - lastQueryDT).TotalMinutes > 9)
            {
                serializeString = JsonConvertDefaultSetting.SerializeObject(ServerListStatus);
                lastQueryDT = now;
            }         
            return serializeString;
        }
    }
}