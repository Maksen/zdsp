using Kopio.JsonContracts;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Repository;
using Zealot.Server.Rules;

namespace Zealot.Server.Entities
{
    /// <summary>
    /// Tutorial Realm Controller
    /// </summary>
    public class RealmControllerTutorial : RealmController
    {
        public TutorialJson mTutorialInfo;
        protected Player mPlayer;

        public RealmControllerTutorial(RealmControllerJson info, GameLogic instance)
            : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            mTutorialInfo = (TutorialJson)mRealmInfo;
        }

        public override bool IsCorrectController()
        {
            return mRealmInfo.type == RealmType.Tutorial;
        }

        public override void OnPlayerEnter(Player player)
        {
            base.OnPlayerEnter(player);
            mPlayer = player;

            // Hardcoded equipments and skills
            // Equipment
            int itemId = GameConstantRepo.GetConstantInt("TutorialChar_Items", 1009);
            Equipment equipment = GameRules.GenerateItem(itemId, null) as Equipment;
            if (equipment != null)
                player.UpdateEquipmentStats((int)EquipmentSlot.Weapon, equipment);

            // Add skill to player
            mPlayer.SkillStats.EquippedSkill[2] = 128;
            mPlayer.SkillStats.AutoSkill[1] = 0;
        }

        public override void OnPlayerExit(Player player)
        {
            base.OnPlayerExit(player);

            // Remove skill
            mPlayer.SkillStats.EquippedSkill[2] = 0;
        }

        public override void OnMissionCompleted(bool success, bool broadcast)
        {
            if (mMissionCompleted)
                return;

            mMissionCompleted = true;
            mCountDownOnMissionCompleted = 1;
            if (timer != null)
                mInstance.StopTimer(timer);
            foreach (var entry in mInstance.maMonsterSpawners)
                entry.DestoryAll();

            GameClientPeer peer = mPlayer.Slot;
            if (peer != null)
            {
                peer.CharacterData.IsTutorialRealmDone = true;
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
