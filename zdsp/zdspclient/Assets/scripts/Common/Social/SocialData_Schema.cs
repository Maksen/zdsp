using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Zealot.Common.Entities.Social
{
    #region VERSION: 0
#if false
    
    /// <summary>
    /// 資料結構描述，用來驗證資料的版本
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SocialData_Schema
    {
        [JsonProperty("version")]
        public int version { get; set; }

        [JsonProperty("goodFriends")]
        public List<SocialFriend_Schema> goodFriends { get; set; }

        [JsonProperty("blackFriends")]
        public List<SocialFriend_Schema> blackFriends { get; set; }

        [JsonProperty("requestFriends")]
        public List<SocialFriend_Schema> requestFriends { get; set; }

        [JsonProperty("tempFriends")]
        public List<SocialFriend_Schema> tempFriends { get; set; }

        [JsonProperty("maxCount")]
        public Social_MaxCount_Schema maxCount { get; set; }

        [JsonProperty("checkdate")]
        public string checkdate { get; set; }

        public SocialData_Schema()
        {
            version = SocialData.VERSION;
            goodFriends = new List<SocialFriend_Schema>();
            blackFriends = new List<SocialFriend_Schema>();
            requestFriends = new List<SocialFriend_Schema>();
            tempFriends = new List<SocialFriend_Schema>();
            maxCount = new Social_MaxCount_Schema();
            checkdate = string.Empty;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Social_MaxCount_Schema
    {
        [JsonProperty("good")]
        public int good { get; set; }
        [JsonProperty("black")]
        public int black { get; set; }
        [JsonProperty("request")]
        public int request { get; set; }
        [JsonProperty("temp")]
        public int temp { get; set; }

        public Social_MaxCount_Schema()
        {
            good = SocialData.DEFALT_MAX_GOOD_COUNT;
            black = SocialData.DEFALT_MAX_BLACK_COUNT;
            request = SocialData.DEFALT_MAX_REQUEST_COUNT;
            temp = SocialData.DEFALT_MAX_TEMP_COUNT;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SocialFriend_Schema
    {
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        public SocialFriend_Schema()
        {
            id = string.Empty;
            name = string.Empty;
        }
    }
#endif
    #endregion
    #region VERSION: 0 (Current Version)
    /// <summary>
    /// 資料結構描述，用來驗證資料的版本
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SocialData_Schema
    {
        [JsonProperty("version")]
        public int version { get; set; }

        [JsonProperty("goodFriends")]
        public List<SocialFriend_Schema> goodFriends { get; set; }

        [JsonProperty("blackFriends")]
        public List<SocialFriend_Schema> blackFriends { get; set; }

        [JsonProperty("requestFriends")]
        public List<SocialFriend_Schema> requestFriends { get; set; }

        [JsonProperty("tempFriends")]
        public List<SocialFriend_Schema> tempFriends { get; set; }

        [JsonProperty("maxCount")]
        public Social_MaxCount_Schema maxCount { get; set; }

        [JsonProperty("checkdate")]
        public string checkdate { get; set; }

        [JsonProperty("errorData", NullValueHandling = NullValueHandling.Ignore)]
        public string errorData { get; set; }

        public SocialData_Schema()
        {
            version = SocialData.VERSION;
            goodFriends = new List<SocialFriend_Schema>();
            blackFriends = new List<SocialFriend_Schema>();
            requestFriends = new List<SocialFriend_Schema>();
            tempFriends = new List<SocialFriend_Schema>();
            maxCount = new Social_MaxCount_Schema();
            checkdate = string.Empty;
        }
    }

    [JsonArray(AllowNullItems = true)]
    public class SocialFriend_Schema : JArray
    {
        public string name;
        public string id;
        public SocialFriend_Schema()
        {
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Social_MaxCount_Schema
    {
        [JsonProperty("good")]
        public int good { get; set; }
        [JsonProperty("black")]
        public int black { get; set; }
        [JsonProperty("request")]
        public int request { get; set; }
        [JsonProperty("temp")]
        public int temp { get; set; }

        public Social_MaxCount_Schema()
        {
            good = SocialData.DEFALT_MAX_GOOD_COUNT;
            black = SocialData.DEFALT_MAX_BLACK_COUNT;
            request = SocialData.DEFALT_MAX_REQUEST_COUNT;
            temp = SocialData.DEFALT_MAX_TEMP_COUNT;
        }
    }
    #endregion
}