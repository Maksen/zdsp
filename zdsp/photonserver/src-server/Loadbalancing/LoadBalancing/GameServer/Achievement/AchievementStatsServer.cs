using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.Entities;
using Zealot.Server.SideEffects;

namespace Photon.LoadBalancing.GameServer
{
    public class AchievementStatsServer : AchievementStats
    {
        private GameClientPeer peer;
        private Player player;
        private List<IPassiveSideEffect> levelPassiveSEs;
        private Dictionary<int, List<IPassiveSideEffect>> storedCollectionPassives;
        private Dictionary<AchievementObjectiveType, HashSet<string>> completedTargets;
        private Dictionary<int, List<CollectionElement>> collectionElementsByIdx;  // for client sync
        private Dictionary<int, List<AchievementElement>> achievementElementsByIdx; // for client sync

        private StringBuilder sb = new StringBuilder();

        public AchievementStatsServer()
        {
            levelPassiveSEs = new List<IPassiveSideEffect>();
            storedCollectionPassives = new Dictionary<int, List<IPassiveSideEffect>>();
            completedTargets = new Dictionary<AchievementObjectiveType, HashSet<string>>();

            collectionElementsByIdx = new Dictionary<int, List<CollectionElement>>();
            int length = Enum.GetNames(typeof(CollectionType)).Length;
            for (int i = 0; i < length; ++i)
                collectionElementsByIdx.Add(i, new List<CollectionElement>());

            achievementElementsByIdx = new Dictionary<int, List<AchievementElement>>();
            length = Enum.GetNames(typeof(AchievementType)).Length;
            for (int i = 0; i < length; ++i)
                achievementElementsByIdx.Add(i, new List<AchievementElement>());
        }

        public void Init(Player _player, GameClientPeer _peer, AchievementInvData invData)
        {
            player = _player;
            peer = _peer;

            player.PlayerSynStats.AchievementLevel = invData.AchievementLevel <= 0 ? 1 : invData.AchievementLevel;
            player.SecondaryStats.AchievementExp = invData.AchievementExp;

            ParseCollections(invData.Collections);
            ParseAchievements(invData.Achievements);
            RewardClaims = invData.RewardClaims;
            ParseRewardClaims();
            LatestCollections = invData.LatestCollections;
            LatestAchievements = invData.LatestAchievements;
            ParseLatestRecords(latestCollectionsList, LatestCollections);
            ParseLatestRecords(latestAchievementList, LatestAchievements);
            ParseCompleteTargets(invData.CompletedTargets);

            AchievementLevel levelInfo = AchievementRepo.GetAchievementLevelInfo(player.PlayerSynStats.AchievementLevel);
            if (levelInfo != null)
                UpdateLevelPassiveSEs(levelInfo);
        }

        public void SaveToInventory(AchievementInvData invData)
        {
            invData.AchievementLevel = player.PlayerSynStats.AchievementLevel;
            invData.AchievementExp = player.SecondaryStats.AchievementExp;
            invData.Collections = CollectionsSerializeForDB();
            invData.Achievements = AchievementsSerializeForDB();
            invData.RewardClaims = RewardClaims;
            invData.LatestCollections = LatestCollections;
            invData.LatestAchievements = LatestAchievements;
            invData.CompletedTargets = CompletedTargetsSerializeForDB();
        }

        #region ParseFromString
        private void ParseCollections(string value)
        {
            collectionsDict.Clear();
            if (!string.IsNullOrEmpty(value))
            {
                string[] colArray = value.Split('|');
                for (int i = 0; i < colArray.Length; ++i)
                {
                    string[] colData = colArray[i].Split(';');
                    int id = int.Parse(colData[0]);
                    DateTime date = DateTime.ParseExact(colData[1], "yyyy/MM/dd", CultureInfo.InvariantCulture);
                    bool claimed = int.Parse(colData[2]) == 1;
                    string photodesc = "";
                    bool stored = false;
                    if (colData.Length > 3)
                    {
                        int isStored;
                        if (int.TryParse(colData[3], out isStored) && isStored == 1)
                            stored = true;
                        else
                            photodesc = colData[3];
                    }
                    CollectionObjective obj = AchievementRepo.GetCollectionObjectiveById(id);
                    if (obj != null)
                    {
                        int idx = (int)obj.type;
                        CollectionElement elem = new CollectionElement(id, date, claimed, photodesc, stored, idx);
                        collectionsDict.Add(id, elem);
                        collectionElementsByIdx[idx].Add(elem);

                        // apply any reward se if claimed
                        if (obj.rewardType == AchievementRewardType.SideEffect && obj.rewardId > 0 && claimed)
                            ApplyPassiveSE(obj.rewardId);
                        // apply se if collection is stored
                        if ((obj.type == CollectionType.Fashion || obj.type == CollectionType.Relic) && elem.Stored)
                            ApplyStoredCollectionPassives(obj);
                    }
                }

                foreach (var elem in collectionElementsByIdx)
                    SyncCollectionByIndex(elem.Key);
            }
        }

