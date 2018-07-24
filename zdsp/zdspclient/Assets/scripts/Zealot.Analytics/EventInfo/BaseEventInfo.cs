namespace Zealot.Analytics.EventInfo
{
    using System.Collections.Generic;

    public interface IZealotAnalytics
    {
        //void Init(Dictionary<string, object> dctEvent);

        string GetEventName();

        Dictionary<string, object> GetEventParam();

        //string ToPipeDelimitedString();
    }

    public class BaseEvent : IZealotAnalytics
    {
        public string serverId { get; set; }

        public string userCharId { get; set; }

        //public Dictionary<string, object> param { get; set; }

        //public virtual void Init(Dictionary<string, object> dctEvent)
        //{
        //    param = dctEvent;
        //}

        public virtual string GetEventName()
        {
            return "BaseEvent";
        }

        public virtual Dictionary<string, object> GetEventParam()
        {
            return new Dictionary<string, object>()
            {
                {"serverId",  serverId},
                {"userCharId",  userCharId}
            };
        }

        //public virtual string ToPipeDelimitedString()
        //{
        //    return null;
        //}
    }
}
