using Kopio.JsonContracts;
using Photon.LoadBalancing.GameServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;

namespace Zealot.Server.Rules
{
    public enum UpdateRetval
    {
        Success,
        AddFailed,
        SlottingFailed,
    }

    public static class EquipmentRules
    {
        public static InvRetval UseMaterials(EquipUpgMaterial material, GameClientPeer peer)
        {
            return peer.mPlayer.Slot.mInventory.UseToolItems((ushort)material.mMat.mItemID, (ushort)material.mMat.mAmount, "EquipUpgrade");
        }

        public static bool IsEnoughReformMaterials(List<EquipModMaterial> matList, GameClientPeer peer)
        {
            for(int i = 0; i < matList.Count; ++i)
            {
                EquipModMaterial mat = matList[i];
                int invItemCount = peer.GetTotalStackCountByItemID(mat.mItemID);

                if(invItemCount < mat.mAmount)
                {
                    return false;
                }
            }

            return true;
        }

        public static void LogEquipUpgrade(string type, string equipSlot, int upgradeLvlAft, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Equip Slot: {1} | New Upgrade Level: {2}",
                type,
                equipSlot,
                upgradeLvlAft);

            Zealot.Logging.Client.LogClasses.EquipmentUpgrade equipUpgradeLog = new Zealot.Logging.Client.LogClasses.EquipmentUpgrade();
            equipUpgradeLog.userId          = peer.mUserId;
            equipUpgradeLog.charId          = peer.GetCharId();
            equipUpgradeLog.message         = message;
            equipUpgradeLog.type            = type;
            equipUpgradeLog.equipSlot       = equipSlot;
            equipUpgradeLog.upgradeLvlAft   = upgradeLvlAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(equipUpgradeLog);
        }

        public static void LogEquipMoney(int moneyCost, int moneyBefore, int moneyAfter, string system, GameClientPeer peer)
        {
            string message = string.Format("Money Used: {0} | Money Before: {1} | Money After: {2} | System: {3}",
                moneyCost,
                moneyBefore,
                moneyAfter,
                system);

            Zealot.Logging.Client.LogClasses.EquipmentMoneyUse equipMoneyUseLog = new Zealot.Logging.Client.LogClasses.EquipmentMoneyUse();
            equipMoneyUseLog.userId     = peer.mUserId;
            equipMoneyUseLog.charId     = peer.GetCharId();
            equipMoneyUseLog.message    = message;
            equipMoneyUseLog.amount     = moneyCost;
            equipMoneyUseLog.moneyBef   = moneyBefore;
            equipMoneyUseLog.moneyAft   = moneyAfter;
            equipMoneyUseLog.system     = system;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(equipMoneyUseLog);
        }

        public static void LogEquipItemUse(int itemid, int count, string system, GameClientPeer peer)
        {
            string message = string.Format("Use Item ID: {0} | Use Count: {1} | System: {2}",
                itemid,
                count,
                system);

            Zealot.Logging.Client.LogClasses.EquipmentItemUse equipItemUseLog = new Zealot.Logging.Client.LogClasses.EquipmentItemUse();
            equipItemUseLog.userId  = peer.mUserId;
            equipItemUseLog.charId  = peer.GetCharId();
            equipItemUseLog.message = message;
            equipItemUseLog.itemId  = itemid;
            equipItemUseLog.count   = count;
            equipItemUseLog.system  = system;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(equipItemUseLog);
        }

        public static void LogEquipItemGet(int itemid, int count, string system, GameClientPeer peer)
        {
            string message = string.Format("Get Item ID: {0} | Get Count: {1} | System: {2}",
                itemid,
                count,
                system);

            Zealot.Logging.Client.LogClasses.EquipmentItemGet equipItemGetLog = new Zealot.Logging.Client.LogClasses.EquipmentItemGet();
            equipItemGetLog.userId  = peer.mUserId;
            equipItemGetLog.charId  = peer.GetCharId();
            equipItemGetLog.message = message;
            equipItemGetLog.itemId  = itemid;
            equipItemGetLog.count   = count;
            equipItemGetLog.system  = system;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(equipItemGetLog);
        }

        public static void LogEquipGemSlot(string type, string equipSlot, int gemID, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Equip Slot: {1} | Gem ID : {2}",
                type,
                equipSlot,
                gemID);

            Zealot.Logging.Client.LogClasses.EquipmentGemSlot equipGemSlotLog = new Zealot.Logging.Client.LogClasses.EquipmentGemSlot();
            equipGemSlotLog.userId          = peer.mUserId;
            equipGemSlotLog.charId          = peer.GetCharId();
            equipGemSlotLog.message         = message;
            equipGemSlotLog.type            = type;
            equipGemSlotLog.equipSlot       = equipSlot;
            equipGemSlotLog.gemID           = gemID;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(equipGemSlotLog);
        }
    }
}
