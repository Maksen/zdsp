using System.Text;
using System;
using System.Collections.Generic;
using Zealot.Common.Datablock;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zealot.DebugTools;
using Zealot.Common.Entities.Social;


namespace Zealot.Common.Entities
{
    public partial class SocialStats : AdvancedLocalObject, IStats
    {
        #region Sub Classes
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
        #endregion

        #region Private Fields
        private Dictionary<string, IndexInfo> allMap;
        private Dictionary<string, string> name2id;
        private bool isMapBuild;
        #endregion

        #region Constructors
        public SocialStats(bool isServer) : base(LOTYPE.SocialStats, isServer)
        {
            NotPatchedItem = new string[] { "checkdate" };
            isMapBuild = false;
            if (!isServer)
            {
                data = new SocialData();
                this.Root = this.data.Root;
            }

            allMap = new Dictionary<string, IndexInfo>();
            name2id = new Dictionary<string, string>();

            //未來拿掉;removed future (BEGIN)
            //friendList = new CollectionHandler<object>(0);
            //friendList.SetParent(this, "friendList");

            //friendRequestList = new CollectionHandler<object>(0);
            //friendRequestList.SetParent(this, "friendRequestList");
            //未來拿掉;removed future (END)
        }
        #endregion

        #region Private Methods
        void InitFromRoot()
        {
            isMapBuild = false;
            BuildNameMap();
        }
        #endregion

        #region Interface Methods

        #region AdvancedLocalObject


        public override void BeforePacth(int cmdIndex, MessageType op, string path)
        {
            if (op == MessageType.Update)
            {
                if (path != string.Empty)
                {
                    PathCapture_ElementOfArray(path, (listName, i) =>
                    {
                        SocialFriendList list = data.getFriends(listName);
                        if (list != null)
                        {
                            allMap.Remove(list[i].id);
                            name2id.Remove(list[i].name);
                        }
                    }, name =>
                    {
                        if (name == "recommandFriends")
                        {
                            for (int i = 0; i < data.recommandFriends.Count; i++)
                            {
                                var f = data.recommandFriends[i];
                                allMap.Remove(f.id);
                                name2id.Remove(f.name);
                            }
                        }
                    });
                }
            }
            else if (op == MessageType.Add)
            {
            }
            else if (op == MessageType.Remove)
            {
                PathCapture_ElementOfArray(path, (listName, i) =>
                {
                    SocialFriendList list = data.getFriends(listName);
                    if (list != null)
                    {
                        allMap.Remove(list[i].id);
                        moveForward(list, i);
                        name2id.Remove(list[i].name);
                    }
                }, null);
            }
        }

        public override void AfterPacth(int cmdIndex, MessageType op, string path)
        {
            DebugTool.Print(Root.ToString());


            if (op == MessageType.Update)
            {
                if (path == string.Empty)
                {
                    InitFromRoot();
                    return;
                }
                else
                {
                    PathCapture_ElementOfArray(path, (listName, i) =>
                    {
                        SocialFriendList list = data.getFriends(listName);

                        if (list != null)
                        {
                            SocialFriend f = list[i];
                            allMap.Add(f.id, new IndexInfo(data.getFriendType(listName), i));
                            name2id.Add(f.name, f.id);
                        }

                    }, name =>
                    {
                        if (name == "recommandFriends")
                        {
                            for (int i = 0; i < data.recommandFriends.Count; i++)
                            {
                                var f = data.recommandFriends[i];
                                allMap.Add(f.id, new IndexInfo(FriendType.Recommand, i));
                                name2id.Add(f.name, f.id);
                            }
                        }
                    });

                }
            }
            else if (op == MessageType.Add)
            {
                SocialFriendList list = data.getFriends(path);
                if (list != null)
                {
                    SocialFriend f = list.GetTail();
                    allMap.Add(f.id, new IndexInfo(data.getFriendType(path), data.goodFriends.Count - 1));
                    name2id.Add(f.name, f.id);
                }
            }
            else if (op == MessageType.Remove)
            {
            }

        }
        #endregion

        #region IStats
        public void LoadFromInventoryData(IInventoryData data)
        {
            SocialInventoryData _data = data as SocialInventoryData;

            DebugTool.Print("Social.LoadFromInventoryData");

            if (m_IsServer)
            {
                this.data = new SocialData(_data.data, this);
                this.Root = this.data.Root;
            }

            PatchFull();
        }

        public void SaveToInventoryData(IInventoryData data)
        {
            SocialInventoryData _data = data as SocialInventoryData;
        }
        #endregion

        #endregion

