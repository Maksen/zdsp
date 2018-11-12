using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zealot.Common.Entities;
using Zealot.Common.Datablock;
using Zealot.Common.Entities.Social;

namespace Zealot.Common
{
    /// <summary>
    /// 社群好友InventoryData for zdsp
    /// </summary>
    public partial class SocialInventoryData: IJsonInventoryData
    {
        #region CONST VALUE
        /// <summary>
        /// 系統最大好友數量
        /// </summary>
        public const int SYS_MAX_GOOD_FRIENDS = 100;
        /// <summary>
        /// 系統最大黑名單數量
        /// </summary>
        public const int SYS_MAX_BLACK_FRIENDS = 100;
        /// <summary>
        /// 系統最大請求名單數量
        /// </summary>
        public const int SYS_MAX_REQUEST_FRIENDS = 100;
        /// <summary>
        /// 系統最大臨時好友數量
        /// </summary>
        public const int SYS_MAX_TEMP_COUNT = 30;
        /// <summary>
        /// 系統最大朋友數量
        /// </summary>
        public const int SYS_MAX_COUNT = SYS_MAX_GOOD_FRIENDS + SYS_MAX_BLACK_FRIENDS + SYS_MAX_REQUEST_FRIENDS + SYS_MAX_TEMP_COUNT;

        /// <summary>
        /// 預設最大好友數量
        /// </summary>
        public const int DEFALT_MAX_GOOD_FRIENDS = 50;
        /// <summary>
        /// 預設最大黑名單數量
        /// </summary>
        public const int DEFALT_MAX_BLACK_FRIENDS = 50;
        /// <summary>
        /// 預設最大請求名單數量
        /// </summary>
        public const int DEFALT_MAX_REQUEST_FRIENDS = 50;
        /// <summary>
        /// 預設最大臨時好友數量
        /// </summary>
        public const int DEFALT_MAX_TEMP_COUNT = 20;
        #endregion

        [JsonProperty(PropertyName = "root")]
        public JToken root { get; set; }

        public SocialData data { get;private set; }

        #region removed future
        //[JsonProperty(PropertyName = "friendList")]
        //public IList<string> friendList= new List<string>();
        //[JsonProperty(PropertyName = "friendList")]
        //public IList<string> friendRequestList= new List<string>();
        #endregion
    }

    namespace Entities.Social
    {
        #region removed future
        //public enum SocialReturnCode : byte
        //{
        //    Ret_AlreadyAdded = 0,
        //    Ret_DoesNotExist,
        //    Ret_RecommendedResult,
        //    Ret_OnCooldown,
        //    Ret_LevelNotEnough
        //}
        #endregion

        public enum SocialResult : byte
        {
            Success,
            InvalidOperation,
            AlreadyAdded,
            ListFull,
            MyListFull,
            PlayerNameNotFound,
            PlayerIdNotFound,
            Blacked,
            RemoveGoodFriendFirst,
            PlayerCantBeSelf,
            TargetBlacked,
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

        #region Social Sub Data Types
        public struct SocialFriend
        {
            public readonly JToken node;
            public readonly string name;

            public string id
            {
                get { return node["id"].Value<string>(); }
            }

            public SocialFriend(JToken node)
            {
                this.node = node;
                this.name = node["name"].Value<string>();
            }
            public SocialFriend(string id, string name)
            {
                node = new JObject();
                node["id"] = new JValue(id);
                node["name"] = new JValue(name);

                this.name = name;
            }
        }
        
        public class SocialFriendList : JArrayNode<SocialFriend>
        {
            FriendType type;
            public FriendType Type { get { return type; } }
            protected override JToken GetToken(SocialFriend node)
            {
                return node.node;
            }
            protected override SocialFriend GetNode(JToken token)
            {
                return new SocialFriend(token);
            }
            public SocialFriendList(FriendType type, JArray array, string path, AdvancedLocalObject obj) : base(array, path, obj)
            {
                this.type = type;
            }

            public bool ContainsPlayerName(string name,out int index)
            {
                int c = Count;
                for (int i = 0; i < c; i++)
                    if (this[i].name == name)
                    {
                        index = i;
                        return true;
                    }
                index = -1;
                return false;
            }
            public bool ContainsPlayerName(string name)
            {
                int c = Count;
                for (int i = 0; i < c; i++)
                    if (this[i].name == name)
                        return true;
                return false;
            }

