using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Zealot.Common
{
    public enum GMMessageType
    {
        Continuous,
        Interval,
        Timing
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class GMMessageData
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "a")]
        public int id;

        [DefaultValue(GMMessageType.Continuous)]
        [JsonProperty(PropertyName = "b")]
        public GMMessageType type;

        [JsonProperty(PropertyName = "c")]
        public DateTime start;

        [JsonProperty(PropertyName = "d")]
        public DateTime end;

        [DefaultValue("")]
        [JsonProperty(PropertyName = "e")]
        public string week;

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "f")]
        public int hour;

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "g")]
        public int min;

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "h")]
        public int interval;

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "i")]
        public int duration;

        [DefaultValue("")]
        [JsonProperty(PropertyName = "j")]
        public string message;

        public string server;

        [JsonProperty(PropertyName = "k")]
        public DateTime updatetime;

        public GMMessageData()
        {
        }

        public GMMessageData(Dictionary<string, object> data)
        {
            id = (int)data["id"];
            type = (GMMessageType)data["type"];
            start = (DateTime)data["start"];
            end = (DateTime)data["end"];
            week = (string)data["week"];
            hour = (int)data["hour"];
            min = (int)data["min"];
            interval = (int)data["interval"];
            duration = (int)data["duration"];
            message = string.Intern((string)data["message"]);
            server = (string)data["server"];
            updatetime = (DateTime)data["updatetime"];
        }

        public bool IsLive()
        {
            return DateTime.Now >= start && DateTime.Now < end;
        }

        public bool IsStandby()
        {
            return DateTime.Now < start;
        }

        public bool IsExpired()
        {
            return start < DateTime.Now && end < DateTime.Now;
        }

        public bool isActive { get; private set; }

        public void CheckIsActive()
        {
            if (type == GMMessageType.Timing)
            {
                if (week == "8")
                {
                    isActive = true;
                    return;
                }
                else
                {
                    //check Monday(1) ~ Sunday(7)
                    int i;
                    var days = new HashSet<int>(week.Split(';').Where(e => int.TryParse(e, out i)).Select(e => int.Parse(e)));
                    int day = (int)DateTime.Now.DayOfWeek;
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                        day = 7;

                    isActive = days.Contains(day);
                    return;
                }
            }

            isActive = true;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static GMMessageData Deserialize(string message)
        {
            return JsonConvert.DeserializeObject<GMMessageData>(message);
        }
    }
}