        private void ParseAchievements(string value)
        {
            achievementsDict.Clear();
            if (!string.IsNullOrEmpty(value))
            {
                string[] achArray = value.Split('|');
                for (int i = 0; i < achArray.Length; ++i)
                {
                    string[] achData = achArray[i].Split(';');
                    int id = int.Parse(achData[0]);
                    int count = int.Parse(achData[1]);
                    bool claimed = int.Parse(achData[2]) == 1;
                    AchievementObjective obj = AchievementRepo.GetAchievementObjectiveById(id);
                    if (obj != null)
                    {
                        int idx = (int)obj.mainType;
                        AchievementElement elem = new AchievementElement(id, count, obj.completeCount, claimed, idx);
                        achievementsDict.Add(id, elem);
                        achievementElementsByIdx[idx].Add(elem);

                        // apply any reward se if claimed
                        if (obj.rewardType == AchievementRewardType.SideEffect && obj.rewardId > 0 && claimed)
                            ApplyPassiveSE(obj.rewardId);
                    }
                }

                foreach (var elem in achievementElementsByIdx)
                    SyncAchievementByIndex(elem.Key);
            }
        }

        private void ParseRewardClaims()
        {
            claimsList.Clear();
            if (!string.IsNullOrEmpty(RewardClaims))
            {
                string[] rcArray = RewardClaims.Split('|');
                for (int i = 0; i < rcArray.Length; ++i)
                {
                    string[] rcData = rcArray[i].Split(';');
                    AchievementKind type = (AchievementKind)int.Parse(rcData[0]);
                    int id = int.Parse(rcData[1]);
                    AchievementRewardClaim claim = new AchievementRewardClaim(type, id);
                    claimsList.Add(claim);
                }
            }
        }

        private void ParseLatestRecords(List<AchievementRecord> list, string value)
        {
            list.Clear();
            if (!string.IsNullOrEmpty(value))
            {
                string[] rArray = value.Split('|');
                for (int i = 0; i < rArray.Length; ++i)
                {
                    string[] rData = rArray[i].Split(';');
                    int id = int.Parse(rData[0]);
                    DateTime date = DateTime.ParseExact(rData[1], "yyyy/MM/dd", CultureInfo.InvariantCulture);
                    AchievementRecord record = new AchievementRecord(id, date);
                    list.Add(record);
                }
            }
        }

        private void ParseCompleteTargets(string value)
        {
            completedTargets.Clear();
            if (!string.IsNullOrEmpty(value))
            {
                string[] strArray = value.Split('|');
                for (int i = 0; i < strArray.Length; ++i)
                {
                    string[] rData = strArray[i].Split(';');
                    AchievementObjectiveType objType = (AchievementObjectiveType)int.Parse(rData[0]);
                    var set = JsonConvertDefaultSetting.DeserializeObject<HashSet<string>>(rData[1]);
                    completedTargets.Add(objType, set);
                }
            }
        }
        #endregion ParseFromString

        #region ConvertToString
        private void SyncCollectionByIndex(int idx)
        {
            List<CollectionElement> list;
            if (collectionElementsByIdx.TryGetValue(idx, out list))
            {
                for (int i = 0; i < list.Count; ++i)
                    sb.AppendFormat("{0}|", list[i].ToClientString());
                Collections[idx] = sb.ToString().TrimEnd('|');
                sb.Clear();
            }
        }

