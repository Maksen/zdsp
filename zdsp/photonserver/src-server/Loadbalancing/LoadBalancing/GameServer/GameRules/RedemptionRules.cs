using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Photon.LoadBalancing.GameServer;
using Photon.LoadBalancing.GameServer.Mail;
using Zealot.Common;

namespace Zealot.Server.Rules
{
    public class RedemptionInfo
    {
        public List<int> ServerList { get; private set; }
        public List<ItemInfo> ItemList { get; private set; }
        public DateTime ExpiryDT { get; private set; }
        public bool Status { get; private set; }

        public RedemptionInfo() { }

        public RedemptionInfo(Dictionary<string, object> data)
        {
            string servers = (string)data["ServerList"];
            ServerList = new List<int>();
            var serverIds = servers.Split(',');
            foreach (var serverid in serverIds)
                ServerList.Add(int.Parse(serverid));
            string items = (string)data["ItemList"];
            ItemList = new List<ItemInfo>();
            var iteminfos = items.Split(';');
            foreach (var item in iteminfos)
            {
                var itemdata = item.Split('|');
                ItemList.Add(new ItemInfo() { itemId = ushort.Parse(itemdata[0]), stackCount = int.Parse(itemdata[1]) });
            }
            ExpiryDT = (DateTime)data["ExpiryDT"];
            Status = (bool)data["Status"];
        }
    }

    public static class RedemptionRules
    {
        public static async Task TryRedeemSerialCode(string serial, GameClientPeer peer)
        {
            Tuple<int, Dictionary<string, object>> results = await GameApplication.dbGM.Redemption.GetSerialInfo(serial, peer.GetCharId());
            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                if (results.Item1 == 0)
                {
                    RedemptionInfo info = new RedemptionInfo(results.Item2);
                    int serverId = GameApplication.Instance.GetMyServerId();
                    bool isValid = info.Status && DateTime.Now < info.ExpiryDT && (info.ServerList.Contains(0) || info.ServerList.Contains(serverId));

                    if (isValid)
                        SendRedemptionMail(peer, serial, info.ItemList);
                    else
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Redeem_InvalidCode", "", false, peer);
                }
                else  // have error
                {
                    if (results.Item1 == 1)
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Redeem_InvalidCode", "", false, peer);
                    else if (results.Item1 == 2)
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Redeem_AlreadyClaimed", "", false, peer);
                    else if (results.Item1 == 3)
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Redeem_ReachedClaimLimit", "", false, peer);
                }
            });
        }

        private static void SendRedemptionMail(GameClientPeer peer, string serial, List<ItemInfo> items)
        {
            MailObject mailObj = new MailObject();
            mailObj.rcvName = peer.mChar;
            mailObj.mailName = "SerialCode_Redemption";
            List<IInventoryItem> list_Attachment = new List<IInventoryItem>();
            foreach (var item in items)
                list_Attachment.Add(GameRules.GenerateItem(item.itemId, null, item.stackCount));
            mailObj.lstAttachment = list_Attachment;

            MailResult mailResult = MailManager.Instance.SendRedemptionMail(mailObj);
            if (mailResult == MailResult.MailSuccess_MailSendOnline)
            {
                GameApplication.Instance.executionFiber.Enqueue(async () => await ClaimSerialCode(serial, peer).ConfigureAwait(false));
            }
            else if (mailResult == MailResult.MailFailed_InboxFull_Online)
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Redeem_MailboxFull", "", false, peer);
        }

        private static async Task ClaimSerialCode(string serial, GameClientPeer peer)
        {
            bool result = await GameApplication.dbGM.Redemption.ClaimSerialCode(serial, peer.GetCharId());

            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                if (result)
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Redeem_ClaimSucess", "", false, peer);
                else
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("DB Error: ClaimSerialCodeFailed", "", false, peer);
            });
        }

    }
}
