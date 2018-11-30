using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.Common.Entities.Social;
using Zealot.Common.Datablock;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.LoadBalancing.GameServer.Extensions;
using System.Threading;
using Zealot.Common.Entities;
using Zealot.Server.Counters;
using Photon.LoadBalancing.GameServer.MultiServer;
using ExitGames.Diagnostics.Counter;
using ExitGames.Concurrency.Fibers;

#pragma warning disable CS0162

namespace Photon.LoadBalancing.GameServer
{
    public class SocialController
    {

        #region Public Social API
        /// <summary>
        /// 將玩家B加入玩家A的臨時好友清單
        /// </summary>
        /// <param name="playerName">玩家A的名稱</param>
        /// <param name="friendName">玩家B的名稱</param>
        public void AddTempFriendsSingle(string name1, string name2, Action<SocialAddTempFriends_Result> completed)
        {
            _AddTempFriendsSingle(name1, name2, completed);
        }
        /// <summary>
        /// 將玩家互相加入對方的臨時好友清單
        /// </summary>
        /// <param name="name1">玩家1</param>
        /// <param name="name2">玩家2</param>
        public void AddTempFriendsBoth(string name1, string name2, Action<SocialAddTempFriends_Result> completed)
        {
            _AddTempFriendsBoth(name1, name2, completed);
        }
        #endregion

        #region Configs
        static double CONFIG_OpenMenuMinInterval_Seconds = 30;
        static bool CONFIG_DEBUG_MODE = SocialData.Debug;
        const bool ENABLE_QUEUE_COUNTER = false;
        #endregion

        #region Constructor
        public SocialController(GameClientPeer peer)
        {
            if (m_StaticNeedInit)
            {
                m_StaticNeedInit = false;
                ExeQueueCounter = GameCounters.ExecutionFiberQueue;
                ExeQueue = GameApplication.Instance.executionFiber;

                MultiServerAPIManager.Instance.RegisterAPI("SocialAcceptRequest_MIRROR", async (difSvr, args) =>
                             new object[] { await SocialAcceptRequest_MIRROR(difSvr, (string)args[0], (string)args[1], (string)args[2]) }, true);
            }

            this.peer = peer;

            me_state = new SocialDataHandler(MultiServerAPIManager.Instance, false, true, true, false);
            me_single = new SocialDataHandler(MultiServerAPIManager.Instance, true, true, true, false);
            fr_single = new SocialDataHandler(MultiServerAPIManager.Instance, true, true, true, false);
            fr_states = new SocialDataHandler(MultiServerAPIManager.Instance, false, true, true, true);
            fr_multi = new SocialDataHandler(MultiServerAPIManager.Instance, true, true, true, true);
            fr_mdata = new SocialDataHandler(MultiServerAPIManager.Instance, true, false, false, true);
        }
        #endregion

        #region Static Methods

        internal static SocialData GetData(GameClientPeer peer)
        {
            var player = peer.mPlayer;
            if (player != null)
            {
                var stats = player.SocialStats;
                if (stats != null && stats.StatsLoaded)
                    return stats.data;
            }
            return peer.mSocialInventory.data;
        }

        internal static GameClientPeer GetPeer(string name)
        {
            var peer = GameApplication.Instance.GetCharPeer(name);
            if (peer != null)
            {
                if (peer.mSocialController == null)
                    return null;
                return peer;
            }
            return null;
        }

        #endregion

        #region Private Fileds
        static PoolFiber ExeQueue;
        static NumericCounter ExeQueueCounter;
        static bool m_StaticNeedInit=true;

        SocialDataHandler me_state;
        SocialDataHandler me_single;
        SocialDataHandler fr_single;
        SocialDataHandler fr_states;
        SocialDataHandler fr_multi;
        SocialDataHandler fr_mdata;

        bool DebugMode = true;
        private GameClientPeer peer;

        private DateTime lastOpenMenuTime = new DateTime(1950, 1, 1);
        readonly static FriendType[] m_ListTypes = new FriendType[] { FriendType.Good, FriendType.Black, FriendType.Request, FriendType.Temp };

        static Dictionary<string, SocialFriendState> m_StateCache = new Dictionary<string, SocialFriendState>();
        #endregion

        #region Private Methods


        static void AddName(List<string> names, SocialFriendList friends)
        {
            foreach (var item in friends)
                names.Add(item.name);
        }

        static void RemoveNotExist(HashSet<string> ids, HashSet<string> removed, SocialFriendList friends)
        {
            for (int i = friends.Count - 1; i >= 0; i--)
            {
                var f = friends[i];
                if (!ids.Contains(f.id))
                {
                    removed.Add(f.id);
                    friends.RemoveAt(i);
                }
            }
        }