        private void SyncAchievementByIndex(int idx)
        {
            List<AchievementElement> list;
            if (achievementElementsByIdx.TryGetValue(idx, out list))
            {
                for (int i = 0; i < list.Count; ++i)
                    sb.AppendFormat("{0}|", list[i].ToClientString());
                Achievements[idx] = sb.ToString().TrimEnd('|');
                sb.Clear();
            }
        }

        public string CollectionsSerializeForDB()
        {
            foreach (var item in collectionsDict)
            {
                sb.AppendFormat("{0};{1};{2}", item.Key, item.Value.CollectDate.ToString("yyyy/MM/dd"), item.Value.Claimed ? 1 : 0);
                if (!string.IsNullOrEmpty(item.Value.PhotoDesc))
                    sb.AppendFormat(";{0}", item.Value.PhotoDesc);
                else if (item.Value.Stored)
                    sb.AppendFormat(";1");
                sb.Append("|");
            }
            string collectionString = sb.ToString().TrimEnd('|');
            sb.Clear();
            return collectionString;
        }

        public string AchievementsSerializeForDB()
        {
            foreach (var item in achievementsDict)
                sb.AppendFormat("{0};{1};{2}|", item.Key, item.Value.Count, item.Value.Claimed ? 1 : 0);
            string achString = sb.ToString().TrimEnd('|');
            sb.Clear();
            return achString;
        }

        private void UpdateRewardClaimsString()
        {
            for (int i = 0; i < claimsList.Count; ++i)
                sb.AppendFormat("{0};{1}|", (int)claimsList[i].ClaimType, claimsList[i].Id);
            RewardClaims = sb.ToString().TrimEnd('|');
            sb.Clear();
        }

        private void UpdateRecordsString(AchievementKind type)
        {
            if (type == AchievementKind.Collection)
            {
                for (int i = 0; i < latestCollectionsList.Count; ++i)
                    sb.AppendFormat("{0};{1}|", latestCollectionsList[i].Id, latestCollectionsList[i].CompleteDate.ToString("yyyy/MM/dd"));
                LatestCollections = sb.ToString().TrimEnd('|');
            }
            else
            {
                for (int i = 0; i < latestAchievementList.Count; ++i)
                    sb.AppendFormat("{0};{1}|", latestAchievementList[i].Id, latestAchievementList[i].CompleteDate.ToString("yyyy/MM/dd"));
                LatestAchievements = sb.ToString().TrimEnd('|');
            }
            sb.Clear();
        }

        private string CompletedTargetsSerializeForDB()
        {
            foreach (var item in completedTargets)
            {
                string hashset = JsonConvertDefaultSetting.SerializeObject(item.Value);
                sb.AppendFormat("{0};{1}|", (int)item.Key, hashset);
            }
            string targetString = sb.ToString().TrimEnd('|');
            sb.Clear();
            return targetString;
        }
        #endregion ConvertToString

        public void UpdateCollection(CollectionType objType, int target)
        {
            CollectionObjective obj = AchievementRepo.GetCollectionObjectiveByKey(objType, target);
            if (obj != null && !collectionsDict.ContainsKey(obj.id))
            {
                string info = "";
                if (objType == CollectionType.Photo)
                {
                    if (player.IsInParty())
                        info = AchievementRepo.GetRandomPhotoDescription(player.PartyStats.MemberCount(false));
                    else
                        info = AchievementRepo.GetRandomPhotoDescription(1);
                    // to insert member names
                }
                DateTime now = DateTime.Now;
                int idx = (int)objType;
                CollectionElement elem = new CollectionElement(obj.id, now, false, info, false, idx);
                collectionsDict.Add(obj.id, elem);
                collectionElementsByIdx[idx].Add(elem);
                SyncCollectionByIndex(idx);

                AddToRewardClaims(AchievementKind.Collection, obj.id);
                UpdateRewardClaimsString();
                AddToLatestRecords(AchievementKind.Collection, obj.id, now);
                UpdateRecordsString(AchievementKind.Collection);
            }
        }

