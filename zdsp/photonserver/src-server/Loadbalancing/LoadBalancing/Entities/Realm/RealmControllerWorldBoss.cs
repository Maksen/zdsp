using Kopio.JsonContracts;
using Newtonsoft.Json;
using Photon.LoadBalancing.GameServer;
using Photon.LoadBalancing.GameServer.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Zealot.Common;
using Zealot.Common.Datablock;
using Zealot.Common.Entities;
using Zealot.Entities;
using Zealot.Logging.Client.LogClasses;
using Zealot.Repository;
using Zealot.Server.Rules;

namespace Photon.LoadBalancing.GameServer
{
    public static partial class GameConfig
    {
        public static int WorldBossRecordCount = 50;
        public static int UnlockWorldBossLevel = 26;
    }
}

namespace Zealot.Server.Entities
{
    struct SimplePlayer
    {
        public string name;
        public string userId;
        public string charId;
        public bool rewardFlag;
    }
    class RealmControllerWorldBoss : RealmControllerBossSlowMotion
    {
        //public ActivityWorldBossJson mActivityWorldBossInfo;
        public RealmPartyDamageList mPartyDamageList;
        private Dictionary<string, int> mNameToIndex;
        private Dictionary<string, int> mPlayerDamageMap;
        private int mLowestDamageInRank = 0;
        private int mLowestIndexInRank = -1;
        private long mUpdateTimer = 5000; //every 5 seconds check mActivityCountryStats dirty.
        private int mRankTotalCount = 5;
        private int mRankCurrentCount = 0;
        private Dictionary<string, int> mNameToRank = new Dictionary<string, int>();
        private bool mIsEnd = false;
        private Dictionary<string, SimplePlayer> mSimplePlayerMap;
        private static Timer aTimer;
        private Dictionary<string, string> mail_parameters = new Dictionary<string, string>();
        //private bool mSuccess = false;

        public RealmControllerWorldBoss(RealmControllerJson info, GameLogic instance)
            : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            //mActivityWorldBossInfo = (ActivityWorldBossJson)mRealmInfo;
            mNameToIndex = new Dictionary<string, int>();
            mPlayerDamageMap = new Dictionary<string, int>();
            mSimplePlayerMap = new Dictionary<string, SimplePlayer>();

        }

        //public override bool IsCorrectController()
        //{
        //    return mRealmInfo.type == RealmType.ActivityWorldBoss;
        //}

        public override void InstanceStartUp()
        {
            base.InstanceStartUp();
            mPartyDamageList = new RealmPartyDamageList();
            //CombatNPCJson boss_archetype = NPCRepo.GetArchetypeById(mActivityWorldBossInfo.boss);
            //mPartyDamageList.health = boss_archetype.healthmax;
            mRankTotalCount = mPartyDamageList.names.Count;
        }

        public override bool CanReconnect()
        {
            return mRealmState != RealmState.Ended && !mMissionCompleted;
        }

        public override void RealmStart()
        {
            base.RealmStart();
            //mInstance.BroadcastEvent(this, "SetArchetype", new object[] { mActivityWorldBossInfo.boss });
            mInstance.BroadcastEvent(this, "OnRealmStart");
        }

        public override void Update(long dt)
        {
            mUpdateTimer -= dt;
            if (mUpdateTimer <= 0)
            {
                mUpdateTimer = 5000;
                if (mPartyDamageList.IsDirty())
                {
                    Dictionary<byte, object> packet = mPartyDamageList.Serialize((byte)LOCATEGORY.SharedStats, -1, false);
                    var eventData = GameRules.GetLocalObjEventData(packet);
                    var sendPara = GameRules.GetSendParam(true);
                    foreach (Player player in mPlayers.Values)
                        player.Slot.SendEvent(eventData, sendPara);
                    mPartyDamageList.Reset();
                }
            }
        }

