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
    DialogUsernamePassword = 405,

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

    DialogClaimReward = 440,
    DialogCutscene = 441,

    DialogEnd = 699,

    //-------------------------------------

    ConsoleCommand = 900
}
