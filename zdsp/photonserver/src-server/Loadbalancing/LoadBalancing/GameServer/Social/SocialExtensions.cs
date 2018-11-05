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


namespace Photon.LoadBalancing.GameServer.Extensions
{
    
    public static partial class GameClientPeerExtensions
    {
        /// <summary>
        /// 先暫時放這邊 放上去時會自動變成以註冊的為主
        /// </summary>
        /// <param name="dic"></param>
        [UnRegisteredSysMsg]
        static void SocialUnRegisteredSysMsg(Dictionary<string,string> dic)
        {
            dic["ret_SocialRaiseRequestByName_AlreadyAdded"] = "已經申請過 {name} 或者 {name} 已經是好友";
            dic["ret_SocialRaiseRequestByName_Success"] = "已向玩家 {name} 發出申請.";
            dic["ret_SocialRaiseRequestByName_ListFull"] = "玩家 {name} 能申請的名額已滿.";
            dic["ret_SocialRaiseRequestByName_PlayerNameNotFound"] = "查無此玩家 {name}.";
            dic["ret_SocialRaiseRequestByName_PlayerIdNotFound"] = "查無此玩家.";
            dic["ret_Social_InvalidOperation"] = "#ret_Social_InvalidOperation";

            dic["ret_SocialAcceptRequest_MyListFull"] = "我的好友名單已滿";
            dic["ret_SocialAcceptRequest_ListFull"] = "對方的好友名單已滿";
            dic["ret_SocialAcceptRequest_AlreadyAdded"] = "已新增過該玩家";
            dic["ret_SocialAcceptRequest_Success"] = "已新增過該玩家";
        }

        static bool _SocialOnOpenFriendsMenu=false;

        static void AddID(List<string> ids,SocialFriendList friends)
        {
            foreach (var item in friends)
                ids.Add(item.id);
        }
        static void RemoveNotExist(HashSet<string> ids, SocialFriendList friends)
        {
            for(int i=friends.Count-1;i>=0 ;i--)
            {
                var f = friends[i];
                if (!ids.Contains( f.id))
                    friends.RemoveAt(i);
            }
        }
        public static async Task SocialOnOpenFriendsMenu(this GameClientPeer peer)
        {
            if(_SocialOnOpenFriendsMenu)
                throw new NotImplementedException();

            var stats = peer.mPlayer.SocialStats;
            var data = stats.data;

            List<string> ids = new List<string>(SocialInventoryData.MAX_COUNT);
            AddID(ids, data.goodFriends);
            AddID(ids, data.blackFriends);
            AddID(ids, data.requestFriends);
            AddID(ids, data.recommandFriends);

            //讀取db
            var chlist = await GameApplication.dbRepository.Character.GetSocialByCharaIds(ids);
            HashSet<string> set = new HashSet<string>();
            foreach(var ch in chlist)
                set.Add((string)ch["charid"]);

            RemoveNotExist(set, data.goodFriends);
            RemoveNotExist(set, data.blackFriends);
            RemoveNotExist(set, data.requestFriends);
            //如果沒有更新日期，一樣是拿掉被刪除的角色
            if (stats.CheckRecommandList())
                RemoveNotExist(set, data.recommandFriends);
        }


        /// <summary>
        /// 發起申請好友請求
        /// </summary>
        public static async Task SocialRaiseRequestByName(this GameClientPeer peer, string friendName)
        {
            var fpeer = GameApplication.Instance.GetCharPeer(friendName);
            string myName = peer.CharacterData.Name;
            SocialResult result;
            //當玩家在線上時
            if (fpeer != null)
            {
                var fstats = fpeer.mPlayer.SocialStats;
                if (fstats.data.goodFriends.ContainsPlayerName(myName))
                {
                    result = SocialResult.AlreadyAdded;
                    goto Final;
                }
                result = fstats.AddFriend(FriendType.Request, peer.GetCharId(), myName);
            }
            //當玩家不在線上時
            else
            {
                var dic = await GameApplication.dbRepository.Character.GetByNameAsync(friendName);
                object obj;
                if(!dic.TryGetValue("friends", out obj))
                {
                    result = SocialResult.PlayerNameNotFound;
                    goto Final;
                }
                string data = (string)obj;
                JToken json = null;
                try { json = JsonConvert.DeserializeObject<JToken>(data); } catch { }
                if(json==null)
                    json = new JObject();
                SocialData sdata = new SocialData(json);

                if (sdata.requestFriends.Count >= SocialInventoryData.MAX_RECOMMAND_COUNT)
                {
                    result = SocialResult.ListFull;
                    goto Final;
                }
                else if (sdata.requestFriends.ContainsPlayerName(myName) ||
                    sdata.goodFriends.ContainsPlayerName(myName))
                {
                    result = SocialResult.AlreadyAdded;
                    goto Final;
                }

                sdata.requestFriends.Add(new SocialFriend( FriendType.Request, peer.GetCharId(), myName));

                string save = JsonConvert.SerializeObject(sdata.Root, Formatting.None);
                await GameApplication.dbRepository.Character.UpdateSocialList(friendName, save, false);

                result = SocialResult.Success;
            }
        Final:
            //發送系統訊息
            peer.SendSystemMessage("ret_SocialRaiseRequestByName_" + result.ToString(), true, "name;"+ friendName);
            //發送結果
            peer.ZRPC.NonCombatRPC.Ret_SocialRaiseRequestByName((int)result,peer);
            
        }

