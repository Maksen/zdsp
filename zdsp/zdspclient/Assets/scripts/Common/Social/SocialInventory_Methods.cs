using System.Text;
using System;
using System.Collections.Generic;
using Zealot.Common.Datablock;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zealot.DebugTools;
using Zealot.Common.Entities;
using Zealot.Common.Entities.Social;
using MsgType = Zealot.Common.Datablock.AdvancedLocalObject.MessageType;

using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Zealot.Common
{
    public partial class SocialInventoryData : IJsonInventoryData
    {
        public SocialInventoryData()
        {

        }

        public void LoadDataFromJsonString(string data)
        {
            JObject _root = null;
            try { _root = JsonConvert.DeserializeObject<JToken>(data) as JObject; }
            catch { };
            if (_root == null)
                _root = new JObject();

            JToken ver;
            if (!_root.TryGetValue("version", out ver) || ver.Type != JTokenType.Integer || ver.Value<int>() < Social_Schema.VERSION)
            {
                _root = AdvancedLocalObject.Validate<Social_Schema>(_root) as JObject;
            }

            root = _root;

            this.data = new Entities.Social.SocialData(root);
        }


        public string SaveDataToJsonString()
        {
            string result = data.BuildRecordsString();

            if (string.IsNullOrEmpty(result))
            {
                data.OnUpdateNewRoot(new JObject());
                result = JsonConvert.SerializeObject(data.Root, Formatting.None);
            }

            return result;
        }

    }
    namespace Entities.Social
    {
        public partial class SocialData
        {
            #region Internal Methods
            public void BeforePacth( MsgType op, string path, object key)
            {
                if (!doMap)
                    return;

                if (op == MsgType.Update)
                {
                    if (path != string.Empty)
                    {
                        PathCapture_ElementOfArray(path, (listName, i) =>
                        {
                            SocialFriendList list = getFriends(listName);
                            if (list != null)
                            {
                                allMap.Remove(list[i].id);
                                name2id.Remove(list[i].name);
                            }
                        }, name =>
                        {
                            if (name == "tempFriends")
                            {
                                for (int i = 0; i < tempFriends.Count; i++)
                                {
                                    var f = tempFriends[i];
                                    allMap.Remove(f.id);
                                    name2id.Remove(f.name);
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
                        SocialFriendList list = getFriends(listName);
                        if (list != null)
                        {
                            allMap.Remove(list[i].id);
                            moveForward(list, i);
                            name2id.Remove(list[i].name);
                        }
                    }, null);
                }
                else if (op == MsgType.RemoveList)
                {
                    int[] indices = AdvancedLocalObject.List_S2I((string[])key);
                    removeIndices(getFriends(path), indices);
                }
            }

            public void AfterPacth(MsgType op, string path, object key)
            {
                if (!doMap)
                    return;
                if (op == MsgType.Update)
                {
                    if (path != string.Empty)
                    {
                        PathCapture_ElementOfArray(path, (listName, i) =>
                        {
                            SocialFriendList list = getFriends(listName);

                            if (list != null)
                            {
                                SocialFriend f = list[i];
                                allMap.Add(f.id, new IndexInfo(getFriendType(listName), i));
                                name2id.Add(f.name, f.id);
                            }

                        }, name =>
                        {
                            if (name == "tempFriends")
                            {
                                for (int i = 0; i < tempFriends.Count; i++)
                                {
                                    var f = tempFriends[i];
                                    allMap.Add(f.id, new IndexInfo(FriendType.Temp, i));
                                    name2id.Add(f.name, f.id);
                                }
                            }
                        });

                    }
                }
                else if (op == MsgType.Add)
                {
                    SocialFriendList list = getFriends(path);
                    if (list != null)
                    {
                        SocialFriend f = list.GetTail();
                        allMap.Add(f.id, new IndexInfo(getFriendType(path), goodFriends.Count - 1));
                        name2id.Add(f.name, f.id);
                    }
                }
                else if (op == MsgType.Remove)
                {

                }
                else if (op == MsgType.RemoveList)
                {
                    SocialFriendList list = getFriends(path);
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
                    if (name2id.ContainsKey(name))
                    {
                        var x = allMap[name2id[name]];
                        info = getFriends(x.type)[x.index];
                        return true;
                    }
                }
                else
                {
                    var lists = new SocialFriendList[] {goodFriends,blackFriends,requestFriends, tempFriends };
                    for(int j=0;j< lists.Length;j++)
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
                        return SocialInventoryData.SYS_MAX_GOOD_FRIENDS;
                    case FriendType.Black:
                        return SocialInventoryData.SYS_MAX_BLACK_FRIENDS;
                    case FriendType.Request:
                        return SocialInventoryData.SYS_MAX_REQUEST_FRIENDS;
                    default:
                    case FriendType.Temp:
                        return SocialInventoryData.SYS_MAX_TEMP_COUNT;
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
                    case "tempFriends":
                        return tempFriends;
                }
            }
            #endregion

            #region MapMethods
            public void BuildNameMap()
            {
                if (doMap)
                {
                    //create name to index map
                    name2id.Clear();
                    allMap.Clear();

                    buildIndexMap(goodFriends, allMap);
                    buildIndexMap(blackFriends, allMap);
                    buildIndexMap(requestFriends, allMap);
                    buildIndexMap(tempFriends, allMap);

                    buildName2IdMap(goodFriends, name2id);
                    buildName2IdMap(blackFriends, name2id);
                    buildName2IdMap(requestFriends, name2id);
                    buildName2IdMap(tempFriends, name2id);
                }
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

            void removeIndices(SocialFriendList friends, int[] indices)
            {
                for (int i = 0; i < indices.Length; i++)
                {
                    int index = indices[i];
                    if (index < 0 || index >= friends.Count)
                        continue;
                    var f = friends[index];
                    allMap.Remove(f.id);
                    name2id.Remove(f.name);
                }
            }

            void reIndexFriends(SocialFriendList friends, int from)
            {
                for (int i = from; i < friends.Count; i++)
                {
                    var id = friends[i].id;
                    var info = allMap[id];
                    info.index = i;
                    allMap[id] = info;
                }
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
            #endregion

            #region Operations Methods
            public void ClearFriendList(FriendType type)
            {
                getFriends(type).Clear();
            }

            public SocialResult RemoveFriendByIndices(FriendType type, int[] indices)
            {
                if (type == FriendType.Temp)
                    return SocialResult.InvalidOperation;

                var friends = getFriends(type);
                if (doMap) removeIndices(friends, indices);
                friends.RemoveList(indices);
                if (doMap) reIndexFriends(friends, 0);

                return SocialResult.Success;
            }

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
                    allMap.Remove(f.id);
                    name2id.Remove(f.name);
                }
                friends.RemoveAt(index);
                if (doMap) reIndexFriends(friends, index);

                return SocialResult.Success;
            }

            public SocialResult RemoveFriendByName(FriendType type, string charaName)
            {
                if (doMap)
                {
                    IndexInfo x;
                    string charaID;
                    if (!name2id.TryGetValue(charaName, out charaID))
                        return SocialResult.PlayerNameNotFound;
                    if (!allMap.TryGetValue(charaID, out x))
                        return SocialResult.PlayerNameNotFound;
                    if (x.type != type)
                        return SocialResult.PlayerNameNotFound;

                    var friends = getFriends(type);
                    int index = x.index;
                    var f = friends[index];

                    allMap.Remove(charaID);
                    name2id.Remove(f.name);
                    friends.RemoveAt(index);
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
                        return SocialResult.PlayerNameNotFound;

                    friends.RemoveAt(index);
                }
                return SocialResult.Success;
            }

            public SocialResult RemoveFriendById(FriendType type, string charaID)
            {
                if (doMap)
                {
                    IndexInfo x;
                    if (!allMap.TryGetValue(charaID, out x))
                        return SocialResult.PlayerIdNotFound;
                    if (x.type != type)
                        return SocialResult.PlayerIdNotFound;

                    var friends = getFriends(type);
                    int index = x.index;
                    var f = friends[index];

                    allMap.Remove(charaID);
                    name2id.Remove(f.name);
                    friends.RemoveAt(index);
                    reIndexFriends(friends, index);
                }
                else
                {
                    var friends = getFriends(type);
                    int index = -1;
                    SocialFriend f;
                    for (int i=0;i< friends.Count;i++)
                    {
                        f = friends[i];
                        if (f.id == charaID)
                        {
                            index = i;
                            break;
                        }
                    }
                    if(index<0)
                        return SocialResult.PlayerIdNotFound;

                    friends.RemoveAt(index);
                }
                return SocialResult.Success;
            }

            public SocialResult AddFriend(FriendType type, string charaID, string charaName)
            {
                var list = getFriends(type);
                //已經新增過
                if (doMap)
                {
                    if (allMap.ContainsKey(charaID))
                        return SocialResult.AlreadyAdded;
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                        if (list[i].id == charaID)
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
                            allMap.Remove(first.id);
                            moveForward(list, 0);
                            name2id.Remove(first.name);
                        }
                        list.RemoveAt(0);
                    }
                    else
                        return SocialResult.ListFull;
                }
                if (doMap)
                {
                    allMap.Add(charaID, new IndexInfo(type, list.Count));
                    name2id.Add(charaName, charaID);
                }
                list.Add(new SocialFriend(type, charaID, charaName));
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
                    if(doMap)
                    {
                        BuildNameMap();
                    }

                    WriteDate();
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

        }
    }

    
}