using Kopio.JsonContracts;
using System.Collections.Generic;
using Photon.SocketServer;
using Photon.LoadBalancing.GameServer;
using Photon.LoadBalancing.GameServer.Mail;
using Photon.LoadBalancing.Operations;
using Zealot.Common;
using Zealot.Common.RPC;
using Zealot.Common.Entities;
using Zealot.Entities;
using Zealot.Repository;
using Zealot.Server.Entities;
using Zealot.Server.SideEffects;

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
            int numOfExits = exitNames.Length;
            List <LocationData> validExits = new List<LocationData>();
            for (int index = 0; index < numOfExits; ++index)
            {
                LocationData mPortalExitData;
                if (PortalInfos.mExits.TryGetValue(exitNames[index], out mPortalExitData))
                {
                    if (mPortalExitData.mLevel == currentlevelname)
                        validExits.Add(mPortalExitData);
                    else
                    {
                        RealmJson realmJson = RealmRepo.GetPortalExitRealmInfo(mPortalExitData.mLevel);
                        if (realmJson != null && myProgressLvl >= realmJson.reqlvl)
                            validExits.Add(mPortalExitData);
                    }
                }
            }

            int validExits_count = validExits.Count;
            if (validExits_count > 0)
            {
                int index = (validExits_count > 1) ? GameUtils.RandomInt(0, validExits_count-1) : 0;
                LocationData mPortalExitData = validExits[index];
                peer.mPortalExit = mPortalExitData;
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

        public static void GenerateRewardByRewardGrpID(int rewardGrpId, int charBaseLv, int charJobLv, int jobId, List<ItemInfo> list_ItemInfo, Dictionary<CurrencyType, int> currencyAdded, float expboost = 1.0f)
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
                        currencyAdded[CurrencyType.Exp] += (int)(jobReward.Exp(charBaseLv) * expboost);
                    else
                        currencyAdded[CurrencyType.Exp] = jobReward.Exp(charBaseLv);
                }
                //if (jobReward.Jxp(charJobLv) > 0)
                //{
                    //if (currencyAdded.ContainsKey(CurrencyType.Exp))
                    //    currencyAdded[CurrencyType.Exp] += jobReward.Exp(charBaseLv);
                    //else
                    //    currencyAdded[CurrencyType.Exp] = jobReward.Exp(charBaseLv);
                //}
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
                ii.itemId = (ushort)jobReward.itemRewardLst[i].itemId;
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
                var retVale = player.Slot.mInventory.AddItemsToInventory(item.itemId, item.stackCount, setnew, reason);
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

            var retValue = player.Slot.mInventory.AddItemsToInventory(itemOutput, setnew, reason);
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
            Dictionary<string, string> parameters, bool checkNotification, bool setnew, string reason, float expboost = 1.0f)
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
                                            currencyAdded, expboost);

            var retValue = player.Slot.mInventory.AddItemsToInventory(itemOutput, setnew, reason);
            if (retValue.retCode != InvReturnCode.AddSuccess)
            {
                //if cant add to bag, send mail
                List<IInventoryItem> items_Attachment = new List<IInventoryItem>();
                foreach (var item in itemOutput)
                    items_Attachment.Add(GenerateItem(item.itemId, null, item.stackCount));
                SendMailWithAttachment(player.Name, mailName, items_Attachment, currencyAdded, parameters);
            }
            else // Add items success, add currency
            {
                foreach (var currency in currencyAdded)
                    player.AddCurrency(currency.Key, currency.Value, reason);

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

            var retValue = player.Slot.mInventory.AddItemsToInventory(itemOutput, setnew, reason);
            if (retValue.retCode != InvReturnCode.AddSuccess)
            {
                // If cant add to bag, send mail
                List<IInventoryItem> items_Attachment = new List<IInventoryItem>();
                foreach (var item in itemOutput)
                    items_Attachment.Add(GenerateItem(item.itemId, null, item.stackCount));
                SendMailWithAttachment(playername, mailName, items_Attachment, currencyAdded, parameters);
            }
            else  // Add items success, add currency
            {
                foreach (var currency in currencyAdded)
                    player.AddCurrency(currency.Key, currency.Value * rewardMultiplier, reason);

                if (checkNotification)
                {
                    for (int index = 0; index < itemOutput.Count; ++index)
                        RareItemNotificationRules.CheckNotification(itemOutput[index].itemId, playername);
                }
            }
            return itemOutput;
        }

        public static void SendMailWithAttachment(string rcvName, string mailName, List<IInventoryItem> itemsToAdd, 
            Dictionary<CurrencyType, int> currencyToAdd, Dictionary<string, string> parameters = null)
        {
            MailObject mailObj = new MailObject();
            mailObj.rcvName = rcvName;
            mailObj.mailName = mailName;
            mailObj.lstAttachment = itemsToAdd;
            mailObj.dicCurrencyAmt = currencyToAdd;
            mailObj.dicBodyParam = parameters;
            MailManager.Instance.SendMail(mailObj);
        }

        public static void SendMail(string playerName, string mailName, Dictionary<string, string> parameters = null)
        {
            MailObject mailObj = new MailObject();
            mailObj.rcvName = playerName;
            mailObj.mailName = mailName;
            mailObj.dicBodyParam = parameters;
            MailManager.Instance.SendMail(mailObj);
        }

        public static IInventoryItem GenerateItem(int itemId, GameClientPeer peer, int stackcount = 1, bool superability = false)
        {
            IInventoryItem newItem = GameRepo.ItemFactory.GetInventoryItem(itemId);
            if (newItem == null)
                return null;

            switch (newItem.JsonObject.itemtype)
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
                    newItem.StackCount = stackcount;
                    break;
                default:
                    break;
            }

            //peer == null when first creating items for new players
            //if false means invalid item or player exceed daily limit or weekely limit
            if (peer != null && peer.CharacterData.ItemLimitData.DropItem(newItem) == false)
                return null;

            //if (peer != null && retval.GenerateUID)
            //    retval.UID = peer.mInventory.GenerateItemUID();
            return newItem;
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

            if (sej.persistentafterdeath)
            {
                SpecialSE sse = new SpecialSE(sej);
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
        public static CharacterData NewCharacterData(bool isTutorialDone, string charName, byte gender, int hairstyle, int haircolor, int makeup, int skincolor)
        {
            NewCharInfo newCharInfo = GameConstantRepo.mNewCharInfo;
            CharacterData newCharData = new CharacterData
            {
                IsTutorialRealmDone = isTutorialDone,
                ProgressLevel = 1,
                Name = charName,
                JobSect = (byte)JobType.Newbie,
                Gender = gender,
                Health = -1,
                Mana = -1,
                portraitID = 1,
                lastlevelid = newCharInfo.levelId,
                lastpos = newCharInfo.pos,
                lastdirection = newCharInfo.dir,
                BotSetting = "",
                CurrencyExchangeTime = 0,
            };
            newCharData.InitDefault(JobType.Newbie);
            SetCharacterAppearance(newCharData.EquipmentInventory, hairstyle, haircolor, makeup, skincolor);
            SetCharacterStartingInventoryItems(newCharData.ItemInventory);
            SetCharacterStartingEquipment(newCharData.EquipmentInventory);
            SetCharacterStartingSkill(newCharData.SkillInventory, newCharData.EquipmentInventory, gender);
            return newCharData;
        }

        private static void SetCharacterAppearance(EquipmentInventoryData equipmentInvData, int hairstyle, int haircolor, int makeup, int skincolor)
        {
            equipmentInvData.SetAppearanceToSlot((int)AppearanceSlot.HairStyle, hairstyle);
            equipmentInvData.SetAppearanceToSlot((int)AppearanceSlot.HairColor, haircolor);
            equipmentInvData.SetAppearanceToSlot((int)AppearanceSlot.MakeUp, makeup);
            equipmentInvData.SetAppearanceToSlot((int)AppearanceSlot.SkinColor, skincolor);
        }

        public static void SetCharacterStartingInventoryItems(ItemInventoryData itemInvData)
        {
            string newCharInvItemsStr = GameConstantRepo.GetConstant("NewChar_InventoryItems");
            if (string.IsNullOrEmpty(newCharInvItemsStr))
                return;

            List<IInventoryItem> invItems = itemInvData.Slots;
            string[] invItemsStrInfos = newCharInvItemsStr.Split('|');
            int length = invItemsStrInfos.Length;
            for (int i = 0; i < length; ++i)
            {
                string[] invItemsStrInfo = invItemsStrInfos[i].Split(';');
                if (invItemsStrInfo.Length == 2)
                {
                    int itemId, amt;
                    if (int.TryParse(invItemsStrInfo[0], out itemId) && itemId > 0)
                        if (int.TryParse(invItemsStrInfo[1], out amt) && amt > 0)
                        {
                            while (amt > 0)
                            {
                                int slotId = itemInvData.GetAvailableSlotByItemId((ushort)itemId);
                                if (slotId == -1)
                                {
                                    slotId = itemInvData.GetEmptySlotIndex();
                                    if (slotId == -1)
                                        break;
                                }

                                IInventoryItem invItem = invItems[slotId];
                                if (invItem != null)
                                {
                                    int avail = invItem.MaxStackCount - invItem.StackCount;
                                    int addAmt = (avail >= amt) ? amt : avail;
                                    invItem.StackCount += addAmt;
                                    amt -= addAmt;
                                }
                                else
                                {
                                    invItem = GenerateItem(itemId, null, amt);
                                    itemInvData.SetSlotItem(slotId, invItem);
                                    break;
                                }
                            }
                        }
                }
            }
        }

        public static void SetCharacterStartingEquipment(EquipmentInventoryData equipmentInvData)
        {
            string newCharEquipStr = GameConstantRepo.GetConstant("NewChar_Equipment");
            if (string.IsNullOrEmpty(newCharEquipStr))
                return;

            string[] equipStrInfos = newCharEquipStr.Split('|');
            int length = equipStrInfos.Length;
            for (int i = 0; i < length; ++i)
            {
                string equipStrInfo = equipStrInfos[i];
                int itemId;
                if (int.TryParse(equipStrInfo, out itemId) && itemId > 0)
                {
                    Equipment equipment = GenerateItem(itemId, null) as Equipment;
                    if (equipment == null)
                        continue;
                    equipmentInvData.SetEquipmentToSlot((int)EquipmentSlot.Weapon, equipment);
                }
            }
        }

        public static void SetCharacterStartingSkill(SkillInventoryData skillinv, EquipmentInventoryData equipInv, byte gender)
        {
            string genderStr = (gender == 0) ? "M" : "F";
            Equipment e = equipInv.GetEquipmentBySlotId((int)EquipmentSlot.Weapon);
            
            skillinv.AutoSkill[0] = SkillRepo.GetGenderWeaponBasicAttackData(e.EquipmentJson.partstype, 1, genderStr).skillJson.id;
            skillinv.EquippedSkill[0] = SkillRepo.GetGenderWeaponBasicAttackData(e.EquipmentJson.partstype, 1, genderStr).skillJson.id;
            skillinv.AutoSkill[1] = SkillRepo.GetSkill(51).skillJson.id;
            skillinv.EquippedSkill[1] = SkillRepo.GetSkill(51).skillJson.id;
            skillinv.SkillInv[0] = 55;
            skillinv.SkillInv[1] = 51;

        }

        #endregion
    }
}
