using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.AI;
using Zealot.Server.Entities;
using Zealot.Server.Rules;
using Zealot.Server.SideEffects;

namespace Photon.LoadBalancing.GameServer
{
    public class HeroBondSideEffect
    {
        public int id;
        public List<IPassiveSideEffect> passiveSEs;

        public HeroBondSideEffect(int bondId, List<IPassiveSideEffect> se)
        {
            id = bondId;
            passiveSEs = se;
        }
    }

    public class HeroStatsServer : HeroStats
    {
        private Player player;
        private GameClientPeer peer;
        private Dictionary<int, HeroBondSideEffect> fulfilledBonds;  // group id -> highest level id
        private HeroEntity summonedHeroEntity;
        private List<IPassiveSideEffect> summonPassiveSEs;
        private Dictionary<int, List<IPassiveSideEffect>> heroPassiveSEs;  // hero id -> list of passives
        private HeroInterestType randomSpinResult;
        private Dictionary<int, Timer> explorationEndTimers;

        public HeroStatsServer() : base()
        {
            fulfilledBonds = new Dictionary<int, HeroBondSideEffect>();
            summonPassiveSEs = new List<IPassiveSideEffect>();
            heroPassiveSEs = new Dictionary<int, List<IPassiveSideEffect>>();
            explorationEndTimers = new Dictionary<int, Timer>();
        }

        public void Init(Player _player, GameClientPeer _peer, HeroInvData invData)
        {
            player = _player;
            peer = _peer;

            List<Hero> heroesList = invData.HeroesList;
            for (int i = 0; i < heroesList.Count; i++)
            {
                Hero hero = heroesList[i];
                HeroJson heroData = HeroRepo.GetHeroById(hero.HeroId);
                if (heroData != null)
                {
                    hero.HeroJson = heroData;
                    hero.ComputeCombatStats();
                    AddHero(hero);
                    ApplyHeroPassiveSEs(hero, false);
                }
            }

            Hero summonedHero = GetHero(invData.SummonedHero);
            if (summonedHero != null)
                SummonedHeroId = summonedHero.HeroId;

            ApplyHeroBondSEs();

            int count = invData.OngoingMaps.Count;
            for (int i = 0; i < count; i++)
            {
                ExploreMapData map = invData.OngoingMaps[i];
                ExplorationMapJson mapData = HeroRepo.GetExplorationMapById(map.MapId);
                if (mapData != null)  // ensure valid map
                {
                    explorationsDict.Add(map.MapId, map);
                    SetExplorationTimer(map);
                }
            }
            if (explorationsDict.Count > 0)
                Explorations = JsonConvertDefaultSetting.SerializeObject(explorationsDict);

            Explored = invData.ExploredMaps;
            ParseExploredString();
        }

        public void SaveToInventory(HeroInvData invData)
        {
            invData.HeroesList.Clear();
            foreach (Hero hero in mHeroesDict.Values)
                invData.HeroesList.Add(hero);
            invData.SummonedHero = SummonedHeroId;
            invData.OngoingMaps.Clear();
            foreach (ExploreMapData map in explorationsDict.Values)
                invData.OngoingMaps.Add(map);
            invData.ExploredMaps = Explored;
        }

        public void PostPlayerSpawn()
        {
            Hero summonedHero = GetHero(SummonedHeroId);
            if (summonedHero != null)
            {
                bool canSpawn = true;
                // if play is in party, only can spawn hero if the hero is in the party
                if (player.IsInParty() && player.PartyStats.GetHeroOwnedByMember(player.Name) == null)
                    canSpawn = false;

                if (canSpawn)
                    SpawnHeroEntity(summonedHero, false);
            }

            // check triggered quests on first login
            if (peer.mFirstLogin)
            {
                foreach (var hero in mHeroesDict.Values)
                {
                    List<int> quests = hero.GetAllTriggeredQuests();
                    for (int i = 0; i < quests.Count; i++)
                    {
                        if (!player.QuestController.HasQuestBeenTriggered(quests[i]))
                            player.QuestController.TriggerNewQuest(quests[i], hero.HeroId);
                    }
                }
            }
        }

        private int GetEmptySlot()
        {
            for (int i = 0; i < heroes.Count; i++)
            {
                if (heroes[i] == null)
                    return i;
            }
            return -1;
        }

        private void AddHero(Hero hero)
        {
            mHeroesDict[hero.HeroId] = hero;
            heroes[hero.SlotIdx] = hero.ToString();
        }

        public int GetHeroCountByRarity(HeroRarity rarity)
        {
           return  mHeroesDict.Values.Count(x => x.HeroJson.rarity == rarity);
        }

        private bool DeductItem(int itemId, int itemCount)
        {
            //itemCount = 0; // testing use
            InvRetval retval = peer.mInventory.DeductItems((ushort)itemId, itemCount, "Hero");
            return retval.retCode == InvReturnCode.UseSuccess;
        }

