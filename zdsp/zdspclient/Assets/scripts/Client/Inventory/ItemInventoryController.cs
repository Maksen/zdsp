using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class ItemInventoryController
{
    public ItemInventoryData itemInvData;

    public ItemInventoryController()
    {
        itemInvData = new ItemInventoryData();
        itemInvData.InitDefault();
    }

    public void UpdateItemInv(int slotIdx, IInventoryItem item)
    {
        if (item != null && item.IsNew)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string color = ItemUtils.GetStrColorByRarity(item.JsonObject.rarity);
            parameters.Add("item", string.Format("<color={0}>{1}</color>", color, item.JsonObject.localizedname));
            int increment = 1;
            IInventoryItem old_item = itemInvData.Slots[slotIdx];
            if (old_item != null && old_item.ItemID == item.ItemID)
                increment = item.StackCount - old_item.StackCount;
            else
                increment = item.StackCount;
            parameters.Add("increment", increment.ToString());
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_ItemIncrement", parameters), true);
        }

        itemInvData.Slots[slotIdx] = item;
    }

    #region tooltip action
    public bool OnClicked_UseItem(int slotId, IInventoryItem item)
    {
        IInventoryItem _item = itemInvData.Slots[slotId];
        if (_item.ItemID == item.ItemID)
        {
            if (GameInfo.gLocalPlayer.GetAccumulatedLevel() < item.JsonObject.requirelvl)
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_UseItemFail_ReqlvlNotMeet"));
            else
            {
                switch(item.JsonObject.itemtype)
                {
                    case ItemType.Material:
                        var _materialJson = (MaterialJson)item.JsonObject;
                        if (_materialJson.mattype == MaterialType.Exchange)
                        {
                            int _itemCount = itemInvData.GetTotalStackCountByItemId(item.ItemID);
                            if (_itemCount < _materialJson.reqval)
                            {
                                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Exchange_CountNotMeet"));
                                return false;
                            }
                            else if (itemInvData.GetEmptySlotCount() == 0)
                            {
                                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_ItemBagFull"));
                                return false;
                            }
                        }
                        break;
                    case ItemType.Equipment:
                        //var _equipmentJson = (EquipmentJson)item.JsonObject;
                        if (itemInvData.GetEmptySlotCount() == 0)
                        {
                            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_ItemBagFull"));
                            return false;
                        }
                        break;
                }
                RPCFactory.CombatRPC.UseItem(slotId, 1);
                return true;
            }
        }
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_UseItemFail_SlotChanged"));
        return false;
    }

    public bool OnClicked_Equip(int slotId, IInventoryItem item, bool equipFashion)
    {
        IInventoryItem _item = itemInvData.Slots[slotId];
        if (_item.ItemID == item.ItemID)
        {
            if (GameInfo.gLocalPlayer.GetAccumulatedLevel() < item.JsonObject.requirelvl)
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_UseItemFail_ReqlvlNotMeet"));
            else
            {
                //var _equipmentJson = (EquipmentJson)item.JsonObject;
                if (itemInvData.GetEmptySlotCount() == 0)
                {
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_ItemBagFull"));
                    return false;
                }
                RPCFactory.CombatRPC.UseItem(slotId, equipFashion ? 0 : 1);
                return true;
            }
        }
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_UseItemFail_SlotChanged"));
        return false;
    }

    public bool OnClicked_UnEquip(int slotId, IInventoryItem item, bool fashionslot)
    {
        PlayerGhost _localplayer = GameInfo.gLocalPlayer;
        if (_localplayer == null)
            return false;
        IInventoryItem _item = fashionslot ? _localplayer.mEquipmentInvData.GetFashionSlot(slotId) : _localplayer.mEquipmentInvData.GetEquipmentBySlotId(slotId);
        if (_item == item)
        {
            if (itemInvData.GetEmptySlotCount() == 0)
            {
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_ItemBagFull"));
                return false;
            }
            RPCFactory.CombatRPC.UnequipItem(slotId, fashionslot);
            return true;
        }
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_UnEquip_SlotChanged"));
        return false;
    }
    #endregion
}
