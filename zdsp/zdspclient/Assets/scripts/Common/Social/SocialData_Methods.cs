using System;
using System.Collections.Generic;
using Zealot.Common.Datablock;
using Newtonsoft.Json.Linq;
using MsgType = Zealot.Common.Datablock.AdvancedLocalObject.MessageType;

namespace Zealot.Common.Entities.Social
{
    public partial class SocialData : AdvancedLocalObjectData
    {
        #region Constructors
        /// <summary>
        /// 給server端的stats使用，因為整塊資料都與Inventory相同，故使用參考連結方式，差別在於會將資料送給客戶端
        /// </summary>
        /// <param name="root"></param>
        public SocialData(SocialData data, AdvancedLocalObject obj) : base(obj, data.Root)
        {
            this.status = data.status;
            isClient = false;
            //先暫時不要建立索引，因為目前還沒需要大量使用的時機
            //doMap = true;
            //allMap = new Dictionary<string, IndexInfo>();
            //name2id = new Dictionary<string, string>();
            data.AffectEntities.Add(this);
            OnUpdateRootValue += () => { initFromRoot(m_Root, obj); };
            UpdateRootValue();

            updateStates = true;
        }

        /// <summary>
        /// for client
        /// </summary>
        public SocialData() : base(null, new JObject())
        {
            status = new DataStatus();
            status.isOnline = false;
            status.isPlayerData = false;
            isClient = true;
            doMap = false;
            OnUpdateRootValue += () => { initFromRoot(m_Root, null); };
        }

        /// <summary>
        /// 給server端的Inventory使用
        /// </summary>
        /// <param name="root"></param>
        public SocialData(JToken root, bool isPlayerData) : base(null, validtae(root))
        {
            status = new DataStatus();
            status.isOnline = isPlayerData;
            status.isPlayerData = isPlayerData;
            isClient = false;
            doMap = false;
            OnUpdateRootValue += () => { initFromRoot(m_Root, null); };
            UpdateRootValue();
        }
        #endregion

        #region Internal Methods
        public void BeforePacth(MsgType op, string path, object key)
        {
            if (!doMap)
                return;
            //目前底下的code都不會被執行到，因為client端設定為不需要建hash
            if (op == MsgType.Update)
            {
                if (path != string.Empty)
                {
                    PathCapture_ElementOfArray(path, (listName, i) =>
                    {
                        SocialFriendStateList list = getFriendStates(getFriendType(listName));
                        if (list != null)
                        {
                            allMap.Remove(list[i].name);
                        }
                    }, name =>
                    {
                        if (name == "tempFriends")
                        {
                            for (int i = 0; i < tempFriendStates.Count; i++)
                            {
                                var f = tempFriendStates[i];
                                allMap.Remove(f.name);
                            }
                        }
                    });
                }
            }
            else if (op == MsgType.Add)
            {
            }
            else if (op == MsgType.Remove)
            {
                PathCapture_ElementOfArray(path, (listName, i) =>
                {
                    SocialFriendStateList list = getFriendStates(getFriendType(listName));
                    if (list != null)
                    {
                        allMap.Remove(list[i].name);
                        moveForward(list, i);
                    }
                }, null);
            }
            else if (op == MsgType.RemoveList)
            {
                int[] indices = AdvancedLocalObject.List_S2I((string[])key);
                removeIndices(getFriends(getFriendType(path)), indices);
            }
        }

        public void AfterPacth(MsgType op, string path, object key)
        {
            if (!doMap)
                return;
            //目前底下的code都不會被執行到，因為client端設定為不需要建hash
            if (op == MsgType.Update)
            {
                if (path != string.Empty)
                {
                    PathCapture_ElementOfArray(path, (listName, i) =>
                    {
                        FriendType type = getFriendType(listName);
                        SocialFriendStateList list = getFriendStates(type);

                        if (list != null)
                        {
                            var f = list[i];
                            allMap.Add(f.name, new IndexInfo(type, i));
                        }

                    }, name =>
                    {
                        if (name == "tempFriends")
                        {
                            for (int i = 0; i < tempFriendStates.Count; i++)
                            {
                                var f = tempFriendStates[i];
                                allMap.Add(f.name, new IndexInfo(FriendType.Temp, i));
                            }
                        }
                    });

                }
            }
            else if (op == MsgType.Add)
            {
                FriendType type = getFriendType(path);
                SocialFriendStateList list = getFriendStates(type);
                if (list != null)
                {
                    var f = list.GetTail();
                    allMap.Add(f.name, new IndexInfo(type, goodFriends.Count - 1));
                }
            }
            else if (op == MsgType.Remove)
            {

            }
            else if (op == MsgType.RemoveList)
            {
                SocialFriendStateList list = getFriendStates(getFriendType(path));
                if (list != null)
                {
                    reIndexFriends(list, 0);
                    //BuildNameMap();
                }
            }

        }
        #endregion

