using Kopio.JsonContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using System.Linq;
using Photon.LoadBalancing.GameServer;
using Zealot.Logging.Client.LogClasses;
using ExitGames.Logging;

namespace Zealot.Server.Rules
{
    public enum DonateAction
    {
        Request,
        Response,
        GetDonate,

        //server
        SendDonateMail,
    }

    public static class DonateRules
    {
        private static Dictionary<int, GuildStatsServer> GuildList = GuildRules.GuildList;
        public static bool IsResetToDay = false;
        private static readonly ILogger DeBug = LogManager.GetCurrentClassLogger();

        public static void SendGuildDonateDataToClient(string lastUpdateTimeStr, GameClientPeer peer)
        {
            //Open DonateUI exec this function
            //First open UI will be send NewData, Because client default time is 0001/01/01
            if (CheckPlayerGuild(peer) == false)
                return;

            string peerName = peer.mPlayer.Name;
            int guildId = peer.mPlayer.SecondaryStats.guildId;

            DonateDataInv donateDataInv = GuildList[guildId].memberDonateInv;     //Server catch data            

            DateTime clientLastTime = JsonConvert.DeserializeObject<DateTime>(lastUpdateTimeStr);
            if (clientLastTime == donateDataInv.lastUpdateTime)
            {
                //Do not updateData
                peer.ZRPC.CombatRPC.SendDonateReturnCode(DonateReturnCode.DataNotDirty, peer);
                return;
            }

            string sendDataStr = GetToClientDataStr(peer);

            peer.ZRPC.CombatRPC.SendDonateData(sendDataStr, DonateReturnCode.UpdateData, peer);
        }

        public static void RequestGuildDonate(int heroCardId, GameClientPeer peer)
        {
            if (CheckPlayerGuild(peer) == false)
                return;
            string peerName = peer.mPlayer.Name;
            int guildId = peer.mPlayer.SecondaryStats.guildId;

            DonateDataInv donateDataInv = GuildList[guildId].memberDonateInv;     //Server catch data

            if (CheckCanRequest(peer) == false)
            {
                CreateServerDeBugLog(DonateAction.Request, "CheckCanRequest() function is false");
                return;
            }

            if (donateDataInv.donateRequestDic.ContainsKey(peerName))
            {
                CreateServerDeBugLog(DonateAction.Request, "This player is in the request");
                return;
            }

            if (donateDataInv.donateRemainingTimes.ContainsKey(peerName) == false)
            {
                //Init player requestCount and responseCount
                DonateRemainingTimesInv remainingTimesInv = new DonateRemainingTimesInv();
                remainingTimesInv.RequestCount = GameConfig.Donate_DailyRequestCount;
                remainingTimesInv.ResponseCount = GameConfig.Donate_DailyResponseCount;
                donateDataInv.donateRemainingTimes.Add(peerName, remainingTimesInv);
            }

            if (donateDataInv.donateRemainingTimes[peerName].RequestCount < 1)
            {
                CreateServerDeBugLog(DonateAction.Request, "This player can request count < 1");
                return;
            }

            //check player have hero
            //List<NewHeroData> list = peer.CharacterData.NewHeroInvData.ownedList;
            //NewHeroData heroCardData = null;
            //foreach (NewHeroData hero in list)
            //{
            //    if (hero.herocardid == heroCardId)
            //    {
            //        heroCardData = hero;
            //        break;
            //    }
            //}
            //if (heroCardData == null)
            //{
            //    CreateServerDeBugLog(DonateAction.Request, "This player don't have this hero");
            //    return;
            //}

            //HeroCardJson herocard = HeroRepo.GetHeroCardByID(heroCardData.herocardid);
            //if (herocard == null)
            //{
            //    CreateServerDeBugLog(DonateAction.Request, "This heroCardData herocardid error");
            //    return;
            //}
            //if (herocard.herotype == Zealot.Common.HeroType.White)
            //{
            //    CreateServerDeBugLog(DonateAction.Request, "Herotype is White");
            //    return;
            //}


            //Start add request
            donateDataInv.lastUpdateTime = DateTime.Now;

            int deductRequestCount = 1;
            donateDataInv.donateRemainingTimes[peerName].RequestCount -= deductRequestCount;

            DonateInventory donateInventory = new DonateInventory();
            donateInventory.PortraitId = peer.mPlayer.PlayerSynStats.PortraitID;
            donateInventory.VipLvl = peer.mPlayer.PlayerSynStats.vipLvl;
            donateInventory.HeroId = heroCardId;
            donateInventory.DonateStatusMin = 0;
            donateInventory.DonateStatusMax = GameConfig.Donate_EachRequestCount;
            donateInventory.RequestEnd = false;
            donateDataInv.donateRequestDic.Add(peerName, donateInventory);

            //Log
            int requestCount = donateDataInv.donateRemainingTimes[peerName].RequestCount;
            CreateRequestDonateLog(heroCardId, deductRequestCount, requestCount, peer);

            //Save DB
            SaveDB(guildId);

            //Send new data to client
            string sendDataStr = GetToClientDataStr(peer);

            peer.ZRPC.CombatRPC.SendDonateData(sendDataStr, DonateReturnCode.RequestSuccess, peer);

            peer.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.GuildWishingPoolBroadcast);
        }

