using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Common.Entities;

namespace Zealot.Repository
{
    public static class HeroRepo
    {
        public static Dictionary<int, HeroJson> heroes;
        public static Dictionary<int, Dictionary<int, HeroGrowthJson>> growthGroupToLevels; // groupId->levels , level->info
        public static Dictionary<int, List<HeroInterestGroupJson>> interestGroupToTypes;
        public static Dictionary<HeroInterestType, HeroInterestJson> heroInterests;
        public static Dictionary<int, HeroTrustJson> heroTrustLevels;
        public static Dictionary<int, HeroBond> heroBonds;
        public static List<HeroBond> heroBondsList;  // sorted by sequence
        public static Dictionary<int, List<HeroBond>> heroIdToHeroBonds;  // all bonds this hero is involved in
        public static Dictionary<int, ExplorationMapJson> explorationMaps;
        public static List<ExplorationMapJson> explorationMapsList; // sorted by sequence
        public static Dictionary<int, ExplorationTargetJson> explorationTargets;
        public static Dictionary<int, List<ExplorationTargetJson>> explorationGroupToTargets;  // group->targets
        public static Dictionary<Pair<TerrainType, HeroInterestType>, float> terrainEfficiencyChart;
        public static Dictionary<TerrainType, TerrainJson> terrainTypes;

        public static int MAX_TRUST_LEVEL;

        static HeroRepo()
        {
            heroes = new Dictionary<int, HeroJson>();
            growthGroupToLevels = new Dictionary<int, Dictionary<int, HeroGrowthJson>>();
            interestGroupToTypes = new Dictionary<int, List<HeroInterestGroupJson>>();
            heroInterests = new Dictionary<HeroInterestType, HeroInterestJson>();
            heroTrustLevels = new Dictionary<int, HeroTrustJson>();
            heroBonds = new Dictionary<int, HeroBond>();
            heroBondsList = new List<HeroBond>();
            heroIdToHeroBonds = new Dictionary<int, List<HeroBond>>();
            explorationMaps = new Dictionary<int, ExplorationMapJson>();
            explorationMapsList = new List<ExplorationMapJson>();
            explorationTargets = new Dictionary<int, ExplorationTargetJson>();
            explorationGroupToTargets = new Dictionary<int, List<ExplorationTargetJson>>();
            terrainEfficiencyChart = new Dictionary<Pair<TerrainType, HeroInterestType>, float>();
            terrainTypes = new Dictionary<TerrainType, TerrainJson>();
        }

        private static void ClearDictionaries()
        {
            heroes.Clear();
            growthGroupToLevels.Clear();
            interestGroupToTypes.Clear();
            heroInterests.Clear();
            heroTrustLevels.Clear();
            heroBonds.Clear();
            heroBondsList.Clear();
            heroIdToHeroBonds.Clear();
            explorationMaps.Clear();
            explorationMapsList.Clear();
            explorationGroupToTargets.Clear();
            terrainEfficiencyChart.Clear();
            terrainTypes.Clear();
        }

        public static void Init(GameDBRepo gameData)
        {
            ClearDictionaries();

            heroes = gameData.Hero;

            foreach (var entry in gameData.HeroGrowth.Values)
            {
                if (growthGroupToLevels.ContainsKey(entry.growthgroup))
                    growthGroupToLevels[entry.growthgroup].Add(entry.herolevel, entry);
                else
                    growthGroupToLevels.Add(entry.growthgroup, new Dictionary<int, HeroGrowthJson>() { { entry.herolevel, entry } });
            }

            foreach (var entry in gameData.HeroInterestGroup.Values)
            {
                if (interestGroupToTypes.ContainsKey(entry.interestgroup))
                    interestGroupToTypes[entry.interestgroup].Add(entry);
                else
                    interestGroupToTypes.Add(entry.interestgroup, new List<HeroInterestGroupJson>() { entry });
            }

            foreach (var entry in gameData.HeroInterest.Values)
                heroInterests.Add(entry.interesttype, entry);

            foreach (var entry in gameData.HeroTrust.Values)
                heroTrustLevels.Add(entry.trustlevel, entry);
            MAX_TRUST_LEVEL = heroTrustLevels.Count;

            List<HeroBondGroupJson> heroBondGroupList = gameData.HeroBondGroup.Values.OrderBy(x => x.sequence).ToList();
            foreach (var entry in heroBondGroupList)
            {
                HeroBond bond = new HeroBond(entry);
                heroBonds.Add(entry.id, bond);
                heroBondsList.Add(bond);
                if (entry.hero1 > 0) AddToHeroIdToHeroBondsMap(entry.hero1, bond);
                if (entry.hero2 > 0) AddToHeroIdToHeroBondsMap(entry.hero2, bond);
                if (entry.hero3 > 0) AddToHeroIdToHeroBondsMap(entry.hero3, bond);
                if (entry.hero4 > 0) AddToHeroIdToHeroBondsMap(entry.hero4, bond);
                if (entry.hero5 > 0) AddToHeroIdToHeroBondsMap(entry.hero5, bond);
            }

            var heroBondDict = gameData.HeroBond;
            for (int i = 0; i < gameData.HeroBond__sideeffects.Count; i++)
            {
                var se = gameData.HeroBond__sideeffects[i];
                if (heroBondDict.ContainsKey(se.herobondid))
                    heroBondDict[se.herobondid].sideeffects.Add(se.sideeffectsid, SideEffectRepo.GetSideEffect(se.sideeffectsid));
            }

            foreach (var entry in heroBondDict.Values)
            {
                HeroBond heroBond = GetHeroBondById(entry.bondgroupid);
                if (heroBond != null)
                    heroBond.heroBondJsonList.Add(entry);
            }
            foreach (int id in heroBonds.Keys.ToList())
                heroBonds[id].heroBondJsonList.Sort((x, y) => x.bondlevel.CompareTo(y.bondlevel));  // can use unstable sort since no level will be same

            explorationMaps = gameData.ExplorationMap;
            explorationMapsList = explorationMaps.Values.OrderBy(x => x.sequence).ToList();  // stable sort

            explorationTargets = gameData.ExplorationTarget;
            foreach (var entry in gameData.ExplorationTarget.Values)
            {
                if (explorationGroupToTargets.ContainsKey(entry.exploregroupid))
                    explorationGroupToTargets[entry.exploregroupid].Add(entry);
                else
                    explorationGroupToTargets.Add(entry.exploregroupid, new List<ExplorationTargetJson>() { entry });
            }

            foreach (var entry in gameData.TerrainEfficiency.Values)
                terrainEfficiencyChart.Add(Pair.Create(entry.terraintype, entry.interesttype), entry.efficiency);

            foreach (var entry in gameData.Terrain.Values)
                terrainTypes.Add(entry.terraintype, entry);
        }