            public bool FindFriendByName(string name,out SocialFriend friend,out int index)
            {
                int c = Count;
                for (int i=0;i<c;i++)
                {
                    friend = this[i];
                    if (this[i].name==name)
                    {
                        index = i;
                        return true;
                    }
                }
                friend = new SocialFriend();
                index = -1;
                return false;
            }
        }

        public struct SocialFriendState
        {
            public const int ID_name = 0;
            public const int ID_online = 1;
            public const int ID_offlineTime = 2;
            public const int ID_channel = 3;
            public const int ID_progressLevel = 4;
            public const int ID_guildName = 5;
            public const int ID_guildIcon = 6;

            public const int FIELDS_COUNT = 7;

            public readonly JArray node;
            public bool IsValid { get { return node != null; } }

            public SocialFriendState(JToken node)
            {
                this.node = node as JArray;
            }

            public string name { get { return node[ID_name].Value<string>(); } }
            public bool online { get { return node[ID_online].Value<bool>(); } }
            public string offlineTime { get { return node[ID_offlineTime].Value<string>(); } }
            public string channel { get { return node[ID_channel].Value<string>(); } }
            public int progressLevel { get { return node[ID_progressLevel].Value<int>(); } }
            public string guildName { get { return node[ID_guildName].Value<string>(); } }
            public string guildIcon { get { return node[ID_guildIcon].Value<string>(); } }

            public SocialFriendState(
                string name,
                string offlineTime,
                string channel,
                int progressLevel,
                string guildName,
                string guildIcon)
            {
                JArray node = new JArray(new object[FIELDS_COUNT]);
                this.node = node;
                node[ID_name] = name;
                if (offlineTime == null)
                {
                    node[ID_online] = true;
                    node[ID_offlineTime] = string.Empty;
                }
                else
                {
                    node[ID_online] = false;
                    node[ID_offlineTime] = offlineTime;
                }

                node[ID_channel] = channel;
                node[ID_progressLevel] = progressLevel;
                node[ID_guildName] = guildName;
                node[ID_guildIcon] = guildIcon;
            }

            public static SocialFriendState NewCharaState(string name)
            {
                return new SocialFriendState(
                    name: name,
                    offlineTime: string.Empty,
                    channel: string.Empty,
                    progressLevel: 0,
                    guildName: string.Empty,
                    guildIcon: string.Empty
                    );
            }
        }

        public class SocialFriendStateList : JArrayNode<SocialFriendState>
        {
            protected override SocialFriendState GetNode(JToken token)
            {
                return new SocialFriendState(token);
            }

            protected override JToken GetToken(SocialFriendState node)
            {
                return node.node;
            }

            public SocialFriendStateList(JArray array, string path, AdvancedLocalObject obj) : base(array, path, obj)
            {
            }
        }
        #endregion

        public partial class SocialData : AdvancedLocalObjectData
        {
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

            public int maxCount_good { get { return GetPathValue<int>("maxCount.good"); } set { SetPathValue("maxCount.good", value); } }
            public int maxCount_black { get { return GetPathValue<int>("maxCount.black"); } set { SetPathValue("maxCount.black", value); } }
            public int maxCount_request { get { return GetPathValue<int>("maxCount.request"); } set { SetPathValue("maxCount.request", value); } }
            public int maxCount_temp { get { return GetPathValue<int>("maxCount.temp"); } set { SetPathValue("maxCount.temp", value); } }

            public string checkdate { get { return GetValue<string>("checkdate"); } set { SetValueNoPatch("checkdate", value); } }

            //狀態清單
            public SocialFriendStateList goodFriendStates { get; private set; }
            public SocialFriendStateList blackFriendStates { get; private set; }
            public SocialFriendStateList requestFriendStates { get; private set; }
            public SocialFriendStateList tempFriendStates { get; private set; }

