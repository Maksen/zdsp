using System.Collections.Generic;
using Photon.LoadBalancing.GameServer;
using Kopio.JsonContracts;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Common.Entities;
using Zealot.Common.Datablock;
using Zealot.Repository;
using Zealot.Server.Rules;
using System.Text;
using System;

namespace Zealot.Server.Entities
{
    public class RealmControllerDungeonStory : RealmControllerBossSlowMotion
    {
        public DungeonStoryJson mDungeonStoryInfo;
        public DungeonObjectiveStats mDungeonObjectiveStats;
        private long mElapsedTime = 0;
        private int mPlayerDeathCount;
        private bool playerNoEntryLeft = false;

        public RealmControllerDungeonStory(RealmControllerJson info, GameLogic instance)
            : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            mDungeonStoryInfo = (DungeonStoryJson)mRealmInfo;
            mCountDownOnMissionCompleted = 30;
        }

        public override bool IsCorrectController()
        {
            return mRealmInfo.type == RealmType.DungeonStory;
        }

        public override bool CanReconnect()
        {
            return mRealmState != RealmState.Ended && !mMissionCompleted;
        }

        public override void InstanceStartUp()
        {
            base.InstanceStartUp();
            mDungeonObjectiveStats = new DungeonObjectiveStats();
        }

        public override void OnMonsterDead(Monster monster, Actor killer)
        {
            base.OnMonsterDead(monster, killer);
            if (!playerNoEntryLeft)
                mDungeonObjectiveStats.OnObjectiveMonsterKill(monster.mArchetype.id);
        }

        public override void OnPlayerDead(Player player, Actor killer)
        {
            base.OnPlayerDead(player, killer);
            if (!playerNoEntryLeft)
                ++mPlayerDeathCount;
        }

        public override void OnPlayerEnter(Player player)
        {
            base.OnPlayerEnter(player);

            Dictionary<int, DungeonStoryInfo> dStoryDict = player.RealmStats.GetDungeonStoryDict();
            DungeonStoryInfo dStoryInfo = dStoryDict[mDungeonStoryInfo.sequence];
            playerNoEntryLeft = (dStoryInfo.DailyEntry + dStoryInfo.ExtraEntry <= 0);
            if (!playerNoEntryLeft)
            {
                // Add player to recieve objective info
                player.Slot.ZRPC.LocalObjectRPC.AddLocalObject((byte)LOCATEGORY.SharedStats, -1, mDungeonObjectiveStats, player.Slot);

                //mDungeonObjectiveStats.OnObjectiveHeroUseOnly(player.SkillStats);
                //mDungeonObjectiveStats.OnObjectiveHeroCannotUse(player.SkillStats);
                //mDungeonObjectiveStats.OnObjectiveHeroTypeCannotUse(player.SkillStats);
                //mDungeonObjectiveStats.OnObjectiveSkillCannotUseAny(player.SkillStats);
                //mDungeonObjectiveStats.OnObjectiveHeroQualityUseOnly(player.SkillStats);
            }
        }

        public override void OnLifeTimeUp(object arg = null)
        {
            timer = null;
            OnMissionCompleted(false, true);
        }

        public override void Update(long dt)
        {        
            if (mRealmState == RealmState.Started && !mMissionCompleted)
            {
                mElapsedTime += dt;
                if (!playerNoEntryLeft)
                    mDungeonObjectiveStats.OnObjectiveTimeNoDeath(mElapsedTime, mPlayerDeathCount);
            }
            if (!playerNoEntryLeft)
            {
                if (mDungeonObjectiveStats.IsDirty())
                {
                    Dictionary<byte, object> packet = mDungeonObjectiveStats.Serialize((byte)LOCATEGORY.SharedStats, -1, false);
                    var eventData = GameRules.GetLocalObjEventData(packet);
                    var sendPara = GameRules.GetSendParam(true);
                    foreach (Player player in mPlayers.Values)
                        player.Slot.SendEvent(eventData, sendPara);
                    mDungeonObjectiveStats.Reset();
                }
            }
        } 

        public override bool OnAllPeerRemoved(bool removeDueDc)
        {
            if (removeDueDc)
                return false;
            return true;
        }

        public override void OnMissionCompleted(bool success, bool broadcast)
        {
            if (mMissionCompleted)
                return;
            base.OnMissionCompleted(success, true);

            // Check Star Objectives
            mDungeonObjectiveStats.OnObjectiveTimeComplete(mElapsedTime, success);
            mDungeonObjectiveStats.OnObjectiveRealmComplete(success);

            if (!success)
            {
                foreach (Player player in mPlayers.Values)
                {
                    RealmStats realmStats = player.RealmStats;
                    Dictionary<int, DungeonStoryInfo> dStoryDict = realmStats.GetDungeonStoryDict();
                    DungeonStoryInfo dStoryInfo = dStoryDict[mDungeonStoryInfo.sequence];
                    int entry = dStoryInfo.DailyEntry + dStoryInfo.ExtraEntry;
                    LogMissionComplete(player.Slot, mRealmInfo.id, (int)mDungeonStoryInfo.difficulty, mRealmInfo.reqlvl, entry, entry, false, 0, "");
                }                  
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
                    if (seq == mDungeonStoryInfo.sequence && realmTypeID == (int)mDungeonStoryInfo.type)
                    {
                        rewardMultiplier = config.mDataList[2];
                        extraRewardItemID = config.mDataList[3];
                        extraRewardPercent = config.mDataList[4];
                        extraRewardStackCount = config.mDataList[5];
                        isEventOn = true;
                        break;
                    }
                }
            }

