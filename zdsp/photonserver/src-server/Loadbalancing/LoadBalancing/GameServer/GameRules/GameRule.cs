﻿using System.Collections.Generic;
using Photon.SocketServer;
using Photon.LoadBalancing.GameServer;
using Photon.LoadBalancing.GameServer.Mail;
using Photon.LoadBalancing.Operations;
using Kopio.JsonContracts;
using Zealot.Entities;
using Zealot.Server.Entities;
using Zealot.Common;
using Zealot.Common.RPC;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.SideEffects;
using Photon.LoadBalancing.Entities;
using UnityEngine;
using System;

namespace Zealot.Server.Rules
{
    public static partial class GameRules
    {
        public static void TeleportToPortalExit(Player player, string exitName)
        {
            GameClientPeer peer = player.Slot;
            string currentlevelname = player.mInstance.currentlevelname;
            int myProgressLvl = player.GetAccumulatedLevel();
            string[] exitNames = exitName.Split(';');
            List<LocationData> validExit = new List<LocationData>();
            for (int index = 0; index < exitNames.Length; index++)
            {
                LocationData mPortalExitData;
                if (PortalInfos.mExits.TryGetValue(exitNames[index], out mPortalExitData))
                {
                    if (mPortalExitData.mLevel == currentlevelname)
                        validExit.Add(mPortalExitData);
                    else
                    {
                        RealmJson realmJson = RealmRepo.GetPortalExitRealmInfo(mPortalExitData.mLevel);
                        if (realmJson != null && myProgressLvl >= realmJson.reqlvl)
                            validExit.Add(mPortalExitData);
                    }
                }
            }
            int validExit_count = validExit.Count;
            if (validExit_count > 0)
            {
                int index = 0;
                if (validExit_count > 1)
                    index = GameUtils.RandomInt(0, validExit_count - 1);
                LocationData mPortalExitData = validExit[index];
                if (mPortalExitData.mLevel == currentlevelname)
                    peer.ZRPC.CombatRPC.TeleportSetPosDirection(mPortalExitData.mPosition.ToRPCPosition(), mPortalExitData.mForward.ToRPCDirection(), peer);
                else
                {
                    string targetLevelName = mPortalExitData.mLevel;
                    RealmJson realmJson = RealmRepo.GetPortalExitRealmInfo(targetLevelName);
                    int realmId = realmJson.id;
                    string roomGuid = GameApplication.Instance.GameCache.TryGetRealmRoomGuid(realmId, realmJson.maxplayer);
                    if (!string.IsNullOrEmpty(roomGuid))
                        peer.TransferRoom(roomGuid, targetLevelName);
                    else
                        peer.CreateRealm(realmId, targetLevelName);
                    peer.mSpawnPos = mPortalExitData.mPosition;
                    peer.mSpawnForward = mPortalExitData.mForward;
                }
            }
        }

        public static void GenerateRewardByRewardGrpID(int rewardGrpId, int charBaseLv, int charJobLv, int jobId, List<ItemInfo> list_ItemInfo, Dictionary<CurrencyType, int> currencyAdded)
        {
            Reward jobReward = RewardListRepo.GetRewardByGrpIDJobID(rewardGrpId, jobId);
            if (jobReward == null)
                return;

            //Get Currency
            if (currencyAdded != null)
            {
                if (jobReward.Exp(charBaseLv) > 0)
                {
                    if (currencyAdded.ContainsKey(CurrencyType.Exp))
                        currencyAdded[CurrencyType.Exp] += jobReward.Exp(charBaseLv);
                    else
                        currencyAdded[CurrencyType.Exp] = jobReward.Exp(charBaseLv);
                }
                if (jobReward.Jxp(charJobLv) > 0)
                {
                    //if (currencyAdded.ContainsKey(CurrencyType.Exp))
                    //    currencyAdded[CurrencyType.Exp] += jobReward.Exp(charBaseLv);
                    //else
                    //    currencyAdded[CurrencyType.Exp] = jobReward.Exp(charBaseLv);
                }
                if (jobReward.guildactivepoint > 0)
                {
                    if (currencyAdded.ContainsKey(CurrencyType.GuildContribution))
                        currencyAdded[CurrencyType.GuildContribution] += jobReward.guildactivepoint;
                    else
                        currencyAdded[CurrencyType.GuildContribution] = jobReward.guildactivepoint;
                }
                if (jobReward.donatepoint > 0)
                {
                    //if (currencyAdded.ContainsKey(CurrencyType.))
                    //    currencyAdded[CurrencyType.GuildContribution] += jobReward.guildactivepoint;
                    //else
                    //    currencyAdded[CurrencyType.GuildContribution] = jobReward.guildactivepoint;
                }
                if (jobReward.money > 0)
                {
                    if (currencyAdded.ContainsKey(CurrencyType.Money))
                        currencyAdded[CurrencyType.Money] += jobReward.money;
                    else
                        currencyAdded[CurrencyType.Money] = jobReward.money;
                }
            }

            //Get item
            for (int i = 0; i < jobReward.itemRewardLst.Count; ++i)
            {
                ItemInfo ii = new ItemInfo();
                ii.itemId = (ushort)jobReward.itemRewardLst[i].id;
                ii.stackCount = jobReward.itemRewardLst[i].count;
                list_ItemInfo.Add(ii);
            }
        }