        private bool DeductItem(string itemStr, int itemCount)
        {
            ushort bindItemId;
            string[] itemIds = itemStr.Split(';');
            if (itemIds.Length > 0 && ushort.TryParse(itemIds[0], out bindItemId))
            {
                if (DeductItem(bindItemId, itemCount))
                    return true;
                else
                {
                    ushort unbindItemId;
                    if (itemIds.Length > 1 && ushort.TryParse(itemIds[1], out unbindItemId))
                    {
                        int currBindItemCount = peer.mInventory.GetItemStackCountByItemId(bindItemId);
                        int currUnbindItemCount = peer.mInventory.GetItemStackCountByItemId(unbindItemId);
                        if (currBindItemCount >= itemCount - currUnbindItemCount)  // check total is enough
                        {
                            int remainingAmt = itemCount;
                            if (currBindItemCount > 0)
                            {
                                remainingAmt -= currBindItemCount;
                                DeductItem(bindItemId, currBindItemCount);  // deduct all current bind amount
                            }
                            DeductItem(unbindItemId, remainingAmt); // deduct remaining amt of unbind
                            return true;
                        }
                        else
                            return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// free only used for debug
        /// </summary>
        public void UnlockHero(int heroId, bool free = false)
        {
            HeroJson heroData = HeroRepo.GetHeroById(heroId);
            if (heroData == null)
                return;

            if (!IsHeroUnlocked(heroId))
            {
                int itemCount = free ? 0 : heroData.unlockitemcount;
                if (DeductItem(heroData.unlockitemid, itemCount))
                {
                    Hero newHero = new Hero(heroId, heroData, GetEmptySlot());
                    AddHero(newHero);

                    PlayerCombatStats combatStats = (PlayerCombatStats)player.CombatStats;
                    combatStats.SuppressComputeAll = true;
                    bool applied1 = ApplyHeroPassiveSEs(newHero, false);
                    bool applied2 = UpdateHeroBonds(heroId);
                    combatStats.SuppressComputeAll = false;
                    if (applied1 || applied2)
                        combatStats.ComputeAll();

                    // achievements
                    player.AchievementStats.UpdateCollection(CollectionType.Hero, heroId);
                    player.UpdateAchievement(AchievementObjectiveType.HeroNumber, ((int)heroData.rarity).ToString(), false,
                        GetHeroCountByRarity(heroData.rarity), false);
                    player.UpdateAchievement(AchievementObjectiveType.HeroLevel, heroId.ToString(), false, newHero.Level, false);

                }
                else
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_ShardsNotEnough", "", false, peer);
            }
        }

        public void LevelUpHero(int heroId)
        {
            Hero hero = GetHero(heroId);
            if (hero != null)
            {
                if (hero.IsMaxLevel())
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_MaxHeroLevel", "", false, peer);
                    return;
                }
                if (hero.Level >= player.PlayerSynStats.Level)
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_MaxPlayerLevel", "", false, peer);
                    return;
                }

                HeroGrowthJson growthData = HeroRepo.GetHeroGrowthData(hero.HeroJson.growthgroup, hero.Level);
                if (player.DeductMoney(growthData.levelupmoney, "Hero"))
                {
                    hero.Level++;

                    // update summoned hero entity if is this hero
                    if (IsHeroSummoned(heroId))
                    {
                        summonedHeroEntity.HeroSynStats.Level = hero.Level;
                        //summonedHeroEntity.SetCombatStats(hero.Level);
                    }
                    hero.ComputeCombatStats();
                    heroes[hero.SlotIdx] = hero.ToString();

                    // update hero bonds
                    PlayerCombatStats combatStats = (PlayerCombatStats)player.CombatStats;
                    combatStats.SuppressComputeAll = true;
                    bool applied = UpdateHeroBonds(heroId);
                    combatStats.SuppressComputeAll = false;
                    if (applied)
                        combatStats.ComputeAll();

                    // if hero is in player's party, need to update partystats
                    if (player.IsInParty())
                    {
                        PartyMember heroMember = player.PartyStats.GetHeroOwnedByMember(player.Name);
                        if (heroMember != null)
                            player.PartyStats.SetMemberLevel(heroMember.name, hero.Level);
                    }

                    // achievements
                    player.UpdateAchievement(AchievementObjectiveType.HeroLevel, heroId.ToString(), false, hero.Level, false);
                    player.UpdateAchievement(AchievementObjectiveType.HeroGrowth);
                }
                else
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_MoneyNotEnough", "", false, peer);
            }
        }

        public void AddSkillPoint(int heroId)
        {
            Hero hero = GetHero(heroId);
            if (hero == null)
                return;

            int itemCount = HeroRepo.GetItemCountForSkillPointByHeroId(heroId, hero.GetTotalSkillPoints() + 1);
            if (itemCount <= 0)
            {
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_MaxSkillPoints", "", false, peer);
                return;
            }

            if (DeductItem(hero.HeroJson.upgradeitemid, itemCount))
            {
                hero.SkillPoints++;

                int highestUnlockedTier = hero.GetHighestUnlockedTier();
                if (hero.ModelTier != highestUnlockedTier)
                {
                    hero.ModelTier = highestUnlockedTier;
                    if (IsHeroSummoned(heroId))
                        SummonHero(heroId, highestUnlockedTier);
                }
                heroes[hero.SlotIdx] = hero.ToString();

                PlayerCombatStats combatStats = (PlayerCombatStats)player.CombatStats;
                combatStats.SuppressComputeAll = true;
                bool applied = UpdateHeroBonds(heroId);
                combatStats.SuppressComputeAll = false;
                if (applied)
                    combatStats.ComputeAll();

                // achievement: hero skill pts
                player.UpdateAchievement(AchievementObjectiveType.HeroSkill, heroId.ToString(), false, hero.GetTotalSkillPoints(), false);

            }
            else
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_ShardsNotEnough", "", false, peer);
        }

        public void ResetSkillPoints(int heroId)
        {
            Hero hero = GetHero(heroId);
            if (hero == null)
                return;

            if (hero.Skill1Level == 1 && hero.Skill2Level == 1 && hero.Skill3Level == 1) // skills are already at base levels
                return;

            if (player.DeductMoney(hero.HeroJson.resetskillmoney, "Hero"))
            {
                hero.ResetSkillPoints();
                heroes[hero.SlotIdx] = hero.ToString();

                // update passive se
                UpdateHeroPassiveSEs(hero);
                if (IsHeroSummoned(heroId))
                {
                    summonedHeroEntity.OnSkillLevelChanged();
                    UpdateSummonPassiveSEs();
                }
            }
            else
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_MoneyNotEnough", "", false, peer);
        }

        public void LevelUpSkill(int heroId, int skillNo)
        {
            Hero hero = GetHero(heroId);
            if (hero != null && hero.SkillPoints > 0 && hero.CanLevelUpSkill(skillNo))
            {
                hero.SkillPoints--;
                switch (skillNo)
                {
                    case 1:
                        hero.Skill1Level++;
                        if (IsHeroSummoned(heroId))
                            summonedHeroEntity.OnSkillLevelChanged();
                        break;
                    case 2:
                        hero.Skill2Level++;
                        if (IsHeroSummoned(heroId))
                            UpdateSummonPassiveSEs();
                        break;
                    case 3:
                        hero.Skill3Level++;
                        UpdateHeroPassiveSEs(hero);
                        break;
                }
                heroes[hero.SlotIdx] = hero.ToString();
            }
        }

        public void ChangeHeroInterest(int heroId, byte assignedInterest, bool acceptResult)
        {
            Hero hero = GetHero(heroId);
            HeroInterestType interest = (HeroInterestType)assignedInterest;
            if (hero != null && hero.Interest != interest && !hero.IsAway)
            {
                if (interest == HeroInterestType.Random) // random
                {
                    if (acceptResult && randomSpinResult != HeroInterestType.Random) // means accept previous spin's result
                    {
                        hero.Interest = randomSpinResult;
                        heroes[hero.SlotIdx] = hero.ToString();
                    }
                    else // means do random spin
                    {
                        randomSpinResult = HeroRepo.GetRandomInterestByGroup(hero.HeroJson.interestgroup, hero.Interest);  // exclude current interest
                        if (randomSpinResult == HeroInterestType.Random)
                        {
                            peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_InterestChangeFailed", "", false, peer);
                            return;
                        }

                        if (DeductItem(hero.HeroJson.randomitemid, 1))
                            peer.ZRPC.CombatRPC.Ret_RandomInterestResult((byte)randomSpinResult, peer);
                        else
                            peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_GiftNotEnough", "", false, peer);
                    }
                }
                else  // assigned
                {
                    HeroInterestJson interestData = HeroRepo.GetInterestByType(interest);
                    if (interestData != null)
                    {
                        if (!HeroRepo.IsInterestInGroup(hero.HeroJson.interestgroup, interest))  // assigned interest not in hero's interest group
                            return;

                        if (DeductItem(interestData.assigneditemid, 1))
                        {
                            hero.Interest = interest;
                            heroes[hero.SlotIdx] = hero.ToString();
                        }
                        else
                            peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_GiftNotEnough", "", false, peer);
                    }
                }
            }
        }

        public void AddHeroTrust(int heroId, int itemId, bool deductItem = true)
        {
            Hero hero = GetHero(heroId);
            HeroItem giftItem = GameRepo.ItemFactory.GetInventoryItem(itemId) as HeroItem;
            if (hero != null && giftItem != null)
            {
                if (!hero.CanAddTrust())
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_MaxTrustLevel", "", false, peer);
                    return;
                }

                // check item used is a gift and is applicable for this hero
                string[] applicableHeros = giftItem.HeroItemJson.heroid.Split(';');
                if (giftItem.HeroItemJson.heroitemtype == HeroItemType.Gift
                    && (giftItem.HeroItemJson.heroid == "-1" || applicableHeros.Contains(heroId.ToString())))
                {
                    if (!deductItem || DeductItem(itemId, 1))
                    {
                        int totalExpAdded = giftItem.HeroItemJson.giftexp;
                        hero.TrustExp += giftItem.HeroItemJson.giftexp;
                        HeroTrustJson trustLevelData = HeroRepo.GetTrustLevelData(hero.TrustLevel);
                        int nextLevelExp = trustLevelData == null ? 0 : trustLevelData.reqtrustexp;
                        if (nextLevelExp > 0 && hero.TrustExp >= nextLevelExp)
                        {
                            while (hero.TrustLevel < HeroRepo.MAX_TRUST_LEVEL && hero.TrustExp >= nextLevelExp)
                            {
                                hero.TrustExp -= nextLevelExp;
                                hero.TrustLevel++;
                                int triggeredQuest = hero.GetTriggeredQuest();
                                if (triggeredQuest > 0)
                                    player.QuestController.TriggerNewQuest(triggeredQuest, heroId);

                                trustLevelData = HeroRepo.GetTrustLevelData(hero.TrustLevel);
                                nextLevelExp = trustLevelData == null ? 0 : trustLevelData.reqtrustexp;

                                //achievement: trust level
                                player.UpdateAchievement(AchievementObjectiveType.HeroTrust, heroId.ToString(), false, hero.TrustLevel, false);
                            }
                        }

                        if (hero.TrustLevel == HeroRepo.MAX_TRUST_LEVEL)
                        {
                            totalExpAdded = giftItem.HeroItemJson.giftexp - hero.TrustExp;
                            hero.TrustExp = 0;
                        }
                        heroes[hero.SlotIdx] = hero.ToString();
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_GiftSuccess",
                            string.Format("hero;{0};exp;{1}", hero.HeroJson.localizedname, totalExpAdded), false, player.Slot);

                        // achievement: gift count
                        string target = heroId + ";" + itemId;
                        player.UpdateAchievement(AchievementObjectiveType.HeroGift, target, false);
                        player.UpdateAchievement(AchievementObjectiveType.HeroInteract);
                    }
                    else
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_GiftNotEnough", "", false, peer);
                }
            }
        }