        #region Methods
        public bool GetFriendInfo(string name, out SocialFriend info)
        {
            if (name2id.ContainsKey(name))
            {
                var x = allMap[name2id[name]];
                info = data.getFriends(x.type)[x.index];
                return true;
            }
            info = new SocialFriend();
            return false;
        }


        static void buildIndexMap(SocialFriendList list, Dictionary<string, IndexInfo> map)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var f = list[i];
                if (!map.ContainsKey(f.id))
                    map.Add(f.id, new IndexInfo(list.Type, i));
            }
        }

        static void buildName2IdMap(SocialFriendList list, Dictionary<string, string> map)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var f = list[i];
                if (!map.ContainsKey(f.name))
                    map.Add(f.name, f.id);
            }
        }

        public void BuildNameMap()
        {
            //create name to index map
            if (!isMapBuild)
            {
                name2id.Clear();

                buildIndexMap(data.goodFriends, allMap);
                buildIndexMap(data.blackFriends, allMap);
                buildIndexMap(data.requestFriends, allMap);
                buildIndexMap(data.recommandFriends, allMap);

                buildName2IdMap(data.goodFriends, name2id);
                buildName2IdMap(data.blackFriends, name2id);
                buildName2IdMap(data.requestFriends, name2id);
                buildName2IdMap(data.recommandFriends, name2id);

                isMapBuild = true;
            }
        }



        public int GetCount(FriendType type)
        {
            switch (type)
            {
                case FriendType.Good:
                    return data.goodFriends.Count;
                case FriendType.Black:
                    return data.blackFriends.Count;
                case FriendType.Request:
                    return data.requestFriends.Count;
                default:
                    return data.recommandFriends.Count;
            }
        }
        public int GetAvailableCount(FriendType type)
        {
            switch (type)
            {
                case FriendType.Good:
                    return MAX_GOOD_FRIENDS - GetCount(type);
                case FriendType.Black:
                    return MAX_BLACK_FRIENDS - GetCount(type);
                case FriendType.Request:
                    return MAX_REQUEST_FRIENDS - GetCount(type);
                default:
                case FriendType.Recommand:
                    return MAX_RECOMMAND_COUNT - GetCount(type);
            }
        }

        public void ClearFriendList(FriendType type)
        {
            data.getFriends(type).Clear();
        }


        public SocialResult RemoveFriendAt(FriendType type, int index)
        {
            if (type == FriendType.Recommand)
                return SocialResult.InvalidOperation;

            var friends = data.getFriends(type);
            if (index < 0 || index >= friends.Count)
                return SocialResult.InvalidOperation;

            var f = friends[index];

            allMap.Remove(f.id);

            for (int x2 = index + 1; x2 < friends.Count; x2++)
            {
                var id = friends[x2].id;
                var info = allMap[id];
                info.index--;
                allMap[id] = info;
            }

            name2id.Remove(f.name);
            friends.RemoveAt(index);
            return SocialResult.Success;
        }

        public SocialResult RemoveFriend(FriendType type, string charaID)
        {
            if (type == FriendType.Recommand)
                return SocialResult.InvalidOperation;

            IndexInfo x;
            if (!allMap.TryGetValue(charaID, out x))
                return SocialResult.PlayerIdNotFound;

            var friends = data.getFriends(x.type);
            var f = friends[x.index];

            allMap.Remove(charaID);

            for (int x2 = x.index + 1; x2 < friends.Count; x2++)
            {
                var id = friends[x2].id;
                var info = allMap[id];
                info.index--;
                allMap[id] = info;
            }

            name2id.Remove(f.name);
            friends.RemoveAt(x.index);
            return SocialResult.Success;
        }


        //要把陣列元素移除後該元素後面所有的元素的索引向前移動1
        void moveForward(SocialFriendList list, int index)
        {
            for (int i = index + 1; i < list.Count; i++)
            {
                var id = list[i].id;
                var info = allMap[id];
                info.index--;
                allMap[id] = info;
            }
        }

        public SocialResult AddFriend(FriendType type, string charaID, string charaName)
        {
            //已經新增過
            if (allMap.ContainsKey(charaID))
                return SocialResult.AlreadyAdded;
            var list = data.getFriends(type);

            //如果達到最大數量
            if (list.Count >= data.getFriendMaxCount(type))
            {
                //如果建議名單已滿，則刪除第一個然後加在最尾端
                if (type == FriendType.Recommand)
                {
                    var first = list.GetFirst();
                    allMap.Remove(first.id);
                    moveForward(list, 0);
                    name2id.Remove(first.name);
                    list.RemoveAt(0);
                }
                else
                    return SocialResult.ListFull;
            }
            allMap.Add(charaID, new IndexInfo(type, list.Count));
            name2id.Add(charaName, charaID);
            list.Add(new SocialFriend(type,  charaID, charaName));
            return SocialResult.Success;
        }

        /// <summary>
        /// 檢查建議清單是否因日期而重設清單
        /// </summary>
        /// <returns>因為日期而重設清單的話傳回false</returns>
        public bool CheckRecommandList()
        {
            if (NeedClearRecommand())
            {
                data.recommandFriends.Clear();
                WriteDate();
                return false;
            }
            return true;
        }

        bool NeedClearRecommand()
        {
            DateTime dt, now = DateTime.Now;
            string checkdate = data.checkdate;
            return (!Parse(checkdate, out dt) || (dt.Year != now.Year || dt.Year != now.Month || dt.Year != now.Day));
        }
        void WriteDate()
        {
            data.checkdate = DateTime.Now.ToString(DateTimeFormat_Default);
        }

        #endregion
    }

    #region removed future
    //public class SocialInfoBase
    //{
    //    public string charName = "";
    //    public int portraitId = 0;
    //    public byte jobSect = 0;
    //    public byte vipLvl = 0;
    //    public int charLvl = 0;
    //    public int combatScore = 0;
    //    public int localObjIdx = 0;

    //    public SocialInfoBase(string charname, int portrait, byte job, byte viplvl, int progresslvl, int combatscore, int mLocalObjIdx)
    //    {
    //        charName = charname;
    //        portraitId = portrait;
    //        jobSect = job;
    //        vipLvl = viplvl;
    //        charLvl = progresslvl;
    //        combatScore = combatscore;
    //        localObjIdx = mLocalObjIdx;
    //    }

    //    public SocialInfoBase(string str = "")
    //    {
    //        InitFromString(str);
    //    }

    //    public void InitFromString(string str)
    //    {
    //        string[] infos = str.Split('`');
    //        if (infos.Length == 6)
    //        {
    //            int idx = 0;
    //            charName = infos[idx++];
    //            portraitId = int.Parse(infos[idx++]);
    //            jobSect = byte.Parse(infos[idx++]);
    //            vipLvl = byte.Parse(infos[idx++]);
    //            charLvl = int.Parse(infos[idx++]);
    //            combatScore = int.Parse(infos[idx++]);
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        sb.Append(charName);
    //        sb.Append("`");
    //        sb.Append(portraitId);
    //        sb.Append("`");
    //        sb.Append(jobSect);
    //        sb.Append("`");
    //        sb.Append(vipLvl);
    //        sb.Append("`");
    //        sb.Append(charLvl);
    //        sb.Append("`");
    //        sb.Append(combatScore);
    //        return sb.ToString();
    //    }
    //}

    //public class SocialInfo : SocialInfoBase
    //{
    //    public byte faction = 0;
    //    public string guildName = "";
    //    public bool isOnline = false;

    //    public SocialInfo(string charname, int portrait, byte job, byte viplvl, int progresslvl, int combatscore,
    //                      byte factiontype, string guildname, bool online, int mLocalObjIdx)
    //                    : base(charname, portrait, job, viplvl, progresslvl, combatscore, mLocalObjIdx)
    //    {
    //        faction = factiontype;
    //        guildName = guildname;
    //        isOnline = online;
    //    }

    //    public SocialInfo(string str = "")
    //    {
    //        InitFromString(str);
    //    }

    //    public new void InitFromString(string str)
    //    {
    //        string[] infos = str.Split('`');
    //        if (infos.Length == 9)
    //        {
    //            int idx = 0;
    //            charName = infos[idx++];
    //            portraitId = int.Parse(infos[idx++]);
    //            jobSect = byte.Parse(infos[idx++]);
    //            vipLvl = byte.Parse(infos[idx++]);
    //            charLvl = int.Parse(infos[idx++]);
    //            combatScore = int.Parse(infos[idx++]);
    //            faction = byte.Parse(infos[idx++]);
    //            guildName = infos[idx++];
    //            isOnline = bool.Parse(infos[idx++]);
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        sb.Append(charName);
    //        sb.Append("`");
    //        sb.Append(portraitId);
    //        sb.Append("`");
    //        sb.Append(jobSect);
    //        sb.Append("`");
    //        sb.Append(vipLvl);
    //        sb.Append("`");
    //        sb.Append(charLvl);
    //        sb.Append("`");
    //        sb.Append(combatScore);
    //        sb.Append("`");
    //        sb.Append(faction);
    //        sb.Append("`");
    //        sb.Append(guildName);
    //        sb.Append("`");
    //        sb.Append(isOnline);
    //        return sb.ToString();
    //    }
    //}
    #endregion




}
