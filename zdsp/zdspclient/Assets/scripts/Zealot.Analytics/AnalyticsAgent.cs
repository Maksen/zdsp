namespace Zealot.Analytics
{
    using System.Collections.Generic;
    using UnityEngine.Analytics;
    using EventInfo;

    public static class AnalyticsAgent
    {
        private static string serverId;
        private static string userCharId;

        //public static void Log<T>(Dictionary<string, object> dctEvent) where T : BaseEvent, new()
        //{
        //    T eventInfo = new T();
        //    eventInfo.Init(dctEvent);

        //    Analytics.CustomEvent(eventInfo.name, eventInfo.param);
        //}

        public static void LogEvent(BaseEvent eventObj)
        {
            eventObj.serverId = serverId;
            eventObj.userCharId = userCharId;

            AnalyticsResult analyticsResult = Analytics.CustomEvent(eventObj.GetEventName(), eventObj.GetEventParam());
        }

        public static void LoginUser(string userId)
        {
            Analytics.SetUserId(userId);
        }

        public static void SetServerId(string serverId)
        {
            AnalyticsAgent.serverId = serverId;
        }

        public static void SetUserCharId(string userCharId)
        {
            AnalyticsAgent.userCharId = userCharId;
        }
    }
}
