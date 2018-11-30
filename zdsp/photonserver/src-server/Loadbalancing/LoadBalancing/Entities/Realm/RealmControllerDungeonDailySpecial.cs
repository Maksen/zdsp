using System.Collections.Generic;
using System.Text;
using Photon.LoadBalancing.GameServer;
using Kopio.JsonContracts;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Common.Entities;
using Zealot.Common.Datablock;
using Zealot.Repository;
using Zealot.Server.Rules;
using System;

namespace Zealot.Server.Entities
{
    public class RealmControllerDungeonDailySpecial : RealmController
    {
        //public DungeonDailySpecialJson mDungeonDailySpecialInfo;

        public RealmControllerDungeonDailySpecial(RealmControllerJson info, GameLogic instance)
            : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            //mDungeonDailySpecialInfo = (DungeonDailySpecialJson)mRealmInfo;
        }

        public override bool IsCorrectController()
        {
            //return mRealmInfo.type == RealmType.DungeonDailySpecial;
            return false;
        }

        public override void OnLifeTimeUp(object arg = null)
        {
            timer = null;
            OnMissionCompleted(false, true);
        }

        public override void OnMissionCompleted(bool success, bool broadcast)
        {
            if (mMissionCompleted)
                return;
            base.OnMissionCompleted(success, true);

            if (!success)
            {
                //foreach (Player player in mPlayers.Values)
                //{
                    //Dictionary<int, RealmInfo> dDailySpecialDict = null;
                    //DungeonType type = mDungeonDailySpecialInfo.dungeontype;
                    //dDailySpecialDict = (type == DungeonType.Daily) ? player.RealmStats.GetDungeonDailyDict()
                    //                                                : player.RealmStats.GetDungeonSpecialDict();
                    //RealmInfo realmInfo = dDailySpecialDict[mDungeonDailySpecialInfo.sequence];
                    //int entry = realmInfo.DailyEntry + realmInfo.ExtraEntry;
                    //LogMissionComplete(player.Slot, mRealmInfo.id, mRealmInfo.reqlvl, entry, entry, false, 0);
                //}
                return;
            }

            bool isEventOn = false;
            int rewardMultiplier = 1, extraRewardItemID = 0, extraRewardPercent = 0, extraRewardStackCount = 0;
            var configs = GMActivityConfig.GetConfigIntList(GMActivityType.Dungeon, DateTime.Now);//get list of activity that is currently ON
            if (configs.Count > 0)
            {
                foreach (var config in configs)
                {
                    int seq = config.mDataList[0];
                    int realmTypeID = config.mDataList[1];
                    int dunTypeID = config.mDataList[6];
                    //if (seq == mDungeonDailySpecialInfo.sequence && realmTypeID == (int)mDungeonDailySpecialInfo.type && dunTypeID == (int)mDungeonDailySpecialInfo.dungeontype)
                    //{
                    //    rewardMultiplier = config.mDataList[2];
                    //    extraRewardItemID = config.mDataList[3];
                    //    extraRewardPercent = config.mDataList[4];
                    //    extraRewardStackCount = config.mDataList[5];
                    //    isEventOn = true;
                    //    break;
                    //}
                }
            }
            // Compute players info
            StringBuilder sb = new StringBuilder();
            foreach (Player player in mPlayers.Values)
            {
                Dictionary<int, RealmInfo> dDailySpecialDict = null;
                CollectionHandler<object> dDailySpecialList = null;
                //DungeonType type = mDungeonDailySpecialInfo.dungeontype;
                //dDailySpecialDict = (type == DungeonType.Daily) ? player.RealmStats.GetDungeonDailyDict() 
                //                                                : player.RealmStats.GetDungeonSpecialDict();
                //dDailySpecialList = (type == DungeonType.Daily) ? player.RealmStats.DungeonDaily 
                //                                                : player.RealmStats.DungeonSpecial;

                /*RealmInfo realmInfo = dDailySpecialDict[mDungeonDailySpecialInfo.sequence];
                bool deductEntry = false;
                int entryBefore = realmInfo.DailyEntry + realmInfo.ExtraEntry;
                if (realmInfo.DailyEntry > 0)
                {
                    --realmInfo.DailyEntry;
                    deductEntry = true;
                }
                else if (realmInfo.ExtraEntry > 0)
                {
                    --realmInfo.ExtraEntry;
                    deductEntry = true;
                }
                int entryAfter = realmInfo.DailyEntry + realmInfo.ExtraEntry;
                string iteminfostr = "False";
                if (deductEntry)
                {
                    dDailySpecialList[realmInfo.LocalObjIdx] = realmInfo.ToString();

                    List<ItemInfo> itemList;
                    if (isEventOn)
                    {
                        itemList = GameRules.GiveRewardGrp_CheckBagSlotThenMail_WithAdditionalItems(player, new List<int>() { mDungeonDailySpecialInfo.rewardgrp }, "Reward_DungeonDailySpecial",
                          null, true, false, string.Format("RealmDaily Event id={0}", mDungeonDailySpecialInfo.id),
                          rewardMultiplier, extraRewardItemID, extraRewardPercent, extraRewardStackCount);
                    }
                    else
                    {
                        itemList = GameRules.GiveReward_CheckBagSlotThenMail(player, new List<int>() { mDungeonDailySpecialInfo.rewardgrp }, "Reward_DungeonDailySpecial",
                          null, true, false, string.Format("RealmDaily id={0}", mDungeonDailySpecialInfo.id));
                    }
                    iteminfostr = GameUtils.SerializeItemInfoList(itemList);
                }

                if (type == DungeonType.Daily)
                {
                    player.Slot.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.DailyDungeon);
                }
               
                sb.AppendFormat("{0}#{1}#{2}#{3}|", player.Name, player.PlayerSynStats.PortraitID, player.PlayerSynStats.vipLvl, iteminfostr);

                LogMissionComplete(player.Slot, mRealmInfo.id, mRealmInfo.reqlvl, entryBefore, entryAfter, true, mDungeonDailySpecialInfo.rewardgrp);*/
            }

            string playersInfo = sb.ToString().TrimEnd('|');
            foreach (Player player in mPlayers.Values)
            {
                player.Slot.ZRPC.CombatRPC.ShowRewardDialog(playersInfo, "", rewardMultiplier, player.Slot);
            }
        }