        //public static void GenerateReward(int rewardId, List<ItemInfo> list_ItemInfo, Dictionary<CurrencyType, int> currencyAdded/*, List<int> rewardItemIndex*/)
        //{
        //    /*
        //    var rewardJson = RewardListRepo.GetRewardById(rewardId);
        //    if (rewardJson == null)
        //        return;
        //    if (currencyAdded != null)
        //    {
        //        if (rewardJson.experience > 0)
        //        {
        //            if (currencyAdded.ContainsKey(CurrencyType.Exp))
        //                currencyAdded[CurrencyType.Exp] += rewardJson.experience;
        //            else
        //                currencyAdded[CurrencyType.Exp] = rewardJson.experience;
        //        }
        //        if (rewardJson.money > 0)
        //        {
        //            if (currencyAdded.ContainsKey(CurrencyType.Money))
        //                currencyAdded[CurrencyType.Money] += rewardJson.money;
        //            else
        //                currencyAdded[CurrencyType.Money] = rewardJson.money;
        //        }
        //        if (rewardJson.lockgold > 0)
        //        {
        //            if (currencyAdded.ContainsKey(CurrencyType.LockGold))
        //                currencyAdded[CurrencyType.LockGold] += rewardJson.lockgold;
        //            else
        //                currencyAdded[CurrencyType.LockGold] = rewardJson.lockgold;
        //        }
        //        if (rewardJson.vippoints > 0)
        //        {
        //            if (currencyAdded.ContainsKey(CurrencyType.VIP))
        //                currencyAdded[CurrencyType.VIP] += rewardJson.vippoints;
        //            else
        //                currencyAdded[CurrencyType.VIP] = rewardJson.vippoints;
        //        }
        //        if (rewardJson.guildgold > 0)
        //        {
        //            if (currencyAdded.ContainsKey(CurrencyType.GuildGold))
        //                currencyAdded[CurrencyType.GuildGold] += rewardJson.guildgold;
        //            else
        //                currencyAdded[CurrencyType.GuildGold] = rewardJson.guildgold;
        //        }
        //        if (rewardJson.guildcontribution > 0)
        //        {
        //            if (currencyAdded.ContainsKey(CurrencyType.GuildContribution))
        //                currencyAdded[CurrencyType.GuildContribution] += rewardJson.guildcontribution;
        //            else
        //                currencyAdded[CurrencyType.GuildContribution] = rewardJson.guildcontribution;
        //        }
        //        if (rewardJson.honorvalue > 0)
        //        {
        //            if (currencyAdded.ContainsKey(CurrencyType.HonorValue))
        //                currencyAdded[CurrencyType.HonorValue] += rewardJson.honorvalue;
        //            else
        //                currencyAdded[CurrencyType.HonorValue] = rewardJson.honorvalue;
        //        }
        //    }

        //    List<RewardItemInfo> items = RewardListRepo.GetRewardItemsById(rewardId);
        //    if (items == null)
        //        return;

        //    int count = items.Count;
        //    if (rewardJson.rewardtype == RewardType.All)
        //    {
        //        for (int index = 0; index < count; ++index)
        //        {
        //            ushort itemid = items[index].itemid;
        //            ushort itemcount = items[index].count;
        //            if (itemcount == 0)
        //                itemcount = (ushort)GameUtils.RandomInt(items[index].min, items[index].max);

