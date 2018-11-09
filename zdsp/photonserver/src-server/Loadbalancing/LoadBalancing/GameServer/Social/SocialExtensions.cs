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
            dic["ret_Social_InvalidOperation"] = "#ret_Social_InvalidOperation";
            dic["ret_Social_PlayerNameNotFound"] = "查無此玩家 {name}.";
            dic["ret_Social_PlayerIdNotFound"] = "查無此玩家.";
            dic["ret_Social_Blacked"] = "被加入黑名單(測試用)";
            dic["ret_Social_PlayerCantBeSelf"] = "無法對自己進行操作."; 

            dic["ret_SocialRaiseRequest_AlreadyAdded"] = "已經申請過 {name} 或者 {name} 已經是好友.";
            dic["ret_SocialRaiseRequest_ListFull"] = "玩家 {name} 能申請的名額已滿.";
            dic["ret_SocialRaiseRequest_Success"] = "向玩家 {name} 發出申請.";
            dic["ret_SocialRaiseRequest_TargetBlacked"] = "無法向玩家 {name} 發出申請，因為黑名單有該玩家.";

            dic["ret_SocialAcceptRequest_MyListFull"] = "我的好友名單已滿.";
            dic["ret_SocialAcceptRequest_ListFull"] = "對方的好友名單已滿.";
            dic["ret_SocialAcceptRequest_AlreadyAdded"] = "已新增過該玩家.";
            dic["ret_SocialAcceptRequest_Success"] = "玩家 {name} 和你成為好友了.";

            dic["ret_SocialRejectRequest_Success"] = "你拒絕了玩家 {name} 的好友申請.";

            dic["ret_SocialRejectAllRequest_Success"] = "已拒絕所有好友申請.";

            dic["ret_SocialAddBlack_RemoveGoodFriendFirst"] = "請先把要加入黑名單的玩家從好友清單中移除.";
            dic["ret_SocialAddBlack_AlreadyAdded"] = "玩家 {name} 已經加入過黑名單了.";
            dic["ret_SocialAddBlack_ListFull"] = "黑名單已滿.";
            dic["ret_SocialAddBlack_Success"] = "將玩家 {name} 加入了黑名單.";

            dic["ret_SocialRemoveBlack_Success"] = "將玩家 {name} 從黑名單移除了.";

            dic["ret_SocialRemoveGood_Success"] = "將玩家 {name} 從好友單移除了.";

            dic["ret_SocialRaiseAllTempRequest"] = "已將向所有臨時玩家發出好友申請.";

            dic["ret_SocialClearTemp"] = "已全部清除臨時玩家.";
        }

    }
}
