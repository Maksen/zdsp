namespace Zealot.Analytics.EventInfo
{
    using System.Collections.Generic;

    public class TestEvent : BaseEvent
    {
        // Limitations of Custom Events:
        //https://docs.unity3d.com/540/Documentation/Manual/UnityAnalyticsCustomEventScripting.html

        //Allowed data types: bool, string, int/float/long/etc
        //Default limit of 10 parameters per custom event.
        //Default limit of 500 characters for the dictionary content.
        //Default limit of 100 custom events per hour, per user.

        public int test_int { get; set; }

        public string test_string { get; set; }

        //public override void Init(Dictionary<string, object> dctEvent)
        //{
        //    if ((dctEvent.ContainsKey("test_int")) && (dctEvent.ContainsKey("test_string")))
        //    {
        //        name = "TestEvent";

        //        base.Init(dctEvent);
        //    }
        //}

        public override string GetEventName()
        {
            return "TestEvent";
        }

        public override Dictionary<string, object> GetEventParam()
        {
            Dictionary<string, object> param = base.GetEventParam();
            param.Add("test_int", test_int);
            param.Add("test_string", test_string);

            return param;
        }

        //public override string ToPipeDelimitedString()
        //{
        //    string result = string.Format("{0}|\"{1}\"", test_int, test_string);
        //    return result;
        //}
    }
}
