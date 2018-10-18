using Zealot.Entities;
using Photon.LoadBalancing.GameServer;
using Kopio.JsonContracts;
using Zealot.Repository;
using Zealot.Server.Rules;
using Zealot.Common;

namespace Zealot.Server.Entities
{
    /// <summary>
    /// this is the first training realm controller. 
    /// </summary>
    public class RealmControllerTutorial : RealmController
    {
        //public RealmTutorialJson mRealmTutorialInfo;
        protected Player mPlayer;
        public RealmControllerTutorial(RealmControllerJson info, GameLogic instance)
            : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            //mRealmInfo = RealmRepo.TutorialRealmJson;
            //mRealmTutorialInfo = (RealmTutorialJson)mRealmInfo; 

            

        }

        public override bool IsCorrectController()
        {
            return true;
        }

        public override void OnPlayerEnter(Player player)
        {
            base.OnPlayerEnter(player);
            mPlayer = player;

            //JobsectJson jobsectJson = JobSectRepo.GetJobById(player.Slot.CharacterData.JobSect);
            //LevelJson lvlJson = LevelRepo.GetInfoById(jobsectJson.level); 
            //player.Slot.SetDefaultLevelBeforeEnterRealm(lvlJson.unityscene, Vector3.zero); 

            // Add skill to player
            mPlayer.SkillStats.SkillGroupIndex[70] = mPlayer.SkillStats.SkillInvCount;
            mPlayer.SkillStats.SkillInv[mPlayer.SkillStats.SkillInvCount++] = 70;
            mPlayer.SkillStats.SkillInv[mPlayer.SkillStats.SkillInvCount++] = 128;
        }

        public override void OnMissionCompleted(bool success, bool broadcast)
        {
            mCountDownOnMissionCompleted = 1;
            if (timer != null)
                mInstance.StopTimer(timer);
            foreach (var entry in mInstance.maMonsterSpawners)
                entry.DestoryAll();

            GameClientPeer peer = mPlayer.Slot;
            if (peer != null)
            {
                peer.CharacterData.TrainingRealmDone = true;

                // remove skill
                mPlayer.SkillStats.SkillGroupIndex[70] = 0;
                mPlayer.SkillStats.SkillInv[--mPlayer.SkillStats.SkillInvCount] = 0;
                mPlayer.SkillStats.SkillInv[--mPlayer.SkillStats.SkillInvCount] = 0;

                /*********************   Faction reward    ***************************/

                if (peer.CharacterData.GetRecommendedFactionReward)
                {
                    string res = GameConstantRepo.GetConstant("RecommendedFactionRewardItemID");
                    int factionItemID;
                    if (string.IsNullOrEmpty(res) || int.TryParse(res, out factionItemID) == false)
                    {
                        factionItemID = 1045;
                    }
                    //else factionItemID = the parse id

                    IInventoryItem factionReward = GameRules.GenerateItem(factionItemID, peer);
                    if (factionReward != null)
                        peer.mInventory.AddItemsToInventory(factionReward, true, "NewCharFaction");
                }

                //var jobItem = JobSectRepo.GetJobByType((JobType)mPlayer.PlayerSynStats.jobsect).itemid;
                //IInventoryItem jobReward = GameRules.GenerateItem(jobItem, peer);
                //if (jobReward != null)
                //    peer.mInventory.AddItemsIntoInventory(jobReward, true, "NewChar");
                peer.mCanSaveDB = true;
            }
            RealmEnd();
        }

        #region Triggers
        public override void CompleteRealm(IServerEntity sender, object[] parameters = null)
        {
            MonsterSpawner sp = sender as MonsterSpawner; 
            base.CompleteRealm(sender, parameters);
        }
        #endregion
    }
}
