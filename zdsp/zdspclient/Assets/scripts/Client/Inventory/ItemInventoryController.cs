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
            ItemBaseJson itemJson = item.JsonObject;
            string color = ItemUtils.GetStrColorByRarity(itemJson.rarity);
            parameters.Add("item", string.Format("<color={0}>{1}</color>", color, itemJson.localizedname));
            int incrementAmt = 1;
            IInventoryItem old_item = itemInvData.Slots[slotIdx];
            if (old_item != null && old_item.ItemID == item.ItemID)
                incrementAmt = item.StackCount - old_item.StackCount;
            else
                incrementAmt = item.StackCount;
            parameters.Add("increment", incrementAmt.ToString());
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_ItemIncrement", parameters), true);
        }

        // Quest
        if (GameInfo.gLocalPlayer != null)
        {
            int itemid = -1;
            if (item == null && itemInvData.Slots[slotIdx] != null)
                itemid = itemInvData.Slots[slotIdx].ItemID;
            else if (item != null)
                itemid = item.ItemID;
            GameInfo.gLocalPlayer.UpdateQuestRequirement(QuestRequirementType.Item, itemid);
        }

        itemInvData.Slots[slotIdx] = item; // Update inventory data
    }

    #region Tooltip actions
    public bool OnClicked_UseItem(int slotId, IInventoryItem item)
    {
        IInventoryItem _item = itemInvData.Slots[slotId];
        if (_item.ItemID == item.ItemID)
        {
            ItemBaseJson itemJson = item.JsonObject;
            if (GameInfo.gLocalPlayer.GetAccumulatedLevel() < itemJson.requirelvl)
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_UseItemFail_ReqlvlNotMeet"));
            else
            {
                switch(itemJson.itemtype)
                {
                    case ItemType.Material:
                        var _materialJson = (MaterialJson)itemJson;
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
                    case ItemType.MercenaryItem:
                        HeroItemJson heroItemJson = (HeroItemJson)itemJson;
                        int heroId;
                        if (int.TryParse(heroItemJson.heroid, out heroId) && heroId > 0)
                        {
                            Hero hero = GameInfo.gLocalPlayer.HeroStats.GetHero(heroId);
                            if (hero == null)
                            {
                                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_hero_HeroNotUnlocked"));
                                return false;
                            }
                            if (heroItemJson.heroitemtype == HeroItemType.Gift && heroItemJson.ischangelike == 0)
                            {
                                if (!hero.CanAddTrust())
                                {
                                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_hero_MaxTrustLevel"));
                                    return false;
                                }
                            }
                            else if (heroItemJson.heroitemtype == HeroItemType.HeroSkin && !string.IsNullOrEmpty(heroItemJson.heroskinpath))
                            {
                                if (hero.IsModelTierUnlocked(item.ItemID))
                                {
                                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_hero_SkinAlreadyUnlocked"));
                                    return false;
                                }
                            }
                            else
                            {
                                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_UseItemFailed"));
                                return false;
                            }
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

    public bool OnClicked_Equip(int slotId, IInventoryItem item, bool equipFashion, int eqSlotIdx)
    {
        IInventoryItem _item = itemInvData.Slots[slotId];
        if (_item.ItemID == item.ItemID)
        {
            if (GameInfo.gLocalPlayer.GetAccumulatedLevel() < item.JsonObject.requirelvl)
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_UseItemFail_ReqlvlNotMeet"));
            else
            {
                if (itemInvData.GetEmptySlotCount() == 0)
                {
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_ItemBagFull"));
                    return false;
                }
                RPCFactory.CombatRPC.UseItem(slotId, equipFashion ? 0 : 1, eqSlotIdx);
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
