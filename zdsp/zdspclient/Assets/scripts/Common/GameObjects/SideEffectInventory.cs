using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SideEffectDBInfo
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "0")]
        public int SEID { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "1")]
        public long Elapsed { get; set; }

        public SideEffectDBInfo(int seid, long elapsed)
        {
            SEID = seid;
            Elapsed = elapsed;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SideEffectInventoryData
    {
        [JsonProperty]
        public List<SideEffectDBInfo> SEList = new List<SideEffectDBInfo>();      
    }
}