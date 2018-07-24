using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using Zealot.Repository;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PrizeGuaranteeData
    {
        [JsonProperty(PropertyName = "PrizeGuaranteeInfos")]
        public List<PrizeGuaranteeInfo> PrizeGuaranteeInfos;

        public void InitDefault()
        {
            PrizeGuaranteeInfos = new List<PrizeGuaranteeInfo>();
        }
    }

    public class PrizeGuaranteeInfo
    {
        [JsonProperty(PropertyName = "ID")]
        public int ID;
        [JsonProperty(PropertyName = "Type")]
        public int Type;
        [JsonProperty(PropertyName = "ItemID")]
        public int ItemID;
        [JsonProperty(PropertyName = "Count")]
        public int Count;
    }
}
