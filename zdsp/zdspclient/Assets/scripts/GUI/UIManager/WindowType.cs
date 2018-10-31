public enum WindowType
{
    None = 0,
    MainMenu = 1,

    // Add window types here
    //---------------------------
    CharacterInfo = 2,
    Inventory = 3,
    Party = 4,
    Quest = 5,
    EquipUpgrade = 7,
    EquipReform = 8,
    DailyQuest = 9,
    Skill = 10,
    Hero = 11,
    Destiny = 12,
    WorldMap = 13,
    DestinyHistory = 14,
    Dungeon = 15,
    DungeonStory = 16,
    ShopSell = 17,
    ShopBarter = 18,
    Mail = 19,
    EquipCraft = 20,
    EquipFusion = 21,
    CharacterCreation = 22,
    CharacterSelection = 23,
    Achievement = 24,
    DNA = 25,
    Relic = 26,

    // Add persistent type window here
    //-----------------------------------
    PersistentWindowStart = 150,

    PersistentWindowEnd = 198,

    WindowEnd = 199,

    // Add dialog types here
    //------------------------------
    DialogStart = 400,

    DialogYesNoOk = 401,

    DialogVideoPlayer = 402,
    DialogLicenseAgreement = 403,
    DialogServerSelection = 404,
    DialogAccountLoginType = 405,

    DialogPartySettings = 406,
    DialogPartyRequestList = 407,
    DialogPartyInvite = 408,
    DialogPartyInfo = 409,

    DialogItemDetail = 410,
    DialogItemSellUse = 411,

    DialogNpcTalk = 412,

    DialogWorldBossRanking = 413,

    DialogHeroSkillPoints = 414,
    DialogHeroSkillDetails = 415,
    DialogHeroTrust = 416,
    DialogHeroInterest = 417,
    DialogHeroTier = 418,
    DialogHeroStats = 419,
    DialogHeroBonds = 420,
    DialogHeroList = 421,
    DialogHeroTargetList = 422,
    DialogHeroEfficiency = 423,

    DialogHistoryFilter = 424,
    DialogMessageFilter = 425,

    DialogEquipFusion = 426,

    DialogAchievementRewards = 427,
    DialogAchievementAbility = 428,
    DialogAchievementFunctions = 429,
    DialogAchievementTier = 430,

    DialogClaimReward = 440,

    DialogCutscene = 441,
    DialogAccountRegister = 442,
    DialogAccountLogin = 443,
    DialogCharacterName = 444,
    DialogNews = 445,
    DialogSettings = 446,

    DialogTutorial = 447,
   
    DialogEnd = 699,

    //-------------------------------------

    ConsoleCommand = 900
}
