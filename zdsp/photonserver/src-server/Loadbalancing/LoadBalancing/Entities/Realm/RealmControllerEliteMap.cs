using System;
using System.Collections.Generic;
using Zealot.Entities;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;
using Kopio.JsonContracts;

namespace Zealot.Server.Entities
{
    public class RealmControllerEliteMap : RealmController
    {
        private class PlayerTimer
        {
            public DateTime mEnterDT;
            public GameTimer mTimer;
        }

        //public EliteMapJson mEliteMapInfo;
        private Dictionary<string, PlayerTimer> mPlayerTimers;

        public RealmControllerEliteMap(RealmControllerJson info, GameLogic instance)
            : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            //mEliteMapInfo = (EliteMapJson)mRealmInfo;
            mPlayerTimers = new Dictionary<string, PlayerTimer>();
        }

        //public override bool IsCorrectController()
        //{
        //    return mRealmInfo.type == RealmType.EliteMap;
        //}

        public override void OnPlayerEnter(Player player)
        {
            string playername = player.Name;
            mPlayers.Add(playername, player);

            PlayerTimer playerTimer = new PlayerTimer();
            playerTimer.mEnterDT = DateTime.Now;
            //int timeLeft = player.RealmStats.GetEliteMapDailyTimeLeft(player.GetAccumulatedLevel());
            //if (timeLeft <= 0)
            //    timeLeft = 1;
            //playerTimer.mTimer = mInstance.SetTimer((long)timeLeft * 1000, OnPlayerTimeUp, playername);
            mPlayerTimers.Add(playername, playerTimer);

            //player.Slot.ZRPC.CombatRPC.EnterRealm(mRealmInfo.id, (byte)mRealmState, timeLeft, player.Slot);
            player.Slot.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.TimeCityFight);

            AddLog(player.Slot, "Enter", mRealmInfo.id, mRealmInfo.excelname);
        }

        public override void OnPlayerExit(Player player)
        {
            base.OnPlayerExit(player);
            string playername = player.Name;
            PlayerTimer playerTimer = mPlayerTimers[playername];
            if (playerTimer.mTimer != null)
            {
                TimeSpan timespan = DateTime.Now - playerTimer.mEnterDT;
                //player.RealmStats.EliteMapTime += (int)timespan.TotalSeconds;
                //int total = player.RealmStats.GetEliteMapDailyTimeTotal(player.GetAccumulatedLevel());
                //if (player.RealmStats.EliteMapTime >= total - 5) //if less than 5 seconds left, just consume all.
                //    player.RealmStats.EliteMapTime = total;
                mInstance.StopTimer(playerTimer.mTimer);
                playerTimer.mTimer = null;
            }
            mPlayerTimers.Remove(playername);
            AddLog(player.Slot, "Exit", mRealmInfo.id, mRealmInfo.excelname);
        }

        public override bool OnAllPeerRemoved(bool removeDueDc)
        {
            return false;
        }

        public void OnPlayerTimeUp(object arg)
        {
            string playername = (string)arg;
            if (mPlayers.ContainsKey(playername))
            {
                Player player = mPlayers[playername];
                PlayerTimer playerTimer = mPlayerTimers[playername];            
                TimeSpan timespan = DateTime.Now - playerTimer.mEnterDT;
                //player.RealmStats.EliteMapTime += (int)timespan.TotalSeconds;
                //int total = player.RealmStats.GetEliteMapDailyTimeTotal(player.GetAccumulatedLevel());
                //if (player.RealmStats.EliteMapTime > total - 5)
                //    player.RealmStats.EliteMapTime = total;
                mPlayerTimers[playername].mTimer = null;
                mPlayers[playername].Slot.LeaveRealm();
            }          
        }

        private void AddLog(GameClientPeer peer, string action, int realmId, string realmName)
        {
            string message = string.Format("action: {0} | realmId: {1} | realmName: {2}",
                action,
                realmId,
                realmName);

            Zealot.Logging.Client.LogClasses.EliteMap sysLog = new Zealot.Logging.Client.LogClasses.EliteMap();
            sysLog.userId = peer.mUserId;
            sysLog.charId = peer.GetCharId();
            sysLog.message = message;
            sysLog.action = action;
            sysLog.realmId = realmId;
            sysLog.realmName = realmName;

            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(sysLog);
        }
    }
}