        //            if (list_ItemInfo != null && itemid > 0 && itemcount > 0)
        //            {
        //                ItemInfo iteminfo = list_ItemInfo.Find(x => x.itemId == itemid);
        //                if (iteminfo != null)
        //                    iteminfo.stackCount += itemcount;
        //                else
        //                    list_ItemInfo.Add(new ItemInfo() { itemId = itemid, stackCount = itemcount });
        //            }
        //        }
        //    }
        //    else if (rewardJson.rewardtype == RewardType.Random)
        //    {
        //        for (int index = 0; index < count; ++index)
        //        {
        //            int randomNum = GameUtils.RandomInt(1, 100000); //1 - 100000
        //            if (randomNum <= items[index].percentage)
        //            {
        //                ushort itemid = items[index].itemid;
        //                ushort itemcount = items[index].count;
        //                if (itemcount == 0)
        //                    itemcount = (ushort)GameUtils.RandomInt(items[index].min, items[index].max);

        //                if (list_ItemInfo != null && itemid > 0 && itemcount > 0)
        //                {
        //                    ItemInfo iteminfo = list_ItemInfo.Find(x => x.itemId == itemid);
        //                    if (iteminfo != null)
        //                        iteminfo.stackCount += itemcount;
        //                    else
        //                        list_ItemInfo.Add(new ItemInfo() { itemId = itemid, stackCount = itemcount });
        //                }
        //            }
        //        }
        //    }
        //    else if (rewardJson.rewardtype == RewardType.Weight)
        //    {
        //        int totalweight = 0;
        //        for (int index = 0; index < count; ++index)
        //            totalweight += items[index].percentage;

        //        int randomNum = GameUtils.RandomInt(1, totalweight);
        //        for (int index = 0; index < count; ++index)
        //        {
        //            int weight = items[index].percentage;
        //            if (randomNum <= weight)
        //            {
        //                ushort itemid = items[index].itemid;
        //                ushort itemcount = items[index].count;
        //                if (itemcount == 0)
        //                    itemcount = (ushort)GameUtils.RandomInt(items[index].min, items[index].max);

        //                if (list_ItemInfo != null && itemid > 0 && itemcount > 0)
        //                {
        //                    ItemInfo iteminfo = list_ItemInfo.Find(x => x.itemId == itemid);
        //                    if (iteminfo != null)
        //                        iteminfo.stackCount += itemcount;
        //                    else
        //                        list_ItemInfo.Add(new ItemInfo() { itemId = itemid, stackCount = itemcount });
        //                }
        //                break;
        //            }
        //            randomNum -= weight;
        //        }
        //    }
        //    */
        //}

        //public static void GenerateRewardGrp(int grpId, List<ItemInfo> list_ItemInfo, Dictionary<CurrencyType, int> currencyAdded/*, List<int> rewardItemIndex*/)
        //{
        //    /*
        //    List<RewardListJson> rewardList = RewardListRepo.GetRewardByGrpId(grpId);
        //    if (rewardList != null)
        //    {
        //        foreach (var reward in rewardList)
        //            GenerateReward(reward.id, list_ItemInfo, currencyAdded);
        //    }
        //    */
        //}

        //public static List<ItemInfo> GenerateRewardGrps(List<int> grpIDs, Dictionary<CurrencyType, int> currencyAdded)
        //{
        //    List<ItemInfo> itemOutput = new List<ItemInfo>();
        //    for (int index = 0; index < grpIDs.Count; ++index)
        //        GenerateRewardGrp(grpIDs[index], itemOutput, currencyAdded);
        //    return itemOutput;
        //}

        public static List<ItemInfo> GiveReward_Bag(Player player, List<int> rewardGrpIDs, 
            bool checkNotification, bool setnew, string reason)
        {
            string playername = player.Name;
            List<ItemInfo> itemOutput = new List<ItemInfo>();
            Dictionary<CurrencyType, int> currencyAdded = new Dictionary<CurrencyType, int>();
            for (int index = 0; index < rewardGrpIDs.Count; ++index)
                GenerateRewardByRewardGrpID(rewardGrpIDs[index],
                                            player.PlayerSynStats.Level, 
                                            player.PlayerSynStats.progressJobLevel, 
                                            player.PlayerSynStats.jobsect, 
                                            itemOutput, 
                                            currencyAdded);

            //try and add all items
            foreach (var item in itemOutput)
            {
                var retVale = player.Slot.mInventory.AddItemsIntoInventory(item.itemId, item.stackCount, setnew, reason);
                if (retVale.retCode == InvReturnCode.AddSuccess)
                {
                    if (checkNotification)
                        RareItemNotificationRules.CheckNotification(item.itemId, playername);
                }
            }

            //even if fail bag check, add currency
            foreach (var currency in currencyAdded)
                player.AddCurrency(currency.Key, currency.Value, reason);
            return itemOutput;
        }