        #region Info Methods
        public bool findFriendInfo(string name, out SocialFriend info)
        {
            if (doMap)
            {
                IndexInfo x;
                if (allMap.TryGetValue(name, out x))
                {
                    info = getFriends(x.type)[x.index];
                    return true;
                }
            }
            else
            {
                var lists = new SocialFriendList[] { goodFriends, blackFriends, requestFriends, tempFriends };
                for (int j = 0; j < lists.Length; j++)
                {
                    SocialFriendList list = lists[j];
                    for (int i = 0; i < list.Count; i++)
                    {
                        info = list[i];
                        if (info.name == name)
                            return true;
                    }
                }
            }
            info = new SocialFriend();
            return false;
        }

        public int getCount(FriendType type)
        {
            switch (type)
            {
                case FriendType.Good:
                    return goodFriends.Count;
                case FriendType.Black:
                    return blackFriends.Count;
                case FriendType.Request:
                    return requestFriends.Count;
                default:
                    return tempFriends.Count;
            }
        }

        public int getAvailableCount(FriendType type)
        {
            switch (type)
            {
                case FriendType.Good:
                    return maxCount_good - getCount(type);
                case FriendType.Black:
                    return maxCount_black - getCount(type);
                case FriendType.Request:
                    return maxCount_request - getCount(type);
                default:
                case FriendType.Temp:
                    return maxCount_temp - getCount(type);
            }
        }

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
                case FriendType.Temp:
                    return tempFriends;
            }
        }
        public SocialFriendStateList getFriendStates(FriendType type)
        {
            switch (type)
            {
                case FriendType.Good:
                    return goodFriendStates;
                case FriendType.Black:
                    return blackFriendStates;
                case FriendType.Request:
                    return requestFriendStates;
                default:
                case FriendType.Temp:
                    return tempFriendStates;
            }
        }

        public int getFriendMaxCount(FriendType type)
        {
            switch (type)
            {
                case FriendType.Good:
                    return maxCount_good;
                case FriendType.Black:
                    return maxCount_black;
                case FriendType.Request:
                    return maxCount_request;
                default:
                case FriendType.Temp:
                    return maxCount_temp;
            }
        }

        public int getSystemFriendMaxCount(FriendType type)
        {
            switch (type)
            {
                case FriendType.Good:
                    return SYS_MAX_GOOD_COUNT;
                case FriendType.Black:
                    return SYS_MAX_BLACK_COUNT;
                case FriendType.Request:
                    return SYS_MAX_REQUEST_COUNT;
                default:
                case FriendType.Temp:
                    return SYS_MAX_TEMP_COUNT;
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
                case "tempFriends":
                    return FriendType.Temp;
            }
        }
        #endregion

        #region MapMethods
        public void BuildNameMap()
        {
            if (doMap)
            {
                //create name to index map
                allMap.Clear();

                if (isClient)
                {
                    buildIndexMap(goodFriendStates, FriendType.Good, allMap);
                    buildIndexMap(blackFriendStates, FriendType.Black, allMap);
                    buildIndexMap(requestFriendStates, FriendType.Request, allMap);
                    buildIndexMap(tempFriendStates, FriendType.Temp, allMap);
                }
                else
                {
                    buildIndexMap(goodFriends, allMap);
                    buildIndexMap(blackFriends, allMap);
                    buildIndexMap(requestFriends, allMap);
                    buildIndexMap(tempFriends, allMap);
                }
            }
        }

        static void buildIndexMap(SocialFriendStateList list, FriendType type, Dictionary<string, IndexInfo> map)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var f = list[i];
                if (!map.ContainsKey(f.name))
                    map.Add(f.name, new IndexInfo(type, i));
            }
        }

        static void buildIndexMap(SocialFriendList list, Dictionary<string, IndexInfo> map)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var f = list[i];
                if (!map.ContainsKey(f.name))
                    map.Add(f.name, new IndexInfo(list.Type, i));
            }
        }

        void removeIndices(SocialFriendList friends, int[] indices)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];
                if (index < 0 || index >= friends.Count)
                    continue;
                var f = friends[index];
                allMap.Remove(f.name);
            }
        }

        void reIndexFriends(SocialFriendStateList friends, int from)
        {
            for (int i = from; i < friends.Count; i++)
            {
                var name = friends[i].name;
                var info = allMap[name];
                info.index = i;
                allMap[name] = info;
            }
        }
        void reIndexFriends(SocialFriendList friends, int from)
        {
            for (int i = from; i < friends.Count; i++)
            {
                var name = friends[i].name;
                var info = allMap[name];
                info.index = i;
                allMap[name] = info;
            }
        }

        //要把陣列元素移除後該元素後面所有的元素的索引向前移動1
        void moveForward(SocialFriendList list, int index)
        {
            for (int i = index + 1; i < list.Count; i++)
            {
                var name = list[i].name;
                var info = allMap[name];
                info.index--;
                allMap[name] = info;
            }
        }
        void moveForward(SocialFriendStateList list, int index)
        {
            for (int i = index + 1; i < list.Count; i++)
            {
                var name = list[i].name;
                var info = allMap[name];
                info.index--;
                allMap[name] = info;
            }
        }
        #endregion

        #region Operations Methods

        public void Offline()
        {
            status.isOnline = false;
        }
        public bool IsOnline { get { return status.isOnline; } }
        public bool IsPlayerData { get { return status.isPlayerData; } }
        public bool IsPlayerDataAndOffline { get { return status.isPlayerData && !status.isOnline; } }

        public void SetOnModify() { status.onModify = true; }
        public void UnsetOnModify() { status.onModify = false; }
        public bool OnModify { get { return status.onModify; } }

        public void ClearFriendList(FriendType type)
        {
            getFriends(type).Clear();
            if (!skipUpdateStates && updateStates)
                getFriendStates(type).Clear();

            m_Dirty = true;
        }

        /// <returns>InvalidOperation;Success</returns>
        public SocialResult RemoveFriendByIndices(FriendType type, int[] indices)
        {
            if (type == FriendType.Temp)
                return SocialResult.InvalidOperation;

            var friends = getFriends(type);
            if (doMap) removeIndices(friends, indices);
            friends.RemoveList(indices);
            if (doMap) reIndexFriends(friends, 0);

            if (!skipUpdateStates && updateStates)
                getFriendStates(type).RemoveList(indices);

            if(indices.Length>0)
                m_Dirty = true;

            return SocialResult.Success;
        }

        /// <returns>InvalidOperation;Success</returns>
        public SocialResult RemoveFriendAt(FriendType type, int index)
        {
            if (type == FriendType.Temp)
                return SocialResult.InvalidOperation;

            var friends = getFriends(type);
            if (index < 0 || index >= friends.Count)
                return SocialResult.InvalidOperation;

            var f = friends[index];

            if (doMap)
            {
                allMap.Remove(f.name);
            }
            friends.RemoveAt(index);
            if (!skipUpdateStates && updateStates)
                getFriendStates(type).RemoveAt(index);

            if (doMap) reIndexFriends(friends, index);

            m_Dirty = true;

            return SocialResult.Success;
        }

        /// <returns>PlayerNameNotFoundInList;Success</returns>
        public SocialResult RemoveFriendByName(FriendType type, string charaName)
        {
            if (doMap)
            {
                IndexInfo x;
                if (!allMap.TryGetValue(charaName, out x))
                    return SocialResult.PlayerNameNotFoundInList;
                if (x.type != type)
                    return SocialResult.PlayerNameNotFoundInList;

                var friends = getFriends(type);
                int index = x.index;
                var f = friends[index];

                allMap.Remove(charaName);
                friends.RemoveAt(index);
                if (!skipUpdateStates && updateStates)
                    getFriendStates(type).RemoveAt(index);
                reIndexFriends(friends, index);
            }
            else
            {
                var friends = getFriends(type);
                int index = -1;
                SocialFriend f;
                for (int i = 0; i < friends.Count; i++)
                {
                    f = friends[i];
                    if (f.name == charaName)
                    {
                        index = i;
                        break;
                    }
                }
                if (index < 0)
                    return SocialResult.PlayerNameNotFoundInList;

                friends.RemoveAt(index);
                if (!skipUpdateStates && updateStates)
                    getFriendStates(type).RemoveAt(index);
            }
            m_Dirty = true;
            return SocialResult.Success;
        }

        /// <returns>PlayerIdNotFoundInList;Success</returns>
        public SocialResult RemoveFriendById(FriendType type, string charaID)
        {
            var friends = getFriends(type);

            int index = -1;
            SocialFriend f = new SocialFriend();
            for (int i = 0; i < friends.Count; i++)
            {
                f = friends[i];
                if (f.id == charaID)
                {
                    index = i;
                    break;
                }
            }
            if (index < 0)
                return SocialResult.PlayerIdNotFoundInList;

            if (doMap)
            {
                allMap.Remove(f.name);
                friends.RemoveAt(index);
                if (!skipUpdateStates && updateStates)
                    getFriendStates(type).RemoveAt(index);
                reIndexFriends(friends, index);
            }
            else
            {
                friends.RemoveAt(index);
                if (!skipUpdateStates && updateStates)
                    getFriendStates(type).RemoveAt(index);
            }
            m_Dirty = true;
            return SocialResult.Success;
        }

        /// <returns>AlreadyAdded;ListFull;Success</returns>
        public SocialResult AddFriend(FriendType type, string charaID, string charaName, Func<SocialFriendState> onGetState)
        {
            var list = getFriends(type);
            //已經新增過
            if (doMap)
            {
                if (allMap.ContainsKey(charaName))
                    return SocialResult.AlreadyAdded;
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i].name == charaName)
                        return SocialResult.AlreadyAdded;
            }

            //如果達到最大數量
            if (list.Count >= getFriendMaxCount(type))
            {
                //如果暫時名單已滿，則刪除第一個然後加在最尾端
                if (type == FriendType.Temp)
                {
                    if (doMap)
                    {
                        var first = list.GetFirst();
                        allMap.Remove(first.name);
                        moveForward(list, 0);
                    }
                    list.RemoveAt(0);
                    if (!skipUpdateStates && updateStates)
                        getFriendStates(type).RemoveAt(0);
                }
                else
                    return SocialResult.ListFull;
            }
            if (doMap)
            {
                allMap.Add(charaName, new IndexInfo(type, list.Count));
            }
            list.Add(new SocialFriend(charaID, charaName));
            if (!skipUpdateStates && updateStates)
                getFriendStates(type).Add(onGetState());
            m_Dirty = true;
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
                tempFriends.Clear();
                if (!skipUpdateStates && updateStates)
                    tempFriendStates.Clear();
                if (doMap)
                {
                    BuildNameMap();
                }

                WriteDate();

                m_Dirty = true;
                return false;
            }
            return true;
        }

        bool NeedClearRecommand()
        {
            DateTime dt, now = DateTime.Now;
            string _checkdate = checkdate;
            return (!AdvancedLocalObject.Parse(_checkdate, out dt) || (dt.Year != now.Year || dt.Month != now.Month || dt.Day != now.Day));
        }

        void WriteDate()
        {
            checkdate = DateTime.Now.ToString(AdvancedLocalObject.DateTimeFormat_Default);
        }

        #endregion

        #region Private Fields
        static JToken validtae(JToken _root)
        {
            JObject root = _root as JObject;
            if (root == null)
                root = new JObject();
            JToken ver;
            int verCode = 0;
            if (!root.TryGetValue("version", out ver) ||
                ver.Type != JTokenType.Integer ||
                (verCode = (ver as JValue).Int32()) < VERSION)
            {
                root = Migrate(SocialData.Debug, root, verCode) as JObject;

                //簡單驗證, 無法驗證邏輯上的資料錯誤
                root = GameUtils.Validate<SocialData_Schema>(root) as JObject;
                root["version"] = VERSION;
            }

            return root;
        }

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

        private bool doMap = false,isClient;
        private bool updateStates = false, skipUpdateStates = true;
        
        private Dictionary<string, IndexInfo> allMap;
        private DataStatus status;
        public void SkipUpdateStates()
        {
            skipUpdateStates = true;
        }
        public void ResumeUpdateStates()
        {
            skipUpdateStates = false;
        }

        class DataStatus
        {
            public bool onModify, isOnline, isPlayerData;
        }

        #endregion
    }

    
}