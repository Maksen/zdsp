using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.Common.Entities.Social;
using Zealot.Common.Datablock;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.LoadBalancing.GameServer.Extensions;

namespace Photon.LoadBalancing.GameServer
{
    public class SocialController
    {
        #region Public Social API

        public enum SocialAddTempFriends_Result
        {
            Success,
            Player1NameNotFound,
            Player2NameNotFound,
            InGood,
            InBlack,
        }

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
        public void AddTempFriendsBoth(string name1, string name2,Action<SocialAddTempFriends_Result> completed)
        {
            _AddTempFriendsBoth(name1, name2, completed);
        }
        #endregion

        #region Constructor
        public SocialController(GameClientPeer peer)
        {
            this.peer = peer;
        }
        #endregion

        #region Private Fileds
        bool DebugMode = true;
        bool _SocialCheckNotCompletedApi = false;
        private GameClientPeer peer;
        #endregion

        #region Private Methods
        static void AddID(List<string> ids, SocialFriendList friends)
        {
            foreach (var item in friends)
                ids.Add(item.id);
        }

        static void RemoveNotExist(HashSet<string> ids, SocialFriendList friends)
        {
            for (int i = friends.Count - 1; i >= 0; i--)
            {
                var f = friends[i];
                if (!ids.Contains(f.id))
                    friends.RemoveAt(i);
            }
        }

