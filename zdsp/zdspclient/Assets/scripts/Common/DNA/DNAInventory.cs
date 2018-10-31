using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Kopio.JsonContracts;
using System.ComponentModel;
using Zealot.Common;
using Zealot.Repository;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class DNAItemElement
    {
        [JsonProperty(PropertyName = "dnaId")]
        public uint DNAId { get; set; }

        [JsonProperty(PropertyName = "dnaStage")]
        public uint DNAStage { get; set; }

        [JsonProperty(PropertyName = "dnaLvl")]
        public uint DNALvl { get; set; }

        [JsonProperty(PropertyName = "dnaExp")]
        public uint DNAExp { get; set; }
    }

    public partial class DNAInvData
    {
        // Permanent
        [JsonProperty(PropertyName = "dnaSlots")]
        public List<DNAItemElement> DNASlots = new List<DNAItemElement>();

        public const int MAX_DNASLOTS = 6;

        public DNAInvData()
        {

        }

        public void InitDefault()
        {
            // DNA Slots
            DNASlots.Clear();
            for(int i = 0; i < MAX_DNASLOTS; ++i)
            {
                DNAItemElement dnaItemEle = new DNAItemElement();
                dnaItemEle.DNAId = 0;
                dnaItemEle.DNAStage = 0;
                dnaItemEle.DNALvl = 0;
                dnaItemEle.DNAExp = 0;

                DNASlots.Add(dnaItemEle);
            }
        }
    }
}