        public static void ResponseGuildDonate(string requestPlayerName, int itemSlotId, int count, GameClientPeer peer)
        {
            if (CheckPlayerGuild(peer) == false)
            {
                CreateServerDeBugLog(DonateAction.Response, "This player guild error");
                return;
            }

            string peerName = peer.mPlayer.Name;
            int guildId = peer.mPlayer.SecondaryStats.guildId;

            if (requestPlayerName.Equals(peerName) || string.IsNullOrEmpty(requestPlayerName) || itemSlotId < 0 || count < 1)
            {
                CreateServerDeBugLog(DonateAction.Response, "Incoming parameter error");
                return;
            }

            DonateDataInv donateDataInv = GuildList[guildId].memberDonateInv;
            Dictionary<string, DonateInventory> donateDic = donateDataInv.donateRequestDic;

            if (donateDic.ContainsKey(requestPlayerName) == false)
            {
                string dataStr = GetToClientDataStr(peer);
                peer.ZRPC.CombatRPC.SendDonateData(dataStr, DonateReturnCode.ResponseUpdateData, peer);
                return;
            }

            DonateInventory requestInv = donateDic[requestPlayerName];
            if (requestInv.RequestEnd)
            {
                string dataStr = GetToClientDataStr(peer);
                peer.ZRPC.CombatRPC.SendDonateData(dataStr, DonateReturnCode.ResponseUpdateData, peer);
                return;
            }

            int canResponseCount = requestInv.DonateStatusMax - requestInv.DonateStatusMin;
            if (count > canResponseCount)
                count = canResponseCount;

            IInventoryItem item = peer.mInventory.mInvData.Slots[itemSlotId];
            if (item == null)
            {
                CreateServerDeBugLog(DonateAction.Response, "ItemSlotId error");
                return;
            }
            if (item.StackCount < count)
            {
                CreateServerDeBugLog(DonateAction.Response, "Item count is not enough");
                return;
            }

            //HeroCardJson hero = HeroRepo.GetHeroCardByID(requestInv.HeroId);
            //if (item.ItemID != hero.heroitemid)
            //{
            //    CreateServerDeBugLog(DonateAction.Response, "Response is not the same as request HeroId");
            //    return;
            //}

            //if (donateDataInv.donateRemainingTimes.ContainsKey(peerName) == false)
            //{
            //    //Init player requestCount and responseCount
            //    DonateRemainingTimesInv remainingTimesInv = new DonateRemainingTimesInv();
            //    remainingTimesInv.RequestCount = GameConfig.Donate_DailyRequestCount;
            //    remainingTimesInv.ResponseCount = GameConfig.Donate_DailyResponseCount;
            //    donateDataInv.donateRemainingTimes.Add(peerName, remainingTimesInv);
            //}

            //if (donateDataInv.donateRemainingTimes[peerName].ResponseCount < count)
            //{
            //    CreateServerDeBugLog(DonateAction.Response, "This player canResponseCount < ResponseCount");
            //    return;
            //}

            //int rewardListId = DonateRewardRepo.GetRewardListIdByHeroQuality(hero.heroquality);

            ////get reward
            //Dictionary<CurrencyType, int> currencyAdded = new Dictionary<CurrencyType, int>();
            //List<IInventoryItem> items = GetRewardItemList(rewardListId, count, currencyAdded, peer);

            //if (CheckBackpackSpaces(items.Count, peer) == false)
            //{
            //    peer.ZRPC.CombatRPC.SendDonateReturnCode(DonateReturnCode.NotEnoughSpace, peer);
            //    return;
            //}

            ////Start Response

            ////1.Deduct Item
            //if (DeductItem(itemSlotId, count, peer) == false)
            //{
            //    CreateServerDeBugLog(DonateAction.Response, "DeductItem error");
            //    return;
            //}

            ////2.Send Reward
            //SendRewardToPlayer(items, currencyAdded, peer, string.Format("Donate RewardId:{0}", rewardListId)); //send guildContribution and item

            ////3.update data
            //donateDataInv.donateRemainingTimes[peerName].ResponseCount -= count;
            //donateDataInv.lastUpdateTime = DateTime.Now;

            //requestInv.DonateStatusMin += count;
            //requestInv.DonateCanGetCount += count;

            //if (requestInv.DonateStatusMin == requestInv.DonateStatusMax)
            //    requestInv.RequestEnd = true;

            ////Save DB
            //SaveDB(guildId);

            ////Log
            //var responseCount = donateDataInv.donateRemainingTimes[peerName].ResponseCount;
            //int getGuildContribution = 0;
            //if (currencyAdded.ContainsKey(CurrencyType.GuildContribution))
            //    getGuildContribution = currencyAdded[CurrencyType.GuildContribution];
            //CreateResponseDonateLog(item.ItemID, count, responseCount, getGuildContribution, peer);

            ////Update Donate NewDot
            //GameClientPeer reqPeer = GameApplication.Instance.GetCharPeer(requestPlayerName);
            //if (reqPeer != null)
            //    if (reqPeer.mPlayer != null)
            //        if (reqPeer.mPlayer.SecondaryStats != null)
            //            reqPeer.mPlayer.SecondaryStats.guildDonateDot = true;

            ////Send newdata to client
            //string sendDataStr = GetToClientDataStr(peer);

            //peer.ZRPC.CombatRPC.SendDonateData(sendDataStr, DonateReturnCode.ResponseSuccess, peer);

            //peer.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.GuildWishingPoolDonate);
        }

