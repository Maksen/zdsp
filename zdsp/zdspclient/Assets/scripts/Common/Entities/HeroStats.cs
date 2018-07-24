using Kopio.JsonContracts;
using System.Collections.Generic;
using System.Text;
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

        public CollectionHandler<object> heroes { get; set; } // Store member info in string

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
    }

    public class HeroBond
    {
        public HeroBondGroupJson heroBondGroupJson;
        public List<HeroBondJson> heroBondJsonList;  // sorted by level
        public List<int> heroIds;  // id of heroes required
        private List<Hero> heroListCache;

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
            heroListCache = new List<Hero>();
        }

        public HeroBondJson GetHighestFulfilledLevel(HeroStats heroStats)
        {
            HeroBondJson highestLevel = null;
            for (int i = 0; i < heroIds.Count; i++)
            {
                Hero hero = heroStats.GetHero(heroIds[i]);
                if (hero == null)  // required hero not unlocked so none fulfilled
                    return null;
                heroListCache.Add(hero);
            }

            for (int i = 0; i < heroBondJsonList.Count; i++)
            {
                HeroBondJson current = heroBondJsonList[i];
                if (IsLevelFulfilled(current.bondtype1, current.bondvalue1, heroListCache)
                    && IsLevelFulfilled(current.bondtype2, current.bondvalue2, heroListCache))
                    highestLevel = current;
                else
                    break;
            }
            heroListCache.Clear();

            return highestLevel;
        }

        private bool IsLevelFulfilled(HeroBondType type, int value, List<Hero> heroes)
        {
            if (type == HeroBondType.None)
                return true;

            for (int i = 0; i < heroes.Count; i++)
            {
                if (type == HeroBondType.HeroLevel)
                {
                    if (heroes[i].Level < value)
                        return false;
                }
                else if (type == HeroBondType.HeroSkill)
                {
                    if (heroes[i].GetTotalSkillPoints() < value)
                        return false;
                }
            }
            return true;
        }
    }
}