        public override void OnPlayerEnter(Player player)
        {
            base.OnPlayerEnter(player);
            string playername = player.Name;
            if (!mPlayerDamageMap.ContainsKey(playername))
                mPlayerDamageMap.Add(playername, 0);
            else
                player.SecondaryStats.realmscore = mPlayerDamageMap[playername];
            player.Slot.ZRPC.LocalObjectRPC.AddLocalObject((byte)LOCATEGORY.SharedStats, -1, mPartyDamageList, player.Slot);

            //RealmInfo dWorldBossInfo = player.RealmStats.GetWorldBossDict()[0];
            //if (dWorldBossInfo.DailyEntry > 0)
            //    --dWorldBossInfo.DailyEntry;
            //else if (dWorldBossInfo.ExtraEntry > 0)
            //    --dWorldBossInfo.ExtraEntry;
            //player.RealmStats.WorldBoss[0] = dWorldBossInfo.ToString();

            if (!mSimplePlayerMap.ContainsKey(playername))
            {
                SimplePlayer simplePlayer = new SimplePlayer();
                simplePlayer.name = playername;
                simplePlayer.userId = player.Slot.mUserId;
                simplePlayer.charId = player.Slot.GetCharId();
                simplePlayer.rewardFlag = false;
                mSimplePlayerMap.Add(playername, simplePlayer);
            }
        }