        public void StoreCollectionItem(int id, bool isStore)
        {
            CollectionElement elem = GetCollectionById(id);
            if (elem == null)
                return;
            if ((elem.Stored && isStore) || (!elem.Stored && !isStore))
                return;

            CollectionObjective obj = AchievementRepo.GetCollectionObjectiveById(id);
            if (obj != null)
            {
                if (obj.type != CollectionType.Fashion && obj.type != CollectionType.Relic)
                    return;

                PlayerCombatStats combatStats = (PlayerCombatStats)player.CombatStats;
                if (isStore)
                {
                    InvRetval retval = peer.mInventory.DeductItems((ushort)obj.targetId, 1, "Achievement");
                    if (retval.retCode != InvReturnCode.UseSuccess)
                    {
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_ach_ItemNotFound", "", false, peer);
                        return;
                    }
                    else
                    {
                        elem.Stored = true;
                        combatStats.SuppressComputeAll = true;
                        ApplyStoredCollectionPassives(obj);
                    }
                }
                else
                {
                    InvRetval retval = peer.mInventory.AddItemsToInventory((ushort)obj.targetId, 1, false, "Achievement");
                    if (retval.retCode != InvReturnCode.AddSuccess)
                    {
                        peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_BagInventoryFull", "", false, peer);
                        return;
                    }
                    else
                    {
                        elem.Stored = false;
                        combatStats.SuppressComputeAll = true;
                        RemoveStoredCollectionPassives(obj.id);
                    }
                }

                SyncCollectionByIndex((int)obj.type);

                combatStats.SuppressComputeAll = false;
                combatStats.ComputeAll();
            }
        }

        public void UpdateAchievement(AchievementObjectiveType objType, string target = "-1", bool isNonTarget = true,
            int count = 1, bool increment = true, bool debug = false)
        {
            if (count <= 0 && !debug)
                return;

            if (isNonTarget && target != "-1" && !CanUpdateAchievement(objType, target))
                return;

            string objTarget = "-1";
            if (target != "-1" && !isNonTarget)
                objTarget = target;

            bool hasNewlyCompleted = false;
            int dirtySlot = -1;
            List<AchievementObjective> achList = AchievementRepo.GetAchievementObjectivesByKey(objType, objTarget);
            if (achList != null)
            {
                int listCount = achList.Count;
                for (int i = 0; i < listCount; ++i)
                {
                    AchievementObjective obj = achList[i];
                    AchievementElement elem = GetAchievementById(obj.id);
                    int idx = (int)obj.mainType;
                    bool isAlreadyCompleted = false;
                    
                    if (elem != null) // existing achievement
                    {
                        isAlreadyCompleted = elem.IsCompleted();
                        if (isAlreadyCompleted)
                            continue;
                        if (!increment && count <= elem.Count)
                            continue;
                        elem.UpdateCount(count, increment);
                        dirtySlot = idx;  // every objective in group should have same slot
                    }
                    else  // new achievement
                    {
                        elem = new AchievementElement(obj.id, count, obj.completeCount, false, idx);
                        achievementsDict.Add(obj.id, elem);
                        achievementElementsByIdx[idx].Add(elem);
                        dirtySlot = idx;  // every objective in group should have same slot
                    }

                    if (!isAlreadyCompleted && elem.IsCompleted())  // just completed this achievement so can claim
                    {
                        AddToRewardClaims(AchievementKind.Achievement, obj.id);
                        AddToLatestRecords(AchievementKind.Achievement, obj.id, DateTime.Now);
                        hasNewlyCompleted = true;
                    }
                }

                // certain objectives need record completed targets
                if (isNonTarget && target != "-1")
                {
                    switch (objType)
                    {
                        case AchievementObjectiveType.MainQuest:
                        case AchievementObjectiveType.SubQuest:
                        case AchievementObjectiveType.DestinyQuest:
                        case AchievementObjectiveType.GuildQuest:
                        case AchievementObjectiveType.Scene:
                        case AchievementObjectiveType.Photography:
                        case AchievementObjectiveType.NPCInteract:
                            if (!completedTargets.ContainsKey(objType))
                                completedTargets.Add(objType, new HashSet<string>());
                            completedTargets[objType].Add(target);
                            break;
                    }
                }
            }

            if (dirtySlot != -1)  // rebuild dirty slot in collectionhandler
                SyncAchievementByIndex(dirtySlot);

            if (hasNewlyCompleted) // has completed new ones so need update string
            {
                UpdateRewardClaimsString();
                UpdateRecordsString(AchievementKind.Achievement);
            }
        }

