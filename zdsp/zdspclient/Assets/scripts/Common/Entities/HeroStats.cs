using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common.Datablock;

namespace Zealot.Common.Entities
{
    public class HeroStats : LocalObject
    {
        private int _summonedHeroId;
        private string _explorations;
        private string _explored;

        public Dictionary<int, Hero> GetHeroesDict() { return mHeroesDict; }
        protected Dictionary<int, Hero> mHeroesDict;
        public Dictionary<int, ExploreMapData> GetExplorationsDict() { return explorationsDict; }
        protected Dictionary<int, ExploreMapData> explorationsDict;
        protected HashSet<int> exploredMaps;

        public HeroStats() : base(LOTYPE.HeroStats)
        {
            _summonedHeroId = 0;
            _explorations = "";

            heroes = new CollectionHandler<object>(HeroData.MAX_HEROES);
            heroes.SetParent(this, "heroes");
            mHeroesDict = new Dictionary<int, Hero>();

            explorationsDict = new Dictionary<int, ExploreMapData>();
            exploredMaps = new HashSet<int>();
        }

        public CollectionHandler<object> heroes { get; set; } // Store hero info in string

        public int SummonedHeroId
        {
            get { return _summonedHeroId; }
            set { this.OnSetAttribute("SummonedHeroId", value); _summonedHeroId = value; }
        }

        public string Explorations
        {
            get { return _explorations; }
            set { this.OnSetAttribute("Explorations", value); _explorations = value; }
        }

        public string Explored
        {
            get { return _explored; }
            set { this.OnSetAttribute("Explored", value); _explored = value; }
        }

        protected void ParseExplorationsString()
        {
            explorationsDict.Clear();
            if (!string.IsNullOrEmpty(Explorations))
                explorationsDict = JsonConvertDefaultSetting.DeserializeObject<Dictionary<int, ExploreMapData>>(Explorations);
        }

        protected void ParseExploredString()
        {
            exploredMaps.Clear();
            if (!string.IsNullOrEmpty(Explored))
                exploredMaps = JsonConvertDefaultSetting.DeserializeObject<HashSet<int>>(Explored);
        }

        public Hero GetHero(int heroId)
        {
            Hero hero;
            mHeroesDict.TryGetValue(heroId, out hero);
            return hero;
        }

        public bool IsHeroUnlocked(int heroId)
        {
            return mHeroesDict.ContainsKey(heroId);
        }

        public bool HasHeroFulfilledBond(HeroBondJson bond, int heroId, out bool heroLocked)
        {
            Hero hero = GetHero(heroId);
            if (hero != null)
            {
                heroLocked = false;
                if (bond == null)
                    return true;
                else
                {
                    return hero.HasFulfilledBondType(bond.bondtype1, bond.bondvalue1)
                         && hero.HasFulfilledBondType(bond.bondtype2, bond.bondvalue2);
                }
            }
            else
            {
                heroLocked = true;
                return false;
            }
        }

        public ExploreMapData GetExploringMap(int mapId)
        {
            ExploreMapData map;
            explorationsDict.TryGetValue(mapId, out map);
            return map;
        }

        public bool IsExploringMap(int mapId)
        {
            return explorationsDict.ContainsKey(mapId);
        }

        public bool HasExploredMap(int mapId)
        {
            return exploredMaps.Contains(mapId);
        }

        public bool IsChestRequirementFulfilled(ChestRequirementType type, int value, List<Hero> heroes)
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
                    fulfilled = heroes.Exists(x => x.TrustLevel >= value);
                    break;
            }
            return fulfilled;
        }
    }

    public class HeroBond
    {
        public HeroBondGroupJson heroBondGroupJson;
        public List<HeroBondJson> heroBondJsonList;  // sorted by level
        public List<int> heroIds;  // id of heroes required

        public HeroBond(HeroBondGroupJson grp)
        {
            heroBondGroupJson = grp;
            heroBondJsonList = new List<HeroBondJson>();
            heroIds = new List<int>();
            if (grp.hero1 > 0) heroIds.Add(grp.hero1);
            if (grp.hero2 > 0) heroIds.Add(grp.hero2);
            if (grp.hero3 > 0) heroIds.Add(grp.hero3);
            if (grp.hero4 > 0) heroIds.Add(grp.hero4);
            if (grp.hero5 > 0) heroIds.Add(grp.hero5);
        }

        public HeroBondJson GetHighestFulfilledLevel(HeroStats heroStats)
        {
            HeroBondJson highestLevel = null;
            for (int i = 0; i < heroBondJsonList.Count; i++)
            {
                bool fulfilled = true;
                HeroBondJson currentBond = heroBondJsonList[i];
                for (int index = 0; index < heroIds.Count; index++)
                {
                    bool isHeroLocked;
                    if (!heroStats.HasHeroFulfilledBond(currentBond, heroIds[index], out isHeroLocked))
                    {
                        fulfilled = false;
                        break;
                    }
                }
                if (fulfilled)
                    highestLevel = currentBond;
                else
                    break;
            }
            return highestLevel;
        }

        public bool IsHeroInvolved(int heroId)
        {
            return heroIds.Contains(heroId);
        }
    }
}