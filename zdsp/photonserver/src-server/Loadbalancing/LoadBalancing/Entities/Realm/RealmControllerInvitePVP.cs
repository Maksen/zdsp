using Zealot.Entities;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;
using Zealot.Server.Rules;
using System.Collections.Generic;

namespace Zealot.Server.Entities
{
    class RealmControllerInvitePVP : RealmControllerBossSlowMotion
    {
        private RealmControllerInvitePVPJson mUnityData;
        private string firstName = "";
        private GameTimer mDelaytimer;
        public Dictionary<string, int> mPlayersHpOnEnter;

        public RealmControllerInvitePVP(RealmControllerJson info, GameLogic instance)
            : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            mUnityData = (RealmControllerInvitePVPJson)mPropertyInfos;
            mCountDownOnMissionCompleted = 10;
            mPlayersHpOnEnter = new Dictionary<string, int>();
        }

        //public override bool IsCorrectController()
        //{
        //    return mRealmInfo.type == RealmType.InvitePVP;
        //}

        public override bool CanReconnect()
        {
            return false;
        }

        public override void SetSpawnPos(Player player)
        {
            if (firstName == "" || player.Name == firstName)
            {
                player.Position = mUnityData.spawnPos[0];
                firstName = player.Name;
            }
            else
                player.Position = mUnityData.spawnPos[1];
        }

        public override void OnPlayerEnter(Player player)
        {
            base.OnPlayerEnter(player);
            mPlayersHpOnEnter[player.Name] = player.GetHealth();
            player.SetHealth(player.GetHealthMax());
            mInstance.mEntitySystem.AddAlwaysShow(player);
            if (mPlayers.Count == 2)
            {
                mDelaytimer = mInstance.SetTimer(3200, (arg) =>
                {
                    mDelaytimer = null;
                    mInstance.BroadcastEvent(this, "OnRealmStart"); //trigger gates to open.
                }, null);
            }
        }

        public override void OnPlayerDead(Player player, Actor killer)
        {
            base.OnPlayerDead(player, killer);
            OnMissionCompleted(false, true);
        }

        public override void OnPlayerExit(Player player)
        {
            base.OnPlayerExit(player);
            mInstance.mEntitySystem.RemoveAlwaysShow(player);
            string playername = player.Name;
            if (mPlayersHpOnEnter.ContainsKey(playername))
                player.SetHealth(mPlayersHpOnEnter[playername]);
            else
                player.SetHealth(player.GetHealthMax());
            RealmRules.RemoveInvitePCPData(player.Name);
            OnMissionCompleted(false, true);
        }

        public override void RealmEnd()
        {
            base.RealmEnd();
            if (mDelaytimer != null)
            {
                mInstance.StopTimer(mDelaytimer);
                mDelaytimer = null;
            }
        }
    }
}