        private bool CanUpdateAchievement(AchievementObjectiveType objType, string target)
        {
            HashSet<string> completedSet;
            completedTargets.TryGetValue(objType, out completedSet);
            return completedSet == null || !completedSet.Contains(target);
        }

        public void PostSpawnCheckAchievements()
        {
            if (peer.mFirstLogin)
            {
                /*** Collections ***/
                // check Fashion collection
                var equipItemsInBag = peer.mInventory.GetItemsByItemType(ItemType.Equipment);
                foreach (var item in equipItemsInBag) // check in bag
                {
                    Equipment equipItem = item.Value as Equipment;
                    if (equipItem.EquipmentJson.fashionsuit)
                        UpdateCollection(CollectionType.Fashion, equipItem.ItemID);
                }
                for (int index = 0; index < (int)FashionSlot.MAXSLOTS; ++index) // check currently equipped fashin
                {
                    Equipment equipItem = peer.CharacterData.EquipmentInventory.FashionSlots[index];
                    if (equipItem != null)
                        UpdateCollection(CollectionType.Fashion, equipItem.ItemID);
                }

                // check Relic collection
                var relicItemsInBag = peer.mInventory.GetItemsByItemType(ItemType.Relic);
                foreach (var equipItem in relicItemsInBag) // check in bag
                    UpdateCollection(CollectionType.Relic, equipItem.Value.ItemID);

                /*** Achievements ***/
                // Collect item
                List<string> targetList = AchievementRepo.GetTargetsByAchievementObjType(AchievementObjectiveType.CollectItem);
                for (int i = 0; i < targetList.Count; ++i)
                {
                    ushort itemId;
                    if (ushort.TryParse(targetList[i], out itemId))
                    {
                        int stackCount = peer.mInventory.GetItemStackCountByItemId(itemId);
                        UpdateAchievement(AchievementObjectiveType.CollectItem, targetList[i], false, stackCount, false);
                    }
                }

                // No. of equipments with target upgrade level
                targetList = AchievementRepo.GetTargetsByAchievementObjType(AchievementObjectiveType.RefineEquipmentLV);
                if (targetList.Count > 0)
                {
                    for (int i = 0; i < targetList.Count; ++i)
                    {
                        int upgradeLv;
                        int count = 0;
                        if (int.TryParse(targetList[i], out upgradeLv))
                        {
                            // check currently equipped
                            for (int index = 0; index < (int)EquipmentSlot.MAXSLOTS; ++index)
                            {
                                Equipment equipment = peer.CharacterData.EquipmentInventory.Slots[index];
                                if (equipment != null && equipment.UpgradeLevel >= upgradeLv)
                                    count++;
                            }
                            // check those in bag
                            foreach (var item in equipItemsInBag)
                            {
                                var equip = item.Value as Equipment;
                                if (equip.UpgradeLevel >= upgradeLv)
                                    count++;
                            }
                            UpdateAchievement(AchievementObjectiveType.RefineEquipmentLV, targetList[i], false, count, false);
                        }
                    }
                }

                // Achievement level
                UpdateAchievement(AchievementObjectiveType.AchievementLevel, "-1", true, player.PlayerSynStats.AchievementLevel, false);

                // Player level
                UpdateAchievement(AchievementObjectiveType.Level, "-1", true, player.PlayerSynStats.Level, false);

                // Hero rarity count
                int rarityCount = Enum.GetValues(typeof(HeroRarity)).Length;
                for (int i = 0; i < rarityCount; ++i)
                {
                    UpdateAchievement(AchievementObjectiveType.HeroNumber, i.ToString(), false,
                        player.HeroStats.GetHeroCountByRarity((HeroRarity)i), false);
                }

                // Hero related
                var ownedHeroes = player.HeroStats.GetHeroesDict();
                int totalHeroLevels = 0;
                foreach (var hero in ownedHeroes.Values)
                {
                    UpdateCollection(CollectionType.Hero, hero.HeroId);
                    string heroId = hero.HeroId.ToString();
                    totalHeroLevels += hero.Level;
                    UpdateAchievement(AchievementObjectiveType.HeroLevel, heroId, false, hero.Level, false);
                    UpdateAchievement(AchievementObjectiveType.HeroTrust, heroId, false, hero.TrustLevel, false);
                    UpdateAchievement(AchievementObjectiveType.HeroSkill, heroId, false, hero.GetTotalSkillPoints(), false);
                }
                UpdateAchievement(AchievementObjectiveType.HeroGrowth, "-1", true, totalHeroLevels - ownedHeroes.Count, false);
            }

            // Visit level
            string levelId = player.mInstance.mCurrentLevelID.ToString();
            UpdateAchievement(AchievementObjectiveType.Scene, levelId, true);
            UpdateAchievement(AchievementObjectiveType.Scene, levelId, false);
        }