        public static void GetDonate(GameClientPeer peer)
        {
            if (CheckPlayerGuild(peer) == false)
            {
                CreateServerDeBugLog(DonateAction.GetDonate, "This player guild error");
                return;
            }

            string peerName = peer.mPlayer.Name;
            int guildId = peer.mPlayer.SecondaryStats.guildId;

            DonateDataInv donateDataInv = GuildList[guildId].memberDonateInv;     //Server catch data
            var donateDataInvDic = donateDataInv.donateRequestDic;

            if (donateDataInvDic.ContainsKey(peerName) == false)
            {
                CreateServerDeBugLog(DonateAction.GetDonate, "This player in not request");
                return;
            }

            int canGetCount = donateDataInvDic[peerName].DonateCanGetCount;
            if (canGetCount < 1)
            {
                CreateServerDeBugLog(DonateAction.GetDonate, "CanGetDonate is zero");
                return;
            }

            if (CheckBackpackSpaces(1, peer) == false)
            {
                peer.ZRPC.CombatRPC.SendDonateReturnCode(DonateReturnCode.NotEnoughSpace, peer);
                return;
            }

            //int heroId = donateDataInvDic[peerName].HeroId;
            //HeroCardJson heroInfo = HeroRepo.GetHeroCardByID(heroId);
            //if (heroInfo == null)
            //    return;
            //IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(heroInfo.heroitemid);

            ////Start GetDonate
            ////send item
            //InvRetval retval = peer.mPlayer.Slot.mInventory.AddItemsIntoInventory(item.ItemID, canGetCount, true, "Donate");
            //if (retval.retCode != InvReturnCode.AddSuccess)
            //{
            //    CreateServerDeBugLog(DonateAction.GetDonate, "AddItems is fail");
            //    return;
            //}

            donateDataInv.lastUpdateTime = DateTime.Now;

            //Log
            //CreateGetDonateLog(item.ItemID, canGetCount, peer);

            //Remove this data
            bool isMaxCount = donateDataInvDic[peerName].DonateStatusMin >= donateDataInvDic[peerName].DonateStatusMax;
            if (donateDataInvDic[peerName].RequestEnd == true || isMaxCount == true)
                donateDataInvDic.Remove(peerName);
            else
                donateDataInvDic[peerName].DonateCanGetCount = 0;

            SaveDB(guildId);

            //Update Donate NewDot
            peer.mPlayer.SecondaryStats.guildDonateDot = false;

            //Send newdata to client
            string sendDataStr = GetToClientDataStr(peer);

            peer.ZRPC.CombatRPC.SendDonateData(sendDataStr, DonateReturnCode.GetDonateSuccess, peer);
        }