        public void UnlockHeroSkin(int heroId, int itemId)
        {
            Hero hero = GetHero(heroId);
            if (hero != null)
            {
                string[] skinitems = hero.HeroJson.skinitemid.Split(';');
                for (int i = 0; i < skinitems.Length; i++)
                {
                    string[] itemids = skinitems[i].Split(',');
                    int bindItemId = 0, unbindItemId = 0;  // get bind and unbind id of the skin item
                    if (itemids.Length > 0)
                        int.TryParse(itemids[0], out bindItemId);
                    if (itemids.Length > 1)
                        int.TryParse(itemids[1], out unbindItemId);
                    // item used is one of bind and unbind item and has previously not used this item
                    if ((itemId == bindItemId || itemId == unbindItemId) && !hero.UnlockedSkinItems.Contains(itemId))
                    {
                        hero.UnlockedSkinItems.Add(bindItemId); // add bind id
                        if (unbindItemId > 0)
                            hero.UnlockedSkinItems.Add(unbindItemId); // add unbind id if have
                        heroes[hero.SlotIdx] = hero.ToString();
                        break;
                    }
                }
            }
        }

        #region Summon Hero
        public bool IsHeroSummoned(int heroId)
        {
            return summonedHeroEntity != null && summonedHeroEntity.Hero.HeroId == heroId;
        }