        public static List<ItemInfo> GiveReward_CheckBagSlot(Player player, List<int> rewardGrpIDs, 
            out bool isfull, bool checkNotification, bool setnew, string reason)
        {
            isfull = false;
            string playername = player.Name;
            List<ItemInfo> itemOutput = new List<ItemInfo>();
            Dictionary<CurrencyType, int> currencyAdded = new Dictionary<CurrencyType, int>();
            for (int index = 0; index < rewardGrpIDs.Count; ++index)
                GenerateRewardByRewardGrpID(rewardGrpIDs[index],
                                            player.PlayerSynStats.Level,
                                            player.PlayerSynStats.progressJobLevel,
                                            player.PlayerSynStats.jobsect,
                                            itemOutput,
                                            currencyAdded);

            var retValue = player.Slot.mInventory.AddItemsIntoInventory(itemOutput, setnew, reason);
            if (retValue.retCode != InvReturnCode.AddSuccess)
            {
                isfull = true;
                return null;
            }

            //if fail bag check, dun need add 
            foreach (var currency in currencyAdded)
            {
                player.AddCurrency(currency.Key, currency.Value, reason);
            }

            if (checkNotification)
            {
                for (int index = 0; index < itemOutput.Count; ++index)
                    RareItemNotificationRules.CheckNotification(itemOutput[index].itemId, playername);
            }
            return itemOutput;
        }

        public static List<ItemInfo> GiveReward_CheckBagSlotThenMail(Player player, List<int> rewardGrpIDs, string mailName, 
            Dictionary<string, string> parameters, bool checkNotification, bool setnew, string reason)
        {
            string playername = player.Name;
            List<ItemInfo> itemOutput = new List<ItemInfo>();
            Dictionary<CurrencyType, int> currencyAdded = new Dictionary<CurrencyType, int>();
            for (int index = 0; index < rewardGrpIDs.Count; ++index)
                GenerateRewardByRewardGrpID(rewardGrpIDs[index],
                                            player.PlayerSynStats.Level,
                                            player.PlayerSynStats.progressJobLevel,
                                            player.PlayerSynStats.jobsect,
                                            itemOutput, 
                                            currencyAdded);

            var retValue = player.Slot.mInventory.AddItemsIntoInventory(itemOutput, setnew, reason);
            if (retValue.retCode != InvReturnCode.AddSuccess)
            {
                //if cant add to bag, send mail
                MailObject mailObj = new MailObject();
                mailObj.rcvName = player.Name;
                mailObj.mailName = mailName;
                List<IInventoryItem> list_Attachment = new List<IInventoryItem>();
                foreach (var item in itemOutput)
                    list_Attachment.Add(GenerateItem(item.itemId, null, item.stackCount));

                mailObj.lstAttachment = list_Attachment;
                mailObj.dicCurrencyAmt = currencyAdded;
                if (parameters != null && parameters.Count > 0)
                    mailObj.dicBodyParam = parameters;
                MailManager.Instance.SendMail(mailObj);
            }
            else
            {
                //add currency
                foreach (var currency in currencyAdded)
                {
                    player.AddCurrency(currency.Key, currency.Value, reason);
                }
                if (checkNotification)
                {
                    for (int index = 0; index < itemOutput.Count; ++index)
                        RareItemNotificationRules.CheckNotification(itemOutput[index].itemId, playername);
                }
            }
            return itemOutput;
        }

        public static List<ItemInfo> GiveReward_Mail(string playername, string mailName, PlayerSynStats playerSynStats, 
                                                    List<int> rewardIDs, Dictionary<string, string> parameters)
        {
            List<ItemInfo> itemOutput = new List<ItemInfo>();
            List<int> rewardItemIndex = new List<int>();
            Dictionary<CurrencyType, int> currencyAdded = new Dictionary<CurrencyType, int>();
            for (int index = 0; index < rewardIDs.Count; ++index)
                GenerateRewardByRewardGrpID(rewardIDs[index],
                                            playerSynStats.Level,
                                            playerSynStats.progressJobLevel,
                                            playerSynStats.jobsect,
                                            itemOutput, 
                                            currencyAdded);

            MailObject mailObj = new MailObject();
            mailObj.rcvName = playername;
            mailObj.mailName = mailName;
            List<IInventoryItem> list_Attachment = new List<IInventoryItem>();
            foreach (var item in itemOutput)
                list_Attachment.Add(GenerateItem(item.itemId, null, item.stackCount));

            mailObj.lstAttachment = list_Attachment;
            mailObj.dicCurrencyAmt = currencyAdded;
            if (parameters != null && parameters.Count > 0)
            {
                mailObj.dicBodyParam.Clear();
                foreach (var e in parameters)
                    mailObj.dicBodyParam.Add(e.Key, e.Value);
            }
            MailManager.Instance.SendMail(mailObj);
            return itemOutput;
        }
        
