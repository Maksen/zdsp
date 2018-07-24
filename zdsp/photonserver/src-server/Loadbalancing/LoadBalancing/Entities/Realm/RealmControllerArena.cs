using System;
using Photon.LoadBalancing.GameServer;
using Zealot.Entities;
using Zealot.Server.Rules;
using Zealot.Common.Entities;
using Zealot.Server.AI;
using Zealot.Common;
using Kopio.JsonContracts;
using Zealot.Repository;
using Photon.LoadBalancing.Entities;

namespace Zealot.Server.Entities
{
    public class RealmControllerArena : RealmController
    {
        public RealmControllerArenaJson mPropertyInfos_Arena;
        //public ArenaJson mArenaInfo;
        private int mMyRank;
        private int mRankToChallenge;
        private int mNewRank;
        private bool mWon = false;
        private bool mHasResult = false;
        private GameTimer mDelaytimer;
        private AIPlayer mAIPlayer;
        private int mPlayerHp = 1;

        public RealmControllerArena(RealmControllerJson info, GameLogic instance)
            : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            mPropertyInfos_Arena = (RealmControllerArenaJson)info;
            //mArenaInfo = (ArenaJson)mRealmInfo;
            mCountDownOnMissionCompleted = 15;
        }

        //public override bool IsCorrectController()
        //{
        //    return mRealmInfo.type == RealmType.Arena;
        //}

        public override void OnPlayerEnter(Player player)
        {
            base.OnPlayerEnter(player);
            mPlayerHp = player.GetHealth();
            player.SetHealth(player.GetHealthMax());
            mMyRank = LadderRules.GetArenaRank(player.Name);
            mRankToChallenge = player.Slot.ArenaRankToChallenge;
            mNewRank = mRankToChallenge;
            SpawnAIPlayer();
            mAIPlayer.SetAIBehaviour(new NullAIBehaviour(mAIPlayer));
            mDelaytimer = mInstance.SetTimer(3200, (arg) =>
            {
                mDelaytimer = null;
                mInstance.BroadcastEvent(this, "OnRealmStart");
                mAIPlayer.SetAIBehaviour(new AIPlayerAIBehaviour(mAIPlayer));
            }, null);
        }

