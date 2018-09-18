using Newtonsoft.Json;
using System.Collections.Generic;

namespace Zealot.Common
{
    public enum MailReturnCode : byte
    {
        DeleteMail_Success,
        DeleteMail_Fail_InvalidIndex,
        DeleteMail_Fail_HasAttachment,
        DeleteAllMail_Success,
        OpenMail_Success,
        OpenMail_Fail_InvalidIndex,
        TakeAttachment_Success,
        TakeAttachment_Fail_InvalidIndex,
        TakeAttachment_Fail_InventoryFull,
        TakeAttachment_Fail_InventoryAddFailed,
        TakeAttachment_Fail_InventoryUnknownRetCode,
        TakeAllAttachment_Success,
        TakeAllAttachment_Fail_InventoryFull,
        TakeAllAttachment_Fail_InventoryAddFailed,
        TakeAllAttachment_Fail_InventoryUnknownRetCode,
    }

    public enum MailStatus : byte
    {
        Unread,
        Read
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MailData
    {
        [JsonProperty(PropertyName = "mName")]
        public string mailName { get; set; }

        [JsonProperty(PropertyName = "iNw")]
        public MailStatus mailStatus { get; set; }

        [JsonProperty(PropertyName = "iTk")]
        public bool isTaken { get; set; }

        [JsonProperty(PropertyName = "eTk")]
        public long expiryTicks { get; set; }

        [JsonProperty(PropertyName = "ath")]
        public List<IInventoryItem> lstIInventoryItem { get; set; }

        [JsonProperty(PropertyName = "curr")]
        public Dictionary<CurrencyType, int> dicCurrencyAmt { get; set; }

        [JsonProperty(PropertyName = "topgold")]
        public bool hasTopupGold;

        [JsonProperty(PropertyName = "bodyParam")]
        public Dictionary<string, string> dicBodyParam { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MailInventoryData
    {
        [JsonProperty(PropertyName = "mail")]
        public List<MailData> lstMailData = new List<MailData>();

        [JsonProperty(PropertyName = "new")]
        public bool hasNewMail { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MailObject
    {
        public string rcvName { get; set; } // don't need to save to db. db got a charname already

        [JsonProperty(PropertyName = "mail")]
        public string mailName { get; set; }

        [JsonProperty(PropertyName = "items")]
        public List<IInventoryItem> lstAttachment;

        [JsonProperty(PropertyName = "currency")]
        public Dictionary<CurrencyType, int> dicCurrencyAmt;

        [JsonProperty(PropertyName = "topgold")]
        public bool hasTopupGold;

        [JsonProperty(PropertyName = "bodyparam")]
        public Dictionary<string, string> dicBodyParam;

        public MailObject()
        {
            lstAttachment = new List<IInventoryItem>();
            dicCurrencyAmt = new Dictionary<CurrencyType, int>();
            dicBodyParam = new Dictionary<string, string>();
            hasTopupGold = false;
        } 
    }
}
