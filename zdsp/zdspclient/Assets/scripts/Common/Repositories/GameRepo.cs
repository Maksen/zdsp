using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kopio.JsonContracts;

/// <summary>
/// GameDB Repository. This class is shared between server and client
/// </summary>
namespace Zealot.Repository
{
    public static class GameRepo
    {
        private static bool _loaded = false;
        public static bool IsLoaded { get { return _loaded; } }

        public static BaseItemFactory ItemFactory;
        static GameRepo()
        {
        }

        public static void InitClient(string strGameDB)
        {
            ProfilerUtils.LogString("[Profiler] InitClient Begin: ");
            GameDBRepo GameData = Parse(strGameDB);
            ProfilerUtils.LogString("[Profiler] GameDB Parse: ");
            InitCommon(GameData);
            ActivityOverviewRepo.Init(GameData);
            ProfilerUtils.LogString("[Profiler] InitClient End: ");
        }

        public static void InitServer(string strGameDB)
        {
            GameDBRepo GameData = Parse(strGameDB);
            InitCommon(GameData);
        }

        public static void InitGM(GameDBRepo GameData)
        {
            InitCommon(GameData);
            ActivityOverviewRepo.Init(GameData);
        }
        
        private static void InitCommon(GameDBRepo GameData)
        {
            long heapsize = ProfilerUtils.GetMonoUsedHeapSize();
            GameConstantRepo.Init(GameData);
            ItemFactory.InitGameData(GameData);
            ProfilerUtils.LogIncrementSize("ItemFactory", ref heapsize);
            GUILocalizationRepo.Init(GameData);
            ProfilerUtils.LogIncrementSize("GUILocalizationRepo", ref heapsize);        
            LevelRepo.Init(GameData);
            RealmRepo.Init(GameData);
            ProfilerUtils.LogIncrementSize("RealmRepo", ref heapsize);
            JobSectRepo.Init(GameData);
            ProfilerUtils.LogIncrementSize("JobSectRepo", ref heapsize);
            StaticNPCRepo.Init(GameData);
            ProfilerUtils.LogIncrementSize("StaticNPCRepo", ref heapsize);
            SDGRepo.Init(GameData);
            ProfilerUtils.LogIncrementSize("SDGRepo", ref heapsize);
            SideEffectRepo.Init(GameData);
            ProfilerUtils.LogIncrementSize("SideEffectRepo", ref heapsize);
            SkillRepo.Init(GameData);
            ProfilerUtils.LogIncrementSize("SkillRepo", ref heapsize);
            NPCRepo.Init(GameData);           
            ProfilerUtils.LogIncrementSize("NPCRepo", ref heapsize);
            SpecialBossRepo.Init(GameData);
            RealmNPCGroupRepo.Init(GameData);
            NPCSkillsRepo.Init(GameData);
            PortraitPathRepo.Init(GameData);
            QuestRepo.Init(GameData);
            RespawnRepo.Init(GameData);
            CharacterLevelRepo.Init(GameData);
            PartyRepo.Init(GameData);
            ElementalChartRepo.Init(GameData);
            SkillTreeRepo.Init(GameData);
            HeroRepo.Init(GameData);
            EquipmentModdingRepo.Init(GameData);
            ProfilerUtils.LogIncrementSize("others", ref heapsize);
            RewardListRepo.Init(GameData);
            _loaded = true;
        }

        public static void SetItemFactory(BaseItemFactory itemfactory)
        {
            ItemFactory = itemfactory;
        }

        public static GameDBRepo Parse(string strGameDB)
        {
            try
            {
                return JsonConvert.DeserializeObject<GameDBRepo>(strGameDB);
            }
            catch (Exception ex)
            {
                JObject jObject = JObject.Parse(strGameDB);
                uint gamedbHash = (uint)jObject["GameDBHash"];

                if (gamedbHash != GameDBRepo.GetSchemaHash())
                {
                    throw new Exception("!!! Mismatched GameDB Hash. Update jsoncontracts and gamedata again.", ex);
                }
                else
                    throw ex;
            }
        }

#if UNITY_EDITOR
        public static void InitLocalizerRepo(string strGameDB)
        {
            GameDBRepo GameData = Parse(strGameDB);
            GUILocalizationRepo.Init(GameData);
        }

        public static void InitSkillRepo(string strGameDB)
        {
            GameDBRepo GameData = Parse(strGameDB);
            SideEffectRepo.Init(GameData);
            SkillRepo.Init(GameData);
            SkillTreeRepo.Init(GameData);
        }

        public static void InitCommonEditor(string strGameDB)
        {
            GameDBRepo GameData = Parse(strGameDB);
            InitCommon(GameData);
        }

        public static void Validate()
        {

        }
#endif
    }
}