            foreach (Player player in mPlayers.Values)
            {
                RealmStats realmStats = player.RealmStats;
                Dictionary<int, DungeonStoryInfo> dStoryDict = realmStats.GetDungeonStoryDict();
                DungeonStoryInfo dStoryInfo = dStoryDict[mDungeonStoryInfo.sequence];
                bool deductEntry = false;
                int entryBefore = dStoryInfo.DailyEntry + dStoryInfo.ExtraEntry;
                if (dStoryInfo.DailyEntry > 0)
                {
                    --dStoryInfo.DailyEntry;
                    deductEntry = true;
                }
                else if (dStoryInfo.ExtraEntry > 0)
                {
                    --dStoryInfo.ExtraEntry;
                    deductEntry = true;
                }
                int entryAfter = dStoryInfo.DailyEntry + dStoryInfo.ExtraEntry;
                string iteminfostr = "False";
                if (deductEntry)
                {
                    realmStats.DungeonStory[dStoryInfo.LocalObjIdx] = dStoryInfo.ToString();

                    List<ItemInfo> itemList;
                    if (isEventOn)
                    {
                        itemList = GameRules.GiveRewardGrp_CheckBagSlotThenMail_WithAdditionalItems(player, new List<int>() { mDungeonStoryInfo.rewardgrp },
                                        "Reward_DungeonStory", null, true, false, string.Format("RealmStory id={0}", mDungeonStoryInfo.id),
                                        rewardMultiplier, extraRewardItemID, extraRewardPercent, extraRewardStackCount);
                    }
                    else
                    {
                        itemList = GameRules.GiveRewardGrp_CheckBagSlotThenMail(player, new List<int>() { mDungeonStoryInfo.rewardgrp },
                                        "Reward_DungeonStory", null, true, false, string.Format("RealmStory id={0}", mDungeonStoryInfo.id));
                    }

                    iteminfostr = GameUtils.SerializeItemInfoList(itemList);
                }
                // Update star objectives
                string stars = mDungeonObjectiveStats.UpdatePlayerStarObjectivesProgress(realmStats, mDungeonStoryInfo, deductEntry);
                         
                // Show reward dialog
                player.Slot.ZRPC.CombatRPC.ShowRewardDialog(stars, iteminfostr, rewardMultiplier,player.Slot);

                player.Slot.mSevenDaysController.UpdateTask(mDungeonStoryInfo.difficulty);
                int totalStars = realmStats.GetTotalStarsCompleted();
                if (totalStars > 0)
                {
                    player.Slot.mSevenDaysController.UpdateTask(NewServerActivityType.ChapterStars, totalStars);
                }

                player.Slot.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.StoryDungeon);

                LogMissionComplete(player.Slot, mRealmInfo.id, (int)mDungeonStoryInfo.difficulty, mRealmInfo.reqlvl, entryBefore, entryAfter, true, mDungeonStoryInfo.rewardgrp, stars);
            }
        }

        private static void LogMissionComplete(GameClientPeer peer, int realmId, int difficulty, int reqLvl, int entryBefore, int entryAfter,
                                            bool isSuccess, int rewardGrp, string stars)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("realmId:{0}|", realmId);
            sb.AppendFormat("difficulty:{0}|", difficulty);
            sb.AppendFormat("reqLvl:{0}|", reqLvl);
            sb.AppendFormat("entryBefore:{0}|", entryBefore);
            sb.AppendFormat("entryAfter:{0}|", entryAfter);
            sb.AppendFormat("isSuccess:{0}|", isSuccess);
            sb.AppendFormat("rewardGrp:{0}|", rewardGrp);
            sb.AppendFormat("stars:{0}", stars);

            Zealot.Logging.Client.LogClasses.DungeonComplete sysLog = new Zealot.Logging.Client.LogClasses.DungeonComplete();
            sysLog.userId = peer.mUserId;
            sysLog.charId = peer.GetCharId();
            sysLog.message = sb.ToString();
            sysLog.realmId = realmId;
            sysLog.difficulty = difficulty;
            sysLog.reqLvl = reqLvl;
            sysLog.entryBefore = entryBefore;
            sysLog.entryAfter = entryAfter;
            sysLog.isSuccess = isSuccess;
            sysLog.rewardGrp = rewardGrp;
            sysLog.stars = stars;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(sysLog);
        }
    }
}
