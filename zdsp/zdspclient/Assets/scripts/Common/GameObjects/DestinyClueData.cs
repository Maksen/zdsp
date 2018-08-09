using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zealot.Common
{
    public enum ClueType
    {
        Normal,
        Dialogue,
        Time,
    }

    public enum ClueStatus
    {
        New,
        Read,
    }

    public class ActivatedClueData
    {
        public int ClueId { get; set; }
        public byte ClueType { get; set; }
        public string ActivatedDate { get; set; }
        public string ActivatedTime { get; set; }
        public long ActivatedDateTime { get; set; }
        public byte Status { get; set; }

        public ActivatedClueData(int clueid, ClueType type, string date, string time, long datetime, ClueStatus status)
        {
            ClueId = clueid;
            ClueType = (byte)type;
            ActivatedDate = date;
            ActivatedTime = time;
            ActivatedDateTime = datetime;
            Status = (byte)status;
        }
    }

    public class LockedClueData
    {
        public int ClueId { get; set; }
        public bool Condition1Status { get; set; }
        public bool Condition2Status { get; set; }
        public bool Condition3Status { get; set; }

        public LockedClueData(int clueid)
        {
            ClueId = clueid;
            Condition1Status = false;
            Condition2Status = false;
            Condition3Status = false;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class DestinyClueInventory
    {
        [JsonProperty(PropertyName = "UnlockMemory")]
        public string UnlockMemory { get; set; }

        [JsonProperty(PropertyName = "ActivatedClues")]
        public string ActivatedClues { get; set; }

        [JsonProperty(PropertyName = "LockedClues")]
        public string LockedClues { get; set; }

        [JsonProperty(PropertyName = "UnlockClues")]
        public string UnlockClues { get; set; }

        public List<ActivatedClueData> DeserializedActivatedClues()
        {
            return string.IsNullOrEmpty(ActivatedClues) ? new List<ActivatedClueData>() : JsonConvertDefaultSetting.DeserializeObject<List<ActivatedClueData>>(ActivatedClues);
        }

        public List<int> DeserializedUnlockMemory()
        {
            return string.IsNullOrEmpty(UnlockMemory) ? new List<int>() : JsonConvertDefaultSetting.DeserializeObject<List<int>>(UnlockMemory);
        }

        public List<LockedClueData> DeserializedLockedClues()
        {
            return string.IsNullOrEmpty(LockedClues) ? new List<LockedClueData>() : JsonConvertDefaultSetting.DeserializeObject<List<LockedClueData>>(LockedClues);
        }

        public List<int> DeserializedUnlockClues()
        {
            return string.IsNullOrEmpty(UnlockClues) ? new List<int>() : JsonConvertDefaultSetting.DeserializeObject<List<int>>(UnlockClues);
        }
    }
}