        private void _AddTempFriendsBoth(string name1, string name2, Action<SocialAddTempFriends_Result> completed)
        {
            if (completed == null)
                throw new ArgumentNullException("completed");

            GameApplication.Instance.executionFiber.Enqueue(async () =>
            {
                SocialData data1;
                SocialData data2;
                string id1, id2;
                GameClientPeer peer1, peer2;

                bool doSave1 = false;
                bool doSave2 = false;

                peer1 = GameApplication.Instance.GetCharPeer(name1);
                if (peer1 != null)
                {
                    data1 = peer1.mPlayer.SocialStats.data;
                    id1 = peer1.GetCharId();
                }
                else
                {
                    var tp = await LoadSocialDataFromDBWithId(name1);
                    data1 = tp.Item1;
                    id1 = tp.Item2;
                    if (data1 == null)
                        return;
                    doSave1 = true;
                }

                peer2 = GameApplication.Instance.GetCharPeer(name1);
                if (peer2 != null)
                {
                    data2 = peer2.mPlayer.SocialStats.data;
                    id2 = peer2.GetCharId();
                }
                else
                {
                    var tp = await LoadSocialDataFromDBWithId(name2);
                    data2 = tp.Item1;
                    id2 = tp.Item2;
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

                data1.AddFriend(FriendType.Temp, id2, name2);
                data2.AddFriend(FriendType.Temp, id1, name1);

                if (doSave1)
                    await Social_SaveSocialDataToDB(data1, name1);
                if (doSave2)
                    await Social_SaveSocialDataToDB(data2, name2);
            });
        }

        private void _AddTempFriendsSingle(string name1, string name2, Action<SocialAddTempFriends_Result> completed)
        {
            if (completed == null)
                throw new ArgumentNullException("completed");

            GameApplication.Instance.executionFiber.Enqueue(async () =>
            {
                SocialData data1;
                SocialData data2;
                string id1, id2;
                GameClientPeer peer1, peer2;

                bool doSave1 = false;

                peer1 = GameApplication.Instance.GetCharPeer(name1);
                if (peer1 != null)
                {
                    data1 = peer1.mPlayer.SocialStats.data;
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

                peer2 = GameApplication.Instance.GetCharPeer(name1);
                if (peer2 != null)
                {
                    data2 = peer2.mPlayer.SocialStats.data;
                    id2 = peer2.GetCharId();
                }
                else
                {
                    var tp = await LoadSocialDataFromDBWithId(name2);
                    data2 = tp.Item1;
                    id2 = tp.Item2;
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

                data1.AddFriend(FriendType.Temp, id2, name2);

                if (doSave1)
                    await Social_SaveSocialDataToDB(data1, name1);
            });
        }


        bool checkIsSelf(string friendName, Action<int, GameClientPeer> act)
        {
            if (peer.CharacterData.Name == friendName)
            {
                peer.SendSystemMessage("ret_Social_PlayerCantBeSelf", true);
                //發送結果
                act((int)SocialResult.PlayerCantBeSelf, peer);

                return true;
            }
            return false;
        }

        static async Task<SocialData> LoadSocialDataFromDB(string name)
        {
            var dic = await GameApplication.dbRepository.Character.GetByNameAsync(name);
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


            return new SocialData(json);
        }

        static async Task<string> GetCharIdByName(string name)
        {
            var dic = await GameApplication.dbRepository.Character.GetByNameAsync(name);
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
            var dic = await GameApplication.dbRepository.Character.GetByNameAsync(name);
            object obj;

            //如果找不到該玩家(可能是被刪除了)
            if (!dic.TryGetValue("charid", out obj))
            {
                return null;
            }

            string charid;
            if (obj is Guid)
                charid = ((Guid)obj).ToString();
            else if (obj is string)
                charid = ((string)obj);
            else
                return null;

            if (!dic.TryGetValue("friends", out obj))
            {
                return null;
            }
            string jsonStr = (string)obj;
            JObject json = null;
            try { json = JsonConvert.DeserializeObject<JToken>(jsonStr) as JObject; } catch { }
            if (json == null)
                json = new JObject();


            return new Tuple<SocialData, string>(new SocialData(json), charid);
        }

        static async Task Social_SaveSocialDataToDB(SocialData data, string name)
        {
            string save = JsonConvert.SerializeObject(data.Root, Formatting.None);
            await GameApplication.dbRepository.Character.UpdateSocialList(name, save, false);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 開啟好友選單時呼叫這個method去檢查是否有玩家被刪除
        /// </summary>
        public void SocialOnOpenFriendsMenu()
        {
            if (_SocialCheckNotCompletedApi)
                throw new NotImplementedException();
            GameApplication.Instance.executionFiber.Enqueue(async () =>
            {
                var stats = peer.mPlayer.SocialStats;
                var data = stats.data;

                List<string> ids = new List<string>(
                    data.goodFriends.Count+ 
                    data.blackFriends.Count+ 
                    data.requestFriends.Count+ 
                    data.tempFriends.Count);
                AddID(ids, data.goodFriends);
                AddID(ids, data.blackFriends);
                AddID(ids, data.requestFriends);
                AddID(ids, data.tempFriends);

                //讀取db
                var chlist = await GameApplication.dbRepository.Character.GetSocialByCharaIds(ids);
                HashSet<string> set = new HashSet<string>();
                foreach (var ch in chlist)
                {
                    object id= ch["charid"];

                    if (id is Guid)
                        set.Add(((Guid)id).ToString());
                    else if (id is string)
                        set.Add(((string)id));
                }

                Guid guid;

                RemoveNotExist(set, data.goodFriends);
                RemoveNotExist(set, data.blackFriends);
                RemoveNotExist(set, data.requestFriends);
                //如果沒有更新日期，一樣是拿掉被刪除的角色
                if (data.CheckRecommandList())
                    RemoveNotExist(set, data.tempFriends);

                data.goodFriendStates.StopPatch();
                data.goodFriendStates.Clear();
                foreach (var item in data.goodFriends)
                {
                    data.goodFriendStates.Add(new SocialFriendState(
                        online: GameApplication.Instance.GetCharPeer(item.name) != null
                        ));
                }
                data.goodFriendStates.ResumePatch();
                data.goodFriendStates.PatchFull();


            Final:
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialOnOpenFriendsMenu((int)SocialResult.Success, peer);
            });
        }

        /// <summary>
        /// 將玩家加入黑名單
        /// </summary>
        public void SocialAddBlack(string friendName)
        {
            if (checkIsSelf(friendName, peer.ZRPC.NonCombatRPC.Ret_SocialAddBlack))
                return;

            GameApplication.Instance.executionFiber.Enqueue(async () => {
                string msg;
                SocialResult result;
                Dictionary<string, string> msgParam = new Dictionary<string, string>();
                msgParam.Add("name", friendName);

                var data = peer.mPlayer.SocialStats.data;
                //要加別人黑名單之前要先移除好友
                if (data.goodFriends.ContainsPlayerName(friendName))
                {
                    msg = "ret_SocialAddBlack_RemoveGoodFriendFirst";
                    result = SocialResult.RemoveGoodFriendFirst;
                    goto Final;
                }
                //已經加過黑名單
                if (data.blackFriends.ContainsPlayerName(friendName))
                {
                    msg = "ret_SocialAddBlack_AlreadyAdded";
                    result = SocialResult.AlreadyAdded;
                    goto Final;
                }
                //黑名單滿了
                if (data.blackFriends.Count >= data.getFriendMaxCount(FriendType.Black))
                {
                    msg = "ret_SocialAddBlack_ListFull";
                    result = SocialResult.ListFull;
                    goto Final;
                }

                string myName = peer.CharacterData.Name;
                SocialData fdata;
                bool doSave = false;
                var fpeer = GameApplication.Instance.GetCharPeer(friendName);
                string fid;
                //當玩家在線上時
                if (fpeer != null)
                {
                    fdata = fpeer.mPlayer.SocialStats.data;
                    fid = fpeer.GetCharId();
                }
                //當玩家不在線上時
                else
                {
                    var tp= await LoadSocialDataFromDBWithId(friendName);
                    fdata = tp.Item1;
                    fid = tp.Item2;
                    if (fdata == null)
                    {
                        msg = "ret_Social_PlayerNameNotFound";
                        result = SocialResult.PlayerNameNotFound;
                        goto Final;
                    }
                    doSave = true;
                }

                //將雙方的請求與暫時名單通通移除
                data.RemoveFriendByName(FriendType.Request, friendName);
                data.RemoveFriendByName(FriendType.Temp, friendName);
                fdata.RemoveFriendByName(FriendType.Request, myName);
                fdata.RemoveFriendByName(FriendType.Temp, myName);

                data.AddFriend(FriendType.Black, fid, friendName);

                if (doSave)
                    await Social_SaveSocialDataToDB(fdata, friendName);

                msg = "ret_SocialAddBlack_Success";
                result = SocialResult.Success;

            Final:
                //發送系統訊息
                peer.SendSystemMessage(msg, true, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialAddBlack((int)result, peer);

            });
        }

        /// <summary>
        /// 將玩家從黑名單中移除
        /// </summary>
        public void SocialRemoveBlack(string friendName)
        {
            if (checkIsSelf(friendName, peer.ZRPC.NonCombatRPC.Ret_SocialRemoveBlack))
                return;

            GameApplication.Instance.executionFiber.Enqueue(async () => {
                string msg;
                SocialResult result;
                Dictionary<string, string> msgParam = new Dictionary<string, string>();
                msgParam.Add("name", friendName);

                var data = peer.mPlayer.SocialStats.data;
                result=data.RemoveFriendByName(FriendType.Black, friendName);

                if (result == SocialResult.Success)
                    msg = "ret_SocialRemoveBlack_Success";
                else
                    msg = "ret_Social_PlayerNameNotFound";

                Final:
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

            GameApplication.Instance.executionFiber.Enqueue(async () => {
                string msg;
                SocialResult result;
                Dictionary<string, string> msgParam = new Dictionary<string, string>();
                msgParam.Add("name", friendName);

                var data = peer.mPlayer.SocialStats.data;

                SocialFriend friend;
                int index;
                if (!data.goodFriends.FindFriendByName(friendName, out friend, out index))
                {
                    msg = "ret_Social_PlayerNameNotFound";
                    result = SocialResult.PlayerNameNotFound;
                    goto Final;
                }

                string myName = peer.CharacterData.Name;
                SocialData fdata;
                bool doSave = false;
                var fpeer = GameApplication.Instance.GetCharPeer(friendName);
                //當玩家在線上時
                if (fpeer != null)
                {
                    fdata = fpeer.mPlayer.SocialStats.data;
                }
                //當玩家不在線上時
                else
                {
                    fdata= await LoadSocialDataFromDB(friendName);
                    if (fdata == null)
                    {
                        msg = "ret_Social_PlayerNameNotFound";
                        result = SocialResult.PlayerNameNotFound;
                        goto Final;
                    }
                    doSave = true;
                }

                data.RemoveFriendAt(FriendType.Good, index);
                data.goodFriendStates.RemoveAt(index);
                fdata.RemoveFriendByName(FriendType.Good,myName);

                if (doSave)
                    await Social_SaveSocialDataToDB(fdata, friendName);

                msg = "ret_SocialRemoveGood_Success";
                result = SocialResult.Success;

            Final:
                //發送系統訊息
                peer.SendSystemMessage(msg, true, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRemoveGood((int)result, peer);
            });
        }
       

        /// <summary>
        /// 發起申請好友請求
        /// </summary>
        public void SocialRaiseRequest(string friendName,Action completed=null)
        {
            if (completed==null)
            {
                if (checkIsSelf(friendName, peer.ZRPC.NonCombatRPC.Ret_SocialRaiseRequest))
                    return;
            }
            else
            {
                if (checkIsSelf(friendName, (i, p) => { }))
                {
                    completed();
                    return;
                }
            }

            GameApplication.Instance.executionFiber.Enqueue(async () =>
            {
                Dictionary<string, string> msgParam = new Dictionary<string, string>();
                msgParam.Add("name", friendName);
                string msg;
                SocialResult result;

                bool ForceSendSysMsg = false;

                SocialData data= peer.mPlayer.SocialStats.data;

                string myName = peer.CharacterData.Name;
                SocialData fdata;
                bool doSave = false;
                var fpeer = GameApplication.Instance.GetCharPeer(friendName);

                //當玩家在線上時
                if (fpeer != null)
                {
                    fdata = fpeer.mPlayer.SocialStats.data;
                }
                //當玩家不在線上時
                else
                {
                    fdata = await LoadSocialDataFromDB(friendName);
                    if (fdata == null)
                    {
                        ForceSendSysMsg = true;
                        msg = "ret_Social_PlayerNameNotFound";
                        result = SocialResult.PlayerNameNotFound;
                        goto Final;
                    }
                    doSave = true;
                }

                //我的黑名單中有對方故無法申請
                if (data.blackFriends.ContainsPlayerName(friendName))
                {
                    msg = "ret_SocialRaiseRequest_TargetBlacked";
                    result = SocialResult.TargetBlacked;
                    goto Final;
                }

                //當對方好友清單已有我，不要發送請求
                if (fdata.goodFriends.ContainsPlayerName(myName))
                {
                    msg = "ret_SocialRaiseRequest_AlreadyAdded";
                    result = SocialResult.AlreadyAdded;
                    goto Final;
                }
                //被加入黑名單故無法申請
                if (fdata.blackFriends.ContainsPlayerName(myName))
                {
                    msg = "ret_Social_Blacked";
                    result = SocialResult.Blacked;
                    goto Final;
                }

                

                //如果臨時名單內有對方，拿掉
                data.RemoveFriendByName(FriendType.Temp, friendName);

                //嘗試新增我至對方請求清單
                result = fdata.AddFriend(FriendType.Request, peer.GetCharId(), myName);

                if (result == SocialResult.ListFull)
                    msg = "ret_SocialRaiseRequest_ListFull";
                else if (result == SocialResult.AlreadyAdded)
                    msg = "ret_SocialRaiseRequest_AlreadyAdded";
                else
                    msg = "ret_SocialRaiseRequest_Success";

                if (doSave)
                    await Social_SaveSocialDataToDB(fdata, friendName);


                Final:
                if(completed==null)
                {
                    //發送系統訊息
                    if (DebugMode || ForceSendSysMsg)
                        peer.SendSystemMessage(msg, true, msgParam);
                    //發送結果
                    peer.ZRPC.NonCombatRPC.Ret_SocialRaiseRequest((int)result, peer);
                }
                else
                    completed();
            });
        }

        /// <summary>
        /// 接受申請好友請求
        /// </summary>
        public void SocialAcceptRequest(string friendName)
        {
            GameApplication.Instance.executionFiber.Enqueue(async () =>
            {
                string msg;
                Dictionary<string, string> msgParam = new Dictionary<string, string>();
                msgParam.Add("name", friendName);

                SocialResult result = SocialResult.InvalidOperation;
                var data = peer.mPlayer.SocialStats.data;
                int index;

                SocialFriend friend;
                //找不到該請求者
                if (!data.requestFriends.FindFriendByName(friendName, out friend, out index))
                {
                    msg = "ret_Social_PlayerNameNotFound";
                    result = SocialResult.InvalidOperation;
                    goto Final;
                }
                //當我的好友名單已滿時
                if (data.goodFriends.Count >= data.getFriendMaxCount(FriendType.Good))
                {
                    msg = "ret_SocialAcceptRequest_MyListFull";
                    result = SocialResult.MyListFull;
                    goto Final;
                }

                string myName = peer.CharacterData.Name;
                SocialData fdata;
                bool doSave = false;
                var fpeer = GameApplication.Instance.GetCharPeer(friendName);
                string fid;

                //當玩家在線上時
                if (fpeer != null)
                {
                    fdata = fpeer.mPlayer.SocialStats.data;
                    fid = fpeer.GetCharId();
                }
                //當玩家不在線上時
                else
                {
                    var tp= await LoadSocialDataFromDBWithId(friendName);
                    fdata = tp.Item1;
                    fid = tp.Item2;
                    if (fdata == null)
                    {
                        msg = "ret_Social_PlayerNameNotFound";
                        result = SocialResult.PlayerNameNotFound;
                        goto Final;
                    }
                    doSave = true;
                }

                if (fdata.blackFriends.ContainsPlayerName(myName))
                {
                    msg = "ret_Social_Blacked";
                    result = SocialResult.Blacked;
                    goto Final;
                }

                //嘗試新增我至對方好友清單
                result = fdata.AddFriend(FriendType.Good, peer.GetCharId(), myName);
                //對方好友清單已滿
                if (result == SocialResult.ListFull)
                {
                    msg = "ret_SocialAcceptRequest_ListFull";
                    result = SocialResult.ListFull;
                }
                //對方好友清單已有我
                else if (result == SocialResult.AlreadyAdded)
                {
                    msg = "ret_SocialAcceptRequest_AlreadyAdded";
                    result = SocialResult.AlreadyAdded;
                }
                else
                {
                    //新增對方至我的好友清單
                    data.AddFriend(FriendType.Good, fid, friendName);
                    data.goodFriendStates.Add(new SocialFriendState(online: fpeer != null));

                    //移除對方發給我的請求
                    data.RemoveFriendAt(FriendType.Request, index);
                    //如果對方有我之前發起的請求，移除它
                    fdata.RemoveFriendById(FriendType.Request, peer.GetCharId());

                    //如果我的臨時名單有對方，移除它
                    data.RemoveFriendByName(FriendType.Temp, friendName);
                    //如果對方臨時名單有我，移除它
                    fdata.RemoveFriendByName(FriendType.Temp, myName);

                    msg = "ret_SocialAcceptRequest_Success";
                }

                if (doSave)
                    await Social_SaveSocialDataToDB(fdata, friendName);

                Final:
                //發送系統訊息
                peer.SendSystemMessage(msg, true, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialAcceptRequest((int)result, peer);
            });
        }


        /// <summary>
        /// 一鍵接受所有申請
        /// </summary>
        public void SocialAcceptAllRequest()
        {
            GameApplication.Instance.executionFiber.Enqueue(async () =>
            {
                Dictionary<string, string> msgParam = null;

                List<string> friendNames = new List<string>();
                var data = peer.mPlayer.SocialStats.data;
                for (int x = 0; x < data.requestFriends.Count; x++)
                    friendNames.Add(data.requestFriends[x].name);

                //好友請求的預定移除清單
                List<int> requestToRemove = new List<int>();

                for (int index = 0; index < friendNames.Count; index++)
                {
                    SocialFriend friend;
                    string friendName;

                    //當我的好友名單已滿時
                    if (data.goodFriends.Count >= data.getFriendMaxCount(FriendType.Good))
                    {
                        break;
                    }
                    friend = data.requestFriends[index];
                    friendName = friend.name;

                    string myName = peer.CharacterData.Name;
                    SocialData fdata;
                    bool doSave = false;
                    var fpeer = GameApplication.Instance.GetCharPeer(friendName);
                    string fid;

                    //當玩家在線上時
                    if (fpeer != null)
                    {
                        fdata = fpeer.mPlayer.SocialStats.data;
                        fid = fpeer.GetCharId();
                    }
                    //當玩家不在線上時
                    else
                    {
                        var tp= await LoadSocialDataFromDBWithId(friendName);
                        fdata = tp.Item1;
                        fid = tp.Item2;
                        if (fdata == null)
                        {
                            requestToRemove.Add(index);
                            continue;
                        }
                        doSave = true;
                    }

                    //新增我至對方好友清單
                    if (fdata.AddFriend(FriendType.Good, peer.GetCharId(), myName) == SocialResult.Success)
                    {
                        //新增對方至我的好友清單
                        data.AddFriend(FriendType.Good, fid, friendName);
                        data.goodFriendStates.Add(new SocialFriendState(online: fpeer != null));
                        //移除對方發給我的請求(先放到預定清單中)
                        requestToRemove.Add(index);
                        //如果對方有我之前發起的請求，移除它
                        fdata.RemoveFriendById(FriendType.Request, peer.GetCharId());
                    }

                    if (doSave)
                        await Social_SaveSocialDataToDB(fdata, friendName);
                }
                //移除已經變成好友的請求
                data.RemoveFriendByIndices(FriendType.Request, requestToRemove.ToArray());

            Final:
                //發送系統訊息
                peer.SendSystemMessage("ret_SocialAcceptAllRequest_Success", true, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialAcceptAllRequest((int)SocialResult.Success, peer);
            });
        }

        /// <summary>
        /// 拒絕申請好友請求
        /// </summary>
        public void SocialRejectRequest(string friendName)
        {
            GameApplication.Instance.executionFiber.Enqueue(async () => {
                string msg;
                SocialResult result = SocialResult.InvalidOperation;
                Dictionary<string, string> msgParam = new Dictionary<string, string>();
                msgParam.Add("name", friendName);

                var data = peer.mPlayer.SocialStats.data;
                int index;
                SocialFriend friend;
                //找不到該請求名稱
                if (!data.requestFriends.FindFriendByName(friendName, out friend, out index))
                {
                    msg = "ret_Social_PlayerNameNotFound";
                    result = SocialResult.InvalidOperation;
                    goto Final;
                }

                msg = "ret_SocialRejectRequest_Success";
                data.requestFriends.RemoveAt(index);
                result = SocialResult.Success;


            Final:
                //發送系統訊息
                peer.SendSystemMessage(msg, true, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRejectRequest((int)result, peer);

            });
        }

        /// <summary>
        /// 一鍵拒絕所有申請
        /// </summary>
        public void SocialRejectAllRequest()
        {
            GameApplication.Instance.executionFiber.Enqueue(() => {
                string msg;
                SocialResult result;
                Dictionary<string, string> msgParam = null;

                var data = peer.mPlayer.SocialStats.data;

                data.requestFriends.Clear();

                msg = "ret_SocialRejectAllRequest_Success";
                result = SocialResult.Success;


            Final:
                //發送系統訊息
                peer.SendSystemMessage(msg, true, msgParam);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialRejectAllRequest((int)result, peer);

            });
        }



        /// <summary>
        /// 一鍵全加臨時名單好友
        /// </summary>
        public void SocialRaiseAllTempRequest()
        {
            var data = peer.mPlayer.SocialStats.data;
            int cntMax = data.tempFriends.Count;
            int cnt = 0;
            foreach (var f in data.tempFriends)
            {
                SocialRaiseRequest(f.name, () => 
                {
                    cnt++;
                    if (cnt >= cntMax)
                    {
                        //發送系統訊息
                        peer.SendSystemMessage("ret_SocialRaiseAllTempRequest", true);
                        //發送結果
                        peer.ZRPC.NonCombatRPC.Ret_SocialRaiseAllTempRequest(peer);
                    }
                });
            }
        }

        /// <summary>
        /// 一鍵清除臨時名單好友
        /// </summary>
        public void SocialClearTemp()
        {
            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                peer.mPlayer.SocialStats.data.tempFriends.Clear();
                //發送系統訊息
                peer.SendSystemMessage("ret_SocialClearTemp", true);
                //發送結果
                peer.ZRPC.NonCombatRPC.Ret_SocialClearTemp(peer);
            });
        }

        #endregion
    }
}
