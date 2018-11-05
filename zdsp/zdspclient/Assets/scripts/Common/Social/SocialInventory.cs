using System.Collections.Generic;
using System.ComponentModel;
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
        /// <summary>
        /// 最大好友數量
        /// </summary>
        public const int MAX_GOOD_FRIENDS = 50;
        /// <summary>
        /// 最大黑名單數量
        /// </summary>
        public const int MAX_BLACK_FRIENDS = 50;
        /// <summary>
        /// 最大請求名單數量
        /// </summary>
        public const int MAX_REQUEST_FRIENDS = 50;
        /// <summary>
        /// 最大請求名單數量
        /// </summary>
        public const int MAX_RECOMMAND_COUNT = 20;
        /// <summary>
        /// 最大朋友數量(上面三者總數)
        /// </summary>
        public const int MAX_COUNT = MAX_GOOD_FRIENDS + MAX_BLACK_FRIENDS+ MAX_REQUEST_FRIENDS+ MAX_RECOMMAND_COUNT;

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
        }

        /// <summary>
        /// 朋友類型
        /// </summary>
        public enum FriendType : byte
        {
            Good,
            Black,
            Request,
            Recommand
        }

        public struct SocialFriend
        {
            public readonly JToken node;
            public readonly FriendType type;

            public string id
            {
                get { return node["id"].Value<string>(); }
            }

            public string name
            {
                get { return node["name"].Value<string>(); }
            }

            public SocialFriend(JToken node, FriendType type)
            {
                this.node = node;
                this.type = type;
            }
            public SocialFriend(FriendType type,  string id, string name)
            {
                node = new JObject();
                node["id"] = new JValue(id);
                node["name"] = new JValue(name);
                this.type = type;
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
                return new SocialFriend(token, type);
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
        }

        public class SocialData : AdvancedLocalObjectData
        {
            void initFromRoot(JToken root, AdvancedLocalObject obj)
            {
                NewIfNotExist(root, "checkdate", () => new JValue(string.Empty));
                goodFriends = new SocialFriendList(FriendType.Good, NewIfNotExist(root, "goodFriends", NewArray), "goodFriends", obj);
                blackFriends = new SocialFriendList(FriendType.Black, NewIfNotExist(root, "blackFriends", NewArray), "blackFriends", obj);
                requestFriends = new SocialFriendList(FriendType.Request, NewIfNotExist(root, "requestFriends", NewArray), "requestFriends", obj);
                recommandFriends = new SocialFriendList(FriendType.Recommand, NewIfNotExist(root, "recommandFriends", NewArray), "recommandFriends", obj);
            }

            /// <summary>
            /// 給server端的stats使用，因為整塊資料都與Inventory相同，故使用參考連結方式
            /// </summary>
            /// <param name="root"></param>
            public SocialData(SocialData data,AdvancedLocalObject obj) : base(obj,data.Root)
            {
                //之後
                data.AffectEntities.Add(this);
                OnUpdateRootValue += () => { initFromRoot(m_Root, obj); };
                UpdateRootValue();
            }
            public SocialData() : base(null, new JObject())
            {
                OnUpdateRootValue += () => { initFromRoot(m_Root, null); };
                UpdateRootValue();

            }
            /// <summary>
            /// 給server端的Inventory使用
            /// </summary>
            /// <param name="root"></param>
            public SocialData(JToken root) : base(null,root)
            {
                OnUpdateRootValue += () => { initFromRoot(m_Root, null); };
                UpdateRootValue();
            }

            public SocialFriendList goodFriends { get; private set; }
            public SocialFriendList blackFriends { get; private set; }
            public SocialFriendList requestFriends { get; private set; }
            public SocialFriendList recommandFriends { get; private set; }

            public string checkdate { get { return GetValue<string>("checkdate"); } set { SetValueNoPatch("checkdate", value);  } }

            public SocialFriendList getFriends(FriendType type)
            {
                switch (type)
                {
                    case FriendType.Good:
                        return goodFriends;
                    case FriendType.Black:
                        return blackFriends;
                    case FriendType.Request:
                        return requestFriends;
                    default:
                    case FriendType.Recommand:
                        return recommandFriends;
                }
            }

            public int getFriendMaxCount(FriendType type)
            {
                switch (type)
                {
                    case FriendType.Good:
                        return SocialStats.MAX_GOOD_FRIENDS;
                    case FriendType.Black:
                        return SocialStats.MAX_BLACK_FRIENDS;
                    case FriendType.Request:
                        return SocialStats.MAX_REQUEST_FRIENDS;
                    default:
                    case FriendType.Recommand:
                        return SocialStats.MAX_RECOMMAND_COUNT;
                }
            }

            public FriendType getFriendType(string name)
            {
                switch (name)
                {

                    case "goodFriends":
                        return FriendType.Good;
                    case "blackFriends":
                        return FriendType.Black;
                    case "requestFriends":
                        return FriendType.Request;
                    default:
                    case "recommandFriends":
                        return FriendType.Recommand;
                }
            }

            public SocialFriendList getFriends(string name)
            {
                switch (name)
                {
                    default:
                        return null;
                    case "goodFriends":
                        return goodFriends;
                    case "blackFriends":
                        return blackFriends;
                    case "requestFriends":
                        return requestFriends;
                    case "recommandFriends":
                        return recommandFriends;
                }
            }

        }
    }


}