        private static void ResetDonate()
        {
            foreach (var guildData in GuildList.Values)
            {
                DonateDataInv data = guildData.memberDonateInv;
                var donateDic = data.donateRequestDic;

                if (data.donateRemainingTimes.Count < 1 && donateDic.Count < 1)
                    continue;

                //Remove DonateCanGetCount < 1 data and Set RequestEnd
                foreach (var kvp in donateDic.ToList())
                {
                    if (donateDic[kvp.Key].DonateCanGetCount < 1)
                        donateDic.Remove(kvp.Key);
                    else
                        donateDic[kvp.Key].RequestEnd = true;
                }

                data.donateRemainingTimes.Clear();
                data.lastUpdateTime = DateTime.Now;

                guildData.saveToDB = true;
                guildData.mGuildDataDirty = true;
            }
        }

        public static void LeaveGuildSendDonateRewardMail(string playerName, int guildId)
        {
            //確認幫會是否存在
            if (GuildList.ContainsKey(guildId) == false)
            {
                CreateServerDeBugLog(DonateAction.SendDonateMail, "GuildId error");
                return;
            }

            //確認幫會內無此會員
            GuildStatsServer guildStats = GuildList[guildId];
            Dictionary<string, GuildMemberStats> mStatsDict = guildStats.GetMemberStatsDict();
            if (mStatsDict.ContainsKey(playerName) == true)
            {
                CreateServerDeBugLog(DonateAction.SendDonateMail, "This player do not leave guild");
                return;
            }

            DonateDataInv donateInv = guildStats.memberDonateInv;

            //確認請求清單上有該會員
            Dictionary<string, DonateInventory> donateRequestDic = donateInv.donateRequestDic;
            if (donateRequestDic.ContainsKey(playerName) == false)
                return;

            int canGetCount = donateRequestDic[playerName].DonateCanGetCount;
            if (canGetCount > 0)
            {
                //取得物品
                int heroId = donateRequestDic[playerName].HeroId;
                //HeroCardJson heroInfo = HeroRepo.GetHeroCardByID(heroId);
                //if (heroInfo == null)
                //{
                //    CreateServerDeBugLog(DonateAction.SendDonateMail, "HeroId is error");
                //    return;
                //}

                //IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(heroInfo.heroitemid);
                //item.StackCount = (ushort)canGetCount;
                //List<IInventoryItem> items = new List<IInventoryItem>();
                //items.Add(item);

                ////發送獎勵(寄信)
                //GameRules.SendMailWithItem(playerName, "Reward_DonateLeaveGuild", items);
            }

            //清除該清單
            donateRequestDic.Remove(playerName);
            donateInv.lastUpdateTime = DateTime.Now;
            if (donateInv.donateRemainingTimes.ContainsKey(playerName))
                donateInv.donateRemainingTimes.Remove(playerName);

            SaveDB(guildId);
        }

