//#define clientrpc 0
using Kopio.JsonContracts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.RPC;
using Zealot.Client.Entities;
using Zealot.Common.Actions;
using Zealot.Repository;
using Zealot.Client.Actions;

public partial class ClientMain : MonoBehaviour
{
    [RPCUnsuspend]
    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.RetSuspendedRPC)]
    public void RetSuspendedRPC(int ret, string optionalparam, bool addToChatLog)
    {
        if (ret > -1)
            UIManager.ShowSystemMessage(
                GUILocalizationRepo.GetLocalizedSysMsgById(ret, GameUtils.FormatString(optionalparam)), addToChatLog);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.SpawnMonsterEntity)]
    public void SpawnMonsterEntity(int pid, int archetypeid, RPCPosition rpcpos, RPCDirection rpcdir, int health)
    {
        Vector3 pos = rpcpos.ToVector3();
        Vector3 forward = rpcdir.ToVector3();

        // Debug.Log("SpawnMonsterEntity persistentid = " + pid + ", archetypeid = " + archetypeid + ",pos = (" + pos.x + ", " + pos.y + "," + pos.z);
        MonsterGhost ghost = mEntitySystem.SpawnNetEntityGhost<MonsterGhost>(pid);
        ghost.IsLocal = false;
        ghost.SetOwnerID(0);
        ghost.Init(archetypeid, pos, forward, health);
        mEntitySystem.AddMonster(ghost);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SpawnHeroEntity)]
    public void SpawnHeroEntity(int pid, int heroid, int tier, int ownerpid, RPCPosition rpcpos, RPCDirection rpcdir)
    {
        Vector3 pos = rpcpos.ToVector3();
        Vector3 forward = rpcdir.ToVector3();

        //Debug.Log("SpawnHeroEntity pid = " + pid + ", heroid = " + heroid + ",pos = " + pos.x + ", " + pos.y + "," + pos.z);
        HeroGhost ghost = mEntitySystem.SpawnNetEntityGhost<HeroGhost>(pid);
        ghost.SetOwnerID(ownerpid);
        ghost.IsLocal = false;
        ghost.Init(heroid, tier, pos, forward);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.DestroyEntity)]
    public void DestroyEntity(int pid)
    {
        BaseNetEntityGhost ghost = (BaseNetEntityGhost) mEntitySystem.GetEntityByPID(pid);
        if (ghost != null)
        {
            LogManager.DebugLog("Destroying BaseNetEntityGhost pid [" + pid + "]");
            mEntitySystem.RemoveEntityByPID(pid);
            //Debug.LogWarning("DestroyEntity [" + pid.ToString());
        }
        else
        {
            Debug.LogWarning("DestroyEntity Cannot find == " + pid.ToString());
        }
    }

    IEnumerator ReturnToStandby(int pid, float duration)
    {
        yield return new WaitForSeconds(duration);
        MonsterGhost ghost = mEntitySystem.GetEntityByPID(pid) as MonsterGhost;
        if (ghost != null && ghost.IsAlive() &&
            ghost.GetAction().mdbCommand.GetActionType() == ACTIONTYPE.IDLE)
        {
            ghost.PlayAnimation("standby", 0.2f);
        }
    }

    IEnumerator FlashContinuousHit(MonsterGhost monsterGhost)
    {
        //Special skill hit 10 times with interval of 0.16f sec, but each flash is 0.2sec
        //So, we flash at interval of 0.3 sec instead
        for (int i = 0; i < 5; ++i)
        {
            if (monsterGhost == null || !monsterGhost.IsAlive())
                break;
            monsterGhost.Flash();
            yield return new WaitForSeconds(0.334f);
        }
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.OnPlayerDead)]
    public void OnPlayerDead(string killer, byte respawnid)
    {
        mPlayerInput.ResetJoystick();
        GameInfo.gLocalPlayer.OnPlayerDead();
        int respawnId = respawnid;
        if(respawnId > 0)
        {
            UIManager.SetWidgetActive(HUDWidgetType.Death, true);
            UI_Death uiDeath = UIManager.GetWidget(HUDWidgetType.Death).GetComponent<UI_Death>();
            uiDeath.Init(killer, respawnId);
        }
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.OnPlayerDragged)]
    public void OnPlayerDragged(RPCPosition rpcpos, float dur, float speed)
    {
        Vector3 pos = rpcpos.ToVector3();
        DraggedActionCommand cmd = new DraggedActionCommand();
        cmd.pos = pos;
        cmd.dur = dur;
        cmd.speed = speed;
        ClientAuthoDragged action = new ClientAuthoDragged(GameInfo.gLocalPlayer, cmd);
        action.SetCompleteCallback(() =>
        {
            GameInfo.gLocalPlayer.Idle();
        });
        GameInfo.gLocalPlayer.PerformAction(action);

    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.TrainingStepDone)]
    public void TrianingStepDone(int step)
    {
        TrainingRealmContoller.Instance.OnQuestStepDone(step);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.TrainingDodgeResult)]
    public void TrainingDodgeResult(bool res)
    {
        //string sysmsg = res ? "tut1_Dodge_Success" : "tut1_Dodge_Fail";
        //UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedString(sysmsg));
        //if (res)
        //{
        //    TrainingRealmContoller.Instance.OnQuestStepDone((int)Trainingstep.ShowFlashButton);
        //}
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.OnIncreaseCD)]
    public void OnIncreaseCooldown(string list, float perctange)
    {
        //int[] listint = list.Split(',', );
        Debug.Log("Skill duration change by:" + perctange);
        GameInfo.gLocalPlayer.OnModifyCooldown(new int[] {0, 1, 2, 3}, perctange);
    }


    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.RespawnPlayer)]
    public void RespawnPlayer(RPCPosition rpcpos, RPCDirection rpcdir)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
        {
            return;
        }

        Vector3 pos = rpcpos.ToVector3();
        Vector3 forward = rpcdir.ToVector3();
        forward.y = 0;
        player.Respawn(pos, forward);
        UI_Death uiDeath = UIManager.GetWidget(HUDWidgetType.Death).GetComponent<UI_Death>();
        uiDeath.Close();
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.TeleportSetPos)]
    public void TeleportSetPos(RPCPosition rpcpos)
    {
        PlayerGhost playerGhost = GameInfo.gLocalPlayer;
        playerGhost.Position = rpcpos.ToVector3();

        playerGhost.ForceIdle();
        GameInfo.gCombat.OnSelectEntity(null);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.TeleportSetPosDirection)]
    public void TeleportSetPosDirection(RPCPosition rpcpos, RPCDirection dir)
    {
        TeleportSetPos(rpcpos);
        PlayerGhost playerGhost = GameInfo.gLocalPlayer;
        playerGhost.Forward = dir.ToVector3();
    }

    #region Party

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_GetPartyList)]
    public void Ret_GetPartyList(string result)
    {
        GameObject obj = UIManager.GetWindowGameObject(WindowType.Party);
        if (obj.activeInHierarchy)
            obj.GetComponent<UI_Party>().OnGetPartyList(result);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.SendPartyInvitation)]
    public void SendPartyInvitation(string senderName)
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("name", senderName);
        string message = GUILocalizationRepo.GetLocalizedString("party_inviteToParty", param);
        UIManager.ShowPartyMessage(message, () => RPCFactory.CombatRPC.AcceptPartyInvitation(senderName, true), 
            () => RPCFactory.CombatRPC.AcceptPartyInvitation(senderName, false));
    }

    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SendFollowInvitation)]
    public void SendFollowInvitation(string senderName)
    {
        PartyStatsClient partyStats = GameInfo.gLocalPlayer.PartyStats;
        if (partyStats != null && PartyFollowTarget.TargetName != senderName)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("name", senderName);
            string message = GUILocalizationRepo.GetLocalizedString("party_inviteToFollow", param);
            UIManager.ShowPartyMessage(message, () => RPCFactory.CombatRPC.AcceptFollowInvitation(senderName, true),
                () => RPCFactory.CombatRPC.AcceptFollowInvitation(senderName, false));
        }
    }

    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.OnFollowPartyMember)]
    public void OnFollowPartyMember(int pid, string targetName, string levelName, RPCPosition position)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null && player.PartyStats != null)
            player.PartyStats.OnFollowPartyMember(pid, targetName, levelName, position);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.OnGetPartyMemberPosition)]
    public void OnGetPartyMemberPosition(string currLevelName, RPCPosition position, int pid)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null && player.PartyStats != null)
            player.PartyStats.OnGetPartyMemberPosition(currLevelName, position, pid);
    }

    #endregion

    #region Hero
    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_RandomInterestResult)]
    public void Ret_RandomInterestResult(byte interest)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
            player.HeroStats.OnInterestRandomSpinResult(interest);
    }
    #endregion

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.SpawnGate)]
    public void SpawnGate(int pid, float width, float height, string prefab, RPCPosition rpcpos, RPCDirection rpcdir)
    {
        Vector3 pos = rpcpos.ToVector3();
        Vector3 forward = rpcdir.ToVector3();
        GateGhost ghost = mEntitySystem.SpawnNetEntityGhost<GateGhost>(pid);
        ghost.IsLocal = false;
        ghost.SetOwnerID(0);
        ghost.Init(width, height, prefab, pos, forward);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.SpawnAnimationObject)]
    public void SpawnAnimationObject(int pid, string prefab, RPCPosition rpcpos, RPCDirection rpcdir)
    {
        Vector3 pos = rpcpos.ToVector3();
        Vector3 forward = rpcdir.ToVector3();
        AnimationObjectGhost ghost = mEntitySystem.SpawnNetEntityGhost<AnimationObjectGhost>(pid);
        ghost.IsLocal = false;
        ghost.SetOwnerID(0);
        ghost.Init(prefab, pos, forward);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.ServerSendChatMessage)]
    public void ServerSendChatMessage(byte messagetype, string message, string sender, string whisperTo, int portraitId,
        byte jobSect, byte vipLvl, byte faction, bool isVoiceChat)
    {
        //if(messagetype == (byte)MessageType.System)
        //HUD.Combat.SetTickerMessage(message);

        HUD_Chatroom hudChatroom = UIManager.GetWidget(HUDWidgetType.Chatroom).GetComponent<HUD_Chatroom>();
        if (hudChatroom != null)
            hudChatroom.AddToChatLog(messagetype, message, sender, whisperTo, portraitId, jobSect, 
                                     vipLvl, faction, isVoiceChat);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_SendSystemMessageById)]
    public void Ret_SendSystemMessageId(int ret, string optionalparam, bool addToChatLog)
    {
        UIManager.ShowSystemMessage(
            GUILocalizationRepo.GetLocalizedSysMsgById(ret, GameUtils.FormatString(optionalparam)), addToChatLog);
        UIManager.StopHourglass();
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_SendSystemMessageByStr)]
    public void Ret_SendSystemMessage(string ret, string optionalparam, bool addToChatLog)
    {
        UIManager.ShowSystemMessage(
            GUILocalizationRepo.GetLocalizedSysMsgByName(ret, GameUtils.FormatString(optionalparam)), addToChatLog);
        UIManager.StopHourglass();
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.SetInspectPlayerInfo)]
    public void SetInspectPlayerInfo(string inspectdata)
    {

    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.BroadcastCutScene)]
    public void BroadcastCutScene(int cutscene)
    {
        GameInfo.gCombat.CutsceneManager.PlayEventCutscene(cutscene);
    }

    //[RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.ProceedToTarget)]
    //public void ProceedToTarget(RPCPosition pos, int id, byte actiontype)
    //{
    //    if (GameInfo.gLocalPlayer != null)
    //    {
    //        GameInfo.gLocalPlayer.ProceedToTarget(pos.ToVector3(), id, (CallBackAction)actiontype);
    //    }
    //}

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.SetServerTime)]
    public void SetServerTime(long time)
    {
        //Debug.Log("SetServerTime from " + mTimers.GetSynchronizedTime() + " to " + time);
        mTimers.SetSynchronizedTimeStamp(time);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.SendInfoOnPlayerSpawner)]
    public void SendInfoOnPlayerSpawner(long serverstartupticks, long servernowticks, int activityStatus,
        long arenarewardticks ,bool inspectMode, string charId)
    {
        GameInfo.mCharId = charId;
        GameInfo.mInspectMode = inspectMode;
        PlayerGhost player = GameInfo.gLocalPlayer;
        GameInfo.mServerStartUpDT = new DateTime(serverstartupticks);
        DateTime serverdt = new DateTime(servernowticks);
        DateTime localdt = DateTime.Now;
        mTimers.mDiffTotalMiliSecondsWithServerDT = (localdt - serverdt).TotalMilliseconds;
        GameUtils.mActivityStatus = activityStatus;

        //OnGMActivityConfigChanged(gmActivityConfig);
        GameInfo.mIsPlayerReady = true;

        if (player.PlayerSynStats.Level >= GameConstantRepo.GetConstantInt("Arena_UnlockLvl", 1))
        {
            player.SetArenaRewardDT(new DateTime(arenarewardticks));
           // UIManager.AlertManager2.SetAlert(AlertType.ArenaFreeCount, arenafreeentry);
        }

        //string[] auctionInfo = auctionStatus.Split(';');
        //player.CheckAuctionStatus(int.Parse(auctionInfo[0]));

        // Equipment
        // Update red dot status
        //player.CheckEquipmentRedDot();
        // Welfare

        // SevenDays
        // QuestExtraRewards
        // Update red dot status 

        RealmJson realmInfo = GameInfo.mRealmInfo;
        //if (realmInfo == null || realmInfo.type != RealmType.RealmTutorial)
        {
            if (GameInfo.mOpenArenaOnPlayerSpawn || GameInfo.mPlayerFirstEnter)
                mTimers.SetTimer(1500, (arg) =>
                {
                    if (GameInfo.mPlayerFirstEnter)
                    {
                        GameInfo.mPlayerFirstEnter = false;
                        if (realmInfo.maptype != MapType.Dungeon)
                        {
                            //UIManager.OpenWindow(WindowType.Secretary);
                            //UIManager.OpenDialog(WindowType.DialogAnnouncement);
                        }
                        
                    }
                    else if (GameInfo.mOpenArenaOnPlayerSpawn)
                    {
                        //if (!UIManager.IsWindowOpen(WindowType.LevelUp))
                        //{
                        //    GameInfo.mOpenArenaOnPlayerSpawn = false;
                        //    UIManager.OpenWindow(WindowType.Arena);
                        //}
                    }
                }, null);
            //if (realmInfo == null || realmInfo.type == RealmType.RealmWorld)
               // UIManager.GetWidget(HUDWidgetType.LotteryTip).GetComponent<UI_LotteryFocusTip>().ShowFocusTip();
        }

        //player.mailInvCtrl.hasMail = hasMail;   //Turn on/off red dot for mail
        //player.offlineExpCtrl2.SetRedDotOnSpawn(offlineExpReady);
       // player.clientInvCtrl.StartAutoUnlock(0);
        //UIManager.AlertManager2.SetAlert(AlertType.Portrait, hasNewPortrait);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.InitGameSetting)]
    public void InitGameSetting(string gameSetting, string botSetting)
    {
        GameSettings.DeserializeServer(gameSetting);
        //BotSettings.Init(botSetting);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.OpenUIWindow)]
    public void OpenUIWindow(byte linkUI, int itemSlotId)
    {
       // ItemUtils.OpenUIWindowSlotId = itemSlotId >= 0 ? itemSlotId : -1;
       // ClientUtils.OpenUIWindowByLinkUI((LinkUIType) linkUI);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.UsingChestResult)]
    public void UsingChestResult(UsingChestCode code)
    {
       // GameInfo.gLocalPlayer.clientChestCtrl.ServerResult(code);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_GetArenaChallenger)]
    public void Ret_GetArenaChallenger(int myrank, int entries, int cooldown, string result)
    {
        //GameObject arena = UIManager.GetWindowGameObject(WindowType.Arena);
        //if (arena.activeInHierarchy)
            //arena.GetComponent<UI_Arena>().Ret_GetArenaChallenger(myrank, entries, cooldown, result);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_ArenaClaimReward)]
    public void Ret_ArenaClaimReward(bool success, int myrank)
    {
        if (success)
        {
            GameInfo.gLocalPlayer.SetArenaRewardDT(GameInfo.GetSynchronizedServerDT());
            UIManager.AlertManager2.SetAlert(AlertType.ArenaReward, false);


            PushNotificationJson sysnoti = PushNotificationRepo.GetPushNotificationByID(11);
            //if (sysnoti != null)
            //{
                //ArenaJson arena_info = RealmRepo.mArenaJson;
                //int totalsecond = arena_info.rewardcd * 3600;
               
            //}
        }
        //GameObject arena = UIManager.GetWindowGameObject(WindowType.Arena);
        //if (arena.activeInHierarchy)
          //  arena.GetComponent<UI_Arena>().Ret_ArenaClaimReward(success, myrank);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_GetArenaReport)]
    public void Ret_GetArenaReport(string report)
    {
        //GameObject report_dialog = UIManager.GetWindowGameObject(WindowType.DialogArenaReport);
        //if (report_dialog.activeInHierarchy)
            //report_dialog.GetComponent<UI_Arena_Report>().RefreshReport(report);
    }

    #region Mail

    //[RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_SendMail)]
    //public void Ret_SendMail(string serializedMailString)
    //{
    //    GameInfo.gLocalPlayer.mailInvCtrl.SendMail_Callback(serializedMailString);
    //}

    //[RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_HasNewMail)]
    //public void Ret_HasNewMail(bool hasNewMail, string hudID)
    //{
    //    GameInfo.gLocalPlayer.mailInvCtrl.HasNewMail_Callback(hasNewMail, hudID);
    //}

    //[RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_RetrieveMail)]
    //public void Ret_RetrieveMail(string serializedMailString)
    //{
    //    GameInfo.gLocalPlayer.mailInvCtrl.RetrieveMail_Callback(serializedMailString);
    //}

    //[RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_OpenMail)]
    //public void Ret_OpenMail(int mailReturnCode, int mailIdx)
    //{
    //    GameInfo.gLocalPlayer.mailInvCtrl.OpenMail_Callback(mailReturnCode, mailIdx);
    //}

    //[RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_TakeAttachment)]
    //public void Ret_TakeAttachment(int mailReturnCode, int mailIdx)
    //{
    //    GameInfo.gLocalPlayer.mailInvCtrl.TakeAttachment_Callback(mailReturnCode, mailIdx);
    //}

    //[RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_TakeAllAttachment)]
    //public void Ret_TakeAllAttachment(int mailReturnCode, string lstTakenMailIndexSerialStr)
    //{
    //    GameInfo.gLocalPlayer.mailInvCtrl.TakeAllAttachment_Callback(mailReturnCode, lstTakenMailIndexSerialStr);
    //}

    //[RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_DeleteMail)]
    //public void Ret_DeleteMail(int mailReturnCode, int mailIdx)
    //{
    //    GameInfo.gLocalPlayer.mailInvCtrl.DeleteMail_Callback(mailReturnCode, mailIdx);
    //}

    //[RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_DeleteAllMail)]
    //public void Ret_DeleteAllMail(int mailReturnCode)
    //{
    //    GameInfo.gLocalPlayer.mailInvCtrl.DeleteAllMail_Callback(mailReturnCode);
    //}

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_OpenNewInvSlot)]
    public void Ret_OpenNewInvSlot(byte retcode, int nextOpenTime)
    {
        var retCode = (OpenSlotRetCode) retcode;
        if (retCode == OpenSlotRetCode.Success)
        {
            var message = GUILocalizationRepo.GetLocalizedSysMsgByName("sys_OpenNewSlot");
            UIManager.AddToChat((byte)MessageType.System, message);

            //if (UIManager.IsWindowOpen(WindowType.Dialog_CharInfo_UnLockItemBox))
            //    UIManager.CloseDialog(WindowType.Dialog_CharInfo_UnLockItemBox);

            //will be called on init and on vipLvl changed amd when there is slot change
            // GameInfo.gLocalPlayer.clientInvCtrl.StartAutoUnlock(nextOpenTime);
        }
        else if (retCode == OpenSlotRetCode.Fail_Gold)
        {
           // ClientUtils.OpenUIWindowByLinkUI(LinkUIType.GoTopUp);
        }
        else if (retCode == OpenSlotRetCode.Fail_AutoOpen)
        {
            //wait another 3 min before attempt to open
          //  GameInfo.gLocalPlayer.clientInvCtrl.StartAutoUnlock(nextOpenTime);
        }
        else if (retCode == OpenSlotRetCode.Fail_AllOpened)
        {
           // GameInfo.gLocalPlayer.clientInvCtrl.StopAutoUnlock();
        }
    }

    #endregion Mail

    #region OfflineExp

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.OfflineExpRedDot)]
    public void OfflineExpRedDot(bool flag)
    {
        UIManager.AlertManager2.SetAlert(AlertType.OfflineExp, flag);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.OfflineExpGetData)]
    public void OfflineExpGetData(string serializedOfflineExp)
    {
        //GameInfo.gLocalPlayer.offlineExpCtrl.OfflineExpGetData_Callback(serializedOfflineExp);
        //GameInfo.gLocalPlayer.offlineExpCtrl2.RequestRewardStatus_Callback(serializedOfflineExp);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.OfflineExpStartReward)]
    public void OfflineExpStartReward(string serializedOfflineExp)
    {
       // GameInfo.gLocalPlayer.offlineExpCtrl2.StartReward_Callback(serializedOfflineExp);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.OfflineExpClaimReward)]
    public void OfflineExpClaimReward(int offlineExpReturnCode, int newlvl)
    {
       // GameInfo.gLocalPlayer.offlineExpCtrl2.GetReward_Callback(offlineExpReturnCode, newlvl);
        //GameInfo.gLocalPlayer.offlineExpCtrl.OfflineExpClaimReward_Callback(offlineExpReturnCode);
    }

    #endregion OfflineExp

    #region Auction

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_AuctionGetAuctionItem)]
    public void Ret_AuctionGetAuctionItem(string stringData, int bidPrice)
    {
        //GameObject obj = UIManager.GetWindowGameObject(WindowType.Auction);
        //if (obj.activeInHierarchy)
          //  obj.GetComponent<UI_Auction>().Ret_OnGetAuctionItem(stringData, bidPrice);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_AuctionGetRecords)]
    public void Ret_AuctionGetRecords(string stringData)
    {
        //GameObject obj = UIManager.GetWindowGameObject(WindowType.Auction);
        //if (obj.activeInHierarchy)
           // obj.GetComponent<UI_Auction>().Ret_OnGetRecords(stringData);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_AuctionGetBidItems)]
    public void Ret_AuctionGetBidItems(string stringData)
    {
        //GameObject obj = UIManager.GetWindowGameObject(WindowType.Auction);
        //if (obj.activeInHierarchy)
           // obj.GetComponent<UI_Auction>().Ret_OnGetBidItems(stringData);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_AuctionCollectItem)]
    public void Ret_AuctionCollectItem(byte result)
    {
        //GameObject obj = UIManager.GetWindowGameObject(WindowType.Auction);
        //if (obj.activeInHierarchy)
            //obj.GetComponent<UI_Auction>().Ret_OnCollectItem(result);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_AuctionPlaceBid)]
    public void Ret_AuctionPlaceBid(byte result, int bidPrice)
    {
        //GameObject obj = UIManager.GetWindowGameObject(WindowType.Auction);
        //if (obj.activeInHierarchy)
            ///obj.GetComponent<UI_Auction>().Ret_OnPlaceBid(result, bidPrice);
    }

    #endregion
 

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_GuildGetGuildInfo)]
    public void Ret_GuildGetGuildInfo(string guildInfoStr)
    {
        //GameObject obj = UIManager.GetWindowGameObject(WindowType.GuildJoin);
        //if (obj.activeInHierarchy)
        //    obj.GetComponent<UI_Guild_Join>().Ret_OnGetGuildInfo(guildInfoStr);
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_GuildResult)]
    public void Ret_GuildResult(byte retcode, string param)
    {
        Dictionary<string, string> sysMsgDict = null;
        switch (retcode)
        {
            case (byte) GuildReturnCode.MemberRequestSuccess:
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Guild_RequestSuccess"));
                break;
            case (byte) GuildReturnCode.MemberRequestExist:
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Guild_AlreadyRequest"));
                break;
            case (byte) GuildReturnCode.GuildNotFound:
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Guild_GuildNotFound"));
                break;
            case (byte) GuildReturnCode.InsufficientGold:
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Guild_InsufficientGold"));
                break;
            case (byte) GuildReturnCode.GuildLevelTooLow:
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Guild_GuildLevelTooLow"));
                break;
            case (byte) GuildReturnCode.TechAlreadyMax:
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Guild_TechAlreadyMax"));
                break;
            case (byte) GuildReturnCode.GuildLeaderOnly:
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Guild_LeaderOnly"), true);
                break;
            case (byte) GuildReturnCode.UnableToAppoint:
                byte appointRank = 0;
                byte.TryParse(param, out appointRank);
                if (appointRank < 0) appointRank = 0;
                sysMsgDict = new Dictionary<string, string>();
                sysMsgDict.Add("rank",
                    GUILocalizationRepo.GetLocalizedString("guild_Rank_" + ((GuildRankType) appointRank).ToString()));
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Guild_UnableToAppoint",
                    sysMsgDict));
                break;
        }
    }


    #region Development

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.SendMessageToConsoleCmd)]
    public void SendMessageToConsoleCmd(string msg)
    {
#if ZEALOT_DEVELOPMENT
        GameObject consoleCmdObj = UIManager.GetWindowGameObject(WindowType.ConsoleCommand);
        if (consoleCmdObj != null)
            consoleCmdObj.GetComponent<UI_ConsoleCommand>().AddToContent(msg);
#endif
    }

    #endregion

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.ReplyValidMonSpawnPos)]
    public void ReplyValidMonSpawnPos(RPCPosition pos)
    {
        if (GameInfo.gLocalPlayer == null)
            return;

        PlayerGhost player = GameInfo.gLocalPlayer;
        player.Bot.SeekToPosition(pos.ToVector3());
    }

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_SocialReturnResult)]
    public void Ret_SocialReturnResult(byte retCode, string param)
    {
        SocialReturnCode returnCode = (SocialReturnCode) retCode;
        switch (returnCode)
        {
            case SocialReturnCode.Ret_AlreadyAdded:
            {
                Dictionary<string, string> paramDict = new Dictionary<string, string>();
                paramDict.Add("name", param);
                string msg = GameUtils.FormatString(
                    GUILocalizationRepo.GetLocalizedSysMsgByName("ret_fr_AlreadyAdded"), paramDict);
                UIManager.ShowSystemMessage(msg, false);
            }
                break;
            case SocialReturnCode.Ret_DoesNotExist:
                UIManager.OpenOkDialog(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_fr_NameDoesNotExist"), null);
                break;
            case SocialReturnCode.Ret_RecommendedResult:
                //GameObject recommendedFriendsWindow = UIManager.GetWindowGameObject(WindowType.DialogFriendsRecommended);
              //  if (recommendedFriendsWindow != null && recommendedFriendsWindow.activeInHierarchy)
                   // recommendedFriendsWindow.GetComponent<Dialog_FriendsRecommended>().ParseStrAndInitData(param);
                break;
            case SocialReturnCode.Ret_OnCooldown:
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_OnCooldown"), false);
                break;
            case SocialReturnCode.Ret_LevelNotEnough:
            {
                Dictionary<string, string> paramDict = new Dictionary<string, string>();
                paramDict.Add("lvl", param);
                string msg =
                    GameUtils.FormatString(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_fr_LevelNotEnough"),
                        paramDict);
                UIManager.ShowSystemMessage(msg, false);
            }
                break;
        }
    }

    #region IAP

    [RPCUnsuspend]
    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_GetProductsWithLockGold)]
    public void Ret_GetProductsWithLockGold(string productsWithLockGold)
    {
        //TopUpController.GetInstance().OnGetProductsWithLockGold(productsWithLockGold);
    }

    [RPCUnsuspend]
    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_VerifyPurchase)]
    public void Ret_VerifyPurchase(int gold, int lockGold, int vipPoints)
    {
       // TopUpController.GetInstance().OnVerifyPurchase(gold, lockGold, vipPoints);
    }

    #endregion

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_OnCurrencyExchange)]
    public void Ret_OnCurrencyExchange(byte result, byte oldExchangeTime)
    {
        //UIManager.CloseDialog(WindowType.DialogAlchemyCheck);
        //if (result != 0)
            //UIManager.OpenWindow(WindowType.AlchemyReward,
              //  (GameObject go) => { go.GetComponent<UI_AlchemyReward>().Init(result, oldExchangeTime); });
    }


    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_GetRandomBoxReward)]
    public void Ret_GetRandomBoxReward(byte result, int reward_id)
    {
        //HUD_RandomChest hudRandomChest = UIManager.GetWidget(HUDWidgetType.RandomChest).GetComponent<HUD_RandomChest>();
        //if (hudRandomChest != null)
        {
           // hudRandomChest.OnGetRewardResult(result, reward_id);
        }
    }

    #region InvitePVP

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_InvitePvpResult)]
    public void Ret_InvitePvpResult(byte retCode, string param)
    {
        string msgstr = "";
        InvitePvpReturnCode returnCode = (InvitePvpReturnCode) retCode;
        switch (returnCode)
        {
            case InvitePvpReturnCode.Ret_NotOnline:
                //param : targetname
                msgstr = "ret_pvp_notonline";
                break;
            case InvitePvpReturnCode.Ret_AskToTarget:
                //param : askername
                //if (UIManager.IsWindowOpen(WindowType.DialogInvitePVPReceive))
                //{
                //    //已經被邀請PVP
                //    RPCFactory.CombatRPC.InvitePvpReply(param, (int) InvitePvpReturnCode.Ret_IngToAsker);
                //}
                //else
                //{
                //    //UIManager.OpenDialog(WindowType.DialogInvitePVPReceive,
                //        //(window) => window.GetComponent<Dialog_InvitePVPReceive>().Init(param));
                //}
                break;
            case InvitePvpReturnCode.Ret_IngToAsker:
                //param : targetname
                msgstr = "sys_learnfrom_returnmsg004";
                break;
            case InvitePvpReturnCode.Ret_NoToAsker:
                //param : targetname
                msgstr = "sys_learnfrom_returnmsg001";
                break;
            case InvitePvpReturnCode.Ret_InRealm:
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_learnfrom_returnmsg002",
                    null));
                break;
            case InvitePvpReturnCode.Ret_NotInCity:
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_learnfrom_returnmsg003",
                    null));
                break;
        }

        if (msgstr != "")
        {
            Dictionary<string, string> paramDict = new Dictionary<string, string>();
            paramDict.Add("name", param);
            string msg = GameUtils.FormatString(GUILocalizationRepo.GetLocalizedSysMsgByName(msgstr), paramDict);
            UIManager.ShowSystemMessage(msg, false);
        }
    }

    #endregion

    #region Wardrobe

    [RPCMethod(RPCCategory.Combat, (byte) ServerCombatRPCMethods.Ret_OnUpdateWardrobe)]
    public void Ret_OnUpdateWardrobe(byte return_code)
    {
        //GameInfo.gLocalPlayer.wardrobeController.Ret_OnUpdateWardrobe((WardrobeRetCode) return_code);
    }

    #endregion

    #region world boss
    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GetWorldBossList)]
    public void Ret_GetWorldBossList(string result)
    {
        GameObject dailyQuestObj = UIManager.GetWindowGameObject(WindowType.DailyQuest);
        if (dailyQuestObj.activeInHierarchy)
        {
            UI_DailyActivity component = dailyQuestObj.GetComponent<UI_DailyActivity>();
            if (component.TabController.IsTabOn(2))
            {
                UIManager.StopHourglass();
                result = result.TrimEnd(';');

                Dictionary<int, SpecialBossStatus> _specialBossStatus = new Dictionary<int, SpecialBossStatus>();
                if (!string.IsNullOrEmpty(result))
                {
                    string[] _bossArray = result.Split(';');
                    for (int index = 0; index < _bossArray.Length; index++)
                    {
                        SpecialBossStatus _bossStatus = SpecialBossStatus.Deserialize(_bossArray[index]);
                        if (_bossStatus != null)
                            _specialBossStatus[_bossStatus.id] = _bossStatus;
                    }
                }

                component.Ret_GetWorldBossList(_specialBossStatus);
            }
        }
    }

    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.Ret_GetWorldBossDmgList)]
    public void Ret_GetWorldBossDmgList(string result)
    {
        GameObject dailyQuestObj = UIManager.GetWindowGameObject(WindowType.DailyQuest);
        if (dailyQuestObj.activeInHierarchy)
        {
            UI_DailyActivity component = dailyQuestObj.GetComponent<UI_DailyActivity>();
            if (component.TabController.IsTabOn(2))
            {
                UIManager.StopHourglass();
                BossKillData _bossKillData = null;
                if (!string.IsNullOrEmpty(result))
                    _bossKillData = BossKillData.DeserializeFromDB(result);
                UIManager.OpenDialog(WindowType.DialogWorldBossRanking, (dialog) => dialog.GetComponent<UI_WorldBossRanking>().Init(_bossKillData));
            }
        }
    }
    #endregion

    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.LootItemDisplay)]
    public void LootItemDisplay(string data)
    {
        LootItemDisplayInventory lootData = LootItemDisplayInventory.ToObject(data);
        if (lootData != null && lootData.records.Count > 0)
        {
            GameObject lootObject = new GameObject();
            SetLootParent(lootObject);
            lootObject.transform.position = new Vector3(lootData.pos[0], lootData.pos[1], lootData.pos[2]);
            LootDisplayController component = lootObject.AddComponent<LootDisplayController>();
            component.Init(lootData);
        }
    }
}