        public static List<ItemInfo> GiveRewardGrp_CheckBagSlotThenMail_WithAdditionalItems(Player player, List<int> grpIDs, string mailName,
            Dictionary<string, string> parameters, bool checkNotification, bool setnew, string reason,
            int rewardMultiplier, int extraRewardID, int extraRewardPercent, int extraRewardStackcount)
        {
            string playername = player.Name;
            List<ItemInfo> itemOutput = new List<ItemInfo>();
            //List<int> rewardItemIndex = new List<int>();
            Dictionary<CurrencyType, int> currencyAdded = new Dictionary<CurrencyType, int>();

            for (int index = 0; index < grpIDs.Count; ++index)
                GenerateRewardByRewardGrpID(grpIDs[index],
                                            player.PlayerSynStats.Level,
                                            player.PlayerSynStats.progressJobLevel,
                                            player.PlayerSynStats.jobsect,
                                            itemOutput,
                                            currencyAdded);

            foreach (var iteminfo in itemOutput)
            {
                iteminfo.stackCount *= rewardMultiplier;
            }

            var rand = GameUtils.Random(0, 100);
            if (rand < extraRewardPercent)
            {
                var newItem = new ItemInfo();
                newItem.itemId = (ushort)extraRewardID;
                newItem.stackCount = extraRewardStackcount;
                itemOutput.Add(newItem);
            }

            var retValue = player.Slot.mInventory.AddItemsIntoInventory(itemOutput, setnew, reason);
            if (retValue.retCode != InvReturnCode.AddSuccess)
            {
                // If cant add to bag, send mail
                MailObject mailObj = new MailObject();
                mailObj.rcvName = playername;
                mailObj.mailName = mailName;
                List<IInventoryItem> list_Attachment = new List<IInventoryItem>();
                foreach (var item in itemOutput)
                    list_Attachment.Add(GenerateItem(item.itemId, null, item.stackCount));

                mailObj.lstAttachment = list_Attachment;
                mailObj.dicCurrencyAmt = currencyAdded;
                if (parameters != null && parameters.Count > 0)
                    mailObj.dicBodyParam = parameters;
                MailManager.Instance.SendMail(mailObj);
            }
            else
            {
                // Add currency
                foreach (var currency in currencyAdded)
                {
                    player.AddCurrency(currency.Key, currency.Value * rewardMultiplier, reason);
                }
                if (checkNotification)
                {
                    for (int index = 0; index < itemOutput.Count; ++index)
                        RareItemNotificationRules.CheckNotification(itemOutput[index].itemId, playername);
                }
            }
            return itemOutput;
        }

        public static void SendMailWithItem(string playerName, string mailName, List<IInventoryItem> itemsToAttach, Dictionary<string, string> parameters = null)
        {
            MailObject mailObj = new MailObject();
            mailObj.rcvName = playerName;
            mailObj.mailName = mailName;
            mailObj.lstAttachment = itemsToAttach;
            if (parameters != null && parameters.Count > 0)
                mailObj.dicBodyParam = parameters;
            MailManager.Instance.SendMail(mailObj);
        }

        public static void SendMail(string playerName, string mailName, Dictionary<string, string> parameters = null)
        {
            MailObject mailObj = new MailObject();
            mailObj.rcvName = playerName;
            mailObj.mailName = mailName;
            if (parameters != null && parameters.Count > 0)
                mailObj.dicBodyParam = parameters;
            MailManager.Instance.SendMail(mailObj);
        }