        public static void CheckResetDonateTime()
        {
            if (DateTime.Now.Hour != GameConfig.Donate_ResetTimeHour)
                return;
            if (IsResetToDay)
                return;
            ResetDonate();
            IsResetToDay = true;
        }

        public static bool CheckDonateNewDot(string name, int guildId)
        {
            if (GuildList.ContainsKey(guildId) == false)
                return false;
            if (GuildList[guildId].GetMemberStatsDict().ContainsKey(name) == false)
                return false;

            var donateDataInvDic = GuildList[guildId].memberDonateInv.donateRequestDic;
            if (donateDataInvDic.ContainsKey(name))
                if (donateDataInvDic[name].DonateCanGetCount > 0)
                    return true;

            return false;
        }

        #region Common private function

        #region Check function        
        private static bool CheckPlayerGuild(GameClientPeer peer)
        {
            if (peer == null)
                return false;

            string peerName = peer.mPlayer.Name;
            int guildId = peer.mPlayer.SecondaryStats.guildId;
            if (guildId < 1 || GuildList.ContainsKey(guildId) == false)
            {
                //GuildId Error or Non't Find Guild
                return false;
            }

            GuildStatsServer guildStats = GuildList[guildId];
            Dictionary<string, GuildMemberStats> mStatsDict = guildStats.GetMemberStatsDict();
            if (mStatsDict.ContainsKey(peerName) == false)
            {
                //Guild No this member
                return false;
            }
            return true;
        }

        private static bool CheckBackpackSpaces(int itemCount, GameClientPeer peer)
        {
            //Check the size of the backpack
            int backpackSpaces = peer.mInventory.mInvData.GetEmptySlotCount();
            if (backpackSpaces < itemCount)
                return false;
            return true;
        }

        private static bool CheckCanRequest(GameClientPeer peer)
        {
            string peerName = peer.mPlayer.Name;
            int guildId = peer.mPlayer.SecondaryStats.guildId;
            var donateDataInv = GuildList[guildId].memberDonateInv;

            //Requests count is not enough
            if (donateDataInv.donateRemainingTimes.ContainsKey(peerName) == true)
                if (donateDataInv.donateRemainingTimes[peerName].RequestCount < 1)
                    return false;

            //In the request
            if (donateDataInv.donateRequestDic.ContainsKey(peerName) == true)
                return false;

            //Check Join time
            if (GuildList[guildId].donateRequestCDTime.ContainsKey(peerName))
            {
                if (DateTime.Now < GuildList[guildId].donateRequestCDTime[peerName])
                    return false;
                else
                    GuildList[guildId].donateRequestCDTime.Remove(peerName);
            }

            return true;
        }
        #endregion

        private static List<IInventoryItem> GetRewardItemList(int rewardListId, int count, Dictionary<CurrencyType, int> currencyAdded, GameClientPeer peer)
        {
            //Get reward from rewardList
            List<ItemInfo> itemsToAttach = new List<ItemInfo>();
            RewardListJson reward = RewardListRepo.GetRewardById(rewardListId);
            for (int i = 0; i < count; i++)
            {
                GameRules.GenerateReward(rewardListId, itemsToAttach, currencyAdded);
            }

            //generate item list
            var res = new List<IInventoryItem>();
            foreach (var item in itemsToAttach)
                res.Add(GameRules.GenerateItem(item.itemId, peer, item.stackCount));

            return res;
        }