        private void _AddTempFriendsBoth(string name1, string name2, Action<SocialAddTempFriends_Result> completed)
        {
            if (completed == null)
                throw new ArgumentNullException("completed");

            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (name1 == name2)
                {
                    completed(SocialAddTempFriends_Result.SameName);
                    return;
                }

                SocialData data1;
                SocialData data2;
                string id1, id2;
                GameClientPeer peer1, peer2;
                Func<SocialFriendState> getState1;
                Func<SocialFriendState> getState2;

                bool doSave1 = false;
                bool doSave2 = false;

                peer1 = GetPeer(name1);
                if (peer1 != null)
                {
                    data1 = GetData(peer1);
                    id1 = peer1.GetCharId();
                    getState1 = () => LoadCharaStateFromPeer(peer1);
                }
                else
                {
                    var tp = await LoadAllInfoFromDB(name1);
                    data1 = tp.data;
                    id1 = tp.id;
                    getState1 = tp.getState;
                    if (data1 == null)
                        return;
                    doSave1 = true;
                }

                peer2 = GetPeer(name2);
                if (peer2 != null)
                {
                    data2 = GetData(peer2);
                    id2 = peer2.GetCharId();
                    getState2 = () => LoadCharaStateFromPeer(peer2);
                }
                else
                {
                    var tp = await LoadAllInfoFromDB(name2);
                    data2 = tp.data;
                    id2 = tp.id;
                    getState2 = tp.getState;
                    if (data2 == null)
                        return;
                    doSave2 = true;
                }

                //臨時名單不能在玩家的好友名單內
                if (data1.goodFriends.ContainsPlayerName(name2))
                {
                    completed(SocialAddTempFriends_Result.InGood);
                    return;
                }

                //臨時名單不能在玩家的黑名單內
                if (data1.blackFriends.ContainsPlayerName(name2))
                {
                    completed(SocialAddTempFriends_Result.InBlack);
                    return;
                }
                if (data2.blackFriends.ContainsPlayerName(name1))
                {
                    completed(SocialAddTempFriends_Result.InBlack);
                    return;
                }

                data1.CheckRecommandList();
                data2.CheckRecommandList();

                data1.AddFriend(FriendType.Temp, id2, name2, getState2);
                data2.AddFriend(FriendType.Temp, id1, name1, getState1);

                if (doSave1)
                    await Social_SaveSocialDataToDB(data1, name1);
                if (doSave2)
                    await Social_SaveSocialDataToDB(data2, name2);

                completed(SocialAddTempFriends_Result.Success);
            });
        }

        private void _AddTempFriendsSingle(string name1, string name2, Action<SocialAddTempFriends_Result> completed)
        {
            if (completed == null)
                throw new ArgumentNullException("completed");

            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (name1 == name2)
                {
                    completed(SocialAddTempFriends_Result.SameName);
                    return;
                }

                SocialData data1;
                SocialData data2;
                Func<SocialFriendState> getState2;
                string id1, id2;
                GameClientPeer peer1, peer2;

                bool doSave1 = false;

                peer1 = GetPeer(name1);
                if (peer1 != null)
                {
                    data1 = GetData(peer1);
                    id1 = peer1.GetCharId();
                }
                else
                {
                    var tp = await LoadSocialDataFromDBWithId(name1);
                    data1 = tp.Item1;
                    id1 = tp.Item2;
                    if (data1 == null)
                    {
                        completed(SocialAddTempFriends_Result.Player1NameNotFound);
                        return;
                    }
                    doSave1 = true;
                }

                peer2 = GetPeer(name2);
                if (peer2 != null)
                {
                    data2 = GetData(peer2);
                    id2 = peer2.GetCharId();
                    getState2 = () => LoadCharaStateFromPeer(peer2);
                }
                else
                {
                    var tp = await LoadAllInfoFromDB(name2);
                    data2 = tp.data;
                    id2 = tp.id;
                    getState2 = tp.getState;
                    if (data2 == null)
                    {
                        completed(SocialAddTempFriends_Result.Player2NameNotFound);
                        return;
                    }
                }

                //臨時名單不能在玩家的好友名單內
                if (data1.goodFriends.ContainsPlayerName(name2))
                {
                    completed(SocialAddTempFriends_Result.InGood);
                    return;
                }

                //臨時名單不能在玩家的黑名單內
                if (data1.blackFriends.ContainsPlayerName(name2))
                {
                    completed(SocialAddTempFriends_Result.InBlack);
                    return;
                }
                if (data2.blackFriends.ContainsPlayerName(name1))
                {
                    completed(SocialAddTempFriends_Result.InBlack);
                    return;
                }

                data1.CheckRecommandList();

                data1.AddFriend(FriendType.Temp, id2, name2, getState2);

                if (doSave1)
                    await Social_SaveSocialDataToDB(data1, name1);

                completed(SocialAddTempFriends_Result.Success);
            });
        }

        bool checkIsSelf(string friendName, Action<int, GameClientPeer> act)
        {
            if (peer.CharacterData.Name == friendName)
            {
                peer.SendSystemMessage("ret_social_PlayerCantBeSelf", false);
                //發送結果
                act((int)SocialResult.PlayerCantBeSelf, peer);

                return true;
            }
            return false;
        }
        bool checkIsSelf(string friendName, Action<int, string, GameClientPeer> act)
        {
            if (peer.CharacterData.Name == friendName)
            {
                peer.SendSystemMessage("ret_social_PlayerCantBeSelf", false);
                //發送結果
                act((int)SocialResult.PlayerCantBeSelf, friendName, peer);

                return true;
            }
            return false;
        }
        


        static string GetChannelInfo()
        {
            JObject json = new JObject();
            json["cid"] = GameApplication.Instance.clusterPeer.ConnectionId;
            json["serverid"] = GameApplication.ServerId;
            return json.ToString(Formatting.None);
        }

        /// <summary>
        /// 取得離線時間
        /// </summary>
        internal static string GetOfflineTime(DateTime time)
        {
            return Zealot.Repository.GUILocalizationRepo.GetLocalizedSingleUnitTimeStringMinute((DateTime.Now - time).TotalSeconds);
        }

        internal static string GetChannel(string name)
        {
            var block = GameApplication.Instance.GetCharDatablock(name);
            if (block != null)
                return block.server;
            else
                return string.Empty;
        }
        internal static CharacterOnlineData GetOnlineData(string name)
        {
            return GameApplication.Instance.GetCharDatablock(name);
        }


        #region Private Removed Future
        static SocialFriendState LoadCharaStateFromPeer(GameClientPeer peer)
        {
            if (peer == null)
                return new SocialFriendState(null);

            string name = peer.mPlayer.Name;

            return new SocialFriendState(
                name: name,
                offlineTime: null,
                channel: GetChannel(name),
                progressLevel: peer.GetProgressLvl(),
                guildName: string.Empty,
                guildIcon: string.Empty
                );
        }
        struct CharaStateInfo
        {
            public SocialFriendState state;
            public string charid;
        }
        static async Task<List<CharaStateInfo>> LoadCharaStatesFromDB(List<string> names)
        {
            var diclist = await GameApplication.dbRepository.Character.GetSocialStateByNames(names);

            List<CharaStateInfo> results = new List<CharaStateInfo>();

            foreach (var dic in diclist)
            {
                string name = (string)dic["charname"];
                object id = dic["charid"];
                string sid = null;
                if (id is Guid)
                    sid = ((Guid)id).ToString();
                else if (id is string)
                    sid = (string)id;

                results.Add(new CharaStateInfo()
                {
                    state = new SocialFriendState(
                        name: name,
                        offlineTime: GetOfflineTime((DateTime)dic["dtlogout"]),
                        channel: GetChannel(name),
                        progressLevel: (int)dic["progresslevel"],
                        guildName: string.Empty,
                        guildIcon: string.Empty),
                    charid = sid
                });
            }

            return results;
        }

        struct AllInfo
        {
            public string id;
            public Func<SocialFriendState> getState;
            public SocialData data;
            public GameClientPeer peer;
        }

        struct SocialData_Peer
        {
            public SocialData data;
            public GameClientPeer peer;
        }

        

        //將全部必要資訊全讀避免跑多次db
        static async Task<AllInfo> LoadAllInfoFromDB(string name)
        {
            var dic = await GameApplication.dbRepository.Character.GetSocialByName(name);
            object obj;

            //如果找不到該玩家(可能是被刪除了)
            if (!dic.TryGetValue("charid", out obj))
            {
                return new AllInfo();
            }

            string charid;
            if (obj is Guid)
                charid = ((Guid)obj).ToString();
            else if (obj is string)
                charid = ((string)obj);
            else
                return new AllInfo();

            if (!dic.TryGetValue("friends", out obj))
            {
                return new AllInfo();
            }
            string jsonStr = (string)obj;
            JObject json = null;
            try { json = JsonConvert.DeserializeObject<JToken>(jsonStr) as JObject; } catch { }
            if (json == null)
                json = new JObject();


            return new AllInfo()
            {
                id = charid,
                getState = () =>
                {
                    string charname = (string)dic["charname"];
                    return new SocialFriendState(
                        name: charname,
                        offlineTime: GetOfflineTime((DateTime)dic["dtlogout"]),
                        channel: GetChannel(charname),
                        progressLevel: (int)dic["progresslevel"],
                        guildName: string.Empty,
                        guildIcon: string.Empty);
                },
                data = new SocialData(json, false),
            };
        }

        static async Task LoadSocialDataListFromDB(List<string> names, Dictionary<string, SocialData_Peer> results)
        {
            if (results == null)
                throw new ArgumentNullException("results");
            var list = await GameApplication.dbRepository.Character.GetSocialByNames(names);
            foreach (var dic in list)
            {
                object obj;

                if (!dic.TryGetValue("charname", out obj))
                {
                    continue;
                }

                string name = (string)obj;

                if (!dic.TryGetValue("friends", out obj))
                {
                    //throw new Exception("[SocialController.LoadSocialDataListFromDB]Could not find column [friends] where [charname] is " + name);
                    continue;
                }
                string jsonStr = (string)obj;
                JObject json = null;
                try { json = JsonConvert.DeserializeObject<JToken>(jsonStr) as JObject; } catch { }
                if (json == null)
                    json = new JObject();

                results.Add(name, new SocialData_Peer() { data = new SocialData(json, false) });
            }
        }


        static async Task LoadAllInfoListFromDB(List<string> names, Dictionary<string, AllInfo> results, bool containsId)
        {
            if (results == null)
                throw new ArgumentNullException("results");
            var list = await GameApplication.dbRepository.Character.GetSocialByNames(names);

            foreach (var dic in list)
            {
                object obj;

                if (!dic.TryGetValue("charname", out obj))
                {
                    continue;
                }
                string name = (string)obj;

                string id = null;

                if (containsId)
                {
                    if (!dic.TryGetValue("charid", out obj))
                        continue;
                    if (obj is Guid)
                        id = ((Guid)obj).ToString();
                    else if (obj is string)
                        id = ((string)obj);
                    else
                        continue;
                }

                if (!dic.TryGetValue("friends", out obj))
                {
                    //throw new Exception("[SocialController.LoadSocialDataListFromDB]Could not find column [friends] where [charname] is " + name);
                    continue;
                }

                string jsonStr = (string)obj;
                JObject json = null;
                try { json = JsonConvert.DeserializeObject<JToken>(jsonStr) as JObject; } catch { }
                if (json == null)
                    json = new JObject();

                results.Add(name, new AllInfo()
                {
                    id = id,
                    getState = () =>
                    {
                        string charname = (string)dic["charname"];
                        return new SocialFriendState(
                            name: charname,
                            offlineTime: GetOfflineTime((DateTime)dic["dtlogout"]),
                            channel: GetChannel(charname),
                            progressLevel: (int)dic["progresslevel"],
                            guildName: string.Empty,
                            guildIcon: string.Empty);
                    },
                    data = new SocialData(json, false)
                });
            }
        }

        static async Task<SocialData> LoadSocialDataFromDB(string name)
        {
            var dic = await GameApplication.dbRepository.Character.GetSocialByName(name);
            object obj;

            //如果找不到該玩家(可能是被刪除了)
            if (!dic.TryGetValue("friends", out obj))
            {
                return null;
            }
            string jsonStr = (string)obj;
            JObject json = null;
            try { json = JsonConvert.DeserializeObject<JToken>(jsonStr) as JObject; } catch { }
            if (json == null)
                json = new JObject();


            return new SocialData(json, false);
        }

        static async Task<string> GetCharIdByName(string name)
        {
            var dic = await GameApplication.dbRepository.Character.GetSocialByName(name);
            object obj;
            //如果找不到該玩家(可能是被刪除了)
            if (!dic.TryGetValue("charid", out obj))
            {
                return null;
            }
            if (obj is Guid)
                return ((Guid)obj).ToString();
            else if (obj is string)
                return ((string)obj);
            else
                return null;
        }

        static async Task<Tuple<SocialData, string>> LoadSocialDataFromDBWithId(string name)
        {
            var dic = await GameApplication.dbRepository.Character.GetSocialByName(name);
            object obj;

            //如果找不到該玩家(可能是被刪除了)
            if (!dic.TryGetValue("charid", out obj))
            {
                return new Tuple<SocialData, string>(null, null);
            }

            string charid;
            if (obj is Guid)
                charid = ((Guid)obj).ToString();
            else if (obj is string)
                charid = ((string)obj);
            else
                return new Tuple<SocialData, string>(null, null);

            if (!dic.TryGetValue("friends", out obj))
            {
                return new Tuple<SocialData, string>(null, null);
            }
            string jsonStr = (string)obj;
            JObject json = null;
            try { json = JsonConvert.DeserializeObject<JToken>(jsonStr) as JObject; } catch { }
            if (json == null)
                json = new JObject();


            return new Tuple<SocialData, string>(new SocialData(json, false), charid);
        }

        public static async Task Social_SaveSocialDataToDB(SocialData data, string name)
        {
            await GameApplication.dbRepository.Character.UpdateSocialList(name, data.BuildRecordsString());
        }

        static async Task Social_SaveSocialDataListToDB(Dictionary<string, AllInfo> infoList, List<string> toSave)
        {
            List<string> names = new List<string>();
            List<string> multi_friends = new List<string>();
            foreach (var name in toSave)
            {
                AllInfo info;
                if (infoList.TryGetValue(name, out info))
                {
                    multi_friends.Add(info.data.BuildRecordsString());
                    names.Add(name);
                }
            }
            await GameApplication.dbRepository.Character.UpdateMultipleSocialList(names, multi_friends);
        }
        static async Task Social_SaveSocialDataListToDB(Dictionary<string, SocialData_Peer> dataList, List<string> toSave)
        {
            List<string> names = new List<string>();
            List<string> multi_friends = new List<string>();
            foreach (var name in toSave)
            {
                SocialData_Peer data;
                if (dataList.TryGetValue(name, out data))
                {
                    multi_friends.Add(data.data.BuildRecordsString());
                    names.Add(name);
                }
            }
            await GameApplication.dbRepository.Character.UpdateMultipleSocialList(names, multi_friends);
        }

        #endregion
        #endregion

        //public static JToken serverInfo;
        //public static SemaphoreSlim signal = new SemaphoreSlim(0, 1);

        #region Public Methods

        HashSet<string> m_SocialOnOpenFriendsMenu_idset = new HashSet<string>();
        HashSet<string> m_SocialOnOpenFriendsMenu_removedID = new HashSet<string>();

        /// <summary>
        /// 開啟好友選單時呼叫這個method去檢查是否有玩家被刪除
        /// </summary>
        public void SocialOnOpenFriendsMenu()
        {
            if ((DateTime.Now - lastOpenMenuTime).TotalSeconds < CONFIG_OpenMenuMinInterval_Seconds)
            {
                //避免玩家頻繁開啟選單造成sever負擔過重
                peer.ZRPC.NonCombatRPC.Ret_SocialOnOpenFriendsMenu((int)SocialResult.Success, peer);
                return;
            }

            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                //底下測試用
                //if (serverInfo == null)
                //{
                //    var masterPeer = GameApplication.Instance.masterPeer;
                //    masterPeer.ZRPC.GameToMasterRPC.GetServerList(masterPeer);
                //    await signal.WaitAsync();
                //}

                HashSet<string> idset = m_SocialOnOpenFriendsMenu_idset;
                HashSet<string> removedID = m_SocialOnOpenFriendsMenu_removedID;

                var data = await me_single.LoadSocialData(peer);

                //將所有朋友的id彙整成一個大清單
                List<string> names = new List<string>(
                    data.goodFriends.Count +
                    data.blackFriends.Count +
                    data.requestFriends.Count +
                    data.tempFriends.Count);
                AddName(names, data.goodFriends);
                AddName(names, data.blackFriends);
                AddName(names, data.requestFriends);
                AddName(names, data.tempFriends);

                fr_states.SetCharNames(names);
                await fr_states.OnLoad();

                //Dictionary<string, SocialFriendState> states = new Dictionary<string, SocialFriendState>();

                foreach(var item in fr_states.Multi)
                    idset.Add(item.Value.id);

                data.SkipUpdateStates();

                //移除不存在的角色
                RemoveNotExist(idset, removedID, data.goodFriends);
                RemoveNotExist(idset, removedID, data.blackFriends);
                RemoveNotExist(idset, removedID, data.requestFriends);

                //如果沒有更新日期，一樣是拿掉被刪除的角色
                if (data.CheckRecommandList())
                    RemoveNotExist(idset, removedID, data.tempFriends);

                foreach (var id in removedID)
                    fr_states.Multi.Remove(id);

                foreach (var t in m_ListTypes)
                {
                    var friends = data.getFriends(t);
                    var stateList = data.getFriendStates(t);
                    stateList.StopPatch();
                    stateList.Clear();
                    foreach (var item in friends)
                    {
                        SocialDataHandler.SocialDataSubHandler h;
                        if (fr_states.Multi.TryGetValue(item.name, out h))
                            stateList.Add(h.getState());
                        else
                        {
                            //未知錯誤
                            stateList.Add(SocialFriendState.NewCharaState(item.name));
                        }
                    }
                    stateList.ResumePatch();
                    stateList.PatchFull();
                }

                data.ResumeUpdateStates();

            Final:
                await me_single.OnSave();
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialOnOpenFriendsMenu((int)SocialResult.Success, peer);

                idset.Clear();
                removedID.Clear();

                lastOpenMenuTime = DateTime.Now;
            });
        }


        /// <summary>
        /// 將玩家加入黑名單
        /// </summary>
        public void SocialAddBlack(string friendName)
        {
            if (checkIsSelf(friendName, peer.ZRPC.NonCombatRPC.Ret_SocialAddBlack))
                return;

            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () => {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msg;
                SocialResult result;
                string msgParam = "name;" + friendName;
                bool addLog = false;

                string myName = peer.CharacterData.Name;
                var data = await me_single.LoadSocialData(peer);

                //要加別人黑名單之前要先移除好友
                if (data.goodFriends.ContainsPlayerName(friendName))
                {
                    msg = "ret_social_AddBlack_RemoveGoodFriendFirst";
                    result = SocialResult.RemoveGoodFriendFirst;
                    goto Final;
                }
                //已經加過黑名單
                if (data.blackFriends.ContainsPlayerName(friendName))
                {
                    msg = "ret_social_AddBlack_AlreadyAdded";
                    result = SocialResult.AlreadyAdded;
                    goto Final;
                }
                //黑名單滿了
                if (data.blackFriends.Count >= data.getFriendMaxCount(FriendType.Black))
                {
                    msg = "ret_social_AddBlack_ListFull";
                    result = SocialResult.ListFull;
                    goto Final;
                }
                
                SocialData fdata=await fr_single.LoadSocialData(friendName);

                if (fr_single.nameNotFound)
                {
                    msg = null;
                    result = SocialResult.PlayerNameNotFound;
                    goto Final;
                }

                //將雙方的請求與暫時名單通通移除
                data.RemoveFriendByName(FriendType.Request, friendName);
                data.RemoveFriendByName(FriendType.Temp, friendName);
                fdata.RemoveFriendByName(FriendType.Request, myName);
                //fdata.RemoveFriendByName(FriendType.Temp, myName);//拿掉這行，不會拿掉被黑單的暫時名單，要不然會因為臨時名單可能馬上發現被加入黑單

                data.AddFriend(FriendType.Black, fr_single.id, friendName, fr_single.Single.getState);

                msg = "ret_social_AddBlack_Success";
                result = SocialResult.Success;
                addLog = true;

            Final:
                await fr_single.OnSave();
                await me_single.OnSave();
                //發送系統訊息
                if (msg != null)
                    peer.SendSystemMessage(msg, addLog, msgParam);
                peer.ZRPC.NonCombatRPC.Ret_SocialAddBlack((int)result, friendName, peer);
                return;
            });
        }

        /// <summary>
        /// 將玩家從黑名單中移除
        /// </summary>
        /// <remarks>no await</remarks>
        public void SocialRemoveBlack(string friendName)
        {
            if (checkIsSelf(friendName, peer.ZRPC.NonCombatRPC.Ret_SocialRemoveBlack))
                return;

            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () => {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msg;
                SocialResult result;
                string msgParam = "name;" + friendName;

                var data = await me_single.LoadSocialData(peer);
                result = data.RemoveFriendByName(FriendType.Black, friendName);

                if (result == SocialResult.Success)
                    msg = "ret_social_RemoveBlack_Success";
                else
                    msg = "ret_social_PlayerNameNotFoundInList";

                Final:
                await me_single.OnSave();
                //發送系統訊息
                peer.SendSystemMessage(msg, true, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRemoveBlack((int)result, peer);

            });
        }

        /// <summary>
        /// 將玩家從好友名單中移除
        /// </summary>
        public void SocialRemoveGood(string friendName)
        {
            if (checkIsSelf(friendName, peer.ZRPC.NonCombatRPC.Ret_SocialRemoveGood))
                return;

            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () => {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msg;
                SocialResult result;
                string msgParam = "name;" + friendName;
                bool addLog = false;

                string myName = peer.CharacterData.Name;
                var data = await me_single.LoadSocialData(peer);
                int index;
                if (!data.goodFriends.ContainsPlayerName(friendName, out index))
                {
                    msg = "ret_social_PlayerNameNotFoundInList";
                    result = SocialResult.PlayerNameNotFoundInList;
                    goto Final;
                }

                var fdata = await fr_single.LoadSocialData(friendName);
                if (fr_single.nameNotFound)
                {
                    data.RemoveFriendAt(FriendType.Good, index);
                    msg = "ret_social_PlayerNameNotFound";
                    result = SocialResult.PlayerNameNotFound;
                    goto Final;
                }

                data.RemoveFriendAt(FriendType.Good, index);
                fdata.RemoveFriendByName(FriendType.Good, myName);

                msg = "ret_social_RemoveGood_Success";
                result = SocialResult.Success;
                addLog = true;

            Final:
                await fr_single.OnSave();
                await me_single.OnSave();
                //發送系統訊息
                peer.SendSystemMessage(msg, addLog, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRemoveGood((int)result, peer);
            });
        }


        /// <summary>
        /// 發起申請好友請求
        /// </summary>
        public void SocialRaiseRequest(string friendName, bool fromTemp)
        {
            if (checkIsSelf(friendName, peer.ZRPC.NonCombatRPC.Ret_SocialRaiseRequest))
                return;

            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msgParam = "name;" + friendName;
                string msg;
                SocialResult result;

                string myName = peer.CharacterData.Name;
                bool ForceSendSysMsg = false;
                var data = await me_single.LoadSocialData(peer);
                var fdata = await fr_single.LoadSocialData(friendName);

                if (fromTemp)//如果臨時名單內有對方，拿掉
                    data.RemoveFriendByName(FriendType.Temp, friendName);

                if (fr_single.nameNotFound)
                {
                    msg = null;
                    result = SocialResult.PlayerNameNotFound;
                    goto Final;
                }

                //我的黑名單中有對方故無法申請
                if (data.blackFriends.ContainsPlayerName(friendName))
                {
                    msg = "ret_social_RaiseRequest_TargetBlacked";
                    result = SocialResult.TargetBlacked;
                    goto Final;
                }

                //當對方好友清單已有我，不要發送請求
                if (fdata.goodFriends.ContainsPlayerName(myName))
                {
                    msg = "ret_social_RaiseRequest_AlreadyAddedGood";
                    ForceSendSysMsg = true;
                    result = SocialResult.AlreadyAddedGood;
                    goto Final;
                }

                //被加入黑名單故無法申請
                if (fdata.blackFriends.ContainsPlayerName(myName))
                {
                    msg = "ret_social_Blacked";
                    result = SocialResult.Blacked;
                    goto Final;
                }

                //嘗試新增我至對方請求清單
                result = fdata.AddFriend(FriendType.Request, peer.GetCharId(), myName, () => LoadCharaStateFromPeer(peer));

                if (result == SocialResult.ListFull)
                    msg = "ret_social_RaiseRequest_ListFull";
                else if (result == SocialResult.AlreadyAdded)
                    msg = "ret_social_RaiseRequest_AlreadyAdded";
                else
                {
                    msg = "ret_social_RaiseRequest_Success";
                }

            Final:
                await fr_single.OnSave();
                await me_single.OnSave();

                //發送系統訊息
                if ((DebugMode || ForceSendSysMsg) && msg != null)
                    peer.SendSystemMessage(msg, false, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRaiseRequest((int)result, friendName, peer);

            });
        }


        private void Example()
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
            });
        }

        /// <summary>
        /// 接受申請好友請求
        /// </summary>
        public void SocialAcceptRequest(string friendName)
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msg;
                string msgParam = "name;" + friendName;
                bool addLog = false;
                bool skip = false;

                string myName = peer.CharacterData.Name;
                SocialResult result = SocialResult.InvalidOperation;
                var data = await me_single.LoadSocialData(peer);
                int index;

                //找不到該請求者
                if (!data.requestFriends.ContainsPlayerName(friendName, out index))
                {
                    msg = "ret_social_PlayerNameNotFoundInList";
                    result = SocialResult.PlayerNameNotFoundInList;
                    goto Final;
                }
                //當我的好友名單已滿時
                if (data.goodFriends.Count >= data.getFriendMaxCount(FriendType.Good))
                {
                    msg = "ret_social_AcceptRequest_MyListFull";
                    result = SocialResult.MyListFull;
                    goto Final;
                }

                SocialData fdata=await fr_single.LoadSocialData(friendName);

                if (fr_single.nameNotFound)
                {
                    msg = "ret_social_PlayerNameNotFound";
                    result = SocialResult.PlayerNameNotFound;
                    goto Final;
                }

                if (fdata.blackFriends.ContainsPlayerName(myName))
                {
                    msg = "ret_social_Blacked";
                    result = SocialResult.Blacked;
                    skip = true;
                    goto Final;
                }

                //嘗試新增我至對方好友清單
                result = fdata.AddFriend(FriendType.Good, me_single.id, myName, me_single.Single.getState);
                //對方好友清單已滿
                if (result == SocialResult.ListFull)
                {
                    msg = "ret_social_AcceptRequest_ListFull";
                    result = SocialResult.ListFull;
                }
                //對方好友清單已有我
                else if (result == SocialResult.AlreadyAdded)
                {
                    msg = "ret_social_AcceptRequest_AlreadyAdded";
                    result = SocialResult.AlreadyAdded;
                }
                else
                {
                    //如果對方臨時名單有我，移除它
                    fdata.RemoveFriendByName(FriendType.Temp, myName);
                    //如果對方有我之前發起的請求，移除它
                    fdata.RemoveFriendByName(FriendType.Request, myName);

                    //新增對方至我的好友清單
                    data.AddFriend(FriendType.Good, fr_single.id, friendName, fr_single.Single.getState);
                    //移除對方發給我的請求
                    data.RemoveFriendAt(FriendType.Request, index);
                    //如果我的臨時名單有對方，移除它
                    data.RemoveFriendByName(FriendType.Temp, friendName);

                    msg = "ret_social_AcceptRequest_Success";
                    addLog = true;
                }

            Final:
                await fr_single.OnSave();
                await me_single.OnSave();
                //發送系統訊息
                if ((!skip && !DebugMode))
                    peer.SendSystemMessage(msg, addLog, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialAcceptRequest((int)result, peer);
            });
        }


        /// <summary>
        /// 一鍵接受所有申請
        /// </summary>
        /// <remarks>(新版,呼叫2次db)</remarks>
        public void SocialAcceptAllRequest()
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                List<string> friendNames = new List<string>();
                Dictionary<string, int> Indices = new Dictionary<string, int>();
                //好友請求的預定移除清單
                List<int> requestToRemove = new List<int>();

                string myName = peer.CharacterData.Name;
                var data = await me_single.LoadSocialData(peer);
                
                //取得線上好友的資料或排入下線好友
                for (int x = 0; x < data.requestFriends.Count; x++)
                {
                    var fname = data.requestFriends[x].name;
                    friendNames.Add(fname);
                    Indices[fname] = x;
                }

                fr_multi.SetCharNames(friendNames);
                await fr_multi.OnLoad();

                foreach (var item in fr_multi.Multi)
                {
                    //當我的好友名單已滿時
                    if (data.goodFriends.Count >= data.getFriendMaxCount(FriendType.Good))
                    {
                        break;
                    }
                    string friendName = item.Key;
                    var handler = item.Value;

                    if (handler.nameNotFound)
                    {
                        requestToRemove.Add(Indices[friendName]);
                        continue;
                    }

                    var fdata = handler.data;

                    //新增我至對方好友清單
                    if (fdata.AddFriend(FriendType.Good, peer.GetCharId(), myName, () => LoadCharaStateFromPeer(peer)) == SocialResult.Success)
                    {
                        //新增對方至我的好友清單
                        data.AddFriend(FriendType.Good, handler.id, friendName, handler.getState);
                        //移除對方發給我的請求(先放到預定清單中)
                        requestToRemove.Add(Indices[friendName]);
                        //如果對方有我之前發起的請求，移除它
                        fdata.RemoveFriendByName(FriendType.Request, myName);
                    }
                }
                //移除已經變成好友的請求
                data.RemoveFriendByIndices(FriendType.Request, requestToRemove.ToArray());

                await fr_multi.OnSave();
                await me_single.OnSave();

                //發送系統訊息
                peer.SendSystemMessage("ret_social_AcceptAllRequest_Success", false);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialAcceptAllRequest((int)SocialResult.Success, peer);

            });
        }

        /// <summary>
        /// 拒絕申請好友請求
        /// </summary>
        /// <remarks>no await</remarks>
        public void SocialRejectRequest(string friendName)
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () => {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msg;
                SocialResult result = SocialResult.InvalidOperation;
                string msgParam = "name;" + friendName;

                var data = await me_single.LoadSocialData(peer);
                int index;
                //找不到該請求名稱
                if (!data.requestFriends.ContainsPlayerName(friendName,out index))
                {
                    msg = "ret_social_PlayerNameNotFoundInList";
                    result = SocialResult.PlayerNameNotFoundInList;
                    goto Final;
                }

                msg = "ret_social_RejectRequest_Success";
                data.RemoveFriendAt(FriendType.Request, index);
                result = SocialResult.Success;

            Final:
                await me_single.OnSave();
                //發送系統訊息
                peer.SendSystemMessage(msg, false, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRejectRequest((int)result, peer);

            });
        }

        /// <summary>
        /// 一鍵拒絕所有申請
        /// </summary>
        /// <remarks>no await</remarks>
        public void SocialRejectAllRequest()
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () => {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msg;
                SocialResult result;

                var data = await me_single.LoadSocialData(peer);

                data.ClearFriendList(FriendType.Request);

                msg = "ret_social_RejectAllRequest_Success";
                result = SocialResult.Success;

                await me_single.OnSave();
                //發送系統訊息
                peer.SendSystemMessage(msg, false);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRejectAllRequest((int)result, peer);

            });
        }

        /// <summary>
        /// 一鍵全加臨時名單好友
        /// </summary>
        /// <remarks>(新版,呼叫2次db)</remarks>
        public void SocialRaiseAllTempRequest()
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string myName = peer.CharacterData.Name;
                var data = await me_single.LoadSocialData(peer);
                SocialFriendState myState = me_single.Single.getState();

                List<string> friendNames = new List<string>();
                Dictionary<string, int> Indices = new Dictionary<string, int>();
                List<int> toRemove = new List<int>();

                for (int i = 0; i < data.tempFriends.Count; i++)
                {
                    var item = data.tempFriends[i];
                    friendNames.Add(item.name);
                    Indices[item.name] = i;
                }

                fr_mdata.SetCharNames(friendNames);

                await fr_mdata.OnLoad();

                foreach (var pair in fr_mdata.Multi)
                {
                    var friendName = pair.Key;
                    var handler = pair.Value;
                    var fdata = handler.data;

                    if(handler.nameNotFound)
                    {
                        toRemove.Add(Indices[friendName]);
                        continue;
                    }
                    
                    //我的黑名單中有對方故無法申請
                    if (data.blackFriends.ContainsPlayerName(friendName))
                    {
                        continue;
                    }

                    //當對方好友清單已有我，不要發送請求
                    if (fdata.goodFriends.ContainsPlayerName(myName))
                    {
                        continue;
                    }
                    //被加入黑名單故無法申請
                    if (fdata.blackFriends.ContainsPlayerName(myName))
                    {
                        continue;
                    }

                    toRemove.Add(Indices[friendName]);

                    //嘗試新增我至對方請求清單
                    fdata.AddFriend(FriendType.Request, peer.GetCharId(), myName, () => new SocialFriendState(myState.node.DeepClone()));
                }

                data.RemoveFriendByIndices(FriendType.Temp, toRemove.ToArray());

                await fr_mdata.OnSave();
                await me_single.OnSave();

                //發送系統訊息
                peer.SendSystemMessage("ret_social_RaiseAllTempRequest", false);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRaiseAllTempRequest(peer);
            });
        }

        /// <summary>
        /// 一鍵清除臨時名單好友
        /// </summary>
        /// <remarks>no await</remarks>
        public void SocialClearTemp()
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;
                var data = await me_single.LoadSocialData(peer);
                data.ClearFriendList(FriendType.Temp);

                await me_single.OnSave();
                //發送系統訊息
                peer.SendSystemMessage("ret_social_ClearTemp", false);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialClearTemp(peer);
            });
        }


        #endregion

        #region MultiServer
        public async Task<int> SocialAcceptRequest_MIRROR(bool diffServer, string myName, string myID, string friendName)
        {
            SocialData fdata = await fr_single.LoadSocialData(friendName);

            if (fr_single.nameNotFound)
                return (int)SocialResult.PlayerNameNotFound;

            if (diffServer && fr_single.Single.State != MultiServerDataState.Online)
                return (int)SocialResult.PlayerServerChanged;

            if (fdata.blackFriends.ContainsPlayerName(myName))
                return (int)SocialResult.Blacked;

            //嘗試新增我至對方好友清單
            SocialResult result = fdata.AddFriend(FriendType.Good, myID, myName, () => me_state.LoadState(myName));

            if (result == SocialResult.Success)
            {
                //如果對方臨時名單有我，移除它
                fdata.RemoveFriendByName(FriendType.Temp, myName);
                //如果對方有我之前發起的請求，移除它
                fdata.RemoveFriendByName(FriendType.Request, myName);
            }
            await fr_single.OnSave();
            return (int)result;
        }

        public void SocialAcceptRequest_MAIN(string friendName)
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();

                if (peer.IsExitGame())
                    return;

                string msg;
                string msgParam = "name;" + friendName;
                bool addLog = false;
                bool skip = false;

                string myName = peer.CharacterData.Name;
                SocialResult result = SocialResult.InvalidOperation;
                var data = await me_single.LoadSocialData(peer);
                int index;

                //找不到該請求者
                if (!data.requestFriends.ContainsPlayerName(friendName, out index))
                {
                    msg = "ret_social_PlayerNameNotFoundInList";
                    result = SocialResult.PlayerNameNotFoundInList;
                    goto Final;
                }
                //當我的好友名單已滿時
                if (data.goodFriends.Count >= data.getFriendMaxCount(FriendType.Good))
                {
                    msg = "ret_social_AcceptRequest_MyListFull";
                    result = SocialResult.MyListFull;
                    goto Final;
                }

                fr_single.SetCharName(friendName);
                result = (SocialResult)(int)(await fr_single.Request("SocialAcceptRequest_MIRROR", myName, me_single.id, friendName))[0];

                switch (result)
                {
                    case SocialResult.Blacked:
                        msg = "ret_social_Blacked";
                        skip = true;
                        break;
                    case SocialResult.PlayerServerChanged:
                        msg = "ret_social_PlayerServerChanged";
                        break;
                    case SocialResult.PlayerNameNotFound:
                        msg = "ret_social_PlayerNameNotFound";
                        break;
                    case SocialResult.ListFull:
                        msg = "ret_social_AcceptRequest_ListFull";
                        break;
                    case SocialResult.AlreadyAdded:
                        msg = "ret_social_AcceptRequest_AlreadyAdded";
                        break;
                    case SocialResult.Success:
                        msg = "ret_social_AcceptRequest_Success";
                        //新增對方至我的好友清單
                        data.AddFriend(FriendType.Good, fr_single.id, friendName, fr_single.Single.getState);
                        //移除對方發給我的請求
                        data.RemoveFriendAt(FriendType.Request, index);
                        //如果我的臨時名單有對方，移除它
                        data.RemoveFriendByName(FriendType.Temp, friendName);

                        msg = "ret_social_AcceptRequest_Success";
                        addLog = true;
                        break;
                    default:
                        msg = "ret_social_InvalidOperation";
                        break;
                }

                await me_single.OnSave();
            Final:
                //發送系統訊息
                if ((!skip && !DebugMode))
                    peer.SendSystemMessage(msg, addLog, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialAcceptRequest((int)result, peer);

            });
        }
        #endregion

        #region Version 0
        /// <summary>
        /// 將玩家從好友名單中移除
        /// </summary>
        public void SocialRemoveGood_V0(string friendName)
        {
            if (checkIsSelf(friendName, peer.ZRPC.NonCombatRPC.Ret_SocialRemoveGood))
                return;

            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () => {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msg;
                SocialResult result;
                string msgParam = "name;" + friendName;
                bool addLog = false;

                var data = GetData(peer);
                SocialFriend friend;
                int index;
                if (!data.goodFriends.FindFriendByName(friendName, out friend, out index))
                {
                    msg = "ret_social_PlayerNameNotFoundInList";
                    result = SocialResult.PlayerNameNotFoundInList;
                    goto Final;
                }

                string myName = peer.CharacterData.Name;
                SocialData fdata;
                bool doSave = false, f_updated = false;
                var fpeer = GetPeer(friendName);
                //當玩家在線上時
                if (fpeer != null)
                {
                    fdata = GetData(fpeer);
                }
                //當玩家不在線上時
                else
                {
                    fdata = await LoadSocialDataFromDB(friendName);
                    if (fdata == null)
                    {
                        data.RemoveFriendAt(FriendType.Good, index);
                        msg = "ret_social_PlayerNameNotFound";
                        result = SocialResult.PlayerNameNotFound;
                        goto Final;
                    }
                    doSave = true;
                }

                data.RemoveFriendAt(FriendType.Good, index);
                f_updated |= fdata.RemoveFriendByName(FriendType.Good, myName) == SocialResult.Success;

                if (doSave)
                {
                    if (f_updated)
                        await Social_SaveSocialDataToDB(fdata, friendName);
                }

                msg = "ret_social_RemoveGood_Success";
                result = SocialResult.Success;
                addLog = true;

            Final:
                //發送系統訊息
                peer.SendSystemMessage(msg, addLog, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRemoveGood((int)result, peer);
            });
        }

        /// <summary>
        /// 將玩家從黑名單中移除
        /// </summary>
        /// <remarks>no await</remarks>
        public void SocialRemoveBlack_V0(string friendName)
        {
            if (checkIsSelf(friendName, peer.ZRPC.NonCombatRPC.Ret_SocialRemoveBlack))
                return;

            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(() => {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msg;
                SocialResult result;
                string msgParam = "name;" + friendName;

                var data = GetData(peer);
                result = data.RemoveFriendByName(FriendType.Black, friendName);

                if (result == SocialResult.Success)
                    msg = "ret_social_RemoveBlack_Success";
                else
                    msg = "ret_social_PlayerNameNotFoundInList";

                Final:
                //發送系統訊息
                peer.SendSystemMessage(msg, true, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRemoveBlack((int)result, peer);

            });
        }

        /// <summary>
        /// 拒絕申請好友請求
        /// </summary>
        /// <remarks>no await</remarks>
        public void SocialRejectRequest_V0(string friendName)
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(() => {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msg;
                SocialResult result = SocialResult.InvalidOperation;
                string msgParam = "name;" + friendName;

                var data = GetData(peer);
                int index;
                SocialFriend friend;
                //找不到該請求名稱
                if (!data.requestFriends.FindFriendByName(friendName, out friend, out index))
                {
                    msg = "ret_social_PlayerNameNotFoundInList";
                    result = SocialResult.PlayerNameNotFoundInList;
                    goto Final;
                }

                msg = "ret_social_RejectRequest_Success";
                data.RemoveFriendAt(FriendType.Request, index);
                result = SocialResult.Success;

            Final:
                //發送系統訊息
                peer.SendSystemMessage(msg, false, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRejectRequest((int)result, peer);

            });
        }

        /// <summary>
        /// 一鍵拒絕所有申請
        /// </summary>
        /// <remarks>no await</remarks>
        public void SocialRejectAllRequest_V0()
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(() => {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msg;
                SocialResult result;

                var data = GetData(peer);

                data.ClearFriendList(FriendType.Request);

                msg = "ret_social_RejectAllRequest_Success";
                result = SocialResult.Success;

            Final:
                //發送系統訊息
                peer.SendSystemMessage(msg, false);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRejectAllRequest((int)result, peer);

            });
        }

        /// <summary>
        /// 一鍵全加臨時名單好友
        /// </summary>
        /// <remarks>(新版,呼叫2次db)</remarks>
        public void SocialRaiseAllTempRequest_V0()
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                SocialData data = GetData(peer);
                SocialFriendState myState = LoadCharaStateFromPeer(peer);

                string myName = peer.CharacterData.Name;

                List<string> fnameToLoadFromDB = new List<string>();
                Dictionary<string, SocialData_Peer> fdataList = new Dictionary<string, SocialData_Peer>();

                foreach (var item in data.tempFriends)
                {
                    var fpeer = GetPeer(item.name);
                    if (fpeer != null)
                    {
                        var fdata = GetData(fpeer);
                        fdataList.Add(item.name, new SocialData_Peer() { data = fdata, peer = fpeer });
                    }
                    else
                        fnameToLoadFromDB.Add(item.name);
                }

                await LoadSocialDataListFromDB(fnameToLoadFromDB, fdataList);

                foreach (var pair in fdataList)
                {
                    var fdata = pair.Value.data;
                    var fpeer = pair.Value.peer;
                    var friendName = pair.Key;
                    //我的黑名單中有對方故無法申請
                    if (data.blackFriends.ContainsPlayerName(friendName))
                    {
                        continue;
                    }

                    //當對方好友清單已有我，不要發送請求
                    if (fdata.goodFriends.ContainsPlayerName(myName))
                    {
                        continue;
                    }
                    //被加入黑名單故無法申請
                    if (fdata.blackFriends.ContainsPlayerName(myName))
                    {
                        continue;
                    }

                    //如果臨時名單內有對方，拿掉
                    data.RemoveFriendByName(FriendType.Temp, friendName);

                    //嘗試新增我至對方請求清單
                    bool modified = fdata.AddFriend(FriendType.Request, peer.GetCharId(), myName, () => new SocialFriendState(myState.node.DeepClone())) == SocialResult.Success;

                    if (!modified)//對象清單無更動，不用存入db
                    {
                        if (fpeer == null)
                            fnameToLoadFromDB.Remove(friendName);
                    }
                }

                await Social_SaveSocialDataListToDB(fdataList, fnameToLoadFromDB);


                //發送系統訊息
                peer.SendSystemMessage("ret_social_RaiseAllTempRequest", false);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRaiseAllTempRequest(peer);
            });
        }

        /// <summary>
        /// 一鍵清除臨時名單好友
        /// </summary>
        /// <remarks>no await</remarks>
        public void SocialClearTemp_V0()
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(() =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;
                var data = GetData(peer);
                data.ClearFriendList(FriendType.Temp);
                //發送系統訊息
                peer.SendSystemMessage("ret_social_ClearTemp", false);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialClearTemp(peer);
            });
        }

        /// <summary>
        /// 一鍵接受所有申請
        /// </summary>
        /// <remarks>(新版,呼叫2次db)</remarks>
        public void SocialAcceptAllRequest_V0()
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                List<string> friendNames = new List<string>();
                List<string> toLoadFromDBFriendNames = new List<string>();
                Dictionary<string, AllInfo> infoList = new Dictionary<string, AllInfo>();
                Dictionary<string, int> Indices = new Dictionary<string, int>();
                //好友請求的預定移除清單
                List<int> requestToRemove = new List<int>();

                var data = GetData(peer);
                string myName = peer.CharacterData.Name;

                //取得線上好友的資料或排入下線好友
                for (int x = 0; x < data.requestFriends.Count; x++)
                {
                    var fname = data.requestFriends[x].name;
                    friendNames.Add(fname);
                    Indices[fname] = x;
                    var fpeer = GetPeer(fname);

                    //當玩家在線上時
                    if (fpeer != null)
                    {
                        var fdata = GetData(fpeer);
                        infoList.Add(fname, new AllInfo()
                        {
                            id = fpeer.GetCharId(),
                            data = fdata,
                            getState = () => LoadCharaStateFromPeer(fpeer),
                            peer = fpeer
                        });
                    }
                    else
                        toLoadFromDBFriendNames.Add(fname);
                }

                //讀取不在線上的好友的資料
                await LoadAllInfoListFromDB(toLoadFromDBFriendNames, infoList, true);

                foreach (var item in infoList)
                {
                    //當我的好友名單已滿時
                    if (data.goodFriends.Count >= data.getFriendMaxCount(FriendType.Good))
                    {
                        break;
                    }
                    string friendName = item.Key;
                    AllInfo info = item.Value;
                    var fdata = info.data;
                    var fpeer = info.peer;
                    bool modified = false;

                    //新增我至對方好友清單
                    if (fdata.AddFriend(FriendType.Good, peer.GetCharId(), myName, () => LoadCharaStateFromPeer(peer)) == SocialResult.Success)
                    {
                        modified = true;
                        //新增對方至我的好友清單
                        data.AddFriend(FriendType.Good, info.id, friendName, info.getState);
                        //移除對方發給我的請求(先放到預定清單中)
                        requestToRemove.Add(Indices[friendName]);
                        //如果對方有我之前發起的請求，移除它
                        fdata.RemoveFriendByName(FriendType.Request, myName);
                    }
                    if (!modified)
                    {
                        if (fpeer == null)
                            toLoadFromDBFriendNames.Remove(friendName);
                    }
                }

                //存入db
                await Social_SaveSocialDataListToDB(infoList, toLoadFromDBFriendNames);

                //移除已經變成好友的請求
                data.RemoveFriendByIndices(FriendType.Request, requestToRemove.ToArray());

                //發送系統訊息
                peer.SendSystemMessage("ret_social_AcceptAllRequest_Success", false);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialAcceptAllRequest((int)SocialResult.Success, peer);

            });
        }

        /// <summary>
        /// 接受申請好友請求
        /// </summary>
        public void SocialAcceptRequest_V0(string friendName)
        {
            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msg;
                string msgParam = "name;" + friendName;
                bool addLog = false;

                SocialResult result = SocialResult.InvalidOperation;
                var data = GetData(peer);
                int index;

                SocialFriend friend;
                //找不到該請求者
                if (!data.requestFriends.FindFriendByName(friendName, out friend, out index))
                {
                    msg = "ret_social_PlayerNameNotFoundInList";
                    result = SocialResult.PlayerNameNotFoundInList;
                    goto Final;
                }
                //當我的好友名單已滿時
                if (data.goodFriends.Count >= data.getFriendMaxCount(FriendType.Good))
                {
                    msg = "ret_social_AcceptRequest_MyListFull";
                    result = SocialResult.MyListFull;
                    goto Final;
                }

                string myName = peer.CharacterData.Name;
                SocialData fdata;
                Func<SocialFriendState> getfstate;
                bool doSave = false, updated = false;
                var fpeer = GetPeer(friendName);
                string fid;
                AllInfo info = new AllInfo();

                //當玩家在線上時
                if (fpeer != null)
                {
                    fdata = GetData(fpeer);
                    fid = fpeer.GetCharId();
                    getfstate = () => LoadCharaStateFromPeer(fpeer);
                }
                //當玩家不在線上時
                else
                {
                    info = await LoadAllInfoFromDB(friendName);
                    fdata = info.data;
                    fid = info.id;
                    getfstate = info.getState;
                    if (fid == null)
                    {
                        msg = "ret_social_PlayerNameNotFound";
                        result = SocialResult.PlayerNameNotFound;
                        goto Final;
                    }
                    doSave = true;
                }

                if (fdata.blackFriends.ContainsPlayerName(myName))
                {
                    msg = "ret_social_Blacked";
                    result = SocialResult.Blacked;
                    goto Final;
                }

                //嘗試新增我至對方好友清單
                result = fdata.AddFriend(FriendType.Good, peer.GetCharId(), myName, () => LoadCharaStateFromPeer(peer));
                //對方好友清單已滿
                if (result == SocialResult.ListFull)
                {
                    msg = "ret_social_AcceptRequest_ListFull";
                    result = SocialResult.ListFull;
                }
                //對方好友清單已有我
                else if (result == SocialResult.AlreadyAdded)
                {
                    msg = "ret_social_AcceptRequest_AlreadyAdded";
                    result = SocialResult.AlreadyAdded;
                }
                else
                {
                    //新增對方至我的好友清單
                    data.AddFriend(FriendType.Good, fid, friendName, getfstate);

                    //移除對方發給我的請求
                    data.RemoveFriendAt(FriendType.Request, index);
                    //如果對方有我之前發起的請求，移除它
                    fdata.RemoveFriendByName(FriendType.Request, myName);

                    //如果我的臨時名單有對方，移除它
                    data.RemoveFriendByName(FriendType.Temp, friendName);
                    //如果對方臨時名單有我，移除它
                    fdata.RemoveFriendByName(FriendType.Temp, myName);

                    updated = true;

                    msg = "ret_social_AcceptRequest_Success";
                    addLog = true;
                }

                if (doSave)
                {
                    if (updated)
                        await Social_SaveSocialDataToDB(fdata, friendName);
                }

            Final:
                //發送系統訊息
                peer.SendSystemMessage(msg, addLog, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialAcceptRequest((int)result, peer);
            });
        }

        /// <summary>
        /// 將玩家加入黑名單
        /// </summary>
        public void SocialAddBlack_V0(string friendName)
        {
            if (checkIsSelf(friendName, peer.ZRPC.NonCombatRPC.Ret_SocialAddBlack))
                return;

            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () => {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msg;
                SocialResult result;
                string msgParam = "name;" + friendName;
                bool addLog = false;

                var data = GetData(peer);
                //要加別人黑名單之前要先移除好友
                if (data.goodFriends.ContainsPlayerName(friendName))
                {
                    msg = "ret_social_AddBlack_RemoveGoodFriendFirst";
                    result = SocialResult.RemoveGoodFriendFirst;
                    goto Final;
                }
                //已經加過黑名單
                if (data.blackFriends.ContainsPlayerName(friendName))
                {
                    msg = "ret_social_AddBlack_AlreadyAdded";
                    result = SocialResult.AlreadyAdded;
                    goto Final;
                }
                //黑名單滿了
                if (data.blackFriends.Count >= data.getFriendMaxCount(FriendType.Black))
                {
                    msg = "ret_social_AddBlack_ListFull";
                    result = SocialResult.ListFull;
                    goto Final;
                }

                string myName = peer.CharacterData.Name;
                SocialData fdata;
                Func<SocialFriendState> getfstate;
                bool doSave = false, f_updated = false;
                var fpeer = GetPeer(friendName);
                string fid;
                //當玩家在線上時
                if (fpeer != null)
                {
                    fdata = GetData(fpeer);
                    fid = fpeer.GetCharId();
                    getfstate = () => LoadCharaStateFromPeer(fpeer);
                }
                //當玩家不在線上時
                else
                {
                    var tp = await LoadAllInfoFromDB(friendName);
                    fdata = tp.data;
                    fid = tp.id;
                    getfstate = tp.getState;
                    if (fdata == null)
                    {
                        result = SocialResult.PlayerNameNotFound;
                        goto FinalSkipSysMsg;
                    }
                    doSave = true;
                }

                //將雙方的請求與暫時名單通通移除
                data.RemoveFriendByName(FriendType.Request, friendName);
                data.RemoveFriendByName(FriendType.Temp, friendName);
                f_updated |= fdata.RemoveFriendByName(FriendType.Request, myName) == SocialResult.Success;
                //f_updated |= fdata.RemoveFriendByName(FriendType.Temp, myName) == SocialResult.Success;//拿掉這行，不會拿掉被黑單的暫時名單，要不然會因為臨時名單可能馬上發現被加入黑單

                data.AddFriend(FriendType.Black, fid, friendName, getfstate);

                if (doSave)
                {
                    if (f_updated)
                        await Social_SaveSocialDataToDB(fdata, friendName);
                }

                msg = "ret_social_AddBlack_Success";
                result = SocialResult.Success;
                addLog = true;

            Final:
                //發送系統訊息
                peer.SendSystemMessage(msg, addLog, msgParam);

            FinalSkipSysMsg:
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialAddBlack((int)result, friendName, peer);

            });
        }

        /// <summary>
        /// 開啟好友選單時呼叫這個method去檢查是否有玩家被刪除
        /// </summary>
        public void SocialOnOpenFriendsMenu_V0()
        {
            if ((DateTime.Now - lastOpenMenuTime).TotalSeconds < CONFIG_OpenMenuMinInterval_Seconds)
            {
                //避免玩家頻繁開啟選單造成sever負擔過重
                peer.ZRPC.NonCombatRPC.Ret_SocialOnOpenFriendsMenu((int)SocialResult.Success, peer);
                return;
            }

            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                //底下測試用
                //if (serverInfo == null)
                //{
                //    var masterPeer = GameApplication.Instance.masterPeer;
                //    masterPeer.ZRPC.GameToMasterRPC.GetServerList(masterPeer);
                //    await signal.WaitAsync();
                //}

                var data = GetData(peer);

                HashSet<string> idset = m_SocialOnOpenFriendsMenu_idset;
                HashSet<string> removedID = m_SocialOnOpenFriendsMenu_removedID;

                //將所有朋友的id彙整成一個大清單
                List<string> names = new List<string>(
                    data.goodFriends.Count +
                    data.blackFriends.Count +
                    data.requestFriends.Count +
                    data.tempFriends.Count);
                AddName(names, data.goodFriends);
                AddName(names, data.blackFriends);
                AddName(names, data.requestFriends);
                AddName(names, data.tempFriends);

                //從db中取得朋友id是否存在
                var statesFromDB = await LoadCharaStatesFromDB(names);

                Dictionary<string, SocialFriendState> states = new Dictionary<string, SocialFriendState>();

                for (int i = 0; i < statesFromDB.Count; i++)
                    idset.Add(statesFromDB[i].charid);

                data.SkipUpdateStates();

                //移除不存在的角色
                RemoveNotExist(idset, removedID, data.goodFriends);
                RemoveNotExist(idset, removedID, data.blackFriends);
                RemoveNotExist(idset, removedID, data.requestFriends);

                //如果沒有更新日期，一樣是拿掉被刪除的角色
                if (data.CheckRecommandList())
                    RemoveNotExist(idset, removedID, data.tempFriends);

                foreach (var item in statesFromDB)
                {
                    if (!removedID.Contains(item.charid))
                        states.Add(item.state.name, item.state);
                }

                foreach (var t in m_ListTypes)
                {
                    var friends = data.getFriends(t);
                    var stateList = data.getFriendStates(t);
                    stateList.StopPatch();
                    stateList.Clear();
                    foreach (var item in friends)
                    {
                        SocialFriendState state;

                        var fpeer = GetPeer(item.name);
                        if (fpeer != null)
                            state = LoadCharaStateFromPeer(fpeer);
                        else
                        {
                            if (!states.TryGetValue(item.name, out state))
                            {
                                //未知錯誤
                                state = SocialFriendState.NewCharaState(item.name);
                            }
                        }
                        stateList.Add(state);
                    }
                    stateList.ResumePatch();
                    stateList.PatchFull();
                }

                data.ResumeUpdateStates();

            Final:
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialOnOpenFriendsMenu((int)SocialResult.Success, peer);

                idset.Clear();
                removedID.Clear();

                lastOpenMenuTime = DateTime.Now;
            });
        }

        /// <summary>
        /// 發起申請好友請求
        /// </summary>
        public void SocialRaiseRequest_V0(string friendName, bool fromTemp)
        {
            if (checkIsSelf(friendName, peer.ZRPC.NonCombatRPC.Ret_SocialRaiseRequest))
                return;

            if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Increment();
            ExeQueue.Enqueue(async () =>
            {
                if (ENABLE_QUEUE_COUNTER) ExeQueueCounter.Decrement();
                if (peer.IsExitGame())
                    return;

                string msgParam = "name;" + friendName;
                string msg;
                SocialResult result;

                bool ForceSendSysMsg = false;

                SocialData data = GetData(peer);

                string myName = peer.CharacterData.Name;
                SocialData fdata;
                bool doSave = false, f_updated = false;
                var fpeer = GetPeer(friendName);

                bool temp_removed;
                if (fromTemp)//如果臨時名單內有對方，拿掉
                    temp_removed = data.RemoveFriendByName(FriendType.Temp, friendName) == SocialResult.Success;

                //當玩家在線上時
                if (fpeer != null)
                {
                    fdata = GetData(fpeer);
                }
                //當玩家不在線上時
                else
                {
                    fdata = await LoadSocialDataFromDB(friendName);
                    if (fdata == null)
                    {
                        msg = null;
                        result = SocialResult.PlayerNameNotFound;
                        goto Final;
                    }
                    doSave = true;
                }

                //我的黑名單中有對方故無法申請
                if (data.blackFriends.ContainsPlayerName(friendName))
                {
                    msg = "ret_social_RaiseRequest_TargetBlacked";
                    result = SocialResult.TargetBlacked;
                    goto Final;
                }

                //當對方好友清單已有我，不要發送請求
                if (fdata.goodFriends.ContainsPlayerName(myName))
                {
                    msg = "ret_social_RaiseRequest_AlreadyAddedGood";
                    ForceSendSysMsg = true;
                    result = SocialResult.AlreadyAddedGood;
                    goto Final;
                }

                //被加入黑名單故無法申請
                if (fdata.blackFriends.ContainsPlayerName(myName))
                {
                    msg = "ret_social_Blacked";
                    result = SocialResult.Blacked;
                    goto Final;
                }


                //嘗試新增我至對方請求清單
                result = fdata.AddFriend(FriendType.Request, peer.GetCharId(), myName, () => LoadCharaStateFromPeer(peer));

                if (result == SocialResult.ListFull)
                    msg = "ret_social_RaiseRequest_ListFull";
                else if (result == SocialResult.AlreadyAdded)
                    msg = "ret_social_RaiseRequest_AlreadyAdded";
                else
                {
                    f_updated = true;
                    msg = "ret_social_RaiseRequest_Success";
                }

                if (doSave)
                {
                    if (f_updated)
                        await Social_SaveSocialDataToDB(fdata, friendName);
                }

            Final:
                //發送系統訊息
                if ((DebugMode || ForceSendSysMsg) && msg != null)
                    peer.SendSystemMessage(msg, false, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRaiseRequest((int)result, friendName, peer);
            });
        }
        #endregion
    }
}

#pragma warning restore CS0162