        private void AddToRewardClaims(AchievementKind type, int id)
        {
            AchievementRewardClaim newClaim = new AchievementRewardClaim(type, id);
            claimsList.Add(newClaim);
        }

        private void AddToLatestRecords(AchievementKind type, int id, DateTime date)
        {
            AchievementRecord newRecord = new AchievementRecord(id, date);
            if (type == AchievementKind.Collection)
            {
                latestCollectionsList.Add(newRecord);
                if (latestCollectionsList.Count > AchievementData.MAX_RECORDS) // remove oldest record if more then max limit
                    latestCollectionsList.RemoveAt(0);
            }
            else
            {
                latestAchievementList.Add(newRecord);
                if (latestAchievementList.Count > AchievementData.MAX_RECORDS) // remove oldest record if more then max limit
                    latestAchievementList.RemoveAt(0);
            }
        }

        #region Rewards
        public void ClaimReward(AchievementKind type, int id)
        {
            AchievementRewardClaim claim = claimsList.Find(x => x.ClaimType == type && x.Id == id);
            if (claim == null)
                return;

            BaseAchievementElement elem = GetElementByTypeAndId(type, id);
            if (elem != null && !elem.Claimed && elem.IsCompleted())
            {
                BaseAchievementObjective obj = AchievementRepo.GetObjectiveByTypeAndId(type, id);
                if (obj == null)
                    return;
                if (!GiveCompletedObjectiveReward(obj))
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_BagInventoryFull", "", false, peer);
                    return;
                }
                player.AddAchievementExp(obj.exp);
                elem.Claimed = true;

                // sync to client
                if (type == AchievementKind.Collection)
                    SyncCollectionByIndex(elem.SlotIdx);
                else
                    SyncAchievementByIndex(elem.SlotIdx);

                // function reward
                if (type == AchievementKind.Achievement)
                {
                    AchievementObjective achObj = obj as AchievementObjective;
                    GiveFunctionReward(achObj.rewardFunction, achObj.rewardFunctionValue);
                }
            }

            claimsList.Remove(claim);
            UpdateRewardClaimsString();
        }

        public void ClaimAllRewards()
        {
            PlayerCombatStats combatStats = (PlayerCombatStats)player.CombatStats;
            combatStats.SuppressComputeAll = true;
            bool inventoryFull = false;
            List<int> indexToRemove = new List<int>();
            HashSet<int> dirtyColSlot = new HashSet<int>();
            HashSet<int> dirtyAchSlot = new HashSet<int>();

            for (int i = 0; i < claimsList.Count; ++i)
            {
                AchievementRewardClaim claim = claimsList[i];
                BaseAchievementElement elem = GetElementByTypeAndId(claim.ClaimType, claim.Id);
                if (elem != null && !elem.Claimed && elem.IsCompleted())
                {
                    BaseAchievementObjective obj = AchievementRepo.GetObjectiveByTypeAndId(claim.ClaimType, claim.Id);
                    if (obj == null)
                        continue;
                    if (!GiveCompletedObjectiveReward(obj))
                    {
                        inventoryFull = true;
                        continue;
                    }
                    player.AddAchievementExp(obj.exp);
                    elem.Claimed = true;
                    indexToRemove.Add(i);
                    sb.AppendFormat("{0};{1}|", (int)claim.ClaimType, claim.Id);
                    if (claim.ClaimType == AchievementKind.Collection)
                        dirtyColSlot.Add(elem.SlotIdx);
                    else
                    {
                        dirtyAchSlot.Add(elem.SlotIdx);
                        // function reward 
                        AchievementObjective achObj = obj as AchievementObjective;
                        GiveFunctionReward(achObj.rewardFunction, achObj.rewardFunctionValue);
                    }
                }
            }

            int claimedCount = indexToRemove.Count;
            if (claimedCount > 0)
            {
                // send to client claimed rewards
                peer.ZRPC.CombatRPC.Ret_ClaimAllAchievementRewards(sb.ToString().TrimEnd('|'), peer);
                sb.Clear();

                if (claimedCount == claimsList.Count)  // all rewards claimed
                    claimsList.Clear();
                else // partially claimed, remove from back
                {
                    for (int i = claimedCount - 1; i >= 0; --i)
                        claimsList.RemoveAt(indexToRemove[i]);
                }
                UpdateRewardClaimsString();

                // sync claimed to client
                foreach (var idx in dirtyColSlot)
                    SyncCollectionByIndex(idx);
                foreach (var idx in dirtyAchSlot)
                    SyncAchievementByIndex(idx);
            }

            if (inventoryFull)
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_BagInventoryFull", "", false, peer);

            combatStats.SuppressComputeAll = false;
            combatStats.ComputeAll();
        }

