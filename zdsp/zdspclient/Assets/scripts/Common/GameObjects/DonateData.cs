using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Zealot.Common
{
    public class DonateOrderData
    {
        [JsonProperty(PropertyName = "Guid")]
        public string Guid { get; set; }

        [JsonProperty(PropertyName = "Id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "Count")]
        public int Count { get; set; }

        public DateTime ActivatedDT { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class DonateInventoryData
    {
        [JsonProperty(PropertyName = "DonateData")]
        public string DonateData { get; set; }

        [JsonProperty(PropertyName = "LastUpdated")]
        public string LastUpdated { get; set; }

        public DonateInventoryData()
        {
            DonateData = "";
            LastUpdated = "";
        }

        public void SerializeData(List<DonateOrderData> datalist)
        {
            DonateData = JsonConvertDefaultSetting.SerializeObject(datalist);
        }

        public void SerializeLastUpdated(DateTime dateTime)
        {
            LastUpdated = dateTime.ToString("yyyy/MM/dd/HH");
        }
    }
}