        public static IInventoryItem GenerateItem(int itemId, GameClientPeer peer, int stackcount = 1, bool superability = false)
        {
            IInventoryItem retval = GameRepo.ItemFactory.GetInventoryItem(itemId);
            if (retval == null)
                return null;

            switch (retval.JsonObject.itemtype)
            {
                case ItemType.PotionFood:
                case ItemType.Material:
                case ItemType.LuckyPick:
                case ItemType.Henshin:
                case ItemType.Features:
                case ItemType.Equipment:
                case ItemType.DNA:
                case ItemType.Relic:
                case ItemType.QuestItem:
                case ItemType.MercenaryItem:
                case ItemType.InstanceItem:
                case ItemType.PetItem:
                    retval.StackCount = (ushort)stackcount;
                    break;
                default:
                    break;
            }

            //if (peer != null && retval.GenerateUID)
            //    retval.UID = peer.mInventory.GenerateItemUID();
            return retval;
        }

        //public static EquipItem GenerateEquipment(int itemid)
        //{
        //    //EquipItem retval = GameRepo.ItemFactory.GetEquipmentItem(itemid);
        //    //List<int> attributes = new List<int>();

        //    //bool isWeapon = GameRepo.ItemFactory.GetEquipmentById(itemid).equipmenttype == EquipmentType.Weapon;
        //    //bool isJewelry = GameRepo.ItemFactory.GetEquipmentById(itemid).equipmenttype == EquipmentType.Decor1 ||
        //    //    GameRepo.ItemFactory.GetEquipmentById(itemid).equipmenttype == EquipmentType.Decor2;

        //    //if (isWeapon || isJewelry)
        //    //    attributes = retval.GenerateSpecialAttributesForWeapon(itemid);
        //    //else
        //    //    attributes = retval.GenerateSpecialAttributesForArmor(itemid);//to do change!

        //    //retval.SpecialAttributes = retval.EncodeSpecialAttributes(attributes);

        //    if (retval == null)
        //        return null;

        //    return retval;
        //}

        public static EventData GetLocalObjEventData(Dictionary<byte, object> dic)
        {
            var eventData = new EventData((byte)OperationCode.LocalObject, dic);
            return eventData;
        }

        public static SendParameters GetSendParam(bool reliable)
        {
            SendParameters para = new SendParameters();
            para.Unreliable = !reliable;
            return para;
        }

        public static void ApplySideEffect(int seid, Actor caster, Actor target)
        {
            SideEffectJson sej = SideEffectRepo.GetSideEffect(seid);
            if (sej == null || !SideEffectFactory.IsSideEffectInstantiatable(sej))
                return;

            if (sej.persistentafterdeath || sej.persistentonlogout)
            {
                SpecailSE sse = new SpecailSE(sej);
                sse.Apply(target);
            }
            else
            {
                SideEffect se = SideEffectFactory.CreateSideEffect(sej);
                se.Apply(target, caster, SideEffectsUtils.IsSideEffectPositive(sej));
            }
        }

        public static void SaveToDB()
        {
            //LadderRules.SaveArenaRank();
            var saved = LootRules.SaveLimitedItemsToDB();
            //CountryRules.SaveReport();
        }

        #region New Character Data
        /// <summary>
        /// Creates and instantiates default character data according to <paramref name="tutorialDone"/>
        /// </summary>
        /// <returns>Default Character Data</returns>
        public static CharacterData NewCharacterData(bool tutorialDone, string charname, byte jobsect, byte faction, byte talent)
        {
            NewCharInfo newCharInfo = GameConstantRepo.mNewCharInfo;
            CharacterData newCharData = new CharacterData
            {
                TrainingRealmDone = true,
                ProgressLevel = 1,
                Name = charname,
                JobSect = jobsect,
                Faction = faction,
                Health = -1,
                portraitID = 1,
                lastlevelid = newCharInfo.levelId,
                lastpos = newCharInfo.pos,
                lastdirection = newCharInfo.dir,
                BotSetting = "",
                CurrencyExchangeTime = 0,
                GetRecommendedFactionReward = false,
            };
            newCharData.InitDefault((JobType)jobsect);
            SetCharacterFirstEquipments(newCharData.EquipmentInventory);

            return newCharData;
        }

        public static void SetCharacterFirstEquipments(EquipmentInventoryData equipmentInvData)
        {
            Dictionary<EquipmentSlot, int> slotItem = new Dictionary<EquipmentSlot, int>();
            slotItem.Add(EquipmentSlot.Weapon, 2);

            foreach(var kvp in slotItem)
            {
                int itemId = kvp.Value;
                if (itemId != 0)
                {
                    Equipment equipItem = GenerateItem(itemId, null) as Equipment;
                    if (equipItem == null)
                        continue;
                    equipmentInvData.SetEquipmentToSlot((int)kvp.Key, equipItem);
                }
            }
        }
        #endregion
    }
}