        private bool GiveCompletedObjectiveReward(BaseAchievementObjective obj)
        {
            bool success = true;
            switch (obj.rewardType)
            {
                case AchievementRewardType.Item:
                    InvRetval retval = peer.mInventory.AddItemsToInventory((ushort)obj.rewardId, obj.rewardCount, true, "Achievement");
                    if (retval.retCode != InvReturnCode.AddSuccess)  // inventory full
                        success = false;
                    break;
                case AchievementRewardType.Currency:
                    player.AddCurrency((CurrencyType)obj.rewardId, obj.rewardCount, "Achievement");
                    break;
                case AchievementRewardType.SideEffect:
                    ApplyPassiveSE(obj.rewardId);
                    break;
            }
            return success;
        }

        private void GiveFunctionReward(LISAFunction function, int value)
        {
            switch (function)
            {
                case LISAFunction.None:
                    break;
                case LISAFunction.OpenBagSlot:
                    peer.OpenNewInvSlot(value, ItemInventoryController.OpenNewSlotType.FREE);
                    break;
            }
        }

        public void GiveLevelUpReward(AchievementLevel levelInfo)
        {
            if (levelInfo.hasReward)
            {
                if (levelInfo.rewardItems.Count > 0)
                    peer.mInventory.AddItemsToInventoryMailIfFail(levelInfo.rewardItems, null, "Achievement");

                foreach (var currency in levelInfo.currencies)
                    player.AddCurrency(currency.Key, currency.Value, "Achievement");

                UpdateLevelPassiveSEs(levelInfo);
            }

            GiveFunctionReward(levelInfo.rewardFunction, levelInfo.rewardFunctionValue);
        }

        private void ApplyPassiveSE(int seid)
        {
            SideEffectJson sejson = SideEffectRepo.GetSideEffect(seid);
            if (sejson != null)
            {
                SideEffect se = SideEffectFactory.CreateSideEffect(sejson, true);
                IPassiveSideEffect pse = se as IPassiveSideEffect;
                if (pse != null)
                    pse.AddPassive(player);
            }
        }

        private void UpdateLevelPassiveSEs(AchievementLevel levelInfo)
        {
            PlayerCombatStats combatStats = (PlayerCombatStats)player.CombatStats;
            bool needCompute = (levelPassiveSEs.Count > 0 || levelInfo.sideEffects.Count > 0) && !combatStats.SuppressComputeAll;
            if (needCompute)
                combatStats.SuppressComputeAll = true;

            // remove existing passives
            for (int i = 0; i < levelPassiveSEs.Count; ++i)
                levelPassiveSEs[i].RemovePassive();
            levelPassiveSEs.Clear();

            // add new passives
            List<SideEffectJson> passiveSEs = levelInfo.sideEffects;
            for (int i = 0; i < passiveSEs.Count; ++i)
            {
                SideEffect se = SideEffectFactory.CreateSideEffect(passiveSEs[i], true);
                IPassiveSideEffect pse = se as IPassiveSideEffect;
                if (pse != null)
                {
                    pse.AddPassive(player);
                    levelPassiveSEs.Add(pse);
                }
            }

            if (needCompute)
            {
                combatStats.SuppressComputeAll = false;
                combatStats.ComputeAll();
            }
        }
        #endregion Rewards