        public HeroEntity GetSummonedHeroEntity()
        {
            return summonedHeroEntity;
        }

        public void SummonHero(int heroId, int tier = 0)
        {
            if (heroId == 0)  // means unsummon
            {
                if (SummonedHeroId > 0)
                {
                    SummonedHeroId = 0;
                    UnSummonHeroEntity();

                    // if player has party and hero is also in party, need to remove hero from party
                    if (player.IsInParty())
                    {
                        PartyMember heroMember = player.PartyStats.GetHeroOwnedByMember(player.Name);
                        if (heroMember != null)
                            PartyRules.LeaveParty(player.PlayerSynStats.Party, heroMember.name, LeavePartyReason.Self);
                    }
                }
            }
            else
            {
                Hero hero = GetHero(heroId);
                if (hero == null)
                    return;

                if (!hero.CanSummon(player.PlayerSynStats.Level))
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_SummonLevelNotReached",
                            string.Format("level;{0}", hero.HeroJson.summonlevel), false, peer);
                    return;
                }

                if (hero.IsAway)
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_HeroAwayExploring", "", false, peer);
                    return;
                }

                if (summonedHeroEntity == null || summonedHeroEntity.HeroSynStats.HeroId != heroId || summonedHeroEntity.HeroSynStats.ModelTier != tier)
                {
                    bool canSummon = !player.IsInParty() || player.PartyStats.CanAddMemberHeroToParty(player.Name);
                    // if player has party, so try to add hero to party
                    if (canSummon)
                    {
                        if (tier == 0)
                            tier = hero.ModelTier;

                        if (SummonedHeroId != heroId)
                            SummonedHeroId = heroId;  // for record

                        if (hero.ModelTier != tier)
                        {
                            hero.ModelTier = tier;  // save this summoned tier
                            heroes[hero.SlotIdx] = hero.ToString();
                        }

                        SpawnHeroEntity(hero, true);

                        if (player.IsInParty())
                        {
                            PartyMember newMember = new PartyMember(hero, player.Name);
                            PartyMember currHeroMember = player.PartyStats.GetHeroOwnedByMember(player.Name);
                            if (currHeroMember != null)  // already have owned hero in party, so replace current hero
                            {
                                newMember.slotIdx = currHeroMember.slotIdx;
                                player.PartyStats.UpdateMemberHero(currHeroMember.name, newMember);
                                player.PartyStats.OnDirty();
                            }
                            else if (!player.PartyStats.IsPartyFull())  // no hero in party and party is not full, so add hero to party
                            {
                                player.PartyStats.AddPartyMember(newMember);
                                player.PartyStats.OnDirty();
                            }
                        }
                    }
                    else
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_PartyFullCannotSummon", "", false, peer);
                }
            }
        }

        public void SetSpawnedHeroPosition(Player player)
        {
            if (summonedHeroEntity != null)
            {
                summonedHeroEntity.SetSpawnPosition(player);
                summonedHeroEntity.Idle();
            }
        }

        public void SpawnHeroEntity(Hero hero, bool summoning)
        {
            bool changedHero = false;
            if (summonedHeroEntity == null)
            {
                summonedHeroEntity = player.mInstance.mEntitySystem.SpawnNetEntity<HeroEntity>(false);
                HeroSynStats playerStats = new HeroSynStats();
                summonedHeroEntity.PlayerStats = playerStats;
                summonedHeroEntity.HeroSynStats = playerStats;
                summonedHeroEntity.SetOwnerID(player.GetPersistentID());
                summonedHeroEntity.SetInstance(player.mInstance);
                changedHero = true;
            }
            else
            {
                int prevHeroId = summonedHeroEntity.Hero.HeroId;
                if (prevHeroId != hero.HeroId)
                    changedHero = true;
            }

            summonedHeroEntity.Init(hero, player, summoning);
            summonedHeroEntity.SetAIBehaviour(new HeroAIBehaviour(summonedHeroEntity));

            if (changedHero)  // need to apply passive se if is summon new hero
                UpdateSummonPassiveSEs();
        }

        public void UnSummonHeroEntity()
        {
            if (summonedHeroEntity != null)
            {
                summonedHeroEntity.CleanUp();
                summonedHeroEntity = null;

                // remove passive se from summoned hero
                PlayerCombatStats combatStats = (PlayerCombatStats)player.CombatStats;
                combatStats.SuppressComputeAll = true;
                bool applied = RemoveSummonPassiveSEs();
                combatStats.SuppressComputeAll = false;
                if (applied)
                    combatStats.ComputeAll();
            }
        }
        #endregion Summon Hero

        #region Passive SEs
        private void UpdateHeroPassiveSEs(Hero hero)
        {
            PlayerCombatStats combatStats = (PlayerCombatStats)player.CombatStats;
            bool canCompute = !combatStats.SuppressComputeAll;
            if (canCompute)
                combatStats.SuppressComputeAll = true;

            bool applied1 = RemoveHeroPassiveSEs(hero.HeroId);  // remove any previous se
            bool applied2 = ApplyHeroPassiveSEs(hero, false);  // apply se from this hero

            if (canCompute)
            {
                combatStats.SuppressComputeAll = false;
                if (applied1 || applied2)
                    combatStats.ComputeAll();
            }
        }

        private void UpdateSummonPassiveSEs()
        {
            PlayerCombatStats combatStats = (PlayerCombatStats)player.CombatStats;
            bool canCompute = !combatStats.SuppressComputeAll;
            if (canCompute)
                combatStats.SuppressComputeAll = true;

            bool applied1 = RemoveSummonPassiveSEs();  // remove any previous se
            bool applied2 = ApplyHeroPassiveSEs(summonedHeroEntity.Hero, true);  // apply se from this hero

            if (canCompute)
            {
                combatStats.SuppressComputeAll = false;
                if (applied1 || applied2)
                    combatStats.ComputeAll();
            }
        }

        private bool ApplyHeroPassiveSEs(Hero hero, bool isSummon)
        {
            bool needCompute = false;
            int skillGroup = isSummon ? hero.HeroJson.skill2grp : hero.HeroJson.skill3grp;
            int skillLevel = isSummon ? hero.Skill2Level : hero.Skill3Level;
            SkillData passiveSkill = SkillRepo.GetSkillByGroupIDOfLevel(skillGroup, skillLevel);
            if (passiveSkill != null && passiveSkill.skills != null)
            {
                List<SideEffectJson> passiveSEs = passiveSkill.skills.mTarget;
                for (int i = 0; i < passiveSEs.Count; i++)
                {
                    SideEffect se = SideEffectFactory.CreateSideEffect(passiveSEs[i], true);
                    IPassiveSideEffect pse = se as IPassiveSideEffect;
                    if (pse != null)
                    {
                        pse.AddPassive(player);
                        needCompute = true;
                        if (isSummon)
                            summonPassiveSEs.Add(pse);
                        else
                        {
                            if (heroPassiveSEs.ContainsKey(hero.HeroId))
                                heroPassiveSEs[hero.HeroId].Add(pse);
                            else
                                heroPassiveSEs.Add(hero.HeroId, new List<IPassiveSideEffect>() { pse });
                        }
                    }
                    else
                        GameUtils.DebugWriteLine("Warning! Invalid sideeffect added as passive seid: " + se.mSideeffectData.id + " for heroid: " + hero.HeroId);
                }
            }
            return needCompute;
        }

        private bool RemoveHeroPassiveSEs(int heroId)
        {
            List<IPassiveSideEffect> pselist;
            if (heroPassiveSEs.TryGetValue(heroId, out pselist))
            {
                for (int i = 0; i < pselist.Count; i++)
                    pselist[i].RemovePassive();
                pselist.Clear();
                return true;
            }
            return false;
        }

        private bool RemoveSummonPassiveSEs()
        {
            bool needCompute = summonPassiveSEs.Count > 0;
            for (int i = 0; i < summonPassiveSEs.Count; i++)
            {
                summonPassiveSEs[i].RemovePassive();
            }
            summonPassiveSEs.Clear();
            return needCompute;
        }
        #endregion Passive SEs

        #region HeroBonds
        // don't need to compute combat stats since will be computed after all stats initialized
        private void ApplyHeroBondSEs()
        {
            foreach (HeroBond bond in HeroRepo.heroBonds.Values)
            {
                HeroBondJson highestLevelData = bond.GetHighestFulfilledLevel(this);
                if (highestLevelData != null)
                    AddBondSEs(bond.heroBondGroupJson.id, highestLevelData);
            }
        }

        private bool UpdateHeroBonds(int heroId)
        {
            bool needComputeAll = false;
            List<HeroBond> bonds = HeroRepo.GetInvolvedBondsByHeroId(heroId);
            for (int i = 0; i < bonds.Count; i++)
            {
                int groupId = bonds[i].heroBondGroupJson.id;
                HeroBondJson highestLevelData = bonds[i].GetHighestFulfilledLevel(this);
                int highestLevelId = highestLevelData != null ? highestLevelData.id : 0;
                if (fulfilledBonds.ContainsKey(groupId))  // currently have this bond
                {
                    if (fulfilledBonds[groupId].id != highestLevelId)  // change in fulfilled level
                    {
                        if (RemoveBondSEs(groupId))  // remove existing passives
                        {
                            needComputeAll = true;
                            //GameUtils.DebugWriteLine("Remove se from groupId: " + groupId);
                        }

                        if (highestLevelId > 0 && AddBondSEs(groupId, highestLevelData)) // add new passives
                            needComputeAll = true;
                        //GameUtils.DebugWriteLine("Update groupId/highestLevelId: " + groupId + "/" + highestLevelId);
                    }
                }
                else if (highestLevelData != null)  // new bond, add side effects
                {
                    if (AddBondSEs(groupId, highestLevelData))
                        needComputeAll = true;
                    //GameUtils.DebugWriteLine("Add groupId: " + groupId);
                }
            }
            return needComputeAll;
        }

        private bool AddBondSEs(int groupId, HeroBondJson bondData)
        {
            bool needCompute = false;
            List<IPassiveSideEffect> passiveSEs = new List<IPassiveSideEffect>();
            foreach (SideEffectJson seJson in bondData.sideeffects.Values)
            {
                SideEffect se = SideEffectFactory.CreateSideEffect(seJson, true);
                IPassiveSideEffect pse = se as IPassiveSideEffect;
                if (pse != null)
                {
                    pse.AddPassive(player);
                    passiveSEs.Add(pse);
                    needCompute = true;
                    //GameUtils.DebugWriteLine("Add se: " + se.mSideeffectData.id);
                }
            }
            fulfilledBonds[groupId] = new HeroBondSideEffect(bondData.id, passiveSEs);
            return needCompute;
        }

        private bool RemoveBondSEs(int groupId)
        {
            bool needCompute = false;
            var passiveSEs = fulfilledBonds[groupId].passiveSEs;
            for (int i = 0; i < passiveSEs.Count; i++)
            {
                passiveSEs[i].RemovePassive();
                needCompute = true;
            }
            passiveSEs.Clear();
            fulfilledBonds.Remove(groupId);
            return needCompute;
        }
        #endregion HeroBonds

        #region Exploration
        public void ExploreMap(int mapId, int targetId, List<int> heroIds)
        {
            ExplorationMapJson mapData = HeroRepo.GetExplorationMapById(mapId);
            if (mapData == null)
                return;
            if (explorationsDict.Count >= HeroRepo.EXPLORE_LIMIT)
            {
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_ReachedExploreLimit", "", false, peer);
                return;
            }
            if (IsExploringMap(mapId))  // check whether map is already currently in progross
                return;
            if (!mapData.repeatable && HasExploredMap(mapId))  // check one time map cannot be explored again
                return;
            if (mapData.prevmapid > 0 && !HasExploredMap(mapData.prevmapid)) // check has completed previous map
                return;
            if (heroIds.Count > mapData.maxherocount)  // check sent heroes not more than max allowed
                return;

            List<Hero> heroesList = new List<Hero>();
            for (int i = 0; i < heroIds.Count; i++) // check heros meet map requirements
            {
                Hero hero = GetHero(heroIds[i]);
                if (hero == null)  // should not happen
                    return;
                if (hero.Level < mapData.reqherolevel || hero.TrustLevel < mapData.reqherotrust)
                    return;
                heroesList.Add(hero);
            }

            if (targetId > 0 && player.PlayerSynStats.AchievementLevel < mapData.reqmonsterlevel)
                return;

            if (DeductItem(mapData.reqitemid, mapData.reqitemcount))
            {
                // deduct combat time
                player.DeductBattleTime(mapData.battletimecost * 60);

                // determine end time and add map to ongoing explorations
                DateTime endTime = DateTime.Now.AddMinutes(mapData.completetime);
                ExploreMapData map = new ExploreMapData(mapId, targetId, heroIds, endTime);
                SetExplorationTimer(map); // set end time timer

                // determine rewards and set heroes as away
                DetermineTargetRewards(targetId, heroesList, map, mapData);

                explorationsDict.Add(mapId, map);
                Explorations = JsonConvertDefaultSetting.SerializeObject(explorationsDict);

                // achievements
                player.UpdateAchievement(AchievementObjectiveType.HeroExploration);
            }
            else
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_ExploreItemNotEnough", "", false, peer);
        }

        private void SetExplorationTimer(ExploreMapData map)
        {
            double remainingTime = (map.EndTime - DateTime.Now).TotalMilliseconds;
            if (remainingTime > 0)
            {
                Timer timer = new Timer(remainingTime);
                timer.Elapsed += delegate { OnExplorationEnd(map.MapId); };
                timer.AutoReset = false;
                timer.Start();
                explorationEndTimers.Add(map.MapId, timer);
            }
            else
                map.Completed = true;
        }

        private void OnExplorationEnd(int mapId)
        {
            Timer timer;
            if (explorationEndTimers.TryGetValue(mapId, out timer))
            {
                timer.Stop();
                explorationEndTimers.Remove(mapId);
                ExploreMapData map = GetExploringMap(mapId);
                if (map != null)
                {
                    map.Completed = true;
                    Explorations = JsonConvertDefaultSetting.SerializeObject(explorationsDict);
                }
            }
        }

        private int GetFulfilledChestCount(ExplorationMapJson mapData, List<Hero> heroesList)
        {
            int count = 1;  // minimum 1
            if (IsChestRequirementFulfilled(mapData.chestreqtype1, mapData.chestreqvalue1, heroesList))
                count++;
            if (IsChestRequirementFulfilled(mapData.chestreqtype2, mapData.chestreqvalue2, heroesList))
                count++;
            if (IsChestRequirementFulfilled(mapData.chestreqtype3, mapData.chestreqvalue3, heroesList))
                count++;
            return count;
        }

        private void DetermineTargetRewards(int targetId, List<Hero> heroesList, ExploreMapData map, ExplorationMapJson mapData)
        {
            // calculate efficiency
            float efficiency = mapData.baseefficiency * 0.01f;
            for (int i = 0; i < heroesList.Count; i++)
            {
                Hero hero = heroesList[i];
                hero.IsAway = true; // set used heroes as away
                heroes[hero.SlotIdx] = hero.ToString();
                efficiency += hero.GetTotalExploreEfficiency(mapData);
            }

            Dictionary<CurrencyType, int> currencyToAdd = new Dictionary<CurrencyType, int>();
            List<ItemInfo> rewardToAdd = new List<ItemInfo>();
            
            // add any fulfilled chest
            int chestCount = GetFulfilledChestCount(mapData, heroesList);  // at least 1
            rewardToAdd.Add(new ItemInfo { itemId = (ushort)mapData.chestitemid, stackCount = chestCount });

            // get target info
            ExplorationTargetJson target = null;
            if (targetId == 0) // explore all, so random pick one target
            {
                var targetList = HeroRepo.GetExplorationTargetsByGroup(mapData.exploregroupid);
                if (targetList.Count > 0)
                    target = targetList[GameUtils.RandomInt(0, targetList.Count - 1)];
            }
            else // has specific target
                target = HeroRepo.GetExplorationTargetById(targetId);

            if (target != null)  // add rewards items from target
            {
                if (target.rewardgroupid > 0)
                {
                    Dictionary<int, int> rewardlistItems = new Dictionary<int, int>();
                    List<ItemInfo> itemInfoList = new List<ItemInfo>();
                    GameRules.GenerateRewardByRewardGrpID(target.rewardgroupid, player.PlayerSynStats.Level, player.PlayerSynStats.progressJobLevel, player.PlayerSynStats.jobsect,
                        itemInfoList, currencyToAdd);
                    // add up items from reward list
                    for (int i = 0; i < itemInfoList.Count; i++)
                    {
                        ItemInfo item = itemInfoList[i];
                        if (item.stackCount > 0)
                        {
                            if (rewardlistItems.ContainsKey(item.itemId))
                                rewardlistItems[item.itemId] += item.stackCount;
                            else
                                rewardlistItems.Add(item.itemId, item.stackCount);
                        }
                    }
                    foreach (int itemid in rewardlistItems.Keys.ToList())
                        rewardlistItems[itemid] = (int)Math.Floor(rewardlistItems[itemid] * efficiency);
                    foreach (var item in rewardlistItems)
                    {
                        int index = rewardToAdd.FindIndex(x => x.itemId == item.Key);
                        if (index != -1) // has item already in list so just add up the count
                            rewardToAdd[index].stackCount += item.Value;
                        else
                            rewardToAdd.Add(new ItemInfo { itemId = (ushort)item.Key, stackCount = item.Value });
                    }
                }
                LootLink lootLink = LootRepo.GetLootLink(target.lootlinkid);
                if (lootLink != null)  // add any item from loot link
                {
                    Dictionary<int, int> lootItems = new Dictionary<int, int>();
                    LootRules.GenerateLootItems(lootLink.gids, lootItems, currencyToAdd);
                    // modify loot item count by efficiency
                    foreach (int itemid in lootItems.Keys.ToList())
                        lootItems[itemid] = (int)Math.Floor(lootItems[itemid] * efficiency);
                    List<ItemInfo> itemInfoList = LootRules.GetItemInfoListToAdd(lootItems, true); // check for limited items
                    for (int i = 0; i < itemInfoList.Count; i++)
                    {
                        int index = rewardToAdd.FindIndex(x => x.itemId == itemInfoList[i].itemId);
                        if (index != -1) // has item already in list so just add up the count
                            rewardToAdd[index].stackCount += itemInfoList[i].stackCount;
                        else
                            rewardToAdd.Add(itemInfoList[i]);
                    }
                }
            }

            map.Rewards = new ExploreReward(rewardToAdd, currencyToAdd);
        }

        public void ClaimExplorationReward(int mapId)
        {
            ExploreMapData map = GetExploringMap(mapId);
            if (map == null)
                return;

            if (map.Completed)
            {
                // try give rewards
                InvRetval retValue = player.Slot.mInventory.AddItemsToInventory(map.Rewards.items, true, "Exploration");
                if (retValue.retCode == InvReturnCode.AddSuccess)
                {
                    // add currency
                    foreach (var currency in map.Rewards.currency)
                        player.AddCurrency(currency.Key, currency.Value, "Exploration");

                    List<Hero> heroesList = new List<Hero>();
                    int count = map.HeroIdList.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Hero hero = GetHero(map.HeroIdList[i]);
                        if (hero == null)  // should not happen
                            continue;
                        hero.IsAway = false;  // release heroes
                        heroes[hero.SlotIdx] = hero.ToString();
                    }

                    if (!HasExploredMap(mapId))  // add to explored maps
                    {
                        exploredMaps.Add(mapId);
                        Explored = JsonConvertDefaultSetting.SerializeObject(exploredMaps);
                    }

                    explorationsDict.Remove(mapId); // remove from in progress maps
                    Explorations = JsonConvertDefaultSetting.SerializeObject(explorationsDict);
                }
                else
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_BagInventoryFull", "", false, peer);
            }
        }
        #endregion Exploration

#if DEBUG

        public void RemoveHero(int heroId)
        {
            if (heroId == 0)
            {
                UnSummonHeroEntity();
                SummonedHeroId = 0;
                mHeroesDict.Clear();
                for (int i = 0; i < heroes.Count; i++)
                    heroes[i] = null;
            }
            else if (IsHeroUnlocked(heroId))
            {
                if (SummonedHeroId == heroId)
                {
                    UnSummonHeroEntity();
                    SummonedHeroId = 0;
                }
                int idx = mHeroesDict[heroId].SlotIdx;
                mHeroesDict.Remove(heroId);
                heroes[idx] = null;
            }
        }

        public void ResetExplorations()
        {
            explorationsDict.Clear();
            exploredMaps.Clear();
            Explorations = "";
            Explored = "";
        }

#endif
    }
}