        public override void OnPlayerExit(Player player)
        {
            base.OnPlayerExit(player);
            player.SetHealth(mPlayerHp);
            string playername = player.Name;
            ArenaInventoryData inventory = player.Slot.CharacterData.ArenaInventory;
            //if (mRankToChallenge < GameConstantRepo.GetConstantInt("Arena_NoCDRankFrom", 300) && VIPRepo.GetVIPPrivilege("ArenaNoCD", player.PlayerSynStats.vipLvl) == 0)
            //    inventory.LastBattleDT = DateTime.Now; 
            inventory.Entries += 1;
            if (mWon)
            {
                if (inventory.ArenaRankHighest > mNewRank)
                    inventory.ArenaRankHighest = mNewRank;

                player.Slot.mSevenDaysController.UpdateTask(NewServerActivityType.Militantrank, inventory.ArenaRankHighest);
            }
            if (!mHasResult)
            {              
                LadderRules.ArenaLose(player, mRankToChallenge);
                GiveReward(player);
            }
            player.Slot.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.ArenaTimes);
        }
        
        private void SpawnAIPlayer()
        {
            ArenaPlayerRecord opponent = LadderRules.GetArenaPlayerRecord(mRankToChallenge);
            CharacterCreationData CharacterCreationData = null;
            BonusCombatStats BonusCombatStats = null;
            SkillInventoryData SkillInventory = null;
            ArenaSkillLevel ArenaSkillLevel = null;

            if (opponent.Fake)
            {
                ArenaPlayerRecord fake_record = SetUpFakePlayer();
                CharacterCreationData = fake_record.CharacterCreationData;
                BonusCombatStats = fake_record.BonusCombatStats;
                SkillInventory = fake_record.SkillInventory;
                ArenaSkillLevel = fake_record.ArenaSkillLevel;
            }
            else
            {
                CharacterCreationData = opponent.CharacterCreationData;
                BonusCombatStats = opponent.BonusCombatStats;
                SkillInventory = opponent.SkillInventory;
                ArenaSkillLevel = opponent.ArenaSkillLevel;
            }

            string oppName = CharacterCreationData.Name;
            AIPlayer aiplayer = mInstance.mEntitySystem.SpawnNetEntity<AIPlayer>(true, oppName);
            
            PlayerSynStats playerStats = new PlayerSynStats();
            playerStats.name = oppName;
            playerStats.jobsect = CharacterCreationData.JobSect;
            playerStats.faction = (byte)FactionType.Dragon;
            playerStats.Level = CharacterCreationData.ProgressLevel;
            playerStats.Team = -2;
            aiplayer.PlayerStats = playerStats;
            aiplayer.PlayerSynStats = playerStats;
            int accumulatedLevel = aiplayer.GetAccumulatedLevel();

            EquipmentStats equipmentStats = new EquipmentStats();
            aiplayer.EquipmentStats = equipmentStats;
            aiplayer.InitEquipInvStats(CharacterCreationData.EquipmentInventory);

            /*********************   SynCombatStats   ***************************/
            LocalCombatStats localCombatStats = new LocalCombatStats();
            localCombatStats.IsInSafeZone = false; // Reset 
            aiplayer.LocalCombatStats = localCombatStats;

            /*********************   CombatStats   ***************************/
            PlayerCombatStats combatStats = new PlayerCombatStats();
            combatStats.SetPlayerLocalAndSyncStats(localCombatStats, playerStats, aiplayer);
            combatStats.SuppressComputeAll = true;

            Player.SetPlayerStats(playerStats.jobsect, playerStats.Level, combatStats);
            if (opponent.Fake)
            {
                // Add equipped combatstats
                EquipmentInventoryData eqInvData = CharacterCreationData.EquipmentInventory;
            }
            else
            {
                if (BonusCombatStats.HealthBonus != 0)
                    combatStats.AddToField(FieldName.HealthBonus, BonusCombatStats.HealthBonus);
                if (BonusCombatStats.HealthPercentBonus != 0)
                    combatStats.AddToField(FieldName.HealthPercBonus, BonusCombatStats.HealthPercentBonus);

                if (BonusCombatStats.AttackBonus != 0)
                    combatStats.AddToField(FieldName.AttackBonus, BonusCombatStats.AttackBonus);
                if (BonusCombatStats.AttackPercentBonus != 0)
                    combatStats.AddToField(FieldName.AttackPercBonus, BonusCombatStats.AttackPercentBonus);
                if (BonusCombatStats.ArmorBonus != 0)
                    combatStats.AddToField(FieldName.ArmorBonus, BonusCombatStats.ArmorBonus);
                if (BonusCombatStats.ArmorPercentBonus != 0)
                    combatStats.AddToField(FieldName.ArmorPercBonus, BonusCombatStats.ArmorPercentBonus);

                if (BonusCombatStats.AccuracyBonus != 0)
                    combatStats.AddToField(FieldName.AccuracyBonus, BonusCombatStats.AccuracyBonus);
                if (BonusCombatStats.AccuracyPercentBonus != 0)
                    combatStats.AddToField(FieldName.AccuracyPercBonus, BonusCombatStats.AccuracyPercentBonus);
                if (BonusCombatStats.EvasionBonus != 0)
                    combatStats.AddToField(FieldName.EvasionBonus, BonusCombatStats.EvasionBonus);
                if (BonusCombatStats.EvasionPercentBonus != 0)
                    combatStats.AddToField(FieldName.EvasionPercBonus, BonusCombatStats.EvasionPercentBonus);

                if (BonusCombatStats.CriticalBonus != 0)
                    combatStats.AddToField(FieldName.CriticalBonus, BonusCombatStats.CriticalBonus);
                if (BonusCombatStats.CriticalPercentBonus != 0)
                    combatStats.AddToField(FieldName.CriticalPercBonus, BonusCombatStats.CriticalPercentBonus);
                if (BonusCombatStats.CoCriticalBonus != 0)
                    combatStats.AddToField(FieldName.CocriticalBonus, BonusCombatStats.CoCriticalBonus);
                if (BonusCombatStats.CoCriticalPercentBonus != 0)
                    combatStats.AddToField(FieldName.CocriticalPercBonus, BonusCombatStats.CoCriticalPercentBonus);
                if (BonusCombatStats.CriticalDmgBonus != 0)
                    combatStats.AddToField(FieldName.CriticalDamageBonus, BonusCombatStats.CriticalDmgBonus);
                //if (BonusCombatStats.CriticalDmgPercentBonus != 0)
                //    combatStats.AddToField(FieldName.CriticalDamagePercBonus, BonusCombatStats.CriticalDmgPercentBonus);
                //if (BonusCombatStats.CoCriticalDmgBonus != 0)
                //    combatStats.AddToField(FieldName.CoCriticalDamageBonus, BonusCombatStats.CoCriticalDmgBonus);
                //if (BonusCombatStats.CoCriticalDmgPercentBonus != 0)
                //    combatStats.AddToField(FieldName.CoCriticalDamagePercBonus, BonusCombatStats.CoCriticalDmgPercentBonus);
                if (BonusCombatStats.AbsorbDmgBonus != 0)
                    combatStats.AddToField(FieldName.AbsorbDamageBonus, BonusCombatStats.AbsorbDmgBonus);
            }

            //skill inv
            SkillSynStats skillStats = new SkillSynStats();
            skillStats.CopyFromInvData(SkillInventory);
            aiplayer.SkillStats = skillStats;
             
            aiplayer.CombatStats = combatStats;//needed for the below .

             SkillComboData data = new SkillComboData();
            //data.red = skillStats.RedHeroCardSkillAttackSId;
            //data.red2 = skillStats.RedHeroCardSubskillId;
            //data.green = skillStats.GreenHeroCardSkillAttackSId;
            //data.green2 = skillStats.GreenHeroCardSubskillId;
            //data.blue = skillStats.BlueHeroCardSkillAttackSId;
            //data.blue2 = skillStats.BlueHeroCardSubskillId;
            data.redlevel = ArenaSkillLevel.RedLvl;
            data.greenlevel = ArenaSkillLevel.GreenLvl;
            data.bluelevel = ArenaSkillLevel.BlueLvl; 

            
            aiplayer.SetInstance(mInstance);
            aiplayer.Position = mPropertyInfos_Arena.aiPos;
            aiplayer.Forward = mPropertyInfos_Arena.aiForward;            
            aiplayer.PlayerStats.MoveSpeed = 12;       
            aiplayer.Idle();
            mAIPlayer = aiplayer;

            //this is done at last
            combatStats.SuppressComputeAll = false;
            combatStats.ComputeAll();
            mAIPlayer.SetHealth(mAIPlayer.GetHealthMax());//set health after computall
            mInstance.mEntitySystem.AddAlwaysShow(aiplayer);
        }

        private ArenaPlayerRecord SetUpFakePlayer()
        {
            ArenaPlayerRecord record = new ArenaPlayerRecord();
            record.Fake = true;
            //ArenaFakeRankJson fakeJson = ArenaRankRepo.mFakeRankArray[mRankToChallenge];
            //CharacterCreationData charData = record.CharacterCreationData;
            //charData.Name = fakeJson.localizedname;
            //charData.JobSect = (byte)fakeJson.jobsect;
            //charData.ProgressLevel = fakeJson.level;
            //charData.EquipScore = fakeJson.equipscore;

            //BonusCombatStats BonusCombatStats = record.BonusCombatStats;
            //EquippedInventoryData equippedInv = charData.EquippedInventory;
            //equippedInv.InitDefault();
            //JobType jobsect = (JobType)fakeJson.jobsect;
            //int equipupgrade = fakeJson.equipupgrade;
            //JobsectJson jobsectJson = JobSectRepo.GetJobById(fakeJson.jobsect);
            //GameUtils.SetCharacterFirstEquipments(equippedInv, jobsectJson);
            //if (equipupgrade > 0)
            //{
            //    for (int index = 0; index < equippedInv.Slots.Count; index++)
            //    {
            //        if (equippedInv.Slots[index] == null)
            //            continue;
            //        equippedInv.Slots[index].UpgradeLevel = (ushort)equipupgrade;
            //    }
            //}

            SkillInventoryData skillInventory = record.SkillInventory;
            ArenaSkillLevel skillLevels = record.ArenaSkillLevel;
             
            //skillInventory.basicAttack1SId = SkillRepo.Rage_BasicAtk1;
            //skillInventory.basicAttack2SId = SkillRepo.Rage_BasicAtk2;
            //skillInventory.basicAttack3SId = SkillRepo.Rage_BasicAtk3;
            

            return record;
        }

        public void OnAIPlayerDead(Player player)
        {
            if (mHasResult)
                return;
            if (timer != null)
                mInstance.StopTimer(timer);

            mWon = true;
            mHasResult = true;
            GiveReward(player);
            mNewRank = LadderRules.ArenaWin(player, mRankToChallenge);

            mDelaytimer = mInstance.SetTimer(3200, (arg) =>
            {
                mDelaytimer = null;             
                OnMissionCompleted(true, true);
            }, null);
        }

        public override void OnPlayerDead(Player player, Actor killer)
        {
            if (mHasResult)
                return;
            if (timer != null)
                mInstance.StopTimer(timer);
            
            mWon = false;
            mHasResult = true;
            GiveReward(player);
            LadderRules.ArenaLose(player, mRankToChallenge);

            mDelaytimer = mInstance.SetTimer(3200, (arg) =>
            {
                mDelaytimer = null;                               
                OnMissionCompleted(false, true);
            }, null);
        }

        public override void OnMissionCompleted(bool success, bool broadcast)
        {
            base.OnMissionCompleted(success, false);

            //mInstance.mEntitySystem.RemoveEntityByPID(mAIPlayer.GetPersistentID());
            foreach (Player player in mPlayers.Values)
            {
                player.Slot.ZRPC.CombatRPC.ShowScoreBoard(success, mCountDownOnMissionCompleted, mNewRank, mMyRank, player.Slot);
            }
        }

        private void GiveReward(Player player)
        {
            //LevelUpJson json = ExperienceRepo.GetLevelInfo(player.PlayerSynStats.Level);
            //if (json != null)
            //{
            //    int exp = json.arenaexp;
            //    if (mWon)
            //        player.AddExperience(exp);
            //    else
            //        player.AddExperience(exp/2);
            //}
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

        public void DebugAIPlayerStats(Player player)
        {
            mAIPlayer.DebugSendCombatStats(player);
        }
    }
}
