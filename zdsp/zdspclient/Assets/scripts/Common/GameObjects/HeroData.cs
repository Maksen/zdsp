using Kopio.JsonContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;

public static class HeroData
{
    public static readonly int MAX_HEROES = 40;
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class HeroSynCombatStats
{
    [JsonProperty(PropertyName = "att")]
    public float Attack { get; set; }

    [JsonProperty(PropertyName = "acc")]
    public float Accuracy { get; set; }

    [JsonProperty(PropertyName = "cri")]
    public float Critical { get; set; }

    [JsonProperty(PropertyName = "cdm")]
    public float CriticalDamage { get; set; }

    [JsonProperty(PropertyName = "ia")]
    public float IgnoreArmor { get; set; }
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Hero
{
    [JsonProperty(PropertyName = "idx")]
    public int SlotIdx { get; set; }

    [JsonProperty(PropertyName = "id")]
    public int HeroId { get; set; }

    [JsonProperty(PropertyName = "lv")]
    public int Level { get; set; }

    [JsonProperty(PropertyName = "sk1lv")]
    public int Skill1Level { get; set; }

    [JsonProperty(PropertyName = "sk2lv")]
    public int Skill2Level { get; set; }

    [JsonProperty(PropertyName = "sk3lv")]
    public int Skill3Level { get; set; }

    [JsonProperty(PropertyName = "skpts")]
    public int SkillPoints { get; set; }

    [JsonProperty(PropertyName = "like")]
    public HeroInterestType Interest { get; set; }

    [JsonProperty(PropertyName = "tlv")]
    public int TrustLevel { get; set; }

    [JsonProperty(PropertyName = "texp")]
    public int TrustExp { get; set; }

    [JsonProperty(PropertyName = "tier")]
    public int ModelTier { get; set; }

    [JsonProperty(PropertyName = "si")]
    public List<int> UnlockedSkinItems { get; set; }

    [JsonProperty(PropertyName = "away")]
    public bool IsAway { get; set; }

    [JsonProperty(PropertyName = "cs")]
    public HeroSynCombatStats SynCombatStats { get; set; }

    //  Non Serialized
    public HeroJson HeroJson { get; set; }
    public ICombatStats CombatStats { get; set; }

    public Hero() { }

    public Hero(int id, HeroJson json, int index)
    {
        HeroJson = json;
        SlotIdx = index;
        HeroId = id;
        Level = 1;
        Skill1Level = 1;
        Skill2Level = 1;
        Skill3Level = 1;
        SkillPoints = 1;
        Interest = HeroRepo.GetRandomInterestByGroup(HeroJson.interestgroup);
        TrustLevel = 1;
        ModelTier = GetFirstModelTier();
        UnlockedSkinItems = new List<int>();
        ComputeCombatStats();
    }

    // Dummy for locked hero
    public Hero(int id, HeroJson json)
    {
        SlotIdx = -1; // to identify is still locked
        HeroJson = json;
        HeroId = id;
        Level = 1;
        Skill1Level = 1;
        Skill2Level = 1;
        Skill3Level = 1;
        ModelTier = GetFirstModelTier();
        UnlockedSkinItems = new List<int>();
        ComputeCombatStats();
    }

    public static Hero ToObject(string str)
    {
        return JsonConvertDefaultSetting.DeserializeObject<Hero>(str);
    }

    public override string ToString()
    {
        return JsonConvertDefaultSetting.SerializeObject(this);
    }

    public bool IsMaxLevel()
    {
        return HeroRepo.GetHeroGrowthData(HeroJson.growthgroup, Level + 1) == null;
    }

    public int GetTotalSkillPoints()
    {
        return Skill1Level + Skill2Level + Skill3Level + SkillPoints;
    }

    public void ResetSkillPoints()
    {
        SkillPoints += Skill1Level - 1;
        SkillPoints += Skill2Level - 1;
        SkillPoints += Skill3Level - 1;
        Skill1Level = 1;
        Skill2Level = 1;
        Skill3Level = 1;
    }

    public void GetSkillGroupAndCurrentLevel(int skillNo, out int skillGrpId, out int currentLevel)
    {
        switch (skillNo)
        {
            case 1:
                skillGrpId = HeroJson.skill1grp;
                currentLevel = Skill1Level;
                break;
            case 2:
                skillGrpId = HeroJson.skill2grp;
                currentLevel = Skill2Level;
                break;
            case 3:
                skillGrpId = HeroJson.skill3grp;
                currentLevel = Skill3Level;
                break;
            default:
                skillGrpId = 0;
                currentLevel = 0;
                break;
        }
    }

    public bool CanLevelUpSkill(int skillNo)
    {
        int skillgroupid, currentLevel;
        GetSkillGroupAndCurrentLevel(skillNo, out skillgroupid, out currentLevel);
        return !SkillRepo.IsSkillMaxLevel(skillgroupid, currentLevel);
    }

    public bool CanAddSkillPoint()
    {
        int totalMaxSkillLevels = SkillRepo.GetSkillGroupMaxLevel(HeroJson.skill1grp);
        totalMaxSkillLevels += SkillRepo.GetSkillGroupMaxLevel(HeroJson.skill2grp);
        totalMaxSkillLevels += SkillRepo.GetSkillGroupMaxLevel(HeroJson.skill3grp);
        return GetTotalSkillPoints() < totalMaxSkillLevels;
    }

    public int GetFirstModelTier()
    {
        string[] unlockpts = HeroJson.tierunlockpts.Split(';');
        for (int i = 0; i < unlockpts.Length; i++)
        {
            int pts = -1;
            if (int.TryParse(unlockpts[i], out pts) && pts > 0)
                return i + 1;
        }
        return -1;
    }

    public int GetModelTierUnlockPoints(int tier)
    {
        string[] unlockpts = HeroJson.tierunlockpts.Split(';');
        if (unlockpts.Length == 3)
        {
            int reqPts;
            if (int.TryParse(unlockpts[tier - 1], out reqPts))
                return reqPts;
            else
                return -1;
        }
        else
            return -1;
    }

    public bool IsModelTierUnlocked(int tier)
    {
        if (tier <= 3)  // t1 - t3
        {
            int reqPts = GetModelTierUnlockPoints(tier);
            return reqPts > 0 && GetTotalSkillPoints() >= reqPts;
        }
        else  // is skin item
        {
            return UnlockedSkinItems.Contains(tier);
        }
    }

    public int GetTriggeredQuest()
    {
        string[] questStr = HeroJson.questid.Split(';');
        for (int i = 0; i < questStr.Length; i++)
        {
            string[] detail = questStr[i].Split(',');
            int level, questId;
            if (int.TryParse(detail[0], out level) && int.TryParse(detail[1], out questId))
            {
                if (level == TrustLevel)
                    return questId;
            }
        }
        return 0;
    }

    public bool CanSummon(int playerLevel)
    {
        return playerLevel >= HeroJson.summonlevel;
    }

    public void ComputeCombatStats()
    {
        HeroGrowthJson growthData = HeroRepo.GetHeroGrowthData(HeroJson.growthgroup, Level);
        if (growthData != null)
        {
            if (CombatStats == null)
                CombatStats = new PlayerCombatStats();

            PlayerCombatStats combatStats = (PlayerCombatStats)CombatStats;
            combatStats.SuppressComputeAll = true;
            CombatStats.SetField(FieldName.WeaponAttackBase, HeroJson.weaponattack);
            CombatStats.SetField(FieldName.StrengthBase, growthData.strength);
            CombatStats.SetField(FieldName.DexterityBase, growthData.dexterity);
            CombatStats.SetField(FieldName.ConstitutionBase, growthData.constitution);
            CombatStats.SetField(FieldName.IntelligenceBase, growthData.intelligence);
            CombatStats.SetField(FieldName.AttackBase, growthData.attackpower);
            CombatStats.SetField(FieldName.AccuracyBase, growthData.accuracy);
            CombatStats.SetField(FieldName.CriticalBase, growthData.critical);

            // set arbitrary health needed for IsAlive check
            CombatStats.SetField(FieldName.HealthBase, 100);
            CombatStats.SetField(FieldName.HealthMax, 100);
            CombatStats.SetField(FieldName.Health, 100);

            combatStats.SuppressComputeAll = false;
            combatStats.ComputeAll();

            if (SynCombatStats == null)
                SynCombatStats = new HeroSynCombatStats();
            SynCombatStats.Attack = CombatStats.GetField(FieldName.Attack);
            SynCombatStats.Accuracy = CombatStats.GetField(FieldName.Accuracy);
            SynCombatStats.Critical = CombatStats.GetField(FieldName.Critical);
            SynCombatStats.CriticalDamage = CombatStats.GetField(FieldName.CriticalDamage);
            SynCombatStats.IgnoreArmor = CombatStats.GetField(FieldName.IgnoreArmor);
        }
    }
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class ExploreMapData
{
    [JsonProperty(PropertyName = "map")]
    public int MapId { get; set; }

    [JsonProperty(PropertyName = "tgt")]
    public int TargetId { get; set; }

    [JsonProperty(PropertyName = "hero")]
    public List<int> HeroIdList { get; set; }

    [JsonProperty(PropertyName = "end")]
    public DateTime EndTime { get; set; }

    //  Non Serialized
    public ExplorationMapJson MapData { get; set; }

    public ExploreMapData() { }

    public ExploreMapData(int map, int target, List<int> heroIds, DateTime end, ExplorationMapJson mapJson)
    {
        MapId = map;
        TargetId = target;
        HeroIdList = heroIds;
        EndTime = end;
        MapData = mapJson;
    }

    public static ExploreMapData ToObject(string str)
    {
        return JsonConvertDefaultSetting.DeserializeObject<ExploreMapData>(str);
    }

    public override string ToString()
    {
        return JsonConvertDefaultSetting.SerializeObject(this);
    }

    public bool IsCompleted()
    {
        return DateTime.Now >= EndTime;
    }

    public float GetHeroEfficiency(Hero hero)
    {
        float terrainEff = HeroRepo.GetTerrainEfficiency(MapData.terraintype, hero.Interest) * 0.01f;
        float levelEff = Math.Min((float)hero.Level * MapData.levelmaxefficiency / MapData.levelrelvalue, MapData.levelmaxefficiency);
        float trustEff = Math.Min((float)hero.TrustLevel * MapData.trustmaxefficiency / MapData.trustrelvalue, MapData.trustmaxefficiency);
        return (levelEff + trustEff) * terrainEff * 0.01f;
    }

    public int GetFulfilledChestCount(List<Hero> heroesList)
    {
        int count = 0;
        if (IsChestRequirementFulfilled(MapData.chestreqtype1, MapData.chestreqvalue1, heroesList))
            count++;
        if (IsChestRequirementFulfilled(MapData.chestreqtype2, MapData.chestreqvalue2, heroesList))
            count++;
        if (IsChestRequirementFulfilled(MapData.chestreqtype3, MapData.chestreqvalue3, heroesList))
            count++;
        return count;
    }

    private bool IsChestRequirementFulfilled(ChestRequirementType type, int value, List<Hero> heroes)
    {
        bool fulfilled = false;
        switch (type)
        {
            case ChestRequirementType.HeroID:
                fulfilled = heroes.Exists(x => x.HeroId == value);
                break;
            case ChestRequirementType.HeroInterest:
                fulfilled = heroes.Exists(x => x.Interest == (HeroInterestType)value);
                break;
            case ChestRequirementType.HeroTrust:
                fulfilled = heroes.Exists(x => x.TrustLevel == value);
                break;
        }
        return fulfilled;
    }
    
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class HeroInvData
{
    [JsonProperty(PropertyName = "heroes")]
    public List<Hero> HeroesList = new List<Hero>();

    [JsonProperty(PropertyName = "summon")]
    public int SummonedHero { get; set; }

    [JsonProperty(PropertyName = "inprogress")]
    public List<ExploreMapData> InProgressMaps = new List<ExploreMapData>();

    [JsonProperty(PropertyName = "explored")]
    public string ExploredMaps { get; set; }

}
