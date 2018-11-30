using Zealot.Common.Datablock;
using Newtonsoft.Json.Linq;

namespace Zealot.Common.Entities.Social
{
    /// <summary>
    /// 朋友的關鍵資訊，由JArray組成，格式為["{charname}","GameUtils.GuidStringToCompressedString({charid})"]
    /// </summary>
    public struct SocialFriend
    {
        public const int ID_name = 0;
        public const int ID_id = 1;

        public const int FIELDS_COUNT = 2;

        public readonly JArray node;
        public string name { get { return node.String(ID_name); } }

        public string id
        {
            get
            {
                var nid = node[ID_id] as JValue;
                string s = nid.Value as string;
                if (s == null)
                {
                    nid.Value = GameUtils.GuidToCompressedString(System.Guid.Empty);
                    return System.Guid.Empty.ToString();
                }
                else if (s.Length > 22)
                {
                    nid.Value = GameUtils.GuidStringToCompressedString(s);
                    return s;
                }
                else
                    return GameUtils.CompressedStringToGuidString(s);
            }
        }

        public SocialFriend(JToken node)
        {
            this.node = node as JArray;
        }
        public SocialFriend(string id, string name)
        {
            node = new JArray();
            node.SetSafely(ID_name, name);
            node.SetSafely(ID_id, GameUtils.GuidStringToCompressedString(id));
        }
    }

    /// <summary>
    /// list of SocialFriend
    /// </summary>
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

        public bool ContainsPlayerName(string name, out int index)
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

        public bool FindFriendByName(string name, out SocialFriend friend, out int index)
        {
            int c = Count;
            for (int i = 0; i < c; i++)
            {
                friend = this[i];
                if (this[i].name == name)
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

    /// <summary>
    /// 包含各種額外資訊的朋友資料
    /// </summary>
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

        public string name { get { return node.String(ID_name); } }
        public bool online { get { return node.Boolean(ID_online); } set { node[ID_online] = value; } }
        public string offlineTime { get { return node.String(ID_offlineTime); } set { node[ID_offlineTime] = value; } }
        public string channel { get { return node.String(ID_channel); } set { node[ID_channel] = value; } }
        public int progressLevel { get { return node.Int32(ID_progressLevel); } }
        public string guildName { get { return node.String(ID_guildName); } }
        public string guildIcon { get { return node.String(ID_guildIcon); } }

        public SocialFriendState(
            string name,
            string offlineTime,
            string channel,
            int progressLevel,
            string guildName,
            string guildIcon)
        {
            node = new JArray();
            node.SetSafely(ID_name, name);
            node.SetSafely(ID_online, !string.IsNullOrEmpty(channel));
            node.SetSafely(ID_offlineTime, offlineTime);
            node.SetSafely(ID_channel, channel);
            node.SetSafely(ID_progressLevel, progressLevel);
            node.SetSafely(ID_guildName, guildName);
            node.SetSafely(ID_guildIcon, guildIcon);
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

    /// <summary>
    /// list of SocialFriendState
    /// </summary>
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

        public bool ContainsPlayerName(string name)
        {
            int c = Count;
            for (int i = 0; i < c; i++)
                if (this[i].name == name)
                    return true;
            return false;
        }
    }
}