        private void ApplyStoredCollectionPassives(CollectionObjective obj)
        {
            storedCollectionPassives.Add(obj.id, new List<IPassiveSideEffect>());
            List<SideEffectJson> passiveSEs = obj.storeSEs;
            for (int i = 0; i < passiveSEs.Count; ++i)
            {
                SideEffect se = SideEffectFactory.CreateSideEffect(passiveSEs[i], true);
                IPassiveSideEffect pse = se as IPassiveSideEffect;
                if (pse != null)
                {
                    pse.AddPassive(player);
                    storedCollectionPassives[obj.id].Add(pse);
                }
            }
        }

        private void RemoveStoredCollectionPassives(int id)
        {
            List<IPassiveSideEffect> passives;
            if (storedCollectionPassives.TryGetValue(id, out passives))
            {
                for (int i = 0; i < passives.Count; ++i)
                    passives[i].RemovePassive();

                passives.Clear();
                storedCollectionPassives.Remove(id);
            }
        }

#if DEBUG
        public void ConsoleResetCollections()
        {
            collectionsDict.Clear();
            Collections.ResetAll();
            foreach (var idx in collectionElementsByIdx.Keys.ToList())
                collectionElementsByIdx[idx].Clear();
            foreach (var id in storedCollectionPassives.Keys.ToList())
                RemoveStoredCollectionPassives(id);
        }

        public void ConsoleGetAllCollections(int objtype)
        {
            if (objtype == -1)
            {
                ConsoleResetCollections();
                foreach (var obj in AchievementRepo.collectionObjectives.Values)
                {
                    int index = (int)obj.type;
                    string info = obj.type == CollectionType.Photo ? "random photo description" : "";
                    CollectionElement elem = new CollectionElement(obj.id, DateTime.Now, false, info, false, index);
                    collectionsDict.Add(obj.id, elem);
                    collectionElementsByIdx[index].Add(elem);

                    AddToRewardClaims(AchievementKind.Collection, obj.id);
                }
                foreach (var elem in collectionElementsByIdx)
                    SyncCollectionByIndex(elem.Key);
            }
            else
            {
                var list = AchievementRepo.GetCollectionObjectivesByType((CollectionType)objtype);
                foreach (var obj in list)
                {
                    if (GetCollectionById(obj.id) == null)
                    {
                        string info = obj.type == CollectionType.Photo ? "random photo description" : "";
                        CollectionElement elem = new CollectionElement(obj.id, DateTime.Now, false, info, false, objtype);
                        collectionsDict.Add(obj.id, elem);
                        collectionElementsByIdx[objtype].Add(elem);
                        string oldString = (string)Collections[objtype];
                        AddToRewardClaims(AchievementKind.Collection, obj.id);
                    }
                }
                SyncCollectionByIndex(objtype);
            }
            UpdateRewardClaimsString();
        }

        public void ConsoleResetAchievements()
        {
            achievementsDict.Clear();
            Achievements.ResetAll();
            completedTargets.Clear();
            foreach (var idx in achievementElementsByIdx.Keys.ToList())
                achievementElementsByIdx[idx].Clear();
        }

        public void ConsoleGetAllAchievements()
        {
            ConsoleResetAchievements();

            foreach (var obj in AchievementRepo.achievementObjectives.Values)
            {
                int idx = (int)obj.mainType;
                AchievementElement elem = new AchievementElement(obj.id, obj.completeCount, obj.completeCount, false, idx);
                achievementsDict.Add(obj.id, elem);
                achievementElementsByIdx[idx].Add(elem);
                AddToRewardClaims(AchievementKind.Achievement, obj.id);
            }
            UpdateRewardClaimsString();
            foreach (var elem in collectionElementsByIdx)
                SyncCollectionByIndex(elem.Key);
        }

        public void ConsoleSetAchievementLevel(int newLevel)
        {
            player.PlayerSynStats.AchievementLevel = newLevel;
            player.SecondaryStats.AchievementExp = 0;

            // levelled up to this level, give this level's reward
            var info = AchievementRepo.GetAchievementLevelInfo(newLevel);
            if (info != null)
                GiveLevelUpReward(info);

            UpdateAchievement(AchievementObjectiveType.AchievementLevel, "-1", true, newLevel, false);
        }

        public void ConsoleClearAchievementRewards()
        {
            claimsList.Clear();
            RewardClaims = "";
        }
#endif
    }
}