            void initFromRoot(JToken root, AdvancedLocalObject obj)
            {
                NewIfNotExist(root, "version", () => new JValue(-1));

                goodFriends = new SocialFriendList(FriendType.Good, NewIfNotExist(root, "goodFriends", NewArray), "goodFriends", obj);
                blackFriends = new SocialFriendList(FriendType.Black, NewIfNotExist(root, "blackFriends", NewArray), "blackFriends", obj);
                requestFriends = new SocialFriendList(FriendType.Request, NewIfNotExist(root, "requestFriends", NewArray), "requestFriends", obj);
                tempFriends = new SocialFriendList(FriendType.Temp, NewIfNotExist(root, "tempFriends", NewArray), "tempFriends", obj);

                JObject count = NewIfNotExist(root, "maxCount", NewObject);
                NewIfNotExist(count, "good", () => new JValue(SocialInventoryData.DEFALT_MAX_GOOD_FRIENDS));
                NewIfNotExist(count, "black", () => new JValue(SocialInventoryData.DEFALT_MAX_BLACK_FRIENDS));
                NewIfNotExist(count, "request", () => new JValue(SocialInventoryData.DEFALT_MAX_REQUEST_FRIENDS));
                NewIfNotExist(count, "temp", () => new JValue(SocialInventoryData.DEFALT_MAX_TEMP_COUNT));

                NewIfNotExist(root, "checkdate", () => new JValue(string.Empty));

                goodFriendStates = new SocialFriendStateList(NewIfNotExist(root, "goodFriendStates", NewArray), "goodFriendStates", obj);
                blackFriendStates = new SocialFriendStateList(NewIfNotExist(root, "blackFriendStates", NewArray), "blackFriendStates", obj);
                requestFriendStates = new SocialFriendStateList(NewIfNotExist(root, "requestFriendStates", NewArray), "requestFriendStates", obj);
                tempFriendStates = new SocialFriendStateList(NewIfNotExist(root, "tempFriendStates", NewArray), "tempFriendStates", obj);
            }

            protected override void OnSetDataUsage()
            {
                this.m_Records = new string[] {"version", "goodFriends", "blackFriends", "requestFriends", "tempFriends","maxCount" };
                this.m_ServerRecords = new string[] { "checkdate" };
                this.m_States = new string[] { "goodFriendStates", "blackFriendStates", "requestFriendStates", "tempFriendStates" };
            }

            #region Constructors
            /// <summary>
            /// 給server端的stats使用，因為整塊資料都與Inventory相同，故使用參考連結方式
            /// </summary>
            /// <param name="root"></param>
            public SocialData(SocialData data,AdvancedLocalObject obj) : base(obj,data.Root)
            {
                //先暫時不要建立索引，因為目前還沒需要大量使用的時機
                //doMap = true;
                //allMap = new Dictionary<string, IndexInfo>();
                //name2id = new Dictionary<string, string>();
                data.AffectEntities.Add(this);
                OnUpdateRootValue += () => { initFromRoot(m_Root, obj); };
                UpdateRootValue();
            }
            public SocialData() : base(null, new JObject())
            {
                doMap = false;
                OnUpdateRootValue += () => { initFromRoot(m_Root, null); };
            }
            /// <summary>
            /// 給server端的Inventory使用
            /// </summary>
            /// <param name="root"></param>
            public SocialData(JToken root) : base(null,root)
            {
                doMap = false;
                OnUpdateRootValue += () => { initFromRoot(m_Root, null); };
                UpdateRootValue();
            }
            #endregion

            #region Private Fields
            struct IndexInfo
            {
                public FriendType type;
                public int index;
                public IndexInfo(FriendType type, int index)
                {
                    this.type = type;
                    this.index = index;
                }
            }

            private bool doMap=false;
            private Dictionary<string, IndexInfo> allMap;
            private Dictionary<string, string> name2id;
            #endregion
        }

        #region Schema
        /// <summary>
        /// 資料結構描述，用來驗證資料的版本
        /// </summary>
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class Social_Schema
        {
            public const int VERSION = 0;
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

            public Social_Schema()
            {
                version = VERSION;
                goodFriends = new List<SocialFriend_Schema>();
                blackFriends = new List<SocialFriend_Schema>();
                requestFriends = new List<SocialFriend_Schema>();
                tempFriends = new List<SocialFriend_Schema>();
                maxCount = new Social_MaxCount_Schema();
                checkdate = string.Empty;
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
                good = SocialInventoryData.DEFALT_MAX_GOOD_FRIENDS;
                black = SocialInventoryData.DEFALT_MAX_BLACK_FRIENDS;
                request = SocialInventoryData.DEFALT_MAX_REQUEST_FRIENDS;
                temp = SocialInventoryData.DEFALT_MAX_TEMP_COUNT;
            }
        }
        #endregion
    }


}