        /// <summary>
        /// 接受申請好友請求
        /// </summary>
        public static async Task SocialAcceptRequest(this GameClientPeer peer,int index)
        {
            string msg=string.Empty;
            Dictionary<string, string> msgParam = null;

            SocialResult result= SocialResult.Success;
            var stats = peer.mPlayer.SocialStats;
            string friendName = null,friendId=null;
            int index2;
            //錯誤的請求索引
            if (index<0||index>=stats.data.requestFriends.Count)
            {
                msg = "ret_Social_InvalidOperation";
                result = SocialResult.InvalidOperation;
                goto Final;
            }
            //當我的好友名單已滿時
            if (stats.data.goodFriends.Count >= SocialInventoryData.MAX_GOOD_FRIENDS)
            {
                msg = "ret_SocialAcceptRequest_MyListFull";
                result = SocialResult.MyListFull;
                goto Final;
            }

            var friend = stats.data.requestFriends[index];
            friendName = friend.name;
            friendId = friend.id;
            var fpeer = GameApplication.Instance.GetCharPeer(friendName);
            string myName = peer.CharacterData.Name;
            //當玩家在線上時
            if (fpeer != null)
            {
                var fstats = fpeer.mPlayer.SocialStats;
                //新增我至對方好友清單
                result = fstats.AddFriend(FriendType.Good, peer.GetCharId(), myName);
                if (result == SocialResult.Success)
                {
                    //新增對方至我的好友清單
                    stats.AddFriend(FriendType.Good, fpeer.GetCharId(), friendName);
                    //移除對方發給我的請求
                    stats.RemoveFriendAt(FriendType.Request, index);
                    //如果對方有我之前發起的請求，移除它
                    fstats.RemoveFriend(FriendType.Request, peer.GetCharId());

                    msg = "ret_SocialAcceptRequest_Success";
                }
                else if (result == SocialResult.ListFull)
                {
                    msg = "ret_SocialAcceptRequest_ListFull";
                    result = SocialResult.ListFull;
                }
                else if (result == SocialResult.AlreadyAdded)
                {
                    msg = "ret_SocialAcceptRequest_AlreadyAdded";
                    result = SocialResult.AlreadyAdded;
                }

            }
            //當玩家不在線上時
            else
            {
                var dic = await GameApplication.dbRepository.Character.GetByNameAsync(friendName);
                object obj;
                if (!dic.TryGetValue("friends", out obj))
                {
                    result = SocialResult.PlayerNameNotFound;
                    goto Final;
                }
                string data = (string)obj;
                JToken json = null;
                try { json = JsonConvert.DeserializeObject<JToken>(data); } catch { }
                if (json == null)
                    json = new JObject();
                var sdata = new SocialData(json);

                //新增我至對方好友清單
                if (sdata.goodFriends.Count >= SocialInventoryData.MAX_RECOMMAND_COUNT)
                {
                    msg = "ret_SocialAcceptRequest_ListFull";
                    result = SocialResult.ListFull;
                    goto Final;
                }
                else if (sdata.goodFriends.ContainsPlayerName(myName))
                {
                    msg = "ret_SocialAcceptRequest_AlreadyAdded";
                    result = SocialResult.AlreadyAdded;
                    goto Final;
                }
                sdata.goodFriends.Add(new SocialFriend(FriendType.Request, peer.GetCharId(), myName));
                //新增對方至我的好友清單
                stats.AddFriend(FriendType.Good, fpeer.GetCharId(), friendName);
                //移除對方發給我的請求
                stats.RemoveFriendAt(FriendType.Request, index);
                //如果對方有我之前發起的請求，移除它
                if (sdata.requestFriends.ContainsPlayerName(myName,out index2))
                    sdata.requestFriends.RemoveAt(index2);

                string save = JsonConvert.SerializeObject(sdata.Root, Formatting.None);
                await GameApplication.dbRepository.Character.UpdateSocialList(friendName, save, false);

                msg = "ret_SocialAcceptRequest_Success";
                result = SocialResult.Success;
            }
        Final:
            //發送系統訊息
            peer.SendSystemMessage(msg, true, msgParam);
            //發送結果
            peer.ZRPC.NonCombatRPC.Ret_SocialAcceptRequest((int)result, peer);
        }
    }
}
