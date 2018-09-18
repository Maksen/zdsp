using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

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
        Collected,
    }

    public class ActivatedClueData
    {
        [JsonProperty(PropertyName = "ClueId")]
        public int ClueId { get; set; }

        [JsonProperty(PropertyName = "ClueType")]
        public byte ClueType { get; set; }

        [JsonProperty(PropertyName = "ActivatedDate")]
        public string ActivatedDate { get; set; }

        [JsonProperty(PropertyName = "ActivatedTime")]
        public string ActivatedTime { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public byte Status { get; set; }

        public DateTime ActivatedDT { get; set; }

        public ActivatedClueData(int clueid, ClueType type, string date, string time, ClueStatus status)
        {
            ClueId = clueid;
            ClueType = (byte)type;
            ActivatedDate = date;
            ActivatedTime = time;
            Status = (byte)status;
        }

        public void UpdateDT()
        {
            ActivatedDT = DateTime.ParseExact(ActivatedDate, "yyyy.MM.dd", CultureInfo.InvariantCulture);
            ActivatedDT = DateTime.ParseExact(ActivatedTime, "HH:mm", CultureInfo.InvariantCulture);
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

        [JsonProperty(PropertyName = "UnlockTimeClues")]
        public string UnlockTimeClues { get; set; }

        public DestinyClueInventory()
        {
            UnlockMemory = "";
            ActivatedClues = "";
            LockedClues = "";
            UnlockClues = "";
            UnlockTimeClues = "";
        }

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

        public List<int> DeserializedUnlockTimeClues()
        {
            return string.IsNullOrEmpty(UnlockTimeClues) ? new List<int>() : JsonConvertDefaultSetting.DeserializeObject<List<int>>(UnlockTimeClues);
        }

        public void SerializedActivatedClues(List<ActivatedClueData> activatedClueDatas)
        {
            ActivatedClues = JsonConvertDefaultSetting.SerializeObject(activatedClueDatas);
        }

        public void SerializedUnlockMemory(List<int> unlockMemory)
        {
            UnlockMemory = JsonConvertDefaultSetting.SerializeObject(unlockMemory);
        }

        public void SerializedLockedClues(List<LockedClueData> lockedClues)
        {
            LockedClues = JsonConvertDefaultSetting.SerializeObject(lockedClues);
        }

        public void SerializedUnlockClues(List<int> unlockClues)
        {
            UnlockClues = JsonConvertDefaultSetting.SerializeObject(unlockClues);
        }

        public void SerializedUnlockTimeClues(List<int> unlockTimeClues)
        {
            UnlockTimeClues = JsonConvertDefaultSetting.SerializeObject(unlockTimeClues);
        }
    }
}
