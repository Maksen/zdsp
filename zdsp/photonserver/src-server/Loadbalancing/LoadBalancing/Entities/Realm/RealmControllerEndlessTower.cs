using System;
using System.Collections.Generic;
using Zealot.Entities;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;
using Zealot.Server.Rules;

namespace Zealot.Server.Entities
{
    public class RealmControllerEndlessTower : RealmController
    {
        //public RealmEndlessTowerJson mRealmEndlessTowerInfo;
        public int mTotalMonsters = 0;
        public int mCurrentMonsters = 0;

        public RealmControllerEndlessTower(RealmControllerJson info, GameLogic instance) : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            //mRealmEndlessTowerInfo = (RealmEndlessTowerJson)mRealmInfo;
        }

        public override bool IsCorrectController()
        {
            //return mRealmInfo.type == RealmType.RealmEndlessTower;
            return false;
        }

        public override void InstanceStartUp()
        {
            base.InstanceStartUp();
            foreach (var entry in mInstance.maMonsterSpawners)
            {
                if (entry.mArchetype != null)
                    mTotalMonsters += entry.GetPopulation();
            }
            mCurrentMonsters = mTotalMonsters;
        }

        public override void OnPlayerEnter(Player player)
        {
            base.OnPlayerEnter(player);
            player.SecondaryStats.realmscore = mCurrentMonsters;     
        }

        public override void OnMonsterDead(Monster monster, Actor killer)
        {
            mCurrentMonsters--;
            foreach (Player player in mPlayers.Values)
                player.SecondaryStats.realmscore = mCurrentMonsters;
            if(mCurrentMonsters == 0)
                OnMissionCompleted(true, true);              
        }

        public override void OnMissionCompleted(bool success, bool broadcast)
        {
            if (mMissionCompleted)
                return;
            base.OnMissionCompleted(success, false);
            //int timespan = (int)(DateTime.Now - mTimerStart).TotalSeconds;
            //foreach (Player player in mPlayers.Values)
            //{
            //    player.RealmInventoryStats.TowerLevel++;
            //    player.Slot.CharacterData.RealmInventory.TowerDT = DateTime.Now;
            //    RewardListJson reward = RewardListRepo.GetInfoById(mRealmEndlessTowerInfo.firstrewardlist);
            //    string mailName = "Reward_EndlessTower";
            //    List<IInventoryItem> lstItemToAttach = new List<IInventoryItem>();
            //    if (reward != null)
            //        GameRules.GiveReward(player, reward, lstItemToAttach);
            //    reward = RewardListRepo.GetInfoById(mRealmEndlessTowerInfo.rewardlist);
            //    if (reward != null)
            //        GameRules.GiveReward(player, reward, lstItemToAttach);
            //    if (lstItemToAttach.Count > 0)
            //        GameRules.SendMailWithReward(player.Name, mailName, lstItemToAttach);
            //    player.Slot.ZRPC.CombatRPC.ShowScoreBoard(success, mCountDownOnMissionCompleted, timespan, 0, player.Slot);
            //}  
        }
    }
}