        private static void AddToHeroIdToHeroBondsMap(int heroId, HeroBond bond)
        {
            if (heroIdToHeroBonds.ContainsKey(heroId))
                heroIdToHeroBonds[heroId].Add(bond);
            else
                heroIdToHeroBonds.Add(heroId, new List<HeroBond>() { bond });
        }

        public static HeroJson GetHeroById(int heroId)
        {
            HeroJson json;
            heroes.TryGetValue(heroId, out json);
            return json;
        }

        public static HeroGrowthJson GetHeroGrowthData(int groupId, int level)
        {
            Dictionary<int, HeroGrowthJson> levelInfos;
            if (growthGroupToLevels.TryGetValue(groupId, out levelInfos))
            {
                HeroGrowthJson json;
                levelInfos.TryGetValue(level, out json);
                return json;
            }
            return null;
        }

        public static bool IsInterestInGroup(int groupId, HeroInterestType type)
        {
            List<HeroInterestGroupJson> interestList;
            if (interestGroupToTypes.TryGetValue(groupId, out interestList))
            {
                return interestList.Exists(x => x.interesttype == type);
            }
            return false;
        }

        public static HeroInterestType GetRandomInterestByGroup(int groupId, HeroInterestType excludeType = HeroInterestType.Random)
        {
            List<HeroInterestGroupJson> interestList;
            if (interestGroupToTypes.TryGetValue(groupId, out interestList))
            {
                if (interestList.Count == 0)
                    return HeroInterestType.Random;

                float totalProb = 0;
                for (int i = 0; i < interestList.Count; i++)
                {
                    if (interestList[i].interesttype == excludeType)
                        continue;
                    totalProb += interestList[i].probability;
                }

                float randomPoint = (float)GameUtils.Random(0, 1) * totalProb;
                for (int i = 0; i < interestList.Count; i++)
                {
                    if (interestList[i].interesttype == excludeType)
                        continue;
                    if (randomPoint < interestList[i].probability)
                        return interestList[i].interesttype;
                    else
                        randomPoint -= interestList[i].probability;
                }
                if (interestList[interestList.Count - 1].interesttype != excludeType)
                    return interestList[interestList.Count - 1].interesttype;
            }
            return HeroInterestType.Random;
        }

        public static HeroInterestJson GetInterestByType(HeroInterestType type)
        {
            HeroInterestJson json;
            heroInterests.TryGetValue(type, out json);
            return json;
        }

        public static HeroTrustJson GetTrustLevelData(int trustLevel)
        {
            HeroTrustJson json;
            heroTrustLevels.TryGetValue(trustLevel, out json);
            return json;
        }

        public static HeroBond GetHeroBondById(int groupId)
        {
            HeroBond herobond;
            heroBonds.TryGetValue(groupId, out herobond);
            return herobond;
        }

        public static List<HeroBond> GetInvolvedBondsByHeroId(int heroId)
        {
            List<HeroBond> list;
            if (heroIdToHeroBonds.TryGetValue(heroId, out list))
                return list;
            return new List<HeroBond>();
        }

        public static ExplorationMapJson GetExplorationMapById(int id)
        {
            ExplorationMapJson json;
            explorationMaps.TryGetValue(id, out json);
            return json;
        }

        public static ExplorationTargetJson GetExplorationTargetById(int id)
        {
            ExplorationTargetJson json;
            explorationTargets.TryGetValue(id, out json);
            return json;
        }

        public static List<ExplorationTargetJson> GetExplorationTargetsByGroup(int groupId)
        {
            List<ExplorationTargetJson> list;
            if (explorationGroupToTargets.TryGetValue(groupId, out list))
                return list;
            return new List<ExplorationTargetJson>();
        }

        public static float GetTerrainEfficiency(TerrainType terrainType, HeroInterestType interestType)
        {
            float efficiency;
            terrainEfficiencyChart.TryGetValue(Pair.Create(terrainType, interestType), out efficiency);
            return efficiency;
        }

        public static TerrainJson GetTerrainByType(TerrainType type)
        {
            TerrainJson json;
            terrainTypes.TryGetValue(type, out json);
            return json;
        }
    }
}
