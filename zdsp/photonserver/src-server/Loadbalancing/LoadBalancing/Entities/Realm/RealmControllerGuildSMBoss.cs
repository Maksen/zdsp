using System.Collections.Generic;
using Photon.LoadBalancing.GameServer;
using Kopio.JsonContracts;
using Zealot.Entities;
using Zealot.Server.Rules;
using Zealot.Common.Entities;
using Zealot.Common;
using Zealot.Repository;

namespace Zealot.Server.Entities
{
    public class RealmControllerGuildSMBoss : RealmControllerBossSlowMotion
    {
        //public ActivityGuildSMBossJson mActivityGuildSMBossInfo;
        private int mGuildId = 0;
        private GuildSMBossJson mGuildSMBossJson = null;
        private int mSMBossTotalDmgDone = 0;
        private int mSMBossDmgDone = 0;
        private string mKiller = "";
        private Dictionary<string, string> param = new Dictionary<string, string>();

        public RealmControllerGuildSMBoss(RealmControllerJson info, GameLogic instance)
            : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            //mActivityGuildSMBossInfo = (ActivityGuildSMBossJson)mRealmInfo;           
        }

        //public override bool IsCorrectController()
        //{
        //    return mRealmInfo.type == RealmType.ActivityGuildSMBoss;
        //}

        public override void OnLifeTimeUp(object arg = null)
        {
            timer = null;
            OnMissionCompleted(false, true);
        }

        public override void OnPlayerEnter(Player player)
        {
            base.OnPlayerEnter(player);
            SecondaryStats secStats = player.SecondaryStats;
            if (mGuildId == 0)
                mGuildId = secStats.guildId;

            if (mGuildId != 0)
            {
                GuildStatsServer guildStats = GuildRules.GetGuildById(mGuildId);
                if (guildStats != null)
                {
                    if (mGuildSMBossJson == null) // Init SM boss
                    {
                        mGuildSMBossJson = GuildRepo.GetGuildSMBossByLvl(guildStats.SMBossLevel);
                        mSMBossTotalDmgDone = guildStats.SMBossDmgDone;
                        mInstance.BroadcastEvent(this, "OnPlayerEnter");
                    }

                    guildStats.SMBossAttacker = player.Name;
                    mSMBossDmgDone = 0; // Reset current damage
                    // Participation deduct entry
                    if (secStats.GuildSMBossEntry > 0)
                        --secStats.GuildSMBossEntry;
                    else if (secStats.GuildSMBossExtraEntry > 0)
                        --secStats.GuildSMBossExtraEntry;
                }
            }
        }

        public override void OnPlayerExit(Player player)
        {
            base.OnPlayerExit(player);
            GuildStatsServer guildStats = GuildRules.GetGuildById(mGuildId);
            if (guildStats != null)
            {
                guildStats.SMBossAttacker = "";
                guildStats.SMBossRoomGuid = "";
                if (mSMBossTotalDmgDone >= mGuildSMBossJson.healthmax)
                {
                    int lvlLimit = (int)guildStats.GetGuildTechStats(GuildTechType.Love);
                    if (guildStats.SMBossLevel < lvlLimit && GuildRepo.GetGuildSMBossByLvl(guildStats.SMBossLevel+1) != null)
                    {
                        ++guildStats.SMBossLevel;
                        guildStats.SMBossDmgDone = 0;
                    }
                    else
                        guildStats.SMBossDmgDone = mSMBossTotalDmgDone;
                    guildStats.saveToDB = true;
                }
                else
                {
                    if (guildStats.SMBossDmgDone < mSMBossTotalDmgDone)
                    {
                        guildStats.SMBossDmgDone = mSMBossTotalDmgDone;
                        guildStats.saveToDB = true;
                    }
                }
            }

            // Participation reward
            param.Clear();
            param.Add("dmg", mSMBossDmgDone.ToString());
            GameRules.GiveReward_Mail(player.Name, "Reward_GuildSMBossEnter", new List<int>() { mGuildSMBossJson.enterrewardlist }, param);

            // QuestExtraRewards
            player.Slot.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.GuildSMBoss);
        }

        public override void OnDealtDamage(Player player, Actor defender, int damage)
        {
            if (mGuildSMBossJson != null)
            {
                int bossMaxHealth = mGuildSMBossJson.healthmax;
                if (mSMBossTotalDmgDone < bossMaxHealth)
                {
                    int newTotalDmg = mSMBossTotalDmgDone + damage;
                    if (newTotalDmg > bossMaxHealth)
                        damage -= (newTotalDmg - bossMaxHealth);
                    mSMBossTotalDmgDone += damage;
                    mSMBossDmgDone += damage;
                }
            }
        }

        public override void OnPlayerDead(Player player, Actor killer)
        {
            base.OnPlayerDead(player, killer);
            OnMissionCompleted(false, true);
        }

        public int GetSMBossLevel()
        {
            return mGuildSMBossJson.level;
        }

        public int GetSMBossDmgDone()
        {
            return mSMBossTotalDmgDone;
        }

        public override void OnMissionCompleted(bool success, bool broadcast)
        {
            if (mMissionCompleted)
                return;
            base.OnMissionCompleted(success, broadcast);

            foreach (Player player in mPlayers.Values)
                player.Slot.ZRPC.CombatRPC.ShowScoreBoard(success, mCountDownOnMissionCompleted, mSMBossDmgDone, 0, player.Slot);

            mInstance.mRoom.EmptyRoomLiveTime = 500;
            if (success)
            {
                //GuildRules.OnGuildRealmCompleted(mGuildId, mInstance.mRoom.Guid, mKiller);
                GuildStatsServer guildStats = GuildRules.GetGuildById(mGuildId);
                if (guildStats != null)
                {
                    guildStats.AddHistory(GuildHistoryType.LoveBossKill, mKiller); // saveToDB in function
                    Dictionary<string, string> _paramters = new Dictionary<string, string>();
                    _paramters.Add("killer", mKiller);
                    GuildRules.SendGuildMessage(guildStats.guildId, GUILocalizationRepo.GetLocalizedString("guild_History_LoveBossKill", _paramters));
                    Dictionary<string, GuildMemberStats>.KeyCollection memberStatsDictKeys = guildStats.GetMemberStatsDict().Keys;
                    foreach (string membername in memberStatsDictKeys)
                        GameRules.GiveReward_Mail(membername, "Reward_GuildSMBossKill", new List<int>() { mGuildSMBossJson.killrewardlist }, param);

                    int guildId = guildStats.guildId;
                    string message = string.Format("id:{0}|bosslvl:{1}|killer:{2}", guildId, mGuildSMBossJson.level, mKiller);
                    Zealot.Logging.Client.LogClasses.GuildBossKill guildBossKillLog = new Zealot.Logging.Client.LogClasses.GuildBossKill();
                    guildBossKillLog.message = message;
                    guildBossKillLog.guildid = guildId;
                    guildBossKillLog.bosslvl = mGuildSMBossJson.level;
                    guildBossKillLog.killer = mKiller;
                    var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(guildBossKillLog);
                }
            }
        }

        #region Triggers
        public void OnBossKilled(IServerEntity sender, object[] parameters = null)
        {
            Player killer = (Player)parameters[0];
            mKiller = killer.Name;          
            OnMissionCompleted(true, true);
        }
        #endregion
    }
}