        private static void SendRewardToPlayer(List<IInventoryItem> itemsToAttach, Dictionary<CurrencyType, int> currencyAdded, GameClientPeer peer, string reason)
        {
            peer.mPlayer.Slot.mInventory.AddItemsIntoInventory(itemsToAttach, true, reason);

            //add currency for player
            foreach (var currency in currencyAdded)
                peer.mPlayer.AddCurrency(currency.Key, currency.Value, reason);
        }

        private static bool DeductItem(int slotId, int count, GameClientPeer peer)
        {
            IInventoryItem item = peer.mInventory.mInvData.Slots[slotId];
            if (item == null || count < 1)
                return false;
            InvRetval retval = peer.mInventory.DeductItem(item.ItemID, (ushort)count, "Donate");
            return retval.retCode == InvReturnCode.UseSuccess;
        }

        private static void SaveDB(int guildId)
        {
            GuildList[guildId].saveToDB = true;
            GuildList[guildId].mGuildDataDirty = true;
        }

        private static string GetToClientDataStr(GameClientPeer peer)
        {
            string peerName = peer.mPlayer.Name;
            int guildId = peer.mPlayer.SecondaryStats.guildId;

            DonateDataInv donateDataInv = GuildList[guildId].memberDonateInv;
            DonateToClientData sendData = new DonateToClientData();

            if (donateDataInv.donateRemainingTimes.ContainsKey(peerName))
            {
                //Load player RequestCount and ResponseCount record
                sendData.DonateRemainingTimes = donateDataInv.donateRemainingTimes[peerName];
            }
            else
            {
                //Player no Request or Response record
                //Init player RequestCount and ResponseCount, but no saveDB
                sendData.DonateRemainingTimes.RequestCount = GameConfig.Donate_DailyRequestCount;
                sendData.DonateRemainingTimes.ResponseCount = GameConfig.Donate_DailyResponseCount;
            }
            sendData.DonateRequestDic = donateDataInv.donateRequestDic;                 //Set DataList
            sendData.LastUpdateTime = donateDataInv.lastUpdateTime;                     //Set UpdateTime
            sendData.CanRequest = CheckCanRequest(peer);                                //Set CanRequest
            return SerializeSendData(sendData);
        }