        private static void LogMissionComplete(GameClientPeer peer, int realmId, int reqLvl, int entryBefore, int entryAfter,
                                    bool isSuccess, int rewardGrp)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("realmId:{0}|", realmId);
            sb.AppendFormat("reqLvl:{0}|", reqLvl);
            sb.AppendFormat("entryBefore:{0}|", entryBefore);
            sb.AppendFormat("entryAfter:{0}|", entryAfter);
            sb.AppendFormat("isSuccess:{0}|", isSuccess);
            sb.AppendFormat("rewardGrp:{0}", rewardGrp);

            Zealot.Logging.Client.LogClasses.DungeonComplete sysLog = new Zealot.Logging.Client.LogClasses.DungeonComplete();
            sysLog.userId = peer.mUserId;
            sysLog.charId = peer.GetCharId();
            sysLog.message = sb.ToString();
            sysLog.realmId = realmId;
            sysLog.difficulty = 0;
            sysLog.reqLvl = reqLvl;
            sysLog.entryBefore = entryBefore;
            sysLog.entryAfter = entryAfter;
            sysLog.isSuccess = isSuccess;
            sysLog.rewardGrp = rewardGrp;
            sysLog.stars = "";
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(sysLog);
        }

        #region Triggers
        /*public void StartCountDown(IServerEntity sender, object[] parameters = null)
        {
            foreach (Player player in mPlayers.Values)
            {
                player.Slot.ZRPC.CombatRPC.StartCountDown(3, player.Slot);                
            }
        }

        public void NotifyNextRound(IServerEntity sender, object[] parameters = null)
        {
            mCurrentRound++;
            //if (mCurrentRound > mRealmDailySoloInfo.totlaround)
            //{
            //    OnMissionCompleted(true, true);
            //    return;
            //}
            foreach (Player player in mPlayers.Values)
            {
                player.Slot.ZRPC.CombatRPC.NotifyRealmNextRound(player.Slot);
                player.SecondaryStats.realmscore = mCurrentRound;
            }            
        }*/
        #endregion
    }
}
