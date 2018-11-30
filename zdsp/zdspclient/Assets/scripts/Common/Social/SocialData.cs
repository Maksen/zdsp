using Zealot.Common.Datablock;
using Newtonsoft.Json.Linq;

namespace Zealot.Common.Entities.Social
{
    /// <summary>
    /// 社群資料json的root data
    /// </summary>
    public partial class SocialData : AdvancedLocalObjectData
    {
        public const int VERSION = 1;

        public static bool Debug = true;

        #region CONST VALUE
        /// <summary>
        /// 系統最大好友數量
        /// </summary>
        public const int SYS_MAX_GOOD_COUNT = 100;
        /// <summary>
        /// 系統最大黑名單數量
        /// </summary>
        public const int SYS_MAX_BLACK_COUNT = 100;
        /// <summary>
        /// 系統最大請求名單數量
        /// </summary>
        public const int SYS_MAX_REQUEST_COUNT = 100;
        /// <summary>
        /// 系統最大臨時好友數量
        /// </summary>
        public const int SYS_MAX_TEMP_COUNT = 30;
        /// <summary>
        /// 系統最大朋友數量
        /// </summary>
        public const int SYS_MAX_COUNT = SYS_MAX_GOOD_COUNT + SYS_MAX_BLACK_COUNT + SYS_MAX_REQUEST_COUNT + SYS_MAX_TEMP_COUNT;

        /// <summary>
        /// 預設最大好友數量
        /// </summary>
        public const int DEFALT_MAX_GOOD_COUNT = 50;
        /// <summary>
        /// 預設最大黑名單數量
        /// </summary>
        public const int DEFALT_MAX_BLACK_COUNT = 50;
        /// <summary>
        /// 預設最大請求名單數量
        /// </summary>
        public const int DEFALT_MAX_REQUEST_COUNT = 50;
        /// <summary>
        /// 預設最大臨時好友數量
        /// </summary>
        public const int DEFALT_MAX_TEMP_COUNT = 20;
        #endregion

        public int version { get { return GetValue<int>("version"); } set { SetValue("version", value); } }

        /// <summary>
        /// 好友清單
        /// </summary>
        public SocialFriendList goodFriends { get; private set; }
        /// <summary>
        /// 黑名單
        /// </summary>
        public SocialFriendList blackFriends { get; private set; }
        /// <summary>
        /// 請求名單
        /// </summary>
        public SocialFriendList requestFriends { get; private set; }
        /// <summary>
        /// 臨時名單
        /// </summary>
        public SocialFriendList tempFriends { get; private set; }

        public int maxCount_good { get { return GetPath("maxCount.good").Int32(); } set { SetPathValue("maxCount.good", value); m_Dirty = true; } }
        public int maxCount_black { get { return GetPath("maxCount.black").Int32(); } set { SetPathValue("maxCount.black", value); m_Dirty = true; } }
        public int maxCount_request { get { return GetPath("maxCount.request").Int32(); } set { SetPathValue("maxCount.request", value); m_Dirty = true; } }
        public int maxCount_temp { get { return GetPath("maxCount.temp").Int32(); } set { SetPathValue("maxCount.temp", value); m_Dirty = true; } }

        public string checkdate { get { return Get("checkdate").String(); } set { SetValueNoPatch("checkdate", value); m_Dirty = true; } }

        //狀態清單
        public SocialFriendStateList goodFriendStates { get; private set; }
        public SocialFriendStateList blackFriendStates { get; private set; }
        public SocialFriendStateList requestFriendStates { get; private set; }
        public SocialFriendStateList tempFriendStates { get; private set; }

        void initFromRoot(JToken root, AdvancedLocalObject obj)
        {
            NewIfNotExist(root, "version", () => new JValue(SocialData.VERSION));

            if (!isClient)
            {
                goodFriends = new SocialFriendList(FriendType.Good, NewIfNotExist(root, "goodFriends", NewArray), "goodFriends", null);
                blackFriends = new SocialFriendList(FriendType.Black, NewIfNotExist(root, "blackFriends", NewArray), "blackFriends", null);
                requestFriends = new SocialFriendList(FriendType.Request, NewIfNotExist(root, "requestFriends", NewArray), "requestFriends", null);
                tempFriends = new SocialFriendList(FriendType.Temp, NewIfNotExist(root, "tempFriends", NewArray), "tempFriends", null);
            }

            JObject count = NewIfNotExist(root, "maxCount", NewObject);
            NewIfNotExist(count, "good", () => new JValue(DEFALT_MAX_GOOD_COUNT));
            NewIfNotExist(count, "black", () => new JValue(DEFALT_MAX_BLACK_COUNT));
            NewIfNotExist(count, "request", () => new JValue(DEFALT_MAX_REQUEST_COUNT));
            NewIfNotExist(count, "temp", () => new JValue(DEFALT_MAX_TEMP_COUNT));

            NewIfNotExist(root, "checkdate", () => new JValue(string.Empty));

            goodFriendStates = new SocialFriendStateList(NewIfNotExist(root, "goodFriendStates", NewArray), "goodFriendStates", obj);
            blackFriendStates = new SocialFriendStateList(NewIfNotExist(root, "blackFriendStates", NewArray), "blackFriendStates", obj);
            requestFriendStates = new SocialFriendStateList(NewIfNotExist(root, "requestFriendStates", NewArray), "requestFriendStates", obj);
            tempFriendStates = new SocialFriendStateList(NewIfNotExist(root, "tempFriendStates", NewArray), "tempFriendStates", obj);
        }

        protected override void OnSetDataUsage()
        {
            this.m_Records = new string[] { "version",  "maxCount" };
            this.m_ServerRecords = new string[] { "goodFriends", "blackFriends", "requestFriends", "tempFriends", "checkdate", "errorData" };
            this.m_States = new string[] { "goodFriendStates", "blackFriendStates", "requestFriendStates", "tempFriendStates" };
        }

    }

    #region Enums
    /// <summary>
    /// API Call Result Code
    /// </summary>
    public enum SocialResult : byte
    {
        Success,
        InvalidOperation,
        AlreadyAdded,
        ListFull,
        MyListFull,
        PlayerNameNotFoundInList,
        PlayerIdNotFoundInList,
        Blacked,
        RemoveGoodFriendFirst,
        PlayerCantBeSelf,
        TargetBlacked,
        AlreadyAddedGood,
        PlayerNameNotFound,
        PlayerServerChanged,
    }

    /// <summary>
    /// AddTempFriends Result Code
    /// </summary>
    public enum SocialAddTempFriends_Result
    {
        Success,
        Player1NameNotFound,
        Player2NameNotFound,
        InGood,
        InBlack,
        SameName,
    }

    /// <summary>
    /// 朋友類型
    /// </summary>
    public enum FriendType : byte
    {
        Good,
        Black,
        Request,
        Temp
    }
    #endregion
}