        public override void OnPlayerExit(Player player)
        {
            base.OnPlayerExit(player);

            if (mIsEnd)
            {
                //give reward when realm end
                ClaimReward(player);
            }

            string playername = player.Name;
            //WorldBossLog worldBossLog = new WorldBossLog();
            //worldBossLog.userId = player.Slot.mUserId;
            //worldBossLog.charId = player.Slot.GetCharId();
            //worldBossLog.type = "WorldBoss";
            //worldBossLog.damage = mPlayerDamageMap[playername];
            //var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(worldBossLog);

            player.Slot.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.WorldBossTimes);
        }

        public override void OnDealtDamage(Player player, Actor defender, int damage)
        {
            string playername = player.Name;
            mPlayerDamageMap[playername] += damage;
            int new_damage = mPlayerDamageMap[playername];
            player.SecondaryStats.realmscore = new_damage;
            mPartyDamageList.health = defender.GetHealth()- damage;
                if (mNameToIndex.ContainsKey(playername))
            {
                int index = mNameToIndex[playername];
                mPartyDamageList.UpdateDamage(index, new_damage);
                if (mLowestIndexInRank == index)
                    UpdateLowestInRank();
            }
            else
            {
                if (mRankCurrentCount < mRankTotalCount)
                {
                    int index = mRankCurrentCount;
                    mPartyDamageList.names[index] = playername;
                    mPartyDamageList.damages[index] = new_damage;
                    mNameToIndex.Add(playername, index);
                    mRankCurrentCount++;
                    if (mNameToIndex.Count == mRankTotalCount)
                        UpdateLowestInRank();
                }
                else if (new_damage > mLowestDamageInRank)
                {
                    mNameToIndex.Remove((string)mPartyDamageList.names[mLowestIndexInRank]);
                    mPartyDamageList.names[mLowestIndexInRank] = playername;
                    mPartyDamageList.damages[mLowestIndexInRank] = new_damage;
                    mNameToIndex.Add(playername, mLowestIndexInRank);
                    UpdateLowestInRank();
                }
            }
        }

        private void UpdateLowestInRank()
        {
            for (int dmg_index = 0; dmg_index < mRankTotalCount; dmg_index++)
            {
                int dmg = (int)mPartyDamageList.damages[dmg_index];
                if (dmg_index == 0 || dmg < mLowestDamageInRank)
                {
                    mLowestIndexInRank = dmg_index;
                    mLowestDamageInRank = dmg;
                }
            }
        }

        private void BuildFinalRank()
        {
            mNameToRank = new Dictionary<string, int>();
            int rank = 1;
            foreach (KeyValuePair<string, int> kvp in mPlayerDamageMap.OrderByDescending(kvp => kvp.Value))
            {
                mNameToRank.Add(kvp.Key, rank);
                rank++;
            }
        }

        public override void ClaimReward(Player player)
        {
            string playername = player.Name;
            if (mPlayerReward.ContainsKey(playername))
            {
                mPlayerReward.Remove(playername);

                SimplePlayer simplePlayer = mSimplePlayerMap[playername];

                RankReward(simplePlayer);
                DamageReward(simplePlayer);

                simplePlayer.rewardFlag = true;
                mSimplePlayerMap[playername] = simplePlayer;
            }
        }

        private void RankReward(SimplePlayer player)
        {
            int outOfRank = 100;
            string playername = player.name;
            int rank = outOfRank;

            //int rewardid = mActivityWorldBossInfo.reward9;
            if (mNameToRank.ContainsKey(playername))
            {
                int playerDamage = mPlayerDamageMap[playername];
                if (playerDamage != 0)
                    rank = mNameToRank[playername];

                //if (rank == 1)
                //    rewardid = mActivityWorldBossInfo.reward1;
                //else if (rank == 2)
                //    rewardid = mActivityWorldBossInfo.reward2;
                //else if (rank == 3)
                //    rewardid = mActivityWorldBossInfo.reward3;
                //else if (rank == 4)
                //    rewardid = mActivityWorldBossInfo.reward4;
                //else if (rank == 5)
                //    rewardid = mActivityWorldBossInfo.reward5;
                //else if (rank <= 10)
                //    rewardid = mActivityWorldBossInfo.reward6;
                //else if (rank <= 20)
                //    rewardid = mActivityWorldBossInfo.reward7;
                //else if (rank <= 50)
                //    rewardid = mActivityWorldBossInfo.reward8;
                //else
                //    rewardid = mActivityWorldBossInfo.reward9;
            }
            else
            {
                //data error!!
                return;
            }
            List<int> rewardIdList = new List<int>();
            //rewardIdList.Add(rewardid);

            WorldBossRewardLog worldBossRewardLog = new WorldBossRewardLog();
            worldBossRewardLog.userId = player.userId;
            worldBossRewardLog.charId = player.charId;
            worldBossRewardLog.type = "WorldBossRank";
            worldBossRewardLog.value = rank;
            worldBossRewardLog.mail = 1;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(worldBossRewardLog);

            if (rank <= 50)
            {
                mail_parameters.Clear();
                mail_parameters.Add("rank", rank.ToString());
                //GameRules.GiveReward_Mail(playername, "Reward_WorldBoss", rewardIdList, mail_parameters);
            }
            else
            {
                //GameRules.GiveReward_Mail(playername, "Reward_WorldBossJoin", rewardIdList, null);
            }
        }

        private void DamageReward(SimplePlayer player)
        {
            string playername = player.name;

            List<int> rewardIdList = new List<int>();

            if (!mPlayerDamageMap.ContainsKey(playername))
            {
                //data error!!
                return;
            }
            int playerDamage = mPlayerDamageMap[playername];

            //if (playerDamage > mActivityWorldBossInfo.score1)
            //    rewardIdList.Add(mActivityWorldBossInfo.scorereward1);
            //if (playerDamage > mActivityWorldBossInfo.score2)
            //    rewardIdList.Add(mActivityWorldBossInfo.scorereward2);
            //if (playerDamage > mActivityWorldBossInfo.score3)
            //    rewardIdList.Add(mActivityWorldBossInfo.scorereward3);
            //if (playerDamage > mActivityWorldBossInfo.score4)
            //    rewardIdList.Add(mActivityWorldBossInfo.scorereward4);
            //if (playerDamage > mActivityWorldBossInfo.score5)
            //    rewardIdList.Add(mActivityWorldBossInfo.scorereward5);
            //if (playerDamage > mActivityWorldBossInfo.score6)
            //    rewardIdList.Add(mActivityWorldBossInfo.scorereward6);
            //if (playerDamage > mActivityWorldBossInfo.score7)
            //    rewardIdList.Add(mActivityWorldBossInfo.scorereward7);
            //if (playerDamage > mActivityWorldBossInfo.score8)
            //    rewardIdList.Add(mActivityWorldBossInfo.scorereward8);
            //if (playerDamage > mActivityWorldBossInfo.score9)
            //    rewardIdList.Add(mActivityWorldBossInfo.scorereward9);
            //if (playerDamage > mActivityWorldBossInfo.score10)
            //    rewardIdList.Add(mActivityWorldBossInfo.scorereward10);

            WorldBossRewardLog worldBossRewardLog = new WorldBossRewardLog();
            worldBossRewardLog.userId = player.userId;
            worldBossRewardLog.charId = player.charId;
            worldBossRewardLog.type = "WorldBossDamage";
            worldBossRewardLog.value = playerDamage;
            worldBossRewardLog.mail = 0;

            if (rewardIdList.Count > 0)
            {
                mail_parameters.Clear();
                mail_parameters.Add("dmg", playerDamage.ToString());
                //GameRules.GiveReward_Mail(playername, "Reward_WorldBossDamage", rewardIdList, mail_parameters);
                worldBossRewardLog.mail = 1;
            }
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(worldBossRewardLog);
        }

        public override bool OnAllPeerRemoved(bool removeDueDc)
        {
            return mRealmState == RealmState.Ended || mMissionCompleted;
        }

        public override void OnMissionCompleted(bool success, bool broadcast)
        {
            if (mMissionCompleted)
                return;
            //mSuccess = success;
            mIsEnd = true;
            Update(5000);
            base.OnMissionCompleted(success, false);
            GameUtils.mActivityStatus = GameUtils.UnsetBit(GameUtils.mActivityStatus, (int)ActivityStatusBitIndex.WorldBoss);
            GameApplication.BroadcastActivityEnd(ActivityStatusBitIndex.WorldBoss);

            BuildFinalRank();
            foreach (Player player in mPlayers.Values)
            {
                string playername = player.Name;
                mPlayerReward.Add(playername, null);
                var rank = mNameToRank[playername];
                int playerDamage = mPlayerDamageMap[playername];
                if (rank <= 50 && playerDamage == 0)
                    rank = 51;

                player.Slot.ZRPC.CombatRPC.ShowScoreBoard(success, mCountDownOnMissionCompleted, rank, mBossPId, player.Slot);
            }

            int recordCount = GameConfig.WorldBossRecordCount;
            if (mNameToRank.Count < recordCount)
                recordCount = mNameToRank.Count;

            if (recordCount > 0)
            {
                KeyValuePair<string, int>[] ladderData = new KeyValuePair<string, int>[recordCount];
                foreach (KeyValuePair<string, int> kvp in mNameToRank)
                {
                    if (kvp.Value <= recordCount)
                    {
                        ladderData[kvp.Value - 1] = new KeyValuePair<string, int>(kvp.Key, mPlayerDamageMap[kvp.Key]);
                    }
                }
                GameApplication.Instance.Leaderboard.SetWorldBossRecordData(ladderData);
            }

            mInstance.mRoom.EmptyRoomLiveTime = 500;

            aTimer = new Timer(60000 * 7);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = false;
            aTimer.Enabled = true;
        }

        public override void OnLifeTimeUp(object arg = null)
        {
            timer = null;
            OnMissionCompleted(false, false);
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            foreach (KeyValuePair<string, SimplePlayer> kvp in mSimplePlayerMap)
            {
                SimplePlayer player = kvp.Value;
                if (player.rewardFlag == false)
                {
                    string playername = player.name;
                    RankReward(player);
                    DamageReward(player);

                    player.rewardFlag = true;
                }
            }
        }
    }
}
