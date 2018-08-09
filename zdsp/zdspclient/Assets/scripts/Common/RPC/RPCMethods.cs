namespace Zealot.Common.RPC
{
    public enum RPCCategory : byte
    {
        GameToMaster,
        MasterToGame,
        MasterToCluster,
        ClusterToMaster,
        ClusterToGame,
        GameToCluster,
        GMToMaster,
        MasterToGMRPC,
        Lobby, //GameLogic dependent category place here onwards
        Combat,
        Action,
        LocalObject,
        UnreliableCombat,
        NonCombat,
        TotalCategory
    }

    public enum MasterToGameRPCMethods : byte
    {
        ForceKick,
        ForceKickMutiple,
        RegCookie,
        Ret_GetServerList,
        Ret_TransferServer,

        EventDataUpdated,
        GMMessage,
        KickPlayer,
        MutePlayer,
        KickFromGuild,
        TeleportPlayer,
        AddSystemMessage,
        DeleteSystemMessage,
        LogUIDShift,
        TongbaoCostBuffChange,
        GMItemMallStatusUpdate,
        ChangeGuildLeader,
        NotifyActivityChange,
        GMAuctionChange,
        GMTopUpActivityUpdate,
        ExchangeRateUpdate,
        GMGetArenaRank,
        GMResetArenaRank,
        GMNotifyTalentModifierChanged,
        WelfareRefreshFirstTopUp,
        WelfareRefreshTotalCredit,
        WelfareRefreshTotalSpend,
        WelfareRefreshOpenServiceFund,
        WelfareRefreshGoldJackpot,
        WelfareRefreshContLogin,
    }

    public enum GameToMasterRPCMethods : byte
    {
        RegularServerStatusUpdate,
        RegUser,
        UnRegUser,
        GetServerList,
        TransferServer,

        GMResultBool,
        GMResultString,
    }

    public enum ClusterToGameRPCMethods : byte
    {
        RegChar,
        UnRegChar,
        RegCharMultiple,
        OnConnectedSyncData,
    }

    public enum GameToClusterRPCMethods : byte
    {
        RegChar,
        UnRegChar,
    }

    public enum MasterToClusterRPCMethods : byte
    {
    }

    public enum ClusterToMasterRPCMethods : byte
    {
    }

    public enum ServerLobbyRPCMethods : byte
    {
        GetCharactersResult,
        TransferRoom,
        LoadLevel,
        DeleteCharacterResult,
        ShowSystemMessage,
        CreateCharacterSuccess
    }

    public enum ClientLobbyRPCMethods : byte
    {
        GetCharacters,
        InsertCharacter,
        EnterGame,
        DeleteCharacter,
        GetLevelData,
    }

    public enum ServerCombatRPCMethods : byte
    {
        RetSuspendedRPC,    // Unsuspends client rpc

        SpawnPlayerEntity,
        SpawnMonsterEntity,
        SpawnHeroEntity,
        DestroyEntity,
        OnPlayerDead,
        RespawnPlayer,
        TeleportSetPos,
        TeleportSetPosDirection,
        TransferRoom,
        LoadLevel,
        SpawnGate,
        SpawnAnimationObject,
        EnterRealm,
        StartCountDown,
        ServerSendChatMessage,
        BroadcastMessageToClient,
        Ret_SendSystemMessageById,
        Ret_SendSystemMessageByStr,
        SetInspectPlayerInfo,
        OnMissionCompleted,
        ShowScoreBoard,
        Ret_RaidReward,
        Ret_DungeonEnterConfirmResult,
        ShowRewardDialog,

        BroadcastCutScene,
        SetServerTime,
        SendInfoOnPlayerSpawner,
        InitGameSetting,
        OpenUIWindow,
        UsingChestResult,
        Ret_GetArenaChallenger,
        Ret_ArenaClaimReward,
        Ret_GetArenaReport,

        #region Party
        Ret_GetPartyList,
        SendPartyInvitation,
        SendFollowInvitation,
        OnFollowPartyMember,
        OnGetPartyMemberPosition,
        #endregion

        #region Hero
        Ret_RandomInterestResult,
        #endregion

        #region Mail

        Ret_HasNewMail,//called after server wants to inform the player the result of new mail
        Ret_RetrieveMail,//called after client wants to see all the mail
        //Ret_SendMail,//called after server recieve the mail and send the mail to client ... TODO: Remove. Change to NewMail when recieve new mail
        Ret_OpenMail,//called after client open the mail
        Ret_DeleteMail,//called after client delete the mail
        Ret_DeleteAllMail,//called after client delete all mail
        Ret_TakeAttachment,//called after client take a attachment from the mail
        Ret_TakeAllAttachment,//called after client take all attachment from the mail

        #endregion Mail        

        Ret_OpenNewInvSlot,
        Ret_MassUseItem,

        #region OfflineExp

        OfflineExpRedDot,
        OfflineExpStartReward,
        OfflineExpClaimReward,
        OfflineExpGetData,

        #endregion OfflineExp

        #region Auction
        Ret_AuctionGetAuctionItem,
        Ret_AuctionGetRecords,
        Ret_AuctionGetBidItems,
        Ret_AuctionCollectItem,
        Ret_AuctionPlaceBid,
        #endregion

        #region Guild
        Ret_GuildGetGuildInfo,
        Ret_GuildResult,

        Ret_GuildAdd,
        Ret_GuildCheckName,
        Ret_GuildLeave,
        Ret_GuildJoin,
        Ret_GuildGetHistory,
        #endregion

        #region Donate
        SendDonateData,
        SendDonateReturnCode,
        #endregion

        #region Development
        SendMessageToConsoleCmd,
        #endregion        

        Ret_SocialReturnResult,

        ReplyValidMonSpawnPos,

        #region IAP

        Ret_GetProductsWithLockGold,
        Ret_GetMyCardAuthCode,
        Ret_DelMyCardAuthCode,
        Ret_VerifyPurchase,

        #endregion

        OnPlayerDragged,
        TrainingStepDone,
        //TrainingBossHpBelow,
        TrainingDodgeResult,
        Ret_OnCurrencyExchange,
        OnIncreaseCD,

        Ret_GetRandomBoxReward,

        ActivityPullHeroResult,

        #region InvitePVP
        Ret_InvitePvpResult,
        #endregion

        #region Wardrobe
        Ret_OnUpdateWardrobe,
        #endregion

        #region SystemSwitch
        InitSystemSwitch,
        #endregion

        #region world boss
        Ret_GetWorldBossList,
        Ret_GetWorldBossDmgList,
        #endregion

        LootItemDisplay,
    }

    public enum GMMasterRPCMethods : byte
    {
        GetServerList,
        GetAllServerStatus,
        BroadcastToGameServers,
        GMMessage,
        KickPlayer,
        MutePlayer,
        KickFromGuild,
        TeleportPlayer,
        SendSystemMessage,
        DeleteSystemMessage,
        TongbaoCostBuffChange,
        GetLevelData,
        GMItemMallStatusUpdate,
        ChangeGuildLeader,
        NotifyActivityChange,
        GMAuctionChange,
        GMTopUpActivityUpdate,
        ExchangeRateUpdate,
        GMGetArenaRank,
        GMResetArenaRank,
        GMNotifyTalentModifierChanged,
        WelfareRefreshFirstTopUp,
        WelfareRefreshTotalCredit,
        WelfareRefreshTotalSpend,
        WelfareRefreshOpenServiceFund,
        WelfareRefreshGoldJackpot,
        WelfareRefreshContLogin,
        GetPushNotificationTopic
    }

    public enum MasterGMRPCMethods : byte
    {
        GMResultBool,
        GMResultString,
        PushNotificationTopicResult
    }
    
    public enum ClientCombatRPCMethods : byte
    {
        OnClientLevelLoaded,
        OnEnterPortal,
        OnEnterSafeZone,
        OnEnterTrigger,
        OnClickWorldMap,
        OnTeleportToLevel,
        OnTeleportToLevelAndPos,
        OnTeleportToPosInLevel,
        ExitGame,
        RespawnOnSpot,
        RespawnAtCity,
        RespawnAtSafeZone,
        RespawnAtSafeZoneWithCost,
        ClientSendChatMessage,
        BroadcastSysMsgToServer,

        CreateRealmByID,
        EnterRealmByID,
        LeaveRealm,
        InspectMode,
        AddStoryExtraEntry,
        DungeonRaid,
        DungeonEnter,
        DungeonEnterState,
        DungeonCollectStarReward,
        OnPickResource,

        AddItem,
        UseItem,
        SellItem,
        MassSellItems,
        MassUseItems,
        SortItem,
        OpenNewSlot,
        OpenNewSlotAuto,
        BuyPotion,
        UnequipItem,
        SetItemHotbar,
        UseItemHotbar,

        RealmCollectReward,

        GetInspectPlayerInfo,

        #region IAP

        GetProductsWithLockGold,
        GetMyCardAuthCode,
        DelMyCardAuthCode,
        VerifyPurchase,

        #endregion IAP

        #region Social
        SocialAcceptRequest,
        SocialRemoveRequest,
        SocialSendRequest,
        SocialRemoveFriend,
        SocialGetRecommendedFriends,
        SocialUpdateFriendsInfo,
        #endregion

        GetArenaChallengers,
        ChallengeArena,
        ArenaClaimReward,
        GetArenaReport,

        AddCurrency,

        #region Mail

        HasNewMail,//called when client loads a new map/login and check if there is a new mail
        RetrieveMail,//called when client wants to retrieve mail
        OpenMail,//called when client open the mail
        DeleteMail,//called when client delete the mail
        DeleteAllMail,//called when client delete all mail
        TakeAttachment,//called when client take mail attachment
        TakeAllAttachment,//call when client take all mail attachment

        #endregion Mail        

        #region OfflineExp

        OfflineExpStartReward,
        OfflineExpClaimReward,
        OfflineExpGetData,

        #endregion OfflineExp

        RemoveBuff,

        #region Auction
        AuctionGetAuctionItem,
        AuctionGetRecords,
        AuctionGetBidItems,
        AuctionCollectItem,
        AuctionPlaceBid,
        #endregion

        #region Guild
        GuildCheckName,
        GuildGetGuildInfo,
        GuildAdd,
        GuildJoin,
        GuildLeave,
        GuildMemberRequest,
        GuildRequestAll,
        GuildRequestSetting,
        GuildSetIcon,
        GuildSetNotice,
        GuildLvlUpTech,
        GuildGetHistory,
        GuildShopExchange,
        GuildOpenSecretShop,
        GuildAppointRank,
        GuildInviteWorld,
        GuildDreamHouseAdd,
        GuildDreamHouseCollect,
        #endregion

        #region Donate
        GetGuildDonateData,
        RequestGuildDonate,
        ResponseGuildDonate,
        GetGuildDonate,
        #endregion       

        CurrencyExchange,

        #region Tutorial
        //StartTutorial,
        //TutorialNextStep,
        //EndTutorial,
        #endregion

        SaveGameSetting,
        BotSetting,
        GetClosestValidMonSpawnPos,

        FirstRealmStep,

        RedeemSerialCode,

        GetCompensate,

        #region Party
        GetPartyList,
        CreateParty,
        JoinParty,
        KickPartyMember,
        ChangePartyLeader,
        ProcessPartyRequest,
        InviteToParty,
        AcceptPartyInvitation,
        LeaveParty,
        ChangePartySetting,
        InviteToFollow,
        AcceptFollowInvitation,
        SendPartyRecruitment,
        SaveAutoFollowSetting,
        FollowPartyMember,
        GetPartyMemberPosition,
        #endregion

        #region Hero
        UnlockHero,
        LevelUpHero,
        AddHeroSkillPoint,
        ResetHeroSkillPoints,
        LevelUpHeroSkill,
        ChangeHeroInterest,
        AddHeroTrust,
        SummonHero,
        ExploreMap,
        ClaimExplorationReward,
        #endregion

        #region Portrait
        CharInfoSetPortrait,
        #endregion

        #region Crafting
        CraftItem,
        #endregion

        #region RandomBox
        ActiveRandomBox,
        GetRandomBoxReward,
        #endregion

        #region Mount
        Mount,
        UnMount,
        #endregion

        #region Wardrobe
        EquipFashion,
        UnequipFashion,
        #endregion

        #region InvitePvp
        InvitePvpAsk,
        InvitePvpReply,
        #endregion
        #region Combat
        SetPlayerTeam,
        #endregion
        #region TutorialList
        UpdateTutorialList,
        SyncAttackResult,
        #endregion
     
        #region Triggers
        OnColliderTrigger,
        #endregion

        #region WorldBoss
        GetWorldBossList,
        GetWorldBossDmgList,
        #endregion
    }

    public enum ServerUnreliableCombatRPCMethods : byte
    {
        SideEffectHit,
        EntityOnDamage,
    }

    public enum ClientNonCombatRPCMethods : byte
    {
        RequestCombatStats,
        RequestArenaAICombatStats,
        RequestSideEffectsInfo,

        #region Development
        ConsoleTeleportToLevel,
        ConsoleTeleportToPosInLevel,
        ConsoleClearItemInventory,
        ConsoleAddExperience,
        ConsoleGetEquipmentItemInfo,
        ConsoleGetInventoryItemInfo,
        ConsoleAddStats,
        ConsoleSetDmgPercent,
        ConsoleAddSideEffect,
        ConsoleTestSideEffect,
        ConsoleEnterActivityByRealmID,
        ConsoleInspect,
        GoToMainQuest,
        ConsoleNewDay,
        ConsoleServerNewDay,
        ConsoleLeaveRealm,
        ConsoleCompleteRealm,
        ConsoleFullHealPlayer,
        ConsoleGetAllRealmInfo,
        ConsoleSpawnSpecialBoss,
        ConsoleGuildList,
        ConsoleGuildAddFavour,
        ConsoleGuildSMLevel,
        ConsoleAddRewardGroupMail,
        ConsoleAddRewardGroupBag,
        ConsoleAddRewardGroupCheckBagSlot,
        ConsoleAddRewardGroupCheckBagMail,
        ConsoleSetAchievement,
        ConsoleAddHero,
        ConsoleRemoveHero,
        ConsoleGetHeroSkin,
        ConsoleResetExplorations,
        ConsoleKillPlayer,
        ConsoleItemMallTopUp,
        ConsoleTestCombatFormula,
        ConsoleAddLotteryFreeTickets,
        ConsoleAddLotteryPoint,
        ConsoleRefreshLeaderBoard,
        ConsoleResetPrizeGuarantee,
        ConsoleDonateReset,
        ConsoleResetDonateRemainingCount,
        ConsoleNotifyNewGMItem,
        ConsoleAddActivePoints,
        ConsoleFinishQERTask,
        ConsoleSocialAddFriend,
        ConsoleSetStoreRefreshTime,
        ConsoleResetGoldJackpotRoll,
        ConsoleResetContLoginClaims,
        ConsoleSetEquipmentUpgradeLevel,
        ConsoleResetArenaEntey,
        ConsoleSetSevenDaysOpenDays,
        ConsoleTransferServer,
        ConsoleDCFromServer,
        ConsoleSpawnPersonalMonster,
        ConsoleUpdateQuestProgress,
        TotalCrit,
        CritRate,
        ConsoleAddStatsPoint,
        ConsoleChangeJob,
        ConsoleAddSkillPoint,
        #endregion

        #region LeaderBoard
        GetLeaderBoard,
        GetLeaderBoardAvatar,
        #endregion

        #region ItemMall

        ItemMallPurchaseItem,
        ItemMallInit,
        ItemMallGetList,

        #endregion ItemMall

        #region Tutorial
        TriggerTutorial,
        #endregion

        #region SevenDays
        SevenDaysBuyDiscountItem,
        SevenDaysClaimTaskReward,
        #endregion

        #region Welfare
        WelfareClaimFirstTimeBuyRewards,
        WelfareClaimDailyBuyRewards,
        WelfareClaimTotalBuyRewards,
        WelfareClaimTotalSpendRewards,
        WelfareClaimDailyActiveRewards,
        WelfareClaimLevelUpRewards,
        WelfareClaimContinuousLoginRewards,
        WelfareClaimTotalLoginRewards,
        WelfareUpdateDailyActiveTask,
        WelfareClaimSignInPrize,
        WelfareClaimOnlinePrizes,
        WelfareClaimOnlinePrizesSingle,
        WelfareBuyOpenServiceFund,
        WelfareClaimOpenServiceFundGoldReward,
        WelfareClaimOpenServiceFundItemReward,
        WelfareClaimFirstGoldCreditRewards,
        WelfareClaimTotalCreditReward,
        WelfareClaimTotalSpendReward,
        WelfareDailyGoldCloseFirstLoginFlag,
        WelfareDailyGoldBuyMCard,
        WelfareDailyGoldClaimMCardGold,
        WelfareDailyGoldBuyPCard,
        WelfareDailyGoldClaimPCardGold,
        WelfareGoldJackpotGetResult,
        WelfareClaimContLoginReward,
        #endregion

        #region QuestExtraRewards
        QERFinishTask,
        QERFinishTaskAll,
        QERClaimTaskReward,
        QERClaimTaskRewardAll,
        QERClaimBoxReward,
        #endregion

        #region Lottery
        LotteryRollOne,
        LotteryRollTen,
        LotteryGetPointReward,
        LotteryGetSimpleInfo,
        LotteryGetInfo,
        LotteryUsePointItem,
        #endregion

        #region ReviveItem
        StartReviveItemRequest,
        RejectReviveItem,
        AcceptReviveItem,
        RequestCancelReviveItem,
        #endregion

        #region EquipmentUpgrade
        EquipmentUpgradeEquipment,
        EquipmentReformEquipment,
        #endregion

        #region Equipment
        HideHelm,
        #endregion

        ConsoleTestComboSkill,
        ClientGuildQuestOperation,

        #region ExchangeShop
        ExchangeShopPurchase,
        #endregion

        #region PortraitUI
        OldenPortrait,
        #endregion

        #region Store
        StoreInit,
        StoreRefresh,
        StorePurchase,
        #endregion

        #region NPCStore
        NPCStoreInit,
        NPCStoreBuy,
        NPCStoreGetPlayerTransactions,
        #endregion

        #region Quest
        UpdateTrakingList,
        DeleteQuest,
        ResetQuest,
        UpdateQuestStatus,
        StartQuest,
        NPCInteract,
        CompleteQuest,
        InteractAction,
        FailQuest,
        SubmitEmptyObjective,
        ApplyQuestEventBuff,
        ApplyQuestEventCompanion,
        #endregion

        #region CharacterInfo
        CharacterInfoSpendStatsPoints,
        #endregion

        #region Skills
        AddToSkillInventory,
        EquipSkill,
        #endregion

        #region PowerUp
        PowerUp,
        #endregion
    }

    public enum ServerNonCombatRPCMethods : byte
    {
        #region LeaderBoard
        Ret_GetLeaderBoard,
        Ret_GetLeaderBoardAvatar,
        #endregion

        #region ItemMall

        Ret_ItemMallPurchaseItem,
        Ret_ItemMallInit,
        Ret_ItemMallGetList,
        //ItemMallGetIsUIOn,
        //ItemMall_Broadcast_LimitedItemSaleOnOff,

        #endregion ItemMall

        #region SevenDays
        SevenDaysNotEnoughGold,
        #endregion

        #region Welfare
        Ret_WelfareNotEnoughGold,
        #endregion

        #region Lottery
        Ret_ShowLotteryRollOneResult,
        Ret_ShowLotteryRollTenResult,
        Ret_LotteryRollFailed,
        Ret_LotteryGetPointReward,
        Ret_LotteryGetSimpleInfo,
        Ret_LotteryGetInfo,
        Ret_LotteryUsePointItem,
        #endregion

        #region ReviveItem
        RequestReviveItem,
        ConfirmAcceptReviveItem,
        ConfirmCancelReviveItem,
        ConfirmCompleteReviveItem,
        //Ret_DeductReviveItemFailed,
        //Ret_ReviveFailed,
        #endregion

        #region EquipmentUpgrade
        EquipmentUpgradeEquipmentFailed,
        EquipmentUpgradeEquipmentSuccess,
        EquipmentReformEquipmentFailed,
        EquipmentReformEquipmentSuccess,
        #endregion

        #region QuestExtraRewards
        QERFailedGold,
        #endregion

        #region Store
        Ret_StoreInit,
        Ret_StorePurchaseItem,
        #endregion

        #region NPCStore
        Ret_NPCStoreInit,
        Ret_NPCStoreBuy,
        Ret_NPCStoreGetPlayerTransactions,
        #endregion

        #region Misc
        SendString,
        #endregion
        Ret_GetGuildQuest,
        Ret_GuildOperationResult,

        #region ExchangeShop
        Ret_ExchangeShopPurchase,
        #endregion

        #region GM
        KickWithReason,
        GMMessage,
        #endregion
        Ret_TransferServer,

        #region Quest
        Ret_DeleteQuest,
        Ret_ResetQuest,
        Ret_CompleteQuest,
        Ret_InteractAction,
        Ret_QuestFull,
        #endregion

        #region CharacterInfo
        Ret_CharacterInfoSpendStatsPoints,
        #endregion

        #region Skill
        Ret_AddToSkillInventory,
        #endregion
    }
}

