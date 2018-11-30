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
            //not used
            dic["ret_social_InvalidOperation"] = "#ret_social_InvalidOperation";
            dic["ret_social_PlayerIdNotFound"] = "查無此玩家";


            //for debug
            dic["ret_social_Blacked"] = "[Debug] 被加入黑名單(測試用)";

            dic["ret_social_RaiseRequest_AlreadyAdded"] = "[Debug] 已經申請過 {name}. (測試用)";
            dic["ret_social_RaiseRequest_ListFull"] = "[Debug] 玩家 {name} 能申請的名額已滿.(測試用)";
            dic["ret_social_RaiseRequest_Success"] = "[Debug] 向玩家 {name} 發出申請.(測試用)";

            //not added
            dic["ret_social_PlayerServerChanged"] = "玩家已經下線或者更換分流";

            //used
            dic["ret_social_PlayerNameNotFound"] = "查無此玩家 {name}";
            dic["ret_social_PlayerCantBeSelf"] = "無法對自己進行操作."; 

            dic["ret_social_AcceptRequest_MyListFull"] = "我的好友名單已滿!";
            dic["ret_social_AcceptRequest_ListFull"] = "{name}的好友名單已滿!";
            dic["ret_social_AcceptRequest_AlreadyAdded"] = "{name}已在好友名單內";
            dic["ret_social_AcceptRequest_Success"] = "玩家 {name} 和你成為好友了.";

            dic["ret_social_RejectRequest_Success"] = "你拒絕了玩家 {name} 的好友申請.";

            dic["ret_social_RejectAllRequest_Success"] = "已拒絕所有好友申請.";

            dic["ret_social_AddBlack_RemoveGoodFriendFirst"] = "請先把要加入黑名單的玩家從好友清單中移除.";
            dic["ret_social_AddBlack_AlreadyAdded"] = "{name}已在黑名單內";
            dic["ret_social_AddBlack_ListFull"] = "黑名單已滿!";
            dic["ret_social_AddBlack_Success"] = "玩家 {name} 加入了黑名單";

            dic["ret_social_AcceptAllRequest_Success"] = "接受了所有好友申請"; 

            dic["ret_social_RemoveBlack_Success"] = "玩家 {name} 已從黑名單移除";

            dic["ret_social_RemoveGood_Success"] = "玩家 {name} 已從好友單移除";

            dic["ret_social_RaiseAllTempRequest"] = "已向所有臨時玩家發出好友申請";

            dic["ret_social_ClearTemp"] = "已全部清除臨時玩家";

            dic["ret_social_RaiseRequest_AlreadyAddedGood"] = "玩家 {name} 已經是好友";
            dic["ret_social_RaiseRequest_TargetBlacked"] = "無法向玩家 {name} 發出申請，因為黑名單有該玩家.";

            dic["ret_social_PlayerNameNotFoundInList"] = "清單中沒有該玩家 {name}";
        }

    }
}
