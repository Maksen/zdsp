using System.Text;
using System;
using System.Collections.Generic;
using Zealot.Common.Datablock;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zealot.DebugTools;
using Zealot.Common.Entities;
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
            JToken _root = null;
            try { _root = JsonConvert.DeserializeObject<JToken>(data); }
            catch { };
            if (_root == null || _root.Type != JTokenType.Object)
                _root = new JObject();
            root = _root;

            this.data = new Entities.Social.SocialData(root);
        }


        public string SaveDataToJsonString()
        {
            string result = null;
            try { result = JsonConvert.SerializeObject(data.BuildRecords()); }
            catch { }

            if (string.IsNullOrEmpty(result))
            {
                data.OnUpdateNewRoot(new JObject());
                result = JsonConvert.SerializeObject(data.Root);
            }

            return result;
        }

    }
    namespace Entities.Social
    {
        public partial class SocialData
        {
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
                            if (name == "recommandFriends")
                            {
                                for (int i = 0; i < recommandFriends.Count; i++)
                                {
                                    var f = recommandFriends[i];
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
                            if (name == "recommandFriends")
                            {
                                for (int i = 0; i < recommandFriends.Count; i++)
                                {
                                    var f = recommandFriends[i];
                                    allMap.Add(f.id, new IndexInfo(FriendType.Recommand, i));
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

            public bool GetFriendInfo(string name, out SocialFriend info)
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
                    var lists = new SocialFriendList[] {goodFriends,blackFriends,requestFriends,recommandFriends };
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


            
            public int GetCount(FriendType type)
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
                        return recommandFriends.Count;
                }
            }
            public int GetAvailableCount(FriendType type)
            {
                switch (type)
                {
                    case FriendType.Good:
                        return SocialStats.MAX_GOOD_FRIENDS - GetCount(type);
                    case FriendType.Black:
                        return SocialStats.MAX_BLACK_FRIENDS - GetCount(type);
                    case FriendType.Request:
                        return SocialStats.MAX_REQUEST_FRIENDS - GetCount(type);
                    default:
                    case FriendType.Recommand:
                        return SocialStats.MAX_RECOMMAND_COUNT - GetCount(type);
                }
            }

            public void ClearFriendList(FriendType type)
            {
                getFriends(type).Clear();
            }

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
                    buildIndexMap(recommandFriends, allMap);

                    buildName2IdMap(goodFriends, name2id);
                    buildName2IdMap(blackFriends, name2id);
                    buildName2IdMap(requestFriends, name2id);
                    buildName2IdMap(recommandFriends, name2id);
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

            public SocialResult RemoveFriendByIndices(FriendType type, int[] indices)
            {
                if (type == FriendType.Recommand)
                    return SocialResult.InvalidOperation;

                var friends = getFriends(type);
                if (doMap) removeIndices(friends, indices);
                friends.RemoveList(indices);
                if (doMap) reIndexFriends(friends, 0);

                return SocialResult.Success;
            }

            public SocialResult RemoveFriendAt(FriendType type, int index)
            {
                if (type == FriendType.Recommand)
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
                    //如果建議名單已滿，則刪除第一個然後加在最尾端
                    if (type == FriendType.Recommand)
                    {
                        var first = list.GetFirst();
                        if (doMap)
                        {
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
                    recommandFriends.Clear();
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
                return (!AdvancedLocalObject.Parse(_checkdate, out dt) || (dt.Year != now.Year || dt.Year != now.Month || dt.Year != now.Day));
            }
            void WriteDate()
            {
                checkdate = DateTime.Now.ToString(AdvancedLocalObject.DateTimeFormat_Default);
            }

        }
    }

    
}