using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public HeroStatsServer() : base()
        {
            fulfilledBonds = new Dictionary<int, HeroBondSideEffect>();
            summonPassiveSEs = new List<IPassiveSideEffect>();
            heroPassiveSEs = new Dictionary<int, List<IPassiveSideEffect>>();
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

            SummonedHeroId = GetHero(invData.SummonedHero) != null ? invData.SummonedHero : 0;

            ApplyHeroBondSEs();

            for (int i = 0; i < invData.InProgressMaps.Count; i++)
            {
                ExploreMapData map = invData.InProgressMaps[i];
                ExplorationMapJson mapData = HeroRepo.GetExplorationMapById(map.MapId);
                if (mapData != null)
                {
                    map.MapData = mapData;
                    explorationsDict.Add(map.MapId, map);
                }
            }
            if (explorationsDict.Count > 0)
                Explorations = JsonConvertDefaultSetting.SerializeObject(explorationsDict);

            Explored = invData.ExploredMaps;
            ParseExploredString();
        }

        public void PostPlayerSpawnSummonHero()
        {
            Hero hero = GetHero(SummonedHeroId);
            if (hero != null)
            {
                bool canSpawn = true;
                // if play is in party, only can spawn hero if the hero is in the party
                if (player.IsInParty() && player.PartyStats.GetHeroOwnedByMember(player.Name) == null)
                    canSpawn = false;

                if (canSpawn)
                    SpawnHeroEntity(hero, false);
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

        private bool DeductItem(int itemId, int itemCount)
        {
            //itemCount = 0;
            InvRetval retval = peer.mInventory.DeductItem((ushort)itemId, (ushort)itemCount, "Hero");
            return retval.retCode == InvReturnCode.UseSuccess;
        }

        private bool DeductHeroGiftItem(string itemStr)
        {
            int bindItemId, unbindItemId;
            string[] itemIds = itemStr.Split(';');
            if (itemIds.Length > 0 && int.TryParse(itemIds[0], out bindItemId))
            {
                if (DeductItem(bindItemId, 1))
                    return true;
                else
                {
                    if (itemIds.Length > 1 && int.TryParse(itemIds[1], out unbindItemId))
                    {
                        if (DeductItem(unbindItemId, 1))
                            return true;
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

            if (!hero.CanAddSkillPoint())
            {
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_SkillPointsMaxed", "", false, peer);
                return;
            }

            if (DeductItem(hero.HeroJson.upgradeitemid, hero.HeroJson.upgradeitemcount))
            {
                hero.SkillPoints++;
                heroes[hero.SlotIdx] = hero.ToString();

                PlayerCombatStats combatStats = (PlayerCombatStats)player.CombatStats;
                combatStats.SuppressComputeAll = true;
                bool applied = UpdateHeroBonds(heroId);
                combatStats.SuppressComputeAll = false;
                if (applied)
                    combatStats.ComputeAll();
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
            if (hero != null && hero.Interest != interest)
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
                            peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_InterestCannotChange", "", false, peer);
                            return;
                        }

                        if (DeductHeroGiftItem(hero.HeroJson.randomitemid))
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

                        if (DeductHeroGiftItem(interestData.assigneditemid))
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

        public void AddHeroTrust(int heroId, int itemId)
        {
            Hero hero = GetHero(heroId);
            HeroItem giftItem = GameRepo.ItemFactory.GetInventoryItem(itemId) as HeroItem;
            if (hero != null && giftItem != null)
            {
                if (hero.TrustLevel >= HeroRepo.MAX_TRUST_LEVEL)
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_MaxTrustLevel", "", false, peer);
                    return;
                }

                // check item used is a gift and is applicable for this hero
                string[] applicableHeros = giftItem.HeroItemJson.heroid.Split(';');
                if (giftItem.HeroItemJson.heroitemtype != HeroItemType.Gift || !applicableHeros.Contains(heroId.ToString()))
                    return;

                if (DeductItem(itemId, 1))
                {
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
                        }
                    }

                    if (hero.TrustLevel == HeroRepo.MAX_TRUST_LEVEL)
                        hero.TrustExp = 0;
                    heroes[hero.SlotIdx] = hero.ToString();
                }
                else
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_GiftNotEnough", "", false, peer);
            }
        }

        public void ChangeHeroModelTier(int heroId, int tier)
        {
            Hero hero = GetHero(heroId);
            if (hero != null && hero.ModelTier != tier && hero.IsModelTierUnlocked(tier))
            {
                hero.ModelTier = tier;
                heroes[hero.SlotIdx] = hero.ToString();

                if (IsHeroSummoned(heroId))  // update model if hero is summoned
                    SummonHero(heroId);
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
                    int skinItemId;
                    if (int.TryParse(skinitems[i], out skinItemId))
                    {
                        if (skinItemId == itemId && !hero.UnlockedSkinItems.Contains(skinItemId))
                        {
                            hero.UnlockedSkinItems.Add(skinItemId);
                            heroes[hero.SlotIdx] = hero.ToString();
                            break;
                        }
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

        public void SummonHero(int heroId)
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
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("player level need " + hero.HeroJson.summonlevel, "", false, peer);
                    //peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_SummonLevelNotReached",
                    //        string.Format("level;{0}", hero.HeroData.summonlevel), false, peer);
                    return;
                }

                if (hero.IsAway)
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_HeroAwayExploring", "", false, peer);
                    return;
                }

                if (summonedHeroEntity == null || summonedHeroEntity.HeroSynStats.HeroId != heroId || summonedHeroEntity.HeroSynStats.ModelTier != hero.ModelTier)
                {
                    bool canSummon = true;
                    // if player has party, so try to add hero to party
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
                        else // don't have owned hero in party and no space to add hero so cannot summon
                            canSummon = false;
                    }

                    if (canSummon)
                    {
                        SpawnHeroEntity(hero, true);
                        if (SummonedHeroId != heroId)
                            SummonedHeroId = heroId;  // for record
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
            combatStats.SuppressComputeAll = true;
            bool applied1 = RemoveHeroPassiveSEs(hero.HeroId);  // remove any previous se
            bool applied2 = ApplyHeroPassiveSEs(hero, false);  // apply se from this hero
            combatStats.SuppressComputeAll = false;
            if (applied1 || applied2)
                combatStats.ComputeAll();
        }

        private void UpdateSummonPassiveSEs()
        {
            PlayerCombatStats combatStats = (PlayerCombatStats)player.CombatStats;
            combatStats.SuppressComputeAll = true;
            bool applied1 = RemoveSummonPassiveSEs();  // remove any previous se
            bool applied2 = ApplyHeroPassiveSEs(summonedHeroEntity.Hero, true);  // apply se from this hero
            combatStats.SuppressComputeAll = false;
            if (applied1 || applied2)
                combatStats.ComputeAll();
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
                    SideEffect se = SideEffectFactory.CreateSideEffect(passiveSEs[i]);
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
                            GameUtils.DebugWriteLine("Remove se from groupId: " + groupId);
                        }

                        if (highestLevelId > 0 && AddBondSEs(groupId, highestLevelData)) // add new passives
                            needComputeAll = true;
                        GameUtils.DebugWriteLine("Update groupId/highestLevelId: " + groupId + "/" + highestLevelId);
                    }
                }
                else if (highestLevelData != null)  // new bond, add side effects
                {
                    if (AddBondSEs(groupId, highestLevelData))
                        needComputeAll = true;
                    GameUtils.DebugWriteLine("Add groupId: " + groupId);
                }
            }
            return needComputeAll;
        }

        private bool AddBondSEs(int groupId, HeroBondJson bondData)
        {
            bool needCompute = false;
            List<IPassiveSideEffect> passiveSEs = new List<IPassiveSideEffect>();
            var seGroup = SideEffectRepo.GetSEGroup(bondData.sideeffectgrp);
            if (seGroup != null)
            {
                foreach (SideEffectJson seJson in seGroup.sideeffects.Values)
                {
                    SideEffect se = SideEffectFactory.CreateSideEffect(seJson, true);
                    IPassiveSideEffect pse = se as IPassiveSideEffect;
                    if (pse != null)
                    {
                        pse.AddPassive(player);
                        passiveSEs.Add(pse);
                        needCompute = true;
                        GameUtils.DebugWriteLine("Add se: " + se.mSideeffectData.id);
                    }
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

            if (IsExploringMap(mapId))  // check whether map is already currently in progross
                return;
            if (!mapData.repeatable && HasExploredMap(mapId))  // check one time map cannot be explored again
                return;
            if (mapData.prevmapid > 0 && !HasExploredMap(mapData.prevmapid)) // check has completed previous map
                return;
            if (heroIds.Count > mapData.maxherocount)  // check sent heroes not more than max allowed
                return;

            for (int i = 0; i < heroIds.Count; i++) // check heros meet map requirements
            {
                Hero hero = GetHero(heroIds[i]);
                if (hero == null)
                    return;
                if (hero.Level < mapData.reqherolevel || hero.TrustLevel < mapData.reqherotrust)
                {
                    GameUtils.DebugWriteLine("hero " + hero.HeroId + " do not meet requirements");
                    return;
                }
            }

            // todo: check player adventure level

            if (DeductItem(mapData.reqitemid, mapData.reqitemcount))
            {
                DateTime endTime = DateTime.Now.AddMinutes(mapData.completetime);
                ExploreMapData map = new ExploreMapData(mapId, targetId, heroIds, endTime, mapData);
                explorationsDict.Add(mapId, map);
                Explorations = JsonConvertDefaultSetting.SerializeObject(explorationsDict);

                // set used heroes as away
                for (int i = 0; i < heroIds.Count; i++) // check heros meet map requirements
                {
                    Hero hero = GetHero(heroIds[i]);
                    if (hero == null)  // should not happen
                        continue;
                    hero.IsAway = true;
                    heroes[hero.SlotIdx] = hero.ToString();
                }

                // todo: to add combat time
            }
            else
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_hero_ItemNotEnough", "", false, peer);
        }

        public void ClaimExplorationReward(int mapId)
        {
            ExplorationMapJson mapData = HeroRepo.GetExplorationMapById(mapId);
            if (mapData == null)
                return;
            ExploreMapData map = GetExploringMap(mapId);
            if (map == null)
                return;

            if (map.IsCompleted())
            {
                float efficiency = mapData.baseefficiency * 0.01f;
                List<Hero> heroesList = new List<Hero>();
                for (int i = 0; i < map.HeroIdList.Count; i++)
                {
                    Hero hero = GetHero(map.HeroIdList[i]);
                    if (hero == null)  // should not happen
                        continue;
                    hero.IsAway = false; // release heros
                    heroes[hero.SlotIdx] = hero.ToString();
                    efficiency += map.GetHeroEfficiency(hero);
                    heroesList.Add(hero);
                }

                if (!HasExploredMap(mapId))  // add to explored maps
                {
                    exploredMaps.Add(mapId);
                    Explored = JsonConvertDefaultSetting.SerializeObject(exploredMaps);
                }

                explorationsDict.Remove(mapId); // remove from in progress maps
                Explorations = JsonConvertDefaultSetting.SerializeObject(explorationsDict);

                int chestCount = map.GetFulfilledChestCount(heroesList);
                GameUtils.DebugWriteLine("chest count: " + chestCount);

                GameUtils.DebugWriteLine("efficiency: " + efficiency);

                // todo: check reward group and target rewards
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