        private static string SerializeSendData(DonateToClientData sendData, bool saveDefaultValue = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, NullValueHandling = NullValueHandling.Ignore };
            if (saveDefaultValue == false)
                jsonSetting.DefaultValueHandling = DefaultValueHandling.Ignore;
            return JsonConvert.SerializeObject(sendData, jsonSetting);
        }

        #region Log        
        private static void CreateRequestDonateLog(int heroId, int deductRequestCount, int requestCount, GameClientPeer peer)
        {
            //RequestDonateLog log = new RequestDonateLog();

            //log.userId = peer.mUserId;
            //log.charId = peer.GetCharId();
            //log.action = DonateAction.Request.ToString();
            //var heroInfo = HeroRepo.GetHeroCardByID(heroId);
            //log.itemId = heroInfo.heroitemid;
            ////log.requestItemCount = GameConfig.Donate_DailyRequestCount;
            //log.deductRequestCount = deductRequestCount;
            //log.requestCount = requestCount;
            //var ignoreAwait = Logging.Client.LoggingAgent.Instance.LogAsync(log);

            var userId = peer.mUserId;
            var charId = peer.GetCharId();
            var action = DonateAction.Request.ToString();
            RequestDonateItemLog itemLog = new RequestDonateItemLog();
            itemLog.userId = userId;
            itemLog.charId = charId;
            itemLog.action = action;
            //var heroInfo = HeroRepo.GetHeroCardByID(heroId);
            //itemLog.itemId = heroInfo.heroitemid;
            var ignoreAwait = Logging.Client.LoggingAgent.Instance.LogAsync(itemLog);

            RequestDonateCountLog countLog = new RequestDonateCountLog();
            countLog.userId = userId;
            countLog.charId = charId;
            countLog.action = action;
            countLog.deductRequestCount = deductRequestCount;
            countLog.requestCount = requestCount;
            var countLogIgnoreAwait = Logging.Client.LoggingAgent.Instance.LogAsync(countLog);
        }

        private static void CreateResponseDonateLog(int itemId, int deductItemCount, int responseCount, int getGuildContribution, GameClientPeer peer)
        {
            //ResponseDonateLog log = new ResponseDonateLog();

            //log.userId = peer.mUserId;
            //log.charId = peer.GetCharId();
            //log.action = DonateAction.Response.ToString();
            //log.itemId = itemId;
            //log.deductItemCount = deductItemCount;
            //IInventoryItem item = peer.CharacterData.ItemInventory.GetItemByItemId((ushort)itemId);
            //if (item == null)
            //    log.remainingItemCount = 0;
            //else
            //    log.remainingItemCount = item.StackCount;
            //log.getGuildContribution = getGuildContribution;
            //log.GuildContributionCount = peer.mPlayer.SecondaryStats.contribute;
            //log.responseCount = responseCount;
            //var ignoreAwait = Logging.Client.LoggingAgent.Instance.LogAsync(log);

            ResponseDonateItemLog log = new ResponseDonateItemLog();

            log.userId = peer.mUserId;
            log.charId = peer.GetCharId();
            log.action = DonateAction.Response.ToString();
            log.itemId = itemId;                        
            log.responseCount = responseCount;
            var ignoreAwait = Logging.Client.LoggingAgent.Instance.LogAsync(log);
        }

        private static void CreateGetDonateLog(int itemId, int getDonateCount, GameClientPeer peer)
        {
            //GetDonateLog log = new GetDonateLog();

            //log.userId = peer.mUserId;
            //log.charId = peer.GetCharId();
            //log.action = DonateAction.GetDonate.ToString();
            //log.itemId = itemId;
            //log.getDonateCount = getDonateCount;
            //IInventoryItem item = peer.CharacterData.ItemInventory.GetItemByItemId((ushort)itemId);
            //if (item == null)
            //    log.itemCount = 0;
            //else
            //    log.itemCount = item.StackCount;
            //var ignoreAwait = Logging.Client.LoggingAgent.Instance.LogAsync(log);
        }

        private static void CreateServerDeBugLog(DonateAction action, string log)
        {
            DeBug.InfoFormat("DonateError! Action : {0}, Log : {1} ", action.ToString(), log);
        }
        #endregion

        #endregion

        #region Console command
        public static void ConsoleResetDonate()
        {
            ResetDonate();
        }

        public static void ConsoleResetDonateRemainingCount()
        {
            foreach (var guildData in GuildList.Values)
            {
                DonateDataInv data = guildData.memberDonateInv;
                if (data.donateRemainingTimes.Count < 1)
                    continue;

                data.donateRemainingTimes.Clear();
                data.lastUpdateTime = DateTime.Now;
                guildData.saveToDB = true;
                guildData.mGuildDataDirty = true;
            }
        }
        #endregion
    }
}

#region GameConfig
namespace Photon.LoadBalancing.GameServer
{
    public static partial class GameConfig
    {
        public static int Donate_CanAddRequestTimeHour = 0;
        public static int Donate_DailyRequestCount = 1;
        public static int Donate_DailyResponseCount = 3;
        public static int Donate_EachRequestCount = 3;
        public static int Donate_ResetTimeHour = 5;        //0~23
    }
}
#endregion