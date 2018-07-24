using System;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Logging.Client.LogClasses;
using Zealot.Repository;
using Zealot.Server.Entities;
using Zealot.Server.Rules;

namespace Photon.LoadBalancing.GameServer.Chest
{
    public class Chest
    {
        private int itemId;
        private int type;
        public void OpenChest(GameClientPeer mSlot, int itemId, int type)
        {
            this.itemId = itemId;
            this.type = type;
            //寶箱福袋開啟後須經歷三步驟
            //1.從獎勵表取得獎勵清單(不直接發獎給玩家)
            //2.與保底機制溝通 寶箱或福袋ID 及 獎勵清單
            //3.真正發獎給玩家
            List<ItemInfo> rewardList = GetRewardList(itemId);
            PrizeGuaranteeRules.GetPrizeGuarantee(mSlot, PrizeGuaranteeType.Chest, itemId, rewardList);
            SendItemToPlayerInv(mSlot.mPlayer, rewardList);
        }
        private List<ItemInfo> GetRewardList(int itemId)
        {
            //從獎勵表取得獎勵清單(不直接發獎給玩家)
            List<ItemInfo> itemsToAttach = new List<ItemInfo>();
            Dictionary<CurrencyType, int> currency = new Dictionary<CurrencyType, int>();

            //int gid = ChestRepo.GetRewardGroupIdByItemId(itemId);
            //GameRules.GenerateRewardGrp(gid, itemsToAttach, currency);

            //currency is not added

            return itemsToAttach;
        }

        private void SendItemToPlayerInv(Player player, List<ItemInfo> itemsToAttach)
        {
            //真正發獎給玩家
            ItemInventoryController inventory = player.Slot.mInventory;
            foreach (ItemInfo itemInfo in itemsToAttach)
            {
                IInventoryItem item = GameRules.GenerateItem(itemInfo.itemId,player.Slot, itemInfo.stackCount);
                InvRetval res = inventory.AddItemsIntoInventory(item, true, "Chest");
                RareItemNotificationRules.CheckNotification(item.ItemID, player.Name);

                ChestLog chestLog = new ChestLog();
                chestLog.userId = player.Slot.mUserId;
                chestLog.charId = player.Slot.GetCharId();
                //chestLog.type = ((ChestType)this.type).ToString();
                chestLog.itemId = this.itemId;
                chestLog.rewardItemId = itemInfo.itemId;
                chestLog.rewardItemCount = itemInfo.stackCount;
                var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(chestLog);
            }
        }

    }
}
